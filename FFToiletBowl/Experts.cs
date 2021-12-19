using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;


namespace FFToiletBowl
{
    public class Experts
    {
        static public int SetExpertRanking(IEnumerable<ExpertRank> data)
        {
            //Which records have been cleared
            HashSet<string> cleared = new HashSet<string>();

            // Create the command.
            int count = 0;
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create a new row.
                using (FFToiletBowlDataSetTableAdapters.ExpertRankingTableAdapter da = new FFToiletBowlDataSetTableAdapters.ExpertRankingTableAdapter())
                {
                    da.Connection = connection;

                    HashSet<string> deleted = new HashSet<string>();
                    foreach (var row in data)
                        if(row.Player!=null && row.Team!=null && row.Pos!=null)
                        {
                            string urlkey = string.Format("{0}?{1}&{2}&{3}", row.URL, row.Year, row.Expert, row.ScoringSystem);
                            if(!deleted.Contains(urlkey))
                                da.DeleteQueryByURL(urlkey);
                            da.InsertQuery(row.Year, row.Expert, row.ScoringSystem, row.Rank, row.PlayerID, row.Player, row.Player2, row.Team, row.Pos, row.URL);
                            count++;
                        }

                    da.ReverseEngineerPointsFromRank();
                }
            }

            return count;
        }

    }


    public class ExpertRank
    {
        public int Year;
        public string Expert;
        public string ScoringSystem;
        public int Rank;
        public string PlayerID;
        public string Player;
        public string Player2;
        public string Team;
        public string Pos;
        public string URL;
    }

}
