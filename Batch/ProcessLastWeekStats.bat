
for /f "tokens=1* delims=" %%a in ('date /T') do set datestr=%%a
mkdir "%datestr:/=-%"
xcopy /y *.* "%datestr:/=-%"
cd "%datestr:/=-%"


regreader
rem sqlcmd -L
rem OSQL -L



SqlCmd -E -S "FUJIBOOKPRO-WIN\SQLSERVER2008" -Q "BACKUP DATABASE FFToiletBowl TO DISK='%BACKUPFILE%'"



GOTO skip_restore_db

REM  Restore database
SET ISRESTORE=FALSE
SqlCmd -U toilet -P toiletbowl -S "FUJIBOOKPRO-WIN\SQLSERVER2008" -d "FFToiletBowl" -W -Q "select 1+1"
if ERRORLEVEL 1 SET ISRESTORE=TRUE
if ISRESTORE==FALSE SqlCmd -U toilet -P toiletbowl -S "FUJIBOOKPRO-WIN\SQLSERVER2008" -d "FFToiletBowl" -W -Q "set nocount on;exec GetData 'vwChecksum'" > actual.txt
echo n | comp expected.txt actual.txt
if ERRORLEVEL 1 SET ISRESTORE=TRUE
if ISRESTORE==TRUE (
    SqlCmd -E -S Server_Name –Q “RESTORE DATABASE [Name_of_Database] FROM DISK=’X:PathToBackupFile[File_Name].bak'”
)
:skip_restore_db




REM  -----------------------------------
REM  Get last week's statistics, IR, schedule grid
REM  TBD schedule changes: http://www.fftoday.com/nfl/schedule.php?o=1&Week=all
REM  injury list
GetStats



REM  ----------------------------------
REM  Retreive the last week that was loaded
for /f %%i in ('FFToiletBowlSQL vwLastLoadedGm') do set LAST_GM=%%i



REM  -----------------------------------

REM  Get rosters for games played in City Island League
if %LAST_GM% gtr 2 (
    for /f "tokens=1* delims=" %%a in ('NumList %LAST_GM%') do set forweeks=%%a
    GetRostersFromESPN %forweeks%
)

REM Get Rosters for next weekend
GetRostersFromESPN



REM  ----------------------------------

REM  get injuries daily http://espn.go.com/nfl/injuries
REM  produce injury delta report
REM  player, today status, yesterday status, 2 days ago status ... to last game day
REM  order by last status change at top
REM  if stats was already loaded for this week, re-handicap for Thu afternoon

REM  terminate script if stats was already loaded for this week.... checks dates loaded from above, are after the game date, then the week has turned over






rem ----------------------------------
for /f "delims=" %%i in ('FFToiletBowlSQL vwBestPerformer') do set BEST=%%i
echo ^<h1^>Best performance for last week: %BEST% > Highlight.html
FFToiletBowlSQL vwPlayerReview | plot PlayerGameRegression.png true size 800 400 subset "Game vs Latest Handicap" Point tonumber 6 tonumber 5 4 1 "%BEST%" subset "Fit during relevant period" Point tonumber 6 tonumber 9 -1 1 "%BEST%" | AddImageToWordpress >> Highlight.html

PostNewToWordpress "Week %LAST_GM%, Player of the week: %BEST%" < Highlight.html








rem ----------------------------------

echo ^<h1^> > LastWeekRankingPerformance.html
FFToiletBowlSQL vwLastLoadedGm >> LastWeekRankingPerformance.html
echo Performance^</h1^>  >> LastWeekRankingPerformance.html

echo ^<p^>Proportional Ranking allows a view of the players according to the performance of the pack.  If the pack has 20 players, then the ranks go from 1 to 20, but each player can have rank anywhere in between.  Players that perform similarly according to the metric, with have closer ranks, ie. 2 players with rank of 7.1 and 7.2 from pack of 28 players means both players perform similarly and are 1/4 between top and last ranked player.  Packs is this case are the top players for the position, enough to occupy starting and backup positions for a team in my league of 14 teams  >> LastWeekRankingPerformance.html
echo ^<p^>Below, shows charting of 4 ranking methods: By 1) season long position winning pct 2) season long point average 3) later games have higher weighting position winning pct 4) later games have higher weighting points average, each compared how they fared the latest week.  ^<br^>  >> LastWeekRankingPerformance.html

REM x-rank by wins up to now, y-rank by points scored in last week
REM column 10 is rank by wins (expected performance based on past)
FFToiletBowlSQL tblExpectedRankingVsActual | plot SeasonWins.png true size 1200 800 series All_Positions Point tonumber 10 tonumber 6 1 subset QB_Performance FastPoint tonumber 10 tonumber 6 -1 21 QB subset RB_Performance FastPoint tonumber 10 tonumber 6 -1 21 RB subset WR_Performance FastPoint tonumber 10 tonumber 6 -1 21 WR subset TE_Performance FastPoint tonumber 10 tonumber 6 -1 21 TE subset K_Performance FastPoint tonumber 10 tonumber 6 -1 21 K subset DST_Performance FastPoint tonumber 10 tonumber 6 -1 21 DST line 1 1 50 50 | AddImageToWordpress >> LastWeekRankingPerformance.html
echo ^<br^>^<i^>How to look at it: Closer to left means top ranked at position, closer to bottom is how they performed last week relative to top ranked last week.  Players above the dashed line did worse than expected.  Players below the dashed line did better than expected.^</i^>  >> LastWeekRankingPerformance.html

REM x-rank by points up to now, y-rank by points scored in last week
REM column 14 is rank by points (expected performance based on past)
FFToiletBowlSQL tblExpectedRankingVsActual | plot SeasonPoints.png true size 1200 800 series All_Positions Point tonumber 14 tonumber 6 1 subset QB_Performance FastPoint tonumber 14 tonumber 6 -1 21 QB subset RB_Performance FastPoint tonumber 14 tonumber 6 -1 21 RB subset WR_Performance FastPoint tonumber 14 tonumber 6 -1 21 WR subset TE_Performance FastPoint tonumber 14 tonumber 6 -1 21 TE subset K_Performance FastPoint tonumber 14 tonumber 6 -1 21 K subset DST_Performance FastPoint tonumber 14 tonumber 6 -1 21 DST line 1 1 50 50 | AddImageToWordpress >> LastWeekRankingPerformance.html
echo ^<br^>^<i^>How to look at it: (See above)^</i^>  >> LastWeekRankingPerformance.html

REM x-rank by wins up to now, y-rank by points scored in last week
REM column 18 is rank by latest wins (expected performance based on past)
FFToiletBowlSQL tblExpectedRankingVsActual | plot LateWins.png true size 1200 800 series All_Positions Point tonumber 18 tonumber 6 1 subset QB_Performance FastPoint tonumber 18 tonumber 6 -1 21 QB subset RB_Performance FastPoint tonumber 18 tonumber 6 -1 21 RB subset WR_Performance FastPoint tonumber 18 tonumber 6 -1 21 WR subset TE_Performance FastPoint tonumber 18 tonumber 6 -1 21 TE subset K_Performance FastPoint tonumber 18 tonumber 6 -1 21 K subset DST_Performance FastPoint tonumber 18 tonumber 6 -1 21 DST line 1 1 50 50 | AddImageToWordpress >> LastWeekRankingPerformance.html
echo ^<br^>^<i^>How to look at it: (See above)^</i^>  >> LastWeekRankingPerformance.html

