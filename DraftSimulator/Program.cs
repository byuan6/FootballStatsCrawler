using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.IO;

using FFToiletBowl;
using CrawlerCommon;
using CrawlerCommon.HtmlPathTest;
using CrawlerCommon.TagDef.StrictXHTML;

namespace DraftSimulator
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                showUsage();
                return 1;
            }

            var teams=0;
            var year = 0;
            var starters = new string[] {"QB", "RB", "RB", "WR", "WR", "TE", "RB|WR|TE", "K", "DST" };
            var bench = new string[] { "QB", "RB", "RB", "WR", "WR", "TE", "K", "*", "DST" };

            if (int.TryParse(args[0], out year))
            {
                if (args[1] == "-import" && args.Length == 3)
                {
                    //https://www.fftoday.com/articles/orth/19_pma_big_board_ppr_1.html
                    //https://www.cbssports.com/fantasy/football/rankings/ppr/top200/
                    //https://www.fftoday.com/mock/19_july.html
                    checkUrlForPlayers(year, args[2]);
                }
                else if (int.TryParse(args[1], out teams))
                {
                    MonteCarloDraftAndSeason(year, teams, starters, bench);
                }
            }
            showUsage();

            Console.WriteLine("Draft simulator is not implemented yet.  This is just a placeholder.");
            Console.WriteLine("Get ranking.  Get simulation of ranking using [S-draft].  Make it an instance class, so it can be run in parallel");
            Console.WriteLine("each sliding held karp should also have a instance class that redirects the permutation, and map it to the actual ranking, lke virtual memory to physical memory");
            return 0;
        }

        static void showUsage()
        {
            Console.WriteLine("Sliding Held Karp algorithm, applied across the top 100 player, or all starters drafted plus another 6 rounds");
            Console.WriteLine("Players are ranked according to their Mvp%");
            Console.WriteLine("Each player has a array whose index represents his pick#");
            Console.WriteLine("draft start 1,2,3,4,5 then 2,1,3,4,5 then 2,3,1,4,5, etc");
            Console.WriteLine("if schedule is available, the End of Season 13gm record is calculated for each team in draft");
            Console.WriteLine("whereever the player was drafted, his team's win/loss record is added in his pick#");
            Console.WriteLine("hopefulle by end of program run, we determine which pick# has highest WinPct");
            Console.WriteLine("hopefulle if he players are drafted too high, other players who he could have gotten, goes to other teams and he losses out as result");
            Console.WriteLine("we can also do a random pick from any rank, to see the effect of drafting player too early");
            Console.WriteLine("we should also be able to use a what if simulation based on a particular season's actual schedule and results");

            Console.WriteLine("Usage:");
            Console.WriteLine("DraftSimulator [Year] -import [url]");
            Console.WriteLine("DraftSimulator [Year] [#Teams] [Pos 1]...[Pos n] ; [Pos 1]...[Pos n]");
            Console.WriteLine("DraftSimulator [Year] [#Teams] ...implied QB RB RB WR WR TE K DST ; QB RB RB WR WR TE K DST");
            Console.WriteLine();
            //                 12345678901234567890123456789012345678901234567890123456789012345678901234567890
        }


        static string GetConnectionString()
        {
            string connectionString = FFToiletBowl.ConnectionString.GetConnectionString();
            return connectionString;

            /*
            string s = ConfigurationManager.AppSettings["SqlServer"];
            if (s != null)
                return "Server=" + s + ";Database=FFToiletBowl;User Id=toilet;Password=toiletbowl;";

            var sqlregistry = RegReader.SqlServerInstance();
            if (sqlregistry != null)
                if (sqlregistry.Count > 0)
                    return "Server=" + sqlregistry[sqlregistry.Count - 1] + ";Database=FFToiletBowl;User Id=toilet;Password=toiletbowl;";

            return "Server=.\\SQLSERVER2008;Database=FFToiletBowl;User Id=toilet;Password=toiletbowl;";*/
        }

        static DataTable getData(string name)
        {
            // Create the command.
            string connectionString = GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "[dbo].[GetDataByWeek]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    SqlParameter param = cmd.CreateParameter();
                    param.Value = name;
                    param.SqlDbType = SqlDbType.VarChar;
                    param.Size = 255;
                    param.ParameterName = "@Table";
                    cmd.Parameters.Add(param);
                    cmd.Connection = connection;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    using (DataSet ds = new DataSet())
                    {
                        da.Fill(ds, "result_name");
                        DataTable dt = ds.Tables["result_name"];
                        return dt;
                    }
                }
            }
        }


        static void setExpertData(int year, string scoring, string url, string expert, List<PlayerListItem> list)
        {
            int rank=1;
            var list2 = list.Where(s=>s.Pos!=null && s.Player!=null && s.Team!=null).Select(s => new ExpertRank() { Year=year, Expert=expert, ScoringSystem=scoring,URL = url, Rank=rank++, PlayerID=s.PlayerID, Player2=s.PlayerURL, Player=s.Player, Team=s.Team, Pos=s.Pos });
            //if(url.Contains("https://www.fftoday.com/") || url.Contains("http://www.fftoday.com/"))
              //  list2 = list.Select(s => new ExpertRank() { Year = year, Expert = expert, ScoringSystem = scoring, URL = url, Rank = rank++, PlayerID = s.PlayerID, Player2 = s.PlayerURL, Player = s.Player, Team = s.Team, Pos = s.Pos });
            Console.WriteLine("Saving [{0} {1}] to database", expert, year);
            var count = Experts.SetExpertRanking(list2);
            Console.WriteLine("Save complete, {0}", count);
        }

        static public void checkUrlForPlayers(int year, string url)
        {
            Dictionary<string, DataRow> pos = new Dictionary<string, DataRow>();
            Dictionary<string, DataRow> team = new Dictionary<string, DataRow>();
            Dictionary<string, List<DataRow>> player = new Dictionary<string, List<DataRow>>();
            var ptp = new Dictionary<string, Dictionary<string, Dictionary<string, List<DataRow>>>>();
            
            
            var dataset = "vwPlayerList";
            for(var i=0; i<10; i++)
                using (var tbl = Reports.GetTableByWeek(dataset, year, 1))
                {
                    if(tbl.Rows.Count==0)
                        break;
                    foreach (DataRow item in tbl.Rows)
                    {
                        if (!pos.ContainsKey((string)item["pos"]))
                            pos.Add((string)item["pos"], item);
                        if (!team.ContainsKey((string)item["team"]))
                            team.Add((string)item["team"], item);
                        var nameparts = ((string)item["player"]).Replace('\'', ' ').Replace(".", "").Split(' '); //remove punctuations, as this makes matching by parts difficult
                        for (int j = 0; j < nameparts.Length; j++)
                            if (!player.ContainsKey(nameparts[j]))
                                player.Add(nameparts[j], new List<DataRow>() { item });
                            else
                                player[nameparts[j]].Add(item);
                        if (!ptp.ContainsKey((string)item["pos"]))
                            ptp.Add((string)item["pos"], new Dictionary<string, Dictionary<string, List<DataRow>>>() { { (string)item["team"], nameparts.ToDictionary(s => s, s => new List<DataRow>() { item }) } });
                        else
                        {
                            if (!ptp[(string)item["pos"]].ContainsKey((string)item["team"]))
                                ptp[(string)item["pos"]].Add((string)item["team"], nameparts.Where(s=>!string.IsNullOrWhiteSpace(s)).ToDictionary(s => s, s => new List<DataRow>() { item }));
                            else
                                foreach (var segment in nameparts)
                                    if (!ptp[(string)item["pos"]][(string)item["team"]].ContainsKey(segment))
                                        ptp[(string)item["pos"]][(string)item["team"]].Add(segment, new List<DataRow>() { item });
                                    else
                                        ptp[(string)item["pos"]][(string)item["team"]][segment].Add(item);
                        }
                    }
                    break;
                }
            __ptp = ptp;


            var playerCounts = new DetectChangingElement(); // new Dictionary<string, int>();
            var posCounts = new DetectChangingElement(); // new Dictionary<string, int>();
            var teamCounts = new DetectChangingElement(); // new Dictionary<string, int>();

            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            //page.Load(url);

            List<Token> last = new List<Token>();
            var count = 0;
            Console.WriteLine("------------------------------------------------------------");
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.BuildTreeFrom(url, builder))
            {
                last.Add(item);
                count++;
                /*
                Console.Write(k++);
                Console.Write("......");
                Console.WriteLine(item);
                */
                if (item is Literal)
                {
                    Console.CursorLeft = 0;
                    Console.Write(item.ActualSymbol);
                    Console.Write("          ");
                    var reversepath = getHPath(item);
                    var parts = item.ActualSymbol.Split(' ');
                    for (int j = 0; j < parts.Length; j++)
                    {
                        var part = parts[j];
                        if (player.ContainsKey(part))
                        {
                            playerCounts.Add(reversepath);
                        }
                        if (team.ContainsKey(part))
                        {
                            var startat = item.ActualSymbol.IndexOf(part);
                            teamCounts.Add("#" + startat + reversepath);
                        }

                        int posrank;
                        if (pos.ContainsKey(part) || (EndWithNumber(part, out posrank) && pos.ContainsKey(part.Substring(0, part.Length - posrank.ToString().Length)))) //also need to consider WR33
                        {
                            var startat = item.ActualSymbol.IndexOf(part);
                            posCounts.Add("#" + startat + reversepath);
                        }
                    }
                }
            }

            //how do we join the counts to produce common table[n]? and order by descending the counts^2 + counts^2
            //then ask which table is best?  show top 5 parsed out for player, team, pos
            var tmp = playerCounts.AllPaths.Join(teamCounts.AllPaths, s => s.Parent, s => s.Parent, (s1, s2) => new Tuple<string, string, string, int, int>(s1.Parent, s1.HPath, s2.HPath, s1.MatchList.Count, s2.MatchList.Count)).ToArray();
