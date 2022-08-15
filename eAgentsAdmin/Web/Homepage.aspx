<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Homepage.aspx.cs" Inherits="PropertyOneAppWeb.Web.Homepage" %>

<%@ MasterType VirtualPath="~/Site.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="Stylesheet" href="/js/Swiper/swiper.css" />
    <script type="text/javascript" src="/js/Swiper/swiper.jquery.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {

            changeNav("/Image/icon_Notices.png", "<%= Resources.Lang.Res_Menu_Homepage %>");
            loadSwiper();
            getNotice();

            var logintype = '<% =Session["logintype"] %>';
            var welcomecustname;
            welcomecustname = '<% =Session["custname"] %>';
            $("#td-cust-name").text(welcomecustname);
            $("#span-outstanding").text("$" + formatMoney('<% =Session["homepageoutstanding"] %>', 2, ""));

            loadchart();
        });

        /* 获取notice */
        function getNotice() {
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Web/Homepage.aspx",
                data: {
                    "action": "getnotice"
                },
                success: function (data) {
                    try {
                        var jsonData = jQuery.parseJSON(data);
                        /* 设置Notice数量 */
                        var noticenum = jsonData.noticenum;
                        $("#span-notice-num").text(noticenum);
                        /* 设置Notice详细 */
                        if (jsonData.noticeinfo != null && jsonData.noticeinfo.length > 0) {
                            for (var i = 0; i < jsonData.noticeinfo.length; i++) {
                                addSwiper(jsonData.noticeinfo[i].title, jsonData.noticeinfo[i].detail, jsonData.noticeinfo[i].imgurl);
                            }
                        }
                    }
                    catch (ex) {
                        showAlert("Error", '<%= Resources.Error.ERR_NOTICE_GET %>' + ex.message + "data: " + data);
                    }
                },
                error: function (data) {
                    showAlert("Error", '<%= Resources.Error.ERR_NETWORK_ERR %>' + data);
                }
            });
        };

        /* 加载Swiper */
        var swiper;
        function loadSwiper() {
            swiper = new Swiper('#MySwiper', {
                pagination: '#MySwiper .swiper-pagination',
                slidesPerView: 3,
                paginationClickable: true,
                spaceBetween: 30,
                slidesPerGroup: 3,
                slidesPerColumn: 2,
                slidesPerColumnFill: 'row'
            });
        };

        /* 添加Swiper */
        function addSwiper(title, content, imgurl) {
            try {
                var area = '<% =Session["shoparea"]%>';
                area = limit(area, 20);
                title = limit(title, 20);
                content = limit(content, 50);
                swiper.appendSlide('<div class="swiper-slide"><div class="div-notice-detail"><div class="div-notice-detail-border"><div><img alt="" src="' + imgurl + '" width="100%" height="125px" /></div><div class="div-notice-detail-blueline"><label>' + area + '</label></div><div class="div-notice-detail-textarea"><div class="div-notice-detail-texttitle"><label>' + title + '</label></div><div class="div-notice-detail-textcontent"><label class="label-notice-detail-textcontent">' + content + '</label></div><div class="div-notice-detail-readmore"><a href="JavaScript:void(0);" class="label-notice-detail-readmore" onclick="showReadMore(\'' + content + '\')">read more</a></div></div></div></div></div>');
            }
            catch (ex) {
                showAlert("Error", "Add swiper error: " + ex.message + "area: " + area + "title: " + title + "content: " + content);
            }
        };

        /* 显示ReadMore模态窗口 */
        function showReadMore(str) {
            $("#label-notice-detail").text(str);
            $("#doc-modal-notice").modal({ closeViaDimmer: false, width: 800 });
        };

        /* 加载图表 */
        function loadchart() {
            try {
                var leasenum = '<% =Session["leasenumber"]%>';
                var leasegroupjson = '<% =Session["leasegroupjson"]%>';
                var jsondata = jQuery.parseJSON(leasegroupjson);
                if (checknotnull(jsondata) && jsondata.length > 0) {
                    for (var i = 0; i < jsondata.length; i++) {
                        var height = parseInt((jsondata[i].outstanding / globaltotaloutstanding) * 150);
                        if (height > 150) {
                            height = 150;
                        }
                        else if (height < 0) {
                            height = 1;
                        }

                        var color;
                        if (i % 3 == 0) {
                            color = "#FF2D55";
                        }
                        else if (i % 3 == 1) {
                            color = "#5AC8FA";
                        }
                        else if (i % 3 == 2) {
                            color = "#FF9500";
                        }
                        else {
                            color = "#4CD964";
                        }

                        insertchartcolumn(100, height, color, jsondata[i].outstanding, jsondata[i].leasenum);
                    }
                }
            } catch (ex) {
                showAlert("Error", '<%= Resources.Error.ERR_CHART_LOAD %>' + ex.message + "lease: " + leasenum);
            }
        };

        /* 插入一列图表 */
        function insertchartcolumn(width, height, color, outstanding, leasenum) {
            try {
                if (outstanding != 0) {
                    if (outstanding > 0) {
                        $("#tr-chart-1").append('<td style="vertical-align: bottom;"><div style="width: ' + width + 'px; height: ' + height + 'px; background-color: ' + color + ';"></div></td>');
                        $("#tr-chart-1-2").append('<td style="vertical-align: top;"><div style="width: ' + width + 'px; height: ' + height + 'px;></div></td>');
                    }
                    else if (outstanding < 0) {
                        $("#tr-chart-1").append('<td style="vertical-align: bottom;"><div style="width: ' + width + 'px; height: ' + height + 'px;"></div></td>');
                        $("#tr-chart-1-2").append('<td style="vertical-align: top;"><div style="width: ' + width + 'px; height: ' + height + 'px; background-color: ' + color + ';"></div></td>');
                    }
                    outstanding = "$" + formatMoney(outstanding, 2, "")
                    $("#tr-chart-2").append('<td class="td-homepage-summary">' + outstanding + '</td>');
                    $("#tr-chart-3").append('<td class="td-homepage-summary">' + leasenum + '</td>');
                }
            } catch (ex) {
                showAlert("Error", "Insert chart column error: " + ex.message + "lease: " + leasenum);
            }
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- Welcome --%>
    <table style="width: 100%">
        <tr>
            <td style="width: 350px; vertical-align: top;">
                <table style="width: 100%; table-layout: fixed;">
                    <tr>
                        <td style="font-size: 30px;">
                            <%= Resources.Lang.Res_Homepage_Welcome%>
                        </td>
                    </tr>
                    <tr>
                        <td id="td-cust-name" class="am-text-truncate" style="font-size: 15px; color: #1C578F;
                            font-weight: bold; word-wrap: break-word; word-break: break-all; white-space: normal;">
                        </td>
                    </tr>
                    <tr style="height: 10px">
                    </tr>
                    <tr>
                        <td style="font-size: 12px;">
                            <%= Resources.Lang.Res_Homepage_LastLogin%>
                            <% =Session["lastlogin"]%>
                        </td>
                    </tr>
                    <tr>
                        <td style="font-size: 12px;">
                            <%= Resources.Lang.Res_Homepage_YouHave%>
                            <span id="span-notice-num">0</span>
                            <%= Resources.Lang.Res_Homepage_NewMessage%>
                        </td>
                    </tr>
                </table>
            </td>
            <td style="width: 250px;">
            </td>
            <td style="vertical-align: top;">
                <table style="width: 100%">
                    <tr>
                        <td style="font-size: 30px;">
                            <%= Resources.Lang.Res_Homepage_Summary%>
                        </td>
                    </tr>
                    <tr style="height: 10px">
                    </tr>
                    <tr>
                        <td style="font-size: 20px;">
                            <%= Resources.Lang.Res_Homepage_TotalOutstanding%>
                            <a id="span-outstanding" href="javascript: void(0);" style="color: #7ED200; font-weight: bold;">
                            </a>
                        </td>
                    </tr>
                    <tr style="height: 30px;">
                    </tr>
                    <tr>
                        <td>
                            <table style="width: auto; margin: 0px auto;">
                                <tr id="tr-chart-1">
                                    <%--<td style="vertical-align: bottom;">
                                                <div style="width: 100px; height: 150px; background-color: #FF2D55;">
                                                </div>
                                            </td>--%>
                                </tr>
                                <tr id="tr-chart-1-2">
                                </tr>
                                <tr id="tr-chart-2">
                                    <%--<td class="td-homepage-summary">
                                                $1645.32
                                            </td>--%>
                                </tr>
                                <tr id="tr-chart-3">
                                    <%--<td class="td-homepage-summary">
                                                AVCDFSDFD
                                            </td>--%>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
    <div style="height: 50px;">
    </div>
    <%-- Notice --%>
    <div class="div-notice-container">
        <!-- Slider 主容器，必需 -->
        <div class="swiper-container" id="MySwiper">
            <!-- slides 容器，必需 -->
            <div class="swiper-wrapper">
                <!-- Slides -->
                <!--
                        <div class="swiper-slide">
                            <div class="div-notice-detail">
                                <div class="div-notice-detail-border">
                                    <div>
                                        <img alt="" src="/Image/home_logo.png" width="100%" height="125px" />
                                    </div>
                                    <div class="div-notice-detail-blueline">
                                        <label>
                                            abc</label>
                                    </div>
                                    <div class="div-notice-detail-textarea">
                                        <div class="div-notice-detail-texttitle">
                                            <label>
                                                title title title title</label>
                                        </div>
                                        <div class="div-notice-detail-textcontent">
                                            <label class="label-notice-detail-textcontent">
                                                content content content content contentcontentcontentcontent content content</label>
                                        </div>
                                        <div class="div-notice-detail-readmore">
                                            <a href="JavaScript:void(0);">read more</a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        -->
            </div>
            <div style="height: 50px">
            </div>
            <!-- 分页容器，可选 -->
            <div class="swiper-pagination">
            </div>
        </div>
    </div>
    <%-- Notice模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="doc-modal-notice">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                Notice <a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>
                    &times;</a>
            </div>
            <div class="am-modal-bd am-scrollable-vertical" style="height: 500px">
                <div class="am-modal-bd-my">
                    <label id="label-notice-detail">
                    </label>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
