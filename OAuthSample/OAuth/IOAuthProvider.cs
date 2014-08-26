using System.Web;

namespace OAuthSample.OAuth
{
    /// <summary>
    /// Interface for interacting with an external OAuth provider
    /// </summary>
    public interface IOAuthProvider
    {
        /// <summary>
        /// Provider Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Returns the redirect URL to the external OAuth provider's
        /// login page.
        /// <para>
        /// The LoginUrl should contain a redirect URL that maps to
        /// ".../callback", which will trigger the LoginCallback method.
        /// </para>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string LoginUrl( HttpContext context );

        /// <summary>
        /// Process the redirect returned from the external OAuth provider's
        /// login page.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        OAuthUser LoginCallback( HttpContext context );

        /// <summary>
        /// "Logs out" of the external OAuth providers. Since OAuth is a single
        /// sign on (SSO) solution, we cannot safely logout out of the external
        /// site. We can, however, ask the external OAuth provider to revoke the
        /// permissions associated with the access token.
        /// </summary>
        /// <param name="context"></param>
        void Logout( HttpContext context );

    }
}
