using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace OAuthSample.OAuth
{
    /// <summary>
    /// Helper class that contains the resolved and parsed values for
    /// the OAuth values.
    /// </summary>
    public class OAuthSettings
    {
        /// <summary>
        /// Creates the data. The metaData is either a JSON string
        /// or an "http" or "file" reference that resolved to a JSON string;
        /// </summary>
        /// <param name="metaData"></param>
        public OAuthSettings( string metaData )
        {
            MetaData = metaData;
        }

        /// <summary>
        /// Source meta data.
        /// </summary>
        public string MetaData { get; private set; }

        /// <summary>
        /// Gets the values associated with the key value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                object tmp;
                if ( Values.TryGetValue( key, out tmp ) )
                {
                    return tmp;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the parsed values.
        /// </summary>
        public IDictionary<string,object> Values
        {
            get
            {
                if ( _values == null )
                {
                    _values = ResolveValues( MetaData );
                }
                return _values;
            }
        }
        private IDictionary<string,object> _values;

        /// <summary>
        /// Resolves and parses the metaData to key/value lookups.
        /// </summary>
        /// <param name="metaData"></param>
        /// <returns></returns>
        protected static IDictionary<string,object> ResolveValues( string metaData )
        {
            string json = null;
            if ( metaData != null )
            {
                if ( metaData.StartsWith( "http", StringComparison.OrdinalIgnoreCase ) )
                {
                    json = WebUtil.Wget( metaData );
                }
                else if ( metaData.StartsWith( "file://", StringComparison.OrdinalIgnoreCase ) )
                {
                    string fileName = metaData.Substring( "file://".Length );
                    string filePath = HttpContext.Current.Server.MapPath( "~/" + fileName );
                    using ( TextReader reader = File.OpenText( filePath ) )
                    {
                        json = reader.ReadToEnd();
                    }
                }
                else
                {
                    json = metaData;
                }
            }

            return json == null ? 
                new Dictionary<string, object>() :
                (IDictionary<string,object>)new JavaScriptSerializer().Deserialize<object>( json );
        }
    }
}
