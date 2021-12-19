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
    public partial class TradeValuation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var yearweek = Request.QueryString["yearweek"];
            // page configuration
            string dataset = "crosstabTradeRank";
            

            string scoringfilter = "ScoringSystem='PPR'";
            var score = Request.QueryString["scoring"];
            if (score == "standard")
                scoringfilter = "ScoringSystem='Standard'";

            var displayset = "PresentValueOfPlayer";
            //displayset = "EstimatedPtPotential";
            //displayset = "EstimatedPtRemaining";
            //displayset = "NextWeekEst";

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
                    data.Columns.Add("PosRank2", typeof(string), "Pos + PosRank");
                    data.Columns.Add("SeasonAvg", typeof(Double), "TotPt/count");
                    data.Columns.Add("QB", typeof(string), "iif(Pos='QB',Player,'')");
                    data.Columns.Add("RB", typeof(string), "iif(Pos='RB',Player,'')");
                    data.Columns.Add("WR", typeof(string), "iif(Pos='WR',Player,'')");
                    data.Columns.Add("TE", typeof(string), "iif(Pos='TE',Player,'')");
                    data.Columns.Add("K", typeof(string), "iif(Pos='K',Player,'')");
                    data.Columns.Add("DST", typeof(string), "iif(Pos='DST',Player,'')");

                    data.Columns.Add("QBPts", typeof(double), string.Format("iif(Pos='QB',{0},null)", displayset));
                    data.Columns.Add("RBPts", typeof(double), string.Format("iif(Pos='RB',{0},null)", displayset));
                    data.Columns.Add("WRPts", typeof(double), string.Format("iif(Pos='WR',{0},null)", displayset));
                    data.Columns.Add("TEPts", typeof(double), string.Format("iif(Pos='TE',{0},null)", displayset));
                    data.Columns.Add("KPts", typeof(double), string.Format("iif(Pos='K',{0},null)", displayset));
                    data.Columns.Add("DSTPts", typeof(double), string.Format("iif(Pos='DST',{0},null)", displayset));

                    data.Columns["QBPts"].AllowDBNull = true;
                    data.Columns["RBPts"].AllowDBNull = true;
                    data.Columns["WRPts"].AllowDBNull = true;
                    data.Columns["TEPts"].AllowDBNull = true;
                    data.Columns["KPts"].AllowDBNull = true;
                    data.Columns["DSTPts"].AllowDBNull = true;

                    view.RowFilter = scoringfilter;
                    var tbl = view.ToTable();

                    var dictionary = tbl.ToEnumerable().GroupBy(s => (string)s["method"]).ToDictionary(s => s.Key, g => g.Count()).OrderByDescending(s => s.Value).FirstOrDefault();
                    ModellingMethod.Text = dictionary.Key;

                    //var positions = tbl.ToEnumerable().GroupBy(s => (string)s["Pos"]).ToDictionary(s => s.Key, g => g.ToArray());
                    RelativeValueGrid.RowDataBound += new GridViewRowEventHandler(RelativeValueGrid_RowDataBound);

                    RelativeValueGrid.DataSource = tbl.ToEnumerable().OrderByDescending(s => s["MvpPct"] == DBNull.Value ? 0 : (double)s["MvpPct"]).Take(16 * (3 + 4 + 4 + 2 + 2 + 2 + 1)).CopyToDataTable();
                    RelativeValueGrid.DataBind();

                    PtsVsMvpPerPosGraph.ImageUrl = getMvpRegressionPointChartBase64String(view);
                }
            }
            */
        }

        void RelativeValueGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //e.Row.DataItem
        }


        int _requestyear = 0;
        int _requestweek = 0;
        string _displayset = "EstimatedPtPotential";



        string getMvpRegressionPointChartBase64String(DataView view)
        {
            var dictionary = view.ToEnumerable().Where(s => (string)s["MvpSource"] == "SIMULATED").GroupBy(s => (string)s["Pos"]).ToDictionary(s => s.Key, g => g.ToArray());

            //var qb = rows.Where(s => (string)s["Pos"] == "QB").Where(s => s["NextWeekEstimate"] != DBNull.Value).OrderByDescending(s => (double)s["NextWeekEstimate"]).Take(32);
            //var x1 = view.ToEnumerable().Min(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            //var x2 = view.ToEnumerable().Max(s => s["SeasonAvg"] != DBNull.Value ? (double)s["SeasonAvg"] : 0);
            //var y1 = view.ToEnumerable().Min(s => s["MvpPct"] != DBNull.Value ? (double)s["MvpPct"] : 0);
            //var y2 = view.ToEnumerable().Max(s => s["MvpPct"] != DBNull.Value ? (double)s["MvpPct"] : 0);

            var converters = new List<DataPoint>();
            var series = new List<DataSeries>();
            foreach (var item in dictionary)
            {
                series.Add(new DataSeries(item.Value.Select(s => s["SeasonAvg"]), item.Value.Select(s => s["MvpPct"])) //, item.Value.Select(s => s["Player"]))
                {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point,
                    Name = item.Key,
                    LabelX = "Season Average",
                    LabelY = "MVP%",
                    IsTrendlineEnabled = true
                });
                converters.Add(new DataPoint() { XType= DataPoint.DataType.tonumber, YType= DataPoint.DataType.tonumber, ZType=DataPoint.DataType.tostring });
            }



            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot QBDefenseHandicap.png true size 1200 800 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset WR_Performance Column tostring 0 tonumber 10 -1 1 WR subset TE_Performance Column tostring 0 tonumber 10 -1 1 TE subset "Passing StDev" FastLine tostring 0 tonumber 11 -1 1 QB | AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot RBDefenseHandicap.png  true size 900 400 subset QB_Performance Column tostring 0 tonumber 10 -1 1 QB subset "RB Performance" Column tostring 0 tonumber 10 -1 1 RB subset "RB +/- StDev" FastLine tostring 0 tonumber 11 -1 1 RB| AddImageToWordpress >> TeamDefense.html
            //FFToiletBowlSQL vwStatsGmToPosDeltaPerTeam | plot KDefenseHandicap.png   true size 600 400 subset "K Performance" Column tostring 0 tonumber 10 -1 1 K| AddImageToWordpress >> TeamDefense.html

            var chartboundaries = new List<Plot.Program.ChartLine>();
            var size = new System.Drawing.Size(1024, 600);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Plot.Program.CreateChart(ms, string.Format("Regression to produce model of Avg Pts to MVP% for {0}, Week {1}", _requestyear, _requestweek), size, series, converters, chartboundaries);
                var bytes = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }
    }
}