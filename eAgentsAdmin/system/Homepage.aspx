<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true"
    CodeBehind="Homepage.aspx.cs" Inherits="PropertyOneAppWeb.system.Homepage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("../Image/icon_Notices.png", "Homepage");
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="am-g am-g-collapse">
        <%-- Welcome --%>
        <div class="am-u-sm-12">
            <table style="width: 100%">
                <tr>
                    <td style="width: 50%">
                        <table style="width: 100%">
                            <tr>
                                <td style="font-size: 30px;">                                    
                                    <%=Resources.Lang.Res_Homepage_Welcome %>
                                </td>
                            </tr>
                            <tr>
                                <td style="font-size: 15px; color: #1C578F; font-weight: bold;">
                                    <% =Session["loginname"]%> 
                                </td>
                            </tr>
                            <tr style="height: 10px">
                            </tr>
                        </table>
                    </td>
                    <td style="width: 50%; vertical-align: top;">                        
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <div>
        <button id="btnTest" style="display:none">Test</button>
    </div>
    <script type="text/javascript">
        if (window.location.href.indexOf("localhost")>=0) {
            $("#btnTest").css("display", "block");
        }
        var paras = { action: "lz77", filePath: "d:\\rwx\\turnover_query.sql" }

        $("#btnTest").click(function () {
            $.post("./homepage.aspx",
            paras,
            function (res) {
                console.log(res);
            });
        });
    </script>
</asp:Content>
