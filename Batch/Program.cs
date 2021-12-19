using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;

using ReadEnvSample;

namespace Batch
{
    public class Program
    {
        static string CMD = "cmd";

        /// <summary>
        /// Next steps are to read
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args) 
        {
            if (!isCmdInPath())
            {
                Console.WriteLine("Command interpreter [cmd] not found");
                return 999;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: Batch [.bat file] [.bat file that can be executed in parallel]");
                Console.WriteLine("Note: If it had to be executed in sequence, it can be in the batch file");
                Console.WriteLine("Errata: Will not handle - early exit /b 0 statements correctly");
                Console.WriteLine("Errata: Will not handle - goto label: statements correctly");
                Console.WriteLine("Errata: Will not handle - if and for loop blocks correctly");
                Console.WriteLine("Errata: Environment variable settings is erratic.  You may have to check in subsequent lines to make sure it was set");
                Console.WriteLine("Errata: MUST HAVE write permission to folder where batch file is located");
                Console.WriteLine("Note: What it WILL DO... restart a linear pipeline of commands, if a single one has failed with non-zero exit code, from where it failed.  Fix the problem and restart it.  So you don't have to write a whole bunch of [if exitcode!=0 subroutines].");
                Console.WriteLine("Note: Changing the batch file before where it failed, is NOT recommended.  It WILL continue to retry starting from old byte offset.  This is by design, so you can change the batch file if there was a problem.  The alternative is to have the program detect for modifications in batch file, and restart it");
                Console.WriteLine("Note: If you wish to restart from beginning, delete the [.bat].progress file");

                return 1;
            }

            if (args.Length == 1)
                if (args[0].EndsWith(".bat") || args[0].EndsWith(".cmd"))
                    Run(args[0]);
                else
                {
                    Console.WriteLine("Usage: Batch [.bat file] [.bat file that can be executed in parallel]");
                    Console.WriteLine("Error batch file must end in .bat or .cmd");
                    return 2;
                }
            else
            {
                foreach (var batch in args)
                    if (batch.EndsWith(".bat") || batch.EndsWith(".cmd"))
                        Run(batch);
            }

            return 0;
        }

        static int logpending = 0;
        static int Run(string batch)
        {
            string filename = batch + ".progress";
            string log = batch + ".log";
            int linenumber = 0;

            //read progress file and re-run
            long offset = getRerunLine(filename, out linenumber);

            using (FileStream ls = new FileStream(log, FileMode.Append, FileAccess.Write, FileShare.Read, 512, true))
            using (FileStream ps = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (StreamReader sr = File.OpenText(batch))
            {
                bool skip = false;
                if (offset != 0)
                {
                    sr.BaseStream.Position = offset;
                    skip = true;
                }

                while (!sr.EndOfStream)
                {
                    int exit = 0;

                    //read batch line
                    string line = sr.ReadLine();
                    int continuation = 1;
                    while (line.EndsWith("^"))
                    {
                        line = line.Substring(0, line.Length - 1) + sr.ReadLine();
                        continuation++;
                    }
                    line = line.Replace("%%", "%");

                    //execute command
                    if (!string.IsNullOrWhiteSpace(line))
                        using (Process proc = new Process() { StartInfo = new ProcessStartInfo(CMD, string.Format("/c {0}", line)) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput=true } })
                        {
                            if (!skip)
                            {
                                //save current line to progress file
                                byte[] progress = BitConverter.GetBytes((int)offset);
                                ps.Write(progress, 0, progress.Length);
                                linenumber += continuation;
                                skip = false;
                            }
                            else
                            {
                                string cont = string.Format("{0,-4}... [{1:M/d/yyyy HH:mm:ss}] Failure Continuation detected, starting at [{2}] \r\n", linenumber, DateTime.Now, offset);
                                logToFile(ls, cont);
                            }

                            //run file
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(line);
                            Console.ResetColor();

                            exit = startProcessAndPollForEnvironmentVariables(proc);
                            
                            Console.Write("--> ");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine(proc.StandardOutput.ReadToEnd());
                            Console.ResetColor();

                            //log results
                            string s = string.Format("{0,-4}... [{1:M/d/yyyy HH:mm:ss}] Exit[{2}] Offset[{4}] [{3}]\r\n", linenumber, DateTime.Now, exit, line, offset);
                            logToFile(ls, s);

                            //error condition detected
                            if (exit != 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("Error returned by process");
                                Console.WriteLine(exit);
                                Console.ResetColor();

                                string error = string.Format("{0,-4}... [{1:M/d/yyyy HH:mm:ss}] Exiting\r\n", linenumber, DateTime.Now);
                                logToFile(ls, error);

                                return exit;
                            }

                            //track beginning of next line to be read
                            offset += sr.CurrentEncoding.GetByteCount(line) + (Environment.NewLine.Length * continuation);
                        }
                }
            }

            //remove progress file when completed
            File.Delete(filename);
            return 0;
        }

