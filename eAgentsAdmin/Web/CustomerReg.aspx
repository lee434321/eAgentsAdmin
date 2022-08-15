<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CustomerReg.aspx.cs" Inherits="PropertyOneAppWeb.Web.CustomerReg" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label runat="server" ID="lblLeasenum"></asp:Label>
        <asp:Label runat="server" ID="lblPwd" Text="请输入密码"></asp:Label>
        <asp:TextBox runat="server" ID="tbxPwd" TextMode="Password"></asp:TextBox>
        <asp:Button runat="server" ID="tbxOK" Text="提交" onclick="tbxOK_Click" />
    </div>
    </form>
</body>
</html>
