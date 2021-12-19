using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Data.Common;


namespace FFToiletBowl
{
    public class ESPNLeagueRoster
    {

        static Dictionary<string, string> sillyNameReuser = new Dictionary<string, string>();
        static Dictionary<string, int> indexReuser = new Dictionary<string, int>();
        static public Guid LoadID = Guid.NewGuid();
        static DateTime saveDate = DateTime.Now;

        static public void SetData(TeamRoster data)
        {

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // string sql = "[FFToiletBowl].[dbo].[Stats]";
                // command.CommandText = sql;
                // command.CommandType = CommandType.TableDirect;

                // Create a new row.
                using (FFToiletBowlDataSetTableAdapters.CityIslandTeamsTableAdapter da = new FFToiletBowlDataSetTableAdapters.CityIslandTeamsTableAdapter())
                using (FFToiletBowlDataSetTableAdapters.CityIslandRosterSpotsTableAdapter da2 = new FFToiletBowlDataSetTableAdapters.CityIslandRosterSpotsTableAdapter())
                {
                    da.Connection = connection;
                    da2.Connection = connection;

                    //don't clear the table... we keep track for historical... create reports can cleanup after
                    int wins, losses, ties;
                    int.TryParse(data.GetWins(), out wins);
                    int.TryParse(data.GetLosses(), out losses);
                    int.TryParse(data.GetTies(), out ties);
                    if (data.TargetGm == 0) data.TargetGm = wins + losses + ties + 1;

                    if (!sillyNameReuser.ContainsKey(data.TeamURL))
                        sillyNameReuser.Add(data.TeamURL, getSillyName());
                    if (!indexReuser.ContainsKey(data.TeamURL))
                        indexReuser.Add(data.TeamURL, sillyNameIndex);

                    da.InsertQuery(data.Name, sillyNameReuser[data.TeamURL], indexReuser[data.TeamURL], data.TeamURL, wins, losses, ties, saveDate, LoadID.ToString(), data.TargetGm, data.GmVersusTeamURL);

                    int rbslot = 0;
                    int wrslot = 0;
                    foreach (var row in data.Players)
                        if (!string.IsNullOrWhiteSpace(row.Name))
                        {
                            int slot = 0;
                            if (row.RosterSpot == "WR")
                                slot = wrslot++;
                            else if (row.RosterSpot == "RB")
                                slot = rbslot++;

                            row.StatPlayerURL = getStatsPlayerURL(row.Name, row.Pos, row.Team);  //row.Team is NFL Team.  data.TeamURL is Fantasy TeamID
                            da2.InsertQuery(row.Pos, slot, false, row.PlayerURL, row.StatPlayerURL, row.Name, row.Team, saveDate, LoadID.ToString(), data.TeamURL, row.RosterSpot);
                        }
                }
            }

        }

        static int sillyNameIndex = 0;
        static string getSillyName()
        {
            string[] sillyNames = new string[] {
                "Commish",
                "MASH Unit",
                "Unbearable good",
                "Bad year after year",
                "Offers stupid trades",
                "New Guy",
                "Draft mis-informer",
                "Draft drunk",
                "Happy Fat Guy",
                "Knowledgeable Fan",
                "WFAN long time listener",
                "Annoying trash talker",
                "Always unprepared",
                "Takes it too seriously",
                "Luckiest Team in the World",
                "Commish's Cousin",
                "The Angry Guy",
                "The Funny Guy",
                "Forgets Bye weeks",
            };
            return sillyNames[sillyNameIndex++];
        }

        static Dictionary<string, DataRow> nameIndex = new Dictionary<string, DataRow>();
        static DataTable cached = null;
        static string getStatsPlayerURL(string player, string pos, string nflteam)
        {
            if (cached == null)
            {
                cached = GameStatsPerPlayer.GetData();
                foreach (DataRow item in cached.Rows)
                {
                    string key = item["Player"].ToString();
                    if (!nameIndex.ContainsKey(key))
                        nameIndex.Add(key, item);
                    else
                        nameIndex[key] = item;
                }
            }

            if (pos == "D/ST")
            {
                pos = "DST";
                nflteam = "";
                var nameparts = player.Replace(" D/ST", string.Empty).Split(' ');
                foreach (var rosterteam in nameparts)
                    foreach (var dbteam in nameIndex)
                        if (dbteam.Value["pos"].ToString().ToLower() == "dst")
                        {
                            string key = dbteam.Key;
                            bool found = false;
                            foreach (var dbteamparts in key.Split(' '))
                                if (rosterteam.ToLower() == dbteamparts.ToLower())
                                {
                                    //hopefully "[Bills] D/ST"(roster) shows up in index as {Buffalo [Bills]}
                                    //ESPN shows it's rosters for defense as D/ST	Rams D/ST D/ST	Free Agency
                                    //                                       Slot   Player (pos)    status
                                    //so the player is "[Full Team name w/o city] D/ST"
                                    //so it tries to match [Full Team name w/o city] with a string that looks like "[City] [Full Team name w/o city]"
                                    //it's a best guess based on the mascot names never match any part of the city names for now
                                    var teaminrecord = dbteam.Value["Team"];
                                    if (teaminrecord == null)
                                        continue; //it's usless, see if there is another close enough matching record
                                    var teamvalue = teaminrecord.ToString();
                                    var teamvaluelen = teamvalue.Length;
                                    nflteam = teamvalue.Substring(0, teamvaluelen > 5 ? 5 : teamvaluelen);
                                    player = key;
                                    found = true;
                                    break;
                                }
                            if (found) break;
                        }
            }
            if (nflteam == "Jax") nflteam = "JAC";
            if (nflteam == "Wsh") nflteam = "WAS";

            int attempt = 0;
            while (altName(ref player, attempt++))
            {
                if (nameIndex.ContainsKey(player))
                    //if (nameIndex[player]["Team"].ToString() == nflteam.ToUpper() || nflteam.ToUpper()=="FA") //FA means he was fired, Zack Hocker moved teams and doesnt have a stat to his name.  
                    if (nameIndex[player]["Pos"].ToString() == pos
                        || nameIndex[player]["Pos"].ToString().Contains(pos)
                        || pos.Contains(nameIndex[player]["Pos"].ToString()))
                        return nameIndex[player]["PlayerID"].ToString();

            }

            foreach (DataRow item in cached.Rows)
            {
                string cname = item["Player"].ToString();
                string cpos = item["Pos"].ToString();
                string cteam = item["Team"].ToString();

                if (nflteam.ToLower() == cteam.ToLower())
                    if (pos.ToLower() == cpos.ToLower())
                        if (player.ToLower() == cname.ToLower())
                            return item["PlayerID"].ToString();
            }

            return null;
        }

