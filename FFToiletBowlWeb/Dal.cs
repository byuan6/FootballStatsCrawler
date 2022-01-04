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

        static public IEnumerable<string> ToJsonParts(this IEnumerable<IJsonAble> lst)
        {
            var delimiter = string.Empty;
            yield return "[";
            foreach (var item in lst)
            {
                yield return delimiter;
                foreach (var item2 in item.ToJsonParts())
                    yield return item2;
                if(delimiter==string.Empty)
                    delimiter=",";
            }
            yield return "]";    
        }

        //https://stackoverflow.com/questions/6346119/datetime-get-next-tuesday
        public static DateTime GetNextWeekday(this DateTime start, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }
        public static DateTime Min(this DateTime day1, DateTime day2)
        {
            return day1 < day2 ? day2 : day2;
        }
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
        void Copy(object from, PlayerIndexEntry to);
        void Copy(object from, SeasonEntry to);
        void Copy(object from, ScheduleGridLine to);
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
            to.ScheduleCount = (int)row["TotalGames"];
            to.GamesPlayedCount = (int)row["GamesPlayed"];

            to.StartDate = (DateTime)row["StartDate"];
            to.EndDate = (DateTime)row["EndDate"];
            if (row["LastUpdateDate"]!=DBNull.Value) to.LastResultDate = (DateTime)row["LastUpdateDate"];
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
            to.InjuryGm = (int)row["InjuryGm"];
            to.RunningPlayCount = (int)row["RunningPlays"];
            to.PlayerID = (string)row["HashedPlayerID"];
            to.Player = (string)row["Player"];
            to.Pos = (string)row["Pos"];
            to.RunningOutCount = (int)row["GamesOut"];
            to.RunningGmCount = (int)row["CountGm"]; 
            to.InjuryRate = (double)row["InjuryRate"];

            if (row["Dob"] != DBNull.Value) to.Dob = (DateTime)row["Dob"];
            if (row["ProbableAge"] != DBNull.Value) to.ProbableAge = (int)row["ProbableAge"];
            if (row["DraftYear"] != DBNull.Value) to.DraftYear = (int)row["DraftYear"];
            if (row["DraftStr"] != DBNull.Value) to.DraftStr = (string)row["DraftStr"];
            if (row["Seasons"] != DBNull.Value) to.Seasons = (int)row["Seasons"];

            to.LastGm = (int)row["LastGm"];
        }

        public void Copy(object from, PlayerIndexEntry to)
        {
            var row = (DataRow)from;
            to.ID = (string)row["ID"];
            to.Player = (string)row["Player"];
            to.Pos = (string)row["Pos"];
            to.Team = (string)row["Team"];
            to.Year = (int)row["Year"];
            to.Games = (int)row["Games"];

        }
        public void Copy(object from, SeasonEntry to)
        {
            var row = (DataRow)from;
            to.Year = (int)row["Year"];
            to.Teams = (int)row["Teams"];
            to.Weeks = (int)row["Weeks"];
            to.FirstGmDate = (DateTime)row["FIRST_GM"];
            to.LastGmDate = (DateTime)row["LAST_GM"]; 
        }
        public void Copy(object from, ScheduleGridLine to)
        {
            var row = (DataRow)from;
            to.Team = (string)row["Team"];
            if (row["1"]!=DBNull.Value) to.Wk1 = (string)row["1"];
            if (row["2"] != DBNull.Value) to.Wk2 = (string)row["2"];
            if (row["3"] != DBNull.Value) to.Wk3 = (string)row["3"];
            if (row["4"] != DBNull.Value) to.Wk4 = (string)row["4"];
            if (row["5"] != DBNull.Value) to.Wk5 = (string)row["5"];
            if (row["6"] != DBNull.Value) to.Wk6 = (string)row["6"];
            if (row["7"] != DBNull.Value) to.Wk7 = (string)row["7"];
            if (row["8"] != DBNull.Value) to.Wk8 = (string)row["8"];
            if (row["9"] != DBNull.Value) to.Wk9 = (string)row["9"];
            if (row["10"] != DBNull.Value) to.Wk10 = (string)row["10"];
            if (row["11"] != DBNull.Value) to.Wk11 = (string)row["11"];
            if (row["12"] != DBNull.Value) to.Wk12 = (string)row["12"];
            if (row["13"] != DBNull.Value) to.Wk13 = (string)row["13"];
            if (row["14"] != DBNull.Value) to.Wk14 = (string)row["14"];
            if (row["15"] != DBNull.Value) to.Wk15 = (string)row["15"];
            if (row["16"] != DBNull.Value) to.Wk16 = (string)row["16"];
            if (row["17"] != DBNull.Value) to.Wk17 = (string)row["17"];
            if (row["18"] != DBNull.Value) to.Wk18 = (string)row["18"];
        }
        
    }

    /// <summary>
    /// This interface is admittedly designed from a database point of view, and is not object oriented nor type-checked or attribute checked.
    /// The interface returns a object types, so in this context, it contains references to DataRow types.
    /// 
    /// But combined with IRowAdaptor interface, it can be implemented to return POCO's, whose values are obtained from deserializing files.
    /// The purpose is to give flexibility in decoupling the data source, in exchange for slight performance losses in copying data unnecessarily in intermediate layers.
    /// The performance loss should be more than compensated, with the gratuitous use of caching, and lowering latency by only pulling data as it needs it.
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

    public interface IJsonAble
    {
        IEnumerable<string> ToJsonParts();
    }
    public class JsonEnumerable<T> : IJsonAble, IEnumerable<T> where T : IJsonAble
    {
        public JsonEnumerable(IEnumerable<T> lst) 
        {
            _data = lst;
        }
        IEnumerable<T> _data = null;

        public IEnumerator<T> GetEnumerator()
        {
            return new JsonEnumerator<T>(_data.GetEnumerator());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            return Dal.ToJsonParts((IEnumerable<FFToiletBowlWeb.IJsonAble>)this);
        }
        #endregion IJsonAble
    }
    public class JsonEnumerator<T> : IEnumerator<T> where T : IJsonAble
    {
        public JsonEnumerator(IEnumerator<T> lst) 
        {
            _head = lst;
        }
        IEnumerator<T> _head = null;

        public T Current
        {
            get { return _head.Current; }
        }

        public void Dispose()
        {
            _head.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return _head.Current; }
        }

        public bool MoveNext()
        {
            return _head.MoveNext();
        }

        public void Reset()
        {
            _head.Reset();
        }
    }

    /// <summary>
    /// I hate "closures" bc of it's scope ambiguousness, 
    /// but easiest way to handle passing in a datafactory of any kind of query, 
    /// is to inline a lamba expression in constructor, 
    /// to return the data being cached
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Cache<T>
    {
        public Cache() 
        {
            this.ExpiresFrom = Cache<T>.NextTueOrWed;
        }
        public Cache(ExpireForumula expiresfrom)
        {
            this.ExpiresFrom = expiresfrom;
        }
        public Cache(Func<IEnumerable<T>> factory) : this(factory, Cache<T>.NextTueOrWed) { }
        public Cache(Func<IEnumerable<T>> factory, ExpireForumula expiresfrom)
        {
            this.DataFactory = factory;
            this.ExpiresFrom = expiresfrom;
        }

        List<T> _cache = null;
        DateTime _cacheexpire = DateTime.Now;
        public Func<IEnumerable<T>> DataFactory;
        public IEnumerable<T> Retreive()
        {
            var iscacheempty = _cache == null;
            var iscacheexpired = DateTime.Now > _cacheexpire;
            var isrefreshcache = iscacheempty || iscacheexpired;
            if (isrefreshcache)
            {
                var cache = new List<T>();
                foreach (var item in this.DataFactory())
                {
                    cache.Add(item);
                    yield return item;
                }
                _cache = cache;
                var now = DateTime.Now;
                _cacheexpire = this.ExpiresFrom(now);
            }
            else
                foreach (var item in _cache)
                    yield return item;
        }
        public delegate DateTime ExpireForumula(DateTime now);
        public ExpireForumula ExpiresFrom;

        static public DateTime NextTueOrWed(DateTime now)
        {
            return now.GetNextWeekday(DayOfWeek.Tuesday).Min(now.GetNextWeekday(DayOfWeek.Wednesday));
        }
        static public DateTime In24Hours(DateTime now)
        {
            return now.AddDays(1);
        }
        static public DateTime AtMidnight(DateTime now)
        {
            return now.AddDays(1).Date;
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
        public SeasonReport GetSeason(int yr)
        {
            return this.All.SingleOrDefault(s => s.Year == yr);
        }
        public void Refresh()
        {
            __cache = null;
        }

        Cache<InjuryData> _injurycache = null;
        public IEnumerable<InjuryData> InjuryModel
        {
            get
            {
                if (_injurycache == null)
                    _injurycache = new Cache<InjuryData>(() => Dal.DataFactory.GetDataFromDB("vwInjuryModel").Select(s => new InjuryData(this, s)));
                return _injurycache.Retreive();
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
        public int ScheduleCount;
        public int GamesPlayedCount;
        public bool IsScheduleLoaded { get { return this.ExpectedNumberOfGames() == this.ScheduleCount; } }
        public DateTime? LastResultDate;
        public DateTime StartDate { get; internal set; }
        public DateTime EndDate { get; internal set; }
        public bool IsDraftSimulationReady { get; internal set; }

        public int ExpectedNumberOfTeams() { return this.ExpectedNumberOfTeams(this.Year); }
        public int ExpectedNumberOfTeams(int yr)
        {
            var expected = yr <= 2001 ? 31 : yr <= 2021 ? 32 : 32;
            return expected;
        }
        public int ExpectedNumberOfWeeks() { return this.ExpectedNumberOfWeeks(); }
        public int ExpectedNumberOfWeeks(int yr)
        {
            var expected = yr <= 2001 ? 17: yr < 2021 ? 17: 18;
            return expected;
        }
        public int ExpectedNumberOfByes() { return ExpectedNumberOfByes(this.Year); }
        public int ExpectedNumberOfByes(int yr)
        {
            var expected = yr <= 2001 ? 1 : yr <= 2021 ? 1 : 1;
            return expected;
        }
        public int ExpectedNumberOfGames() { return this.ExpectedNumberOfGames(this.Year); }
        public int ExpectedNumberOfGames(int yr)
        {
            var expected = this.ExpectedNumberOfTeams(yr) * (this.ExpectedNumberOfWeeks(yr)-ExpectedNumberOfByes(yr));
            expected /= 2;
            return expected;
        }
        public int ExpectedNumberOfScheduleRecordsIncludingBye(int yr)
        {
            var expected = this.ExpectedNumberOfTeams(yr) * this.ExpectedNumberOfWeeks(yr);
            return expected;
        }

        Cache<WeekReport> _cache = null;
        public IEnumerable<WeekReport> AllWeeks
        {
            get
            {
                if (_cache == null)
                    _cache = new Cache<WeekReport>(() => Dal.DataFactory.GetDataForWeb("AllWeeks", this.Year, 0, "", "").Select(s => new WeekReport(this, s)));
                return _cache.Retreive();
            }
        }


        Cache<ScheduleGridLine> _schedulecache = null;
        public IEnumerable<ScheduleGridLine> ScheduleGrid()
        {
            if (_schedulecache == null)
                _schedulecache = new Cache<ScheduleGridLine>(() => Dal.DataFactory.GetDataForWeb("SeasonScheduleGrid", this.Year, 0, "", "").Select(s => new ScheduleGridLine(this, s)));
            return _schedulecache.Retreive();
        }
    }
    public class ScheduleGridLine : IJsonAble
    {
        public ScheduleGridLine(object parent, object row)
        {
            this.Parent = parent;
            Dal.Convertor.Copy(row, this);
        }
        public readonly object Parent;

        public string Team { get; internal set; }
        public string Wk1 { get; internal set; }
        public string Wk2 { get; internal set; }
        public string Wk3 { get; internal set; }
        public string Wk4 { get; internal set; }
        public string Wk5 { get; internal set; }
        public string Wk6 { get; internal set; }
        public string Wk7 { get; internal set; }
        public string Wk8 { get; internal set; }
        public string Wk9 { get; internal set; }
        public string Wk10 { get; internal set; }
        public string Wk11 { get; internal set; }
        public string Wk12 { get; internal set; }
        public string Wk13 { get; internal set; }
        public string Wk14 { get; internal set; }
        public string Wk15 { get; internal set; }
        public string Wk16 { get; internal set; }
        public string Wk17 { get; internal set; }
        public string Wk18 { get; internal set; }
        //public string Wk19 { get; internal set; }
        //public string Wk20 { get; internal set; }

        public IEnumerable<string> ToJsonParts()
        {
            yield return "{";
            yield return "\"team\":\"";
            yield return this.Team;
            yield return "\",\"wk1\":\"";
            yield return this.Wk1;
            yield return "\",\"wk2\":\"";
            yield return this.Wk2;
            yield return "\",\"wk3\":\"";
            yield return this.Wk3;
            yield return "\",\"wk4\":\"";
            yield return this.Wk4;
            yield return "\",\"wk5\":\"";
            yield return this.Wk5;
            yield return "\",\"wk6\":\"";
            yield return this.Wk6;
            yield return "\",\"wk7\":\"";
            yield return this.Wk7;
            yield return "\",\"wk8\":\"";
            yield return this.Wk8;
            yield return "\",\"wk9\":\"";
            yield return this.Wk9;
            yield return "\",\"wk10\":\"";
            yield return this.Wk10;
            yield return "\",\"wk11\":\"";
            yield return this.Wk11;
            yield return "\",\"wk12\":\"";
            yield return this.Wk12;
            yield return "\",\"wk13\":\"";
            yield return this.Wk13;
            yield return "\",\"wk14\":\"";
            yield return this.Wk14;
            yield return "\",\"wk15\":\"";
            yield return this.Wk15;
            yield return "\",\"wk16\":\"";
            yield return this.Wk16;
            yield return "\",\"wk17\":\"";
            yield return this.Wk17;
            yield return "\",\"wk18\":\"";
            yield return this.Wk18;
            yield return "\"}";
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

        Cache<GameReport> _cache = null;
        public IEnumerable<GameReport> AllGames
        {
            get
            {
                if(_cache == null)
                    _cache = new Cache<GameReport>(() => Dal.DataFactory.GetDataForWeb("AllGames", this.Parent.Year, this.Week, "", "").Select(s => new GameReport(this, s)));
                return _cache.Retreive();
            }
        }

        Cache<DefensiveRatingReport> _cacheDefRate = null;
        public IEnumerable<DefensiveRatingReport> DefensiveRatings
        {
            get
            {
                if (_cacheDefRate == null)
                    _cacheDefRate = new Cache<DefensiveRatingReport>(() => Dal.DataFactory.GetDataFromDB("vwStatsGmToPosDeltaPerTeam").Select(s => new DefensiveRatingReport(this, s)));
                return _cacheDefRate.Retreive();
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
        public GameReport(WeekReport parent, object row)
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

    /// <summary>
    /// list for Dropdowns for Seasons with data.  No drilldown, capability
    /// </summary>
    public class SeasonList : List<SeasonEntry>
    {
        Cache<SeasonEntry> _cache = null;
        public SeasonList()
        {
            if (_cache == null)
                _cache = new Cache<SeasonEntry>(() => Dal.DataFactory.GetDataFromDB("vwSeasonList").Select(s => new SeasonEntry(this, s)));
            this.AddRange(_cache.Retreive().Select(s => (SeasonEntry)s.Clone()));
        }
    }
    /// <summary>
    /// item for Season list
    /// </summary>
    public class SeasonEntry : ICloneable
    {
        public SeasonEntry(object parent, object row)
        {
            this.Parent = parent;
            Dal.Convertor.Copy(row, this);
        }
        public readonly object Parent;
        public int Year { get; set; }
        public int Teams { get; set; }
        public int Weeks { get; set; }
        public DateTime FirstGmDate { get; set; }
        public DateTime LastGmDate { get; set; }

        public object Clone() 
        {
            return this.MemberwiseClone();
        }
    }


    public class PlayerIndexByYear
    {
        Cache<PlayerIndexEntry> _cache = null;
        public IEnumerable<PlayerIndexEntry> this[int year]
        {
            get
            {
                int counter = 0;
                for (int i = year; i >= year - 1; i--)
                {
                    var cache = _cache;
                    if (cache == null)
                        cache = new Cache<PlayerIndexEntry>(() => Dal.DataFactory.GetDataForWeb("SeasonPlayers", i, 0, "", "").Select(s => new PlayerIndexEntry(this, s)));

                    foreach (var item in cache.Retreive())
                    {
                        counter++;
                        yield return item;
                    }
                    if (_cache == null && counter != 0)
                    {
                        _cache = cache;
                        break;
                    }
                }
            }
        }
    }
    public class PlayerIndexEntry : IJsonAble
    {
        public PlayerIndexEntry(object parent, object row)
        {
            this.Parent = parent;
            Dal.Convertor.Copy(row, this);
        }
        public readonly object Parent;

        public string ID;
        public string Player { get; set; }
        public string Pos { get; set; }
        public string Team { get; set; }
        public int Year { get; set; }
        public int Games { get; set; }
        
        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            yield return "{";
            yield return "\"id\":\"";
            yield return this.ID;
            yield return "\",\"player\":\"";
            yield return this.Player;
            yield return "\",\"pos\":\"";
            yield return this.Pos;
            yield return "\",\"team\":\"";
            yield return this.Team;
            yield return "\",\"year\":";
            yield return this.Year.ToString();
            yield return ",\"games\":";
            yield return this.Games.ToString();
            yield return "}";
        }
        #endregion IJsonAble
    }

    /// <summary>
    /// https://www.mathsisfun.com/data/least-squares-regression.html
    /// </summary>
    public class Regression : IJsonAble
    {
        public double m
        {
            get { return (n*Exy-Ex*Ey)/(n*Ex2-Ex*Ex); }
        }
        public double b
        {
            get { return (Ey - m*Ex)/(n); }
        }
        /// <summary>
        /// https://www.investopedia.com/terms/r/r-squared.asp
        /// https://en.wikipedia.org/wiki/Coefficient_of_determination
        /// </summary>
        public double R2
        {
            get
            {
                var ye = Ey / n;
                var SStot = this._dataset.Select(s => (s.Y - ye) * (s.Y - ye)).Sum();
                return 1d - SSres / SStot;
            }
        }

        int _ssres_n=0;
        double _ssres=0;
        /// <summary>
        /// Variance of prediction (without division by n)... The lane
        /// </summary>
        public double SSres
        {
            get
            {
                if (this._ssres_n == this.n)
                    return this._ssres_n; //return cached result, if nothing has changed
                var sumOfSquaresRes = this._dataset.Select(s => Math.Pow(s.Y - this.FindY(s.X), 2)).Sum();
                this._ssres_n = n;
                return this._ssres = sumOfSquaresRes;
            }
        }
        /// <summary>
        /// Variance of slope... the cone
        /// There is no statistical term I know about this, but it seems to me, that 
        /// if you predicted 1injury every 100plays, and there 500plays and got 8 injuries, the variance
        /// for (1injury every 100plays) is (8-500/100)/x or 3/500.
        /// </summary>
        public double VariancePerX
        {
            get
            {
                return this._dataset.Select(s => Math.Pow(s.Y - this.FindY(s.X), 2)/s.X).Sum()/this.n;
            }
        }


        public enum RegType { Linear }
        public RegType Type = RegType.Linear;
        public double Ex;
        public double Ey;
        public int n;
        public double Ex2;
        public double Exy;
        struct xy
        {
            public xy(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }
            public double X;
            public double Y;
        }
        List<xy> _dataset = new List<xy>();
        public IEnumerable<Tuple<double, double>> Data
        {
            get
            {
                return _dataset.Select(s => new Tuple<double, double>(s.X, s.Y));
            }
        }


        public void Add(double x, double y)
        {
            this.n++;
            this.Ex += x;
            this.Ey += y;
            this.Ex2 += x*x;
            this.Exy += x * y;
            _dataset.Add(new xy(x,y));
        }

        public double FindY(double x) 
        {
            return m * x + b;
        }
        public double FindX(double y)
        {
            return (y - b) /m;
        }
        IEnumerable<xy> Curve 
        {
            get
            {
                var minX=this._dataset.Min(s=>s.X)-1;
                yield return new xy(minX, this.FindY(minX));
                var maxX=this._dataset.Max(s=>s.X)+1;
                yield return new xy(maxX, this.FindY(maxX));
            }
        }


        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            yield return "{";
            
            yield return "\"n\":";
            yield return this.n.ToString();
            yield return ",\"type\":\"";
            yield return this.Type.ToString();
            yield return "\", \"m\":";
            yield return this.m.ToString();
            yield return ",\"b\":";
            yield return this.b.ToString();
            yield return ",\"r2\":";
            yield return this.R2.ToString();
            yield return ",\"variancePerX\":";
            yield return this.VariancePerX.ToString();

            yield return ",\"curve\":[";
            foreach (var item in this.Curve)
                yield return string.Format("{{\"x\":{0},\"y\":{1}}},",item.X, item.Y);
            yield return "]";

            yield return "}";
        }
        #endregion IJsonAble
    }
    public class InjuryModel : IJsonAble
    {
        public Dictionary<string, Regression> PosModel = new Dictionary<string, Regression>()
        {
            {"QB",new Regression()},
            {"RB",new Regression()},
            {"WR",new Regression()},
            {"TE",new Regression()},
            {"K",new Regression()},
        };
        
        public Dictionary<string, Regression> PlayerRunningModel = new Dictionary<string, Regression>();
        public Dictionary<string, Regression> PlayerFinalModel = new Dictionary<string, Regression>();
        public List<InjuryData> Data = new List<InjuryData>();
        public List<InjuryData> FinalData = new List<InjuryData>();
        public void Add(InjuryData data)
        {
            if (data.IsInjuryDate())
            {
                this.Data.Add(data);
                var posmodel = this.PosModel;
                if (posmodel.ContainsKey(data.Pos))
                {
                    var x = data.RunningPlayCount;
                    var y = data.RunningOutCount;
                    posmodel[data.Pos].Add(x, y);
                    {
                        Regression pmodel = null;
                        if (this.PlayerRunningModel.ContainsKey(data.PlayerID))
                            pmodel = this.PlayerRunningModel[data.PlayerID];
                        else
                            this.PlayerRunningModel.Add(data.PlayerID, pmodel = new Regression());
                        //condition The player data, to have running sum, per player
                        pmodel.Add(x, y);
                    }
                }
            }
            else
                this.FinalData.Add(data);
        }

        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            yield return "{";
            if (this.PlayerRunningModel.Keys.Count > 1) //only include pos analysis, if more than 1 players was analyzed.
            {
                yield return "\"posModel\":{";
                foreach (var item in this.PosModel)
                {
                    yield return "\"";
                    yield return item.Key.ToLower();
                    yield return "\":";
                    foreach (var json in item.Value.ToJsonParts())
                        yield return json;
                    yield return ",";
                }
                yield return "}, \"playerInjuryModel\":";
                foreach (var item in this.PlayerRunningModel)
                {
                    yield return "{\"playerId\":";
                    yield return item.Key;
                    yield return ", \"injuryModel\":";
                    foreach (var json in item.Value.ToJsonParts())
                        yield return json;
                    yield return "}";
                }

                yield return ", \"playerFinalData\":";
                foreach (var item in this.FinalData.ToJsonParts()) //IEnumerable.ToJsonPart() automatically puts [,]
                    yield return item;
            }
            else
            { //if only single player data, don't use array to store the models, and final aggregate data
                var player = this.FinalData.Single();
                yield return "\"playerID\":\"";
                yield return player.PlayerID;
                yield return "\", ";

                yield return "\"player\":\"";
                yield return player.Player;
                yield return "\",";

                yield return "\"playerInjuryModel\":";
                foreach (var item in this.PlayerRunningModel.Values.Single().ToJsonParts()) //.Single() DOES NOT return IEnumerable, so NO AUTO []
                    yield return item;

                yield return ", \"playerFinalData\":";
                foreach (var item in this.FinalData.Single().ToJsonParts())//.Single() DOES NOT return IEnumerable, so NO AUTO []
                    yield return item;
            }

            yield return ", \"data\":";
            foreach (var item in this.Data.ToJsonParts()) //IEnumerable.ToJsonPart() automatically puts [,]
                yield return item;

            yield return "}";
        }
        #endregion IJsonAble
    }
    public class InjuryData : IJsonAble
    {
        public InjuryData(object parent, object row)
        {
            this.Parent = parent;
            Dal.Convertor.Copy(row, this);
        }
        public readonly object Parent;


        public int InjuryGm;
        public int Year { get{ return InjuryGm/100; }}
        public int Gm { get{ return InjuryGm%100; }}
        public string PlayerID;
        public string Player {get;set;}
        public string Pos {get;set;}
        public int RunningPlayCount { get; set; } //RunningPlays
        public int RunningOutCount { get; set; } //GamesOut
        public int RunningGmCount { get; set; } //CountGm
		public double InjuryRate {get;set;}
		public DateTime? Dob {get;set;}
        public int? ProbableAge {get;set;}
        public int? DraftYear {get;set;}
        public string DraftStr;
        public int? Seasons {get;set;}
        public int LastGm { get; set; }

        public bool IsInjuryDate()
        {
            return this.InjuryGm != 999999;
        }


        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            yield return "{";

            yield return "\"injuryGm\":";
            yield return this.InjuryGm.ToString();
            yield return ", \"year\":";
            yield return this.Year.ToString();
            yield return ", \"gm\":";
            yield return this.Gm.ToString();
            yield return ", \"playerID\":\"";
            yield return this.PlayerID;
            yield return "\", \"player\":\"";
            yield return this.Player;
            yield return "\", \"pos\":\"";
            yield return this.Pos;
            yield return "\", \"runningPlayCount\":";
            yield return this.RunningPlayCount.ToString();
            yield return ", \"runningOutCount\":";
            yield return this.RunningOutCount.ToString();
            yield return ", \"runningGmCount\":";
            yield return this.RunningGmCount.ToString();
            yield return ", \"injuryRate\":";
            yield return this.InjuryRate.ToString();
            yield return ", \"dob\":\"";
            yield return this.Dob.HasValue ? this.Dob.Value.ToShortDateString() : string.Empty;
            yield return "\", \"probableAge\":";
            yield return this.ProbableAge.ToString();
            yield return ", \"draftYear\":";
            yield return this.DraftYear.ToString();
            yield return ", \"draftStr\":\"";
            yield return this.DraftStr;
            yield return "\", \"seasons\":";
            yield return this.Seasons.ToString();
            yield return ", \"lastGm\":";
            yield return this.LastGm.ToString();

            yield return "}";
        }
        #endregion IJsonAble
    }
}