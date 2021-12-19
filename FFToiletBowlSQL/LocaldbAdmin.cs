using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;

namespace FFToiletBowlSQL
{
    public class LocaldbAdmin
    {

        public bool IsInstalled
        {
            get
            {
                //sqllocaldb.exe
                const int INFO_ERROR_CODE = -1983577836;
                const int NOT_IN_PATH_ERROR_CODE = 9009;

                using (var p = new Process() { StartInfo = new ProcessStartInfo() { UseShellExecute = true, FileName = @"SqlLocalDB.exe" } })
                {
                    p.Start();
                    p.WaitForExit();
                    var x = p.ExitCode;
                    if (x == NOT_IN_PATH_ERROR_CODE)
                        return false;
                    if (x == INFO_ERROR_CODE)
                    {
                        this.SqlLocalDbExeFilepath = FindExePath(@"SqlLocalDB.exe");
                        if(!string.IsNullOrWhiteSpace(this.SqlLocalDbExeFilepath))
                            return true;
                        throw new ApplicationException("Got the expected code, for info screen.  Can't find SqlLocalDB.exe in %PATH%");
                    }
                    throw new ApplicationException("Requested info screen.  Didn't get the expected code, for info screen.  Something is installed that returned a code for SqlLocalDB, but it didn't return the correct code");
                }

            }
        }

        public string SqlLocalDbExeFilepath { get; private set; }

        public void Install()
        {
            using (var p = new Process() { StartInfo = new ProcessStartInfo() { FileName=@"LocalDBInstaller\SqlLocalDB.msi" } })
            {
                p.Start();
                p.WaitForExit();
            }
        }


        public string OriginalConnectionString { get { return _connect; } }
        string _connect;
        public void ParseConnectionString(string connectionstring)
        {
            _connect = connectionstring;
            //Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=aspnet-MvcMovie;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\Movies.mdf
            var parts = connectionstring.Split(';');
            foreach (var pair in parts)
            {
                if (pair.StartsWith("Data Source="))
                {
                    this.DataSource = pair.Substring("Data Source=".Length);
                    this.InstanceName = this.DataSource.Substring(this.DataSource.IndexOf("\\") + 1);
                }
                if(pair.StartsWith("Initial Catalog="))
                    this.DatabaseName = pair.Substring("Initial Catalog=".Length);
                if(pair.StartsWith("AttachDBFilename="))
                    this.MdfFilename = pair.Substring("AttachDBFilename=".Length);
            }
            //return this.List.Contains(this.InstanceName);
        }

        public string DataSource { get; set; }
        public string InstanceName { get; set; }
        public string DatabaseName { get; set; }
        public string MdfFilename { get; set; }
        //https://d-fens.ch/2017/01/19/howto-create-localdb-file-mdf-manually-in-visual-studio-2015/
        //post indicates it will create MDF file automatically, if it doesn't exist

        public bool IsForLocalDB
        {
            get
            {
                return this.DataSource.ToLower().StartsWith("(localdb)\\");
            }
        }

