using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using giscada.classes;

using System.Web.Services;

namespace giscada
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //FeederBreakers F = new FeederBreakers();
            //F.Generate();
            //FB.InfoForQGIS();
        }

        [WebMethod]
        public static List<TopLocation> GetTopResults(string label)
        {
            Search s = new Search();
            List<TopLocation> l = s.GetTopResults(label);
            //return s.GetTopResults(label);
            return l;
        }
    }
}