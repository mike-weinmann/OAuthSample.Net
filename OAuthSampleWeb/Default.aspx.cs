using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OAuthSample.OAuth;

namespace OAuthSampleWeb
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            OAuthUser user = Session[OAuthHandler.UserKey] as OAuthUser;
            if ( user == null )
            {
                Response.Redirect( "~/Login.aspx" );
            }
            else
            {
                OAuthUser = user;
            }
        }

        protected OAuthUser OAuthUser { get; set; }

        protected void Button1_Click(object sender, EventArgs e)
        {
            OAuthHandler.Logout();
            Session.Abandon();
            Response.Redirect( "~/Login.aspx" );
        }
    }
}