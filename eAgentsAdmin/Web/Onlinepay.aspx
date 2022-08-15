<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Onlinepay.aspx.cs" Inherits="PropertyOneAppWeb.Web.Onlinepay" %>

<%@ MasterType VirtualPath="~/Site.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("/Image/icon_OnlinePayment.png", "<%= Resources.Lang.Res_Menu_Onlinepay %>");
            loadOutstandingInfo();
        });

        /* 加载待支付信息 */
        function loadOutstandingInfo() {
            /* showLoading(); */
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Web/Onlinepay.aspx",
                data: "action=loadOutstandingInfo",
                success: function (data) {
                    if (data != null && data != "") {
                        var jsonData = jQuery.parseJSON(data);
                        $("#label-amount").text("$" + formatMoney(jsonData.totalamount, 2, ""));
                    }
                    search();
                },
                error: function (data) {
                    showAlert("Notice", "Load data error!");
                }
            });
        };

        /* 加载bootgird数据 */
        function search() {
            $("#grid-data").bootgrid({
                navigation: 0,
                rowCount: -1,
                ajax: true,
                url: "/Web/Onlinepay.aspx",
                selection: true,
                multiSelect: true,
                rowSelect: true,
                sorting: true,
                ajaxSettings: {
                    cache: false
                },
                post: function () {
                    return {
                        action: "search"
                    };
                },
                formatters: {
                    "amount": function (column, row) {
                        return formatMoney(row.amount, 2, "");
                    },
                    "outstanding": function (column, row) {
                        return formatMoney(row.outstanding, 2, "");
                    },
                    "duedate": function (column, row) {
                        return formatDate(row.duedate);
                    }
                }
            }).on("selected.rs.jquery.bootgrid", function (e, rows) {
                var sum = calcOutstandingSum();
                $("#LabelPaymentAmount").text(formatMoney(sum, 2, ""));
            }).on("deselected.rs.jquery.bootgrid", function (e, rows) {
                var sum = calcOutstandingSum();
                $("#LabelPaymentAmount").text(formatMoney(sum, 2, ""));
            });
        };

        /*从bootgrid获取所有数据*/
        function getDataFromBootgrid() {
            var data = [];
            data = $("#grid-data").bootgrid("getCurrentRows");
            return data;
        };

        /*从bootgrid获取所有选中的rowid*/
        function getSelectedRowid() {
            var data = [];
            data = $("#grid-data").bootgrid("getSelectedRows");
            return data;
        };

        /*根据rowid返回整条数据*/
        function getRowdataByRowid(rowid) {
            var data = getDataFromBootgrid();
            if (data != null && data.length > 0) {
                for (var i = 0; i < data.length; i++) {
                    if (data[i].rowid == rowid) {
                        return data[i];
                    }
                }
            }
            return null;
        };

        /* 遍历所有选中行计算outstanding总和 */
        function calcOutstandingSum() {
            var sum = 0;
            var data = getSelectedRowid();
            if (data != null && data.length > 0) {
                for (var i = 0; i < data.length; i++) {
                    var rowdata = getRowdataByRowid(data[i]);
                    sum = sum + parseFloat(rowdata.outstanding);
                }
            }
            return sum;
        };

        /* 校验选中数据进行支付 */
        function getpaydata() {
            try {
                var data = getSelectedRowid();
                if (data != null && data.length > 0) {
                    var rowstrings = "";
                    for (var i = 0; i < data.length; i++) {
                        var rowdata = getRowdataByRowid(data[i]);
                        rowstrings += rowdata.transno + "," + rowdata.amount + "," + rowdata.outstanding + "," + rowdata.chargeitem + "," + rowdata.invoicelinenum;
                        if (i != data.length - 1) {
                            rowstrings += "|";
                        }
                    }
                    return rowstrings;
                }
                else {
                    return null;
                }
            }
            catch (ex) {
                return null;
            }
        };

        /* 跳转到支付准备页面 */
        function goReady() {
            var payamount = clearFormatMoney($("#LabelPaymentAmount").text());
            var rows = getpaydata();
            if (rows != null && payamount != null && payamount > 0) {
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Web/Onlinepay.aspx",
                    data: {
                        "action": "goready",
                        "payamount": payamount,
                        "paydata": rows
                    },
                    traditional: true,
                    success: function (data) {
                        window.location.href = "/Web/OnlinepayReady.aspx";
                    },
                    error: function (data) {

                    }
                });
            }
            else {
                showAlert("Notice", "Please select to pay");
            }
        };

        
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="am-g am-g-collapse">
        <div class="am-u-sm-12 global-bluepanel" style="height: auto;">
            <div class="am-u-sm-6" style="padding-left: 20px">
                <div class="am-u-sm-12" style="height: 60px; line-height: 60px;">
                    <label class="label-onlinepay1">
                        <%= Resources.Lang.Res_Op_CurrentBalance %></label>
                </div>
                <div class="am-u-sm-12">
                    <table style="width: 100%;">
                        <tr>
                            <th style="width: 5%;">
                            </th>
                            <th style="width: 95%;">
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
            </div>
            <div class="am-u-sm-6">
                <div class="am-u-sm-12" style="height: 100px; line-height: 100px; text-align: right;
                    padding-right: 20px;">
                    <label id="label-amount" class="label-onlinepay3">
                        $0.00
                    </label>
                </div>
            </div>
        </div>
        <div class="am-u-sm-12" style="height: 30px">
        </div>
        <%-- 外边框 --%>
        <div class="am-u-sm-12">
            <%-- 深色标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p>
                    <%= Resources.Lang.Res_Onlinepay_MerInfo %>
                </p>
            </div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">
                    <div class="am-u-sm-6">
                        <table style="width: 100%;">
                            <tr>
                                <td style="width: 150px; vertical-align: top;">
                                    <label class="label-onlinepay4">
                                        <%= Resources.Lang.Res_Onlinepay_LeaseNum %></label>
                                </td>
                                <td style="vertical-align: top;">
                                    <label id="label-leasenum" class="label-onlinepay4">
                                        <% =Session["leasenumber"]%>
                                    </label>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="am-u-sm-6">
                        <table style="width: 100%;">
                            <tr>
                                <td style="width: 150px; vertical-align: top;">
                                    <label class="label-onlinepay4">
                                        <%= Resources.Lang.Res_Onlinepay_CustName %></label>
                                </td>
                                <td style="vertical-align: top;">
                                    <label id="label-custname" class="label-onlinepay4">
                                        <% =Session["custname"]%>
                                    </label>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
                <div class="am-u-sm-12 global_table_line">
                </div>
                <div class="am-u-sm-12">
                    <%--bootgrid--%>
                    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid"
                        data-toggle="bootgrid">
                        <thead>
                            <tr>
                                <th data-column-id="rowid" data-identifier="true" data-visible="false">
                                    rowid
                                </th>
                                <th data-column-id="invoicelinenum" data-visible="false">
                                    invoicelinenum
                                </th>
                                <th data-column-id="transno">
                                    <%= Resources.Lang.Res_Onlinepay_TransactionNo%>
                                </th>
                                <th data-column-id="chargeitem">
                                    <%= Resources.Lang.Res_Onlinepay_ChargeItem%>
                                </th>
                                <th data-column-id="descr">
                                    <%= Resources.Lang.Res_Onlinepay_Desc%>
                                </th>
                                <th data-column-id="amount" data-formatter="amount" data-css-class="t-bg-cell" data-header-css-class="t-bg-head">
                                    <%= Resources.Lang.Res_Onlinepay_Amount%>
                                </th>
                                <th data-column-id="outstanding" data-formatter="outstanding" data-css-class="t-bg-cell"
                                    data-header-css-class="t-bg-head">
                                    <%= Resources.Lang.Res_Onlinepay_Outstandingamount%>
                                </th>
                                <th data-column-id="duedate" data-formatter="duedate">
                                    <%= Resources.Lang.Res_Onlinepay_Duedate%>
                                </th>
                            </tr>
                        </thead>
                    </table>
                </div>
                <div class="am-u-sm-12" style="height: 20px">
                </div>
                <%-- bootgird下按钮 --%>
                <div class="am-u-sm-5">
                    <%-- 查询历史按钮 --%>
                    <a class="am-btn am-btn-secondary am-round" href="/Web/Payhistory.aspx" target="_blank">
                        <%= Resources.Lang.Res_Onlinepay_History%></a>
                </div>
                <div class="am-u-sm-5">
                    <label class="label-onlinepay1">
                        <%= Resources.Lang.Res_Onlinepay_PaymentAmount %>
                    </label>
                    <label id="LabelPaymentAmount" class="label-onlinepay1">
                        0
                    </label>
                </div>
                <div class="am-u-sm-2">
                    <%--支付按钮--%>
                    <button id="buttonpay" type="button" class="am-btn am-btn-secondary am-round button-pay"
                        onclick="goReady()">
                        <%= Resources.Lang.Res_Onlinepay_Pay %>
                    </button>
                </div>
                <div class="am-u-sm-12" style="height: 50px">
                </div>
            </div>
        </div>
    </div>
</asp:Content>
