using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

using CrawlerCommon;
using CrawlerCommon.TagDef.StrictXHTML;
using CrawlerCommon.HtmlPathTest;

using FFToiletBowl;

namespace GetStats
{
    class Program
    {
        static object __CONSOLEHANDLE = new object();
        static void Main(string[] args)
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            if (month < 9)
                year--;
            if (args.Length != 0)
            {
                int.TryParse(args[0], out year);
                Console.WriteLine("User set Year is " + args[0] + ", set to "+ year);
            }
            else
                Console.WriteLine("Default: Year is " + year);

            //the schedule is the basis of
            if (!retreiveSchedule(year))
            {
                Console.WriteLine("No records parsed for Season " + year);
                Console.WriteLine("Probably hasn't started " + year);
                return;
            }
            fillGameScore();
            fillPlayByPlay();
            fillUnparsedPlayByPlay();
            fillPlayerProfile(false);
            


            HtmlPath col1Path = getTestForStatsColumn1();
            var group1Path = getTestForStatsGroup1();
            var group2Path = getTestForStatsGroup2();

            var rowPath = new HtmlPath();
            rowPath.SetTest(new ElementTest<Closure>() { getTestForParentIsPlayerRows() });
            HtmlPath idPath = getTestForPlayerID();
            HtmlPath namePath = getTestForPlayerName();
            HtmlPath teamPath = getTestForColumn(1);
            HtmlPath gamesPath = getTestForColumn(2);

            HtmlPath col2Path = getTestForColumn(2);
            HtmlPath col3Path = getTestForColumn(3);
            HtmlPath col4Path = getTestForColumn(4);
            HtmlPath col5Path = getTestForColumn(5);
            HtmlPath col6Path = getTestForColumn(6);
            HtmlPath col7Path = getTestForColumn(7);
            HtmlPath col8Path = getTestForColumn(8);
            HtmlPath col9Path = getTestForColumn(9);
            HtmlPath col10Path = getTestForColumn(10);
            HtmlPath col11Path = getTestForColumn(11);
            HtmlPath col12Path = getTestForColumn(12);
            HtmlPath col13Path = getTestForColumn(14);
            HtmlPath nextPath = getTestForNextPage();


            //DataTable table = new DataTable();
            string col1 = null; 
            string type1 = null;
            string type2 = null;
            int tmp;
            double tmpd;
            float ftmp;
            List<StatLine> list = new List<StatLine>();
            List<Loaded> loaded = GameStatsPerPlayer.GetLoaded();

            //load all weeks in schedule, in past, that don't have stats
            //otherwise load 
            if (loaded.Count == 0)
                loaded = Enumerable.Range(1, 17).Select(s => new Loaded() { Year = year, Gm=s, Count=0 }).ToList();

