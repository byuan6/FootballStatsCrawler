using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon;
using CrawlerCommon.TagDef.StrictXHTML;
using CrawlerCommon.HtmlPathTest;


namespace Html2Tsv
{
    public class Program
    {
        static int Main(string[] args)
        {
            //
            if (args.Length == 0)
            {
                showUsage();
                return 1;
            }

            var test = ParseXPath(args[0].ToUpper());
            var display = ToStringBuilder(test);
            var str = display.ToString();
            Console.WriteLine(str);

            Console.ReadLine();

            return 0;
        }

        static void showUsage()
        {
            Console.WriteLine("Usage: Html2Tsv [http://...] [columns]");
            Console.WriteLine("       Html2Tsv [filename] [columns]");
            Console.WriteLine("Automatically saves HTML to file, \"/\" converted to !");
        }

        static void getDataFromInternet(string url, HtmlPath[] columns)
        {
            string filename = Url2Filename(url);
            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            page.Load(url, filename);

            int len = columns.Length;
            int end = len-1;
            string[] row = new string[len];
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.Tokens.BuildTree(builder))
            {
                for (int i = 0; i < len; i++)
                    if (columns[i].IsMatch(item))
                        if (i != end)
                            row[i] = item.ActualSymbol;
                        else
                        {
                            for (int j = 0; j < len; j++)
                                if (j == 0)
                                    Console.Write(row[i]);
                                else
                                    Console.Write("\t{0}", row[i]);
                            Console.WriteLine();
                        }
            }
        }

        static string Url2Filename(string url)
        {
            string filename = url.Replace("/", "!");
            return filename;
        }

