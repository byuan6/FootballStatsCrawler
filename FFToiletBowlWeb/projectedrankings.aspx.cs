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
    public partial class projectedrankings : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /*
            var yearweek = Request.QueryString["yearweek"];
            // page configuration
            string dataset = "crosstabFutureRank";
            

            string scoringfilter = "ScoringSystem='PPR'";
            var score = Request.QueryString["scoring"];
            if (score == "standard")
                scoringfilter = "ScoringSystem='Standard'";

            var displayset = "NextWeekEst";
            displayset = "EstimatedPtPotential";
            displayset = "EstimatedPtRemaining";
            displayset = "PresentValueOfPlayer";

            string year = null;
            string week = null;
            if (yearweek.TryParseYearWeekQuery(out year, out week, out _requestyear, out _requestweek))
            {
                this.Year.Text = year;
                this.Week.Text = week;

                using (var data = Reports.GetTableByWeek(dataset, _requestyear, _requestweek))
                using (var view = new DataView(data))
                {
                    //data.Columns.Add("EstimatedPtRemaining", typeof(Double), "EstimatedPotentialPt+TotPt");
                    data.Columns.Add("SeasonAvg", typeof(Double), "TotPt/count");
                    view.RowFilter = scoringfilter;
                    var tbl = view.ToTable();

                    var dictionary = tbl.ToEnumerable().GroupBy(s => (string)s["method"]).ToDictionary(s => s.Key, g => g.Count()).OrderByDescending(s => s.Value).FirstOrDefault();
                    ModellingMethod.Text = dictionary.Key;

                    var positions = tbl.ToEnumerable().GroupBy(s => (string)s["Pos"]).ToDictionary(s => s.Key, g => g.ToArray());

                    QBView.DataSource = positions["QB"].OrderByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["NextWeekEstimate"]).Take(32).CopyToDataTable();
                    QBView.DataBind();
                    //RBView.DataSource = tbl.ToEnumerable().Where(s => (string)s["Pos"] == "RB").OrderByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["NextWeekEstimate"]).Take(80).CopyToDataTable();
                    RBView.DataSource = positions["RB"].OrderByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["NextWeekEstimate"]).Take(80).CopyToDataTable();
                    RBView.DataBind();
                    WRView.DataSource = positions["WR"].OrderByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["NextWeekEstimate"]).Take(80).CopyToDataTable();
                    WRView.DataBind();
                    TEView.DataSource = positions["TE"].OrderByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["NextWeekEstimate"]).Take(32).CopyToDataTable();
                    TEView.DataBind();
                    KView.DataSource = positions["K"].OrderByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["NextWeekEstimate"]).Take(32).CopyToDataTable();
                    KView.DataBind();
                    DSTView.DataSource = positions["DST"].OrderByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["NextWeekEstimate"]).CopyToDataTable();
                    DSTView.DataBind();
                    FlexView.DataSource = positions["RB"].Concat(positions["WR"]).Concat(positions["TE"]).OrderByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["NextWeekEstimate"]).Take(16 * 7).CopyToDataTable();
                    FlexView.DataBind();

                    PassTrend.ImageUrl = getQbTrendPointChartBase64String(positions["QB"]); //(view);
                    RunTrend.ImageUrl = getRbTrendPointChartBase64String(positions["RB"]); //(view);
                    DefenseTrend.ImageUrl = getDefenseTrendPointChartBase64String(positions["DST"]); //(view);
                }

                
                using (var data = Reports.GetData("vwExpectedVsActualYearlySummary"))
                using (var view = new DataView(data))
                {
                    view.RowFilter = scoringfilter;
                    var tbl = view.Table;

                    ModelTrend.DataSource = view;
                    ModelTrend.DataBind();
                }

                using (var data = Reports.GetData("vwExpectedVsActualSeasonDepthSummary"))
                using (var view = new DataView(data))
                {
                    view.RowFilter = scoringfilter;
                    var tbl = view.Table;

                    ModelEffectiveness.DataSource = view;
                    ModelEffectiveness.DataBind();
                }
            }
             */
        }

        int _requestyear = 0;
        int _requestweek = 0;

        string getQbTrendPointChartBase64String(DataView view)
        {
            var qb = view.ToTable().ToEnumerable().Where(s => (string)s["Pos"] == "QB").Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);
            return getRbTrendPointChartBase64String(qb.ToArray());
        }
        string getQbTrendPointChartBase64String(DataRow[] rows)
        {
            var qb = rows.Where(s => (string)s["Pos"] == "QB").Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);
                
            var series = new List<DataSeries>()
            {
                 new DataSeries(qb.Select(s=>s["SeasonAvg"]), qb.Select(s=>s["NextWeekEstimate"]), qb.Select(s=>s["Player"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="QB Estimates",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                },
            };
            var converters = new List<DataPoint>()
            {
                new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring },
                new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring },
                new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring },
            };

            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot QBDefenseHandicap.png true size 1200 800 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset WR_Performance Column tostring 0 tonumber 10 -1 1 WR subset TE_Performance Column tostring 0 tonumber 10 -1 1 TE subset "Passing StDev" FastLine tostring 0 tonumber 11 -1 1 QB | AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot RBDefenseHandicap.png  true size 900 400 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset "RB Performance" Column tostring 0 tonumber 10 -1 1 RB subset "RB +/- StDev" FastLine tostring 0 tonumber 11 -1 1 RB| AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot KDefenseHandicap.png   true size 600 400 subset "K Performance" Column tostring 0 tonumber 10 -1 1 K| AddImageToWordpress >> TeamDefense.html


            var chartboundaries = new List<Plot.Program.ChartLine>() { new Plot.Program.ChartLine() { x1 = 0, y1 = 0, x2 = 25, y2 = 25 } };
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("QB expectations for {0}, Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }

        string getRbTrendPointChartBase64String(DataView view)
        {
            var rb = view.ToTable().ToEnumerable().Where(s => (string)s["Pos"] == "RB").Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);
            return getRbTrendPointChartBase64String(rb.ToArray());
        }
        string getRbTrendPointChartBase64String(DataRow[] rows)
        {
            var rb = rows.Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);

            var series = new List<DataSeries>()
            {
                 new DataSeries(rb.Select(s=>s["SeasonAvg"]), rb.Select(s=>s["NextWeekEstimate"]), rb.Select(s=>s["Player"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="RB Estimates",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                },
            };
            var converters = new List<DataPoint>()
            {
                new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring },
                new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring },
                new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring },
            };

            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot QBDefenseHandicap.png true size 1200 800 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset WR_Performance Column tostring 0 tonumber 10 -1 1 WR subset TE_Performance Column tostring 0 tonumber 10 -1 1 TE subset "Passing StDev" FastLine tostring 0 tonumber 11 -1 1 QB | AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot RBDefenseHandicap.png  true size 900 400 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset "RB Performance" Column tostring 0 tonumber 10 -1 1 RB subset "RB +/- StDev" FastLine tostring 0 tonumber 11 -1 1 RB| AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot KDefenseHandicap.png   true size 600 400 subset "K Performance" Column tostring 0 tonumber 10 -1 1 K| AddImageToWordpress >> TeamDefense.html


            var chartboundaries = new List<Plot.Program.ChartLine>() { new Plot.Program.ChartLine() { x1=0, y1=0, x2=25, y2=25 } };
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("RB expectations for {0}, Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }


        string getDefenseTrendPointChartBase64String(DataView view)
        {
            var dst = view.ToTable().ToEnumerable().Where(s => (string)s["Pos"] == "DST").Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);
            return getRbTrendPointChartBase64String(dst.ToArray());
        }
        string getDefenseTrendPointChartBase64String(DataRow[] rows)
        {
            var dst = rows.Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);

            var series = new List<DataSeries>()
            {
                 new DataSeries(dst.Select(s=>s["SeasonAvg"]), dst.Select(s=>s["NextWeekEstimate"]), dst.Select(s=>s["Player"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="DST Estimates",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                },
            };
            var converters = new List<DataPoint>()
            {
                new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring },
                new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring },
                new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring },
            };

            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot QBDefenseHandicap.png true size 1200 800 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset WR_Performance Column tostring 0 tonumber 10 -1 1 WR subset TE_Performance Column tostring 0 tonumber 10 -1 1 TE subset "Passing StDev" FastLine tostring 0 tonumber 11 -1 1 QB | AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot RBDefenseHandicap.png  true size 900 400 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset "RB Performance" Column tostring 0 tonumber 10 -1 1 RB subset "RB +/- StDev" FastLine tostring 0 tonumber 11 -1 1 RB| AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot KDefenseHandicap.png   true size 600 400 subset "K Performance" Column tostring 0 tonumber 10 -1 1 K| AddImageToWordpress >> TeamDefense.html


            var chartboundaries = new List<Plot.Program.ChartLine>() { new Plot.Program.ChartLine() { x1 = 0, y1 = 0, x2 = 10, y2 = 10, label2="y=x (Line of expected average performance)" } };
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("DST expectations for {0}, Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }


    }
}