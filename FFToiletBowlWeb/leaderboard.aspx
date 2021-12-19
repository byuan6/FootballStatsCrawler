<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="leaderboard.aspx.cs" Inherits="FFToiletBowlWeb.leaderboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .hideColumn 
        {
            display:none;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="captionBlock grow" style="display:inline-block; width:49%;vertical-align:top;">
        <h1>
            <asp:Label ID="Year" runat="server"/> Season<br>
            Week <asp:Label ID="Week" runat="server"/>
        </h1>
        <h2>Points Leaderboard</h2>
    </div>


    <!--  -->
    <div class="graphPane">
        <asp:Image ID="PointsVsRankLine" runat="server" />
    </div>

    <!-- Let's create a graph here, re-order when the graph is reordered. whatever column was clicked, it shows up in graph -->
    <script>
        const PAATT_INDEX = 4;
        const PAYD_INDEX = 5;
        const PATD_INDEX = 6;
        const PAINT_INDEX = 7;
        const RUATT_INDEX = 8;
        const RUYD_INDEX = 9;
        const RUTD_INDEX = 10;
        const RETGT_INDEX = 11; 
        const REATT_INDEX = 12;
        const REYD_INDEX = 13;
        const RETD_INDEX = 14;
        const KiFGA_INDEX = 15;
        const KiFGM_INDEX = 16;
        const DSACK_INDEX = 17;
        const DFR_INDEX = 18;
        const DINT_INDEX = 19;
        const DTD_INDEX = 20;
        const DPA_INDEX = 21;
        const DPAYD_INDEX = 22;
        const DRUYD_INDEX = 23;
        const DTOTYD_INDEX = 24;
        const AVGPT_INDEX = 25;
        function posfilterUI() {
            var tbl = document.getElementById("MainContent_RelativeValueGrid");
            var r = tbl.rows;
            var len = r.length;
            var posfilter = null;
            switch (posfilter) 
            {
                case "QB":
                    for(var i=0; i<len; i++) {
                        r[i].cells[PAATT_INDEX].classList.remove("hideColumn");
                        r[i].cells[PAYD_INDEX].classList.remove("hideColumn");
                        r[i].cells[PATD_INDEX].classList.remove("hideColumn");
                        r[i].cells[PAINT_INDEX].classList.remove("hideColumn");

                        r[i].cells[RUATT_INDEX].classList.remove("hideColumn");
                        r[i].cells[RUYD_INDEX].classList.remove("hideColumn");
                        r[i].cells[RUTD_INDEX].classList.remove("hideColumn");
                    }
                    break;
                case "RB":
                    for(var i=0; i<len; i++) {
                        r[i].cells[RUATT_INDEX].classList.remove("hideColumn");
                        r[i].cells[RUYD_INDEX].classList.remove("hideColumn");
                        r[i].cells[RUTD_INDEX].classList.remove("hideColumn");

                        r[i].cells[RETGT_INDEX].classList.remove("hideColumn");
                        r[i].cells[REATT_INDEX].classList.remove("hideColumn");
                        r[i].cells[REYD_INDEX].classList.remove("hideColumn");
                        r[i].cells[RETD_INDEX].classList.remove("hideColumn");
                    }
                    break;
                case "WR":
                case "TE":
                    for(var i=0; i<len; i++) {
                        r[i].cells[RETGT_INDEX].classList.remove("hideColumn");
                        r[i].cells[REATT_INDEX].classList.remove("hideColumn");
                        r[i].cells[REYD_INDEX].classList.remove("hideColumn");
                        r[i].cells[RETD_INDEX].classList.remove("hideColumn");
                    }
                    break;
                case "K":
                    for(var i=0; i<len; i++) {
                        r[i].cells[KiFGA_INDEX].classList.remove("hideColumn");
                        r[i].cells[KiFGM_INDEX].classList.remove("hideColumn");
                    }
                    break;
                case "DST":
                    for(var i=0; i<len; i++) {
                        r[i].cells[DSACK_INDEX].classList.remove("hideColumn");
                        r[i].cells[DFR_INDEX].classList.remove("hideColumn");
                        r[i].cells[DINT_INDEX].classList.remove("hideColumn");
                        r[i].cells[DTD_INDEX].classList.remove("hideColumn");
                        r[i].cells[DPA_INDEX].classList.remove("hideColumn");
                        r[i].cells[DPAYD_INDEX].classList.remove("hideColumn");
                        r[i].cells[DRUYD_INDEX].classList.remove("hideColumn");
                        r[i].cells[DTOTYD_INDEX].classList.remove("hideColumn");
                    }
                    break;
            }
        }

        window.onload = function () {
            setTimeout(function() {
                var tbl1 = document.getElementById("MainContent_RelativeValueGrid");
			    var ctx1 = document.getElementById('graphcanvas').getContext('2d');

                var parseNullableFloat = new Isnull(null, parseFloat);
                var test = selectToChartData(selectWhere(tbl1, 2, null, function(tr) { return tr.rowIndex!=0; }), 'Avg Pt/Gm', selectWhere(tbl1, AVGPT_INDEX, parseNullableFloat.convert, null), window.chartColors.blue);
                
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
						    text: 'Player Performance'
					    }
				    }
			    });
            }, 2000);

        }
    </script>
    <div style="width:100%; height:400px;">
        <canvas id="graphcanvas" style="width:100%; height:100%; background-color:white"></canvas>
    </div>

    <h2>Leaders</h2>
    <span>PPR | Standard</span>
    <span>4 | 6 | 8 | 10 | 12 | 14 | 16</span>
    <span>Starters | Rosterable | All</span>
    <span>QB,RB,RB,WR,WR,Flex,TE,K,DST | QB,RB,RB,WR,WR,TE,K,DST</span>
    <span>All | QB | RB | WR | TE | RB,WR | RB,WR,TE | K | DST</span>

    <!-- make this the player profile page, for this year? -->
    <!-- but then how are we going to keep it sortable? -->
    <div style="padding:0px;">
        <asp:GridView ID="RelativeValueGrid" DataKeyNames="PlayerID" AutoGenerateColumns="false" ShowHeader="true" CssClass="sortable contentTable" BorderStyle="None" runat="server">
        <Columns>
            <asp:BoundField HeaderText="#" DataField="OverallRank" ItemStyle-CssClass="" />
            <asp:BoundField HeaderText="PosRk" DataField="PosRank" ItemStyle-CssClass="" />

            <asp:BoundField HeaderText="Player" DataField="Player" ItemStyle-CssClass="" />
            <asp:BoundField HeaderText="Team" DataField="Team" ItemStyle-CssClass="" />

            
            <asp:BoundField HeaderText="PaAtt" DataField="PaAtt" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn" HeaderStyle-CssClass="hideColumn" />
            <asp:BoundField HeaderText="PaYd" DataField="PaYd" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn" HeaderStyle-CssClass="hideColumn" />
            <asp:BoundField HeaderText="PaTD" DataField="PaTD" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="PaINT" DataField="PaINT" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>

            <asp:BoundField HeaderText="RuAtt" DataField="RuAtt" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="RuYd" DataField="RuYd" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="RuTD" DataField="RuTD" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>

            <asp:BoundField HeaderText="ReTgt" DataField="ReTgt" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="ReRec" DataField="ReRec" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="ReYd" DataField="ReYd" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="ReTD" DataField="ReTD" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>

            <asp:BoundField HeaderText="KiFGA" DataField="KiFGA" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="KiFGM" DataField="KiFGM" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
                
            <asp:BoundField HeaderText="DSack" DataField="DSack" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="DFR" DataField="DFR" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="DInt" DataField="DInt" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="DTD" DataField="DTD" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="DPA" DataField="DPA" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="DPaYd" DataField="DPaYd" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="DRuYd" DataField="DRuYd" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>
            <asp:BoundField HeaderText="DTotYd" DataField="DTotYd" ReadOnly="true" ItemStyle-CssClass="altColorColumn hideColumn"  HeaderStyle-CssClass="hideColumn"/>


            <asp:BoundField HeaderText="AvgPt" DataField="AvgPt" DataFormatString="{0:0.0}" ItemStyle-CssClass="" />
            <asp:BoundField HeaderText="LstGmPt" DataField="LastGmPt" DataFormatString="{0:0}" ItemStyle-CssClass="" />
            
            <asp:BoundField HeaderText="YrTotPt" DataField="TotPt" DataFormatString="{0:0}" ItemStyle-CssClass="" />
            <asp:BoundField HeaderText="PrjYrTotPt" DataField="ProjectedEndOfSeasonPoints" DataFormatString="{0:0}" ItemStyle-CssClass="" />
            <asp:BoundField HeaderText="Remaining" DataField="GamesRemaining" DataFormatString="{0:0}" ItemStyle-CssClass="" />
        </Columns>
        </asp:GridView>
    </div>
</asp:Content>



