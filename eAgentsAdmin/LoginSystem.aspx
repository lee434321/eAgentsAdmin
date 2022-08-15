<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginSystem.aspx.cs" Inherits="PropertyOneAppWeb.LoginSystem" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=EDGE" />
    <link rel="stylesheet" href="Css/Site.css" />
    <link rel="stylesheet" href="js/AmazeUI/assets/css/amazeui.css" />
    <!--[if (gte IE 9)|!(IE)]><!-->
    <script type="text/javascript" src="js/jQuery/jquery-3.2.1.min.js"></script>
    <!--<![endif]-->
    <!--[if lte IE 8]>
    <script src="/js/jQuery/jquery-1.11.1.min.js"></script>
    <script src="http://cdn.staticfile.org/modernizr/2.8.3/modernizr.js"></script>
    <script src="/js/AmazeUI/assets/js/amazeui.ie8polyfill.min.js"></script>
    <![endif]-->
    <script type="text/javascript" src="js/AmazeUI/assets/js/amazeui.min.js"></script>
    <script type="text/javascript" src="js/SiteJScript.js"></script>
    <title>eAgents Login</title>
    <script type="text/javascript">
        var logintype = '<%=System.Configuration.ConfigurationManager.AppSettings["logintype"].ToString()%>';
        $(document).ready(function () {           
            /* 设置回车键 */
            $(window).keydown(function (event) {
                if (event.keyCode == 13) {
                    $("#button-login").click();
                }
            });
        });

        /* 校验登录用户 */
        function userCheck() {
            try {
                $("#button-login").prop("disabled", true);
                var lang = $("#SelectCulture").val();
                var username = $("#input-username").val();
                var psw = $("#input-password").val();

                if (checknotnull(username) == false) {
                    throw "Please input user name";
                }

                if (checknotnull(psw) == false) {
                    throw "Please input password";
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "LoginSystem.aspx",
                    data: {
                        "action": "check",
                        "username": username,
                        "password": psw,
                        "lang": lang,
                        "logintype": logintype
                    },
                    success: function (data) {
                        $("#button-login").prop("disabled", false);
                        if (data == "ok") {

                            /// -- start -- 这里如果用户属于多个权限组，则弹出对话框，选择后进入home页面。
                            httpost("LoginSystem.aspx"
                            , { "action": "getgroup", "username": username }
                            , function (rsp) {
                                if (rsp.err_code != 0) {
                                    showAlert("Error", rsp.err_msg);
                                } else {
                                    if (rsp.result.length > 1) {
                                        $("#rdogroups > label").remove();
                                        for (var i = 0; i < rsp.result.length; i++) {
                                            $r = $("<label class='am-text-left'>" + rsp.result[i].GROUPNAME + "</label>");
                                            $r.addClass("am-radio");
                                            $i = $("<input type='radio' name='radiogroup' data-am-uncheck value=" + rsp.result[i].GROUPID + " class='left'/>");
                                            $r.append($i);
                                            $("#rdogroups").append($r);
                                        }
                                        showModal("doc-modal-group", 300, false);
                                    } else if (rsp.result.length == 0) {
                                        showAlert("Error", "No group info.");
                                    } else
                                        loadsysuserinfo(rsp.result[0].GROUPID);
                                }

                            });
                            
                            /// --  end  --       
                        }
                        else {
                            showAlert("Notice", data);
                        }
                    },
                    error: function (data) {
                        $("#button-login").prop("disabled", false);
                        showAlert("Notice", data);
                    }
                });
            }
            catch (ex) {
                $("#button-login").prop("disabled", false);
                showAlert("Notice", ex);
            }
        };

        /* 加载System用户信息 */
        function loadsysuserinfo(groupid) {
            $.ajax({
                cache: false,
                type: "POST",
                url: "LoginSystem.aspx",
                data: { "action": "loadsysuserinfo", "groupid": groupid },
                success: function (data) {
                    if (data == "ok") {
                        window.location.href = "./system/Homepage.aspx";
                    }
                    else {
                        showAlert("Notice", data);
                    }
                },
                error: function (data) {
                    showAlert("Notice", data);
                }
            });
        };

        function selectgroup() {
            $("#rdogroups input").each(function () {
                if (this.checked) {
                    loadsysuserinfo(this.value);
                }
            });
        }

        function httpost(url, paras, callback) {
            $.ajax({
                url: url,
                method: "POST",
                data: paras,
                dataType: "json",
                async: true,
                success: callback,
                error: callback
            });
        }
    </script>
