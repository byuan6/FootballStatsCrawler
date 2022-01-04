# FootballStatsCrawler
Project to steal other website's Football Stats, in a misguided attempt to evaluate them, the same way as finance.

## If you plan to do this for a commercial product, I suggest you buy it from someone who licensed the data, as it is technically owned by NFL.
Web-scraping is notoriously unreliable process for getting data, and you need to regularly monitor your process, for fixes, when the sources change.  And you will need to make changes to your code yourself once they make changes, bc I don't plan on updatings this project much after this.
If this is for programming fun, I don't see much harm here.

## FFtoiletbowl
FFtoiletbowl has routines to create a MS Sql Server database to store Football Stats.
You need to have MS SQL Server installed.  MS SQL Server Express is free.
This is necessary to have a place to store the football stats.  It's much easier to organize the stats for analysis, within a database.
If you encounter problems with FFtoiletbowl's database creation process (bc I haven't tested it in years), 
there should be a "createdatabase.txt" or something like that in a folder in this project

## FFtoiletbowlSQL
Commandline access to web scraped data

## FFtoiletbowlWeb
never really started.  Intended as a dynamic webpage to show analysis results and drill-down to underlying data, and even make changes to data, to see changes to analysis.

## GetStats
GetStats produces the command line utility to scrape NFL stats data from several sources, and store them into SQL Server.  You will need (SSMS) SQL Server Management Studio to access the data in the FFtoiletBowl database.  The web interface is not ready, and would be less flexible than the SQL programming.

## GetRosters
Doesn't work.  It did work, in 2017.  ESPN has since changed it's stance on publicly viewable leagues.  It might still work, if publicly viewable leagues eventually gets reinstated (and I don't see why it wouldn't, since e-sports paradigm only expands participation), if your league is set to be publicly viewwable.  But it's intent was to scrape your league's fantasy football schedule and roster information, so it will be easier for the program to make "custom" recommendations for you.

Today, I found this (http://espn-fantasy-football-api.s3-website.us-east-2.amazonaws.com/), so maybe you can get rosters from a more dependable source.

## TwoTeamMonteCarlo
Does an analysis in command line, of a player's worth.  It requires 3 passes to get accurate analysis of how many wins a player is worth.  Each pass can be cancelled at anytime, but the longer it runs, the more precise the analsysi since it is a MonteCArlo simulation.

## DraftSimulator
has never been started.  It is intended as a montecarlo type simulation of possible fantasy football drafts, and presumably which players you should pick, if they fall down to you.  but most likely where you should expect them, and if there is a chance you can wait to get a player.  And give you a running estimate of the standings at end of 13 weeks.

## CrawlerCommon, CrawlerHpathUI
The dll's for parsing HTML.  PLus a utility to see how the parser will return data to you.  These were copied from HTMLParser repo.  This copy of the project probably will not change, unless there is bug to be fixed.  Or someone needs a feature developed in a future HTMLparser project, to implement a new feature here... yeah, it's never going to be updated.

## Batch
This was create at a time, I thought it would be sufficient to view my analysis in a blog.  But wordpress, as easy as it is to use from a publishing point of view, is incredible non-dynamic and non-interactive, if you want to see your project in a "live" way.  So, I started FFToiletBowlWeb to present a more dynamic view of the analysis.  But never finished.  It's not useless, but it is outdated as https://beyondtoiletbowl.wordpress.com/ is no longer being updated.

## Histogram, HtmlTable, Plot
CLI utilities to create content files, in a automation process.

## ViewPng
Just in case your image viewer isn't working.  This one also doesn't expand to maximized, so I can see all different charts in different windows.

## HTml2tsv, Browserscript, CacheStdout, ConsoleApplication1
I don't remember what these were for anymore and I'm too lazy to look in them

## Wordpress
CLI utilities to upload to wordpress.

