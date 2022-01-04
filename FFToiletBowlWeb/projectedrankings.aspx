<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="projectedrankings.aspx.cs" Inherits="FFToiletBowlWeb.projectedrankings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .altColorColumn
        {
            background-color:#f7f7f7;
        }
        .lessPaddedColumn
        {
            padding-left:4px;
            padding-right:4px;
        }
        
        
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="captionBlock grow" style="display:inline-block; width:49%;vertical-align:top;">
        <h1>
            <asp:Label ID="Year" runat="server"/> Season<br/>
            Before Week <asp:Label ID="Week" runat="server"/>
        </h1>
        <h2>Projected Player Output</h2>
        <h3><asp:Literal ID="ModellingMethod" runat="server"></asp:Literal></h3>
    </div>

    <!-- Let's create a visualization of which players are expected to do well this week -->
    <!-- x is season average, y is predicted value, see if top end players are expected to above diagonal -->
    <div class="graphPane">
        <asp:Image ID="PassTrend" runat="server" />
        <!--img src="Images/rbdefensehandicap1.png" /-->
    </div>

            
    <div class="graphPane">
        <asp:Image ID="RunTrend" runat="server" />
        <!--img src="Images/qbdefensehandicap1.png" /-->
    </div>

            
    <div class="graphPane">
        <asp:Image ID="DefenseTrend" runat="server" />
        <!--img src="Images/kdefensehandicap1.png" /-->
    </div>

    
    <h2>Projected Results for week</h2>
    <div style="padding:5px;">
        Avg(%)/gm of <span style="background-color:#EEEEEE"><asp:HyperLink ID="PointsLink" Text="FF Points" runat="server" /> |
                                               <asp:HyperLink ID="StatsLink" Text="Yards" runat="server" /></span>
        &nbsp;&nbsp;&nbsp;&nbsp;
        FF Points formula <span style="background-color:#EEEEEE"><asp:HyperLink ID="PPRLink" Text="PPR" runat="server" /> |
                                               <asp:HyperLink ID="StandardLink" Text="Standard" runat="server" /></span>
    </div>
    
    <p></p>
    <div style="padding:0px;">
        <detail>
            <summary>Legend explanation</summary>
            The legend shows the color coding for tiering of player levels.  Tiering is based the fact that games are won and decided by starting players, not backups.
            A fantasy football league has maximum number of 16-teams, and when top starters play bottom starters, top starters usually win the face off matchup.  
            Additionally, each position is mostly saturated when (#&nbsp;of&nbsp;starting&nbsp;positions)&nbsp;*&nbsp;(#&nbsp;of&nbsp;teams) have been drafted.  
            This will fill every starting slot, on every team in the league 
            (assuming a team hasn't adopted a strategy of drafting to take away a player from 1 other team, vs addressing it's need against 15 others).  
            Every team after that, is drafting for a backup role for that position.
            Consequently, the starters can be divided into 2 halves.  The upper half starters should average more face off wins against their counterparts on the other team, than lower half starters.
            The positions are divided between upper half starters, fence starters, lower half starters, should be bench players, might be a bench player, and should not be rostered.
            The math is easy enough for you to figure out yourself, but our javascript will color code it for you.  
            Adjust the number of teams in your league and how many players at each position start and are rostered on the bench, and press Refresh.

            <p>We thought about using a Kruskals-like algorithm to do clustering (connect the players with least distance between their averages, until there are only 5 or 6 clusters), but we don't (yet).  And quite frankly, don't know if there is any statistical or competitive significance to doing so.
            </p>
        </detail>
        <div style="position:sticky;top:0px;background-color: White; padding-top:5px; padding-bottom:5px;">
            <div class="legendSample bgGreen">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Top Starters, top 30%<br />
            <div class="legendSample ">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> On the fence Starters, middle 40%<br />
            <div class="legendSample bgBlue">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Starter Quality, but should be losses vs top 30%<br />
            <div class="legendSample bgYellow">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Bench<br />
            <div class="legendSample bgOrange">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Marginally Rosterable<br />
            <div class="legendSample bgRed">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Should not be considered<br />
            <div class="legendSample bgGrey">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Only for flex. Should be already rostered as starter<br />
            <div class="legendSample bgPurple">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div> Only for flex. Ideal flex is every starting position is filled, then next XX player for flex eligible positions<br />
        </div>



        <div>
            <input type="text" name="teams" id="teams" value="16" size="2" /> Teams in League
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type="button" value="Refresh" onclick="highlight_tiers();"><br />

            <b>Starters/team</b>
            <input type="text" name="qb1" id="qb1" value="1" size="1" /> QB
            <input type="text" name="rb1" id="rb1" value="2" size="1" /> RB
            <input type="text" name="wr1" id="wr1" value="2" size="1" /> WR
            <input type="text" name="te1" id="te1" value="1" size="1" /> TE
            <input type="text" name="flex" id="flex" value="1" size="1" /> Flex
            <input type="text" name="k1" id="k1" value="1" size="1" /> K
            <input type="text" name="dst1" id="dst1" value="1" size="1" /> DST

            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Backups/team</b>
            <input type="text" name="qb2" id="qb2" value="1" size="1" /> QB
            <input type="text" name="rb2" id="rb2" value="3" size="1" /> RB
            <input type="text" name="wr2" id="wr2" value="3" size="1" /> WR
            <input type="text" name="te2" id="te2" value="1" size="1" /> TE
            <input type="text" name="k2"  id="k2" value="1" size="1" /> K
            <input type="text" name="dst2" id="dst2" value="1" size="1" /> DST

            <script>
                //MainContent_KView
                function highlight_tiers() {
                    var qbgrid = document.getElementById("MainContent_QBView");
                    var rbgrid = document.getElementById("MainContent_RBView");
                    var wrgrid = document.getElementById("MainContent_WRView");
                    var tegrid = document.getElementById("MainContent_TEView");
                    var kgrid = document.getElementById("MainContent_KView");
                    var dstgrid = document.getElementById("MainContent_DSTView");
                    var flexgrid = document.getElementById("MainContent_FlexView");

                    var qbavg = document.getElementById("qbavg");
                    var rbavg = document.getElementById("rbavg");
                    var wravg = document.getElementById("wravg");
                    var teavg = document.getElementById("teavg");
                    var kavg = document.getElementById("kavg");
                    var dstavg = document.getElementById("dstavg");

                    var teams = parseInt(document.getElementById("teams").value);

                    var qb1 = parseInt(document.getElementById("qb1").value);
                    var rb1 = parseInt(document.getElementById("rb1").value);
                    var wr1 = parseInt(document.getElementById("wr1").value);
                    var te1 = parseInt(document.getElementById("te1").value);
                    var k1 = parseInt(document.getElementById("k1").value);
                    var dst1 = parseInt(document.getElementById("dst1").value);

                    var qb2 = parseInt(document.getElementById("qb2").value);
                    var rb2 = parseInt(document.getElementById("rb2").value);
                    var wr2 = parseInt(document.getElementById("wr2").value);
                    var te2 = parseInt(document.getElementById("te2").value);
                    var k2 = parseInt(document.getElementById("k2").value);
                    var dst2 = parseInt(document.getElementById("dst2").value);

                    for (var j = 0; j < 7; j++) {
                        var o = null;
                        var l1 = 0;
                        var l2 = 0;
                        var span = null;
                        if (j == 0) {
                            o = qbgrid;
                            l1 = qb1;
                            l2 = qb2;
                            span = qbavg;
                        } else if (j == 1) {
                            o = rbgrid;
                            l1 = rb1;
                            l2 = rb2;
                            span = rbavg;
                        } else if (j == 2) {
                            o = wrgrid;
                            l1 = wr1;
                            l2 = wr2;
                            span = wravg;
                        } else if (j == 3) {
                            o = tegrid;
                            l1 = te1;
                            l2 = te2;
                            span = teavg;
                        } else if (j == 4) {
                            o = kgrid;
                            l1 = k1;
                            l2 = k2;
                            span = kavg;
                        } else if (j == 5) {
                            o = dstgrid;
                            l1 = dst1;
                            l2 = dst2;
                            span = dstavg;
                        } else {
                            o = flexgrid;
                            l1 = dst1;
                            l2 = dst2;
                        }

                        if (o == flexgrid) {
                            var skips = 1 + (rb1 + wr1 + te1) * teams;
                            var take = skips + teams;

                            /*for (var i = 1; i < skips; i++) {
                            var tr = o.rows[i];
                            clearhighlight(tr);
                            tr.classList.add("bgGrey");
                            }
                            for (var i = skips; i < take; i++) {
                            var tr = o.rows[i];
                            clearhighlight(tr);
                            tr.classList.add("bgGreen");
                            }*/

                            for (var i = 1; i < take; i++) {
                                var tr = o.rows[i];
                                clearhighlight(tr);
                                var pos = tr.cells[2].innerText;
                                if ((pos.startsWith("RB") && parseInt(pos.substring(2)) <= rb2 * teams)
                                    || (pos.startsWith("WR") && parseInt(pos.substring(2)) <= wr2 * teams)
                                    || (pos.startsWith("TE") && parseInt(pos.substring(2)) <= te2 * teams))
                                    if ((pos.startsWith("RB") && parseInt(pos.substring(2)) > rb1 * teams)
                                        || (pos.startsWith("WR") && parseInt(pos.substring(2)) > wr1 * teams)
                                        || (pos.startsWith("TE") && parseInt(pos.substring(2)) > te1 * teams))
                                        tr.classList.add("bgPurple");
                                    else
                                        tr.classList.add("bgGrey");
                            }
                        } else {
                            var tier1 = l1 * teams / 3;
                            var tier2 = l1 * teams * 2 / 3;
                            var tier3 = l1 * teams;
                            var tier4 = (l2 + l1) * teams;
                            var tier5 = (l2 + l1 + 1) * teams;
                            var starters = selecttake(o, 4, tier3);
                            var avg = average(starters);
                            var halfstdev = Math.sqrt(variance(starters, avg)) / 2;
                            if (span != null)
                                span.innerHTML = "Starter middle 40% is Avg " + Math.round(avg) + ", +/-" + Math.round(halfstdev);
                            starters.forEach(function (item, index) {
                                if (item > avg + halfstdev)
                                    tier1 = index+1;
                                else if (item > avg - halfstdev)
                                    tier2 = index+1;
                            });
                            for (var i = 1; i < o.rows.length; i++) {
                                var tr = o.rows[i];

                                var hilite = null;
                                if (i <= tier1)
                                    hilite = "bgGreen";
                                else if (i <= tier2) { }
                                else if (i <= tier3)
                                    hilite = "bgBlue";
                                else if (i <= tier4)
                                    hilite = "bgYellow";
                                else if (i <= tier5)
                                    hilite = "bgOrange";
                                else
                                    hilite = "bgRed";

                                clearhighlight(tr);
                                tr.classList.add(hilite);

                                //for (var x = 0; x < tr.cells.length; x++) {
                                //    var td = tr.cells[x];
                                //    clearhighlight(td);
                                //    td.classList.add(hilite);
                                //}
                            }
                        }
                    }
                }
                function clearhighlight(tr) {
                    tr.classList.remove("bgGreen");
                    tr.classList.remove("bgBlue");
                    tr.classList.remove("bgYellow");
                    tr.classList.remove("bgOrange");
                    tr.classList.remove("bgRed");
                    tr.classList.remove("bgGrey");
                }


                function Isnull(replacement,castfn) {
                    this.replacement = replacement;
                    this.castfn = castfn;
                    this.convert = function(text) {
                        if(this.castfn==null)
                            return text;
                        else {
                            var value = this.castfn(text);
                            if(isNaN(value))
                                return this.replacement;
                            else
                                return value;
                        }
                    };
                }

              
                window.onload = function () {
                    highlight_tiers();

                    setTimeout(function() {
                        var tbl1 = document.getElementById("MainContent_ModelTrend");
			            var ctx1 = document.getElementById('graphcanvas1').getContext('2d');
                    
                        var parseNullableFloat = new Isnull(null, parseFloat);
                        var test = selectToChartData(selectWhere(tbl1, 0, parseInt, null), 'A', selectWhere(tbl1, 1, parseNullableFloat.convert, null), window.chartColors.red
                                                                                         , 'B', selectWhere(tbl1, 2, parseNullableFloat.convert, null), window.chartColors.orange
                                                                                         , 'C', selectWhere(tbl1, 3, parseNullableFloat.convert, null), window.chartColors.yellow
                                                                                         , 'D', selectWhere(tbl1, 4, parseNullableFloat.convert, null), window.chartColors.green
                                                                                         , 'E', selectWhere(tbl1, 5, parseNullableFloat.convert, null), window.chartColors.purple
                                                                                         , 'F', selectWhere(tbl1, 6, parseNullableFloat.convert, null), window.chartColors.grey
                                                                                         , 'G', selectWhere(tbl1, 7, parseNullableFloat.convert, null), window.chartColors.blue
                                                                                         , 'H', selectWhere(tbl1, 8, parseNullableFloat.convert, null), window.userChartColors.teal
                                                                                         , 'I', selectWhere(tbl1, 9, parseNullableFloat.convert, null), window.userChartColors.lime);

			            window.myLineChart1 = new Chart(ctx1, {
				            type: 'line',
				            data: test,
				            options: {
					            responsive: true,
					            legend: {
						            position: 'top',
					            },
					            title: {
						            display: true,
						            text: 'Year vs error, per Modeling method'
					            }
				            }
			            });
                    }, 2000);

                    
                    setTimeout(function() {
                        var tbl2 = document.getElementById("MainContent_ModelEffectiveness");
			            var ctx2 = document.getElementById('graphcanvas2').getContext('2d');
                    
                        var parseNullableFloat = new Isnull(null, parseFloat);
                        var test2 = selectToChartData(selectWhere(tbl2, 0, parseInt, null), 'A', selectWhere(tbl2, 1, parseNullableFloat.convert, null), window.chartColors.red
                                                                                         , 'B', selectWhere(tbl2, 2, parseNullableFloat.convert, null), window.chartColors.orange
                                                                                         , 'C', selectWhere(tbl2, 3, parseNullableFloat.convert, null), window.chartColors.yellow
                                                                                         , 'D', selectWhere(tbl2, 4, parseNullableFloat.convert, null), window.chartColors.green
                                                                                         , 'E', selectWhere(tbl2, 5, parseNullableFloat.convert, null), window.chartColors.purple
                                                                                         , 'F', selectWhere(tbl2, 6, parseNullableFloat.convert, null), window.chartColors.grey
                                                                                         , 'G', selectWhere(tbl2, 7, parseNullableFloat.convert, null), window.chartColors.blue
                                                                                         , 'H', selectWhere(tbl2, 8, parseNullableFloat.convert, null), window.userChartColors.teal
                                                                                         , 'I', selectWhere(tbl2, 9, parseNullableFloat.convert, null), window.userChartColors.lime
                                                                                         , 'J', selectWhere(tbl2, 28, parseNullableFloat.convert, null), window.userChartColors.maroon);

			            window.myLineChart2 = new Chart(ctx2, {
				            type: 'line',
				            data: test2,
				            options: {
					            responsive: true,
					            legend: {
						            position: 'top',
					            },
					            title: {
						            display: true,
						            text: 'games into season vs error, per modeling method'
					            }
				            }
			            });
                    },4000);
                }
            </script>
        </div>

        <div class="HStackable">
            <h4>QB</h4>
            <span id="qbavg"></span>
            <asp:GridView ID="QBView" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
            <Columns>
                <asp:BoundField HeaderText="Player" DataField="Player" ReadOnly="true" />
                <asp:BoundField HeaderText="Tm" DataField="Team" ReadOnly="true" />

                <asp:BoundField HeaderText="Avg" DataField="SeasonAvg" ReadOnly="true" DataFormatString="{0:0.0}" />
                <asp:BoundField HeaderText="Vs." DataField="Versus" ReadOnly="true" />
                <asp:BoundField HeaderText="Est" DataField="NextWeekEstimate" ReadOnly="true" />
            </Columns>
            </asp:GridView>
        </div>

        <div class="HStackable">
            <h4>TE</h4>
            <span id="teavg"></span>
            <asp:GridView ID="TEView" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
            <Columns>
                <asp:BoundField HeaderText="Player" DataField="Player" />
                <asp:BoundField HeaderText="Tm" DataField="Team" />
                <asp:BoundField HeaderText="Avg" DataField="SeasonAvg" DataFormatString="{0:0.0}" />
                <asp:BoundField HeaderText="Vs." DataField="Versus" />
                <asp:BoundField HeaderText="Est" DataField="NextWeekEstimate" />
            </Columns>
            </asp:GridView>
        </div>
        
        <div class="HStackable">
            <h4>RB</h4>
            <span id="rbavg"></span>
            <asp:GridView ID="RBView" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
            <Columns>
                <asp:BoundField HeaderText="Player" DataField="Player" />
                <asp:BoundField HeaderText="Team" DataField="Team" />
                <asp:BoundField HeaderText="Sea. Avg" DataField="SeasonAvg" DataFormatString="{0:0.0}" />
                <asp:BoundField HeaderText="Vs." DataField="Versus" />
                <asp:BoundField HeaderText="Est" DataField="NextWeekEstimate" />
            </Columns>
            </asp:GridView>
        </div>

        <div class="HStackable">
            <h4>WR</h4>
            <span id="wravg"></span>
            <asp:GridView ID="WRView" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
            <Columns>
                <asp:BoundField HeaderText="Player" DataField="Player" />
                <asp:BoundField HeaderText="Team" DataField="Team" />
                <asp:BoundField HeaderText="Sea. Avg" DataField="SeasonAvg" DataFormatString="{0:0.0}" />
                <asp:BoundField HeaderText="Vs." DataField="Versus" />
                <asp:BoundField HeaderText="Est" DataField="NextWeekEstimate" />
            </Columns>
            </asp:GridView>
        </div>

        <div class="HStackable">
            <h4>K</h4>
            <span id="kavg"></span>
            <asp:GridView ID="KView" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
            <Columns>
                <asp:BoundField HeaderText="Player" DataField="Player" />
                <asp:BoundField HeaderText="Team" DataField="Team" />
                <asp:BoundField HeaderText="Sea. Avg" DataField="SeasonAvg" DataFormatString="{0:0.0}" />
                <asp:BoundField HeaderText="Vs." DataField="Versus" />
                <asp:BoundField HeaderText="Est" DataField="NextWeekEstimate" />
            </Columns>
            </asp:GridView>
        </div>

        <div class="HStackable">
            <h4>DST</h4>
            <span id="dstavg"></span>
            <asp:GridView ID="DSTView"  AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
            <Columns>
                <asp:BoundField HeaderText="Player" DataField="Player" />
                <asp:BoundField HeaderText="Team" DataField="Team" />
                <asp:BoundField HeaderText="Sea. Avg" DataField="SeasonAvg" DataFormatString="{0:0.0}" />
                <asp:BoundField HeaderText="Vs." DataField="Versus" />
                <asp:BoundField HeaderText="Est" DataField="NextWeekEstimate" />
            </Columns>
            </asp:GridView>
        </div>

        <div class="HStackable">
            <h4>Flex(RB,WR,TE)</h4>
            <asp:GridView ID="FlexView" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
            <Columns>
                <asp:BoundField HeaderText="#" DataField="OverallRank" />
                <asp:BoundField HeaderText="Player" DataField="Player" />
                <asp:TemplateField HeaderText="Pos">
                <ItemTemplate><asp:Label ID="Label1" Text='<%#Eval("Pos") %>' runat="server"/><asp:Label ID="Label2" Text='<%#Eval("PosRank") %>' runat="server"/></ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField HeaderText="Team" DataField="Team" />
                <asp:BoundField HeaderText="Sea. Avg" DataField="SeasonAvg" DataFormatString="{0:0.0}" />
                <asp:BoundField HeaderText="Vs." DataField="Versus" />
                <asp:BoundField HeaderText="Est" DataField="NextWeekEstimate" />
            </Columns>
            </asp:GridView>
        </div>

    </div>

    <details>
        <summary><h3>Model Performance...<h3></summary>
        <div style="border:1px solid black; padding:10px; margin:10px;">
            Standard deviations calculated using Expected Value as:
            <ol type="A">
                <li A>Implied Pts From Expert Ranking as EV</li>
	            <li B>Implied Pts From Expert Ranking as EV, linearly approximated by player defense coefficient </li>
	            <li C>Implied Pts From Expert Ranking as EV, w/ defensive handicap to average </li>
	            <li D>Season Running Average as Expected Value</li>
	            <li E>Season Running Average as Expected Value, linearly approximated by player defense coefficient</li>
	            <li F>Season Running Average as Expected Value, w/ defensive handicap to average</li>
	            <li G>Trailing 17-Week Average as Expected Value</li>
	            <li H>Trailing 17-Week Average as Expected Value, linearly approximated by player defense coefficient</li>
	            <li I>Trailing 17-Week Average as Expected Value, w/ defensive handicap to average</li>
            </ol>
        </div>

        <div style="width:100%; height:400px;">
            <canvas id="graphcanvas1" style="width:100%; height:100%; background-color:white"></canvas>
        </div>
        <div>
            <!-- actual results per week with running average, in column chart, stacked with error from closest projection method, include running season pt total -->
            <!-- included table of stats for each game -->
            <asp:GridView ID="ModelTrend" DataKeyNames="Year" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
            <Columns>
                <asp:BoundField HeaderText="Year" DataField="year" />
                <asp:BoundField HeaderText="A" DataField="A" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="B" DataField="B" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="C" DataField="C" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="D" DataField="D" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="E" DataField="E" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="F" DataField="F" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="G" DataField="G" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="H" DataField="H" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="I" DataField="I" DataFormatString="{0:0.000}" />
            </Columns>
            </asp:GridView>
        </div>

        <div style="border:1px solid black; padding:10px; margin:10px;">
            Standard deviations calculated using Expected Value as:
            <ol type="A">
                <li A>Implied Pts From Expert Ranking as EV</li>
	            <li B>Implied Pts From Expert Ranking as EV, linearly approximated by player defense coefficient </li>
	            <li C>Implied Pts From Expert Ranking as EV, w/ defensive handicap to average </li>
	            <li D>Season Running Average as Expected Value</li>
	            <li E>Season Running Average as Expected Value, linearly approximated by player defense coefficient</li>
	            <li F>Season Running Average as Expected Value, w/ defensive handicap to average</li>
	            <li G>Trailing 17-Week Average as Expected Value</li>
	            <li H>Trailing 17-Week Average as Expected Value, linearly approximated by player defense coefficient</li>
	            <li I>Trailing 17-Week Average as Expected Value, w/ defensive handicap to average</li>
                <li J>Least error value</li>
            </ol>
            Table (below), uses the projection methods indicated in legend (above), subdivides the data by week, so you can see which methods are more effective as more data from current year is available.<br />
            Suffixed with (all) Stdev using all data, so every week should be same value.<br />
            Suffixed with (lastyr) Stdev using last year's data, so every week should be same value.<br />
            If it's available for the player (sometimes, it's not bc there is insufficient past data), this webpage selects the projection method with least error at that point of the season.
            If you wish to see the other projection results (it's not that interesting), you need to browse to player's page.
        </div>
        
        <div style="width:100%; height:400px;">
            <canvas id="graphcanvas2" style="width:100%; height:100%; background-color:white"></canvas>
        </div>
        <div>
            <!-- actual results per week in column chart, with future projected values in column of different colors of best method, included projected season total -->
            <!-- line chart of all the methods estimates from current week -->
            <asp:GridView ID="ModelEffectiveness" DataKeyNames="Gm" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" ViewStateMode="Disabled" runat="server">
            <Columns>
                <asp:BoundField HeaderText="Gm" DataField="Gm" />
                <asp:BoundField HeaderText="A" DataField="A" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="B" DataField="B" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="C" DataField="C" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="D" DataField="D" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="E" DataField="E" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="F" DataField="F" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="G" DataField="G" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="H" DataField="H" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="I" DataField="I" DataFormatString="{0:0.000}" />

                <asp:BoundField HeaderText="A(all)" DataField="BA" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="B(all)" DataField="BB" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="C(all)" DataField="BC" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="D(all)" DataField="BD" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="E(all)" DataField="BE" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="F(all)" DataField="BF" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="G(all)" DataField="BG" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="H(all)" DataField="BH" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="I(all)" DataField="BI" DataFormatString="{0:0.000}" />

                <asp:BoundField HeaderText="A(lastyr)" DataField="LA" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="B(lastyr)" DataField="LB" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="C(lastyr)" DataField="LC" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="D(lastyr)" DataField="LD" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="E(lastyr)" DataField="LE" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="F(lastyr)" DataField="LF" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="G(lastyr)" DataField="LG" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="H(lastyr)" DataField="LH" DataFormatString="{0:0.000}" />
                <asp:BoundField HeaderText="I(lastyr)" DataField="LI" DataFormatString="{0:0.000}" />

                <asp:BoundField HeaderText="J(min)" DataField="J" DataFormatString="{0:0.000}" />
            </Columns>
            </asp:GridView>
        </div>
    </details>

</asp:Content>
