using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FFToiletBowlWeb
{
    public partial class InjuryModelData : System.Web.UI.Page, IDataExposed
    {
        //static public SeasonList Seasons = new SeasonList();
        //public int Year = PlayerIndex.Seasons[0].Year;

        #region IDataExposed
        public IJsonAble Obj
        {
            get
            {
                var datalist = this.Model;
                return datalist;
            }
        }
        public object[] Parameters { get; set; }
        public string[] ParameterDesc { get { return new string[] { "Needs to be 'Json'", "Needs to be 'InjuryModelData'", "PlayerID" }; } }
        #endregion IDataExposed
        public IEnumerable<InjuryData> List
        {
            get
            {
                var datalist = new ScheduleSeasonReport();
                var lst = datalist.InjuryModel;
                return lst;
            }
        }
        public InjuryModel Model
        {
            get
            {
                InjuryModel model = new InjuryModel();
                var p = this.Parameters;
                if (p == null)
                {
                    foreach (var item in this.List)
                        model.Add(item);
                    return model;
                }
                else if (p.Length == 3 && (string)p[0] == "Json" && (string)p[1] == "InjuryModelData")
                {
                    var playerID = (string)p[2];
                    foreach (var item in this.List.Where(s => s.PlayerID == playerID))
                        model.Add(item);
                    return model;
                }
                else
                    throw new ArgumentOutOfRangeException("check .ParameterDesc for list of expected parameters");
            }
        }
        public void BindModel(string pos, Regression model, Label l, GridView grid)
        {
            l.Text = string.Format("#injuries = ({0:0.00000000})#plays + {1:0.00}, R2={2:0.000}, StDevForM={3:0.000}inj/play", model.m, model.b, model.R2, Math.Sqrt(model.VariancePerX));
            if (model.Data.Count(s=>true)!=0 ) 
            {
                var maxX = model.Data.Max(s => s.Item1);
                var modelXY = new List<Tuple<double, double, string>>() {
                        new Tuple<double,double,string>(0, model.FindY(0), string.Empty ),
                        new Tuple<double,double,string>(maxX,  model.FindY(maxX), "Avg "+pos+" Injury Rate (per play)"),
                    };
                grid.DataSource = modelXY;
                grid.DataBind();
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.Request.QueryString["playerID"] != null)
                this.Parameters = new object[] { "Json", "InjuryModelData", this.Request.QueryString["playerID"] };

            var data = this.Model;
            var lst = data.Data;

            this.QBInjuredBefore.DataSource = lst.Where(s => s.Pos == "QB" && s.InjuryGm != 999999);
            this.QBInjuredBefore.DataBind();
            this.RBInjuredBefore.DataSource = lst.Where(s => s.Pos == "RB" && s.InjuryGm != 999999);
            this.RBInjuredBefore.DataBind();
            this.WRInjuredBefore.DataSource = lst.Where(s => s.Pos == "WR" && s.InjuryGm != 999999);
            this.WRInjuredBefore.DataBind();
            this.TEInjuredBefore.DataSource = lst.Where(s => s.Pos == "TE" && s.InjuryGm != 999999);
            this.TEInjuredBefore.DataBind();
            this.KInjuredBefore.DataSource = lst.Where(s => s.Pos == "K" && s.InjuryGm != 999999);
            this.KInjuredBefore.DataBind();

            this.CurrentQB.DataSource = lst.Where(s => s.Pos == "QB" && s.InjuryGm == 999999);
            this.CurrentQB.DataBind();
            this.CurrentRB.DataSource = lst.Where(s => s.Pos == "RB" && s.InjuryGm == 999999);
            this.CurrentRB.DataBind();
            this.CurrentWR.DataSource = lst.Where(s => s.Pos == "WR" && s.InjuryGm == 999999);
            this.CurrentWR.DataBind();
            this.CurrentTE.DataSource = lst.Where(s => s.Pos == "TE" && s.InjuryGm == 999999);
            this.CurrentTE.DataBind();
            this.CurrentK.DataSource = lst.Where(s => s.Pos == "K" && s.InjuryGm == 999999);
            this.CurrentK.DataBind();

            BindModel("QB", data.PosModel["QB"], EquationQB, ModelQB);
            BindModel("RB", data.PosModel["RB"], EquationRB, ModelRB);
            BindModel("WR", data.PosModel["WR"], EquationWR, ModelWR);
            BindModel("TE", data.PosModel["TE"], EquationTE, ModelTE);
            BindModel("K", data.PosModel["K"], EquationK, ModelK);
            
        }

    }
}