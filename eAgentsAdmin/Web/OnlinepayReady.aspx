<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="OnlinepayReady.aspx.cs" Inherits="PropertyOneAppWeb.Web.OnlinepayReady" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("/Image/icon_OnlinePayment.png", "<%= Resources.Lang.Res_OnlinepayReady_Confirm %>");

            var payamount = '<% =Session["payamount"]%>';
            payamount = formatMoney(payamount, 2, "$");
            $("#label-payamount").text(payamount);

            /* 此画面切换租约号不可用 */
            setbuttonenabel(false);
        });

        /* 将待支付信息写入数据库 */
        function payready() {
            /* 先打开新窗口 */
            var newTab = window.open('about:blank');
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Web/OnlinepayReady.aspx",
                data: {
                    "action": "payready"
                },
                success: function (data) {
                    if (data == "ok") {
                        /* 再跳转 */
                        newTab.location.href = "/Web/OnlinepayGenDO.aspx";
                        $("#buttonpaywithpps").text("Return Onlinepay");
                        $("#buttonpaywithpps").attr("onclick", "returnonlinepay();");
                    }
                },
                error: function (data) {

                }
            });
        };

        /* 返回onlinepay页面 */
        function returnonlinepay() {
            window.location.href = "/Web/Onlinepay.aspx";
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="am-g am-g-collapse">
        <%-- 外边框 --%>
        <div class="am-u-sm-12">
            <%-- 深色标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p>
                    <%= Resources.Lang.Res_OnlinepayReady_Confirm%>
                </p>
            </div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">
                    <div class="am-u-sm-12">
                        <table style="width: 100%;">
                            <tr>
                                <th style="width: 30px;">
                                </th>
                                <th>
                                </th>
                            </tr>
                            <tr>
                                <td style="vertical-align: top;">
                                    <img alt="" src="/Image/icon_map.png" width="15px" height="22px" />
                                </td>
                                <td style="vertical-align: top;">
                                    <label id="label-shoparea" class="label-onlinepay2">
                                        <% =Session["premises"]%>
                                    </label>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="am-u-sm-12" style="height: 20px">
                    </div>
                    <div class="am-u-sm-12">
                        <div class="am-u-sm-4">
                            <label class="label-onlinepay4">
                                <%= Resources.Lang.Res_Onlinepay_LeaseNum %></label>
                        </div>
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                        <div class="am-u-sm-4">
                            <label class="label-onlinepay4">
                                <%= Resources.Lang.Res_Onlinepay_CustName %></label>
                        </div>
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                    </div>
                    <div class="am-u-sm-12">
                        <div class="am-u-sm-4">
                            <label class="am-form-field label-profile">
                                <% =Session["leasenumber"]%>
                            </label>
                        </div>
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                        <div class="am-u-sm-4">
                            <label class="am-form-field label-profile">
                                <% =Session["custname"]%>
                            </label>
                        </div>
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                    </div>
                    <div class="am-u-sm-12" style="height: 20px">
                    </div>
                    <div class="am-u-sm-12">
                        <div class="am-u-sm-4">
                            <label class="label-onlinepay4">
                                <%= Resources.Lang.Res_OnlinepayReady_PremiseName%></label>
                        </div>
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                        <div class="am-u-sm-4">
                            <label class="label-onlinepay4">
                                <%= Resources.Lang.Res_OnlinepayReady_PaymentAmount%></label>
                        </div>
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                    </div>
                    <div class="am-u-sm-12">
                        <div class="am-u-sm-4">
                            <label class="am-form-field label-profile">
                                <% =Session["premises"]%>
                            </label>
                        </div>
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                        <div class="am-u-sm-4">
                            <label id="label-payamount" class="am-form-field label-profile">
                            </label>
                        </div>
                        <div class="am-u-sm-2">
                            <p>
                            </p>
                        </div>
                    </div>
                    <div class="am-u-sm-12" style="height: 50px">
                    </div>
                    <div class="am-u-sm-12">
                        <button id="buttonpaywithpps" type="button" class="am-btn am-btn-secondary am-round button-pay"
                            onclick="payready()">
                            <%= Resources.Lang.Res_OnlinepayReady_PayWithPPS%>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