        /// <summary>
        /// Custom XPATH like expression
        /// Rules: Each character is traversal of parsing tree
        ///          table/tr
        ///        When a reset is warranted, it checks that the prev position in parsing tree is valid expression
        ///          table '/' triggers check of t-a-b-l-e node
        ///        if no leaf with test, it fails
        ///        otherwise it gets put on stack, depending on rules of the test in the leaf
        ///          if parenttest, it will pop test on stack, add the test to itself, and add parenttest to stack
        ///          if elementtest, it will pop test on stack, add the test to iteself and add elementtest to stack
        ///          etc.
        ///          if or, it needs to put -or- test marker to stack, that way, then 2nd test completes, it pops 3 times
        /// Reset back to top of parsing tree when
        ///        1. terminating character is encountered, forcing check of current parsing traveral is valid
        ///           - Is prev node a valid keyword?  If not - FAIL
        ///        2. end of branch reached, forcing reset (end of branch should always be valid keyword), but start of next is not forced
        ///        3. current character is not found in current next children, and current character is NOT terminating character - FAIL
        /// </summary>
        /// <returns></returns>
        static public ITokenTest ParseXPath(string expr)
        {
            /*  literal/td/tr[1]/[table/td/tr/table[5]|tbody/table/td/tr/tbody/table[5]]
             
                is how the object test tree is created w the object of interest on left side (or top of tree), 
                but xpath puts object of interest on right side of expression.  
                    plus tree expressions are part of node conditional expressions
                    but for us, a tree represents either a or/and condition on anynumber of nodes
             
                [table[5]/tr/td/table|table[5]/tbody/tr/td/table/tbody]/tr[1]/td/literal
             
                we will parse a xpath-like expression for familarity of user
             */

            // pretend this is a cisc-like instruction byte stream, instructions can alter prev instructions
            // instruction1, instruction 2 popstack alter pushonstack, instruction 3 popstack alter pushonstack 
            Stack<ITokenTest> stack = new Stack<ITokenTest>();
            RecursiveDictionary<Type> tagIndex = getXpathExprIndexTree();
            RecursiveDictionary<Type> prev = null;
            //bool isPushed = false;
            int notided = 0;
            int len = expr.Length;
            for (int i=0; i < len; i++)
            {
                var ch = expr[i];
                var found = tagIndex.Traverse(ch);
                
                if (found != null)
                {
                    // rule 2
                    if (found.Count == 0)
                    {
                        // no need to check if current has leaf, by definition it has to
                        var test = createTest(stack, found);
                        /*
                        if (stack.Count > 0)
                        {
                            var instr = stack.Pop();
                            var clone = (ITokenTest)Activator.CreateInstance(found.Leaf); //clone too
                            if (clone is BaseTestLinkage)
                            {
                                //if new test is tree-able, then make what's on stack a child
                                var casted = (BaseTestLinkage)clone;
                                casted.Add(instr); //order doesn't matter, if it checks the current node is root, or the element
                                //here it checks the elementtest first, then if it's root
                                stack.Push(casted);
                            }
                            // else vice versa
                            else if (instr is BaseTestLinkage)
                            {
                                var casted = (BaseTestLinkage)instr;
                                casted.Add(clone); //order doesn't matter, if it checks the current node is root, or the element
                                //here it checks the elementtest first, then if it's root
                                stack.Push(casted);
                            }
                            // not a supported expression
                            else
                                throw new InvalidOperationException("There is something very wrong with the exception.  a test does not apply to an element (tree-able)");
                         
                        }
                        else
                        {
                            var clone = (ITokenTest)Activator.CreateInstance(found.Leaf);
                            stack.Push(clone); //clone this, instead
                        }*/
                        if (test is IExprHandler)
                        {
                            var casted = (IExprHandler)test;
                            casted.ParseExpr(expr, notided, i - notided - 1, found.Operator);
                        }

                        System.Diagnostics.Debug.WriteLine("Expression [{0}]", expr.Substring(notided, i - notided));
                        notided = i+1;
                        tagIndex.ResetTraversal();
                    }

                    // rule 1
                    else if (found.IsTerminationChar && notided != i)  //may have been reset by rule3
                    {
                        // still need to check if current traversal is valid keyword
                        if (prev.Leaf == null)
                            throw new ArgumentException("Invalid keyword");
                        var test = createTest(stack, prev);
                        /*
                        if (stack.Count > 0)
                        {
                            var instr = stack.Pop();
                            var clone = (BaseTestLinkage)Activator.CreateInstance(prev.Leaf); //clone too
                            clone.Add(instr); //order doesn't matter, if it checks the current node is root, or the element
                            //here it checks the elementtest first, then if it's root
                            stack.Push(clone);
                        }
                        else
                        {
                            var clone = (ITokenTest)Activator.CreateInstance(prev.Leaf);
                            stack.Push(clone); //clone this, instead
                        }*/
  
                        if (test is IExprHandler)
                        {
                            var casted = (IExprHandler)test;
                            casted.ParseExpr(expr, notided, i - notided-1, prev.Operator);
                        }
                        System.Diagnostics.Debug.WriteLine("Expression [{0}]", expr.Substring(notided, i - notided));
                        notided = i;
                        tagIndex.ResetTraversal();
                        tagIndex.Traverse(ch);
                    }
                }
                else
                    // rule3
                    if (prev != null)
                    {
                        if (prev.Leaf != null)
                        {
                            // see if you can build this if-tree in as part of ???Test interface
                            var test = createTest(stack, prev);
                            /*
                            if (prev.Leaf == typeof(ParentTest))
                            {
                                if (i > 0)
                                {
                                    var instr = stack.Pop();
                                    var clone = (ParentTest)Activator.CreateInstance(prev.Leaf); //clone too
                                    clone.Add(instr); //whatever test was in stack, now it checks the parent
                                    stack.Push(clone);
                                }
                                else
                                {
                                    stack.Push(new RootTest());
                                }
                            }
                            else // if (prev.Leaf == typeof(ElementTest<>))
                            {
                                if (stack.Count > 0)
                                {
                                    var instr = stack.Pop();
                                    var clone = (ElementTest)Activator.CreateInstance(prev.Leaf); //clone too
                                    clone.Add(instr); //order doesn't matter, if it checks the current node is root, or the element
                                    //here it checks the elementtest first, then if it's root
                                    stack.Push(clone);
                                }
                                else
                                {
                                    var clone = (ElementTest)Activator.CreateInstance(prev.Leaf);
                                    stack.Push(clone); //clone this, instead
                                }
                            }*/
 
                            if (test is IExprHandler)
                            {
                                var casted = (IExprHandler)test;
                                casted.ParseExpr(expr, notided, i - notided, prev.Operator);
                            }
                            System.Diagnostics.Debug.WriteLine("Expression [{0}]", expr.Substring(notided, i - notided));
                            notided = i;
                            tagIndex.ResetTraversal();
                            found = tagIndex.Traverse(ch);
                        }
                        else
                            throw new ArgumentException("expression not found " + expr.Substring(notided, i - notided));
                    }
                prev = found;
            }

            if (notided != len) 
                if( prev.Leaf == null)
                    throw new ArgumentException("expression not found " + expr.Substring(notided));

                // process last expression
                //should not be parent test, either it ends wo /, or implicit . added
                else //if (typeof(BaseTestLinkage).IsAssignableFrom(prev.Leaf))
                {
                    var test = createTest(stack, prev);
                    if (test is IExprHandler)
                    {
                        var casted = (IExprHandler)test;
                        casted.ParseExpr(expr, notided, len - notided - 1, prev.Operator);
                    }
                }

            ITokenTest result = stack.Pop();
            while (stack.Count > 0)
            {
                throw new ArgumentException("stack should be empty");
            }



            // return new CacheTestResult(test); // add this optimization in, if you can figure out which parent has a lot of children, so the parent call fails a lot.
            return result;
        }

