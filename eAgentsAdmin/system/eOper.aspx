<%@ Page Language="C#" MasterPageFile="~/SiteSystemBootcss.Master" AutoEventWireup="true" 
    CodeBehind="eOper.aspx.cs" Inherits="PropertyOneAppWeb.system.eOper" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/vue.js" type="text/javascript"></script>
    <script src="../js/vue-resource.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        var vm = null;
        $(document).ready(function () {
            changeNav("../Image/icon_MyStatment.png", "Process List Of Request Form");

            var dev = '<%=System.Configuration.ConfigurationManager.AppSettings["dev"]%>';
            var url = "eOper.aspx?act=";
            vm = new Vue({
                el: "#vue-dt",
                data: {
                    searching: false,
                    lang: '<% =Session["lang"] %>',
                    log: dev == "1" ? true : false,       //根据是否为测试环境来console输出。
                    row: {},        //当前编辑的行
                    page: {
                        rows: [],   //当前页条目数组
                        size: 10,   //每页条目数
                        count: 0,   //总页数
                        idx: 1,     //当前页
                        range: 5,   //可显示的页码范围
                        start: 1    //可显示的起始页码
                    },
                    misc: { //各种参数，用于界面传值
                        loginname: "",
                        groupname: ""
                    },
                    fromEmail: false,
                    status: "new"   //editor status. 'new' or 'edit'
                },
                filters: {
                    fmtDate: function (v) {     //“日期”过滤器                       
                        return df(v, "yyyy-mm-dd"); // lang=zh-CHS"中文 lang=en-US">English 
                    },
                    fmtDate2: function (v) {    //“日期”过滤器,转换为系统本地格式
                        if (v) {
                            var d = new Date(v.toString());
                            return d.toLocaleDateString();
                        } else {
                            return "";
                        }
                    },
                    fmtStatus: function (v) {
                        //Oper Status： R-Reject， P：Processing， F：Completed
                        if (v) {
                            if (v.indexOf('R') >= 0) {
                                return "Rejected";
                            } else if (v.indexOf('P') >= 0) {
                                return "Processing";
                            } else if (v.indexOf('F') >= 0) {
                                return "Completed";
                            } else if (v.indexOf('W') >= 0) {
                                return "Withdraw";
                            } else
                                return "N/A";
                        } else
                            return "";
                    }
                },
                methods: {
                    init: function () {
                        //修正row样式
                        $(".row").css("margin", "10px 5px 0px 5px");
                        //修正datepicker事件处理
                        $(".am-dp").datepicker({ format: 'yyyy-mm-dd' }).on('changeDate.datepicker.amui', function (event) {
                            event.target.value = event.target.defaultValue;
                        });

                        this.misc.loginname = $("#input-loginname-hidden").val();
                        this.misc.groupname = $("#input-groupname-hidden").val();

                        var operid = qs(window.location.href, 'id');
                        var sp = qs(window.location.href, 'sp');
                        //检索数据 all                        
                        if (qs(window.location.href, 'pidx') != "") {
                            this.search("", parseInt(qs(window.location.href, 'pidx')), this.page.size);
                        } else {
                            if (operid) {
                                this.search(operid, 1, this.page.size);
                            } else if (sp) {
                                this.search(sp, 1, this.page.size);
                                this.fromEmail = true;
                            } else {
                                this.search("", 1, this.page.size);
                            }
                        }
                    },
                    timeout: function () {
                        window.location.href = "../98.html";
                    },
                    search: function (c, i, s) { //c,i,s => criteria,pidx,psize 。（"s"参数有些多余，后期考虑去除）
                        var req = url + "search" + "&ts=" + Math.random();
                        var paras = {
                            searchParse: c,
                            pageIdx: isNaN(parseInt(i)) ? 1 : parseInt(i), //检查数字是否非法
                            pageSize: s
                        }

                        var _this = this;
                        try {
                            this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                                if (res.body.err_code != 0) {
                                    if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                        that.timeout();
                                    } else
                                        alert(res.body.err_msg);
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
                            }, function (err) {
                                console.log('request failure.');
                            });
                        } catch (e) {
                            console.log(e);
                        }
                    },
                    create: function () {

                    },
                    edit: function () {

                    },
                    del: function (id) {

                    },
                    jump: function (pidx) {
                        this.search($("#input-search").val(), pidx, this.page.size);
                    },
                    save: function () {

                    },
                    cleanup: function () {

                    },
                    reset: function () {
                        document.querySelector("#ul-eforms #li-eopers a").click();
                    },
                    txtChange: function (e) { // 用于检索（未完成）                    
                        /*                         
                        8:Backspace,
                        27:Escape,
                        16:Shift,
                        20:CapsLock,
                        18:Alt,
                        17:Control  
                        45:Insert, 
                        46:Delete,
                        35:End,
                        36:Home,
                        33:PageUp,
                        34:PageDown,
                        123:F12,
                        122:F11
                        */
                        var fkey = [27, 17, 20, 18, 16, 45, 46, 35, 36, 34, 33];
                        if (fkey.indexOf(e.keyCode) >= 0) {
                            return;
                        } else {
                            if (!this.searching) {
                                this.searching = true;
                                //primes(100000);
                                this.search($("#input-search").val(), this.page.idx, this.page.size);
                                this.searching = false;
                            }
                        }
                    }
                }
            });
            vm.init();
        });
    </script>     
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="col-md-12 global_table_head global_color_navyblue">
        <p>Process List Of Request Form</p>
    </div> 
    <div class="global_color_lightblue" id="vue-dt">
        <!--修正-->
        <div class="row"></div>

        <!--检索-->
        <div class="row">
            <div class="col-md-5">                         
            </div>
            <div class="col-md-4 text-right">
                <a class="btn btn-warning" v-if="fromEmail" v-on:click="reset">Reset Search</a>
            </div>
            <div class="col-md-3">                     
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
                            <th style="width:8%" class="text-left">Oper Id</th>                            
                            <th>Form Name</th>                            
                            <th>Form No</th>
                            <th>Tenant</th>
                            <th style="width:22%">Apply By/Date</th>       
                            <th style="width:10%">Status</th>
                            <th>Current Group</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in page.rows">
                            <td class="text-center ">{{item.Oper_Id}}</td>                                                        
                            <td class="text-center">{{item.Form_Name}}</td>                            
                            <td class="text-center">{{item.Oper_RefNo}}</td>                          
                            <td class="text-center">{{item.Tenant_Name}}</td>
                            <td class="text-center">{{item.Oper_CreateBy}}/{{item.Oper_CreateDate|fmtDate}}</td>          
                            <td class="text-center">{{item.Oper_Status|fmtStatus}}</td>
                            <td class="text-left">{{item.Flow_CurGroup}}</td>
                            <td class="text-center">
                                <a v-bind:href="'tpls4form/'+item.Form_Template+'?operid='+item.Oper_Id+'&formid='+item.Form_Id+'&loginname='+misc.loginname+'&groupname='+misc.groupname+'&refno='+item.Oper_RefNo+'&fwdeseq='+item.Fwde_Seq+'&pidx='+page.idx+'&lang='+lang">
                                    {{item.Operate}}
                                </a>
                            </td>
                        </tr>
                    </tbody>                    
                 </table>
            </div>
        </div>

        <!--翻页-->
        <div class="row form-grid">  
            <div class="col-md-3">               
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
    </div>
</asp:Content>
