<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true"
    CodeBehind="UserMaintenance.aspx.cs" Inherits="PropertyOneAppWeb.system.UserMaintenance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/ecmascript">
        $(document).ready(function () {
            changeNav("../Image/icon_Notices.png", '<%= Resources.Lang.Res_User_Maintenance%>');
            search();
            SearchGroup();
        });

        var firstPageload = true; /* 是否第一次加载页面 */
        function search() {
            if (firstPageload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-data").bootgrid({
                    navigation: 3,
                    rowCount: 10,
                    columnSelection: false,
                    ajax: true,
                    url: "./UserMaintenance.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    searchSettings: {
                        delay: 100,
                        characters: 3
                    },
                    post: function () {
                        return {
                            action: "search"
                        };
                    },
                    formatters: {
                        "edit": function (column, row) {
                            var userid = row.USERID;
                            var loginName = row.LOGINNAME;
                            return '<a class="am-badge am-badge-primary am-radius" href="JavaScript:void(0);" onclick="EditUser(\'' + userid + '\')">Edit</a> <a class="am-badge am-badge-danger am-radius" href="JavaScript:void(0);" onclick="OpenModelDel(\'' + userid + '\', \'' + loginName + '\')">Delete</a>';
                        }
                    }
                });
                firstPageload = false;
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");
            };
        };

        var firstGroupload = true; /* 是否第一次加载Group页面 */
        function SearchGroup() {
            if (firstGroupload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-group").bootgrid({
                    navigation: 0,
                    rowCount: -1,
                    selection: true,
                    multiSelect: true,
                    rowSelect: true,
                    ajax: true,
                    url: "./UserMaintenance.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        return {
                            action: "searchgroup"
                        };
                    }
                });
                firstGroupload = false;
            } else { /* 如果不是第一次 */
                $("#grid-group").bootgrid("reload");
            };
        };

        /* 保存新用户 */
        function SaveNewUser() {
            try {
                var loginname = $("#input-loginname").val();
                var status = $("#select-status").val();
                var email = $("#input-email").val();
                var phone = $("#input-phone").val();
                var arrayGroup = $("#grid-group").bootgrid("getSelectedRows");
                var sendstr = loginname + "|" + status + "|" + email + "|" + phone + "|" + arrayGroup;

                if (checknotnull(loginname) == false) {
                    throw "Please input user name";
                }

                if (checknotnull(status) == false) {
                    throw "Please select status";
                }

                if (checknotnull(email) == false) {
                    throw "Please input email address";
                }

                if (checknotnull(phone) == false) {
                    throw "Please input phone number";
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./UserMaintenance.aspx",
                    data: {
                        "action": "savenewuser",
                        "saveinfo": sendstr
                    },
                    success: function (data) {
                        if (checknotnull(data) && data == "ok") {
                            search();

                            /* 隐藏下方区域 */
                            $("#div-adduser").hide();

                            showAlert("Notice", "Save success. Your password has been sent to your email address.");
                        }
                        else {
                            showAlert("Error", data);
                        }
                    },
                    error: function (data) {
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            catch (ex) {
                showAlert("Error", ex);
            }
        };

        /* 创建新用户 */
        function NewUser() {
            /* 变更title文字 */
            $("#p-title").text("New User");

            /* 可以编辑login name*/
            $("#input-loginname").prop("disabled", false);

            /* 清空input内容 */
            $("#input-userid").val("");
            $("#input-loginname").val("");
            $("#input-email").val("");
            $("#input-phone").val("");

            /* 重置Select */
            $("#select-status").get(0).selectedIndex = 0;
            /* 不支持 MutationObserver 的浏览器使用 JS 操作 select 以后需要手动触发 `changed.selected.amui` 事件 */
            $("#select-status").trigger('changed.selected.amui');

            /* 清空复选框 */
            $("#grid-group").bootgrid("deselect");

            /* 显示下方区域 */
            $("#div-adduser").show();

            /* 只显示编辑保存按钮 */
            $("#button-save-edit-user").hide();
            $("#button-save-new-user").show();
        };

        /* 编辑用户 */
        function EditUser(userid) {
            /* 清空复选框 */
            $("#grid-group").bootgrid("deselect");

            /* 查找数据 */
            $.ajax({
                cache: false,
                type: "POST",
                url: "./UserMaintenance.aspx",
                data: {
                    "action": "searchoneuser",
                    "userid": userid
                },
                success: function (data) {
                    if (checknotnull(data)) {
                        try {
                            var jsonData = jQuery.parseJSON(data);
                            var userid = jsonData.userid;
                            var loginname = jsonData.loginname;
                            var email = jsonData.email;
                            var phone = jsonData.phone;
                            var status = jsonData.status;
                            var groupinfo = jsonData.groupinfo;

                            $("#input-userid").val(userid);
                            $("#input-loginname").val(loginname);
                            $("#input-email").val(email);
                            $("#input-phone").val(phone);

                            /* 设置下拉框 */
                            if (status == "A") {
                                $("#select-status").get(0).selectedIndex = 0;
                            }
                            else {
                                $("#select-status").get(0).selectedIndex = 1;
                            }
                            /* 不支持 MutationObserver 的浏览器使用 JS 操作 select 以后需要手动触发 `changed.selected.amui` 事件 */
                            $("#select-status").trigger('changed.selected.amui');

                            var arrayGroupInfo = new Array();
                            /* 遍历Group Info数组 */
                            groupinfo.forEach(function (value, index, arr) {
                                arrayGroupInfo.push(value.groupid);
                            });

                            /* 根据返回结果，填充复选框 */
                            $("#grid-group").bootgrid("select", arrayGroupInfo);

                            /* 不能修改login name */
                            $("#input-loginname").prop("disabled", true);

                            /* 变更title文字 */
                            $("#p-title").text("Edit User - " + loginname);

                            /* 显示下方区域 */
                            $("#div-adduser").show();

                            /* 只显示编辑保存按钮 */
                            $("#button-save-edit-user").show();
                            $("#button-save-new-user").hide();
                        }
                        catch (ex) {
                            showAlert("Notice", ex.message);
                        }
                    }
                    else {
                        showAlert("Notice", "Load Error");
                    }
                },
                error: function (data) {
                    AlertJqueryAjaxError(data);
                }
            });
        };

        /* 保存编辑后的用户 */
        function SaveEditUser() {
            try {
                var userid = $("#input-userid").val();
                var loginname = $("#input-loginname").val();
                var status = $("#select-status").val();
                var email = $("#input-email").val();
                var phone = $("#input-phone").val();
                var arrayGroup = $("#grid-group").bootgrid("getSelectedRows");
                var sendstr = userid + "|" + loginname + "|" + status + "|" + email + "|" + phone + "|" + arrayGroup;

                if (checknotnull(userid) == false) {
                    throw "User ID can not be null";
                }

                if (checknotnull(loginname) == false) {
                    throw "Please input user name";
                }

                if (checknotnull(status) == false) {
                    throw "Please select status";
                }

                if (checknotnull(email) == false) {
                    throw "Please input email address";
                }

                if (checknotnull(phone) == false) {
                    throw "Please input phone number";
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./UserMaintenance.aspx",
                    data: {
                        "action": "saveedituser",
                        "saveinfo": sendstr
                    },
                    success: function (data) {
                        if (checknotnull(data) && data == "ok") {
                            search();

                            /* 隐藏下方区域 */
                            $("#div-adduser").hide();

                            showAlert("Notice", "Save success");
                        }
                        else {
                            showAlert("Error", data);
                        }
                    },
                    error: function (data) {
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            catch (ex) {
                showAlert("Error", ex);
            }
        };

        /* 删除用户 */
        function DelUser() {
            try {
                var userid = $("#label-del-userid").text();
                var loginName = $("#label-del-loginname").text();
                var currentUser = '<% =Session["loginname"]%>';

                if (checknotnull(loginName) == false) {
                    throw "Login name is null";
                }

                if (checknotnull(userid) == false) {
                    throw "User id is null";
                }

                if (checknotnull(currentUser) == false) {
                    throw "Current user is null";
                }

                if (currentUser == loginName) {
                    throw "Cannot delete yourself";
                }

                if (loginName == "admin") {
                    throw "Cannot delete Admin";
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./UserMaintenance.aspx",
                    data: {
                        "action": "deluser",
                        "userid": userid
                    },
                    success: function (data) {
                        if (checknotnull(data) && data == "ok") {
                            closeModal("model-del-confirm");
                            search();

                        }
                        else {
                            closeModal("model-del-confirm");
                            showAlert("Error", data);
                        }
                    },
                    error: function (data) {
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            catch (ex) {
                closeModal("model-del-confirm");
                showAlert("Error", ex);
            }
            finally {
                /* 隐藏下方区域 */
                $("#div-adduser").hide();
            }
        }

        /* 打开删除确认窗口 */
        function OpenModelDel(userid, loginName) {
            $("#label-del-loginname").text(loginName);
            $("#label-del-userid").text(userid);
            showModal("model-del-confirm", 600, false);
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="am-g am-g-collapse">
        <%-- 外边框 --%>
        <div class="am-u-sm-12">
            <%-- 深色标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p><%= Resources.Lang.Res_User_Maintenance%></p>
            </div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">
                    <div class="am-u-sm-12">
                        <button id="button-add" type="button" class="am-btn am-btn-secondary am-radius" onclick="NewUser()"
                            style="float: right">
                            <%= Resources.Lang.Res_New_User%></button>
                    </div>
                    <div class="am-u-sm-12" style="height: 20px;">
                    </div>
                    <%--bootgrid--%>
                    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid"
                        data-toggle="bootgrid">
                        <thead>
                            <tr>
                                <th data-column-id="USERID" data-visible="false" data-type="numeric" data-identifier="true"
                                    data-align="center" data-header-align="center">
                                    User Id
                                </th>
                                <th data-column-id="LOGINNAME" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Login_Name%>
                                </th>
                                <th data-column-id="EMAIL" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Email%>
                                </th>
                                <th data-column-id="PHONE" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Phone%> 
                                </th>
                                <th data-column-id="STATUS" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Status%>
                                </th>
                                <th data-column-id="EDIT" data-formatter="edit" data-header-css-class="boot-header-1"
                                    data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Edit%>
                                </th>
                            </tr>
                        </thead>
                    </table>
                </div>
            </div>
            <div class="am-u-sm-12" style="height: 30px;">
            </div>
            <%-- 新增User --%>
            <div id="div-adduser" class="am-u-sm-12" hidden="hidden">
                <%-- 深色标题 --%>
                <div class="am-u-sm-12 global_table_head global_color_navyblue">
                    <p id="p-title">
                       <%= Resources.Lang.Res_New_User%>
                    </p>
                </div>
                <%-- 内容 --%>
                <div class="am-u-sm-12 global_table_body global_color_lightblue">
                    <%-- 边距内内容 --%>
                    <div class="am-u-sm-12">
                        <table style="width: 100%">
                            <tr>
                                <th style="width: 15%;">
                                </th>
                                <th style="width: 34%;">
                                </th>
                                <th style="width: 2%;">
                                </th>
                                <th style="width: 15%;">
                                </th>
                                <th style="width: 34%;">
                                </th>
                            </tr>
                            <tr>
                                <td>
                                </td>
                                <td>
                                </td>
                                <td>
                                </td>
                                <td>
                                </td>
                                <td>
                                    <button id="button-save-new-user" type="button" hidden="hidden" class="am-btn am-btn-secondary am-radius button-all"
                                        onclick="SaveNewUser()" style="float: right;">
                                        <%= Resources.Lang.Res_Save_New_User%></button>
                                    <button id="button-save-edit-user" type="button" hidden="hidden" class="am-btn am-btn-secondary am-radius button-all"
                                        onclick="SaveEditUser()" style="float: right;">
                                        <%= Resources.Lang.Res_Save_Edit_User%></button>
                                </td>
                            </tr>
                            <tr style="height: 20px;">
                            </tr>
                            <%-- 用来保存User Id --%>
                            <tr hidden="hidden">
                                <td>
                                    User Id:
                                </td>
                                <td>
                                    <input id="input-userid" type="text" class="am-form-field" maxlength="20" />
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Login_Name%>:
                                </td>
                                <td>
                                    <input id="input-loginname" type="text" class="am-form-field" maxlength="20" />
                                </td>
                                <td>
                                </td>
                                <td style="text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Status%>:
                                </td>
                                <td>
                                    <select id="select-status" data-am-selected="{btnWidth: '100%', btnSize: 'xl'}">
                                        <option value="A" selected>Valid</option>
                                        <option value="I">Invalid</option>
                                    </select>
                                </td>
                            </tr>
                            <tr style="height: 20px;">
                            </tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Email%>:
                                </td>
                                <td>
                                    <input id="input-email" type="text" class="am-form-field" maxlength="50" />
                                </td>
                                <td>
                                </td>
                                <td style="text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Phone%>:
                                </td>
                                <td>
                                    <input id="input-phone" type="text" class="am-form-field" maxlength="20" />
                                </td>
                            </tr>
                            <tr style="height: 20px;">
                            </tr>
                            <tr>
                                <td style="vertical-align: top; text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_User_Group%>:
                                </td>
                                <td colspan="4">
                                    <%--bootgrid--%>
                                    <table id="grid-group" class="table table-condensed table-hover table-striped global-bootgrid"
                                        data-toggle="bootgrid">
                                        <thead>
                                            <tr>
                                                <th data-column-id="GROUPID" data-type="numeric" data-visible="false" data-identifier="true"
                                                    data-align="center" data-header-align="center">
                                                    Group Id
                                                </th>
                                                <th data-column-id="GROUPNAME" data-align="center" data-header-align="center">
                                                    <%= Resources.Lang.Res_Select_Group%>
                                                </th>
                                            </tr>
                                        </thead>
                                    </table>
                                </td>
                            </tr>
                            <tr style="height: 50px;">
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <%-- 确认删除模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="model-del-confirm">
        <div class="am-modal-dialog">
            <div class="am-modal-hd">
                Delete user ?<a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>
                    &times;</a>
            </div>
            <div class="am-modal-bd">
                <label id="label-del-loginname">
                </label>
                <label id="label-del-userid" hidden="hidden">
                </label>
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="DelUser()"
                                style="float: right">
                                Delete</button>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
