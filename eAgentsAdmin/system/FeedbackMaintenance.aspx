<%@ Page Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true"
    CodeBehind="FeedbackMaintenance.aspx.cs" Inherits="PropertyOneAppWeb.system.FeedbackMaintenance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/vue.js" type="text/javascript"></script>    
    <script src="../js/vue-resource.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        var typejson = '<% =Session["feedbacktype"]%>';
        var admingrpid = '<% =Session["admingrpid"]%>';

        var lang = '<% =Session["lang"] %>';
        var vue = null;
        $(document).ready(function () {
            if (!admingrpid) {
                $("#interval").css("display", "none");
                $("#btnSendMail").css("display", "none");
            } else {
                $("#interval").css("display", "block");
                $("#btnSendMail").css("display", "block");
            }
            $("#btnSendMail").on('click', function () {
                var $btn = $(this);
                $btn.attr("disabled", "disabled");
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./FeedbackMaintenance.aspx",
                    data: {
                        "action": "sendrpt"
                    },
                    dataType: "json",
                    success: function (data) {
                        if (data.err_code != 0) {
                            alert(data.err_msg);
                        } else {
                            alert("Send email successfully.");
                        }
                        $btn.removeAttr("disabled");
                    }, error: function (err) {
                        $btn.removeAttr("disabled");
                    }
                });
            });

            /// -- start -- new chat biz added by lizw 2020-08-05   
            vue = new Vue({
                el: "#vue-dt",
                data: {
                    feedback: {},
                    feedbackres: [],
                    userid: '<%=Session["UserId"] %>',
                    grpid: '<%=Session["groupid"] %>',
                    loginname: '<%=Session["loginname"] %>', //当前登陆的用户名
                    userinfo: {},
                    grpinfo: {},
                    tickedres: {},      //a reply ticked.
                    res4approve: [],    //entries for pendding approve
                    flipWidth: 0,       //flip comment box width
                    flipHeight: 0,      //flip comment box height
                    status: ""          //r:revise an entry ，此时save按钮有效，编辑框里有待修改的资料，如果编辑框里内容被清除，则不会触发保存事件。
                },
                mounted: function () { //hook函数
                    console.log("vue instance mounted");
                },
                filters: {
                    fmtDate: function (v) {
                        if (lang == "en-US") {
                            return df(v, "yyyy-mm-dd hh:mi");
                        } else {
                            return df(v, "yyyy-mm-dd hh:mi");
                        }
                    },
                    escapejs: function (value) {
                        if (!value) return "";
                        return value.replace(new RegExp("<br/>", 'g'), "\n"); //var n=str.replace(/blue/g,"red");
                    },
                    rightside: function (val) {
                        return '<div>' + val + '</div>';
                    }
                },
                updated: function () { //hook函数，修正界面显示
                    this.flipWidth = parseInt($(".reply-list .am-comment-flip .am-comment-bd div").css("width"));
                },
                methods: {
                    init: function (id) {
                        //确认对话框样式调整
                        $("#modal-confirm .am-modal-dialog").css("background-color", "#F0F0F0");
                        /// -- start -- 虚拟创建一条comment获取高度，修正界面显示                      
                        var d = $('<div style="border:1px solid red;width:85%; margin:0 0 0 auto; display: inline-block;"></div>').text("123");
                        $("body").append(d);
                        var h = parseInt($(d).css("height")); // 单行高度
                        this.flipHeight = parseInt(h); //27px                        
                        $(d).remove();
                        /// --  end  --

                        this.res4approve.length = 0;  //清空待审批的条目
                        $("#vue-input-text").val(""); //清空一次输入框                       
                        var _this = this;
                        //发送get请求,获取用户角色                        
                        this.$http.get("./FeedbackMaintenance.aspx?grpid=" + this.grpid).then(function (res) {
                            if (typeof (res.body.err_code) === "undefined") {
                                alert("Session time out, please reload this page");
                                return;
                            }
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.grpinfo = res.body.result;
                                _this.load(id);
                            }
                        }, function (res) {
                            console.log('请求失败');
                        });
                    },
                    load: function (fid) {
                        var _this = this;
                        //发送get请求
                        var url = "./FeedbackMaintenance.aspx?fid=" + fid + "&ts=" + Math.random();
                        this.$http.get(url).then(function (res) {
                            _this.feedback = res.body.result.feedback;
                            _this.feedbackres = res.body.result.feedbackres;

                            //DOM更新后回调函数处理回复列表滚动到最新一条。
                            _this.$nextTick(function () {
                                var t = 0;
                                $("#vue-dt .am-scrollable-vertical .am-margin-bottom-sm").each(function (x) {
                                    t += parseInt($(this).css("height"));
                                });
                                $("#vue-dt .am-scrollable-vertical").scrollTop(t);
                            });
                        }, function (res) {
                            console.log('请求失败');
                        });
                    },
                    req2close: function (fbid) { // 提出“关闭请求”
                        var paras = {
                            action: "vue-req2close",
                            fbid: fbid
                        }

                        /// -- start -- 弹出确认框                       
                        var _this = this;
                        $("#modal-confirm").modal({
                            relatedTarget: this,
                            onConfirm: function (options) {
                                _this.$http.post("./FeedbackMaintenance.aspx", paras, { emulateJSON: true }).then(function (rsp) {
                                    if (rsp.body.err_code != 0) {
                                        alert(rsp.body.err_msg);
                                    } else {
                                        _this.load(_this.feedback.FeedbackId);
                                        search();
                                    }
                                });
                            },
                            onCancel: function () {
                            }
                        });
                        /// --  end  --                       
                    },
                    aprv2close: function (fbid) { // 审批“关闭请求”
                        var paras = {
                            action: "vue-aprv2close",
                            fbid: fbid
                        }
                        /// -- start -- 弹出确认框     
                        var _this = this;
                        $("#modal-confirm").modal({
                            relatedTarget: this,
                            onConfirm: function (options) {
                                _this.$http.post("./FeedbackMaintenance.aspx", paras, { emulateJSON: true }).then(function (rsp) {
                                    if (rsp.body.err_code != 0) {
                                        alert(rsp.body.err_msg);
                                    } else {
                                        _this.load(_this.feedback.FeedbackId);
                                        search();
                                    }
                                });
                            },
                            onCancel: function () {
                            }
                        });
                        /// --  end  --
                    },
                    approve: function () { // 批量审批“回复”
                        if (this.res4approve.length <= 0) {
                            if (this.feedbackres.length > 0) {
                                alert("No replies selected, can not approve.");
                            }
                            return;
                        }
                        var _this = this;
                        var paras = {
                            action: "vue-approve",
                            ids: this.res4approve.toString() // 
                        }
                        this.$http.post("./FeedbackMaintenance.aspx", paras, { emulateJSON: true }).then(function (rsp) {
                            if (rsp.body.err_code != 0) {
                                alert(rsp.body.err_msg);
                            } else {
                                _this.load(_this.feedback.FeedbackId);
                                search();
                            }
                            $("#vue-input-text").val("");
                            _this.res4approve.length = 0;
                            this.status = "";
                        });
                    },
                    tick: function (item) { //选择1个条目的事件处理
                        if (this.grpinfo.Approver == "1") { //如果是审批者，可以多选
                            if (item.Approve == "U") {
                                var arrs = [];
                                var exist = false;
                                var idx = 0;
                                if (this.res4approve.length > 0) { //如果已经有选择存在，则需要更新
                                    for (var i = 0; i < this.res4approve.length; i++) {
                                        if (this.res4approve[i] == item.Id) {
                                            exist = true;
                                            idx = i;
                                            break;
                                        } else {
                                            arrs.push(this.res4approve[i]);
                                        }
                                    }
                                    if (!exist) {
                                        this.res4approve.push(item.Id);
                                    } else {
                                        for (var i = idx + 1; i < this.res4approve.length; i++) {
                                            arrs.push(this.res4approve[i]);
                                        }
                                        this.res4approve = arrs;
                                    }
                                } else {
                                    this.res4approve.push(item.Id);
                                }
                            }
                        } else {
                            if (item.Approve == "U" && item.CreateBy == this.loginname && this.feedback.STATUS != 400) { //如果条目未审批并且当前用户与所选条目的创建用户一致
                                this.tickedres = item;
                                this.status = "r";
                                $("#vue-input-text").val(item.Detail);
                            }
                        }
                    },
                    isTicked: function (rid) { // reply id
                        for (var i = 0; i < this.res4approve.length; i++) {
                            if (rid == this.res4approve[i]) {
                                return "Y";
                            }
                        }
                        return "N";
                    },
                    del: function () { // 删除回复
                        var _this = this;
                        var paras = {
                            action: "vue-del",
                            rid: this.tickedres.Id // replyid                            
                        }
                        this.$http.post("./FeedbackMaintenance.aspx", paras, { emulateJSON: true }).then(function (rsp) {
                            if (rsp.body.err_code != 0) {
                                alert(rsp.body.err_msg);
                            } else {
                                _this.load(_this.feedback.FeedbackId);
                            }
                            $("#vue-input-text").val("");
                            this.status = "";
                        });
                    },
                    save: function () { // 保存回复
                        if ($("#vue-input-text").val() == "") {
                            this.status = "";
                            return;
                        }
                        var _this = this;
                        var paras = {
                            action: "vue-save",
                            rid: this.tickedres.Id, // replyid
                            content: $("#vue-input-text").val()
                        }
                        this.$http.post("./FeedbackMaintenance.aspx", paras, { emulateJSON: true }).then(function (rsp) {
                            if (rsp.body.err_code != 0) {
                                alert(rsp.body.err_msg);
                            } else {
                                _this.load(_this.feedback.FeedbackId);
                            }
                            $("#vue-input-text").val("");
                            this.status = "";
                        });
                    },
                    reply: function () { // 新建回复
                        if ($("#vue-input-text").val() == "") {
                            return;
                        }
                        var _this = this;
                        var paras = {
                            action: "vue-reply",
                            fid: this.feedback.FeedbackId,
                            userid: this.userid,
                            leasenum: this.feedback.LEASENUMBER,
                            content: $("#vue-input-text").val()
                        }
                        this.$http.post("./FeedbackMaintenance.aspx", paras, { emulateJSON: true }).then(function (rsp) {
                            if (rsp.body.err_code != 0) {
                                alert(rsp.body.err_msg);
                            } else {
                                _this.load(_this.feedback.FeedbackId);
                                $("#vue-input-text").val("");
                            }
                        });
                    },
                    applySpecialCls: function (cmt) { // 判断是否为后台人员的回复
                        var d = $('<div style="border:1px solid red;width:' + this.flipWidth + 'px' + '; margin:0 0 0 auto; display: inline-block;"></div>').text(cmt);
                        $("body").append(d);
                        var h = parseInt($(d).css("height")); // 行高 //27px                          
                        //console.log(h);

                        $(d).remove();
                        if (cmt.search(/\n/g) > 0 || h > this.flipHeight) { //如果有换行符号，则返回text-left
                            return { "am-text-left": true }
                        } else {
                            return { "am-text-right": true }
                        }
                    }
                }
            });
            /// --  end  --

            changeNav("../Image/icon_feedback.png", "<%= Resources.Lang.Res_Menu_FeedbackMaintenance %>");

            $('#datapicker-from').datepicker({ format: 'mm-yyyy', locale: 'en_US', viewMode: 'months', minViewMode: 'months' }).on('changeDate.datepicker.amui', function (event) {
            });

            /* 获取feedback类型 */
            getfeedbacktype("select-type", typejson);

            /* 生成bootgrid */
            search();
        });

        /* 搜索Feedback */
        var firstPageload = true; /* 是否第一次加载页面 */
        function search() {
            if (firstPageload==true) {
                $("#grid-data").bootgrid({
                    navigation: 2,
                    rowCount: 10,
                    sorting: false,
                    ajax: true,
                    url: "./FeedbackMaintenance.aspx",
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        var startdate = $("#datapicker-from").val();
                        var type = $("#select-type").val();
                        var reply = $("#select-reply").val();
                        return {
                            action: "search",
                            startdate: startdate,
                            type: type,
                            reply: reply
                        };
                    },
                    formatters: {
                        "chat": function (column, row) {
                            //return '<a href="JavaScript:void(0);" onclick="chatload('+row.feedbackid+"\')"'+'</a>'
                            return "<a href='javaScript:void(0);' onclick=chatload(" + row.feedbackid + ")>" + row.feedbackid + "</a>";
                        },
                        "approve": function (column, row) { // (废弃)
                            if (row.approve == "A") {
                                return "Approved";
                            } else if (row.approve == "W") {
                                return "Wait for Reply";
                            } else if (row.approve == "U") {
                                return '<a href="JavaScript:void(0);" onclick="showApproveDlg(' + row.feedbackid + ",\'" + row.id + '\')">' + 'Need Approve' + '</a>';
                            } else {
                                return "Unknown";
                            }
                        },
                        "title": function (column, row) {
                            feedbackid = row.feedbackid;
                            return "<a href='javaScript:void(0);' onclick=chatload(" + row.feedbackid + ")>" + row.title + "</a>";
                        },
                        "status": function (column, row) {
                            var reply = row.status;
                            if (reply == "200") { /* 如果已回复 */
                                return '<%= Resources.Lang.Res_Feedback_Status_Replied %>';
                            }
                            else {  /* 如果未回复 */
                                return '<%= Resources.Lang.Res_Feedback_Status_NotReply %>';
                            }
                        },
                        "date": function (column, row) {
                            return df(row.date, "yyyy-mm-dd");             
                        },
                        "type": function (column, row) {
                            return getfeedbacktypebyid(row.type, typejson);
                        }
                    }
                });
                firstPageload = false;
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");
            };
           
            /// --  end --
        };        

        /// -- start -- 当前feedback对话窗口
        function chatload(id) {
            vue.init(id);
            $("#vue-dt").modal({ closeViaDimmer: false, width: 800 });        
        }
        /// --  end  --        
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <!-- 表格 -->
    <div class="am-g am-g-collapse">
        <%-- 外边框 --%>
        <div class="am-u-sm-12">
            <%-- 深色标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p>
                    <%= Resources.Lang.Res_Menu_FeedbackMaintenance%>
                </p>
            </div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">
                    <%-- 搜索区域 --%>
                    <div class="am-u-sm-12">
                        <table style="width: 100%">
                            <tr>
                                <td style="width: 6%; text-align: right;">
                                    <label>
                                        <%= Resources.Lang.Res_Feedback_Search_Type%></label>
                                </td>
                                <td style="width: 24%; padding: 0px 20px 0px 10px;">
                                    <select id="select-type" data-am-selected="{btnWidth: '100%', btnSize: 'lg'}">
                                        <option value="0" selected><%= Resources.Lang.Res_Feedback_Type_All%></option>
                                       <%-- <option value="100">园艺</option>
                                        <option value="200">保安</option>
                                        <option value="300">清洁</option>
                                        <option value="400">员工</option>--%>
                                        <option value="500">Lease related</option>
                                        <option value="600">Estate Management related</option>
                                        <option value="700">Billing related</option>
                                        <option value="900">Others</option>
                                    </select>
                                </td>
                                <td style="width: 6%; text-align: right;">
                                    <label>
                                        <%= Resources.Lang.Res_Feedback_Search_From%>
                                    </label>
                                </td>
                                <td style="width: 24%; padding: 0px 20px 0px 10px;">
                                    <input id="datapicker-from" type="text" class="am-form-field" placeholder="MM-YYYY"
                                        readonly />
                                </td>
                                <td style="width: 6%; text-align: right;">
                                    <label>
                                        <%= Resources.Lang.Res_Feedback_Search_Status%>
                                    </label>
                                </td>
                                <td style="width: 24%; padding: 0px 20px 0px 10px;">
                                    <select id="select-reply" data-am-selected="{btnWidth: '100%', btnSize: 'lg'}">
                                        <option value="all" selected>
                                            <%= Resources.Lang.Res_Feedback_Status_All%></option>
                                        <option value="notreplay">
                                            <%= Resources.Lang.Res_Feedback_Status_NotReply%></option>
                                        <option value="replied">
                                            <%= Resources.Lang.Res_Feedback_Status_Replied%></option>
                                        <option value="closed">
                                            <%= Resources.Lang.Res_Feedback_Status_Closed%></option>
                                        <option value="pending2close">
                                            <%= Resources.Lang.Res_Feedback_Status_Pending2Close%></option>
                                    </select>
                                </td>
                                <td style="width: 10%;">
                                    <%--搜索按钮--%>
                                    <button id="button-search" type="button" class="am-btn am-btn-secondary am-round" style="float: right;" onclick="search()"> <%= Resources.Lang.Res_Site_Search%></button>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="am-u-sm-12" style="height: 20px">
                    </div>
                    <%--bootgrid--%>
                    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid"
                        data-toggle="bootgrid">
                        <thead>
                            <tr>
                                <th data-column-id="feedbackid" data-identifier="true" data-header-css-class="boot-header-2" data-formatter="chat">
                                    <%= Resources.Lang.Res_Feedback_Id%>
                                </th>
                                <th data-column-id="type" data-formatter="type" data-header-css-class="boot-header-2">
                                    <%= Resources.Lang.Res_Feedback_Type%>
                                </th>
                                <th data-column-id="date" data-formatter="date" data-header-css-class="boot-header-2" >
                                    <%= Resources.Lang.Res_Feedback_Date%>
                                </th>
                                <th data-column-id="title" data-formatter="title" data-header-css-class="boot-header-1">
                                    <%= Resources.Lang.Res_Feedback_Title%>
                                </th>
                                <th data-column-id="detail" data-visible="false">
                                    <%= Resources.Lang.Res_Feedback_Detail%>
                                </th>
                                <th data-column-id="feedbackStatus" data-header-css-class="boot-header-2">
                                    <%= Resources.Lang.Res_Feedback_Status%>
                                </th>
                                <th data-column-id="reply" data-visible="false">
                                    <%= Resources.Lang.Res_Feedback_Reply%>
                                </th>
                                <th data-column-id="approveStatus" data-align="center" data-header-align="center" data-header-css-class="boot-header-2">
                                    <%= Resources.Lang.Res_Feedback_ApproveStatus%> 
                                </th>
                            </tr>
                        </thead>
                    </table>
                    <button type="button" class="btn btn-primary" id="btnSendMail">Send summary report mail</button>

                    <div class="form-inline hide" id="interval">
                      <div class="form-group">
                        <label class="sr-only">Email</label>
                        <p>Interval Days</p>
                      </div>
                      <div class="form-group">
                        <label for="inputPassword2" class="sr-only">Password</label>
                        <input type="text" class="form-control" id="inputPassword2">
                      </div>
                      <button type="submit" class="btn btn-default">Confirm</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
       
    <!-- vue模态对话框 -->
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="vue-dt">
        <div class="am-modal-dialog" >
            <div class="am-modal-hd am-modal-hd-my">
                Chat [{{feedback.Title}}] 
                <a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>&times;</a>
            </div>           
            <div class="am-modal-bd " >
                <div class="am-g am-g-collapse am-margin-top-sm">
                    <!-- 主条目 -->
                    <div class="am-u-sm-11 am-margin-bottom-sm">
                        <div class="am-comment am-text-left">
                            <img class="am-comment-avatar" src="../Image/icon_MyProfile.png" alt=""/> <!-- 评论者头像 -->
                            <div class="am-comment-main"> <!-- 评论内容容器 -->
                                <header class="am-comment-hd">
                                    <!--<h3 class="am-comment-title">评论标题</h3>-->
                                    <div class="am-comment-meta"> <!-- 评论元数据 -->
                                        <strong>{{feedback.CreateBy}}</strong> <!-- 评论者 --> - {{feedback.CreateDate |fmtDate}}
                                        <span class="am-badge am-badge-warning am-radius" v-if="feedback.STATUS==300">Pending to close</span>
                                        <span class="am-badge am-badge-success am-radius" v-if="feedback.STATUS==400">Closed</span>
                                    </div>
                                </header>
                                <div class="am-comment-bd am-text-break" style="white-space: pre-wrap;">{{feedback.Detail | escapejs}}</div> <!-- 评论内容 -->
                            </div>  
                        </div> 
                    </div>
                </div>
                <div class="am-g am-g-collapse am-scrollable-vertical reply-list" style="height:300px;">                    
                    <!-- 回复列表 -->
                    <div class="am-margin-bottom-sm" v-for="item in feedbackres" v-bind:class="item.Src=='front'?'am-u-sm-11':'am-u-sm-offset-1 am-u-sm-11'">
                        <div class="am-comment " v-bind:class="item.Src=='front'?'am-text-left':'am-text-right am-comment-flip'">
                            <img class="am-comment-avatar" src="../Image/icon_MyProfile.png" alt="" v-bind:style="item.Src!='front'?'background-color:gold':''" /> <!-- 评论者头像 -->
                            <div class="am-comment-main" v-on:click="tick(item)"> <!-- 评论内容容器 -->
                                <header class="am-comment-hd">
                                    <!--<h3 class="am-comment-title">评论标题</h3>-->
                                    <div class="am-comment-meta"> <!-- 评论元数据 -->                                        
                                      <span class="am-badge am-badge-primary am-radius" v-if="isTicked(item.Id)=='Y' && item.Src!='front'?true:false">Mark</span>&nbsp;
                                      <span class="am-badge am-badge-danger am-radius" v-if="item.Approve=='A' && item.Src!='front'?true:false">Approved</span>&nbsp;
                                      <strong>{{item.CreateBy}}</strong> <!-- 评论者 --> - {{item.CreateDate |fmtDate}}
                                    </div>
                                </header>
                                <div class="am-comment-bd" style="white-space: pre-wrap;">                                    
                                    <div v-if="item.Src!='front'" v-bind:class="applySpecialCls(item.Detail)" style="width:85%; margin:0 0 0 auto; display: inline-block;">{{item.Detail}}</div>
                                    <div v-else>{{item.Detail}}</div>                                   
                                </div> <!-- 评论内容 -->
                            </div>
                        </div>          
                    </div>  
                    <div class="am-u-sm-offset-1 am-u-sm-11"><!-- 空div 用于修正上一条div的offset位置（不要去除，原因未知）-->
                    </div>                               
                </div>
                <div class="am-g am-g-collapse">
                    <!-- 内容输入 -->
                    <div class="am-u-sm-10 am-u-sm-offset-1 am-text-left">                      
                        <div class="am-form">
                            <div class="am-form-group" v-bind:disabled="grpinfo.Approver=='1'?true:false">                               
                                <textarea class="am-form-field" rows="3" id="vue-input-text" v-if="grpinfo.Approver!='1'"></textarea>
                            </div>
                            <p>
                                <button class="am-btn am-btn-secondary" v-on:click="reply" v-if="status=='' && grpinfo.Approver!='1' && feedback.STATUS!=400">Reply</button>&nbsp;
                                <button class="am-btn am-btn-secondary" v-on:click="save" v-if="status=='r' && grpinfo.Approver!='1'">Revise</button>&nbsp;
                                <button class="am-btn am-btn-secondary" v-on:click="del"  v-if="status=='r' && grpinfo.Approver!='1'">Delete</button>&nbsp;                                
                                <button class="am-btn am-btn-success" v-on:click="approve" v-if="grpinfo.Approver=='1' && feedbackres.length>0">Approve</button>&nbsp;<span v-if="res4approve.length>0">{{res4approve.length}} marked!</span>
                                <button class="am-btn am-btn-warning" v-on:click="req2close(feedback.FeedbackId)" v-if="grpinfo.Approver!='1' && feedbackres.length>0 && feedback.STATUS<300 ">Request to close</button>
                                <button class="am-btn am-btn-warning" v-on:click="aprv2close(feedback.FeedbackId)" v-if="grpinfo.Approver=='1' && feedback.STATUS==300 ">Approve to close</button>
                            </p>
                        </div>                    
                    </div>
                </div>
            </div>
        </div>
    </div>   

    <!-- 模态确认对话框 -->
    <div class="am-modal am-modal-confirm " tabindex="-1" id="modal-confirm">
      <div class="am-modal-dialog am-table-bordered">
        <div class="am-modal-hd">Confirm</div>
        <div class="am-modal-bd">
            Are you sure ?
        </div>
        <div class="am-modal-footer">
            <span class="am-modal-btn" data-am-modal-cancel>No</span>
            <span class="am-modal-btn" data-am-modal-confirm>Yes</span>
        </div>
        </div>
    </div> 
</asp:Content>
