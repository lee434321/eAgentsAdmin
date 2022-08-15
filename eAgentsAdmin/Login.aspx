﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="PropertyOneAppWeb.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <style type="text/css">
        .disclaimer-td-title
        {
            padding: 20px 0px 20px 0px;
            font-weight: bold;
        }
        
        .disclaimer-td-content
        {
            padding: 10px 0px 0px 0px;
        }
        
        .disclaimer-td-sub
        {
            padding: 0px 0px 0px 0px;
        }
        
        .disclaimer-td-sub2
        {
            padding: 0px 0px 0px 40px;
        }
    </style>
    <meta http-equiv="X-UA-Compatible" content="IE=EDGE" />
    <link rel="stylesheet" href="/Css/Site.css" />
    <link rel="stylesheet" href="/js/AmazeUI/assets/css/amazeui.css" />
    <!--[if (gte IE 9)|!(IE)]><!-->
    <script type="text/javascript" src="/js/jQuery/jquery-3.2.1.min.js"></script>
    <!--<![endif]-->
    <!--[if lte IE 8]>
    <script src="/js/jQuery/jquery-1.11.1.min.js"></script>
    <script src="http://cdn.staticfile.org/modernizr/2.8.3/modernizr.js"></script>
    <script src="/js/AmazeUI/assets/js/amazeui.ie8polyfill.min.js"></script>
    <![endif]-->
    <script type="text/javascript" src="/js/AmazeUI/assets/js/amazeui.min.js"></script>
    <script type="text/javascript" src="/js/SiteJScript.js"></script>
    <title>PropertyOne Onlinepay</title>
    <script type="text/javascript">
        var logintype = '<%=System.Configuration.ConfigurationManager.AppSettings["logintype"].ToString()%>';
        $(document).ready(function () {
            SettingDefaultLang();
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
                    throw '<%= Resources.Error.ERR_INPUT_LEASE %>';
                }

                if (checknotnull(psw) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_PSW %>';
                }

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Login.aspx",
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
                            /* 登录成功，读取用户信息 */
                            loadUserInfo();
                        }
                        else if (data == "first") {
                            /* 如果是第一次登录, 需要修改初始密码 */
                            showchangepswmodal();
                        }
                        else {
                            showAlert("Notice", '<%= Resources.Error.ERR_LOGIN_FAILED %>');
                        }
                    },
                    error: function (data) {
                        $("#button-login").prop("disabled", false);
                        showAlert("Notice", '<%= Resources.Error.ERR_NETWORK_ERR %>');
                    }
                });
            }
            catch (ex) {
                $("#button-login").prop("disabled", false);
                showAlert("Notice", ex);
            }
        };

        /* 加载Lease用户信息 */
        function loadUserInfo() {
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Login.aspx",
                data: {
                    "action": "loadUserInfo"
                },
                success: function (data) {
                    if (data == "ok") {
                        window.location.href = "/Web/Homepage.aspx";
                    }
                    else {
                        showAlert("Notice", '<%= Resources.Error.ERR_LOGIN_FAILED %>');
                    }
                },
                error: function (data) {
                    showAlert("Notice", '<%= Resources.Error.ERR_NETWORK_ERR %>');
                }
            });
        };

        /* 切换语言 */
        function ChangeLang() {
            var lang = $("#SelectCulture").val();
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Login.aspx",
                data: {
                    "action": "changelang",
                    "lang": lang
                },
                success: function (data) {
                    if (data == "ok") {
                        window.location.href = "/Login.aspx";
                    }
                    else {
                        showAlert("Notice", '<%= Resources.Error.ERR_LANG_CHANGE %>');
                    }
                },
                error: function (data) {
                    showAlert("Notice", '<%= Resources.Error.ERR_NETWORK_ERR %>');
                }
            });
        }

        /* 设定默认语言 */
        function SettingDefaultLang() {
            var lang = '<%=Session["preferredculture"] %>';
            var disclaimerUrl = "/Public/disclaimer.aspx";
            var langIndex = 0;

            if (lang == "en-US") {
                langIndex = 0;
                disclaimerUrl = "/Public/disclaimer.aspx";
            }
            else if (lang == "zh-CHT") {
                langIndex = 1;
                disclaimerUrl = "/Public/disclaimer-cht.aspx";
            }
            else if (lang == "zh-CHS") {
                langIndex = 2;
                disclaimerUrl = "/Public/disclaimer-cht.aspx";
            }
            $("#a-term").prop("href", disclaimerUrl);

            $('#SelectCulture').find('option').eq(langIndex).attr('selected', true);

            /* 不支持 MutationObserver 的浏览器使用 JS 操作 select 以后需要手动触发 `changed.selected.amui` 事件 */
            $('#SelectCulture').trigger('changed.selected.amui');
        };

        /* 显示更改密码窗口 */
        function showchangepswmodal() {
            showModal("doc-modal-changepsw", 600, false);
        };

        /* 提交修改密码 */
        function submitchangepsw() {
            try {
                $("#button-change-psw").prop("disabled", true);
                var psw1 = $("#input-change-newpsw").val();
                var psw2 = $("#input-change-conpsw").val();

                if (checknotnull(psw1) == false || checknotnull(psw2) == false) {
                    throw '<%= Resources.Error.ERR_INPUT_PSW %>';
                }

                if (isPasswd(psw1) == false || isPasswd(psw2) == false) {
                    throw '<%= Resources.Error.ERR_PSW_LENGTH %>';
                }

                if (psw1 != psw2) {
                    throw '<%= Resources.Error.ERR_PSW_SAME %>';
                }

                /* 开始更改密码 */
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Login.aspx",
                    data: {
                        "action": "changepsw",
                        "newpsw": psw1
                    },
                    success: function (data) {
                        if (data == "ok") {
                            loadUserInfo();
                        }
                        else {
                            $("#button-change-psw").prop("disabled", false);
                            alert('<%= Resources.Error.ERR_PSW_CHANGE %>');
                        }
                    },
                    error: function (data) {
                        $("#button-change-psw").prop("disabled", false);
                        alert('<%= Resources.Error.ERR_NETWORK_ERR %>');
                    }
                });
            }
            catch (ex) {
                $("#button-change-psw").prop("disabled", false);
                alert(ex);
            }
        };

        /* 更改密码窗口取消 */
        function cancelchangepsw() {
            closeModal("doc-modal-changepsw");
            $("#input-change-newpsw").val("");
            $("#input-change-conpsw").val("");
        };

        /* 开始注册 */
        function Register() {
            var lang = $("#SelectCulture").val();
            if (lang == "en-US") {
                showModal("modal-disclaimer-en", 800, false);
            }
            else if (lang == "zh-CHT") {
                showModal("modal-disclaimer-cht", 800, false);
            }
            else if (lang == "zh-CHS") {
                showModal("modal-disclaimer-cht", 800, false);
            }
        }

        /* 同意 */
        function AgreeDisclaimer() {
            window.location.href = "/Public/register.aspx";
        }

        /* 不同意 */
        function DisAgreeDisclaimer() {
            window.location.href = "/Login.aspx";
        }
    </script>
