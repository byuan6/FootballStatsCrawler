<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="volatilitycandidates.aspx.cs" Inherits="FFToiletBowlWeb.volatilitycandidates" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="captionBlock grow" style="display:inline-block; width:49%;vertical-align:top;">
        <h1>
            <asp:Label ID="Year" runat="server"/> Season<br>
            Before Week <asp:Label ID="Week" runat="server"/>
        </h1>
        <h2>Volatility Candidates</h2>
        <h3><asp:Literal ID="ModellingMethod" runat="server"></asp:Literal></h3>
    </div>


    <!-- Highlight players in different color, those that leapfrog other players -->
    <!-- avg vs expected, with background points, different but same color for the group of colored and labelled points for leap frog players, AND normally better players they surpass  -->
    <!-- top 3 to 5, absolute diff. in both positive and negtive sides  -->
    <div class="graphPane">
        <asp:Image ID="QbShuffle" runat="server" />
        <!--img src="Images/rbdefensehandicap1.png" /-->
    </div>
            
    <div class="graphPane">
        <asp:Image ID="RbShuffle" runat="server" />
        <!--img src="Images/qbdefensehandicap1.png" /-->
    </div>
        
    <div class="graphPane">
        <asp:Image ID="WrTeShuffle" runat="server" />
        <!--img src="Images/kdefensehandicap1.png" /-->
    </div>

    <div class="graphPane">
        <asp:Image ID="KShuffle" runat="server" />
        <!--img src="Images/kdefensehandicap1.png" /-->
    </div>

    <div class="graphPane">
        <asp:Image ID="DstShuffle" runat="server" />
        <!--img src="Images/kdefensehandicap1.png" /-->
    </div>



    <script>
        window.onload = function () {
            var color = Chart.helpers.color;
            var ctx = document.getElementById('canvas').getContext('2d');
            var tbl = document.getElementById('MainContent_BetterView');

            var betterA = window.better1 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText == "X")
                    return true;
                else
                    return false;
            }, xyrz, 5, 7, 8, 2);
            var betterB = window.better2 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText != "X")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var qbA = window.qb1 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "QB")
                    return true;
                else
                    return false;
            }, xyrz, 5, 7, 8, 2);
            var qbB = window.qb2 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "QB")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var rbA = window.rb1 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "RB")
                    return true;
                else
                    return false;
            }, xyrz, 5, 7, 8, 2);
            var rbB = window.rb2 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "RB")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var wrA = window.wr1 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "WR")
                    return true;
                else
                    return false;
            }, xyrz, 5, 7, 8, 2);
            var wrB = window.wr2 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "WR")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var teA = window.te1 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "TE")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);
            var teB = window.te2 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "TE")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var kA = window.k1 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "K")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);
            var kB = window.k2 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "K")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var dstA = window.dst1 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "DST")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);
            var dstB = window.dst2 = selectIntoWhere(tbl, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "DST")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);



            // get lesser list data
            var tbl2 = document.getElementById('MainContent_StruggleView');

            var lesserA = window.lesser1 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText == "X")
                    return true;
                else
                    return false;
            }, xyrz, 5, 7, 8, 2);
            var lesserB = window.lesser2 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText != "X")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var qbC = window.qb3 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "QB")
                    return true;
                else
                    return false;
            }, xyrz, 5, 7, 8, 2);
            var qbD = window.qb4 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "QB")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var rbC = window.rb3 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "RB")
                    return true;
                else
                    return false;
            }, xyrz, 5, 7, 8, 2);
            var rbD = window.rb4 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "RB")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var wrC = window.wr3 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "WR")
                    return true;
                else
                    return false;
            }, xyrz, 5, 7, 8, 2);
            var wrD = window.wr4 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "WR")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var teC = window.te3 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "TE")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);
            var teD = window.te4 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "TE")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var kC = window.k3 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "K")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);
            var kD = window.k4 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "K")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);


            var dstC = window.dst3 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText == "X" && tr.cells[4].innerText == "DST")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);
            var dstD = window.dst4 = selectIntoWhere(tbl2, function (tr) {
                if (tr.cells[1].innerText != "X" && tr.cells[4].innerText == "DST")
                    return true;
                else
                    return false;
            }, xyz, 5, 7, 2);

            //window.chartColors.red
            //window.chartColors.orange
            //window.chartColors.yellow
            //window.chartColors.green
            //window.chartColors.purple
            //window.chartColors.grey
            //window.chartColors.blue
            //window.userChartColors.teal
            //window.userChartColors.lime

            //window.userChartColors.maroon
            //window.userChartColors.pink

            // create a mixed chart with columns representing expected points, labels as points scored, and datasets as QB
            var maxXY = maximum(selectWhere(tbl, 7, parseFloat, null));
            
            var scatterChartData = selectToScatterData("Line of expected average output", [{ x: 0, y: 0, r: 0 }, { x: maxXY, y: maxXY, r: 0}], window.chartColors.red, true
                                                    , "Players expected to outperform his average", betterA, window.userChartColors.lime, false
                                                    , "Players with better average, expected to underperform player in green, left of him of graph", betterB, window.chartColors.yellow, false
                                                    , "Players expected to underperform his average", lesserA, window.chartColors.purple, false
                                                    , "Players with worse average, BUT STILL expected to outperform player in purple, right of him of graph", lesserB, window.chartColors.blue, false);

            //the (type: 'bubble') bubble graph's radius isn't true.  a value of 5,5 and radius of 5, should touch both axes.  But may be useful as a relative measure
            window.myScatter = new Chart(ctx, {
                type: 'scatter',
                data: scatterChartData,
                options: {
                    title: {
                        display: true,
                        text: 'Players Projected to explode over better players or implode worse than lesser players'
                    },
                    tooltips: {
                        callbacks: {
                            label: function (tooltipItem, data) {
                                //var label = data.labels[tooltipItem.index];
                                var pt = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
                                if (pt.z == null)
                                    return '(Season Avg=' + tooltipItem.xLabel + ', Expected Output=' + tooltipItem.yLabel + ')';
                                else
                                    return pt.z + ' (Season Avg=' + tooltipItem.xLabel + ', Expected Output=' + tooltipItem.yLabel + ')';
                            }
                        }
                    }
                }
            });


            /*
            window.myScatter = Chart.Scatter(ctx, {
            data: scatterChartData,
            options: {
            title: {
            display: true,
            text: 'Chart.js Scatter Chart'
            },
            tooltips: {
            callbacks: {
            label: function (tooltipItem, data) {
            //var label = data.labels[tooltipItem.index];
            var pt = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
            if(pt.z==null)
            return '(Season Avg=' + tooltipItem.xLabel + ', Expected Output=' + tooltipItem.yLabel + ')';
            else
            return pt.z + ' (Season Avg=' + tooltipItem.xLabel + ', Expected Output=' + tooltipItem.yLabel + ')';
            }
            }
            }
            }
            });
            */

            //add event handler to table, click on a row and filter on the group
            //or filter controls for both graph and table.
            //just better or lesser, double click on pos rank
            //just position, double click on position
            //just specific player group, double click on player with marker
            //show filter, clear on click?
        };

        
    </script>

    

    <h2>Projected Results for week</h2>
    <div style="padding:5px;">
        Avg(%)/gm of <span style="background-color:#EEEEEE"><asp:HyperLink ID="PointsLink" Text="FF Points" runat="server" /> |
                                               <asp:HyperLink ID="StatsLink" Text="Yards" runat="server" /></span>
        &nbsp;&nbsp;&nbsp;&nbsp;
        FF Points formula <span style="background-color:#EEEEEE"><asp:HyperLink ID="PPRLink" Text="PPR" runat="server" /> |
                                               <asp:HyperLink ID="StandardLink" Text="Standard" runat="server" /></span>
    </div>
    
    
    <div style="position:sticky;top:0px;background-color: White; padding-top:5px; padding-bottom:5px;">
        <div style="width:100%; height:400px;">
            <canvas id="canvas" style="width:100%; height:100%; background-color:white"></canvas>
        </div>
    
        <h3>Legend of table of players below</h3>
        <div class="legendSample bgGreen">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Expected to do much better than average<br />
        <div class="legendSample ">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Normally better than above, in green<br />
        <div class="legendSample bgRed">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Expected to do much worse than average<br />
        <div class="legendSample bgYellow">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Normally worse than above, in red<br />
    </div>

    <div>
        <h4>Could be better than normal</h4>
        <asp:GridView ID="BetterView" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
        <Columns>
            <asp:BoundField HeaderText="Wk's Pos Rk" DataField="PosRank" ReadOnly="true"  />
            <asp:BoundField HeaderText="Flag" DataField="FlagMarker" ReadOnly="true" />
            <asp:BoundField HeaderText="Player" DataField="Player" ReadOnly="true" />
            <asp:BoundField HeaderText="Tm" DataField="Team" ReadOnly="true" />
            <asp:BoundField HeaderText="Pos" DataField="Pos" ReadOnly="true" />
            <asp:BoundField HeaderText="Avg" DataField="SeasonAvg" ReadOnly="true" DataFormatString="{0:0.0}" />
            <asp:BoundField HeaderText="Vs." DataField="Versus" ReadOnly="true" />
            <asp:BoundField HeaderText="Est" DataField="NextWeekEstimate" ReadOnly="true" />
            <asp:BoundField HeaderText="Diff" DataField="Diff" ReadOnly="true" />
        </Columns>
        </asp:GridView>
    </div>

    
    <div>
        <h4>Might struggle</h4>
        <asp:GridView ID="StruggleView" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
        <Columns>
            <asp:BoundField HeaderText="Wk's Pos Rk" DataField="PosRank" ReadOnly="true"  />
            <asp:BoundField HeaderText="Flag" DataField="FlagMarker" ReadOnly="true" />
            <asp:BoundField HeaderText="Player" DataField="Player" ReadOnly="true" />
            <asp:BoundField HeaderText="Tm" DataField="Team" ReadOnly="true" />
            <asp:BoundField HeaderText="Pos" DataField="Pos" ReadOnly="true" />
            <asp:BoundField HeaderText="Avg" DataField="SeasonAvg" ReadOnly="true" DataFormatString="{0:0.0}" />
            <asp:BoundField HeaderText="Vs." DataField="Versus" ReadOnly="true" />
            <asp:BoundField HeaderText="Est" DataField="NextWeekEstimate" ReadOnly="true" />
            <asp:BoundField HeaderText="Diff" DataField="Diff" ReadOnly="true" />
        </Columns>
        </asp:GridView>
    </div>

    
</asp:Content>
