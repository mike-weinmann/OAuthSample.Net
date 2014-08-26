using System.Collections.Generic;
using System.Web;

namespace OAuthSample.OAuth
{
    /// <summary>
    /// Google implementation of the OAuthProvider
    /// </summary>
    public class GoogleProvider : BaseOAuthProvider
    {
        /// <summary>
        /// Google specific handling of user profile fields. 
        /// Google uses "sub" for the user ID.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        protected override OAuthUser FetchUserProfile( HttpContext context, string accessToken )
        {
            string url = (string)Settings["userinfo_endpoint"];
            var parameters = new Dictionary<string,object> { { "access_token", accessToken } };
            string resp = WebUtil.Wget( url, parameters );
            IDictionary<string,object> values = WebUtil.ParseResponse( resp );

            OAuthUser user = new OAuthUser();
            user.Provider = Name;
            user.AccessToken = accessToken;
            if ( values.ContainsKey( "sub" ) )
            {
                user.Id = values["sub"].ToString();
            }
            if ( values.ContainsKey( "email" ) )
            {
                user.Email = values["email"].ToString();
            }

            return user;
        }

        /// <summary>
        /// Remove permissions from external OAuth provider.
        /// </summary>
        public override void Logout( HttpContext context )
        {
            OAuthUser user = context.Session[OAuthHandler.UserKey] as OAuthUser;
            if ( user != null && user.AccessToken != null )
            {
                string url = (string)Settings["revocation_endpoint"];
                if ( url != null )
                {
                    var parameters = new Dictionary<string,object> { { "token", user.AccessToken } };
                    WebUtil.Wget( url, parameters );
                }
            }
        }
    }
}
