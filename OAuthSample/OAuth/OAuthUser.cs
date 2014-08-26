
namespace OAuthSample.OAuth
{
    /// <summary>
    /// Information about a user authenticated by an OAuth provider
    /// </summary>
    public class OAuthUser
    {
        /// <summary>
        /// Provider name
        /// </summary>
        public string Provider { get; set; }
        
        /// <summary>
        /// User's unique identifier for the Provider
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User's primary e-mail for the Provider
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Session access token
        /// </summary>
        public string AccessToken { get; set; }
    }
}
