<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystemPublic.Master" AutoEventWireup="true"
    CodeBehind="forgetpassword.aspx.cs" Inherits="PropertyOneAppWeb.systempublic.forgetpassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            /* 导航栏图片 */
            changeNav("../Image/icon_MyProfile.png", "Forget or change password");
        });

        /* 提交lease number 获取code和email */
        function GetVerificationCode() {
            try {
                $("#button-get-code").prop("disabled", true);
                var leasenum = $("#input-userid").val();

                if (checknotnull(leasenum) == false) {
                    throw "Please input your lease number";
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./forgetpassword.aspx",
                    data: {
                        "action": "getcode",
                        "loginname": leasenum
                    },
                    success: function (data) {
                        $("#button-get-code").prop("disabled", false);
                        if (data == "ok") {
                            showAlert("Notice", "Your verification code has been sent to your email address");
                        }
                        else {
                            showAlert("Notice", data);
                        }
                    },
                    error: function (data) {
                        $("#button-get-code").prop("disabled", false);
                        showAlert("Notice", data);
                    }
                });
            }
            catch (ex) {
                $("#button-get-code").prop("disabled", false);
                showAlert("Notice", ex);
            }
        };

        /* 校验验证码是否正确， 并更改密码 */
        function ChangePassword() {
            try {
                $("#button-change-psw").prop("disabled", true);

                var codeUser = $("#input-code").val();
                var psw1 = $("#input-psw-1").val();
                var psw2 = $("#input-psw-2").val();

                if (checknotnull(codeUser) == false) {
                    throw "Please input verification code";
                }

                if (checknotnull(psw1) == false || checknotnull(psw2) == false) {
                    throw "Please input password";
                }

                if (psw1 != psw2) {
                    throw "Please input same password";
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./forgetpassword.aspx",
                    data: {
                        "action": "changepsw",
                        "code": codeUser,
                        "newpsw": psw1
                    },
                    success: function (data) {
                        $("#button-change-psw").prop("disabled", false);
                        if (data == "ok") {
                            $("#input-userid").val("");
                            $("#input-code").val("");
                            $("#input-psw-1").val("");
                            $("#input-psw-2").val("");
                            showAlert("Notice", "Password reset success");
                            window.location = "../loginsystem.aspx";
                        }
                        else {
                            showAlert("Notice", data);
                        }
                    },
                    error: function (data) {
                        $("#button-change-psw").prop("disabled", false);
                        showAlert("Notice", data);
                    }
                });
            }
            catch (ex) {
                $("#button-change-psw").prop("disabled", false);
                showAlert("Notice", ex);
            }
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
                        User name</label>
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
                    <button id="button-get-code" type="button" class="am-btn am-btn-secondary am-round button-all"
                        style="float: right;" onclick="GetVerificationCode()">
                        Get verification code</button>
                </td>
            </tr>
            <tr style="height: 50px;">
            </tr>
            <%-- 验证验证码部分 --%>
            <tr>
                <td>
                    <label class="g-font-1">
                        Verification code</label>
                </td>
            </tr>
            <tr>
                <td>
                    <input type="text" id="input-code" class="am-form-field" maxlength="6" />
                </td>
            </tr>
            <tr style="height: 30px;">
            </tr>
            <%-- 更改密码部分 --%>
            <tr>
                <td>
                    <label class="g-font-1">
                        New password</label>
                </td>
            </tr>
            <tr>
                <td>
                    <input type="password" id="input-psw-1" class="am-form-field" maxlength="20" />
                </td>
            </tr>
            <tr style="height: 30px;">
            </tr>
            <tr>
                <td>
                    <label class="g-font-1">
                        Confirm password</label>
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
                    <button id="button-change-psw" type="button" class="am-btn am-btn-secondary am-round button-all"
                        style="float: right;" onclick="ChangePassword()">
                        Change password</button>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
