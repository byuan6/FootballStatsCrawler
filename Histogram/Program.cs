using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Histogram
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: [/header | /names \"List of names, comma delimited\"] [column index | column name w /header]... [/number] [bin1] [bin2]... [/date] [bin1] [bin2]...");
                Console.WriteLine("Returns: [column value] [column count] [column2 count]");
                Console.WriteLine("Expects comma delimited data, crlf delimited row, passed in by stdin");
                Console.WriteLine("Column Names cannot be numbers, or it will assume you mean column index");
                Console.WriteLine("[/header] 1st row contains column name, should be first");
                Console.WriteLine("[/number] or [/date], not both AND occurs after list of columns to count");
                Console.WriteLine("[/number] [boundaries for bins], or /number 0 10 20 means bins ...0, 0..10, 10..20, 20..30, 30...");
                Console.WriteLine("[/date] [boundaries for bins], or /date 11-28-2015 12-24-2015 01-01-2015 means bins");
                Console.WriteLine("        ...11-28-2015, 11-28-2015..12-24-2015, 12-24-2015...01-01-2015, 01-01-2015...");
                return 1;
            }

            bool isColumnHeaders = false;
            string[] columnNameList = null;
            int argcount = args.Length;
            int startcol=0;
            if (args[0].ToLower() == "/header")
            {
                isColumnHeaders = true;
                startcol = 1;
            }
            else if (args[0].ToLower() == "/names")
            {
                if(argcount<2) {
                    Console.Error.WriteLine("Usage: /names \"name for column1,name for column2,name for column3,etc\"]");
                    return 1;
                }
                isColumnHeaders = false;
                columnNameList = args[0].Split(',');
                startcol = 2;
            } else if(startcol<argcount) {
                Console.Error.WriteLine("Usage: [column index | column name w /header flag]");
                return 1;
            }

            string lower = null;
            bool isFixedBin = false;
            List<string> columnArgs = new List<string>();
            for (int i = startcol; i < argcount; i++)
            {
                lower=args[i].ToLower();
                if (lower == "/number" || lower == "/date")
                {
                    isFixedBin = true;
                    startcol = i;
                    break;
                }
                columnArgs.Add(args[i]);
            }

            int columncount = columnArgs.Count;
            Dictionary<string, Proxy>[] dynamicStringBin = new Dictionary<string, Proxy>[columncount];
            for (int i = 0; i < columncount; i++) dynamicStringBin[i] = new Dictionary<string, Proxy>();
            //Dictionary<int, Dictionary<string, Proxy>> dynamicStringBin = new Dictionary<int, Dictionary<string, Proxy>>();
            //foreach(var item in columnArgs) dynamicStringBin.Add(item,)

            List<DateTime> fixeddateBin = null;
            List<double> fixednumBin = null;
            if (lower == "/number")
            {
                fixednumBin = new List<double>() { double.MaxValue };
                for (int i = startcol + 1; i < argcount; i++)
                {
                    double tmp;
                    if (double.TryParse(args[i], out tmp))
                        //for (int j = 0; j < columncount; j++)
                        {
                            //if (fixednumBin[j] == null) fixednumBin[j] = new Dictionary<double, Proxy>() { { double.MaxValue, new Proxy() } };
                            fixednumBin.Add(tmp);
                        }
                    else
                    {
                        Console.Error.WriteLine("Usage: /number [bins have to be numbers]");
                        return 1;
                    }
                }
            }
            else if (lower == "/date")
            {
                fixeddateBin = new List<DateTime>() { DateTime.MaxValue };
                for (int i = startcol; i < argcount; i++)
                {
                    DateTime tmp;
                    if (DateTime.TryParse(args[i], out tmp))
                        //for (int j = 0; j < columncount; j++)
                        {
                            //if (fixeddateBin[j] == null) fixeddateBin[j] = new Dictionary<DateTime, Proxy>() { { DateTime.MaxValue, new Proxy() } };
                            fixeddateBin.Add(tmp);
                        }
                    else
                    {
                        Console.Error.WriteLine("Usage: /date [bins have to be MM-DD-YYYY]");
                        return 1;
                    }
                }
            }
            fixeddateBin = fixeddateBin == null ? null : fixeddateBin.OrderBy<DateTime, DateTime>(s => s).ToList<DateTime>();
            fixednumBin = fixednumBin == null ? null : fixednumBin.OrderBy<double, double>(s => s).ToList<double>();
            string prev = ".";
            List<string> fixeddateBinNames = fixeddateBin==null ? null : fixeddateBin
                                            .Select<DateTime, string>(s => { string t = prev + ".." + (s == DateTime.MaxValue ? "." : s.ToString()); prev = s.ToString(); return t; })
                                            .ToList<string>();
            prev = ".";
            List<string> fixednumBinNames = fixednumBin == null ? null : fixednumBin
                                            .Select<double, string>(s => { string t = prev + ".." + (s==double.MaxValue ? "." : s.ToString()); prev = s.ToString(); return t; })
                                            .ToList<string>();
            
            int[] resolvedToColumnIndex = getColumnIndexFromName(columnArgs, columnNameList, false);
            if (columnNameList!=null && resolvedToColumnIndex == null)
            {
                //Console.WriteLine("123456789012345678901234567890123456789012345678901234567890123456789012345678");
                Console.Error.WriteLine("Usage:[columnname1] [columnindex2] /names \"columnname1,columnname2,etc\"]");
                Console.Error.WriteLine("      the columnname has to exist in the names list");
                Console.Error.WriteLine("   ...[columnname1] [columnindex2] specifies which columns are to be counted");
                Console.Error.WriteLine("      but can specify column number instead");
                Console.Error.WriteLine("      If use column name, /names needs to be specified");
                Console.Error.WriteLine("      Or /headers flagged and the file has to have header row w/ column names");
                Console.Error.WriteLine("   .../names \"columnname1,columnname2,columnname3,etc\"]");
                Console.Error.WriteLine("      specifies the name of the column in the order it appears");
                    
                return 1;
            }

            //add bins for the fixed bins...they all have to show up in output
            if (fixeddateBinNames != null)
                for (int i = 0; i < columncount; i++)
                    foreach (var bin in fixeddateBinNames)
                        if (!dynamicStringBin[i].ContainsKey(bin))
                            dynamicStringBin[i].Add(bin, new Proxy() { BinCount = 0 });
            if (fixednumBinNames != null)
                for (int i = 0; i < columncount; i++)
                    foreach (var bin in fixednumBinNames)
                        if (!dynamicStringBin[i].ContainsKey(bin))
                            dynamicStringBin[i].Add(bin, new Proxy() { BinCount = 0 });

            foreach (var record in readDataFromConsole())
            {
                Dictionary<string, Proxy> binAndCounts = null;
                string value = null;
                if (isColumnHeaders)
                {
                    columnNameList = record; //first record is considered column name list
                    resolvedToColumnIndex = getColumnIndexFromName(columnArgs, record, true);
                    if (resolvedToColumnIndex == null)
                    {
                        Console.Error.WriteLine("Error in file: Header record's column names can never a integer");
                        return 2;
                    }
                    isColumnHeaders = false;
                }
                else
                    for(int i=0; i<columncount; i++)
                    {
                        var field = resolvedToColumnIndex[i]; //i is arg index, which represent which file column to count
                        binAndCounts = dynamicStringBin[i];

                        if (field >= record.Length) //no guarantee the file is good
                            break; //if not, move to next column

                        if (!isFixedBin)
                            value = record[field];
                        else if (fixednumBin != null)
                        {
                            double tmp = 0;
                            if (double.TryParse(record[field], out tmp))
                            {
                                int bin = fixednumBin.Count<double>(s => tmp > s);
                                value = fixednumBinNames[bin];
                            }
                            else
                                break; //not expected datatype, but dont abort.  move on to next one.
                        }
                        else if (fixeddateBin != null)
                        {
                            DateTime tmp = DateTime.MinValue;
                            if (DateTime.TryParse(record[field], out tmp))
                            {
                                int bin = fixeddateBin.Count<DateTime>(s => tmp > s);
                                value = fixeddateBinNames[bin];
                            }
                            else
                                break; //not expected datatype, but dont abort.  move on to next one.
                        }
                        else
                            throw new Exception("What's the deal here?  Did you change something and break the program?  It has to be dynamic string bin (picks it up from data), or fixed bins.  But fixed bins lists are all null.");

                        //check somewhere that the column index was not repeated
                        if (binAndCounts.ContainsKey(value))
                            binAndCounts[value].BinCount++;
                        else
                            binAndCounts.Add(value, new Proxy() { BinCount = 1 });
                    }
            }

            //output results
            Console.Write("Bin");
            for (int i = 0; i < columncount; i++)
            {
                Console.Write("\t");
                Console.Write(columnArgs[i]);
            }
            Console.WriteLine();
            foreach (var binName in dynamicStringBin[0].Keys)
            {
                Console.Write(binName);
                for(int i=0; i<columncount; i++)
                {
                    //var columnindex = resolvedToColumnIndex[i];
                    Dictionary<string, Proxy> binAndCounts = dynamicStringBin[i];
                    int counts = binAndCounts[binName].BinCount;
                    Console.Write("\t");
                    Console.Write(counts);
                }
                Console.WriteLine();
            }
            return 0;
        }



        static IEnumerable<string[]> readDataFromConsole()
        {
            using (TextReader reader = System.Console.In)
            {
                while (reader.Peek() >= 0)
                {
                    yield return reader.ReadLine().Split('\t');
                }
            }
        }

        static int[] getColumnIndexFromName(List<string> args, string[] columnNameList, bool isFileHeader)
        {
            int number=0;
            if (columnNameList == null) return null;
            for (int i = 0; i < columnNameList.Length; i++)
                if (int.TryParse(columnNameList[i], out number))
                    return null;

            int column=0;
            int count = args.Count;
            int[] returnValue = new int[count];
            for (int i = 0; i < count; i++)
            {
                if (!int.TryParse(args[i], out column))
                    column = indexOf(columnNameList, args[i]);
                
                if (column >= 0)
                    returnValue[i] = column;
                else
                    return null;
            }

            return returnValue;
        }

        static int indexOf(string[] list, string value)
        {
            int count = list.Length;
            for (int i = 0; i < count; i++)
                if(list[i]==value)
                    return i;
            return -1;
        }
    }

    public class Proxy
    {
        public int BinCount;
    }
}
