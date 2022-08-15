<%@ Page Language="C#" MasterPageFile="~/SiteSystemBootcss.Master" AutoEventWireup="true" 
 CodeBehind="eForms.aspx.cs" Inherits="PropertyOneAppWeb.system.eForms" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- vue.js-->
    <script src="../js/vue.min.js" type="text/javascript"></script>
    <!-- vue ajax-->
    <script src="../js/vue-resource.min.js" type="text/javascript"></script>       
    <!-- ue配置文件 -->
    <script type="text/javascript" src="../js/ueditor/ueditor.config.js"></script>
    <!-- ue编辑器源码文件 -->
    <script type="text/javascript" src="../js/ueditor/ueditor.all.js"></script>     

    <!-- main app -->
    <script type="text/javascript">              
        ///处理ueditor        
        function uep(t, y, v) { //t=target ,y=type, v=value
            var tt = tt || UE.getEditor(t);
            if (y == "r") {
                var c = tt.getContent();
                return c;
            } else if (y == "w") {
                tt.ready(function () {
                    this.setContent(v);
                });
            }
        }

        var uex = {
            inst: null,
            init: function (t) {
                inst = UE.getEditor(t);
            },
            set: function (v) {
                this.inst.ready(function () {
                    this.setContent(v);
                });
            }
        }

        var url = "eForms.aspx?act=";       
        var uh, uc, uf = null;  //定义ueditor变量
        var vm = null;          //定义vue变量
        $(document).ready(function () {
            var navi = wb(); //获取浏览器标识。
            if (navi == "Chrome" || navi == "FF") { //如果是Chrome,要在vue外部定义ueditor实例
                uh = UE.getEditor('ue-header');
                uc = UE.getEditor('ue-content');
                uf = UE.getEditor('ue-footer');
            }

            vm = new Vue({ //定义页面vue应用
                el: "#vue-dt",
                data: {
                    leases: {               //用于lease的弹窗选择
                        searching: false,   //“查询中”状态标识（避免重复查询，暂时不实现）
                        result: []
                    },
                    log: true,
                    demo: false, //true表示开发环境，false不是
                    props: [],
                    row: { FORM_PROPERTY: "", FORM_LEASE: "" }, //当前编辑的行
                    groupInfo: {},  //用户组信息
                    tpls: [],       //“模板”下拉框数据源(考虑废弃)
                    sels: [],
                    timeperiod: {   //“time period”下拉框数据源
                        selected: -1,
                        opts: []
                    },
                    flows: {        //“flow”下拉框数据源
                        selected: -1,
                        opts: []
                    },
                    tpls2: {        //“template”下拉框数据源
                        selected: -1,
                        opts: []
                    },
                    lpro: {
                        loginname: "",
                        groupname: ""
                    },
                    page: {
                        rows: [],   //当前页条目数组
                        size: 10,   //每页条目数
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
                        return df(v, "yyyy-mm-dd");
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
                        if (v && v.indexOf("I") >= 0) {
                            return "UnApproved";
                        } else if (v.indexOf("A") >= 0) {
                            return "Approved";
                        } else
                            return "";
                    },
                    trim: function (v) {    //去掉前后的空格
                        if (v)
                            return v.replace(/(^\s*)|(\s*$)/g, "");
                        else
                            return "";
                    }
                },
                created: function () {
                    this.load();
                    this.deco();
                    var fid = qs(window.location.href, "id");       //如果link含有querystring为id的情况，需要带入查询条件 
                    var fcode = qs(window.location.href, "code");   //如果link含有querystring为code的情况，需要带入查询条件 
                    if (fid != "0" && fid != "") {
                        this.search(fid, 1, this.page.size);        //检索数据,fid
                        this.approve_mode = true;
                    } else if (fcode != "") {
                        this.search(fcode, 1, this.page.size);      //检索数据 code
                        this.approve_mode = true;
                    } else {
                        this.search("", 1, this.page.size);         //检索数据 all
                        this.approve_mode = false;
                    }

                    //下拉框预存参数，避免刷新后绑定空值。
                    var that = this;
                    this.$nextTick(function () {
                        $("#editor-x select.p-field").off('click');
                        $("#editor-x select.p-field").on('click', function () {
                            if (that.status == "new") that.row = that.preparas();
                        });
                    });
                    //修正attachment输入框位置
                    $("#input-attach-url").css("position", "absolute");

                    this.lpro.loginname = $("#input-loginname-hidden").val();
                    this.lpro.groupname = $("#input-groupname-hidden").val();
                },
                methods: {
                    timeout: function () {
                        window.location.href = "../98.html";
                    },
                    load: function () { //加载初始数据
                        var that = this;
                        this.$http.post(url + "load", null, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    that.timeout();
                                } else
                                    alert("[load] with error:" + res.body.err_msg);
                            } else {
                                console.log(res.body);
                                that.demo = res.body.result.demo;
                                that.tpls = res.body.result.tpls;
                                that.groupInfo = res.body.result.grpinfo.group;
                                that.props = res.body.result.grpinfo.props;
                                that.tpls2.opts = res.body.result.tpls;
                                that.timeperiod.opts = res.body.result.etds;
                            }
                        }, function (err) {
                            alert(err);
                        });
                    },
                    genFlowList: function () {
                        var paras = {
                            loginname: $("#input-loginname-hidden").val(),
                            groupname: $("#input-groupname-hidden").val(),
                            propertylist: this.row.FORM_PROPERTY,
                            status: "A"
                        }
                        var that = this;
                        this.$http.post("ajax.ashx?act=get_eflow_by_backuser", paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    that.timeout();
                                } else
                                    alert("[genFlowList] with error:" + res.body.err_msg);
                            } else {
                                that.flows.opts = res.body.result;
                            }
                        }, function (err) {
                            alert(err);
                        });
                    },
                    deco: function () { //修饰界面
                        this.$nextTick(function () {
                            $(".row").css("margin", "10px 5px 0px 5px"); //修正row样式
                            $(".pagination").css("margin-top", "0px").css("margin-bottom", "0px"); //修正pagination样式                                      

                            $(".am-dp").datepicker({ format: 'yyyy-mm-dd' }).on('changeDate.datepicker.amui', function (event) {
                                event.target.value = event.target.defaultValue; //修正datepicker事件处理
                            });

                            $("#confirmDlg #btnNo").on('click', function () {
                                $("#confirmDlg #btnYes").off('click');
                            });
                        });
                    },
                    search: function (criteria, pIdx, pSize) { //检索数据，生成grid
                        var req = url + "search" + "&ts=" + Math.random();
                        var paras = {
                            searchParse: criteria,
                            pageIdx: isNaN(parseInt(pIdx)) ? 1 : parseInt(pIdx), //检查数字是否非法
                            pageSize: pSize
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
                    create: function () { //“创建”按钮事件
                        this.row = { FORM_PROPERTY: "", FORM_LEASE: ""}  //当前编辑的行                        
                        for (var i = 0; i < this.props.length; i++) {
                            this.props[i].Ticked = false;
                        }
                        this.status = "new";

                        $(".prop").each(function (i, e) {
                            $(e)[0].checked = false;
                        });

                        $("#input-attach-url").attr("rpath", "");
                        $("#input-attach-url").parent().find(".preview a").text("");
                        $("#logo").attr("src", "");
                        $("#pic").attr("src", "");

                        $(".form-grid").hide(500);
                        $("#editor-x").show(500);

                        /// ueditor初始值 (识别浏览器区别处理)
                        if (navi == "Chrome" || navi == "FF") {
                            uh.setContent("");
                            uc.setContent("");
                            uf.setContent("");
                        } else if (navi == "IE") {
                            uep('ue-header', 'w', '');
                            uep('ue-content', 'w', '');
                            uep('ue-footer', 'w', '');
                        }
                        this.genFlowList();
                        this.flows.selected = -1; //flow控件初始化     
                        this.tpls2.selected = -1; //template初始化
                    },
                    edit: function (id) {   //“编辑”按钮事件
                        this.status = "edit";
                        for (var i = 0; i < this.page.rows.length; i++) {
                            if (this.page.rows[i].FORM_ID == id) {
                                this.row = this.page.rows[i];
                                $(".preview a").text(this.page.rows[i].FORM_ATTACHNAME); //填充界面                                                                                  
                                break;
                            }
                        }
                        if (this.row.FLOW_ID)
                            this.flows.selected = this.row.FLOW_ID;
                        else
                            this.flows.selected = -1;

                        if (this.row.FMTB_ID) {
                            this.tpls2.selected = this.row.FMTB_ID;
                        } else
                            this.tpls2.selected = -1;

                        if (this.log) {
                            console.log("invoke edit id:" + id);
                            console.log(this.row);
                        }

                        /// 填充ueditor ,识别浏览器区别处理                        
                        if (navi == "Chrome" || navi == "FF") {
                            uh.setContent(this.row.FORM_HEADER_HTML);
                            uc.setContent(this.row.FORM_WAIST_HTML);
                            uf.setContent(this.row.FORM_FOOTER_HTML);
                        } else if (navi == "IE") {
                            uep('ue-header', 'w', this.row.FORM_HEADER_HTML);
                            uep('ue-content', 'w', this.row.FORM_WAIST_HTML);
                            uep('ue-footer', 'w', this.row.FORM_FOOTER_HTML);
                        }

                        $("#editor-x").removeClass('hide');
                        $(".form-grid").hide(500);
                        $("#editor-x").show(500);

                        this.genFlowList();
                    },
                    shrinkImg: function (e) {
                        var w = $(e.target).width();
                        var h = $(e.target).height();
                        if (w > 384 || h > 384) {
                            $(e.target).css("width", "384px").css("height", "384px");
                        } else {

                        }
                    },
                    txtSearchLease: function (e) { //“关键字”检索事件
                        var sp = e == "" ? "" : $(e.target).val();
                        var req = url + "search_lease";
                        var paras = { searchPharse: sp, props: this.row.FORM_PROPERTY }

                        var that = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    that.timeout();
                                } else
                                    alert("[txtSearchLease] with error:" + res.body.err_msg);
                            } else {
                                that.leases.result = res.body.result;
                                if (that.row.FORM_LEASE) {
                                    for (var i = 0; i < that.leases.result.length; i++) {
                                        if (that.row.FORM_LEASE.indexOf(that.leases.result[i].Lease_Number) >= 0) {
                                            that.leases.result[i].Ticked = true;
                                        } else {
                                            that.leases.result[i].Ticked = false;
                                        }
                                    }
                                }
                            }
                        }, function (err) {

                        });
                    },
                    popSearchLease: function (e) {
                        var that = this;
                        var w = $("#popWinLeases");
                        if (this.status == "new") this.row = this.preparas();

                        $("#input-leases").val("");
                        this.txtSearchLease("");
                        w.modal('show');
                        w.find(".modal-footer button").off('click'); //先取消一次事件绑定。
                        w.find(".modal-footer button").on('click', function (e) { // 绑定OK按钮事件
                            var ls = [];
                            w.find(".modal-body tbody input").each(function (i) {
                                if (this.checked) {
                                    ls.push(that.leases.result[i].Lease_Number);
                                }
                            });
                            that.row["FORM_LEASE"] = ls.toString();
                        });
                    },
                    popSearchProps: function (e) {
                        var that = this;
                        var w = $("#popWinProps");
                        if (this.status == "new") this.row = this.preparas();

                        if (this.row.FORM_PROPERTY) { //如果当前编辑条目有值
                            for (var i = 0; i < this.props.length; i++) { //props列表ticked赋值，用于弹出框的checkbox绑定
                                if (this.row.FORM_PROPERTY.indexOf(this.props[i].PropertyCode) >= 0) {
                                    this.props[i].Ticked = true;
                                } else {
                                    this.props[i].Ticked = false;
                                }
                            }
                        }

                        w.modal('show');

                        w.find(".modal-footer button").off('click'); //先取消一次事件绑定。
                        w.find(".modal-footer button").on('click', function (e) { // 绑定OK按钮事件
                            var propss = [];
                            w.find(".modal-body tbody input").each(function (i) {
                                if (this.checked) {
                                    propss.push(that.props[i].PropertyCode);
                                }
                            });
                            that.row["FORM_PROPERTY"] = propss.toString();
                        });
                    },
                    tick: function (id) { //考虑废弃
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
                    isTick: function (id) { //考虑废弃
                    },
                    tickAll: function () {  //考虑废弃
                    },
                    approve: function (id) {    //“审批”按钮事件
                        var req = url + "approve";
                        var paras = { ids: this.row.FORM_ID }
                        var _this = this;
                        this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    _this.timeout();
                                } else
                                    alert("[approve] with error:" + res.body.err_msg);
                            } else {
                                _this.search($("#input-search").val(), _this.page.idx, _this.page.size);
                                if (this.log) console.log("invoke approve id:" + JSON.stringify(paras));

                                var link = window.location.href;
                                var link = link.replace(window.location.search, "");
                                window.location = link;
                                _this.cleanup();
                            }
                        }, function (res) {
                            console.log('request failure. ' + JSON.stringify(res));
                            _this.cleanup();
                        });
                    },
                    del: function (id) {    //“删除”按钮事件
                        $("#confirmDlg").modal("show");
                        var _this = this;
                        $("#confirmDlg #btnYes").on('click', function (e) {
                            if (_this.log) console.log("invoke delete id:" + id);
                            var req = url + "del";
                            var paras = { id: id }
                            _this.$http.post(req, paras, { emulateJSON: true }).then(function (res) {
                                if (res.body.err_code != 0) {
                                    if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                        _this.timeout();
                                    } else
                                        alert("[del] with error:" + res.body.err_msg);
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
                    jump: function (p) {    //“翻页”事件
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
                        } else if (paras.FORM_LEASE) {
                            var patt = /(\w{4})(\d{5}(?!\w\d))/g; //租约号正则匹配
                            var result = patt.test(paras.FORM_LEASE);
                            if (!result) {
                                err_msg = "lease format error.";
                            } else {
                                //paras.FORM_LEASE = paras.FORM_LEASE.match(patt)[0];
                                ;
                            }
                        } else if (!paras.FORM_PROPERTY || paras.FORM_PROPERTY == "" || paras.FORM_PROPERTY==null) {
                            err_msg = "property is required";
                        } else if (paras.FLOW_ID == "-1" && paras.FMTB_ID == "-1" && !paras.FORM_ATTACHURL) {
                            err_msg = "attachment is required when both flow and template is unselected.";
                        }

                        if (err_msg.length > 0) {
                            alert(err_msg);
                            return false;
                        } else
                            return true;
                    },
                    preparas: function () { //准备参数
                        var paras = {}

                        //获取基本参数(收集"input"控件中含有"alt"属性的值)
                        $("#editor-x .p-field").each(function (i, e) {
                            var key = $(e).attr("alt");
                            var val = $(e).val();
                            //checkbox类型赋值
                            if (e.tagName == "INPUT" && e.type == "checkbox") {
                                paras[key] = e.checked ? 'Y' : 'N';
                            } else if (e.tagName == "SELECT") { //如果是select控件，可以取两个值，一个是选择的值，一个是显示的名称。（通常“select”的值作为id，option值作为name）
                                paras[key] = val; //select 赋值
                                //以下处理option值
                                key = $(e).attr("alt_opt");
                                if (key) { //如果没有找到alt_opt属性，说明不需要显示的名称值
                                    $(e).find("option").each(function (i, el) {
                                        if (el.selected) {
                                            val = el.innerText;
                                        }
                                    });
                                    paras[key] = val;
                                }
                            } else { //input控件其他类型的赋值
                                paras[key] = val;
                            }
                        });
                        //获取附件参数
                        paras["FORM_ATTACHURL"] = $("#input-attach-url").attr("rpath");
                        paras["FORM_ATTACHNAME"] = $("#input-attach-url").parent().find(".preview a").text();
                        //获取logo参数
                        if ($("#logo").attr("src")) {
                            var b64 = $("#logo").attr("src").replace("data:image/jpg;base64,", ""); //data:image/jpg;base64,
                            if (b64 != "" || b64 != null) {
                                paras["FORM_LOGO_Base64"] = b64;
                            }
                        }
                        //获取picture参数
                        if ($("#pic").attr("src")) {
                            var b64 = $("#pic").attr("src").replace("data:image/jpg;base64,", ""); //data:image/jpg;base64,
                            if (b64 != "" || b64 != null) {
                                paras["FORM_PICTURE_Base64"] = b64;
                            }
                        }
                        //新建时要给form_id默认值0，否则后台反序列化报错。
                        if (this.status == "new") paras.FORM_ID = 0;
                        //获取ueditor数据
                        if (navi == "Chrome" || navi == "FF") {
                            paras["FORM_HEADER_HTML"] = uh.getContent();
                            paras["FORM_WAIST_HTML"] = uc.getContent(); //
                            paras["FORM_FOOTER_HTML"] = uf.getContent();
                        } else if (navi == "IE") {
                            paras["FORM_HEADER_HTML"] = uep('ue-header', 'r');
                            paras["FORM_FOOTER_HTML"] = uep('ue-footer', 'r');
                            paras["FORM_WAIST_HTML"] = uep('ue-content', 'r');
                        }
                        //赋值编辑状态
                        paras.action = this.status;

                        return paras;
                    },
                    save: function (e) {
                        /// 准备参数
                        var paras = this.preparas();

                        /// validation                         
                        if (!this.validate(paras)) {
                            return;
                        }
                        if (this.log) console.log("invoke save with paras：" + JSON.stringify(paras));

                        e.target.disabled = true;
                        var req = url + "save";
                        var _this = this;
                        this.$http.post(req, JSON.stringify(paras), { emulateJSON: true }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    _this.timeout();
                                } else
                                    alert("[save] with error:" + res.body.err_msg);
                            } else {
                                this.cleanup();
                                _this.search($("#input-search").val(), _this.page.idx, _this.page.size);
                            }
                            e.target.disabled = false;
                        }, function (res) {
                            console.log('request failure.');
                            this.cleanup();
                            e.target.disabled = false;
                        });
                    },
                    cleanup: function () {
                        if (this.log) console.log("invoke cleanup.");
                        this.row = {}
                        $("#input-attach-url").val("");
                        $("#logo").attr("src", "").removeAttr("style");
                        $("#pic").attr("src", "").removeAttr("style");

                        $("#editor-x").hide(500);
                        $(".form-grid").show(500);
                    },
                    upload: function (event) { //上传文件或图片
                        if (this.log) console.log("invoke upload.");
                        var a = event.target.files[0];
                        if (a) {
                            $(".preview a").text('Uploading... please wait.');
                            var _this = this;
                            var formData = new FormData();
                            formData.append('file', a);
                            var req = url + "upload";
                            this.$http.post(req, formData, { headers: { 'Content-Type': 'multipart/form-data'} }).then(function (res) {
                                if (res.body.err_code != 0) {
                                    $(".preview a").text('');
                                    if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                        _this.timeout();
                                    } else
                                        alert("[upload] with error:" + res.body.err_msg);
                                } else {
                                    $(".preview a").text(res.body.result.name);
                                    $("#input-attach-url").attr("rpath", res.body.result.url);
                                }
                            }, function (res) {
                                console.log(res.body);
                                $(".preview a").text('');
                            });
                        } else {
                        }
                    },
                    getBase64Str: function (t) { //参数t表示目标img控件id                   
                        var a = event.target.files[0];
                        var that = this;
                        var formData = new FormData();
                        formData.append('file', a);
                        var req = url + "getbase64str";
                        this.$http.post(req, formData, {
                            headers: { 'Content-Type': 'multipart/form-data' }
                        }).then(function (res) {
                            if (res.body.err_code != 0) {
                                if (typeof (res.body.err_code) == "undefined" || res.body.err_code == -98) {
                                    that.timeout();
                                } else
                                    alert("[getbase64str] with error:" + res.body.err_msg);
                            } else {
                                var src = "data:image/jpg;base64,";
                                src += res.body.result;
                                $("#" + t).attr("src", src).css("width", "384px").css("height", "384px");

                                if (t == "logo") {
                                    that.row.FORM_LOGO_Base64 = res.body.result;
                                } else if (t == "pic") {
                                    that.row.FORM_PICTURE_Base64 = res.body.result;
                                }
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

                    },
                    save_as: function () {
                        this.status = "new";
                        this.row.FORM_ID = 0;
                        this.row.FORM_CODE += "_copy";
                        this.row.FORM_STATUS = "I";
                        this.row.FORM_APPROVEBY = "";
                        this.row.FORM_APPROVEDATE = "";
                    },
                    remove: function (e) {
                        if (e == 'logo') {
                            this.row.FORM_LOGO_Base64 = "";
                            $("#logo").css("width", "0px").css("height", "0px");
                        } else if (e == 'pic') {
                            this.row.FORM_PICTURE_Base64 = "";
                            $("#pic").css("width", "0px").css("height", "0px");
                        } else if (e == 'atch') {
                            $(".preview a").text("");
                            this.row.FORM_ATTACHNAME = "";
                            this.row.FORM_ATTACHURL = "";
                        }
                    },
                    preview: function (e) {
                        if (parseInt($("select[alt=FMTB_ID]").val()) > 0 && !(this.row.FORM_TEMPLATE.indexOf("None") >= 0)) {
                            var url = 'tpls4form/' + this.row.FORM_TEMPLATE + '?formid=' + this.row.FORM_ID + '&loginname=' + this.lpro.loginname + '&groupname=' + this.lpro.groupname + '&from=def';
                            window.open(url);
                        } else {
                            alert('Can not preview without a template, please save first with a tempalte selected.');
                        }
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
        <div class="row form-grid">
            <div class="col-md-5"></div>
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
                            <td class="text-center">{{item.FORM_STATUS|fmtStatus}}</td>
                            <td class="text-left"><a v-bind:href="'../'+item.FORM_ATTACHURL" target="_blank">{{item.FORM_ATTACHNAME}}</a></td>
                            <td class="text-center" style="width:100px;">
                                <a class="label label-danger" v-on:click="del(item.FORM_ID)">Delete</a>                                
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>             
        </div>

        <!--翻页-->
        <div class="row form-grid">  
            <div class="col-md-3">
                <button class="btn btn-primary" v-on:click="create">Create</button> 
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
                                    <input type="number" class="form-control p-field" alt="FORM_ID" v-bind:value="row.FORM_ID" />
                                </div>                                    
                            </div>                                
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Code:</label>
                                <div class="col-sm-3">
                                    <input type="text" class="form-control p-field" alt="FORM_CODE" v-bind:value="row.FORM_CODE" />
                                </div>    
                                <label class="col-sm-2 control-label">Name:</label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control p-field" alt="FORM_NAME"  v-bind:value="row.FORM_NAME"/>
                                </div>                                
                            </div>
                            <div class="form-group">                                
                                <label class="col-sm-2 control-label">Description:</label>
                                <div class="col-sm-9">
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
                                <div class="col-sm-3">
                                    <select class="form-control p-field" alt="FLOW_ID" v-model="flows.selected"  >
                                        <option value="-1">-select-</option>
                                        <option v-for="item in flows.opts" v-bind:value="item.Flow_Id">{{item.FLOW_NAME}}</option>                                        
                                    </select>
                                </div>
                                <label class="col-sm-2 control-label">Template:</label>
                                <div class="col-sm-3">                                        
                                    <%--<select class="form-control p-field" alt="FMTB_ID" v-bind:value="row.FMTB_ID">
                                        <option value="-1">-select-</option>
                                        <option v-for="item in tpls" :value="item.fmtb_id">{{item.form_template}}</option>                                       
                                    </select>--%>
                                    <select class="form-control p-field" alt="FMTB_ID" v-model="tpls2.selected">
                                        <option value="-1">-select-</option>
                                        <option v-for="item in tpls2.opts" :value="item.fmtb_id">{{item.form_template}}</option>                                       
                                    </select>
                                </div>                                    
                            </div>                           
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Attachment:</label>
                                <div class="col-sm-3">
                                    <label class="btn btn-info" for="input-attach-url">Upload (PDF, PNG, JPG)</label>&nbsp;<div v-if="row.FORM_ATTACHURL" class="btn btn-danger btn-sm" v-on:click="remove('atch')">del</div>
                                    <input type="file" alt="FORM_ATTACHURL" id="input-attach-url" v-bind:rpath="row.FORM_ATTACHURL" style="opacity: 0; float:right; z-index:-100;" v-on:change="upload($event)"  />
                                    <div class="preview" style="z-index:100;">
                                        <a target=_blank v-bind:href="'../'+row.FORM_ATTACHURL" style="margin-bottom: 0 !important; overflow:hidden">{{row.FORM_ATTACHNAME}}</a> <!-- No files currently selected for upload-->
                                    </div>
                                </div>                           
                                <label class="col-sm-2 control-label">Time period:</label>
                                <div class="col-sm-3">
                                    <select class="form-control p-field" alt="TMDF_ID" alt_opt="TMDF_CODE" >
                                        <option value="-1" selected>-select-</option>
                                        <option v-for="item in timeperiod.opts" v-bind:value="item.tmdf_id" :selected="item.tmdf_id==row.TMDF_ID">{{item.tmdf_code}}</option>                                        
                                    </select>
                                </div>
                            </div>                            
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Properties:</label>
                                <div class="col-sm-3">
                                    <div class="input-group">
                                        <input type="text" class="form-control p-field" alt="FORM_PROPERTY" placeholder="Search for..." :value="row.FORM_PROPERTY"  />
                                        <span class="input-group-btn">
                                            <button class="btn btn-success" type="button" v-on:click="popSearchProps">...</button>
                                        </span>
                                    </div>
                                </div>
                                <label class="col-sm-2 control-label">Lease Number:</label>
                                <div class="col-sm-3">
                                    <div class="input-group">
                                        <input type="text" class="form-control p-field" alt="FORM_LEASE" placeholder="Search for..." :value="row.FORM_LEASE"  />
                                        <span class="input-group-btn">
                                            <button class="btn btn-success" type="button" v-on:click="popSearchLease">...</button>
                                        </span>
                                    </div>                                    
                                </div>                    
                            </div>                            
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Logo:</label>
                                <div class="col-sm-9">
                                    <label class="btn btn-info" for="input-image">Upload (PNG, JPG)</label>&nbsp;<div v-if="row.FORM_LOGO_Base64" class="btn btn-danger btn-sm" v-on:click="remove('logo')">del</div>
                                    <input type="file" alt="FORM_LOGO" id="input-image" style="opacity: 0; float:right;" v-on:change="getBase64Str('logo')"  /> <br />                                   
                                    <img class="preview" id="logo" v-bind:src="row.FORM_LOGO_Base64?'data:image/jpg;base64,'+row.FORM_LOGO_Base64:''" v-on:load="shrinkImg" />
                                    
                                </div>                              
                            </div>
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Picture:</label>
                                <div class="col-sm-9">
                                    <label class="btn btn-info" for="input-pic">Upload (PNG, JPG)</label> &nbsp;<div v-if="row.FORM_PICTURE_Base64" class="btn btn-danger btn-sm" v-on:click="remove('pic')">del</div>
                                    <input type="file" alt="FORM_PICTURE" id="input-pic" style="opacity: 0; float:right;" v-on:change="getBase64Str('pic')"  /> <br />                                   
                                    <img class="preview" id="pic" v-bind:src="(row.FORM_PICTURE_Base64?'data:image/jpg;base64,'+row.FORM_PICTURE_Base64:'')" v-on:load="shrinkImg" />
                                </div>
                            </div>
                            <div class="form-group" v-bind:class="{ hide: !demo }">
                                <label class="col-sm-2 control-label">Header:</label>
                                <div class="col-sm-9">                                    
                                    <!-- 加载编辑器的容器 -->   
                                    <%--<div id="ue-header" name="header"></div>--%>
                                    <div id="ue-header" name="content"></div>
                                </div>
                            </div>
                            <div class="form-group" v-bind:class="{ hide: !demo }">
                                <label class="col-sm-2 control-label">Content:</label>
                                <div class="col-sm-9 text-left">
                                    <div id="ue-content" name="content"></div>
                                </div>
                            </div>
                            <div class="form-group" v-bind:class="{ hide: !demo }">
                                <label class="col-sm-2 control-label">Footer:</label>
                                <div class="col-sm-9">                                    
                                    <div id="ue-footer" name="footer"></div>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-2 control-label">Agreement:</label> 
                                <div class="col-sm-2">
                                    <input class="form-check-input position-static p-field" type="checkbox" alt="FORM_AGREEMENT"  :checked="row.FORM_AGREEMENT=='Y'?true:false" style=" margin-top:1.5rem;" />
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
                                <div class="col-sm-3">
                                    <span class="form-control">{{row.FORM_APPROVEBY}}/{{row.FORM_APPROVEDATE|fmtDate}}</span>
                                </div>
                            </div>      
                                                                                
                        </div>
                    </div>
                    <div class="panel-footer text-right">
                        <%--<a class="btn btn-success" v-if="row.FORM_TEMPLATE!=null && row.FORM_ID " target="_blank" v-bind:href="'tpls4form/'+row.FORM_TEMPLATE+'?formid='+row.FORM_ID+'&loginname='+lpro.loginname+'&groupname='+lpro.groupname+'&from=def'" v-on:click="preview">Preview</a>--%>
                        <a class="btn btn-success" v-if="row.FORM_TEMPLATE!=null && row.FORM_ID" v-on:click="preview">Preview</a>
                        <button type="button" class="btn btn-default" data-dismiss="modal" v-on:click="cleanup">Close</button>
                        <button type="button" class="btn btn-warning" v-on:click="approve" v-if="groupInfo !=null && groupInfo.Approver=='1'" :disabled="row.FORM_STATUS?row.FORM_STATUS.indexOf('A')>=0:false">Approve</button>
                        <button type="button" class="btn btn-primary" v-on:click="save($event)">Save</button>
                        <button type="button" class="btn btn-primary" v-on:click="save_as()" v-if="row.FORM_STATUS?row.FORM_STATUS.indexOf('A')>=0:false">Save As</button>
                    </div>
                </div>                
            </div>
        </div>

        <!-- 模态窗口 “lease”选择 -->
        <div class="row">            
            <div class="modal fade" id="popWinLeases" tabindex="-1" role="dialog"  aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" >&times;</button>
                            <h4 class="modal-title">Select Leases</h4>
                        </div>
                        <div class="modal-body">
                            <div class="input-group col-md-6">
                                <span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>
                                <input type="text"  id="input-leases" class="form-control" v-on:keyup="txtSearchLease" />                    
                            </div> 
                            <br />
                            <div >
                                <div>
                                    <table class="table table-bordered small" style="width:100%; margin-bottom:0">
                                        <colgroup>
                                            <col style="width: 80px;" />
                                            <col />
                                        </colgroup>
                                        <thead>
                                            <tr>
                                                <th class="text-right" style="width:10%"></th>
                                                <th class="text-center" style="width:25%">Lease Number</th>
                                                <th class="text-center" style="width:25%">Customer No.</th>
                                                <th class="text-left">Customer Name</th>
                                            </tr>
                                        </thead> 
                                    </table>
                                </div>
                                <div  style="height:450px; overflow:scroll;">
                                    <table class="table table-bordered small" style="width:100%">
                                         <colgroup>                                 
                                            <col style="width: 80px;" />
                                            <col />
                                        </colgroup> 
                                        <tbody>
                                            <tr v-for="item in leases.result">
                                                <td class="text-right" style="width:10%"><input type="checkbox" class='form-check-input' v-model="item.Ticked" /></td>
                                                <td class="text-center" style="width:25%">{{item.Lease_Number}}</td>
                                                <td class="text-center" style="width:25%">{{item.Customer_Number}} </td>
                                                <td class="text-left">{{item.Customer_Name}}</td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </div>                           
                        </div>
                        <div class="modal-footer"> 
                            <button type="button" class="btn btn-default" data-dismiss="modal">OK</button>                    
                        </div>
                    </div><!-- /.modal-content -->
                </div><!-- /.modal -->
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
                        <div class="modal-body"> 
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
                                        <tr v-for="item in props">
                                            <td class="text-right"><input type="checkbox" class='form-check-input form-check-inpt' v-model="item.Ticked" /></td>
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