</head>
<body class="login-body-all-browser" style="overflow: hidden;">
    <div class="div-login-main-logo">
        <img id="img-logo" alt="" src="Image/SystemM_logo.png" width="283" height="97" />
    </div>
    <div class="div-login-building">
        <div class="div-login-form">
            <div class="am-g am-g-collapse">
                <div class="am-u-sm-12">
                    <table style="width: 100%;">
                        <tr>
                            <td>
                                <table style="width: 100%;">
                                    <tr>
                                        <td style="vertical-align: bottom; padding-left: 20px; padding-bottom: 15px;">
                                            <table style="width: 100%;">
                                                <tr>
                                                    <th style="width: 8%;">
                                                    </th>
                                                    <th style="width: 92%;">
                                                    </th>
                                                </tr>
                                                <tr>
                                                    <td style="text-align: left;">
                                                        <img alt="" src="Image/login_icon01.png" width="25px" height="25px" />
                                                    </td>
                                                    <td style="text-align: left;">
                                                        <label class="div-login-main-container-title-text">
                                                            <%= Resources.Lang.Res_Login_Title%>
                                                        </label>
                                                    </td>
                                                </tr>
                                                <tr style="height: 5px;">
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <div class="div-login-main-container-title-line">
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr style="height: 10px;">
                        </tr>
                        <tr>
                            <td>
                                <table style="width: 100%;">
                                    <tr>
                                        <th style="width: 150px;">
                                        </th>
                                        <th>
                                        </th>
                                    </tr>
                                    <tr style="height: 20px;">
                                    </tr>
                                    <%-- UserName --%>
                                    <tr id="tr-select-username">
                                        <td colspan="2">
                                            <table style="width: 100%;">
                                                <tr>
                                                    <td style="width: 150px; padding-left: 20px;">
                                                        <label id="label-loginid" class="label-login-input">
                                                            <%= Resources.Lang.Res_LoginSystem_Username%>:
                                                        </label>
                                                    </td>
                                                    <td>
                                                        <input id="input-username" type="text" class="am-form-field" maxlength="20" />
                                                    </td>
                                                </tr>
                                                <tr style="height: 20px;">
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <%-- Password --%>
                                    <tr>
                                        <td style="padding-left: 20px;">
                                            <label class="label-login-input">
                                                 <%= Resources.Lang.Res_LoginSystem_Password%>:
                                            </label>
                                        </td>
                                        <td>
                                            <input id="input-password" type="password" class="am-form-field" maxlength="20" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr style="height: 30px;">
                        </tr>
                        <tr>
                            <td style="padding-left: 20px;">
                                <table style="width: 100%;">
                                    <tr>
                                        <th style="width: 45%;">
                                        </th>
                                        <th style="width: 10%;">
                                        </th>
                                        <th style="width: 45%;">
                                        </th>
                                    </tr>
                                    <tr>
                                        <td>
                                            <button id="button-login" type="button" class="am-btn am-btn-block button-login"
                                                onclick="userCheck()">
                                                <%= Resources.Lang.Res_Login_Button_Login %></button>
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                            <a class="am-btn am-btn-block button-login" href="systempublic/forgetpassword.aspx" target="_blank"><%= Resources.Lang.Res_Login_ForgetPassword%></a> 
                                        </td>
                                    </tr>
                                    <tr style="height: 20px;"></tr>
                                    <tr><td><a class="am-btn am-btn-link" href="?lang=zh-CHS">中文</a> &nbsp;<a href="?lang=en-US">English</a></td></tr>
                                    <tr hidden="hidden">
                                        <td>
                                            <button id="button-term" type="button" class="am-btn am-btn-block button-login" data-am-modal="{target: '#doc-modal-terms', closeViaDimmer: 0, width: 800, height: 500}">
                                                <%= Resources.Lang.Res_Login_Button_Term%></button>
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                            <select id="SelectCulture" runat="server" data-am-selected="{btnWidth: '100%', btnSize: 'xl', btnStyle: 'danger'}">
                                                <option value="en-US" selected>English</option>
                                                <option value="zh-CHT">繁體中文</option>
                                                <option value="zh-CHS">简体中文</option>
                                            </select>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <%--警告模态窗口--%>
    <div class="am-modal am-modal-alert" tabindex="-1" id="doc-modal-alert">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                <label id="label-alert-title">
                    Select A Group
                </label>
            </div>
            <div class="am-modal-bd">
                <div class="am-modal-bd-my">      
                    <div class="am-g">
                        <div class="am-u-md-10 left" id="label-alert-content">                           
                        </div>                        
                    </div>                           
                </div>
            </div>
            <div class="am-modal-footer">
                <span class="am-modal-btn">OK</span>
            </div>
        </div>
    </div>

    <%--登录用户选择分组模态窗口--%>
    <div class="am-modal am-modal-alert" tabindex="-1" id="doc-modal-group">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                <label id="label-dialog-title"><%=Resources.Lang.Res_Select_Group %></label>
            </div>
            <div class="am-modal-bd">
                <div class="am-modal-bd-my">      
                    <div class="am-g">
                        <div class="am-u-md-10 left" id="rdogroups">                           
                        </div>                        
                    </div>                           
                </div>
            </div>
            <div class="am-modal-footer">
                <span class="am-modal-btn" onclick="selectgroup()">OK</span>
            </div>
        </div>
    </div>
</body>
</html>
