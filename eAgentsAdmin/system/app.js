function ThreadModel(id, pid, title, content, userid,username) {
    this.id = id;
    this.pid = pid;
    this.title = title;
    this.content = content;
    this.userid = userid;
    this.username = username;
    this.time = new Date().toLocaleString()  ;
}

function Httpost(url, param, dtype, ctype, callback) {
    $.ajax({
        url: url,
        data: param,
        method: 'POST',
        dataType: dtype,
        success: callback,
        error: callback
    });
}

/*
pending:
1. 实际url返回值处理
2. New，Edit处理
2.1 input 控件：new时，读取schema,
2.2 select控件：new时，预置select值，由后台提供
2.3 result含有所有字段，schema中只需要一个元素来识别
3. tick 事件处理
4. cellstyle 处理优化，根据schema来判断
5. 处理排序
6. 处理检索
*/
Vue.component('v-datable', {
    data: function () {
        return {
            cid: "",    // component id
            sels: [],   // selected rows
            schema: [], // schema for editor
            pidx: 0,    // current page index
            pgs: 0,     // total count of pages
            page: [],   // current page content
            size: 10,   // page size
            dts: [],    // data source
            pis: 5,     // pis=>page icons for display
            left: 0     // icon left limit
        }
    },
    computed: {
        disablePrev: function () {
            return this.pidx == 0 ? "disabled" : "";
        },
        disableNext: function () {
            return this.pidx == this.pgs - 1 ? "disabled" : "";
        }
    },
    filters: { // +		Aid.getTime(1584635328000)	{3/20/2020 12:28:48 AM}	System.DateTime
        filter2date: function (val, type) {
            try {
                if (type == "DateTime") {
                    var d = Common.parseRawDate(val);
                    if (d) {
                        return d.toLocaleString();
                    } else {
                        return val;
                    }
                } else
                    return val;

            } catch (e) {
            }
        },
        fillCell: function (val, type) {
            if (type == "DateTime") {
                var d = Common.parseRawDate(val);
                if (d) {
                    return d.toLocaleString();
                } else {
                    return val;
                }
            } else
                return val;
        }
    },
    mounted: function () {
        $(this.$el).attr("id", "v" + Math.round(Math.random() * 137)); //赋值控件id
        this.cid = $(this.$el).attr("id");

        //css reboot
        $("#" + this.cid + " .pagination").css("width", "fit-content").css("width", "-moz-fit-content"); //width:fit-content,-moz-fit-content;

        var _data = this.$data; //赋值外部data引用
        axios.get(this.url, { params: { action: "getAcc"} }).then(function (r) {
            if (r.data.err_code != 0) {
                alert(r.data.err_msg);
                return;
            }
            //获得数据源
            _data.dts = r.data.result;
            _data.schema = r.data.schema;
            //获得页数
            var r = _data.dts.length % _data.size;
            var q = Math.floor(_data.dts.length / _data.size);
            _data.pgs = r == 0 ? q : q + 1;
            //第一页
            _data.pidx = 0;
            _data.left = 1;
            for (var i = 0; i < _data.size; i++) {
                var idx = _data.pidx * _data.size + i;
                if (idx < _data.dts.length) {
                    _data.page.push(_data.dts[idx]);
                } else
                    break;
            }
        });

        ///modal窗口事件绑定
        $("#" + this.cid).find(".modal").on('shown.bs.modal', function (e) {
            //取schema
            //console.log(e.relatedTarget);
        });
    },
    props: ["url"],
    template: '<div>' +
            ' <div>' +
            '   <button class="btn btn-primary" data-toggle="modal" v-bind:data-target="\'#\'+cid+\'modal\'">New</button>' +
            '   <button class="btn btn-primary" data-toggle="modal" v-bind:data-target="\'#\'+cid+\'modal\'">Edit</button>' +
            '   <div class="modal" tabindex="-1" v-bind:id="cid+\'modal\'" role="dialog" aria-labelledby="vmodal" aria-hidden="true">' +
            '     <div class="modal-dialog" role="document">' +
            '       <div class="modal-content">' +
            '         <div class="modal-header">' +
            '           <h5 class="modal-title">Editor</h5>' +
            '             <button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
            '               <span aria-hidden="true">&times;</span>' +
            '             </button>' +
            '         </div>' +
            '         <div class="modal-body">' +
            '           <div class="mx-auto" style="width:80%">' +
            '             <div class="form-group row" v-for="item in schema">' +
            '               <label v-bind:for="\'input-\'+item.field" class="col-sm-3 col-form-label">{{item.field}}</label>' +
            '               <div class="col-sm-9 my-auto">' +
            '                 <select v-if="item.vals.length>0" v-bind:id="\'input-\'+item.field"><option v-for=" x2 in item.vals" v-bind:value="x2.key">{{x2.value}}</option></select>' +
            '                 <input v-else v-bind:type="ctype(item.type)" class="form-control" v-bind:id="\'input-\'+item.field" v-bind:placeholder="item.field">' +
            '               </div>' +
            '             </div>' +
            '           </div>' +
            '         </div>' +
            '         <div class="modal-footer">' +
            '           <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>' +
            '           <button type="button" class="btn btn-primary" v-on:click="save" >Save changes</button>' +
            '         </div>' +
            '       </div>' +
            '     </div>' +
            '   </div>' +
            ' </div>' +
            ' <table class="table table-bordered" v-on:click=tick>' +
            '     <thead>' +
            '      <tr><th class="text-center" v-for="item in schema">{{item.field}}</th></tr>' +
            '     </thead>' +
            '     <tbody>' +
            '     <tr v-for="(i,x) in page" v-bind:class="isTicked(pidx*size+x)"><td v-for="j in schema" v-bind:class="cellStyle(i[j.field])">{{i[j.field] | fillCell(j.type)}}</td></tr> ' +
            '     </tbody>' +
            ' </table>' +
            ' <div>' +
            '   <span class="py-2" style="float:left" v-show="this.sels.length>0">You selected {{this.sels.length}} row(s) </span>' +
            '   <ul class="pagination" style="margin-left:auto;margin-right:0;">' +
            '     <li class="page-item" v-bind:class="disablePrev"><a class="page-link" href="#" v-on:click="jump(pidx-1)">Prev</a></li>' +
            '     <li class="page-item" v-for="i in pgs" v-show="i>=left && i<=(left+pis-1)"><a class="page-link" href="#" v-on:click="jump(i-1)">{{i}}</a></li>' +
            '     <li class="page-item" v-bind:class="disableNext"><a class="page-link" href="#" v-on:click="jump(pidx+1)">Next</a></li>' +
            '  </ul>' +
            ' </div>' +
            '</div>',
    methods: {
        load: function () {

        },
        search: function () {

        },
        jump: function (ix) {
            if (ix >= 0 && ix < this.pgs) {
                this.page.length = 0;
                for (var i = 0; i < this.size; i++) {
                    var idx = this.size * ix + i;
                    if (idx < this.dts.length) {
                        this.page.push(this.dts[idx]);
                    }
                }
                this.pidx = ix;
            }

            var q = Math.floor((this.pidx + 1) / this.pis);
            if (q == 0) {
                this.left = 1;
            } else if (this.pgs - this.pidx < this.pis) {
                this.left = this.pgs - this.pis + 1;
            } else if (this.pidx + 1 - this.left + 1 >= this.pis) {
                this.left = this.pidx + 1 - Math.floor(this.pis / 2);
            } else if (this.pidx + 1 <= this.left) {
                this.left = this.pidx + 1 - this.pis;
            }
        },
        cellStyle: function (o) {
            if (typeof (o) == "number") {
                return "text-right";
            } else {
                return "text-left";
            }
        },
        isTicked: function (ix) {
            for (var i = 0; i < this.sels.length; i++) {
                if (this.sels[i] == ix) {
                    return "table-primary";
                }
            }
            return "";
        },
        ctype: function (t) { //后台数据类型转换为html前台input控件类型
            switch (t) {
                case "int":
                case "decimal":
                    return "number";
                default:
                    return "text";
            }
        },
        tick: function (event) {
            //当前tick的元素
            var $e = $(event.target);
            //处理选中的行
            if ($e.parent().parent()[0].tagName == "TBODY") { //如果是tbody内的元素               
                //更新已选数组                
                var x = this.pidx * this.size + $e.parent()[0].rowIndex - 1; //在dts数组中的索引（从0开始）                
                var r = $.inArray(x, this.sels);
                if (r < 0) {
                    this.sels.push(x);
                } else {
                    var ta = []; //temp array
                    for (var j = 0; j < this.sels.length; j++) {
                        if (this.sels[j] != x) {
                            ta.push(this.sels[j]);
                        }
                    }
                    this.sels = ta;
                }
            }
        },
        save: function () {
            $("#" + this.cid + "modal").modal('hide');
            console.log("invoke save");
        }
    }
});

