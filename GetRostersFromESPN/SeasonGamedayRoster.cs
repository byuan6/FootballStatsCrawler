using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using CrawlerCommon;
using CrawlerCommon.TagDef.StrictXHTML;
using CrawlerCommon.HtmlPathTest;

using FFToiletBowl;

namespace GetRostersFromESPN
{
    public class SeasonGamedayRoster
    {
        // Phase2, see if data can be pulled from http://espn-fantasy-football-api.s3-website.us-east-2.amazonaws.com/

        static public void RetreiveGamedayRosters(string leagueID, string wk)
        {
            int targetGm = 0;
            int.TryParse(wk, out targetGm);

            Console.WriteLine("------------------------------------------------------------");
            string url2 = string.Format("http://games.espn.go.com/ffl/scoreboard?leagueId={0}&matchupPeriodId={1}", leagueID, wk);
            Console.WriteLine("Getting ESPN gameday matchup links [{0}] from wk {0}...", wk, url2);
            Page page2 = new Page();
            page2.DebugMode = true;
            page2.TrimWhitespace = true;
            page2.Load(url2);

            var matchuplinkPath = getTestForMatchupLinkTable();
            List<string> links = new List<string>();
            var builder2 = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in page2.Tokens.BuildTree(builder2))
            {
                if (matchuplinkPath.IsMatch(item)) {
                    Hyperlink atag = (Hyperlink)item.ParentNode;
                    string href = atag.Attrib.Single<TagAttribute>(s=>s.Name.ToLower()=="href").Value;
                    Console.WriteLine("Found {0}, adding...", href);
                    links.Add(href);
                }
            }

            foreach (var link in links)
            {
                ESPNLeagueRoster.LoadID = Guid.NewGuid(); //need to create new loadID for new week.

                string url = "http://games.espn.go.com" + link;
                Console.WriteLine("Getting ESPN Rosters for gameday {0} from {0}...", wk, url);
                Page page = new Page();
                page.DebugMode = true;
                page.TrimWhitespace = true;
                page.Load(url);

                var twoTeamNamePath = getTestForTeamNameTable();
                var twoTeamUrlPath = getTestForTeamUrlTable();

                var teamRosterSpotPath = getTestForRosterSlot();
                var teamPlayerNamePath = getTestForPlayerName();
                var teamPlayerUrlPath = getTestForPlayerURL();
                var teamPlayerPositionPath = getTestForPlayerTeamPosition();
                var teamRosterSpotEndPath = getTestForSlotEnd();
                var teamEndPath = getTestForTeamEnd();

                int rosterindex = 0;
                int teamnameindex = 0;
                int teamurlindex = 0;
                TeamRoster[] roster = new TeamRoster[] { new TeamRoster() { Record = string.Empty, TargetGm = targetGm }, new TeamRoster() { Record = string.Empty, TargetGm = targetGm } };
                PlayerOnRoster player = new PlayerOnRoster();
                Console.WriteLine("------------------------------------------------------------");
                var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
                foreach (var item in page.Tokens.BuildTree(builder))
                {
                    if (twoTeamNamePath.IsMatch(item))
                        roster[teamnameindex++].Name = item.ActualSymbol;
                    if (twoTeamUrlPath.IsMatch(item))
                        roster[teamurlindex++].TeamURL = ((Hyperlink)item.ParentNode).Attrib.Single<TagAttribute>(s => s.Name.ToLower() == "href").Value;

                    if (teamRosterSpotPath.IsMatch(item))
                        player.RosterSpot = item.ActualSymbol;
                    if (teamPlayerNamePath.IsMatch(item))
                        player.Name = item.ActualSymbol;
                    if (teamPlayerUrlPath.IsMatch(item))
                        player.PlayerURL = ((Hyperlink)item).Attrib.Single<TagAttribute>(s => s.Name.ToLower() == "playerid").Value; // ESPN uses javascript to create a pop up.  But it probably uses values in the tag attributes to do the ajax lookup(it's what i would do).  here we pull up the attribute playerID (this may change at any time.  We're hacking their data for our purposes.
                    if (teamPlayerPositionPath.IsMatch(item))
                    {
                        var literal = item.ActualSymbol.Split(new string[] { "&nbsp;" }, StringSplitOptions.None);

                        string team = string.IsNullOrWhiteSpace(literal[0]) ? null : literal[0].Substring(literal[0].IndexOf(",") + 1).Trim();
                        string pos = literal.Length > 1 ? literal[1] : null;
                        player.Team = team;
                        player.Pos = pos;
                    }
                    if (teamRosterSpotEndPath.IsMatch(item))
                    {
                        Console.WriteLine("Player for {0} ({1}-{2}-{3})... {4,-3} | {5,-15} | {6} | {7} | {8} | {9}", roster[rosterindex].Name, null, null, null, player.RosterSpot, player.Name, player.Team, player.Pos, player.PlayerURL, roster[rosterindex].TeamURL);
                        roster[rosterindex].Players.Add(player);
                        player = new PlayerOnRoster();
                    }
                    if (teamEndPath.IsMatch(item))
                    {
                        
                        Console.WriteLine("Finished roster for {0}...", roster[rosterindex].Name);
                        rosterindex++;
                        if (rosterindex > 1) break; //dont bother with the bench for this load
                    }
                }
                Console.WriteLine("Saving both starting rosters only...");
                if (!ESPNLeagueRoster.IsCleared)
                {
                    ESPNLeagueRoster.ClearRosterData();
                    ESPNLeagueRoster.IsCleared = true;
                }
                roster[0].GmVersusTeamURL = roster[1].TeamURL;
                roster[1].GmVersusTeamURL = roster[0].TeamURL;
                ESPNLeagueRoster.SetData(roster[0]);
                ESPNLeagueRoster.SetData(roster[1]);
            }
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
            TableTag <table class="playerTableTable tableBody" cellpadding="0" cellspacing="1" border="0" id="playertable_0">  --but Not HideableGroup
            Div <div style="width: 49%; float: left;">
            Div <div class="games-fullcol games-fullcol-extramargin">
            Div <div class="games-innercol2">
            */
            ITokenTest top = new CacheTestResult(
                    new ParentTest() {
                        new ElementTest<TableTag>() {
                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="playerTableTable" },
                            new NotTest(new AttribContainsTest() { AttributeName="CLASS", CompareValue="hideableGroup" }),
                            new ParentTest() {
                                new ElementTest<Div>() {
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
                            }
                        }
                    });

