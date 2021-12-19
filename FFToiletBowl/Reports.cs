using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace FFToiletBowl
{
    public class Reports
    {



        static public void SetYear(int year)
        {
            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            if (connectionString != null)
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandTimeout = 60;
                        cmd.CommandText = "UPDATE dbo.[Active] SET Yr=@year";
                        cmd.CommandType = System.Data.CommandType.Text;
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = year;
                        param.SqlDbType = SqlDbType.Int;
                        param.ParameterName = "@year";
                        cmd.Parameters.Add(param);
                        cmd.Connection = connection;

                        var count = cmd.ExecuteNonQuery();
                        if (count != 1)
                            throw new ApplicationException("No record count returned with update");
                    }

                }
        }




        static public IEnumerable<string> GetData(string name)
        {
            try
            {
                using (var dt = GetDataFromDB(name))
                    return SerializeAsTabDelimited(dt);
            }
            catch
            {
                return new List<string>();
            }
        }

        static public DataTable GetDataFromDB(string name)
        {
            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            if (connectionString != null)
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandTimeout = 60;
                        cmd.CommandText = "[dbo].[GetData]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = name;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 255;
                        param.ParameterName = "@Table";
                        cmd.Parameters.Add(param);
                        cmd.Connection = connection;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet ds = new DataSet())
                            {
                                da.Fill(ds, "result_name");
                                DataTable dt = ds.Tables["result_name"];
                                return dt;
                            }
                        }
                    }
                }
            throw new ApplicationException("No connection string");
        }


        static public IEnumerable<string> SerializeAsTabDelimited(DataTable dt)
        {
            var col = dt.Columns;
            int length = col.Count;
            for (int i = 0; i < length; i++)
            {
                if (i != 0) yield return "\t";
                yield return col[i].ColumnName;
            }
            yield return "\n";

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < length; i++)
                {
                    if (i != 0) yield return "\t";
                    yield return row[i].ToString();
                }
                yield return "\n";
            }
        }


        static public IEnumerable<DataRow> GetDataForWeb(string name, int year, int week, string game, string player)
        {
            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            if (connectionString != null)
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandTimeout = 60;
                        cmd.CommandText = "[dbo].[GetWebData]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        {
                            SqlParameter param1 = cmd.CreateParameter();
                            param1.Value = name;
                            param1.SqlDbType = SqlDbType.VarChar;
                            param1.Size = 255;
                            param1.ParameterName = "@Table";
                            cmd.Parameters.Add(param1);
                        }
                        {
                            SqlParameter param2 = cmd.CreateParameter();
                            param2.Value = year;
                            param2.SqlDbType = SqlDbType.Int;
                            param2.ParameterName = "@Year";
                            cmd.Parameters.Add(param2);
                        }
                        {
                            SqlParameter param3 = cmd.CreateParameter();
                            param3.Value = week;
                            param3.SqlDbType = SqlDbType.Int;
                            param3.ParameterName = "@Wk";
                            cmd.Parameters.Add(param3);
                        }
                        {
                            SqlParameter param4 = cmd.CreateParameter();
                            param4.Value = game;
                            param4.SqlDbType = SqlDbType.VarChar;
                            param4.Size = 255;
                            param4.ParameterName = "@Team";
                            cmd.Parameters.Add(param4);
                        }
                        {
                            SqlParameter param5 = cmd.CreateParameter();
                            param5.Value = player;
                            param5.SqlDbType = SqlDbType.VarChar;
                            param5.Size = 255;
                            param5.ParameterName = "@Player";
                            cmd.Parameters.Add(param5);
                        }
                        cmd.Connection = connection;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        using (DataSet ds = new DataSet())
                        {
                            da.Fill(ds, "result_name");
                            using (DataTable dt = ds.Tables["result_name"])
                                foreach (DataRow row in dt.Rows)
                                    yield return row;
                        }

                    }

                }
        }

    }

}
