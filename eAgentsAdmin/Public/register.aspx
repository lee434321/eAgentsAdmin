<%@ Page Title="" Language="C#" MasterPageFile="~/SitePublic.Master" AutoEventWireup="true"
    CodeBehind="register.aspx.cs" Inherits="PropertyOneAppWeb.Public.register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("/Image/icon_Notices.png", "<%= Resources.Lang.Res_Menu_Register%>");
            $("#a-disclaimer").prop("href", disclaimerUrl);

            /* 初始化用户校验界面 */
            InitUserCheck();
        });

        function Register() {
            try {
                /* 是否已选择免责声明 */
                /*
                if ($("#checkbox-register").prop("checked") == false) {
                throw "Please read and agree to the Terms and Conditions";
                }
                */

                /* 是否已输入租约号 */
                var leasenum = $("#input-lease").val();
                if (checknotnull(leasenum) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_LEASE %>';
                }

                /* 是否已输入Email */
                var email = $("#input-email").val();
                if (checknotnull(email) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_EMAIL %>';
                }

                /* 是否已输入Contact Name */
                var contactName = $("#input-name").val();
                if (checknotnull(contactName) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_CONTACT %>';
                }

                /* 是否已输入Phone Number */
                var phone = $("#input-phone").val();
                if (checknotnull(phone) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_PHONE %>';
                }

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

                /* 将注册按钮置为无效 */
                $("#button-register").prop("disabled", true);

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Public/register.aspx",
                    data: {
                        "action": "register",
                        "leasenum": leasenum,
                        "statementnum": statementnum,
                        "balance": balance,
                        "expiredate": expiredate,
                        "rental": rental,
                        "email": email,
                        "name": contactName,
                        "phone": phone
                    },
                    success: function (data) {
                        $("#button-register").prop("disabled", false);
                        if (data == "ok") {
                            showModal("modal-success", 600, false);
                        }
                        else {
                            showAlert("Notice", '<%= Resources.Error.ERR_REG_FAILED %>' + data);
                        }
                    },
                    error: function (data) {
                        $("#button-register").prop("disabled", false);
                        AlertJqueryAjaxError(data);
                    }
                });

            }
            catch (ex) {
                $("#button-register").prop("disabled", false);
                showAlert("Notice", ex);
            }
        };

        /* 跳转到注册页面 */
        function GoLogin() {
            window.location.href = "/Login.aspx";
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
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- Rider --%>
    <div>
        <table style="width: 100%;">
            <tr>
                <td>
                    <%= Resources.Lang.Res_Rider%>
                </td>
            </tr>
        </table>
    </div>
    <div style="padding: 50px 200px 20px 200px;">
        <table style="width: 100%;">
            <tr>
                <td>
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_Register_LeaseNumber%></label>
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
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_Register_Email%></label>
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
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_ChangeEmail_ContactName %></label>
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
            <tr style="display: none;">
                <td>
                    <%= Resources.Lang.Res_Register_CheckBox_Please%>
                    <a id="a-disclaimer" href="/Public/disclaimer.aspx" target="_blank">
                        <%= Resources.Lang.Res_Register_CheckBox_ClickHere%></a>
                    <%= Resources.Lang.Res_Register_CheckBox_ReadTerms%>
                </td>
            </tr>
            <tr style="display: none;">
                <td>
                    <label>
                        <input id="checkbox-register" type="checkbox" />
                        <%= Resources.Lang.Res_Register_CheckBox%>
                    </label>
                </td>
            </tr>
            <tr style="height: 20px;">
            </tr>
            <tr>
                <td>
                    <table style="width: 100%;">
                        <tr>
                            <td>
                                <button type="button" class="am-btn am-btn-default am-round button-all" onclick="showModal('modal-cancel', 600, false);"
                                    style="float: right">
                                    <%= Resources.Lang.Res_Register_Cancel%></button>
                            </td>
                            <td style="width: 130px;">
                                <button id="button-register" type="button" class="am-btn am-btn-secondary am-round button-all"
                                    onclick="Register()" style="float: right;">
                                    <%= Resources.Lang.Res_Register_Register%></button>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
    <%-- 注册成功模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="modal-success">
        <div class="am-modal-dialog">
            <div class="am-modal-hd">
                <%= Resources.Lang.Res_Register_Success%><a href="javascript: void(0)" class="am-close am-close-spin"
                    data-am-modal-close> &times;</a>
            </div>
            <div class="am-modal-bd">
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <button type="button" class="am-btn am-btn-secondary am-round button-all" onclick="GoLogin()"
                                style="float: right">
                                Ok</button>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
    <%-- 取消注册模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="modal-cancel">
        <div class="am-modal-dialog">
            <div class="am-modal-hd" style="padding: 30px 0px 0px 0px;">
                <%= Resources.Lang.Res_Register_CancelProcess%><a href="javascript: void(0)" class="am-close am-close-spin"
                    data-am-modal-close> &times;</a>
            </div>
            <div class="am-modal-bd" style="padding: 30px 0px 30px 0px;">
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <button type="button" class="am-btn am-btn-default am-round button-all" onclick="closeModal('modal-cancel')"
                                style="float: right;">
                                <%= Resources.Lang.Res_Register_ButtonContinue%></button>
                        </td>
                        <td style="width: 20px;">
                        </td>
                        <td>
                            <button type="button" class="am-btn am-btn-secondary am-round button-all" onclick="GoLogin()"
                                style="float: left;">
                                <%= Resources.Lang.Res_Register_ButtonCancel%></button>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