            return top;
        }

        static private ITokenTest getTestForTeamUrlTable()
        {
            /*
                Hyperlink <a style="border:none; width:151; height:129.0;" href="/ffl/clubhouse?leagueId=51111&teamId=21">
                Div <div style="float:left; border-right:1px solid #dddddd; line-height:0px;">
                Div <div style="float:left; border: 1px solid #DDDDDD; padding: 0px; float: left; width:455px; background:#EFEFEF;">
                Div <div style="float:right;">
                Div <div id="teamInfos" style="clear:both; width:956px; margin-bottom:10px;">
            */
            ITokenTest top =    new ElementTest<Img>() {
                                    new ParentTest() {
                                        new ElementTest<Hyperlink>() {
                                            new ParentTest() {
                                                new ElementTest<Div>() {
                                                    new ParentTest() {
                                                        new ElementTest<Div>() {
                                                            new ParentTest() {
                                                                new ElementTest<Div>() {
                                                                    new ParentTest() {
                                                                        new ElementTest<Div>() {
                                                                            new AttribContainsTest() { AttributeName="id", CompareValue="teamInfos" },
                                                                            new ParentTest() {
                                                                                new ElementTest<Div>()
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                        }
                                    }
                                };

            return top;
        }

        static private ITokenTest getTestForTeamNameTable()
        {
            /*
                Literal AL Davis Just win baby
                Bold <b>
                Div <div style="font-size:18px; margin-bottom:14px; font-family:Helvetica,sans-serif;">
                Div <div style="text-align:left; padding:5px; float:left; width:285px; line-height:18px;" class="bodyCopy">
                Div <div style="float:left; border: 1px solid #DDDDDD; padding: 0px; float: left; width:455px; background:#EFEFEF;">
                Div <div style="float:right;">
                Div <div id="teamInfos" style="clear:both; width:956px; margin-bottom:10px;">
            */
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Bold>() {
                                    new ParentTest() {
                                        new ElementTest<Div>() {
                                            new ParentTest() {
                                                new ElementTest<Div>() {
                                                    new ParentTest() {
                                                        new ElementTest<Div>() {
                                                            new ParentTest() {
                                                                new ElementTest<Div>() {
                                                                    new ParentTest() {
                                                                        new ElementTest<Div>() {
                                                                            new AttribContainsTest() { AttributeName="id", CompareValue="teamInfos" },
                                                                            new ParentTest() {
                                                                                new ElementTest<Div>()
                                                                            }
                                                                        }
                                                                    }
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
                        };

            return top;
        }


        static private ITokenTest getTestForMatchupLinkTable()
        {
            /*
            Literal Quick Box Score
            Hyperlink <a href="/ffl/boxscorequick?leagueId=51111&teamId=5&scoringPeriodId=12&seasonId=2015&view=scoringperiod&version=quick">
            Div <div class="boxscoreLinks">
            Td <td colspan="2" class="info">
            Tr <tr>
            TableTag <table class="ptsBased matchup">
            Td <td width="49%" class="matchupContainer">
            --- Tr <tr>
            --- TableTag <table width="100%" cellspacing="0" cellpadding="0" border="0">
            --- Div <div>
            --- Div <div id="scoreboardMatchups">
            */
            ITokenTest top = 
                new ElementTest<Literal>() {
                    new ElementComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, Operator=0, CompareValue="Quick Box Score" },
                    new ParentTest() {
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Div>() {
                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="boxscoreLinks" },
                                    new ParentTest() {
                                        new ElementTest<Td>() {
                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="info" },
                                            new ParentTest() {
                                                new ElementTest<Tr>() {
                                                    new ParentTest() {
                                                        new ElementTest<TableTag>() {
                                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="ptsBased" },
                                                            new ParentTest() {
                                                                new ElementTest<Td>() {
                                                                    
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
                    }
                };

            return top;
        }


    }
}
