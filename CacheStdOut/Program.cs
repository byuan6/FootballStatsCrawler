using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;

namespace CacheStdOut
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                showUsage();
                return 1;
            }

            string filename = args[0];
            if (File.Exists(filename))
                using (var fs = File.OpenRead(filename))
                using (var sr = new StreamReader(fs))
                    while (!sr.EndOfStream)
                        Console.WriteLine(sr.ReadLine());
            else
                using (var fs = File.OpenWrite(filename))
                using (var sw = new StreamWriter(fs))
                using (var p = new Process() { EnableRaisingEvents = true })
                {
                    int len = args.Length;
                    string cmd = args[1];
                    string arg = len==2 ? string.Empty : string.Join(" ", args, 2, len-2);
                    ProcessStartInfo info = new ProcessStartInfo(cmd, arg) { 
                        RedirectStandardOutput=true, UseShellExecute=true, CreateNoWindow=false, 
                    };
                    p.OutputDataReceived += new DataReceivedEventHandler(delegate(object sender, DataReceivedEventArgs e)
                    {
                        string data = e.Data;
                        Console.WriteLine(data);
                        sw.WriteLine(data);
                    });
                    p.Start();
                    p.WaitForExit();
                }

            return 0;
        }

        static void showUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("CacheStdOut [cachefile] [commands] [command arguments]");
        }

    }
}