var vm2 = new Vue({
    el: "#vu-det",
    data: {
        threadsInDb: [],
        threadsInChat: [],
        threadTicked: {},
        backendUsers: [],
        user: {}
    },
    computed: {
        chatLine: function (a, b) {
            return "123";
        },
        chat2: function (a, b) {
            return "ccc";
        }
    },
    filters: {
        escapeRN: function (val) {
            console.log(typeof (val));
            console.log(val);
            if (typeof (val) == "string") {
                return val;
            } else
                return "";
        }
    },
    methods: {
        httpost: function (url, paras, naive, callback) { // naive => false：直接使用json的String型式;true:转换为form型式
            if (naive) {
                this.$http.post(url, paras, { emulateJSON: true }).then(callback, function (err) {
                    alert(err.status);
                });
            } else {
                this.$http.post(url, paras).then(callback, function (err) {
                    alert(err.status);
                });
            }
        },
        init: function () {
            this.user = { LoginName: "CKCT00349F", UserId: 1, Role: "F" }
            this.search();
            var paras = { action: "loadBeu" }
            this.httpost('Threads.aspx', paras, true, function (rsp) {
                if (rsp.body.err_code == 0) {
                    vm2.$data.backendUsers = rsp.body.result;
                } else {
                    alert(rsp.body.err_msg);
                }
            });
        },
        search: function () {
            var paras = { action: "load" }
            this.httpost('Threads.aspx', paras, true, function (rsp) {
                if (rsp.body.err_code == 0) {
                    vm2.$data.threadsInDb = rsp.body.result;
                } else {
                    alert(rsp.err_msg);
                }
            });
        },
        build: function () {
            var paras = { action: "build", title: $("#input-user-title").val(), content: $("#text-user-content").val(), type: 0, userid: this.user.UserId }
            this.httpost('Threads.aspx', paras, true, function (rsp) {
                if (rsp.body.err_code == 0) {
                    console.log(rsp.body.result);
                } else {
                    alert(rsp.body.err_msg);
                }
            });
        },
        send: function () {
            var paras = { action: "send", title: this.threadTicked.Title + " [Append]", content: $("#text-user-content").val(), type: this.threadTicked.Type, userid: this.user.UserId, pid: this.threadTicked.Id }
            this.httpost('Threads.aspx', paras, true, function (rsp) {
                if (rsp.body.err_code == 0) {
                    console.log(rsp.body.result);
                    if (rsp.body.result == true) {

                    }
                } else {
                    alert(rsp.body.err_msg);
                }
            });
        },
        reply: function () {
            var paras = { action: "reply", title: this.threadTicked.Title + " [Reply]", content: $("#text-sysUser-Content").val(), type: this.threadTicked.Type, userid: parseInt($("#select-User-Content").val()), pid: this.threadTicked.Id }
            this.httpost('Threads.aspx', paras, true, function (rsp) {
                if (rsp.body.err_code == 0) {
                    console.log(rsp.body.result);
                } else {
                    alert(rsp.body.err_msg);
                }
            });
        },
        tick: function (item) {
            this.threadTicked = item;
            this.threadsInChat.length = 0;
            var paras = { action: "tick", pid: item.Pid, id: item.Id }
            this.httpost('Threads.aspx', paras, true, function (rsp) {
                if (rsp.body.err_code == 0) {
                    vm2.$data.threadsInChat = rsp.body.result;
                    this.$nextTick(function () { // 这里的nextTick会在dom更新后再次渲染。                                                
                        //如下操作会去取更新后的元素高度，如果不使用nextTick，则取不到正确的值，因为没有重新渲染dom。
                        var $scrollContent = $("#scrollContent");
                        var $scrollWrapper = $("#scrollContent").parent();
                        var contentHeight = parseInt($scrollContent.css("height").replace("px", ""));
                        var wrapperHeight = parseInt($scrollWrapper.css("height").replace("px", ""));
                        if (contentHeight > wrapperHeight) {
                            $scrollWrapper.scrollTop(contentHeight - wrapperHeight + parseInt($(".card-body").css("padding-bottom")));
                        }
                    });
                } else {
                    alert(rsp.body.err_msg);
                }
            });
        },
        isBackend: function (loginName) {
            for (var i = 0; i < this.backendUsers.length; i++) {
                if (this.backendUsers[i].LoginName == loginName) {
                    return true;
                }
            }
            return false;
        },
        chatExchange: function (a, b) {
            if (this.isBackend(a)) {
                return b + " : " + a;
            } else
                return a + " : " + b;
        }
    }
});

