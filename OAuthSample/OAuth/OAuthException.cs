using System;
using System.Net;

namespace OAuthSample.OAuth
{
    /// <summary>
    /// Exception processing OAuth
    /// </summary>
    public class OAuthException : Exception
    {
        /// <summary>
        /// Creates the Exception
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public OAuthException( HttpStatusCode statusCode, string message ) : this( (int)statusCode, message )
        {
        }

        /// <summary>
        /// Creates the Exception
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public OAuthException( int statusCode, string message ) : base( message )
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public int StatusCode { get; private set; }
    }
}
