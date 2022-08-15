<%@ Page Language="C#" MasterPageFile="~/SiteSystemBootcss.Master" AutoEventWireup="true" 
 CodeBehind="eFlowDef.aspx.cs" Inherits="PropertyOneAppWeb.system.eFlowDef" %>
    
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/vue.js" type="text/javascript"></script>
    <script src="../js/vue-resource.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        var vm = null;
        $(document).ready(function () {
            changeNav("../Image/icon_MyStatment.png", "Flow Definition");

            var dev = '<%=System.Configuration.ConfigurationManager.AppSettings["dev"]%>';
            var url = "eFlowDef.aspx?act=";
            vm = new Vue({
                el: "#vue-dt",
                data: {
                    searching: false,
                    log: dev == "1" ? true : false, //根据是否为测试环境来console输出。
                    row: {},     //当前编辑的行
                    page: {
                        rows: [],   //当前页条目数组
                        size: 10,   //每页条目数
                        count: 0,   //总页数
                        idx: 1,     //当前页
                        range: 5,   //可显示的页码范围
                        start: 1    //可显示的起始页码
                    },
                    lpro: {},       //login profile 包含登陆用户的基本信息(grps，groupinfo,login,porps)。
                    status: "new"   //editor status. 'new' or 'edit'
                },
                filters: {
                    ddlDefault: function (v) {
                        return v == "" ? "-1" : v;
                    },
                    fmtDate: function (v) {     //“日期”过滤器
                        var lang = $("#input-lang-hidden").val();
                        if (lang == "en-US") { // lang=zh-CHS"中文 lang=en-US">English
                            return df(v, "yyyy-mm-dd");
                        } else if (lang == "zh-CHS") {
                            return df(v, "yyyy-mm-dd");
                        } else
                            if (v) return v.substr(0, 10); else return "";
                    },
                    fmtActive: function (v) {   //“有效”过滤器
                        if (v) {
                            if (v.indexOf("I") >= 0) {
                                return "Inactive";
                            } else if (v.indexOf("A") >= 0) {
                                return "Active";
                            } else
                                return "";
                        } else
                            return "";
                    },
                    fmtStatus: function (v) {   //“状态”过滤器
                        if (v) {
                            if (v.indexOf("I") >= 0) {
                                return "UnApproved";
                            } else if (v.indexOf("A") >= 0) {
                                return "Approved";
                            } else
                                return "";
                        } else
                            return "";
                    },
                    trim: function (v) {    //去掉前后的空格过滤器  
                        if (v)
                            return v.replace(/(^\s*)|(\s*$)/g, "");
                        else
                            return "";
                    }
                },
                created: function () {
                    var _this = this;
                    var paras = { groupid: $("#input-groupid-hidden").val(), loginname: $("#input-loginname-hidden").val() }
                    this.$http.post("ajax.ashx?act=get_login_profile", paras, { emulateJSON: true }).then(function (res) {
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
                methods: {
                    init: function () {
                        //修正row样式
                        $(".row").css("margin", "10px 5px 0px 5px");
                        //修正datepicker事件处理
                        $(".am-dp").datepicker({ format: 'yyyy-mm-dd' }).on('changeDate.datepicker.amui', function (event) {
                            event.target.value = event.target.defaultValue;
                        });

                        //检索数据 all
                        this.search("", 1, this.page.size);
                    },
                    timeout: function () {
                        window.location.href = "../98.html";
                    },
                    search: function (c, i, s) { //c,i,s => criteria,pidx,psize
                        var req = url + "search" + "&ts=" + Math.random();
                        var paras = {
                            searchParse: c,
                            pageIdx: isNaN(parseInt(i)) ? 1 : parseInt(i), //检查数字是否非法
                            pageSize: s
                        }

                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    _this.timeout();
                                } else
                                    alert("[search] with error:" + res.body.err_msg);
                            } else {
                                _this.page.rows = res.body.result.data;
                                _this.page.size = res.body.result.pageSize;
                                _this.page.count = res.body.result.count;
                                _this.page.idx = res.body.result.pageIdx;
                                /// -- start -- 页码显示逻辑
                                var q = Math.floor(_this.page.idx / _this.page.range); //5, 5/5=1 => 4,5,6,7,8 ;8/5
                                var r = _this.page.idx % _this.page.range; //5, 5%5=0 
                                if (q >= 1) {
                                    if (r == 0) {
                                        _this.page.start = q * _this.page.range - 1;
                                    } else if (r == 1) {
                                        _this.page.start = q * _this.page.range;
                                    } else
                                        _this.page.start = q * _this.page.range + 1;
                                    if ((_this.page.start + _this.page.range - 1) >= _this.page.count) {
                                        _this.page.start = _this.page.count - _this.page.range + 1;
                                    }
                                } else
                                    _this.page.start = 1;
                                /// --  end  --
                            }
                        }, function (res) {
                            console.log('request failure.');
                        });
                    },
                    popSearchProps: function (e) {
                        var that = this;
                        var w = $("#popWinProps");

                        if (this.status == "new") { //如果是“新建”状态，则要对当前row做一次保存                            
                            this.row = this.preparas();
                        }

                        var $e = $("#editor-x input[alt='FLOW_PROPERTY']"); //获取props输入元素
                        if ($e) { //如果当前编辑条目有值
                            for (var i = 0; i < this.lpro.props.length; i++) { //props列表ticked赋值，用于弹出框的checkbox绑定
                                if ($e.val().indexOf(this.lpro.props[i].PropertyCode) >= 0) {
                                    this.lpro.props[i].Ticked = true;
                                } else {
                                    this.lpro.props[i].Ticked = false;
                                }
                            }
                        }
                        w.modal('show');
                        w.find(".modal-footer button").off('click'); //先取消一次事件绑定。
                        w.find(".modal-footer button").on('click', function (e) { // 绑定"Confirm"按钮事件
                            var propss = [];
                            w.find(".modal-body tbody input").each(function (i) {
                                if (this.checked) {
                                    propss.push(that.lpro.props[i].PropertyCode);
                                }
                            });
                            $e.val(propss.toString());
                        });
                    },
                    create: function () {
                        this.row = { FLOW_STATUS: "I" }
                        this.status = "new";
                        //清除一次子表编辑控件
                        $("#editor-x table tbody tr").remove();

                        $(".form-grid").hide(500);
                        $("#editor-x").show(500);
                    },
                    edit: function (id) {
                        this.status = "edit";
                        //查找选中的行
                        for (var i = 0; i < this.page.rows.length; i++) {
                            if (this.page.rows[i].Flow_Id == id) {
                                this.row = this.page.rows[i];
                                break;
                            }
                        }

                        //清除一次子表编辑控件
                        $("#editor-x table tbody tr").remove();

                        //创建子表控件                        
                        _this = this;
                        for (var j = 0; j < this.row.detail.length; j++) {
                            var l = this.row.detail[j];
                            this.buildSingleDtlRow(l);
                        }

                        if (this.log) {
                            console.log("invoke edit id:" + id);
                            console.log(this.row);
                        }

                        if (this.row.FLOW_STATUS.indexOf("A") >= 0) {
                            $("#editor-x table a").addClass('invisible');
                        } else {
                            $("#editor-x table a").removeClass('invisible');
                        }

                        $("#editor-x").removeClass('hide');
                        $(".form-grid").hide(500);
                        $("#editor-x").show(500);
                    },
                    del: function (id) {
                        $("#confirmDlg").modal("show");
                        var _this = this;
                        $("#confirmDlg #btnYes").on('click', function (e) {
                            if (_this.log) console.log("invoke delete id:" + id);
                            var req = url + "del";
                            var paras = { id: id }
                            _this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                                if (res.body.err_code != 0) {
                                    alert(res.body.err_msg);
                                } else {
                                    alert("Delete success.");
                                    _this.search($("#input-search").val(), _this.page.idx, _this.page.size);
                                }
                            }, function (res) {
                                console.log('request failure.');
                            });
                            $("#confirmDlg #btnYes").off('click');
                        });
                    },
                    jump: function (pidx) {
                        this.search($("#input-search").val(), pidx, this.page.size);
                    },
                    preparas: function () { //准备参数 prepare paras
                        //收集子表参数                        
                        var subs = [];
                        $("#editor-x table tbody tr").each(function (e) {
                            var model = {}
                            model.FWDE_ID = $(this).find("td input[alt='sid']").val();
                            model.FWDE_SEQ = $(this).find("td input[alt='seq']").val();
                            model.FWDE_CODE = $(this).find("td input[alt='code']").val();
                            model.FWDE_NAME = $(this).find("td input[alt='name']").val()
                            model.FWDE_EMAIL = $(this).find("td input[alt='email']").val();

                            // 获取groupname，从options列表里取
                            var $sel = $(this).find("td select[alt='groupid']");
                            model.FWDE_GROUPID = $sel.val();
                            var sidx = $sel[0].options.selectedIndex;
                            model.FWDE_GROUP = $sel[0].options[sidx].innerHTML;
                            subs.push(model);
                        });

                        var paras = {}
                        $("#editor-x .p-field").each(function (i, e) {
                            var key = $(e).attr("alt");
                            var val = $(e).val();
                            // 特殊字段处理
                            /*1
                            if (key == "FLOW_EFFFROM" || key == "FLOW_EFFTO") {
                                var lang = $("#input-lang-hidden").val();
                                paras[key] = lang == "en-US" ? dmyr(val) : val;
                            } else {
                                paras[key] = val;
                            }
                            */
                            paras[key] = val;                         
                        });
                        paras.dtl = JSON.stringify(subs);
                        paras.action = this.status;
                        return paras;
                    },
                    save: function () {
                        var paras = this.preparas();
                        var msg = this.validate(paras);
                        if (msg != "") {
                            alert(msg);
                            return;
                        }

                        if (this.log) console.log("invoke save with paras：" + JSON.stringify(paras));
                        var req = url + "save";
                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    _this.timeout();
                                } else
                                    alert("[save] with error:" + res.body.err_msg);
                            } else {
                                _this.cleanup();
                                _this.search($("#input-search").val(), _this.page.idx, _this.page.size);
                            }
                        }, function (res) {
                            console.log('request failure.');
                            this.cleanup();
                        });
                    },
                    cleanup: function () {
                        $("#editor-x").hide(500);
                        $(".form-grid").show(500);
                    },
                    validate: function (o) { //o => object
                        if (o.Flow_Code == "" || typeof (o.Flow_Code) == "undefined") {
                            return "'flow code' is required";
                        } else if (o.FLOW_NAME == "" || typeof (o.FLOW_NAME) == "undefined") {
                            return "'flow name' is required";
                        } else if (o.FLOW_EFFFROM == "" || typeof (o.FLOW_EFFFROM) == "undefined") {
                            return "'flow efffrom' is required";
                        } else if (o.FLOW_EFFTO == "" || typeof (o.FLOW_EFFTO) == "undefined") {
                            return "'flow to' is required";
                        } else if (o.FLOW_PROPERTY == "" || typeof (o.FLOW_PROPERTY) == "undefined") {
                            return "'flow property' is required";
                        } else if (o.dtl) {
                            var detail = JSON.parse(o.dtl);
                            for (var i = 0; i < detail.length; i++) {
                                if (detail[i].FWDE_CODE == "" || typeof (detail[i].FWDE_CODE) == "undefined") {
                                    return "'fwde code' is required";
                                } else if (detail[i].FWDE_NAME == "" || typeof (detail[i].FWDE_NAME) == "undefined") {
                                    return "'fwde name' is required";
                                } else if (detail[i].FWDE_GROUP == "" || typeof (detail[i].FWDE_GROUP) == "undefined") {
                                    return "'fwde group' is required";
                                }
                            }
                            return "";
                        } else
                            return "";
                    },
                    approve: function () {
                        var _this = this;
                        var req = url + "approve";
                        var paras = { id: this.row.Flow_Id }
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    _this.timeout();
                                } else
                                    alert("[approve] with error:" + res.body.err_msg);
                            } else {
                                _this.cleanup();
                                _this.search($("#input-search").val(), _this.page.idx, _this.page.size);
                            }
                        }, function (err) {
                            alert(err);
                        });
                    },
                    buildSingleDtlRow: function (r) { // append detail html element     
                        var _this = this;
                        //开启一行
                        $tr = $("<tr></tr>");

                        var ts = "";
                        if (r)
                            $tr.append($("<td class='hidden'><input type='text' class='form-control' alt='sid' value='" + r.FWDE_ID + "'/></td>"));
                        else
                            $tr.append($("<td class='hidden'><input type='text' class='form-control' alt='sid' value='0'/></td>"));
                        //seq控件填值
                        if (r)
                            $tr.append($("<td><input type='text' class='form-control' alt='seq' readonly value='" + r.FWDE_SEQ + "'/></td>"));
                        else
                            $tr.append($("<td><input type='text' class='form-control' alt='seq' readonly value='0'/></td>"));
                        //code填值
                        if (r)
                            $tr.append($("<td><input type='text' class='form-control' alt='code' value='" + r.FWDE_CODE + "'/></td>"));
                        else
                            $tr.append($("<td><input type='text' class='form-control' alt='code' value=''/></td>"));
                        //name填值
                        if (r)
                            $tr.append($("<td><input type='text' class='form-control' alt='name' value='" + r.FWDE_NAME + "'/></td>"));
                        else
                            $tr.append($("<td><input type='text' class='form-control' alt='name' value=''/></td>"));
                        //select 控件赋值
                        $es = $("<select class='form-control' alt='groupid'></select>");
                        $opt = $("<option value='-1'>-select-</option>");
                        $es.append($opt);
                        for (var i = 0; i < _this.lpro.grps.length; i++) {
                            $opt = $("<option value='" + _this.lpro.grps[i].GroupId + "'>" + _this.lpro.grps[i].GroupName + "</option>");
                            $es.append($opt);
                        }

                        if (r) $es.val(r.FWDE_GROUPID);
                        $td = $("<td></td>").append($es);
                        $tr.append($td);
                        //email填值
                        if (r) {
                            if (r.FWDE_EMAIL) {
                                $tr.append($("<td><input type='text' class='form-control' alt='email' value='" + r.FWDE_EMAIL + "' /></td>"));
                            } else
                                $tr.append($("<td><input type='text' class='form-control' alt='email' /></td>"));
                        }
                        else
                            $tr.append($("<td><input type='text' class='form-control' alt='email' /></td>"));
                        //oper.创建删除按钮
                        $ea = $("<a class='label label-danger' v-on:click='delDtl'>del</a>");
                        $ea.on('click', function (e) {
                            $(e.currentTarget).parent().parent().remove();
                        });
                        $tr.append($("<td></td>").append($ea));
                        //插入行
                        $("#editor-x table tbody").append($tr);
                    },
                    txtChange: function (e) { // 用于字符串检索
                        console.log($(e.target).val());
                        this.search($("#input-search").val(), 1, 10);
                    }
                }
            });
            vm.init();
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="col-md-12 global_table_head global_color_navyblue">
        <p>Flow Definition</p>
    </div>    
    <div class="global_color_lightblue" id="vue-dt">
        <!--修正-->
        <div class="row"></div>     

        <!--检索-->
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
                            <th class="hide">Flow Id</th>                        
                            <th>Flow Code</th>
                            <th>Flow Name</th>
                            <th>Flow Desc.</th>
                            <th>Status</th>                    
                            <th class="text-center" style="width:100px;">Operation</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in page.rows">
                            <td class="text-center hide">{{item.Flow_Id}}</td>                            
                            <td class="text-center"><a href="javascript:void(0)" v-on:click="edit(item.Flow_Id)">{{item.Flow_Code}}</a></td>                            
                            <td class="text-center">{{item.FLOW_NAME}}</td>
                            <td class="text-center">{{item.FLOW_DESC}}</td>
                            <td class="text-center">{{item.FLOW_STATUS|fmtStatus}}</td>                            
                            <td class="text-center" style="width:100px;"><a class="label label-danger" v-on:click="del(item.Flow_Id)">Delete</a></td>
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
                                    <input type="number" class="form-control p-field" alt="Flow_Id" v-bind:value="row.Flow_Id" />
                                </div>                                    
                            </div>       
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Flow Code:</label>
                                <div class="col-sm-2">
                                    <input type="text" class="form-control p-field" alt="Flow_Code" v-bind:value="row.Flow_Code" />
                                </div>    
                                <label class="col-sm-2 control-label">Flow Name:</label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control p-field" alt="FLOW_NAME" v-bind:value="row.FLOW_NAME"/>
                                </div>                                
                            </div>
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Flow EffFrom:</label>
                                <div class="col-sm-3">                                        
                                    <input id="Text1" type="text" class="am-dp form-control p-field" alt="FLOW_EFFFROM" v-bind:value="row.FLOW_EFFFROM |fmtDate" readonly />
                                </div>
                                    <label class="col-sm-2 control-label">Flow EffTo:</label>
                                <div class="col-sm-3">
                                    <input id="Text2" type="text" class="am-dp form-control p-field" alt="FLOW_EFFTO" v-bind:value="row.FLOW_EFFTO|fmtDate" readonly/>
                                </div>
                            </div>  
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Description:</label>
                                <div class="col-sm-8">                                        
                                    <input type="text" class="form-control p-field" alt="FLOW_DESC" v-bind:value="row.FLOW_DESC" />
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Properties:</label>
                                <div class="col-sm-8">
                                    <div class="input-group">
                                        <input type="text" class="form-control p-field" alt="FLOW_PROPERTY" placeholder="Search for..." v-bind:value="row.FLOW_PROPERTY" />
                                        <span class="input-group-btn">
                                            <button class="btn btn-success" type="button" v-on:click="popSearchProps">...</button>
                                        </span>
                                    </div>
                                </div>
                            </div>                                            
                            <div class="form-group" v-if="status=='edit'">
                                <label class="col-sm-2 control-label">Create By:</label> 
                                <div class="col-sm-3">
                                    <span class="form-control" v-if="row.FLOW_CREATEBY!='' &&row.FLOW_CREATEBY!=null ">{{row.FLOW_CREATEBY}}/{{row.FLOW_CREATEDATE|fmtDate}}</span>
                                </div>
                                <label class="col-sm-2 control-label">Update By:</label> 
                                <div class="col-sm-3">
                                    <span class="form-control" v-if="row.FLOW_UPDATEBY!='' &&row.FLOW_UPDATEBY!=null ">{{row.FLOW_UPDATEBY}}/{{row.FLOW_UPDATEDATE|fmtDate}}</span>
                                </div>                                    
                            </div>    
                            <div class="form-group" v-if="status=='edit'">
                                <label class="col-sm-2 control-label">Approved By:</label> 
                                <div class="col-sm-3">
                                    <span class="form-control" v-if="row.FLOW_APPROVEBY!='' &&row.FLOW_APPROVEBY!=null ">{{row.FLOW_APPROVEBY}}/{{row.FLOW_APPROVEDATE|fmtDate}}</span>
                                </div>
                            </div>
                            <div role="separator" class="divider">
                                <button class="btn btn-primary" v-on:click="buildSingleDtlRow(null)">Add</button>
                            </div>                                        
                            <table class="table">
                                <thead>
                                    <tr>                               
                                        <th class="hidden">Sid</th>             
                                        <th style="width:10%">Seq.</th>
                                        <th>Code</th>
                                        <th>Name</th>
                                        <th>Group</th>
                                        <th>Email</th>
                                        <th>Oper</th>
                                    </tr>
                                </thead>  
                                <tbody></tbody>
                            </table> 
                         </div>
                    </div>
                    <div class="panel-footer text-right">
                        <button type="button" class="btn btn-default" data-dismiss="modal" v-on:click="cleanup">Close</button>                            
                        <button type="button" class="btn btn-primary" v-on:click="save" :disabled="row.FLOW_STATUS && row.FLOW_STATUS.indexOf('A')>0?true:false">Save</button>                                                
                        <button type="button" class="btn btn-success" v-if="lpro.grpinfo && lpro.grpinfo.Approver=='1'" v-on:click="approve" key="btn-approve"  :disabled="row.FLOW_STATUS && row.FLOW_STATUS.indexOf('A')>0?true:false">Approve</button>                                                                                 
                    </div>
                </div>
             </div>
        </div>

        <!-- 模态窗口 “property”选择 -->
        <div class="row">            
            <div class="modal fade" id="popWinProps" tabindex="-1" role="dialog" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" >&times;</button>
                            <h4 class="modal-title">Select Properties</h4>
                        </div>
                        <div class="modal-body" > 
                            <div>
                                <table class="table table-bordered small" style="width:100%; margin-bottom:0">
                                    <colgroup>
                                        <col style="width: 80px;" />
                                        <col />
                                    </colgroup>
                                    <thead>
                                        <tr>
                                            <th class="text-center">Select</th>
                                            <th class="text-left">Peoperty</th>
                                        </tr>
                                    </thead> 
                                </table>
                            </div>                            
                            <div style="height:450px; overflow:scroll;">
                                <table class="table table-bordered small" style="width:100%">       
                                    <colgroup>  
                                        <col style="width: 80px;" />
                                        <col />
                                    </colgroup>                           
                                    <tbody>
                                        <tr v-for="item in lpro.props">
                                            <td class="text-right"><input type="checkbox" class="form-control" v-model="item.Ticked" /></td>
                                            <td class="text-left">{{item.PropertyCode}} <span v-if="row.FORM_PROPERTY?row.FORM_PROPERTY.indexOf(item.PropertyCode)>=0:false">&nbsp;<i class="fa fa-check-square"></i></span></td>
                                        </tr>
                                    </tbody>
                                </table>    
                            </div>                                                                                                  
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-default" data-dismiss="modal">Confirm</button>
                        </div>
                    </div><!-- /.modal-content -->
                </div><!-- /.modal -->
            </div>
        </div>

        <!-- 模态窗口-->
        <div class="row">            
            <div class="modal fade" id="editor" tabindex="-1" role="dialog"  aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                            <h4 class="modal-title">Editor</h4>
                        </div>
                        <div class="modal-body">                            
                            <div class="form-horizontal">                                             
                            </div>
                        </div>
                        <div class="modal-footer">
                           
                        </div>
                    </div><!-- /.modal-content -->
                </div><!-- /.modal -->
            </div>
        </div>   

        <!-- 模态提示对话框 -->
        <div class="modal fade" tabindex="-1" role="dialog" id="confirmDlg" >
          <div class="modal-dialog" role="document">
            <div class="modal-content">
              <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                <h4 class="modal-title">Confirm Dialog</h4>
              </div>
              <div class="modal-body">
                <p>Are you sure to delete ?</p>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-default" id="btnYes" data-dismiss="modal">Yes</button>
                <button type="button" class="btn btn-primary" id="btnNo" data-dismiss="modal">No</button>
              </div>
            </div><!-- /.modal-content -->
          </div><!-- /.modal-dialog -->
        </div><!-- /.modal -->   
    </div>
</asp:Content>