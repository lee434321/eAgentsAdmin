<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Statement.aspx.cs" Inherits="PropertyOneAppWeb.Web.Statement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            /* 添加默认模板中的一些元素 */
            $("#div-title").show();
            $("#div-title").addClass("global_color_navyblue");
            $("#div-content").addClass("global_color_lightblue");

            changeNav("/Image/icon_MyStatment.png", "<%= Resources.Lang.Res_Menu_Statement %>");
            search();
        });

        /* 打开新窗口，加载报表 */
        function viewPdf(filename, statementnum) {
            /* 先打开新窗口 */
            var newTab = window.open('about:blank');
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Web/Statement.aspx",
                data: {
                    "action": "loadpdf",
                    "filename": filename,
                    "statementnum": statementnum
                },
                success: function (data) {
                    /* 写session成功 */
                    if (data == "ok") {
                        /* 再跳转 */
                        newTab.location.href = "/Web/StatementViewer.aspx";
                    }
                    else {
                        showAlert("Error", '<%= Resources.Error.ERR_PDF_LOAD %>' + data);
                    }
                },
                error: function (data) {
                    showAlert("Error", '<%= Resources.Error.ERR_NETWORK_ERR %>' + data);
                }
            });
        };


        /* 加载bootgird数据 */
        function search() {
            $("#grid-data").bootgrid({
                navigation: 2,
                rowCount: 10,
                ajax: true,
                url: "/Web/Statement.aspx",
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
                    "link": function (column, row) {
                        var filename = row.url;
                        var statementnum = row.statementnum;
                        return '<a href="JavaScript:void(0);" class="am-badge am-badge-primary am-radius" onclick="viewPdf(\'' + filename + '\',\'' + statementnum + '\')">' + '<%= Resources.Lang.Res_Statement_ButtonDetail %>' + '</a>';

                    },
                    "statementdate": function (column, row) {
                        return formatDate(row.statementdate);
                    },
                    "paymentduedate": function (column, row) {
                        return formatDate(row.paymentduedate);
                    },
                    "statementamount": function (column, row) {
                        return formatMoney(row.statementamount, 2, "");
                    }
                }
            });
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%--bootgrid--%>
    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid"
        data-toggle="bootgrid">
        <thead>
            <tr>
                <th data-column-id="statementnum" data-identifier="true" data-align="center" data-header-align="center">
                    <%= Resources.Lang.Res_Statement_StatementNumber%>
                </th>
                <th data-column-id="statementdate" data-formatter="statementdate" data-align="center"
                    data-header-align="center">
                    <%= Resources.Lang.Res_Statement_StatementDate%>
                </th>
                <th data-column-id="paymentduedate" data-formatter="paymentduedate" data-align="center"
                    data-header-align="center">
                    <%= Resources.Lang.Res_Statement_PaymentDueDate%>
                </th>
                <th data-column-id="statementamount" data-formatter="statementamount" data-align="center"
                    data-header-align="center">
                    <%= Resources.Lang.Res_Statement_StatementAmount%>
                </th>
                <th data-column-id="url" data-visible="false">
                </th>
                <th data-column-id="link" data-formatter="link" data-header-css-class="boot-header-1"
                    data-sortable="false" data-align="center" data-header-align="center">
                    <%= Resources.Lang.Res_Statement_Detail%>
                </th>
            </tr>
        </thead>
    </table>
</asp:Content>
