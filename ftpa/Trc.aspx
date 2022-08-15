<%@ Page Title="Turnover Setting" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Trc.aspx.cs" Inherits="ftpa.Trc" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <!-- vue.js-->
    <script src="assets/vue.js" type="text/javascript"></script>
    <!-- vue ajax-->
    <script src="assets/vue-resource.min.js" type="text/javascript"></script> 
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 class="h2">提成设定</h1>        
    </div>  

   <div id="vue-dt">
        <!--检索-->
        <div class="row form-grid">
            <div class="col-3">
                <div class="input-group mb-3" hidden>
                    <span class="input-group-text" id="Span1">开始日</span>                    
                    <input type="date" class="form-control form-control-sm" id="input-start-date" v-model="start_date"/>     
                </div>                  
            </div>
            <div class="col-3">
                <div class="input-group mb-3" hidden>
                    <span class="input-group-text" id="Span2">结束日</span>                    
                    <input type="date" class="form-control form-control-sm" id="input-end-date" v-model="end_date"/>     
                </div>       
            </div>
            <div class="col-2">
                <div class="input-group mb-3" hidden>
                    <button type="button" class="btn btn-primary">Refresh</button>
                </div>
            </div>
            <div class="col-4">
                <div class="input-group mb-3">
                    <span class="input-group-text" id="basic-addon1">
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-search" viewBox="0 0 16 16">
                            <path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"/>
                        </svg>
                    </span>                    
                    <input type="text" id="input-search" class="form-control" placeholder="Search"  v-on:keyup="txtChange($event)" />     
                </div>
            </div>            
        </div>

        <!--表格-->
        <div class="row form-grid">     
            <div class="col-md-12">
                <table class="table table-sm table-condensed table-hover table-striped global-bootgrid bootgrid-table">
                    <thead>
                        <tr>
                            <th>店铺ID</th>                            
                            <th>租约号</th>
                            <th>PROD_ID</th>
                            <th class="text-center">TR_FLAG</th>
                            <th class="text-center">开始日期</th>
                            <th class="text-center">结束日期</th>
                            <th>客户编号</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in page.rows">
                            <td><a href="javascript:void(0)" v-on:click="edit(item)">{{item.Shortcut_Key}}</a></td>
                            <td>{{item.Lease_Number}}</td>
                            <td>{{item.Prod_Id}}</td>                            
                            <td class="text-center">{{item.Tr_Flag}}</td>
                            <td class="text-center">{{item.Lease_Term_From}}</td>
                            <td class="text-center">{{item.Lease_Term_To}}</td>
                            <td>{{item.Customer_Number}}</td>
                        </tr>
                    </tbody>
                </table>
            </div>             
        </div>

        <!--翻页-->
        <div class="row form-grid">              
            <div class="col-8">     
                 <button type="button" class="btn btn-sm btn-primary" v-on:click="create($event)">Create</button>
            </div>
            <div class="col-4" v-show="page.count>0">
                <ul class="pagination justify-content-end">
                    <li class="page-item" v-on:click="jump(page.idx-1)"><a class="page-link" href="javascript:void(0)">&laquo;</a></li>
                    <li class="page-item" v-for="i in page.count" v-bind:class="{active:i==page.idx}" v-if="i>=page.start && i<=page.start+page.range-1 && i<=page.count" v-on:click="jump(i)"><a class="page-link" href="javascript:void(0)">{{i}}</a></li>
                    <li class="page-item" v-on:click="jump(page.idx+1)"><a class="page-link" href="javascript:void(0)">&raquo;</a></li>
                </ul>                
            </div>
        </div>

         <!-- 编辑区域（一览时隐藏） -->
        <div class="row" id="editor-x" style="display:none;">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        Editor
                    </div>
                    <div class="card-body">                       
                        <div class="row mb-3">
                            <label for="input-shop-id" class="col-2 col-form-label">店铺ID:</label>
                            <div class="col-4">
                              <input type="text" class="form-control" id="input-shop-id" :value="row.Shortcut_Key" :readonly="status==1?true:false"/>
                            </div>

                            <label for="input-lease-number" class="col-2 col-form-label">租约号:</label>
                            <div class="col-4">
                              <input type="text" class="form-control" id="input-lease-number" :value="row.Lease_Number" :readonly="status==1?true:false" />
                            </div>
                        </div>
                        <div class="row mb-3">
                            <label for="input-prod-id" class="col-2 col-form-label">商品ID:</label>
                            <div class="col-4">
                              <input type="text" class="form-control" id="input-prod-id" :value="row.Prod_Id" />
                            </div>
                            <label for="input-tr-flag" class="col-2 col-form-label">提成标识:</label>
                            <div class="col-4">
                              <input type="text" class="form-control" id="input-tr-flag" :value="row.Tr_Flag"/>
                            </div>
                        </div>
                     </div>
                    <div class="card-footer text-end">
                        <button type="button" class="btn btn-sm btn-secondary" data-dismiss="modal" v-on:click="cleanup">Close</button>
                        <button type="button" class="btn btn-sm btn-primary" v-on:click="save($event)">Save</button>                        
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript">
        var url = "engine.ashx?act=";
        var vm = null;
        $(document).ready(function () {
            vm = new Vue({
                el: "#vue-dt",
                data: {
                    start_date: "2020-01-01",
                    end_date: "",
                    status: 0, //0=新建；1=编辑
                    row: {},    //正在编辑的行（只有一条记录）
                    page: {
                        rows: [],   //当前页条目数组
                        size: 10,   //每页条目数
                        count: 0,   //总页数
                        idx: 1,     //当前页
                        range: 5,   //可显示的页码范围
                        start: 1    //可显示的起始页码
                    }
                },
                filters: {
                    trim: function (v) {    //去掉前后的空格
                        if (v)
                            return v.replace(/(^\s*)|(\s*$)/g, "");
                        else
                            return "";
                    }
                },
                created: function () {
                    //var d = new Date();
                    //this.start_date = d.getFullYear().toString() + "-" + (d.getMonth() + 1) + "-" + "01";
                    //this.end_date = "2021-10-31";

                    this.search("", 1, this.page.size);
                },
                methods: {
                    cleanup: function () {
                        $("#editor-x").hide(500);
                        $(".form-grid").show(500);
                    },
                    preparas: function () {
                        var paras = {
                            shopId: $("#input-shop-id").val(),
                            leaseNumber: $("#input-lease-number").val(),
                            prodId: $("#input-prod-id").val(),
                            trFlag: $("#input-tr-flag").val(),
                            status: this.status
                        }
                        return paras;
                    },
                    save: function (e) {
                        console.log(this.preparas());
                        this.cleanup();
                    },
                    edit: function (item) {
                        this.row = item;
                        this.status = 1;
                        $("#editor-x").removeClass('hide');
                        $(".form-grid").hide(500);
                        $("#editor-x").show(500);
                    },
                    create: function () {
                        this.status = 0;
                        this.row = {};
                        $("#editor-x").removeClass('hide');
                        $(".form-grid").hide(500);
                        $("#editor-x").show(500);
                    },
                    jump: function (p) {    //“翻页”事件                      
                        this.search($("#input-search").val(), (p ? p : ""), this.page.size);
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
                    search: function (criteria, pIdx, pSize) { //检索数据，生成grid
                        var req = url + "search_trc" + "&ts=" + Math.random();
                        var paras = {
                            startDate: this.start_date,
                            endDate: this.end_date,
                            searchParse: criteria ? criteria : "",
                            pageIdx: isNaN(parseInt(pIdx)) ? 1 : parseInt(pIdx), //检查数字是否非法
                            pageSize: pSize
                        }

                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    //_this.timeout();
                                } else
                                    alert("[search] with error:" + res.body.err_msg);
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
                    }
                }
            });
        });
    </script>
</asp:Content>