        string ExpandedMdfFilename
        {
            get
            {
                var filename = this.MdfFilename;
                if (filename.Contains("|DataDirectory|"))
                {
                    var dir = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                    if (!string.IsNullOrWhiteSpace(dir))
                        filename = filename.Replace("|DataDirectory|", dir);
                }
                return filename;
            }
        }
        public bool MdfExists
        {
            get
            {
                return File.Exists(this.ExpandedMdfFilename);
            }
        }
        public bool IsDatabaseLive
        {
            get
            {
                try
                {
                    using(var cn = new System.Data.SqlClient.SqlConnection(_connect))
                    {
                        cn.Open();
                        using (var cm = new System.Data.SqlClient.SqlCommand("SELECT count(0) FROM sys.tables", cn))
                            cm.ExecuteScalar();
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Create() 
        {
            //  create|c ["instance name" [version-number] [-s]]
            //Creates a new LocalDB instance with a specified name and version
            //If the [version-number] parameter is omitted, it defaults to the
            //latest LocalDB version installed in the system.
            //-s starts the new LocalDB instance after it's created


            using (var p = new Process() { StartInfo = new ProcessStartInfo() { UseShellExecute = false, FileName = this.SqlLocalDbExeFilepath, Arguments = "install " + this.InstanceName } })
            {
                p.Start();
                p.WaitForExit();
            }
        }
        public void Delete() {
            //delete|d ["instance name"]
            //Deletes the LocalDB instance with the specified name

            using (var p = new Process() { StartInfo = new ProcessStartInfo() { UseShellExecute = false, FileName = this.SqlLocalDbExeFilepath, Arguments = "delete " + this.InstanceName } })
            {
                p.Start();
                p.WaitForExit();
            }
        }
        public void Start()
        {
            //start|s ["instance name"]
            //Starts the LocalDB instance with the specified name

            using (var p = new Process() { StartInfo = new ProcessStartInfo() { UseShellExecute = false, FileName = this.SqlLocalDbExeFilepath, Arguments = "start " + this.InstanceName } })
            {
                p.Start();
                p.WaitForExit();
            }
        }
        public void Stop()
        {
            //stop|p ["instance name" [-i|-k]]
            //Stops the LocalDB instance with the specified name,
            //after current queries finish
            //-i request LocalDB instance shutdown with NOWAIT option
            //-k kills LocalDB instance process without contacting it

            using (var p = new Process() { StartInfo = new ProcessStartInfo() { UseShellExecute = false, FileName = this.SqlLocalDbExeFilepath, Arguments = "stop " + this.InstanceName } })
            {
                p.Start();
                p.WaitForExit();
            }
        }



        string[] _list;
        public string[] List 
        {
            //info|i
            //  Lists all existing LocalDB instances owned by the current user
            //  and all shared LocalDB instances.
            get
            {
                if (_list == null)
                    using (var p = new Process() { StartInfo = new ProcessStartInfo() { UseShellExecute = false, FileName = this.SqlLocalDbExeFilepath, Arguments = "info", RedirectStandardOutput = true } })
                    {
                        p.Start();

                        // Synchronously read the standard output of the spawned process. 
                        using (StreamReader reader = p.StandardOutput)
                        {
                            string output = reader.ReadToEnd();
                            p.WaitForExit();

                            if (p.ExitCode == 0)
                                return _list = output.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                            return null;
                        }
                    }
                return _list;
            }
        }


        public InstanceInfo Info(string instance)
        {
            //info|i "instance name"
            //Prints the information about the specified LocalDB instance.
            using (var p = new Process() { StartInfo = new ProcessStartInfo() { UseShellExecute = false, FileName = this.SqlLocalDbExeFilepath, Arguments = "info " + instance, RedirectStandardOutput = true } })
            {
                p.Start();

                // Synchronously read the standard output of the spawned process. 
                using (StreamReader reader = p.StandardOutput)
                {
                    string output = reader.ReadToEnd();
                    p.WaitForExit();

                    if (p.ExitCode == 0)
                        return new InstanceInfo(output);
                    return null;
                }
            }
        }

        string[] _versions;
        public string[] Versions 
        {
            get
            {
                //versions|v
                //Lists all LocalDB versions installed on the computer.
                if (_versions==null)
                    using (var p = new Process() { StartInfo = new ProcessStartInfo() { UseShellExecute = false, FileName = this.SqlLocalDbExeFilepath, Arguments = "v", RedirectStandardOutput=true } })
                    {
                        p.Start();

                        // Synchronously read the standard output of the spawned process. 
                        using (StreamReader reader = p.StandardOutput)
                        {
                            string output = reader.ReadToEnd();
                            p.WaitForExit();

                            if (p.ExitCode == 0)
                                return _versions = output.Split(new string[] {"\n", "\r"}, StringSplitOptions.RemoveEmptyEntries);
                            return null;
                        }
                    }
                return _versions;
            }
        }

        /// <summary>
        /// http://csharptest.net/526/how-to-search-the-environments-path-for-an-exe-or-dll/index.html
        /// Expands environment variables and, if unqualified, locates the exe in the working directory
        /// or the evironment's path.
        /// </summary>
        /// <param name="exe">The name of the executable file</param>
        /// <returns>The fully-qualified path to the file</returns>
        /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
        public static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            {
                if (Path.GetDirectoryName(exe) == String.Empty)
                {
                    foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                    {
                        string path = test.Trim();
                        if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                            return Path.GetFullPath(path);
                    }
                }
                throw new FileNotFoundException(new FileNotFoundException().Message, exe);
            }
            return Path.GetFullPath(exe);
        }

    }

    public class InstanceInfo
    {
        public InstanceInfo() { }
        public InstanceInfo(string output) { this.Parse(output); }

        public string Name;
        public string Version;
        public string Shared;
        public string Owner;
        public string AutoCreate;
        public string State;
        public string LastStart;
        public string Pipename;

        public void Parse(string output)
        {
            //Name:               MSSQLLocalDB
            //Version:            14.0.1000.169
            //Shared name:
            //Owner:              FujiBookPro-Win\Bob
            //Auto-create:        Yes
            //State:              Stopped
            //Last start time:    5/17/2019 2:17:48 PM
            //Instance pipe name:
            var parts = output.Split(new string[] {"\n","\r"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in parts)
                if (pair.StartsWith("Name:"))
                    this.Name = pair.Substring(20);
                else if (pair.StartsWith("Version:"))
                    this.Version = pair.Substring(20);
                else if (pair.StartsWith("Shared name:"))
                    this.Shared = pair.Substring(20);
                else if (pair.StartsWith("Owner:"))
                    this.Owner = pair.Substring(20);
                else if (pair.StartsWith("Auto-create:"))
                    this.AutoCreate = pair.Substring(20);
                else if (pair.StartsWith("State:"))
                    this.State = pair.Substring(20);
                else if (pair.StartsWith("Last start time:"))
                    this.LastStart = pair.Substring(20);
                else if (pair.StartsWith("Instance pipe name:"))
                    this.Pipename = pair.Substring(20);
        }
    }

}
