using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Select
{
    class Program
    {
        static int Main(string[] args)
        {
            int lastarg = args.Length-1;
            if (lastarg == -1)
            {
                showUsage();
                return 1;
            }

            string filename = args[lastarg];
            IEnumerable<string> data = null;
            if (IsPiped())
            {
                data = readStdin();
                filename = null;
            }
            else if (File.Exists(filename))
                data = readFile(filename);

            if (data == null)
            {
                Console.WriteLine("File {0} not found AND no data from stdin", filename);
                return 2;
            }

            List<CopySelect> exprList = new List<CopySelect>();
            for (int i = 0; i <= lastarg; i++)
                if(i!=lastarg || filename==null)
                {
                    var item = args[i];
                    CopySelect field = new CopySelect(item);
                    exprList.Add(field);
                }

            string delimiter = "\t";
            var sourcedelimiter = new string[] { delimiter };
            bool isHeaderRow = true;
            string[] headers = null;
            try
            {
                var bom = @"∩╗┐";
                foreach (var row in data)
                {
                    var row2 = row;
                    if (row.StartsWith(bom)) row2 = row2.Substring(bom.Length);
                    if (isHeaderRow)
                    {
                        headers = row2.Split(sourcedelimiter, StringSplitOptions.None);
                        //Console.WriteLine("==>" + isHeaderRow + row2 + headers.Length + "," + exprList.Count);
                        string fielddelimiter = null;
                        foreach (var field in exprList)
                        {
                            field.Header = headers;
                            Console.Write("{1}{0}", field.Alias, fielddelimiter);
                            if (fielddelimiter == null) fielddelimiter = delimiter;
                        }
                        Console.WriteLine();
                        isHeaderRow = false;
                    }
                    else
                    {
                        string[] col = row.Split(sourcedelimiter, StringSplitOptions.None);
                        string fielddelimiter = null;
                        foreach (var field in exprList)
                        {
                            Console.Write("{1}{0}", field.ToString(col), fielddelimiter);
                            if(fielddelimiter==null) fielddelimiter = delimiter;
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 2;
            }


            return 0;
        }

        static void showUsage()
        {
            Console.WriteLine("Usage: Select [column] [file]");
            Console.WriteLine("file | Select [column]");
        }

        static bool IsPiped()
        {
            bool isPiped = false;
            try
            {
                var test = Console.KeyAvailable;
            }
            catch
            {
                isPiped = true;
            }
            return isPiped;
        }
        static public IEnumerable<string> readStdin()
        {
            bool isPiped = IsPiped();

            if (isPiped)
                while (Console.In.Peek() >= 0)
                {
                    var line = Console.ReadLine();
                    // Console.WriteLine("==> " + line);
                    yield return line;
                }
        }
        static public IEnumerable<string> readFile(string filename)
        {
            bool isFile = File.Exists(filename);

            if (isFile)
                foreach (var line in File.ReadAllLines(filename))
                    yield return line;
        }

        class CopySelect
        {
            public CopySelect(string expr)
            {
                this.Expr = expr;
                this.Alias = expr;
            }

            public string[] Header
            {
                set
                {
                    string[] header = value;
                    var expr = this.Expr;
                    int num = 0;
                    if (int.TryParse(expr, out num))
                    {
                        this.Column = num;
                    }
                    else if (header != null)
                    {
                        num = Array.IndexOf<string>(header, expr);
                        if (num >= 0)
                            this.Column = num;
                        else
                            throw new ArgumentException(string.Format("Column specified in arguments {0}, missing in data header", expr));
                    }
                    else
                        throw new ArgumentException(string.Format("No header. Unable to column {0} in data", expr));
                }
            }

            public string Alias { get; private set; }
            public string Expr { get; private set; }
            public int Column { get; private set; }

            public string ToString(string[] row)
            {
                var data = row[this.Column];
                return data;
            }
        }
    }
}
