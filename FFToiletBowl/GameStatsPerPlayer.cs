using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;


namespace FFToiletBowl
{
    public class GameStatsPerPlayer
    {

        static public DataTable GetData()
        {
            string name = "vwStatsPointsForCityIsland";

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
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

        }

        static public void SetData(List<StatLine> data)
        {
            DataSet productsDataSet = new DataSet();

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Do work here; connection closed on following line.
                using (DbCommand command = connection.CreateCommand())
                {
                    // string sql = "[FFToiletBowl].[dbo].[Stats]";
                    // command.CommandText = sql;
                    // command.CommandType = CommandType.TableDirect;

                    // Create a new row.
                    using (FFToiletBowlDataSetTableAdapters.StatsTableAdapter da = new FFToiletBowlDataSetTableAdapters.StatsTableAdapter())
                    {
                        foreach (var row in data)
                        {
                            /*
                             * StatLine row;
                            FFToiletBowlDataSet ds = new FFToiletBowlDataSet(); 
                            FFToiletBowlDataSet.StatsRow newRow;
                            newRow = ds.Stats.NewStatsRow();
                            newRow.PlayerID = row.PlayerID;
                            newRow.Player = row.Player;
                            newRow.Pos = row.Pos;
                            newRow.Team = row.Team;
                            newRow.Year = row.Year;
                            newRow.Gm = row.Gm;
                            newRow.PaComp = row.PaComp;
                            newRow.PaAtt = row.PaAtt;
                            newRow.PaYd = row.PaYd;
                            newRow.PaTD = row.PaTD;
                            newRow.PaINT = row.PaINT;
                            newRow.RuAtt = row.RuAtt;
                            newRow.RuYd = row.RuYd;
                            newRow.RuTD = row.RuTD;
                            newRow.ReTgt = row.ReTgt;
                            newRow.ReRec = row.ReRec;
                            newRow.ReYd = row.ReYd;
                            newRow.ReTD = row.ReTD;
                            newRow.KiFGM = row.KiFGM;
                            newRow.KiFGA = row.KiFGA;
                            newRow.KiFGP = row.KiFGP;
                            newRow.KiEPM = row.KiEPM;
                            newRow.KiEPA = row.KiEPA;
                            newRow.DSack = row.DSack;
                            newRow.DFR = row.DFR;
                            newRow.DINT = row.DINT;
                            newRow.DTD = row.DTD;
                            newRow.DPA = row.DPA;
                            newRow.DPaYd = row.DPaYd;
                            newRow.DRuYd = row.DRuYd;
                            newRow.DSafety = row.DSafety;
                            newRow.DKickTD = row.DKickTD;

                            // Add the row to the Region table 
                            ds.Stats.Rows.Add(newRow);
                            */
                            da.Connection = connection;
                            da.Insert(row.PlayerID,
                                        row.Player,
                                        row.Pos,
                                        row.Team,
                                        row.Year,
                                        row.Gm,
                                        row.PaComp,
                                        row.PaAtt,
                                        row.PaYd,
                                        row.PaTD,
                                        row.PaINT,
                                        row.RuAtt,
                                        row.RuYd,
                                        row.RuTD,
                                        row.ReTgt,
                                        row.ReRec,
                                        row.ReYd,
                                        row.ReTD,
                                        row.KiFGM,
                                        row.KiFGA,
                                        Convert.ToDecimal(row.KiFGP),
                                        row.KiEPM,
                                        row.KiEPA,
                                        row.DSack,
                                        row.DFR,
                                        row.DINT,
                                        row.DTD,
                                        row.DPA,
                                        row.DPaYd,
                                        row.DRuYd,
                                        row.DSafety,
                                        row.DKickTD);
                        }
                    }
                }
            }

        }


