using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;


namespace FFToiletBowl
{

    public class PlayerIdentification
    {
        static Dictionary<string, DataRow> nameIndex = new Dictionary<string, DataRow>();
        static DataTable cached = null;
        /// <summary>
        /// Let's see if we can find a player with similar name, in database AND return the ID for this player using FFToday's ID
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pos"></param>
        /// <param name="nflteam"></param>
        /// <returns></returns>
        static public string GetStatsPlayerURL(string player, string pos, string nflteam)
        {
            if (cached == null)
                try
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
                catch (Exception ex)
                {
                    Console.Error.WriteLine("error in lookup population");
                    throw ex;
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
                                    nflteam = dbteam.Value["Team"].ToString().Substring(0, 5);
                                    player = key;
                                    found = true;
                                    break;
                                }
                            if (found) break;
                        }
            }
            if (nflteam == "Jax") nflteam = "JAC";
            if (nflteam == "Wsh") nflteam = "WAS";
            if (string.IsNullOrWhiteSpace(player)) return "(deleteme)";

            int attempt = 0;
            while (altName(ref player, attempt++))
                try
                {
                    if (nameIndex.ContainsKey(player))
                        //if (nameIndex[player]["Team"].ToString() == nflteam.ToUpper() || nflteam.ToUpper()=="FA") //FA means he was fired, Zack Hocker moved teams and doesnt have a stat to his name.  
                        if (nameIndex[player]["Pos"].ToString() == pos
                            || nameIndex[player]["Pos"].ToString().Contains(pos)
                            || pos.Contains(nameIndex[player]["Pos"].ToString()))
                            return nameIndex[player]["PlayerID"].ToString();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error in altname " + player +"-"+ attempt.ToString());
                    throw ex;
                }

            foreach (DataRow item in cached.Rows)
                try
                {
                    string cname = item["Player"].ToString();
                    string cpos = item["Pos"].ToString();
                    string cteam = item["Team"].ToString();

                    if (nflteam.ToLower() == cteam.ToLower())
                        if (pos.ToLower() == cpos.ToLower())
                            if (player.ToLower() == cname.ToLower())
                                return item["PlayerID"].ToString();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error in search");
                    throw ex;
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
            else
                return false;
        }
    }

}
