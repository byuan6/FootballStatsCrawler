using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using FFToiletBowl;

namespace FFToiletBowlWeb
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var data = this.DataPseudoInjector;
            Seasons.RowDataBound += OnRowDataBound;
            Seasons.DataSource = data.All;
            Seasons.DataBind();
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
                GridView childview = e.Row.FindControl("Weeks") as GridView;
                childview.DataSource = season.AllWeeks;
                childview.DataBind();
            }
        }

    }
}
