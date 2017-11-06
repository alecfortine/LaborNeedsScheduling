<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SecurityError.aspx.cs" Inherits="SecurityError" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>
        Security Error
    </title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <center>
        <p style="font-family:Verdana; font-size: 12pt; color: Red">You are not authorized to view the requested page.</p>
        <br />
        <asp:Label ID="lblLogonUser" runat="server" Text="" CssClass="lbl2"></asp:Label>
    </center>
    </div>
    </form>
</body>
</html>