REM column 20 is rank by latest points (expected performance based on past)
FFToiletBowlSQL tblExpectedRankingVsActual | plot LatePoints.png true size 1200 800 series All_Positions Point tonumber 20 tonumber 6 1 subset QB_Performance FastPoint tonumber 20 tonumber 6 -1 21 QB subset RB_Performance FastPoint tonumber 20 tonumber 6 -1 21 RB subset WR_Performance FastPoint tonumber 20 tonumber 6 -1 21 WR subset TE_Performance FastPoint tonumber 20 tonumber 6 -1 21 TE subset K_Performance FastPoint tonumber 20 tonumber 6 -1 21 K subset DST_Performance FastPoint tonumber 20 tonumber 6 -1 21 DST line 1 1 50 50 | AddImageToWordpress >> LastWeekRankingPerformance.html
echo ^<br^>^<i^>How to look at it: (See above)^</i^>  >> LastWeekRankingPerformance.html

REM column 13 is avg points scored by player (expected performance based on past), column 2 is actual points scored
echo ^<p^>Below is a comparison of how the player's season average(x-axis), against their last week's performance(points). ^<br^>  >> LastWeekRankingPerformance.html
FFToiletBowlSQL tblExpectedRankingVsActual | plot AveragePoints.png true size 1200 800 series All_Positions Point tonumber 13 tonumber 2 1 subset QB_Performance FastPoint tonumber 13 tonumber 2 -1 21 QB subset RB_Performance FastPoint tonumber 13 tonumber 2 -1 21 RB subset WR_Performance FastPoint tonumber 13 tonumber 2 -1 21 WR subset TE_Performance FastPoint tonumber 13 tonumber 2 -1 21 TE subset K_Performance FastPoint tonumber 13 tonumber 2 -1 21 K subset DST_Performance FastPoint tonumber 13 tonumber 2 -1 21 DST line 1 1 40 40 | AddImageToWordpress >> LastWeekRankingPerformance.html
echo ^<br^>^<i^>How to look at it: X-axis is the season average points per game by the player, y-axis is points they scored last week.  Opposite of above charts, players above the dashed line performed better than their average.  Below the line means they performed worse.^</i^>  >> LastWeekRankingPerformance.html


REM column 20 is rank by latest points (expected performance based on past)
echo ^<p^>Below is a comparison of how the player's late game weighted, season average(x-axis), against their last week's performance(points). ^<br^>  >> LastWeekRankingPerformance.html
FFToiletBowlSQL tblExpectedRankingVsActual | plot AveragePoints.png true size 1200 800 series All_Positions Point tonumber 13 tonumber 2 1 subset QB_Performance FastPoint tonumber 13 tonumber 2 -1 21 QB subset RB_Performance FastPoint tonumber 13 tonumber 2 -1 21 RB subset WR_Performance FastPoint tonumber 13 tonumber 2 -1 21 WR subset TE_Performance FastPoint tonumber 13 tonumber 2 -1 21 TE subset K_Performance FastPoint tonumber 13 tonumber 2 -1 21 K subset DST_Performance FastPoint tonumber 13 tonumber 2 -1 21 DST line 1 1 40 40 | AddImageToWordpress >> LastWeekRankingPerformance.html
echo ^<br^>^<i^>How to look at it: (See above)^</i^>  >> LastWeekRankingPerformance.html

echo ^<p^>Below, shows charting of how our 4 ranking methods performed, how many differences to their predicted rank (are they who we think they are, rewording Cardinal's coach, Denny Green): By 1) season long position winning pct 2) season long point average 3) later games have higher weighting position winning pct 4) later games have higher weighting points average, putting them into groups of how they performed according to their rank.  We don't have negative bins.  Every player that did better, has to result in a player that did worse using the ranking system.  Plus there no going up from rank #1.  So players that do slightly better, are in same group as players who do slightly worse, the 0..4 bin.  Plus here is, if the horde all decides to be better at same time b/c the weather is nice that sunday, it reflects as everyone did better using points, but everyone did mostly the same using rankings.>> LastWeekRankingPerformance.html
REM DEVIATION FROM EXPECTED, column 1 has bins, column 2 has count of players that for that level of deviation from predicted/expected rank (win ranked)
FFToiletBowlSQL vwExpectedRankingVsActualHistogram | plot RankAccuracy1.png true size 600 400 subset QB_Deviations StackedColumn tostring 1 tonumber 2 -1 0 QB subset RB_Deviations StackedColumn tostring 1 tonumber 2 -1 0 RB subset WR_Deviations StackedColumn tostring 1 tonumber 2 -1 0 WR subset TE_Deviations StackedColumn tostring 1 tonumber 2 -1 0 TE subset "K Deviations" StackedColumn tostring 1 tonumber 2 -1 0 K subset "DST Deviations" StackedColumn tostring 1 tonumber 2 -1 0 DST  | AddImageToWordpress >> LastWeekRankingPerformance.html
REM DEVIATION FROM EXPECTED, season pts rank
FFToiletBowlSQL vwExpectedRankingVsActualHistogram | plot RankAccuracy2.png true size 600 400 subset QB_Deviations StackedColumn tostring 1 tonumber 3 -1 0 QB subset RB_Deviations StackedColumn tostring 1 tonumber 3 -1 0 RB subset WR_Deviations StackedColumn tostring 1 tonumber 3 -1 0 WR subset TE_Deviations StackedColumn tostring 1 tonumber 3 -1 0 TE subset "K Deviations" StackedColumn tostring 1 tonumber 3 -1 0 K subset "DST Deviations" StackedColumn tostring 1 tonumber 3 -1 0 DST  | AddImageToWordpress >> LastWeekRankingPerformance.html
REM DEVIATION FROM EXPECTED, late wins rank
FFToiletBowlSQL vwExpectedRankingVsActualHistogram | plot RankAccuracy3.png true size 600 400 subset QB_Deviations StackedColumn tostring 1 tonumber 4 -1 0 QB subset RB_Deviations StackedColumn tostring 1 tonumber 4 -1 0 RB subset WR_Deviations StackedColumn tostring 1 tonumber 4 -1 0 WR subset TE_Deviations StackedColumn tostring 1 tonumber 4 -1 0 TE subset "K Deviations" StackedColumn tostring 1 tonumber 4 -1 0 K subset "DST Deviations" StackedColumn tostring 1 tonumber 4 -1 0 DST  | AddImageToWordpress >> LastWeekRankingPerformance.html
REM DEVIATION FROM EXPECTED, late pts rank
FFToiletBowlSQL vwExpectedRankingVsActualHistogram | plot RankAccuracy4.png true size 600 400 subset QB_Deviations StackedColumn tostring 1 tonumber 5 -1 0 QB subset RB_Deviations StackedColumn tostring 1 tonumber 5 -1 0 RB subset WR_Deviations StackedColumn tostring 1 tonumber 5 -1 0 WR subset TE_Deviations StackedColumn tostring 1 tonumber 5 -1 0 TE subset "K Deviations" StackedColumn tostring 1 tonumber 5 -1 0 K subset "DST Deviations" StackedColumn tostring 1 tonumber 5 -1 0 DST  | AddImageToWordpress >> LastWeekRankingPerformance.html

echo ^<br^>^<i^>How to look at it: (See above)^</i^>  >> LastWeekRankingPerformance.html












rem ----------------------------------

rem Performance of last week's handicap
rem Average Expected vs Actual
echo ^<p^>^<b^>How did the defensive handicaps do?^</b^>  Below are 3 charts: 1) season average (x) against last game pts(y) 2) forecasted pts using defense handicap (x) against last game points(y) 3) The amount of point difference from average predicted by our defense handicap(x) against actual point difference from average and season average(y). What we are looking for is better correlation from the 1st chart, to the 2nd.  And the 3rd chart, we hope to see more points closer to line y=x, but in the past, the effects of a defense aren't that stable from play to play or game to game.  There are large random effects observed, as seen from the large y-range.  >> LastWeekRankingPerformance.html
FFToiletBowlSQL tblDefenseHandicappedExpectedVsActual | plot AvgExpectedVsActual.png true size 1200 800 series All_Positions Point tonumber 17 tonumber 10 0 subset QB_Performance FastPoint tonumber 17 tonumber 10 -1 2 QB subset RB_Performance FastPoint tonumber 17 tonumber 10 -1 2 RB subset WR_Performance FastPoint tonumber 17 tonumber 10 -1 2 WR subset TE_Performance FastPoint tonumber 17 tonumber 10 -1 2 TE subset K_Performance FastPoint tonumber 17 tonumber 10 -1 2 K subset DST_Performance FastPoint tonumber 17 tonumber 10 -1 2 DST line 1 1 40 40 | AddImageToWordpress >> LastWeekRankingPerformance.html
rem Handicapped Expected vs Actual
FFToiletBowlSQL tblDefenseHandicappedExpectedVsActual | plot HandicappedExpectedVsActual.png true size 1200 800 series All_Positions Point tonumber 33 tonumber 10 0 subset QB_Performance FastPoint tonumber 33 tonumber 10 -1 2 QB subset RB_Performance FastPoint tonumber 33 tonumber 10 -1 2 RB subset WR_Performance FastPoint tonumber 33 tonumber 10 -1 2 WR subset TE_Performance FastPoint tonumber 33 tonumber 10 -1 2 TE subset K_Performance FastPoint tonumber 33 tonumber 10 -1 2 K subset DST_Performance FastPoint tonumber 33 tonumber 10 -1 2 DST line 1 1 40 40 | AddImageToWordpress >> LastWeekRankingPerformance.html
rem Handicapped pct handicapped vs pct Actual
FFToiletBowlSQL tblDefenseHandicappedExpectedVsActual | plot HandicappedExpectedVsActualFromAvg.png true size 1200 800 series All_Positions Point tonumber 26 tonumber 34 0 subset QB_Performance FastPoint tonumber 26 tonumber 34 -1 2 QB subset RB_Performance FastPoint tonumber 26 tonumber 34 -1 2 RB subset WR_Performance FastPoint tonumber 26 tonumber 34 -1 2 WR subset TE_Performance FastPoint tonumber 26 tonumber 34 -1 2 TE subset K_Performance FastPoint tonumber 26 tonumber 34 -1 2 K subset DST_Performance FastPoint tonumber 26 tonumber 34 -1 2 DST line 1 1 40 40 | AddImageToWordpress >> LastWeekRankingPerformance.html