        /// <summary>
        /// creates the test indicated by the current node in parsing tree, and pushes it on the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="node"></param>
        static ITokenTest createTest(Stack<ITokenTest> stack, RecursiveDictionary<Type> node)
        {
            ITokenTest result = null;
            if (stack.Count > 0)
            {
                var instr = stack.Pop();
                var clone = (ITokenTest)Activator.CreateInstance(node.Leaf); //clone too
                if (clone is BaseTestLinkage)
                {
                    //if new test is tree-able, then make what's on stack a child
                    var casted = (BaseTestLinkage)clone;
                    casted.Add(instr); //order doesn't matter, if it checks the current node is root, or the element
                    //here it checks the elementtest first, then if it's root
                    stack.Push(casted);
                }
                // else vice versa
                else if (instr is BaseTestLinkage)
                {
                    var casted = (BaseTestLinkage)instr;
                    casted.Add(clone); //order doesn't matter, if it checks the current node is root, or the element
                    //here it checks the elementtest first, then if it's root
                    stack.Push(casted);
                }
                // not a supported expression
                else
                    throw new InvalidOperationException("There is something very wrong with the exception.  a test does not apply to an element (tree-able)");
                result = clone;
            }
            else
            {
                var clone = (ITokenTest)Activator.CreateInstance(node.Leaf);
                stack.Push(clone); //clone this, instead
                result = clone;
            }

            if (result is IConverterNeeded<int>)
            {
                var casted = (IConverterNeeded<int>)result;
                casted.convert = Convert.ToInt32;
            }
            if (result is IConverterNeeded<string>)
            {
                var casted = (IConverterNeeded<string>)result;
                casted.convert = IConverter.PassThru;
            }


            return result;
        }


        static StringBuilder ToStringBuilder(ITokenTest test)
        {
            return ToStringBuilder(test, 0, null);
        }
        static StringBuilder ToStringBuilder(ITokenTest test, int level, StringBuilder sb)
        {
            if(sb==null)
                sb = new StringBuilder();
            for (int i = 0; i < level; i++)
                sb.Append("\t");

            Type type = test.GetType();
            Type[] generic = type.GetGenericArguments();
            sb.AppendLine(type.Name + (generic.Length>0 ? generic[0].Name : string.Empty));
            if (test is BaseTestLinkage)
                foreach (var item in (BaseTestLinkage)test)
                {
                    ToStringBuilder(item, level + 1, sb);
                }


            return sb;
        }

        class RecursiveDictionary<Q> : Dictionary<char, RecursiveDictionary<Q>>
        {
            public Q Leaf;
            public string Operator;
            public bool IsTerminationChar=false;

            public Q Find(string expr)
            {
                RecursiveDictionary<Q> top = this;
                foreach (var ch in expr)
                    if (top.ContainsKey(ch))
                        top = top[ch];
                    else
                        return default(Q);

                return top.Leaf;
            }

            private RecursiveDictionary<Q> step;
            public void ResetTraversal()
            {
                step = this;
            }
            public RecursiveDictionary<Q> Traverse(char ch)
            {
                if (step == null)
                    step = this; // we can put this in constructor instead later, so we arent re-running code that only has significance during construction

                // RecursiveDictionary<Q> top = step;
                // if (step.ContainsKey(ch))
                //     step = step[ch];
                // else
                //     return null;
                step = step.NextNode(ch);

                return step;
            }

            virtual public RecursiveDictionary<Q> NextNode(char ch)
            {
                 if (this.ContainsKey(ch))
                     return this[ch];
                 else
                     return null;
            }
        }


