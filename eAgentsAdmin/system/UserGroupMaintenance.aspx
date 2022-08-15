<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true"
    CodeBehind="UserGroupMaintenance.aspx.cs" Inherits="PropertyOneAppWeb.system.UserGroupMaintenance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/ecmascript">
        var firstPageload = true; /* 是否第一次加载页面 */
        function search() {
            if (firstPageload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-data").bootgrid({
                    navigation: 3,
                    rowCount: 10,
                    columnSelection: false,
                    ajax: true,
                    url: "./UserGroupMaintenance.aspx",
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
                            var groupid = row.GROUPID;
                            var groupName = row.GROUPNAME;
                            //<a class="am-badge am-badge-danger am-radius" href="JavaScript:void(0);" onclick="OpenModelDel(\'' + userid + '\', \'' + loginName + '\')">Delete</a>'
                            return '<a class="am-badge am-badge-primary am-radius" href="JavaScript:void(0);" onclick="editgroup(' + groupid + ')">Edit</a>' + '<a class="am-badge am-badge-danger am-radius" href="JavaScript:void(0);" onclick="OpenModelDel(\'' + groupid + '\', \'' + groupName + '\')">Delete</a>';
                        },
                        "approver": function (column, row) {
                            if (row.APPROVER == "1")
                                return "Y";
                            else
                                return "N";
                        }
                    }
                });
                firstPageload = false;
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");
            };
        };

        var firstAuthload = true; /* 是否第一次加载Auth页面 */
        function searchauth() {
            if (firstAuthload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-auth").bootgrid({
                    navigation: 0,
                    rowCount: -1,
                    selection: true,
                    multiSelect: true,
                    rowSelect: true,
                    ajax: true,
                    url: "./UserGroupMaintenance.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        return {
                            action: "searchauth"
                        };
                    }
                });
                firstAuthload = false;
            } else { /* 如果不是第一次 */
                $("#grid-auth").bootgrid("reload");
            };
        };

        var firstPropertyload = true; /* 是否第一次加载Property页面 */
        function searchproperty() {
            if (firstPropertyload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-property").bootgrid({
                    navigation: 0,
                    rowCount: -1,
                    selection: true,
                    multiSelect: true,
                    rowSelect: true,
                    ajax: true,
                    url: "./UserGroupMaintenance.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        return {
                            action: "searchproperty"
                        };
                    }
                });
                firstPropertyload = false;
            } else { /* 如果不是第一次 */
                $("#grid-property").bootgrid("reload");
            };
        };

        /* 显示New User窗口 */
        function showaddmodal() {
            /* 变更title文字 */
            $("#p-group-title").text("New Group");

            /* 清空input内容 */
            $("#input-add-groupid").val("");
            $("#input-add-groupname").val("");
            $("#input-approver").removeAttr("checked");

            /* 重置Select */           
            $("#select-status").get(0).selectedIndex = 0;
            /* 不支持 MutationObserver 的浏览器使用 JS 操作 select 以后需要手动触发 `changed.selected.amui` 事件 */
            $("#select-status").trigger('changed.selected.amui');

            $("#select-dept").get(0).selectedIndex = 0;
            $("#select-dept").trigger('changed.selected.amui');

            /* 清空复选框 */
            $("#grid-auth").bootgrid("deselect");
            $("#grid-property").bootgrid("deselect");

            /* 显示下方区域 */
            $("#div-addgroup").show();

            /* 只显示编辑保存按钮 */
            $("#button-save-edit-group").hide();
            $("#button-save-new-group").show();
        };

        /* 保存新用户组 */
        function savenewgroup() {
            if (SaveCheck() == true) {
                var groupname = $("#input-add-groupname").val();
                var status = $("#select-status").val();
                var arrayauth = $("#grid-auth").bootgrid("getSelectedRows");
                var arrayproperty = $("#grid-property").bootgrid("getSelectedRows");

                /// -- start -- #19493 新增字段
                var dept = $("#select-dept").val();
                var approver = $("#input-approver").get(0).checked;
                if (dept == "-1") {
                    showAlert("Notice", "Please select an department");
                    $("#select-dept").focus();
                    return;
                }
                /// --  end  --
                var sendstr = groupname + "|" + status + "|" + arrayauth + "|" + arrayproperty + "|" + dept + "|" + approver;

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./UserGroupMaintenance.aspx",
                    data: {
                        "action": "savenewgroup",
                        "saveinfo": sendstr
                    },
                    success: function (data) {
                        if (checknotnull(data) && data == "ok") {
                            closeModal("doc-modal-add");
                            showAlertRefresh("Notice", "Save Success");
                        }
                        else {
                            showAlert("Notice", "Save Error");
                        }
                    },
                    error: function (data) {
                        showAlert("Notice", "Save Error");
                    }
                });
            }
            else {
                showAlert("Notice", "Please Input Group Name");
            }
        };

        /* 保存编辑后的用户组 */
        function saveeditgroup() {
            var groupid = $("#input-add-groupid").val();
            var groupname = $("#input-add-groupname").val();
            var status = $("#select-status").val();
            var arrayauth = $("#grid-auth").bootgrid("getSelectedRows");
            var arrayproperty = $("#grid-property").bootgrid("getSelectedRows");

            /// -- start -- #19493 新增字段
            var dept = $("#select-dept").val();
            var approver = $("#input-approver").get(0).checked;
            if (dept=="-1") {
                showAlert("Notice", "Please select a department");
                $("#select-dept").focus();
                return;
            }        
            /// --  end  --
            var sendstr = groupid + "|" + groupname + "|" + status + "|" + arrayauth + "|" + arrayproperty + "|" + dept + "|" + approver;

            
            $.ajax({
                cache: false,
                type: "POST",
                url: "./UserGroupMaintenance.aspx",
                data: {
                    "action": "saveeditgroup",
                    "saveinfo": sendstr
                },
                success: function (data) {
                    if (checknotnull(data) && data == "ok") {
                        closeModal("doc-modal-add");
                        showAlertRefresh("Notice", "Save Success");
                    }
                    else {
                        showAlert("Notice", "Save Error");
                    }
                },
                error: function (data) {
                    showAlert("Notice", "Save Error");
                }
            });
        };

        /* 编辑用户组 */
        function editgroup(groupid) {
            /* 清空复选框 */
            $("#grid-auth").bootgrid("deselect");
            $("#grid-property").bootgrid("deselect");

            /* 查找数据 */
            $.ajax({
                cache: false,
                type: "POST",
                url: "./UserGroupMaintenance.aspx",
                data: {
                    "action": "searchonegroup",
                    "groupid": groupid
                },
                success: function (data) {
                    if (checknotnull(data)) {
                        try {
                            var jsonData = jQuery.parseJSON(data);
                            var groupid = jsonData.groupid;
                            var groupname = jsonData.groupname;
                            var status = jsonData.status;
                            var authinfo = jsonData.authinfo;
                            var propertyinfo = jsonData.propertyinfo;
                            /// -- start -- #19493 新增字段
                            var dept = jsonData.dept;
                            var approver = jsonData.approver;
                            /// --  end  --

                            $("#input-add-groupid").val(groupid);
                            $("#input-add-groupname").val(groupname);

                            /* 设置下拉框 */
                            if (status == "A") {
                                $("#select-status").get(0).selectedIndex = 0;
                            }
                            else {
                                $("#select-status").get(0).selectedIndex = 1;
                            }
                            /* 不支持 MutationObserver 的浏览器使用 JS 操作 select 以后需要手动触发 `changed.selected.amui` 事件 */
                            $("#select-status").trigger('changed.selected.amui');

                            $("#select-dept").val(dept); // 设置部门
                            $("#select-dept").trigger('changed.selected.amui');

                            if (approver == "1") {  //设置审批
                                $("#input-approver").get(0).checked = true;
                            } else {
                                $("#input-approver").get(0).checked = false;
                            }

                            var arrayAuthInfo = new Array();
                            /* 遍历Auth Info数组 */
                            authinfo.forEach(function (value, index, arr) {
                                arrayAuthInfo.push(value.authid);
                            });

                            /* 根据返回结果，填充复选框 */
                            $("#grid-auth").bootgrid("select", arrayAuthInfo);

                            var arrayPropertyInfo = new Array();
                            /* 遍历Property Info数组 */
                            propertyinfo.forEach(function (value, index, arr) {
                                arrayPropertyInfo.push(value.propertycode);
                            });

                            /* 根据返回结果，填充复选框 */
                            $("#grid-property").bootgrid("select", arrayPropertyInfo);

                            /* 变更title文字 */
                            $("#p-group-title").text("Edit Group - " + groupname);

                            /* 显示下方区域 */
                            $("#div-addgroup").show();

                            /* 只显示编辑保存按钮 */
                            $("#button-save-edit-group").show();
                            $("#button-save-new-group").hide();
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
                    showAlert("Notice", "Load Error");
                }
            });

            $("#grid-users").bootgrid({
                navigation: 0,
                rowCount: 10,
                columnSelection: false,
                ajax: true,
                ajaxSettings: {
                    method: "post",
                    cache: false
                },
                url: "./UserGroupMaintenance.aspx",
                sorting: false,
                searchSettings: {
                    delay: 100,
                    characters: 3
                },
                post: function () {
                    return {
                        action: "getusersingrp",
                        grpid: groupid
                    };
                }
            }).bootgrid("search", groupid);
        };

        /* 保存前校验 */
        function SaveCheck() {
            try {
                var groupname = $("#input-add-groupname").val();
                if (checknotnull(groupname)) {
                    return true;
                }
                return false;
            } catch (ex) {
                return false;
            }
        };

        /* 打开删除确认窗口 */
        function OpenModelDel(groupid, groupName) {
            $("#label-del-groupname").text(groupName);
            $("#label-del-groupid").text(groupid);

            /// 1.检查该groupid下是否有用户存在 checkgroup
            httpost("./UserGroupMaintenance.aspx"
            , { action: "checkgroup", groupid: $("#label-del-groupid").text() }
            , true
            , "json"
            , function (rsp) {
                if (rsp.err_code != 0) {
                    showAlert("Error", rsp.err_msg);
                } else {
                    /// 2.执行删除 delgroup          
                    showModal("model-del-confirm", 600, false);
                }
            });

        };

        function DelGroup() {
            var param = { action: "delgroup", groupid: $("#label-del-groupid").text() }
            httpost("./UserGroupMaintenance.aspx"
            , param
            , true
            , "json"
            , function (rsp) {
                closeModal("model-del-confirm");
                if (rsp.err_code != 0) {
                    showAlert("Error", rsp.err_msg);
                } else {
                    search();
                }
            });
        }

        function httpost(url, paras, asy, type, callback) {
            $.ajax({
                url: url,
                method: "POST",
                data: paras,
                dataType: type,
                async: asy,
                success: callback,
                error: callback
            });
        }

        $(document).ready(function () {
            changeNav("../Image/icon_Notices.png", '<%= Resources.Lang.Res_User_Group_Maintenance%>');
            search();
            searchauth();
            searchproperty();

            $("#grid-data").bootgrid().on("loaded.rs.jquery.bootgrid", function (e) {
                var $d = null;
                $("#grid-data tbody tr").find("td:first").hover(function (e) {                   
                    $d = $("<div class='pop' style='border:solid blue 1px;position:absolute;background-color:yellow'></div>");
                    $d.css("top", e.pageY + "px").css("left", e.target.clientWidth * 1.5 + "px");
                    $.ajax({
                        cache: false,
                        type: "POST",
                        dataType: "json",
                        url: "./UserGroupMaintenance.aspx",
                        data: {
                            "action": "getusersingrp",
                            "grpname": $(this).text()
                        },
                        success: function (res) {
                            if (res.err_code == 0) {
                                if (res.result.length > 0) {
                                    for (var i = 0; i < res.result.length; i++) {
                                        var $p = $("<p style='margin:0;padding:1px'></p>");
                                        $p.text(res.result[i].LoginName);
                                        $d.append($p);
                                    }
                                } else {
                                    var $p = $("<p style='margin:0;padding:1px'></p>");
                                    $p.text("[no users exist.]");
                                    $d.append($p);
                                }
                                $d.appendTo("body");
                            }
                        }
                    });
                }, function () {
                    $d.remove();
                    $(".pop").remove();
                });
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="am-g am-g-collapse">
        <%-- 外边框 --%>
        <div class="am-u-sm-12">
            <%-- 深色标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue"><p><%= Resources.Lang.Res_User_Group_Maintenance%></p></div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">
                    <%--bootgrid--%>
                    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid"
                        data-toggle="bootgrid">
                        <thead>
                            <tr>
                                <th data-column-id="GROUPID" data-visible="false" data-header-css-class="boot-header-1"
                                    data-align="center" data-header-align="center">
                                    Group Id
                                </th>
                                <th data-column-id="GROUPNAME" data-header-css-class="boot-header-1" data-align="center"
                                    data-header-align="center">
                                   <%= Resources.Lang.Res_Group_Name%>
                                </th>
                                <th data-column-id="STATUS" data-header-css-class="boot-header-1" data-align="center"
                                    data-header-align="center">
                                    <%= Resources.Lang.Res_Status%>
                                </th>
                                <th data-column-id="DEPT" data-header-css-class="boot-header-1" data-align="center"
                                    data-header-align="center"> 
                                    <%= Resources.Lang.Res_Department%>
                                </th>
                                <th data-column-id="APPROVER" data-formatter="approver" data-header-css-class="boot-header-1" data-align="center"
                                    data-header-align="center"> 
                                    <%= Resources.Lang.Res_Approver%>
                                </th>
                                <th data-column-id="EDIT" data-formatter="edit" data-header-css-class="boot-header-1"
                                    data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Edit%>
                                </th>
                            </tr>
                        </thead>                        
                    </table>
                </div>
                <div class="am-u-sm-12" style="height: 20px;">
                </div>
                <div class="am-u-sm-12">
                    <button id="button-add" type="button" class="am-btn am-btn-secondary am-round" onclick="showaddmodal()"
                        style="float: left">
                       <%= Resources.Lang.Res_New_Group%></button>
                </div>
            </div>
            <div class="am-u-sm-12" style="height: 30px;">
            </div>
            <%-- 新增User Group --%>
            <div id="div-addgroup" class="am-u-sm-12" hidden="hidden">
                <%-- 深色标题 --%>
                <div class="am-u-sm-12 global_table_head global_color_navyblue">
                    <p id="p-group-title">
                       <%= Resources.Lang.Res_New_Group%></button>
                    </p>
                </div>
                <%-- 内容 --%>
                <div class="am-u-sm-12 global_table_body global_color_lightblue">
                    <%-- 边距内内容 --%>
                    <div class="am-u-sm-12">
                        <table style="width: 100%">
                            <tr>
                                <th style="width: 150px;">
                                </th>
                                <th>
                                </th>
                            </tr>
                            <tr>
                                <td>
                                </td>
                                <td>
                                    <button id="button-save-new-group" type="button" hidden="hidden" class="am-btn am-btn-secondary am-round button-all"
                                        onclick="savenewgroup()" style="float: right;">
                                         <%= Resources.Lang.Res_Save_New_Group%></button>
                                    <button id="button-save-edit-group" type="button" hidden="hidden" class="am-btn am-btn-secondary am-round button-all"
                                        onclick="saveeditgroup()" style="float: right;">
                                        <%= Resources.Lang.Res_Save_Edit_Group%></button>
                                </td>
                            </tr>
                            <tr style="height: 20px;">
                            </tr>
                            <%-- 用来保存Group Id --%>
                            <tr hidden="hidden">
                                <td>
                                    Group Id:
                                </td>
                                <td>
                                    <input id="input-add-groupid" type="text" class="am-form-field" maxlength="20" />
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                   <%= Resources.Lang.Res_Group_Name%>:
                                </td>                                
                                <td>
                                    <table style="width: 100%;">
                                        <tr>
                                            <td style="width: 50%; padding-right: 20px;">
                                                <input id="input-add-groupname" type="text" class="am-form-field" maxlength="20" />
                                            </td>
                                            <td style="width: 20%; text-align: right; padding-right: 20px;">
                                                <%= Resources.Lang.Res_Status%>:
                                            </td>
                                            <td>
                                                <select id="select-status" data-am-selected="{btnWidth: '100%', btnSize: 'xl'}">
                                                    <option value="A" selected>Valid</option>
                                                    <option value="I">Invalid</option>
                                                </select>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr style="height: 20px;">
                            </tr>
                            <tr>
                                <td style="text-align:right; padding-right:20px;">
                                    <%= Resources.Lang.Res_Department%>:
                                </td>
                                <td>
                                    <table style="width: 100%;">
                                        <tr>
                                            <td style="width: 50%; padding-right: 20px;">
                                                <select id="select-dept" data-am-selected="{btnWidth: '100%', btnSize: 'xl'}">
                                                    <option value="-1" selected>--Please select--</option>                                                   
                                                    <option value="MKT">MKT</option>
                                                    <option value="FIN">FIN</option>
                                                    <option value="EM">EM</option>                                                    
                                                    <option value="PRO">Promotion</option>
                                                    <option value="Admin">Admin</option>                                                    
                                                </select>
                                            </td>
                                            <td style="width: 20%; text-align: right; padding-right: 20px;">
                                                <%= Resources.Lang.Res_Approver%>:
                                            </td>
                                            <td>
                                                 <input id="input-approver" type="checkbox" class="checkbox" />
                                            </td>                                        
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr style="height: 20px;">
                            </tr>
                            <tr>
                                <td style="vertical-align: top; text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Authority%>:
                                </td>
                                <td>
                                    <%--bootgrid--%>
                                    <table id="grid-auth" class="table table-condensed table-hover table-striped global-bootgrid"
                                        data-toggle="bootgrid">
                                        <thead>
                                            <tr>
                                                <th data-column-id="AUTHTYPEID" data-type="numeric" data-identifier="true" data-align="center"
                                                    data-header-align="center">
                                                    <%= Resources.Lang.Res_Authority_Id%>
                                                </th>
                                                <th data-column-id="AUTHTYPENAME" data-align="center" data-header-align="center">
                                                    <%= Resources.Lang.Res_Authority_Type%> Res_Users
                                                </th>
                                            </tr>
                                        </thead>
                                    </table>
                                </td>
                            </tr>
                            <tr style="height: 20px;">
                            </tr>
                            <tr>
                                <td style="vertical-align: top; text-align: right; padding-right: 20px;">
                                   <%= Resources.Lang.Res_Users%>:
                                </td>
                                <td>
                                    <table id="grid-users" class="table table-condensed table-hover table-striped global-bootgrid" data-toggle="bootgrid">
                                        <thead>
                                            <tr>
                                                <th data-column-id="LoginName" data-align="center" data-header-align="center">
                                                    <%= Resources.Lang.Res_Login_Name%>
                                                </th>
                                                <th data-column-id="Email" data-align="center" data-header-align="center">
                                                    <%= Resources.Lang.Res_Email%>  
                                                </th>
                                            </tr>
                                        </thead>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align: top; text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Property%>:
                                </td>
                                <td>
                                    <%--bootgrid--%>
                                    <table id="grid-property" class="table table-condensed table-hover table-striped global-bootgrid"
                                        data-toggle="bootgrid">
                                        <thead>
                                            <tr>
                                                <th data-column-id="PROPERTY_NAME" data-align="center" data-header-align="center">
                                                    <%= Resources.Lang.Res_Property_Name%>
                                                </th>
                                                <th data-column-id="PROPERTY_CODE" data-identifier="true" data-align="center" data-header-align="center">
                                                    <%= Resources.Lang.Res_Property_Code%>
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
    <%-- 确认删除模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="model-del-confirm">
        <div class="am-modal-dialog">
            <div class="am-modal-hd">
                Delete group ?<a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>
                    &times;</a>
            </div>
            <div class="am-modal-bd">
                <label id="label-del-groupname">
                </label>
                <label id="label-del-groupid" hidden="hidden">
                </label>
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="DelGroup()"
                                style="float: right">
                                Delete</button>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
