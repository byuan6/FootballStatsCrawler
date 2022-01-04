using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FFToiletBowlWeb
{
    public class FFUserJson : IJsonAble
    {
        public string Key { get; set; }
        public FFLeague RosterList = new FFLeague();


        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            throw new NotImplementedException();
        }
        #endregion IJsonAble
    }
    public class FFLeague : IJsonAble
    {

        public FFRoster[] Roster = null;
        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            throw new NotImplementedException();
        }
        #endregion IJsonAble
    }
    public class FFRoster : IJsonAble
    {
        public string TeamAbbr = null;
        public RealPlayerSlot[] Slot = new RealPlayerSlot[18];
        public int[] FFSchedule = new int[18];

        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            throw new NotImplementedException();
        }
        #endregion IJsonAble
    }
    public class RealPlayerSlot : IJsonAble
    {
        public string PlayerID;
        public string Player;
        public string PlayerURL;
        public string[] PlayerProjectionURL = new string[18];
        public RealTeamSlot Team;

        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            throw new NotImplementedException();
        }
        #endregion IJsonAble
    }
    public class RealTeamSlot : IJsonAble
    {
        public string TeamAbbr;
        public string[] Schedule;
        public string[] WeeklyHandicapURL = new string[18];

        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            throw new NotImplementedException();
        }
        #endregion IJsonAble
    }
    public class FFTeamHandicap : IJsonAble
    {
        public string Key {get;set;}

        public string TeamAbbr;

        public int ForWeek;
        public double HandicapQB;
        public double HandicapRB;
        public double HandicapWR;
        public double HandicapTE;
        public double HandicapK;
        public double HandicapDST;

        #region IJsonAble
        public IEnumerable<string> ToJsonParts()
        {
            throw new NotImplementedException();
        }
        #endregion IJsonAble
    }
}