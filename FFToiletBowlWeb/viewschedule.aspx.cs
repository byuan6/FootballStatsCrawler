using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FFToiletBowlWeb
{
    public partial class viewschedule : System.Web.UI.Page, IDataExposed
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var yr = this.Request.QueryString["year"];
            if (yr != null)
                this.Parameters = new object[] {"Json","ViewSchedule", yr };
            
            // ScheduleGrid.RowDataBound += OnRowDataBound;
            ScheduleGrid.DataSource = (IEnumerable<ScheduleGridLine>)this.Obj;
            ScheduleGrid.DataBind();
        }

        public ScheduleSeasonReport DataPseudoInjector
        {
            get
            {
                if (Session["seasonlist"] == null) //tree cached
                {
                    var data = new ScheduleSeasonReport();
                    Session["seasonlist"] = data;
                    return data;
                }
                return (ScheduleSeasonReport)Session["seasonlist"];
            }
            set
            {
                Session["seasonlist"] = value;
            }
        }

        protected void OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //var customerId = (int)Seasons.DataKeys[e.Row.RowIndex].Value;
                var season = e.Row.DataItem as SeasonReport;

                var schedulelink = e.Row.FindControl("SeasonSchedule") as HyperLink;
                schedulelink.Text = season.ScheduleCount.ToString() + " " + season.ExpectedNumberOfGames();
                if (season.IsScheduleLoaded)
                {
                    schedulelink.Enabled = true;
                    if (season.LastResultDate == null)
                        schedulelink.Text = "Schedule published";
                    else
                        schedulelink.Text = "Schedule published, Updated " + season.LastResultDate.Value.ToShortDateString();
                }
                else
                    schedulelink.Enabled = false;

                GridView childview = e.Row.FindControl("Weeks") as GridView;
                childview.DataSource = season.AllWeeks;
                childview.DataBind();
            }
        }

        #region IDataExposed
        public IJsonAble Obj
        {
            get 
            { 
                var p = this.Parameters;
                if (p != null && p.Length == 3 && (string)p[0] == "Json" && (string)p[1] == "ViewSchedule") 
                {
                    var year = (string)p[2];
                    int yr = 0;
                    if (int.TryParse(year, out yr))
                    {
                        return new JsonEnumerable<ScheduleGridLine>(this.DataPseudoInjector.GetSeason(yr).ScheduleGrid());
                    }
                }
                throw new ArgumentException("A valid year was not provided");
            }
        }

        public object[] Parameters { get;set;}
        #endregion IDataExposed
    }
}