#if DEBUG
            var pbug = new HashSet<string>(playerCounts.AllPaths.Select(s=>s.HPath));
            var tbug = new HashSet<string>(teamCounts.AllPaths.Select(s => s.HPath));
            var obug = new HashSet<string>(posCounts.AllPaths.Select(s => s.HPath));
            //System.Diagnostics.Debug.Assert(pbug.Count == playerCounts.AllPaths.Count());
            //System.Diagnostics.Debug.Assert(tbug.Count == teamCounts.AllPaths.Count());
            //System.Diagnostics.Debug.Assert(obug.Count == posCounts.AllPaths.Count());

            var n1bug = new List<string>(tmp.Select(s => s.Item2));
            var n2bug = new List<string>(tmp.Select(s => s.Item3));

            var m1bug = new HashSet<string>(n1bug);
            var m2bug = new HashSet<string>(n2bug);
            //System.Diagnostics.Debug.Assert(pbug.Count == m1bug.Count);
            //System.Diagnostics.Debug.Assert(tbug.Count == m2bug.Count);
#endif //DEBUG

            var joined = playerCounts.AllPaths.Join(teamCounts.AllPaths, s => s.Parent, s => s.Parent, (s1, s2) => new Tuple<string, string, string, int, int>(s1.Parent, s1.HPath, s2.HPath, s1.MatchList.Count, s2.MatchList.Count))
                                              .Join(posCounts.AllPaths, s => s.Item1, s => s.Parent, (s1, s2) => new Tuple<string, string, string, string, int, int, int>(s1.Item1, s1.Item2, s1.Item3, s2.HPath, s1.Item4, s1.Item5, s2.MatchList.Count));

            var best = new List<Tuple<string, string,string,string, int>>();
            foreach (var item in joined.OrderByDescending(s => s.Item5 * s.Item5 + s.Item6 * s.Item6 + s.Item7 * s.Item7))
            {
                best.Add(new Tuple<string, string, string, string, int>(item.Item1, item.Item2, item.Item3, item.Item4, item.Item5 * item.Item5 + item.Item6 * item.Item6 + item.Item7 * item.Item7));
            }

            var hpaths = new Dictionary<Tuple<string, string, string, string, int>, DataExtractorSet>();
            foreach (var item in best)
            {
                var hpath1 = string.Join("/", item.Item2.Replace("UnknownTag", ".").Replace("/Document", string.Empty).ToUpper().Replace("/HYPERLINK", "/A").Replace("/TABLETAG", "/TABLE").Replace("/TBODY", "/.").Replace("/BOLD", "/B").Split('/').Reverse());
                var hpath2 = string.Join("/", item.Item3.Replace("UnknownTag", ".").Replace("/Document", string.Empty).ToUpper().Replace("/HYPERLINK", "/A").Replace("/TABLETAG", "/TABLE").Replace("/TBODY", "/.").Replace("/BOLD", "/B").Split('/').Reverse());
                var hpath3 = string.Join("/", item.Item4.Replace("UnknownTag", ".").Replace("/Document", string.Empty).ToUpper().Replace("/HYPERLINK", "/A").Replace("/TABLETAG", "/TABLE").Replace("/TBODY", "/.").Replace("/BOLD", "/B").Split('/').Reverse());
                hpath1 = hpath1.Substring(0, hpath1.Length-1);
                var divider2 = hpath2.LastIndexOf("/#");
                var divider3 = hpath3.LastIndexOf("/#");
                var startat2 = int.Parse(hpath2.Substring(divider2 + 2));
                var startat3 = int.Parse(hpath3.Substring(divider3 + 2));
                hpath2 = hpath2.Substring(0,divider2);
                hpath3 = hpath3.Substring(0,divider3);
                hpaths.Add(item, new DataExtractorSet(item.Item1 + " | " + item.Item2 + " | " + item.Item3 + " | " + item.Item4, new HtmlPath(hpath1), new HtmlPath(hpath2), new HtmlPath(hpath3), new TextRange(0, 255), new TextRange(startat2, 5), new TextRange(startat3, 5), 0));
            }

            var players = pickatable(last, hpaths);
            if (url.Contains("https://www.fftoday.com/") || url.Contains("http://www.fftoday.com/")) //if fftoday, always replace the ID, with the url
                foreach (var item in players.Where(s => s.Player != null && s.Pos != null && s.Team != null))
                    if (item.PlayerURL != null)
                    {
                        item.PlayerID = Uri.UnescapeDataString(item.PlayerURL.Replace("../../stats/players/", "/stats/players/").Replace("../stats/players/", "/stats/players/"));
                        item.Player = string.Join(" ", item.Player.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                        item.PlayerURL = string.Empty;
                    }
            
            //save to database
            //ask name of expert
            Console.WriteLine("Which Scoring System is this ranking applied to?");
            var scoringsystem = Console.ReadLine();
            Console.WriteLine("What is the name of this expert?");
            var expert = Console.ReadLine();

            foreach(var item in players.Where(s=>s.Player!=null && s.Pos!=null && s.Team!=null))
                item.Pos = item.Pos.Replace("0", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "").Replace("5", "").Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "");
            setExpertData(year, scoringsystem, url, expert, players);
        }

        static Dictionary<string, Dictionary<string, Dictionary<string, List<DataRow>>>> __ptp;
        static bool matchptp(string pos, string team, string[] playername)
        {
            var ptp = __ptp;
            if(ptp.ContainsKey(pos))
                if(ptp[pos].ContainsKey(team)) 
                {
                    foreach (var item in playername)
                        if (ptp[pos][team].ContainsKey(item))
                            return true;
                    return false;
                }
            return false;
        }
        
        static bool EndWithNumber(string text, out int num)
        {
            var len = text.Length-1;
            while (len >= 0 && text[len] >= '0' && text[len] <= '9')
                len--;
            if (len >= 0)
            {
                if (len < text.Length - 1 && int.TryParse(text.Substring(len + 1), out num))
                    return true;
            }
            num = 0;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="replay"></param>
        /// <param name="tablelist">reversehpath,  player hpath,team hpath,pos hpath,  player range, team range, pos range</param>
        static public Dictionary<DataExtractorSet, List<PlayerListItem>>
            reextract(List<Token> replay, Dictionary<Tuple<string, string, string, string, int>, DataExtractorSet> tablelist)
        {
            var ptp = __ptp;
            var list = tablelist.ToDictionary(k => k.Value, v => new List<PlayerListItem>());
            foreach (var item in replay) //page.Tokens.BuildTree(builder))
            {
                foreach (var test in list)
                {
                    var last = test.Value.Count - 1;
                    if (last < 0)
                    {
                        test.Value.Add(new PlayerListItem());
                        last = 0;
                    }
                    var lastitem = test.Value[last];
                    //if (item.ActualSymbol.Contains("Kamara"))
                    //  Console.Write("break");
                    if (test.Key.PlayerSelector.IsMatch(item))
                    {
                        lastitem.Player = test.Key.PlayerSubstr.Substring(item.ActualSymbol).Trim();
                        if (item.ParentNode is Hyperlink)
                        {
                            var link=(Hyperlink)item.ParentNode;
                            var playerurl = link.Href;
                            lastitem.PlayerURL = playerurl;
                        }
                        else if (item.ParentNode.ParentNode is Hyperlink)
                        {
                            var link = (Hyperlink)item.ParentNode.ParentNode;
                            var playerurl = link.Href;
                            lastitem.PlayerURL = playerurl;
                        }
                        else if (item.ParentNode.ParentNode.ParentNode is Hyperlink)
                        {
                            var link = (Hyperlink)item.ParentNode.ParentNode.ParentNode;
                            var playerurl = link.Href;
                            lastitem.PlayerURL = playerurl;
                        }
                    }
                    if (test.Key.TeamSelector.IsMatch(item))
                    {
                        lastitem.Team = test.Key.TeamSubstr.Substring(item.ActualSymbol).Trim();
                    }
                    if (test.Key.PosSelector.IsMatch(item))
                    {
                        lastitem.Pos = test.Key.PosSubstr.Substring(item.ActualSymbol).Trim();
                    }

                    if (lastitem.Player != null && lastitem.Pos != null && lastitem.Team != null)
                    {
                        var n = lastitem.Player.Split(' ');
                        if (matchptp(lastitem.Pos, lastitem.Team, n))
                        {
                            test.Key.KnownPlayerCount++;
                            var rowswithname = ptp[lastitem.Pos][lastitem.Team].Where(s => Array.IndexOf(n, s.Key) >= 0).Select(s=>s.Value);
                            IEnumerable<DataRow> set = rowswithname.FirstOrDefault();
                            if (set != null)
                            {
                                foreach (var r in rowswithname.Skip(1))
                                    set = set.Intersect(r);
                                var possible = set.ToArray();
                                if(possible.Length==1)
                                    lastitem.PlayerID = (string)possible[0]["PlayerID"];
                            }
                        }
                        test.Value.Add(new PlayerListItem());
                    }
                }
            }
            return list;
        }
        static public List<PlayerListItem> pickatable(List<Token> replay, Dictionary<Tuple<string, string, string, string, int>, DataExtractorSet> tablelist)
        {
            //Page page = new Page();
            //page.DebugMode = true;
            //page.TrimWhitespace = true;
            //page.Load(url);
            
            var list = reextract(replay, tablelist);

            /*
            var list = tablelist.ToDictionary(k=>k.Value, v=>new List<PlayerListItem>());

            Console.WriteLine("------------------------------------------------------------");
            //var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in replay) //page.Tokens.BuildTree(builder))
            {
                foreach(var test in list)
                {
                    var last = test.Value.Count-1;
                    if (last < 0)
                    {
                        test.Value.Add(new PlayerListItem());
                        last = 0;
                    }
                    var lastitem = test.Value[last];
                    //if (item.ActualSymbol.Contains("Kamara"))
                      //  Console.Write("break");
                    if (test.Key.Item2.IsMatch(item))
                    {
                        lastitem.Player = item.ActualSymbol.Trim();
                    }
                    if (test.Key.Item3.IsMatch(item))
                    {
                        lastitem.Team = item.ActualSymbol.Substring(test.Key.Item5,5).Trim();
                    }
                    if (test.Key.Item4.IsMatch(item))
                    {
                        lastitem.Pos = item.ActualSymbol.Substring(test.Key.Item6,5).Trim();
                    }

                    if (lastitem.Player != null && lastitem.Pos != null && lastitem.Team != null)
                        test.Value.Add(new PlayerListItem());
                }
            }*/

            var counter = 0;
            foreach (var item in list.OrderByDescending(s => s.Key.KnownPlayerCount).Take(6))
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Table {0} ({1}/{2}), {3}", counter++, item.Key.KnownPlayerCount, item.Value.Count, item.Key.SharedParent);
                Console.WriteLine("Tm    Pos   Player");
                Console.WriteLine("---   ---   ------------------------------");
                foreach (var record in item.Value.Take(6))
                    Console.WriteLine("{0, 3} | {1,3} | {2}", record.Team, record.Pos, record.Player);
            }
            Console.WriteLine("Which table do you wish to import?");
            var inputindex = -1;
            var input = Console.ReadLine();
            while (!int.TryParse(input, out inputindex) || inputindex < 0 || inputindex >= list.Count)
            {
                Console.WriteLine("Try again.  Pick number between 0 and {0}. Which table do you wish to import?", list.Count-1);
                input = Console.ReadLine();
            }
            var extractor = list.ToList()[inputindex].Key;
            
            var inputindex2 = -1;
            while (inputindex2 != 0)
            {
                Console.WriteLine("Do you wish to change parsing for one of the fields (0) Start import (1)Tm (2)Pos (3)Player?");

                inputindex2 = -1;
                var input2 = Console.ReadLine();
                while (!int.TryParse(input2, out inputindex2) || inputindex2 < 0 || inputindex2 > 3)
                {
                    Console.WriteLine("Do you wish to change parsing for one of the fields (0) Start import (1)Tm (2)Pos (3)Player?", list.Count - 1);
                    input = Console.ReadLine();
                }
                HtmlPath hpath = null;
                TextRange range = new TextRange() { Start = 0, Len = 5 };
                if (inputindex2 == 1)
                {
                    hpath = extractor.TeamSelector;
                    range = extractor.TeamSubstr;
                }
                else if (inputindex2 == 2)
                {
                    hpath = extractor.PosSelector;
                    range = extractor.PosSubstr;
                }
                else if (inputindex2 == 3)
                {
                    hpath = extractor.PlayerSelector;
                    range = extractor.PlayerSubstr;
                }
                
                
                int lines = 19;
                int skip = 0;
                char delimiter = '\0';
                int col = 0;
                var t = Console.CursorTop;
                var l = Console.CursorLeft;
                if (t + Console.WindowHeight > Console.BufferHeight)
                    t = Console.BufferHeight - Console.WindowHeight;
                ConsoleModifiers alt = default(ConsoleModifiers);
                ConsoleKey pressed = default(ConsoleKey);
                var bg = Console.BackgroundColor;
                var fg = Console.ForegroundColor;
                while (inputindex2!=0 && !(pressed == ConsoleKey.Enter && alt== ConsoleModifiers.Shift))
                {
                    var skip2 = skip;
                    var lines2 = lines;
                    Console.WriteLine("Shift-enter to accept extraction param.  Up/down keys to view column data.");
                    Console.WriteLine("right/left to change hilight start, shft-right/left to change hilite width");
                    Console.WriteLine("any other key to use as delimiter, and 0-9 keys to select column");
                    Console.WriteLine(" 123456789 123456789 123456789 123456789 123456789 123456789 ");
                    foreach (var item in replay) //page.Tokens.BuildTree(builder))
                    {
                        if (hpath.IsMatch(item))
                        {
                            if (skip2-- <= 0)
                                if (delimiter != '\0')
                                {
                                    var parts = item.ActualSymbol.Split(delimiter);
                                    Console.Write("|");
                                    for(int i=0; i<parts.Length; i++)
                                        if (i == col)
                                        {
                                            Console.BackgroundColor = bg;
                                            Console.ForegroundColor = fg;
                                            if (i != 0)
                                                Console.Write(delimiter);
                                            Console.BackgroundColor = fg;
                                            Console.ForegroundColor = bg;
                                            Console.Write(parts[i]);
                                        }
                                        else
                                        {
                                            Console.BackgroundColor = bg;
                                            Console.ForegroundColor = fg;
                                            if (i != 0)
                                                Console.Write(delimiter);
                                            Console.Write(parts[i]);
                                        }
                                    Console.WriteLine("|        ");

                                    if (--lines2 == 0) break;
                                }
                                else
                                {
                                    Console.Write("|");
                                    Console.Write(range.Before(item.ActualSymbol));
                                    Console.BackgroundColor = fg;
                                    Console.ForegroundColor = bg;
                                    Console.Write(range.Substring(item.ActualSymbol));
                                    Console.BackgroundColor = bg;
                                    Console.ForegroundColor = fg;
                                    Console.WriteLine(range.After(item.ActualSymbol) + "|      ");

                                    if (--lines2 == 0) break;
                                }
                        }
                    }
                    var key = Console.ReadKey(false);
                    pressed = key.Key;
                    alt = key.Modifiers;
                    if (key.Key == ConsoleKey.DownArrow)
                        skip++;
                    else if (key.Key == ConsoleKey.UpArrow)
                        skip--;
                    else if (key.Key == ConsoleKey.RightArrow)
                    {
                        delimiter = '\0';
                        if (key.Modifiers == ConsoleModifiers.Shift) range.Len++; else range.Start++;
                    }
                    else if (key.Key == ConsoleKey.LeftArrow)
                    {
                        delimiter = '\0';
                        if (key.Modifiers == ConsoleModifiers.Shift) range.Len--; else range.Start--;
                    }
                    else if (key.KeyChar >= '0' && key.KeyChar <= '9')
                        col = key.KeyChar - '0';
                    else
                        delimiter = key.KeyChar;

                    if (!(pressed == ConsoleKey.Enter && alt == ConsoleModifiers.Shift))
                        Console.SetCursorPosition(l, t);
                }
                if (inputindex2 == 1)
                {
                    extractor.TeamSubstr.Start = range.Start;
                    extractor.TeamSubstr.Len = range.Len;
                }
                else if (inputindex2 == 2)
                {
                    extractor.PosSubstr.Start = range.Start;
                    extractor.PosSubstr.Len = range.Len;
                }
                else if (inputindex2 == 3)
                {
                    extractor.PlayerSubstr.Start = range.Start;
                    extractor.PlayerSubstr.Len = range.Len;
                }
                var extract2 = reextract(replay, new Dictionary<Tuple<string, string, string, string, int>, DataExtractorSet>() { 
                    { new Tuple<string, string, string, string, int>(null, null, null, null, 0), extractor } 
                });
                list.ToList()[inputindex].Value.Clear();
                list.ToList()[inputindex].Value.AddRange(extract2.ToList()[0].Value);
            }


            return list.ToList()[inputindex].Value;
        }

        static private string getLatestTable(string reversehpath)
        {
            var index = reversehpath.IndexOf("TableTag");
            if (index < 0)
                return null;
            else
                return reversehpath.Substring(index);
        }

        static private string getHPath(Token item)
        {
            var active = item;
            StringBuilder sb = new StringBuilder();
            while (active != null)
            {
                //if(sb.Length!=0)
                sb.Append("/");
                var parent = active.ParentNode;
                var type = active.GetType();
                if (parent != null && !(active is Literal)) // do not record position of literal.  It makes no sense, and selector fails (surprisingly! bug?) on position selector for Literal.
                    sb.AppendFormat("{0}[{1}]", type.Name, ((Node)parent).ChildElements.Count(s => s.GetType() == type)-1);
                else
                    sb.AppendFormat("{0}", active.GetType().Name);

                active = parent;
            }
            return sb.ToString();
        }


        static private PlayerDraftSimulation[] getExpertRankings(int year)
        {
            var dataset = "crosstabFutureRank"; //"vwModeledExpertSeasonRanking";
            using (var tbl = Reports.GetTableByWeek(dataset, year, 1))
            using (var view = new DataView(tbl))
            {
                //view.RowFilter = string.Format("[TeamsInLeague]={0} AND [StartingSlots]='{1}' AND [BenchSlots]='{2}'", 14, "QB,RB,RB,WR,WR,TE,RB|WR|TE,K,DST", "QB,RB,RB,WR,WR,TE,*,*,K");
                
                int counter=0;
                PlayerDraftSimulation[] result = new PlayerDraftSimulation[view.Count]; //[tbl.Rows.Count];
                foreach (DataRowView item in view)
                {
                    result[counter++] = new PlayerDraftSimulation() {
                        ScoringSystem = (string)item["ScoringSystem"], 
                        PlayerID = (string)item["PlayerID"],
                        Player = item["Player"]==DBNull.Value ? null : (string)item["Player"],
                        Pos = (string)item["Pos"],
                        Team = (string)item["Team"],
                        MvpPct = (double)item["ImpliedMvpPct"],
                        ImpliedPoints = (double)item["impliedpts"],
                        ImpliedPointsStDev = (double)item["impliedPosStdev"],
                    };
                }
                return result;
            }
        }


        static public void MonteCarloDraftAndSeason(int year, int teams, string[] starter, string[] bench)
        {
            Console.WriteLine("Getting players eligible for draft in {0}", year);
            var players = getExpertRankings(year);
            var len = players.Length;
            Console.WriteLine("Generating swap orders");
            var changes = getAllPermutations(10);
            var chgcount = changes.Length;
            var rounds = len/teams + (len%teams==0 ? 0 : 1);
            var draft = Enumerable.Range(0, teams).Select(s => new DraftPick[starter.Length + bench.Length]).ToArray();
            var team = Enumerable.Range(0, teams).Select(s => new DraftPick[starter.Length + bench.Length]).ToArray();
            var rostercounts = starter.Concat(bench).GroupBy(s=>s).ToDictionary(s=>s.Key, s=>s.Count());
            var slotcount = starter.Length + bench.Length;
            VirtualRank vrank = new VirtualRank();
            DraftBoard available = new DraftBoard();
            //var turn = 0;
            for (int i = 0; i < rounds; i++)
            {
                vrank.VirtualStart = i;
                for (int j = 0; i < chgcount; j++)
                {
                    vrank.VirtualMap = changes[j];
                    available.ResetBoard(players, vrank);

                    //simulate snake draft until roster is filled
                    for (int k = 0; k < len; k++)
                    {
                        var round = k / teams;
                        if(round>=slotcount)
                            break;
                        var turn = (round & 1) == 0 ? k % teams : teams - (k % teams) - 1;
                        var pick = draft[turn][round] = available.DraftFor(draft[turn], rostercounts);
                        Console.WriteLine("{0} {1} {2}", round, turn, pick.Player.Player == null ? pick.Player.PlayerID : pick.Player.Player);
                    }

                    // create the starting lineup
                    var rosterslots = starter.Concat(bench).ToArray();
                    var counter=0;
                    foreach (var item in draft)
                    {
                        var roster = new List<DraftPick>();
                        foreach (var pos in rosterslots)
                        {
                            var index = item.TakeWhile(s => s==null || !pos.Contains(s.Player.Pos)).Count();
                            var playerForPos = item[index];
                            // System.Diagnostics.Debug.Assert(playerpos.Pos==pos);
                            if (pos.Contains(playerForPos.Player.Pos))
                            {
                                item[index] = null;
                                roster.Add(playerForPos);
                            }
                            else
                                throw new Exception("You need to fix the draft logic, so that it drafts positions it needs.  or ignore problems w bench...");
                        }
                        team[counter++] = roster.ToArray();
                    }

                    //simulate 13 game season, replacing each starter w bench player once during bye week
                    for (var h = 0; h < teams; h++)
                        for (var a = h + 1; a < teams; a++)
                            for (int g = 0; g < 13; g++)
                                model(starter, team[h], team[a], g);

                    //save
                    
                }
            }

        }

        static void model(string[] starter, DraftPick[] a, DraftPick[] b, int bye)
        {
            double sum = 0;
            var len = starter.Length;
            for (int i = 0; i < len; i++) //we aren't actually taking the bye week in consideration for now... we are assuming only one player is on bye week at any time
                if(i==bye)
                {
                    if (starter[i] != "*")
                    {
                        System.Diagnostics.Debug.Assert(a[i].Player.Pos == starter[i]);
                        System.Diagnostics.Debug.Assert(b[i].Player.Pos == starter[i]);
                    }
                    sum += a[i].Player.MvpPct - b[i].Player.MvpPct;
                }
            var avg = sum / len;
            foreach (var item in a)
            {
                item.Player.WinPctSum[item.Pick] += 0.500 + avg;
                item.Player.WinPctCount[item.Pick]++;
            }
            foreach (var item in b)
            {
                item.Player.WinPctSum[item.Pick] += 0.500 - avg;
                item.Player.WinPctCount[item.Pick]++;
            }
        }
        static void simulate(string[] starter, DraftPick[] a, DraftPick[] b, int bye)
        {
            double sumA = 0;
            double sumB = 0;
            var len = starter.Length;
            for (int i = 0; i < len; i++) //we aren't actually taking the bye week in consideration for now... we are assuming only one player is on bye week at any time
                if (i == bye)
                {
                    if (starter[i] != "*")
                    {
                        System.Diagnostics.Debug.Assert(a[i].Player.Pos == starter[i]);
                        System.Diagnostics.Debug.Assert(b[i].Player.Pos == starter[i]);
                    }
                    sumA += a[i].Player.SimulatePoints();
                    sumB += b[i].Player.SimulatePoints();
                }
            foreach (var item in a)
            {
                item.Player.WinPctSum[item.Pick] += sumA > sumB ? 1 : 0;
                if (sumA != sumB)
                    item.Player.WinPctCount[item.Pick]++;
            }
            foreach (var item in b)
            {
                item.Player.WinPctSum[item.Pick] += sumA < sumB ? 1 : 0;
                if(sumA!=sumB)
                    item.Player.WinPctCount[item.Pick]++;
            }
        }

        static int[][] getAllPermutations(int n)
        {
            return getPermutations(n).Select(s => s.ToArray()).ToArray();
        }
        static IEnumerable<List<int>> getPermutations(int n)
        {
            if (n <= 1)
            {
                yield return new List<int>() { 0 };
            }
            else
            {
                var n1 = n - 1;
                foreach (var item in getPermutations(n1))
                {
                    for (int i = n1; i >= 0; i--)
                        if (i == n1)
                        {
                            item.Add(n1);
                            yield return item;
                        }
                        else
                        {
                            var copy = item.ToList();
                            copy.Insert(i, n1);
                            yield return copy;
                        }
                }
            }
        }

        //http://www.onlinestatbook.com/2/calculators/inverse_normal_dist.html
        public enum DistArea
        {
            Above, Below, Between, Outside
        }
        static public double InverseNormalDist(double mean, double stdev, double prob)
        {
            return InverseNormalDist(mean, stdev, prob, DistArea.Below);
        }
        static public double InverseNormalDist(double mean, double stdev, double prob, DistArea area) 
        {
	        double ll, ul, x1,x2;
	        var M = mean;
	        var sd = stdev;
	        var tail = false;
	
	        var p = prob;
	        x1 = zinv(p);
	        x1=M+sd*x1;
	
	        //above
            if (area==DistArea.Above) 
            {
		        x1 = zinv(p);
		        x1=-M+sd*x1;
		        ul=M+3.1*sd;
		        ll=-x1;
		        
		        //ll = Math.Round(1000*ll)/1000;
		
		        return ll;
	        }
	        else if (area==DistArea.Above) //below
            {
		        ll=M-3.1*sd;
		        ul=x1;
		        //ul = Math.Round(1000*ul)/1000;
                return ul;
	        }
            else if (area==DistArea.Between) //between
            {
                var p2=p/2;
                x1=zinv(.5-p2);
                ll=x1;
                ul=-x1;
                //ll=Math.Round((M+sd*ll)*1000)/1000;
                //ul=Math.Round((M+sd*ul)*1000)/1000;
                
                //document.getElementById("betweenX").value=ll + " and " + ul
		
	            return ul-ll;
            }
            else if (area==DistArea.Outside) //outside
            {
                var p2=p/2;
                x1=zinv(p2);
                ll=x1;
                ul=-x1;
                //ll=Math.round((M+sd*ll)*1000)/1000
                //ul=Math.round((M+sd*ul)*1000)/1000
                //document.getElementById("outsideX").value=ll + " and " + ul
                return ll + ul;
            }
            throw new Exception("unknown type");
        }

        static double zinv(double prob) 
        {
            var p = prob;
            const double a1 = -39.6968302866538, a2 = 220.946098424521, a3 = -275.928510446969;
            const double a4 = 138.357751867269, a5 = -30.6647980661472, a6 = 2.50662827745924;
            const double b1 = -54.4760987982241, b2 = 161.585836858041, b3 = -155.698979859887;
            const double b4 = 66.8013118877197, b5 = -13.2806815528857, c1 = -7.78489400243029E-03;
            const double c2 = -0.322396458041136, c3 = -2.40075827716184, c4 = -2.54973253934373;
            const double c5 = 4.37466414146497, c6 = 2.93816398269878, d1 = 7.78469570904146E-03;
            const double d2 = 0.32246712907004, d3 = 2.445134137143, d4 = 3.75440866190742;
            const double p_low = 0.02425, p_high = 1 - p_low;
            double q, r;
            double retVal;

            if ((p < 0) || (p > 1)) 
            {
                throw new Exception("p out of range.");
                retVal = 0;
            }
            else if (p < p_low) {
                q = Math.Sqrt(-2 * Math.Log(p));
                retVal = (((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) / ((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
            }
            else if (p <= p_high) {
                q = p - 0.5;
                r = q * q;
                retVal = (((((a1 * r + a2) * r + a3) * r + a4) * r + a5) * r + a6) * q / (((((b1 * r + b2) * r + b3) * r + b4) * r + b5) * r + 1);
            }
            else {
                q = Math.Sqrt(-2 * Math.Log(1 - p));
                retVal = -(((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) / ((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
            }

            return retVal;

        }

        static double zProb(double z) 
        {
            bool flag;

            if (z < -7) {
                return 0.0;
            }
            if (z > 7) {
                return 1.0;
            }


            if (z < 0.0) {
                flag = true;
            }
            else {
                flag = false;
            }

            z = Math.Abs(z);
            var b = 0.0d;
            var s = Math.Sqrt(2) / 3 * z;
            var HH = .5d;
            for (var i = 0; i < 12; i++) 
            {
                var a = Math.Exp(-HH * HH / 9) * Math.Sin(HH * s) / HH;
                b = b + a;
                HH = HH + 1.0;
            }
            var p = .5 - b / Math.PI;
        //p=b/Math.PI;
            if (!flag) {
                p = 1.0 - p;
            }
            return p;
        }
    }

    public class PlayerListItem
    {
        public string PlayerID;
        public string PlayerURL;
        public string Player;
        public string Team;
        public string Pos;
    }

    public class PlayerDraftSimulation
    {
        public string ScoringSystem;
        public string PlayerID;
        public string Player;
        public string Pos;
        public string Team;
        public double MvpPct;
        
        public double ImpliedPoints;
        public double ImpliedPointsStDev;
        public double ImpliedAvg { get { return this.ImpliedPoints / 16; } }
        public double ImpliedAvgStDev { get { return this.ImpliedPointsStDev / 16; } } //is this right??  It should be bigger than /16, shouldn't it?  /sqrt(16)?
        public int SimulatePoints()
        {
            var rnd = new Random(Environment.TickCount);
            var pts = Program.InverseNormalDist(this.ImpliedAvg, this.ImpliedAvgStDev, rnd.NextDouble());
            return Convert.ToInt32(pts);
        }

        public int ByeWeek;

        public double[] WinPctSum = new double[300];
        public int[] WinPctCount = new int[300];

        public double MaxWinPct
        {
            get
            {
                var i = this.MaxWinPctRound;
                return this.WinPctSum[i] / (double)this.WinPctCount[i];
            }
        }
        public int MaxWinPctRound
        {
            get
            {
                double max = 0;
                var imax = -1;
                for (var i = 0; i < 300; i++)
                {
                    var pct = this.WinPctSum[i] / (double)this.WinPctCount[i];
                    if (pct > max)
                    {
                        imax = i;
                        max = pct;
                    }
                }
                return imax;
            }
        }
    }

    // we need to plan for shuffling players between 3 rounds, to really observe the effect of opportunity cost, bc if you let go of a player, what we really want to know, if when he wont be there the next round, or if we can skip a round to draft, and it not really cost anything.
    public class VirtualRank
    {
        public int VirtualStart;
        public int[] VirtualMap;
        public int[] RealMap = Enumerable.Range(0, 300).ToArray();
        
        int[] _offset = { 0, 1, 2, 3, 9, 16, 26, 42, 68, 100 }; //last item is 4 rounds away
         
        int _teams = 16;
        public int Teams { 
            get { return _teams; } 
            set 
            {
                for (int i = 1; i < 10; i++)
                    if (_offset[i] * _teams / 16 > _offset[i - 1])
                        _offset[i] = _offset[i] * _teams / 16;
            } 
        }

        public int GetRealAddress(int virtualaddress)
        {
            var offset = virtualaddress - this.VirtualStart;
            for (int i = 0; i < 10; i++) //0123456789
                if (_offset[i] == offset) //02468
                {
                    return this.VirtualStart + _offset[VirtualMap[i]];
                }

            return this.RealMap[virtualaddress];
        }

    }
    public class DraftBoard
    {
        int _pick = 1;
        public List<PlayerDraftSimulation> Board;
        public List<PlayerDraftSimulation> Order;
        public void ResetBoard(PlayerDraftSimulation[] origboard, VirtualRank vrank)
        {
            _pick = 1;
            this.Order = new List<PlayerDraftSimulation>();
            this.Board = Enumerable.Range(0, origboard.Length).Select(s=> origboard[vrank.GetRealAddress(s)]).ToList();

            System.Diagnostics.Debug.Assert(this.Board.Count==this.Board.Distinct().Count());
        }
        public DraftPick DraftFor(DraftPick[] draft, Dictionary<string, int> roster)
        {
            var slots = availablePos(draft, roster);
            
            //get best player at an available pos slot
            PlayerDraftSimulation item = this.Board.SkipWhile(s => matchingPos(s.Pos,slots)==null).FirstOrDefault();
            if (item == null)
            {
            }
            else
                Console.WriteLine("{0} drafted out of [{1}]", item.Pos, string.Join(",", slots));
            this.Board.Remove(item);
            this.Order.Add(item);
            return new DraftPick(_pick++, item);
        }
        Dictionary<string, int> availablePos(DraftPick[] draft, Dictionary<string, int> roster)
        {
            var remaining = new Dictionary<string, int>(roster);
            foreach (var item in draft.TakeWhile(s=>s!=null))
            {
                string pairkey = matchingPos(item.Player.Pos, remaining);

                if (pairkey!=null)
                {
                    var left = --remaining[pairkey];
                    if (left == 0)
                        remaining.Remove(pairkey);
                }
            }
            return remaining;
        }
        string matchingPos(string pos, Dictionary<string, int> slots)
        {
            string pairkey = null;
            if (slots.ContainsKey(pos))
                return pos;
            else if ((pairkey = slots.Keys.FirstOrDefault(s => s.Contains(pos))) != null)
                return pairkey;
            else if (slots.ContainsKey("*"))
                return "*";
            return null;
        }
    }
    public class DraftPick 
    {
        public DraftPick(int pick, PlayerDraftSimulation player) 
        {
            this.Pick = pick;
            this.Player = player;
        }
        public int Pick {get; private set;}
        public PlayerDraftSimulation Player {get; private set;}
    }

    public class DetectChangingElement
    {
        List<ParentChildPath> _path = new List<ParentChildPath>();
        List<string> _all = new List<string>();
        string _last;
        public void Add(string path)
        {
            if (_last == path)
                return;
            _last = path;
            foreach (var item in _path.Reverse<ParentChildPath>())
                if (item.Match(path))
                    return;
            foreach (var item in _all)
            {
                var repeatpath = ParentChildPath.CommonParent(path, item);
                if (repeatpath != null)
                {
                    _path.Add(repeatpath);
                    break;
                }
            }
            _all.Add(path);
        }
        public List<ParentChildPath> Top5 
        {
            get
            {
                return _path.OrderByDescending(s=>s.MatchList.Count).Take(5).ToList();
            }
        }
        public IEnumerable<ParentChildPath> AllPaths
        {
            get
            {
                return _path.OrderByDescending(s => s.MatchList.Count);
            }
        }
    }
    public class ParentChildPath
    {
        public ParentChildPath(string p, string c) { this.Parent = p; this.Child = c; }
        
        public string Parent;
        public string Child;
        
        List<string> _list = new List<string>();
        public List<string> MatchList { get { return _list; } }
        public bool Match(string item)
        {
            if (item.StartsWith(this.Child) && item.EndsWith(this.Parent))
            {
                _list.Add(item);
                return true;
            }
            else 
                return false;
        }
        public string HPath
        {
            get
            {
                return this.Child + this.Parent;
            }
        }
        public override string ToString()
        {
            return this.HPath;
        }

        static public ParentChildPath CommonParent(string item1, string item2)
        {
            if(item1==item2)
                return new ParentChildPath(item1, string.Empty);

            var len = Math.Min(item1.Length, item2.Length);
            int ii = 0;
            int i = 0;
            while (i < len && item1[i] == item2[i])
                if (item1[i] == '[')
                    ii = i++;
                else
                    i++;

            int jj = 0;
            int j = 1;
            while (j < len && item1[item1.Length - j] == item2[item2.Length - j])
                if (item1[item1.Length - j] == ']')
                    jj = j++;
                else
                    j++;

            int tmp=0;
            var num1 = item1.Substring(ii+1, item1.Length - jj - ii-1);
            var num2 = item2.Substring(ii+1, item2.Length - jj - ii-1);
            if (ii != 0 && jj > 1 && int.TryParse(num1, out tmp) && int.TryParse(num2, out tmp))
                return new ParentChildPath(item1.Substring(item2.Length - jj+1, jj - 1), item1.Substring(0, ii));
            else
                return null;
        }
    }
    public class TextRange
    {
        public TextRange() { }
        public TextRange(int start, int len) { this.Start = start; this.Len = len; }

        public int Start;
        public int Len;
        public string Substring(string text)
        {
            if (this.Start > text.Length)
                return null;
            else if (this.Start+this.Len > text.Length)
                return text.Substring(this.Start);
            else
                return text.Substring(this.Start, this.Len);
        }
        public string Before(string text)
        {
            if (this.Start < text.Length)
                return text.Substring(0, this.Start);
            else
                return text;
        }
        public string After(string text)
        {
            if (text.Length > this.Start + this.Len)
                return text.Substring(this.Start + this.Len);
            else 
                return null;
        }
    }


    public class DataExtractorSet
    {
        public DataExtractorSet() { }
        public DataExtractorSet(string sharedparent, HtmlPath playerselector, HtmlPath teamselector, HtmlPath posselector, TextRange playersubstr, TextRange teamsubstr, TextRange possubstr, int knownplayercount) 
        {
            this.SharedParent = sharedparent;
            this.PlayerSelector = playerselector;
            this.TeamSelector = teamselector;
            this.PosSelector = posselector;

            this.PlayerSubstr = playersubstr;
            this.TeamSubstr = teamsubstr;
            this.PosSubstr = possubstr;

            this.KnownPlayerCount = knownplayercount;
        }

        public string SharedParent;
        public HtmlPath PlayerSelector;
        public HtmlPath TeamSelector;
        public HtmlPath PosSelector;

        public TextRange PlayerSubstr;
        public TextRange TeamSubstr;
        public TextRange PosSubstr;

        public int KnownPlayerCount;
    }
}