rem put histograms here for deviations using average, and using forecast.
echo ^<p^>Below is the distribution of our difference (or deviation or delta) from the average, and the Forecast with a defensive handicap applied.  If our handicap was effective, there should be an observable number of predictions in the bins closer to zero for our handicap, compared to the average.  Keep your expectations in check, however, and remember that 20% is only 2 points for a player averaging 10pts a game.  >> LastWeekRankingPerformance.html
FFToiletBowlSQL tblDefenseHandicappedExpectedVsActual | Histogram /header PctDeviationFromAverage PctDeviationFromForecast /number -200 -100 -60 -20 20 60 100 200 300 | plot HandicappedExpectedVsActualFromAvg.png true size 800 600 series "Delta Average" Line tostring 0 tonumber 1 1 series "Delta Forecast" Line tostring 0 tonumber 2 2 | AddImageToWordpress >> LastWeekRankingPerformance.html
echo ^<p^>Look below for view of players that are worse or better.  If the defense coefficient was major factor in predicting how the majority of players (market) will move from week to week, it's predictions will be more balanced than using the average.  >> LastWeekRankingPerformance.html
FFToiletBowlSQL tblDefenseHandicappedExpectedVsActual | Histogram /header PctDeviationFromAverage PctDeviationFromForecast /number 0 | plot HandicappedExpectedVsActualFromAvg.png true size 800 600 series "Delta Average" Line tostring 0 tonumber 1 1 series "Delta Forecast" Line tostring 0 tonumber 2 2 | AddImageToWordpress >> LastWeekRankingPerformance.html
echo ^<p^>Look below to see the middle 40%.  If there was some truth to be seen, our defense handicap should have a higher number in the middle 40%, where some sort of windfall or unstability didnt result in some huge, or dreadful performance.  >> LastWeekRankingPerformance.html
FFToiletBowlSQL tblDefenseHandicappedExpectedVsActual | Histogram /header PctDeviationFromAverage PctDeviationFromForecast /number -20 20 | plot HandicappedExpectedVsActualFromAvg.png true size 800 600 series "Delta Average" Line tostring 0 tonumber 1 1 series "Delta Forecast" Line tostring 0 tonumber 2 2 | AddImageToWordpress >> LastWeekRankingPerformance.html
echo ^<br^>^<i^>How to look at it: More players in bins closer to zero means our predictions are more precise.  But as long as it's even on both sides of 0, we are in the ballpark^</i^>  >> LastWeekRankingPerformance.html

echo ^<p^>Do players feast on bad defenses?  Or is it a famine?  Role of Linear Regression in a handicap. >> LastWeekRankingPerformance.html
FFToiletBowlSQL tblDefenseLinearHandicappedExpectedVsActual | Histogram /header PctDeviationFromAverage PctDeviationFromForecast /number -200 -100 -60 -20 20 60 100 200 300 | plot LinearHandicappedExpectedVsActualFromAvg.png true size 800 600 series "Delta Average" Line tostring 0 tonumber 1 1 series "Delta Forecast" Line tostring 0 tonumber 2 2 | AddImageToWordpress >> LastWeekRankingPerformance.html
FFToiletBowlSQL tblDefenseLinearHandicappedExpectedVsActual | Histogram /header PctDeviationFromAverage PctDeviationFromForecast /number 0 | plot LinearHandicappedExpectedVsActualFromAvg2.png true size 800 600 series "Delta Average" Line tostring 0 tonumber 1 1 series "Delta Forecast" Line tostring 0 tonumber 2 2 | AddImageToWordpress >> LastWeekRankingPerformance.html
FFToiletBowlSQL tblDefenseLinearHandicappedExpectedVsActual | Histogram /header PctDeviationFromAverage PctDeviationFromForecast /number -20 20 | plot LinearHandicappedExpectedVsActualFromAvg3.png true size 800 600 series "Delta Average" Line tostring 0 tonumber 1 1 series "Delta Forecast" Line tostring 0 tonumber 2 2 | AddImageToWordpress >> LastWeekRankingPerformance.html

PostNewToWordpress "How did we do with our rankings for week %LAST_GM%" < LastWeekRankingPerformance.html


rem
rem vwDefenseHandicappedExpectedVsActual










rem ----------------------------------
rem set LAST_GM=11
rem Past Performance ranking
FFToiletBowlSQL tblGrade | HtmlTable.exe 0 1 2 3 4 5 | PostNewToWordpress "Past performance ranking by POS for Week %LAST_GM%"
FFToiletBowlSQL tblFlexGrade | HtmlTable.exe 0 1 2 3 4  | PostNewToWordpress "Past performance ranking for Flex for Week %LAST_GM%"

rem Next week forecast - what points number do we use to when creating expected value for points scored using winpct
rem Anything using the word "expected", use rank
FFToiletBowlSQL tblFlexRank | plot HowManyPointsIsFlexWin.png true size 1000 700 series All_Positions Point tonumber 5 tonumber 6 3 | AddImageToWordpress.exe | PostNewToWordpress.exe "Does flex winpct imply our expected value from Player's average should be higher or lower?"











rem ----------------------------------
rem set LAST_GM=11

echo ^<p^>Fundamental Truths in a league where winning and losing is all important.  Every extra point over the next guy is exponentially important in producing wins.  And in fantasy, wins are what is important.  Look at a graph of a player's average points per game VS how many other players he's outperformed every week.  ^<p^>Over-supply vs. Shortage.  What can you drop down to, to get a scarcer resource.  My league starts 1QB, 2RB, 2WR, 1TE,1 Flex(RB,WR or TE), 1K, and 1DST.  For each starting slot, we are allowed to roster a backup.  My league has 14 teams this year, so that's 14 starting QB for instance.  Below is data, where you can assess if a position has a shortage, and which ones has a abundance that you may be willing to trade away and not really lose performance. > PositionShortageReports.html

