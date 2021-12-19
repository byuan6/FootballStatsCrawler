using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
//needed for charts
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;


namespace Plot
{
    public class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: [filename:required] [header records=true/false/comma list:optional] [size X y: optional] [series name type-x index-x type-y index-y index-z]");
                Console.WriteLine("Returns: filename successful, return value>0 otherwise");
                return 1;
            }
            string filename = args[0];

            int argstart = 1;
            bool pickupHeader = args.Length >= 2 ? args[1].ToLower() == "true" : false;
            if (pickupHeader) argstart = 2;

            Size size = new Size(600,400); //default ... get from config in future to configure
            bool customSize = args.Length >= 3 ? args[2].ToLower() == "size" : false;
            if (customSize)
            {
                bool valid = false;
                int w,h;
                if(int.TryParse(args[3], out w))
                    if (int.TryParse(args[4], out h))
                    {
                        size = new Size(w, h);
                        valid = true;
                    }
                if (!valid)
                {
                    Console.WriteLine("Usage: [filename:required] [header records=true/false/comma list:optional] [size X y: optional] [series name index-x index-y] [subset name index-x index-y index-compare value]");
                    Console.WriteLine("Invalid Size");
                    return 1;
                }
                argstart = 5;
            }


            List<ChartLine> chartboundaries = new List<ChartLine>();
            //charttype : default is column
            //Series : default is string(X), number(Y)
            //datatype in series definition : string, number, date
            List<TextSegmentator> segmentorList = new List<TextSegmentator>();
            List<DataPoint> converterList = new List<DataPoint>();
            List<DataSeries> seriesList = new List<DataSeries>();
            if (args.Length <= argstart)
            {
                TextSegmentator segmentator = new TextSegmentator()
                {
                    SourceX = 0,
                    SourceY = 1,
                    SourceZ = -1
                };
                segmentorList.Add(segmentator);

                DataPoint point = new DataPoint() { XType = DataPoint.DataType.tostring, YType = DataPoint.DataType.tonumber };
                converterList.Add(point);

                DataSeries series = new DataSeries();
                series.Name = "Data";
                series.ChartType = SeriesChartType.Column;
                seriesList.Add(series);
            }
            else
            {
                //read from command line
                //series [name] [chart] [datatype] [x-index] [datatype] [y-index]
                //[subset ... index-compare value]
                TextSegmentator segmentator = null;
                int start = argstart;
                int end = args.Length;
                while(start<end) {
                    if (args[start].ToLower() == "line")
                    {
                        if (start + 4 >= args.Length)
                        {
                            Console.WriteLine("not enough parameters for line");
                            return 1;
                        }
                        double x1 = 0;
                        if (!double.TryParse(args[start + 1], out x1))
                        {
                            Console.WriteLine("line, unknown x1: {0}", args[start + 1]);
                            return 1;
                        }
                        double y1 = 0;
                        if (!double.TryParse(args[start + 2], out y1))
                        {
                            Console.WriteLine("line, unknown y1: {0}", args[start + 2]);
                            return 1;
                        }
                        double x2 = 0;
                        if (!double.TryParse(args[start + 3], out x2))
                        {
                            Console.WriteLine("line, unknown x2: {0}", args[start + 3]);
                            return 1;
                        }
                        double y2 = 0;
                        if (!double.TryParse(args[start + 4], out y2))
                        {
                            Console.WriteLine("line, unknown y2: {0}", args[start + 4]);
                            return 1;
                        }
                        ChartLine newline = new ChartLine();
                        newline.x1 = x1;
                        newline.y1 = y1;
                        newline.x2 = x2;
                        newline.y2 = y2;
                        chartboundaries.Add(newline);

                        start += 5;
                    }
                    else if (args[start].ToLower() == "series" || args[start].ToLower() == "subset")
                    {
                        if (start + 6 >= args.Length)
                        {
                            Console.WriteLine("not enough parameters for series");
                            return 1;
                        }
                        

                        DataSeries series = new DataSeries();
                        series.Name = args[start + 1];
                        SeriesChartType charttype = default(SeriesChartType); 
                        if (Enum.TryParse<SeriesChartType>(args[start + 2], out charttype))
                            series.ChartType = charttype;
                        else
                        {
                            Console.WriteLine("Unknown Chart type: {0}, expected are {1}", args[start + 2], chartTypeList());
                            return 1;
                        }

                        DataPoint.DataType xtype = default(DataPoint.DataType);
                        if (!Enum.TryParse<DataPoint.DataType>(args[start + 3], out xtype))
                        {
                            Console.WriteLine("Unknown x-type: {0}", args[start + 3]);
                            return 1;
                        }
                        DataPoint.DataType ytype = default(DataPoint.DataType);
                        if (!Enum.TryParse<DataPoint.DataType>(args[start + 5], out ytype))
                        {
                            Console.WriteLine("Unknown y-type: {0}", args[start + 5]);
                            return 1;
                        }
                        int xindex = 0;
                        if (!int.TryParse(args[start + 4], out xindex))
                        {
                            Console.WriteLine("Unknown index-x: {0}", args[start + 4]);
                            return 1;
                        }
                        int yindex= 0;
                        if (!int.TryParse(args[start + 6], out yindex))
                        {
                            Console.WriteLine("Unknown index-y: {0}", args[start + 6]);
                            return 1;
                        }
                        int zindex = 0;
                        if (!int.TryParse(args[start + 7], out zindex))
                        {
                            Console.WriteLine("Unknown index-z: {0}", args[start + 7]);
                            return 1;
                        }
                        TextSegmentator tmp;
                        if (segmentator == null)
                        {
                            tmp = new TextSegmentator()
                            {
                                SourceX = xindex,
                                SourceY = yindex,
                                SourceZ = zindex
                            };
                            segmentator = tmp;
                        }
                        else {
                            tmp = segmentator.NewInstance();
                            tmp.SourceX = xindex;
                            tmp.SourceY = yindex;
                            tmp.SourceZ = zindex;
                        }
                        segmentorList.Add(tmp);

                        DataPoint point = new DataPoint() { XType = xtype, YType = ytype, ZType = zindex < 0 ? DataPoint.DataType.none : DataPoint.DataType.tostring };
                        converterList.Add(point);

                        seriesList.Add(series);

                        tmp.SourceCompare = -1;
                        if (args[start].ToLower() == "subset")
                        {
                            if(start + 9>=args.Length)
                            {
                                Console.WriteLine("not enough parameters for subset");
                                return 1;
                            }
                            int cindex = 0;
                            if (!int.TryParse(args[start + 8], out cindex))
                            {
                                Console.WriteLine("Unknown compare-index: {0}", args[start + 8]);
                                return 1;
                            }
                            tmp.SourceCompare = cindex;
                            point.CompareValue = args[start + 9];

                            start += 2;
                        }
                        start += 8;
                    }
                    else
                    {
                        Console.WriteLine("Usage: [filename:required] [header records=true/false/comma list:optional] [size X y: optional] [series name index-x index-y] [subset name index-x index-y index-compare value]");
                        Console.WriteLine("Expected series or subset");
                        return 1;
                    }
                }
            }


            System.Diagnostics.Debug.Assert(segmentorList.Count==converterList.Count);
            System.Diagnostics.Debug.Assert(seriesList.Count==converterList.Count);

            
            int length = segmentorList.Count;
            foreach (var record in readDataFromConsole())
            {
                segmentorList[0].Raw = record; //all the other get it's data from the first
                if (!pickupHeader)
                    for (int index = 0; index < length; index++)
                    {
                        converterList[index].IsValid = true;
                        converterList[index].X = segmentorList[index].ValueX();
                        converterList[index].Y = segmentorList[index].ValueY();
                        converterList[index].Z = segmentorList[index].ValueZ();
                        if (segmentorList[index].SourceCompare >= 0)
                            converterList[index].Compare(segmentorList[index].CompareValue());

                        //if (converterList[index].IsValid)
                        bool isInserted = seriesList[index].Add(converterList[index]);

                    }
                else
                {
                    for (int index = 0; index < length; index++)
                    {
                        seriesList[index].LabelX = segmentorList[index].ValueX();
                        seriesList[index].LabelY = segmentorList[index].ValueY();
                        //seriesList[index].LabelZ = segmentorList[index].ValueZ();
                    }
                    pickupHeader = false;
                }
            }


            bool result = createChartFile(filename, size, seriesList, converterList, chartboundaries);
            if (result)
                Console.WriteLine(filename);
            else
                Console.WriteLine("!Error!");

            return result ? 0 : 2;
        }
        

        static IEnumerable<string> readDataFromConsole()
        {
            using (TextReader reader = System.Console.In)
            {
                while (reader.Peek() >= 0)
                {
                    yield return reader.ReadLine();
                }
            }
        }

        public struct ChartLine
        {
            public double x1;
            public double y1;
            public double x2;
            public double y2;
            public string label2;
        }

        // input series definition / datatype / 
        // input SQL with connection string

        static string chartTypeList()
        {
            string[] list = Enum.GetNames(typeof(SeriesChartType));
            string output = string.Join(", ", list);
            return output;
        }

        static private bool createChartFile(string filename, Size size, List<DataSeries> seriesList, List<DataPoint> setting, List<ChartLine> chartboundaries)
        {
            /*
            // set up some data
            var xvals = new[]
                {
                    new DateTime(2012, 4, 4), 
                    new DateTime(2012, 4, 5), 
                    new DateTime(2012, 4, 6), 
                    new DateTime(2012, 4, 7)
                };
            var yvals = new[] { 1, 3, 7, 12 };
            */

            // create the chart
            var chart = new Chart();
            chart.Size = size;  // new Size(600, 250);

            var chartArea = new ChartArea();
            //chartArea.AxisX.LabelStyle.Format = "dd/MMM\nhh:mm"; //default format for datatype
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.LabelStyle.Font = new Font("Consolas", 8);
            chartArea.AxisY.LabelStyle.Font = new Font("Consolas", 8);
            chart.ChartAreas.Add(chartArea);

            const string INDEX_LEGEND = "Legend";
            if (seriesList.Count >= 1)
            {
                // Create a new legend called "Legend2".
                chart.Legends.Add(new Legend(INDEX_LEGEND));

                // Set Docking of the Legend chart to the Default Chart Area.
                chart.Legends[INDEX_LEGEND].DockedToChartArea = string.Empty;

                // Assign the legend to Series1.
                //chart.Series["Series1"].Legend = "Legend2";
                //chart.Series["Series1"].IsVisibleInLegend = true;
            }

            int index=0;
            foreach (var dataseries in seriesList)
            {
                var series = new Series();
                series.Name = dataseries.Name;
                series.ChartType = dataseries.ChartType;
                series.Legend = INDEX_LEGEND;
                series.IsVisibleInLegend = true;

                series.XValueType = mapFromAppType(setting[index].XType, dataseries.isXFloatDetected);
                series.YValueType = mapFromAppType(setting[index].YType, dataseries.isYFloatDetected);

                chart.Series.Add(series);

                // bind the datapoints
                chart.Series[dataseries.Name].Points.DataBindXY(dataseries.X, dataseries.Y);
                int labelcount = dataseries.Z.Count;
                if (labelcount == dataseries.X.Count)
                    for (int i = 0; i < labelcount; i++)
                        series.Points[i].Label = (string)dataseries.Z[i];
                else
                    series.Label = string.Empty;

                if (dataseries.ChartType == SeriesChartType.Point && setting[index].XType == DataPoint.DataType.tonumber && setting[index].YType == DataPoint.DataType.tonumber)
                {
                    LinearRegressionFormula formula = CalculateRegressionToLine(dataseries);

                    var trend = new Series();
                    string trendname = dataseries.Name + "_Trendline";
                    trend.Name = trendname;
                    trend.ChartType = SeriesChartType.Line;
                    //trend.IsVisibleInLegend = false;
                    chart.Series.Add(trend);
                    double[] xvals = new double[] { formula.min_x, formula.max_x };
                    double[] yvals = new double[] { formula.min_y, formula.max_y };
                    trend.Points.DataBindXY(xvals, yvals);

                    trend.Points[1].Label = string.Format("y = {0:0.000} + {1:0.000}x", formula.a, formula.b);
                }
                //X-axis title
                if(!string.IsNullOrWhiteSpace(dataseries.LabelX))
                    if (chart.ChartAreas[0].AxisX.Title != dataseries.LabelX)
                        if (string.IsNullOrWhiteSpace(chart.ChartAreas[0].AxisX.Title))
                            chart.ChartAreas[0].AxisX.Title = dataseries.LabelX;
                        else
                            chart.ChartAreas[0].AxisX.Title += dataseries.LabelX;
                //Y-axis title
                if (!string.IsNullOrWhiteSpace(dataseries.LabelY))
                    if (chart.ChartAreas[0].AxisY.Title != dataseries.LabelY)
                        if (string.IsNullOrWhiteSpace(chart.ChartAreas[0].AxisY.Title))
                            chart.ChartAreas[0].AxisY.Title = dataseries.LabelY;
                        else
                            chart.ChartAreas[0].AxisY.Title += dataseries.LabelY;

                //if data is strings, show all the axis points
                if(setting[index].XType == DataPoint.DataType.tostring ) {
                    chart.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
                }
                if (setting[index].YType == DataPoint.DataType.tostring)
                {
                    chart.ChartAreas[0].AxisY.LabelStyle.Interval = 1;
                }

                index++;
            }


            // copy the series and manipulate the copy
            /*
            chart.DataManipulator.CopySeriesValues("Series1", "Series2");
            chart.DataManipulator.FinancialFormula(
                FinancialFormula.WeightedMovingAverage,
                "Series2"
            );
            chart.Series["Series2"].ChartType = SeriesChartType.Column;
            */
            if(chartboundaries.Count>0)
            {
                var line = new Series();
                string trendname = "ArbitraryLine";
                line.Name = trendname;
                line.ChartType = SeriesChartType.Line;
                line.BorderWidth = 4;
                line.BorderDashStyle = ChartDashStyle.Dot;
                line.IsVisibleInLegend = false;
                chart.Series.Add(line);
                List<double> xvals = new List<double>();
                List<double> yvals = new List<double>();
                foreach (var lineseries in chartboundaries)
                {
                    xvals.Add(lineseries.x1);
                    yvals.Add(lineseries.y1);
                    xvals.Add(lineseries.x2);
                    yvals.Add(lineseries.y2);
                }
                line.Points.DataBindXY(xvals, yvals);
            }

            
            // draw!
            chart.Invalidate();

            // write out a file
            chart.SaveImage(filename, ChartImageFormat.Png);
            chart.Dispose();
            return true;
        }


        static public void CreateChart(Stream output, string title, Size size, List<DataSeries> seriesList, List<DataPoint> setting, List<ChartLine> chartboundaries)
        {
            /*
            // set up some data
            var xvals = new[]
                {
                    new DateTime(2012, 4, 4), 
                    new DateTime(2012, 4, 5), 
                    new DateTime(2012, 4, 6), 
                    new DateTime(2012, 4, 7)
                };
            var yvals = new[] { 1, 3, 7, 12 };
            */

            // create the chart
            using (var chart = new Chart())
            {
                chart.Size = size;  // new Size(600, 250);

                if(title!=null)
                    chart.Titles.Add(title);

                var chartArea = new ChartArea();
                //chartArea.AxisX.LabelStyle.Format = "dd/MMM\nhh:mm"; //default format for datatype
                chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
                chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
                chartArea.AxisX.LabelStyle.Font = new Font("Consolas", 8);
                chartArea.AxisY.LabelStyle.Font = new Font("Consolas", 8);
                chart.ChartAreas.Add(chartArea);

                const string INDEX_LEGEND = "Legend";
                if (seriesList.Count >= 1)
                {
                    // Create a new legend called "Legend2".
                    chart.Legends.Add(new Legend(INDEX_LEGEND));

                    // Set Docking of the Legend chart to the Default Chart Area.
                    chart.Legends[INDEX_LEGEND].DockedToChartArea = string.Empty;

                    // Assign the legend to Series1.
                    //chart.Series["Series1"].Legend = "Legend2";
                    //chart.Series["Series1"].IsVisibleInLegend = true;
                }

                int index = 0;
                foreach (var dataseries in seriesList)
                {
                    var series = new Series();
                    series.IsVisibleInLegend = dataseries.Name != null;
                    if (dataseries.Name==null) dataseries.Name = Guid.NewGuid().ToString();
                    series.Name = dataseries.Name;
                    series.ChartType = dataseries.ChartType;
                    series.Legend = INDEX_LEGEND;
                    

                    series.XValueType = mapFromAppType(setting[index].XType, dataseries.isXFloatDetected);
                    series.YValueType = mapFromAppType(setting[index].YType, dataseries.isYFloatDetected);

                    chart.Series.Add(series);

                    // bind the datapoints
                    chart.Series[dataseries.Name].Points.DataBindXY(dataseries.X, dataseries.Y);
                    int labelcount = dataseries.Z.Count;
                    if (labelcount == dataseries.X.Count)
                        for (int i = 0; i < labelcount; i++)
                            series.Points[i].Label = (string)dataseries.Z[i];
                    else
                        series.Label = string.Empty;

                    if (dataseries.ChartType == SeriesChartType.Point && setting[index].XType == DataPoint.DataType.tonumber && setting[index].YType == DataPoint.DataType.tonumber && dataseries.IsTrendlineEnabled)
                    {
                        LinearRegressionFormula formula = CalculateRegressionToLine(dataseries);

                        var trend = new Series();
                        string trendname = dataseries.Name + "_Trendline";
                        trend.Name = trendname;
                        trend.ChartType = SeriesChartType.Line;
                        //trend.IsVisibleInLegend = false;
                        chart.Series.Add(trend);
                        double[] xvals = new double[] { formula.min_x, formula.max_x };
                        double[] yvals = new double[] { formula.min_y, formula.max_y };
                        trend.Points.DataBindXY(xvals, yvals);

                        trend.Points[1].Label = string.Format("y = {0:0.000} + {1:0.000}x", formula.a, formula.b);
                    }
                    //X-axis title
                    if (!string.IsNullOrWhiteSpace(dataseries.LabelX))
                        if (chart.ChartAreas[0].AxisX.Title != dataseries.LabelX)
                            if (string.IsNullOrWhiteSpace(chart.ChartAreas[0].AxisX.Title))
                                chart.ChartAreas[0].AxisX.Title = dataseries.LabelX;
                            else
                                chart.ChartAreas[0].AxisX.Title += dataseries.LabelX;
                    //Y-axis title
                    if (!string.IsNullOrWhiteSpace(dataseries.LabelY))
                        if (chart.ChartAreas[0].AxisY.Title != dataseries.LabelY)
                            if (string.IsNullOrWhiteSpace(chart.ChartAreas[0].AxisY.Title))
                                chart.ChartAreas[0].AxisY.Title = dataseries.LabelY;
                            else
                                chart.ChartAreas[0].AxisY.Title += dataseries.LabelY;

                    //if data is strings, show all the axis points
                    if (setting[index].XType == DataPoint.DataType.tostring)
                    {
                        chart.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
                    }
                    if (setting[index].YType == DataPoint.DataType.tostring)
                    {
                        chart.ChartAreas[0].AxisY.LabelStyle.Interval = 1;
                    }

                    index++;
                }


                // copy the series and manipulate the copy
                /*
                chart.DataManipulator.CopySeriesValues("Series1", "Series2");
                chart.DataManipulator.FinancialFormula(
                    FinancialFormula.WeightedMovingAverage,
                    "Series2"
                );
                chart.Series["Series2"].ChartType = SeriesChartType.Column;
                */
                if (chartboundaries.Count > 0)
                {
                    var line = new Series();
                    string trendname = "ArbitraryLine";
                    line.Name = trendname;
                    line.ChartType = SeriesChartType.Line;
                    line.BorderWidth = 4;
                    line.BorderDashStyle = ChartDashStyle.Dot;
                    line.IsVisibleInLegend = false;
                    chart.Series.Add(line);
                    List<double> xvals = new List<double>();
                    List<double> yvals = new List<double>();
                    var linelabels = new List<string>();
                    foreach (var lineseries in chartboundaries)
                    {
                        xvals.Add(lineseries.x1);
                        yvals.Add(lineseries.y1);
                        xvals.Add(lineseries.x2);
                        yvals.Add(lineseries.y2);
                        linelabels.Add(null);
                        linelabels.Add(lineseries.label2);
                    }
                    line.Points.DataBindXY(xvals, yvals);
                    int counter=0;
                    foreach (var lbl in linelabels)
                        line.Points[counter++].Label = lbl;
                }


                // draw!
                chart.Invalidate();

                // write out a file
                chart.SaveImage(output, ChartImageFormat.Png);
                chart.Dispose();
            }
        }

        static ChartValueType mapFromAppType(DataPoint.DataType apptype, bool isFloat)
        {
            if (apptype == DataPoint.DataType.todate)
                return ChartValueType.DateTime;
            else if (apptype == DataPoint.DataType.tonumber && isFloat)
                return ChartValueType.Double;
            else if (apptype == DataPoint.DataType.tonumber)
                return ChartValueType.Int32;
            else if (apptype == DataPoint.DataType.tostring)
                return ChartValueType.String;

            throw new NotSupportedException("Undefined mapping for " + apptype.ToString());
        }

        struct LinearRegressionFormula
        {
            public double a;
            public double b;
            public double min_x;
            public double min_y;
            public double max_x;
            public double max_y;
            public double Y(double x)
            {
                //y’ = a + bx
                return a + (b * x);
            }
        }

        //http://www.statisticshowto.com/how-to-find-a-linear-regression-equation/
        static LinearRegressionFormula CalculateRegressionToLine(DataSeries dataseries )
        {
            double min_x=double.MaxValue, min_y=0, max_x=double.MinValue, max_y=0;
            double Ey=0, Ex2=0, Ex=0, Exy=0, n=0;
            double x,y, xy, x2;

            int count = dataseries.X.Count;
            for (int i = 0; i < count; i++)
            {
                x = (double)dataseries.X[i];
                y = (double)dataseries.Y[i];
                xy = x * y;
                x2 = x * x;
                Ey += y;
                Ex2 += x2;
                Ex += x;
                Exy += xy;
                n+=1;
                if (x > max_x) max_x = x;
                if (x < min_x) min_x = x;
            }
            LinearRegressionFormula result = new LinearRegressionFormula();
            result.a = ((Ey*Ex2)-(Ex*Exy))/((n*Ex2)-(Ex*Ex));
            result.b = ((n*Exy)-(Ex*Ey))/((n*Ex2)-(Ex*Ex));
            result.min_x = min_x;
            result.min_y = result.Y(min_x);
            result.max_x = max_x;
            result.max_y = result.Y(max_x);
            return result;
        }
    }


    public class TextSegmentator //if you ever implement database as source, create interface with ValueX, ValueY,ValueZ as the common fields.
    {
        public TextSegmentator()
        {
            this._master = this;
            this.Delimiter = '\t';
        }
        public TextSegmentator(TextSegmentator master) : base()
        {
            this._master = master;
        }
        TextSegmentator _master;

        public char Delimiter { get; set; }
        string _raw;
        public string Raw
        {
            get
            {
                return this._raw;
            }
            set
            {
                this._raw = value;
                this.Record = this._raw.Split(this.Delimiter);
            }
        }
        public string[] Record { get; set; }
        public int SourceX { get; set; }
        public int SourceY { get; set; }
        public int SourceZ { get; set; }
        public int SourceCompare { get; set; }
        public string ValueX()
        {
            return this._master.Record[this.SourceX];
        }
        public string ValueY()
        {
            return this._master.Record[this.SourceY];
        }
        public string ValueZ()
        {
            if (this.SourceZ < 0) return null;

            return this._master.Record[this.SourceZ];
        }
        public string CompareValue()
        {
            if (this.SourceCompare < 0) return null;

            return this._master.Record[this.SourceCompare];
        }

        public TextSegmentator NewInstance()
        {
            TextSegmentator copy = new TextSegmentator(this)
            {
                SourceX = this.SourceX,
                SourceY = this.SourceY,
                SourceZ = this.SourceZ
            };
            return copy;
        }
    }

    public class DataPoint
    {
        public bool IsValid {get;set;}
        public string X { get; set; }
        public string Y {get;set;}
        public string Z { get; set; }
        public enum DataType { none, tostring, tonumber, todate }
        public DataType XType { get; set; }
        public DataType YType { get; set; }
        public DataType ZType { get; set; }
        public object ConvertX()
        {
            return this.convert(this.X, this.XType);
        }
        public object ConvertY()
        {
            return this.convert(this.Y, this.YType);
        }
        public object ConvertZ()
        {
            return this.convert(this.Z, this.ZType);
        }
        
        public string CompareValue { get; set; }
        public bool Compare(string value)
        {
            if(this.CompareValue!=null)
                if (this.CompareValue == value)
                {
                    return true;
                }
                else
                {
                    this.IsValid = false;
                    return false;
                }
            else return true;
        }

        object convert(string value, DataType datatype)
        {
            double dbltmp;
            DateTime datetmp;
            //there's no way to know if the data has decimal points until it's been fully parsed, otherwise specific numeric type has to be indicated in command line (we can autodetect instead)
            switch (datatype)
            {
                case DataType.tostring:
                    return value;
                    break;
                case DataType.tonumber:
                    if (double.TryParse(value, out dbltmp))
                        return dbltmp;
                    else
                    {
                        this.IsValid = false;
                        return null;
                    }
                    break;
                case DataType.todate:
                    if (DateTime.TryParse(value, out datetmp))
                        return datetmp;
                    else
                    {
                        this.IsValid = false;
                        return null;
                    }
                    break;
            }
            return null;
        }
    }
    
    public class DataSeries
    {
        public DataSeries() { }
        public DataSeries(IEnumerable<object> x, IEnumerable<object> y) { this.X.AddRange(x); this.Y.AddRange(y); }
        public DataSeries(IEnumerable<object> x, IEnumerable<object> y, IEnumerable<object> z) : this(x, y) { this.Z.AddRange(z); }

        public string Name { get; set; }
        public string LabelX { get; set; }
        public string LabelY { get; set; }
        public readonly List<Object> X = new List<Object>();
        public readonly List<Object> Y = new List<Object>();
        public readonly List<Object> Z = new List<Object>();
        public bool isXFloatDetected { get; private set; }
        public bool isYFloatDetected { get; private set; }
        public bool isZFloatDetected { get; private set; }

        public bool Add(DataPoint tuple)
        {
            object x = tuple.ConvertX();
            object y = tuple.ConvertY();
            if (!tuple.IsValid) return false;
            X.Add(x);
            Y.Add(y);
            if(tuple.ZType!= DataPoint.DataType.none)
                Z.Add(tuple.ConvertZ());
            return true;
        }

        public SeriesChartType ChartType { get; set; }
        public bool IsTrendlineEnabled = true;
    }

}
