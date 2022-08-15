  <%@ Page Title="Turnover ftp" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="ftpa._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <!-- vue.js-->
    <script src="assets/vue.js" type="text/javascript"></script>
    <!-- vue ajax-->
    <script src="assets/vue-resource.min.js" type="text/javascript"></script>             
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 class="h2">文件查询</h1>        
    </div>

    <div id="vue-dt">
        <!--检索-->
        <div class="row form-grid">
            <div class="col-3">
                <div class="input-group mb-3">
                    <span class="input-group-text" id="Span1">年份</span>
                    <select class="form-select form-select-sm" id="select-year" v-on:change="onYearChange">
                      <option value="-1" disabled>-select-</option>
                      <option v-for="item in years" :value="item">{{item}}</option>
                    </select>                    
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
                            <th>#</th>                            
                            <th class="text-center">类型</th>
                            <th>名称 &nbsp;<span v-on:click="sort($event)" alt-sort="Name"><i class="bi bi-caret-up-square"></i></span></th>                                                        
                            <th class="text-center">接收时间</th>                            
                            <th>租约号 &nbsp;<span v-on:click="sort($event)" alt-sort="Lease_Number"><i class="bi bi-caret-up-square"></i></span></th>                            
                            <th>备注</th>
                            <th class="text-center">状态</th>
                            <th>商铺ID</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in page.rows">
                            <td>{{item.Id}}</td>
                            <td class="text-center">{{item.File_Type}}</td>
                            <td>{{item.Name}}</td>                            
                            <td class="text-center">{{item.Recv_Time|fmtDate}}</td>
                            <td>{{item.Lease_Number}}</td>
                            <td>{{item.Memo}}</td>                            
                            <td class="text-center">{{item.Status|trim}}</td>
                            <td>{{item.Shortcut_key}}</td>                           
                        </tr>
                    </tbody>
                </table>
            </div>             
        </div>

        <!--翻页-->
        <div class="row form-grid">              
            <div class="col-8">              
            </div>
            <div class="col-4" v-show="page.count>0">
                <ul class="pagination justify-content-end">
                    <li class="page-item" v-on:click="jump(page.idx-1)"><a class="page-link" href="javascript:void(0)">&laquo;</a></li>
                    <li class="page-item" v-for="i in page.count" v-bind:class="{active:i==page.idx}" v-if="i>=page.start && i<=page.start+page.range-1 && i<=page.count" v-on:click="jump(i)"><a class="page-link" href="javascript:void(0)">{{i}}</a></li>
                    <li class="page-item" v-on:click="jump(page.idx+1)"><a class="page-link" href="javascript:void(0)">&raquo;</a></li>
                </ul>                
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
                    sort_track: 0,
                    years: [],
                    start_date: "2020-01-01",   //（废弃）
                    end_date: "",               //（废弃）
                    page: {         //表格分页（关键数据）
                        rows: [],   //当前页条目数组
                        size: 10,   //每页条目数
                        count: 0,   //总页数
                        idx: 1,     //当前页
                        range: 5,   //可显示的页码范围
                        start: 1    //可显示的起始页码
                    }
                },
                filters: {                    
                    fmtDate: function (v) {
                        return df(v, "yyyy-mm-dd hh:mi:ss");
                    },
                    trim: function (v) {    //去掉前后的空格
                        if (v)
                            return v.replace(/(^\s*)|(\s*$)/g, "");
                        else
                            return "";
                    }
                },
                created: function () {
                    var d = new Date();

                    for (var i = d.getFullYear(); i > d.getFullYear() - 5; i--) {
                        this.years.push(i);
                    }

                    var that = this;
                    this.$nextTick(function () {
                        $("#select-year").val(d.getFullYear());
                        that.search();
                    });
                },
                methods: {
                    onYearChange: function (e) {
                        console.log($(e.target).val());
                        if ($(e.target).val() == "-1") {
                            alert("year is required.");
                        } else {
                            this.search();
                        }
                    },
                    sort: function (e) {
                        var $span = $(e.srcElement.parentElement);
                        if ($($span.find("i")).attr("class").indexOf("bi-caret-up-square") >= 0) {
                            $($span.find("i")).removeClass("bi-caret-up-square");
                            $($span.find("i")).addClass("bi-caret-down-square");
                        } else {
                            $($span.find("i")).removeClass("bi-caret-down-square");
                            $($span.find("i")).addClass("bi-caret-up-square");
                        }
                        $span.attr("alt-track", this.sort_track++);
                        this.page.idx = 1;
                        this.search();
                    },
                    jump: function (p) {    //“翻页”事件         
                        this.page.idx = p;
                        this.search();
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
                    preparas: function () {
                        var sortingStr = "";
                        $("tr span i").each(function () {
                            $span = $(this.parentElement);
                            sortingStr += $span.attr("alt-sort") + "|";
                            sortingStr += ($span.attr("alt-track") ? $span.attr("alt-track") : -1) + "|";
                            if ($(this).attr("class").indexOf("-up-") >= 0) {
                                sortingStr += "ASC"
                            } else {
                                sortingStr += "DESC";
                            }
                            sortingStr += ",";
                        });
                        sortingStr = sortingStr ? sortingStr.substr(0, sortingStr.length - 1) : "";

                        var paras = {
                            sorts: sortingStr,
                            year: $("#select-year").val(),
                            //常规检索
                            searchParse: $("#input-search").val(),
                            pageIdx: this.page.idx, //检查数字是否非法
                            pageSize: this.page.size
                        }
                        return paras;
                    },
                    search: function () { //检索数据，生成grid
                        var req = url + "search_files" + "&ts=" + Math.random();
                        var paras = this.preparas();
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

            // 初始化日期
        });
    </script>
</asp:Content>