rem pos bottom top spread totalpt players gms slot
rem DST	102	122	20	1829	17	10	0
FFToiletBowlSQL tblPositionPtPerformance | plot QualityOfPosition.png true size 600 400 subset "Starter" StackedColumn tostring 0 tonumber 3 4 7 0 subset "2nd Starter" StackedColumn tostring 0 tonumber 3 4 7 1 subset "Backup" StackedColumn tostring 0 tonumber 3 4 7 2   | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>^<i^>How to read: The spread indicates the points scored so far by the top player and the 14th player.  Inside the bar is a number of the total points scored by that those players.  If the spread is great, then there is a big difference between the 2 players.  If the number in bar is great, then that means that most of the players are the same level, compared to a bar with a smaller number.^</i^>  See below for the data in table format.  isbackup=0 means good enough to be starter.  isbackup=1 means good enough to be 2nd starter.  isbackup=2 means it player that would be a rostered as backup.  Ideally there would only be 14 players in a starter, but sometimes computer will rank 3 players as the same, so the 15,16,and 17th players are all ranked 17 and end up in the starter spot.  Not a big difference really, if they are ranked the same.   >> PositionShortageReports.html
FFToiletBowlSQL tblPositionPtPerformance | HtmlTable.exe 0 1 2 3 4 5 7 >> PositionShortageReports.html

rem create histogram of bins(0-5, 5-10, etc), pos, and numberof players who scored in that bin
echo ^<p^>Looking at it from another angle, you can see how each position scores it's points, before deciding on improving one position over another. >> PositionShortageReports.html
echo ^<p^>Have to start somewhere: Starting from how all the players who have scored in points in fantasy this year >> PositionShortageReports.html
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogram | plot PerformanceDistribution1.png true size 600 400 series "All Players" Column tostring 0 tonumber 2 2 | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Then: We see the share that each position shares in those performances.  WR, RB, and TE are outsized bc there are many more marginally performing players, while QB, K, DST position always starts the same player.  There are much less players at that position that come in and do one or 2 plays.  Same kicker kicks all FG.  DST always starts despite who get hurt.  And backup QB usually only play when the starter is hurt.  The other positions depend on the offensive package, and the play that the coach has called.  They are the main reason the 0 to 5, and 5 to 10 point bins are so outsized.  (Which statistic may be deceiving? Average pts/player@pos/gm, OR Average pts for all player@pos/gm?  Why?).  The Line graphs show the performances based-lined to 0, rather than slice of the whole universe.   >> PositionShortageReports.html
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplits | plot PerformanceDistribution2.png true size 600 400 subset "QB Slice" StackedColumn tostring 0 tonumber 2 2 3 QB subset "RB Slice" StackedColumn tostring 0 tonumber 2 2 3 RB subset "WR Slice" StackedColumn tostring 0 tonumber 2 2 3 WR subset "TE Slice" StackedColumn tostring 0 tonumber 2 2 3 TE subset "K Slice" StackedColumn tostring 0 tonumber 2 2 3 K subset "DST Slice" StackedColumn tostring 0 tonumber 2 2 3 DST subset "QB Distribution" FastLine tostring 0 tonumber 2 2 3 QB subset "RB Distribution" FastLine tostring 0 tonumber 2 2 3 RB subset "WR Distribution" FastLine tostring 0 tonumber 2 2 3 WR subset "TE Distribution" FastLine tostring 0 tonumber 2 2 3 TE subset "K Distribution" FastLine tostring 0 tonumber 2 2 3 K subset "DST Distribution" FastLine tostring 0 tonumber 2 2 3 DST | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Focus: Here we change the graph for RB, WR, TE, to make each category standout more.  You can tell there are a lot of RB, WR, TE who do bit offensive category roles for their teams.  Kickers and Defense perform in a tight distribution area.  QB perform at a higher, but slightly more distributed area. >> PositionShortageReports.html
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplits | plot PerformanceDistribution3.png true size 800 600 subset "WR Distribution" Area tostring 0 tonumber 2 2 3 WR subset "RB Distribution" Area tostring 0 tonumber 2 2 3 RB subset "TE Distribution" Area tostring 0 tonumber 2 2 3 TE subset "QB Distribution" Line tostring 0 tonumber 2 2 3 QB subset "K Distribution" Line tostring 0 tonumber 2 2 3 K subset "DST Distribution" Line tostring 0 tonumber 2 2 3 DST | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Refinement: Let's weed out players that didn't make our Rankings lists >> PositionShortageReports.html
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsRankedOnly | plot PerformanceDistribution4.png true size 800 600 subset "WR Distribution" Area tostring 0 tonumber 2 2 3 WR subset "RB Distribution" Area tostring 0 tonumber 2 2 3 RB subset "TE Distribution" Area tostring 0 tonumber 2 2 3 TE subset "QB Distribution" Line tostring 0 tonumber 2 2 3 QB subset "K Distribution" Line tostring 0 tonumber 2 2 3 K subset "DST Distribution" Line tostring 0 tonumber 2 2 3 DST | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Hindsight: If we included the performance of players that were in lineups this year, and compare them to the performances of players that should be in starting lineups this year, would it mean that we pick the best players?  No, in our case, it is more a matter of our ability to draft well.  Those fantasy owners that do, can hold good players on the bench, away from the starting lineups of other owners.  The Rank distribution is a hypothetical one based on premise that we all drafted the best performing player for the future 16 weeks (13 reg fantasy season + 3 playoffs) at draft time.  >> PositionShortageReports.html
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsStarter | plot PerformanceDistribution5.png true size 1800 800 subset "WR in Lineup" Area tostring 0 tonumber 2 2 4 WRinlineup subset "RB in Lineup" Area tostring 0 tonumber 2 2 4 RBinlineup subset "K in Lineup" Area tostring 0 tonumber 2 2 4 Kinlineup subset "DST in Lineup" Area tostring 0 tonumber 2 2 4 DSTinlineup subset "QB in Lineup" Area tostring 0 tonumber 2 2 4 QBinlineup subset "TE in Lineup" Area tostring 0 tonumber 2 2 4 TEinlineup subset "QB  Ranked Starter" Line tostring 0 tonumber 2 2 4 QBrankedstarters subset "RB  Ranked Starter" Line tostring 0 tonumber 2 2 4 RBrankedstarters subset "WR  Ranked Starter" Line tostring 0 tonumber 2 2 4 WRrankedstarters subset "TE  Ranked Starter" Line tostring 0 tonumber 2 2 4 TErankedstarters subset "K  Ranked Starter" Line tostring 0 tonumber 2 2 4 Krankedstarters subset "DST Ranked Starter" Line tostring 0 tonumber 2 2 4 DSTrankedstarters   | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Point distribution for Players in Fantasy Lineups this year, split by position.
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsStarter | plot PerformanceDistribution11.png true size 1800 800 subset "WR in Lineup" Line tostring 0 tonumber 2 2 4 WRinlineup subset "RB in Lineup" Line tostring 0 tonumber 2 2 4 RBinlineup subset "K in Lineup" Line tostring 0 tonumber 2 2 4 Kinlineup subset "DST in Lineup" Line tostring 0 tonumber 2 2 4 DSTinlineup subset "QB in Lineup" Line tostring 0 tonumber 2 2 4 QBinlineup subset "TE in Lineup" Line tostring 0 tonumber 2 2 4 TEinlineup | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Point distribution for Players that should be hypothetically starting, split by position.
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsStarter | plot PerformanceDistribution12.png true size 1800 800 subset "QB  Ranked Starter" Line tostring 0 tonumber 2 2 4 QBrankedstarters subset "RB  Ranked Starter" Line tostring 0 tonumber 2 2 4 RBrankedstarters subset "WR  Ranked Starter" Line tostring 0 tonumber 2 2 4 WRrankedstarters subset "TE  Ranked Starter" Line tostring 0 tonumber 2 2 4 TErankedstarters subset "K  Ranked Starter" Line tostring 0 tonumber 2 2 4 Krankedstarters subset "DST Ranked Starter" Line tostring 0 tonumber 2 2 4 DSTrankedstarters   | AddImageToWordpress >> PositionShortageReports.html

