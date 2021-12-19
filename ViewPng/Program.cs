using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using System.IO;


namespace ViewPng
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ViewPNG() { IniFilename = getStdIn() });
        }


        static string getStdIn()
        {
            using (TextReader reader = System.Console.In)
            {
                string output = reader.ReadToEnd();
                if (output.StartsWith("Created "))
                    return output.Substring(8);
                else if (output.Length > 0)
                    return output;
                else
                    return null;
            }
        }

    }
}
