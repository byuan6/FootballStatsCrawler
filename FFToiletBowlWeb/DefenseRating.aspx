<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DefenseRating.aspx.cs" Inherits="FFToiletBowlWeb.DefenseRating" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="captionBlock grow" style="display:inline-block; width:49%;vertical-align:top;">
        <h1>
            <asp:Label ID="Year" runat="server"/> Season<br>
            Before Week <asp:Label ID="Week" runat="server"/>
        </h1>
        <h2>Defensive Deference</h2>
        <h3>Season Averages (avail after week 6)</h3>
    </div>

    <div class="graphPane">
        <asp:Image ID="RunPassVersusSplits" runat="server" />
        <!--img src="Images/rbdefensehandicap1.png" /-->
    </div>

            
    <div class="graphPane">
        <asp:Image ID="PassVersusSplits" runat="server" />
        <!--img src="Images/qbdefensehandicap1.png" /-->
    </div>

            
    <div class="graphPane">
        <asp:Image ID="KickerVersusSplits" runat="server" />
        <!--img src="Images/kdefensehandicap1.png" /-->
    </div>


    <!-- click on a team, and get's history for the season -->
    <!-- click on a pct, and get the relevant players for this week facing that defense -->
    <!-- hover, and get the yd and td splits -->
    <!-- click on a position and sort -->

    <h2>Average Outcome of Position's Stats, when playing against a Team</h2>
    <div style="padding:5px;">
        Avg(%)/gm of <span style="background-color:#EEEEEE"><asp:HyperLink ID="PointsLink" Text="FF Points" runat="server" /> |
                                               <asp:HyperLink ID="StatsLink" Text="Yards" runat="server" /></span>
        &nbsp;&nbsp;&nbsp;&nbsp;
        FF Points formula <span style="background-color:#EEEEEE"><asp:HyperLink ID="PPRLink" Text="PPR" runat="server" /> |
                                               <asp:HyperLink ID="StandardLink" Text="Standard" runat="server" /></span>
    </div>
    <div>
        <asp:GridView ID="PPR" DataKeyNames="Year" AutoGenerateColumns="false" ShowHeader="true" CssClass="contentTable sortable" BorderStyle="None" RowDataBound="PPR_RowDataBound" runat="server">
        <Columns>
            <asp:HyperLinkField HeaderText="Opp. Team" DataNavigateUrlFields="Team" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="Team" />
            <asp:HyperLinkField HeaderText="QB" DataNavigateUrlFields="QB" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="QB" DataTextFormatString="{0:0.0}%" ItemStyle-HorizontalAlign="Right" />
            <asp:HyperLinkField HeaderText="RB" DataNavigateUrlFields="RB" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="RB" DataTextFormatString="{0:0.0}%" ItemStyle-HorizontalAlign="Right" />
            <asp:HyperLinkField HeaderText="WR" DataNavigateUrlFields="WR" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="WR" DataTextFormatString="{0:0.0}%" ItemStyle-HorizontalAlign="Right" />
            <asp:HyperLinkField HeaderText="TE" DataNavigateUrlFields="TE" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="TE" DataTextFormatString="{0:0.0}%" ItemStyle-HorizontalAlign="Right" />
            <asp:HyperLinkField HeaderText="K"  DataNavigateUrlFields="K" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="K" DataTextFormatString="{0:0.0}%" ItemStyle-HorizontalAlign="Right" />
            <asp:HyperLinkField HeaderText="DST" DataNavigateUrlFields="DST" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="DST" DataTextFormatString="{0:0.0}%" ItemStyle-HorizontalAlign="Right" />
        </Columns>
        </asp:GridView>
    </div>

    1. The table is sortable.  <br />
    2. The [Team] link show graph of each game the Team has played included in the defense percentage, the average for Versus, and result for that week, and defensive percentage for that week.  <br />
    3. The links for the percentages will take you the projected stats for the relevant players, playing aginst that defense.<br />
    4. The chart means, for example, whenever a team plays ARI, the opposing QB's output on average is 20% less than his season average.  <br />
    5. But when chart shows that the ARI's DST scores 20% more, that means ARI's offense collectively terrible and opposing defenses easily hold down their defense, increasing the opposing DST point output on average by 20%.<br />
    6. Kicker Pct when Yards is select, is count of FGM, not yards.  DST shows Total Yards Against (reverse of DST points b/c DST scores more, when it gives up less yards and touchdowns).
    <div id="callout" class="hiddenPane">
		<canvas id="canvas" style="width:100%; height:100%; background-color:white"></canvas>
	</div>
    <script>
		window.onload = function() {
            var tbl = document.getElementById("MainContent_PPR");
			var ctx = document.getElementById('canvas').getContext('2d');

			window.myBarChart = new Chart(ctx, {
				type: 'bar',
				data: selectFromTable(tbl),
				options: {
					responsive: true,
					legend: {
						position: 'top',
					},
					title: {
						display: true,
						text: 'Team Defenses vs Opposing Pos output'
					}
				}
			});

            if (tbl.tHead == null) tbl.tHead = tbl.getElementsByTagName('thead')[0];
            var headerrow = tbl.tHead.rows[0].cells;
            for(var i=0; i <headerrow.length; i++) {
                headerrow[i].onmouseover = function() {
                    if(event.target.cellIndex==0)
                        return;
                    var x = event.target.offsetLeft;
                    var y = event.target.offsetTop;
                    var h = event.target.offsetHeight
                    var callout = document.getElementById("callout");
                    callout.className="unhiddenPane";

                    window.myBarChart.data.labels = selectColumnFromTable(tbl, 0, null);
                    window.myBarChart.data.datasets[0].label = event.target.innerText;
                    window.myBarChart.data.datasets[0].data = selectColumnFromTable(tbl,event.target.cellIndex, parseFloat);
                    window.myBarChart.data.datasets[1].data = selectColumnFromTable(tbl, 6, parseFloat);
                    window.myBarChart.update();
                }
                headerrow[i].onmouseout = function() {
                    var callout = document.getElementById("callout");
                    callout.className="hiddenPane";
                }
                headerrow[i].addEventListener("click", function() {
                    if(event.target.cellIndex==0)
                        return;
                    var x = event.target.offsetLeft;
                    var y = event.target.offsetTop;
                    var h = event.target.offsetHeight
                    var callout = document.getElementById("callout");
                    callout.className="unhiddenPane";

                    window.myBarChart.data.labels = selectColumnFromTable(tbl, 0, null);
                    window.myBarChart.data.datasets[0].label = event.target.innerText;
                    window.myBarChart.data.datasets[0].data = selectColumnFromTable(tbl,event.target.cellIndex, parseFloat);
                    window.myBarChart.data.datasets[1].data = selectColumnFromTable(tbl, 6, parseFloat);
                    window.myBarChart.update();
                }); 
            }
		};

        function selectColumnFromTable(tbl, columnnumber, castfn) {
            
            var tr = tbl.getElementsByTagName("tr");
            var td = new Array(tr.length-1);
            if(castfn!=null)
                for(var i = 0; i < tr.length-1; i++)
                    td[i] = castfn(tr[i+1].getElementsByTagName("td")[columnnumber].innerText);
            else
                for(var i = 0; i < tr.length-1; i++)
                    td[i] = tr[i+1].getElementsByTagName("td")[columnnumber].innerText;

            return td;
        }
        function selectFromTable(tbl) {
            //var t = document.getElementById("table"), // This have to be the ID of your table, not the tag

        	//var MONTHS = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
		    var color = Chart.helpers.color;
		    var barChartData = {
			    //labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July'],
                labels: selectColumnFromTable(tbl, 0, null),
			    datasets: [{
				    label: 'QB',
				    backgroundColor: color(window.chartColors.red).alpha(0.5).rgbString(),
				    borderColor: window.chartColors.red,
				    borderWidth: 1,
				    data: selectColumnFromTable(tbl, 1, parseFloat)
                }, {
				    label: 'DST',
				    backgroundColor: color(window.chartColors.purple).alpha(0.5).rgbString(),
				    borderColor: window.chartColors.purple,
				    borderWidth: 1,
				    data: selectColumnFromTable(tbl, 6, parseFloat)
			    }]
		    };
            
            return barChartData;
        }

        /*
        selectFromTable.datasets.forEach(function(dataset) {
			dataset.data = dataset.data.map(function() {
				return zero ? 0.0 : randomScalingFactor();
			});
		});

		window.myBar.update();*/
    </script>
</asp:Content>
