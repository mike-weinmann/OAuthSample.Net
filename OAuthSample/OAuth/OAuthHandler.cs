using System.Net;
using System.Web;
using System.Web.SessionState;

namespace OAuthSample.OAuth
{
    /// <summary>
    /// HTTP Handler for OAuth Login
    /// <para>
    /// Register by adding the following to web.config 
    /// You will need to configure this handler in the Web.config file of your 
    /// web and register it with IIS before being able to use it. For more information
    /// see the following link: http://go.microsoft.com/?linkid=8101007
    /// </para>
    /// </summary>
    public class OAuthHandler : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// Session key that information about the logged in user.
        /// </summary>
        public const string UserKey = "oauth.user";

        /// <summary>
        /// IHttpHandler interface to indicate whether a single instance
        /// can handle multiple requests.
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="context"></param>
        public virtual void ProcessRequest( HttpContext context )
        {
            //
            // Simple Dispatcher based on the following format:
            //
            //  OAuth/{provider}/{action}
            //
            // Actually, the path could be anything. The logic only
            // looks at the last two nodes of the path.
            //
            string path = context.Request.Path;
            string[] parts = path.Split( '/' );

            int pos = parts.Length - 1;
            string action = parts[pos];
            if ( action.Length == 0 )
            {
                //special logic for trailing '/'
                --pos;
                action = pos < 0 ? null : parts[pos];
            }
            
            IOAuthProvider provider = null;
            if ( pos > 0 )
            {
                string providerName = parts[pos-1];
                provider = OAuthProviderFactory.Get( providerName );
            }

            if ( provider == null )
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.StatusDescription = "Unknown OAuth Provider";
                return;
            }

            try
            {
                switch( action )
                {
                    case "login":
                    {
                        string url = provider.LoginUrl( context );
                        context.Response.Redirect( url  );

                    } break;
                    case "callback":
                    {
                        OAuthUser user = provider.LoginCallback( context );
                        if ( user == null )
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.StatusDescription = "Unknown User";
                        }
                        else
                        {
                            context.Session[UserKey] = user;
                            string appPath = context.Request.ApplicationPath;
                            context.Response.Redirect( appPath );
                        }
                    } break;
                    default:
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        context.Response.StatusDescription = "Unknown OAuth Action";
                    } break;
                }
            }
            catch ( OAuthException ex )
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.StatusDescription = ex.Message;
            }
        }

        /// <summary>
        /// Calls the OAuth provider associated with the Session user to logout.
        /// </summary>
        public static void Logout()
        {
            HttpContext context = HttpContext.Current;
            OAuthUser user = context.Session[UserKey] as OAuthUser;
            if ( user != null )
            {
                IOAuthProvider provider = OAuthProviderFactory.Get( user.Provider );
                provider.Logout( context );
            }
        }
    }
}
