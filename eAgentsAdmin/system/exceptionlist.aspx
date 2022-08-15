<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true"
    CodeBehind="exceptionlist.aspx.cs" Inherits="PropertyOneAppWeb.system.exceptionlist" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("../Image/icon_Notices.png", "Statement date exception list");
            EnquiryExceptionList();

            var statementDate = '<% =Session["statementdate"]%>';
            $("#p_title").text("Statement Date: " + statementDate);
        });

        /* 查询Exception List */
        var firstPageload = true; /* 是否第一次加载页面 */
        function EnquiryExceptionList() {
            if (firstPageload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-data").bootgrid({
                    navigation: 2,
                    rowCount: 10,
                    selection: true,
                    multiSelect: true,
                    rowSelect: true,
                    ajax: true,
                    url: "./exceptionlist.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        return {
                            action: "enquiryexceptionlist"
                        };
                    },
                    formatters: {
                        "edit": function (column, row) {
                            var lease = row.LEASE_NUMBER;
                            return '<a class="am-badge am-badge-danger am-radius" href="JavaScript:void(0);" onclick="OpenModelDel(\'' + lease + '\')">Delete</a>';
                        }
                    }
                });
                firstPageload = false;
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");
            };
        };

        /* 保存新增的Exception List */
        function SaveNewException() {
            var lease = $("#input-lease").val();
            $.ajax({
                cache: false,
                type: "POST",
                url: "./exceptionlist.aspx",
                data: {
                    "action": "newexceptionlist",
                    "lease": lease
                },
                success: function (data) {
                    if (checknotnull(data)) {
                        if (data == "ok") {
                            closeModal("doc-modal-add");
                            EnquiryExceptionList();
                        }
                        else {
                            alert(data);
                        }
                    }
                },
                error: function (data) {
                    alert(data);
                }
            });
        };

        /* 删除Exception List */
        function DelException() {
            var lease = $("#label-del-lease").text();
            $.ajax({
                cache: false,
                type: "POST",
                url: "./exceptionlist.aspx",
                data: {
                    "action": "delexceptionlist",
                    "lease": lease
                },
                success: function (data) {
                    if (checknotnull(data)) {
                        if (data == "ok") {
                            closeModal("model-del-confirm");
                            EnquiryExceptionList();
                        }
                        else {
                            alert(data);
                        }
                    }
                },
                error: function (data) {
                    alert(data);
                }
            });
        };

        /* 删除选中的Exception List */
        function DelSelected() {
            var arraySelectedLease = $("#grid-data").bootgrid("getSelectedRows");
            $.ajax({
                cache: false,
                type: "POST",
                url: "./exceptionlist.aspx",
                data: {
                    "action": "delselectedlease",
                    "lease": arraySelectedLease
                },
                success: function (data) {
                    if (checknotnull(data)) {
                        if (data == "ok") {
                            closeModal("model-del-confirm");
                            EnquiryExceptionList();
                        }
                        else {
                            showAlert("Notice", data);
                        }
                    }
                },
                error: function (data) {
                    showAlert("Notice", data);
                }
            });
        };

        /* 打开新增Exception List窗口 */
        function OpenModelNewException() {
            $("#input-lease").val("");
            showModal("doc-modal-add", 600, false);
        };

        /* 打开删除确认窗口 */
        function OpenModelDel(lease) {
            $("#label-del-lease").text(lease);
            showModal("model-del-confirm", 600, false);
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- 外边框 --%>
    <div class="am-u-sm-12">
        <%-- 深色标题 --%>
        <div class="am-u-sm-12 global_table_head global_color_navyblue">
            <p id="p_title">
                Statement date exception list
            </p>
        </div>
        <%-- 内容 --%>
        <div class="am-u-sm-12 global_table_body global_color_lightblue">
            <%-- 边距内内容 --%>
            <div class="am-u-sm-12">
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="OpenModelNewException()"
                                style="float: right">
                                Add lease number</button>
                        </td>
                        <td style="width: 150px;">
                            <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="DelSelected()"
                                style="float: right">
                                Delete selected</button>
                        </td>
                    </tr>
                    <tr style="height: 20px;">
                    </tr>
                </table>
                <%--bootgrid--%>
                <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid"
                    data-toggle="bootgrid">
                    <thead>
                        <tr>
                            <th data-column-id="STATEMENT_DATE_ID" data-visible="false" data-type="numeric">
                                STATEMENT_DATE_ID
                            </th>
                            <th data-column-id="STATEMENT_DATE" data-visible="false">
                                STATEMENT_DATE
                            </th>
                            <th data-column-id="LEASE_NUMBER" data-identifier="true" data-header-css-class="boot-c-property"
                                data-align="center" data-header-align="center">
                                Lease Number
                            </th>
                            <th data-column-id="edit" data-formatter="edit" data-header-css-class="boot-c-edit"
                                data-align="center" data-header-align="center">
                                Edit
                            </th>
                        </tr>
                    </thead>
                </table>
            </div>
        </div>
    </div>
    <%-- 新增Exception List模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="doc-modal-add">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                New Exception Lease Number<a href="javascript: void(0)" class="am-close am-close-spin"
                    data-am-modal-close> &times;</a>
            </div>
            <div class="am-modal-bd">
                <div style="padding: 20px 100px 20px 100px;">
                    <table style="width: 100%;">
                        <tr>
                            <td style="text-align: left;">
                                <label>
                                    Lease number</label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <input id="input-lease" type="text" class="am-form-field" maxlength="30" />
                            </td>
                        </tr>
                        <tr style="height: 20px;">
                        </tr>
                        <tr>
                            <td>
                                <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="SaveNewException()"
                                    style="float: right">
                                    Save</button>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <%-- 确认删除模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="model-del-confirm">
        <div class="am-modal-dialog">
            <div class="am-modal-hd">
                Delete exception lease number ?<a href="javascript: void(0)" class="am-close am-close-spin"
                    data-am-modal-close> &times;</a>
            </div>
            <div class="am-modal-bd">
                <label id="label-del-lease">
                </label>
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="DelException()"
                                style="float: right">
                                Delete</button>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