        static int startProcessAndPollForEnvironmentVariables(Process proc)
        {
            int exit = 0;
            proc.Start();

            //check for environment variable changes
            var env = proc.ReadEnvironmentVariables();
            proc.Exited += new EventHandler(delegate(object sender, EventArgs e) { try { env = proc.ReadEnvironmentVariables(); } finally { } });
            proc.EnableRaisingEvents = true;

            while (!proc.HasExited)
                try
                {
                    env = proc.ReadEnvironmentVariables();
                }
                finally { }

            proc.WaitForExit();
            exit = proc.ExitCode;
            foreach (System.Collections.DictionaryEntry item in env)
                if (Environment.GetEnvironmentVariable((string)item.Key) != (string)item.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("VAR>");
                    Console.Write(item.Key);
                    Console.Write("=");
                    Console.WriteLine(item.Value);
                    Console.ResetColor();
                    Environment.SetEnvironmentVariable((string)item.Key, (string)item.Value);
                }

            return exit;
        }

        static void logToFile(FileStream openfileStream, string s)
        {
            byte[] logline = ASCIIEncoding.ASCII.GetBytes(s);
            logpending++;
            openfileStream.BeginWrite(logline, 0, logline.Length, new AsyncCallback(endlog), null); //new AsyncCallback(EndWriteCallback), null);
        }

        static void endlog(IAsyncResult target)
        {
            logpending--;
        }

        static int getRerunLine(string progressfile, out int linenumber)
        {
            linenumber = 0;
            if(!File.Exists(progressfile))
                return 0;

            int last=0;
            long size = (new FileInfo(progressfile)).Length;
            if (size > int.MaxValue)
                throw new OverflowException("Does not support batch files more than " + (int.MaxValue/4) + " lines");
            if (size == 0)
                return 0;

            if (size % 4 == 0)
                last = (int)size - 4;
            else
            {
                //long remainder = size%4; 
                last = (((int)size / 4) * 4); //incomplete progress
                using (FileStream fs = File.OpenWrite(progressfile))
                    fs.SetLength(last);
                last -= 4;
            }
            linenumber = last / 4;

            if (last < 0)
            {
                File.Delete(progressfile);
                linenumber = 0;
                return 0; //not a single completed progress line
            }

            int rerun=0;
            using (FileStream fs = File.OpenRead(progressfile))
            {
                fs.Position = last;
                byte[] buffer = new byte[4];
                int length = fs.Read(buffer, 0, buffer.Length);
                if (length != buffer.Length)
                    throw new InvalidOperationException("Expecting a full Int32 returned from file");
                rerun = BitConverter.ToInt32(buffer, 0);
            }

            if (rerun==0 || rerun >= size)
            {
                File.Delete(progressfile); //it completed last time
                linenumber = 0;
                return 0;
            } 
            else
                return rerun;
        }

        static public bool isCmdInPath() 
        {
            using (Process proc = new Process() { StartInfo = new ProcessStartInfo(CMD, "/c") { UseShellExecute = true, CreateNoWindow = true, WindowStyle= ProcessWindowStyle.Hidden } })
            {
                proc.Start();
                proc.WaitForExit();
                return proc.ExitCode==0;
            }
        }
    }
}
