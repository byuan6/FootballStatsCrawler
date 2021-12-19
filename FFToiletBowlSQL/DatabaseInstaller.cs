using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using System.IO;

namespace FFToiletBowlSQL
{
    public class DatabaseInstaller
    {
        public string MDFDirectory = Environment.CurrentDirectory;

        public LocaldbAdmin Config;

        public void CreateMDF()
        {
            var filename = this.Config.MdfFilename;
            var dbname = this.Config.DatabaseName;
            var logname = Path.GetFileNameWithoutExtension(filename);

            //SqlConnection connection = new SqlConnection(@"server=(localdb)\v11.0")
            var connect = @"server=" + this.Config.DataSource;
            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();

                string sql = string.Format(@"
CREATE DATABASE
    [{2}]
ON PRIMARY (
    NAME={2}_data,
    FILENAME = '{0}'
)
LOG ON (
    NAME={2}_log,
    FILENAME = '{1}.ldf'
)", this.MDFDirectory + "\\" + filename, this.MDFDirectory + "\\" + logname, dbname);

                SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();

                //string sql2 = "DROP DATABASE [" + dbname + "]";
                string sql2 = "EXEC sp_detach_db '" + dbname + "', 'true'";
                SqlCommand drop = new SqlCommand(sql2, connection);
                drop.ExecuteNonQuery();
            }
        }

        const string _sqlfile = @"LocalDBInstaller\createdatabaseSQL.txt";
        public void InstallDatabase()
        {
            var connect = this.Config.OriginalConnectionString;
            using (var cn = new SqlConnection(connect))
            {
                cn.Open();
                foreach (var item in getStatement())
                using(var cmd = new SqlCommand(item, cn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        IEnumerable<string> getStatement()
        {
            StringBuilder sb = new StringBuilder();
            using (var sr = new StreamReader(_sqlfile))
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == "GO")
                    {
                        yield return sb.ToString();
                        sb = new StringBuilder();
                    }
                    else
                        sb.AppendLine(line);
                }
            if(sb.Length!=0)
                yield return sb.ToString();
        }
    }
}
