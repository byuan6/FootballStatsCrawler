using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using FFToiletBowl;
using Plot;


namespace FFToiletBowlWeb
{
    public partial class DefenseRating : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var yearweek = Request.QueryString["yearweek"];
            // page configuration
            string dataset = "crosstabPointsGmToPosDeltaPerTeam";
            var set = Request.QueryString["dataset"];
            if (set == "stats")
                dataset = "crosstabStatsGmToPosDeltaPerTeam";

            string scoringfilter = "ScoringSystem='PPR'";
            var score = Request.QueryString["scoring"];
            if (score == "standard")
                scoringfilter = "ScoringSystem='Standard'";

            //UI states
            PointsLink.Enabled = dataset != "crosstabPointsGmToPosDeltaPerTeam";
            StatsLink.Enabled = dataset != "crosstabStatsGmToPosDeltaPerTeam";
            PPRLink.Enabled = !PointsLink.Enabled && scoringfilter != "ScoringSystem='PPR'";
            StandardLink.Enabled = !PointsLink.Enabled && dataset != "ScoringSystem='Standard'";

            PointsLink.NavigateUrl = string.Format("{0}?dataset=points&yearweek={1}", this.Request.Url.AbsolutePath, yearweek);
            StatsLink.NavigateUrl = string.Format("{0}?dataset=stats&yearweek={1}", this.Request.Url.AbsolutePath, yearweek);
            PPRLink.NavigateUrl = string.Format("{0}?scoring=ppr&yearweek={1}", this.Request.Url.AbsolutePath, yearweek);
            StandardLink.NavigateUrl = string.Format("{0}?scoring=standard&yearweek={1}", this.Request.Url.AbsolutePath, yearweek);

            if (dataset == "crosstabPointsGmToPosDeltaPerTeam") { PointsLink.Style.Add("font-weight", "bold"); PointsLink.Style.Add("color", "black"); }
            if (dataset == "crosstabStatsGmToPosDeltaPerTeam") { StatsLink.Style.Add("font-weight", "bold"); StatsLink.Style.Add("color", "black"); }
            if (StatsLink.Enabled && scoringfilter == "ScoringSystem='PPR'") { PPRLink.Style.Add("font-weight", "bold"); PPRLink.Style.Add("color", "black"); }
            if (StatsLink.Enabled && dataset == "ScoringSystem='Standard'") { StandardLink.Style.Add("font-weight", "bold"); StandardLink.Style.Add("color", "black"); }

