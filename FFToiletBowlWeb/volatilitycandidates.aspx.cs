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
    public partial class volatilitycandidates : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
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

                /*
                using (var data = Reports.GetTableByWeek(dataset, _requestyear, _requestweek))
                using (var view = new DataView(data))
                {
                    data.Columns.Add("Reason", typeof(string), "Player + '(' + Team + ') vs ' + Versus");
                    data.Columns.Add("EstimatedPtRemaining", typeof(Double), "EstimatedPotentialPt-TotPt");
                    data.Columns.Add("SeasonAvg", typeof(Double), "TotPt/count");
                    data.Columns.Add("Diff", typeof(Double), "NextWeekEstimate-TotPt/count");
                    data.Columns.Add("Flag", typeof(bool));
                    data.Columns.Add("FlagMarker", typeof(string), "iif(Flag,'X','')");
                    view.RowFilter = scoringfilter;
                    var tbl = view.ToTable();

                    var dictionary = tbl.ToEnumerable().GroupBy(s => (string)s["method"]).ToDictionary(s => s.Key, g => g.Count()).OrderByDescending(s => s.Value).FirstOrDefault();
                    ModellingMethod.Text = dictionary.Key;

                    var positions = tbl.ToEnumerable().GroupBy(s => (string)s["Pos"]).ToDictionary(s => s.Key, g => g.ToArray());


                    //sort by diff, get the top 4 or 5, then sort by estimate value, skip until one of the 4 shows up, then show highlighted, then show next value sorted by estimate value, dont highlight if not flagged
                    //show next 5 they beat, then skip to next 
                      
                    //show most volatile player   20pts (avg 15pts)
                                                                   will perform better than (by average AND projected) 18pts(avg 15pts)
                     ///next most voltile player  
                     //                                              will perform better than (by average AND projected) 18pts(avg 15pts)

                    BetterView.RowDataBound += new GridViewRowEventHandler(View_RowDataBound);
                    StruggleView.RowDataBound += new GridViewRowEventHandler(LowView_RowDataBound);


                    BetterView.DataSource = GetHighestDiff(view).CopyToDataTable();
                    BetterView.DataBind();
                    StruggleView.DataSource = GetLowestDiff(view).CopyToDataTable();
                    StruggleView.DataBind();

                    // show the defense faced for each point, in the graph label as indicator of primary mode of difference from average

                    QbShuffle.ImageUrl = getQbTrendPointChartBase64String(positions["QB"]); //(view);
                    RbShuffle.ImageUrl = getRbTrendPointChartBase64String(positions["RB"]); //(view);
                    WrTeShuffle.ImageUrl = getWrTrendPointChartBase64String(positions["WR"].Concat(positions["TE"])); //(view);
                    KShuffle.ImageUrl = getKTrendPointChartBase64String(positions["K"]); //(view);
                    DstShuffle.ImageUrl = getDstTrendPointChartBase64String(positions["DST"]); //(view);

                }*/

                /*
                using (var data = Reports.GetTable("vwExpectedVsActualYearlySummary"))
                using (var view = new DataView(data))
                {
                    view.RowFilter = scoringfilter;
                    var tbl = view.Table;

                    ModelTrend.DataSource = view;
                    ModelTrend.DataBind();
                }

                using (var data = Reports.GetTable("vwExpectedVsActualSeasonDepthSummary"))
                using (var view = new DataView(data))
                {
                    view.RowFilter = scoringfilter;
                    var tbl = view.Table;

                    ModelEffectiveness.DataSource = view;
                    ModelEffectiveness.DataBind();
                }
                 * */
            }
        }

        void View_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Check the XXXX column - if empty, the YYYY needs highlighting!
            var row = e.Row.DataItem as DataRowView;
            if (row != null && row["Flag"] != null && row["Flag"]!=DBNull.Value && (bool)row["Flag"])
            {
                e.Row.CssClass = "bgGreen";
            }
        }
        void LowView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Check the XXXX column - if empty, the YYYY needs highlighting!
            var row = e.Row.DataItem as DataRowView;
            if (row != null)
            {
                if (row["Flag"] != null && row["Flag"] != DBNull.Value && (bool)row["Flag"])
                {
                    e.Row.CssClass = "bgRed";
                }
                else
                    e.Row.CssClass = "bgYellow";
            }
        }

        int _requestyear = 0;
        int _requestweek = 0;

        IEnumerable<DataRow> GetHighestDiff(DataView view)
        {
            double max = 0;
            var found = new Dictionary<string, int>();
            //var found = false;
            var high = view.ToEnumerable().OrderByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["Diff"]).Take(10).Select(s=>s.Row).ToList();
            foreach (var item in high)
                item["Flag"] = true;
            foreach (var item in view.ToEnumerable().OrderBy(s=>(string)s["Pos"]).ThenByDescending(s => s["NextWeekEstimate"] == DBNull.Value ? 0d : (double)s["NextWeekEstimate"]).Select(s => s.Row))
            {
                var pos = (string)item["Pos"];
                if (found.ContainsKey(pos) || high.Contains(item))
                {
                    if (!found.ContainsKey(pos))
                        found.Add(pos, 5);
                    else if (high.Contains(item))
                        found[pos] = 5;
                    yield return item;
                    if (found[pos]-- <= 0)
                        found.Remove(pos);
                }
            }
        }

        IEnumerable<DataRow> GetLowestDiff(DataView view)
        {
            var found = new Dictionary<string, int>();
            var low = view.ToEnumerable().OrderBy(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["Diff"]).Take(10).Select(s => s.Row).ToList();
            foreach (var item in low)
                item["Flag"] = true;
            foreach (var item in view.ToEnumerable().OrderBy(s =>(string)s["Pos"]).ThenBy(s => s["NextWeekEstimate"] == DBNull.Value ? 0 : (double)s["NextWeekEstimate"]).Select(s => s.Row))
            {
                var pos = (string)item["Pos"];
                if (found.ContainsKey(pos) || low.Contains(item))
                {
                    if (!found.ContainsKey(pos))
                        found.Add(pos, 5);
                    else if (low.Contains(item))
                        found[pos] = 5;
                    yield return item;
                    if (found[pos]-- <= 0)
                        found.Remove(pos);
                }
            }
        }


        string getQbTrendPointChartBase64String(IEnumerable<DataRow> rows)
        {
            var qb = rows.Where(s => (string)s["Pos"] == "QB").Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);
            var x1 = qb.Min(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var x2 = qb.Max(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var y1 = qb.Min(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            var y2 = qb.Max(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            const double MARGIN_OF_SIGNIFICANCE = 0.2;
            var qbbetter = qb.Where(s => (double)s["NextWeekEstimate"] > (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2);
            var qbworse = qb.Where(s => (double)s["NextWeekEstimate"] < (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);
            var qbmiddling = qb.Where(s => (double)s["NextWeekEstimate"] <= (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2 && (double)s["NextWeekEstimate"] >= (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);

            var series = new List<DataSeries>()
            {
                 new DataSeries(qbbetter.Select(s=>s["SeasonAvg"]), qbbetter.Select(s=>s["NextWeekEstimate"]), qbbetter.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Better than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(qbworse.Select(s=>s["SeasonAvg"]), qbworse.Select(s=>s["NextWeekEstimate"]), qbworse.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Worse than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(qbmiddling.Select(s=>s["SeasonAvg"]), qbmiddling.Select(s=>s["NextWeekEstimate"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Approx. Season Avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
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

            var chartboundaries = new List<Plot.Program.ChartLine>() { new Plot.Program.ChartLine() { x1 = Math.Min(x1, y1), y1 = Math.Min(x1, y1), x2 = Math.Max(x2, y2), y2 = Math.Max(x2, y2), label2 = "Line of expected avg performance" } };
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("QB Season Avg vs Projected Score for {0}, Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }


        string getRbTrendPointChartBase64String(IEnumerable<DataRow> rows)
        {
            var rb = rows.Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(16*4);

            var x1 = rb.Min(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var x2 = rb.Max(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var y1 = rb.Min(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            var y2 = rb.Max(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            const double MARGIN_OF_SIGNIFICANCE = 0.2;
            var rbbetter = rb.Where(s => (double)s["NextWeekEstimate"] > (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2);
            var rbworse = rb.Where(s => (double)s["NextWeekEstimate"] < (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);
            var rbmiddling = rb.Where(s => (double)s["NextWeekEstimate"] <= (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2 && (double)s["NextWeekEstimate"] >= (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);

            var series = new List<DataSeries>()
            {
                 new DataSeries(rbbetter.Select(s=>s["SeasonAvg"]), rbbetter.Select(s=>s["NextWeekEstimate"]), rbbetter.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Better than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(rbworse.Select(s=>s["SeasonAvg"]), rbworse.Select(s=>s["NextWeekEstimate"]), rbworse.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Worse than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(rbmiddling.Select(s=>s["SeasonAvg"]), rbmiddling.Select(s=>s["NextWeekEstimate"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Approx. Season Avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
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

            var chartboundaries = new List<Plot.Program.ChartLine>() { new Plot.Program.ChartLine() { x1 = Math.Min(x1, y1), y1 = Math.Min(x1, y1), x2 = Math.Max(x2, y2), y2 = Math.Max(x2, y2), label2 = "Line of expected avg performance" } };
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("RB Season Avg vs Projected Score for {0}, Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }

        string getWrTrendPointChartBase64String(IEnumerable<DataRow> rows)
        {
            var wr = rows.Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(16 * 6);
            var x1 = wr.Min(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var x2 = wr.Max(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var y1 = wr.Min(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            var y2 = wr.Max(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            const double MARGIN_OF_SIGNIFICANCE = 0.2;
            var wrbetter = wr.Where(s => (double)s["NextWeekEstimate"] > (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2);
            var wrworse = wr.Where(s => (double)s["NextWeekEstimate"] < (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);
            var wrmiddling = wr.Where(s => (double)s["NextWeekEstimate"] <= (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2 && (double)s["NextWeekEstimate"] >= (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);

            var series = new List<DataSeries>()
            {
                 new DataSeries(wrbetter.Select(s=>s["SeasonAvg"]), wrbetter.Select(s=>s["NextWeekEstimate"]), wrbetter.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Better than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(wrworse.Select(s=>s["SeasonAvg"]), wrworse.Select(s=>s["NextWeekEstimate"]), wrworse.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Worse than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(wrmiddling.Select(s=>s["SeasonAvg"]), wrmiddling.Select(s=>s["NextWeekEstimate"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Approx. Season Avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
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

            var chartboundaries = new List<Plot.Program.ChartLine>() { new Plot.Program.ChartLine() { x1 = Math.Min(x1, y1), y1 = Math.Min(x1, y1), x2 = Math.Max(x2, y2), y2 = Math.Max(x2, y2), label2 = "Line of expected avg performance" } };
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("WR & TE Season Avg vs Projected Score for {0}, Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }


        string getKTrendPointChartBase64String(IEnumerable<DataRow> rows)
        {
            var k = rows.Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);
            var x1 = k.Min(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var x2 = k.Max(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var y1 = k.Min(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            var y2 = k.Max(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            const double MARGIN_OF_SIGNIFICANCE = 0.2;
            var kbetter = k.Where(s => (double)s["NextWeekEstimate"] > (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2);
            var kworse = k.Where(s => (double)s["NextWeekEstimate"] < (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);
            var kmiddling = k.Where(s => (double)s["NextWeekEstimate"] <= (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2 && (double)s["NextWeekEstimate"] >= (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);

            var series = new List<DataSeries>()
            {
                 new DataSeries(kbetter.Select(s=>s["SeasonAvg"]), kbetter.Select(s=>s["NextWeekEstimate"]), kbetter.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Better than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(kworse.Select(s=>s["SeasonAvg"]), kworse.Select(s=>s["NextWeekEstimate"]), kworse.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Worse than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(kmiddling.Select(s=>s["SeasonAvg"]), kmiddling.Select(s=>s["NextWeekEstimate"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Approx. Season Avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
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
            
            var chartboundaries = new List<Plot.Program.ChartLine>() { new Plot.Program.ChartLine() { x1 = Math.Min(x1, y1), y1 = Math.Min(x1, y1), x2 = Math.Max(x2, y2), y2 = Math.Max(x2, y2), label2 = "Line of expected avg performance" } };
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("K Season Avg vs Projected Score for {0}, Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }

        }


        string getDstTrendPointChartBase64String(IEnumerable<DataRow> rows)
        {
            var dst = rows.Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);
            var x1 = dst.Min(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var x2 = dst.Max(s => s["NextWeekEstimate"] != DBNull.Value ? (double)s["NextWeekEstimate"] : 0);
            var y1 = dst.Min(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            var y2 = dst.Max(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            const double MARGIN_OF_SIGNIFICANCE = 0.2;
            var dstbetter = dst.Where(s => (double)s["NextWeekEstimate"] > (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2);
            var dstworse = dst.Where(s => (double)s["NextWeekEstimate"] < (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);
            var dstmiddling = dst.Where(s => (double)s["NextWeekEstimate"] <= (double)s["SeasonAvg"] + MARGIN_OF_SIGNIFICANCE * y2 && (double)s["NextWeekEstimate"] >= (double)s["SeasonAvg"] - MARGIN_OF_SIGNIFICANCE * y2);

            var series = new List<DataSeries>()
            {
                 new DataSeries(dstbetter.Select(s=>s["SeasonAvg"]), dstbetter.Select(s=>s["NextWeekEstimate"]), dstbetter.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Better than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(dstworse.Select(s=>s["SeasonAvg"]), dstworse.Select(s=>s["NextWeekEstimate"]), dstworse.Select(s=>s["Reason"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Worse than Season avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
                },
                new DataSeries(dstmiddling.Select(s=>s["SeasonAvg"]), dstmiddling.Select(s=>s["NextWeekEstimate"]) ) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name="Approx. Season Avg",
                    LabelX="Season Average",
                    LabelY="Projected Pts", 
                    IsTrendlineEnabled = false,
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

            var chartboundaries = new List<Plot.Program.ChartLine>() { new Plot.Program.ChartLine() { x1 = Math.Min(x1, y1), y1 = Math.Min(x1, y1), x2 = Math.Max(x2, y2), y2 = Math.Max(x2, y2), label2 = "Line of expected avg performance" } };
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("DST Season Avg vs Projected Score for {0}, Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }

        }


    }
}


