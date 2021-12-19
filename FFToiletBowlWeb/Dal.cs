using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using FFToiletBowl;


namespace FFToiletBowlWeb
{
    /// <summary>
    /// Consider this dependency injection from a different datasource, into front-end expected datatypes
    /// ...except there are no other implmentations of 
    /// </summary>
    static public class Dal
    {
        static public IDataFactory DataFactory = new DBFactory();
        static public IRowAdaptor Convertor = new DBAdapter();
    }

    /// <summary>
    /// This is so the data factory can return several different types of data, but this gives option to 
    /// convert them to the UI types
    /// </summary>
    public interface IRowAdaptor
    {
        void Copy(object from, WeekReport to);
        void Copy(object from, SeasonReport to);
        void Copy(object from, DefensiveRatingReport to);
        void Copy(object from, GameReport to);
        void Copy(object from, InjuryData to);
        
    }
    public class DBAdapter : IRowAdaptor
    {
        public void Copy(object from, WeekReport to)
        {
            var row = (DataRow)from;
            to.Week = (int)row["Wk"];
            to.YearWeek = string.Format("{0}-{1}", row["Year"], to.Week);

            to.IsDefenseEvaluationReady = true;
            to.IsEstimatesReady = true;
            to.IsVolatilityAlertReady = true;
            to.IsLeadersReady = true;
            to.IsEstimateResultsReady = true;


        }
        public void Copy(object from, SeasonReport to)
        {
            var row = (DataRow)from;
            to.Year = (int)row["Year"];
        }
        public void Copy(object from, DefensiveRatingReport to)
        {
            var row = (DataRow)from;
            to.Team = (string)row["Team"];
            to.OpposingPos = (string)row["OpposingPos"];

            to.YdDiff = (int)row["YdDiff"];
            to.YdPct = (decimal)row["YdPct"];
            to.YdPtDiff = (decimal)row["YdPtDiff"];
            to.YdPtPctDiff = (decimal)row["YdPtPctDiff"];

            to.TdDiff = (int)row["TdDiff"];
            to.TdPct = (decimal)row["TdPct"];
            to.TdPtDiff = (decimal)row["TdPtDiff"];
            to.TdPtPctDiff = (decimal)row["TdPtPctDiff"];
        }
        public void Copy(object from, GameReport to)
        {
            var row = (DataRow)from;
            to.Team = (string)row["Team"];
            to.Versus = (string)row["Versus"];
        }


        public void Copy(object from, InjuryData to)
        {
            var row = (DataRow)from;
            to.Year = (int)row["Year"];
            to.Gm = (int)row["Gm"];
            to.PlayerID = (string)row["PlayerID"];
            to.Player = (string)row["Player"];
            to.Pos = (string)row["Pos"];
            to.Team = (string)row["Team"];
            to.Plays = (int)row["Plays"];
            to.GmsOut = (int)row["GmsOut"];
        }
    }

    /// <summary>
    /// This is admittedly designed from a database point of view.
    /// But similar interface also fits list of json, which should have option to turn into a Dictionary but doesn't in .NET
    /// and there is no reason, why it cant server list of POCO, either
    /// </summary>
    public interface IDataFactory
    {
        IEnumerable<object> GetDataForWeb(string name, int year, int week, string game, string player);
        IEnumerable<object> GetDataFromDB(string name);
    }
    public class DBFactory : IDataFactory
    {
        public IEnumerable<object> GetDataForWeb(string name, int year, int week, string game, string player) 
        {
            return Reports.GetDataForWeb(name, year, week, game, player);
        }
        public IEnumerable<object> GetDataFromDB(string name)
        {
            using (var dt = Reports.GetDataFromDB(name))
                foreach (var row in dt.Rows)
                    yield return (DataRow)row;
        }
    }
    


    public class ScheduleSeasonReport
    {
        static object __surrogatelock = new object();

        static List<SeasonReport> __cache = null;
        public IEnumerable<SeasonReport> All
        {
            get
            {
                List<SeasonReport> cache = null;
                if (__cache != null) //data cached
                    cache = new List<SeasonReport>(__cache);
                else
                    cache = new List<SeasonReport>();

                foreach (var item in Dal.DataFactory.GetDataForWeb("AllSeasons", 0, 0, "", ""))
                {
                    var converted = new SeasonReport(this, item);
                    cache.Add(converted);
                    yield return converted;
                }

                if (__cache != null)
                    __cache = new List<SeasonReport>(cache);
            }
        }
        public void Refresh()
        {
            __cache = null;
        }

        public InjuryModel AllInjury
        {
            get
            {
                var convert = Dal.Convertor;
                InjuryModel model = new InjuryModel();
                foreach (var item in Dal.DataFactory.GetDataFromDB("vwInjuryModel"))
                {
                    var pt = new InjuryData();
                    convert.Copy(item, pt);
                    model.Add(pt);
                    var pmodel = model.PlayerModel[pt.PlayerID];
                    pt.Plays = (int)pmodel.Ex;
                    pt.GmsOut = (int)pmodel.Ey;
                }
                return model;
            }
        }
        
    }
    public class SeasonReport
    {
        public SeasonReport(ScheduleSeasonReport parent, object row)
        {
            this.Parent = parent;
            Dal.Convertor.Copy(row, this);
            //this.Year = (int)row["Year"];
        }
        public readonly ScheduleSeasonReport Parent;
        public int Year { get; internal set; }
        public bool IsDraftSimulationReady { get; internal set; }

