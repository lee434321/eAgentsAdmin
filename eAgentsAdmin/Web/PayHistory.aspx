<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="PayHistory.aspx.cs" Inherits="PropertyOneAppWeb.Web.PayHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("/Image/icon_OnlinePayment.png", "<%= Resources.Lang.Res_Payhistory_PaymentHistory %>");
            $('#datapicker-from').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' }).on('changeDate.datepicker.amui', function (event) { });
            $('#datapicker-to').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' }).on('changeDate.datepicker.amui', function (event) { });
            search();
        });

        var firstPageload = true; /* 是否第一次加载页面 */
        function search() {
            if (firstPageload == true) {  /* 如果是第一次 */
                $("#grid-data").bootgrid({
                    navigation: 2,
                    rowCount: 10,
                    sorting: true,
                    ajax: true,
                    url: "/Web/PayHistory.aspx",
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        var startdate = $("#datapicker-from").val();
                        var enddate = $("#datapicker-to").val();
                        return {
                            action: "search",
                            startdate: startdate,
                            enddate: enddate
                        };
                    },
                    formatters: {
                        "payresult": function (column, row) {
                            var payresult = row.payresult;
                            return '<a href="JavaScript:void(0);" class="am-badge am-badge-primary am-radius">' + 'Success' + '</a>';
                        },
                        "paydate": function (column, row) {
                            return formatDate(row.paydate);
                        },
                        "payamount": function (column, row) {
                            return formatMoney(row.payamount, 2, "");
                        }
                    }
                });
                firstPageload = false;
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");
            };
        };

        
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="am-g am-g-collapse">
        <div class="am-u-sm-12">
            <%-- 标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p>
                    <%= Resources.Lang.Res_Payhistory_PaymentHistory%></p>
            </div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 搜索时间区域 --%>
                <div class="am-u-sm-12" style="height: 37px">
                    <div class="am-u-sm-2" style="line-height: 37px">
                        <label>
                            <%= Resources.Lang.Res_Onlinepay_PaymentDate %>
                        </label>
                    </div>
                    <div class="am-u-sm-10">
                        <!-- 开始日期 -->
                        <div class="am-u-sm-5">
                            <div class="am-u-sm-2" style="line-height: 37px; text-align: right; padding-right: 10px;">
                                <label>
                                    From:
                                </label>
                            </div>
                            <div class="am-u-sm-9">
                                <input id="datapicker-from" type="text" class="am-form-field" placeholder="DD-MM-YYYY"
                                    readonly />
                            </div>
                            <div class="am-u-sm-1">
                                <p>
                                </p>
                            </div>
                        </div>
                        <div class="am-u-sm-5">
                            <!-- 结束日期 -->
                            <div class="am-u-sm-2" style="line-height: 37px; text-align: right; padding-right: 10px;">
                                <label>
                                    To:
                                </label>
                            </div>
                            <div class="am-u-sm-9">
                                <input id="datapicker-to" type="text" class="am-form-field" placeholder="DD-MM-YYYY"
                                    readonly />
                            </div>
                            <div class="am-u-sm-1">
                                <p>
                                </p>
                            </div>
                        </div>
                        <div class="am-u-sm-2">
                            <!-- 搜索按钮 -->
                            <button id="button-search" type="button" class="am-btn am-btn-secondary am-round"
                                onclick="search()">
                                <%= Resources.Lang.Res_Site_Search%></button>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12" style="height: 20px">
                </div>
                <div class="am-u-sm-12">
                    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid"
                        data-toggle="bootgrid">
                        <thead>
                            <tr>
                                <th data-column-id="paydate" data-formatter="paydate" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Payhistory_Paydate%>
                                </th>
                                <th data-column-id="payamount" data-formatter="payamount" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Payhistory_Payamount%>
                                </th>
                                <th data-column-id="currency" data-visible="false">
                                    <%= Resources.Lang.Res_Payhistory_Currency%>
                                </th>
                                <th data-column-id="payresult" data-formatter="payresult" data-align="center" data-header-align="center"
                                    data-sortable="false">
                                    <%= Resources.Lang.Res_Payhistory_Payresult%>
                                </th>
                            </tr>
                        </thead>
                    </table>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