</head>
<body class="login-body-all-browser" style="overflow: hidden;">
    <div class="div-login-main-logo">
        <img id="img-logo" alt="" src="/Image/HEAL-master2017.png" width="209px" height="60px" />
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
                                                        <img alt="" src="/Image/login_icon01.png" width="25px" height="25px" />
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
                                    <%-- UserName --%>
                                    <tr id="tr-select-username">
                                        <td colspan="2">
                                            <table style="width: 100%;">
                                                <tr>
                                                    <td style="width: 150px; padding-left: 20px;">
                                                        <label id="label-loginid" class="label-login-input">
                                                            <%= Resources.Lang.Res_Login_Label_UserName %>
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
                                                <%= Resources.Lang.Res_Login_Label_Password %>
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
                                            <button id="button-register" type="button" class="am-btn am-btn-block button-login"
                                                onclick="Register()">
                                                <%= Resources.Lang.Res_Login_Register%></button>
                                            <%--<a class="am-btn am-btn-block button-login" href="/Public/register.aspx">
                                                <%= Resources.Lang.Res_Login_Register%></a>--%>
                                        </td>
                                    </tr>
                                    <tr style="height: 20px;">
                                    </tr>
                                    <tr>
                                        <td style="text-align: center;">
                                            <a class="am-btn am-btn-block button-login" href="/Public/ForgetPassword.aspx">
                                                <%= Resources.Lang.Res_Login_ForgetPassword%></a>
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                            <select id="SelectCulture" runat="server" data-am-selected="{btnWidth: '100%', btnSize: 'xl', btnStyle: 'danger'}"
                                                onchange="ChangeLang()">
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
    <%--第一次登录修改密码模态窗口--%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="doc-modal-changepsw">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                <%= Resources.Lang.Res_Login_FirstTimeLogin%>
            </div>
            <div class="am-modal-bd">
                <div style="padding: 50px 50px 20px 50px;">
                    <table style="width: 100%;">
                        <tr>
                            <td style="text-align: left;">
                                <label>
                                    <%= Resources.Lang.Res_Login_FirstTimeNewPsw%>
                                </label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <input id="input-change-newpsw" type="password" class="am-form-field" maxlength="20" />
                            </td>
                        </tr>
                        <tr style="height: 20px;">
                        </tr>
                        <tr>
                            <td style="text-align: left;">
                                <label>
                                    <%= Resources.Lang.Res_Login_FirstTimeConfirmPsw%>
                                </label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <input id="input-change-conpsw" type="password" class="am-form-field" maxlength="20" />
                            </td>
                        </tr>
                        <tr style="height: 50px;">
                        </tr>
                        <tr>
                            <td>
                                <table style="width: 100%;">
                                    <tr>
                                        <td>
                                            <button id="button-change-psw" type="button" class="am-btn am-btn-secondary am-round button-all"
                                                style="float: right;" onclick="submitchangepsw()">
                                                <%= Resources.Lang.Res_Global_OK%></button>
                                        </td>
                                        <td style="width: 130px;">
                                            <button type="button" class="am-btn am-btn-default am-round button-all" style="float: right;"
                                                onclick="cancelchangepsw()">
                                                <%= Resources.Lang.Res_Global_Cancel%></button>
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
    <%--免责申明(EN)--%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="modal-disclaimer-en">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                <%= Resources.Lang.Res_Login_Button_Term%>
            </div>
            <div class="am-modal-bd">
                <div style="padding: 20px 50px 20px 50px; height: 700px; overflow-y: scroll">
                    <table style="width: 100%; text-align: left;">
                        <tr>
                            <td class="disclaimer-td-title">
                                1. ACCEPTANCE OF TERMS AND CONDITIONS
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                Hutchison Estate Agents Limited (“HEAL”) operates and administers a website at www.heal.com.hk
                                (“Site”) which provides users of the Site with access to e-statement services ("Services")
                                relating to their tenancies of the premises owned by landlord companies within the
                                group of HWPL Hong Kong Holdings Limited (“Landlords”) for whom HEAL is acting as
                                agents. The terms "HEAL" and "we" as used herein refer to HEAL and the Landlords
                                and unless the context otherwise requires such expression shall also include any
                                of its directors, employees, officers and agents (and the terms “us” and “our” shall
                                be so construed accordingly).
                                <br />
                                <br />
                                The terms "you", "User" and “Users” as used herein refer to all individuals and
                                / or entities accessing the Services and/or the Site. The term "Contents" is used
                                herein to refer to all or any of, as the case may be, the data, text, button icons,
                                links, HTML codes, trademark, software, music, sound, photographs, graphics, still
                                pictures, series of moving pictures (whether animated or not), videos, merchandise,
                                products, advertisements, services or any compilation or combination of them and
                                any other contents or materials that may be found on the Site.
                                <br />
                                <br />
                                HEAL provides the Services to you, subject to the following Terms and Conditions
                                of Use ("T&C"), which may be updated and revised by us from time to time by posting
                                the revised version on the Site. Unless otherwise stated by us, any changes we make
                                will be effective immediately upon posting. Your use of the Site after such posting
                                amounts to your conclusive acceptance of such change. Be sure to review the T&C
                                regularly to ensure familiarity with the most current version. You undertake that
                                you will not assert your lack of knowledge or our lack of notification of any changes
                                to the relevant terms and conditions as a defence in the event of a dispute. Some
                                of the Services may be subject to additional terms and conditions governing their
                                provision which additional terms will be made known to you upon you expressing your
                                intent to use those Services. Those additional terms and conditions together with
                                the Privacy Policy (as referred to in paragraph 4 below) are hereby incorporated
                                by reference into the T&C.
                                <br />
                                <br />
                                You accept and agree to be bound by the T&C upon your registration for/using the
                                Services or otherwise accessing the Site or using any information found therein.
                                If you do not accept the T&C, you may not and should not access the Site or use
                                the Services or information provided thereunder. Any other terms and conditions
                                proposed by you which are in addition to or which conflict with the T&C are expressly
                                rejected by HEAL and shall be of no force and effect. If you have any questions
                                about the T&C, or about accessing and using the Site, please contact HEAL. We reserve
                                the right to interpret the T&C and decide on any questions or disputes arising under
                                the T&C. You agree that all such interpretations and decisions shall be final and
                                conclusive, and binding on you as a user of the Site and/or the Services.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                2. REGISTRATION, USER ACCOUNT AND PASSWORD
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                You will be asked to complete registration on the Site before the Services become
                                available to you. In completing the registration, you undertake to (a) provide true,
                                accurate, current and complete information as required and (b) maintain and promptly
                                update such information to keep it true, accurate, current and complete. If you
                                provide any information that is untrue, inaccurate, not current or incomplete, or
                                HEAL has reasonable grounds to suspect that such is untrue, inaccurate, not current
                                or incomplete, HEAL has the right to suspend or terminate your account and refuse
                                any and all current or future use of the Services (or any portion thereof).
                                <br />
                                <br />
                                Upon successful registration by a User, an account ("User Account") will be set
                                up for the User, who will be provided with a password ("Password") to enable the
                                User to access and use of the Site and/or the Service. You are responsible for maintaining
                                the confidentiality of your User Account and/or Password (as may be altered from
                                time to time). You are fully responsible (and shall be liable to HEAL) for all activities
                                that occur under the User Account and/or Password.
                                <br />
                                <br />
                                Users shall give a prior notification, according to the specific requirements given
                                by HEAL from time to time, informing HEAL to terminate or update any changes on
                                the Services for the Users’ respective User Account(s).
                                <br />
                                <br />
                                You are strictly prohibited from assigning, transferring, licensing or sub-licensing
                                to any other person their right to access or use the Site and/or the Service or
                                any part thereof. You shall not use, or allow anyone to use, any means to circumvent
                                log in password and other protections which HEAL may put in place to restrict access
                                to certain areas of the Site and/or the Service.
                                <br />
                                <br />
                                You shall immediately notify HEAL of any unauthorised use of any User Account or
                                Password or any other breach of security through any User Account.
                                <br />
                                <br />
                                You acknowledge and agree that the only duty of HEAL is to verify Passwords inputted
                                by the Users and HEAL shall not be liable in respect of:
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                (a) any loss or damage suffered by you or any other person as a result of any failure
                                to effect or execute instructions through various electronic delivery channels or
                                perform any obligation thereunder where such failure is attributable either directly
                                or indirectly to any circumstances or events outside our control; or
                                <br />
                                (b) any other loss or damage whatsoever suffered by you or by any other person as
                                a result of any instructions through various electronic delivery channels given
                                with the correct Password and/or other information.
                                <br />
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                You note that only Secure Sockets Layer (SSL) Software, which encrypts information
                                you input, is used to protect the security of your personally identifiable information
                                during transmission. By using or accessing the Site and/or the Services and in consideration
                                of such access and use, you acknowledge that you are satisfied that the security
                                features that we have adopted are adequate for all your use of the Site and/or the
                                Services in accordance with the T&C.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                3. SERVICES
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                All Services together with any new features that augment or enhance any such services
                                currently offered shall, unless explicitly stated otherwise, be subject to the T&C.
                                <br />
                                <br />
                                You agree, understand and acknowledge that:
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                (a) HEAL will send notifications by e-mail to the Users’ designated e-mail addresses
                                informing the Users that the e-statements of their respective registered User Account
                                in electronic form is available for viewing online and the Users shall check their
                                designated e-mail addresses regularly for such notifications;
                                <br />
                                (b) HEAL will retain the e-statements of the registered User Account(s) at the Site
                                for a period of 6 months (or such other period as prescribed by HEAL from time to
                                time) and the Users shall examine each e-statement upon receiving the e-mail notice
                                from HEAL and if necessary, print and/or download the e-statement for future reference;
                                <br />
                                (c) HEAL is entitled to levy fee and charges against the Users to cover the cost
                                and expenses for the Users requisition of obtaining a hard copy of e-statement that
                                is no longer available for access and downloading through the Site and in any event,
                                HEAL is not under any obligation to comply with any such request; and
                                <br />
                                (d) User Accounts (and access to the Services via the same) will be disabled/ terminated
                                upon the expiry or earlier termination of the Users’ respective tenancies with the
                                Landlords.
                                <br />
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                You acknowledge that there are inherent hazards in communications over the Internet
                                and telecommunication networks and as such there may be delay, omission or inaccuracies
                                in the information so provided. You also understand and agree that the Services
                                are provided on an "AS IS" and "AS AVAILABLE" basis and that HEAL assumes no responsibility
                                for the timeliness, deletion, mis-delivery or failure of the provision of any of
                                the Services.
                                <br />
                                <br />
                                You understand that the technical processing and transmission of the Services may
                                involve (a) transmissions over various networks; and (b) changes to conform and
                                adapt to technical requirements of connecting networks, devices or media. HEAL shall,
                                accordingly, in no circumstances, be liable for any failure of any Services in whole
                                or in part or for your inability to gain access in whole or in part to the Site
                                and/or the Services due to the delay or failure of any communication networks, devices
                                or media or any party providing such access or necessary support including power
                                supply. We do not guarantee uninterrupted, continuous or secure access to the Site.
                                Part or the entire Site and/or the Services may be unexpectedly unavailable for
                                whatever duration and for various reasons that may include system malfunctions and
                                disruptions, Internet and/or telecommunication network access downtime and other
                                technical problems beyond our control for which we cannot and shall not be held
                                responsible. You agree that you will not hold us responsible for any damage or loss
                                caused by your inability to use the Site and/or the Services for any reason whatsoever.
                                We reserve the right to take any part or all of the Site and/or the Services offline
                                for various reasons including urgent system maintenance or upgrading, in which case
                                we will try to give you notice in advance as far as practically possible.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                4. PRIVACY POLICY
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                Personally identifiable information is subject to our Privacy Policy. Please see
                                our Privacy Policy <a href="/Public/privacy-policy.aspx" target="_blank">here</a>.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                5. USER CONDUCT
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                You undertake not to:
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                (a) interfere with, impair or disrupt the provision or operation of the Services
                                or servers or networks or telecommunication service through which the Services are
                                provided, or disobey any requirements, procedures, policies or regulations of such
                                networks or telecommunication service;
                                <br />
                                (b) attempt to gain unauthorised access to the Site, other User's accounts or passwords,
                                computer systems or networks connected to the mobile app, through password mining
                                or any other means;
                                <br />
                                (c) modify, adapt, sub-license, translate, sell, reverse engineer, decompile or
                                disassemble any portion of the Site or software contained in the Siteor used in
                                connection with the Services (including any files, images incorporated in or generated
                                by the software, and data accompanying the software);
                                <br />
                                (d) remove any copyright, trademark, or other proprietary rights notices contained
                                in the Site;
                                <br />
                                (e) "frame" or "mirror" any part of the Site without our prior written authorisation;
                                <br />
                                (f) use any robot, "spider", site search/retrieval application, or other manual
                                or automatic device or process to retrieve, index, "data mine", or in any way reproduce
                                or circumvent the navigational structure or presentation of the Siteor its contents;
                                or
                                <br />
                                (g) collect or store personally identifiable information about other Users.
                                <br />
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                Without limiting the generality of the foregoing, you further agree not to trespass,
                                break into, access, use or attempt to trespass, break into, access or use any other
                                parts of our servers and/or any data areas for which you have not been authorised
                                by us.
                                <br />
                                <br />
                                Any unauthorised alteration or addition to the Site and any information, data and
                                material contained therein is strictly prohibited. All rights not expressly granted
                                herein are reserved. These T&C supersede any prior or contemporaneous communications
                                between us (including any of our employees, officers, directors and agents) and
                                you in respect of the Site. We reserve the rights from time to time, without notice,
                                to access your User Account or to observe and record your access to and use of the
                                Site to determine if you are complying with the T&C.
                                <br />
                                <br />
                                You shall comply with all applicable laws, statutes, ordinances and regulations
                                (whether or not having the force of law) (the "Applicable Laws ") regarding your
                                use of the Site. You recognise the global nature of the Internet and you understand
                                that the Applicable Laws may be of a jurisdiction other than your own and you agree
                                that compliance with the Applicable Laws is your sole responsibility. We recommend
                                that you seek legal advice on your own account if you are not sure what Applicable
                                Laws comprise.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                6. INDEMNITY
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                You agree to indemnify, defend and hold harmless HEAL, and its parent/holding companies,
                                subsidiaries, affiliates, officers, agents, co-branders or other partners, and employees
                                against any and all claims, proceedings, damages, liabilities, cost and expenses
                                (including all legal costs on full indemnity basis) arising from:
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                (a) your acts or inaction in breach of these T&C;
                                <br />
                                (b) your use of the Services or any use of the Site through your User Account;
                                <br />
                                (c) the Contents or personally identifiable information submitted, posted to or
                                transmitted through the Services/ the Site by you or using your User Account; and/or
                                <br />
                                (d) the provision of the Services by HEAL, whether or not arising from or in connection
                                with:
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub2">
                                (i) improper use of the Site, the Service, the e-statements by you or any other
                                person using your User Account (whether authorized by you or not);
                                <br />
                                (ii) inaccurate information provided by you in relation to the User Account, e-mail
                                address(es), contact person(s), contact number(s) or any other information; or
                                <br />
                                (iii) any damage to the computer hardware, devices, facilities or software as a
                                result of your accessing and/or using the Site and/or Service.
                                <br />
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                Termination of your right to use the Services or the Site by either HEAL or you
                                shall in no way removes or otherwise affects your obligation to indemnify HEAL hereunder.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                7. MODIFICATIONS TO SERVICES
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                HEAL reserves the right at any time and from time to time to modify or discontinue,
                                temporarily or permanently, the Site and/or the Services (or any part thereof) that
                                may be available to you without prior notice.
                                <br />
                                <br />
                                You agree that HEAL shall not be liable to you or to any third party for any modification,
                                suspension or discontinuance of the Services and/or the Site.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                8. DISCLAIMER
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                YOU EXPRESSLY UNDERSTAND AND AGREE THAT:
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                A. YOUR USE OF THE SERVICES AND/OR THE SITE IS AT YOUR OWN RISK. TO THE FULLEST
                                EXTENT PERMITTED BY LAW, HEAL EXPRESSLY DISCLAIMS ALL WARRANTIES OF ANY KIND, WHETHER
                                EXPRESS OR IMPLIED.
                                <br />
                                B. HEAL MAKES NO WARRANTY THAT: (I) THE SERVICES WILL MEET YOUR REQUIREMENTS; (II)
                                THE SERVICES WILL BE UNINTERRUPTED, TIMELY, SECURE, OR ERROR-FREE; (III) THE RESULTS
                                THAT MAY BE OBTAINED FROM THE USE OF THE SERVICES WILL BE ACCURATE OR RELIABLE;
                                (IV) THE QUALITY OF ANY CONTENTS OR OBTAINED BY YOU THROUGH THE SERVICES WILL MEET
                                YOUR EXPECTATIONS; OR (V) ANY ERRORS IN ANY SOFTWARE WILL BE CORRECTED.
                                <br />
                                C. ANY CONTENTS DOWNLOADED OR OTHERWISE OBTAINED THROUGH THE USE OF THE SERVICES
                                AND/OR THE SITE ARE OBTAINED AT YOUR OWN DISCRETION AND RISK AND THAT YOU WILL BE
                                SOLELY RESPONSIBLE FOR ANY DAMAGE TO YOUR COMPUTER SYSTEM OR LOSS OF DATA THAT RESULTS
                                FROM THE DOWNLOAD OF ANY SUCH CONTENTS.
                                <br />
                                D. NO ADVICE OR INFORMATION, WHETHER ORAL OR WRITTEN, OBTAINED BY YOU FROM HEAL
                                OR THROUGH OR FROM THE SERVICES SHALL CONSTITUTE ANY WARRANTY.
                                <br />
                                E. HEAL DOES NOT GUARANTEE TIMELINESS OF THE SERVICES AND YOU ALSO AWARE THAT CERTAIN
                                SERVICES ARE PROVIDED ON A DELAYED BASIS.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                9. LIMITATION OF LIABILITY
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                YOU UNDERSTAND AND EXPRESSLY AGREE THAT, TO THE FULLEST EXTENT PERMITTED BY LAW,
                                HEAL SHALL NOT BE LIABLE WHETHER IN TORT OR CONTRACT OR OTHERWISE FOR ANY DIRECT,
                                INDIRECT, INCIDENTAL, SPECIAL, CONSEQUENTIAL OR EXEMPLARY DAMAGES, INCLUDING BUT
                                NOT LIMITED TO, DAMAGES FOR LOSS OF PROFITS, GOODWILL, REVENUE, USE, DATA OR OTHER
                                INTANGIBLE LOSSES, RESULTING FROM: (I) THE USE, THE INABILITY TO USE OR THE UNAVAILABILITY
                                OF THE SERVICES AND/OR THE SITE; (II) UNAUTHORIZED ACCESS TO OR ALTERATION OF YOUR
                                USER ACCOUNTS, PASSWORDS, TRANSMISSIONS OR PERSONALLY IDENTIFIABLE INFORMATION;
                                OR (III) ANY OTHER MATTER RELATING TO THE SERVICES AND/OR THE SITE.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                10. INTELLECTUAL PROPERTY RIGHTS
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                The Site together with all Contents made available as part of theSite are our property
                                or are licensed to us and are protected by copyright, trademarks, service marks,
                                patents or other proprietary rights and laws with all rights reserved. We and/or
                                our licensors own copyright in the selection, co-ordination, arrangement and enhancement
                                of such Contents, as well as in the content original to it. Unauthorized copying,
                                distribution, modification, exploitation and public display of or dealings with
                                copyrighted works is an infringement of the copyright holders' rights. HEAL and
                                other parties own the trademarks, logos and service marks displayed on the Site
                                and any necessary software used in connection with the Services and Users are prohibited
                                from using, modifying, lending, selling, renting, leasing, distributing or creating
                                derivative works based on or in any way tempering with the same, in whole or in
                                part, without the written permission of HEAL or such other parties (as the case
                                may be). HEAL reserves the right to terminate the registration of any User upon
                                notice of any infringement of the copyrights or other intellectual property rights
                                of others in conjunction with use of the Site and/or the Servcies.
                                <br />
                                <br />
                                The availability of any information, data and materials contained in the Site in
                                all circumstances should not be taken to be or constitute a transfer of copyrights,
                                trademarks or other intellectual property rights of HEAL and/or any licensor in
                                the Site and the information, data and materials contained therein to any users
                                or visitors of the Site or any other third parties.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                11. NO AGENCY
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                You and HEAL are independent and no agency, partnership, joint venture, trustee,
                                beneficiary, employee-employer or franchiser-franchisee relationship is intended
                                or created by your use of the Site or the Services.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                12. WAIVER
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                No right under the T&C shall be deemed to be waived except by notice in writing
                                signed by HEAL and you. A waiver by HEAL will not prejudice its rights in respect
                                of any subsequent breach of the T&C by you.
                                <br />
                                <br />
                                Subject to the foregoing provisions of this Clause 12, any failure by HEAL to enforce
                                any provisions of the T&C, or any forbearance, delay or indulgence granted by HEAL
                                to you, will not be construed as a waiver by HEAL of its rights or remedies under
                                the T&C.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                13. NOTICES
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                Notices to HEAL under the T&C may be delivered by e-mail to the addresses shown
                                at “Contact Us” in the Site.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                14. GOVERNING LAW
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                The T&C shall be construed in accordance with the laws of the Hong Kong Special
                                Administrative Region of the People's Republic of China ("Hong Kong ").
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                15. SEVERABILITY
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                If any one or more of these T&C, or their application in any circumstance, is held
                                invalid, illegal or unenforceable in any respect for any reason, the validity, legality
                                and enforceability of that term or condition in any other respect and the remaining
                                T&C shall not in any way be impaired.
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                16. GENERAL PROVISIONS
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                Unless the context otherwise requires, the T&C should be interpreted using the following
                                rules:
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                (a) words importing one gender include the other genders;
                                <br />
                                (b) words importing the singular shall include the plural and vice versa;
                                <br />
                                (c) references to Clauses and references to clauses of the T&C;
                                <br />
                                (d) expressions defined in the main body of the T&C bear the defined meanings in
                                the whole of the T&C;
                                <br />
                                (e) headings are for ease of reference only and shall not affect the interpretation
                                of the T&C;
                                <br />
                                (f) any reference to a person shall include that person's successors, representatives
                                and permitted assigns; and
                                <br />
                                (g) in the event that there is any inconsistency between the English and Chinese
                                versions of the T&C, the English version shall prevail.
                            </td>
                        </tr>
                        <tr>
                            <td style="padding: 30px 0px 10px 0px">
                                <table style="width: 100%;">
                                    <tr>
                                        <td>
                                            <button type="button" class="am-btn am-btn-secondary am-round button-all" style="float: right;"
                                                onclick="AgreeDisclaimer()">
                                                <%= Resources.Lang.Res_Global_Agree%></button>
                                        </td>
                                        <td style="width: 130px;">
                                            <button type="button" class="am-btn am-btn-default am-round button-all" style="float: right;"
                                                onclick="DisAgreeDisclaimer()">
                                                <%= Resources.Lang.Res_Global_Cancel%></button>
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
    <%--免责申明(CHT)--%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="modal-disclaimer-cht">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                <%= Resources.Lang.Res_Login_Button_Term%>
            </div>
            <div class="am-modal-bd">
                <div style="padding: 20px 50px 20px 50px; height: 700px; overflow-y: scroll">
                    <table style="width: 100%; text-align: left;">
                        <tr>
                            <td class="disclaimer-td-title">
                                1. 接 受 條 款 及 條 件
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                本 文 所 用 「 和 記 」 及 「 吾 等 」 詞 語 乃 指 和 記 物 業 管 理 有 限 公 司 ， 除 非 文 意 另 有 所 指 ， 否 則 該
                                等 詞 語 亦 包 括 其 任 何 董 事 、 僱 員 、 高 級 職 員 及 代 理 。 和 記 透 過 所 經 營 的 手 機 應 用 程（下 稱「 手 機
                                應 用 程 式 」 ），為 本 手 機 應 用 程 式 用 戶（下 稱「用 戶」）提 供 多 元 化 之 物 業 管 理 服 務（下 稱「服 務」），包 括 通
                                告 及 資 訊、會 所 資 訊 及 意 見 箱。
                                <br />
                                <br />
                                本 文 所 用 「 閣 下 」 及 「 用 戶 」 詞 語 是 指 為 任 何 理 由 或 目 的 存 取 服 務 之 所 有 個 人 及 ／ 或 實 體 。
                                本 文 所 用 「 內 容 」 一 詞 乃 指 所 有 或 任 何 （ 視 情 況 而 定 ） 數 據 、 文 本 、 按 鈕 圖 像 、 連 結 、 HTML編
                                碼 、 商 標 、 軟 件 、 音 樂 、 音 響 、 照 片 、 圖 解 、 硬 照 、 電 影 片 斷 （ 不 論 是 否 動 畫 ） 、 錄 影 、 商
                                品 、 產 品 、 廣 告 、 服 務 或 其 任 何 匯 輯 或 結 合 ， 以 及 在 任 何 手 機 應 用 程 式 或 任 何 網 頁 發 現 之 任
                                何 其 他 內 容 或 資 料 。
                                <br />
                                <br />
                                和 記 向 閣 下 提 供 服 務 ， 惟 須 受 以 下 使 用 條 款 及 條 件 （下 稱「 章 則 」 ） 之 規 限 ， 吾 等 可 以 不 時 透
                                過 在 網 上 登 載 經 修 訂 版 本 而 更 新 及 修 訂 此 等 條 款 及 條 件 。 除 非 吾 等 另 有 註 明 外 ， 否 則 任 何 吾
                                等 所 作 之 變 動 將 於 登 載 時 即 時 生 效 。 閣 下 於 登 載 最 新 版 本 後 使 用 手 機 應 用 程 式 ， 即 表 示 閣 下
                                不 可 推 翻 地 接 納 有 關 變 動 。 請 定 期 審 閱 章 則 ， 以 確 保 閣 下 熟 悉 最 新 版 本 。 閣 下 承 諾 不 會 以 閣
                                下 不 知 道 或 吾 等 沒 有 通 知 閣 下 有 關 條 款 及 條 件 出 現 任 何 變 動 而 作 為 出 現 爭 議 時 之 抗 辯 理 由 。
                                若 干 服 務 可 能 須 受 制 於 在 提 供 時 進 行 規 管 之 額 外 條 款 及 條 件 ， 而 在 閣 下 表 示 有 意 使 用 此 等 服
                                務 時 ， 閣 下 會 獲 悉 此 等 額 外 條 款 。 此 等 額 外 條 款 及 條 件 ， 連 同 免 責 聲 明、版 權 聲 明 及 私 隱 政 策
                                聲 明， 均 透 過 提 述 納 入 章 則 內 。
                                <br />
                                <br />
                                閣 下 接 受 及 同 意 ， 在 使 用 任 何 服 務 或 以 其 他 方 式 存 取 手 機 應 用 程 式 或 使 用 其 內 出 現 之 任 何 資
                                料 時 ， 須 受 章 則 約 束 。 如 果 閣 下 不 接 受 章 則 ， 則 不 可 亦 不 應 存 取 手 機 應 用 程 式 或 使 用 手 機 應
                                用 程 式 所 提 供 之 服 務 或 資 料 。 閣 下 提 議 之 任 何 其 他 條 款 及 條 件 ， 如 果 附 加 於 章 則 之 上 或 與 章
                                則 有 衝 突 ， 均 被 和 記 明 文 拒 絕 ， 概 無 效 力 及 效 果 。 如 果 閣 下 對 章 則 或 存 取 及 使 用 手 機 應 用 程
                                式 有 任 何 疑 問 ， 請與 吾 等 聯 絡 。 吾 等 保 留 解 釋 章 則 及 對 章 則 所 引 致 問 題 或 爭 議 作 裁 決 之 權 利 。
                                閣 下 同 意 所 有 該 等 解 釋 和 裁 決 均 屬 最 終 定 論 及 不 可 推 翻 ， 對 閣 下 作 為 手 機 應 用 程 式 用 戶 具 約
                                束 力 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                2. 服 務 簡 介
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                所 有 服 務 ， 連 同 提 高 或 改 善 現 時 所 提 供 服 務 之 任 何 新 特 色 ， 除 非 另 有 明 文 作 相 反 註 明 ， 否 則
                                均 受 章 則 之 規 限 。 閣 下 承 認 ， 透 過 互 聯 網 及 電 訊 網 絡 作 通 訊 有 其 固 有 風 險 ， 所 提 供 資 訊 可 能
                                受 到 延 誤 、 遺 漏 或 不 準 確 。 閣 下 亦 明 白 及 同 意 ， 服 務 乃 以 「 按 現 狀 」 及 「 按 所 能 提 供 」 方 式
                                提 供 ， 和 記 不 就 服 務 之 適 時 、 刪 除 、 交 付 出 錯 或 未 能 提 供 而 負 上 責 任 。 手 機 應 用 程 式 及 連 繫
                                手 機 應 用 程 式 （下 稱「 連 繫 」 ） 登 載 取 自 第 三 者 之 數 據 、 廣 告 、 文 章 及 資 訊 ， 以 上 種 種 可 能 不
                                準 確 或 有 錯 漏 。 有 關 資 訊 乃 按 以 下 附 帶 條 件 而 透 過 服 務 或 任 何 連 繫 而 提 供 ： (a) 吾 等 不 就 (i)
                                任 何 錯 漏 ； 或 (ii) 在 該 等 資 訊 基 礎 上 採 取 任 何 行 動 之 結 果 或 所 出 現 之 遺 漏 負 責 ； (b) 吾 等 並
                                無 評 估 或 核 實 所 載 資 料 ； 及 (c) 該 等 資 料 不 應 視 為 經 由 和 記 明 示 或 暗 示 認 可 或 核 實 。
                                <br />
                                <br />
                                閣 下 明 白 ， 服 務 之 技 術 處 理 及 傳 送 可 能 涉 及 (a) 透 過 不 同 網 絡 傳 送 ； 及 (b) 為 符 合 及 適 應 接
                                駁 網 絡 、 裝 置 或 媒 體 之 技 術 要 求 而 作 出 改 變 。 和 記 因 而 在 任 何 情 況 下 均 毋 須 就 因 任 何 通 訊 網
                                絡 、 裝 置 或 媒 體 或 提 供 接 駁 或 所 需 支 援 （ 包 括 電 力 供 應 ） 人 士 之 延 誤 或 故 障 而 令 服 務 全 部 或
                                部 分 出 現 故 障 或 閣 下 未 能 全 部 或 部 分 取 得 該 等 服 務 而 負 責 。 吾 等 並 無 擔 保 接 駁 手 機 應 用 程 式
                                會 從 不 中 斷 、 持 續 可 靠 。 部 分 或 整 個 手 機 應 用 程 式 可 能 會 在 任 何 時 間 因 任 何 理 由 而 突 發 性 地
                                不 能 如 常 運 作 ， 這 包 括 系 統 失 靈 及 干 擾 、 互 聯 網 接 駁 停 頓 及 其 他 超 出 吾 等 控 制 能 力 範 圍 之 技
                                術 問 題 ， 吾 等 不 能 亦 不 應 為 此 負 上 責 任 。 閣 下 同 意 ， 閣 下 不 會 因 任 何 理 由 未 能 使 用 手 機 應 用
                                程 式 而 引 致 之 任 何 損 害 或 損 失 而 需 要 吾 等 負 責 。 吾 等 保 留 權 利 ， 因 各 種 理 由 （ 包 括 緊 急 系 統
                                維 修 或 提 升 ） 而 令 任 何 部 份 或 整 個 手 機 應 用 程 式 離 線 ， 惟 吾 等 在 實 際 可 行 的 情 形 下 ， 盡 可 能
                                向 閣 下 發 出 事 先 通 知 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                3. 登 記
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                閣 下 可 能 被 要 求 填 報 一 份 或 以 上 登 記 或 申 請 表 格 （ 在 網 上 或 以 其 他 方 式 填 報 ） ， 然 後 若 干 服
                                務 才 會 向 閣 下 提 供 。 在 填 報 該 等 表 格 時 ， 閣 下 承 諾 (a) 提 供 有 關 閣 下 之 真 確 、 現 行 及 完 整 之
                                個 人 可 識 別 資 料 ， (b) 提 供 有 關 閣 下 透 過 任 何 手 機 應 用 程 式 刊 登 之 任 何 內 容 之 真 確 、 現 行 及
                                完 整 資 料 （下 稱「 其 他 資 料 」 ） 及 (c) 維 持 及 即 時 更 新 閣 下 之 個 人 可 識 別 資 料 及 其 他 資 料 ， 以
                                保 持 真 確 、 現 行 及 完 整 。 倘 若 閣 下 提 供 任 何 不 真 確 、 不 現 行 或 不 完 整 之 其 他 資 料 或 個 人 可 識
                                別 資 料 ， 又 或 者 和 記 有 合 理 理 由 懷 疑 該 等 資 料 不 真 確 、 不 現 行 或 不 完 整 ， 則 和 記 有 權 暫 停 或
                                終 止 閣 下 賬 戶 ， 並 拒 絕 閣 下 現 行 或 將 來 使 用 任 何 及 全 部 服 務 （ 或 其 任 何 部 分 ） 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                4. 未 成 年 人
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                倘 若 閣 下 不 足 十 八 歲 或 未 能 承 擔 法 律 責 任 ， 則 閣 下 應 就 此 等 條 款 及 條 件 之 涵 義 及 後 果 尋 求 父
                                母 或 監 護 人 意 見 ， 然 後 才 可 使 用 任 何 服 務 。 除 非 閣 下 已 徵 得 父 母 或 監 護 人 同 意 ， 否 則 不 應 向
                                吾 等 提 供 個 人 可 識 別 資 料 。 和 記 假 定 ， 每 名 手 機 應 用 程 式 用 戶 均 擁 有 使 用 服 務 之 所 需 行 為 能
                                力 ， 而 閣 下 使 用 任 何 服 務 ， 即 等 於 向 吾 等 承 諾 及 保 證 閣 下 已 具 備 必 需 之 行 為 能 力 。 和 記 毋 須
                                核 實 任 何 用 戶 之 年 齡 或 行 為 能 力 ， 但 假 如 發 現 任 何 用 戶 缺 乏 所 需 行 為 能 力 ， 和 記 則 保 留 向 該
                                等 用 戶 或 其 父 母 或 監 護 人 採 取 行 動 之 權 利 ， 包 括 有 權 要 求 彼 等 就 因 缺 乏 行 為 能 力 而 導 致 吾 等
                                蒙 受 之 所 有 損 失 或 損 害 而 作 出 彌 償 ， 又 或 註 銷 其 登 記 及 終 止 其 存 取 手 機 應 用 程 式 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                5. 私 隱 政 策
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                個 人 可 識 別 資 料 乃 受 吾 等 之 私 隱 政 策 規 限 。 請 參 閱 吾 等 之 私 隱 政 策。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                6. 個 人 識 別 號 碼 、 密 碼 及 保 安
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                若 果 閣 下 已 收 到 作 為 存 取 手 機 應 用 程 式 之 若 干 部 份 的 密 碼 及 ／ 或 個 人 識 別 號 碼 （下 稱「 PIN」 ）
                                ， 閣 下 可 不 時 改 動 密 碼 或 PIN（ 透 過 手 機 應 用 程 式 或 填 報 申 請 表 格） 。 閣 下 須 負 責 為 上 述 密 碼
                                及 PIN保 密 ， 並 就 在 該 等 密 碼 或 PIN下 發 生 之 所 有 活 動 而 負 全 責 。 閣 下 同 意 ， (a) 即 時 通 知 和
                                記 有 關 閣 下 密 碼 或 PIN遭 擅 自 使 用 或 任 何 其 他 違 反 保 安 之 情 況 ， 及 (b) 確 保 於 每 次 瀏 覽 手 機 應
                                用 程 式 後 均 退 出 閣 下 之 密 碼 或 PIN。 和 記 不 能 亦 不 會 就 因 閣 下 無 法 遵 從 本 第 6條 規 定 或 閣 下 無
                                法 遵 從 採 用 任 何 與 服 務 有 關 之 數 碼 或 電 子 證 書 而 引 致 之 任 何 損 失 或 損 害 而 負 責 。
                                <br />
                                <br />
                                閣 下 承 認 上 述 密 碼 及 PIN乃 屬 保 密 ， 在 任 何 情 況 下 均 不 得 向 其 他 人 披 露 。 閣 下 之 任 何 字 句 、 行
                                動 或 行 為 ， 不 論 是 有 意 或 無 意 ， 如 果 違 反 保 密 規 定 ， 則 須 就 因 此 而 引 致 之 一 切 損 失 及 損 害 向
                                和 記 負 責 。 閣 下 須 採 取 一 切 必 需 預 防 措 施 ， 維 持 上 述 密 碼 及 PIN之 保 密 性 。 閣 下 同 意 及 承 認 ，
                                任 何 人 士 以 該 等 密 碼 及 /或 PIN(視 乎 情 況 而 定 )使 用 服 務 （ 不 論 是 否 已 獲 閣 下 授 權 ） ， 均 構 成
                                及 被 視 為 閣 下 在 使 用 服 務 。 如 發 現 任 何 實 際 或 可 能 擅 自 使 用 該 等 密 碼 及 /或 PIN， 閣 下 須 立 即
                                通 知 和 記 ， 並 沒 有 延 誤 地 以 書 面 向 和 記 確 認 上 述 情 況 。 在 和 記 接 獲 該 等 書 面 確 認 之 前 ， 閣 下
                                不 得 向 和 記 索 償 ， 並 須 就 因 使 用 服 務 （ 不 論 是 否 已 獲 閣 下 授 權 ）而 令 閣 下 招 致 之 一 切 損 失 及 損
                                害 而 向 和 記 負 責 。
                                <br />
                                <br />
                                閣 下 承 認 及 同 意 ， 和 記 之 唯 一 職 責 是 核 對 該 等 密 碼 及 PIN， 而 和 記 毋 須 就 以 下 事 項 負 責 :
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                (a) 因 未 能 透 過 各 種 電 子 交 付 渠 道 去 執 行 指 示 或 履 行 任 何 義 務 （ 直 接 或 間 接 歸 咎 於 超 出 吾 等
                                控 制 能 力 範 圍 之 任 何 情 況 或 事 件 ） 而 導 致 閣 下 或 任 何 其 他 人 士 蒙 受 任 何 損 失 或 損 害 ； 或
                                <br />
                                (b) 給 予 正 確 密 碼 及 ／ 或 PIN而 透 過 各 種 電 子 交 付 渠 道 發 出 之 任 何 指 示 而 導 致 令 閣 下 或 任 何 其
                                他 人 士 蒙 受 任 何 其 他 損 失 或 損 害 。 閣 下 承 認 ， 鑑 於 互 聯 網 之 性 質 ， 吾 等 不 能 擔 保 在 服 務 項 下
                                個 人 可 識 別 資 料 、 內 容 或 其 他 資 料 之 傳 送 一 定 完 全 穩 妥 。 閣 下 留 意 到 ， 在 傳 送 時 用 作 保 障 閣
                                下 個 人 可 識 別 資 料 安 全 的 只 有 加 密 閣 下 所 輸 入 資 料 之 Secure Sockets Layer（ SSL） 軟 件 。 閣
                                下 使 用 或 取 覽 任 何 服 務 ， 就 等 於 承 認 閣 下 信 納 吾 等 所 採 取 之 保 安 措 施 就 達 致 閣 下 所 有 目 的 而
                                言 已 經 足 夠 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                7. 用 戶 行 為
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                刊 登 於 手 機 應 用 程 式 或 透 過 手 機 應 用 程 式 取 覽 之 任 何 內 容 ， 不 論 是 公 開 張 貼 或 私 人 傳 送 ， 均
                                須 由 最 先 發 出 該 等 內 容 之 人 士 負 全 責 。 即 是 說 ， 作 為 內 容 起 源 之 用 戶 （ 而 非 和 記 ） 須 就 其 就
                                透 過 服 務 而 上 載 、 張 貼 、 電 郵 、 提 供 或 以 其 他 方 式 傳 送 之 一 切 內 容 負 全 責 。 和 記 並 不 監 控 透
                                過 服 務 而 張 貼 之 內 容 ， 故 此 並 不 擔 保 該 等 內 容 之 準 確 性 、 完 整 性 或 質 素 。 就 透 過 服 務 而 取 覽
                                內 容 之 用 戶 而 言 ， 彼 等 明 白 及 承 認 ， 在 使 用 服 務 之 同 時 ， 彼 等 會 接 觸 到 令 人 厭 惡 、 不 雅 或 不
                                良 之 內 容 。 在 任 何 情 況 下 ， 和 記 均 毋 須 就 任 何 內 容 之 錯 漏 或 因 使 用 透 過 服 務 而 張 貼 、 電 郵 、
                                提 供 或 以 其 他 方 式 傳 送 之 任 何 內 容 而 招 致 任 何 損 失 或 損 害 而 負 責 。<br />
                                <br />
                                閣 下 承 諾 不 使 用 服 務 作 以 下 用 途 ：
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                a. 上 載 、 張 貼 、 電 郵 、 提 供 、 刊 登 、 廣 播 、 分 派 、 複 製 或 以 任 何 方 式 傳 送 閣 下 沒 有 權 利 （ 所
                                有 權 、 合 約 權 或 受 信 權 ） 傳 送 或 本 身 屬 非 法 、 有 害 、 有 威 脅 性 、 粗 俗 、 騷 擾 、 侵 權 、 不 道 德
                                、 不 妥 當 、 淫 褻 、 不 雅 、 煽 動 性 、 令 人 反 感 、 歧 視 性 、 誹 謗 性 或 其 他 令 人 反 感 之 內 容 或 其 任
                                何 部 分 ；
                                <br />
                                b. 透 過 使 用 服 務 傳 送 任 何 偽 造 或 篡 改 之 內 容 ， 從 而 假 冒 任 何 人 士 或 實 體 又 或 訛 稱 閣 下 與 某 人
                                或 某 實 體 有 從 屬 關 係 ；
                                <br />
                                c. 上 載 、 張 貼 、 電 郵 、 提 供 、 刊 登 、 廣 播 、 分 派 、 複 製 或 以 任 何 方 式 傳 送 侵 犯 所 有 權 或 知 識
                                產 權 （ 包 括 但 不 限 於 任 何 一 方 之 專 利 、 商 標 、 商 業 秘 密 或 版 權 ） 之 內 容 或 其 任 何 部 分 ；
                                <br />
                                d. 上 載 、 張 貼 、 電 郵 、 提 供 、 刊 登 、 廣 播 、 分 派 、 複 製 或 以 任 何 方 式 傳 送 任 何 主 動 提 供 或 未
                                經 授 權 之 廣 告 、 宣 傳 資 料 、 「 垃 圾 郵 件 」 、 「 濫 發 郵 件 」 、 「 連 鎖 郵 件 」 、 「 層 壓 式 推 銷 計
                                劃 」 、 投 資 機 會 或 任 何 其 他 方 式 之 兜 售 ， 惟 獲 吾 等 明 文 授 權 者 除 外 ；
                                <br />
                                e. 上 載 、 張 貼 、 電 郵 、 廣 播 、 分 派 、 複 製 或 以 任 何 方 式 傳 送 含 有 軟 件 病 毒 或 會 干 擾 、 毀 壞 或
                                限 制 手 機 應 用 程 式 及 /或 電 腦 軟 件 或 硬 件 或 電 訊 設 備 之 功 能 之 任 何 其 他 電 腦 密 碼 、 檔 案 或 程 式
                                ；
                                <br />
                                f. 干 擾 、 減 損 或 中 斷 服 務 或 伺 服 器 或 網 絡 或 電 訊 服 務 之 提 供 或 操 作 ， 又 或 不 遵 從 該 等 網 絡 或
                                電 訊 服 務 之 任 何 規 定 、 程 序 、 政 策 或 規 例 ；
                                <br />
                                g. 透 過 搜 尋 密 碼 或 任 何 其 他 方 法 ， 試 圖 擅 自 取 覽 手 機 應 用 程 式 ， 取 得 其 他 用 戶 之 賬 戶 或 密 碼
                                ， 以 及 與 手 機 應 用 程 式 有 連 繫 之 電 腦 系 統 或 網 絡 ；
                                <br />
                                h. 干 擾 對 話 之 正 常 運 行 ， 引 致 屏 幕 的 滑 動 速 度 比 其 他 用 戶 能 夠 打 字 之 速 度 為 快 ， 又 或 以 影 響
                                其 他 用 戶 作 即 時 交 流 之 能 力 之 其 他 方 式 進 行 干 擾 ；
                                <br />
                                i. 偽 造 標 題 或 以 其 他 方 式 篡 改 識 別 標 記 ， 以 掩 飾 透 過 服 務 傳 送 內 容 之 來 源 ；
                                <br />
                                j. 修 改 、 改 編 、 再 特 許 、 翻 譯 、 出 售 、 反 向 製 作 、 拆 散 或 分 解 手 機 應 用 程 式 任 何 部 分 或 手 機
                                應 用 程 式 所 載 或 與 服 務 有 關 而 使 用 之 軟 件 （ 包 括 以 軟 件 收 納 或 產 生 之 任 何 檔 案 、 影 像 及 伴 隨
                                軟 件 之 數 據 ） ；
                                <br />
                                k. 移 走 手 機 應 用 程 式 所 載 任 何 有 關 版 權 、 商 標 或 其 他 所 有 權 之 通 告 ；
                                <br />
                                l. 未 經 吾 等 事 前 書 面 授 權 ， 為 手 機 應 用 程 式 任 何 部 分 「 設 框 」 或 「 裝 鏡 」；
                                <br />
                                m. 使 用 任 何 機 械 人 、 「 蛛 網 」 、 手 機 應 用 程 式 搜 尋 ／ 檢 索 方 法 或 其 他 人 手 或 自 動 裝 置 或 程 序
                                進 行 檢 索 、 索 引 及 「 搜 尋 數 據 」 ， 又 或 以 任 何 方 式 複 製 或 規 避 手 機 應 用 程 式 或 其 內 容 之 導 航
                                索 引 結 構 或 展 示 方 法；及
                                <br />
                                n. 收 集 或 貯 存 有 關 其 他 用 戶 之 個 人 可 識 別 資 料 。
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                在 不 局 限 前 文 之 一 般 性 原 則 下 ， 閣 下 進 一 步 同 意 不 會 侵 入 、 撞 入 、 存 取 、 使 用 或 試 圖 侵 入 、
                                撞 入 、 存 取 、 使 用 未 獲 吾 等 授 權 之 吾 等 伺 服 器 之 任 何 其 他 部 份 及 /或 任 何 數 據 範 圍 。
                                <br />
                                <br />
                                閣 下 承 認 ， 和 記 並 不 積 極 監 察 用 戶 透 過 手 機 應 用 程 式 而 提 交 、 張 貼 、 傳 送 或 寄 發 之 內 容 ， 亦
                                沒 有 義 務 去 預 先 篩 選 該 等 內 容 ， 但 和 記 有 全 權 （ 但 並 非 有 義 務 ） 以 任 何 理 由 去 剪 輯 、 拒 絕 、
                                移 除 或 重 新 編 排 透 過 服 務 而 提 供 之 任 何 內 容 ， 毋 須 於 事 前 徵 詢 或 取 得 閣 下 意 見 ， 亦 毋 須 就 該
                                等 剪 輯 、 拒 絕 、 移 除 或 重 新 編 排 而 引 致 之 任 何 損 失 或 損 害 而 向 閣 下 負 責 。 在 不 局 限 上 述 權 利
                                之 情 況 下 ， 和 記 有 權 移 除 其 認 為 違 反 章 則 又 或 其 他 其 認 為 令 人 反 感 之 任 何 內 容 。 閣 下 承 諾 評
                                估 及 同 意 評 估 及 承 擔 與 使 用 任 何 內 容 有 關 之 一 切 風 險 ， 包 括 對 該 等 內 容 之 推 定 準 確 性 、 完 整
                                性 或 實 用 性 之 倚 賴 。 為 此 ， 閣 下 承 認 ， 閣 下 不 會 倚 賴 和 記 所 撰 寫 或 提 交 予 和 記 之 任 何 內 容 ，
                                包 括 但 不 限 於 在 意 見 箱 及 服 務 所 有 其 他 部 分 所 載 之 資 料 。 吾 等 毋 須 就 沒 有 移 除 或 延 遲 移 除 由
                                第 三 者 起 源 或 提 供 之 有 害 、 不 確 、 不 合 法 令 人 反 感 之 內 容 而 負 責 。
                                <br />
                                <br />
                                閣 下 承 認 及 同 意 ， 當 法 律 要 求 時 又 或 當 其 真 誠 相 信 該 等 保 留 或 披 露 屬 合 理 需 要 時 ， 和 記 可 保
                                留 或 披 露 內 容 ， 這 不 限 於 ： (a) 遵 從 合 法 要 求 及 ／ 或 法 律 程 序 ； (b) 強 制 執 行 章 則 ； 或 (c)
                                保 障 和 記 及 /或 其 聯 屬 成 員 、 其 用 戶 或 公 眾 之 權 利 、 財 物 或 個 人 安 全 ； (d) 就 針 對 有 內 容 侵 犯
                                第 三 者 權 利 所 提 索 償 而 作 出 回 應 ， 或 (e) 管 理 有 關 服 務 。
                                <br />
                                <br />
                                吾 等 可 就 服 務 制 訂 一 般 慣 例 或 釐 定 限 制 ， 包 括 但 不 限 於 為 電 郵 及 其 他 用 戶 內 容 之 貯 存 時 間 、
                                用 戶 賬 戶 進 出 信 息 之 數 目 、 信 息 之 大 小 ， 以 及 用 戶 可 用 貯 存 總 量 而 訂 出 上 限 。 吾 等 毋 須 就 透
                                過 服 務 而 維 持 或 傳 送 之 信 息 、 其 他 通 訊 或 其 他 內 容 之 刪 除 或 遺 失 而 負 責 。 閣 下 承 認 ， 和 記 保
                                留 權 利 ， 在 有 通 知 或 沒 有 通 知 之 情 況 下 隨 時 酌 情 更 改 此 等 一 般 慣 例 及 限 制 。
                                <br />
                                <br />
                                擅 自 在 手 機 應 用 程 式 及 所 載 任 何 資 料 、 數 據 及 材 料 上 作 出 改 動 或 增 添 ， 均 屬 嚴 格 禁 止 之 列 。
                                沒 有 在 明 文 授 予 之 一 切 權 利 均 予 保 留 。 此 等 條 款 及 條 件 乃 取 代 吾 等 （ 包 括 吾 等 任 何 僱 員 、 高
                                級 職 員 、 董 事 及 代 理 ） 與 閣 下 之 間 就 手 機 應 用 程 式 所 作 之 任 何 先 前 或 同 一 時 期 之 通 訊 。
                                <br />
                                <br />
                                吾 等 保 留 權 利 ， 不 時 毋 須 通 知 而 取 覽 閣 下 賬 戶 或 觀 察 及 記 錄 閣 下 取 覽 及 使 用 手 機 應 用 程 式 之
                                情 況 ， 以 斷 定 閣 下 是 否 遵 從 章 則 。
                                <br />
                                <br />
                                閣 下 須 遵 從 與 使 用 手 機 應 用 程 式 有 關 之 一 切 適 用 法 律 、 法 規 、 條 例 及 規 例 （ 不 論 是 否 有 法 律
                                效 力 ） （下 稱「 適 用 法 律 」 ） 。 閣 下 認 識 到 互 聯 網 之 全 球 性 質 ， 閣 下 明 白 ， 適 用 法 律 可 能 出 於
                                並 非 閣 下 本 身 所 處 之 司 法 地 區 ， 而 閣 下 亦 同 意 ， 遵 從 適 用 法 律 乃 閣 下 之 獨 家 責 任 。 吾 等 建 議
                                ， 倘 若 閣 下 不 清 楚 適 用 法 律 所 指 何 物 ， 應 就 此 自 行 尋 求 法 律 意 見 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                8. 商 品
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                第 三 者 在或透過 手 機 應 用 程 式 上 提 供 、 介 紹 或 提 述 之 所 有 商 品 、 產 品 或 服 務 （ 「 商 品 」 ） 並 非 由
                                吾 等 提 出 或 提 供 。 閣 下 在 任 何 情 況 下 不 得 就 手 機 應 用 程 式 所 提 供 之 平 台 所 閱 覽 有 關 商 品 之 任
                                何 資 料 之 準 確 性 或 可 靠 性 而 依 賴 和 記 。 購 買 、 購 入 或 取 得 商 品 之 合 約 ， 將 由 閣 下 與 出 售 、 提
                                呈 或 提 供 商 品 之 第 三 者 達 成 。和 記 並 非 有 關 合 約 之 其 中 一 方 ， 乃 僅 提 供 一 個 平 台 ， 得 以 提 供 有
                                關 服 務 或 進 行 商 業 活 動。任 何 付 款 應 直 接 向 銷 售 或 提 供 商 品 之 第 三 方。和 記 不 涉 及 閣 下 與 第 三 者
                                之 間 的 任 何 交 易，且 不 承 擔 任 何 責 任，也 不 承 擔 任 何 因 任 何 付 款 （無 論 是 網 上 或 非 網 上）而 造 成 或 據
                                稱 造 成 或 與 之 有 關 的 任 何 損 失 或 損 害。閣 下 務 請 在 與 任 何 上 述 第 三 者 進 行 任 何 交 易 （ 不 論 網 上
                                或 非 網 上 ） 之 前 作 出 閣 下 認 為 所 須 或 合 適 之 調 查 。 因 此 ，和 記 對 有 關 供 應 是 否 可 得 到 或 是 否 適
                                宜 購 買 或 是 否 適 宜 作 某 個 用 途 方 面 ， 並 未 作 出 明 示 或 暗 示 之 陳 述 及 保 證 及 ／ 或 對 上 述 種 種 概
                                不 負 責 ， 亦 並 未 就 有 關 商 品 之 表 現 過 程 或 行 為 過 程 作 出 任 何 暗 示 之 保 證 及 ／ 或 對 上 述 種 種 概
                                不 負 責 。和 記 對 於 在 手 機 應 用 程 式 或 透 過 手 機 應 用 程 式 所 提 供 、 介 紹 、 提 述 或 供 應 之 商 品 未 能
                                獲 供 應 、 使 用 、 未 能 使 用 、 性 能 表 現 或 性 能 失 常 概 不 負 責 及 在 任 何 情 況 下 概 不 負 責 任 何 賠 損
                                ， 包 括 但 不 限 於 採 購 取 代 貨 品 或 服 務 、 溢 利 損 失 、 遺 失 數 據 或 任 何 直 接 、 間 接 、 特 別 、 懲 款
                                性 、 相 關 、 懲 罰 性 或 相 應 賠 償 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                9. 與 廣 告 商 之 交 易
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                閣 下 與 在 手 機 應 用 程 式 刊 登 廣 告 之 第 三 者 或 在 手 機 應 用 程 式 或 透 過 手 機 應 用 程 式 發 現 之 其 他
                                各 方 之 通 訊 或 業 務 交 易 或 參 與 其 中 之 推 廣 ， 包 括 支 付 及 交 付 有 關 貨 品 或 服 務 以 及 涉 及 該 項 交
                                易 之 任 何 其 他 條 款 、 條 件 、 保 證 或 陳 述 ， 僅 屬 閣 下 與 該 第 三 者 之 間 之 事 宜 。 閣 下 同 意 ，和 記 無
                                須 因 任 何 有 關 交 易 或 因 在 手 機 應 用 程 式 上 出 現 有 關 廣 告 而 招 致 而 任 何 損 失 或 賠 償 負 責 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                10. 向 公 眾 披 露 資 料
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                閣 下 在 發 出 任 何 內 容 、 其 他 資 料 或 可 識 別 閣 下 之 資 料 前 ， 閣 下 必 須 查 核 是 否 向 手 機 應 用 程 式
                                之 公 用 區 域 發 出 。 倘 閣 下 向 公 用 區 域 發 出 上 述 者 ， 閣 下 知 悉 閣 下 乃 自 願 如 此 ， 而 (i) 只 要 閣
                                下 仍 選 擇 將 有 關 內 容 載 入 手 機 應 用 程 式 ， 則 和 記 獲 授 全 球 、 免 專 利 費 、 無 條 件 、 永 久 、 不 可
                                撤 回 及 （ 透 過 多 層 渠 道 ） 可 再 授 出 之 許 可 證 ， 以 使 用 、 複 製 、 修 訂 、 採 用 、 編 排 及 刊 載 有 關
                                內 容 ， 而 且 (ii) 任 何 人 士 可 利 用 有 關 內 容 以 交 換 電 郵 或 與 用 戶 彼 此 聊 天 或 將 閣 下 介 紹 予 其 他
                                用 戶 。 閣 下 自 願 在 公 用 區 域 提 供 之 所 有 內 容 ， 不 得 被 視 為 和 記 所 收 集 者 。
                                <br />
                                <br />
                                閣 下 進 一 步 同 意 吾 等 可 隨 意 使 用 、 披 露 、 採 用 及 修 訂 閣 下 就 手 機 應 用 程 式 而 向 吾 等 提 供 之 所
                                有 及 任 何 意 見 、 概 念 、 技 術 、 建 議 、 意 見 、 評 論 及 其 他 通 訊 及 資 料 （ 「 回 應 」 ） ， 而 無 須 向
                                閣 下 支 付 任 何 款 項 。 閣 下 謹 此 豁 免 涉 及 有 關 使 用 、 披 露 、 採 納 及 ／ 或 修 訂 任 何 或 所 有 閣 下 之
                                回 應 之 任 何 代 價 、 費 用 、 專 利 費 、 收 費 及 ／ 或 其 他 款 項 之 所 有 及 任 何 權 利 及 索 償 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                11. 彌 償
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                閣 下 同 意 就 因 (i) 閣 下 之 行 為 或 未 有 所 行 為 而 違 反 章 則 ； (ii) 閣 下 使 用 任 何 服 務 或 透 過 閣 下
                                之 登 入 而 使 用 任 何 服 務 或 (iii) 閣 下 或 透 過 使 用 閣 下 之 登 入 而 經 過 服 務 提 呈 、 張 貼 或 傳 送 之
                                內 容 、 其 他 資 料 或 可 識 別 閣 下 之 資 訊 所 導 致 之 任 何 及 所 有 索 償 、 訴 訟 、 賠 損 、 損 傷 、 責 任 而
                                對 和 記 及 其 母 公 司、附 屬 公 司 、 聯 屬 公 司 、 高 級 職 員 、 代 理 、 共 同 商 標 者 或 其 他 夥 伴 及 僱 員 作
                                出 彌 償 、 抗 辯 及 令 其 無 損 。<br />
                                <br />
                                和 記 或 閣 下 終 止 閣 下 使 用 服 務 之 權 利 ， 絕 不 會 除 去 或 在 其 他 方 面 影 響 閣 下 向 和 記 作 出 彌 償 之
                                責 任 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                12. 不 得 轉 售 服 務
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                閣 下 承 諾 不 得 複 製 、 複 印 、 翻 印 、 出 售 、 轉 售 任 何 部 份 之 服 務 、 服 務 用 途 或 存 取 服 務 方 式 以
                                作 商 業 用 途 。 閣 下 不 得 以 任 何 可 導 致 或 加 強 和 記 及 ／ 或 其 聯 屬 公 司 之 業 務 出 現 任 何 直 接 或 間
                                接 競 爭 之 形 式 使 用 、 轉 讓 、 分 發 或 處 置 服 務 所 載 之 任 何 材 料 及 資 料 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                13. 修 訂 服 務
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                和 記 保 留 權 利 ， 不 論 有 否 向 閣 下 發 出 通 知 下 基 於 下 列 或 其 他 原 因 ， 隨 時 及 不 時 地 暫 時 或 永 久
                                修 訂 或 終 止 向 閣 下 提 供 之 服 務 （ 或 其 中 任 何 部 份 ） ， 並 可 明 確 訂 明 有 關 適 用 之 任 何 額 外 條 款
                                或 任 何 變 動 ：
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                (a) 所 有 和 記 與 其 下物 業 就 提 供 （ 其 中 包 括 ） 服 務 之 全 部 協 議 被 終 止 ；
                                <br />
                                (b) 就 有 關 服 務 而 出 現 之 任 何 技 術 上 之 失 靈 、 修 正 或 維 修 ；
                                <br />
                                (c) 和 記 認 為 任 何 關 於 服 務 或 和 記 或 服 務 之 任 何 其 他 使 用 者 之 保 安 實 為 或 變 得 不 能 接 受 ；
                                <br />
                                (d) 閣 下 違 反 章 則 之 任 何 條 款 或 與 和 記 所 訂 立 之 任 何 其 他 安 排 ；
                                <br />
                                (e) 閣 下 從 事 或 容 許 從 事 任 何 和 記 認 為 可 能 對 服 務 之 運 作 有 損 之 事 宜 ； 或
                                <br />
                                (f) 倘 閣 下 不 再 為 物 業 之 住 戶 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                閣 下 同 意 和 記 不 必 就 服 務 之 任 何 修 改 、 暫 停 或 中 斷 向 閣 下 或 任 何 第 三 者 負 責 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                14. 連 結
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                手 機 應 用 程 式 中 載 有 連 結 ， 連 結 至 並 非 由 和 記 所 維 持 或 控 制 之 萬 維 網 其 他 網 站或 網 頁 ， 而 和 記
                                概 不 對 此 負 責 。 該 等 連 結 的 提 供 僅 為 方 便 閣 下 瀏 覽 ， 而 載 入 任 何 連 結 並 不 表 示 經 和 記 認 可 或
                                核 證 。 倘 閣 下 啟 動 任 何 該 等 連 結 後 閣 下 離 開 手 機 應 用 程 式 ， 則 閣 下 須 自 己 承 擔 接 達 該 等 網 站或
                                網 頁 的 風 險 ， 而 和 記 毋 須 就 該 等 網 址 或 網 頁 所 提 供 之 任 何 內 容 、 廣 告 、 產 品 或 其 他 材 料 負 責
                                。 閣 下 進 一 步 確 認 及 同 意 ， 和 記 毋 須 直 接 或 間 接 就 因 或 涉 及 使 用 或 依 賴 於 或 透 過 任 何 該 等網 站或
                                網 頁 提 供 之 任 何 有 關 內 容 、 貨 品 或 服 務 所 導 致 或 據 此 導 致 之 任 何 賠 償 或 損 失 負 責 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                15. 保 證 之 免 責 聲 明
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                閣 下 明 確 明 白 及 同 意 ：
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                A. 閣 下 使 用 服 務 ， 風 險 自 負 。 在 法 律 盡 可 能 容 許 之 情 況 下 ， 和 記 明 確 表 明 ， 概 不 就 任 何 形 式
                                之 所 有 保 證 （ 不 論 明 示 或 暗 示 ） ， 包 括 但 不 限 於 涉 及 可 銷 售 性 ， 是 否 適 合 作 某 個 用 途 及 並 無
                                侵 權 所 作 之 隱 含 保 證 負 責 。
                                <br />
                                B. 和 記 概 不 保 證 (I) 服 務 將 符 合 閣 下 之 要 求 ； (II) 服 務 將 會 不 受 干 擾 、 及 時 、 穩 妥 或 並 無
                                錯 誤 ； (III) 使 用 服 務 而 可 能 取 得 之 結 果 將 為 準 確 或 可 靠 ； (IV) 閣 下 透 過 服 務 所取 得 之 任 何
                                內 容 或 貨 品 之 質 素 將 符 合 閣 下 之 期 望 ， 或 (V) 任 何 軟 件 中 之 任 何 差 誤 將 獲 修 正 。
                                <br />
                                C. 所 下 載 或 以 其 他 形 式 透 過 使 用 服 務 而 取 得 之 任 何 內 容 ， 將 按 閣 下 之 酌 情 而 取 得 ， 風 險 由 閣
                                下 自 負 ， 而 閣 下 須 獨 自 就 下 載 任 何 有 關 內 容 所 導 致 閣 下 之 電 腦 系 統 出 現 之 任 何 損 毀 或 數 據 遺
                                失 負 責 。
                                <br />
                                D. 閣 下 從 和 記 或 透 過 或 從 服 務 所 取 得 之 建 議 或 資 料 （ 不 論 口 頭 或 書 面 ） ， 概 不 會 產 生 任 何保
                                證 。
                                <br />
                                E. 和 記 概 不 保 證 服 務 之 及 時 性 ， 而 閣 下 亦 須 知 悉 ， 若 干 服 務 乃 按 延 遲 之 基 準 提 供 。
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                16. 責 任 之 限 制
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                閣 下 明 確 明 白 及 同 意 ， 在 法 律 盡 可 能 容 許 之 情 況 下 ， 和 記 不 會 就 (I) 使 用 或 未 能 使 用 服 務 ；
                                (II) 因 透 過 或 從 服 務 所 購 入 或 取 得 之 任 何 內 容 、 其 他 資 訊 或 服 務 或 所 收 取 之 訊 息 或 訂 立 之 交
                                易 而 產 生 的 採 購 替 代 貨 品 或 服 務 之 成 本 ； (III) 未 經 許 可 接 連 或 更 改 閣 下 之 傳 送 之 信 息 或 可
                                辨 別 身 份 之 個 人 資 料 ； (IV) 有 關 任 何 服 務 之 第 三 者 的 聲 明 或 行 為 ； 或 (V) 任 何 其 他 有 關 服 務
                                之 事 宜 ， 所 導 致 之 任 何 直 接 、 間 接 、 相 應 、 特 別 或 隨 之 而 產 生 的 或 懲 罰 性 賠 償 ， 包 括 但 不 限
                                於 有 關 溢 利 、 商 譽 、 收 入 、 用 途 、 數 據 或 其 他 無 形 之 損 失 而 負 責 （ 不 論 屬 侵 權 法 或 合 約 法 或
                                其 他 方 面 的 ） 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                17. 知 識 產 權
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                手 機 應 用 程 式 連 同 作 為 手 機 應 用 程 式 之 一 部 份 所 提 供 之 所 有 內 容 屬 吾 等 之 財 產 或 吾 等 已 獲 許
                                可 使 用 的 ， 且 受 版 權 、 商 標 、 服 務 標 誌 、 專 利 權 或 其 他 專 利 權 及 法 例 保 障 ， 吾 等 並 保 留 所 有
                                權 利 。 吾 等 之 許 可 者 及 吾 等 均 擁 有 就 選 取 、 協 調 、 安 排 及 改 善 有 關 內 容 以 及 原 本 內 容 之 版 權
                                。 和 記 尊 重 版 權 法 ， 並 期 望 其 用 戶 亦 同 樣 尊 重 版 權 法 。 未 經 許 可 而 複 製 、 分 發 、 修 訂 、 利 用
                                及 在 公 眾 場 合 展 示 及 買 賣 版 權 作 品 乃 侵 犯 版 權 持 有 人 權 利 。 和 記 及 其 他 人 士 擁 有 在 手 機 應 用
                                程 式 所 展 示 之 商 標 、 標 誌 和 服 務 商 標 以 及 有 關 服 務 所 使 用 之 所 須 軟 件 ， 而 用 戶 在 未 得 到 和 記
                                或 有 關 之 其 他 各 方 之 書 面 批 准 前 ， 不 得 使 用 、 修 訂 、 租 用 、 出 售 、 租 用 、 租 賃 、 分 發 或 創 造
                                全 部 或 部 份 基 於 或 在 任 何 情 況 下 揉 合 上 述 者 之 衍 生 作 品 或 以 任 何 方 法 篡 改 以 上 種 種 。 作 為 閣
                                下 使 用 服 務 之 條 件 ， 閣 下 同 意 閣 下 將 不 會 使 用 手 機 應 用 程 式 以 侵 犯 其 他 之 知 識 產 權 。 和 記 保
                                留 權 利 ， 於 知 悉 任 何 用 戶 侵 犯 其 他 人 士 之 版 權 或 其 他 知 識 產 權 後 ， 終 止 該 用 戶 之 登 記 及 終 止
                                其 使 用 手 機 應 用 程 式 。 和 記 將 對 其 收 到 之 有 關 手 機 應 用 程 式 用 戶 涉 及 侵 犯 版 權 及 涉 及 有 關 手
                                機 應 用 程 式 之 使 用 通 知 作 出 調 查 。
                                <br />
                                <br />
                                提 供 在 手 機 應 用 程 式 上 所 載 之 任 何 資 訊 、 數 據 及 資 料 ， 在 所 有 情 況 下 ， 均 不 得 被 視 作 或 構 成
                                和 記 或 本 手 機 應 用 程 式 之 任 何 許 可 者 將 版 權 、 商 標 或 其 他 知 識 產 權 以 及 其 中 所 載 之 資 訊 、 數
                                據 及 資 料 轉 讓 予 手 機 應 用 程 式 之 任 何 用 戶 或 訪 客 或 任 何 其 他 第 三 者 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                18. 不 可 抗 力 事 件
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                倘 因 超 出 和 記 所 能 控 制 之 情 況 使 其 無 法 履 行 有 關 責 任 ， 導 致 不 能 及 時 遵 守 或 履 行 章 則 之 責 任
                                ， 則 和 記 無 須 履 行 其 根 據 章 則 之 任 何 責 任 。 有 關 情 況 包 括 但 不 限 於 ：
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                (a) 天 災 、 閃 電 、 地 震 、 水 災 、 風 暴 、 爆 炸 、 火 災 以 及 任 何 自 然 災 害 ；
                                <br />
                                (b) 戰 爭 、 公 敵 之 行 為 、 恐 怖 主 義 、 暴 動 、 民 變 、 惡 意 破 壞 、 蓄 意 破 壞 及 革 命 ； 及
                                <br />
                                (c) 罷 工 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                19. 並 非 代 理
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                閣 下 及 和 記 乃 互 相 獨 立 ， 閣 下 使 用 手 機 應 用 程 式 或 服 務 並 不 擬 或 並 未 產 生 代 理 、 夥 伴 、 合 營
                                、 受 託 人 、 受 益 人 、 僱 員 － 僱 主 或 發 出 專 營 權 人 士 － 申 領 專 營 權 人 士 之 關 係 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                20. 豁 免
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                除 由 和 記 及 閣 下 以 書 面 簽 訂 通 知 外 ， 章 則 之 權 利 概 不 得 視 作 被 豁 免 。 和 記 所 作 出 之 豁 免 ， 將
                                無 損 其 就 閣 下 隨 後 違 反 章 則 之 權 利 。
                                <br />
                                <br />
                                在 不 抵 觸 本 第 18條 上 述 條 文 下 ， 和 記 未 能 執 行 章 則 任 何 條 款 或 和 記 向 閣 下 授 出 之 任 何 延 期 償
                                付 款 項 、 延 誤 或 寬 免 ， 將 不 得 詮 釋 為 和 記 豁 免 其 根 據 章 則 下 可 享 有 之 權 利 或 補 救 方 法 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                21. 通 告
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                根 據 章 則 向 和 記 發 出 之 通 告 ， 可 親 手 、 以 郵 寄 、 傳 真 或 電 郵 方 式 交 付 至「 聯 繫 我 們」內 顯 示 的 地
                                址 或 傳 真 號 碼 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                22. 監 管 法 律
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                章 則 須 按 中 華 人 民 共 和 國 香 港 特 別 行 政 區 （ 「 香 港 」 ） 之 法 律 詮 釋 ， 而 本 協 議 乃 受 其 規 管。 閣
                                下 同 意 ， 閣 下 可 對 和 記 作 出 之 任 何 索 償 ， 須 提 交 香 港 法 院 處 理 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                23. 各 別 性
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                倘 基 於 任 何 理 由 導 致 一 條 或 以 上 之 章 則 或 其 在 任 何 情 況 下 之 應 用 ， 在 任 何 方 面 被 判 定 為 無 效
                                、 不 合 法 或 不 能 強 制 執 行 ， 則 該 條 款 及 條 件 在 任 何 其 他 方 面 以 及 餘 下 之 章 則 之 有 效 性 、 合 法
                                性 及 可 強 制 執 行 性 ， 在 任 何 方 式 下 將 不 會 有 損 。
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-title">
                                24. 一 般 條 款
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-content">
                                除 非 文 義 另 有 所 指 ， 章 則 將 按 下 列 規 則 詮 釋 ：
                            </td>
                        </tr>
                        <tr>
                            <td class="disclaimer-td-sub">
                                (a) 提 述 其 中 單 一 性 別 之 詞 彙 將 包 括 其 他 性 別 ；
                                <br />
                                (b) 提 述 單 數 之 詞 彙 將 包 括 複 數 ， 反 之 亦 然 ；
                                <br />
                                (c) 提 述 條 文 乃 指 章 則 之 條 文 ；
                                <br />
                                (d) 章 則 主 要 內 容 中 所 界 定 之 詞 彙 ， 於 章 則 全 文 中 將 具 所 界 定 之 涵 義 ；
                                <br />
                                (e) 標 題 僅 供 便 覽 ， 將 不 影 響 章 則 之 詮 釋 ； 及
                                <br />
                                (f) 提 述 一 名 人 士 將 包 括 該 名 人 士 之 繼 承 人 、 代 表 及 經 許 可 受 讓 人 。
                                <br />
                                (g) 中 文 版 本 及 英 文 版 本 倘 有 歧 異 ， 概 以 英 文 版 本 為 準 。
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td style="padding: 30px 0px 10px 0px">
                                <table style="width: 100%;">
                                    <tr>
                                        <td>
                                            <button type="button" class="am-btn am-btn-secondary am-round button-all" style="float: right;"
                                                onclick="AgreeDisclaimer()">
                                                <%= Resources.Lang.Res_Global_Agree%></button>
                                        </td>
                                        <td style="width: 130px;">
                                            <button type="button" class="am-btn am-btn-default am-round button-all" style="float: right;"
                                                onclick="DisAgreeDisclaimer()">
                                                <%= Resources.Lang.Res_Global_Cancel%></button>
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
</body>
</html>
