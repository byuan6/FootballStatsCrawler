using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32;

namespace FFToiletBowlSQL
{
    static public class RegReader
    {
        static public List<string> SqlServerInstance() 
        {
            List<string> result = new List<string>();

            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    foreach (var instanceName in instanceKey.GetValueNames())
                        if (instanceName == "MSSQLSERVER")
                            result.Add(".");
                        else
                            result.Add(".\\" + instanceName);
                    return result;
                }
                RegistryKey sqlKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server", false);
                if (sqlKey != null)
                {
                    foreach (var keyName in sqlKey.GetValueNames())
                        if (keyName == "InstalledInstances")
                        {
                            result.Add(".");
                            return result;
                        }
                }
            }
            return null;
        }

        //https://dba.stackexchange.com/questions/152881/how-to-programmatically-find-out-where-localdb-is-installed
        static public List<string> LocalDbInstance() 
        {
            List<string> result = new List<string>();

            //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions
            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions", false);
                if (instanceKey != null)
                {
                    foreach (var subkeyName in instanceKey.GetSubKeyNames())
                    {
                        var name = instanceKey.OpenSubKey(subkeyName);
                        foreach (var valuekey in name.GetValueNames())
                        if(valuekey=="InstanceAPIPath")
                        {
                            result.Add( (string)name.GetValue(valuekey) );
                        }
                    }
                    return result;
                }
            }
            return null;
        }

    }
}
