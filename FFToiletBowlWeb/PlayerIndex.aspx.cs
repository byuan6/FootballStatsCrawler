using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace FFToiletBowlWeb
{
    public partial class PlayerIndex : System.Web.UI.Page, IDataExposed
    {
        static public SeasonList Seasons = new SeasonList();
        public int Year = PlayerIndex.Seasons[0].Year;

        #region IDataExposed
        public IJsonAble Obj 
        {
            get
            {
                var datalist = this.List;
                var json = new JsonEnumerable<PlayerIndexEntry>(datalist);
                return json;
            }
        }
        public object[] Parameters { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        #endregion IDataExposed
        public IEnumerable<PlayerIndexEntry> List
        {
            get
            {
                var datalist = new PlayerIndexByYear();
                var lst = datalist[this.Year];
                return lst;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int year = 0;
            if (this.Request.QueryString.HasKeys())
            {
                var input = this.Request.QueryString["year"];
                if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out year))
                    this.Year = year;
            }
            var data = this.List;
            this.Players.DataSource = data;
            this.Players.DataBind();
        }
    }

}