        static public bool altName(ref string name, int attempt)
        {
            if (attempt == 0) return true;

            if (name.Contains("Steven ") && attempt < 2)
            {
                name = name.Replace("Steven ", "Steve ");
                return true;
            }
            else if (name.EndsWith(" I") || name.EndsWith(" II") || name.EndsWith(" III") || name.EndsWith(" IV") || name.EndsWith(" V") || name.EndsWith(" VI") || name.EndsWith(" VII") && attempt < 7)
            {
                if (name.EndsWith(" I")) name = name.Substring(0, name.Length - 2);
                if (name.EndsWith(" II")) name = name.Substring(0, name.Length - 3);
                if (name.EndsWith(" III")) name = name.Substring(0, name.Length - 4);
                if (name.EndsWith(" IV")) name = name.Substring(0, name.Length - 3);
                if (name.EndsWith(" V")) name = name.Substring(0, name.Length - 2);
                if (name.EndsWith(" VI")) name = name.Substring(0, name.Length - 3);
                if (name.EndsWith(" VII")) name = name.Substring(0, name.Length - 4);
                return true;
            }
            else if (name.EndsWith(" Jr.") && attempt < 4)
            {
                name = name.Substring(0, name.Length - 4);
                return true;
            }
            else if (name.EndsWith(" Sr.") && attempt < 5)
            {
                name = name.Substring(0, name.Length - 4);
                return true;
            }
            else if (name.StartsWith("Benjamin ") && attempt < 6)
            {
                name = name.Replace("Benjamin ", "Ben ");
                return true;
            }
            else if (name.Contains("Stevie ") && attempt < 7)
            {
                name = name.Replace("Stevie ", "Steve ");
                return true;
            }
            else if (name.Contains("Ben ") && attempt < 8)
            {
                name = name.Replace("Ben ", "Benny ");
                return true;
            }
            else if (name.Contains("Mike ") && attempt < 9)
            {
                name = name.Replace("Mike ", "Michael ");
                return true;
            }
            else if (name.Contains("William ") && attempt < 10)
            {
                name = name.Replace("William ", "Will ");
                return true;
            }
            else if (name.Contains("Will ") && attempt < 11)
            {
                name = name.Replace("Will ", "William ");
                return true;
            }
            else if (name.Contains("Robert ") && attempt < 12)
            {
                name = name.Replace("Robert ", "Rob ");
                return true;
            }
            else if (name.Contains("Rob ") && attempt < 13)
            {
                name = name.Replace("Rob ", "Robert ");
                return true;
            }
            else
                return false;
        }


        static public bool IsCleared = false;
        static public void ClearRosterData()
        {

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // string sql = "[FFToiletBowl].[dbo].[Stats]";
                // command.CommandText = sql;
                // command.CommandType = CommandType.TableDirect;

                // Create a new row.
                using (FFToiletBowlDataSetTableAdapters.CityIslandTeamsTableAdapter da = new FFToiletBowlDataSetTableAdapters.CityIslandTeamsTableAdapter())
                {
                    da.Connection = connection;

                    da.ClearCityIslandRoster();

                }
            }

        }

    }


    public class TeamRoster
    {
        public string TeamURL;
        public string Name;
        public string Record;
        public string GetWins()
        {
            var record = this.Record.Replace("(", "").Replace(")", "").Split(new string[] { "-" }, StringSplitOptions.None);
            //Console.WriteLine("[{0}][{1}]", this.Record, record[0].Trim());
            return record[0].Trim();
        }
        public string GetLosses()
        {
            var record = this.Record.Replace("(", "").Replace(")", "").Split(new string[] { "-" }, StringSplitOptions.None);
            if (record.Length < 2) return null;
            //Console.WriteLine("[{0}][{1}]", this.Record, record[1].Trim());
            return record[1].Trim();
        }
        public string GetTies()
        {
            var record = this.Record.Replace("(", "").Replace(")", "").Split(new string[] { "-" }, StringSplitOptions.None);
            if (record.Length < 3) return null;
            return record[2].Trim();
        }

        public List<PlayerOnRoster> Players = new List<PlayerOnRoster>();

        public int TargetGm;
        public string GmVersusTeamURL;
    }

    public class PlayerOnRoster
    {
        public string RosterSpot;

        public string PlayerURL;
        public string StatPlayerURL;
        public string Name;
        public string Pos;
        public string Team;
    }
}