        List<WeekReport> _cache = null;
        public IEnumerable<WeekReport> AllWeeks
        {
            get
            {
                List<WeekReport> cache = null;
                if (_cache != null)
                    cache = _cache;
                else
                    cache = new List<WeekReport>();

                foreach (var item in Reports.GetDataForWeb("AllWeeks", this.Year, 0, "", ""))
                {
                    var converted = new WeekReport(this, item);
                    cache.Add(converted);
                    yield return converted;
                }

                if (_cache != null)
                    _cache = cache;
            }
        }
    }
    public class WeekReport
    {
        public WeekReport(SeasonReport parent, object row)
        {
            this.Parent = parent;
            Dal.Convertor.Copy(row, this);

        }

        public readonly SeasonReport Parent;
        public int Week { get; internal set; }
        public string YearWeek { get; internal set; }
        public bool IsDefenseEvaluationReady { get; internal set; }
        public bool IsEstimatesReady { get; internal set; }
        public bool IsVolatilityAlertReady { get; internal set; }
        public bool IsLeadersReady { get; internal set; }
        public bool IsEstimateResultsReady { get; internal set; }

        List<GameReport> _cache = null;
        public IEnumerable<GameReport> AllGames
        {
            get
            {
                List<GameReport> cache = null;
                if (_cache != null)
                    cache = _cache;
                else
                    cache = new List<GameReport>();

                foreach (var item in Reports.GetDataForWeb("AllGames", this.Parent.Year, this.Week, "", ""))
                {
                    var converted = new GameReport(this, item);
                    cache.Add(converted);
                    yield return converted;
                }

                if (_cache != null)
                    _cache = cache;
            }
        }

        List<DefensiveRatingReport> _cacheDefRate = null;
        public IEnumerable<DefensiveRatingReport> DefensiveRatings
        {
            get
            {
                List<DefensiveRatingReport> cache = null;
                if (_cacheDefRate != null)
                    cache = _cacheDefRate;
                else
                    cache = new List<DefensiveRatingReport>();

                foreach (var item in Reports.GetData("vwStatsGmToPosDeltaPerTeam"))
                {
                    var converted = new DefensiveRatingReport(this, item);
                    cache.Add(converted);
                    yield return converted;
                }

                if (_cacheDefRate != null)
                    _cacheDefRate = cache;
            }
        }
    }
    public class DefensiveRatingReport
    {
        static int DEFAULT_PCT_MULTIPLIER = 100;

        public DefensiveRatingReport(WeekReport parent, object row, int pctmultiplier)
        {
            this.PctMultiplier = pctmultiplier;
            this.Parent = parent;
            Dal.Convertor.Copy(row, this);

        }
        public DefensiveRatingReport(WeekReport parent, object row)
            : this(parent, row, DEFAULT_PCT_MULTIPLIER) { }

        public readonly WeekReport Parent;

        public int PctMultiplier { get; internal set; }

        public string Team { get; internal set; }
        public string OpposingPos { get; internal set; }

        public int YdDiff { get; internal set; }
        public decimal YdPct { get; internal set; }
        public decimal YdPtDiff { get; internal set; }
        public decimal YdPtPctDiff { get; internal set; }

        public int TdDiff { get; internal set; }
        public decimal TdPct { get; internal set; }
        public decimal TdPtDiff { get; internal set; }
        public decimal TdPtPctDiff { get; internal set; }


        public decimal TotPtPctDiff { get; internal set; }
        public decimal StDevPtPct { get; internal set; }
    }
    public class GameReport
    {
        public GameReport(WeekReport parent, DataRow row)
        {
            this.Parent = parent;
            Dal.Convertor.Copy(row, this);
        }
        public readonly WeekReport Parent;
        public string Team { get; internal set; }
        public string Versus { get; internal set; }
        public bool Away { get; internal set; }
        public DateTime GameDate { get; internal set; }
        public string GameResult { get; internal set; }

        public string Stats { get; internal set; }
        public string PlayByPlay { get; internal set; }
    }

    public class Regression
    {
        public double m
        {
            get { return 0; }
        }
        public double b
        {
            get { return 0; }
        }
        public enum RegType { Linear }
        public RegType Type = RegType.Linear;
        public double Ex;
        public double Ey;
        public int n;
    }
    public class InjuryModel
    {
        public Dictionary<string, Regression> Model = new Dictionary<string, Regression>()
        {
            {"QB",new Regression()},
            {"RB",new Regression()},
            {"WR",new Regression()},
            {"TE",new Regression()},
            {"K",new Regression()},
        };
        public Dictionary<string, Regression> PlayerModel = new Dictionary<string, Regression>();
        public List<InjuryData> Data;
        public void Add(InjuryData data)
        {
            this.Add(data,null,null);
        }
        public void Add(InjuryData data, Func<double, double> runningx, Func<double, double> runningy)
        {
            var catalog = this.Model;
            if (catalog.ContainsKey(data.Pos))
            {
                Regression pmodel = null;
                if (this.PlayerModel.ContainsKey(data.PlayerID))
                    pmodel = this.PlayerModel[data.PlayerID];
                else
                    this.PlayerModel.Add(data.PlayerID, pmodel = new Regression());
                //condition The player data, to have running sum, per player
                pmodel.n++;
                data.Plays = (int)(pmodel.Ex += data.Plays);
                data.GmsOut = (int)(pmodel.Ey += data.GmsOut);

                //the running sum(s), is the x,y data
                var model = catalog[data.Pos];
                model.n++;
                model.Ex += data.Plays;
                model.Ey += data.GmsOut;
                if (runningx != null)
                    runningx(model.Ex);
                if (runningy != null)
                    runningy(model.Ey);
            }
        }
    }
    public class InjuryData
    {
        public int Year;
        public int Gm;
        public string PlayerID;
        public string Player;
        public string Pos;
        public string Team;
        public int PlayerData;
        public int Plays;
        public int GmsOut;
    }
}