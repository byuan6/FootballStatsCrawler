using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;


namespace FFToiletBowl
{
    public class InjuredReserve
    {

        static public void SetInjuredReserve(List<WklyInjuredReserve> data)
        {
            //Which records have been cleared
            HashSet<string> cleared = new HashSet<string>();

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // string sql = "[FFToiletBowl].[dbo].[Stats]";
                // command.CommandText = sql;
                // command.CommandType = CommandType.TableDirect;

                // Create a new row.
                using (FFToiletBowlDataSetTableAdapters.InjuredReserveTableAdapter da = new FFToiletBowlDataSetTableAdapters.InjuredReserveTableAdapter())
                {
                    da.Connection = connection;
                    //da.DeleteByWeek();
                    foreach (var row in data)
                    {
                        var week = string.Format("{0}-{1}", row.Year, row.Gm);
                        if (!cleared.Contains(week))
                        {
                            cleared.Add(week);
                            da.DeleteByWeek(row.Gm, row.Year);
                        }

                        row.StatsPlayerID = PlayerIdentification.GetStatsPlayerURL(row.Player, row.Pos, row.Team);

                        da.Connection = connection;
                        da.InsertQuery(row.InjuredID,
                            row.Gm,
                            row.Year,
                            row.Player,
                            row.Pos,
                            row.Team,
                            row.Status,
                            row.StatsPlayerID);
                    }
                }
            }

        }


        static public void SetInjuryReport(List<DailyInjuryReport> data)
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
                    using (FFToiletBowlDataSetTableAdapters.InjuryReportTableAdapter da = new FFToiletBowlDataSetTableAdapters.InjuryReportTableAdapter())
                    {
                        da.Connection = connection;
                        //da.ClearInjuredReport();
                        foreach (var row in data)
                        {
                            if (string.IsNullOrWhiteSpace(row.Player))
                                continue;

                            row.StatsPlayerID = PlayerIdentification.GetStatsPlayerURL(row.Player, row.Pos, row.Team);
                            if (string.IsNullOrWhiteSpace(row.Injury)) row.Injury = "-";
                            if (string.IsNullOrWhiteSpace(row.Notes)) row.Notes = "-";
                            if (string.IsNullOrWhiteSpace(row.Status)) row.Status = "-";
                            if (string.IsNullOrWhiteSpace(row.Source)) row.Source = "-";

                            da.Connection = connection;
                            da.DeleteQuery(row.EspnPlayerURL, row.Year, row.Gm);
                            da.InsertQuery(row.Year, row.Gm,
                                        row.Player,
                                        row.Pos,
                                        row.EspnPlayerURL,
                                        row.EspnPlayerID,
                                        row.EspnTeam,
                                        row.Team,
                                        row.Injury,
                                        row.Notes,
                                        row.Status,
                                        row.Source,
                                        row.ReportDate,
                                        row.LoadDate,
                                        row.LoadID,
                                        row.StatsPlayerID);
                        }
                    }
                }
            }
        }


        static Dictionary<string, string> team2abbr = new Dictionary<string, string>() {
            {"Arizona Cardinals","ARI"},
            {"Atlanta Falcons","ATL"},
            {"Baltimore Ravens","BAL"},
            {"Buffalo Bills","BUF"},
            {"Carolina Panthers","CAR"},
            {"Chicago Bears","CHI"},
            {"Cincinnati Bengals","CIN"},
            {"Cleveland Browns","CLE"},
            {"Dallas Cowboys","DAL"},
            {"Denver Broncos","DEN"},
            {"Detroit Lions","DET"},
            {"Green Bay Packers","GB"},
            {"Houston Texans","HOU"},
            {"Indianapolis Colts","IND"},
            {"Jacksonville Jaguars","JAC"},
            {"Kansas City Chiefs","KC"},
            {"Los Angeles Rams","LAR"},
            {"Los Angeles Chargers","LAC"},
            {"Miami Dolphins","MIA"},
            {"Minnesota Vikings","MIN"},
            {"New England Patriots","NE"},
            {"New Orleans Saints","NO"},
            {"New York Giants","NYG"},
            {"New York Jets","NYJ"},
            {"Oakland Raiders","OAK"},
            {"Philadelphia Eagles","PHI"},
            {"Pittsburgh Steelers","PIT"},
            {"San Diego Chargers","SD"},
            {"San Francisco 49ers","SF"},
            {"Seattle Seahawks","SEA"},
            {"Tampa Bay Buccaneers","TB"},
            {"Tennessee Titans","TEN"},
            {"Washington Redskins","WAS"},

            {"Cardinals","ARI"},
            {"Falcons","ATL"},
            {"Ravens","BAL"},
            {"Bills","BUF"},
            {"Panthers","CAR"},
            {"Bears","CHI"},
            {"Bengals","CIN"},
            {"Browns","CLE"},
            {"Cowboys","DAL"},
            {"Broncos","DEN"},
            {"Lions","DET"},
            {"Bay Packers","GB"},
            {"Texans","HOU"},
            {"Colts","IND"},
            {"Jaguars","JAC"},
            {"City Chiefs","KC"},
            {"Angeles Rams","LAR"},
            {"Angeles Chargers","LAC"},
            {"Dolphins","MIA"},
            {"Vikings","MIN"},
            {"Patriots","NE"},
            {"Saints","NO"},
            {"Giants","NYG"},
            {"Jets","NYJ"},
            {"Raiders","LV"},
            {"Eagles","PHI"},
            {"Steelers","PIT"},
            {"Chargers","SD"},
            {"49ers","SF"},
            {"Seahawks","SEA"},
            {"Buccaneers","TB"},
            {"Titans","TEN"},
            {"Redskins","WAS"},
        };

        static public string TeamAbbrFor(string name)
        {
            if (team2abbr.ContainsKey(name))
                return team2abbr[name];
            else
                return name;
        }
    }


    public class WklyInjuredReserve
    {
        public string InjuredID;
        public string Player;
        public string StatsPlayerID;
        public string Pos;
        public string Team;
        public int Year;
        public int Gm;
        public string Status;
    }


    public class DailyInjuryReport
    {
        public int Year;
        public int Gm;

        public string Player;
        public string StatsPlayerID;
        public string Pos;
        public string EspnPlayerURL;
        public string EspnPlayerID;
        public string EspnTeam;
        public string Team;
        public string Injury;
        public string Notes;
        public string Status;
        public string Source;
        public DateTime ReportDate;
        public DateTime LoadDate;
        public string LoadID;
    }

}
