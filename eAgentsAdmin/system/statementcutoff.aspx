<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true"
    CodeBehind="statementcutoff.aspx.cs" Inherits="PropertyOneAppWeb.system.statementcutoff" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var lang = '<% =Session["lang"] %>';
        $(document).ready(function () {
            changeNav("../Image/icon_Notices.png", "Statement Date Setting");
            EnquiryStatementDate();
            LoadProperty();
        });
        
        /* 查询所有Statement date */
        var firstPageload = true; /* 是否第一次加载页面 */
        function EnquiryStatementDate() {
            if (firstPageload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-data").bootgrid({
                    navigation: 2,
                    rowCount: 10,
                    columnSelection: false,
                    ajax: true,
                    url: "./statementcutoff.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        return {
                            action: "enquirystatementdate"
                        };
                    },
                    formatters: {
                        "IS_EMAIL": function (column, row) {
                            var isEmail = row.IS_EMAIL;
                            if (isEmail == "Y") {
                                return '<input type="checkbox" checked="checked" disabled="disabled" />';
                            }
                            else {
                                return '<input type="checkbox" disabled="disabled" />';
                            }
                        },
                        "edit": function (column, row) {
                            var id = row.STATEMENT_DATE_ID;
                            var date = row.STATEMENT_DATE;
                            var isMail = row.IS_EMAIL;
                            var anchors = '<a class="am-badge am-badge-success am-radius" href="JavaScript:void(0);" onclick="OpenExceptionList(\'' + id + '\',\'' + date + '\')">Exception List</a> '
                            anchors += '<a class="am-badge am-badge-primary am-radius" href="JavaScript:void(0);" onclick="OpenModelEdit(\'' + id + '\',\'' + date + '\',\'' + isMail + '\')">Edit</a>'
                            anchors += ' <a class="am-badge am-badge-danger am-radius" href="JavaScript:void(0);" onclick="OpenModelDel(\'' + id + '\',\'' + date + '\')">Delete</a>';
                            anchors += ' <a class="am-badge am-badge-secondary am-radius" href="JavaScript:void(0);" onclick="SelectProperty(\'' + id + '\',\'' + date + '\')">Property</a>';
                            return anchors;
                        },
                        "fmtDate": function (col, row) {
                            return df(row.STATEMENT_DATE, "yyyy-mm-dd");
                        }

                    }
                });
                firstPageload = false;
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");
            };
        };

        function LoadProperty() {
            $("#grid-property").bootgrid({
                navigation: 0,
                rowCount: -1,
                selection: true,
                multiSelect: true,
                rowSelect: true,
                ajax: true,
                url: "./statementcutoff.aspx",
                sorting: false,
                ajaxSettings: {
                    cache: false
                },
                post: function () {
                    return {
                        action: "loadproperties"
                    };
                }
            });
        }

        var sid;
        /* 点击一个条目的 property */
        function SelectProperty(id, date) {
            $("#grid-property").bootgrid("deselect");

            /* 显示下方区域 */
            $("#div-select-property").show();
            sid = id;
            $("#lastEditDate").text(" for statement date:" + date);
            $.ajax({
                cache: false,
                type: "POST",
                url: "./statementcutoff.aspx",
                data: {
                    action: "getproperties",
                    sid: sid
                },
                dataType: "json",
                success: function (data) {
                    if (data.err_code != 0) {
                        showAlert("Notice", data.err_msg);
                    } else {
                        /* 根据返回结果，填充复选框 */
                        var arrprops = new Array();
                        for (var i = 0; i < data.result.length; i++) {
                            arrprops.push(data.result[i].PROPERTY_CODE);
                        }
                        $("#grid-property").bootgrid("select", arrprops);                        
                    }
                },
                error: function (data) {
                    showAlert("Notice", data.err_msg);
                }
            });
        }

        /* 保存所选 */
        function SaveProperties() {
            var arrayproperty = $("#grid-property").bootgrid("getSelectedRows");
            $.ajax({
                cache: false,
                type: "POST",
                url: "./statementcutoff.aspx",
                data: {
                    action: "saveproperties",
                    saveinfo: arrayproperty.join(),
                    sid: sid
                },
                success: function (data) {                 
                    if (data == "ok") {
                        showAlertRefresh("Notice", "Save Success");
                    }
                    else {
                        showAlert("Notice", data);
                    }
                },
                error: function (data) {
                    showAlert("Notice", "Save Error");
                }
            });
        }

        /* 保存新增Statement date */
        function SaveNewStatementDate() {
            var statementDate = $('#datapicker-new').val();
            var isSendMail;
            if ($("#checkboxSendMail").prop("checked")) {
                isSendMail = "Y";
            }
            else {
                isSendMail = "N";
            }

            $.ajax({
                cache: false,
                type: "POST",
                url: "./statementcutoff.aspx",
                data: {
                    "action": "newstatementdate",
                    "statementdate": statementDate,
                    "issendmail": isSendMail
                },
                success: function (data) {
                    if (checknotnull(data)) {
                        if (data == "ok") {
                            closeModal("doc-modal-add");
                            EnquiryStatementDate();
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

        /* 更新Statement date */
        function EditStatementDate() {
            var id = $("#label-edit-id").text();
            var isSendMail;
            if ($("#checkbox-edit-statement-date").prop("checked")) {
                isSendMail = "Y";
            }
            else {
                isSendMail = "N";
            }

            $.ajax({
                cache: false,
                type: "POST",
                url: "./statementcutoff.aspx",
                data: {
                    "action": "updatestatementdate",
                    "id": id,
                    "ismail": isSendMail
                },
                success: function (data) {
                    if (checknotnull(data)) {
                        if (data == "ok") {
                            closeModal("doc-modal-edit");
                            EnquiryStatementDate();
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

        /* 删除Statement date */
        function DelStatementDate() {
            var id = $("#label-del-id").text();
            $.ajax({
                cache: false,
                type: "POST",
                url: "./statementcutoff.aspx",
                data: {
                    "action": "delstatementdate",
                    "id": id
                },
                success: function (data) {
                    if (checknotnull(data)) {
                        if (data == "ok") {
                            closeModal("doc-modal-del");
                            EnquiryStatementDate();
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

        /* 打开新增Statement date窗口 */
        function OpenModelNewStatementDate() {
            ResetModelNewStatementDate();
            showModal("doc-modal-add", 600, false);
        };

        /* 打开编辑Statement date窗口 */
        function OpenModelEdit(id, date, checked) {
            $("#label-edit-id").text(id);
            $("#input-edit-statement-date").val(df(date, "yyyy-mm-dd"));
            if (checked == "Y") {
                $("#checkbox-edit-statement-date").prop("checked", true);
            }
            else {
                $("#checkbox-edit-statement-date").prop("checked", false);
            }
            showModal("doc-modal-edit", 600, false);
        };

        /* 打开删除Statement date窗口 */
        function OpenModelDel(id, date) {
            $("#label-del-id").text(id);
            $("#label-del-date").text(df(date, "yyyy-mm-dd"));
            showModal("doc-modal-del", 600, false);
        };

        /* 打开Exception List页面 */
        function OpenExceptionList(id, date) {
            /* 先打开新窗口 */
            var newTab = window.open('about:blank');
            $.ajax({
                cache: false,
                type: "POST",
                url: "./statementcutoff.aspx",
                data: {
                    "action": "openexceptionlist",
                    "id": id,
                    "date": date
                },
                success: function (data) {
                    if (checknotnull(data)) {
                        if (data == "ok") {
                            /* 再跳转 */
                            newTab.location.href = "./exceptionlist.aspx";
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

        /* 重置新增Statement date窗口 */
        function ResetModelNewStatementDate() {
            if (lang == "en-US") {
                $('#datapicker-new').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });
            } else {
                $('#datapicker-new').datepicker({ format: 'yyyy-mm-dd' });
            }

            $('#datapicker-new').datepicker({ format: 'yyyy-mm-dd' });
            $('#datapicker-new').datepicker('setValue', '');
            $("#checkboxSendMail").prop("checked", false);
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
                    Statement Date Setting
                </p>
            </div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">
                    <table style="width: 100%;">
                        <tr>
                            <td>
                                <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="OpenModelNewStatementDate()"
                                    style="float: right">
                                    New statement date</button>
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
                                <th data-column-id="STATEMENT_DATE_ID" data-identifier="true" data-visible="false"
                                    data-type="numeric">
                                    STATEMENT_DATE_ID
                                </th>
                                <th data-column-id="STATEMENT_DATE" data-header-css-class="boot-c-property" data-align="center"
                                    data-header-align="center" data-formatter="fmtDate">
                                    Statement Date
                                </th>
                                <th data-column-id="IS_EMAIL" data-formatter="IS_EMAIL" data-header-css-class="boot-c-property"
                                    data-align="center" data-header-align="center">
                                    IS_EMAIL
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
            <div class="am-u-sm-12" style="height: 30px;">
            </div>
            <%-- 选择 Property --%>
            <div id="div-select-property"  class="am-u-sm-12" hidden="hidden">
                <%-- 深色标题 --%>
                <div class="am-u-sm-12 global_table_head global_color_navyblue">
                    <p>
                        Select Property &nbsp; <span id="lastEditDate"> </span>
                    </p>
                </div>
                <%-- 内容 --%>
                <div class="am-u-sm-12 global_table_body global_color_lightblue">
                    <%-- 边距内内容 --%>
                    <div class="am-u-sm-12">
                        <table style="width: 100%">
                            <tr>
                                <td>
                                </td>
                                <td>
                                </td>
                                <td>
                                    <button id="button-save-new-user" type="button" hidden="hidden" class="am-btn am-btn-secondary am-radius button-all"
                                        onclick="SaveProperties()" style="float: right;">
                                        Save</button>                                    
                                </td>
                            </tr>
                            <tr style="height: 20px;">
                            </tr>
                            <tr id="property">
                                <td style="vertical-align: top; text-align: right; padding-right: 20px;">
                                    Property:
                                </td>
                                <td colspan="4">
                                    <%--bootgrid--%>
                                    <table id="grid-property" class="table table-condensed table-hover table-striped global-bootgrid" data-toggle="bootgrid">
                                        <thead>
                                            <tr>                                                                               
                                                <th data-column-id="PROPERTY_CODE" data-identifier="true"  data-align="center" data-header-align="center" >
                                                    Property Code
                                                </th>                                              
                                                <th data-column-id="PROPERTY_NAME" data-align="center" data-header-align="center" >
                                                    Property Name
                                                </th>
                                            </tr>
                                        </thead>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <%-- 新增Statement date模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="doc-modal-add">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                New statement date<a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>
                    &times;</a>
            </div>
            <div class="am-modal-bd">
                <div style="padding: 20px 50px 20px 50px;">
                    <table style="width: 100%;">
                        <tr>
                            <td style="width: 300px; text-align: left;">
                                <label>
                                    Statement date</label>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <input id="datapicker-new" type="text" class="am-form-field" placeholder="DD-MM-YYYY"   readonly />
                            </td>
                            <td style="text-align: right;">
                                <label>
                                    Send mail
                                    <input id="checkboxSendMail" type="checkbox" /></label>
                            </td>
                        </tr>
                        <tr style="height: 20px;">
                        </tr>
                        <tr>
                            <td>
                            </td>
                            <td>
                                <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="SaveNewStatementDate()"
                                    style="float: right">
                                    Save</button>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <%-- 编辑Statement date模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="doc-modal-edit">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                Edit statement date<a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>
                    &times;</a>
            </div>
            <div class="am-modal-bd">
                <div style="padding: 20px 50px 20px 50px;">
                    <table style="width: 100%;">
                        <tr>
                            <td style="width: 300px; text-align: left;">
                                <label>
                                    Statement date</label>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <input id="input-edit-statement-date" type="text" class="am-form-field" readonly />
                                <label id="label-edit-id" hidden="hidden">
                                </label>
                            </td>
                            <td style="text-align: right;">
                                <label>
                                    Send mail
                                    <input id="checkbox-edit-statement-date" type="checkbox" /></label>
                            </td>
                        </tr>
                        <tr style="height: 20px;">
                        </tr>
                        <tr>
                            <td>
                            </td>
                            <td>
                                <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="EditStatementDate()"
                                    style="float: right">
                                    Save</button>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <%-- 删除Statement date模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="doc-modal-del">
        <div class="am-modal-dialog">
            <div class="am-modal-hd">
                Delete statement date ?<a href="javascript: void(0)" class="am-close am-close-spin"
                    data-am-modal-close> &times;</a>
            </div>
            <div class="am-modal-bd">
                <label id="label-del-id" hidden="hidden">
                </label>
                <label id="label-del-date">
                </label>
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="DelStatementDate()"
                                style="float: right">
                                Ok</button>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