echo ^<p^>QB, comparing fantasy lineup Pt distribution vs hypothetical starter
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsStarter | plot PerformanceDistribution5.png true size 600 400 subset "QB in Lineup" Column tostring 0 tonumber 2 2 4 QBinlineup    subset "QB  Ranked Starter" Line tostring 0 tonumber 2 2 4 QBrankedstarters | AddImageToWordpress  >> PositionShortageReports.html
echo ^<p^>RB, comparing fantasy lineup Pt distribution vs hypothetical starter
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsStarter | plot PerformanceDistribution6.png true size 600 400 subset "RB in Lineup" Column tostring 0 tonumber 2 2 4 RBinlineup    subset "RB  Ranked Starter" Line tostring 0 tonumber 2 2 4 RBrankedstarters | AddImageToWordpress  >> PositionShortageReports.html
echo ^<p^>WR, comparing fantasy lineup Pt distribution vs hypothetical starter
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsStarter | plot PerformanceDistribution7.png true size 600 400 subset "WR in Lineup" Column tostring 0 tonumber 2 2 4 WRinlineup    subset "WR  Ranked Starter" Line tostring 0 tonumber 2 2 4 WRrankedstarters | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>TE, comparing fantasy lineup Pt distribution vs hypothetical starter
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsStarter | plot PerformanceDistribution8.png true size 600 400 subset "TE in Lineup" Column tostring 0 tonumber 2 2 4 TEinlineup    subset "TE  Ranked Starter" Line tostring 0 tonumber 2 2 4 TErankedstarter | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>K, comparing fantasy lineup Pt distribution vs hypothetical starter
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsStarter | plot PerformanceDistribution9.png true size 600 400 subset "K in Lineup"  Column tostring 0 tonumber 2 2 4 Kinlineup    subset "K  Ranked Starter" Line tostring 0 tonumber 2 2 4 Krankedstarters | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>DST, comparing fantasy lineup Pt distribution vs hypothetical starter
FFToiletBowlSQL vwPtsPerPlayerPerGameHistogramPosSplitsStarter | plot PerformanceDistribution10.png true size 600 400 subset "DST in Lineup" Column tostring 0 tonumber 2 2 4 DSTinlineup subset "DST  Ranked Starter" Line tostring 0 tonumber 2 2 4 DSTrankedstarters | AddImageToWordpress >> PositionShortageReports.html

rem Flex Performance vs Ranking (Week weighted wins)
echo ^<p^>Position - Season Long Wins Ranking >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot QBWinRanking.png true size 600 400 subset "QB" Bar tonumber 0 tonumber 6 3 2 QB | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot RBWinRanking.png true size 600 800 subset "RB" Bar tonumber 0 tonumber 6 3 2 RB | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot WRWinRanking.png true size 600 800 subset "WR" Bar tonumber 0 tonumber 6 3 2 WR | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot TEWinRanking.png true size 600 400 subset "TE" Bar tonumber 0 tonumber 6 3 2 TE | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot K_WinRanking.png true size 600 400 subset "K" Bar tonumber 0 tonumber 6 3 2 K | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot DSWinRanking.png true size 600 400 subset "DST" Bar tonumber 0 tonumber 6 3 2 DST | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Flex - Season long Wins Ranking >> PositionShortageReports.html
rem Flex Performance vs Ranking (season wins)
FFToiletBowlSQL tblFlexRank | plot FlexWinsRanking.png true size 600 1200 series "Flex" Bar tonumber 0 tonumber 7 3 | AddImageToWordpress >> PositionShortageReports.html
rem Flex Performance vs Ranking (season pts)
echo ^<p^>Position - Season Long Pts Ranking >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot QBPtsRanking.png true size 600 400 subset "QB" Bar tonumber 1 tonumber 7 3 2 QB | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot RBPtsRanking.png true size 600 800 subset "RB" Bar tonumber 1 tonumber 7 3 2 RB | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot WRPtsRanking.png true size 600 800 subset "WR" Bar tonumber 1 tonumber 7 3 2 WR | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot TEPtsRanking.png true size 600 400 subset "TE" Bar tonumber 1 tonumber 7 3 2 TE | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot K_PtsRanking.png true size 600 400 subset "K" Bar tonumber 1 tonumber 7 3 2 K | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot DSPtsRanking.png true size 600 400 subset "DST" Bar tonumber 1 tonumber 7 3 2 DST | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Flex - Season long Points Ranking >> PositionShortageReports.html
rem Flex Performance vs Ranking (season points)
FFToiletBowlSQL tblFlexRank | plot FlexPtsRanking.png true size 600 1200 series "Flex" Bar tonumber 1 tonumber 8 3 | AddImageToWordpress >> PositionShortageReports.html

echo ^<p^>Position - Ranking using the week number as weighting for ^<i^>Wins^</i^>, so later games weigh more heavily >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot QBLWinRanking.png true size 600 400 subset "QB" Bar tonumber 8 tonumber 9 3 2 QB | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot RBLWinRanking.png true size 600 800 subset "RB" Bar tonumber 8 tonumber 9 3 2 RB | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot WRLWinRanking.png true size 600 800 subset "WR" Bar tonumber 8 tonumber 9 3 2 WR | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot TELWinRanking.png true size 600 400 subset "TE" Bar tonumber 8 tonumber 9 3 2 TE | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot K_LWinRanking.png true size 600 400 subset "K" Bar tonumber 8 tonumber 9 3 2 K | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot DSLWinRanking.png true size 600 400 subset "DST" Bar tonumber 8 tonumber 9 3 2 DST | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Flex - Ranking using the week number as weighting, so later games weigh more heavily >> PositionShortageReports.html
rem Flex Performance vs Ranking (Week weighted wins)
FFToiletBowlSQL tblFlexRank | plot FlexLWinsRanking.png true size 600 1200 series "Flex" Bar tonumber 9 tonumber 10 3 | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>Position - Ranking using the week number as weighting for ^<i^>Points^</i^>, so later games weigh more heavily >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot QBLPtsRanking.png true size 600 400 subset "QB" Bar tonumber 10 tonumber 11 3 2 QB | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot RBLPtsRanking.png true size 600 800 subset "RB" Bar tonumber 10 tonumber 11 3 2 RB | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot WRLPtsRanking.png true size 600 800 subset "WR" Bar tonumber 10 tonumber 11 3 2 WR | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot TELPtsRanking.png true size 600 400 subset "TE" Bar tonumber 10 tonumber 11 3 2 TE | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot K_LPtsRanking.png true size 600 400 subset "K" Bar tonumber 10 tonumber 11 3 2 K | AddImageToWordpress >> PositionShortageReports.html
FFToiletBowlSQL tblRank | plot DSLPtsRanking.png true size 600 400 subset "DST" Bar tonumber 10 tonumber 11 3 2 DST | AddImageToWordpress >> PositionShortageReports.html
rem Flex Performance vs Ranking (Week weighted pts)
FFToiletBowlSQL tblFlexRank | plot FlexLPtsRanking.png true size 600 1200 series "Flex" Bar tonumber 11 tonumber 12 3 | AddImageToWordpress >> PositionShortageReports.html
echo ^<p^>^<i^>How to read: The vertical axis indicates his rank according to the metric.  So the better players are at bottom.  The horizontal axis indicates their performance relative to the top and bottom performers in the category.^</i^>   >> PositionShortageReports.html

