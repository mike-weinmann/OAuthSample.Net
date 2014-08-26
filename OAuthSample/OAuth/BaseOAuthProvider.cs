using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.SessionState;

namespace OAuthSample.OAuth
{
    /// <summary>
    /// Base OAuth provider implementation.
    /// </summary>
    public class BaseOAuthProvider : IOAuthProvider
    {
        //------------------------------------------------------------------
        // IOAuthProvider
        //------------------------------------------------------------------
        #region IOAuthProvider

        /// <summary>
        /// Session Key that contains a value created on initial request
        /// to prevent against cross-site request forgery
        /// </summary>
        const string StateKey = "oauth.state";


        /// <summary>
        /// Returns the redirect URL to the external OAuth provider's
        /// login page.
        /// </summary>
        /// <param name="context"></param>
        public virtual string LoginUrl( HttpContext context )
        {
            //
            // Add the state to the session so when we get the redirect request
            // we can verify that the we initiated the request.
            string state = Guid.NewGuid().ToString();
            context.Session.Add( StateKey, state );

            var parameters = new Dictionary<string,object>
            {
                { "client_id", ClientId },
                { "response_type", "code" },
                { "state", state },
                { "redirect_uri", CallbackUrl }
            };
            if ( Scope != null )
            {
                parameters["scope"] = Scope;
            }

            return Settings["authorization_endpoint"] + "?" + WebUtil.EncodeParameters( parameters );
        }

        /// <summary>
        /// Completes the login process. After completed the login at the OAuth
        /// provider's login page, the browser is redirected back to the
        /// Callback URL.
        /// </summary>
        /// <param name="context"></param>
        public virtual OAuthUser LoginCallback( HttpContext context )
        {
            string code = VerifyAuthorizationCode( context );
            if ( code != null )
            {
                //
                // Get an accessToken
                string accessToken= FetchAccessToken( context, code );
                if ( accessToken != null )
                {
                    OAuthUser user = FetchUserProfile( context, accessToken );
                    return user;
                }
            }
            return null;
        }


        /// <summary>
        /// "Logs out" of the external OAuth providers. Since OAuth is a single
        /// sign on (SSO) solution, we cannot safely logout out of the external
        /// site. We can, however, ask the external OAuth provider to revoke the
        /// permissions associated with the access token.
        /// </summary>
        public virtual void Logout( HttpContext context )
        {
            OAuthUser user = context.Session["ouath.user"] as OAuthUser;
            if ( user != null && user.AccessToken != null )
            {
                string url = (string)Settings["revocation_endpoint"];
                if ( url != null )
                {
                    var parameters = new Dictionary<string,object> { { "access_token", user.AccessToken } };
                    WebUtil.Wget( url, parameters );
                }
            }
        }

        #endregion IOAuthProvider

        //------------------------------------------------------------------
        // Login processing (extension points for sub-classes)
        //------------------------------------------------------------------
        #region Login processing

        /// <summary>
        /// Verifies the HTTP call back request by checking the "state" 
        /// parameter and returns the "code" parameter. Any errors are
        /// sent to the response directly.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual string VerifyAuthorizationCode( HttpContext context )
        {
            HttpSessionState session = context.Session;
            HttpRequest request = context.Request;

            //
            // Make sure that the User's session has our state token
            string sessionState = session[StateKey] as string;
            if ( sessionState == null )
            {
                throw new OAuthException( HttpStatusCode.Forbidden, "Missing Session 'state' token" );
            }
            session.Remove( StateKey );

            //
            // Verify that the returned state token matches the session's
            string reqState = request.QueryString["state"];
            if ( !sessionState.Equals( reqState ) )
            {
                throw new OAuthException( HttpStatusCode.Forbidden, "Invalid request 'state' token" );
            }

            //
            // Check for the authorization code
            string code = request.QueryString["code"];
            if ( code == null )
            {
                throw new OAuthException( HttpStatusCode.Forbidden, "Missing authorization 'code'" );
            }
            return code;
        }

