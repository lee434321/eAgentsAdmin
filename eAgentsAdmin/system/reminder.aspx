<%@ Page Language="C#" MasterPageFile="~/SiteSystemBootcss.Master" AutoEventWireup="true"
CodeBehind="reminder.aspx.cs" Inherits="PropertyOneAppWeb.system.reminder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/vue.js" type="text/javascript"></script>
    <script src="../js/vue-resource.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        var url = "reminder.aspx?act=";
        var vue = null;
        $(document).ready(function () {
            vue = new Vue({ //定义vue应用
                el: "#vue-dt",
                data: {
                    log: true,
                    lang: '<% =Session["lang"] %>',
                    page: {
                        rows: [],   //当前页条目数组
                        size: 5,    //每页条目数
                        count: 0,   //总页数
                        idx: 1,     //当前页
                        range: 5,   //可显示的页码范围
                        start: 1    //可显示的起始页码
                    },
                    groupInfo: {},
                    props: [],
                    row: {},
                    status: "new"
                },
                filters: {
                    fmtDate: function (v) {     //“日期”过滤器
                        if (this.vue && this.vue.lang == "en-US") {
                            return df(v, "yyyy-mm-dd");
                        } else {
                            return df(v, "yyyy-mm-dd");
                        }
                    }
                },
                methods: {
                    init: function () {
                        this.getGroup();
                        this.search("", 1, this.page.size); //检索数据
                        $(".row").css("margin", "10px 5px 0px 5px"); //修正row样式
                        $(".pagination").css("margin-top", "0px").css("margin-bottom", "0px"); //修正pagination样式     

                        if (this.lang == "en-US") {
                            $(".am-dp").datepicker({ format: 'dd-mm-yyyy' }).on('changeDate.datepicker.amui', function (event) {//修正datepicker事件处理
                                event.target.value = event.target.defaultValue;
                            });
                        } else {
                            $(".am-dp").datepicker({ format: 'yyyy-mm-dd' }).on('changeDate.datepicker.amui', function (event) {//修正datepicker事件处理
                                event.target.value = event.target.defaultValue;
                            });
                        }
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
                    txtChange: function (e) {
                        var p = $(e.target).val();
                        if (p.length == 0) {
                            this.search("", 1, this.page.size);
                        }

                        if (p.length >= 3) {
                            this.search(p, 1, this.page.size);
                        }
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
                                        _this.page.start = q * _this.page.size - 1;
                                    } else if (r == 1) {
                                        _this.page.start = q * _this.page.size;
                                    } else
                                        _this.page.start = q * _this.page.size + 1;

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
                        this.cleanup();
                        this.status = "new";
                    },
                    edit: function (id) {
                        if (this.log) console.log("invoke edit [" + id + "]");
                        this.status = "edit";

                        for (var i = 0; i < this.page.rows.length; i++) {
                            if (this.page.rows[i].Id == id) {
                                this.row = this.page.rows[i];
                                break;
                            }
                        }
                        if (this.log) console.log(this.row);

                        var _this = this;
                        if (this.row.Properties) {
                            $(".prop").each(function (i, e) {
                                if (_this.row.Properties.indexOf($(e).parent().text().replace(/\s/g, "")) >= 0) {
                                    e.checked = true;
                                } else
                                    e.checked = false;
                            });
                        }

                        $("#editor").modal('show');
                    },
                    save: function () {
                        var that = this;
                        var paras = {}
                        $("#editor .p-field").each(function (i, e) {
                            var key = $(e).attr("alt");
                            if (e.type == "checkbox") {
                                paras[key] = e.checked;
                            } else {
                                if (key == "Date" && that.lang == "en-US") {
                                    paras[key] = dmyr($(e).val());
                                } else {
                                    paras[key] = $(e).val();
                                }
                            }
                        });
                        var props = "";
                        $(".prop").each(function (i, e) {
                            if ($(e)[0].checked) {
                                props += $(e).parent()[0].innerText + ",";
                            }
                        });
                        paras.Props = props.substr(0, props.length - 1);

                        paras.action = this.status;
                        if (this.log) console.log("invoke save with paras：" + JSON.stringify(paras));
                        var req = url + "save";
                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                _this.search($("#input-search").val(), _this.page.idx, _this.page.size);
                                alert("save success!");
                            }
                        }, function (res) {
                            console.log('request failure.');
                        });
                        this.cleanup();
                        $("#editor").modal('hide');
                    },
                    del: function (id) {
                        if (this.log) console.log("invoke delete id:" + id);
                        var req = url + "del";
                        var paras = { id: id }
                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                alert(res.body.err_msg);
                            } else {
                                alert("Delete success.");
                                _this.search($("#input-search").val(), _this.page.idx, _this.page.size);
                            }
                        }, function (err) {
                            console.log('request failure.');
                        });
                    },
                    cleanup: function () {
                        this.row = {};                        
                        $(".prop").each(function (i, e) {
                            e.checked = false;
                        });
                    }
                }
            });
            vue.init();

            changeNav("../Image/icon_MyStatment.png", "Reminder");
        });        
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="col-md-12 global_table_head global_color_navyblue">
        <p>Reminder Setting</p>         
    </div>
    <div  class="global_color_lightblue" id="vue-dt">
        <!--修正-->
        <div class="row"></div> 
        <!--检索-->
        <div class="row">
            <div class="col-md-5">              
            </div>
            <div class="col-md-3 col-md-offset-4">
                <div class="input-group">
                    <span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>
                    <input type="text" id="input-search" class="form-control" v-on:keyup="txtChange($event)">                    
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
                            <th>Date</th>
                            <th class="text-center">IsMail</th>
                            <th>Edit</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in page.rows"> 
                            <td class="hide">{{item.Id}}</td>
                            <td class="text-center"><a href="javascript:void(0)" v-on:click="edit(item.Id)">{{item.Rmd_Date|fmtDate}}</a></td>                                        
                            <td class="text-center">{{item.Is_Email}}</td>
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
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" v-on:click="cleanup">&times;</button>
                            <h4 class="modal-title">Editor</h4>
                        </div>
                        <div class="modal-body">
                            <div class="form-horizontal">
                                <div class="form-group" style="display:none;">
                                    <label class="col-sm-3 control-label">Id:</label>
                                    <div class="col-sm-3">
                                        <input type="number" class="form-control p-field" alt="Id" v-bind:value="row.Id" />
                                    </div>                                    
                                </div>  
                                <div class="form-group">
                                    <label class="col-sm-2 control-label" for="datepicker-date">Date:</label>
                                    <div class="col-sm-3">
                                        <input type="text" class="am-dp form-control p-field" id="datepicker-date" alt="Date" v-bind:value="row.Rmd_Date|fmtDate"/>
                                    </div>         
                                    <label class="col-sm-2 control-label" for="input-isEmail">Is Email:</label>
                                    <div class="col-sm-2">
                                        <input type="checkbox" class="form-control p-field" id="input-isEmail" alt="Is_Email" v-bind:checked="row.Is_Email=='Y'">                           
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-2 control-label">Properties:</label>       
                                    <div class="col-sm-9">
                                        <ul class="list-group">
                                            <li class="list-group-item">
                                                <p style="margin-bottom:0px" class="hide">
                                                    <label>
                                                        <input type="checkbox" v-on:click="selectAll($event)" />All
                                                    </label>
                                                </p>                                          
                                                <label class="checkbox-inline" v-for="item in props">
                                                    <input type="checkbox" class="prop"/>{{item.PropertyCode}}
                                                </label> 
                                            </li>
                                        </ul>                                    
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