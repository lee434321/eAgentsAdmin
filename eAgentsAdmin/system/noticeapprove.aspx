<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true"
    CodeBehind="noticeapprove.aspx.cs" Inherits="PropertyOneAppWeb.system.noticeapprove" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .boot-c-property
        {
            width: 250px;
        }
        .boot-c-date
        {
            width: 120px;
        }
        .boot-c-edit
        {
            width: 120px;
        }
    </style>
    <script type="text/javascript">
        var lang = '<% =Session["lang"] %>';
        $(document).ready(function () {
            changeNav("../Image/icon_Notices.png",'<%= Resources.Lang.Res_NoticeApprove %>');
            search();
        });

        var firstPageload = true; /* 是否第一次加载页面 */
        function search() {
            if (firstPageload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-data").bootgrid({
                    navigation: 3,
                    rowCount: 20,
                    columnSelection: false,
                    selection: true,
                    multiSelect: true,
                    rowSelect: true,
                    ajax: true,
                    url: "./noticeapprove.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    searchSettings: {
                        delay: 100,
                        characters: 3
                    },
                    post: function () {
                        return {
                            action: "search"
                        };
                    },
                    formatters: {
                        "IMGURLLARGE": function (column, row) {
                            var imgurl = row.IMGURLLARGE;
                            var website = '<% =Session["website"]%>';
                            return '<img alt="" src="..' + imgurl + '" width="100px" height="50px"/>';
                        },
                        "preview": function (column, row) {
                            var imgurl = row.IMGURLLARGE;
                            var website = '<% =Session["website"]%>';
                            var title = q1safe(row.TITLE);
                            var startdate = row.STARTDATE;
                            var detail = q1safe(row.DETAIL);
                            var attachurl = row.ATTACH_URL != null ? ".." + row.ATTACH_URL : null; // 附件地址
                            var c = ' <a class="am-badge am-badge-secondary am-radius" href="Javascript:void(0);" onclick="Preview(\'' + title + '\',\'' + startdate + '\', \'' + detail + '\',\'' + (typeof (imgurl) == "string" ? imgurl.replace('\'', '\\\'') : "") + '\',\'' + (typeof (attachurl) == "string" ? attachurl.replace('\'', '\\\'') : "") + '\')">Preview</a>';
                            return c;
                        },
                        "fmtDateStart": function (column, row) {
                            return df(row.STARTDATE, "yyyy-mm-dd");    
                        },
                        "fmtDateEnd": function (column, row) {
                            return df(row.ENDDATE, "yyyy-mm-dd");
                        }
                    }
                });
                firstPageload = false;
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");
            };
        };

        /* 显示预览窗口 */
        function Preview(title, date, detail, imgurl, attach_url) {
            $p = $("#doc-modal-preview");
            $p.modal({ closeViaDimmer: false });
            $p.find(".media-heading").text(title);
            $p.find(".media-body span").text(df(date, "yyyy-mm-dd"));
            
            $p.find(".media-body p").text(detail);
            $p.find("img").attr('src', '..' + imgurl);

            if (attach_url != null && attach_url != "null") {
                $p.find("#attach").attr("href", attach_url);
                $p.find("#attach").css("display", "block");
            } else {
                $p.find("#attach").css("display", "none");
            }            
        }

        /* 审批 */
        function Approve() {
            try {

                var approveList = $("#grid-data").bootgrid("getSelectedRows");
                var sendStr = "sendStr" + "|" + approveList;

                if (checknotnull(approveList)) {
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: "./noticeapprove.aspx",
                        data: {
                            "action": "approve",
                            "sendStr": sendStr
                        },
                        success: function (data) {
                            if (checknotnull(data) && data == "ok") {
                                showAlert("Notice", '<%= Resources.Lang.Res_Alert_Approve_0%>');
                                search();
                            }
                            else {
                                showAlert("Notice", '<%= Resources.Lang.Res_Alert_Approve_2%>');
                            }
                        },
                        error: function (data) {
                            showAlert("Notice", '<%= Resources.Lang.Res_Alert_Approve_2%>');
                        }
                    });
                }
                else {
                    showAlert("Notice", '<%= Resources.Lang.Res_Alert_Approve_1%>'); //
                }
            } catch (ex) {
                showAlert("Notice", "Approve error");
            }
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="am-g am-g-collapse">
        <%-- 外边框 --%>
        <div class="am-u-sm-12">
            <%-- 深色标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue"><p><%= Resources.Lang.Res_NoticeApprove %></p></div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">
                    <%--bootgrid--%>
                    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid"
                        data-toggle="bootgrid">
                        <thead>
                            <tr>
                                <th data-column-id="NOTICEID" data-identifier="true" data-type="numeric" data-visible="false">
                                    Notice Id
                                </th>
                                <th data-column-id="PROPERTY_NAME" data-header-css-class="boot-c-property" data-align="center"
                                    data-header-align="center">
                                    <%= Resources.Lang.Res_Property%>
                                </th>
                                <th data-column-id="TITLE" data-header-css-class="boot-header-1" data-align="center"
                                    data-header-align="center">
                                     <%= Resources.Lang.Res_Subject%>
                                </th>
                                <th data-column-id="STARTDATE" data-header-css-class="boot-c-data" data-align="center" data-formatter="fmtDateStart"
                                    data-header-align="center">
                                    <%= Resources.Lang.Res_Start_Date%> 
                                </th>
                                <th data-column-id="ENDDATE" data-header-css-class="boot-c-data" data-align="center" data-formatter="fmtDateEnd"
                                    data-header-align="center">
                                    <%= Resources.Lang.Res_End_Date%>  
                                </th>
                                <th data-column-id="DETAIL" data-header-css-class="boot-header-1" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Detail%>
                                </th>
                                <th data-column-id="IMGURLLARGE" data-formatter="IMGURLLARGE" data-header-css-class="boot-header-1"
                                    data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Image%> 
                                </th>
                                <th data-column-id="preview" data-formatter="preview" data-header-css-class="boot-c-edit"
                                    data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Preview%>  
                                </th> 
                            </tr>
                        </thead>
                    </table>
                </div>
                <div class="am-u-sm-12" style="height: 30px;">
                </div>
                <div class="am-u-sm-12">
                    <button type="button" class="am-btn am-btn-secondary am-round button-all" onclick="Approve()"
                        style="float: left">
                        <%= Resources.Lang.Res_Approve%> </button>
                        
                </div>
            </div>
        </div>
    </div>
    <%-- 预览模态窗口 --%>
    <div class="am-modal am-model-no-btn" tabindex="-1" id="doc-modal-preview">
        <div class="am-modal-dialog am-round">
            <div class="am-modal-hd am-modal-hd-my">
                Preview <a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>&times;</a>
            </div>
            <div class="am-modal-bd">
                <section class="am-margin-top-sm">                    
                    <div class="media">
                        <div class="media-left">
                            <img class="media-object" src="" alt="..." style="width:150px;height:150px">
                        </div>
                        <div class="media-body">
                            <h4 class="media-heading text-left"></h4>
                            <div  class="text-left"><span></span></div>
                            <p class="text-left am-margin-top-0"></p>
                        </div>
                        <div class="media-bottom">
                            <a id="attach" href="#" target="_blank">Attachment</a>
                        </div>
                    </div>         
                </section>                
            </div>            
        </div>
    </div>
</asp:Content>
