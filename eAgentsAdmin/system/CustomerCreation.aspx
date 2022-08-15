 
<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true" 
    CodeBehind="CustomerCreation.aspx.cs" Inherits="PropertyOneAppWeb.system.CustomerCreation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/ecmascript">
        $(document).ready(function () {
            changeNav("../Image/icon_Notices.png", '<%= Resources.Lang.Res_Customer_Creation%>');
            search();            
            //searchCustGroup();
            $("#grid-group").bootgrid().on("click.rs.jquery.bootgrid", function (e, cols, rows) {
                searchpropertyfromgroup(rows.GROUPID);
                //$("#property").removeClass("hidden");
            });
        });

        var firstPageload = true; /* 是否第一次加载页面 */
        function search() {            
            if (firstPageload) {
                /* 加载bootgird数据 */
                $("#grid-data").bootgrid({
                    navigation: 3,
                    rowCount: 10,
                    columnSelection: false,
                    ajax: true,
                    url: "./CustomerCreation.aspx",
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
                    requestHandler: function (req) { /// 这里用来设置请求参数
                        req["type"] = "mkt";
                        return req;
                    },
                    formatters: {
                        "edit": function (column, row) {
                            var userid = row.USERID;
                            var loginName = row.LOGINNAME;
                            return '<a class="am-badge am-badge-primary am-radius" href="JavaScript:void(0);" onclick="EditUser(\'' + userid + '\')">Edit</a> <a class="am-badge am-badge-danger am-radius" href="JavaScript:void(0);" onclick="OpenModelDel(\'' + userid + '\', \'' + loginName + '\')">Delete</a>';
                        }
                    }
                });
                /// -- start -- #20112 这里对刷新按钮赋予一个事件：只要点击该按钮，下方编辑区域就隐藏。
                $("#grid-data").siblings("#grid-data-header")
                .find(".actionBar .actions button[title='Refresh']")
                .on('click', function () {
                    $("#div-addcustuser").hide();
                });
                /// --  end  --
                firstPageload = false;
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");              
            };

            /// -- start -- 加载后处理
            /// 检查是否为Admin登录，用来控制是否需要选择MKT或FIN角色。
            $("#grid-data").bootgrid().on("loaded.rs.jquery.bootgrid", function (e) {
                httpost2("./CustomerCreation.aspx", { action: "checkisadmin" }, "", function (rsp) {
                    if (rsp.err_code == 0 && rsp.result == true) { //如果是admin登录，则显示下拉选择；否则隐藏。
                        $("#tr-role").removeClass("hidden");
                        $("#button-send-email").removeClass("hidden");
                        $("#button-batchSendMail").removeClass("hidden"); 
                    } else {
                        $("#tr-role").addClass("hidden");
                        $("#button-send-email").addClass("hidden");
                        $("#button-batchSendMail").addClass("hidden"); 
                    }
                });
            });
            /// --  end --
        }

        var firstCustGroup = true;
        function searchCustGroup() {
            if (firstCustGroup) {
                /* 加载bootgird数据 */
                $("#grid-group").bootgrid({
                    navigation: 0,
                    rowCount: -1,
                    selection: true,
                    rowSelect: true,
                    ajax: true,
                    url: "./CustomerCreation.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        return {
                            action: "searchcustgroup"
                        };
                    }
                });
                firstCustGroup = false;

               
            } else {
                $("#grid-group").bootgrid("reload");
            }           
        }

        function searchpropertyfromgroup(id) {
            $("#grid-property").bootgrid({
                navigation: 0,
                rowCount: -1,
                ajax: true,
                url: "./CustomerCreation.aspx",
                sorting: false,
                ajaxSettings: {
                    cache: false
                },
                post: function () {
                    return {
                        action: "searchpropertyfromgroup",
                        groupid: id
                    };
                }
            });
        }

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
                    url: "./CustomerCreation.aspx",
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

        // 检索关联的lease列表
        var firstsearchgrouplease = true; /* 是否第一次加载Property页面 */
        function searchgrouplease(userid) {
            if (firstsearchgrouplease == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-grouplease").bootgrid({
                    navigation: 0,
                    rowCount: -1,
                    ajax: true,
                    url: "./CustomerCreation.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        return {
                            action: "searchgrouplease",
                            userid:userid
                        };
                    }
                });
                //firstsearchgrouplease = false;
            } else { /* 如果不是第一次 */
                $("#grid-grouplease").bootgrid("reload");
            };
        }
        /* 保存新用户 */
        function SaveNewCustUser() {
            try {
                var loginname = $("#input-loginname").val(); 
                var leasenum = $("#input-leasenum").val();
                var status = $("#select-status").val();
                var email = $("#input-email").val();
                var propeties = $("#grid-property").bootgrid("getSelectedRows");
                var groups = $("#grid-group").bootgrid("getSelectedRows");
                var role = $("#tr-role").attr("class") != "hidden" ? $("#select-role").val() : "";     //
                var contactname = $("#input-contactname").val();
                var contactnbr = $("#input-contactnbr").val();
                var custname = $("#customer_name").html();
                var position = $("#input-position").val();
                var isprimary = $("#input-isprimary")[0].checked; //[0].checked
                var sendstr = leasenum + "|" + status + "|" + email + "|" + propeties + "|" + groups + "|" + loginname + "|" + role + "|" + contactname + "|" + contactnbr + "|" + custname;
                sendstr += "|" + position + "|" + isprimary;

                if (checknotnull(leasenum) == false) {
                    throw "Please input lease number";
                }

                if (checknotnull(status) == false) {
                    throw "Please select status";
                }

                if (checknotnull(email) == false) {
                    throw "Please input email address";
                }
                if ($("#tr-role").attr("class") != "hidden") {
                    if (role == "-1") {
                        throw "Please select a role";
                    }
                }
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./CustomerCreation.aspx",
                    data: {
                        "action": "savenewcustuser",
                        "saveinfo": sendstr
                    },
                    success: function (data) {
                        if (checknotnull(data) && data == "success") {
                            search();
                            showAlert("Notice", "Save success!");

                            //显示发送邮件按钮
                            $("#button-send-email").css("display", "block");

                            //显示“编辑保存”按钮
                            $("#button-save-edit-cust-user").show();
                            $("#button-save-new-cust-user").hide();
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
        function BatchSendMail() {
            $("#button-batchSendMail")[0].disabled = true;
            $.ajax({
                type: "POST",
                url: "./CustomerCreation.aspx",
                data: {
                    "action": "batchSendmail"
                },
                dataType: "json",
                success: function (data) {
                    if (data.err_code != 0) {
                        showAlert("Error", data.err_msg);
                    } else {
                        showAlert("Notice", data.result);
                    }
                    $("#button-batchSendMail")[0].disabled = false;
                },
                error: function () {
                    $("#button-batchSendMail")[0].disabled = false;
                }
            });
        }

        /* 创建新用户 */
        function NewCreation() {
            /* 清空input内容 */
            $("#input-loginname").val("");
            $("#customer_name").text("");

            $("#input-userid").val("");
            $("#input-leasenum").val("");
            $("#input-email").val("");

            $("#input-leasenum").prop("disabled", false);
            $("#input-contactnbr").val("");
            $("#input-contactname").val("");

            $("#input-position").val("");
            $("#input-isprimary")[0].checked = false;
            //$("#select-role").find('option').eq(0).attr('selected', true);
            //$("#select-role").trigger('changed.selected.amui'); //这里要重新触发一次，才能更新select控件的选中
                            
            /* group lease 清空*/
            $("#grid-grouplease").bootgrid("destroy");
            //firstsearchgrouplease = true;
            //searchgrouplease(0);

            /* 变更title文字 */
            $("#p-title").text('<%= Resources.Lang.Res_Customer_Creation%>');

            /* 清空复选框 */
            $("#grid-property").bootgrid("deselect");
                        
            /* 显示下方编辑区域 */
            $("#div-addcustuser").show();

            /* 只显示编辑保存按钮 */
            $("#button-save-edit-cust-user").hide();
            $("#button-save-new-cust-user").show();
            $("#button-send-email").hide();
        };

        /* 编辑用户 */
        function EditUser(userid) {
            /* 清空 grip 控件*/
            $("#grid-grouplease").bootgrid("destroy");
            /* 清空复选框 */
            //$("#grid-group").bootgrid("deselect");            

            /* 查找数据 */
            $.ajax({
                cache: false,
                type: "POST",
                url: "./CustomerCreation.aspx",
                data: {
                    "action": "searchonecustuser",
                    "userid": userid
                },
                success: function (data) {

                    if (checknotnull(data)) {
                        try {
                            var jsonData = jQuery.parseJSON(data);
                            var userid = jsonData.UserId;
                            var loginname = jsonData.LoginName;
                            var email = jsonData.Email;
                            var status = jsonData.Status;
                            var properties = jsonData.CU_Properties;
                            var group = jsonData.CU_Group;
                            var role = jsonData.Role;
                            var create_from_lease = jsonData.Create_From_Lease;
                            var contactname = jsonData.Contact_Name;
                            var contactnbr = jsonData.Contact_Number;
                            var position = jsonData.Position;
                            var isprimary = jsonData.IsPrimary;

                            $("#input-position").val(position);
                            $("#input-isprimary")[0].checked = isprimary == "Y" ? true : false;
                            $("#input-userid").val(userid);
                            $("#input-leasenum").val(create_from_lease);
                            $("#input-email").val(email);
                            $("#input-contactname").val(contactname);
                            $("#input-contactnbr").val(contactnbr);
                            /* 设置下拉框 */
                            if (status == "A") {
                                $("#select-status").get(0).selectedIndex = 0;
                            }
                            else {
                                $("#select-status").get(0).selectedIndex = 1;
                            }

                            /* 填充Role下拉框 */
                            if ($("#tr-role").attr("class") != "hidden") {
                            }
                            $("#select-role").val(role);

                            // 填充loginname
                            $("#input-loginname").val(loginname);

                            /* 不能修改login name */
                            $("#input-leasenum").prop("disabled", true);

                            /* 变更title文字 */
                            $("#p-title").text('<%= Resources.Lang.Res_Edit_Cust_User%>' + loginname); 

                            /* 显示下方区域 */
                            $("#div-addcustuser").show();

                            /* 只显示编辑保存按钮 */
                            $("#button-save-edit-cust-user").show();
                            $("#button-save-new-cust-user").hide();
                            if (jsonData.CU_Status == "0") {
                                $("#button-send-email").show();
                            }

                            $("select").trigger('changed.selected.amui'); //这里要重新触发一次select控件，才能更新select控件的选中

                            searchgrouplease(userid);
                            getCustName(create_from_lease);
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
        function SaveEditCustUser() {
            try {
                var userid = $("#input-userid").val();
                var loginname = $("#input-loginname").val();
                var status = $("#select-status").val();
                var email = $("#input-email").val();
                var properties = $("#grid-property").bootgrid("getSelectedRows");
                var groups = $("#grid-group").bootgrid("getSelectedRows");
                var role = $("#select-role").val();
                var contactname = $("#input-contactname").val();
                var contactnbr = $("#input-contactnbr").val();
                var custname = $("#customer_name").html();
                var position = $("#input-position").val();
                var isprimary = $("#input-isprimary")[0].checked;              

                var sendstr = userid + "|" + loginname + "|" + status + "|" + email + "|" + properties + "|" + groups + "|" + role + "|" + contactname + "|" + contactnbr + "|" + custname;
                sendstr += "|" + position + "|" + isprimary;

                if (checknotnull(userid) == false) {
                    //throw "User ID can not be null";
                }

                if (checknotnull(loginname) == false) {
                    throw "Please input lease number";
                }

                if (checknotnull(status) == false) {
                    throw "Please select status";
                }

                if (checknotnull(email) == false) {
                    throw "Please input email address";
                }

                if (role == "-1") {
                    $("#select-role").focus();
                    throw "Please select an role!";
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./CustomerCreation.aspx",
                    data: {
                        "action": "saveeditcustuser",
                        "saveinfo": sendstr
                    },
                    success: function (data) {
                        if (checknotnull(data) && data == "success") {
                            search();

                            /* 隐藏下方区域 */
                            //$("#div-addcustuser").hide(); //暂时屏蔽
                            showAlert("Notice", "Save success");
                        }
                        else if (data == "timeout") {
                            window.location = "../loginsystem.aspx";
                        } else if (data != "") {
                            showAlert("Error", data);
                        }
                        else {
                            window.location = "../loginsystem.aspx";
                            //showAlert("Error", data);
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
                    url: "./CustomerCreation.aspx",
                    data: {
                        "action": "deluser",
                        "userid": userid
                    },
                    success: function (data) {
                        if (data == "timeout") {
                            window.location = "../loginsystem.aspx";
                            return;
                        }
                        if (checknotnull(data) && data == "success") {
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
                $("#div-addcustuser").hide();
            }
        }

        /* 打开删除确认窗口 */
        function OpenModelDel(userid, loginName) {
            $("#label-del-loginname").text(loginName);
            $("#label-del-userid").text(userid);
            showModal("model-del-confirm", 600, false);
        };

        function SendEmail2CustUser() {
            var userid = $("#input-userid").val();
            var loginName = $("#input-loginname").val(); //input-loginname
            $("#button-send-email").attr("disabled", "disabled");            
            $.ajax({
                cache: false,
                type: "POST",
                url: "./CustomerCreation.aspx",
                data: {
                    "action": "sendemail2custuser",
                    "userid": userid,
                    "loginName":loginName
                },
                success: function (data) {
                    if (data == "timeout") {
                        window.location = "../loginsystem.aspx";
                        return;
                    }
                    if (checknotnull(data) && data == "success") {
                        showAlert("Notice", "Email has been sent!");
                    }
                    else {                      
                        showAlert("Error", data);
                    }
                    $("#button-send-email").removeAttr("disabled");
                },
                error: function (data) {
                    AlertJqueryAjaxError(data);
                    $("#button-send-email").removeAttr("disabled");
                }
            });
        }

        function getCustNameOnblur() {
            if ($("#input-leasenum").val()!="") {
                getCustName($("#input-leasenum").val());   
            }                    
        }

        function getCustName(leasenum) {
            $("#customer_name").text(""); //先清空一次。
            var data = { "action": "getcustname", "lease_num": leasenum }
            httpost("./CustomerCreation.aspx"
            , data
            , true
            , "json"
            , function (rsp) {
                if (rsp.err_code >= 0) {
                    $("#customer_name").text(rsp.result.CUSTOMER_NAME);
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
    </script>    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="am-g am-g-collapse">
        <%-- 外边框 --%>
        <div class="am-u-sm-12">
            <%-- 深色标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue"><p><%= Resources.Lang.Res_Customer_Creation%></p></div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">
                    <%--bootgrid--%>
                    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid" data-toggle="bootgrid">
                        <thead>
                            <tr>
                                <th data-column-id="USERID" data-visible="false" data-type="numeric" data-identifier="true" data-align="center" data-header-align="center">
                                    User Id
                                </th>
                                <th data-column-id="LOGINNAME" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Login_Name%>
                                </th>
                                <th data-column-id="EMAIL" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Email%>
                                </th>
                                <th data-column-id="CONTACT_NUMBER" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Phone%>
                                </th>
                                <th data-column-id="STATUS" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Status%>
                                </th>
                                <th data-column-id="EDIT" data-formatter="edit" data-header-css-class="boot-header-1" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Edit%>
                                </th>
                            </tr>
                        </thead>
                    </table>
                </div>
                <div class="am-u-sm-12">
                    <button id="button-add" type="button" class="am-btn am-btn-secondary am-radius" onclick="NewCreation()" style="float:right; margin-left:12px;"><%= Resources.Lang.Res_New_Creation%></button>
                    <button id="button-batchSendMail" type="button" class="am-btn am-btn-secondary am-radius" onclick="BatchSendMail()" style="float: right"><%= Resources.Lang.Res_Send_Mail_Batch%></button>
                </div>
                <div class="am-u-sm-12" style="height: 20px;">
                </div>                
            </div>
            <div class="am-u-sm-12" style="height: 30px;">
            </div>
            <%-- 新增Creation --%>
            <div id="div-addcustuser"  class="am-u-sm-12" hidden="hidden">
                <%-- 深色标题 --%>
                <div class="am-u-sm-12 global_table_head global_color_navyblue">
                    <p id="p-title">
                        <%= Resources.Lang.Res_New_Creation%>
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
                                    <button id="button-save-new-cust-user" type="button" hidden="hidden" class="am-btn am-btn-secondary am-radius button-all" onclick="SaveNewCustUser()" style="float: right;">
                                        <%= Resources.Lang.Res_Save_New_Customer_User%></button>
                                    <button id="button-save-edit-cust-user" type="button" hidden="hidden" class="am-btn am-btn-secondary am-radius button-all" onclick="SaveEditCustUser()" style="float: right;">
                                        <%= Resources.Lang.Res_Save_Edit_Customer_User%></button>                                                                            
                                </td>
                            </tr>
                            <tr style="height: 10px;"></tr>
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
                                    <input id="input-loginname" type="text" class="am-form-field" maxlength="40" />
                                </td>
                                <td></td> 
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
                            <tr style="height: 10px;"></tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Lease_Number%>:
                                </td>
                                <td>
                                    <input id="input-leasenum" type="text" class="am-form-field" maxlength="20" onblur="getCustNameOnblur()" />
                                </td>
                                <td></td>
                                <td style="text-align: right; padding-right: 20px;">
                                   <%= Resources.Lang.Res_Customer_Name%>:
                                </td>
                                <td>
                                    <span id="customer_name"></span>                                  
                                </td>
                            </tr>
                            <tr style="height: 10px;" >
                            </tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Email%>:
                                </td>
                                <td>
                                    <input id="input-email" type="text" class="am-form-field" maxlength="50" />
                                </td>
                                <td></td>                                                    
                                <td>
                                    <button id="button-send-email" type="button" hidden="hidden" class="am-btn am-btn-secondary am-radius button-all" onclick="SendEmail2CustUser()" style="float: right;">
                                       <%= Resources.Lang.Res_Send_Mail%>:
                                    </button>
                                </td>                               
                            </tr>
                            <tr style="height: 10px;"></tr>
                            <tr>
                                <td style="text-align:right;padding-right:20px">
                                    <%= Resources.Lang.Res_Contact_Name%>:
                                </td>
                                <td>
                                     <input id="input-contactname" type="text" class="am-form-field" maxlength="50" />
                                </td>
                                <td></td>
                                <td style="text-align:right;padding-right:20px">
                                    <%= Resources.Lang.Res_Contact_Number%>:
                                </td>
                                <td>
                                     <input id="input-contactnbr" type="text" class="am-form-field" maxlength="50" />
                                </td>
                            </tr>
                            <tr style="height: 10px;"></tr>
                            <tr id="tr-role">
                                <td style="text-align: right; padding-right: 20px;"> <%= Resources.Lang.Res_Role%>:</td>
                                <td>
                                     <select id="select-role" data-am-selected="{btnWidth: '100%', btnSize: 'xl'}">
                                        <option value="-1" selected>(Please select...)</option>
                                        <option value="M">MKT</option>
                                        <option value="F">FIN</option>
                                        <option value="E">EM</option>
                                        <option value="MF">MKT,FIN</option>
                                        <option value="ME">MKT,EM</option>
                                        <option value="EF">EM,FIN</option>                  
                                        <option value="X">MKT,FIN,EM</option>
                                    </select>
                                </td>
                                <td></td>
                                <td></td>
                                <td></td>
                            </tr>
                            <tr style="height: 10px;"></tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                   <%= Resources.Lang.Res_Position%>:
                                </td>
                                <td>
                                    <input id="input-position" type="text" class="am-form-field" />                          
                                </td>
                                <td></td>
                                <td style="text-align: right; padding-right: 20px;"> <%= Resources.Lang.Res_Primary%>:</td>
                                <td>
                                    <input id="input-isprimary" type="checkbox" />
                                </td>                                
                            </tr>
                            <tr style="height: 10px;"></tr>
                            <tr>
                                <td style="vertical-align: top; text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Lease_List%>:  
                                </td>
                                <td colspan="4">
                                    <table id="grid-grouplease" class="table table-condensed table-hover table-striped global-bootgrid" data-toggle="bootgrid">
                                        <thead>
                                            <tr>                                   
                                                <th data-column-id="USERID"  data-identifier="true" data-type="numeric" data-align="center" data-header-align="center" >
                                                    <%= Resources.Lang.Res_User_Id%>  
                                                </th>
                                                <th data-column-id="LEASENUMBER" data-align="center" data-header-align="center" >
                                                    <%= Resources.Lang.Res_Lease_Number%> 	
                                                </th>
                                            </tr>
                                        </thead>
                                    </table>
                                </td>
                            </tr>
                            <tr style="height: 10px;"></tr>                      
                            <tr class="hidden">
                                <td style="vertical-align: top; text-align: right; padding-right: 20px;">
                                  <%= Resources.Lang.Res_Select_Group%>:
                                </td>
                                <td colspan="4">
                                    <table id="grid-group" class="table table-condensed table-hover table-striped global-bootgrid" data-toggle="bootgrid">
                                        <thead>
                                            <tr>                                   
                                                <th data-column-id="GROUPID"  data-identifier="true" data-type="numeric" data-align="center" data-header-align="center" >
                                                    Id
                                                </th>
                                                <th data-column-id="GROUPNAME" data-align="center" data-header-align="center" >
                                                    <%= Resources.Lang.Res_Group_Name%>
                                                </th>
                                            </tr>
                                        </thead>
                                    </table>
                                </td>
                            </tr>
                            <tr style="height: 10px;"></tr>                      
                            <tr class="hidden" id="property">
                                <td style="vertical-align: top; text-align: right; padding-right: 20px;">
                                    <%= Resources.Lang.Res_Property%>:
                                </td>
                                <td colspan="4">
                                    <%--bootgrid--%>
                                    <table id="grid-property" class="table table-condensed table-hover table-striped global-bootgrid" data-toggle="bootgrid">
                                        <thead>
                                            <tr>                                   
                                                <th data-column-id="PROPERTY_CODE" data-align="center" data-header-align="center" >
                                                    <%= Resources.Lang.Res_Property_Code%>
                                                </th>
                                                <th data-column-id="PROPERTY_NAME" data-align="center" data-header-align="center" >
                                                    <%= Resources.Lang.Res_Property_Name%>
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