        /// <summary>
        /// Calls the OAuth provider to exchange the one-time authorization
        /// code to an access token that can be used for subsequent API calls.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected virtual string FetchAccessToken( HttpContext context, string code )
        {
            //
            // Exchange authorization code for an access token
            //
            string url = (string)Settings["token_endpoint"];
            var parameters = new Dictionary<string,object>
            {
                { "client_id", ClientId },
                { "client_secret", SecretKey },
                { "redirect_uri", CallbackUrl },
                { "code", code },
                { "grant_type", "authorization_code" } 
            };
            string accessResponse = WebUtil.Wpost( url, parameters );

            IDictionary<string,object> json = WebUtil.ParseResponse( accessResponse );
            object accessToken;
            if ( json.TryGetValue( "access_token", out accessToken ) )
            {
                return accessToken.ToString();
            }
            else
            {
                throw new OAuthException( HttpStatusCode.Forbidden, "Missing 'access_token' code" );
            }
        }

        /// <summary>
        /// Fetches the User Profile
        /// </summary>
        /// <param name="context"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        protected virtual OAuthUser FetchUserProfile( HttpContext context, string accessToken )
        {
            string url = (string)Settings["userinfo_endpoint"];
            var parameters = new Dictionary<string,object> { { "access_token", accessToken } };
            string resp = WebUtil.Wget( url, parameters );
            IDictionary<string,object> values = WebUtil.ParseResponse( resp );

            OAuthUser user = new OAuthUser();
            user.Provider = Name;
            user.AccessToken = accessToken;
            if ( values.ContainsKey( "id" ) )
            {
                user.Id = values["id"].ToString();
            }
            if ( values.ContainsKey( "email" ) )
            {
                user.Email = values["email"].ToString();
            }

            return user;
        }

        #endregion

        //------------------------------------------------------------------
        // Properties
        //------------------------------------------------------------------
        #region Properties

        /// <summary>
        /// Provider name. This is internal to the application and
        ///  matches name in AppSettings.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Provider issued Client ID for the application.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Provider issues Secret Key for the application. This is used
        /// when exchanging an authorization code for an access token. It
        /// should never be seen by the client.
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Authorization scope (permissions) being requested. For instance, to ask 
        /// for the user's e-mail, the scope would include be "email". Use space
        /// to delimit multiple permissions.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Open ID MetaData for the OAuth provider. This could be a 
        /// reference to the HTTP site that returns the MetaData.
        /// The MetaData is a JSON string that is parsed and stored
        /// in the Values dictionary.
        /// </summary>
        public string MetaData { get; set; }

        /// <summary>
        /// Creates the settings.
        /// </summary>
        public OAuthSettings Settings
        {
            get
            {
                if ( _settings == null )
                {
                    _settings = new OAuthSettings( MetaData );
                }
                return _settings;
            }
        }
        private OAuthSettings _settings;

        /// <summary>
        /// Gets the callback URL for the provider.
        /// request.
        /// </summary>
        /// <returns></returns>
        protected virtual string CallbackUrl
        {
            get
            {
                if ( _callbackUrl == null )
                {
                    //dynamically generate based on current request

                    HttpRequest req = HttpContext.Current.Request;
                    string url = req.Url.ToString();

                    //
                    // Check for HTTPS handled by load balancer.
                    if ( url.StartsWith( "http:" ) && "https".Equals( req.Headers["X-Forwarded-Proto"] ) )
                    {
                        url = "https" + url.Substring( 4 );
                    }

                    //
                    // Look for the provider name in the URL. This should be Okay
                    // as long as we are calling this method in the context of a
                    // request mapped to the OAuthHandler.
                    //
                    int pos = url.LastIndexOf( "/" + Name + "/", StringComparison.OrdinalIgnoreCase );
                    if ( pos > 0 )
                    {
                        url = url.Substring( 0, pos + Name.Length + 1 );
                    }
                    url +=  "/callback";
                    return url;
                }
                else
                {
                    return _callbackUrl;
                }
            }
            set { _callbackUrl = value; }
        }
        private string _callbackUrl;

        #endregion
    }
}