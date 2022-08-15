<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="changemail.aspx.cs" Inherits="PropertyOneAppWeb.Web.changemail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("/Image/icon_Notices.png", '<%= Resources.Lang.Res_Menu_EmailSetting%>');

            /* 初始化用户校验界面 */
            InitUserCheck();
        });

        /* 更新Email */
        function UpdateEmail() {
            try {
                $("#button-update-email").prop("disabled", true);

                var code = $("#input-code").val();
                var email = $("#input-email").val();
                var name = $("#input-name").val();
                var phone = $("#input-phone").val();

                if (checknotnull(code) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_VCODE %>';
                }

                if (checknotnull(email) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_EMAIL %>';
                }

                if (checknotnull(name) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_CONTACT %>';
                }

                if (checknotnull(phone) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_PHONE %>';
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Web/changemail.aspx",
                    data: {
                        "action": "change",
                        "email": email,
                        "contactname": name,
                        "phone": phone,
                        "code": code
                    },
                    success: function (data) {
                        $("#button-update-email").prop("disabled", false);
                        if (data == "ok") {
                            showModal("modal-success", 600, false);
                        }
                        else {
                            showAlert("Notice", '<%= Resources.Error.ERR_EMAIL_CHANGE %>' + data);
                        }
                    },
                    error: function (data) {
                        $("#button-update-email").prop("disabled", false);
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            catch (ex) {
                $("#button-update-email").prop("disabled", false);
                showAlert("Notice", ex);
            }
        };

        /* 刷新页面 */
        function Refresh() {
            location.reload();
        };

        /* 校验用户 */
        function VerifyUser() {
            try {
                /* 校验是否已经回答问题 */
                var valQ1 = $("#select-question-1").val();
                var valQ2 = $("#select-question-2").val();
                if (checknotnull(valQ1) == false || checknotnull(valQ2) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_QUESTION %>';
                }

                var answerQ1;
                var answerQ2;
                if (valQ1 == "ed") {
                    answerQ1 = $("#datapicker-q1").val();
                }
                else {
                    answerQ1 = $("#input-q1").val();
                }

                if (valQ2 == "ed") {
                    answerQ2 = $("#datapicker-q2").val();
                }
                else {
                    answerQ2 = $("#input-q2").val();
                }

                if (checknotnull(answerQ1) == false || checknotnull(answerQ2) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_QUESTION %>';
                }

                /* 准备传送数据 */
                var statementnum;
                var balance;
                var expiredate;
                var rental;
                if (valQ1 == "ed") {
                    expiredate = answerQ1;
                }
                else if (valQ1 == "sn") {
                    statementnum = answerQ1;
                }
                else if (valQ1 == "sb") {
                    balance = answerQ1;
                }
                else if (valQ1 == "mr") {
                    rental = answerQ1;
                }

                if (valQ2 == "ed") {
                    expiredate = answerQ2;
                }
                else if (valQ2 == "sn") {
                    statementnum = answerQ2;
                }
                else if (valQ2 == "sb") {
                    balance = answerQ2;
                }
                else if (valQ2 == "mr") {
                    rental = answerQ2;
                }

                /* 将校验按钮置为无效 */
                $("#button-verify").prop("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Web/changemail.aspx",
                    data: {
                        "action": "check",
                        "statementnum": statementnum,
                        "balance": balance,
                        "expiredate": expiredate,
                        "rental": rental
                    },
                    success: function (data) {
                        $("#button-verify").prop("disabled", false);
                        if (data == "true") {  /* 校验成功 */
                            $("#tr-verify").hide();
                            $("#tr-email").show();
                            $("#tr-update").hide();
                        }
                        else if (data == "false") {
                            showAlert("Notice", '<%= Resources.Error.ERR_QUESTION_VERIFICATION %>');
                        }
                        else {
                            showAlert("Notice", '<%= Resources.Error.ERR_QUESTION_VERIFICATION %>' + data);
                        }
                    },
                    error: function (data) {
                        $("#button-verify").prop("disabled", false);
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            catch (ex) {
                $("#button-verify").prop("disabled", false);
                showAlert("Notice", ex);
            }
        };

        /* 切换问题 */
        function ChangeQuestion(clickTarget, anotherTarget) {
            var clickVal = $("#" + clickTarget).val();

            if (clickVal == "sb") {              /* 如果选择了Statement balance */
                $("#" + anotherTarget).empty();
                $("#" + anotherTarget).append('<option selected value=""></option>');
                $("#" + anotherTarget).append('<option value="sn"><%= Resources.Lang.Res_Register_StatementNumber%></option>');
            }
            else if (clickVal == "sn") {         /* 如果选择了Statement number */
                AddQuestions(anotherTarget);
                $("#" + anotherTarget).find('option[value=' + clickVal + ']').eq(0).remove();
            }
            else if (clickVal == "mr" || clickVal == "ed") {
                AddQuestions(anotherTarget);
                $("#" + anotherTarget).find('option[value=' + clickVal + ']').eq(0).remove();
                $("#" + anotherTarget).find('option[value="sb"]').eq(0).remove();
            }

            /* 不支持 MutationObserver 的浏览器使用 JS 操作 select 以后需要手动触发 `changed.selected.amui` 事件 */
            $("#" + anotherTarget).trigger('changed.selected.amui');
        };

        /* 添加所有问题 */
        function AddQuestions(target) {
            $("#" + target).empty();
            $("#" + target).append('<option selected value=""></option>');
            $("#" + target).append('<option value="mr"><%= Resources.Lang.Res_Register_MonthlyRental%></option>');
            $("#" + target).append('<option value="ed"><%= Resources.Lang.Res_Register_ExpiryDate%></option>');
            $("#" + target).append('<option value="sn"><%= Resources.Lang.Res_Register_StatementNumber%></option>');
            $("#" + target).append('<option value="sb"><%= Resources.Lang.Res_Register_CurrentBalance%></option>');

            /* 不支持 MutationObserver 的浏览器使用 JS 操作 select 以后需要手动触发 `changed.selected.amui` 事件 */
            $("#" + target).trigger('changed.selected.amui');
        };

        /* 根据new email 获取code */
        function GetVerificationCode() {
            var newEmail = $("#input-email").val();
            if (checknotnull(newEmail)) {
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Web/changemail.aspx",
                    data: {
                        "action": "getcode",
                        "newemail": newEmail
                    },
                    success: function (data) {
                        if (data == "ok") {
                            showAlert("Notice", "<%= Resources.Lang.Res_ForgetPassword_Send_Success%>");
                            $("#tr-verify").hide();
                            $("#tr-email").hide();
                            $("#tr-update").show();
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
                showAlert("Notice", '<%= Resources.Error.ERR_INPUT_EMAIL %>');
            }
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 20px 200px 20px 200px;">
        <table style="width: 100%;">
            <%-- 验证用户 --%>
            <tr id="tr-verify">
                <td>
                    <table style="width: 100%;">
                        <tr>
                            <td>
                                <table style="width: 100%;">
                                    <tr>
                                        <td style="width: 400px;">
                                            <select id="select-question-1" placeholder="<%= Resources.Lang.Res_Register_SecurityQuestion1%>"
                                                data-am-selected="{btnWidth: '400'}">
                                            </select>
                                        </td>
                                        <td style="width: 20px;">
                                        </td>
                                        <td>
                                            <table style="width: 100%;">
                                                <tr>
                                                    <td id="td-input-q1">
                                                        <input id="input-q1" type="text" class="am-form-field" maxlength="30" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td id="td-datapicker-q1" hidden="hidden">
                                                        <input id="datapicker-q1" type="text" class="am-form-field" placeholder="DD-MM-YYYY"
                                                            readonly />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr style="height: 20px;">
                        </tr>
                        <tr>
                            <td>
                                <table style="width: 100%;">
                                    <tr>
                                        <td style="width: 400px;">
                                            <select id="select-question-2" placeholder="<%= Resources.Lang.Res_Register_SecurityQuestion2%>"
                                                data-am-selected="{btnWidth: '400'}">
                                            </select>
                                        </td>
                                        <td style="width: 20px;">
                                        </td>
                                        <td>
                                            <table style="width: 100%;">
                                                <tr>
                                                    <td id="td-input-q2">
                                                        <input id="input-q2" type="text" class="am-form-field" maxlength="30" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td id="td-datapicker-q2" hidden="hidden">
                                                        <input id="datapicker-q2" type="text" class="am-form-field" placeholder="DD-MM-YYYY"
                                                            readonly />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr style="height: 20px;">
                        </tr>
                        <tr>
                            <td>
                                <button id="button-verify" type="button" class="am-btn am-btn-secondary am-round button-all"
                                    onclick="VerifyUser()" style="float: right">
                                    <%= Resources.Lang.Res_ChangeEmail_CheckUser%></button>
                            </td>
                        </tr>
                        <tr style="height: 20px;">
                        </tr>
                    </table>
                </td>
            </tr>
            <%-- 发送验证码 --%>
            <tr id="tr-email" style="display: none;">
                <td>
                    <table style="width: 100%;">
                        <tr>
                            <td>
                                <label class="g-font-1">
                                    <%= Resources.Lang.Res_ChangeEmail_NewEmail%></label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <input id="input-email" type="email" class="am-form-field" maxlength="30" />
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
                        <tr style="height: 30px;">
                        </tr>
                    </table>
                </td>
            </tr>
            <%-- 修改邮箱等信息 --%>
            <tr id="tr-update" style="display: none;">
                <td>
                    <table style="width: 100%;">
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
                        <tr>
                            <td>
                                <label class="g-font-1">
                                    <%= Resources.Lang.Res_ChangeEmail_ContactName%></label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <input id="input-name" type="text" class="am-form-field" maxlength="20" />
                            </td>
                        </tr>
                        <tr style="height: 20px;">
                        </tr>
                        <tr>
                            <td>
                                <label class="g-font-1">
                                    <%= Resources.Lang.Res_ChangeEmail_PhoneNumber %></label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <input id="input-phone" type="text" class="am-form-field" maxlength="20" />
                            </td>
                        </tr>
                        <tr style="height: 20px;">
                        </tr>
                        <tr>
                            <td>
                                <button id="button-update-email" type="button" class="am-btn am-btn-secondary am-round button-all"
                                    onclick="UpdateEmail()" style="float: right">
                                    <%= Resources.Lang.Res_ChangeEmail_UpdateEmail%></button>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
    <%-- 变更email成功模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="modal-success">
        <div class="am-modal-dialog">
            <div class="am-modal-hd">
                <%= Resources.Lang.Res_ChangeEmail_Success%><a href="javascript: void(0)" class="am-close am-close-spin"
                    data-am-modal-close> &times;</a>
            </div>
            <div class="am-modal-bd">
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <button type="button" class="am-btn am-btn-secondary am-round button-all" onclick="Refresh()"
                                style="float: right">
                                <%= Resources.Lang.Res_Global_OK%></button>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
