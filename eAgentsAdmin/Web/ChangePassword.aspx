<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ChangePassword.aspx.cs" Inherits="PropertyOneAppWeb.Web.ChangePassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("/Image/icon_ChangePassword.png", "<%= Resources.Lang.Res_Menu_ChangePsw %>");
        });

        /* 保存密码 */
        function savepsw() {
            try {
                $("#button-change-psw").prop("disabled", true);
                var pswold = $("#input-psw-old").val();
                var pswnew = $("#input-psw-new").val();
                var pswnew2 = $("#input-psw-new2").val();

                if (checknotnull(pswold) == false) {
                    throw '<%= Resources.Error.ERR_PSW_CURRENT %>';
                }

                if (checknotnull(pswnew) == false || checknotnull(pswnew2) == false) {
                    throw '<%= Resources.Error.ERR_PSW_NEW %>';
                }

                if (pswnew != pswnew2) {
                    throw '<%= Resources.Error.ERR_PSW_SAME %>';
                }

                /* 提交密码 */
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Web/ChangePassword.aspx",
                    data: {
                        "action": "change",
                        "pswold": pswold,
                        "pswnew": pswnew
                    },
                    success: function (data) {
                        $("#button-change-psw").prop("disabled", false);
                        if (data == "ok") {
                            $("#input-psw-old").val("");
                            $("#input-psw-new").val("");
                            $("#input-psw-new2").val("");
                            showAlert("Notice", '<%= Resources.Lang.Res_ChangePsw_Success %>');
                        }
                        else {
                            showAlert("Notice", '<%= Resources.Error.ERR_PSW_CHANGE %>' + data);
                        }
                    },
                    error: function (data) {
                        $("#button-change-psw").prop("disabled", false);
                        AlertJqueryAjaxError(data);
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
        <table style="width: 100%;">
            <tr>
                <td>
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_ChangePsw_UserName%></label>
                </td>
            </tr>
            <tr>
                <td>
                    <input id="input-userid" type="text" class="am-form-field" maxlength="20" readonly="readonly"
                        value='<% =Session["loginname"] %>' />
                </td>
            </tr>
            <tr style="height: 20px">
            </tr>
            <tr>
                <td>
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_ChangePsw_CurrentPsw%></label>
                </td>
            </tr>
            <tr>
                <td>
                    <input id="input-psw-old" type="password" class="am-form-field" maxlength="20" />
                </td>
            </tr>
            <tr style="height: 20px">
            </tr>
            <tr>
                <td>
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_ChangePsw_NewPsw1%>
                    </label>
                </td>
            </tr>
            <tr>
                <td>
                    <input id="input-psw-new" type="password" class="am-form-field" maxlength="20" />
                </td>
            </tr>
            <tr style="height: 20px">
            </tr>
            <tr>
                <td>
                    <label class="g-font-1">
                        <%= Resources.Lang.Res_ChangePsw_NewPsw2%>
                    </label>
                </td>
            </tr>
            <tr>
                <td>
                    <input id="input-psw-new2" type="password" class="am-form-field" maxlength="20" />
                </td>
            </tr>
            <tr style="height: 80px">
            </tr>
            <tr>
                <td style="text-align: right;">
                    <%-- 按钮 --%>
                    <button id="button-change-psw" type="button" class="am-btn am-btn-secondary am-round button-all "
                        onclick="savepsw()">
                        <%= Resources.Lang.Res_ChangePsw_Button%>
                    </button>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