PostNewToWordpress "Position Shortage Report for Week %LAST_GM%"< PositionShortageReports.html












rem ----------------------------------
rem terminate projections and MVP analysis after week 13
if %LAST_GM% gtr 13 exit /b 0


rem after week 13 is loaded, show end of season awards... 
rem 1. was there a toilet bowl 2? Was there a meeting of the 2 worst that season?
rem 2. who was the luckiest team  
rem     - would they fared better, if they faced a different team that week... brute force winpct of team(x) vs actual winpct of team(y) for 13 weeks, not facing the same team.
rem     - points scored(x) vs winpct of team(y)
rem 3. who was the unluckiest team
rem     - did most points scored against mean all the good players went somewhere else?
rem Most consistent team
rem Most inconsistent team
rem Hindsight, draft position vs ranking

rem Jeremy Langford problem
rem ... Backups who are extremely effective in taking over for the starter in short spells
rem     end up higher ranked, than players who play a lot
rem ... so the system has to take into account how many actual games he has played
rem ... but the current system penalizes injuries (or games not played) so heavily, it creates a ranking that benefits positions where a player stays healthy, ie.QB.  
rem ... But if a single player stays healthy at RB or WR, it is apparent that that player produces a higher win pct over QB.
rem 1) We can determine the most effective player when they played
rem 2) We can determine the most effective player over the season, by marking 0 for games they missed
rem 3) We have no system of determining what value to assign when he missed a game
rem    - We can pull a random player from the backup ranks to also add what kind of effect his injury would had on team
rem datasets
rem A. Who made the most of their opportunities
rem B. who was full season best performing player? ...creates a ranking full-season
rem C. Who should have been flex? ...should be based on full-season testing, bc costing at a full season measurement
rem D. How many wins did a player add, when he was starting... meaning 
rem E. Redo most valuable player, taking into account the drop off into backup(based on A's 2nd tier) territory

set STAT_SIGNIFICANT=4

rem dataset A
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<h1^>Do we have our MVP candidates?</h1^> > EndOfSeasonMVPcandidates.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% TwoTeamMonteCarlo -threads 4
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<p^>Which player played a important role in his team's success?  In fantasy football, this is answered by asking what would happen to a team, if they didn't have that player vs having that player.  But as things go, there are trade-offs.  Here we will assess the value of that player, when he plays.  If they are in the season-long list as well that should qualify them as a MVP season.  And here are our MVP candidates as chosen by the computer. >> EndOfSeasonMVPcandidates.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% FFToiletBowlSQL vwGameDayMonteCarloResults | HtmlTable 1 4 5 6 7 36 >> EndOfSeasonMVPcandidates.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% PostNewToWordpress "After week %LAST_GM%, who made the most of their opportunities when they played?"< EndOfSeasonMVPcandidates.html
rem dataset B
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<h1^>How important is health to player's value at draft time?^</h1^> > EndOfSeasonDraftHindsight.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% TwoTeamMonteCarlo Season -threads 4
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<p^>This is extremely difficult to quantify.  It largely depends on the cost of subsequently drafting his backup/handcuff versus having to draft a open starting spot.  Can you wait after all your starting positions are filled, is that position so critical that you have to fill early, or are the players that are left of the same value that you can still wait a round and a similar player will still be there?  We start with trying to assess the difference being healthy has on a player's stats.  This might be misleading because, it penalizes missing games heavily without assessing what his replacement could do.  So without much ado, here is the importance of each player for his fantasy team for a full season. >> EndOfSeasonDraftHindsight.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% FFToiletBowlSQL vwGameDayMonteCarloResults | HtmlTable 1 4 5 6 7 36 >> EndOfSeasonDraftHindsight.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% PostNewToWordpress "After week %LAST_GM%, who contributed for the entire season for their fantasy team's success?"< EndOfSeasonDraftHindsight.html
rem dataset D
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<h1^>Who should've been drafted this year^</h1^> > EndOfSeasonWhoShouldveBeenDrafted.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% TwoTeamMonteCarlo Roster -threads 4
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<p^>We know from the last post who played the most, and therefore should be ideal candidates for starting spots.  But unlike the previous simulation, we know that there aren't such a large universe of players, to do game simulations.  Our universe in my league is limited by how many roster spots we can hold.  We re-run the simulation with only players that would've been rosterable based on previous simulation, and try to determine which of those players is flex material.  Players ranked lower should be considered, rostered as backups. >> EndOfSeasonWhoShouldveBeenDrafted.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% FFToiletBowlSQL vwGameDayMonteCarloResultsPlusIdealFlex | HtmlTable 1 4 5 6 7 36 37 38 39 >> EndOfSeasonWhoShouldveBeenDrafted.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% PostNewToWordpress "After week %LAST_GM%, the flex, the DMZ between starter and backup"< EndOfSeasonWhoShouldveBeenDrafted.html
rem dataset D
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<h1^>Can we quantify the value of a player to a fantasy team?^</h1^> > EndOfSeasonHowManyWinsCanSinglePlayerAdd.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% TwoTeamMonteCarlo Starter -threads 4
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<p^>In a 13 game fantasy season, a win is 0.077 to their winning percentage.  The hypthothesis of this analysis is that when the player in this list plays, if he is 0.077 higher than a player ranked 14 places later, he will average one extra win over team selecting the player ranked 14 places later.  So the differences in Winpct of all the players faced, that would be the predicted record for that team.  The advantage of this analysis, is that it will include any value that a scarcity of a position will have (if any), whereas the previous ranking systems assess who is the best player at a position, relative to the top performer.  That's not saying a player is better than another, but given this year's crop of performances, who was more valuable on a fantasy team this year.  And here we list who were the most effective starters.  >> EndOfSeasonHowManyWinsCanSinglePlayerAdd.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% FFToiletBowlSQL vwGameDayMonteCarloResults | HtmlTable 1 4 5 6 7 2 36 >> EndOfSeasonHowManyWinsCanSinglePlayerAdd.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% PostNewToWordpress "After week %LAST_GM%, can we tell how much more valuable a player was over an other?"< EndOfSeasonHowManyWinsCanSinglePlayerAdd.html
rem dataset E
rem not here yet...


REM  Get exception for missing data
REM  Name cannot be matched between ESPN and FFToday data
FFToiletBowlSQL vwFFTodayEspnPlayerReconcile > FFTodayEspnException.tsv
for /f %%i in ("FFTodayEspnException.tsv") do set size=%%~zi
if %size% gtr 93 (
    echo vwFFTodayEspnPlayerReconcile Not empty
	exit /b 1
)

REM  There are players started by owners in City Island, that aren't top players in their position
FFToiletBowlSQL vwCityIslandMatchupException | Select.exe PlayerID > MonteExceptionReport.tsv

REM missing data, refeed into Monte, as file input, Starter -include
if %LAST_GM% gtr %STAT_SIGNIFICANT% TwoTeamMonteCarlo Starter -includefromfile MonteExceptionReport.tsv -threads 4




rem ----------------------------------
REM if %LAST_GM% gtr %STAT_SIGNIFICANT% TwoTeamMonteCarlo All
rem dataset B

rem this dataset updates win count
FFToiletBowlSQL vwCityIslandMatchupsSummary

if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<h1^>How well did we quantify a player's contribution to a fantasy team. ^</h1^> > AreWeHavingFunYet.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<p^>Look for yourself. Monte Win Pct is the Winning Pct that we calculated for a player.  We averaged it out here, bc the Distributive Law can be applied for our model.  Hypothesis was if we took the differences of the winpct between the 2 teams that would tell us how many extra wins that the other team would have over the other, if they played each other all season.  We're interpreting that as how many games over 0.500.  And we applied it to every team on the schedule, to get the difference, and we got an expect number of wins.  >> AreWeHavingFunYet.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% FFToiletBowlSQL vwCityIslandMatchupsExpectedWins | HtmlTable 1 3 4 8 10 2 11 >> AreWeHavingFunYet.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<p^>You're probably wondering: what is the difference between this winning percentage, and the WINS percentage ranking.  Wins percentage ranking, can be thought of the probability that a player will outscore another (if conditions were exactly the same when his games were played).  Remember the checkerboard dartboard?  We can still use the checkboard analogy for Monte win pct, but it gets more complicated.  So the player's stats are now part of a portfolio, and we want to know the odds that that portfolio outscores another.  Now our player also has to compensate or add to deficiencies to other players in the lineup to produce a win.  Let's say we have a hypothetical lineup with only a QB and RB, and they have played 3 games, and he's matched up with another owner's lineup with the same positions.  And we believe (because we are naive, or lazy) that they will only score those 3 outcomes again for the 4th game.  Next is NOT another checkerboard dartboard, but how we come about producing possible outcomes.  ^<table^>^<tr^>^<td^>RB\QB^<td^>5^<td^>10^<td^>5^<tr^>^<td^>3<td^>8^<td^>13^<td^>8<tr^>^<td^>10<td^>15^<td^>20^<td^>15<tr^>^<td^>15<td^>20^<td^>25^<td^>15^</table^>  The row and column headers contain the scores produced by one team's hypothetical lineup with QB scores at column headers and RB scores at row headers.  The inside represents the possible score possibilities/outcomes for that lineup.  To do the dartboard analogy again, we have to take all the score possibilities for one team, and put it in a another checkerboard dartboard's column header, and put the other team's in the row header.  But you see how with a lineup of just 2 positions, and 3 games played, that is already a 9 by 9 grid.  Now say we have 9 positions per team, and at least 10 games played, how many in that grid?  1trillion column by 1 trillion rows.  And what if did this for all combinations of 14 teams?  I'm too lazy to figure that out.  So we just took a random sample of lineups.  Players where they were part of matchups where their team won and loss about the same, we describe as average player.  Theroetically, 2 random lineups should win and lose about the same number.  But players whose teams won more, we believe their contribution in points added to those wins.  So a win pct over .500 is their personal contribution.  >> AreWeHavingFunYet.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<p^>Our contention is that players with a Monte Winning Pct of 0.500 neither add nor contribute to their team's winning percentage more than the average player.  That's not to say they are worthless.  Take them out of the line up and that winning percentage turns to 0.000.    >> AreWeHavingFunYet.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<p^>What if one team's Monte total winpct has a difference of 0.077 with another team's total Monte win pct?  Does that guarantee a win?  Sadly, no.  But if they played 13 games against each other all season, and their lineups scored what they did before, then the team with the extra 0.077, should have one extra win over the other, out of 13 games (in other words a record of 7-6, vs a 6-7 record).  >> AreWeHavingFunYet.html
rem dataset A
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<h1^>My league's fantasy matchups^</h1^> >> AreWeHavingFunYet.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% echo ^<p^>Here are the actual outcomes of matchups in my league.  >> AreWeHavingFunYet.html
if %LAST_GM% gtr %STAT_SIGNIFICANT% FFToiletBowlSQL vwCityIslandMatchupsSummary | HtmlTable 0 3 5 7 4 6 8 9 >> AreWeHavingFunYet.html

echo ^<p^>^<h3^>A little extra explanation on the Monte winning pct.^</h3^> >> AreWeHavingFunYet.html
echo ImagineThatEachPlayerDoesThis.png | AddImageToWordpress >> AreWeHavingFunYet.html
echo Imagine that the average player scores 20pts/week.  That is what the diagram displays.  Add 4pts to each bin, and let's say that's Tom Brady's score distribution.  The average player in the Monte win pct, is 0.500.  Let's say Tom Brady is 0.63.  If we dropped 100 balls in each the average player's bin, and 100 in Tom Brady's bin.  And we subtracted balls in average player's bin, from Tom Brady's bin, and vice versa.  Tom Brady should have 13 extra balls in bins where his scores are higher than the average player.  That is what I believe the Monte numbers are to be interpreted.  >> AreWeHavingFunYet.html
echo ^<p^>Now imagine that the average player scores 20pts/week.  Now let's say he has a team mate.  So is the team going to score 20 or 40 pts?  Obviously 40pts.  What if they face a team that averages 21pts a game, each?  That means they will average 42pts a week.  Does this mean our intrpeid first team that they are doomed to lose this game because 42>40?  Obviously no, because they dont always score 40pts, and the other doesnt always score 42pts.  That's where the difference in Monte winning pct comes in.  >> AreWeHavingFunYet.html
echo Whatifwedopercentages.png | AddImageToWordpress >> AreWeHavingFunYet.html 
echo ^<p^>Let's say instead of pts, we say both players on first team are Monte 0.500.  They are average players.  If they faced other average players on another "Monte AVERAGE" team (comprised of monte average players), they have an equal chance of winning or losing.  But the other team in our example is better than the average team.  Let's say each player on second team is 0.510, or 0.10 better than a player on the first team.  Does this mean that they have a 0.51 or 51% chance of beating the other team?  Remember that using points, we add the points up, to get a new average pts total FOR THE TEAM.  So if we apply the same logic, the team's winning percentage should be increased to 52% chance of winning or the difference between the 2 players(0.010/each * 2 players = 0.020 difference, higher probablility to outscore the other team), which is supposed to how much more likely they are to outscore the other team.  Which in this case, the other team is our hypothetical Monte average 0.500 winning percentage team. But a similar case can be made for team that aren't average.  Because they are both measured from this hypothetical average player.     >> AreWeHavingFunYet.html
echo ^<p^>You might ask who exactly is the hypothetical Monte Average player?  It's whatever player comes out of the computer simulation with a winning pct of 0.500.  It means whenever the computer put him in a lineup, he was just as likely to win the game, as he was to lose the game, against another random lineup.  He was even steven.  And this changes depending on how good the other players are, because better Monte Players score more, and more often, than the average Monte player, teams that they are on, tend to win slightly more (See above for what good position WINS pct players have to over come, once they are put into a monte simulation).  There may not even be a real average player that has winning pct of 0.500.  But we could probably make up a player and his stats, where if we re-ran the simulation, he would be 0.500.  However, the computer simulation, is just that, a simulation.  There is a degree of error involved.  As well as in real life, past outcomes don't guarantee that the future outcomes will turn out exactly out same way.  So a player at 0.503, might as well be considered our Monte 0.500 average player.    >> AreWeHavingFunYet.html

echo ^<p^>There is an relationship to Wins Pct and the Monte Probability Percentages? Even if we subtract 0.500 from WinsPct, divide by number of sqrt positions (this seems to get it closer to Monte, than by starting slots of 9) on a team, and re-add to 0.500 to get closer approximation to a player's contribution to his portfolio beating another portfolio.  But still the relationship isnt perfect.  So we'll continue examining if we can use WINS pct as a faster approximation of Monte Percentage.  (Using Monte Percentage, we come up with an expected number of wins that is pretty close actual.  The trick is, how accurate are our forecasted distributions.  Remember we are using nothing but point scored distribution to determine win pct for one team vs another).  >> AreWeHavingFunYet.html
FFToiletBowlSQL vwCompareMonteToWinsPct | plot CompareMonteToWinsPct.png   true size 1000 800 series "All Wins Ranked Positions" Point tonumber 5 tonumber 4 0 line 45 45 55 55 | AddImageToWordpress >> AreWeHavingFunYet.html


if %LAST_GM% gtr %STAT_SIGNIFICANT% PostNewToWordpress "Week %LAST_GM%, fantasy stretch for owners"< AreWeHavingFunYet.html
if %LAST_GM% gtr 13 PostNewToWordpress "Week %LAST_GM%, playoffs for owners"< AreWeHavingFunYet.html







rem ------------------------------------
rem Show current week's lineups
rem show who should be in, if first 4 weeks distribution is representative of player's future performance.










rem ----------------------------------
rem terminate script after rankings... no need for forecasts for play-offs... you have what you have
if %LAST_GM% == 13 echo Good Luck. By now, you got what you got.  It's up to the fantasy gods now.  | PostNewToWordpress "Regular season is over.  Hope you got in the playoffs!"
rem calculate best 8 teams and the favorite to win it all (the lottery)

if %LAST_GM% gtr 16 exit /b 0











rem ----------------------------------
rem Handicaps applied to offense bc of opposing defense
rem set LAST_GM=13
echo Effect of Defense from a player's average for Week %LAST_GM% > TeamDefense.html
echo ^<p^>There is obvious certain teams that have an effect on a player's fantasy performance, or this statistic wouldn't have such a trend.  But the effect has a great deal of variation that the predictive value for a single game has limited utility.  Plus the percentage change indicated is usually so small, that other effects can overwhelm the trend created by the defense.  But as more and more games are played.  It is obvious that players will perform above or below their season's average consistently when facing them.  But that doesn't mean, that a fluke play here or there, or a well executed game plan by a team's coach won't skew the stats one or two games.  However, it should be noted that the best effect averaged out for a team is +/-30^% from their average.  If the player is good they average 20pts/gm.  That is only 6 pts increase or decrease predicted by our model based on if they face a good or bad defense.  A practical illustration is : A break-out game out of now-where can be a +20pts over their average.  Or an injury results in a -15pts from season average.  So don't put all your eggs in one basket.  >> TeamDefense.html
echo ^<p^>One charts is QB, WR,TE bc those are passing statistics.  If QB numbers go up, the WR and/or TE should also go up.  It is possible for WR to go down, and TE's up, if a team guards WR extremely well, or a game plan is to avoid those strengths.  But as you can see, that rarely happens.  TE's generally average less per game, so a point increase probably increases the percentage higher.  >> TeamDefense.html
echo ^<p^>The other charts are divided into RB, K.  The effect on RB chart also includes QB, as a contrast because we are using QB's as an analog of the entire passing game.  That you can see if a team does well in defending one category, but not so well in the other.  This might just as well be because they are so bad in one category that the opposing team doesn't bother with the other category.  It happens.    >> TeamDefense.html

rem Passing handicap
FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot QBDefenseHandicap.png true size 1200 800 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset WR_Performance Column tostring 0 tonumber 10 -1 1 WR subset TE_Performance Column tostring 0 tonumber 10 -1 1 TE subset "Passing StDev" FastLine tostring 0 tonumber 11 -1 1 QB | AddImageToWordpress >> TeamDefense.html
rem Running, K, DST handicap
FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot RBDefenseHandicap.png  true size 900 400 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset "RB Performance" Column tostring 0 tonumber 10 -1 1 RB subset "RB +/- StDev" FastLine tostring 0 tonumber 11 -1 1 RB| AddImageToWordpress >> TeamDefense.html
FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot KDefenseHandicap.png   true size 600 400 subset "K Performance" Column tostring 0 tonumber 10 -1 1 K| AddImageToWordpress >> TeamDefense.html

rem Show the team data in charts
FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | HtmlTable true "Handicap Team applies to Opposing Position" "Teams" 0 1 2 3 4 5 6 7 8 9 10 11 12  >> TeamDefense.html

rem Show player forecasts with handicaps applied, based on opposing team faced 
echo ^<p^>Below is a table of fantasy relevant players, with an handicap applied to them, based on the opposition defense they are facing this week.   >> TeamDefense.html
FFToiletBowlSQL vwDefenseHandicappedForecast | htmltable true 0 1 2 3 6 15 16 17 18 25 32 >> TeamDefense.html

rem show the tendency for a player to react to a defense. 
rem ie, y=a+by... get b for a player.  calculate new forecast.  determine performance report and move it up.
rem Show player forecasts with handicaps applied, based on opposing team faced 
echo ^<p^>We assume that certain players react paricularly bad or good depending on the defense they face.  For those players, we adjust their handicap based on a linear regression of past defenses they faced.  If their history indicates they fit "the feasting on bad defense/famine during good defense" model, we use that forecast number instead.  Below is a table of fantasy relevant players, with an handicap applied to them, based on the opposition defense they are facing this week.  The feast or famine model, also predicts if a player really doesn't react to a defense.  Or if he acts in reverse, perhaps another player gets even bigger share of the pie when they face a bad defense, or best player is double covered. >> TeamDefense.html
FFToiletBowlSQL vwDefenseHandicappedForecastWithBeta | htmltable true 0 1 2 3 6 15 16 17 18 25 34 36 37 >> TeamDefense.html

PostNewToWordpress.exe "Defensive handicap for week %LAST_GM%" < TeamDefense.html

rem 3 models so far... 1) average only 2) average w/ defense applied 3) average w defense applied and player's performance vs defense
rem 4th model involves portfolio 4) monte 
rem future models 4) 





