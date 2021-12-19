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

namespace GetRostersFromESPN
{
    class Program
    {
        static void Main(string[] args)
        {
            const string CITY_ISLAND_LEAGUE_ID = "51111";
            string leagueID = CITY_ISLAND_LEAGUE_ID;
            if (args.Length == 1)
            {
                if (args[0] == "-?")
                    showUsage();
                else
                {
                    leagueID = args[0];
                    retreiveRosters(leagueID);
                }
            }
            else if (args.Length > 1)
            {
                if (args[0].StartsWith("-"))
                {
                    leagueID = args[0].Substring(1);
                    foreach (string wk in args.Skip(1))
                        SeasonGamedayRoster.RetreiveGamedayRosters(leagueID, wk);
                }
                else
                {
                    foreach (string wk in args)
                        SeasonGamedayRoster.RetreiveGamedayRosters(leagueID, wk);
                }
            } 
            else
                retreiveRosters(leagueID);


            Console.WriteLine("Finished!");
        }

        static void showUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("GetRostersFromESPN ........................... Get rosters from league 51111 (City Island)");
            Console.WriteLine("GetRostersFromESPN [leagueID]................. Get rosters from league ");
            Console.WriteLine("GetRostersFromESPN [week1] [week2] [week3] ... Get rosters from league 51111, on indicated weeks");
            Console.WriteLine("                                               at least 2 weeks need to be indicated");
            Console.WriteLine("GetRostersFromESPN -[leagueID] [wk1] [wk2] ... Get rosters from league 51111, on indicated weeks");
            
            Console.WriteLine("GetRostersFromESPN 2 3 4 5 ... Get rosters from league 51111, on weeks 2,3,4,5");
            Console.WriteLine("GetRostersFromESPN -41234 2 ... Get rosters from league 41234, on weeks 2");
        }