            //2000-->
            foreach (var missing in loaded.Where(s => s.Count == 0))
            {
                var y = missing.Year;
                var w = missing.Gm;
                Console.WriteLine("Clearing old statistics for {0}, Week {1}", y, w);
                GameStatsPerPlayer.ClearStatsFor(y, w);

                //for (int y = year; y <= year; y++)
                //for (int w = 0; w < 18; w++)
                foreach (pos j in Enum.GetValues(typeof(pos)))
                //if (loaded.Count<Loaded>(s => s.Year == y && s.Gm == w) == 0)
                {

                    int pageID = 1;
                    bool isMore = true;
                    string url = "https://www.fftoday.com/stats/playerstats.php?Season=" + y + "&GameWeek=" + w + "&PosID=" + ((int)j).ToString();

                    while (isMore)
                    {
                        isMore = false;

                        Page page = new Page();
                        page.DebugMode = true;
                        page.TrimWhitespace = true;
                        page.Load(url);

                        StatLine data = new StatLine();
                        Console.WriteLine("------------------------------------------------------------");
                        var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
                        int k = 0;
                        foreach (var item in page.Tokens.BuildTree(builder))
                        {
                            /*
                            Console.Write(k++);
                            Console.Write("......");
                            Console.Write(item);
                            Console.Write("......");
                            Console.WriteLine(item.ActualSymbol);
                            */

                            if (col1Path.IsMatch(item)) col1 = ((Td)item).Attrib.Single<TagAttribute>(s => s.Name.ToLower() == "colspan").Value;
                            if (group1Path.IsMatch(item)) type1 = item.ActualSymbol;
                            if (group2Path.IsMatch(item)) type2 = item.ActualSymbol;

                            data.Gm = w;
                            data.Year = y;
                            data.Pos = j.ToString();
                            if (idPath.IsMatch(item)) data.PlayerID = ((Hyperlink)item).Href;
                            if (namePath.IsMatch(item)) data.Player = item.ActualSymbol;
                            if (teamPath.IsMatch(item)) data.Team = item.ActualSymbol;
                            switch (j)
                            {
                                case pos.QB:
                                    if (col3Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.PaComp = tmp;
                                    if (col4Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.PaAtt = tmp;
                                    if (col5Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.PaYd = tmp;
                                    if (col6Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.PaTD = tmp;
                                    if (col7Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.PaINT = tmp;
                                    if (col8Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.RuAtt = tmp;
                                    if (col9Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.RuYd = tmp;
                                    if (col10Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.RuTD = tmp;
                                    break;
                                case pos.RB:
                                    if (col3Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.RuAtt = tmp;
                                    if (col4Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.RuYd = tmp;
                                    if (col5Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.RuTD = tmp;
                                    if (col6Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReTgt = tmp;
                                    if (col7Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReRec = tmp;
                                    if (col8Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReYd = tmp;
                                    if (col9Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReTD = tmp;
                                    break;
                                case pos.WR:
                                    if (col3Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReTgt = tmp;
                                    if (col4Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReRec = tmp;
                                    if (col5Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReYd = tmp;
                                    if (col6Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReTD = tmp;
                                    if (col7Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.RuAtt = tmp;
                                    if (col8Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.RuYd = tmp;
                                    if (col9Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.RuTD = tmp;
                                    break;
                                case pos.TE:
                                    if (col3Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReTgt = tmp;
                                    if (col4Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReRec = tmp;
                                    if (col5Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReYd = tmp;
                                    if (col6Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.ReTD = tmp;
                                    break;
                                case pos.K:
                                    if (col3Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.KiFGM = tmp;
                                    if (col4Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.KiFGA = tmp;
                                    if (col5Path.IsMatch(item) && float.TryParse(item.ActualSymbol, out ftmp)) data.KiFGP = ftmp;
                                    if (col6Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.KiEPM = tmp;
                                    if (col7Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.KiEPA = tmp;
                                    break;
                                case pos.DST:
                                    if (col2Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.DSack = tmp;
                                    if (col3Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.DFR = tmp;
                                    if (col4Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.DINT = tmp;
                                    if (col5Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.DTD = tmp;
                                    if (col6Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.DPA = tmp;
                                    if (col7Path.IsMatch(item)) if (int.TryParse(item.ActualSymbol, out tmp)) data.DPaYd = tmp; else if (double.TryParse(item.ActualSymbol, out tmpd)) data.DPaYd = (int)tmpd;
                                    if (col8Path.IsMatch(item)) if (int.TryParse(item.ActualSymbol, out tmp)) data.DRuYd = tmp; else if (double.TryParse(item.ActualSymbol, out tmpd)) data.DRuYd = (int)tmpd;
                                    if (col9Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.DSafety = tmp;
                                    if (col10Path.IsMatch(item) && int.TryParse(item.ActualSymbol, out tmp)) data.DKickTD = tmp;
                                    if (data.Player != null)
                                        data.Team = data.Player.Length > 5 ? data.Player.Substring(0, 5) : data.Player;
                                    break;
                            }

                            if (rowPath.IsMatch(item))
                            {
                                Console.WriteLine(String.Format("{0:20} | {1:4} | {2:3} | {3} | {4} | {5}",
                                    data.Player, data.Team, data.Gm, data.Pos, col1, type1, type2));
                                list.Add(data);
                                data = new StatLine();
                            }

                            if (nextPath.IsMatch(item))
                            {
                                isMore = true;
                                Console.WriteLine("Next Page detected...");
                            }
                        }

                        //Console.ReadLine();
                        GameStatsPerPlayer.SetData(list);
                        list = new List<StatLine>();

                        //url = "http://www.fftoday.com/stats/playerstats.php?Season=2015&GameWeek=1&PosID=30"
                        url = "https://www.fftoday.com/stats/playerstats.php?Season=" + y + "&GameWeek=" + w  + "&PosID=" + ((int)j).ToString() + "&LeagueID=1&order_by=FFPts&sort_order=DESC&cur_page=" + pageID;
                        pageID++;
                    }
                }

                // retreiveInjuredReserve(w, y); sporttrack now requires login for previous year data
                retreiveInjuryReport(w,y);
            }
            

            

            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Creating Reports...");
            GameStatsPerPlayer.CreateReports();

            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Finished!");

            
        }

        enum pos { QB = 10, RB = 20, WR = 30, TE = 40, K = 80, DST = 99 }



        #region Injury Report
        static void retreiveInjuryReport(int wk, int yr)
        {

            /*
             * https://www.fftoday.com/nfl/18_injury_wk2.html is 2018, week 2
             * FFToday has injury report, but it's hand-made.  The format changes from week to week, season to season.
             */

            string url = string.Format("https://www.nfl.com/injuries/league/{1}/reg{0}",wk,yr);
            Console.WriteLine("Getting Daily Injury Report from {0}...", url);
            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            page.Load(url);
            /* Every site has different abbreviations for each player and team.  I expect NFL.com has same issue and reconciling players to FFToday's standard is always going to be an issue
             * 
                https://www.nfl.com/injuries/league/2020/reg1
                /HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD/A/LITERAL player
                /HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD/A url
             * /HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD[1]/LITERAL pos
                /HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD[2]/LITERAL injury_location
                /HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD[3]/LITERAL practice_status
                /HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD[4]/LITERAL game_status
                /HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR end of record
             * 
                /HTML/BODY/DIV///DIV/DIV/DIV//DIV[1]/DIV/SPAN/LITERAL home 
                /HTML/BODY/DIV///DIV/DIV/DIV//DIV[3]/DIV/SPAN/LITERAL away
                /HTML/BODY/DIV///DIV/DIV/DIV//DIV/DIV/DIV/P/SPAN/LITERAL abbr as they appear
             * 
            */
            DateTime loaddate = DateTime.Now;
            string loadid = Guid.NewGuid().ToString();
            var injuredTmPath1 = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV[1]/DIV/SPAN/LITERAL");  //getTestForInjuryReportTeam();
            var injuredTmPath2 = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV[3]/DIV/SPAN/LITERAL");  //getTestForInjuryReportTeam();
            var injuredTmAbbrPath = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV/DIV/DIV/P/SPAN/LITERAL");  //getTestForInjuryReportTeam();
            var injuredPlyrPath = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD/A/LITERAL");  //getTestForInjuryReportPlayer();
            var injuredPlyrUrlPath = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD/A");  //getTestForInjuryReportPlayerURL();
            var injuredPosPath = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD[1]/LITERAL");  //getTestForInjuryReportPos();
            var injuredStatusPath = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD[4]/LITERAL"); //getTestForInjuryReportStatus();
            var injuredPracticePath = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD[3]/LITERAL"); //getTestForInjuryReportStatus();
            var injuredLocationPath = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/TD[2]/LITERAL"); //getTestForInjuryReportStatus();
            var injuredReportDate = DateTime.Now; //getTestForInjuryReportDate();
            var injuredEORPath = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV//DIV/TABLE/TBody/TR/CLOSURE"); 

            string dateformat = "MMM d"; //Oct 17
            string lastteam = null;
            string lastteamfull = null;
            List<string> tmabbr = new List<string>();
            int teamindex = -1;
            DailyInjuryReport injured = new DailyInjuryReport() { Year = yr, Gm = wk };
            Console.WriteLine("------------------------------------------------------------");
            List<DailyInjuryReport> list = new List<DailyInjuryReport>();
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.Tokens.BuildTree(builder))
            {
                if (injuredTmAbbrPath.IsMatch(item))
                    tmabbr.Add(item.ActualSymbol);
                if (injuredPlyrUrlPath.IsMatch(item))
                {
                    injured.EspnPlayerURL = ((Hyperlink)item).Href;
                    if (injured.EspnPlayerURL.Length > 36)
                    {
                        //https://www.nfl.com/players/lonnie-johnson/
                        var endofid = injured.EspnPlayerURL.IndexOf("/", 28);
                        if (endofid > 28)
                            injured.EspnPlayerID = injured.EspnPlayerURL.Substring(28, endofid - 28);
                    }
                }
                if (injuredPlyrPath.IsMatch(item))
                    injured.Player = item.ActualSymbol;
                if (injuredPosPath.IsMatch(item))
                    injured.Pos = item.ActualSymbol;
                if (injuredTmPath1.IsMatch(item) || injuredTmPath2.IsMatch(item))
                {
                    teamindex++;
                    lastteam = tmabbr[teamindex];
                    lastteamfull = item.ActualSymbol;
                }
                if (injuredStatusPath.IsMatch(item))
                    injured.Status = item.ActualSymbol;
                if (injuredPracticePath.IsMatch(item))
                    injured.Notes =item.ActualSymbol;
                if (injuredLocationPath.IsMatch(item))
                    injured.Injury = item.ActualSymbol;
                //end of record, too
                //if (injuredDatePath.IsMatch(item))
                //{
                //    DateTime casted = default(DateTime);
                //    if (!DateTime.TryParseExact(item.ActualSymbol.Trim(), dateformat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out casted))
                //        throw new ArgumentException(item.ActualSymbol + " not recognized as " + dateformat);
                //   injured.ReportDate = casted;
                //    injured.EspnTeam = lastteam;
                //    injured.Team = InjuredReserve.TeamAbbrFor(lastteam);
                //    injured.LoadID = loadid;
                //    injured.LoadDate = loaddate;
                //}
                if (injuredEORPath.IsMatch(item))
                {
                    injured.ReportDate = injuredReportDate;
                    injured.EspnTeam = lastteamfull;
                    injured.Team = InjuredReserve.TeamAbbrFor(lastteam);
                    injured.Source = "NFL.com";
                    injured.LoadID = loadid;
                    injured.LoadDate = loaddate;
                    //injured.Status = injured.Status.Trim();
                    Console.WriteLine("{0} | {1} | {2} | {3} | {4} | {5}",
                        injured.Player, injured.Pos, injured.Team, injured.Injury , injured.Status, injured.Notes);
                    list.Add(injured);
                    injured = new DailyInjuryReport() { Year=yr, Gm = wk };
                }
            }

            Console.WriteLine("Saving Daily Injury Report...");
            InjuredReserve.SetInjuryReport(list);

            /*
            string url = "http://www.espn.com/nfl/injuries";
            Console.WriteLine("Getting Daily Injury Report from {0}...", url);
            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            page.Load(url);
            
             * ESPN injury report only shows current week, never historical... So it's good for timely information, bad for testing predictive value of a model based on historical performance
            TABLE[@class="tablehead"]
            /HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR[@class="stathead"]/TD[@colspan="3"]/LITERAL
            /HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR/TD[0]/A/LITERAL
            /HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR/TD[0]/A/@HREF
            /HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR/TD[0]/LITERAL
            /HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR/TD[1]/LITERAL
            /HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR/TD[2]/LITERAL
            /HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR[@class CONTAINS "row "]/TD[2]/CLOSURE
            

            DateTime loaddate = DateTime.Now;
            string loadid = Guid.NewGuid().ToString();
            var injuredTmPath = getTestForInjuryReportTeam();
            var injuredPlyrPath = getTestForInjuryReportPlayer();
            var injuredPlyrUrlPath = getTestForInjuryReportPlayerURL();
            var injuredPosPath = getTestForInjuryReportPos();
            var injuredStatusPath = getTestForInjuryReportStatus();
            var injuredDatePath = getTestForInjuryReportDate();

            string dateformat = "MMM d"; //Oct 17
            string lastteam = null;
            DailyInjuryReport injured = new DailyInjuryReport();
            Console.WriteLine("------------------------------------------------------------");
            List<DailyInjuryReport> list = new List<DailyInjuryReport>();
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.Tokens.BuildTree(builder))
            {
                if (injuredPlyrUrlPath.IsMatch(item))
                {
                    injured.EspnPlayerURL = ((Hyperlink)item).Href;
                    if (injured.EspnPlayerURL.Length > 36)
                    {
                        //http://www.espn.com/nfl/player/_/id/16172/jaron-brown
                        var endofid = injured.EspnPlayerURL.IndexOf("/", 36);
                        if(endofid>36)
                            injured.EspnPlayerID = injured.EspnPlayerURL.Substring(36, endofid - 36);
                    }
                }
                if (injuredPlyrPath.IsMatch(item))
                    injured.Player = item.ActualSymbol;
                if (injuredPosPath.IsMatch(item))
                    injured.Pos = item.ActualSymbol.Substring(1).Trim();
                if (injuredTmPath.IsMatch(item))
                    lastteam = item.ActualSymbol;
                if (injuredStatusPath.IsMatch(item))
                    injured.Status = item.ActualSymbol;
                //end of record, too
                if (injuredDatePath.IsMatch(item))
                {
                    DateTime casted = default(DateTime);
                    if (!DateTime.TryParseExact(item.ActualSymbol.Trim(), dateformat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out casted))
                        throw new ArgumentException(item.ActualSymbol + " not recognized as " + dateformat);
                    injured.ReportDate = casted;
                    injured.EspnTeam = lastteam;
                    injured.Team = InjuredReserve.TeamAbbrFor(lastteam);
                    injured.LoadID = loadid;
                    injured.LoadDate = loaddate;
                    
                    Console.WriteLine("{0} | {1} | {2} | {3}",
                        injured.Player, injured.Pos, injured.Team, injured.Status);
                    list.Add(injured);
                    injured = new DailyInjuryReport();
                }
            }

            Console.WriteLine("Saving Daily Injury Report...");
            InjuredReserve.SetInjuryReport(list);
             * 
             */
        }

        //HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR[@class="stathead"]/TD[@colspan="3"]/LITERAL
        static private HtmlPath getTestForInjuryReportTeam()
        {
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="COLSPAN", Operator=0, CompareValue="3" },
                                    new ParentTest() {
                                        new ElementTest<Tr>() {
                                            new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="stathead" },
                                            new ParentTest() {
                                                new CacheTestResult(
                                                    getTestForInjuryReportParent()
                                                )
                                            }
                                        }
                                    }
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        //HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR/TD[0]/A/LITERAL
        static private HtmlPath getTestForInjuryReportPlayer()
        {
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Hyperlink>() {
                                    new ParentTest() {
                                        new ElementTest<Td>() {
                                            new ElementPositionTest<Td>() { CompareValue=0 },
                                            new CacheTestResult(
                                                new ParentTest() {
                                                    new ElementTest<Tr>() {
                                                        new ParentTest() {
                                                            getTestForInjuryReportParent()
                                                        }
                                                    }
                                                }
                                            )
                                        }
                                    }
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        //HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR/TD[0]/A/@HREF
        static private HtmlPath getTestForInjuryReportPlayerURL()
        {
            ITokenTest top =    new ElementTest<Hyperlink>() {
                                    new ParentTest() {
                                        new ElementTest<Td>() {
                                            new ElementPositionTest<Td>() { CompareValue=0 },
                                            new CacheTestResult(
                                                new ParentTest() {
                                                    new ElementTest<Tr>() {
                                                        new ParentTest() {
                                                            getTestForInjuryReportParent()
                                                        }
                                                    }
                                                }
                                            )
                                        }
                                    }
                                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        //HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR/TD[0]/LITERAL
        static private HtmlPath getTestForInjuryReportPos()
        {
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    new ElementPositionTest<Td>() { CompareValue=0 },
                                    new CacheTestResult(
                                        new ParentTest() {
                                            new ElementTest<Tr>() {
                                                new ParentTest() {
                                                    getTestForInjuryReportParent()
                                                }
                                            }
                                        }
                                    )
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        //HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR/TD[1]/LITERAL
        static private HtmlPath getTestForInjuryReportStatus()
        {
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    new ElementPositionTest<Td>() { CompareValue=1 },
                                    new CacheTestResult(
                                        new ParentTest() {
                                            new ElementTest<Tr>() {
                                                new ParentTest() {
                                                    getTestForInjuryReportParent()
                                                }
                                            }
                                        }
                                    )
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        //HTML/BODY/DIV/DIV/DIV/DIV/DIV/DIV/DIV/DIV/TABLE/TR[@class CONTAINS "row"]/TD[2]/LITERAL
        static private HtmlPath getTestForInjuryReportDate()
        {
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    new ElementPositionTest<Td>() { CompareValue=2 },
                                    new CacheTestResult(
                                        new ParentTest() {
                                            new ElementTest<Tr>() {
                                                new AttribContainsTest() { AttributeName="CLASS", CompareValue="row " },
                                                new ParentTest() {
                                                    getTestForInjuryReportParent()
                                                }
                                            }
                                        }
                                    )
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }

        // TABLE[@class="tablehead"]
        static private ITokenTest getTestForInjuryReportParent()
        {
            ITokenTest test = new ElementTest<TableTag>() {
                                new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="tablehead" },
                            };

            return test;
        }


        #endregion Injury Report



        #region Schedule Grid
        static bool retreiveSchedule(int yr)
        {
            if (ScheduleNFL.IsScheduleFilled(yr))
            {
                Console.WriteLine("Schedule exists for {0}!  Skipping...", yr);
                System.Threading.Thread.Sleep(1000);
                return true;
            }
            var numweeks=18;
            if (yr < 2021)
                numweeks = 17;

            string SCHEDULE_GRID_URL = "http://espn.go.com/nfl/schedulegrid/_/year/{0}"; //this is still HTTP
            string url = string.Format(SCHEDULE_GRID_URL, yr);
            Console.WriteLine("Getting Schedule from {0}...", url);
            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            page.Load(url);


            var teamSchedulePath = getTestForScheduledTeam();
            HtmlPath[] versusPath = new HtmlPath[numweeks];
            for (int i = 0; i < numweeks; i++)
                versusPath[i] = getTestForScheduledVersus(i);
            var teamEndPath = getTestForScheduleRowEnd();

            string team = null;
            TeamSchedule game = new TeamSchedule();
            Console.WriteLine("------------------------------------------------------------");
            List<TeamSchedule> list = new List<TeamSchedule>();
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.Tokens.BuildTree(builder))
            {
                game.Year = yr;
                if (teamSchedulePath.IsMatch(item))
                    team = item.ActualSymbol;

                for (int i = 0; i < numweeks; i++)
                    if (versusPath[i].IsMatch(item))
                    {
                        game.Team = team;
                        game.Versus = item.ActualSymbol;
                        game.Wk = i + 1;
                        game.Away = game.Versus.StartsWith("@");
                        if (game.Away) game.Versus = game.Versus.Substring(1);

                        //FFToday's Abbr
                        if (game.Team == "WSH") game.Team = "WAS";
                        if (game.Versus == "WSH") game.Versus = "WAS";
                        if (game.Team == "JAX") game.Team = "JAC";
                        if (game.Versus == "JAX") game.Versus = "JAC";

                        list.Add(game);
                        Console.WriteLine("{0} | {1} | {2} | {3} | {4}", game.Year, game.Wk, game.Team, game.Versus, game.Away);
                        game = new TeamSchedule();
                    }

                /* record is per versus, not row
                if (teamEndPath.IsMatch(item))
                {
                    game.Away = game.Versus.StartsWith("@");
                    if (game.Away) game.Versus = game.Versus.Substring(1);
                    list.Add(game);
                    Console.WriteLine("{0} | {1} | {2} | {3} | {4}", game.Year, game.Wk, game.Team, game.Versus, game.Away);
                    game = new TeamSchedule();
                }*/
            }
            if (yr == 2001) //SD doesn't have BYE week on week 17 recorded on ESPN's site
            {
                list.Add(new TeamSchedule() { Team="LAC", Versus="BYE", Away=false, Wk=17, Year=2001 });
            }
            else if (yr == 2014) //NYJ and BUF, starting week 12, week 12 is wrong, and pushed out the schedule a week for both of them
            {
                list.RemoveAll(s=>(s.Team=="NYJ" || s.Team=="BUF")&&(s.Wk==12));
                foreach (var item in list.Where(s => (s.Team == "NYJ" || s.Team == "BUF") && (s.Wk > 12)))
                    item.Wk--;
                list.Add(new TeamSchedule() { Team = "NYJ", Versus = "MIA", Away = true, Wk = 17, Year = 2014 });
                list.Add(new TeamSchedule() { Team = "BUF", Versus = "NE", Away = true, Wk = 17, Year = 2014 });
            }
            else if (yr == 2017) //in 2017, mia and TB game 1, was postponed to week 11, though for some reason, the schedule shows the old game still, AND the new game, so week1 is effectively a bye
            {
                list.RemoveAll(s => (s.Team == "MIA" || s.Team == "TB") && (s.Wk == 1));
                list.Add(new TeamSchedule() { Team = "MIA", Versus = "BYE", Away = false, Wk = 1, Year = 2017 });
                list.Add(new TeamSchedule() { Team = "TB", Versus = "BYE", Away = false, Wk = 1, Year = 2017 });
            }

            if (list.Count == 0)
                return false;

            Console.WriteLine("Saving schedule...");
            ScheduleNFL.SetSchedule(list);

            return true;
        }

        static private HtmlPath getTestForScheduledVersus(int wk)
        {
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new OrTest {
                                    new ElementTest<Hyperlink>() {
                                        new ParentTest() {
                                            new ElementTest<Td>() {
                                                getTestForScheduleRow(),
                                                new ElementPositionTest<Td>() { CompareValue=wk+1 }
                                            }
                                        }
                                    },
                                    new ElementTest<Td>() {
                                        getTestForScheduleRow(),
                                        new ElementPositionTest<Td>() { CompareValue=wk+1 }
                                    }
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForScheduledTeam()
        {
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Hyperlink>() {
                                    new ParentTest() {
                                        new ElementTest<Bold>() {
                                            new ParentTest() {
                                                new ElementTest<Td>() {
                                                    getTestForScheduleRow(),
                                                    new ElementPositionTest<Td>() { CompareValue=0 }
                                                }
                                            }
                                        } 
                                    }
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForScheduleRowEnd()
        {
            ITokenTest top = new ElementTest<Closure>() {
                                    getTestForScheduleRow()
                            };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private ITokenTest getTestForScheduleRow()
        {
            ITokenTest top = new CacheTestResult(
                                    new ParentTest() {
                                        new ElementTest<Tr>() {
                                            new ElementPositionComparisonTest<Tr>() { Operator=1, CompareValue=1 },
                                            new ParentTest() {
                                                new ElementTest<TableTag>() {
                                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="tablehead" },
                                                    new ParentTest() {
                                                        new ElementTest<Div>() {
                                                            new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="mod-content" },
                                                        }
                                                    }
                                                }
                                            },
                                        }
                                    });

            return top;
        }

        public class TmpGameWeek
        {
            public int Year;
            public int Week;
        }
        static string displayWeekSchedule(IEnumerable<TvSchedule> schedule, List<TvSchedule> updates)
        {
            var find = updates.ToDictionary(s => s.Team + "vs" + s.Versus);
            StringBuilder sb = new StringBuilder();
            foreach (var item in schedule)
            {
                var key = item.Team + "vs" + item.Versus;
                sb.AppendLine(string.Format("{0,4} | {1,2} | {2,-3} | {3,-3} | {4,-26} | {5,-35} | {6}"
                    , item.Year
                    , item.Wk
                    , item.Team
                    , item.Versus
                    , (item.ScheduleDate.HasValue ? item.ScheduleDate.Value.ToShortDateString() : "") + (find.ContainsKey(key) ? "-->" + find[key].ScheduleDate.Value.ToShortDateString() : "")
                    , item.LoadResult + (find.ContainsKey(key) ? "-->" + find[key].LoadResult : "")
                    , (!string.IsNullOrWhiteSpace(item.PlayByPlayURL)).ToString() 
                       + (find.ContainsKey(key) ? "-->" + (!string.IsNullOrWhiteSpace(find[key].PlayByPlayURL)).ToString() : "")));
            }
            return sb.ToString();
        }
        static void fillGameScore()
        {
            //get records from schedule and loop
            List<TeamSchedule> list = new List<TeamSchedule>();
            //TvSchedule game = new TvSchedule();
            var schedule = ScheduleNFL.GetScheduleGrid().ToList();
            var missingresults = schedule.Where(s => s.Team != "BYE" && s.Versus != "BYE" && (s.LoadURL == null || s.LoadResult==null) && (!s.ScheduleDate.HasValue || s.ScheduleDate.Value.AddDays(1) < DateTime.Now) && !s.Away).OrderBy(s => s.Wk).OrderBy(s => s.Year).ToList();
            var missingresultweeks = new List<TmpGameWeek>();
            foreach(var item in missingresults)
                if(missingresultweeks.FirstOrDefault(s=>s.Year==item.Year && s.Week==item.Wk)==null)
                    missingresultweeks.Add(new TmpGameWeek() { Year = item.Year, Week=item.Wk });
            foreach (var week in missingresultweeks)
            {
                Console.WriteLine("Yr:{0}, Wk:{1}", week.Year, week.Week);
                Console.WriteLine(displayWeekSchedule(missingresults.Where(s => week.Year == s.Year && week.Week == s.Wk && !s.Away)
                    , new List<TvSchedule>()));
            }
            
            var updated = new List<TvSchedule>();
            //foreach (var item in missingresultweeks)
            var options = new System.Threading.Tasks.ParallelOptions() { MaxDegreeOfParallelism = Math.Max(1,Environment.ProcessorCount-1) };
            System.Threading.Tasks.Parallel.ForEach(missingresultweeks, options, delegate(TmpGameWeek item) 
            {
                //Console.WriteLine("Missing data detected for {0}, {1}", item.Year, item.Week);
                var weeksgames = missingresults.Where(s=>s.Year==item.Year && s.Wk==item.Week).ToList();
                if (weeksgames.Count != 0)
                {
                    fillGameScoreFromWeeklyResults(item.Year, item.Week, weeksgames);
                    var before = updated.Count;
                    lock(updated)
                        updated.AddRange(weeksgames.Where(s => s.LoadURL != null));
                    lock (__CONSOLEHANDLE)
                        Console.WriteLine("Queueing game results to be saved {0} to {1}...", before, updated.Count);
                    System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        ScheduleNFL.UpdateScheduleWithTV(weeksgames.Where(s => s.LoadURL != null).ToList());
                    }, null);

                    var output = displayWeekSchedule(
                        schedule.Where(s=>s.Year==item.Year && s.Wk==item.Week)
                        , weeksgames.Where(s => s.LoadURL != null).ToList());
                    lock (__CONSOLEHANDLE)
                        Console.WriteLine(output);
                }
            });
            



            var playbyplay = new List<PlayByPlay>();
            var missingplaybyplay = schedule.Where(s => s.LoadURL != null && s.LoadResult!=null && s.PlayByPlayURL==null).OrderBy(s => s.Wk).OrderBy(s => s.Year).ToList();
            var misingplayweek = new List<TmpGameWeek>();
            foreach (var item in missingplaybyplay)
                if (misingplayweek.FirstOrDefault(s => s.Year != item.Year || s.Week != item.Wk) == null)
                    misingplayweek.Add(new TmpGameWeek() { Year = item.Year, Week = item.Wk });
            
            //consider adding threadpool state, a AutoResetEvent, that has to be flipped inside ThreadPool delegate, and iteratively checked at end of foreach, 
            //    to bypass, in case program ends too abruptly
            int j = 0;
            var savejoin = missingplaybyplay.Select(s => new System.Threading.AutoResetEvent(true)).ToDictionary(s => missingplaybyplay[j++]);

            
            //var options2 = new System.Threading.Tasks.ParallelOptions() { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)+4 };
#if DEBUG
            foreach (var item in missingplaybyplay)
#else
            System.Threading.Tasks.Parallel.ForEach(missingplaybyplay, options, delegate(TvSchedule item)
#endif
            {
                if (item.PlayByPlayURL == null)
                {
                    // LoadURL should look like http://www.espn.com/nfl/game/_/gameId/401030710
                    var gameidstart = item.LoadURL.IndexOf("gameId/");
                    var gameid = item.LoadURL.Substring("gameId/".Length + gameidstart);
                    var pbp = getPlayByPlay(gameid, item);
                    //Console.WriteLine("fillGameScore().PlayByPlayURL {0}", item.PlayByPlayURL);

                    var join = savejoin[item];
                    join.Reset();
                    System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        try
                        {
                            var pkg = (Tuple<object,object,object,object>)state;
                            var missingplaybyplay2 = (List<TvSchedule>)pkg.Item1;
                            var item2 = (TvSchedule)pkg.Item2;
                            var pbp2 = (List<PlayByPlay>)pkg.Item3;

                            lock (missingplaybyplay2) //concurrent inserts are oddly causing deadlock issues, so we are going to queue them in the threadpool, but execute serially
                                ScheduleNFL.SavePlayByPlay(item2, pbp2);

                            var last = pbp2.Count - 1;
                            if (last >= 0)
                            {
                                var a = pbp2[last].Play.ToUpper().IndexOf("END");
                                var b = pbp2[last].Play.ToUpper().IndexOf("GAME");
                                var dist = b - a;
                                if (a >= 0 && b >= 0 && dist <= 12)
                                    ScheduleNFL.UpdateScheduleWithTV(new List<TvSchedule>() { item2 });
                            }
                        }
                        finally
                        {
                            var pkg = (Tuple<object, object, object, object>)state;
                            var join2 = (System.Threading.AutoResetEvent)pkg.Item4;
                            join2.Set();
                        }
                    }, new Tuple<object, object, object, object>(missingplaybyplay, item, pbp, join));
                    
                    if (!updated.Contains(item))
                        lock(updated)
                            updated.Add(item);
                }
            }
#if DEBUG
#else
            );
#endif
            Console.WriteLine("Completing Deferred Writes...{0}", savejoin.Count==0 ? "None" : (savejoin.Count - 1).ToString());
            int i = 0;
            foreach (var item in savejoin)
            {
                Console.Write(i++);
                Console.Write("\r");
                item.Value.WaitOne();
            }
            Console.WriteLine();

            if(updated.Count!=0) //when play by play is each saved, then save the Schedule record with updated values 
            {
                Console.WriteLine("Saving queued game results and play by play link to schedules...");
                ScheduleNFL.UpdateScheduleWithTV(updated);
            }
            
        }
        static void fillGameScoreFromWeeklyResults(int yr, int wk, IEnumerable<TvSchedule> schedulegridlist)
        {
            var start = DateTime.Now;
            long bytes = 0;

            const string WEEKLY_SCHEDULE_URL = "https://www.espn.com/nfl/schedule/_/week/{0}/year/{1}";
            string url = string.Format(WEEKLY_SCHEDULE_URL, wk, yr);
            lock(__CONSOLEHANDLE)
                Console.WriteLine("Getting Weekly Schedule Details from {0}...", url);
            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            for(int i=0; i<20; i++)
                try
                {
                    page.Load(url);
                    break;
                }
                catch (Exception ex)
                {
                    if (i == 19)
                        throw;
                    else
                    {
                        System.Threading.Thread.Sleep(500+ (Environment.TickCount%1000)); //random wait, of average of 1sec
                        lock (__CONSOLEHANDLE)
                            Console.WriteLine(url + " retry " +(i + 1));
                    }
                }

            var scheduledate        = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/H/LITERAL");
            //var hometeamhref        = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/TABLE/TBody/TR/TD[0]/A");
/*            var hometeamname        = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[0]/A/SPAN/LITERAL");
            var hometeamname2       = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[0]/SPAN/SPAN/LITERAL");
            var hometeamabbr        = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[0]/A//LITERAL"); //Actual is: /HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[0]/A/ ABBR /LITERAL
            //var awayteamhref        = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/A");
            var awayteamname        = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/A/SPAN/LITERAL");
            var awayteamname2       = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/SPAN/SPAN/LITERAL");
            var awayteamabbr        = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/A//LITERAL"); //Actual is: /HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/A/ ABBR /LITERAL
*/
            var awayteamname = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[0]/A/SPAN/LITERAL");
            var awayteamname2 = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[0]/SPAN/SPAN/LITERAL");
            var awayteamabbr = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[0]/A//LITERAL"); //Actual is: /HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[0]/A/ ABBR /LITERAL
            //var awayteamhref        = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/A");
            var hometeamname = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/A/SPAN/LITERAL");
            var hometeamname2 = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/SPAN/SPAN/LITERAL");
            var hometeamabbr = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/A//LITERAL"); //Actual is: /HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[1]/DIV/A/ ABBR /LITERAL

            var gameteamresulthref  = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[2]/A");
            var gameteamresulttext  = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIV/DIV/TABLE/TBody/TR/TD[2]/A[0]/LITERAL");
            var eof                 = new HtmlPath("/HTML/BODY/DIV///DIV//DIV/DIV/DIVDIV//TABLE/TBody/TR/CLOSURE");

            string debughome = null;
            string debugaway = null;
            string debughome2 = null;
            string debugaway2 = null;
            TvSchedule parsed = new TvSchedule() { Year = yr, Wk=wk };
            List<TvSchedule> parsedbuffer = new List<TvSchedule>();
            string gamedate = null;
            lock (__CONSOLEHANDLE)
                Console.WriteLine("------------------------------------------------------------");
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.Tokens.BuildTree(builder))
            {
                bytes += item.ActualSymbol.Length;

                if (scheduledate.IsMatch(item)) 
                {
                    var x = item.ActualSymbol;
                    gamedate = x.Substring(x.IndexOf(",")+1).Trim();
                    parsed.ScheduleDate = DateTime.ParseExact(gamedate + ", " + yr, "MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                //if (hometeamhref.IsMatch(item)) parsed.;
                if (hometeamname.IsMatch(item))
                    debughome = item.ActualSymbol;
                if (hometeamname2.IsMatch(item))
                    debughome2 = item.ActualSymbol;

                if (hometeamabbr.IsMatch(item))
                    parsed.Team = item.ActualSymbol;

                //if (awayteamhref.IsMatch(item));
                if (awayteamname.IsMatch(item))
                    debugaway = item.ActualSymbol;
                if (awayteamname2.IsMatch(item))
                    debugaway2 = item.ActualSymbol;
                
                if (awayteamabbr.IsMatch(item))
                    parsed.Versus = item.ActualSymbol;
                //if (awayteamname.IsMatch(item));
                if (gameteamresulthref.IsMatch(item))
                {
                    var href = item as Hyperlink;
                    if (href.Href.StartsWith("/nfl/game/_/gameId/"))
                        parsed.LoadURL = href.Href;
                }
                if (gameteamresulttext.IsMatch(item) && parsed.LoadURL!=null) 
                    parsed.LoadResult = item.ActualSymbol;

                if (eof.IsMatch(item))
                {
                    //fudging errors in data.  The week schedule data is missing the abbr for relocated teams, so we add the original abbr here.
                    if(string.IsNullOrEmpty(parsed.Team))
                        if (debughome2 == "San Diego" || debughome == "San Diego")
                            parsed.Team = "SD";
                        else if(debughome2=="St. Louis" || debughome=="St. Louis")
                            parsed.Team = "STL";
                        else if (debughome2 == "Oakland" || debughome == "Oakland")
                            parsed.Team = "OAK";
                        else if (debughome2 == "Washington" || debughome == "Washington")
                            parsed.Team = "WSH";
                    if(string.IsNullOrEmpty(parsed.Versus))
                        if (debugaway2 == "San Diego" || debugaway == "San Diego")
                            parsed.Versus = "SD";
                        else if(debugaway2=="St. Louis" || debugaway=="St. Louis")
                            parsed.Versus = "STL";
                        else if (debugaway2 == "Oakland" || debugaway == "Oakland")
                            parsed.Versus = "OAK";
                        else if (debugaway2 == "Washington" || debugaway == "Washington")
                            parsed.Versus = "WSH";
                    

                    if (parsed.Team == "JAX") //using FFToday's abbr
                        parsed.Team = "JAC";
                    if (parsed.Versus == "JAX")
                        parsed.Versus = "JAC";
                    if (parsed.Team == "WSH")
                        parsed.Team = "WAS";
                    if (parsed.Versus == "WSH")
                        parsed.Versus = "WAS";

                    //accomdating Team moves, where the team abbr change seems to have carried to past seasons
                    if (parsed.Team == "LV" && parsed.Year < 2020)
                        parsed.Team = "OAK";
                    if (parsed.Versus == "LV" && parsed.Year < 2020)
                        parsed.Versus = "OAK";
                    
                    //Postponed game, ignore: https://www.espn.com/nfl/schedule/_/week/12/year/2014
                    if(parsed.LoadResult!="Postponed")
                        parsedbuffer.Add(parsed);
                    parsed = new TvSchedule() { Year = yr, Wk = wk, ScheduleDate = DateTime.ParseExact(gamedate + ", " + yr, "MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture) };
                }
            }
            var end = DateTime.Now;
            lock (__CONSOLEHANDLE)
                Console.WriteLine("Extract finished {0} kB in {1} ms", bytes/1000, (end - start).TotalMilliseconds);

            //check for missing abbr
            foreach (var item in parsedbuffer)
                if(string.IsNullOrWhiteSpace(item.Team) || string.IsNullOrWhiteSpace(item.Versus))
                {
                    var teams = item.LoadResult.Split(',');
                    if (teams.Length != 2)
                    {
                        var one = teams[0].Split(' ');
                        var two = teams[1].Split(' ');
                        var teamfound = item.Team == one[0] ? two[0] : item.Team == two[0] ? one[0] : null;
                        var versusfound = item.Versus == one[0] ? two[0] : item.Versus == two[0] ? one[0] : null;
                        if (teamfound!=null)
                        {
                            item.Versus = teamfound;
                            lock (__CONSOLEHANDLE)
                            Console.WriteLine("Wk {0} {1} cannot find versus, extracted from results ({2}), to be {3}", item.Wk, item.Team, item.LoadResult, item.Versus);
                        }
                        else if (teamfound!=null)
                        {
                            item.Team = versusfound;
                            lock (__CONSOLEHANDLE)
                            Console.WriteLine("Wk {0} {1} cannot find versus, extracted from results ({2}), to be {3}", item.Wk, item.Versus, item.LoadResult, item.Team);
                        }
                        else 
                        {
                            lock (__CONSOLEHANDLE)
                            Console.WriteLine("Wk {0} {1} vs {2}, Unable to determine all teams abbr ", item.Wk, item.Versus, item.Team);
                        }
                    }
                }

            lock (__CONSOLEHANDLE)
                Console.WriteLine("Merging {0},{1} with schedule grid...", yr,wk);
            foreach (var bufferedgameresult in parsedbuffer.ToList())
                try
                {
                    var foundonschedule = schedulegridlist.Where(s => s.Away ? (s.Team == bufferedgameresult.Versus && s.Versus == bufferedgameresult.Team) : (s.Team == bufferedgameresult.Team && s.Versus == bufferedgameresult.Versus));
                    var notfound = true;
                    //if (bufferedgame != null)
                    foreach (var item in foundonschedule)
                    {
                        item.LoadURL = bufferedgameresult.LoadURL;
                        item.ScheduleDate = bufferedgameresult.ScheduleDate;
                        item.LoadResult = bufferedgameresult.LoadResult;
                        lock (__CONSOLEHANDLE)
                            Console.WriteLine("Updated {0} Season, Wk {1}, {2} vs {3} with results {4}", yr, wk, item.Team, item.Versus, item.LoadResult);
                        parsedbuffer.Remove(bufferedgameresult);
                        if (notfound) notfound = false;
                    }
                    if (notfound)
                    {
                        Console.WriteLine("Parsed {0} Season, Wk {1}, {2}, but data was already there", yr, wk, bufferedgameresult.LoadResult);
                        //    throw new Exception("Detected unable to match season week grid data, with game results data");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(url);
                    Console.Error.WriteLine("Tried to locate {0} Season, Wk {1}, {2} vs {3}", yr, wk, bufferedgameresult.Team, bufferedgameresult.Versus, bufferedgameresult.LoadResult);
                    throw;
                }
            /*foreach (var item in schedulegridlist)
                try
                {
                    var bufferedgamelist = parsedbuffer.Where(s => item.Away ? (s.Team == item.Versus && s.Versus == item.Team) : (s.Team == item.Team && s.Versus == item.Versus));
                    var notfound = true;
                    //if (bufferedgame != null)
                    foreach(var bufferedgameresult in bufferedgamelist)
                    {
                        item.LoadURL = bufferedgameresult.LoadURL;
                        item.ScheduleDate = bufferedgameresult.ScheduleDate;
                        item.LoadResult = bufferedgameresult.LoadResult;
                        lock (__CONSOLEHANDLE)
                            Console.WriteLine("Updated {0} Season, Wk {1}, {2} vs {3} with results {4}", yr, wk, item.Team, item.Versus, item.LoadResult);
                        parsedbuffer.Remove(bufferedgameresult);
                        if (notfound) notfound = false;
                    }
                    if (notfound)
                        throw new Exception("Detected unable to match season week grid data, with game results data");
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(url);
                    Console.Error.WriteLine("Tried to locate {0} Season, Wk {1}, {2} vs {3}", yr, wk, item.Team, item.Versus, item.LoadResult);
                    throw;
                }*/
            if(parsedbuffer.Count!=0) 
            {
                var unupdated = parsedbuffer.Select(s=>s.Year + " " +s.Wk+"," + s.Team + " v " + s.Versus);
                var text = string.Join(" | ", unupdated);
                Console.Error.WriteLine("downloaded these game results, but did not located these games on the schedule, to update with results:" + text);
                Console.Error.WriteLine(url);
                Console.Error.WriteLine("May not be error.  These games may already have results stored, so they did not show up in results to retreive.  These are the games missing results:");
                foreach(var item in schedulegridlist.Where(s=>s.Year==yr && s.Wk==wk))
                    Console.Error.WriteLine("{0} {1}, {2} vs {3}", item.Year, item.Wk, item.Team, item.Versus);
                //throw new Exception("did not located these games on the schedule, to update with results:" + text);
            }
        }


        static void fillPlayByPlay()
        {
            //get records from schedule and loop
            List<TeamSchedule> list = new List<TeamSchedule>();
            //TvSchedule game = new TvSchedule();
            var schedule = ScheduleNFL.GetScheduleGrid();
            var updated = new List<TvSchedule>();
#if DEBUG
            foreach (var item in schedule.Where(s => s.LoadURL != null && s.LoadResult != null && s.PlayByPlayURL == null).OrderBy(s => s.Wk).OrderBy(s => s.Year))
#else
            var options = new System.Threading.Tasks.ParallelOptions() { MaxDegreeOfParallelism = Math.Max(1,Environment.ProcessorCount-1) };
            System.Threading.Tasks.Parallel.ForEach(schedule.Where(s => s.LoadURL != null && s.LoadResult != null && s.PlayByPlayURL == null).OrderBy(s => s.Wk).OrderBy(s => s.Year)
                , options, delegate(TvSchedule item)
#endif
            {
                // LoadURL should look like http://www.espn.com/nfl/game/_/gameId/401030710
                var gameidstart = item.LoadURL.IndexOf("gameId/");
                var gameid = item.LoadURL.Substring("gameId/".Length + gameidstart);
                var pbp = getPlayByPlay(gameid, item);
                //Console.WriteLine("PlayByPlayURL {0}", item.PlayByPlayURL);
                ScheduleNFL.SavePlayByPlay(item, pbp);
            }
#if DEBUG
#else
            );
#endif
            if (updated.Count!=0)
                ScheduleNFL.UpdateScheduleWithTV(updated);

        }
        
        static List<PlayByPlay> getPlayByPlay(string gameid, TvSchedule missing)
        {
            long bytes = 0;
            var begin = DateTime.Now;
            StringBuilder sb = new StringBuilder();

            const string PLAY_BY_PLAY_URL = "https://www.espn.com/nfl/playbyplay?gameId={0}";
            string url = string.Format(PLAY_BY_PLAY_URL, gameid); //the game id should be pulled from a URL, it should be UrlEncoded already
            lock (__CONSOLEHANDLE)
                Console.WriteLine("Getting Play by Play for from {0}...", url);
            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            for(var i=0; i<30; i++)
                try
                {
                    page.Load(url);
                    break;
                }
                catch
                {
                    if (i == 29)
                        throw;
                    else
                    {
                        System.Threading.Thread.Sleep(500);
                        lock (__CONSOLEHANDLE)
                        {
                            Console.WriteLine(url + " , retry " + (i + 1));
                        }
                    }
                }

            var posessionlogo   = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV/DIV/DIV/DIV//DIV/DIV/UL/LI/DIV/A/DIV/SPAN/IMG");
            var downdistance    = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV/DIV/DIV/DIV//DIV/DIV/UL/LI/DIV/DIV/UL/LI/H/LITERAL");
            var playdescription = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV/DIV/DIV/DIV//DIV/DIV/UL/LI/DIV/DIV/UL/LI/P/SPAN/LITERAL");
            var eor             = new HtmlPath("/HTML/BODY/DIV///DIV/DIV/DIV/DIV/DIV/DIV//DIV/DIV/UL/LI/DIV/DIV/UL/LI/CLOSURE");

            int counter = 0;
            string possession = null;
            PlayByPlay parsed = new PlayByPlay() { URL = url, Seq = counter++ };
            List<PlayByPlay> parsedbuffer = new List<PlayByPlay>();
#if DEBUG
            lock (__CONSOLEHANDLE)
                Console.WriteLine("------------------------------------------------------------");
#else
            sb.AppendLine("------------------------------------------------------------");
            sb.AppendLine(string.Format("Output Play by Play for from {0}...", url));
#endif
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.Tokens.BuildTree(builder))
            {
                bytes += item.ActualSymbol.Length;

                if (posessionlogo.IsMatch(item))
                {
                    //<img class="team-logo" src="https://a.espncdn.com/combiner/i?img=/i/teamlogos/nfl/500/sf.png&h=100&w=100"/>
                    var imgsrc = ((Img)item).Src;
                    var start = imgsrc.LastIndexOf("/");
                    var end = imgsrc.LastIndexOf(".png");
                    possession = imgsrc.Substring(start + 1, end - start - 1);
                    parsed.Possession = possession;
                }
                if (downdistance.IsMatch(item))
                    parsed.DownDistance = item.ActualSymbol;
                if (playdescription.IsMatch(item))
                    parsed.Play = item.ActualSymbol;
                if (eor.IsMatch(item))
                {
                    if (parsed.DownDistance == null)
                        parsed.DownDistance = parsed.EXTRACT_ERR_DOWN_DISTANCE;
                    if (parsed.Play == null)
                        parsed.Play = parsed.EXTRACT_ERR_PLAY;
                    if (parsed.DownDistance == null || parsed.Play == null) //worst data ever: https://www.espn.com/nfl/playbyplay?gameId=261210022
                        throw new Exception("bad parse of play by play in url");

                    parsedbuffer.Add(parsed);
#if DEBUG
                    lock (__CONSOLEHANDLE)
                        Console.WriteLine("{0,-3} {1} {2}", parsed.Seq, parsed.DownDistance, parsed.Play);
#else
                    sb.AppendLine(string.Format("{0,-3} {1} {2}", parsed.Seq, parsed.DownDistance, parsed.Play));
#endif
                    parsed = new PlayByPlay() { URL= url, Possession = possession, Seq = counter++ };
                }
            }
            var finished = DateTime.Now;
            lock (__CONSOLEHANDLE)
                Console.WriteLine("Extract finished {0} kB in {1} ms", bytes / 1000, (finished - begin).TotalMilliseconds);
#if DEBUG
#else
            var output = sb.ToString();
            lock (__CONSOLEHANDLE)
                Console.WriteLine(output);
#endif

            missing.PlayByPlayURL = url;
            return parsedbuffer;
        }

        static void fillUnparsedPlayByPlay()
        {
            //DictionaryNode tree = new DictionaryNode();
            var hr = new string('-', 70);
            var count=0;
            var parsed=0;
            var list = ScheduleNFL.GetUnparsedPlayByPlay();
            List<PlayByPlay> parsedbuffer = new List<PlayByPlay>();
            foreach (var item in list)
            {
                //you want to ensure 100% coverage of the parsing of play by plays
                //every keywords needs to be identified

                Console.WriteLine(item.Play);
                item.Parse();
                count++;
                if (item.IsValidated)
                    parsed++;
                else
                {
                    Console.WriteLine(item.ValidatedAgainst);
                    Console.WriteLine(hr);
                }
                parsedbuffer.Add(item);
            }
            var pct = 100d * parsed/ count;
            if (pct < 50)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (pct < 60)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (pct > 80)
                Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Reconciled {0}/{1} ({2:0.00}) Play-by-plays", parsed, count, pct);
            Console.ResetColor();

            Console.WriteLine("Saving Parsed out statistical relevance in play by plays... ");
            ScheduleNFL.UpdatePlayByPlay(parsedbuffer);
        }

        #endregion Schedule Grid


        #region Injured Reserve
        static void retreiveInjuredReserve(int wk, int yr)
        {
            string url = string.Format("http://www.spotrac.com/nfl/injured-reserve/cash/{0}/",yr);
            Console.WriteLine("Getting Injured Reserve from {0}...", url);
            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            page.Load(url);

            bool isInjuredStart = false;
            var injuredStartPath = getTestForInjuredPlayerSectionStart();
            var injuredIdPath = getTestForInjuredPlayerID();
            var injuredNamePath = getTestForInjuredPlayerName();
            var injuredPosPath = getTestForInjuredPlayerPos();
            var injuredTeamPath = getTestForInjuredPlayerTeam();
            var injuredCategoryPath = getTestForInjuredPlayerCategory();
            var injuredEndPath = new CrawlerCommon.HtmlPath();
            injuredEndPath.SetTest(new ElementTest<Closure>() { getTestForInjuredPlayerRow() });

            WklyInjuredReserve injured = new WklyInjuredReserve();
            Console.WriteLine("------------------------------------------------------------");
            List<WklyInjuredReserve> list = new List<WklyInjuredReserve>();
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.Tokens.BuildTree(builder))
            {
                injured.Gm = wk;
                injured.Year = yr;

                if (isInjuredStart)
                {
                    if (injuredIdPath.IsMatch(item))
                        injured.InjuredID = ((Hyperlink)item).Href;
                    if (injuredNamePath.IsMatch(item))
                        injured.Player = item.ActualSymbol;
                    if (injuredPosPath.IsMatch(item))
                        injured.Pos = item.ActualSymbol;
                    if (injuredTeamPath.IsMatch(item))
                        injured.Team = item.ActualSymbol;
                    if (injuredCategoryPath.IsMatch(item))
                        injured.Status = item.ActualSymbol;
                    if (injuredEndPath.IsMatch(item))
                    {
                        Console.WriteLine("{0} | {1} | {2} | {3}",
                            injured.Player, injured.Pos, injured.Team, injured.Status);
                        list.Add(injured);
                        injured = new WklyInjuredReserve();
                    }
                }
                else if (injuredStartPath.IsMatch(item))
                    isInjuredStart = true;
            }

            Console.WriteLine("Saving Injured Reserve...");
            InjuredReserve.SetInjuredReserve(list);
        }




        static private HtmlPath getTestForInjuredPlayerPos()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Td>() {
                            getTestForInjuredPlayerRow(),
                            new ElementPositionTest<Td>() { CompareValue=1 }
                        }
                    }
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForInjuredPlayerTeam()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Td>() {
                            getTestForInjuredPlayerRow(),
                            new ElementPositionTest<Td>() { CompareValue=2 }
                        }
                    }
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForInjuredPlayerName()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    getTestForInjuredPlayerRow(),
                                    new ElementPositionTest<Td>() { CompareValue=0 }
                                }
                            }
                        }
                    } 
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForInjuredPlayerCategory()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Span>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    getTestForInjuredPlayerRow(),
                                    new ElementPositionTest<Td>() { CompareValue=0 }
                                }
                            },
                            new ElementPositionTest<Span>() { CompareValue=1 }
                        }
                    } 
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForInjuredPlayerID()
        {
            ElementTest top =
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    getTestForInjuredPlayerRow(),
                                    new ElementPositionTest<Td>() { CompareValue=0 }
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private ITokenTest getTestForInjuredPlayerRow()
        {
            ITokenTest top = new CacheTestResult(
                                    new ParentTest() {
                                        new ElementTest<Tr>() {
                                            new ParentTest() {
                                                new ElementTest<TBody>() {
                                                    new ParentTest() {
                                                        new ElementTest<TableTag>() {
                                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="datatable" },
                                                            new ParentTest() {
                                                                new ElementTest<Div>() {
                                                                    new AttribContainsTest() { AttributeName="id", CompareValue="player-current" },
                                                                }
                                                            }
                                                        }
                                                    },
                                                }
                                            }
                                        } 
                                    });

            return top;
        }

        static private HtmlPath getTestForInjuredPlayerSectionStart()
        {
            ITokenTest top = getTestForInjuredPlayerRow();
            /*
                new ElementTest<Literal>() {
                    new ElementComparisonTest<string>() {convert = AttribComparisonTest<string>.PassThru, Operator=0, CompareValue="Reserve/Injured List By Player" },
                    new ParentTest() {
                        new ElementTest<BaseH>() {
                            new ParentTest() {
                                new ElementTest<UnknownTag>() {
                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="team-header" },
                                    new ParentTest() {
                                        new ElementTest<Div>() {
                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="teams" },
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            */

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        #endregion Injured Reserve






        static private HtmlPath getTestForNextPage()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    
                    new ParentTest() {
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Div>() {
                                },
                            }
                        }
                    },
                    new ElementComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, CompareValue="Next Page", Operator=0 },
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }

        /// <summary>
        /// getTestForParentIsPlayerRows() /Td[@CLASS="sort1"]/A/Literal
        /// </summary>
        /// <returns></returns>
        static private HtmlPath getTestForPlayerName()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="sort1" },
                                    new ElementPositionTest() { CompareValue=0 },
                                    getTestForParentIsPlayerRows()
                                },
                            }
                        }
                    } 
                };
            /*
            top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    new ParentTest() {

                                    }
                                }
                            }
                        }
                    } 
                };
            */

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForPlayerID()
        {
            ElementTest top =
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="sort1" },
                                    new ElementPositionTest() { CompareValue=0 },
                                    getTestForParentIsPlayerRows()
                                },
                            }
                        };


            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }


        /// <summary>
        /// getTestForParentIsPlayerRows() /Td[@CLASS="sort1" and position()=index]/Literal
        /// </summary>
        /// <returns></returns>
        static private HtmlPath getTestForColumn(int index)
        {

            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Td>() {
                            new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="sort1" },
                            new ElementPositionTest() { CompareValue=index },
                            getTestForParentIsPlayerRows()
                        },
                    } 
                };



            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }


        static BaseTestLinkage madeTestForParentIsPlayerRows;
        /// <summary>
        /// getTestForPlayerRows() /...
        /// </summary>
        /// <returns></returns>
        static private BaseTestLinkage getTestForParentIsPlayerRows()
        {
            if (madeTestForParentIsPlayerRows == null)
            {
                BaseTestLinkage top = new ParentTest() {
                                getTestForPlayerRows()
                };
                madeTestForParentIsPlayerRows = top;
            }


            return madeTestForParentIsPlayerRows;
        }

        static ITokenTest madeTestForPlayerRows;
        /// <summary>
        /// //TableTag/Tr[1]/Td/TableTag/Tr[position()>1]
        /// </summary>
        /// <returns></returns>
        static private ITokenTest getTestForPlayerRows()
        {
            if (madeTestForPlayerRows == null)
            {
                var eureka = new ElementTest<TableTag>(); //the top most node/tag

                ITokenTest top = 
                                new CacheTestResult(new ElementTest<Tr>() {
                                    new ElementPositionComparisonTest() { CompareValue=1, Operator=1 },
                                    new ParentTest(){
                                        new OrTest() {
                                            new ElementTest<TableTag>() {
                                                new ParentTest() {
                                                    new ElementTest<Td>() {
                                                        new ParentTest() {
                                                            new ElementTest<Tr>() {
                                                                new ParentTest() {
                                                                    eureka,
                                                                    new ElementPositionTest<TableTag>() { CompareValue=5 }
                                                                }
                                                            }
                                                        }
                                                    }
                                                } 
                                            },
                                            new ElementTest<TBody>() {
                                                new ParentTest() {
                                                    new ElementTest<TableTag>() {
                                                        new ParentTest() {
                                                            new ElementTest<Td>() {
                                                                new ParentTest() {
                                                                    new ElementTest<Tr>() {
                                                                        new ParentTest() {
                                                                            new ElementTest<TBody>() {
                                                                                new ParentTest() {
                                                                                    eureka,
                                                                                    new ElementPositionTest<TableTag>() { CompareValue=5 }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        } 
                                                    }
                                                }
                                            }
                                        }
                                    },
                                });
                            
                madeTestForPlayerRows = top;
            }

            return madeTestForPlayerRows;
        }
        /// <summary>
        /// //TableTag/Tr/Td/TableTag/Tr[@class=tablehdr]/td[1]/@COLSPAN
        /// </summary>
        /// <returns></returns>
        static private HtmlPath getTestForStatsColumn1()
        {

            var eureka = new ElementTest<TableTag>(); //the top most node/tag
            eureka.Match += new MatchHandler(delegate(object sender, MatchEventArgs<Token> e)
            {
                Console.WriteLine("Stats" + e.SelectedValue);
            });

            BaseTestLinkage top =
                        new ElementTest<Td>() {
                            new ElementPositionTest() { CompareValue=1 },
                            
                            new ParentTest(){
                                new ElementTest<Tr>() {
                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="tablehdr" },
                                    
                                    new ParentTest(){
                                        new ElementTest<TableTag>() {
                                            new ParentTest() {
                                                new ElementTest<Td>() {
                                                    new ParentTest() {
                                                        new ElementTest<Tr>() {
                                                            new ParentTest() {
                                                                eureka,
                                                            }
                                                        }
                                                    }
                                                }
                                            } 
                                        }
                                    },
                                    //new ElementPositionComparisonTest() { CompareValue=0, Operator=1 }
                                }
                            },
                        };



            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        /// <summary>
        /// //TableTag/Tr/Td/TableTag/Tr[@class=tablehdr]/td[1]
        /// </summary>
        /// <returns></returns>
        static private HtmlPath getTestForStatsGroup1()
        {

                var eureka = new ElementTest<TableTag>(); //the top most node/tag
                eureka.Match += new MatchHandler(delegate(object sender, MatchEventArgs<Token> e) {
                    Console.WriteLine("Stats"+e.SelectedValue);
                });

                BaseTestLinkage top =
                    new ElementTest<Literal>() {
                        new ParentTest(){
                        new ElementTest<Td>() {
                            new ElementPositionTest() { CompareValue=1 },
                            new ParentTest(){
                                new ElementTest<Tr>() {
                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="tablehdr" },
                                    
                                    new ParentTest(){
                                        new ElementTest<TableTag>() {
                                            new ParentTest() {
                                                new ElementTest<Td>() {
                                                    new ParentTest() {
                                                        new ElementTest<Tr>() {
                                                            new ParentTest() {
                                                                eureka,
                                                            }
                                                        }
                                                    }
                                                }
                                            } 
                                        }
                                    },
                                    //new ElementPositionComparisonTest() { CompareValue=0, Operator=1 }
                                }
                            },
                            
                        }
                        }
                    };


                HtmlPath hpath = new HtmlPath();
                hpath.SetTest(top);
                return hpath;
        }

        /// <summary>
        /// //TableTag/Tr/Td/TableTag/Tr[@class=tablehdr]/td[2]
        /// </summary>
        /// <returns></returns>
        static private HtmlPath getTestForStatsGroup2()
        {

            var eureka = new ElementTest<TableTag>(); //the top most node/tag
            eureka.Match += new MatchHandler(delegate(object sender, MatchEventArgs<Token> e)
            {
                Console.WriteLine("Stats" + e.SelectedValue);
            });

            BaseTestLinkage top =
                new ElementTest<Literal>() {
                        new ParentTest(){
                        new ElementTest<Td>() {
                            new ElementPositionTest() { CompareValue=2 },
                            new ParentTest(){
                                new ElementTest<Tr>() {
                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="tablehdr" },
                                    
                                    new ParentTest(){
                                        new ElementTest<TableTag>() {
                                            new ParentTest() {
                                                new ElementTest<Td>() {
                                                    new ParentTest() {
                                                        new ElementTest<Tr>() {
                                                            new ParentTest() {
                                                                eureka,
                                                            }
                                                        }
                                                    }
                                                }
                                            } 
                                        }
                                    },
                                    //new ElementPositionComparisonTest() { CompareValue=0, Operator=1 }
                                }
                            },
                            
                        }
                        }
                    };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }


        static void fillPlayerProfile(bool verbose)
        {
            StringBuilder sb = new StringBuilder();
            var signallist = new List<System.Threading.AutoResetEvent>();
            foreach (var item in GameStatsPerPlayer.GetAllProfile(!verbose))
                if (!item.IsSaved)
                {
                    var updated = getPlayerProfile(item);

                    var signal = new System.Threading.AutoResetEvent(true);
                    signallist.Add(signal);
                    signal.Reset();
                    System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        try
                        {
                            var obj = (Tuple<PlayerProfile, System.Threading.AutoResetEvent>)state;
                            GameStatsPerPlayer.SetPlayerProfile(obj.Item1);
                        }
                        finally
                        {
                            var obj = (Tuple<PlayerProfile, System.Threading.AutoResetEvent>)state;
                            var sw = obj.Item2;
                            sw.Set();
                        }
                    }, new Tuple<PlayerProfile, System.Threading.AutoResetEvent>(updated, signal));
                }
                else
                {
                    sb.AppendLine(string.Format("Profile [{0}] exists already!", item.Player));
                }
            foreach (var item in signallist)
                item.WaitOne();
            lock (__CONSOLEHANDLE)
                Console.WriteLine(sb.ToString());
        }

        static PlayerProfile getPlayerProfile(PlayerProfile profile)
        {
            long bytes = 0;
            var begin = DateTime.Now;
            StringBuilder sb = new StringBuilder();

            const string PLAY_BY_PLAY_URL = "https://www.fftoday.com{0}";
            string url = string.Format(PLAY_BY_PLAY_URL, profile.PlayerID);
            lock (__CONSOLEHANDLE)
                Console.WriteLine("Getting PlayerProfile for from {0}...", profile.Player == null ? url : profile.Player);
            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            for (var i = 0; i < 30; i++)
                try
                {
                    page.Load(url);
                    break;
                }
                catch
                {
                    if (i == 29)
                        throw;
                    else
                    {
                        System.Threading.Thread.Sleep(500);
                        lock (__CONSOLEHANDLE)
                        {
                            Console.WriteLine(url + " , retry " + (i + 1));
                        }
                    }
                }

            var label = new HtmlPath("/HTML/BODY/CENTER/TABLE/TR/TD/TABLE/TR/TD/STRONG/LITERAL");
            var data = new HtmlPath("/HTML/BODY/CENTER/TABLE/TR/TD/TABLE/TR/TD/LITERAL");
            var seasonstart = new HtmlPath("/HTML/BODY/CENTER/TABLE/TR/TD/TABLE/TR/TD/TABLE/TR/TD[0]/B/LITERAL");
            var season = new HtmlPath("/HTML/BODY/CENTER/TABLE/TR/TD/TABLE/TR/TD/TABLE/TR/TD[0]/LITERAL");



            //PlayerProfile profile = new PlayerProfile();
            int tmp;
            DateTime d;
            bool isseasonstart=false;

            string next = null;
#if DEBUG
            lock (__CONSOLEHANDLE)
                Console.WriteLine("------------------------------------------------------------");
#else
            sb.AppendLine("------------------------------------------------------------");
            sb.AppendLine(string.Format("Output Play by Play for from {0}...", url));
#endif
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.Tokens.BuildTree(builder))
            {
                bytes += item.ActualSymbol.Length;

                if (label.IsMatch(item))
                {
                    next = item.ActualSymbol;
                }
                if (data.IsMatch(item))
                    switch (next)
                    {
                        case "Draft:":
                            if (profile.DraftStr == null)
                            {
                                profile.DraftStr = item.ActualSymbol;
                                if (int.TryParse(profile.DraftStr.Substring(0, 4), out tmp))
                                    profile.DraftYear = tmp;
                            }
                            break;
                        case "College:":
                            profile.CollegeStr = item.ActualSymbol;
                            break;
                        case "Ht:":
                            var full = profile.HtStr = item.ActualSymbol.Replace("&rsquo;","'").Replace("&rdquo;","\"").Replace("&nbsp;",""); //5&rsquo;10&rdquo;&nbsp;&nbsp;
                            var split = full.Split('\'', '\"');
                            int tmp2;
                            if (split.Length >= 1 && int.TryParse(split[0], out tmp) && int.TryParse(split[1], out tmp2))
                                profile.HtInches = tmp*12 + tmp2;
                            break;
                        case "Wt:":
                            profile.WtStr = item.ActualSymbol;
                            if (int.TryParse(profile.WtStr, out tmp))
                                profile.WtLbs = tmp;
                            break;
                        case "DOB:":
                            profile.DOBStr = item.ActualSymbol;
                            if (DateTime.TryParseExact(profile.DOBStr, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out d)) //1997-08-07
                                profile.Dob = d;
                            break;
                        case "Age:":
                            profile.AgeStr = item.ActualSymbol;
                            if (int.TryParse(profile.AgeStr, out tmp))
                                profile.Age = tmp;
                            break;
                    }
                if (seasonstart.IsMatch(item))
                {
                    if(item.ActualSymbol == "Season")
                        isseasonstart = true;
                }
                if (isseasonstart && !profile.DraftYear.HasValue && season.IsMatch(item))
                    if (int.TryParse(item.ActualSymbol, out tmp))
                        profile.DraftYear = tmp;
            }
#if DEBUG
            lock (__CONSOLEHANDLE)
                Console.WriteLine("Url:{0}\nDrafted:{1}\nCollege:{2}\nHt:{3}\nWt:{4}\nDob:{5}\nAge:{6}", profile.PlayerID
                    , profile.DraftYear, profile.CollegeStr
                    , profile.HtInches, profile.WtLbs, profile.Dob, profile.Age);
#else
                sb.AppendLine(string.Format("Url:{0}\nDrafted:{1}\nCollege:{2}\nHt:{3}\nWt:{4}\nDob:{5}\nAge:{6}", profile.PlayerID
                    , profile.DraftYear, profile.CollegeStr
                    , profile.HtInches, profile.WtLbs, profile.Dob, profile.Age));
#endif

            var finished = DateTime.Now;
            lock (__CONSOLEHANDLE)
                Console.WriteLine("Extract finished {0} kB in {1} ms", bytes / 1000, (finished - begin).TotalMilliseconds);
#if DEBUG
#else
            var output = sb.ToString();
            lock (__CONSOLEHANDLE)
                Console.WriteLine(output);
#endif
            return profile;
        }

    }
}
