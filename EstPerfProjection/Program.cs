using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EstPerfProjection
{
    class Program
    {
        /// <summary>
        /// Show me 
        /// 1.changes to players projected end+thru automated formula+thru manual changes 
        /// 2.players total projected end-changes / games 
        /// 3. new player coeffic*defense %+playeravg 
        /// 5. show history of prediction for player and stdev
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int year;
            int wk;
            if (args.Length < 2)
            {
                Console.WriteLine("EstPerfProjection [year] [week]");
                return;
            }
            if (int.TryParse(args[1], out wk) && wk==1 && args.Length < 3)
            {
                Console.WriteLine("EstPerfProjection [year] 1 [url of website with rankings]");
                Console.WriteLine();
                Console.WriteLine("EstPerfProjection   has no basis to determine this year's projections, ");
                Console.WriteLine("except last years data, which is unreliable as things change between seasons.  ");
                Console.WriteLine("It requires that you submit a url with ranking data, so it can estimate");
                Console.WriteLine("what experts believe is their projected output.  And creates the season");
                Console.WriteLine("projections for each player, based on that.  And derives game estimates.");
                return;
            }
        }
    }
}
