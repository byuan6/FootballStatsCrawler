using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

using FFToiletBowl;

namespace FFToiletBowlSQL
{
    class Program
    {

        static void Main(string[] args)
        {
            if(args.Length==0) {
                Console.WriteLine("Usage: FFToiletBowlSQL [data]");
                Console.WriteLine("Usage: FFToiletBowlSQL -?, to list datasets available");
                Console.WriteLine("Note:  If connection string is for LocalDB, then it will install the database if necessary (Even the LocalDB, in fact, if it doesn't detect it).");
                return;
            }
            string source = args[0];

            int relevant = DateTime.Now.Year;
            int year = 0;
            if (int.TryParse(source, out year) && year > relevant - 200 && year < relevant+200)
            {
                Reports.SetYear(year);
                Console.WriteLine("Saved " + year);
            }
            else
            {
                //getData(source);
                StringBuilder sb = new StringBuilder();
                foreach (var item in Reports.GetData(source))
                    sb.Append(item);
                Console.Write(sb.ToString());
            }
        }


    }
}
