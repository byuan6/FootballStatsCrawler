using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.IO;

namespace TwoTeamMonteCarlo
{
    public class Program
    {
        static int GmWeek=0;
        static void Main(string[] args)
        {
            int indexHelp = Array.IndexOf<string>(args, "-?");
            if (indexHelp >= 0)
            {
                showUsage();
                return;
            }

            string dataset = "vwPtsForPlayersWhoPlayedAtLeastHalfSeason";
            string byeweekset = null;
            if (args.Length > 1 && args[0] == "All")
            {
                GameCenter.Title="All";
                dataset = "vwStatsPointsForCityIsland";
                Console.WriteLine("All. Uses all player data, even the one shot wonders");
            }
            else if (args.Length > 0 && args[0] == "Season")
            {
                GameCenter.Title = "Season w Injury";
                dataset = "vwStatsPointsForCityIslandPlusOut";
                Console.WriteLine("Season. Using out weeks... To determine who was more valuable during entire season.  Penalizes injuries.");
            }
            else if (args.Length > 0 && args[0] == "Roster")
            {
                GameCenter.Title = "Rosterable comparison";
                dataset = "vwStatsPointsForCityIslandMinusExceptRosterable";
                Console.WriteLine("Roster. Using top players at position that would fill roster");
            }
            else if (args.Length > 0 && args[0] == "Starter")
            {
                GameCenter.Title = "Starters only comparison";
                dataset = "vwStatsPointsForCityIslandMinusExceptStartable";
                Console.WriteLine("Starter. Using top players at position that would fill start spots");
            }
            else
            {
                GameCenter.Title = "Played at least 1/2 season";
                Console.WriteLine("Default.  Not Season, Roster or Starter. Using Players with more than 3 performances... To determine who is valuable when they played");
            }

            DataTable includeplayers = null;
            if (args.Length == 3 && args[1] == "-includefromfile" && File.Exists(args[2]))
            {
                var iplayers = File.ReadAllLines(args[2]);
                includeplayers = GetDataForOnly(iplayers);

                GameCenter.Title += " (incl xtra players)";
                dataset = "vwStatsPointsForCityIslandMinusExceptStartable";
                Console.WriteLine("Starter. Using top players at position that would fill start spots");
            }

            int indexForceThreads = Array.IndexOf<string>(args, "-threads");
            if (indexForceThreads >= 0 && (indexForceThreads + 1) < args.Length)
                if (!int.TryParse(args[indexForceThreads + 1], out forceThreads))
                    Console.WriteLine("Invalid number of threads submitted.  Ignoring.");

            Console.WriteLine("Getting statistics...");
            DataTable universe = getData(dataset);
            Console.WriteLine("{0} Records", universe.Rows.Count);
            if (universe.Rows.Count == 0)
            {
                Console.WriteLine("No statistics from database... Aborting!");
                return;
            }

            Console.WriteLine("Indexing...");
            Dictionary<Pos, List<DataRow>> dividedByPos = IndexPos(universe);

            Console.WriteLine();
            Console.WriteLine("Generating Mock Scenarios...");

            List<ThreadInputOutput> threadBuffers = spinupWorkers(universe, dividedByPos);
            var threadcount = threadBuffers==null ? 0 : threadBuffers.Count;

            ManualResetEvent signal = new ManualResetEvent(true);
            for (int i = 0; i < Console.WindowHeight; i++) Console.WriteLine();
            Dictionary<string, Proxy> players = new Dictionary<string, Proxy>();
            Duel pair = default(Duel);
            Team winner = default(Team);
            Team loser = default(Team);
            int iterations = 1000000000;
            int gamecenterdelay = 10000;
            int leaderboarddelay = 20000;
            int nextmerge = 0;
            int joindelay = universe.Rows.Count; //let's give it chance to fill up, before re-merging with the set.
            int savedelay = 10000000;
            for (int i = 0; i < iterations; i++)
            {
                pair = RandomStat(dividedByPos, dividedByPos);

                if (includeplayers != null)
                {
                    var turn = i % includeplayers.Rows.Count;
                    var replacement = includeplayers.Rows[turn];
                    var rpos = replacement["pos"].ToString();
                    if (rpos == "QB")
                        pair.Left.QB = replacement;
                    else if (rpos == "RB")
                        pair.Left.RB1 = replacement;
                    else if (rpos == "WR")
                        pair.Left.WR1 = replacement;
                    else if (rpos == "TE")
                        pair.Left.TE = replacement;
                    else if (rpos == "K")
                        pair.Left.K = replacement;
                    else if (rpos == "DST")
                        pair.Left.DST = replacement;
                    else
                    {
                        Console.WriteLine("Pos [{0}] is unknown", rpos);
                        continue;
                    }
                }

                int lp = pair.Left.TotalPts();
                int rp = pair.Right.TotalPts();
                if (lp > rp)
                {
                    winner = pair.Left;
                    loser = pair.Right;
                }
                else if (lp < rp)
                {
                    winner = pair.Right;
                    loser = pair.Left;
                }
                else  //tie, both teams gets both a win and a loss... moves winpct to 50%
                {
                    winner = pair.Right;
                    loser = pair.Left;
                    SaveWinners(loser, players);
                    SaveLosers(winner, players);
                }


                SaveWinners(winner, players);
                SaveLosers(loser, players);

                //if(i%10000==0)gamecenterdelay = false;
                if (UI.IsReady && i>gamecenterdelay) 
                {
                    gamecenterdelay += 9697;
                    ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        if (UI.IsReady)
                            GameCenter.Show(i, winner, loser);
                    }, null);
                }


