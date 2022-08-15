<%@ Page Language="C#" MasterPageFile="~/SiteSystemBootcss.Master" AutoEventWireup="true" 
 CodeBehind="eFormDateDef.aspx.cs" Inherits="PropertyOneAppWeb.system.eFormDateDef" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">    
    <script src="../js/vue.js" type="text/javascript"></script>
    <script src="../js/vue-resource.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        var vm = null;
        $(document).ready(function () {
            changeNav("../Image/icon_MyStatment.png", "Date Definition");


            var url = "eFormDateDef.aspx?act=";
            vm = new Vue({
                el: "#vue-dt",
                data: {
                    row: { dtdf_define: "HLD" },        //当前编辑的行
                    weekdays: ['MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT', 'SUN', 'HLD'],
                    page: {
                        rows: [],   //当前页条目数组
                        size: 10,   //每页条目数
                        count: 0,   //总页数
                        idx: 1,     //当前页
                        range: 5,   //可显示的页码范围
                        start: 1    //可显示的起始页码
                    },
                    lpro: {},       //login profile 包含登陆用户的基本信息(grps，groupinfo,login,porps)。
                    status: 0        //0=new;1=edit
                },
                created: function () {
                    //修正row样式
                    $(".row").css("margin", "10px 5px 0px 5px");

                    this.$nextTick(function () {
                        //修正datepicker事件处理
                        $(".am-dp").datepicker({ format: 'yyyy-mm-dd' }).on('changeDate.datepicker.amui', function (event) {
                            event.target.value = event.target.defaultValue;
                        });
                        //检索数据 all
                        this.search();
                    });

                    var _this = this;
                    var paras = { groupid: $("#input-groupid-hidden").val(), loginname: $("#input-loginname-hidden").val() }
                    this.$http.post("ajax.ashx?act=get_login_profile", paras, { emulateJSON: true }).then(function (res) {
                        console.log(res);
                        if (res.body.err_code != 0) {
                            if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                _this.timeout();
                            } else
                                alert("[vue created] with error:" + res.body.err_msg);
                        } else {
                            _this.lpro = res.body.result;
                        }
                    }, function (err) {
                        alert(err);
                    });
                },
                filters: {
                    fmtDate: function (v) {     //“日期”过滤器
                        var lang = $("#input-lang-hidden").val();
                        if (lang == "en-US") { // lang=zh-CHS"中文 lang=en-US">English
                            return df(v, "yyyy-mm-dd");
                        } else if (lang == "zh-CHS") {
                            return df(v, "yyyy-mm-dd");
                        } else
                            if (v) return v.substr(0, 10); else return "";
                    },
                    trim: function (v) {    //去掉前后的空格
                        if (v)
                            return v.replace(/(^\s*)|(\s*$)/g, "");
                        else
                            return "";
                    }
                },
                methods: {
                    timeout: function () {
                        window.location.href = "../98.html";
                    },
                    jump: function (pidx) {
                        this.page.idx = pidx;
                        this.search();
                    },
                    setPager: function (res) {
                        this.page.rows = res.body.result.data;
                        this.page.size = res.body.result.pageSize;
                        this.page.count = res.body.result.count;
                        this.page.idx = res.body.result.pageIdx;
                        /// -- start -- 页码显示逻辑
                        var q = Math.floor(this.page.idx / this.page.range); //5, 5/5=1 => 4,5,6,7,8 ;8/5
                        var r = this.page.idx % this.page.range; //5, 5%5=0 
                        if (q >= 1) {
                            if (r == 0) {
                                this.page.start = q * this.page.range - 1;
                            } else if (r == 1) {
                                this.page.start = q * this.page.range;
                            } else
                                this.page.start = q * this.page.range + 1;
                            if ((this.page.start + this.page.range - 1) >= this.page.count) {
                                this.page.start = this.page.count - this.page.range + 1;
                            }
                        } else
                            this.page.start = 1;
                        /// --  end  --
                    },
                    search: function () { //i,s => pidx,psize
                        var req = url + "search" + "&ts=" + Math.random();
                        var paras = {
                            searchParse: $("#input-search").val(),
                            pageIdx: isNaN(parseInt(this.page.idx)) ? 1 : this.page.idx, //检查数字是否非法
                            pageSize: this.page.size
                        }
                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    _this.timeout();
                                } else
                                    alert("[search] with error:" + res.body.err_msg);
                            } else {
                                _this.setPager(res);
                            }
                        }, function (res) {
                            console.log('request failure.');
                        });
                    },
                    preparas: function () { //准备参数 prepare paras
                        var paras = {}
                        $("#editor-x .p-field").each(function (i, e) {
                            var key = $(e).attr("alt");
                            var val = $(e).val();
                            paras[key] = val;
                        });
                        paras.action = this.status;
                        return paras;
                    },
                    validate: function (paras) {
                        var err = "";
                        if (!paras.dtdf_date) {
                            err = "date is required.";
                        }
                        return err;
                    },
                    create: function () {
                        this.status = 0;
                        this.row = {};
                        this.row["dtdf_id"] = 0;
                        this.row["dtdf_define"] = "HLD";
                        this.row["dtdf_date"] = "";
                        $(".form-grid").hide(500);
                        $("#editor-x").show(500);
                    },
                    edit: function (id) {
                        this.status = 1;
                        for (var i = 0; i < this.page.rows.length; i++) {
                            if (this.page.rows[i].dtdf_id == id) {
                                this.row = this.page.rows[i];
                                break;
                            }
                        }
                        $("#editor-x").removeClass('hide');
                        $(".form-grid").hide(500);
                        $("#editor-x").show(500);
                    },
                    del: function (dtdf_id) {
                        var _this = this;
                        var req = url + "del";
                        var paras = { id: dtdf_id }
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    _this.timeout();
                                } else
                                    alert("[delete] with error:" + res.body.err_msg);
                            } else {
                                _this.cleanup();
                                _this.search();
                            }
                        }, function (err) {
                            alert(err);
                        });
                    },
                    approve: function () {
                        var _this = this;
                        var req = url + "approve";
                        var paras = { id: this.row.dtdf_id }
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    _this.timeout();
                                } else
                                    alert("[approve] with error:" + res.body.err_msg);
                            } else {
                                _this.cleanup();
                                _this.search();
                            }
                        }, function (err) {
                            alert(err);
                        });
                    },
                    save: function () {
                        var paras = this.preparas();
                        var errmsg = this.validate(paras);
                        if (errmsg.length > 0) {
                            alert(errmsg);
                        } else {
                            var req = url + "save";
                            var that = this;
                            this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                                if (res.body.err_code != 0) {
                                    if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                        that.timeout();
                                    } else
                                        alert("[save] with error:" + res.body.err_msg);
                                } else {
                                    that.cleanup();
                                    that.search();
                                }
                            }, function (res) {
                                console.log('request failure.');
                                this.cleanup();
                            });
                        }
                    },
                    cleanup: function () {
                        $("#editor-x").hide(500);
                        $(".form-grid").show(500);
                    },
                    txtChange: function (e) {
                        console.log($(e.target).val());
                        this.search(); // 后期最好做个触发控制,避免频繁查询
                    }
                }
            });
        });        
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="col-md-12 global_table_head global_color_navyblue">
        <p>Date Definition</p>
    </div>
    <div class="global_color_lightblue" id="vue-dt">
        <!-- 修正 -->
        <div class="row"></div>

        <!-- 检索 -->
        <div class="row form-grid">
            <div class="col-md-5">               
            </div>
            <div class="col-md-3 col-md-offset-4">
                <div class="input-group">
                    <span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>
                    <input type="text" id="input-search" class="form-control" v-on:keyup="txtChange($event)" />                    
                </div>                 
            </div>
        </div>

        <!-- 表格 -->
        <div class="row form-grid">     
            <div class="col-md-12">
                 <table class="table table-condensed table-hover table-striped global-bootgrid bootgrid-table">
                    <thead>
                        <tr>
                            <th class="hide">Id</th>                        
                            <th>Date</th>
                            <th>Define</th>
                            <th>Active</th>
                            <th>Status</th>                    
                            <th class="text-center" style="width:100px;">Operation</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in page.rows">
                            <td class="text-center hide">{{item.dtdf_id}}</td>                            
                            <td class="text-center"><a href="javascript:void(0)" v-on:click="edit(item.dtdf_id)">{{item.dtdf_date |fmtDate}}</a></td>                            
                            <td class="text-center">{{item.dtdf_define}}</td>
                            <td class="text-center">{{item.dtdf_active}}</td>
                            <td class="text-center">{{item.dtdf_status}}</td>                            
                            <td class="text-center" style="width:100px;"><a class="label label-danger" v-on:click="del(item.dtdf_id)">Delete</a></td>
                        </tr>
                    </tbody>                    
                 </table>
            </div>
        </div>

        <!-- 翻页 -->
        <div class="row form-grid">  
            <div class="col-md-3">
                <button class="btn btn-primary" data-toggle="modal" v-on:click="create">Create</button> 
            </div>
            <div class="col-md-3">              
            </div>
            <div class="col-md-2">
            </div>
            <div class="col-md-4 text-right" v-show="page.count>0">
                <ul class="pagination">
                    <li v-on:click="jump(page.idx-1)"><a href="javascript:void(0)">&laquo;</a></li>
                    <li v-for="i in page.count" v-bind:class="{active:i==page.idx}" v-if="i>=page.start && i<=page.start+page.range-1 && i<=page.count" v-on:click="jump(i)"><a href="javascript:void(0)">{{i}}</a></li>
                    <li v-on:click="jump(page.idx+1)"><a href="javascript:void(0)">&raquo;</a></li>
                </ul>
            </div>
        </div>              

        <!-- 编辑区域（一览时隐藏） -->
        <div class="row" id="editor-x" style="display:none;">
             <div class="col-sm-12">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Editor
                    </div>
                    <div class="panel-body">
                         <div class="form-horizontal">
                            <div class="form-group hide">
                                <label class="col-sm-3 control-label">Id:</label>
                                <div class="col-sm-3">
                                    <input type="number" class="form-control p-field" alt="dtdf_id" v-bind:value="row.dtdf_id" />
                                </div>                                    
                            </div>       
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Date:</label>
                                <div class="col-sm-3">
                                    <input type="text" class="am-dp form-control p-field" alt="dtdf_date" v-bind:value="row.dtdf_date |fmtDate" readonly />
                                </div>    
                                <label class="col-sm-2 control-label">Define:</label>
                                <div class="col-sm-3">
                                    <select class="form-control p-field" alt="dtdf_define" v-bind:value="row.dtdf_define|trim">
                                        <option value="-1" disabled>-select-</option>
                                        <option v-for="item in weekdays" :value="item">{{item}}</option>
                                    </select>
                                </div>                                
                            </div>                                                                    
                            <div class="form-group" v-if="status=='1'">
                                <label class="col-sm-2 control-label">Create By:</label> 
                                <div class="col-sm-3">
                                    <span class="form-control" v-if="row.dtdf_createby!='' &&row.dtdf_createby!=null ">{{row.dtdf_createby}}/{{row.dtdf_createdate|fmtDate}}</span>
                                </div>
                                <label class="col-sm-2 control-label">Update By:</label> 
                                <div class="col-sm-3">
                                    <span class="form-control" v-if="row.dtdf_updateby!='' &&row.dtdf_updateby!=null ">{{row.dtdf_updateby}}/{{row.dtdf_updatedate|fmtDate}}</span>
                                </div>                                    
                            </div>    
                            <div class="form-group" v-if="status=='1' && row.dtdf_approveby!='' &&row.dtdf_approveby!=null ">
                                <label class="col-sm-2 control-label">Approved By:</label> 
                                <div class="col-sm-3">
                                    <span class="form-control">{{row.dtdf_approveby}}/{{row.dtdf_approvedate|fmtDate}}</span>
                                </div>
                            </div>                           
                         </div>
                    </div>
                    <div class="panel-footer text-right">
                        <button type="button" class="btn btn-default" data-dismiss="modal" v-on:click="cleanup">Close</button>                            
                        <button type="button" class="btn btn-primary" v-on:click="save">Save</button>                                                
                        <button type="button" class="btn btn-success" v-if="status==1 && lpro.grpinfo && lpro.grpinfo.Approver=='1'" v-on:click="approve" key="btn-approve" >Approve</button>                                                                                 
                    </div>
                </div>
             </div>
        </div>
    </div>
</asp:Content>
