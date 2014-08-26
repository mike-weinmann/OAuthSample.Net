OAuthSample.Net
===============

This is a sample of using OAuth 2.0 for authentication users in a .NET application. It is mostly intended as a learning project for the mechanism between an application and an OAuth provider. In most cases (all?), an API library is more suitable for a production project.

The solution has two projects—the OAuthSample and OAuthSampleWeb.

The OAuthSample project is library that contains all of the logic for communicating with an external OAuth provider. The starting point is an IHttpHandler (OAuthHandler) that processes login requests and the subsequent callback redirect from the external OAuth provider. The rationale for using an IHttpHandler is that the OAuth protocol requires a redirect back to the application after the user has validated their credentials on the external providers web site. The IHttpHandler allows for a self-contained mechnanism that can be added to a web application.

The OAuthSample library contains an implementation for FaceBook, Google, and LinkedIn. For all three, most everything is the same until the call to retrieve the user's profile for the user's ID and e-mail address. 

The OAuthSampleWeb application is a simple web application that uses the OAuthSample library. It forwards every request starting with “OAuth” to the OAuthHandler. The configuration values for each OAuth provider is read from the <appSettings> in the web.config. Additionally, the Open ID meta data is used to find the OAuth provider's end points. For providers that do not publish a well-known URL for their metadata, a separate JSON configuration file is needed (see the FaceBook and LinkedIn cofigurations).
