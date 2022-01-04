<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InjuryModelData.aspx.cs" Inherits="FFToiletBowlWeb.InjuryModelData" MasterPageFile="~/Site.Master" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h1><b><u>Rate of injury (or Injuries you should expect, for each number of plays)</u></b></h1>
    <div>
        For each position, this chart shows injuries that were reported where a player was taken out or ruled out.
        <b>The x-axis shows the running count of plays</b> that were recorded for this player, or PaAtt+RuATT+ReTgt+KiAtt.
        That number represents numerical analog of wear and tear.  The <b>y-axis shows the running count of each time
        a player was ruled out</b>.  It only creates a point when the player was ruled out, and shows the number of plays
        leading up to that point.  
        <p>This doesn't translate to risk of injury per game, 
        bc each player has different usage, or number of plays per game.
        Which is fine, bc players also have different injury rate per play, as you can tell from following certain groupings
        of points on the chart.  Better player you are, more plays, and consequently more discrete instances of injury.  
        Each player will be given their own injury risk model, per game, on another page</p>
        <p>I am not trying to model a cumulative effect here, where injury causes more injury.  
        That probably would be trying to fit a polynomial 
        (or the results of linear fit model here, re-plotted as input into another linear regression vs injury)</p>
        <p>The purpose here, is to demonstrate that there is a defined risk of losing stats, as a result of injury 
        and that this can be translated as a percentage that represents future risk, like rolling a pair dice, 
        where the first die that stops is if it's a 6 and that means injury and the 2nd die that stops is number of games,
        and therefore losing stats and therefore fantasy points</p>
        <p>It can also be improved bc it assumed all plays result in injury equally, but this is offset someone by the positions.
        Receivers catch, running backs run, and quarterbacks pass. And very few, actually do multiple categories.  
        However, QB's do run now.  And sacks aren't part of the PaAtt stat, so the most likely indicator of
        QB injury isn't even in the stats I have, but you have the code, if you want see if you can find and add on
        the count of how many times a qb has been sacked, you can try this yourself.</p>
        <p>It is easier to read the chart, if you deactivate the other positions and just leave one position visible.  
        Click on the legend to make other positions invisible. Esp deactivate the QB position to view the other positions.  
        QB's get a lot of PaAtt throughout their long careers, and their data de-empahsizes everyone else's in a small cluster when viewed together.</p>
        <div><canvas id="myChart" style="width:800px;height:600px;"></canvas></div>

        <hr />
        <a name="top"></a>
        <a href="#qbinjuries">QB Injuries Data</a><br/>
        <a href="#rbinjuries">RB Injuries Data</a><br/>
        <a href="#wrinjuries">WR Injuries Data</a><br/>
        <a href="#teinjuries">TE Injuries Data</a><br/>
        <a href="#kinjuries">K Injuries Data</a><br/>
        <br/>
        <a href="#qbfinalrate">QB Last Injury Rate</a><br/>
        <a href="#rbfinalrate">RB Last Injury Rate</a><br/>
        <a href="#wrfinalrate">WR Last Injury Rate</a><br/>
        <a href="#tefinalrate">TE Last Injury Rate</a><br/>
        <a href="#kfinalrate">K Last Injury Rate</a><br/>
        <br/>
        <a href="#qbmodel">QB Injuries/Plays linear model</a><br/>
        <a href="#rbmodel">RB Injuries/Plays linear model</a><br/>
        <a href="#wrmodel">WR Injuries/Plays linear model</a><br/>
        <a href="#temodel">TE Injuries/Plays linear model</a><br/>
        <a href="#kmodel">K Injuries/Plays linear model</a><br/>

        
        <hr />
        <a name="qbinjuries">&nbsp;</a><a href="#top">Back to contents</a>
        <h2>QB Injuries</h2>
        <asp:GridView ID="QBInjuredBefore" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="rbinjuries">&nbsp;</a><a href="#top">Back to contents</a>
        <h2>RB Injuries</h2>
        <asp:GridView ID="RBInjuredBefore" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="wrinjuries">&nbsp;</a><a href="#top">Back to contents</a>
        <h2>WR Injuries</h2>
        <asp:GridView ID="WRInjuredBefore" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="teinjuries">&nbsp;</a><a href="#top">Back to contents</a>
        <h2>TE Injuries</h2>
        <asp:GridView ID="TEInjuredBefore" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="kinjuries">&nbsp;</a><a href="#top">Back to contents</a>
        <h2>K Injuries</h2>
        <asp:GridView ID="KInjuredBefore" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>

        <a name="qbfinalrate"></a><a href="#top">Back to contents</a>
        <h2>QB Last Injury Rate</h2>
        <asp:GridView ID="CurrentQB" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="rbfinalrate"></a><a href="#top">Back to contents</a>
        <h2>RB Last Injury Rate</h2>
        <asp:GridView ID="CurrentRB" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="wrfinalrate"></a><a href="#top">Back to contents</a>
        <h2>WR Last Injury Rate</h2>
        <asp:GridView ID="CurrentWR" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="tefinalrate"></a><a href="#top">Back to contents</a>
        <h2>TE Last Injury Rate</h2>
        <asp:GridView ID="CurrentTE" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="kfinalrate"></a><a href="#top">Back to contents</a>
        <h2>K Last Injury Rate</h2>
        <asp:GridView ID="CurrentK" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>

        <a name="qbmodel"></a><a href="#top">Back to contents</a>
        
        <br/>
        <br/>
        <br/>
        <br/>Item1 is Max number of plays, ever for that position when injury has occurred.  
        <br/>Item2 is Max number of games player has ever been declared out.
        <br/>Item3 is label given to this edge of the longevity envelope.
        <h2>QB Model of Injuries</h2>
        <asp:Label ID="EquationQB" runat="server"/>
        <asp:GridView ID="ModelQB" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="rbmodel"></a><a href="#top">Back to contents</a>
        <h2>RB Model of Injuries</h2>
        <asp:Label ID="EquationRB" runat="server"/>
        <asp:GridView ID="ModelRB" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="wrmodel"></a><a href="#top">Back to contents</a>
        <h2>WR Model of Injuries</h2>
        <asp:Label ID="EquationWR" runat="server"/>
        <asp:GridView ID="ModelWR" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="temodel"></a><a href="#top">Back to contents</a>
        <h2>TE Model of Injuries</h2>
        <asp:Label ID="EquationTE" runat="server"/>
        <asp:GridView ID="ModelTE" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
        <a name="kmodel"></a><a href="#top">Back to contents</a>
        <h2>K Model of Injuries</h2>
        <asp:Label ID="EquationK" runat="server"/>
        <asp:GridView ID="ModelK" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
    </div>
    <div>
        <blockquote>
        The data on this page, can be retreived by a json URL <i>/Json/InjuryModelData.json</i>.
        Filtered data on this page can be retreived by replacing the playerID in the URL <i>/Json/InjuryModelData/[playerID].json</i>.
        </blockquote>
        The format of the filtered data is below.  Filtering can be thought of as "single player mode".
        <pre>
        {   
            "playerID":(string, id of player),
            "player":(),
            "playerInjuryModel":{
                "n": (int, number of injuries examined),
                "type": ("Linear", only value it currently supports), 
                "m":(float, slope of line that is multiplied to expected injuries),
                "b":(float, y-intercept which I guess in this concept means the number of injuries he expected to get just by walking out in his first game),
                "r2":(float, value between 0-1 and look in wikipedia under statistics R-squared, bc it's supposed to mean how accurate the estimate is, 1 being completely accurate),
                "variancePerX":(float, I don't know if this has a name in statistics, I dub it stdev of m),
                "curve":[{"x":59,"y":2.52894642557668}, (objects w/ attr [x] and [y] that are floats, supposed to be enough points to draw the line above, so 2 pts right now) ]
            }, 
            "playerFinalData":{
                "injuryGm":(999999, only 99999 right now, bc it represents an imaginary unachieavable bound like int.maxValue), 
                "year":(9999, only 9999 right now, see above), 
                "gm":(99, only 99 right now, see above), 
                "playerID":(string, unique id of player), 
                "player":(string, name of player), 
                "pos":(string, pos of player), 
                "runningPlayCount":(int, number of plays total), 
                "runningOutCount":(int, number of injuries total), 
                "runningGmCount":(int, number of games played total), 
                "injuryRate":(float, #injuries/#plays), 
                "dob":(string, date of birth for player), 
                "probableAge":(int, probable age at time of last statistic), 
                "draftYear":(int, first year I have data for him), 
                "draftStr":(string, definitive authority on year he was drafted, which will will override and overwrite above), 
                "seasons":(int, total seasons player has played), 
                "lastGm":(int, yyyymm, last game with stats that player played)
            }, 
            "Data":[
                {"injuryGm":(int, yyyyww, represents the game week he was ruled "out"), 
                 "year":(int, yyyy, represents the season (above) he was ruled "out"), 
                 "gm":(int, w, represents the game week (above) he was ruled "out"), 
                 "playerID":(string, unique id of player), 
                 "player":(string, name of player), 
                 "pos":(string, pos of player), 
                 "runningPlayCount":(int, number of plays up until this injury), 
                 "runningOutCount":(int, number of injuries that resulted in being Ruled "out" until and including this injury), 
                 "runningGmCount":(int, number of games played up until this injury), 
                 "injuryRate":(float, #injuries/#plays up until and including this injury), 
                 "dob":(string, date of birth for player), 
                 "probableAge":(int, probable age at time of injury), 
                 "draftYear":(int, first year I have data for him), 
                 "draftStr":(string, definitive authority on year he was drafted, which will will override and overwrite above), 
                 "seasons":(int, total seasons player has played, up until injury), 
                 "lastGm":(int, yyyymm, last game with stats that player played, up until injury)
            ]
        }
        </pre>
        <b>The unfiltered data</b>, has data for all players, and therefore the there is no player,, or playerID attributes on the top level.
        But they are available in the "data" and "playerFinalData".  And playerId is available in playerInjuryModel.
        The value for "PlayerInjuryModel" attribute is now wrapped in [] and multiple values are returned in array.   
        The "PlayerFinalData" attribute also will return data for all players and therefore it's value will also be now wrapped in [] 
        and multiple values returned.
        Additionally, posModel attribute is added at top level, replacing the missing "player and playerID".  It returns an object with
        atributes qb,rb,wr,te,and k.  Each containing a object with same fields as playerInjuryModel, except it only contains analysis results of the position.
        <p>I think it obvious that the API does not support filter for just QB or other multiple player returns on the server side.
    </div>
    <script>
        var ctx = document.getElementById("myChart").getContext('2d');

        // Define the data 
        var data = []; // Add data values to array
        // End Defining data
        var options = {
            responsive: true, // Instruct chart js to respond nicely.
            maintainAspectRatio: false, // Add to prevent default behaviour of full-width/height 
            tooltips: {
                callbacks: {
                    label: function(tooltipItem, data) {
                        //tooltipItem is some internal representation of point
                        var datasetData = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
                        var datasetLabel = datasetData.z +"("+datasetData.x+" Plays,"+datasetData.y+" RuledOut)" || 'Other'
                        var label = datasetLabel;
                        return label;
                    }
                }
            }
        };

        // End Defining data
        var chartsettings = {
            type: 'scatter',
            data: {
                datasets: [{
                        label: 'Population', // Name the series
                        data: data, // Specify the data values array
                  borderColor: '#2196f3', // Add custom color border            
                  backgroundColor: '#2196f3', // Add custom color background (Points and Fill)
                }]
            },
            options: options
        };
        var myChart = new Chart(ctx, chartsettings);

        var dataQB=[];
        var dataRB=[];
        var dataWR=[];
        var dataTE=[];
        var dataK=[];
        var modelQB=[];
        var modelRB=[];
        var modelWR=[];
        var modelTE=[];
        var modelK=[];
        function refreshChart() {
            setTimeout(function() {
                var settings=selectToScatterData("QB injuries", dataQB, window.userChartColors.aqua, false
                                                ,"RB injuries", dataRB, window.userChartColors.peru, false
                                                ,"WR injuries", dataWR, window.userChartColors.maroon, false
                                                ,"TE injuries", dataTE, window.userChartColors.navy, false
                                                ,"K injuries", dataK, window.userChartColors.lime, false
                                                ,"QB model", modelQB, window.userChartColors.fushsia, true
                                                ,"RB model", modelRB, window.userChartColors.brown, true
                                                ,"WR model", modelWR, window.userChartColors.pink, true
                                                ,"TE model", modelTE, window.userChartColors.teal, true
                                                ,"K model", modelK, window.userChartColors.olive, true);
                chartsettings.data = settings;
                myChart.update();
            },1);
        }
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_QBInjuredBefore"), s=>new xyrz(s.cells[4].innerText,s.cells[5].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            dataQB=data;
            refreshChart();
        },0);
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_RBInjuredBefore"), s=>new xyrz(s.cells[4].innerText,s.cells[5].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            dataRB=data;
            refreshChart();
        },0);
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_WRInjuredBefore"), s=>new xyrz(s.cells[4].innerText,s.cells[5].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            dataWR=data;
            refreshChart();
        },0);
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_TEInjuredBefore"), s=>new xyrz(s.cells[4].innerText,s.cells[5].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            dataTE=data;
            refreshChart();
        },0);
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_KInjuredBefore"), s=>new xyrz(s.cells[4].innerText,s.cells[5].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            dataK=data;
            refreshChart();
        },0);

        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_ModelQB"), s=>new xyrz(s.cells[0].innerText,s.cells[1].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            modelQB=data;
            refreshChart();
        },0);
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_ModelQB"), s=>new xyrz(s.cells[0].innerText,s.cells[1].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            modelQB=data;
            refreshChart();
        },0);
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_ModelRB"), s=>new xyrz(s.cells[0].innerText,s.cells[1].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            modelRB=data;
            refreshChart();
        },0);
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_ModelWR"), s=>new xyrz(s.cells[0].innerText,s.cells[1].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            modelWR=data;
            refreshChart();
        },0);
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_ModelTE"), s=>new xyrz(s.cells[0].innerText,s.cells[1].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            modelTE=data;
            refreshChart();
        },0);
        setTimeout(function() {
            var data=selectRowWhere(document.getElementById("MainContent_ModelK"), s=>new xyrz(s.cells[0].innerText,s.cells[1].innerText,0.5,s.cells[2].innerText), s=>s.rowIndex!=0);
            modelK=data;
            refreshChart();
        },0);
    </script>
</asp:Content>