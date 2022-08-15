<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Leaselink.aspx.cs" Inherits="PropertyOneAppWeb.Web.Leaselink" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            /* 添加默认模板中的一些元素 */
            $("#div-title").show();
            $("#div-title").addClass("global_color_navyblue");
            $("#div-content").addClass("global_color_lightblue");

            changeNav("/Image/icon_MyProfile.png", "<%= Resources.Lang.Res_Menu_LinkLease %>");
            $('#datapicker').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });
            search();

            $('#datapicker-q1').datepicker({ format: 'yyyy-mm-dd', locale: 'en_US' });
            $('#datapicker-q2').datepicker({ format: 'yyyy-mm-dd', locale: 'en_US' });

            /* 添加问题 */
            AddQuestions("select-question-1");

            /* 问题1事件 */
            $("#select-question-1").on('change', function () {
                ChangeQuestion('select-question-1', 'select-question-2')
                $("#select-question-2").selected('enable');
                $("#input-q2").prop("disabled", false);

                var val = $("#select-question-1").val();
                if (val == "ed") {
                    $("#td-input-q1").hide();

                    $("#datapicker-q1").remove();
                    $("#td-datapicker-q1").append('<input id="datapicker-q1" type="text" class="am-form-field" placeholder="DD-MM-YYYY" readonly />');
                    $('#datapicker-q1').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });
                    $("#td-datapicker-q1").show();
                }
                else {
                    $("#input-q1").val("");
                    $("#td-input-q1").show();
                    $("#td-datapicker-q1").hide();
                }

                /* 清除问题2的内容 */
                $("#input-q2").val("");
                $("#datapicker-q2").remove();
                $("#td-datapicker-q2").append('<input id="datapicker-q2" type="text" class="am-form-field" placeholder="DD-MM-YYYY" readonly />');
                $('#datapicker-q2').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });
            });

            /* 问题2事件 */
            $("#select-question-2").on('change', function () {
                var val = $("#select-question-2").val();
                if (val == "ed") {
                    $("#td-input-q2").hide();

                    $("#datapicker-q2").remove();
                    $("#td-datapicker-q2").append('<input id="datapicker-q2" type="text" class="am-form-field" placeholder="DD-MM-YYYY" readonly />');
                    $('#datapicker-q2').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });
                    $("#td-datapicker-q2").show();
                }
                else {
                    $("#input-q2").val("");
                    $("#td-input-q2").show();
                    $("#td-datapicker-q2").hide();
                }
            });

            /* 禁用问题2 */
            $("#select-question-2").selected('disable');
            $("#input-q2").prop("disabled", true);
        });

        var firstPageload = true; /* 是否第一次加载页面 */
        function search() {
            if (firstPageload == true) {  /* 如果是第一次 */
                $("#grid-data").bootgrid({
                    navigation: 0,
                    rowCount: -1,
                    sorting: false,
                    ajax: true,
                    url: "/Web/Leaselink.aspx",
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        return {
                            action: "search"
                        };
                    },
                    formatters: {
                        "edit": function (column, row) {
                            var leasenum = row.leasenum;
                            return '<a class="am-badge am-badge-danger am-radius" href="JavaScript:void(0);" onclick="ShowDelModal(\'' + leasenum + '\')">Delete</a>';
                        }
                    }
                });
                firstPageload = false;
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");
            };
        };

        /* 新增账号对应的租约号 */
        function AddLink() {
            try {
                var leasenum = $("#input-add-leasenum").val();

                /* 是否已输入租约号 */
                if (checknotnull(leasenum) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_LEASE %>';
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

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Web/Leaselink.aspx",
                    data: {
                        "action": "link",
                        "leasenum": leasenum,
                        "statementnum": statementnum,
                        "balance": balance,
                        "expiredate": expiredate,
                        "rental": rental,
                    },
                    success: function (data) {
                        if (data == "ok") {
                            location.reload();
                        }
                        else {
                            alert('<%= Resources.Error.ERR_LINK_LEASE %>' + data);
                        }
                    },
                    error: function (data) {
                        AlertJqueryAjaxError(data);
                    }
                });
            }
            catch (ex) {
                alert(ex);
            }
        };

        /* 显示删除窗口 */
        function ShowDelModal(leasenum) {
            $("#label-del-lease").text(leasenum);
            showModal("model-del-confirm", 600, false);
        };

        /* 显示新增窗口 */
        function ShowAddModal() {
            showModal("doc-modal-add", 800, false);
        };

        /* 删除对应的租约号 */
        function DelLink() {
            var leasenum = $("#label-del-lease").text();

            $.ajax({
                cache: false,
                type: "POST",
                url: "/Web/Leaselink.aspx",
                data: {
                    "action": "delete",
                    "leasenum": leasenum
                },
                success: function (data) {
                    if (data == "ok") {
                        location.reload();
                    }
                    else {
                        alert('<%= Resources.Error.ERR_LINKE_LEASE_DEL %>' + data);
                    }
                },
                error: function (data) {
                    AlertJqueryAjaxError(data);
                }
            });
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
    <table style="width: 100%;">
        <tr>
            <td>
                <button id="button-new-lease" type="button" class="am-btn am-btn-secondary am-round button-all"
                    style="float: right;" onclick="ShowAddModal()">
                    <%= Resources.Lang.Res_LinkLease_Button%></button>
            </td>
        </tr>
        <tr style="height: 20px;">
        </tr>
        <tr>
            <td>
                <%--bootgrid--%>
                <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid"
                    data-toggle="bootgrid">
                    <thead>
                        <tr>
                            <th data-column-id="leasenum" data-css-class="t-bg-cell2" data-header-css-class="t-bg-head2">
                                <%= Resources.Lang.Res_Menu_LinkLease %>
                            </th>
                            <th data-column-id="premises" data-css-class="t-bg-cell2" data-header-css-class="t-bg-head2">
                                <%= Resources.Lang.Res_LinkLease_Premises%>
                            </th>
                            <th data-column-id="edit" data-formatter="edit" data-css-class="t-bg-cell2" data-header-css-class="t-bg-head3">
                                <%= Resources.Lang.Res_LinkLease_Edit%>
                            </th>
                        </tr>
                    </thead>
                </table>
            </td>
        </tr>
    </table>
    <%-- 新增租约模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="doc-modal-add">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                <%= Resources.Lang.Res_LinkLease_LinkNewLease%><a href="javascript: void(0)" class="am-close am-close-spin"
                    data-am-modal-close> &times;</a>
            </div>
            <div class="am-modal-bd">
                <div style="padding: 20px 50px 20px 50px;">
                    <table style="width: 100%;">
                        <tr>
                            <td style="text-align: left;">
                                <label class="g-font-1">
                                    <%= Resources.Lang.Res_LinkLease_LeaseNumber%></label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <input id="input-add-leasenum" type="text" class="am-form-field" maxlength="20" />
                            </td>
                        </tr>
                        <tr style="height: 30px;">
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
                        <tr>
                            <td>
                                <button id="button1" type="button" style="float: right;" class="am-btn am-btn-secondary am-round button-all"
                                    onclick="AddLink()">
                                    <%= Resources.Lang.Res_LinkLease_LinkNewLease%></button>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <%-- 确认删除模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="model-del-confirm">
        <div class="am-modal-dialog">
            <div class="am-modal-hd">
                <%= Resources.Lang.Res_LinkLease_Modal_DeleteLease%><a href="javascript: void(0)"
                    class="am-close am-close-spin" data-am-modal-close> &times;</a>
            </div>
            <div class="am-modal-bd">
                <label id="label-del-lease">
                </label>
                <table style="width: 100%;">
                    <tr>
                        <td>
                            <button type="button" style="float: right;" class="am-btn am-btn-secondary am-round button-all"
                                onclick="DelLink()">
                                <%= Resources.Lang.Res_LinkLease_Button_Delete%></button>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
