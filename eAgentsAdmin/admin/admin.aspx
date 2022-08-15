<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystemBootcss.Master" AutoEventWireup="true"
    CodeBehind="admin.aspx.cs" Inherits="PropertyOneAppWeb.admin.admin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("/Image/icon_Notices.png", "Admin Configuration");

            /// -- start --
            var d = new Date();
            var month = d.getMonth() + 1;
            var day = d.getDate();
            var logFile = "Log_" + d.getFullYear() + (month < 10 ? '0' + month : month) + (day < 10 ? '0' + day : day) + ".txt";
            $("#log").text(logFile);
            $.ajax({
                url: "../log/" + logFile,
                type: "GET",
                success: function (r) {
                    var str = "hello \r\n world!";
                    var patt1 = /\r\n/g;

                    console.log(str.search(patt1));
                    $("#log").text(r);
                },
                error: function (err) {
                    $("#log").text(err.responseText);
                }
            });
            /// --  end  --

        });

        /* 校验密码 */
        function VerifyPsw() {
            try {
                var psw = $("#input-psw").val();
                if (checknotnull(psw) == false) {
                    throw "Please input admin password";
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/admin/admin.aspx",
                    data: {
                        "action": "verify",
                        "psw": psw
                    },
                    success: function (data) {
                        if (checknotnull(data)) {
                            if (data == "ok") {
                                $("#table-psw").hide();
                                $("#table-config").show();
                            }
                            else {
                                $("#table-psw").show();
                                $("#table-config").hide();
                                showAlert("Notice", data);
                            }
                        }
                    },
                    error: function (data) {
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            catch (ex) {
                showAlert("Notice", ex);
            }
        };

        /* 运行 Notification of eStatement Service (Run once only) */
        function RunNotification() {
            showLoading();
            try {
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/admin/admin.aspx",
                    data: {
                        "action": "RunNotification"
                    },
                    success: function (data) {
                        closeLoading();
                        if (checknotnull(data)) {
                            if (data == "ok") {
                                showAlert("Notice", "Finished");
                            }
                            else {
                                showAlert("Notice", data);
                            }
                        }
                    },
                    error: function (data) {
                        closeLoading();
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            catch (ex) {
                closeLoading();
                showAlert("Notice", ex);
            }
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 20px 200px 20px 200px;">
        <table id="table-psw" style="width: 100%;">
            <tr>
                <td style="text-align: left;">
                    <label>
                        Input admin password</label>
                </td>
            </tr>
            <tr>
                <td>
                    <input id="input-psw" type="password" class="am-form-field" maxlength="30" />
                </td>
            </tr>
            <tr style="height: 30px;">
            </tr>
            <tr>
                <td>
                    <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="VerifyPsw()"
                        style="float: right">
                        Next</button>
                </td>
            </tr>
        </table>
        <table id="table-config" style="width: 100%; display: none;">
            <tr>
                <td style="text-align: left;">
                    <label>
                        Notification of eStatement Service (Run once only)</label>
                </td>
                <td style="width: 300px;">
                    <button type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="RunNotification()"
                        style="float: right">
                        Run</button>
                </td>
            </tr>
        </table>
    </div>
    <pre id="log">
        <textarea rows="50" class="am-form-field"></textarea>        
    </pre>    
    <asp:Label ID="lblConnection" runat="server"></asp:Label>
    <br />
    <form runat="server">    
        <asp:TextBox ID="tbxConnStr" runat="server" Columns=100></asp:TextBox>    
        <asp:Button ID="btnTestConn" runat="server" Text="Test Connection" onclick="btnTestConn_Click" />
    </form>
</asp:Content>
