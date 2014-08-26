<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OAuthSampleWeb.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>

        <dl>
            <dt>Provider</dt> <dd><%=OAuthUser.Provider%></dd>
            <dt>Access Token</dt> <dd><%=OAuthUser.AccessToken%></dd>
            <dt>User ID</dt> <dd><%=OAuthUser.Id%></dd>
            <dt>Email</dt> <dd><%=OAuthUser.Email%></dd>
        </dl>
            
    </div>
    <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Logout" />

    </form>
</body>
</html>