        static public List<Loaded> GetLoaded()
        {
            List<Loaded> list = new List<Loaded>();

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Do work here; connection closed on following line.
                using (SqlCommand command = connection.CreateCommand())
                {
                    //string sql = "SELECT [Pos] ,[Team] ,[Year] ,[Gm] ,COUNT(0)"
                    //            + " FROM [FFToiletBowl].[dbo].[Stats]"
                    //            + " GROUP BY [Pos] ,[Team] ,[Year] ,[Gm]";

                    string sql = "SELECT s.[Year], s.[Wk], COUNT(p.[PlayerID]), MAX(LoadDate) as EndOfWeek"
                                + " FROM dbo.[Schedule] s"
                                + " LEFT JOIN dbo.[Stats] p ON s.[Year]=p.[Year] AND s.[Wk]=p.[Gm]"
                                + " GROUP BY s.[Year], s.[Wk]"
                                + " HAVING MAX(LoadDate)+1 < GETDATE()";


                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    // Open the connection.
                    
                    using(SqlDataReader dr = command.ExecuteReader())
                    while (dr.Read())
                    {
                        Loaded record = new Loaded();
                        //record.Pos = dr.GetString(0);
                        //record.Team = dr.GetString(1);
                        record.Year = dr.GetInt32(0);
                        record.Gm = dr.GetInt32(1);
                        record.Count = dr.GetInt32(2);

                        //record.Count = dr.GetInt32(4);
                        
                        list.Add(record);

                        //dr.NextResult();
                    }
                }
            }

