<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="MyProfile.aspx.cs" Inherits="PropertyOneAppWeb.Web.MyProfile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {

            changeNav("/Image/icon_MyProfile.png", "<%= Resources.Lang.Res_Menu_Profile %>");
            loadUserProfile();
        });

        /* 加载租户信息 */
        function loadUserProfile() {
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Web/MyProfile.aspx",
                data: "action=search",
                success: function (data) {
                    if (checknotnull(data)) {
                        var jsonData = jQuery.parseJSON(data);
                        $("#label-cust-num").text(jsonData.custnum);
                        $("#label-cust-name").text(jsonData.custname);
                        $("#label-bill-addr").text(jsonData.billingaddress);
                        $("#label-leasenum").text(jsonData.leasenum);
                        try {
                            var period = jsonData.leasestartdate + " - " + jsonData.leaseenddate;
                            period = period.replace("12:00:00 AM", "");
                            period = period.replace("12:00:00 AM", "");
                        }
                        catch (ex) {
                            var period = "";
                        }
                        $("#label-leaseperiod").text(period);
                        $("#label-tradename").text(jsonData.tradename);
                        $("#label-premises").text(jsonData.premises);
                        $("#label-email").text(jsonData.email);
                        $("#label-contactperson").text(jsonData.contactperson);
                        $("#label-contactnum").text(jsonData.contactnum);
                    }
                },
                error: function (data) {
                    AlertJqueryAjaxError(data);
                }
            });
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- Rider --%>
    <div style="padding: 0px 0px 20px 0px;">
        <table style="width: 100%;">
            <tr>
                <td>
                    <%= Resources.Lang.Res_Rider%>
                </td>
            </tr>
        </table>
    </div>
    <div class="am-g am-g-collapse">
        <%--Basic Info--%>
        <div class="am-u-sm-12">
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p>
                    <%= Resources.Lang.Res_Profile_BasicInfo %>
                </p>
            </div>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <div class="am-u-sm-12">
                    <div class="am-u-sm-6">
                        <div class="am-u-sm-11">
                            <label>
                                <%= Resources.Lang.Res_Profile_CustNum%>
                            </label>
                        </div>
                        <div class="am-u-sm-1">
                        </div>
                    </div>
                    <div class="am-u-sm-6">
                        <div class="am-u-sm-1">
                        </div>
                        <div class="am-u-sm-11">
                            <label>
                                <%= Resources.Lang.Res_Profile_CustName %>
                            </label>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12">
                    <div class="am-u-sm-6">
                        <div class="am-u-sm-11">
                            <label id="label-cust-num" class="am-form-field label-profile">
                            </label>
                        </div>
                        <div class="am-u-sm-1">
                        </div>
                    </div>
                    <div class="am-u-sm-6">
                        <div class="am-u-sm-1">
                        </div>
                        <div class="am-u-sm-11">
                            <label id="label-cust-name" class="am-form-field label-profile">
                            </label>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12">
                    <label>
                        <%= Resources.Lang.Res_Profile_BillAddr %></label>
                </div>
                <div class="am-u-sm-12">
                    <label id="label-bill-addr" class="am-form-field label-profile">
                    </label>
                </div>
            </div>
        </div>
        <%--Lease Info--%>
        <div class="am-u-sm-12">
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p>
                    <%= Resources.Lang.Res_Profile_LeaseInfo %>
                </p>
            </div>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <div class="am-u-sm-12">
                    <div class="am-u-sm-6">
                        <div class="am-u-sm-11">
                            <label>
                                <%= Resources.Lang.Res_Profile_LeaseNum%>
                            </label>
                        </div>
                        <div class="am-u-sm-1">
                        </div>
                    </div>
                    <div class="am-u-sm-6">
                        <div class="am-u-sm-1">
                        </div>
                        <div class="am-u-sm-11">
                            <label>
                                <%= Resources.Lang.Res_Profile_LeasePeriod %>
                            </label>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12">
                    <div class="am-u-sm-6">
                        <div class="am-u-sm-11">
                            <label id="label-leasenum" class="am-form-field label-profile">
                            </label>
                        </div>
                        <div class="am-u-sm-1">
                        </div>
                    </div>
                    <div class="am-u-sm-6">
                        <div class="am-u-sm-1">
                        </div>
                        <div class="am-u-sm-11">
                            <label id="label-leaseperiod" class="am-form-field label-profile">
                            </label>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12">
                    <label>
                        <%= Resources.Lang.Res_Profile_TradeName %></label>
                </div>
                <div class="am-u-sm-12">
                    <label id="label-tradename" class="am-form-field label-profile">
                    </label>
                </div>
                <div class="am-u-sm-12">
                    <label>
                        <%= Resources.Lang.Res_Profile_Premises %></label>
                </div>
                <div class="am-u-sm-12">
                    <label id="label-premises" class="am-form-field label-profile">
                    </label>
                </div>
            </div>
        </div>
        <%--Contact--%>
        <div class="am-u-sm-12">
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p>
                    <%= Resources.Lang.Res_Profile_Contact %>
                </p>
            </div>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <div class="am-u-sm-12">
                    <div class="am-u-sm-4">
                        <div class="am-u-sm-10">
                            <label>
                                <%= Resources.Lang.Res_Profile_Email %></label>
                        </div>
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                    </div>
                    <div class="am-u-sm-4">
                        <div class="am-u-sm-1">
                            <p>
                            </p>
                        </div>
                        <div class="am-u-sm-10">
                            <label>
                                <%= Resources.Lang.Res_Profile_ContactPerson %></label>
                        </div>
                        <div class="am-u-sm-1">
                            <p>
                            </p>
                        </div>
                    </div>
                    <div class="am-u-sm-4">
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                        <div class="am-u-sm-10">
                            <label>
                                <%= Resources.Lang.Res_Profile_ContactNum %></label>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12">
                    <div class="am-u-sm-4">
                        <div class="am-u-sm-10">
                            <label id="label-email" class="am-form-field label-profile">
                            </label>
                        </div>
                        <div class="am-u-sm-2" style="padding: 5px 0px 0px 5px;">
                            <a href="/Web/changemail.aspx">
                                <%= Resources.Lang.Res_Profile_ChangeEmail %></a>
                        </div>
                    </div>
                    <div class="am-u-sm-4">
                        <div class="am-u-sm-1">
                            <p>
                            </p>
                        </div>
                        <div class="am-u-sm-10">
                            <label id="label-contactperson" class="am-form-field label-profile">
                            </label>
                        </div>
                        <div class="am-u-sm-1">
                            <p>
                            </p>
                        </div>
                    </div>
                    <div class="am-u-sm-4">
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                        <div class="am-u-sm-10">
                            <label id="label-contactnum" class="am-form-field label-profile">
                            </label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <%--Others--%>
        <div class="am-u-sm-12">
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p>
                    <%= Resources.Lang.Res_Profile_Others %>
                </p>
            </div>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
            </div>
        </div>
    </div>
</asp:Content>
