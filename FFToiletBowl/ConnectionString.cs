using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;

namespace FFToiletBowl
{
    public class ConnectionString
    {
        static string __connectionstring = null;
        static public string GetConnectionString()
        {
            if (!string.IsNullOrWhiteSpace(__connectionstring))
                return __connectionstring;

            if (string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.GetData("DataDirectory") as string))
                AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

            string name = ConfigurationManager.AppSettings["Database"];
            if (name != null)
            {
                var entry = ConfigurationManager.ConnectionStrings[name];
                if (entry != null)
                {
                    var connect = entry.ConnectionString;
                    if (entry.ProviderName != "System.Data.SqlClient")
                    {
                        Console.WriteLine("This supports SQL server at this time!");
                        return null;
                    }
                    if (!string.IsNullOrWhiteSpace(connect))
                    {
#if DEBUG
                        Console.WriteLine("Debug connection string:{0}", connect);
#endif
                        LocaldbAdmin localdb = new LocaldbAdmin();
                        localdb.ParseConnectionString(connect);
                        if (localdb.IsForLocalDB)
                        {
                            if (!localdb.IsInstalled)
                            {
                                Console.WriteLine("SQL Server LocalDB is not installed.  Your connectionstring is for a LocalDB database.");
                                Console.WriteLine("Do you wish to install (Y/N)?");
                                var yn = Console.ReadLine();
                                if (yn.ToLower().StartsWith("y"))
                                    localdb.Install();
                                else
                                {
                                    Console.WriteLine("Application cannot continue w/o a valid database");
                                    return null;
                                }
                            }
                            if (!localdb.MdfExists)
                            {
                                var installer = new DatabaseInstaller() { Config = localdb };
                                installer.CreateMDF();
                                installer.InstallDatabase();
                            }
                            return __connectionstring = connect;
                        }
                        return connect;
                    }

                }

            }

            var sqlregistry = RegReader.SqlServerInstance();
            if (sqlregistry != null)
                if (sqlregistry.Count > 0)
                    return "Server=" + sqlregistry[sqlregistry.Count - 1] + ";Initial Catalog=FFToiletBowl;Integrated Security=SSPI;";

            return @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=FFToiletBowl;Integrated Security=SSPI;AttachDBFilename=.\FFToiletBowl.mdf";
        }

    }
}
