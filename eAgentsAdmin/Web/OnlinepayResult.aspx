<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="OnlinepayResult.aspx.cs" Inherits="PropertyOneAppWeb.Web.OnlinepayResult" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table style="width: 100%;">
        <tr>
            <td style="text-align: center;">
                <img alt="" src="/Image/temp/ppsresult.png" />
            </td>
        </tr>
        <tr>
            <td style="text-align: center;">
                <a href="/Web/Onlinepay.aspx" class="am-btn am-btn-secondary am-round button-pay">Return</a>
            </td>
        </tr>
    </table>
</asp:Content>
