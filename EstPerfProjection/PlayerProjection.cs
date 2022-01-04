using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EstPerfProjection
{
    public class PlayerProjection
    {
        public int Year;

        public string PlayerID;
        public string Player;
        public string Pos;

        public int EndOfYearProjection;
 
        public int PaComp;
        public int PaAtt;
        public int PaYd;
        public int PaTD;
        public int PaINT;
        
        public int RuAtt;
        public int RuYd;
        public int RuTD;

        public int ReTgt;
        public int ReRec;
        public int ReYd;
        public int ReTD;
     
        public int KiFGM;
        public int KiFGA;
        public int KiFGP;
        public int KiEPM;
        public int KiEPA;
     
        public int DSack;
        public int DFR;
        public int DINT;
        public int DTD;
        public int DPA;
        public int DPaYd;
        public int DRuYd;
        public int DSafety;
        public int DKickTD;

        public int StartValue;

        public float RuYdPerAtt;
        public float RedZoneShareRate;
        public float RedZoneVisits;
        public float RedZoneConversion;

        public int Games;
        public double InjuryRate;

        public double MvpPct;

        public void PPRScoringProjections()
        {
            this.RuYd = Convert.ToInt32(this.RuAtt * RuYdPerAtt);
            this.RuTD = Convert.ToInt32(this.RuAtt * RedZoneVisits * RedZoneConversion * RedZoneShareRate);

            this.ReTD = Convert.ToInt32(this.ReTgt * RedZoneVisits * RedZoneConversion * RedZoneShareRate / 4);
        }
        public int PPRScoring()
        {
            return StartValue
                +PaYd/10
                +PaTD*6
                +PaINT*-1
        
                +RuYd/10
                +RuTD*6

                +ReRec
                +ReYd/10
                +ReTD*6
     
                +KiFGM*3
                +KiEPM*-1
                +KiEPA*-1
     
                +DSack
                +DFR
                +DINT
                +DTD*6
                +DPA<10 ? 10 : 0
                +DPaYd <200 ? 5 : 0
                +DRuYd <100 ? 5 : 0
                +DSafety*2
                +DKickTD*6;
        }
    }
}
