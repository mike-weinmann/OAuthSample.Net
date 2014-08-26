using System.Collections.Generic;
using System.Web;

namespace OAuthSample.OAuth
{
    /// <summary>
    /// Linked In implementation of the OAuthProvider
    /// </summary>
    public class LinkedinProvider : BaseOAuthProvider
    {
        /// <summary>
        /// Linked In specific handling of user profile fields.
        /// Need to specify fields to include "email-address"
        /// </summary>
        /// <param name="context"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        protected override OAuthUser FetchUserProfile( HttpContext context, string accessToken )
        {
            string url = (string)Settings["userinfo_endpoint"] + ":(id,email-address)";
            var parameters = new Dictionary<string,object> { { "oauth2_access_token", accessToken } };
            string resp = WebUtil.Wget( url, parameters );
            IDictionary<string,object> values = WebUtil.ParseResponse( resp );

            OAuthUser user = new OAuthUser();
            user.Provider = Name;
            user.AccessToken = accessToken;
            if ( values.ContainsKey( "id" ) )
            {
                user.Id = values["id"].ToString();
            }
            if ( values.ContainsKey( "email-address" ) )
            {
                user.Email = values["email-address"].ToString();
            }

            return user;
        }
    }
}
