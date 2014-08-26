using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace OAuthSample.OAuth
{
    public static class OAuthProviderFactory
    {
        /// <summary>
        /// Returns the provider with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IOAuthProvider Get( string name )
        {
            IOAuthProvider provider;
            if ( RegisteredProviders.TryGetValue( name, out provider ) )
            {
                return provider;
            }
            return null;
        }

        /// <summary>
        /// Register a sub-class of OAuthProvider for the name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        public static void Register<T>( string name ) where T : IOAuthProvider
        {
            _registeredTypes[name] = typeof( T );
        }
        private static readonly IDictionary<string,Type> _registeredTypes = new Dictionary<string, Type>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Read in OAuth configuration from AppSettings. Expecting
        /// entries keys to have the following format:
        /// <pre>
        ///     oauth.{provider}.{name}
        /// </pre>
        /// 
        /// <para>
        /// The "type" name is used to give the fully qualified name of a
        /// sub-class of OAuthProvider that should be instantiated for the
        /// provider.
        /// </para>
        /// </summary>
        static OAuthProviderFactory()
        {
            Register<FacebookProvider>( "facebook" );
            Register<GoogleProvider>( "google" );
            Register<LinkedinProvider>( "linkedin" );
        }

        /// <summary>
        /// Gets the list of registered providers.
        /// </summary>
        static IDictionary<string,IOAuthProvider> RegisteredProviders
        {
            get
            {
                if ( _registeredProviders == null )
                {
                    _registeredProviders = LoadProviders();
                }
                return _registeredProviders;
            }
        }
        private static IDictionary<string, IOAuthProvider> _registeredProviders;

        /// <summary>
        /// Load the providers.
        /// </summary>
        static IDictionary<string, IOAuthProvider> LoadProviders()
        {

            const string prefix = "oauth.";
            const int prefixLen = 6;

            var providers = new Dictionary<string, IOAuthProvider>();

            foreach( string key in ConfigurationManager.AppSettings.AllKeys )
            {
                if ( key.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) )
                {
                    int pos = key.LastIndexOf( '.' );
                    if ( pos > prefixLen )
                    {
                        string providerName = key.Substring( prefixLen, pos - prefixLen );
                        IOAuthProvider provider;
                        string val = ConfigurationManager.AppSettings[key];
                        string name = key.Substring( pos + 1 );
                        bool isType = "type".Equals( name, StringComparison.OrdinalIgnoreCase );

                        if ( !providers.TryGetValue( providerName, out provider ) )
                        {
                            string type = isType ? val : ConfigurationManager.AppSettings[prefix + providerName + ".type"];
                            if ( type != null )
                            {
                                provider = NewInstance<IOAuthProvider>( type );
                            }
                            else
                            {
                                Type t;
                                if ( _registeredTypes.TryGetValue( providerName, out t ) )
                                {
                                    provider = (IOAuthProvider)Activator.CreateInstance( t );
                                }
                                else
                                {
                                    provider = new BaseOAuthProvider();
                                }
                            }
                            provider.Name = providerName;
                            providers[providerName] = provider;
                        }
                        
                        if ( !isType )
                        {
                            SetProperty( provider, name, val );
                        }
                    }
                }
            }
            return providers;
        }

        /// <summary>
        /// Sets the property (using reflection). Only works for simple properties.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetProperty( object obj, string property, object value )
        {
            foreach( PropertyInfo pi in obj.GetType().GetProperties() )
            {
                if ( pi.Name.Equals( property, StringComparison.OrdinalIgnoreCase ) )
                {
                    pi.SetValue( obj, value, null );
                    break;
                }
            }
        }

        /// <summary>
        /// Creates a new instances based on the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T NewInstance<T>( string type )
        {
            string[] tokens = type.Split( ',' );
            Assembly assembly = tokens.Length > 1 ? Assembly.Load( tokens[1].Trim() ) : Assembly.GetExecutingAssembly();
            Type t = assembly.GetType( tokens[0].Trim() );
            return (T)Activator.CreateInstance( t );
        }

    }
}
