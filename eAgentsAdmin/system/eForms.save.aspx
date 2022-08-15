<%@ Page Language="C#" MasterPageFile="~/SiteSystemBootcss.Master" AutoEventWireup="true" 
    CodeBehind="eForms.save.aspx.cs" Inherits="PropertyOneAppWeb.system.eForms" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/vue.js" type="text/javascript"></script>
    <script src="../js/vue-resource.min.js" type="text/javascript"></script>    
    <script type="text/javascript">
        //查找querystring变量值
        function qs(search, key) {
            var patt = /(\w*)=(\w*)/g;
            var q = search.match(patt);
            if (q) {
                for (var i = 0; i < q.length; i++) {
                    var kv = q[i].split("=");
                    if (kv[0] == key) {
                        return kv[1];
                    }
                }
            }
            return "";
        }
        var url = "eForms.aspx?act=";
        var vue = null;
        var ue = null;

        $(document).ready(function () {

            vue = new Vue({ //定义vue应用
                el: "#vue-dt",
                data: {
                    log: true,
                    demo: false, //true表示开发环境，false不是
                    props: [],
                    row: {},        //当前编辑的行
                    groupInfo: {},  //用户组信息
                    sels: [],
                    page: {
                        rows: [],   //当前页条目数组
                        size: 10,    //每页条目数
                        count: 0,   //总页数
                        idx: 1,     //当前页
                        range: 5,   //可显示的页码范围
                        start: 1    //可显示的起始页码
                    },
                    approve_mode: false,
                    status: "new"   //editor status. 'new' or 'edit'                                 
                },
                filters: {
                    fmtDate: function (v) {     //“日期”过滤器
                        if (v) return v.substr(0, 10); else return "";
                    },
                    fmtActive: function (v) {   //“有效”过滤器
                        if (v.indexOf("I") >= 0) {
                            return "Inactive";
                        } else if (v.indexOf("A") >= 0) {
                            return "Active";
                        } else
                            return "";
                    },
                    fmtStatus: function (v) {   //“状态”过滤器
                        if (v.indexOf("I") >= 0) {
                            return "UnApproved";
                        } else if (v.indexOf("A") >= 0) {
                            return "Approved";
                        } else
                            return "";
                    },
                    trim: function (v) {
                        if (v)
                            return v.replace(/(^\s*)|(\s*$)/g, ""); //去掉前后的空格                        
                        else
                            return "";
                    }
                },
                methods: {
                    init: function () {
                        this.checkEnv();
                        this.getGroup();        //获取当前登录用户的组信息及property权限
                        this.addTickListener(); //为grid添加事件处理，保存已选的行(目前未使用)       
                        var fid = qs(window.location.href, "id");   //如果link含有querystring为id的情况，需要带入查询条件 
                        var fcode = qs(window.location.href, "code"); //如果link含有querystring为code的情况，需要带入查询条件 
                        if (fid != "0" && fid != "") {
                            this.search(fid, 1, this.page.size); //检索数据,fid
                            this.approve_mode = true;
                        } else if (fcode != "") {
                            this.search(fcode, 1, this.page.size); //检索数据 code
                            this.approve_mode = true;
                        }
                        else {
                            this.search("", 1, this.page.size); //检索数据 all
                            this.approve_mode = false;
                        }

                        $(".row").css("margin", "10px 5px 0px 5px"); //修正row样式
                        $(".pagination").css("margin-top", "0px").css("margin-bottom", "0px"); //修正pagination样式                       
                        $(".am-dp").datepicker({ format: 'yyyy-mm-dd' }).on('changeDate.datepicker.amui', function (event) {
                            event.target.value = event.target.defaultValue; //修正datepicker事件处理
                        });

                        $("#confirmDlg #btnNo").on('click', function () {
                            $("#confirmDlg #btnYes").off('click');
                        });
                    },
                    addTickListener: function () {
                        var _this = this;
                        $("tbody").on('click', function (e) {
                            if (e.target.tagName != "TD") {
                                return;
                            }
                            var id = parseInt($(e.target).parent()[0].cells[0].innerText);
                            var tmpArr = [];
                            var exist = false;
                            for (var i = 0; i < _this.sels.length; i++) {
                                if (id == _this.sels[i]) {
                                    exist = true;
                                } else {
                                    tmpArr.push(_this.sels[i]);
                                }
                            }
                            if (!exist) {
                                tmpArr.push(id);
                            }
                            _this.sels = tmpArr;
                            if (_this.log) console.log("invoke tick id: " + id + "[" + _this.sels + "]");
                        });
                    },
                    checkEnv: function () {
                        var _this = this;
                        var req = url + "checkenv" + "&ts=" + Math.random();
                        this.$http.post(req, null, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.demo = res.body.result;
                                console.log(_this.demo);
                            }
                        }, function (res) {
                            console.log('request failure.');
                        });
                    },
                    getGroup: function () {
                        var _this = this;
                        var req = url + "getGroup" + "&ts=" + Math.random();
                        this.$http.post(req, null, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.groupInfo = res.body.result;
                                _this.props = res.body.result.props;
                                if (this.log) console.log("invoke get group:" + JSON.stringify(_this.groupInfo));
                            }
                        }, function (res) {
                            console.log('request failure.');
                        });
                    },
                    getProps: function () {
                        var _this = this;
                        var req = url + "getProps" + "&ts=" + Math.random();
                        this.$http.post(req, null, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.props = res.body.result;

                            }
                        }, function (res) {
                            console.log('request failure.');
                        });
                    },
                    search: function (criteria, pIdx, pSize) { //"eForms.aspx?act="; http://localhost:64307/system/eForms.aspx?token=fef1263&id=116
                        var req = url + "search" + "&ts=" + Math.random();
                        var paras = {
                            searchParse: criteria,
                            pageIdx: isNaN(parseInt(pIdx)) ? 1 : parseInt(pIdx), //检查数字是否非法
                            pageSize: pSize
                        }

                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.page.rows = res.body.result.data;
                                _this.page.size = res.body.result.pageSize;
                                _this.page.count = res.body.result.count;
                                _this.page.idx = res.body.result.pageIdx;
                                /// -- start -- 翻页逻辑
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
                    create: function () {
                        this.row = {}
                        this.status = "new";
                        $(".prop").each(function (i, e) {
                            $(e)[0].checked = false;
                        });

                        $("#input-attach-url").attr("rpath", "");
                        $("#input-attach-url").parent().find(".preview p").text("");
                        $("#logo").attr("src", "");
                    },
                    edit: function (id) {
                        this.status = "edit";
                        for (var i = 0; i < this.page.rows.length; i++) {
                            if (this.page.rows[i].FORM_ID == id) {
                                this.row = this.page.rows[i];
                                $(".preview p").text(this.page.rows[i].FORM_ATTACHNAME); //填充界面
                                break;
                            }
                        }
                        if (this.log) {
                            console.log("invoke edit id:" + id);
                            console.log(this.row);
                        }

                        //填充ueditor                        
                        ue.setContent(this.row.FORM_WAIST_HTML);

                        $("#editor").modal('show');
                    },
                    tick: function (id) {
                        var tmpArr = [];
                        var exist = false;
                        for (var i = 0; i < this.sels.length; i++) {
                            if (id == this.sels[i]) {
                                exist = true;
                            } else {
                                tmpArr.push(this.sels[i]);
                            }
                        }
                        if (!exist) {
                            tmpArr.push(id);
                        }
                        var _this = this;
                        setTimeout(function () {
                            _this.sels = tmpArr;
                        }, 500);
                    },
                    isTick: function (id) {
                        if (this.sels.toString().indexOf(id) > 0)
                            return true;
                        else
                            return false;
                    },
                    tickAll: function () {

                    },
                    approve: function (id) {
                        var req = url + "approve";
                        var paras = { ids: this.row.FORM_ID }
                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.search($("#input-search").val(), _this.page.idx, _this.page.size);
                                if (this.log) console.log("invoke approve id:" + JSON.stringify(paras));
                            }
                        }, function (res) {
                            console.log('request failure.');
                        });
                        this.cleanup();
                        $("#editor").modal('hide');

                        var link = window.location.href;
                        var link = link.replace(window.location.search, "");
                        window.location = link;
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
                    jump: function (p) {
                        this.search($("#input-search").val(), p, this.page.size);
                    },
                    validate: function (paras) {
                        var err_msg = "";
                        if (paras.FORM_CODE == "" || paras.FORM_CODE == null) {
                            err_msg = "Form Code is required.";
                        } else if (paras.FORM_NAME == "" || paras.FORM_NAME == null) {
                            err_msg = "Form Name is required.";
                        } else if (paras.FORM_EFFFROM == "" || paras.FORM_EFFFROM == null) {
                            err_msg = "Form EffFrom is required.";
                        } else if (paras.props == "" || paras.props == null) {
                            if (this.status == "new") {
                                err_msg = "Form Properties is required.";
                            } else {
                                if (paras.FORM_PROPERTY == "" || paras.FORM_PROPERTY == null) {
                                    err_msg = "Form Property is required.";
                                }
                            }
                        } else if (paras.FORM_ATTACHNAME == "" || paras.FORM_ATTACHNAME == null) {
                            err_msg = "Form Attach is required.";
                        } else if (paras.FORM_LEASE) {
                            var patt = /(\w{4})(\d{5}(?!\w\d))/g;
                            var result = patt.test(paras.FORM_LEASE);
                            if (!result) {
                                err_msg = "lease format error.";
                            } else
                                paras.FORM_LEASE = paras.FORM_LEASE.match(patt)[0];
                        }

                        if (err_msg.length > 0) {
                            alert(err_msg);
                            return false;
                        } else
                            return true;
                    },
                    save: function () {
                        var paras = {}
                        $("#editor .p-field").each(function (i, e) {
                            var key = $(e).attr("alt");
                            var val = $(e).val();
                            paras[key] = val;
                        });
                        //获取附件参数
                        paras["FORM_ATTACHURL"] = $("#input-attach-url").attr("rpath");
                        paras["FORM_ATTACHNAME"] = $("#input-attach-url").parent().find(".preview p").text();
                        //获取logo参数
                        if ($("#logo").attr("src")) {
                            var b64 = $("#logo").attr("src").replace("data:image/jpg;base64,", ""); //data:image/jpg;base64,
                            if (b64 != "" || b64 != null) {
                                paras["FORM_LOGO_Base64"] = b64;
                            }
                        }
                        var props = "";
                        if (this.status == "new") {
                            paras.FORM_ID = 0;  //新建时要给个默认值0，否则后台反序列化报错。
                            $(".prop").each(function (i, e) {
                                if ($(e)[0].checked) {
                                    props += $(e).parent()[0].innerText + ",";
                                }
                            });
                            paras.props = props.substr(0, props.length - 1);
                        } else {
                            paras.props = $("select[alt='FORM_PROPERTY'] ").val();
                        }

                        //获取ueditor数据
                        ue.ready(function () {
                            //获取html内容，返回: <p>hello</p>
                            var html = ue.getContent();
                            paras["FORM_WAIST_HTML"] = html;
                        });

                        /// validation                         
                        if (!this.validate(paras)) {
                            return;
                        }
                        paras.action = this.status;
                        if (this.log) console.log("invoke save with paras：" + JSON.stringify(paras));
                        var req = url + "save";
                        var _this = this;
                        this.$http.post(req, JSON.stringify(paras), { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                this.cleanup();
                                $("#editor").modal('hide');
                                _this.search($("#input-search").val(), _this.page.idx, _this.page.size);
                            }
                        }, function (res) {
                            console.log('request failure.');
                            this.cleanup();
                            $("#editor").modal('hide');
                        });
                    },
                    cleanup: function () {
                        if (this.log) console.log("invoke cleanup.");
                        this.row = {}
                        $("#input-attach-url").val("");
                    },
                    upload: function (event) { //上传文件或图片
                        if (this.log) {
                            console.log("invoke upload.");
                        }
                        var _this = this;
                        var a = event.target.files[0];
                        var that = this;
                        var formData = new FormData();
                        formData.append('file', a);
                        var req = url + "upload";
                        this.$http.post(req, formData, {
                            headers: { 'Content-Type': 'multipart/form-data' }
                        }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                $(".preview p").text(res.body.result.name);
                                $("#input-attach-url").attr("rpath", res.body.result.url);
                            }
                        }, function (res) {
                            console.log(res.body);
                        });
                    },
                    getBase64Str: function () {
                        var a = event.target.files[0];
                        var that = this;
                        var formData = new FormData();
                        formData.append('file', a);
                        var req = url + "getbase64str";
                        this.$http.post(req, formData, {
                            headers: { 'Content-Type': 'multipart/form-data' }
                        }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                //console.log(a);
                                var src = "data:image/jpg;base64,";
                                src += res.body.result;
                                $("#logo").attr("src", src);
                            }
                        }, function (res) {
                            console.log(res.body);
                        });
                    },
                    selectAll: function ($event) {
                        if ($event.target.checked) {
                            $(".prop")[0].checked = true;
                        }
                        else {
                            $(".prop")[0].checked = false;
                        }
                    },
                    close: function () {
                        //$(".form-grid").toggle("hide");
                    },
                    txtChange: function (e) {
                        var p = $(e.target).val();
                        if (p.length == 0) {
                            this.search("", 1, this.page.size);
                        }

                        if (p.length >= 3) {
                            this.search(p, 1, this.page.size);
                        }
                    }
                }
            });
            vue.init();

            changeNav("../Image/icon_MyStatment.png", "eApplication");
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">       
    <div class="col-md-12 global_table_head global_color_navyblue">
        <p>eApplication</p>         
    </div>    
    <div class="global_color_lightblue" id="vue-dt">             
        <!--修正-->
        <div class="row"></div>        
        <!--检索-->
        <div class="row">
            <div class="col-md-5">
               <span v-if="approve_mode">You are enter approve mode, you can click <a href="/system/eforms.aspx">[quit]</a> to exit.</span>
            </div>
            <div class="col-md-3 col-md-offset-4">
                <div class="input-group">
                    <span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>
                    <input type="text" id="input-search" class="form-control" v-on:keyup="txtChange($event)" />                    
                </div>                 
            </div>
        </div>
        <!--表格-->
        <div class="row form-grid">     
            <div class="col-md-12">
                <table class="table table-condensed table-hover table-striped global-bootgrid bootgrid-table">
                    <thead>
                        <tr>
                            <th class="hide">Id</th>
                            <th class="text-center hide" style="width:50px;"><input type="checkbox" v-on:click="tickAll" /></th>
                            <th>Code</th>
                            <th>Name</th>
                            <th>Desc.</th>
                            <th>Property</th>
                            <th>Active</th>
                            <th>Status</th>
                            <th class="text-left">Attachment</th>
                            <th class="text-center" style="width:100px;">Operation</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in page.rows">
                            <td class="text-center hide" v-bind:class="{'bg-info':isTick(item.FORM_ID)}">{{item.FORM_ID}}</td>
                            <td class="text-center hide" style="width:50px;"><input type="checkbox" class="form-control" v-bind:checked="isTick(item.FORM_ID)" /></td>
                            <td class="text-center"><a href="javascript:void(0)" v-on:click="edit(item.FORM_ID)">{{item.FORM_CODE}}</a></td>                            
                            <td class="text-center">{{item.FORM_NAME}}</td>
                            <td class="text-center">{{item.FORM_DESC}}</td>
                            <td class="text-center">{{item.FORM_PROPERTY}}</td>
                            <td class="text-center ">{{item.FORM_ACTIVE|fmtActive}}</td>
                            <td class="text-center">{{item.FORM_STATUS|fmtStatus}}</td>
                            <td class="text-left"><a v-bind:href="'../'+item.FORM_ATTACHURL" target="_blank">{{item.FORM_ATTACHNAME}}</a></td>
                            <td class="text-center" style="width:100px;"><a class="label label-danger" v-on:click="del(item.FORM_ID)">Delete</a></td>
                        </tr>
                    </tbody>
                </table>
            </div>             
        </div>

        <!--翻页-->
        <div class="row form-grid">  
            <div class="col-md-3">
                <button class="btn btn-primary" data-toggle="modal" data-target="#editor" v-on:click="create">Create</button> 
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
        <div class="row">
            <div class="col-md-12">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Editor
                    </div>
                    <div class="panel-body">
                        <div class="form-horizontal">
                            <div class="form-group hide">
                                <label class="col-sm-3 control-label">Id:</label>
                                <div class="col-sm-3">
                                    <input type="number" class="form-control p-field" alt="FORM_ID" v-bind:value="row.FORM_ID" />
                                </div>                                    
                            </div>                                
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Code:</label>
                                <div class="col-sm-3">
                                    <input type="text" class="form-control p-field" alt="FORM_CODE" v-bind:value="row.FORM_CODE" />
                                </div>    
                                <label class="col-sm-2 control-label">Name:</label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control p-field" alt="FORM_NAME"  v-bind:value="row.FORM_NAME"/>
                                </div>                                
                            </div>
                        </div>

                        <button type="button" class="btn btn-default" data-dismiss="modal" v-on:click="close">Close</button>
                        <button type="button" class="btn btn-warning" v-on:click="approve" v-if="groupInfo.group !=null && groupInfo.group.Approver=='1'">Approve</button>
                        <button type="button" class="btn btn-primary" v-on:click="save">Save</button>
                    </div>
                </div>
                
            </div>            
        </div>

        <!--模态窗口-->
        <div class="row hide">            
            <div class="modal fade" id="editor" tabindex="-1" role="dialog"  aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" v-on:click="cleanup">&times;</button>
                            <h4 class="modal-title">Editor</h4>
                        </div>
                        <div class="modal-body">                            
                            <div class="form-horizontal">
                                <div class="form-group hide">
                                    <label class="col-sm-3 control-label">Id:</label>
                                    <div class="col-sm-3">
                                        <input type="number" class="form-control p-field" alt="FORM_ID" v-bind:value="row.FORM_ID" />
                                    </div>                                    
                                </div>                                
                                <div class="form-group">
                                    <label class="col-sm-2 control-label">Code:</label>
                                    <div class="col-sm-3">
                                        <input type="text" class="form-control p-field" alt="FORM_CODE" v-bind:value="row.FORM_CODE" />
                                    </div>    
                                    <label class="col-sm-2 control-label">Name:</label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control p-field" alt="FORM_NAME"  v-bind:value="row.FORM_NAME"/>
                                    </div>                                
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-2 control-label">Lease Number:</label>
                                    <div class="col-sm-3">
                                        <input type="text" class="form-control p-field" alt="FORM_LEASE"  v-bind:value="row.FORM_LEASE | trim" maxlength="10"/>
                                    </div>
                                    <label class="col-sm-2 control-label">Description:</label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control p-field" alt="FORM_DESC"  v-bind:value="row.FORM_DESC"/>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-2 control-label">EFF From:</label>
                                    <div class="col-sm-3">                                        
                                        <input id="datepicker-from" type="text" class="am-dp form-control p-field" alt="FORM_EFFFROM" v-bind:value="row.FORM_EFFFROM |fmtDate" readonly />
                                    </div>
                                     <label class="col-sm-2 control-label">EFF To:</label>
                                    <div class="col-sm-3">
                                        <input id="datepicker-to" type="text" class="am-dp form-control p-field" alt="FORM_EFFTO" v-bind:value="row.FORM_EFFTO|fmtDate" readonly/>
                                    </div>
                                </div>   
                                <div class="form-group" v-bind:class="{ hide: !demo }">
                                    <label class="col-sm-2 control-label">Flow:</label>
                                    <div class="col-sm-4">                                        
                                        <select class="form-control" alt="FORM_FLOW">
                                            <option value="-1">-select-</option>
                                            <option value="33">Flow01</option>
                                            <option value="34">Flow02</option>
                                        </select>
                                    </div>
                                    <label class="col-sm-2 control-label hide">Template:</label>
                                    <div class="col-sm-4 hide">                                        
                                        <select class="form-control" alt="FORM_TEMPLATE" v-bind:value="row.FORM_TEMPLATE">
                                            <option value="-1">-select-</option>
                                            <option value="23">Template1</option>
                                            <option value="24">Template2</option>
                                        </select>
                                    </div>                                    
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-2 control-label">Attachment:</label>
                                    <div class="col-sm-9">
                                        <label class="btn btn-info" for="input-attach-url">Upload (PDF, PNG, JPG)</label>
                                        <input type="file" alt="FORM_ATTACHURL" id="input-attach-url" v-bind:rpath="row.FORM_ATTACHURL" style="opacity: 0; float:right;" v-on:change="upload($event)"  />
                                        <div class="preview">
                                            <p style="margin-bottom: 0 !important;">{{row.FORM_ATTACHNAME}}</p> <!-- No files currently selected for upload-->
                                        </div>
                                    </div>
                                </div>
                                <div class="form-group" v-if="status=='edit'">
                                    <label class="col-sm-2 control-label">Property:</label>       
                                    <div class="col-sm-4">
                                        <select class="form-control p-field" alt="FORM_PROPERTY">
                                            <option value="-1">-- select --</option>
                                            <option v-for="item in groupInfo.props" v-bind:value="item.PropertyCode" v-bind:selected="item.PropertyCode==row.FORM_PROPERTY">{{item.PropertyCode}}</option>
                                        </select>
                                        <!--<input type="text" class="form-control p-field" alt="FORM_PROPERTY" v-bind:value="row.FORM_PROPERTY" />-->
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-2 control-label">Logo:</label>
                                    <div class="col-sm-9">
                                        <label class="btn btn-info" for="input-image">Upload (PNG, JPG)</label> 
                                        <input type="file" alt="FORM_LOGO" id="input-image" style="opacity: 0; float:right;" v-on:change="getBase64Str($event)"  />
                                        <img class="preview" id="logo" v-if="row.FORM_LOGO_Base64!=''" v-bind:src="'data:image/jpg;base64,'+row.FORM_LOGO_Base64" />                                       
                                    </div>
                                </div>
                                <div class="form-group" v-bind:class="{ hide: !demo }">
                                    <label class="col-sm-2 control-label">Header:</label>
                                    <div class="col-sm-9">
                                        <textarea class="form-control p-field" alt="FORM_HEADER" rows="1">{{row.FORM_HEADER|trim}}</textarea>
                                        <!-- 加载编辑器的容器 -->   
                                    </div>
                                </div>
                                <div class="form-group" v-bind:class="{ hide: !demo }">
                                    <label class="col-sm-2 control-label">Content:</label>
                                    <div class="col-sm-9 text-left">
                                        <div id="ueditor" name="content"></div>
                                    </div>
                                </div>
                                <div class="form-group" v-bind:class="{ hide: !demo }">
                                    <label class="col-sm-2 control-label">Footer:</label>
                                    <div class="col-sm-9">
                                        <textarea class="form-control p-field" alt="FORM_FOOTER" rows="1">{{row.FORM_FOOTER|trim}}</textarea>
                                    </div>
                                </div>
                                <div class="form-group" v-if="status=='edit'">
                                    <label class="col-sm-2 control-label">Create By:</label> 
                                    <div class="col-sm-3">
                                        <span class="form-control small">{{row.FORM_CREATEBY}}/{{row.FORM_CREATEDATE|fmtDate}}</span>
                                    </div>
                                    <label class="col-sm-2 control-label">Update By:</label> 
                                    <div class="col-sm-3">
                                        <span class="form-control">{{row.FORM_UPDATEBY}}/{{row.FORM_UPDATEDATE|fmtDate}}</span>
                                    </div>                                    
                                </div> 
                                <div class="form-group" v-if="status=='edit'">
                                    <label class="col-sm-2 control-label">Approve By:</label> 
                                    <div class="col-sm-4">
                                        <span class="form-control">{{row.FORM_APPROVEBY}}/{{row.FORM_APPROVEDATE|fmtDate}}</span>
                                    </div>
                                </div>                            
                                <div class="form-group" v-if="status=='new'">
                                    <label class="col-sm-2 control-label">Properties:</label>       
                                    <div class="col-sm-9">
                                        <ul class="list-group">
                                            <li class="list-group-item">
                                                <p style="margin-bottom:0px">
                                                    <label>
                                                        <input type="checkbox" v-on:click="selectAll($event)" />All
                                                    </label>
                                                </p>                                          
                                                <label class="checkbox-inline" v-for="item in props">
                                                    <input type="checkbox" class="prop" />{{item.PropertyCode}}
                                                </label> 
                                            </li>
                                        </ul>                                    
                                    </div>                                                                 
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-default" data-dismiss="modal" v-on:click="cleanup">Close</button>
                            <button type="button" class="btn btn-warning" v-on:click="approve" v-if="groupInfo.group !=null && groupInfo.group.Approver=='1'">Approve</button>
                            <button type="button" class="btn btn-primary" v-on:click="save">Save</button>
                        </div>
                    </div><!-- /.modal-content -->
                </div><!-- /.modal -->
            </div>
        </div>

        <!--模态提示对话框 -->
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

    <!-- 配置文件 -->
    <script type="text/javascript" src="../js/ueditor/ueditor.config.js"></script>
    <!-- 编辑器源码文件 -->
    <script type="text/javascript" src="../js/ueditor/ueditor.all.js"></script>
    <!-- 实例化编辑器 -->
    <script type="text/javascript">
        ue = UE.getEditor('ueditor');
    </script>
</asp:Content>