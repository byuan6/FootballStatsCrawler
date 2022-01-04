using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EstPerfProjection
{
    static class ListStuff 
    {
        static public T GetMax<T,V>(this IEnumerable<T> list, Func<T,V> f)
        {
            var comp = Comparer<V>.Default;
            int count=0;
            T max=default(T);
            V maxV=default(V);
            foreach (var item in list)
                if (count == 0 || comp.Compare(f(item),maxV)>0) 
                {
                    max = item;
                    maxV = f(item);
                }
            return max;
        }
    }
    class FFTeamProjection
    {
        public string[] Starter = {"QB","RB","RB","WR","WR","RB|WR|TE","TE","K","DST"};
        public PlayerProjection[] Players;
        public FFTeamProjection[] FFSchedule;

        public double NaiveWinPct() 
        {
            return Players.Average(s => s.MvpPct);
        }
        public double CompareWinPct(FFTeamProjection other) {
            var a = new List<PlayerProjection>(this.Players);
            var b = new List<PlayerProjection>();
            var diff = new List<double>();
            foreach (var item in this.Starter)
            {
                var aa = a.Where(s=>s.Pos==item).GetMax(s => s.MvpPct);
                a.Remove(aa);
                var bb = b.Where(s => s.Pos == item).GetMax(s => s.MvpPct);
                b.Remove(bb);
                diff.Add(aa.MvpPct-bb.MvpPct);
            }
            return diff.Average()+0.5;
        }
        public double ProjectedScheduleWinPct(int aftergm)
        {
            var diff = new List<double>();
            var l = FFSchedule.Length;
            var end = Math.Min(l, aftergm);
            for (int i = 0; i < end; i++)
                diff.Add(this.CompareWinPct(this.FFSchedule[i]));
            return diff.Average() + 0.5;
        }
    }
}
