using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;


namespace FFToiletBowlWeb
{
    static public class Helper
    {
        static public IEnumerable<DataRow> ToEnumerable(this DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
                yield return row;
        }
        static public IEnumerable<DataRowView> ToEnumerable(this DataView dataview)
        {
            int len = dataview.Count;
            for(int i=0; i<len; i++)
                yield return dataview[i];
        }

        static public bool TryParseYearWeekQuery(this string yearweek, out int yearout, out int weekout)
        {
            string y = null;
            string w = null;
            return yearweek.TryParseYearWeekQuery(out y, out w, out yearout, out weekout);
        }

        static public bool TryParseYearWeekQuery(this string yearweek, out string yeartext, out string weektext, out int yearout, out int weekout)
        {
            var parts = yearweek.Split('-');
            if (parts.Length == 2)
            {
                var yr = 0;
                var wk = 0;
                yeartext = parts[0];
                weektext = parts[1];
                if (int.TryParse(parts[0], out yr) && int.TryParse(parts[1], out wk))
                {
                    yearout = yr;
                    weekout = wk;
                    return true;
                }
            }
            else
            {
                yeartext = null;
                weektext = null;
            }
            yearout = 0;
            weekout = 0;
            return false;
        }
    }
}