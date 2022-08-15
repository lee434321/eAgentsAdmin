<%@ Page Language="C#" MasterPageFile="~/SiteSystemBootcss.Master" AutoEventWireup="true" 
CodeBehind="eFormTimeDef.aspx.cs" Inherits="PropertyOneAppWeb.system.eFormTimeDef" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/vue.js" type="text/javascript"></script>
    <script src="../js/vue-resource.min.js" type="text/javascript"></script>
    <script type="text/javascript">
         var vm = null;
         $(document).ready(function () {
             changeNav("../Image/icon_MyStatment.png", "Time Definition");

             var url = "eFormTimeDef.aspx?act=";
             vm = new Vue({
                 el: "#vue-dt",
                 data: {
                     row: {},
                     page: {
                         rows: [],   //当前页条目数组
                         size: 10,   //每页条目数
                         count: 0,   //总页数
                         idx: 1,     //当前页
                         range: 5,   //可显示的页码范围
                         start: 1    //可显示的起始页码
                     },
                     lpro: {},
                     typedBuff: "",
                     sts: 0,        //start timestamp
                     status: 0        //0=new;1=edit
                 },
                 watch: {
                     sts: function (v) {
                         setInterval(function () {
                             console.log(v);
                         }, 500);
                     }
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
                     search: function () {
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
                     create: function () {
                         this.status = 0;
                         this.row = {}
                         $("#editor-x table tbody tr").remove();
                         $(".form-grid").hide(500);
                         $("#editor-x").show(500);
                     },
                     del: function (id) {
                         var _this = this;
                         var req = url + "del";
                         var paras = { tmdf_id: id }
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
                     edit: function (id) {
                         this.status = 1;
                         //取主条目       
                         for (var i = 0; i < this.page.rows.length; i++) {
                             if (this.page.rows[i].tmdf_id == id) {
                                 this.row = this.page.rows[i];
                                 break;
                             }
                         }
                         //构建子表
                         $("#editor-x table tbody tr").remove();
                         for (var j = 0; j < this.row.detail.length; j++) {
                             this.buildSingleDtlRow(this.row.detail[j]);
                         }

                         $(".form-grid").hide(500);
                         $("#editor-x").show(500);
                     },
                     save: function () {
                         var paras = this.preparas();
                         console.log(paras);
                         var err_msg = this.validate(paras);
                         if (err_msg.length > 0) {
                             alert(err_msg);
                             return;
                         }
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
                         });
                     },
                     validate: function (paras) {
                         var err_msg = "";

                         //检查主表
                         if (!paras.tmdf_code || paras.tmdf_code == "") {
                             err_msg = "'Code' is required.";
                         } else if (!paras.tmdf_name) {
                             err_msg = "'Name' is required.";
                         } else if (true) {
                             var dtl = JSON.parse(paras.dtl);
                             if (!dtl || dtl.length == 0) {
                                 err_msg = "'detail' is required.";
                             } else {
                                 //检查明细
                                 for (var i = 0; i < dtl.length; i++) {
                                     if (dtl[i].tmper_code == "" || !dtl[i].tmper_code) {
                                         err_msg = "line " + (i + 1) + " 'code' in detail is required.";
                                         break;
                                     } else if (!dtl[i].tmper_from || !dtl[i].tmper_to) {
                                         err_msg = "line " + (i + 1) + " 'from' or 'to' is required.";
                                         break;
                                     } else {
                                         var f = Date.parse("2021-01-01 " + dtl[i].tmper_from);
                                         var t = Date.parse("2021-01-01 " + dtl[i].tmper_to);
                                         if (f >= t) {
                                             err_msg = "line " + (i + 1) + " 'from' need earlier than 'to'.";
                                             break;
                                         }
                                     }
                                 }
                             }
                         }
                         return err_msg;
                     },
                     preparas: function () { //准备参数 prepare paras
                         //收集子表参数                        
                         var subs = [];
                         $("#editor-x table tbody tr").each(function (e) { //这里有优化空间，不必每个控件取值，可以通过自动识别控件来赋值。暂时不处理。
                             var model = {}
                             model.tmper_id = $(this).find("td input[alt='tmper_id']").val();
                             model.tmper_seq = $(this).find("td input[alt='tmper_seq']").val();
                             model.tmper_code = $(this).find("td input[alt='tmper_code']").val();
                             model.tmper_mon = $(this).find("td input[alt='tmper_mon']")[0].checked ? "Y" : "N";
                             model.tmper_tue = $(this).find("td input[alt='tmper_tue']")[0].checked ? "Y" : "N";
                             model.tmper_wed = $(this).find("td input[alt='tmper_wed']")[0].checked ? "Y" : "N";
                             model.tmper_thu = $(this).find("td input[alt='tmper_thu']")[0].checked ? "Y" : "N";
                             model.tmper_fri = $(this).find("td input[alt='tmper_fri']")[0].checked ? "Y" : "N";
                             model.tmper_sat = $(this).find("td input[alt='tmper_sat']")[0].checked ? "Y" : "N";
                             model.tmper_sun = $(this).find("td input[alt='tmper_sun']")[0].checked ? "Y" : "N";
                             model.tmper_hld = $(this).find("td input[alt='tmper_hld']")[0].checked ? "Y" : "N";
                             model.tmper_from = $(this).find("td input[alt='tmper_from']").val();
                             model.tmper_to = $(this).find("td input[alt='tmper_to']").val();
                             subs.push(model);
                         });

                         var paras = {}
                         $("#editor-x .p-field").each(function (i, e) {
                             var key = $(e).attr("alt");
                             var val = $(e).val();
                             paras[key] = val;
                         });
                         paras.dtl = JSON.stringify(subs);
                         paras.action = this.status;
                         return paras;
                     },
                     approve: function () {
                         var _this = this;
                         var req = url + "approve";
                         var paras = { id: this.row.tmdf_id }
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
                     buildSingleDtlRow: function (r) { // append detail html element     
                         //开启一行
                         $tr = $("<tr draggable='true'></tr>");
                         //temp_id
                         if (r)
                             $tr.append($("<td class='hidden'><input type='text' class='form-control' alt='tmper_id' value='" + r.tmper_id + "'/></td>"));
                         else
                             $tr.append($("<td class='hidden'><input type='text' class='form-control' alt='tmper_id' value='0'/></td>"));
                         //seq
                         if (r)
                             $tr.append($("<td><input type='text' class='form-control' alt='tmper_seq' value='" + r.tmper_seq + "'/></td>"));
                         else
                             $tr.append($("<td><input type='text' class='form-control' alt='tmper_seq' value='0'/></td>"));
                         //code
                         if (r) {
                             $tr.append($("<td><input type='text' class='form-control' alt='tmper_code' value='" + r.tmper_code + "'/></td>"));
                         } else
                             $tr.append($("<td><input type='text' class='form-control' alt='tmper_code' /></td>"));
                         // mon
                         if (r) {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_mon' " + (r.tmper_mon == 'Y' ? 'checked="checked"' : '') + " /></td>"));
                         } else {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_mon' /></td>"));
                         }
                         // tue
                         if (r) {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_tue' " + (r.tmper_tue == 'Y' ? 'checked="checked"' : '') + " /></td>"));
                         } else {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_tue' /></td>"));
                         }
                         // wed
                         if (r) {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_wed' " + (r.tmper_wed == 'Y' ? 'checked="checked"' : '') + " /></td>"));
                         } else {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_wed' /></td>"));
                         }
                         // thu
                         if (r) {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_thu' " + (r.tmper_thu == 'Y' ? 'checked="checked"' : '') + " /></td>"));
                         } else {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_thu' /></td>"));
                         }
                         // fri
                         if (r) {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_fri' " + (r.tmper_fri == 'Y' ? 'checked="checked"' : '') + " /></td>"));
                         } else {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_fri' /></td>"));
                         }
                         // sat
                         if (r) {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_sat' " + (r.tmper_sat == 'Y' ? 'checked="checked"' : '') + " /></td>"));
                         } else {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_sat' /></td>"));
                         }
                         // sun
                         if (r) {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_sun' " + (r.tmper_sun == 'Y' ? 'checked="checked"' : '') + " /></td>"));
                         } else {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_sun' /></td>"));
                         }
                         // hld
                         if (r) {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_hld' " + (r.tmper_hld == 'Y' ? 'checked="checked"' : '') + " /></td>"));
                         } else {
                             $tr.append($("<td class='text-center'><input type='checkbox' class='form-check-input' alt='tmper_hld' /></td>"));
                         }
                         //from
                         if (r) {
                             $tr.append($("<td><input type='time' class='form-control' alt='tmper_from' value='" + r.tmper_from + "'/></td>"));
                         } else
                             $tr.append($("<td><input type='time' class='form-control' alt='tmper_from' value=''/></td>"));
                         //to
                         if (r) {
                             $tr.append($("<td><input type='time' class='form-control' alt='tmper_to' value='" + r.tmper_to + "'/></td>"));
                         } else
                             $tr.append($("<td><input type='time' class='form-control' alt='tmper_to' value=''/></td>"));
                         //oper.创建删除按钮
                         $ea = $("<a class='label label-danger'>del</a>");
                         $ea.on('click', function (e) {
                             $(e.currentTarget).parent().parent().remove(); // a->td->tr remove
                         });
                         $tr.append($("<td class='text-center'></td>").append($ea));
                         //插入行
                         $("#editor-x table tbody").append($tr);
                     },
                     cleanup: function () {
                         $("#editor-x").hide(500);
                         $(".form-grid").show(500);
                     },
                     onSearch: function () {
                         //this.sts = new Date().valueOf();
                         //console.log("sts:" + this.sts);
                     },
                     txtChange: function (e) {
                         //                         this.typedBuff = $(e.target).val()
                         //                         if (this.typedBuff.length == 1) {
                         //                             this.sts = new Date().valueOf();
                         //                             console.log("sts:" + this.sts);
                         //                         }

                         //var lastCharTs = new Date().valueOf();
                         //console.log(this.typedBuff + " length:" + this.typedBuff.length + " speed:" + (lastCharTs - this.sts) / this.typedBuff.length + "ms/pcs");

                         this.search(); // 后期最好做个触发控制,避免频繁查询
                     }
                 }
             });
         });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="col-md-12 global_table_head global_color_navyblue">
        <p>Time Definition</p>
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
                    <input type="text" id="input-search" class="form-control" v-on:keyup="txtChange($event)" v-on:focus="onSearch" />                    
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
                            <th>Code</th>
                            <th>Name</th>
                            <th>Desc</th>
                            <th>Active</th>
                            <th>Status</th>                    
                            <th class="text-center" style="width:100px;">Operation</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in page.rows">
                            <td class="text-center hide">{{item.tmdf_id}}</td>                            
                            <td class="text-center"><a href="javascript:void(0)" v-on:click="edit(item.tmdf_id)">{{item.tmdf_code}}</a></td>                            
                            <td class="text-center">{{item.tmdf_name}}</td>
                            <td class="text-center">{{item.tmdf_desc}}</td>
                            <td class="text-center">{{item.tmdf_active}}</td>   
                            <td class="text-center">{{item.tmdf_status}}</td>                            
                            <td class="text-center" style="width:100px;"><a class="label label-danger" v-on:click="del(item.tmdf_id)">Delete</a></td>
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
                                    <input type="number" class="form-control p-field" alt="tmdf_id" v-bind:value="row.tmdf_id" />
                                </div>                                    
                            </div>       
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Code:</label>
                                <div class="col-sm-2">
                                    <input type="text" class="form-control p-field" alt="tmdf_code" v-bind:value="row.tmdf_code" />
                                </div>    
                                <label class="col-sm-2 control-label">Name:</label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control p-field" alt="tmdf_name" v-bind:value="row.tmdf_name"/>
                                </div>                                
                            </div>
                            <div class="form-group">
                                 <label class="col-sm-2 control-label">Description:</label>
                                 <div class="col-sm-9">
                                    <input type="text" class="form-control p-field" alt="tmdf_desc" v-bind:value="row.tmdf_desc" />
                                </div>    
                            </div>
                            <div class="form-group" v-if="status=='1'">
                                <label class="col-sm-2 control-label">Create By:</label> 
                                <div class="col-sm-3">
                                    <span class="form-control" v-if="row.tmdf_createby!='' &&row.tmdf_createby!=null ">{{row.tmdf_createby}}/{{row.tmdf_createdate|fmtDate}}</span>
                                </div>
                                <label class="col-sm-2 control-label">Update By:</label> 
                                <div class="col-sm-3">
                                    <span class="form-control" v-if="row.tmdf_updateby!='' &&row.tmdf_updateby!=null ">{{row.tmdf_updateby}}/{{row.tmdf_updatedate|fmtDate}}</span>
                                </div>                                    
                            </div>    
                            <div class="form-group" v-if="status=='1' && row.tmdf_approveby!='' && row.tmdf_approveby!=null">
                                <label class="col-sm-2 control-label">Approved By:</label> 
                                <div class="col-sm-3">
                                    <span class="form-control" v-if="row.tmdf_approveby!='' &&row.tmdf_approveby!=null ">{{row.tmdf_approveby}}/{{row.tmdf_approvedate|fmtDate}}</span>
                                </div>
                            </div>
                            <div role="separator" class="divider">
                                <button class="btn btn-primary" v-on:click="buildSingleDtlRow(null)">Add</button>
                            </div>  
                            <table class="table">
                                <thead>
                                    <tr>                               
                                        <th class="hidden">tmper_id</th>             
                                        <th style="width:6%">Seq.</th>
                                        <th style="width:15%">Code</th>
                                        <th>MON</th>
                                        <th>TUE</th>
                                        <th>WED</th>
                                        <th>THU</th>
                                        <th>FRI</th>
                                        <th>SAT</th>
                                        <th>SUN</th>
                                        <th>HLD</th>
                                        <th style="width:10%">From</th>
                                        <th style="width:10%">To</th>
                                        <th>Oper.</th>
                                    </tr>
                                </thead>  
                                <tbody></tbody>
                            </table>       
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
