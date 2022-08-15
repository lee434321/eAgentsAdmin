<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ContactUs.aspx.cs" Inherits="PropertyOneAppWeb.Web.ContactUs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            /* 添加默认模板中的一些元素 */
            $("#div-title").show();
            $("#div-title").addClass("global_color_navyblue");
            $("#div-content").addClass("global_color_lightblue");

            changeNav("/Image/icon_ContactUs.png", "<%= Resources.Lang.Res_Menu_ContactUs %>");
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table style="width: 100%;">
        <tr>
            <td style="width: 600px;">
                <img alt="" src="/Image/contactUs_pic.png" width="100%" height="550px" />
            </td>
            <td style="width: 50px;">
            </td>
            <td>
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <p class="p-contactus-1">
                                <%= Resources.Lang.Res_ContactUs_Leasing%>
                            </p>
                            <p class="p-contactus-2">
                                Phone: (852)2128 7500
                            </p>
                            <p class="p-contactus-2">
                                Email: infohk@hpgl.com
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <div style="background-color: #4BB5E7; width: 100%; height: 1px">
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p class="p-contactus-1">
                                <%= Resources.Lang.Res_ContactUs_Billing%>
                            </p>
                            <p class="p-contactus-2">
                                Phone: (852)2128 0066
                            </p>
                            <p class="p-contactus-2">
                                Email: billing@hpgl.com
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <div style="background-color: #4BB5E7; width: 100%; height: 1px">
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p class="p-contactus-1" style="font-size: 4.0rem;">
                                <%= Resources.Lang.Res_ContactUs_EstateManagement%>
                            </p>
                            <p class="p-contactus-2">
                                Phone: TEST
                            </p>
                            <p class="p-contactus-2">
                                Email: TEST
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</asp:Content>
