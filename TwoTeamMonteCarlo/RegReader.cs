using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32;
using System.Management;


namespace TwoTeamMonteCarlo
{
    static public class RegReader
    {
        static public List<string> SqlServerInstance() {
            List<string> result = new List<string>();

            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    foreach (var instanceName in instanceKey.GetValueNames())
                        if(instanceName=="MSSQLSERVER")
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

        static public int CoreCount()
        {
            int coreCount = 0;
            foreach (var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            return coreCount;
        }

    }
}
