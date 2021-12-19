using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumList
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Produces space delimited list of numbers 1 to [specified argument]");
                Console.WriteLine("Please specify end of range");
                return 1;
            }

            int end = -1;
            if (!int.TryParse(args[0], out end))
            {
                Console.WriteLine("Please specify a number greater than 1, as argument");
                return 1;
            }

            for (int i = 1; i <= end; i++)
                Console.Write("{0} ", i);
            Console.WriteLine();

            return 0;
        }
    }
}