        static void retreiveRosters(string leagueID)
        {
            string url = string.Format("http://games.espn.go.com/ffl/leaguerosters?leagueId={0}", leagueID);
            Console.WriteLine("Getting ESPN Rosters from {0}...", url);
            Page page = new Page();
            page.DebugMode = true;
            page.TrimWhitespace = true;
            page.Load(url);

            var teamNamePath = getTestForTeamName();
            var teamUrlPath = getTestForTeamURL();
            var teamRecordPath = getTestForTeamRecord();
            var teamRosterSpotPath = getTestForRosterSlot();
            var teamPlayerNamePath = getTestForPlayerName();
            var teamPlayerUrlPath = getTestForPlayerURL();
            var teamPlayerPositionPath = getTestForPlayerTeamPosition();
            var teamRosterSpotEndPath = getTestForSlotEnd();
            var teamEndPath = getTestForTeamEnd();

            TeamRoster roster = new TeamRoster();
            PlayerOnRoster player = new PlayerOnRoster();
            Console.WriteLine("------------------------------------------------------------");
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page.Tokens.BuildTree(builder))
            {
                if (teamNamePath.IsMatch(item))
                    roster.Name = item.ActualSymbol;
                if (teamUrlPath.IsMatch(item))
                    roster.TeamURL = ((Hyperlink)item).Attrib.Single<TagAttribute>(s => s.Name.ToLower() == "href").Value;
                if (teamRecordPath.IsMatch(item))
                    roster.Record = item.ActualSymbol;

                if (teamRosterSpotPath.IsMatch(item))
                    player.RosterSpot = item.ActualSymbol;
                if (teamPlayerNamePath.IsMatch(item))
                    player.Name = item.ActualSymbol;
                if (teamPlayerUrlPath.IsMatch(item))
                    player.PlayerURL = ((Hyperlink)item).Attrib.Single<TagAttribute>(s => s.Name.ToLower() == "playerid").Value; // ESPN uses javascript to create a pop up.  But it probably uses values in the tag attributes to do the ajax lookup(it's what i would do).  here we pull up the attribute playerID (this may change at any time.  We're hacking their data for our purposes.
                if (teamPlayerPositionPath.IsMatch(item))
                {
                    var literal = item.ActualSymbol.Split(new string[] {"&nbsp;"}, StringSplitOptions.None);

                    string team = string.IsNullOrWhiteSpace(literal[0]) ? null : literal[0].Substring(literal[0].IndexOf(",")+1).Trim();
                    string pos = literal.Length>1 ? literal[1] : null;
                    player.Team = team;
                    player.Pos = pos;
                }
                if (teamRosterSpotEndPath.IsMatch(item))
                {
                    Console.WriteLine("Player for {0} ({1}-{2}-{3})... {4,-3} | {5,-15} | {6} | {7} | {8} | {9}", roster.Name, roster.GetWins(), roster.GetLosses(), roster.GetTies(), player.RosterSpot, player.Name, player.Team, player.Pos, player.PlayerURL, roster.TeamURL);
                    roster.Players.Add(player);
                    player = new PlayerOnRoster();
                }
                if (teamEndPath.IsMatch(item))
                {
                    Console.WriteLine("Saving roster for {0}...", roster.Name);
                    ESPNLeagueRoster.SetData(roster);

                    roster = new TeamRoster();
                }
            }
        }


        static private HtmlPath getTestForTeamName()
        {
            /*
                Literal VALLEY STREAM RANGERS
                Hyperlink <a href="/ffl/clubhouse?leagueId=51111&teamId=1">
                Th <th colspan="3">
                Tr <tr class="playerTableBgRowHead tableHead playertableSectionHeader">
                TableTag <table class="playerTableTable tableBody" cellpadding="0" cellspacing="1" border="0" id="playertable_0">
             */
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Hyperlink>() {
                                    new ParentTest() {
                                        new ElementTest<Th>() {
                                            new ParentTest() {
                                                new ElementTest<Tr>() {
                                                    getTestForRosterTable()
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
        static private HtmlPath getTestForTeamURL()
        {
            /*
                Hyperlink <a href="/ffl/clubhouse?leagueId=51111&teamId=1">
                Th <th colspan="3">
                Tr <tr class="playerTableBgRowHead tableHead playertableSectionHeader">
                TableTag <table class="playerTableTable tableBody" cellpadding="0" cellspacing="1" border="0" id="playertable_0">
             */
            ITokenTest top =
                                new ElementTest<Hyperlink>() {
                                    new ParentTest() {
                                        new ElementTest<Th>() {
                                            new ParentTest() {
                                                new ElementTest<Tr>() {
                                                    getTestForRosterTable()
                                                }
                                            }
                                        }
                                    }
                                };
    
            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForTeamRecord()
        {
            /*
                Literal (5-7)
                Th <th colspan="3">
                Tr <tr class="playerTableBgRowHead tableHead playertableSectionHeader">
             */
            ITokenTest top =
                        new ElementTest<Literal>() { //I think this is first and only literal
                            new ParentTest() {
                                new ElementTest<Th>() {
                                    new ParentTest() {
                                        new ElementTest<Tr>() {
                                            getTestForRosterTable()
                                        }
                                    }
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForRosterSlot()
        {
            /*
            Literal QB
            Td <td id="slot_5526" class="slot_0 playerSlot" style="font-weight: bold;">
            Tr <tr id="plyr5526" class="pncPlayerRow playerTableBgRow0">
            TableTag <table class="playerTableTable tableBody" cellpadding="0" cellspacing="1" border="0" id="playertable_0">
             */
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                        new ElementTest<Td>() {
                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="playerSlot" },
                                            new ParentTest() {
                                                new ElementTest<Tr>() {
                                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="pncPlayerRow" },
                                                    getTestForRosterTable()
                                                }
                                            }
                                        }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForPlayerName()
        {
            /*
            Literal Eli Manning
            Hyperlink <a href="" class="flexpop" content="tabs#ppc" instance="_ppc" fpopHeight="357px" fpopWidth="490px" tab="null" leagueId="51111" playerId="5526" teamId="-2147483648" seasonId="2015" cache="true">
            Td <td class="playertablePlayerName" id="playername_5526" style="">
            Tr <tr id="plyr5526" class="pncPlayerRow playerTableBgRow0">
            TableTag <table class="playerTableTable tableBody" cellpadding="0" cellspacing="1" border="0" id="playertable_0">
             */
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Hyperlink>() {
                                    new ParentTest() {
                                        new ElementTest<Td>() {
                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="playertablePlayerName" },
                                            new ParentTest() {
                                                new ElementTest<Tr>() {
                                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="pncPlayerRow" },
                                                    getTestForRosterTable()
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
        static private HtmlPath getTestForPlayerURL()
        {
            /*
            Literal Eli Manning
            Hyperlink <a href="" class="flexpop" content="tabs#ppc" instance="_ppc" fpopHeight="357px" fpopWidth="490px" tab="null" leagueId="51111" playerId="5526" teamId="-2147483648" seasonId="2015" cache="true">
            Td <td class="playertablePlayerName" id="playername_5526" style="">
            Tr <tr id="plyr5526" class="pncPlayerRow playerTableBgRow0">
            TableTag <table class="playerTableTable tableBody" cellpadding="0" cellspacing="1" border="0" id="playertable_0">
             */
            ITokenTest top =
                                new ElementTest<Hyperlink>() {
                                    new ParentTest() {
                                        new ElementTest<Td>() {
                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="playertablePlayerName" },
                                            new ParentTest() {
                                                new ElementTest<Tr>() {
                                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="pncPlayerRow" },
                                                    getTestForRosterTable()
                                                }
                                            }
                                        }
                                    }
                                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForPlayerTeamPosition()
        {
            /*
            Literal , NYG&nbsp;QB
            Td <td class="playertablePlayerName" id="playername_5526" style="">
            Tr <tr id="plyr5526" class="pncPlayerRow playerTableBgRow0">
            TableTag <table class="playerTableTable tableBody" cellpadding="0" cellspacing="1" border="0" id="playertable_0">
             */
            ITokenTest top =
                        new ElementTest<Literal>() { //I think it's the first literal, after hyperlink
                            new ParentTest() {
                                        new ElementTest<Td>() {
                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="playertablePlayerName" },
                                            new ParentTest() {
                                                new ElementTest<Tr>() {
                                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="pncPlayerRow" },
                                                    getTestForRosterTable()
                                                }
                                            }
                                        }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForSlotEnd()
        {
            ITokenTest top = 
                                        new ElementTest<Closure>() {
                                            new ParentTest() {
                                                new ElementTest<Tr>() {
                                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="pncPlayerRow" },
                                                    getTestForRosterTable()
                                                }
                                            }
                                        };
   
            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForTeamEnd()
        {
            ITokenTest top = new ElementTest<Closure>() {
                                    getTestForRosterTable()
                            };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private ITokenTest getTestForRosterTable()
        {
            /*
            TableTag <table class="playerTableTable tableBody" cellpadding="0" cellspacing="1" border="0" id="playertable_0">
            Td <td width="33%" valign="top" style="vertical-align: top;">
            Tr <tr>
            TableTag <table cellpadding="2" cellspacing="1" border="0" width="100%" class="tableBody">
            Div <div class="games-fullcol games-fullcol-extramargin">
            Div <div class="games-innercol2">
            */
            ITokenTest top = new CacheTestResult(
                    new ParentTest() {
                        new ElementTest<TableTag>() {
                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="playerTableTable" },
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    new ParentTest() {
                                        new ElementTest<Tr>() {
                                            new ParentTest() {
                                                new ElementTest<TableTag>() {
                                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="tableBody" },
                                                    new ParentTest() {
                                                        new ElementTest<Div>() {
                                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="games-fullcol" },
                                                            new ParentTest() {
                                                                new ElementTest<Div>() {
                                                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="games-innercol2" },
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                    });

            return top;
        }



    }

}
