<%@ Page Title="" Language="C#" MasterPageFile="~/SitePublic.Master" AutoEventWireup="true"
    CodeBehind="ForgetPassword.aspx.cs" Inherits="PropertyOneAppWeb.Public.ForgetPassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            /* 导航栏图片 */
            changeNav("/Image/icon_MyProfile.png", "<%= Resources.Lang.Res_Menu_ForgetPassword%>");
        });

        /* 提交lease number 获取code和email */
        function GetVerificationCode() {
            var leasenum = $("#input-userid").val();
            if (checknotnull(leasenum)) {
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Public/ForgetPassword.aspx",
                    data: {
                        "action": "getcode",
                        "leasenum": leasenum
                    },
                    success: function (data) {
                        if (data == "ok") {
                            showAlert("Reset Password", "<%= Resources.Lang.Res_ForgetPassword_Send_Success%>");
                        }
                        else {
                            showAlert("Error", '<%= Resources.Error.ERR_VCODE_GET %>' + data);
                        }
                    },
                    error: function (data) {
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            else {
                showAlert("Notice", '<%= Resources.Error.ERR_INPUT_LEASE %>');
            }
        };

        /* 校验验证码是否正确， 并更改密码 */
        function ChangePassword() {
            var codeUser = $("#input-code").val();
            var psw1 = $("#input-psw-1").val();
            var psw2 = $("#input-psw-2").val();

            if (checknotnull(codeUser) && checknotnull(psw1) && checknotnull(psw2) && psw1 == psw2) {
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Public/ForgetPassword.aspx",
                    data: {
                        "action": "changepsw",
                        "code": codeUser,
                        "newpsw": psw1
                    },
                    success: function (data) {
                        if (data == "ok") {
                            $("#input-userid").val("");
                            $("#input-code").val("");
                            $("#input-psw-1").val("");
                            $("#input-psw-2").val("");
                            showAlert("Notice", '<%= Resources.Lang.Res_ChangePsw_Changed %>');
                        }
                        else {
                            showAlert("Error", '<%= Resources.Error.ERR_PSW_CHANGE %>' + data);
                        }
                    },
                    error: function (data) {
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            else {
                showAlert("Notice", '<%= Resources.Error.ERR_INPUT_PSW_VCODE %>');
            }
        };

        /* 跳转到注册页面 */
        function GoLogin() {
            window.location.href = "/Login.aspx";
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 20px 200px 20px 200px;">
        <%-- 获取验证码部分 --%>
        <table style="width: 100%;">
            <tr>
                <td>
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_ForgetPassword_LeaseNumber%></label>
                </td>
            </tr>
            <tr>
                <td>
                    <input type="text" id="input-userid" class="am-form-field" maxlength="20" />
                </td>
            </tr>
            <tr style="height: 20px;">
            </tr>
            <tr>
                <td>
                    <button type="button" class="am-btn am-btn-secondary am-round button-all" style="width: 200px;
                        float: right;" onclick="GetVerificationCode()">
                        <%= Resources.Lang.Res_ForgetPassword_GetCode%></button>
                </td>
            </tr>
            <tr style="height: 20px;">
            </tr>
            <%-- 验证验证码部分 --%>
            <tr>
                <td>
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_ForgetPassword_YourCode%></label>
                </td>
            </tr>
            <tr>
                <td>
                    <input type="text" id="input-code" class="am-form-field" maxlength="6" />
                </td>
            </tr>
            <tr style="height: 20px;">
            </tr>
            <%-- 更改密码部分 --%>
            <tr>
                <td>
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_ForgetPassword_NewPsw%></label>
                </td>
            </tr>
            <tr>
                <td>
                    <input type="password" id="input-psw-1" class="am-form-field" maxlength="20" />
                </td>
            </tr>
            <tr style="height: 20px;">
            </tr>
            <tr>
                <td>
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_ForgetPassword_ConfirmPsw%></label>
                </td>
            </tr>
            <tr>
                <td>
                    <input type="password" id="input-psw-2" class="am-form-field" maxlength="20" />
                </td>
            </tr>
            <tr style="height: 20px;">
            </tr>
            <tr>
                <td>
                    <table style="width: 100%;">
                        <tr>
                            <td>
                                <button type="button" class="am-btn am-btn-secondary am-round button-all" style="width: 200px;
                                    float: right;" onclick="ChangePassword()">
                                    <%= Resources.Lang.Res_ForgetPassword_ChangePsw%></button>
                            </td>
                            <td style="width: 130px;">
                                <button type="button" class="am-btn am-btn-default am-round button-all" onclick="GoLogin()"
                                    style="float: right">
                                    <%= Resources.Lang.Res_Register_Cancel%></button>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
