using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;

namespace HtmlTable
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: [header records=true/false:optional] [colspan:optional] [rowspan:optional] [index] [index] ...");
                Console.WriteLine("Returns: html table to stdout");
                return 1;
            }

            int argcount = args.Length;
            int startcol;
            if (argcount>0 && int.TryParse(args[0], out startcol))
                startcol = 0;
            else if (argcount > 1 && int.TryParse(args[1], out startcol))
                startcol = 1;
            else if (argcount > 2 && int.TryParse(args[2], out startcol))
                startcol = 2;
            else if (argcount > 3 && int.TryParse(args[3], out startcol))
                startcol = 3;
            else
                startcol = 4;

            if (startcol >= argcount)
            {
                Console.WriteLine("Usage: [header records=true/false:optional] [colspan:optional] [rowspan:optional] [index] [index] ...");
                Console.WriteLine("No output column indexes");
                return 1;
            }

            //charttype : default is column
            //Series : default is string(X), number(Y)
            //datatype in series definition : string, number, date
            List<TextSegmentator> segmentorList = new List<TextSegmentator>();
            List<DataPoint> converterList = new List<DataPoint>();

            TextSegmentator segmentator = null;
            int value;
            for(int i=startcol; i<args.Length; i++)
                if (int.TryParse(args[i], out value))
                {
                    if (segmentator == null)
                    {
                        segmentator = new TextSegmentator() { Source = value };
                        segmentorList.Add(segmentator);
                    }
                    else
                    {
                        TextSegmentator tmp = segmentator.NewInstance();
                        tmp.Source = value;
                        segmentorList.Add(tmp);
                    }

                    DataPoint point = new DataPoint() { ValueType = DataPoint.DataType.tostring };
                    converterList.Add(point);
                }

            

            System.Diagnostics.Debug.Assert(segmentorList.Count == converterList.Count);



            Console.Write("<table>");
            string colspan = args.Length >= 2 && startcol > 1 ? args[1] : null;
            string rowspan = args.Length >= 3 && startcol > 2 ? args[2] : null;
            bool pickupHeader = args.Length >= 1 ? args[0].ToLower() == "true" : false;
            int length = segmentorList.Count;
            if (colspan != null)
                if (rowspan != null)
                    Console.WriteLine("<tr><th>&nbsp;</th><th colspan=\"{0}\">{1}</th></tr>", length, colspan);
                else
                    Console.WriteLine("<tr><th colspan=\"{0}\">{1}</th></tr>", length, colspan);
            foreach (var record in readDataFromConsole())
            {
                StringBuilder output = new StringBuilder();
                bool isValid = true;
                segmentorList[0].Raw = record; //all the other get it's data from the first
                if (!pickupHeader)
                {
                    for (int index = 0; index < length; index++)
                    {
                        converterList[index].IsValid = true;
                        converterList[index].Value = segmentorList[index].Value();
                        output.AppendFormat("<td>{0}</td>", converterList[index].Convert());
                        isValid = isValid && converterList[index].IsValid && (converterList[index].Convert() != null);
                    }
                }
                else
                {
                    if (rowspan != null)
                    {
                        output.AppendFormat("<td rowspan=\"{0}\">{1}</td>", 999, rowspan);
                        rowspan = null;
                    }
                    for (int index = 0; index < length; index++)
                    {
                        output.AppendFormat("<th>{0}</th>", segmentorList[index].Value());
                    }
                    pickupHeader = false;
                }
                if (isValid)
                {
                    Console.Write("<tr>");
                    Console.Write(output.ToString());
                    Console.Write("</tr>");
                    Console.WriteLine();
                }
            }
            Console.Write("</table>");

            return 0;
        }




        static IEnumerable<string> readDataFromConsole()
        {
            using (TextReader reader = System.Console.In)
            {
                while (reader.Peek() >= 0)
                {
                    yield return reader.ReadLine();
                }
            }
        }

    }
}