$(document).ready(function () {
    console.log("DOM ready");
    //vm2.init();
});

/*
var vm = new Vue({
    el: "#vu-det1",
    data: {
        threadsInDb: [],
        threadChatsInDb: [],

        threads: [],      //所有主题会话列表
        threadChats: [],  //当前对话列表。如果有值，第一行表示主题。
        currentUserId: 0,
        custs: [{ CustId: 1, LoginName: "CKCT00349F", Role: "F" },
        { CustId: 2, LoginName: "CKCT00349M", Role: "M"}],

        users: [{ UserId: 258, LoginName: "kin" },
            { UserId: 198, LoginName: "alee" },
            { UserId: 238, LoginName: "derek"}]
    },
    filters: {
    },
    methods: {
        loadAll: function () { //加载所有会话
            this.$http.post('Threads.aspx', { action: "loadAll" }, { emulateJSON: true }).then(function (rsp) {
                if (rsp.body.err_code == 0) {
                    this.threadsInDb = rsp.body.result;
                } else {
                    alert(rsp.err_msg);
                }
            }, function (res) {
                console.log("error");
                console.log(res.status);
            });
        },
        onTick: function (id) { //点击主题事件
            this.threadChats.length = 0;
            var th = this.getThread(parseInt(id));   //检索主题               
            this.threadChats.push(th);               //加入当前会话第一行
            var ths = this.getThreads(parseInt(th.Id), null);   //检索所有与主题相关的会话
            this.threadChats = this.threadChats.concat(ths);    //追加到当前会话

            $("#selectCustId").val(this.threadChats[0].User_Id);
            this.$nextTick(function () { //这里的nextTick会在dom更新后再次渲染。
                // 如下操作会去取更新后的元素高度，如果不使用nextTick，则取不到正确的值，因为没有重新渲染dom。
                var $scrollContent = $("#scrollContent");
                var $scrollWrapper = $("#scrollContent").parent();
                var contentHeight = parseInt($scrollContent.css("height").replace("px", ""));
                var wrapperHeight = parseInt($scrollWrapper.css("height").replace("px", ""));
                if (contentHeight > wrapperHeight) {
                    $scrollWrapper.scrollTop(contentHeight - wrapperHeight);
                }
            });
        },
        getThread: function (id) { //后期要改为从数据库里获取
            for (var i = 0; i < this.threadsInDb.length; i++) {
                if (this.threadsInDb[i].Id == id) {
                    return this.threadsInDb[i];
                }
            }
            return null;
        },
        getThreads: function (pid, callback) { //后期要改为从数据库里获取。callback用于从数据库获取数据后的回调处理，目前暂时不用，直接传null.
            var arr = [];
            for (var i = 0; i < this.threadsInDb.length; i++) {
                if (this.threadsInDb[i].Pid == pid) {
                    arr.push(this.threadsInDb[i]);
                }
            }

            if (callback)
                callback(arr);
            else
                return arr;
        },
        invokeReply: function () { // 回复
            if (this.threadChats.length <= 0) return;   //如果没有主题或任何会话
            if ($("#selectReplyUserId") == "-1") return;

            this.currentUserId = parseInt($("#selectReplyUserId").val());

            var ety = new Object();
            ety.Pid = this.threadChats[0].Id;
            ety.Content = $("#replyContent").val();
            ety.User_Id = parseInt($("#selectReplyUserId").val());
            ety.Create_By = this.getUser(ety.User_Id).LoginName;
            ety.Title = "reply [" + this.threadChats[0].Title + "] with " + this.getRandomInt();
            ety.action = "replyThread";

            this.$http.post('Threads.aspx', ety, { emulateJSON: true }).then(function (rsp) {
                if (rsp.body.err_code != 0) {
                    alert(rsp.body.err_msg);
                } else {
                    this.threadsInDb.push(ety);
                    this.threadChats.push(ety);
                    this.$nextTick(function () { // 这里的nextTick会在dom更新后再次渲染。
                        // 如下操作会去取更新后的元素高度，如果不使用nextTick，则取不到正确的值，因为没有重新渲染dom。                               
                        var contentHeight = parseInt($("#scrollContent").css("height").replace("px", ""));
                        var wrapperHeight = parseInt($("#scrollContent").parent().css("height").replace("px", ""));
                        if (contentHeight > wrapperHeight) {
                            $("#scrollContent").parent().scrollTop(contentHeight - wrapperHeight);
                        }
                    });
                }
            }, function (res) {
                console.log("error");
                console.log(res.status);
            });
        },
        invokeSend: function () { // 模拟前台发送消息，实际后台并不使用
            if ($("#selectCustId").val() == "-1") {
                alert("please select customer user");
                return;
            }

            var ety = new Object();
            ety.Pid = this.threadChats[0].Id;
            ety.Content = $("#inputContent").val();
            ety.User_Id = parseInt($("#selectCustId").val()); // 这里是custid                   
            ety.Create_By = this.getCust(ety.User_Id).LoginName;
            ety.Title = this.threadChats[0].Title + " [Append] ";
            ety.action = "sendThread";

            this.$http.post('Threads.aspx', ety, { emulateJSON: true }).then(function (rsp) {
                if (rsp.body.err_code != 0) {
                    alert(rsp.body.err_msg);
                } else {
                    this.threadsInDb.push(ety);
                    this.threadChats.push(ety);
                    this.$nextTick(function () { // 这里的nextTick会在dom更新后再次渲染。
                        // 如下操作会去取更新后的元素高度，如果不使用nextTick，则取不到正确的值，因为没有重新渲染dom。                               
                        var contentHeight = parseInt($("#scrollContent").css("height").replace("px", ""));
                        var wrapperHeight = parseInt($("#scrollContent").parent().css("height").replace("px", ""));
                        if (contentHeight > wrapperHeight) {
                            $("#scrollContent").parent().scrollTop(contentHeight - wrapperHeight);
                        }
                    });
                }
            }, function (res) {
                console.log("error");
                console.log(res.status);
            });
        },
        invokeCreate: function () { //创建一条新主题会话
            if ($("#selectCustId").val() == "-1") {
                alert("please select an user");
                return;
            }

            this.threadChats.length = 0; //清空当前会话数据；
            var pid = 0;
            var title = "Title of new id:" + id;
            var content = $("#inputContent").val();
            var userid = parseInt($("#selectCustId").val());
            var ety = new ThreadModel(id, pid, title, content, userid, this.getCust(userid).LoginName);

            ety["action"] = "createThread";
            this.$http.post('Threads.aspx', ety, { emulateJSON: true }).then(function (rsp) {
                if (rsp.body.err_code != 0) {
                    alert(rsp.body.err_msg);
                } else {

                }
            }, function (res) {
                console.log("error");
                console.log(res.status);
            });
        },
        createThread: function () {
            if ($("#selectCustId").val() == "-1") {
                alert("please select an user");
                return;
            }

            this.threadChats.length = 0; //清空当前会话数据；
            var id = this.getRandomInt();
            var pid = 0;
            var title = "Title of new id:" + id;
            var content = $("#inputContent").val();
            var userid = parseInt($("#selectCustId").val());
            var ety = new ThreadModel(id, pid, title, content, userid, this.getCust(userid).LoginName);

            this.threads.push(ety);
            this.threadChats.push(ety);
            ety["action"] = "createThread";

            this.$http.post('Threads.aspx', ety, { emulateJSON: true }).then(function (rsp) {
                if (rsp.body.err_code != 0) {
                    alert(rsp.body.err_msg);
                } else {

                }
            }, function (res) {
                console.log("error");
                console.log(res.status);
            });
        },
        getCust: function (id) {
            for (var i = 0; i < this.custs.length; i++) {
                if (this.custs[i].CustId == id) {
                    return this.custs[i];
                }
            }
            return null;
        },
        getUser: function (id) {
            for (var i = 0; i < this.users.length; i++) {
                if (this.users[i].UserId == id) {
                    return this.users[i];
                }
            }
            return null;
        },
        getRandomInt: function () {
            return Math.floor(new Date().valueOf() % 10000 * Math.random());
        },
        init: function () { //初始化操作
            var id = this.getRandomInt();
            var title = "Cras justo odio";
            var content = "Cras justo odio with content";
            var ety = new ThreadModel(id, 0, title, content, 1, this.getCust(1).LoginName);
            this.threads.push(ety);

            var id = this.getRandomInt();
            title = "Dapibus ac facilisis in";
            content = "Dapibus ac facilisis in with content";
            var ety = new ThreadModel(id, 0, title, content, 2, this.getCust(2).LoginName);
            this.threads.push(ety);

            var id = this.getRandomInt();
            title = "Porta ac consectetur ac";
            content = "Porta ac consectetur ac with content";
            var ety = new ThreadModel(id, 0, title, content, 2, this.getCust(2).LoginName);
            this.threads.push(ety);

            $("#selectReplyUserId").val(this.users[1].UserId);
            this.currentUserId = parseInt($("#selectReplyUserId").val());

            $("#selectReplyUserId").on("change", this.$data, function (e) {
                e.data.currentUserId = 112;
                console.log(e);
            });
        },
        send: function () {
            var id = this.getRandomInt();
            var pid = $("#inputThreadId").val();
            var title = "Title" + id;
            var content = $("#inputContent").val();
            var userid = parseInt($("#inputUserId").val());
            var ety = new ThreadModel(id, pid, title, content, userid, this.getCust(userid).LoginName);
            this.threads.push(ety);
            this.threadChats.push(ety);
            this.$nextTick(function () { // 这里的nextTick会在dom更新后再次渲染。
                // 如下操作会去取更新后的元素高度，如果不使用nextTick，则取不到正确的值，因为没有重新渲染dom。
                var $scrollContent = $("#scrollContent");
                var $scrollWrapper = $("#scrollContent").parent();
                var contentHeight = parseInt($scrollContent.css("height").replace("px", ""));
                var wrapperHeight = parseInt($scrollWrapper.css("height").replace("px", ""));
                if (contentHeight > wrapperHeight) {
                    $scrollWrapper.scrollTop(contentHeight - wrapperHeight);
                }
            });
        },
        onThreadTick: function (id) {
            this.threadChats.length = 0;            //清除数组

            var a = this.searchById(parseInt(id));  //检索主题
            this.threadChats.push(a);               //加入当前会话第一行
            var arr = this.searchByPid(parseInt(a.id));         //检索所有与主题相关的会话
            this.threadChats = this.threadChats.concat(arr);    //追加到当前会话

            $("#selectCustId").val(this.threadChats[0].userid);
            this.$nextTick(function () { //这里的nextTick会在dom更新后再次渲染。
                // 如下操作会去取更新后的元素高度，如果不使用nextTick，则取不到正确的值，因为没有重新渲染dom。
                var $scrollContent = $("#scrollContent");
                var $scrollWrapper = $("#scrollContent").parent();
                var contentHeight = parseInt($scrollContent.css("height").replace("px", ""));
                var wrapperHeight = parseInt($scrollWrapper.css("height").replace("px", ""));
                if (contentHeight > wrapperHeight) {
                    $scrollWrapper.scrollTop(contentHeight - wrapperHeight);
                }
            });
            // scroll to the end                    
        },
        searchByPid: function (pid) {
            var arr = [];
            for (var i = 0; i < this.threads.length; i++) {
                if (this.threads[i].pid == pid) {
                    arr.push(this.threads[i]);
                }
            }
            return arr;
        },
        searchById: function (id) {

            for (var i = 0; i < this.threads.length; i++) {
                if (this.threads[i].id == id) {
                    return this.threads[i];
                }
            }
            return null;
        },
        searchRootThread: function (id) {
            for (var i = 0; i < this.threads.length; i++) {
                var r = this.threads[i]
                if (r.id == id && r.pid == 0) {
                    return r;
                }
            }
        },
        reply: function () {
            if (this.threadChats.length <= 0) return;
            if ($("#selectReplyUserId") == "-1") return;
            this.currentUserId = parseInt($("#selectReplyUserId").val());
            var id = this.getRandomInt();
            var pid = this.threadChats[0].id;
            var title = "re" + this.threadChats[0].title;
            var content = $("#replyContent").val();
            var userid = parseInt($("#selectReplyUserId").val());
            var ety = new ThreadModel(id, pid, title, content, userid, this.getUser(userid).LoginName);

            this.threads.push(ety);
            this.threadChats.push(ety);

            this.$nextTick(function () { // 这里的nextTick会在dom更新后再次渲染。
                // 如下操作会去取更新后的元素高度，如果不使用nextTick，则取不到正确的值，因为没有重新渲染dom。
                var $scrollContent = $("#scrollContent");
                var $scrollWrapper = $("#scrollContent").parent();
                var contentHeight = parseInt($scrollContent.css("height").replace("px", ""));
                var wrapperHeight = parseInt($scrollWrapper.css("height").replace("px", ""));
                if (contentHeight > wrapperHeight) {
                    $scrollWrapper.scrollTop(contentHeight - wrapperHeight);
                }
            });
        },
        gateway: function () {   
            var i31 = { from: "sender",
                timestamp: "2019-12-09 15:07:17",
                nonce: "48", sign: "x1dAZJz6XTv54b4UPEBBeoT+gKg=",
                data: { username: "HFT100174M",
                    password: "123456",
                    logintype: "C"
                }
            }
            var i32 = { from: "forgetpsw",
                timestamp: "2019-12-09 15:07:17",
                nonce: "32", sign: "x1dAZJz6XTv54b4UPEBBeoT+gKg=",
                data: { userid: "HFT100174M",
                    logintype: "C"
                }
            }
            var i34 = { from: "getonlinepayinfo",
                timestamp: "2019-12-09 15:07:17",
                nonce: "32", sign: "x1dAZJz6XTv54b4UPEBBeoT+gKg=",
                data: { leasenum: "HFT100174"
                }
            }

            var i37 = {
                from: "sender",
                timestamp: "2019-12-10 16:5:55",
                nonce: "71",
                sign: "1212ed13edweqd32r2124r1234f",
                data: {
                    leasenum: "HFT100219",
                    startdate: "2018-06-12",
                    statementnum: "S000336518"
                }
            }

            var i38 = {
                from: "getfeedback",
                timestamp: "2019-12-10 16:5:55",
                nonce: "38",
                sign: "1212ed13edweqd32r2124r1234f",
                data: {
                    leasenum: "HFT100174M",
                    startdate: "2018-12-09",
                    enddate: "",
                    type: "",
                    status: "",
                    feedbackid: ""
                }
            }
            Httpost("../gateway.ashx?act=getstatement", JSON.stringify(i37), "json", "application/json", function (rsp) {
                console.log(rsp);
            });
        }
    }
});
*/

