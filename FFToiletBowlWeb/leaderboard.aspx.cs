using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


using FFToiletBowl;
using Plot;
using System.Data;


namespace FFToiletBowlWeb
{
    public partial class leaderboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var yearweek = Request.QueryString["yearweek"];
            // page configuration
            string dataset = "tblPointsGrade";


            string scoringfilter = "ScoringSystem='PPR'";
            var score = Request.QueryString["scoring"];
            if (score == "standard")
                scoringfilter = "ScoringSystem='Standard'";

            /*
            string year = null;
            string week = null;
            if (yearweek.TryParseYearWeekQuery(out year, out week, out _requestyear, out _requestweek))
            {
                this.Year.Text = year;
                this.Week.Text = week;

                using (var data = Reports.GetTableByWeek(dataset, _requestyear, _requestweek))
                using (var view = new DataView(data))
                {
                    data.Columns.Add("PosRank", typeof(string), "Pos + iif(Rank<10,'0','') + Rank");
                    data.Columns.Add("SeasonAvg", typeof(Double), "TotPt/count");
                    data.Columns.Add("ProjectedEndOfSeasonPoints", typeof(Double), "EstimatedPotentialPt+TotPt-NextWeekEstimate");
                    data.Columns.Add("EstimatedPotentialPt2", typeof(Double), "EstimatedPotentialPt-NextWeekEstimate");

                    view.RowFilter = scoringfilter;
                    var tbl = view.ToTable();


                    //var positions = tbl.ToEnumerable().GroupBy(s => (string)s["Pos"]).ToDictionary(s => s.Key, g => g.ToArray());
                    //RelativeValueGrid.RowDataBound += new GridViewRowEventHandler(RelativeValueGrid_RowDataBound);

                    RelativeValueGrid.DataSource = tbl.ToEnumerable().OrderByDescending(s => (string)s["Pos"]).ThenByDescending(s => s["LastGmPt"] == DBNull.Value ? 0 : Convert.ToDouble(s["LastGmPt"])).Take(NUM_DISPLAYED_PLAYERS).CopyToDataTable();
                    RelativeValueGrid.DataBind();

                    PointsVsRankLine.ImageUrl = getRankVsProjectedPointsLineChartBase64String(view);

                }
            }*/
        }


        int _requestyear = 0;
        int _requestweek = 0;
        const int NUM_DISPLAYED_PLAYERS = 16 * (3 + 4 + 4 + 2 + 2 + 2 + 1);

        
        string getRankVsProjectedPointsLineChartBase64String(DataView view)
        {
            var data = view.ToEnumerable().OrderBy(s => s["OverallRank"] == DBNull.Value ? 0 : Convert.ToDouble(s["OverallRank"])).Take(NUM_DISPLAYED_PLAYERS);

            var converters = new List<DataPoint>() { new DataPoint() { XType = DataPoint.DataType.tostring, YType = DataPoint.DataType.tonumber, ZType = DataPoint.DataType.tostring }, new DataPoint() { XType = DataPoint.DataType.tostring, YType = DataPoint.DataType.tonumber, ZType = DataPoint.DataType.tostring } };
            var series = new List<DataSeries>()
            {
                new DataSeries(data.Select(s => s["Player"]), data.Select(s => s["TotPt"]))
                {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedColumn,
                    Name = null,
                    LabelX = "Player",
                    LabelY = "Points",
                },
                new DataSeries(data.Select(s => s["Player"]), data.Select(s => s["EstimatedPotentialPt2"]))
                {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedColumn,
                    Name = null,
                    LabelX = "Player",
                    LabelY = "Points",
                }
            };


            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot QBDefenseHandicap.png true size 1200 800 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset WR_Performance Column tostring 0 tonumber 10 -1 1 WR subset TE_Performance Column tostring 0 tonumber 10 -1 1 TE subset "Passing StDev" FastLine tostring 0 tonumber 11 -1 1 QB | AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot RBDefenseHandicap.png  true size 900 400 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset "RB Performance" Column tostring 0 tonumber 10 -1 1 RB subset "RB +/- StDev" FastLine tostring 0 tonumber 11 -1 1 RB| AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot KDefenseHandicap.png   true size 600 400 subset "K Performance" Column tostring 0 tonumber 10 -1 1 K| AddImageToWordpress >> TeamDefense.html


            var chartboundaries = new List<Plot.Program.ChartLine>();
            var size = new System.Drawing.Size(2048, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("Rank, Player vs Season Pts, Projected End of Year Points {0}, including Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }

    }
}