            return list;
        }

        static public void ClearAllStats()
        {
            List<Loaded> list = new List<Loaded>();

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (FFToiletBowlDataSetTableAdapters.StatsTableAdapter da = new FFToiletBowlDataSetTableAdapters.StatsTableAdapter())
                {
                    da.Connection = connection;
                    da.ClearStats();
                }
            }

        }
        static public void ClearStatsFor(int year, int wk)
        {
            List<Loaded> list = new List<Loaded>();

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (FFToiletBowlDataSetTableAdapters.StatsTableAdapter da = new FFToiletBowlDataSetTableAdapters.StatsTableAdapter())
                {
                    da.Connection = connection;
                    da.DeleteByWeek(year, wk);
                }
            }

        }

        static public List<Loaded> CreateReports()
        {
            List<Loaded> list = new List<Loaded>();

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Do work here; connection closed on following line.
                using (FFToiletBowlDataSetTableAdapters.StatsTableAdapter da = new FFToiletBowlDataSetTableAdapters.StatsTableAdapter())
                {
                    da.Connection = connection;
                    da.SelectCommandTimeout = 0;
                    da.InsertCommandTimeout = 0;
                    //da.DeleteCommandTimeout = 0;
                    //da.UpdateCommandTimeout = 0;
                    da.CreateReports();
                }
            }

            return list;
        }


        static public List<PlayerProfile> GetAllProfile(bool onlymissing)
        {
            List<PlayerProfile> list = new List<PlayerProfile>();

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Do work here; connection closed on following line.
                using (SqlCommand command = connection.CreateCommand())
                {
                    string sql = @"SELECT DISTINCT  a.PlayerID KnownPLayerID, a.Player KnownPlayer
                                            , b.*
	                            FROM        dbo.Stats a
	                            LEFT JOIN   dbo.PlayerProfile b ON a.PlayerID=b.PlayerID";
                    if (onlymissing)
                        sql += " WHERE b.PlayerID IS NULL";

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    // Open the connection.
                    using (SqlDataReader dr = command.ExecuteReader())
                        while (dr.Read())
                        {
                            PlayerProfile record = new PlayerProfile();
                            record.PlayerID = dr.GetString(0);
                            record.Player = dr.GetString(1);
                            record.IsSaved = !dr.IsDBNull(2);

                            record.DraftStr = dr.IsDBNull(4) ? null : dr.GetString(4);
                            record.CollegeStr = dr.IsDBNull(5) ? null : dr.GetString(5);
                            record.HtStr = dr.IsDBNull(6) ? null : dr.GetString(6);
                            record.DOBStr = dr.IsDBNull(7) ? null : dr.GetString(7);
                            record.WtStr = dr.IsDBNull(8) ? null : dr.GetString(8);
                            record.AgeStr = dr.IsDBNull(9) ? null : dr.GetString(9);

                            if (!dr.IsDBNull(10)) record.DraftYear = dr.GetInt32(10);
                            if (!dr.IsDBNull(11)) record.HtInches = dr.GetInt32(11);
                            if (!dr.IsDBNull(12)) record.Dob = dr.GetDateTime(12);
                            if (!dr.IsDBNull(13)) record.WtLbs = dr.GetInt32(13);
                            if (!dr.IsDBNull(14)) record.Age = dr.GetInt32(14);

                            list.Add(record);
                        }
                }
            }

            return list;
        }

        static public void SetPlayerProfile(PlayerProfile data)
        {
            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Do work here; connection closed on following line.
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"INSERT INTO [dbo].[PlayerProfile]
                               ([PlayerID]
                               ,[Player]
                               ,[DraftStr]
                               ,[CollegeStr]
                               ,[HtStr]
                               ,[DOBStr]
                               ,[WtStr]
                               ,[AgeStr]
                               ,[DraftYear]
                               ,[HtInches]
                               ,[Dob]
                               ,[WtLbs]
                               ,[Age])
                         VALUES
                               (@PlayerID
                               ,@Player
                               ,@DraftStr
                               ,@CollegeStr
                               ,@HtStr
                               ,@DOBStr
                               ,@WtStr
                               ,@AgeStr
                               ,@DraftYear
                               ,@HtInches
                               ,@Dob
                               ,@WtLbs
                               ,@Age)";
                    {
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = data.PlayerID;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 150;
                        param.ParameterName = "@PlayerID";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = data.Player == null ? string.Empty : data.Player;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 100;
                        param.ParameterName = "@Player";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = data.DraftStr == null ? string.Empty : data.DraftStr;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 20;
                        param.ParameterName = "@DraftStr";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = data.CollegeStr == null ? string.Empty : data.CollegeStr;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 20;
                        param.ParameterName = "@CollegeStr";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = data.HtStr == null ? string.Empty : data.HtStr;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 30;
                        param.ParameterName = "@HtStr";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = data.DOBStr == null ? string.Empty : data.DOBStr;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 10;
                        param.ParameterName = "@DOBStr";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = data.WtStr == null ? string.Empty : data.WtStr;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 10;
                        param.ParameterName = "@WtStr";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        param.Value = data.AgeStr == null ? string.Empty : data.AgeStr;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 10;
                        param.ParameterName = "@AgeStr";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        if(data.DraftYear.HasValue) param.Value = data.DraftYear.Value; else param.Value = DBNull.Value;
                        param.SqlDbType = SqlDbType.Int;
                        param.ParameterName = "@DraftYear";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        if (data.HtInches.HasValue) param.Value = data.HtInches.Value; else param.Value = DBNull.Value;
                        param.SqlDbType = SqlDbType.Int;
                        param.ParameterName = "@HtInches";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        if (data.Dob.HasValue) param.Value = data.Dob.Value; else param.Value = DBNull.Value;
                        param.SqlDbType = SqlDbType.VarChar;
                        param.Size = 30;
                        param.ParameterName = "@Dob";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        if (data.WtLbs.HasValue) param.Value = data.WtLbs.Value; else param.Value = DBNull.Value;
                        param.SqlDbType = SqlDbType.Int;
                        param.ParameterName = "@WtLbs";
                        cmd.Parameters.Add(param);
                    }
                    {
                        SqlParameter param = cmd.CreateParameter();
                        if (data.Age.HasValue) param.Value = data.Age.Value; else param.Value = DBNull.Value;
                        param.SqlDbType = SqlDbType.Int;
                        param.ParameterName = "@Age";
                        cmd.Parameters.Add(param);
                    }
                    cmd.Connection = connection;
                    cmd.ExecuteNonQuery();
                    data.IsSaved = true;
                }
            }

        }
    }


    public struct Loaded
    {
        //public string Pos;
        //public string Team;
        public int Year;
        public int Gm;
        public int Count;
    }

    public struct StatLine
    {
        public string PlayerID;
        public string Player;
        public string Pos;
        public string Team;
        public int Year;
        public int Gm;
        public int PaComp;
        public int PaAtt;
        public int PaYd;
        public int PaTD;
        public int PaINT;
        public int RuAtt;
        public int RuYd;
        public int RuTD;
        public int ReTgt;
        public int ReRec;
        public int ReYd;
        public int ReTD;
        public int KiFGM;
        public int KiFGA;
        public float KiFGP;
        public int KiEPM;
        public int KiEPA;
        public int DSack;
        public int DFR;
        public int DINT;
        public int DTD;
        public int DPA;
        public int DPaYd;
        public int DRuYd;
        public int DSafety;
        public int DKickTD;
    }

    public struct PlayerProfile
    {
        public string PlayerID;
        public string Player;

        public string DraftStr;
	    public string CollegeStr;
	    public string HtStr;
	    public string DOBStr;
	    public string WtStr;
	    public string AgeStr;

        public Nullable<int> DraftYear;
        public Nullable<int> HtInches;
	    public Nullable<DateTime> Dob;
        public Nullable<int> WtLbs;
        public Nullable<int> Age;

        public bool IsSaved;
    }


}