rem ----------------------------------
rem get pundit predictions

rem better than average
rem have the players picked produced better than average?  by how much?  enough to be a starter?

rem performance prediction
rem is it better than average?
rem is it better than yours?





rem ------------------------------------
rem *** XXX *** XXX ***
rem indoor / outdoor stadium splits
rem performance in month
rem performance based on rain. temp, cloudiness, stadium
rem player performance during months
rem player performance during months, in certain stadiums
rem gameday to weather condition on a particular day
rem treat weather as defense (use stadium too, if there is considerable difference, ie. domed vs green bay in nov dec)
rem http://www.wunderground.com/history/airport/KTEB/2015/11/7/DailyHistory.html?req_city=East+Rutherford&req_state=NJ&req_statename=New+Jersey&reqdb.zip=07073&reqdb.magic=1&reqdb.wmo=99999
rem https://en.wikipedia.org/wiki/List_of_current_National_Football_League_stadiums
rem http://www.pro-football-reference.com/years/2002/games.htm
rem use weather forcast to create a handicap for future gameday.  do 96% of score during snow * 80% of score versus team.  Or 105% of games in dome * 90% of stats vs team.

rem performance of one coach vs another
rem 






rem -------------------------------------
rem download play by plays

rem histogram by down and distance, redzone
rem when they go for it on 4th, what is distance?  time?
rem punt pull from samples
rem if in end zone td
rem if past kicking distance, kick

see point histogram
    number of points scored by a team, should be higher by humans, assuming they know optimal reordering
	meaning humans can reorder the plans for success, whereas randomly it should succeed less.
	check how many points actually scored, is in which win... and the percentile of that bin
if compared against another team, what is win pct  








rem -------------------------------------
rem pre-season/offseason work
rem life cycle of career of each position
rem year vs pts scored 
rem year vs pts scored vs distrubtion for that year
rem divide players by who they are now... RB1 material, WR2 material, etc.  see if there are indicators in past seasons.

rem the chance a player misses a game in a season... presumably bc of injury
rem take profile of player to determine start
rem take year of first stat
rem calculate weeks missing stats
rem see if there is injury report for each week, and see if it corresponds
rem determine number of games expected to miss in a season
rem determine number of touches in a season expected before injury


rem at what point in the season, are the past performance, most indicative of the performance in the rest of the season
rem at what point in a career, is past performance, most indicative of performance in rest of career

rem close to last season's stats, are this season's stats?