            //databbinding
            //PPR.RowDataBound += new GridViewRowEventHandler(PPR_RowDataBound);
            if (!string.IsNullOrWhiteSpace(yearweek))
            {
                var parts = yearweek.Split('-');
                if (parts.Length == 2)
                {
                    var year = 0;
                    var wk = 0;
                    /*
                    if (int.TryParse(parts[0], out year) && int.TryParse(parts[1], out wk))
                    {
                        this.Year.Text = parts[0];
                        this.Week.Text = parts[1];

                        Reports.SetYear(year);
                        using (var data = Reports.GetTableByWeek(dataset, year, wk))
                        using (var view1 = new System.Data.DataView(data))
                        {
                            view1.RowFilter = scoringfilter;
                            //PPR.RowDataBound += OnRowDataBound;

                            PPR.DataSource = view1;
                            PPR.DataBind();
                        }

                        using (var data2 = Reports.GetTable("vwStatsGmToPosDeltaPerTeam"))
                        using (var view2 = new System.Data.DataView(data2))
                        {
                            view2.RowFilter=scoringfilter;
                            RunPassVersusSplits.ImageUrl = getPassingSplitsColumnChartBase64String(view2);
                            PassVersusSplits.ImageUrl = getRunPassSplitsColumnChartBase64String(view2);
                            KickerVersusSplits.ImageUrl = getKickerVersusSplitsColumnChartBase64String(view2);
                        }
                    }
                     * */
                }
            }
        }

        void PPR_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row != null && e.Row.DataItem!=null)
            {
                var item = ((System.Data.DataRowView)e.Row.DataItem).Row;
                e.Row.Cells[1].Attributes.Add("title", item["QB"].ToString());
                e.Row.Cells[2].Attributes.Add("title", item["RB"].ToString());
                e.Row.Cells[3].Attributes.Add("title", item["WR"].ToString());
                e.Row.Cells[4].Attributes.Add("title", item["TE"].ToString());
                e.Row.Cells[5].Attributes.Add("title", item["K"].ToString());
                e.Row.Cells[6].Attributes.Add("title", item["DST"].ToString());
            }
        }

        private IEnumerable<System.Data.DataRow> ToEnumerable(System.Data.DataTable dataTable)
        {
            return dataTable.ToEnumerable();
        }

        string getPassingSplitsColumnChartBase64String(System.Data.DataView view)
        {
            var qb = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "QB").ToList();
            //var rb = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "RB").ToList();
            var wr = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "WR").ToList();
            var te = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "TE").ToList();
            //var k = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "K").ToList();
            //var dst = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "DST").ToList();
            var series = new List<DataSeries>()
            {
                new DataSeries(qb.Select(s=>s["Team"]), qb.Select(s=>s["TotPtPct"])) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                    Name="QB Performance Versus",
                    LabelX="Team",
                    LabelY="Tot Pt (% of Avg)", 
                },
                new DataSeries(wr.Select(s=>s["Team"]), wr.Select(s=>s["TotPtPct"])) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                    Name="WR Performance Versus",
                    LabelX="Team",
                    LabelY="Tot Pt (% of Avg)", 
                },
                new DataSeries(te.Select(s=>s["Team"]), te.Select(s=>s["TotPtPct"])) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                    Name="TE Performance Versus",
                    LabelX="Team",
                    LabelY="Tot Pt (% of Avg)", 
                },
                new DataSeries(qb.Select(s=>s["Team"]), qb.Select(s=>s["StdevPtPct"])) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                    Name="QB Performance Versus (St dev)",
                    LabelX="Team",
                    LabelY="Tot Pt (St. Dev)", 
                }
            };
            var converters = new List<DataPoint>()
            {
                new DataPoint() { XType= DataPoint.DataType.tostring, YType= DataPoint.DataType.tonumber },
                new DataPoint() { XType= DataPoint.DataType.tostring, YType= DataPoint.DataType.tonumber },
                new DataPoint() { XType= DataPoint.DataType.tostring, YType= DataPoint.DataType.tonumber },
                new DataPoint() { XType= DataPoint.DataType.tostring, YType= DataPoint.DataType.tonumber },
            };

            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot QBDefenseHandicap.png true size 1200 800 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset WR_Performance Column tostring 0 tonumber 10 -1 1 WR subset TE_Performance Column tostring 0 tonumber 10 -1 1 TE subset "Passing StDev" FastLine tostring 0 tonumber 11 -1 1 QB | AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot RBDefenseHandicap.png  true size 900 400 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset "RB Performance" Column tostring 0 tonumber 10 -1 1 RB subset "RB +/- StDev" FastLine tostring 0 tonumber 11 -1 1 RB| AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot KDefenseHandicap.png   true size 600 400 subset "K Performance" Column tostring 0 tonumber 10 -1 1 K| AddImageToWordpress >> TeamDefense.html


            var chartboundaries = new List<Plot.Program.ChartLine>();
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, "Versus Pass / Receiving splits", size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
            
        }


        string getRunPassSplitsColumnChartBase64String(System.Data.DataView view)
        {
            var qb = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "QB").ToList();
            var rb = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "RB").ToList();
            //var wr = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "WR").ToList();
            //var te = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "TE").ToList();
            //var k = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "K").ToList();
            //var dst = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "DST").ToList();
            var series = new List<DataSeries>()
            {
                 new DataSeries(rb.Select(s=>s["Team"]), rb.Select(s=>s["TotPtPct"])) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                    Name="RB Performance Versus",
                    LabelX="Team",
                    LabelY="Tot Pt (% of Avg)", 
                },
                new DataSeries(qb.Select(s=>s["Team"]), qb.Select(s=>s["TotPtPct"])) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                    Name="QB Performance Versus",
                    LabelX="Team",
                    LabelY="Tot Pt (% of Avg)", 
                },
                new DataSeries(qb.Select(s=>s["Team"]), qb.Select(s=>s["StdevPtPct"])) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                    Name="RB Performance Versus (St dev)",
                    LabelX="Team",
                    LabelY="Tot Pt (% of Avg)", 
                }
            };
            var converters = new List<DataPoint>()
            {
                new DataPoint() { XType= DataPoint.DataType.tostring, YType= DataPoint.DataType.tonumber },
                new DataPoint() { XType= DataPoint.DataType.tostring, YType= DataPoint.DataType.tonumber },
                new DataPoint() { XType= DataPoint.DataType.tostring, YType= DataPoint.DataType.tonumber },
            };

            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot QBDefenseHandicap.png true size 1200 800 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset WR_Performance Column tostring 0 tonumber 10 -1 1 WR subset TE_Performance Column tostring 0 tonumber 10 -1 1 TE subset "Passing StDev" FastLine tostring 0 tonumber 11 -1 1 QB | AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot RBDefenseHandicap.png  true size 900 400 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset "RB Performance" Column tostring 0 tonumber 10 -1 1 RB subset "RB +/- StDev" FastLine tostring 0 tonumber 11 -1 1 RB| AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot KDefenseHandicap.png   true size 600 400 subset "K Performance" Column tostring 0 tonumber 10 -1 1 K| AddImageToWordpress >> TeamDefense.html


            var chartboundaries = new List<Plot.Program.ChartLine>();
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, "Versus Run / Pass splits", size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }


        string getKickerVersusSplitsColumnChartBase64String(System.Data.DataView view)
        {
            //var qb = ToEnumerable(view.Table).Where(s => (string)s["Pos"] == "QB").ToList();
            //var rb = ToEnumerable(view.Table).Where(s => (string)s["Pos"] == "RB").ToList();
            //var wr = ToEnumerable(view.Table).Where(s => (string)s["Pos"] == "WR").ToList();
            //var te = ToEnumerable(view.Table).Where(s => (string)s["Pos"] == "TE").ToList();
            var k = ToEnumerable(view.Table).Where(s => (string)s["OpposingPos"] == "K").ToList();
            //var dst = ToEnumerable(view.Table).Where(s => (string)s["Pos"] == "DST").ToList();
            var series = new List<DataSeries>()
            {
                new DataSeries(k.Select(s=>s["Team"]), k.Select(s=>s["TotPtPct"])) {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                    Name="K Performance Versus",
                    LabelX="Team",
                    LabelY="Tot Pt (% of Avg)", 
                }
            };
            var converters = new List<DataPoint>()
            {
                new DataPoint() { XType= DataPoint.DataType.tostring, YType= DataPoint.DataType.tonumber },
            };

            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot QBDefenseHandicap.png true size 1200 800 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset WR_Performance Column tostring 0 tonumber 10 -1 1 WR subset TE_Performance Column tostring 0 tonumber 10 -1 1 TE subset "Passing StDev" FastLine tostring 0 tonumber 11 -1 1 QB | AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot RBDefenseHandicap.png  true size 900 400 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset "RB Performance" Column tostring 0 tonumber 10 -1 1 RB subset "RB +/- StDev" FastLine tostring 0 tonumber 11 -1 1 RB| AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot KDefenseHandicap.png   true size 600 400 subset "K Performance" Column tostring 0 tonumber 10 -1 1 K| AddImageToWordpress >> TeamDefense.html


            var chartboundaries = new List<Plot.Program.ChartLine>();
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, "Versus Kickers", size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }
    }
}