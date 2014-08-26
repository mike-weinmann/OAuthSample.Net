using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace OAuthSample.OAuth
{
    /// <summary>
    /// FaceBook implementation of the OAuthProvider
    /// </summary>
    public class FacebookProvider : BaseOAuthProvider
    {
        /// <summary>
        /// Remove permissions from external OAuth provider.
        /// </summary>
        /// <param name="context"></param>
        public override void Logout( HttpContext context)
        {
            OAuthUser user = context.Session[OAuthHandler.UserKey] as OAuthUser;
            if ( user != null )
            {
                string url = (string)Settings["revocation_endpoint"];
                url += "/" + user.Id + "/permissions?access_token=" + user.AccessToken;

                WebRequest req  = WebRequest.Create( url );
                req.Method = "DELETE";
                WebUtil.Wget( req );
            }
        }
    }
}