                //if (i % 10000 == 0) leaderboarddelay = false;
                if (UI.IsReady && i>leaderboarddelay)
                {
                    leaderboarddelay += 20001;
                    ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        if (UI.IsReady)
                        {
                            LeaderBoard.Show((Dictionary<string, Proxy>)state);
                        }
                    }, new Dictionary<string, Proxy>(players));
                }

                if (UI.IsReady)
                    ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        if (UI.IsReady)
                        {
                            ProgressBar.Show(i, iterations, Console.WindowTop + 24);
                        }
                    }, null);

                if (threadBuffers != null && i>joindelay)
                {
                    /*ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        int position = (int)state;
                        while(!SaveMeter.Show(position,position.ToString(), false));
                    }, nextmerge);*/

                    //the indictor should be real-time
                    while (!SaveMeter.Show(nextmerge, nextmerge.ToString(), false, null)) ;

                    joindelay = i+10000;
                    var buffer = threadBuffers[nextmerge];
                    Interlocked.Add(ref i, buffer.OutputCounter);
                    buffer.SwapBufferAndAddTo(players);
                    
                    /*ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        int position = (int)state;
                        while(!SaveMeter.Show(position," " +position, false));
                    }, nextmerge);*/
                    while (!SaveMeter.Show(nextmerge, " " + nextmerge, false, null)) ;
                    
                    nextmerge++;
                    if (nextmerge >= threadcount)
                        nextmerge = 0;

                    /*foreach (var buffer in threadBuffers)
                    {
                        //i += buffer.OutputCounter;
                        Interlocked.Add(ref i, buffer.OutputCounter);
                        buffer.SwapBufferAndAddTo(players);
                    }*/
                }

                if (i > savedelay)
                {
                    savedelay += 10000000;
                    ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        while (!SaveMeter.Show(0, null, true, "@")) ;
                        try {
                            signal.Reset();
                            setData((Dictionary<string, Proxy>)state, GmWeek);
                        } finally {
                            signal.Set();
                        }
                        while (!SaveMeter.Show(0, null, true, " ")) ;
                    }, new Dictionary<string, Proxy>(players));
                    //players = new Dictionary<string, Proxy>();

                    //Console.SetWindowPosition(50,25);
                    //Console.Write("Saving...");
                }
            }
            if(threadBuffers!=null)
                foreach (var buffer in threadBuffers)
                    buffer.IsActive = false;

            UI.IsAbort = true;
            while (!UI.IsReady) ;
            UI.IsAbort = false;
            ProgressBar.Show(iterations, iterations, Console.WindowTop + 24);

            Console.SetCursorPosition(78,Console.WindowTop+Console.WindowHeight-1);
            Console.WriteLine();
            Console.WriteLine("Cleaning up...");
            /// wait for threadpool to 
            signal.WaitOne();
            setData(players, GmWeek);

            Console.WriteLine("Finished!");
        }

        static void showUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("TwoTeamMonteCarlo");
            Console.WriteLine();
            //                 12345678901234567890123456789012345678901234567890123456789012345678901234567890
            Console.WriteLine("TwoTeamMonteCarlo [-teams number]");
            Console.WriteLine("   ...Default 14. Specify number of teams in the league");
            Console.WriteLine("TwoTeamMonteCarlo [-season number]");
            Console.WriteLine("   ...Default is now (" + GetCurrentSeason() + ") season. Specify season");
            Console.WriteLine("      Season is the year of 1st game in Sept.  Used for getting stats from www.");
            Console.WriteLine("TwoTeamMonteCarlo [-pos CSV list of active positions|CSV list of reserve]");
            Console.WriteLine("   ...Default is QB,RB,RB,WR,WR,TE,RB WR TE,K,D|QB,*,RB,*,WR,*,TE,*,D");
            Console.WriteLine("      Specify the amount of player positions allowed on roster");
            Console.WriteLine("TwoTeamMonteCarlo [-file filename]");
            Console.WriteLine("   ...instead of specifying season, load stats from file");
            Console.WriteLine("      [PlayerID], [Player], [Pos], [Team], [Year], [Gm] --Req.Player Profile");
	        Console.WriteLine("      [PaComp], [PaAtt], [PaYd], [PaTD], [PaINT], --Optional passing stats");
	        Console.WriteLine("      [RuAtt], [RuYd], [RuTD], --optional running stats");
	        Console.WriteLine("      [ReTgt], [ReRec], [ReYd], [ReTD], --optional receiving stats");
	        Console.WriteLine("      [KiFGA], [KiFGP], [KiEPM], [KiEPA], --optional kicking stats");
            Console.WriteLine("      [DSack], [DFR], [DINT], [DTD], [DPA], [DPaYd], [DRuYd], [DSafety], ");
            Console.WriteLine("      [DKickTD] --optional defensive stats");

            Console.WriteLine("TwoTeamMonteCarlo [-rank:all]");
            Console.WriteLine("   ...Rank all players who have stats");
            Console.WriteLine("TwoTeamMonteCarlo [-rank:reg]");
            Console.WriteLine("   ...(Default ranking) Includes players who have stats in at least 1/2 season");
            Console.WriteLine("TwoTeamMonteCarlo [-rank:roster]");
            Console.WriteLine("   ...Ranks the players that should make rosters in your league");
            Console.WriteLine("      The size depends on # of players allowed on roster, # of teams in league");
            Console.WriteLine("TwoTeamMonteCarlo [-rank:starter]");
            Console.WriteLine("   ...Ranks just the players that should be starting");
            Console.WriteLine("      Only here, do the percentages, be used to calculated the winning% versus");
            Console.WriteLine("      Avg the winning % of starting players and that should be winning % of team");
            Console.WriteLine("      Get difference of each player versus another roster, +0.5 and you get odds");
            Console.WriteLine("      Put that on a graph versus actual point differential,"); 
            Console.WriteLine("      and you get the linear odds for 'points'.  And you notice implied odds with");
            Console.WriteLine("      points, assuming they aren't point shaving.");

            Console.WriteLine("TwoTeamMonteCarlo -includefromfile [filename of list of players]");
            Console.WriteLine("   ...Make sure these players are included");
            Console.WriteLine("      Primary purpose is to ensure that players that normally wouldnt be on a");
            Console.WriteLine("      roster is included");
            
            Console.WriteLine("TwoTeamMonteCarlo [-limit minutes]");
            Console.WriteLine("   ...Default: 90min");
            Console.WriteLine("      Limit the run time of the ranking, in minutes");

            Console.WriteLine("TwoTeamMonteCarlo [-threads number of threads]");
            Console.WriteLine("   ...Default: virtual cores/2");
            Console.WriteLine("      Limit the run time of the ranking, in minutes");

            Console.WriteLine("TwoTeamMonteCarlo [-output filename]");
            Console.WriteLine("   ...default is console output");
            Console.WriteLine("      When output to file, there is no GUI output");

            Console.WriteLine("TwoTeamMonteCarlo [-progress minutes]");
            Console.WriteLine("   ...default: none");
            Console.WriteLine("      When output to file, if this flag is set, it will write to file every specified ");
            Console.WriteLine("      number of minutes");
            Console.WriteLine();
            Console.WriteLine("This program is strictly for fun.  Trying to apply some statistical analysis to");
            Console.WriteLine("fantasy football stats.  It does not predict winners, bc it assumes that the past");
            Console.WriteLine("and present and future performance is the same.  And we all know, that isn't the case.");
            Console.WriteLine("It all started from a simple idea... how do I measure how 'lucky' a fantasy owner is?");
            Console.WriteLine("To know that, you need to know what his record should be.  Then you compare it to");
            Console.WriteLine("his actual record.  In the long run, people perform their average.  In small sample");
            Console.WriteLine("weird stuff throws the average off, or luck");

            // #teams, season, roster, file, include, 

            Console.WriteLine("                              at least 1/2 season");
            Console.WriteLine("TwoTeamMonteCarlo All      ...Includes all players");
            Console.WriteLine("TwoTeamMonteCarlo Roster   ...Run after All players, and calc win percentages");
            Console.WriteLine("                              for what should be roster players only");
            Console.WriteLine("TwoTeamMonteCarlo Starter  ...Run after Roster players, and calculates win");
            Console.WriteLine("                              percentages for what should be starters only");
            Console.WriteLine("TwoTeamMonteCarlo Starter -includefromfile [list of players]");
            Console.WriteLine("                           ...Run after Starter.  include list of players");
            Console.WriteLine("                              to see how they perform against best]");
            Console.WriteLine("Flag: -threads [number of threads] ...force number of threads");
            Console.WriteLine("Note: Assumes 14 team league");
            Console.WriteLine("      (team w 2QB, 4RB, 4WR, 2TE, 1Flex, 2K, 2DST for roster)");
            Console.WriteLine("      (starts 1QB, 2RB, 2WR, 1Flex of RB,WR,TE, 1TE, 1K, 1DST for simulation)");
        }

        /// <summary>
        /// Rolls over in Sept
        /// </summary>
        /// <returns></returns>
        static public int GetCurrentSeason()
        {
            var now  = DateTime.Now;
            var year = now.Year;
            var month = now.Month;
            if (month < 9)
                year--;
            return year;
        }


        static int forceThreads = -1;
        static int getThreadCount()
        {
            if (forceThreads != -1 && forceThreads>0)
                return forceThreads;

            int cores = RegReader.CoreCount();
            Console.WriteLine("{0} Cores detected", cores);

            return cores-1;
        }
        static List<ThreadInputOutput> spinupWorkers(DataTable playerUniverse, Dictionary<Pos, List<DataRow>> playerPosIndex)
        {
            int FLOOR = 1;
            int count = getThreadCount();
            Console.WriteLine("{0} Threads used", count);

            if (count <= FLOOR)
                return null;

            Console.WriteLine("Spinning up auxillary threads...");
            List<ThreadInputOutput> output = new List<ThreadInputOutput>();

            for (int i = FLOOR; i < count; i++)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(secondaryThread))
                {
                    IsBackground = true
                };

                ThreadInputOutput io = new ThreadInputOutput() { InputPlayerUniverse = playerUniverse, InputPlayerPosIndex = playerPosIndex };
                thread.Start(io);
                output.Add(io);
                Console.Write(i);
                Console.WriteLine("...");

            }

            return output;
        }

        class ThreadInputOutput
        {
            public bool IsActive = true;
            volatile public int OutputCounter = 0;
            volatile public Dictionary<string, Proxy> OutputBuffer = new Dictionary<string, Proxy>();
            public Dictionary<string, Proxy> TempBuffer = new Dictionary<string, Proxy>();
            volatile public Dictionary<string, Proxy> ConfirmBuffer;
            public DataTable InputPlayerUniverse;
            public Dictionary<Pos, List<DataRow>> InputPlayerPosIndex;

            /// <summary>
            /// Not thread safe.  Must run on main thread as "main" arg
            /// </summary>
            /// <param name="main"></param>
            public void SwapBufferAndAddTo(Dictionary<string, Proxy> main)
            {
                Dictionary<string, Proxy> frame = this.OutputBuffer;
                this.OutputBuffer = this.TempBuffer;
                this.TempBuffer = frame;
                this.OutputCounter = 0;

                while(this.OutputBuffer!=this.ConfirmBuffer); //spinwait until new .OutputBuffer has been written to? bc then it's done with the old buffer

                foreach (var p in frame)
                {
                    int wins = p.Value.Win;
                    int losses = p.Value.Loss;
                    if (wins != 0 && losses != 0)
                    {
                        if (!main.ContainsKey(p.Key))
                            main.Add(p.Key, new Proxy() { Win = wins, Loss = losses, Name = p.Value.Name });
                        else
                        {
                            main[p.Key].Win += wins;
                            main[p.Key].Loss += losses;
                        }
                        p.Value.Win = 0;
                        p.Value.Loss = 0;
                    }
                }
            }
        }
        

        static void secondaryThread(object state)
        {
            ThreadInputOutput io = (ThreadInputOutput)state;
            DataTable universe = io.InputPlayerUniverse;
            Dictionary<Pos, List<DataRow>> dividedByPos = io.InputPlayerPosIndex;
            Dictionary<string, Proxy> players = io.OutputBuffer;
            
            Duel pair = default(Duel);
            Team winner = default(Team);
            Team loser = default(Team);
            
            while (io.IsActive)
            {
                io.OutputCounter++;
                players = io.OutputBuffer;

                pair = RandomStat(dividedByPos, dividedByPos);
                int lp = pair.Left.TotalPts();
                int rp = pair.Right.TotalPts();
                if (lp > rp)
                {
                    winner = pair.Left;
                    loser = pair.Right;
                }
                else if (lp < rp)
                {
                    winner = pair.Right;
                    loser = pair.Left;
                }
                else  //tie, both teams gets both a win and a loss... moves winpct to 50%
                {
                    winner = pair.Right;
                    loser = pair.Left;
                    SaveWinners(loser, players);
                    SaveLosers(winner, players);
                }

                SaveWinners(winner, players);
                SaveLosers(loser, players);

                io.ConfirmBuffer = io.OutputBuffer;
            }

        }


        public enum Pos
        {
            QB,RB,WR,TE,F,K,DST
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

        static DataTable GetDataForOnly(string[] players) 
        {
            var all = getData("vwStatsPointsForCityIslandPlusOut");
            int len = all.Rows.Count;
            List<DataRow> tbd = new List<DataRow>();
            foreach(DataRow row in all.Rows)
                if(Array.IndexOf<string>(players, row["PlayerID"].ToString())<0)
                {
                    tbd.Add(row);
                }
            foreach (var item in tbd)
                all.Rows.Remove(item);
            return all;
        }

        static string GetConnectionString()
        {
            string s = ConfigurationManager.AppSettings["SqlServer"];
            if (s != null)
                return "Server=" + s + ";Database=FFToiletBowl;User Id=toilet;Password=toiletbowl;";
            
            var sqlregistry = RegReader.SqlServerInstance();
            if (sqlregistry != null)
                if(sqlregistry.Count>0)
                    return "Server=" + sqlregistry[sqlregistry.Count - 1] + ";Database=FFToiletBowl;User Id=toilet;Password=toiletbowl;";

            return "Server=.\\SQLSERVER2008;Database=FFToiletBowl;User Id=toilet;Password=toiletbowl;";
        }

        static Dictionary<Pos, List<DataRow>> IndexPos(DataTable dt)
        {
            Dictionary<Pos, List<DataRow>> posIndex = new Dictionary<Pos, List<DataRow>>();
            Pos parsed = default(Pos);
            int count = dt.Rows.Count;
            int status = 0;
            foreach (DataRow row in dt.Rows)
            {
                string pos = row["Pos"].ToString();
                if(Enum.TryParse<Pos>(pos, out parsed))
                    if(!posIndex.ContainsKey(parsed))
                        posIndex.Add(parsed, new List<DataRow>(){row});
                    else
                        posIndex[parsed].Add(row);
                else
                    throw new Exception("Unknown position received from database "+pos);
                int tmp = (int)row["Gm"];
                if (tmp > GmWeek) GmWeek = tmp;

                status++;
                UI.IsAbort = false;
                if (UI.IsReady)
                    ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        if (UI.IsReady)
                            ProgressBar.Show(status, count, Console.CursorTop);
                    }, null);
            }
            UI.IsAbort = true;
            while (!UI.IsReady);
            UI.IsAbort = false;
            ProgressBar.Show(count, count, Console.CursorTop);

            return posIndex;
        }

        public struct Team
        {
            public DataRow QB;
            public DataRow RB1;
            public DataRow RB2;
            public DataRow WR1;
            public DataRow WR2;
            public DataRow TE;
            public DataRow F;
            public DataRow K;
            public DataRow DST;
            public int TotalPts()
            {
                string FIELD_NAME="TotPt";
                return (int)QB[FIELD_NAME] + (int)RB1[FIELD_NAME] + (int)RB2[FIELD_NAME] + (int)WR1[FIELD_NAME] + (int)WR2[FIELD_NAME] + (int)TE[FIELD_NAME] + (int)F[FIELD_NAME] + (int)K[FIELD_NAME] + (int)DST[FIELD_NAME];
            }
        }
        public struct Duel {
            public Team Left;
            public Team Right;
        }

        public struct PosCount
        {
            public int QB;
            public int RB;
            public int WR;
            public int TE;
            public int K;
            public int DST;
        }
        static Dictionary<Pos, List<DataRow>> lastleft, lastright;
        static PosCount leftcount, rightcount;
        static public Duel RandomStat(Dictionary<Pos, List<DataRow>> left, Dictionary<Pos, List<DataRow>> right)
        {
            if (left != lastleft)
            {
                leftcount = new PosCount()
                {
                    QB = left[Pos.QB].Count-1,
                    RB = left[Pos.RB].Count-1,
                    WR = left[Pos.WR].Count-1,
                    TE = left[Pos.TE].Count-1,
                    K = left[Pos.K].Count-1,
                    DST = left[Pos.DST].Count-1,
                };
                lastleft = left;
            }
            if (right != lastright)
            {
                rightcount = new PosCount()
                {
                    QB = right[Pos.QB].Count-1,
                    RB = right[Pos.RB].Count-1,
                    WR = right[Pos.WR].Count-1,
                    TE = right[Pos.TE].Count-1,
                    K = right[Pos.K].Count-1,
                    DST = right[Pos.DST].Count-1,
                };
                lastright = right;
            }

            string FIELD = "PlayerID";
            Random gen = new Random();
            Team l = new Team()
            {
                QB = left[Pos.QB][gen.Next(leftcount.QB)],
                RB1 = left[Pos.RB][gen.Next(leftcount.RB)],
                RB2 = left[Pos.RB][gen.Next(leftcount.RB)],
                WR1 = left[Pos.WR][gen.Next(leftcount.WR)],
                WR2 = left[Pos.WR][gen.Next(leftcount.WR)],
                TE = left[Pos.TE][gen.Next(leftcount.TE)],
                F = (gen.Next(2)<2) ? ((gen.Next(1)==1) ? left[Pos.RB][gen.Next(leftcount.RB)] : left[Pos.WR][gen.Next(leftcount.WR)]) : left[Pos.TE][gen.Next(leftcount.TE)],
                K = left[Pos.K][gen.Next(leftcount.K)],
                DST = left[Pos.DST][gen.Next(leftcount.DST)],
            };
            while (l.RB1[FIELD] == l.RB2[FIELD] || l.WR1[FIELD] == l.WR2[FIELD] || l.F[FIELD] == l.RB1[FIELD] || l.F[FIELD] == l.RB2[FIELD] || l.F[FIELD] == l.WR1[FIELD] || l.F[FIELD] == l.WR2[FIELD])
            {
                l = new Team()
                {
                    QB = left[Pos.QB][gen.Next(leftcount.QB)],
                    RB1 = left[Pos.RB][gen.Next(leftcount.RB)],
                    RB2 = left[Pos.RB][gen.Next(leftcount.RB)],
                    WR1 = left[Pos.WR][gen.Next(leftcount.WR)],
                    WR2 = left[Pos.WR][gen.Next(leftcount.WR)],
                    TE = left[Pos.TE][gen.Next(leftcount.TE)],
                    F = (gen.Next(2)<2) ? ((gen.Next(1)==1) ? left[Pos.RB][gen.Next(leftcount.RB)] : left[Pos.WR][gen.Next(leftcount.WR)]) : left[Pos.TE][gen.Next(leftcount.TE)],
                    K = left[Pos.K][gen.Next(leftcount.K)],
                    DST = left[Pos.DST][gen.Next(leftcount.DST)],
                };
            }

            Team r = new Team()
            {
                QB = right[Pos.QB][gen.Next(rightcount.QB)],
                RB1 = right[Pos.RB][gen.Next(rightcount.RB)],
                RB2 = right[Pos.RB][gen.Next(rightcount.RB)],
                WR1 = right[Pos.WR][gen.Next(rightcount.WR)],
                WR2 = right[Pos.WR][gen.Next(rightcount.WR)],
                TE = right[Pos.TE][gen.Next(rightcount.TE)],
                F = (gen.Next(2)<2) ? ((gen.Next(1)==1) ? right[Pos.RB][gen.Next(rightcount.RB)] : right[Pos.WR][gen.Next(rightcount.WR)]) : right[Pos.TE][gen.Next(rightcount.TE)],
                K = right[Pos.K][gen.Next(rightcount.K)],
                DST = right[Pos.DST][gen.Next(rightcount.DST)],
            };
            while (l.QB[FIELD].ToString() == r.QB[FIELD].ToString() || l.RB1[FIELD].ToString() == r.RB1[FIELD].ToString() || l.RB2[FIELD].ToString() == r.RB2[FIELD].ToString() || l.WR1[FIELD].ToString() == r.WR1[FIELD].ToString() || l.WR2[FIELD].ToString() == r.WR2[FIELD].ToString() || l.TE[FIELD].ToString() == r.TE[FIELD].ToString() || l.F[FIELD].ToString() == r.F[FIELD].ToString() || l.K[FIELD].ToString() == r.K[FIELD].ToString() || l.DST[FIELD].ToString() == r.DST[FIELD].ToString()
                    || r.RB1[FIELD].ToString() == r.RB2[FIELD].ToString() || r.WR1[FIELD].ToString() == r.WR2[FIELD].ToString() || r.F[FIELD].ToString() == r.RB1[FIELD].ToString() || r.F[FIELD].ToString() == r.RB2[FIELD].ToString() || r.F[FIELD].ToString() == r.WR1[FIELD].ToString() || r.F[FIELD].ToString() == r.WR2[FIELD].ToString())
            {
                r = new Team()
                {
                    QB = right[Pos.QB][gen.Next(rightcount.QB)],
                    RB1 = right[Pos.RB][gen.Next(rightcount.RB)],
                    RB2 = right[Pos.RB][gen.Next(rightcount.RB)],
                    WR1 = right[Pos.WR][gen.Next(rightcount.WR)],
                    WR2 = right[Pos.WR][gen.Next(rightcount.WR)],
                    TE = right[Pos.TE][gen.Next(rightcount.TE)],
                    F = (gen.Next(2)<2) ? ((gen.Next(1)==1) ? right[Pos.RB][gen.Next(rightcount.RB)] : right[Pos.WR][gen.Next(rightcount.WR)]) : right[Pos.TE][gen.Next(rightcount.TE)],
                    K = right[Pos.K][gen.Next(rightcount.K)],
                    DST = right[Pos.DST][gen.Next(rightcount.DST)],
                };
            }

            return new Duel() { Left=l, Right=r };
        }

        static void SaveWinners(Team winner, Dictionary<string, Proxy> players)
        {
            string playerID = winner.QB["PlayerID"].ToString();
            string player = winner.QB["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Win = 1, Name = player });
            else
                players[playerID].Win++;

            playerID = winner.RB1["PlayerID"].ToString();
            player = winner.RB1["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Win = 1, Name = player });
            else
                players[playerID].Win++;

            playerID = winner.RB2["PlayerID"].ToString();
            player = winner.RB2["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Win = 1, Name = player });
            else
                players[playerID].Win++;

            playerID = winner.WR1["PlayerID"].ToString();
            player = winner.WR1["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Win = 1, Name = player });
            else
                players[playerID].Win++;

            playerID = winner.WR2["PlayerID"].ToString();
            player = winner.WR2["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Win = 1, Name = player });
            else
                players[playerID].Win++;

            playerID = winner.TE["PlayerID"].ToString();
            player = winner.TE["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Win = 1, Name = player });
            else
                players[playerID].Win++;

            playerID = winner.F["PlayerID"].ToString();
            player = winner.F["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Win = 1, Name = player });
            else
                players[playerID].Win++;

            playerID = winner.K["PlayerID"].ToString();
            player = winner.K["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Win = 1, Name = player });
            else
                players[playerID].Win++;

            playerID = winner.DST["PlayerID"].ToString();
            player = winner.DST["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Win = 1, Name = player });
            else
                players[playerID].Win++;
        }

        static void SaveLosers(Team loser, Dictionary<string, Proxy> players)
        {
            string playerID = loser.QB["PlayerID"].ToString();
            string player = loser.QB["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Loss = 1, Name = player });
            else
                players[playerID].Loss++;

            playerID = loser.RB1["PlayerID"].ToString();
            player = loser.RB1["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Loss = 1, Name = player });
            else
                players[playerID].Loss++;

            playerID = loser.RB2["PlayerID"].ToString();
            player = loser.RB2["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Loss = 1, Name = player });
            else
                players[playerID].Loss++;

            playerID = loser.WR1["PlayerID"].ToString();
            player = loser.WR1["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Loss = 1, Name = player });
            else
                players[playerID].Loss++;

            playerID = loser.WR2["PlayerID"].ToString();
            player = loser.WR2["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Loss = 1, Name = player });
            else
                players[playerID].Loss++;

            playerID = loser.TE["PlayerID"].ToString();
            player = loser.TE["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Loss = 1, Name = player });
            else
                players[playerID].Loss++;

            playerID = loser.F["PlayerID"].ToString();
            player = loser.F["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Loss = 1, Name = player });
            else
                players[playerID].Loss++;

            playerID = loser.K["PlayerID"].ToString();
            player = loser.K["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Loss = 1, Name = player });
            else
                players[playerID].Loss++;

            playerID = loser.DST["PlayerID"].ToString();
            player = loser.DST["Player"].ToString();
            if (!players.ContainsKey(playerID))
                players.Add(playerID, new Proxy() { Loss = 1, Name = player });
            else
                players[playerID].Loss++;
        }
 


        static void setData(Dictionary<string, Proxy> data, int GameWeek)
        {
            // Create the command.
            string connectionString = GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // string sql = "[FFToiletBowl].[dbo].[Stats]";
                // command.CommandText = sql;
                // command.CommandType = CommandType.TableDirect;
                
                // Create a new row.
                using (FFToiletBowlDataSetTableAdapters.GameDayMonteCarloTableAdapter da = new FFToiletBowlDataSetTableAdapters.GameDayMonteCarloTableAdapter())
                {
                    da.Connection = connection;
                    da.ClearGameDayMonteCarlo();
                    foreach (var row in data)
                    {
                        da.InsertQuery(row.Key,
                            row.Value.Win,
                            row.Value.Loss,
                            GameWeek);
                    }
                }
            }
        }

    }

    public class Proxy
    {
        public string Name;
        public int Win;
        public int Loss;
        public double WinPct()
        {
            return (double)Win / ((double)Win + (double)Loss);
        }
        public int Count()
        {
            return Win + Loss;
        }
        public double SumOffset;
    }
    public class UI
    {
        static public object Surrogate = new object();
        static public bool IsAbort = false;
        static public bool IsReady = true;
    }
    public class ProgressBar
    {
        static public void Show(int value, int max, int top)
        {
            if (UI.IsAbort) return;

            lock (UI.Surrogate)
                if (UI.IsReady)
                    UI.IsReady = false;
                else
                    return;

            int WIDTH = 50;
            Console.CursorTop = top;
            Console.CursorLeft = 0;
            Console.Write("[");
            Console.CursorLeft = WIDTH-1;
            Console.Write("]");
            int progress = Convert.ToInt32(((float)value * ((float)WIDTH-2.0f)) / (float)max);
            string bar = new String('*', progress);
            Console.CursorLeft = 1;
            Console.Write(bar);

            Console.CursorLeft = WIDTH+1;
            if (!UI.IsAbort)
                Console.Write("{0:#,###0}/{1:#,###0}", value, max);

            lock (UI.Surrogate)
                UI.IsReady = true;
        }
    }

    public class GameCenter
    {
        static DateTime startTime = DateTime.Now;
        static long lastIteration = 0;
        static long lastTick = 0;
        static public string Title { get; set; }
        static public void Show(int iteration, Program.Team winner, Program.Team loser)
        {
            lock (UI.Surrogate)
                if (UI.IsReady)
                    UI.IsReady = false;
                else
                    return;

            long slice = (Environment.TickCount - lastTick);
            if (slice != 0)
            {
                long rate = (iteration - lastIteration) / slice;
                lastTick = lastTick+slice;
                lastIteration = iteration;

                TimeSpan duration = DateTime.Now.Subtract(startTime);
                int speed = Convert.ToInt32(iteration / duration.TotalMilliseconds);

                Console.SetCursorPosition(0, Console.WindowTop);
                Console.Write("{0}, Sample# {1}... {2}/ms, avg {3}/ms   ", Title, iteration, rate, speed);
            }

            int top = Console.WindowTop+1;
            Console.SetCursorPosition(0, top+0);
            Console.Write("QB  {0,30} {1,-2}", winner.QB["Player"], winner.QB["TotPt"]);
            Console.SetCursorPosition(0, top + 1);
            Console.Write("RB  {0,30} {1,-2}", winner.RB1["Player"].ToString(), winner.RB1["TotPt"]);
            Console.SetCursorPosition(0, top + 2);
            Console.Write("RB  {0,30} {1,-2}", winner.RB2["Player"].ToString(), winner.RB2["TotPt"]);
            Console.SetCursorPosition(0, top + 3);
            Console.Write("WR  {0,30} {1,-2}", winner.WR1["Player"].ToString(), winner.WR1["TotPt"]);
            Console.SetCursorPosition(0, top + 4);
            Console.Write("WR  {0,30} {1,-2}", winner.WR2["Player"].ToString(), winner.WR2["TotPt"]);
            Console.SetCursorPosition(0, top + 5);
            Console.Write("F   {0,30} {1,-2}", winner.F["Player"].ToString(), winner.F["TotPt"]);
            Console.SetCursorPosition(0, top + 6);
            Console.Write("TE  {0,30} {1,-2}", winner.TE["Player"].ToString(), winner.TE["TotPt"]);
            Console.SetCursorPosition(0, top + 7);
            Console.Write("K   {0,30} {1,-2}", winner.K["Player"].ToString(), winner.K["TotPt"]);
            Console.SetCursorPosition(0, top + 8);
            Console.Write("DST {0,30} {1,-2}", winner.DST["Player"].ToString(), winner.DST["TotPt"]);
            Console.SetCursorPosition(0, top + 9);
            Console.Write("Score {0}    ", winner.TotalPts());

            Console.SetCursorPosition(41, top + 0);
            Console.Write("QB  {0,30} {1,-2}", loser.QB["Player"].ToString(), loser.QB["TotPt"]);
            Console.SetCursorPosition(41, top + 1);
            Console.Write("RB  {0,30} {1,-2}", loser.RB1["Player"].ToString(), loser.RB1["TotPt"]);
            Console.SetCursorPosition(41, top + 2);
            Console.Write("RB  {0,30} {1,-2}", loser.RB2["Player"].ToString(), loser.RB2["TotPt"]);
            Console.SetCursorPosition(41, top + 3);
            Console.Write("WR  {0,30} {1,-2}", loser.WR1["Player"].ToString(), loser.WR1["TotPt"]);
            Console.SetCursorPosition(41, top + 4);
            Console.Write("WR  {0,30} {1,-2}", loser.WR2["Player"].ToString(), loser.WR2["TotPt"]);
            Console.SetCursorPosition(41, top + 5);
            Console.Write("F   {0,30} {1,-2}", loser.F["Player"].ToString(), loser.F["TotPt"]);
            Console.SetCursorPosition(41, top + 6);
            Console.Write("TE  {0,30} {1,-2}", loser.TE["Player"].ToString(), loser.TE["TotPt"]);
            Console.SetCursorPosition(41, top + 7);
            Console.Write("K   {0,30} {1,-2}", loser.K["Player"].ToString(), loser.K["TotPt"]);
            Console.SetCursorPosition(41, top + 8);
            Console.Write("DST {0,30} {1,-2}", loser.DST["Player"].ToString(), loser.DST["TotPt"]);
            Console.SetCursorPosition(41, top + 9);
            Console.Write("Score {0}      ", loser.TotalPts());


            lock (UI.Surrogate)
                UI.IsReady = true;
        }
    }

    public class LeaderBoard
    {
        static public void Show(Dictionary<string, Proxy> players)
        {
            lock (UI.Surrogate)
                if (UI.IsReady)
                    UI.IsReady = false;
                else
                    return;

            var ranked= players.OrderByDescending<KeyValuePair<string, Proxy>, double>(s => s.Value.WinPct()).Take<KeyValuePair<string, Proxy>>(22);

            int top = Console.WindowTop + 12;
            int num = 0;
            int col = 0;
            foreach (var item in ranked)
            {
                Console.SetCursorPosition(col, top + num);
                Console.Write("{0,-20} {1,-5:0.000} {2,5}   ", item.Value.Name, item.Value.WinPct(), item.Value.Count());
                num++;
                if (num == 11)
                {
                    num = 0;
                    col = 41;
                }
            }
            

            lock (UI.Surrogate)
                UI.IsReady = true;
        }
    }

    public class SaveMeter
    {
        static public bool Show(int join, string ch, bool issaveposition, string saveind)
        {
            lock (UI.Surrogate)
                if (UI.IsReady)
                    UI.IsReady = false;
                else
                    return false;
            
            if (ch != null)
            {
                var position = join;
                Console.SetCursorPosition(1 + position * 2, Console.WindowTop + 24);
                Console.Write(ch);
            }
            if (issaveposition)
            {
                Console.SetCursorPosition(48, Console.WindowTop + 24);
                Console.Write(saveind);
            }

            lock (UI.Surrogate)
                UI.IsReady = true;

            return true;
        }
    }
}