        class LoopedTraversalNode<Q> : RecursiveDictionary<Q>
        {
            public List<char> ValidCharButNoTraversal = new List<char>();
            public List<char> InvalidChar = new List<char>();
            public override RecursiveDictionary<Q> NextNode(char ch)
            {
                if (this.ContainsKey(ch))
                    return this[ch];
                else
                {
                    if (InvalidChar != null) //dominant property, ignore valid char if used
                    {
                        if (InvalidChar.Contains(ch))
                            return null;
                        else
                            return this;
                    }
                    else
                    {
                        if (ValidCharButNoTraversal.Contains(ch))
                            return this; //now you have to plan for no movement
                        else 
                            return null;
                    }
                }
            }
        }

        static RecursiveDictionary<Type> getXpathExprIndexTree()
        {
            DefaultBuilder taglist = new DefaultBuilder();

            // include the parent test
            RecursiveDictionary<Type> tree = new RecursiveDictionary<Type>()
            {
                {'/', new RecursiveDictionary<Type>() { Leaf = typeof(ParentTest) } }
            };
            tree['/'].IsTerminationChar = true;


            tree.Add('L', new RecursiveDictionary<Type>()
            {
                {'I', new RecursiveDictionary<Type>()
                    {{'T', new RecursiveDictionary<Type>()
                        {{'E', new RecursiveDictionary<Type>()
                            {{'R', new RecursiveDictionary<Type>()
                                {{'A', new RecursiveDictionary<Type>()
                                    {{'L', new RecursiveDictionary<Type>() { Leaf = typeof(ElementTest<Literal>)} }}
                                }}
                            }}
                        }}
                    }}
                }
            });

            //add in the parsing tree for each tag identifier, which each to a map to ElementTest
            foreach(var tag in taglist.SupportedTags())
                if(tag != null)
                {
                    RecursiveDictionary<Type> reset = tree;
                    string keyword = tag.GetExpectedTagString();
                    if (keyword == null)
                        continue;
                    foreach (var ch in keyword)
                    {
                        if (!reset.ContainsKey(ch))
                            reset.Add(ch, new RecursiveDictionary<Type>());
                        reset = reset[ch];
                    }
                    Type genericized = typeof(ElementTest<>).MakeGenericType(tag.GetType());
                    reset.Leaf = genericized;

                    // reset.Leaf = new ElementTest<Tr>(); // really put the tag your looking for here
                }


            tree.Add('.', new RecursiveDictionary<Type>() { Leaf = typeof(ElementTest) });

            tree.Add('[', new RecursiveDictionary<Type>() { 
                {'@', new LoopedTraversalNode<Type>() },
                {'<', new RecursiveDictionary<Type>() },
                {'>', new RecursiveDictionary<Type>() },
                {'!', new RecursiveDictionary<Type>() }
            });
            tree['[']['@'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribTest) });
            tree['[']['@'].Add('=', new RecursiveDictionary<Type>()); //A1) attrib = value, waiting for " or 0-9
            tree['[']['@'].Add('<', new RecursiveDictionary<Type>()); //A2) attrib < value, waiting for " or 0-9
            tree['[']['@'].Add('>', new RecursiveDictionary<Type>()); //A3) attrib > value, waiting for " or 0-9.
            tree['[']['@'].Add('!', new RecursiveDictionary<Type>());
            tree['[']['@']['<'].Add('=', new RecursiveDictionary<Type>()); //A4) attrib <=, waiting for " or 0-9
            tree['[']['@']['>'].Add('=', new RecursiveDictionary<Type>()); //A5) attrib >=, waiting for " or 0-9
            tree['[']['@']['!'].Add('=', new RecursiveDictionary<Type>()); //A6) attrib !=, waiting for " or 0-9
            var numericchar = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            foreach (var num in numericchar)
            {
                //finish a1
                tree['[']['@']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal=numericchar });
                tree['[']['@']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator="=" });
                //finish a2
                tree['[']['@']['<'].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['<'][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = "<" });
                //finish a3
                tree['[']['@']['>'].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['>'][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = ">" });
                //finish a4
                tree['[']['@']['<']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['<']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = "<=" });
                //finish a5
                tree['[']['@']['>']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['>']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = ">=" });
                //finish a6
                tree['[']['@']['!']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['!']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = "!=" });
            }

            //attrib = string
            tree['[']['@']['='].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['=']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator="=" });
            //attrib <string
            tree['[']['@']['<'].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['<']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = "<" });
            //attrib >string
            tree['[']['@']['>'].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['>']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = ">" });
            //attrib >=string
            tree['[']['@']['>']['='].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['>']['=']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = ">=" });
            //attrib <=string
            tree['[']['@']['<']['='].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['<']['=']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = "<=" });
            //attrib !=string
            tree['[']['@']['!']['='].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['!']['=']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = "!=" });


            tree['['].IsTerminationChar = true;
            tree['[']['<'].Add('=', new RecursiveDictionary<Type>());
            tree['[']['>'].Add('=', new RecursiveDictionary<Type>());
            tree['[']['!'].Add('=', new RecursiveDictionary<Type>());
            foreach(var num in numericchar)
            {
                tree['['].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['['][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionTest) });

                //no operator, implcitly means equals

                tree['[']['<'].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['<'][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator = "<" });
                
                tree['[']['>'].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['>'][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator = ">" });

                tree['[']['<']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['<']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator="<=" });

                tree['[']['>']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['>']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator = ">=" });

                tree['[']['!']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['!']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator = "!=" });
            }
            tree['[']['@'].Add(' ', new RecursiveDictionary<Type>()
                                {{'C', new RecursiveDictionary<Type>()
                                    {{'O', new RecursiveDictionary<Type>()
                                        {{'N', new RecursiveDictionary<Type>()
                                            {{'T', new RecursiveDictionary<Type>()
                                                {{'A', new RecursiveDictionary<Type>()
                                                    {{'I', new RecursiveDictionary<Type>()
                                                        {{'N', new RecursiveDictionary<Type>()
                                                            {{'S', new RecursiveDictionary<Type>()
                                                                {{' ', new RecursiveDictionary<Type>()
                                                                    {{'\"', new LoopedTraversalNode<Type>()
                                                                        {{'\"', new RecursiveDictionary<Type>()
                                                                            {{']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribContainsTest)} }}
                                                                        }}
                                                                    }}
                                                                }}
                                                            }}
                                                        }}
                                                    }}
                                                }}
                                            }}
                                        }}
                                    }}
                                }});

            Type ortype = typeof(OrTest);
            tree.Add('(', new RecursiveDictionary<Type>() { Leaf = ortype });
            tree['('].IsTerminationChar = true;
            tree.Add(')', new RecursiveDictionary<Type>() { Leaf = ortype });
            tree[')'].IsTerminationChar = true;
            tree.Add('|', new RecursiveDictionary<Type>() { Leaf = ortype });
            tree['|'].IsTerminationChar = true;


            // attribute test [@name]
            // attribute test [@name="value"]
            // attribute test [@name!=0]
            // attribute test [@name contains "value"]
            // position test  [0], only if element test above
            // position test  [<0], [>=0], [!=0]
            // or test        (...|...)

            // any tag        .
            // any token      *
            
            // and test       element[0][name=""]
            // is different from xpath

            // xpath has ./@attrib to extract attribute.  None here for now
            // no "//" until I can figure it out what it means
            // only "table/tr/td/literal" or "/table/tr/td/literal"

            // override Traverse(char) for 
            //                              [@                  for ] to terminate
            //                                                  for = to upgrade to numeric test
            //                                                      for ] to terminate
            //                                                      for " to upgrade to string test
            //                                                          for "] to terminate
            //                              [@name!=            for ![0-9] to terminate
            //                              [@name contains "   for "] to terminate
            //                              [>=                 for only numbers and ] to terminate
            //                              [0                  for only numbers and ] to terminate

            /*
            RecursiveDictionary<ITokenTest> tree = new RecursiveDictionary<ITokenTest>()
            {
                {'t', new RecursiveDictionary<ITokenTest>() {
                    {'r', new RecursiveDictionary<ITokenTest>() { Leaf = null } }, //put tr element test there
                    {'d', new RecursiveDictionary<ITokenTest>() { Leaf = null } }, //put td element test there
                    {'h', new RecursiveDictionary<ITokenTest>() { Leaf = null } }, //put th element test there
                    {'a', new RecursiveDictionary<ITokenTest>() { 
                        {'b', new RecursiveDictionary<ITokenTest>() {
                            {'l', new RecursiveDictionary<ITokenTest>() {
                                {'e', new RecursiveDictionary<ITokenTest>() { Leaf = null }}, //put table element test there
                            }},
                        }},
                    }}, 
                }},
                {'h', null},
                {'t', null},
                {'b', null},
                {'i', null},
                {'/', null},
                {'/', null},
            };
            */
            return tree;
        }


    }

}
