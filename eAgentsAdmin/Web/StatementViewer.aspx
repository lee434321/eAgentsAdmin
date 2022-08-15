<%@ Page Title="" Language="C#" MasterPageFile="~/SiteBlank.Master" AutoEventWireup="true"
    CodeBehind="StatementViewer.aspx.cs" Inherits="PropertyOneAppWeb.Web.StatementViewer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .sv-td-title
        {
            font-weight: bold;
            text-align: center;
            text-decoration: underline;
            font-size: 2.0rem;
        }
        .sv-td-text-left
        {
            vertical-align: top;
        }
        .sv-td-text-right
        {
            vertical-align: top;
        }
        .sv-td-blank
        {
            height: 20px;
        }
        .sv-td-mao
        {
            vertical-align: middle;
            text-align: center;
        }
        .sv-td-xiahua
        {
            vertical-align: bottom;
        }
        .sv-td-line
        {
            border: 0.5px solid;
        }
    </style>
    <script type="text/javascript" src="/js/thenBy.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            showLoading();
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Web/StatementViewer.aspx",
                data: {
                    "action": "loadstatement"
                },
                success: function (data) {
                    try {
                        var jsonData = jQuery.parseJSON(data);
                        loadstatement(jsonData);

                        /* 添加上期结余 */
                        appendtrans("", "", "Previous balance 上期結餘", "", jsonData.previousbalance);

                        /* 获取全部trans数据 */
                        var sortarray = new Array();
                        for (var k = 0; k < jsonData.paymentinfo.length; k++) {
                            sortarray.push(new modeltrans(formatDateByType(jsonData.paymentinfo[k].paymentdate, "yyyy/MM/dd"), jsonData.paymentinfo[k].paymenttrans, jsonData.paymentinfo[k].paymentchargedesc));
                        }
                        for (var i = 0; i < jsonData.transactioninfo.length; i++) {
                            sortarray.push(new modeltrans(formatDateByType(jsonData.transactioninfo[i].transactiondate, "yyyy/MM/dd"), jsonData.transactioninfo[i].transactionno, jsonData.transactioninfo[i].chargedesc));
                        }

                        /* 按date，transno, charge排序 */
                        sortarray.sort(
                            firstBy(function (v1, v2) { return v1.date.localeCompare(v2.date); })
                            .thenBy(function (v1, v2) { return v1.no.localeCompare(v2.no); })
                            .thenBy(function (v1, v2) { return v1.charge.localeCompare(v2.charge); })
                        );

                        /* 向table中插入排序后的数据 */
                        for (var m = 0; m < sortarray.length; m++) {
                            /* 添加Payment 付款 */
                            for (var k = 0; k < jsonData.paymentinfo.length; k++) {
                                if (checkinsertdata(sortarray[m], jsonData.paymentinfo[k].paymentdate, jsonData.paymentinfo[k].paymenttrans, jsonData.paymentinfo[k].paymentchargedesc) == true) {
                                    appendtrans(jsonData.paymentinfo[k].paymentdate, jsonData.paymentinfo[k].paymenttrans, jsonData.paymentinfo[k].paymentchargedesc, jsonData.paymentinfo[k].paymentdesc, jsonData.paymentinfo[k].paymentamount);
                                    /* 如果是 Payment需要显示History */
                                    for (var j = 0; j < jsonData.paymentinfo[k].paymenthistory.length; j++) {
                                        appendpaymenthistory(jsonData.paymentinfo[k].paymenthistory[j].historychargedesc,
                                            jsonData.paymentinfo[k].paymenthistory[j].historypaymentdesc,
                                            jsonData.paymentinfo[k].paymenthistory[j].historyamount);
                                    }
                                    break;
                                }
                            }

                            /* 添加transactioninfo */
                            for (var i = 0; i < jsonData.transactioninfo.length; i++) {
                                if (checkinsertdata(sortarray[m], jsonData.transactioninfo[i].transactiondate, jsonData.transactioninfo[i].transactionno, jsonData.transactioninfo[i].chargedesc) == true) {
                                    appendtrans(jsonData.transactioninfo[i].transactiondate,
                                        jsonData.transactioninfo[i].transactionno,
                                        jsonData.transactioninfo[i].chargedesc,
                                        jsonData.transactioninfo[i].paymentdesc,
                                        jsonData.transactioninfo[i].transactionamount);
                                    break;
                                }
                            }
                        }

                        /* 添加賬單結餘 */
                        for (var i = 0; i < 5; i++) {
                            appendtrans("", "", "", "", "");
                        }
                        appendtrans("", "", "Statement balance 賬單結餘：", "", jsonData.statementbalance);
                    }
                    catch (ex) {
                        alert(ex.message);
                    }
                    finally {
                        closeLoading();
                    }
                },
                error: function (data) {
                    AlertJqueryAjaxError(data);
                    closeLoading();
                }
            });
        });

        /* 校验插入table的数据是否符合要求 */
        function checkinsertdata(modeltrans, date, no, charge) {
            /* alert(modeltrans.date + "||" + modeltrans.no + "||" + modeltrans.charge + "||" + formatDateByType(date, "yyyy/MM/dd") + "||" + no + "||" + charge);*/
            try {
                if (modeltrans.date == formatDateByType(date, "yyyy/MM/dd") && modeltrans.no == no && modeltrans.charge == charge) {
                    return true;
                }
                else {
                    return false;
                }
            }
            catch (ex) {
                return false;
            }
        };

        /* 动态添加transction */
        function appendtrans(date, transno, chargedesc, paymentdesc, hkd) {
            if (date == "" && transno == "" && chargedesc == "" && paymentdesc == "" && hkd == "") {
                hkd = "";
            }
            else {
                hkd = formatMoney(hkd, 2, "");
            }

            var pdate = "<p class='p-statementviewer'>" + formatDate(date) + "</p>";
            var ptransno = "<p class='p-statementviewer'>" + transno + "</p>";
            var ppaymentdesc = "<p class='p-statementviewer'>" + paymentdesc + "</p>";
            var phistoryamount = "<p class='p-statementviewer'></p>";

            /* 如果是賬單結餘显示粗体字 */
            if (chargedesc == "Statement balance 賬單結餘：") {
                var pchargedesc = "<p class='p-statementviewer f-v-bold'>" + chargedesc + "</p>";
                var phkd = "<p class='p-statementviewer f-v-bold'>" + hkd + "</p>";
            } else {
                var pchargedesc = "<p class='p-statementviewer'>" + chargedesc + "</p>";
                var phkd = "<p class='p-statementviewer'>" + hkd + "</p>";
            }

            $("#table-trans").append('<tr><td class="t-sv-td2 t-sv-td3 t-sv-td5" style="width: 11%">' + pdate
                + '</td><td class="t-sv-td2 t-sv-td3 t-sv-td5" style="width: 15%">' + ptransno
                + '</td><td class="t-sv-td2 t-sv-td5" style="width: 61%"><table class="t-sv2"><tr><td class="t-sv-td5 t-sv-td6" style="width: 50%">' + pchargedesc
                + '</td><td class="t-sv-td5" style="width: 50%">' + ppaymentdesc
                + '</td></tr></table></td><td class="t-sv-td2 t-sv-td4 t-sv-td5 t-sv-td7" style="width: 13%">' + phkd
                + '</td></tr>');

        };

        /* 动态添加payment history */
        function appendpaymenthistory(chargedesc, paymentdesc, historyamount) {
            historyamount = formatMoney(historyamount, 2, "");
            var pdate = "<p class='p-statementviewer'></p>";
            var ptransno = "<p class='p-statementviewer'></p>";
            var pchargedesc = "<p class='p-statementviewer p-statementviewer-history'>" + chargedesc + "</p>";
            var ppaymentdesc = "<p class='p-statementviewer p-statementviewer-history'>" + paymentdesc + "</p>";
            var phistoryamount = "<p class='p-statementviewer p-statementviewer-history'>" + historyamount + "</p>";
            var phkd = "<p class='p-statementviewer'></p>";

            $("#table-trans").append('<tr><td class="t-sv-td2 t-sv-td5" style="width: 11%">' + pdate
                + '</td><td class="t-sv-td2 t-sv-td5" style="width: 15%">' + ptransno
                + '</td><td class="t-sv-td2 t-sv-td5" style="width: 61%"><table class="t-sv2"><tr><td class="t-sv-td5 t-sv-td6" style="width: 50%">' + pchargedesc
                + '</td><td class="t-sv-td5" style="width: 25%">' + ppaymentdesc
                + '</td><td class="t-sv-td4 t-sv-td5 t-sv-td7" style="width: 25%">' + phistoryamount
                + '</td></tr></table></td><td class="t-sv-td2 t-sv-td5 t-sv-td7" style="width: 13%">' + phkd
                + '</td></tr>');
        };

        /* 加载对账单内容 */
        function loadstatement(jsonData) {
            $("#label-customername").text(jsonData.customername);
            $("#label-contactaddress1").text(jsonData.contactaddress1);
            $("#label-contactaddress2").text(jsonData.contactaddress2);
            $("#label-contactaddress3").text(jsonData.contactaddress3);
            $("#label-contactaddress4").text(jsonData.contactaddress4);

            $("#label-ppsmerchantcode").text(jsonData.ppsmerchantcode);
            $("#label-accountnumber").text(jsonData.accountnumber);
            $("#label-statementnumber").text(jsonData.statementnumber);
            $("#label-statementdate").text(formatDate(jsonData.statementdate));
            $("#label-duedate").text(formatDate(jsonData.duedate));
            $("#label-cutoffdate").text(formatDate(jsonData.cutoffdate));

            $("#label-companyname").text(jsonData.companyname);
            $("#label-leasenumber-en").text(jsonData.leasenumber);
            $("#label-leasenumber-cn").text(jsonData.leasenumber);
            $("#label-premise-en").text(jsonData.premise);
            $("#label-premise-cn").text(jsonData.premise4);

            $("#label-previousbalance").text(formatMoney(jsonData.previousbalance, 2, ""));
            $("#label-payment").text(formatMoney(jsonData.payment, 2, ""));
            $("#label-currentdue").text(formatMoney(jsonData.currentdue, 2, ""));
            $("#label-adjustment").text(formatMoney(jsonData.adjustment, 2, ""));
            $("#label-overdueinterest").text(formatMoney(jsonData.overdueinterest, 2, ""));
            $("#label-total").text(formatMoney(jsonData.total, 2, ""));

            var overdueprime = "2.Interest will be charged on overdue account at ";
            var overdueprimech = "2.逾期付款將須繳交";
            $("#label-overdueprime").text(overdueprime + jsonData.overdueprime);
            $("#label-overdueprimech").text(overdueprimech + jsonData.overdueprimech);

            $("#label-eoe-accountnumber").text(jsonData.accountnumber);
            $("#label-eoe-customername").text(jsonData.customername);
            $("#label-eoe-statementnumber").text(jsonData.statementnumber);
            $("#label-eoe-statementdate").text(formatDate(jsonData.statementdate));
            $("#label-eoe-duedate").text(formatDate(jsonData.duedate));
            $("#label-eoe-leasenumber").text(jsonData.leasenumber);
            $("#label-eoe-total").text(formatMoney(jsonData.total, 2, ""));
        };

        /* 下载pdf */
        function downloadpdf() {
            showLoading();
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Web/StatementViewer.aspx",
                data: {
                    "action": "genpdf"
                },
                success: function (data) {
                    try {
                        if (data == "ok") {
                            var form = $("<form>");
                            form.attr("style", "display:none");
                            form.attr("target", "");
                            form.attr("method", "post");
                            form.attr("action", "StatementViewer.aspx");
                            var input1 = $("<input>");
                            input1.attr("type", "hidden");
                            input1.attr("name", "downloadpdf");
                            input1.attr("value", (new Date()).getMilliseconds());
                            $("body").append(form);
                            form.append(input1);
                            form.submit();
                        }
                        else {
                            throw '<%= Resources.Error.ERR_PDF_DOWNLOAD %>' + data;
                        }
                    }
                    catch (ex) {
                        alert(ex);
                    }
                    finally {
                        closeLoading();
                    }
                },
                error: function (data) {
                    AlertJqueryAjaxError(data);
                    closeLoading();
                }
            });
        };

        /* 定义数据结构 */
        function modeltrans(date, no, charge) {
            this.date = date;
            this.no = no;
            this.charge = charge;
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="div-statementviewer-background">
        <div class="div-statementviewer-main">
            <div class="am-g am-g-collapse">
                <%-- logo --%>
                <div class="am-u-sm-12">
                    <img alt="" src="/Image/logo.png" width="420px" height="30px" />
                </div>
                <div class="am-u-sm-12" style="height: 10px">
                </div>
                <div class="am-u-sm-12">
                    <%-- 表头左侧 --%>
                    <div class="am-u-sm-6">
                        <%-- customername --%>
                        <div class="am-u-sm-12">
                            <label id="label-customername" class="label-statementviewer-font1">
                            </label>
                        </div>
                        <%-- contactaddress1 --%>
                        <div class="am-u-sm-12">
                            <label id="label-contactaddress1" class="label-statementviewer-font1">
                            </label>
                        </div>
                        <%-- contactaddress2 --%>
                        <div class="am-u-sm-12">
                            <label id="label-contactaddress2" class="label-statementviewer-font1">
                            </label>
                        </div>
                        <%-- contactaddress3 --%>
                        <div class="am-u-sm-12">
                            <label id="label-contactaddress3" class="label-statementviewer-font1">
                            </label>
                        </div>
                        <%-- contactaddress4 --%>
                        <div class="am-u-sm-12">
                            <label id="label-contactaddress4" class="label-statementviewer-font1">
                            </label>
                        </div>
                    </div>
                    <%-- 表头右侧 --%>
                    <div class="am-u-sm-6">
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-7">
                                <label class="label-statementviewer-font1">
                                    PPS Merchant Code 繳費聆商戶編號
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :</label>
                            </div>
                            <div class="am-u-sm-4" style="text-align: right">
                                <label id="label-ppsmerchantcode" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-7">
                                <label class="label-statementviewer-font1">
                                    Account Number 賬戶號碼
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :</label>
                            </div>
                            <div class="am-u-sm-4" style="text-align: right">
                                <label id="label-accountnumber" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-7">
                                <label class="label-statementviewer-font1">
                                    Statement Number 月結單號碼
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :</label>
                            </div>
                            <div class="am-u-sm-4" style="text-align: right">
                                <label id="label-statementnumber" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-7">
                                <label class="label-statementviewer-font1">
                                    Statement Date 月結單日期
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :</label>
                            </div>
                            <div class="am-u-sm-4" style="text-align: right">
                                <label id="label-statementdate" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-7">
                                <label class="label-statementviewer-font1">
                                    Due Date 到期日
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :</label>
                            </div>
                            <div class="am-u-sm-4" style="text-align: right">
                                <label id="label-duedate" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-7">
                                <label class="label-statementviewer-font1">
                                    Cut-off Date 截數日
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :</label>
                            </div>
                            <div class="am-u-sm-4" style="text-align: right">
                                <label id="label-cutoffdate" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
                <%-- 租約號碼及租賃物業区域 --%>
                <div class="am-u-sm-12 div-statementviewer-company">
                    <div class="am-u-sm-12">
                        <label id="label-companyname" class="f-v-bold">
                        </label>
                    </div>
                    <div class="am-u-sm-12">
                        <div class="am-u-sm-3">
                            <label class="label-statementviewer-font1">
                                Lease No and Premises:
                            </label>
                        </div>
                        <div class="am-u-sm-9">
                            <label id="label-leasenumber-en" class="label-statementviewer-font1">
                            </label>
                            <label class="label-statementviewer-font1">
                                -
                            </label>
                            <label id="label-premise-en" class="label-statementviewer-font1">
                            </label>
                        </div>
                    </div>
                    <div class="am-u-sm-12" style="height: 10px">
                    </div>
                    <div class="am-u-sm-12">
                        <div class="am-u-sm-3">
                            <label class="label-statementviewer-font1">
                                租約號碼及租賃物業:
                            </label>
                        </div>
                        <div class="am-u-sm-9">
                            <label id="label-leasenumber-cn" class="label-statementviewer-font1">
                            </label>
                            <label class="label-statementviewer-font1">
                                -
                            </label>
                            <label id="label-premise-cn" class="label-statementviewer-font1">
                            </label>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12" style="height: 10px">
                </div>
                <%-- 表格区域 --%>
                <div class="am-u-sm-12">
                    <table id="table-trans" class="t-sv">
                        <tr>
                            <td class="t-sv-td" style="width: 9%">
                                <table class="t-sv2">
                                    <tr>
                                        <td class="t-sv-td3">
                                            Date
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="t-sv-td3">
                                            日期
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td class="t-sv-td" style="width: 15%">
                                <table class="t-sv2">
                                    <tr>
                                        <td class="t-sv-td3">
                                            Transaction No
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="t-sv-td3">
                                            交易號碼
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td class="t-sv-td" style="width: 61%">
                                <table class="t-sv2">
                                    <tr>
                                        <td class="t-sv-td3">
                                            Particulars
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="t-sv-td3">
                                            賬項說明
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td class="t-sv-td" style="width: 15%">
                                <table class="t-sv2">
                                    <tr>
                                        <td class="t-sv-td3">
                                            HKD
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="t-sv-td3">
                                            港幣
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="am-u-sm-12" style="height: 20px">
                </div>
                <%-- 合计区域 --%>
                <div class="am-u-sm-12 div-statementviewer-tableline">
                    <%-- 上层 --%>
                    <div class="am-u-sm-12 div-statementviewer-bottomline">
                        <div class="am-u-sm-2 div-statementviewer-rightline">
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    Previous Balance
                                </label>
                            </div>
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    上期結餘
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-2 div-statementviewer-rightline">
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    Payment
                                </label>
                            </div>
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    付款
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-2 div-statementviewer-rightline">
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    Current Due
                                </label>
                            </div>
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    本期應付
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-2 div-statementviewer-rightline">
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    Adjustment
                                </label>
                            </div>
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    調整
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-2 div-statementviewer-rightline">
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    Overdue Interest
                                </label>
                            </div>
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    逾期利息
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-2">
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    Total(HKD)
                                </label>
                            </div>
                            <div class="am-u-sm-12 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    合計
                                </label>
                            </div>
                        </div>
                    </div>
                    <%-- 下层 --%>
                    <div class="am-u-sm-12">
                        <div class="am-u-sm-2 div-statementviewer-rightline div-statementviewer-center">
                            <label id="label-previousbalance" class="label-statementviewer-font1">
                            </label>
                        </div>
                        <div class="am-u-sm-2 div-statementviewer-rightline div-statementviewer-center">
                            <label id="label-payment" class="label-statementviewer-font1">
                            </label>
                        </div>
                        <div class="am-u-sm-2 div-statementviewer-rightline div-statementviewer-center">
                            <label id="label-currentdue" class="label-statementviewer-font1">
                            </label>
                        </div>
                        <div class="am-u-sm-2 div-statementviewer-rightline div-statementviewer-center">
                            <label id="label-adjustment" class="label-statementviewer-font1">
                            </label>
                        </div>
                        <div class="am-u-sm-2 div-statementviewer-rightline div-statementviewer-center">
                            <label id="label-overdueinterest" class="label-statementviewer-font1">
                            </label>
                        </div>
                        <div class="am-u-sm-2 div-statementviewer-center">
                            <label id="label-total" class="label-statementviewer-font1">
                            </label>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12" style="height: 20px">
                </div>
                <%-- Notes: --%>
                <div class="am-u-sm-12">
                    <div class="am-u-sm-7">
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-1">
                                <label class="f-v-small">
                                    Notes:
                                </label>
                            </div>
                            <div class="am-u-sm-11">
                                <label class="f-v-small">
                                    1.See payment instructions overleaf. For more information, please contact us at
                                    2128 0066.
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-1">
                                <p>
                                </p>
                            </div>
                            <div class="am-u-sm-11">
                                <label id="label-overdueprime" class="f-v-small">
                                </label>
                            </div>
                        </div>
                    </div>
                    <div class="am-u-sm-5">
                    </div>
                </div>
                <%-- 注意 --%>
                <div class="am-u-sm-12">
                    <div class="am-u-sm-7">
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-1">
                                <label class="f-v-small">
                                    注意:
                                </label>
                            </div>
                            <div class="am-u-sm-11">
                                <label class="f-v-small">
                                    1.請參閱背面有關繳款辦法。如有垂詢，請致電2128 0066。
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-1">
                                <p>
                                </p>
                            </div>
                            <div class="am-u-sm-11">
                                <label id="label-overdueprimech" class="f-v-small">
                                </label>
                            </div>
                        </div>
                    </div>
                    <div class="am-u-sm-5">
                    </div>
                </div>
                <div class="am-u-sm-12" style="height: 50px">
                </div>
                <%-- E. & O. E --%>
                <div class="am-u-sm-12 div-statementviewer-center">
                    <label class="label-statementviewer-font1">
                        E. & O. E
                    </label>
                </div>
                <div class="am-u-sm-12" style="height: 50px">
                </div>
                <div class="am-u-sm-12">
                    <%-- E. & O. E左侧 --%>
                    <div class="am-u-sm-7">
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-4">
                                <label class="label-statementviewer-font1">
                                    Account Number 賬戶號碼
                                </label>
                            </div>
                            <div class="am-u-sm-1 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    :
                                </label>
                            </div>
                            <div class="am-u-sm-7">
                                <label id="label-eoe-accountnumber" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-4">
                                <label class="label-statementviewer-font1">
                                    Customer Name 客戶名稱
                                </label>
                            </div>
                            <div class="am-u-sm-1 div-statementviewer-center">
                                <label class="label-statementviewer-font1">
                                    :
                                </label>
                            </div>
                            <div class="am-u-sm-7">
                                <label id="label-eoe-customername" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                    </div>
                    <%-- E. & O. E右侧 --%>
                    <div class="am-u-sm-5">
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-1">
                                <p>
                                </p>
                            </div>
                            <div class="am-u-sm-6">
                                <label class="label-statementviewer-font1">
                                    Statement No. 月結單號碼
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :
                                </label>
                            </div>
                            <div class="am-u-sm-4 div-statementviewer-right">
                                <label id="label-eoe-statementnumber" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-1">
                                <p>
                                </p>
                            </div>
                            <div class="am-u-sm-6">
                                <label class="label-statementviewer-font1">
                                    Statement Date 月結單日期
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :
                                </label>
                            </div>
                            <div class="am-u-sm-4 div-statementviewer-right">
                                <label id="label-eoe-statementdate" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-1">
                                <p>
                                </p>
                            </div>
                            <div class="am-u-sm-6">
                                <label class="label-statementviewer-font1">
                                    Due Date 到期日
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :
                                </label>
                            </div>
                            <div class="am-u-sm-4 div-statementviewer-right">
                                <label id="label-eoe-duedate" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                        <div class="am-u-sm-12">
                            <div class="am-u-sm-1">
                                <p>
                                </p>
                            </div>
                            <div class="am-u-sm-6">
                                <label class="label-statementviewer-font1">
                                    Lease Number. 租約號碼
                                </label>
                            </div>
                            <div class="am-u-sm-1">
                                <label class="label-statementviewer-font1">
                                    :
                                </label>
                            </div>
                            <div class="am-u-sm-4 div-statementviewer-right">
                                <label id="label-eoe-leasenumber" class="label-statementviewer-font1">
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12" style="height: 5px">
                </div>
                <%-- 應繳款項總額 --%>
                <div class="am-u-sm-12">
                    <div class="am-u-sm-5">
                        <p>
                        </p>
                    </div>
                    <div class="am-u-sm-7">
                        <div class="am-u-sm-6" style="height: 40px; line-height: 40px">
                            <label class="label-statementviewer-font1">
                                Total Amount Due 應繳款項總額：
                            </label>
                        </div>
                        <div class="am-u-sm-6" style="background-color: #CCCCCC; padding-left: 10px; padding-right: 10px;
                            height: 40px;">
                            <div class="am-u-sm-12">
                                <div class="am-u-sm-3" style="height: 40px; line-height: 40px; text-align: left;">
                                    <label class="f-v-bold">
                                        HKD
                                    </label>
                                </div>
                                <div class="am-u-sm-9" style="height: 40px; line-height: 40px; text-align: right;">
                                    <label id="label-eoe-total" class="f-v-bold">
                                    </label>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="am-u-sm-12" style="height: 5px">
                </div>
                <%-- 页底文字en --%>
                <div class="am-u-sm-12">
                    <label class="f-v-small">
                        For cheque payment, please return this payment slip with cheque. Cheque should be
                        crossed and made payable to "HUTCHISON ESTATE AGENTS LIMITED".
                    </label>
                </div>
                <%-- 页底文字cn --%>
                <div class="am-u-sm-12">
                    <label class="f-v-small">
                        若以支票付款，請連同付款回條一併交回。劃綫支票抬頭請寫“和記地產代理有限公司”。
                    </label>
                </div>
                <div class="am-u-sm-12" style="height: 20px">
                </div>
            </div>
        </div>
        <div style="height: 30px;">
        </div>
        <div class="div-statementviewer-main">
            <table style="width: 100%;">
                <tr>
                    <td style="width: 200px;">
                    </td>
                    <td>
                    </td>
                </tr>
                <tr>
                    <td colspan="2" class="sv-td-title">
                        繳款辦法
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td class="sv-td-text-left">
                        1. 郵寄
                    </td>
                    <td class="sv-td-text-right">
                        請將劃線支票連同付款回條寄回香港九龍紅磡德豐街十八號海濱廣場一座三樓會計部收，請於支票背面寫上貴戶之賬戶號碼，請勿郵寄現金。
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td class="sv-td-text-left">
                        2.「繳費靈」
                    </td>
                    <td class="sv-td-text-right">
                        請致電18033或瀏覽http://www.ppshk.com網頁，透過繳費靈服務從閣下指定的銀行戶口轉賬付款，以繳費靈付款需時兩個工作天辦理，查詢請致電繳費靈熱線90
                        000 222 328，本公司的商戶編號已列印在月結單正面。
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td class="sv-td-text-left">
                        3. 直接轉賬
                    </td>
                    <td class="sv-td-text-right">
                        按閣下所指示從指定之銀行戶口扣除款額，直接付款授權書可向本公司索取。
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td class="sv-td-text-left">
                        注意事項：
                    </td>
                    <td class="sv-td-text-right">
                        1. 如支票或直接轉賬，需待收妥始作實。
                        <br />
                        2. 截數日之後所收之賬項，未有計算在內。
                        <br />
                        3. 除特別安排外，本公司恕不負責任何以現金形式繳付之賬項。
                        <br />
                        4. 逾期付款將收取利息。
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td colspan="2" class="sv-td-text-left">
                        如有任何查詢，請致電2128 0066。
                    </td>
                </tr>
                <tr style="height: 50px;">
                </tr>
                <tr>
                    <td colspan="2" class="sv-td-title">
                        Payment Instructions
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td class="sv-td-text-left">
                        1. By mail
                    </td>
                    <td class="sv-td-text-right">
                        Send a cheque together with the payment slip, to 3/F, One Harbourfront, 18 Tak Fung
                        Street, Hunghom, Kowloon, Hong Kong and attention to Accounts Department. Please
                        write your account number on the back of the cheque. Do not send cash by post.
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td class="sv-td-text-left">
                        2. PPS
                    </td>
                    <td class="sv-td-text-right">
                        Make payment from any designated bank account by calling 18033 with any tone-dial
                        phone or visiting http://www.ppshk.com. Please allow 2 working days to process your
                        phone payment. For details, please call PPS hotline on 90 000 222 329. Please refer
                        to monthly statement (front page) for PPS merchant code.
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td class="sv-td-text-left">
                        3. Direct Debit
                    </td>
                    <td class="sv-td-text-right">
                        Direct debited from your designated bank account. Direct Debit Authorization Forms
                        are obtainable from our company.
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td class="sv-td-text-left">
                        Notes:
                    </td>
                    <td class="sv-td-text-right">
                        1. Payment by cheque and direct debit will be subject to collection and clearance.
                        <br />
                        2. Payments received after the cut-off date have not been taken into account.
                        <br />
                        3. The company will not take any responsibility on any cash payment except arranged
                        with us in advance.
                        <br />
                        4. Interest will be charged on overdue account.
                    </td>
                </tr>
                <tr class="sv-td-blank">
                </tr>
                <tr>
                    <td colspan="2" class="sv-td-text-left">
                        If you have any queries, please feel free to contact us at 2128 0066.
                    </td>
                </tr>
            </table>
        </div>
        <div class="div-statementviewer-main" style="padding: 0px 0px 0px 0px;">
            <div style="border-width: 1px; border-style: dashed;">
            </div>
        </div>
        <div class="div-statementviewer-main">
            <table style="width: 100%;">
                <tr style="height: 30px;">
                    <td style="width: 250px;">
                    </td>
                    <td style="width: 50px;">
                    </td>
                    <td style="width: 350px;">
                    </td>
                    <td style="width: 150px;">
                    </td>
                    <td style="width: 50px;">
                    </td>
                    <td>
                    </td>
                </tr>
                <tr>
                    <td class="sv-td-title" colspan="6">
                        CHANGE OF CORRESPONDENCE ADDRESS/TELEPHONE NUMBER 更改通訊地址/電話號碼
                    </td>
                </tr>
                <tr style="height: 20px;">
                </tr>
                <tr>
                    <td>
                        Customer Name<br />
                        客戶姓名
                    </td>
                    <td class="sv-td-mao">
                        :
                    </td>
                    <td class="sv-td-xiahua">
                        <div class="sv-td-line">
                        </div>
                    </td>
                    <td style="padding-left: 10px;">
                        Account Number<br />
                        賬戶號碼
                    </td>
                    <td class="sv-td-mao">
                        :
                    </td>
                    <td class="sv-td-xiahua">
                        <div class="sv-td-line">
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        New Correspondence Address<br />
                        新通訊地址
                    </td>
                    <td class="sv-td-mao">
                        :
                    </td>
                    <td class="sv-td-xiahua" colspan="4">
                        <div class="sv-td-line">
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <br />
                        <br />
                    </td>
                    <td>
                    </td>
                    <td class="sv-td-xiahua" colspan="4">
                        <div class="sv-td-line">
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        New Contact Telephone No<br />
                        新聯絡電話
                    </td>
                    <td class="sv-td-mao">
                        :
                    </td>
                    <td class="sv-td-xiahua">
                        <div class="sv-td-line">
                        </div>
                    </td>
                    <td style="padding-left: 10px;">
                        Effective Date<br />
                        生效日期
                    </td>
                    <td class="sv-td-mao">
                        :
                    </td>
                    <td class="sv-td-xiahua">
                        <div class="sv-td-line">
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <div class="div-statementviewer-main">
            <table style="width: 500px;">
                <tr>
                    <td>
                        <div class="sv-td-line">
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        Authorised Signature & Company Chop<br />
                        簽署及蓋章
                    </td>
                </tr>
                <tr style="height: 30px;">
                </tr>
                <tr>
                    <td>
                        <%-- 下载PDF --%>
                        <a class="am-btn am-btn-secondary am-round" href="JavaScript:void(0);" onclick="downloadpdf()">
                            <%= Resources.Lang.Res_StatementViewer_Download%></a>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <%-- Loading窗口 --%>
    <div class="am-modal am-modal-loading am-modal-no-btn" tabindex="-1" id="my-modal-loading">
        <div class="am-modal-dialog">
            <div class="am-modal-hd">
                Loading...</div>
            <div class="am-modal-bd">
                <span class="am-icon-spinner am-icon-spin"></span>
            </div>
        </div>
    </div>
    <%--警告模态窗口--%>
    <div class="am-modal am-modal-alert" tabindex="-1" id="doc-modal-alert">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                <label id="label-alert-title">
                </label>
            </div>
            <div class="am-modal-bd">
                <div class="am-modal-bd-my">
                    <label id="label-alert-content">
                    </label>
                </div>
            </div>
            <div class="am-modal-footer">
                <span class="am-modal-btn">OK</span>
            </div>
        </div>
    </div>
</asp:Content>
