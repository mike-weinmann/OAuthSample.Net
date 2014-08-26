using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;

namespace OAuthSample.OAuth
{
    /// <summary>
    /// Web utilities
    /// </summary>
    public static class WebUtil
    {
        /// <summary>
        /// Returns the contents of the Web page as string.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string Wget( string url, IDictionary<string,object> parameters = null  )
        {
            if ( parameters != null )
            {
                url += "?" + EncodeParameters( parameters );
            }
            return Wget( WebRequest.Create( url ) );
        }

        /// <summary>
        /// Returns the contents of the Web page as string.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static string Wget( WebRequest req )
        {
            using ( WebResponse resp = req.GetResponse() )
            {
                Stream str = resp.GetResponseStream();
                if ( str != null )
                {
                    using ( var reader = new StreamReader( str ) )
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the contents of the Web page as string, using a POST
        /// instead of GET. The body should be in url encoded (same format
        /// as a query string, without leading '?').
        /// </summary>
        /// <param name="url">Page URL</param>
        /// <param name="parameters">Body </param>
        /// <returns></returns>
        public static string Wpost( string url, IDictionary<string,object> parameters = null  )
        {
            return Wpost( url, EncodeParameters( parameters ) );
        }

        /// <summary>
        /// Returns the contents of the Web page as string, using a POST
        /// instead of GET. The body should be in url encoded (same format
        /// as a query string, without leading '?').
        /// </summary>
        /// <param name="url">Page URL</param>
        /// <param name="body">Body </param>
        /// <returns></returns>
        public static string Wpost( string url, string body )
        {
            WebRequest req = WebRequest.Create( url );
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            using ( TextWriter writer = new StreamWriter( req.GetRequestStream() ) )
            {
                writer.Write( body );
            }
            return Wget( req );
        }

        /// <summary>
        /// Creates a query or form-urlencoded string from the
        /// key/value pairs.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string EncodeParameters( IDictionary<string,object> values )
        {
            StringBuilder buf = new StringBuilder();
            if ( values != null )
            {
                foreach( KeyValuePair<string,object> entry in values )
                {
                    if ( buf.Length > 0 )
                    {
                        buf.Append( "&" );
                    }
                    buf.Append( HttpUtility.UrlEncode( entry.Key ) );
                    buf.Append( "=" );

                    object val = entry.Value ?? "";
                    buf.Append( HttpUtility.UrlEncode( val.ToString() ) );
                }
            }
            return buf.ToString();
        }


        /// <summary>
        /// De-serializes the JSON string to a IDictionary of Name/Value pairs.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IDictionary<string,object> ParseResponse( string s )
        {
            IDictionary<string,object> values;
            if ( string.IsNullOrWhiteSpace( s ) )
            {
                values = new Dictionary<string, object>();
            }
            else
            {
                s = s.Trim();
                if ( s.StartsWith( "{" ) )
                {
                    values = (IDictionary<string,object>)new JavaScriptSerializer().Deserialize<object>( s );
                }
                else if ( s.StartsWith( "<" ) )
                {
                    //
                    // Linked In will return XML
                    //
                    values = new Dictionary<string, object>();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml( s );
                    if ( doc.DocumentElement != null )
                    {
                        // Simple parsing--no nesting, no attributes
                        foreach( XmlNode child in doc.DocumentElement.ChildNodes )
                        {
                            XmlElement elem = child as XmlElement;
                            if ( elem != null )
                            {
                                values[elem.LocalName] = elem.InnerText;
                            }
                        }
                    }
                }
                else
                {
                    //
                    // Facebook will return a query string parameter and not JSON
                    //
                    values = new Dictionary<string,object>();
                    foreach( string t in s.Split( '&' ) )
                    {
                        int pos = t.IndexOf( '=' );
                        if ( pos > 0 )
                        {
                            string key = HttpUtility.UrlDecode( t.Substring( 0, pos ) );
                            string val = HttpUtility.UrlDecode( t.Substring( pos + 1 ) );
                            values[key] = val;
                        }
                    }
                }
            }
            return values;
        }
    }
}