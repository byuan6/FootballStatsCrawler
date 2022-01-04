using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;


namespace FFToiletBowl
{
    public class ScheduleNFL
    {

        static public bool IsScheduleFilled(int yr)
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
                using (var da = new FFToiletBowlDataSetTableAdapters.ScheduleTableAdapter())
                {
                    da.Connection = connection;
                    var result = da.GetScheduleCount(yr);
                    int count = (int)result;
                    //#games = 32teams * 17weeks - byes
                    //2001 had 31 teams, 2002 had 32 teams
                    //2020 had 17 weeks, 2021 had 18 weeks
                    var expected = yr <= 2001 ? 17 * 31 : yr <= 2021 ? 17 * 32 : 18 * 32;
                    return (count == expected);
                }
            }
        }

        static public void SetSchedule(List<TeamSchedule> data)
        {
            HashSet<int> clearedyear = new HashSet<int>();

            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // string sql = "[FFToiletBowl].[dbo].[Stats]";
                // command.CommandText = sql;
                // command.CommandType = CommandType.TableDirect;

                // Create a new row.
                using (var da = new FFToiletBowlDataSetTableAdapters.ScheduleTableAdapter())
                {
                    da.Connection = connection;
                    foreach (var row in data)
                    {
                        if (!clearedyear.Contains(row.Year)) //we are assuming this only is used to insert an entire year of schedules at once
                        {
                            da.DeleteSeason(row.Year);
                            clearedyear.Add(row.Year);
                        }

                        //https://en.wikipedia.org/wiki/Relocation_of_professional_sports_teams
                        //1995: Los Angeles Raiders moved back to Oakland after 13 seasons.
                        //1995: Los Angeles Rams moved to St. Louis.
                        //1996: Cleveland Browns players and coaching staff moved to Baltimore and became the Ravens. The move was one of the most controversial in major professional sports history. In response to a fan revolt and legal threats, the NFL awarded a new franchise to Cleveland in 1999, which for historical purposes is considered a continuation of the original Browns franchise.
                        //1997: Houston Oilers moved to Memphis and became the Tennessee Oilers. The team originally planned to play both 1997 and 1998 in Liberty Bowl Memorial Stadium in Memphis before moving to their intended destination of Nashville. However, due to poor attendance, the team moved to Nashville in 1998, playing in Vanderbilt University's stadium. The team was renamed as the Titans in 1999, when their new stadium was opened. The NFL granted Houston a new expansion franchise in 2002.
                        //2016: St. Louis Rams moved back to Los Angeles after 21 seasons in St. Louis. The team is scheduled to move to a new stadium in nearby Inglewood in 2020.
                        //2017: San Diego Chargers returned to their original home of Los Angeles after 56 seasons in San Diego. The team is playing in the suburb of Carson before joining the Rams at their new stadium in 2020.
                        //2020: Oakland Raiders were approved to move to a new stadium in the Las Vegas area in 2020.[7] The team played in Oakland for the 2018 season and, due to being thwarted in its plans to play in San Francisco by their regional rivals the 49ers, were forced to play in Oakland in 2019 as well before completing the move to Las Vegas in 2020.[8][9]
                        if(row.Year>=1995 && row.Year<2016)
                        {
                            if(row.Team=="LAR")
                                row.Team = "STL";
                            else if (row.Versus == "LAR")
                                row.Versus = "STL";
                        }
                        if (row.Year < 2017)
                        {
                            if (row.Team == "LAC")
                                row.Team = "SD";
                            else if (row.Versus == "LAC")
                                row.Versus = "SD";
                        }
                        if (row.Year < 2020)
                        {
                            if (row.Team == "LV")
                                row.Team = "OAK";
                            else if (row.Versus == "LV")
                                row.Versus = "OAK";
                        }

                        da.InsertQuery(row.Year,
                            row.Wk,
                            row.Team,
                            row.Versus,
                            row.Away, null, null, null);
                    }
                }
            }
        }



        static public IEnumerable<TvSchedule> GetScheduleGrid()
        {
            DateTime? NULL_DATE = null;
            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                // Create a new row.
                using (var da = new FFToiletBowlDataSetTableAdapters.ScheduleTableAdapter())
                {
                    da.Connection = connection;
                    var result = da.GetData();
                    
                    //var a= (FFToiletBowlDataSet.ScheduleRow)result.Rows[0];
                    foreach (FFToiletBowlDataSet.ScheduleRow item in result.Rows)
                        yield return new TvSchedule() { Year = item.Year, Wk = item.Wk, Team = item.Team, Versus = item.Versus, Away = item.Away, LoadURL = item.IsLoadURLNull() ? null : item.LoadURL, LoadResult = item.IsLoadResultNull() ? null : item.LoadResult, ScheduleDate = item.IsLoadDateNull() ? NULL_DATE : item.LoadDate, PlayByPlayURL = item.IsPlayByPlayURLNull() ? null : item.PlayByPlayURL };
                }
            }
        }

        /// <summary>
        /// just updates the schedule record with 
        ///   LoadURL(url of game, where gameid is derived, but not where schedule is loaded), 
        ///   ScheduleDate(date of the game), 
        ///   LoadResult(null unless played, then score), 
        ///   PlayByPlayURL(null unless playby plays are loaded, then this is foreign key and url of source of play-by plays)
        ///   
        /// the rest are the composite priamry keys (year, week, team, versus, away flag)
        /// </summary>
        /// <param name="data"></param>
        static public void UpdateScheduleWithTV(List<TvSchedule> data)
        {
            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            for(int i=0;i<10;i++)
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        // string sql = "[FFToiletBowl].[dbo].[Stats]";
                        // command.CommandText = sql;
                        // command.CommandType = CommandType.TableDirect;

                        // Create a new row.
                        using (var da = new FFToiletBowlDataSetTableAdapters.ScheduleTableAdapter())
                        {
                            da.Connection = connection;
                            foreach (var row in data)
                            {
                                da.UpdateQuery(row.LoadURL, row.ScheduleDate, row.LoadResult, row.PlayByPlayURL,
                                    row.Year,
                                    row.Wk,
                                    row.Team,
                                    row.Versus,
                                    row.Away);
                            }
                        }
                    }
                    break;
                }
                catch (SqlException ex)
                {
                    //for some reason, this deadlocks a lot.  So wait a second and try again, up to 10 times
                    if(ex.Message.Contains("was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction"))
                        System.Threading.Thread.Sleep((new Random(Environment.TickCount)).Next(1000)+500);
                    else
                        throw;
                }
        }
        /// <summary>
        /// Will clear the play-by-play for the parent game in question, before re-inserting all the plays
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="plays"></param>
        static public void SavePlayByPlay(TvSchedule parent, IEnumerable<PlayByPlay> plays)
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
                using (var da = new FFToiletBowlDataSetTableAdapters.PlayByPlayTableAdapter())
                {
                    da.Connection = connection;
                    var url = parent.PlayByPlayURL;
                    //Console.WriteLine("SavePlayByPlay() parent.PlayByPlayURL {0} ",url);
                    da.DeleteQueryByURL(url);
                    foreach (var row in plays)
                    {
                        if (string.IsNullOrWhiteSpace(row.Possession)) row.Possession = "-";
                        da.InsertQuery(url, row.Possession, row.DownDistance == null ? string.Empty : row.DownDistance, row.Play, row.Seq);
                    }
                }
            }
        }
        static object __MUTUALLY_EXCLUSIVE_OR_BUST = new object();

        /// <summary>
        /// Left joins PlayByPlay and ParsedPlayByPlay, so missing records are returned
        /// </summary>
        /// <returns></returns>
        static public IEnumerable<PlayByPlay> GetUnparsedPlayByPlay()
        {
            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create a new row.
                using (var da = new FFToiletBowlDataSetTableAdapters.PlayByPlayTableAdapter())
                {
                    da.Connection = connection;
                    //var result = da.GetData();
                    var result = da.GetDataUnparsed();

                    var teams = new Dictionary<string, Tuple<string,string>>();
                    //var a = (FFToiletBowlDataSet.PlayByPlayRow)result.Rows[0];
                    foreach (FFToiletBowlDataSet.PlayByPlayRow item in result.Rows)
                    {
                        if (!teams.ContainsKey(item.URL))
                            using (var da2 = new FFToiletBowlDataSetTableAdapters.ScheduleTableAdapter())
                            {
                                da2.Connection = connection;
                                var game = da2.GetDataByPlayByPlayURL(item.URL);
                                if (game.Rows.Count != 0)
                                {
                                    var row = (FFToiletBowlDataSet.ScheduleRow)game.Rows[0];
                                    teams.Add(item.URL, new Tuple<string, string>(row.Team, row.Versus));
                                }
                                else
                                    throw new ApplicationException("Data is inconsistent.  Unable to find original schedule for :" + item.URL);
                            }
                        yield return new PlayByPlay() { Seq = item.Seq, URL = item.URL, DownDistance = item.DownDistance, Play = item.Play, Team1 = teams[item.URL].Item1, Team2 = teams[item.URL].Item2, Possession = item.Possession };
                    }
                }
            }
        }

        /// <summary>
        /// Not an update.  It inserts into ParsedPlayByPlay bc it's faster to insert with cross-referencing key, and select later with GetUnparsedPlayByPlay()
        /// than to look for each record (with no index on PLayByPlay) and update.
        /// 
        /// Will not wipe out the previous ParsedPlayByPlay records, b/c it is assumed that calls here are done from result from GetUnparsedPlayByPlay().
        /// If PlayByPlay was reloaded, and it's same url, it is assuming the data is immutable (and the wipe out was to reload data unintentionally deleted)
        /// </summary>
        /// <param name="list"></param>
        static public void UpdatePlayByPlay(IEnumerable<PlayByPlay> list)
        {
            // Create the command.
            string connectionString = ConnectionString.GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create a new row.
                using (var da = new FFToiletBowlDataSetTableAdapters.ParsedPlayByPlayTableAdapter())
                {
                    da.Connection = connection;
                    foreach (var item in list)
                        da.InsertQuery(item.URL, item.Possession, item.Seq
                            , item.Down, item.FieldPosition.Side, item.FieldPosition.Distance, (int)item.Time.TimeLeft.TotalSeconds, item.Time.Qtr
                            , item.Team1, item.Team2, item.IsGameMilestone, item.IsTimeout, item.Timeout
                            , item.IsKick, item.KickPlayer, item.KickYards.NullYards, item.KickStart.Side, item.KickStart.NullableDistance, item.KickLand.Side, item.KickLand.NullableDistance
                            , item.KickReturnPlayer, item.IsKickTouchback, item.KickAdvanceStop.Side, item.KickAdvanceStop.NullableDistance, item.KickReturn.NullYards, item.IsOnsideKick
                            , item.IsPunt, item.PuntPlayer, item.PuntYardage.NullYards, item.PuntLanded.Side, item.PuntLanded.NullableDistance, item.PuntSnapper, item.IsPuntFairCatch, item.IsPuntMuffed
                            , item.PuntReceiver, item.IsPuntTouchback, item.PuntingTeamPlayerDowned, item.PuntReturningPlayer, item.PuntStopped.Side
                            , item.PuntStopped.NullableDistance, item.PuntReturnYardage.NullYards, item.IsPuntOutOfBounds
                            , item.IsFieldGoalAttempt, item.IsFieldGoalGood, item.FieldGoalYardage.NullYards, item.FieldGoalPlayer, item.FieldGoalMissBy, item.FieldGoalCenter, item.FieldGoalHolder
                            , item.Formation, item.PassPlayer, item.IsIncomplete, item.IsPass, item.IsSacked, item.SackedAt.Side, item.SackedAt.NullableDistance
                            , item.PassDirection, item.PassDepth, item.PassCaught.NullableDistance, item.PassCaught.Side, item.PassYAC.Yards, item.PassYardage.Yards
                            , item.PassStop.Side, item.PassStop.NullableDistance, item.ReceivePlayer
                            , item.IsRun, item.IsScramble, item.RunPlayer, item.RunDirection, item.RunFormation, item.RunStop.NullableDistance, item.RunStop.Side, item.RunYardage.NullYards
                            , item.IsOb, item.Ob
                            , item.TackledByPlayer, item.IsFumble, item.IsFumbleLost, item.IsIntercepted
                            , item.IsTouchdown, item.Conversion, item.TdYardPassFrom, item.IsTouchdownStandardPlayFormat
                            , item.IsPenaltyCalled, item.IsPenaltyAccepted, item.IsPenaltyNoPlay
                            , item.PenaltyPlayer, item.PenaltyRule, item.PenaltyYardage.NullYards, item.PenaltyEnforcedAt.NullableDistance, item.PenaltyEnforcedAt.Side
                            , item.ValidatedAgainst, item.IsValidated);
                }
            }
        }

    }


    public class TeamSchedule
    {
        public int Year;
        public int Wk;
        public string Team;
        public string Versus;
        public bool Away;
    }

    public class TvSchedule : TeamSchedule
    {
        public string LoadURL;
        public DateTime? ScheduleDate;
        public string LoadResult;
        public string PlayByPlayURL;
    }

    public struct YardMark
    {
        public string Side;
        public int Distance;
        public string Preposition;
        public bool IsEmpty { get { return this.Side == null && this.Distance == 0; } }
        public bool IsEndZone { get { return this.Side == "END"; } }
        public int? NullableDistance
        {
            get
            {
                int? result=null;
                if (this.IsEmpty)
                    return result;
                else
                    return this.Distance;
            }
        }
        public override string  ToString()
        {
 	         if(this.Distance==50)
                 return "50";
             else if(this.IsEndZone)
                 return "end zone";
             else 
                 return string.Format("{0} {1}", this.Side, this.Distance);
        }

        static public YardMark ParseYardmark(int at, string[] tokens)
        {
            int yd = 0;
            if (tokens[at] == "50" || NumberStart(tokens[at]) == "50")
                return new YardMark() { Side = null, Distance = 50, Preposition="to" };
            else if (at + 1 < tokens.Length && int.TryParse(NumberStart(tokens[at + 1]), out yd))
                return new YardMark() { Side = tokens[at], Distance = yd, Preposition = "to" };
            else if(tokens[at] == "end" && tokens[at+1].StartsWith("zone"))
                return new YardMark() { Side = "END", Distance = 0, Preposition = "to" };
            else
                throw new ApplicationException("Unable to parse " + tokens[at] + tokens[at + 1]);
        }
        static public string NumberStart(IEnumerable<char> charlist)
        {
            int counter = 0;
            int counter2 = 0;
            if (charlist is string && charlist.All(s => (counter++==0 && s=='-') || (s >= '0' && s <= '9')))
            {
                return (string)charlist;
            }
            else
                return new string(charlist.TakeWhile(s => (counter2++ == 0 && s == '-') || (s >= '0' && s <= '9')).ToArray());
        }
    }
    public struct GameTime
    {
        public TimeSpan TimeLeft;
        public int Qtr; //1,2,3,4,5
    }
    public class PlayByPlay
    {
        public string Team1;
        public string Team2;

        public string URL;
        public string Possession; //Team
        public string DownDistance;
        public string Play;

        public int Seq;
        public int Down;
        public int Distance;
        public YardMark FieldPosition;
        public GameTime Time;
        public bool IsGameMilestone;
        public bool IsTimeout;
        public int Timeout;

        public bool IsKick;
        public string KickPlayer;
        public Yardage KickYards;
        public YardMark KickStart;
        public YardMark KickLand;
        public string KickReturnPlayer;
        public bool IsKickTouchback;
        public YardMark KickAdvanceStop;
        public Yardage KickReturn;
        public bool IsOnsideKick;

        public bool IsPunt;
        public string PuntPlayer;
        public Yardage PuntYardage;
        public YardMark PuntLanded;
        public string PuntSnapper;
        public bool IsPuntFairCatch;
        public bool IsPuntMuffed;
        public string PuntReceiver;
        public bool IsPuntTouchback;
        public string PuntingTeamPlayerDowned;
        public string PuntReturningPlayer;
        public YardMark PuntStopped;
        public Yardage PuntReturnYardage;
        public bool IsPuntOutOfBounds;

        public bool IsFieldGoalAttempt;
        public bool IsFieldGoalGood;
        public Yardage FieldGoalYardage;
        public string FieldGoalPlayer;
        public string FieldGoalMissBy;
        public string FieldGoalCenter;
        public string FieldGoalHolder;

        public string Formation;
        public string PassPlayer;
        public bool IsIncomplete;
        public bool IsPass;
        public bool IsSacked;
        public YardMark SackedAt;
        public string PassDirection;
        public string PassDepth;
        public YardMark PassCaught;
        public Yardage PassYAC;
        public Yardage PassYardage;
        public YardMark PassStop;
        

        public bool IsRun;
        public bool IsScramble;
        public string ReceivePlayer;
        public string RunPlayer;
        public string RunDirection;
        public string RunFormation;
        public YardMark RunStop;
        public Yardage RunYardage;

        //public bool IsPushedOb;
        //public bool IsRanOb;
        public bool IsOb;
        public string Ob;
        public string TackledByPlayer;

        public bool IsFumble;
        public bool IsFumbleLost;
        public bool IsIntercepted;
        public bool IsTurnover 
        {
            get
            {
                return this.IsTurnover || this.IsIntercepted;
            }
        }

        public bool IsTouchdown;
        public string Conversion;
        public bool TdYardPassFrom;
        public bool IsTouchdownStandardPlayFormat;

        public bool IsPenaltyCalled;
        public bool IsPenaltyAccepted;
        public bool IsPenaltyNoPlay;
        public string PenaltyPlayer;
        public string PenaltyRule;
        public Yardage PenaltyYardage;
        public YardMark PenaltyEnforcedAt;

        public string ReportedEligible;

        public string Reconciled;

        public string ValidatedAgainst;
        public bool IsValidated;

        public readonly string EXTRACT_ERR_DOWN_DISTANCE = "Not avail";
        public readonly string EXTRACT_ERR_PLAY = "Bad HTML";

        public void Parse()
        {
            //1st & 10 at WSH 23
            if (!string.IsNullOrWhiteSpace(this.DownDistance) && this.DownDistance != EXTRACT_ERR_DOWN_DISTANCE)
            {
                var currentdown = this.DownDistance.Split(' ');
                if (currentdown.Length == 6)
                {
                    this.Down = (int)(currentdown[0][0] - '0') + 1;
                    this.FieldPosition = YardMark.ParseYardmark(4, currentdown);

                    this.Distance = currentdown[2] != "Goal" ? int.Parse(currentdown[2]) : 0;
                }
            }


            var gameclock = new ParenToken();
            gameclock.Match += delegate(object sender, MatchEventArgs e)
            {
                //(10:13 - 1st)
                var casted = (ParenToken)sender;
                var time = casted.Elements[0].Substring(1);
                var remain = TimeSpan.ParseExact(time, "m\\:ss", System.Globalization.CultureInfo.InvariantCulture);

                var qtr = casted.Elements[2];
                var num = qtr[0] - '0';
                if (qtr == "OT)")
                    num = 5;
                if (num > 5)
                    throw new ApplicationException("Unable to parse quarter from : " + qtr);
                this.Time = new GameTime() { Qtr = num, TimeLeft = remain };
            };
            var expectgameclock = new ParseState() { Identifier = gameclock };
            var endqtr = new PrepositionToken() { Preposition="END", PrepositionalObjects=new string[1], IsExpectedRegEx=true, ExpectedObjects=new string[1] { "QUARTER|GAME" } };
            endqtr.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                //add a noplay tag
                this.IsGameMilestone = true;
            };
            var expectendqtr = new ParseState() { Identifier = endqtr };
            var twomin = new PrepositionToken() { Preposition = "Two-Minute", PrepositionalObjects = new string[1], ExpectedObjects = new string[1] { "Warning" } };
            twomin.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                //add a noplay tag
                this.IsGameMilestone = true;
            };
            var expecttwomin = new ParseState() { Identifier = twomin };
            expectgameclock.Transitions.Add(expecttwomin);
            var timeout = new PrepositionToken() { Preposition="Timeout ", PrepositionalObjects=new string[5], IsExpectedRegEx=true, ExpectedObjects=new string[] { "#[1-3]", "by", null, "at", null } };
            timeout.Match += delegate(object sender, MatchEventArgs e)
            {
                //(0:43 - 2nd) Timeout #2 by WAS at 00:43.
                var casted = (PrepositionToken)sender;
                this.IsTimeout = true;
                this.Timeout = casted.PrepositionalObjects[0]=="#1" ? 1 : casted.PrepositionalObjects[0]=="#2" ? 2 : casted.PrepositionalObjects[0]=="#3" ? 3 : 0;
            };
            var expecttimeout = new ParseState() { Identifier = timeout };
            expectgameclock.Transitions.Add(expecttimeout);

            ParseState bof = new ParseState();
            bof.Transitions.Add(expectgameclock);
            bof.Transitions.Add(expectendqtr);
            

            var formation = new ParenToken();
            formation.Match += delegate(object sender, MatchEventArgs e)
            {
                this.Formation = string.Join(" ", ((ParenToken)sender).Elements);
            };
            var expectformation = new ParseState() { Identifier = formation };

            var passplay = new SubjectVerbToken() { VerbPatterns=new string[] { "pass" } };
            passplay.Match += delegate(object sender, MatchEventArgs e)
            {
                this.IsPass = true;
                this.PassPlayer = string.Join(" ", ((SubjectVerbToken)sender).Subject);
            };
            var expectpassplay = new ParseState() { Identifier = passplay };

            var passincompetion = new AdverbToken() { AdverbPatterns = new string[] { "incomplete" } };
            passincompetion.Match += delegate(object sender, MatchEventArgs e)
            {
                this.IsIncomplete = true;
            };
            var expectpassincompetion = new ParseState() { Identifier = passincompetion };
            expectpassplay.Transitions.Add(expectpassincompetion);


            var runplay = new SubjectVerbToken() { VerbPatterns = new string[] { "up", "right", "left", "kneels" } };
            runplay.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (SubjectVerbToken)sender;
                this.RunDirection = casted.Verb;
                this.RunPlayer = string.Join(" ",casted.Subject);
                this.IsRun = true;
            };
            var expectrunplay = new ParseState() { Identifier = runplay };

            var scramblesplay = new SubjectVerbToken() { VerbPatterns = new string[] { "scrambles" } };
            scramblesplay.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (SubjectVerbToken)sender;
                this.RunDirection = casted.Verb;
                this.RunPlayer = string.Join(" ", casted.Subject);
                this.IsRun = true;
            };
            var expectscramblesplay = new ParseState() { Identifier = scramblesplay };
            bof.Transitions.Add(expectscramblesplay);



            var scrambledirection = new AdverbToken() { AdverbPatterns = new string[] { "up", "right", "left" } };
            scrambledirection.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (AdverbToken)sender;
                this.RunDirection = casted.Adverb;
            };
            var expectscrambledirection = new ParseState() { Identifier = scrambledirection };
            expectscramblesplay.Transitions.Add(expectscrambledirection);


            var sackplay = new SubjectVerbToken() { VerbPatterns = new string[] { "sacked" } };
            sackplay.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (SubjectVerbToken)sender;
                this.IsSacked=true;
                this.IsPass = true;
                this.PassPlayer = string.Join(" ", casted.Subject);
            };
            var expectsackplay = new ParseState() { Identifier = sackplay };

            expectgameclock.Transitions.Add(expectformation);
            expectgameclock.Transitions.Add(expectpassplay);
            expectgameclock.Transitions.Add(expectrunplay);
            expectgameclock.Transitions.Add(expectsackplay);

            var sackedob = new AdverbToken() { AdverbPatterns = new string[] { "ob" } };
            sackedob.Match += delegate(object sender, MatchEventArgs e)
            {
                //"(6:13 - 3rd)  (Shotgun) A.Smith sacked ob at WAS 46 for -1 yards (R.Nkemdiche)."
                this.IsOb = true;
                this.Ob = "";
            };
            var expectsackob = new ParseState() { Identifier = sackedob };
            expectsackplay.Transitions.Add(expectsackob);


            var passdirection = new AdverbToken() { AdverbPatterns = new string[] { "left", "right", "middle", "left.", "right.", "middle." } };
            passdirection.Match += delegate(object sender, MatchEventArgs e)
            {
                this.PassDirection = ((AdverbToken)sender).Adverb;
            };
            var expectpassdirection = new ParseState() { Identifier = passdirection };
            var passdepth = new AdverbToken() { AdverbPatterns = new string[] { "deep", "short" } };
            passdepth.Match += delegate(object sender, MatchEventArgs e)
            {
                this.PassDepth = ((AdverbToken)sender).Adverb;
            };
            var expectpassdepth = new ParseState() { Identifier = passdepth };
            expectpassplay.Transitions.Add(expectpassdepth);
            expectpassplay.Transitions.Add(expectpassdirection);
            expectpassdepth.Transitions.Add(expectpassdirection);
            expectpassdirection.Transitions.Add(expectpassdepth);

            expectpassincompetion.Transitions.Add(expectpassdepth);
            expectpassincompetion.Transitions.Add(expectpassdirection);

            var receiverpreposition = new PrepositionToken() { Preposition="to", PrepositionalObjects=new string[1] };
            receiverpreposition.Match += delegate(object sender, MatchEventArgs e)
            {
                this.ReceivePlayer = ((PrepositionToken)sender).PrepositionalObjects[0];
                //"(8:36 - 2nd)  A.Rodgers pass incomplete short right to E.St. Brown."
                //play.ToString()
                //"(8:36 - 2nd)  A.Rodgers pass incomplete short right to E.St."
                //this.Play
                //"(8:36 - 2nd)  A.Rodgers pass incomplete short right to E.Brown. Aaron rodgers injured on play."
                //how do you keep from thinking the name is E. Brown. Aaron?
                //... you could identify the noun phrases... Esp the specific nouns... like player names

                //if(this.ReceivePlayer.EndsWith(".") && e.StartIndex+2 < e.SourceTokens.Length)
                //    if(e.SourceTokens[e.StartIndex+2]=="to" && e.SourceTokens[e.StartIndex+2]=="for")
            };
            var expectreceiverpreposition = new ParseState() { Identifier = receiverpreposition };
            expectpassdepth.Transitions.Add(expectreceiverpreposition);
            expectpassdirection.Transitions.Add(expectreceiverpreposition);
            expectpassplay.Transitions.Add(expectreceiverpreposition);

            //pushed ob at _ _, to _ _
            var fieldpositionpreposition1 = new PrepositionToken() { Preposition = "pushed", PrepositionalObjects = new string[1], ExpectedObjects = new string[] { "ob" } };
            fieldpositionpreposition1.Match += delegate(object sender, MatchEventArgs e)
            {
                this.IsOb = true;
                this.Ob = "pushed";
            };
            var fieldpositionpreposition1b = new PrepositionToken() { Preposition = "ran", PrepositionalObjects = new string[1], ExpectedObjects = new string[] { "ob" } };
            fieldpositionpreposition1b.Match += delegate(object sender, MatchEventArgs e)
            {
                this.IsOb = true;
                this.Ob = "ran";
            };
            var expectfieldpositionprepositionPushedOB = new ParseState() { Identifier = fieldpositionpreposition1 };
            var expectfieldpositionprepositionRanOB = new ParseState() { Identifier = fieldpositionpreposition1b };
            var fieldpositionpreposition2 = new PrepositionToken() { Preposition = "to", PrepositionalObjects = new string[2], IsExpectedRegEx=true, ExpectedObjects = new string[] { "(" + this.Team1 + ")|(" + this.Team2 + ")|([A-Z][A-Z][A-Z]?)", "([0-9]+)|(zone)" } };
            fieldpositionpreposition2.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                //var fieldside = casted.PrepositionalObjects[0];
                //this is field position
                //var mark = casted.PrepositionalObjects[1];
                //var yd = 0;
                //int.TryParse(mark, out yd);
                var mark = YardMark.ParseYardmark(0, casted.PrepositionalObjects);
                if(this.IsPass)
                    this.PassStop = mark;
                else
                    this.RunStop = mark;
            };
            var expectfieldpositionprepositionToYardMarker = new ParseState() { Identifier = fieldpositionpreposition2 };
            var fieldpositionpreposition3 = new PrepositionToken() { Preposition = "at", PrepositionalObjects = new string[2], IsExpectedRegEx = true, ExpectedObjects = new string[] { "(" + this.Team1 + ")|(" + this.Team2 + ")|([A-Z][A-Z][A-Z]?)", "([0-9]+)|(zone)" } };
            fieldpositionpreposition3.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                //var fieldside = casted.PrepositionalObjects[0];
                //this is field position
                //var mark = casted.PrepositionalObjects[1];
                //var yd = 0;
                //int.TryParse(mark, out yd);
                //this.FieldPosition = new YardMark() { Side = fieldside, Distance = yd };
                var mark = YardMark.ParseYardmark(0, casted.PrepositionalObjects);
                mark.Preposition = "at";
                if (this.IsPass)
                    this.PassStop = mark;
                else
                    this.RunStop = mark;
            };
            var expectfieldpositionprepositionAtYardMarker = new ParseState() { Identifier = fieldpositionpreposition3 };
            expectfieldpositionprepositionPushedOB.Transitions.Add(expectfieldpositionprepositionAtYardMarker);
            expectfieldpositionprepositionRanOB.Transitions.Add(expectfieldpositionprepositionAtYardMarker);
            expectsackplay.Transitions.Add(expectfieldpositionprepositionAtYardMarker);
            expectsackob.Transitions.Add(expectfieldpositionprepositionAtYardMarker);

            expectpassdepth.Transitions.Add(expectfieldpositionprepositionPushedOB);
            expectpassdirection.Transitions.Add(expectfieldpositionprepositionPushedOB);
            expectpassplay.Transitions.Add(expectfieldpositionprepositionPushedOB);
            expectreceiverpreposition.Transitions.Add(expectfieldpositionprepositionPushedOB);

            expectpassdepth.Transitions.Add(expectfieldpositionprepositionRanOB);
            expectpassdirection.Transitions.Add(expectfieldpositionprepositionRanOB);
            expectpassplay.Transitions.Add(expectfieldpositionprepositionRanOB);
            expectreceiverpreposition.Transitions.Add(expectfieldpositionprepositionRanOB);

            expectpassdepth.Transitions.Add(expectfieldpositionprepositionToYardMarker);
            expectpassdirection.Transitions.Add(expectfieldpositionprepositionToYardMarker);
            expectpassplay.Transitions.Add(expectfieldpositionprepositionToYardMarker);
            expectreceiverpreposition.Transitions.Add(expectfieldpositionprepositionToYardMarker);

            var fieldposition50 = new PrepositionToken() { Preposition = "to", PrepositionalObjects = new string[1], ExpectedObjects = new string[] { "50" } };
            fieldposition50.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                if (this.IsPass)
                    this.PassStop = new YardMark() { Side = null, Distance = 50 };
                else
                    this.RunStop = new YardMark() { Side = null, Distance = 50 };
            };
            var expectfieldposition50 = new ParseState() { Identifier = fieldposition50 };
            expectpassdepth.Transitions.Add(expectfieldposition50);
            expectpassdirection.Transitions.Add(expectfieldposition50);
            expectpassplay.Transitions.Add(expectfieldposition50);
            expectreceiverpreposition.Transitions.Add(expectfieldposition50);



            //for _ yards
            var yardagepreposition = new PrepositionToken() { Preposition = "for", PrepositionalObjects = new string[2], IsExpectedRegEx = true, ExpectedObjects = new string[] { null, "yards|yard|yards\\.|yard\\.|gain" } };
            yardagepreposition.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                var num = 0;
                if (casted.PrepositionalObjects[0] == "no" && casted.PrepositionalObjects[0] == "gain")
                    num = 0;
                else
                {
                    var yards = casted.PrepositionalObjects[0];
                    int.TryParse(yards, out num);
                }
                if (this.IsPass)
                    this.PassYardage = new Yardage(num);
                else
                    this.RunYardage = new Yardage(num);
            };
            var expectedyardageprepositionForYds = new ParseState() { Identifier = yardagepreposition };
            expectpassdepth.Transitions.Add(expectedyardageprepositionForYds);
            expectpassdirection.Transitions.Add(expectedyardageprepositionForYds);
            expectpassplay.Transitions.Add(expectedyardageprepositionForYds);
            expectreceiverpreposition.Transitions.Add(expectedyardageprepositionForYds);
            expectfieldpositionprepositionPushedOB.Transitions.Add(expectedyardageprepositionForYds);
            expectfieldpositionprepositionRanOB.Transitions.Add(expectedyardageprepositionForYds);
            expectfieldpositionprepositionToYardMarker.Transitions.Add(expectedyardageprepositionForYds);
            //expectfieldpositionprepositionAtYardMarker.Transitions.Add(expectedyardageprepositionForYds);
            expectfieldposition50.Transitions.Add(expectedyardageprepositionForYds);
            expectfieldpositionprepositionAtYardMarker.Transitions.Add(expectedyardageprepositionForYds);
            expectrunplay.Transitions.Add(expectedyardageprepositionForYds); //for kneels

            //up the middle to GB _ for _ yards 
            var rundirection = new AdverbToken() { AdverbPatterns = new string[] { "guard", "tackle", "end" } };
            rundirection.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (AdverbToken)sender;
                this.RunFormation = casted.Adverb;
            };
            var expectrundirection = new ParseState() { Identifier=rundirection };
            var runmiddle = new PrepositionToken() { Preposition = "the",  PrepositionalObjects=new string[1], ExpectedObjects=new string[] { "middle" } };
            runmiddle.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
            };
            var expectrunmiddle = new ParseState() { Identifier = runmiddle };
            expectrunplay.Transitions.Add(expectrundirection);
            expectrunplay.Transitions.Add(expectrunmiddle);
            expectrunplay.Transitions.Add(expectedyardageprepositionForYds);

            expectrundirection.Transitions.Add(expectfieldpositionprepositionPushedOB);
            expectrundirection.Transitions.Add(expectfieldpositionprepositionRanOB);
            expectrundirection.Transitions.Add(expectfieldpositionprepositionToYardMarker);
            expectrundirection.Transitions.Add(expectedyardageprepositionForYds);
            expectrunmiddle.Transitions.Add(expectfieldpositionprepositionPushedOB);
            expectrunmiddle.Transitions.Add(expectfieldpositionprepositionRanOB);
            expectrunmiddle.Transitions.Add(expectfieldpositionprepositionToYardMarker);
            expectrunmiddle.Transitions.Add(expectedyardageprepositionForYds);

            expectformation.Transitions.Add(expectpassplay);
            expectformation.Transitions.Add(expectsackplay);
            expectformation.Transitions.Add(expectrunplay);
            //expectformation.Transitions.Add(expectrunmiddle);

            expectscrambledirection.Transitions.Add(expectrundirection);
            expectscrambledirection.Transitions.Add(expectrunmiddle);
            expectscrambledirection.Transitions.Add(expectedyardageprepositionForYds);


            var runfieldposition = new PrepositionToken() { Preposition = "to", PrepositionalObjects = new string[2], IsExpectedRegEx=true, ExpectedObjects = new string[] { "(" + this.Team1 + ")|(" + this.Team2 + ")|([A-Z][A-Z][A-Z]?)", null } };
            runfieldposition.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                //var fieldside = casted.PrepositionalObjects[0];
                //this is field position
                //var mark = casted.PrepositionalObjects[1];
                //var yd = 0;
                //int.TryParse(mark, out yd);
                //this.FieldPosition = new YardMark() { Side = fieldside, Distance = yd };
                var mark = YardMark.ParseYardmark(0, casted.PrepositionalObjects);
                this.RunStop = mark;
            };
            var expectrunToYardMarker = new ParseState() { Identifier = runfieldposition };
            expectrunToYardMarker.Transitions.Add(expectedyardageprepositionForYds);
            expectrunplay.Transitions.Add(expectrunToYardMarker);
            expectrundirection.Transitions.Add(expectrunToYardMarker);
            expectfieldpositionprepositionPushedOB.Transitions.Add(expectrunToYardMarker);
            expectfieldpositionprepositionRanOB.Transitions.Add(expectrunToYardMarker);
            expectfieldpositionprepositionToYardMarker.Transitions.Add(expectrunToYardMarker);
            expectedyardageprepositionForYds.Transitions.Add(expectrunToYardMarker);
            var runfieldposition2 = new PrepositionToken() { Preposition = "to", PrepositionalObjects = new string[1], ExpectedObjects=new string[] {"50"} };
            runfieldposition2.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                //var fieldside = casted.PrepositionalObjects[0];
                //this is field position
                //var mark = casted.PrepositionalObjects[1];
                //var yd = 0;
                //int.TryParse(mark, out yd);
                //this.FieldPosition = new YardMark() { Side = fieldside, Distance = yd };
                var mark = YardMark.ParseYardmark(0, casted.PrepositionalObjects);
                this.RunStop = mark;
            };
            var expectrunfieldpositionTo50 = new ParseState() { Identifier = runfieldposition2 };
            expectrundirection.Transitions.Add(expectrunfieldpositionTo50);
            expectrunmiddle.Transitions.Add(expectrunfieldpositionTo50);
            expectfieldpositionprepositionPushedOB.Transitions.Add(expectrunfieldpositionTo50);
            expectfieldpositionprepositionRanOB.Transitions.Add(expectrunfieldpositionTo50);
            expectfieldpositionprepositionToYardMarker.Transitions.Add(expectrunfieldpositionTo50);
            expectedyardageprepositionForYds.Transitions.Add(expectrunfieldpositionTo50);
            expectrunplay.Transitions.Add(expectrunfieldpositionTo50);

            expectrunfieldpositionTo50.Transitions.Add(expectedyardageprepositionForYds);


            var tackledby = new ParenToken();
            tackledby.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (ParenToken)sender;
                if(this.TackledByPlayer==null)
                    this.TackledByPlayer = string.Join(" ", casted.Elements);
                else
                    this.TackledByPlayer += " " + string.Join(" ", casted.Elements);
            };
            var expecttackledby = new ParseState() { Identifier = tackledby };
            expecttackledby.Transitions.Insert(0,expecttackledby); //there may be more than one tackler
            expectedyardageprepositionForYds.Transitions.Insert(0,expecttackledby);

            expectpassdirection.Transitions.Insert(0,expecttackledby);
            expectpassdepth.Transitions.Insert(0,expecttackledby);

            expectreceiverpreposition.Transitions.Add(expecttackledby);
            expectfieldposition50.Transitions.Add(expecttackledby);

            var tackledby2 = new ParenToken() { Open="[", Close=']' };
            tackledby2.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (ParenToken)sender;
                if (this.TackledByPlayer == null)
                    this.TackledByPlayer = string.Join(" ", casted.Elements);
                else
                    this.TackledByPlayer += " " + string.Join(" ", casted.Elements);
            };
            var expecttackledby2 = new ParseState() { Identifier = tackledby2 };
            expecttackledby2.Transitions.Insert(0, expecttackledby2); //there may be more than one tackler
            expectedyardageprepositionForYds.Transitions.Insert(0, expecttackledby2);

            expectpassdirection.Transitions.Insert(0, expecttackledby2);
            expectpassdepth.Transitions.Insert(0, expecttackledby2);

            expecttackledby.Transitions.Insert(0, expecttackledby2);
            expecttackledby2.Transitions.Insert(0, expecttackledby);

            expectreceiverpreposition.Transitions.Add(expecttackledby2);
            expectfieldposition50.Transitions.Add(expecttackledby2);

            //PENALTY on WAS-T.Williams, False Start, 5 yards, enforced at ARZ 30 - No Play.
            var penaltyaccepted = new SentenceToken() { StartPattern = "PENALTY" };
            penaltyaccepted.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (SentenceToken)sender;
                this.IsPenaltyCalled = true;
                this.IsPenaltyAccepted = true;
                var pos = Array.IndexOf(casted.Elements, "enforced");
                if (pos >= 0 && casted.Elements[pos+1]=="at")
                    this.PenaltyEnforcedAt = YardMark.ParseYardmark(pos + 2, casted.Elements);
                var yds = Array.IndexOf(casted.Elements, "yards,"); //no penalty is 1 yd, heh, heh
                int num = 0;
                if (yds >= 0 && int.TryParse(casted.Elements[yds - 1], out num))
                    this.PenaltyYardage = new Yardage( num );

                var on = Array.IndexOf(casted.Elements, "on");
                if (on >= 0)
                {
                    this.PenaltyPlayer = casted.Elements[on + 1];
                    for (int i = on + 2; i < casted.Elements.Length; i++)
                    {
                        if (this.PenaltyRule == null)
                            this.PenaltyRule = casted.Elements[i];
                        else
                            this.PenaltyRule += " " + casted.Elements[i];
                        if (casted.Elements[i].Contains(','))
                            break;
                    }
                }
                if (casted.Contains("-", "No", "Play."))
                    this.IsPenaltyNoPlay = true;
            };
            var expectedpenaltyaccepted = new ParseState() { Identifier = penaltyaccepted };
            expectfieldpositionprepositionPushedOB.Transitions.Add(expectedpenaltyaccepted);
            expectfieldpositionprepositionRanOB.Transitions.Add(expectedpenaltyaccepted);
            expectfieldpositionprepositionToYardMarker.Transitions.Add(expectedpenaltyaccepted);
            expectedyardageprepositionForYds.Transitions.Add(expectedpenaltyaccepted);
            expectrundirection.Transitions.Add(expectedpenaltyaccepted);

            expectpassdepth.Transitions.Add(expectedpenaltyaccepted);
            expectpassdirection.Transitions.Add(expectedpenaltyaccepted);
            expectpassplay.Transitions.Add(expectedpenaltyaccepted);
            expectreceiverpreposition.Transitions.Add(expectedpenaltyaccepted);
            //expectfieldpositionpreposition1.Transitions.Add(expectedpenaltyaccepted);
            //expectfieldpositionpreposition2.Transitions.Add(expectedpenaltyaccepted);
            expecttackledby.Transitions.Add(expectedpenaltyaccepted);
            expecttackledby2.Transitions.Add(expectedpenaltyaccepted);
            expectformation.Transitions.Add(expectedpenaltyaccepted);

            expectgameclock.Transitions.Add(expectedpenaltyaccepted);
            expectformation.Transitions.Add(expectedpenaltyaccepted);


            var penaltydeclined = new SentenceToken() { StartPattern = "Penalty" };
            penaltydeclined.Match += delegate(object sender, MatchEventArgs e)
            {
                //(12:52 - 3rd)  A.Peterson left end to WAS 4 for -4 yards (C.Peters). Penalty on WAS-C.Roullier, Offensive Holding, declined.
                var casted = (SentenceToken)sender;
                this.IsPenaltyCalled = true;
                this.IsPenaltyAccepted = false;

                var on = Array.IndexOf(casted.Elements, "on");
                if (on >= 0)
                {
                    this.PenaltyPlayer = casted.Elements[on + 1];
                    for (int i = on + 2; i < casted.Elements.Length; i++)
                    {
                        if (this.PenaltyRule == null)
                            this.PenaltyRule = casted.Elements[i];
                        else
                            this.PenaltyRule += " " + casted.Elements[i];
                        if (casted.Elements[i].Contains(','))
                            break;
                    }
                }
            };
            var expectpenaltydeclined = new ParseState() { Identifier = penaltydeclined };
            expectfieldpositionprepositionPushedOB.Transitions.Add(expectpenaltydeclined);
            expectfieldpositionprepositionRanOB.Transitions.Add(expectpenaltydeclined);
            expectfieldpositionprepositionToYardMarker.Transitions.Add(expectpenaltydeclined);
            expectedyardageprepositionForYds.Transitions.Add(expectpenaltydeclined);
            expectrundirection.Transitions.Add(expectpenaltydeclined);

            expectpassdepth.Transitions.Add(expectpenaltydeclined);
            expectpassdirection.Transitions.Add(expectpenaltydeclined);
            expectpassplay.Transitions.Add(expectpenaltydeclined);
            expectreceiverpreposition.Transitions.Add(expectpenaltydeclined);
            //expectfieldpositionpreposition1.Transitions.Add(expectpenaltydeclined);
            //expectfieldpositionpreposition1b.Transitions.Add(expectpenaltydeclined);
            //expectfieldpositionpreposition2.Transitions.Add(expectpenaltydeclined);
            expecttackledby.Transitions.Add(expectpenaltydeclined);
            expecttackledby2.Transitions.Add(expectpenaltydeclined);
            expectformation.Transitions.Add(expectpenaltydeclined);



            var kickoff = new SubjectVerbToken() { VerbPatterns = new string[] { "kicks" } };
            kickoff.Match += delegate(object sender, MatchEventArgs e)
            {
                //(15:00 - 1st) P.Dawson kicks 66 yards from ARZ 35 to WAS -1. D.Johnson to WAS 23 for 24 yards (B.Benwikere).
                var casted = (SubjectVerbToken)sender;
                this.IsKick = true;
                this.KickPlayer = string.Join(" ", casted.Subject);

            };
            var expectedkickoff = new ParseState() { Identifier = kickoff };
            expectgameclock.Transitions.Add(expectedkickoff);

            var onside = new AdverbToken() { AdverbPatterns = new string[] { "onside" } };
            onside.Match += delegate(object sender, MatchEventArgs e)
            {
                this.IsOnsideKick = true;
            };
            var expectedonside = new ParseState() { Identifier = onside };
            expectedkickoff.Transitions.Add(expectedonside);



            var kickoffyards = new SubjectVerbToken() { VerbPatterns = new string[] { "yards" } };
            kickoffyards.Match += delegate(object sender, MatchEventArgs e)
            {
                //(15:00 - 1st) P.Dawson kicks 66 yards from ARZ 35 to WAS -1. D.Johnson to WAS 23 for 24 yards (B.Benwikere).
                var casted = (SubjectVerbToken)sender;
                this.KickYards = new Yardage(int.Parse( casted.Subject[0]));

            };
            var expectedkickoffyards = new ParseState() { Identifier = kickoffyards };
            expectedkickoff.Transitions.Add(expectedkickoffyards);
            expectedonside.Transitions.Add(expectedkickoffyards);

            var kickfrom = new PrepositionToken() { Preposition="from", PrepositionalObjects=new string[2] };
            kickfrom.Match += delegate(object sender, MatchEventArgs e)
            {
                //(15:00 - 1st) P.Dawson kicks 66 yards from ARZ 35 to WAS -1. D.Johnson to WAS 23 for 24 yards (B.Benwikere).
                var casted = (PrepositionToken)sender;
                this.KickStart = YardMark.ParseYardmark(0, casted.PrepositionalObjects);

            };
            var expectedkickfrom = new ParseState() { Identifier = kickfrom };
            expectedkickoffyards.Transitions.Add(expectedkickfrom);


            var kickto = new PrepositionToken() { Preposition = "to", PrepositionalObjects = new string[2] };
            kickto.Match += delegate(object sender, MatchEventArgs e)
            {
                //(15:00 - 1st) P.Dawson kicks 66 yards from ARZ 35 to WAS -1. D.Johnson to WAS 23 for 24 yards (B.Benwikere).
                var casted = (PrepositionToken)sender;
                this.KickLand = YardMark.ParseYardmark(0, casted.PrepositionalObjects);

            };
            var expectedkickto = new ParseState() { Identifier = kickto };
            expectedkickfrom.Transitions.Add(expectedkickto);

            var kickreturn = new PhraseToken() { ExpectedPattern=new string[] { null, "to", null, null, "for", null, "yards", null } };
            kickreturn.Match += delegate(object sender, MatchEventArgs e)
            {
                //(15:00 - 1st) P.Dawson kicks 66 yards from ARZ 35 to WAS -1. D.Johnson to WAS 23 for 24 yards (B.Benwikere).
                var casted = (PhraseToken)sender;
                this.KickReturnPlayer = casted.Words[0];
                this.KickAdvanceStop = YardMark.ParseYardmark(2, casted.Words);
                this.KickReturn = new Yardage(casted.Words[5]);
                this.TackledByPlayer = casted.Words[7];
                if (this.TackledByPlayer.StartsWith("(") && !this.TackledByPlayer.Contains(')'))
                    for (int i = e.StartIndex + 8; i < e.SourceTokens.Length; i++)
                        if (this.TackledByPlayer.Contains(')'))
                        {
                            this.TackledByPlayer += " " + e.SourceTokens[i];
                            break;
                        }
                        else
                            this.TackledByPlayer += " " + e.SourceTokens[i];
            };
            var expectedkickreturn= new ParseState() { Identifier = kickreturn };
            expectedkickto.Transitions.Add(expectedkickreturn);

            var kickreturn2 = new PhraseToken() { ExpectedPattern = new string[] { null, "to", null, null, "for", "no", "gain", null } };
            kickreturn2.Match += delegate(object sender, MatchEventArgs e)
            {
                //(15:00 - 1st) P.Dawson kicks 66 yards from ARZ 35 to WAS -1. D.Johnson to WAS 23 for 24 yards (B.Benwikere).
                var casted = (PhraseToken)sender;
                this.KickReturnPlayer = casted.Words[0];
                this.KickAdvanceStop = YardMark.ParseYardmark(2, casted.Words);
                this.KickReturn = new Yardage(0); //new Yardage(casted.Words[5]);
                this.TackledByPlayer = casted.Words[7];
                if (this.TackledByPlayer.StartsWith("(") && !this.TackledByPlayer.Contains(')'))
                    for (int i = e.StartIndex + 8; i < e.SourceTokens.Length; i++)
                        if (this.TackledByPlayer.Contains(')'))
                        {
                            this.TackledByPlayer += " " + e.SourceTokens[i];
                            casted.Next++;
                            break;
                        }
                        else
                            this.TackledByPlayer += " " + e.SourceTokens[i];
            };
            var expectedkickreturn2 = new ParseState() { Identifier = kickreturn2 };
            expectedkickto.Transitions.Add(expectedkickreturn2);

            var kickreturn3 = new PhraseToken() { IsExpectedRegEx=true, ExpectedPattern = new string[] { null, null, "ob", "at", null, null, "for", null, "yard*" } };
            kickreturn3.Match += delegate(object sender, MatchEventArgs e)
            {
                //"(5:51 - 3rd) M.Crosby kicks 67 yards from GB 35 to ATL -2. M.Hall pushed ob at ATL 24 for 26 yards (W.Redmond)."
                var casted = (PhraseToken)sender;
                this.KickReturnPlayer = casted.Words[0];
                this.IsOb = true;
                this.Ob = casted.Words[1];
                this.KickAdvanceStop = YardMark.ParseYardmark(4, casted.Words);
                this.KickReturn = new Yardage(casted.Words[7]);
                if(e.StartIndex+9<e.SourceTokens.Length)
                {
                    var evaluatin = e.SourceTokens[e.StartIndex+9];
                    if (evaluatin.StartsWith("("))
                        for (int i = e.StartIndex + 9; i < e.SourceTokens.Length; i++)
                            if (e.SourceTokens[i].Contains(')'))
                            {
                                this.TackledByPlayer += " " + e.SourceTokens[i];
                                break;
                            }
                            else
                                this.TackledByPlayer += " " + e.SourceTokens[i];
                }
                /*this.TackledByPlayer = casted.Words[9];
                if (this.TackledByPlayer.StartsWith("(") && !this.TackledByPlayer.Contains(')'))
                    for (int i = e.StartIndex + 10; i < e.SourceTokens.Length; i++)
                        if (this.TackledByPlayer.Contains(')'))
                        {
                            this.TackledByPlayer += " " + e.SourceTokens[i];
                            break;
                        }
                        else
                            this.TackledByPlayer += " " + e.SourceTokens[i];*/
            };
            var expectedkickreturn3 = new ParseState() { Identifier = kickreturn3 };
            expectedkickto.Transitions.Add(expectedkickreturn3);

            var kicktouchback = new PhraseToken() { ExpectedPattern=new string[] { "Touchback." } };
            kicktouchback.Match += delegate(object sender, MatchEventArgs e)
            {
                //(14:54 - 2nd) D.Hopkins kicks 65 yards from WAS 35 to end zone, Touchback.
                var casted = (PhraseToken)sender;
                this.IsKickTouchback = true;
            };
            var expectedkicktouchback= new ParseState() { Identifier = kicktouchback };
            expectedkickto.Transitions.Add(expectedkicktouchback);
            

            var punt = new SubjectVerbToken() { VerbPatterns = new string[] { "punts" } };
            punt.Match += delegate(object sender, MatchEventArgs e)
            {
                //(3:27 - 2nd)  A.Lee punts 56 yards to WAS 17, Center-A.Brewer, fair catch by T.Quinn. PENALTY on WAS-R.Kelley, Offensive Holding, 9 yards, enforced at WAS 17.
                var casted = (SubjectVerbToken)sender;
                this.IsPunt = true;
                this.PuntPlayer = string.Join(" ", casted.Subject);

                //for(int i=0; i<casted.
                //public string PuntSnapper;
                //public string IsPuntFairCatch;
                //public string PuntReceiver;

            };
            var expectedpunt = new ParseState() { Identifier = punt };
            expectgameclock.Transitions.Add(expectedpunt);

            
            var puntyards = new SubjectVerbToken() { VerbPatterns = new string[] { "yards" } };
            puntyards.Match += delegate(object sender, MatchEventArgs e)
            {
                //(3:27 - 2nd)  A.Lee punts 56 yards to WAS 17, Center-A.Brewer, fair catch by T.Quinn. PENALTY on WAS-R.Kelley, Offensive Holding, 9 yards, enforced at WAS 17.
                var casted = (SubjectVerbToken)sender;
                int yd=0;
                if(int.TryParse(casted.Subject[0], out yd))
                    this.PuntYardage = new Yardage(yd);
            };
            var expectpuntyards= new ParseState() { Identifier = puntyards };
            expectedpunt.Transitions.Add(expectpuntyards);

            var puntlanded = new PrepositionToken() { Preposition = "to", PrepositionalObjects = new string[2]};
            puntlanded.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                //casted.PrepositionalObjects == {end,zone}
                var mark = YardMark.ParseYardmark(0, casted.PrepositionalObjects);
                this.PuntLanded = mark;
            };
            var expectpuntlanded = new ParseState() { Identifier = puntlanded };
            expectpuntyards.Transitions.Add(expectpuntlanded);


            var punttouchback = new AdverbToken() { AdverbPatterns = new string[] { "Touchback", "Touchback." } };
            punttouchback.Match += delegate(object sender, MatchEventArgs e)
            {
                this.IsPuntTouchback = true;
            };
            var expectpunttouchback = new ParseState() { Identifier = punttouchback };
            expectpuntlanded.Transitions.Add(expectpunttouchback);
            expectpunttouchback.Transitions.Add(expectpenaltydeclined);
            expectpunttouchback.Transitions.Add(expectedpenaltyaccepted);


            var puntsnapper = new RegExToken() { RegEx = "Center-.+" };
            puntsnapper.Match += delegate(object sender, MatchEventArgs e)
            {
                this.PuntSnapper = ((RegExToken)sender).Token;
            };
            var expectpuntsnapper = new ParseState() { Identifier = puntsnapper };
            expectpuntlanded.Transitions.Add(expectpuntsnapper);
            expectpuntsnapper.Transitions.Add(expectpunttouchback);

            var puntoutbounds = new PrepositionToken() { Preposition="out", ExpectedObjects=new string[] { "of", "bounds."}, PrepositionalObjects = new string[2] };
            puntoutbounds.Match += delegate(object sender, MatchEventArgs e)
            {
                this.IsPuntOutOfBounds = true;
            };
            var expectpuntoutbounds = new ParseState() { Identifier = puntoutbounds };
            expectpuntsnapper.Transitions.Add(expectpuntoutbounds);
            
            var faircatch = new AdverbToken() { AdverbPatterns = new string[] { "fair" } };
            faircatch.Match += delegate(object sender, MatchEventArgs e)
            {
                this.IsPuntFairCatch = true;
            };
            var expectfaircatch = new ParseState() { Identifier = faircatch };
            expectpuntsnapper.Transitions.Add(expectfaircatch);

            var puntcaught = new PrepositionToken() { Preposition = "catch", PrepositionalObjects = new string[2], ExpectedObjects = new string[] {"by",null} };
            puntcaught.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                this.PuntReceiver = casted.PrepositionalObjects[1];
            };
            var expectedpuntcaught = new ParseState() { Identifier = puntcaught };
            //expectpuntsnapper.Transitions.Add(expectedpuntcaught);
            expectfaircatch.Transitions.Add(expectedpuntcaught);
            //expectmuffed.Transitions.Add(expectedpuntcaught);


            var puntdowned = new PrepositionToken() { Preposition = "downed", PrepositionalObjects = new string[2], ExpectedObjects = new string[] { "by", null } };
            puntdowned.Match += delegate(object sender, MatchEventArgs e)
            {
                var casted = (PrepositionToken)sender;
                this.PuntingTeamPlayerDowned = casted.PrepositionalObjects[1];
            };
            var expectdowned = new ParseState() { Identifier = puntdowned };
            expectpuntsnapper.Transitions.Add(expectdowned);
            expectpuntsnapper.Transitions.Add(expectpenaltydeclined);
            expectpuntsnapper.Transitions.Add(expectedpenaltyaccepted);


            var puntreturned = new PhraseToken() { Words = new string[7], ExpectedPattern = new string[] { null, "to", null, null, "for", null, null  } };
            puntreturned.Match += delegate(object sender, MatchEventArgs e)
            {
                //(6:50 - 1st)  T.Way punts 39 yards to PHI 19, Center-A.East. D.Sproles to PHI 21 for 2 yards (A.Alexander).
                var casted = (PhraseToken)sender;
                this.PuntReturningPlayer= casted.Words[0];
                this.PuntStopped = YardMark.ParseYardmark(2, casted.Words);
                var num = 0;
                int.TryParse(casted.Words[5], out num);
                this.PuntReturnYardage = new Yardage(num);
            };
            var expectpuntreturned = new ParseState() { Identifier = puntreturned };
            expectpuntsnapper.Transitions.Add(expectpuntreturned);
            expectpuntreturned.Transitions.Add(expecttackledby);
            expectpuntreturned.Transitions.Add(expecttackledby2);


            var puntreturnedob = new PhraseToken() { Words = new string[9], ExpectedPattern = new string[] { null, null, "ob", "at", null, null, "for", null, null } };
            puntreturnedob.Match += delegate(object sender, MatchEventArgs e)
            {
                //"(8:55 - 1st)  S.Koch punts 51 yards to KC 19, Center-M.Cox. T.Hill pushed ob at KC 25 for 6 yards (C.Clark)."
                var casted = (PhraseToken)sender;
                this.PuntReturningPlayer = casted.Words[0];
                this.PuntStopped = YardMark.ParseYardmark(4, casted.Words);
                var num = 0;
                int.TryParse(casted.Words[7], out num);
                this.PuntReturnYardage = new Yardage(num);
                this.IsOb = true;
                this.Ob = casted.Words[1];
            };
            var expectpuntreturnedob = new ParseState() { Identifier = puntreturnedob };
            expectpuntsnapper.Transitions.Add(expectpuntreturnedob);
            expectpuntreturnedob.Transitions.Add(expecttackledby);
            expectpuntreturnedob.Transitions.Add(expecttackledby2);


            var fieldgoal = new SubjectVerbToken() { VerbPatterns = new string[] { "Goal" } };
            fieldgoal.Match += delegate(object sender, MatchEventArgs e)
            {
                //(13:14 - 2nd) Justin Tucker 41 Yd Field Goal
                //(0:22 - 1st) Jake Elliott 33 Yd Field Goal
                var casted = (SubjectVerbToken)sender;
                if (casted.Subject[casted.Subject.Length - 1] == "Field")
                {
                    this.IsFieldGoalAttempt = true;
                    this.IsFieldGoalGood = true;
                    var yd = Array.IndexOf(casted.Subject, "Yd");
                    int num = 0;
                    if (yd >= 0 && int.TryParse(casted.Subject[yd - 1], out num))
                        this.FieldGoalYardage = new Yardage(num);
                    if (num == 0)
                        this.FieldGoalPlayer = string.Join(" ", casted.Subject.Take(casted.Subject.Length - 2));
                    else
                        this.FieldGoalPlayer = string.Join(" ", casted.Subject.Take(num-1));
                }
            };
            var expectfieldgoal = new ParseState() { Identifier = fieldgoal };
            expectgameclock.Transitions.Add(expectfieldgoal);

            var fieldgoalmiss = new PhraseToken() { ExpectedPattern = new string[] { null, null, "yard", "field", "goal", "is", "No", "Good", null, null, null } };
            fieldgoalmiss.Match += delegate(object sender, MatchEventArgs e)
            {
                //(Field Goal formation) S.Hauschka 52 yard field goal is No Good, Short, Center-R.Ferguson, Holder-C.Bojorquez.
                var casted = (PhraseToken)sender;
                this.IsFieldGoalAttempt = true;
                this.FieldGoalPlayer = casted.Words[0];
                this.FieldGoalYardage = new Yardage(casted.Words[1]);
                this.FieldGoalMissBy = casted.Words[8];
                this.FieldGoalCenter = casted.Words[9];
                this.FieldGoalHolder = casted.Words[10];
            };
            var expectfieldgoalmiss = new ParseState() { Identifier = fieldgoalmiss };
            expectformation.Transitions.Add(expectfieldgoalmiss);
            expectgameclock.Transitions.Add(expectfieldgoalmiss);


            //touchdowns....
            var runtd = new LookForToken() { LookFor = new string[] {"Yard","Rush"}};
            runtd.Match += delegate(object sender, MatchEventArgs e)
            {
                //(4:10 - 2nd) Adrian Peterson 1 Yard Rush D.Hopkins extra point is GOOD, Center-N.Sundberg, Holder-T.Way.
                var casted = (LookForToken)sender;
                this.IsRun = true;
                this.IsTouchdown = true;
                this.RunPlayer = string.Join(" ", casted.Before.Take(casted.Before.Length-1));
                this.RunYardage = new Yardage(casted.Before[casted.Before.Length-1]);
                this.Conversion = string.Join(" ", casted.After);
            };
            var expectruntd = new ParseState() { Identifier = runtd };
            expectformation.Transitions.Insert(2,expectruntd);
            expectgameclock.Transitions.Insert(2,expectruntd);

            var runtd2 = new PhraseToken() { IsExpectedRegEx = true, ExpectedPattern = new string[] { null, "right|left", "guard|tackle|end", "for", null, "yards,", "^TOUCHDOWN.", null } };
            runtd2.Match += delegate(object sender, MatchEventArgs e)
            {
                //"(2:26 - 1st)  A.Blue right guard for 3 yards, TOUCHDOWN.K.Fairbairn extra point is GOOD, Center-J.Weeks, Holder-T.Daniel."
                //_ _ _ for _ yards, TOUCHDOWN.... conversion
                var casted = (PhraseToken)sender;
                this.IsRun = true;
                this.IsTouchdown = true;
                this.IsTouchdownStandardPlayFormat = true;
                this.RunPlayer = casted.Words[0];
                this.RunDirection = casted.Words[1];
                this.RunFormation = casted.Words[2];
                this.RunYardage = new Yardage(casted.Words[4]);
                this.Conversion = string.Join(" ", e.SourceTokens.Skip(e.StartIndex + 6)); //casted.Words.Skip(6));
            };
            var expectruntd2 = new ParseState() { Identifier = runtd2 };
            expectformation.Transitions.Insert(2, expectruntd2);
            expectgameclock.Transitions.Insert(2, expectruntd2);


            var passtd = new LookForToken() { LookFor = new string[] {"Pass","From"} };
            passtd.Match += delegate(object sender, MatchEventArgs e)
            {
                //(11:36 - 1st) Julio Jones 16 Yd pass from Matt Ryan (Matt Bryant Kick)
                //(2:13 - 4th) Randall Cobb Pass From Aaron Rodgers for 75 Yrds M.Crosby extra point is GOOD, Center-H.Bradley, Holder-J.Scott.
                var casted = (LookForToken)sender;
                this.IsPass = true;
                this.IsTouchdown = true;
                if(casted.Before[casted.Before.Length-1]=="Yd") 
                {
                    this.ReceivePlayer = string.Join(" ", casted.Before.Take(casted.Before.Length-2));
                    this.PassYardage= new Yardage(casted.Before[casted.Before.Length-2]);
                    var convert = casted.After.TakeWhile(s=>!s.StartsWith("(")).Count();
                    this.PassPlayer = string.Join(" ", casted.After.Take(convert));
                    this.Conversion = string.Join(" ", casted.After.Skip(convert));
                    this.TdYardPassFrom = true;
                } else {
                    this.ReceivePlayer = string.Join(" ", casted.Before);
                    var foryds = Array.IndexOf(casted.After, "for");
                    this.PassPlayer = string.Join(" ", casted.After.Take(foryds));
                    this.PassYardage = new Yardage(casted.After[foryds+1]);
                    this.Conversion = string.Join(" ", casted.After.Skip(foryds + 3));
                    this.TdYardPassFrom = false;
                }
            };
            passtd.ExclusionCheck = delegate(string[] token)
            {
                var findlateral = Array.IndexOf(token, "Lateral");
                if (findlateral >= 0 && findlateral + 2 < token.Length && token[findlateral + 1].ToLower() == "pass" && token[findlateral + 2].ToLower() == "from")
                    return true; //this will be a false positive, so we kick it out here.
                else
                    return false;
            };
            var expectpasstd = new ParseState() { Identifier = passtd };
            expectformation.Transitions.Insert(2,expectpasstd);
            expectgameclock.Transitions.Insert(2,expectpasstd);

            var passtd2 = new PhraseToken() { IsExpectedRegEx = true, ExpectedPattern = new string[] { null, "pass", "short|long", "left|middle|right", "to", null, "for", null, "yards,", "^TOUCHDOWN." } };
            passtd2.Match += delegate(object sender, MatchEventArgs e)
            {
                //(1:55 - OT)  (Shotgun) D.Prescott pass short left to A.Cooper for 15 yards, TOUCHDOWN. Ball caught at the 7 yard line after deflection by Rasul Douglas
                //_ pass _ _ to _ for _ yards, TOUCHDOWN.... conversion
                var casted = (PhraseToken)sender;
                this.IsPass = true;
                this.IsTouchdown = true;
                this.IsTouchdownStandardPlayFormat = true;
                this.PassPlayer = casted.Words[0];
                this.PassDepth = casted.Words[2];
                this.PassDirection = casted.Words[3];
                this.ReceivePlayer = casted.Words[5];
                this.PassYardage = new Yardage(casted.Words[7]);
                this.Conversion = string.Join(" ", e.SourceTokens.Skip(e.StartIndex + 9)); //casted.Words.Skip(9));  //casted.Words[9];
            };
            var expectpasstd2 = new ParseState() { Identifier = passtd2 };
            expectformation.Transitions.Insert(2, expectpasstd2);
            expectgameclock.Transitions.Insert(2, expectpasstd2);


            var reportaseligible = new SubjectVerbToken() { VerbPatterns = new string[] {"eligible." } };
            reportaseligible.Match += delegate(object sender, MatchEventArgs e)
            {
                //(8:47 - 1st)  J.Wetzel reported in as eligible.  D.Johnson up the middle to ARZ 31 for 11 yards (D.Swearinger).
                var casted = (SubjectVerbToken)sender;
                if (this.ReportedEligible == null)
                    this.ReportedEligible = string.Join(" ", casted.Subject) + " eligible.";
                else
                    this.ReportedEligible = " " + string.Join(" ", casted.Subject) + " eligible.";
            };
            var expectreportaseligible = new ParseState() { Identifier = reportaseligible };
            expectgameclock.Transitions.Add(expectreportaseligible);
            expectreportaseligible.Transitions.AddRange(expectgameclock.Transitions);


            
            var lateral = new SentenceToken() { StartPattern = "Lateral" };
            lateral.Match += delegate(object sender, MatchEventArgs e)
            {
                //"(0:14 - 4th)  (Shotgun) B.Roethlisberger pass short left to J.Washington to PIT 35 for 5 yards. Lateral to J.Smith-Schuster ran ob at OAK 22 for 43 yards (K.Joseph)."
                //eventually, we'll throw this in a lateral field
            };
            var expectlateral = new ParseState() { Identifier = lateral };
            expectedyardageprepositionForYds.Transitions.Insert(0,expectlateral);


            var challenge = new SentenceToken() { StartPattern = "challenged" };
            challenge.Match += delegate(object sender, MatchEventArgs e)
            {
                //"(0:14 - 4th)  (Shotgun) B.Roethlisberger pass short left to J.Washington to PIT 35 for 5 yards. Lateral to J.Smith-Schuster ran ob at OAK 22 for 43 yards (K.Joseph)."
                //eventually, we'll throw this in a lateral field
            };
            var expectchallenge = new ParseState() { Identifier = challenge };
            expectedyardageprepositionForYds.Transitions.Insert(0, expectchallenge);


            var passback = new SentenceToken() { StartPattern = "Pass" };
            passback.Match += delegate(object sender, MatchEventArgs e)
            {
                //"(9:54 - 2nd)  D.Johnson right end to CLV 44 for -3 yards. Pass back to N.Chubb to CLV 49 for 5 yards (F.Oluokun)."

            };
            var expectpassback = new ParseState() { Identifier = passback };
            expectedyardageprepositionForYds.Transitions.Insert(0, expectpassback);





            try
            {
                ParseStateNavigator navigator = new ParseStateNavigator() { Current = bof };
                int index = 0;
                navigator.Parse(ref index, this.Play.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
            catch { } //do nothing, if exception.  We're expecting 75% parse reconciliation rate






            //this.ParseDownDistance();
            __all++;
            this.IsValidated = false;
            //Console.WriteLine(this.Play);


            if (this.IsGameMilestone)
            {
                //(1:57 - 2nd) Two-Minute Warning
                this.IsValidated = true;
                this.ValidatedAgainst = "(trust result of milestone parse)";
                Console.WriteLine("MILESTONE " + this.Play);
            }
            else if (this.IsTimeout)
            {
                //(0:43 - 2nd) Timeout #2 by WAS at 00:43.
                this.IsValidated = true;
                this.ValidatedAgainst = "(trust result of timeout parse)";
                Console.WriteLine("TIMEOUT " + this.Play);
            }
            else if (this.IsTouchdown)
            {
                StringBuilder play = new StringBuilder();
                play.AppendFormat("({0:0}:{1:00} - {2})  ", this.Time.TimeLeft.Minutes, this.Time.TimeLeft.Seconds, this.Time.Qtr == 1 ? "1st" : this.Time.Qtr == 2 ? "2nd" : this.Time.Qtr == 3 ? "3rd" : this.Time.Qtr == 4 ? "4th" : "OT");
                if (!string.IsNullOrWhiteSpace(this.ReportedEligible))
                    play.AppendFormat("{0} ", this.ReportedEligible);
                if (!string.IsNullOrWhiteSpace(this.Formation))
                    play.AppendFormat("{0} ", this.Formation);

                if(this.IsRun) 
                {
                    if (this.IsTouchdownStandardPlayFormat)
                    {
                        //"(2:26 - 1st)  A.Blue right guard for 3 yards, TOUCHDOWN.K.Fairbairn extra point is GOOD, Center-J.Weeks, Holder-T.Daniel."
                        play.AppendFormat("{0} {1} {2} for {3}, {4}", this.RunPlayer, this.RunDirection, this.RunFormation, this.RunYardage, this.Conversion);
                    }
                    else
                    {
                        //(4:10 - 2nd) Adrian Peterson 1 Yard Rush D.Hopkins extra point is GOOD, Center-N.Sundberg, Holder-T.Way.
                        play.AppendFormat("{0} {1} Yard Rush {2}", this.RunPlayer, this.RunYardage.Yards, this.Conversion);
                    }
                } 
                else if(this.IsPass)
                {
                    if (this.IsTouchdownStandardPlayFormat)
                    {
                        //(1:55 - OT)  (Shotgun) D.Prescott pass short left to A.Cooper for 15 yards, TOUCHDOWN. Ball caught at the 7 yard line after deflection by Rasul Douglas
                        play.AppendFormat("{0} pass {1} {2} to {3} for {4}, {5}", this.PassPlayer, this.PassDepth, this.PassDirection, this.ReceivePlayer, this.PassYardage, this.Conversion);
                    }
                    else if(this.TdYardPassFrom) 
                    {
                        //(11:36 - 1st) Julio Jones 16 Yd pass from Matt Ryan (Matt Bryant Kick)
                        play.AppendFormat("{0} {1} Yd pass from {2} {3}", this.ReceivePlayer, this.PassYardage.Yards, this.PassPlayer, this.Conversion);
                    } 
                    else 
                    {
                        //(2:13 - 4th) Randall Cobb Pass From Aaron Rodgers for 75 Yrds M.Crosby extra point is GOOD, Center-H.Bradley, Holder-J.Scott.
                        if (this.PassYardage.Yards!=1)
                            play.AppendFormat("{0} Pass From {1} for {2} Yrds {3}", this.ReceivePlayer, this.PassPlayer, this.PassYardage.Yards, this.Conversion);
                        else
                            play.AppendFormat("{0} Pass From {1} for {2} Yard {3}", this.ReceivePlayer, this.PassPlayer, this.PassYardage.Yards, this.Conversion);
                    }
                }
                else
                {
                    __miss++;
                    Console.WriteLine("SKIPPING " + this.Play);
                }

                if(this.IsPass || this.IsRun)
                {
                    this.ValidatedAgainst = play.ToString();
                    if (!this.IsReconciled)
                    {
                        foreach (var item in this.Play.Split(' ').Except(play.ToString().Split(' ')))
                            if (__perato.ContainsKey(item))
                                __perato[item]++;
                            else
                                __perato.Add(item, 1);
                        Console.WriteLine(this.Play.Replace(".","").Replace(",","").Replace(".","").Replace("  "," "));
                        Console.WriteLine(play.ToString().Replace(".","").Replace(",","").Replace(".","").Replace("  "," "));
                        Console.WriteLine(play.ToString());
                        Console.WriteLine("-------------------------------------");
                        __miss++;
                    }
                    else
                        this.IsValidated = true;
                }
            }
            else if(this.IsRun || this.IsPass || this.IsSacked || this.IsKick|| this.IsPunt)
            {
                StringBuilder play = new StringBuilder();
                play.AppendFormat("({0:0}:{1:00} - {2})  ", this.Time.TimeLeft.Minutes, this.Time.TimeLeft.Seconds, this.Time.Qtr == 1 ? "1st" : this.Time.Qtr == 2 ? "2nd" : this.Time.Qtr == 3 ? "3rd" : this.Time.Qtr == 4 ? "4th" : "OT");
                if (!string.IsNullOrWhiteSpace(this.ReportedEligible))
                    play.AppendFormat("{0} ", this.ReportedEligible);
                if (!string.IsNullOrWhiteSpace(this.Formation))
                    play.AppendFormat("{0} ", this.Formation);


                if (this.IsSacked)
                {
                    //"(0:21 - 2nd)  (Shotgun) ASmith at ARZ 4 for no gain (P.Peterson)."
                    if(this.IsOb)
                        play.AppendFormat("{0} sacked ob at {1} for {2}", this.PassPlayer, this.PassStop, this.PassYardage.Yards == 0 ? "0 yards" : this.PassYardage.ToString());
                    else
                        play.AppendFormat("{0} sacked at {1} for {2}", this.PassPlayer, this.PassStop, this.PassYardage.Yards == 0 ? "0 yards" : this.PassYardage.ToString());


                    if (!string.IsNullOrWhiteSpace(this.TackledByPlayer))
                        play.AppendFormat(" {0}", this.TackledByPlayer);
                }
                else if (this.IsPass) 
                {
                    //(13:47 - 1st)  (Shotgun) A.Smith pass short right to J.Reed to WAS 44 for 16 yards (B.Baker). PENALTY on ARZ-T.Boston, Unnecessary Roughness, 15 yards, enforced at WAS 44.
                    //(13:47 - 1st)  A.Smith pass incomplete short right to C.Thompson.

                    play.AppendFormat("{0} pass", this.PassPlayer);
                    if (this.IsIncomplete)
                        play.Append(" incomplete");
                    play.AppendFormat(" {0} {1}", this.PassDepth, this.PassDirection);
                    if (!string.IsNullOrWhiteSpace(this.ReceivePlayer))
                        play.AppendFormat(" to {0}", this.ReceivePlayer);
                    if (!this.PassStop.IsEmpty)
                        if(this.IsOb)
                            play.AppendFormat(" {0} ob at {1}", this.Ob, this.PassStop);
                        else
                            play.AppendFormat(" to {0}", this.PassStop);
                    if (!this.IsIncomplete)
                        play.AppendFormat(" for {0}", this.PassYardage);
                    if (!string.IsNullOrWhiteSpace(this.TackledByPlayer))
                        play.AppendFormat(" {0}", this.TackledByPlayer);
                    //play.Append(".");
                    
                }
                else if (this.IsRun)
                {
                    //(13:47 - 1st)  A.Peterson right tackle to ARZ 35 for 6 yards (B.Baker).
                    play.AppendFormat("{0} {1}", this.RunPlayer, this.RunDirection);

                    if (!string.IsNullOrWhiteSpace(this.RunFormation))
                        play.AppendFormat(" {0}", this.RunFormation);
                    else if (this.RunDirection == "up")
                        play.Append(" the middle");

                    if (!this.RunStop.IsEmpty)
                        if (this.IsOb)
                            play.AppendFormat(" {0} ob at {1}", this.Ob, this.RunStop);
                        else
                            play.AppendFormat(" to {0}", this.RunStop);
                    //play.AppendFormat(" to {0}", this.RunStop);
                    play.AppendFormat(" for {0}", this.RunYardage);
                    //play.Append(".");

                    if (!string.IsNullOrWhiteSpace(this.TackledByPlayer))
                        play.AppendFormat(" {0}", this.TackledByPlayer);
                }
                else if(this.IsKick)
                {
                    //(15:00 - 1st) P.Dawson kicks 66 yards from ARZ 35 to WAS -1. D.Johnson to WAS 23 for 24 yards (B.Benwikere).
                    if (!this.IsOnsideKick)
                        play.AppendFormat("{0} kicks {1} from {2} to {3}", this.KickPlayer, this.KickYards, this.KickStart, this.KickLand);
                    else
                        play.AppendFormat("{0} kicks onside {1} from {2} to {3}", this.KickPlayer, this.KickYards, this.KickStart, this.KickLand);

                    if (!string.IsNullOrWhiteSpace(this.KickReturnPlayer))
                        play.AppendFormat(" {0}", this.KickReturnPlayer);
                    if(this.IsKickTouchback)
                        play.AppendFormat(" Touchback");
                    //"(5:51 - 3rd) M.Crosby kicks 67 yards from GB 35 to ATL -2. M.Hall pushed ob at ATL 24 for 26 yards (W.Redmond)."
                    else if (this.IsOb)
                        play.AppendFormat(" {0} ob at {1} for {2}", this.Ob, this.KickAdvanceStop, this.KickReturn);
                    else
                        play.AppendFormat(" to {0} for {1}", this.KickAdvanceStop, this.KickReturn);

                    if (!string.IsNullOrWhiteSpace(this.TackledByPlayer))
                        play.AppendFormat(" {0}", this.TackledByPlayer);
                }
                else if(this.IsPunt)
                {
                    //(3:27 - 2nd)  A.Lee punts 56 yards to WAS 17, Center-A.Brewer, fair catch by T.Quinn. PENALTY on WAS-R.Kelley, Offensive Holding, 9 yards, enforced at WAS 17.

                    play.AppendFormat("{0} punts {1} to {2}", this.PuntPlayer, this.PuntYardage, this.PuntLanded);

                    if (!string.IsNullOrWhiteSpace(this.PuntSnapper))
                        play.AppendFormat(", {0}", this.PuntSnapper);

                    if (this.IsPuntFairCatch)
                        play.AppendFormat(" fair catch by {0}", this.PuntReceiver);
                    if (!string.IsNullOrWhiteSpace(this.PuntingTeamPlayerDowned))
                        play.AppendFormat(" downed by {0}", this.PuntingTeamPlayerDowned);
                    if(this.IsPuntOutOfBounds)
                        play.AppendFormat(" out of bounds");

                    //if (!string.IsNullOrWhiteSpace(this.TackledByPlayer))
                    //    play.AppendFormat(" {0}", this.TackledByPlayer);

                    if (this.IsPuntTouchback)
                        play.AppendFormat(", Touchback");

                    if (!string.IsNullOrWhiteSpace(this.PuntReturningPlayer))
                    {
                        if(!this.IsOb)
                            play.AppendFormat(" {0} to {1} for {2}", this.PuntReturningPlayer, this.PuntStopped, this.PuntReturnYardage);
                        else
                            //"(8:55 - 1st)  S.Koch punts 51 yards to KC 19, Center-M.Cox. T.Hill pushed ob at KC 25 for 6 yards (C.Clark)."
                            play.AppendFormat(" {0} {1} ob at {2} for {3}", this.PuntReturningPlayer, this.Ob, this.PuntStopped, this.PuntReturnYardage);
                    }
                    if (!string.IsNullOrWhiteSpace(this.TackledByPlayer))
                        play.AppendFormat(" {0}", this.TackledByPlayer);

                    if (!play.ToString().EndsWith("."))
                        play.Append(".");

                }

                if (this.IsPenaltyCalled)
                {
                    //PENALTY on WAS-R.Kelley, Offensive Holding, 9 yards, enforced at WAS 17.
                    //Penalty on WAS-C.Roullier, Offensive Holding, declined.
                    if (this.IsPenaltyAccepted)
                        play.Append(" PENALTY");
                    else
                        play.Append(" Penalty");
                    if (!string.IsNullOrWhiteSpace(this.PenaltyPlayer))
                        play.AppendFormat(" on {0}", this.PenaltyPlayer);
                    play.AppendFormat(" {0}", this.PenaltyRule);
                    if (this.IsPenaltyAccepted)
                    {
                        play.AppendFormat(" {0} yards, enforced at {1}.", this.PenaltyYardage.Yards, this.PenaltyEnforcedAt);
                        if (this.IsPenaltyNoPlay)
                            play.AppendFormat(" - No Play.");
                    }
                    else
                        play.Append(" declined.");
                }

                this.ValidatedAgainst = play.ToString();
                if (!this.IsReconciled)
                {
                    foreach (var item in this.Play.Split(' ').Except(play.ToString().Split(' ')))
                        if (__perato.ContainsKey(item))
                            __perato[item]++;
                        else
                            __perato.Add(item, 1);
                    //Console.WriteLine(this.Play.Replace(".","").Replace(",","").Replace(".","").Replace("  "," "));
                    //Console.WriteLine(play.ToString().Replace(".","").Replace(",","").Replace(".","").Replace("  "," "));
                    //Console.WriteLine(play.ToString());
                    //Console.WriteLine("-------------------------------------");
                    __miss++;
                }
                else
                    this.IsValidated = true;
            }
            else
            {
                __miss++;
                Console.WriteLine("SKIPPING " + this.Play);
            }

        }

        static public Dictionary<string, int> __perato = new Dictionary<string, int>();
        static int __miss = 0;
        static int __all = 0;
        static public List<KeyValuePair<string, int>> __topworderrors()
        {
            return __perato.OrderByDescending<KeyValuePair<string, int>,int>(s => s.Value).ToList();
        }

        public bool IsReconciled
        {
            get
            {
                if (this.ValidatedAgainst == null)
                    return false;
                return this.Play.Replace(".", "").Replace(",", "").Replace(".", "").Replace("  ", " ").ToLower().Trim() == this.ValidatedAgainst.Replace(".", "").Replace(",", "").Replace("  ", " ").ToLower().Trim();
            }
        }

        






        
        public class MatchEventArgs : EventArgs
        {
            public MatchEventArgs(int startindex, string[] tokens)
            {
                this.StartIndex = startindex;
                this.SourceTokens = tokens;
            }
            public int StartIndex { get; private set; }
            public string[] SourceTokens { get; private set; }
        }
        public interface TokenIdent
        {
            int Next { get; set; }
            bool Handled(int index, string[] tokens);
            event EventHandler<MatchEventArgs> Match;
        }
        public class ParenToken : TokenIdent
        {
            public string Open = "(";
            public char Close = ')';
            public string[] Elements;
            public int Next { get; set; }
            public bool Handled(int index, string[] tokens)
            {
                if (tokens[index].StartsWith(this.Open))
                {
                    for(int i=index; i<tokens.Length; i++)
                        if (tokens[i].Contains(this.Close))
                        {
                            this.Elements = tokens.Skip(index).Take(i-index+1).ToArray();
                            this.Next = this.Elements.Length;
                            if (this.Match != null)
                                this.Match(this, new MatchEventArgs(index, tokens));
                            return true;
                        }
                }
                return false;
            }
            public event EventHandler<MatchEventArgs> Match;
        }
        public class PrepositionToken : TokenIdent
        {
            public string Preposition;
            public string[] PrepositionalObjects;
            public string[] ExpectedObjects;
            public bool IsExpectedRegEx = false;

            public int Next { get; set; }
            public bool Handled(int index, string[] tokens)
            {
                if (tokens[index] == Preposition && index + PrepositionalObjects.Length < tokens.Length)
                    if (this.ExpectedObjects == null || this.matchExpected(tokens.Skip(index + 1).Take(PrepositionalObjects.Length)))
                    {
                        Array.Copy(tokens, index + 1, this.PrepositionalObjects, 0, this.PrepositionalObjects.Length);
                        this.Next = 1 + PrepositionalObjects.Length;
                        if (this.Match != null)
                            this.Match(this, new MatchEventArgs(index, tokens));
                        return true;
                    }
                return false;
            }
            bool matchExpected(IEnumerable<string> tokens)
            {
                int counter = 0;
                if (!this.IsExpectedRegEx)
                {
                    foreach (var item in tokens)
                    {
                        if (this.ExpectedObjects[counter] != null && this.ExpectedObjects[counter] != item)
                            return false;
                        if (++counter > this.ExpectedObjects.Length)
                            break;
                    }
                }
                else
                    foreach (var item in tokens)
                    {
                        if (this.ExpectedObjects[counter] != null && !(new System.Text.RegularExpressions.Regex(this.ExpectedObjects[counter])).IsMatch(item))
                            return false;
                        if (++counter > this.ExpectedObjects.Length)
                            break;
                    }
                return true;
            }
            public event EventHandler<MatchEventArgs> Match;
        }
        public class SubjectVerbToken : TokenIdent
        {
            public string[] SubjectPatterns;
            public string[] VerbPatterns;

            public string[] Subject;
            public string Verb;

            public int Next { get; set; }
            public bool Handled(int index, string[] tokens)
            {
                for(int i=index; i<tokens.Length; i++)
                {
                    var found = Array.IndexOf(VerbPatterns, tokens[i]);
                    if (found >= 0)
                    {
                        this.Verb = tokens[i];
                        this.Subject = tokens.Skip(index).Take(i - index).ToArray();
                        this.Next = 1 + this.Subject.Length;
                        if (this.Match != null)
                            this.Match(this, new MatchEventArgs(i, tokens));
                        return true;
                    }
                }
                return false;
            }
            public event EventHandler<MatchEventArgs> Match;
        }
        public class AdverbToken : TokenIdent
        {
            public string[] AdverbPatterns;

            public string Adverb;

            public int Next { get; set; }
            public bool Handled(int index, string[] tokens)
            {
                var found = Array.IndexOf(AdverbPatterns, tokens[index]);
                if (found >= 0)
                {
                    this.Adverb = tokens[index];
                    this.Next = 1;
                    if (this.Match != null)
                        this.Match(this, new MatchEventArgs(index, tokens));
                    return true;
                }
                return false;
            }
            public event EventHandler<MatchEventArgs> Match;
        }
        public class RegExToken : TokenIdent
        {
            public string RegEx;

            public string Token;

            public int Next { get; set; }
            public bool Handled(int index, string[] tokens)
            {
                if ((new System.Text.RegularExpressions.Regex(this.RegEx)).IsMatch(tokens[index]))
                {
                    this.Token = tokens[index];
                    this.Next = 1;
                    if (this.Match != null)
                        this.Match(this, new MatchEventArgs(index, tokens));
                    return true;
                }
                return false;
            }
            public event EventHandler<MatchEventArgs> Match;
        }
        public class SentenceToken : TokenIdent
        {
            public bool IsStartPatternRegEx = false;
            public string StartPattern;
            public string[] Elements;

            public int Next { get; set; }
            public bool Handled(int index, string[] tokens)
            {
                var match = false;
                if (!this.IsStartPatternRegEx)
                {
                    match = tokens[index] == this.StartPattern;
                }
                else
                {
                    match = !(new System.Text.RegularExpressions.Regex(this.StartPattern)).IsMatch(tokens[index]);
                }
                if (match)
                    for (int i = index; i < tokens.Length; i++)
                        if (tokens[i].EndsWith("."))
                        {
                            this.Elements = tokens.Skip(index).Take(i - index + 1).ToArray();
                            this.Next = this.Elements.Length;
                            if (this.Match != null)
                                this.Match(this, new MatchEventArgs(index,tokens));
                            return true;
                        }
                return false;
            }
            public event EventHandler<MatchEventArgs> Match;

            public bool Contains(params string[] tokens)
            {
                for (int i = 0; i < this.Elements.Length; i++)
                    if (this.Elements.Skip(i).Take(tokens.Length).SequenceEqual(tokens))
                        return true;
                return false;
            }
        }
        public class PhraseToken : TokenIdent
        {
            public bool IsExpectedRegEx = false;
            public string[] ExpectedPattern;

            public string[] Words;

            public int Next { get; set; }
            public bool Handled(int index, string[] tokens)
            {
                if (!IsExpectedRegEx)
                {
                    for (int i = 0; i < ExpectedPattern.Length; i++)
                        if (i + index < tokens.Length)
                        {
                            if (this.ExpectedPattern[i] != null && tokens[i + index] != this.ExpectedPattern[i])
                                return false;
                        }
                        else
                            return false;
                }
                else
                {
                    for (int i = 0; i < ExpectedPattern.Length; i++)
                        if (i + index < tokens.Length)
                        {
                            if (this.ExpectedPattern[i] != null && !(new System.Text.RegularExpressions.Regex(this.ExpectedPattern[i])).IsMatch(tokens[i + index]))
                                return false;
                        }
                        else
                            return false;
                }

                if(this.Words==null)
                    this.Words = new string[this.ExpectedPattern.Length];
                Array.Copy(tokens,index, this.Words, 0, this.Words.Length);
                this.Next = this.Words.Length;
                if (this.Match != null)
                    this.Match(this, new MatchEventArgs(index,tokens));

                return true;
            }

            public event EventHandler<MatchEventArgs> Match;
        }
        public class LookForToken : TokenIdent
        {
            public string[] LookFor;

            public string[] Before;
            public string[] After;

            public delegate bool exclusioncondition(string[] tokens);
            public exclusioncondition ExclusionCheck;

            public int Next { get; set; }
            public bool Handled(int index, string[] tokens)
            {
                for (int i = index; i < tokens.Length; i++)
                {
                    var found = true;
                    for (int j = 0; j < this.LookFor.Length; j++)
                        if (j + i >= tokens.Length || tokens[i + j].ToLower() != this.LookFor[j].ToLower())
                        {
                            found = false;
                            break;
                        }
                    if (found)
                    {
                        if (this.ExclusionCheck != null && this.ExclusionCheck(tokens))
                            return false;

                        this.Before = tokens.Skip(index).Take(i-index).ToArray();
                        this.After = tokens.Skip(i + 2).ToArray();
                        this.Next = tokens.Length;
                        if (this.Match != null)
                            this.Match(this, new MatchEventArgs(index, tokens));
                        return true;
                    }
                }

                return false;
            }

            public event EventHandler<MatchEventArgs> Match;
        }

        public class ParseState
        {
            public TokenIdent Identifier;
            public List<ParseState> Transitions = new List<ParseState>();
        }
        public class ParseStateNavigator
        {
            public ParseState Current;
            
            public void Parse(ref int index, string[] tokens)
            {
                if (tokens.Length > 25)
                    return; //this isn't clever enough, for 50+ items.
                while (index < tokens.Length )
                {
                    var found = false;
                    foreach (var item in this.Current.Transitions)
                        if (item.Identifier.Handled(index, tokens))
                        {
                            found = true;
                            this.Current = item;
                            index += item.Identifier.Next;
                            break;
                        }
                    if(!found)
                        index++;
                }
                //return true;
            }

            public int Error = -1;
        }
        public struct Yardage
        {
            public Yardage(string yards) : this(int.Parse(yards)) { }
            public Yardage(int yards)
            {
                this.IsSet = true;
                this.Yards = yards;
            }
            public int Yards;
            public bool IsSet;
            public int? NullYards
            {
                get
                {
                    int? result=null;
                    if (this.IsSet)
                        return this.Yards;
                    else
                        return result;
                }
            }

            public override string ToString()
            {
                return this.Yards == 0 ? "no gain" : this.Yards == 1 ? "1 yard" : this.Yards + " yards";
            }
        }
        

    }
}
