<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OAuthSampleWeb.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <ul>
            <li><a runat="server" href="~/oauth/facebook/login">Login with Facebook</a></li>
            <li><a runat="server" href="~/oauth/google/login">Login with Google</a></li>
            <li><a runat="server" href="~/oauth/linkedin/login">Login with Linked In</a></li>
        </ul>
    </form>
</body>
</html>
