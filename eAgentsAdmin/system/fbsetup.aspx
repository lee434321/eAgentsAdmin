<%@ Page Language="C#" MasterPageFile="~/SiteSystemBootcss.Master" AutoEventWireup="true" 
    CodeBehind="fbsetup.aspx.cs" Inherits="PropertyOneAppWeb.system.fbsetup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/vue.js" type="text/javascript"></script>    
    <script src="../js/vue-resource.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        var url = "fbsetup.aspx?act=";
        var vue = null;
        $(document).ready(function () {
            vue = new Vue({
                el: "#vue-dt",
                data: {
                    page: {
                        rows: [],   //当前页条目数组
                        size: 10,   //每页条目数
                        count: 0,   //总页数
                        idx: 1,     //当前页
                        range: 5,   //可显示的页码范围
                        start: 1    //可显示的起始页码
                    },
                    row: {},
                    notis: [],      //所有的notification。
                    props: [],      //所有property
                    users: [],      //所有后台人员
                    status: "new",
                    log: true
                },
                methods: {
                    init: function () {
                        this.getNotis();
                        this.search("", 1, 10);
                        $(".row").css("margin", "10px 5px 0px 5px"); //修正row样式
                        $(".pagination").css("margin-top", "0px").css("margin-bottom", "0px"); //修正pagination样式      
                    },
                    del: function (id) {
                        var req = url + "del";
                        var paras = { id: id }
                        var _this = this;
                        promptEx("", "Are you sure to delete?", function (e) {
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
                        });                       
                    },
                    edit: function (id) {
                        this.status = "edit";
                        for (var i = 0; i < this.page.rows.length; i++) {
                            if (this.page.rows[i].Id == id) {
                                this.row = this.page.rows[i];
                                break;
                            }
                        }
                        if (this.log) {
                            console.log("invoke edit id:" + id);
                            console.log(this.row);
                        }
                        $("#editor").modal('show');
                    },
                    save: function () {
                        var paras = {}
                        $("#editor .p-field").each(function (i, e) {
                            var key = $(e).attr("alt");
                            var val = $(e).val();
                            paras[key] = val;
                        });

                        /// validation                                                
                        paras.action = this.status;
                        if (this.log) console.log("invoke save with paras：" + JSON.stringify(paras));
                        var req = url + "save";
                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
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
                    create: function () {
                        this.row = { PARAM_TYPE: "-1", PARAM_NAME: "-1", PARAM_VALUE: "-1" }
                        this.status = "new";
                    },
                    cleanup: function () {
                    },
                    jump: function (p) {
                        this.search($("#input-search").val(), p, this.page.size);
                    },
                    getNotis: function () {
                        var req = url + "getNotis";
                        var _this = this;
                        this.$http.post(req, null, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.notis = res.body.result;
                            }
                        });

                        var req2 = url + "getProps";
                        this.$http.post(req2, null, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.props = res.body.result;
                            }
                        });

                        var req3 = url + "getUsers"; //这里是获取后台用户
                        this.$http.post(req3, null, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.users = res.body.result;
                            }
                        });
                    },
                    search: function (criteria, pIdx, pSize) {
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
            changeNav("../Image/icon_MyStatment.png", "Mail Setup");
        });       
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="col-md-12 global_table_head global_color_navyblue">
        <p>Feedback Mail Setup</p>         
    </div>    
    <div class="global_color_lightblue" id="vue-dt">
        <!--修正-->
        <div class="row"></div>    
        <!--检索-->
        <div class="row">
            <div class="col-md-5">
             
            </div>
            <div class="col-md-3 col-md-offset-4">
                <div class="input-group">
                    <span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>
                    <input type="text" id="input-search" class="form-control" v-on:keyup="txtChange($event)" />                
                </div>                 
            </div>
        </div>
        <!--表格-->
        <div class="row">     
            <div class="col-md-12">
                <table class="table table-condensed table-hover table-striped global-bootgrid bootgrid-table">
                    <thead>
                        <tr>
                            <th class="hide">Id</th>                            
                            <th style="width:400px;">Param_Type</th>
                            <th>Param_Name</th>
                            <th>Param_Desc</th>
                            <th>Param_Value</th>                                                        
                            <th class="text-center" style="width:100px;">Operation</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in page.rows">
                            <td class="text-center hide">{{item.Id}}</td>                            
                            <td class="text-left"><a href="javascript:void(0)" v-on:click="edit(item.Id)">{{item.PARAM_TYPE}}</a></td>                            
                            <td class="text-center">{{item.PARAM_NAME}}</td>                            
                            <td class="text-center">{{item.PARAM_DESC}}</td>
                            <td class="text-center">{{item.PARAM_VALUE}}</td>                            
                            <td class="text-center" style="width:100px;"><a class="label label-danger" v-on:click="del(item.Id)">Delete</a></td>
                        </tr>
                    </tbody>
                </table>
            </div>             
        </div>
        <!--翻页-->
        <div class="row">  
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
        <!--模态窗口-->
        <div class="row">            
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
                                        <input type="number" class="form-control p-field" alt="Id" v-bind:value="row.Id" />
                                    </div>                                    
                                </div>                                
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Param Type:</label>
                                    <div class="col-sm-7">                                        
                                        <select class="form-control p-field" alt="Param_Type" v-bind:value="row.PARAM_TYPE">
                                            <option value="-1">--select--</option>
                                            <option v-for="i in notis" v-bind:value="i">{{i}}</option>
                                        </select>
                                    </div>                                                           
                                </div>
                                <div class="form-group">                                    
                                    <label class="col-sm-3 control-label">Param Name:</label>
                                    <div class="col-sm-7">
                                        <select class="form-control p-field" alt="Param_Name"  v-bind:value="row.PARAM_NAME">
                                            <option value="-1">--select--</option>
                                            <option value="DEFAULT">DEFAULT</option>
                                            <option v-for="j in props" v-bind:value="j.Property_Code">{{j.Property_Code}}</option>
                                        </select>
                                    </div>                                
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Description:</label>
                                    <div class="col-sm-7">
                                        <input type="text" class="form-control p-field" alt="Param_Desc"  v-bind:value="row.PARAM_DESC"/>
                                    </div>                                    
                                </div>  
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Param Value:</label>
                                    <div class="col-sm-7">
                                        <select class="form-control p-field" alt="Param_Value"  v-bind:value="row.PARAM_VALUE">
                                            <option value="-1">--select--</option>
                                            <option v-for="k in users" v-bind:value="k.Email">{{k.LoginName}} -> {{k.Email}}</option>
                                        </select>
                                    </div>                                    
                                </div>                                                               
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-default" data-dismiss="modal" v-on:click="cleanup">Close</button>                            
                            <button type="button" class="btn btn-primary" v-on:click="save">Save</button>
                        </div>
                    </div><!-- /.modal-content -->
                </div><!-- /.modal -->
            </div>
        </div>
    </div>
</asp:Content>