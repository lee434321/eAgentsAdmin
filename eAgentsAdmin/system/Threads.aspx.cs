using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using PropertyOneAppWeb.Commom;
namespace PropertyOneAppWeb.system
{
   

    /// <summary>
    /// Thread Class
    /// </summary>
    public partial class Threads : PageBase
    {
        private class PushNotification
        {
            public string[] registrationIds { get; set; }
            public string title { get; set; }
            public string message { get; set; }
        }
        JsonRsp rsp = null;  

        protected void Page_Load(object sender, EventArgs e)
        {
            string result = "";
            string action = string.IsNullOrEmpty(Request.Form["action"]) ? Request.QueryString["action"] : Request.Form["action"];
            if (action != null && action != "")
            {
                try
                {
                    if (action == "search")
                    {

                    }
                    else if (action == "searchRoot")
                    {
                        this.SearchRoot();
                    }
                    else if (action == "loadBeu") //load backend users
                    {
                        this.LoadBeu();
                    }
                    else if (action == "tick")  //tick a thread
                    {
                        this.Tick();
                    }
                    else if (action == "load")    //load main threads(pid=0)
                    {
                        this.Loads();
                    }
                    else if (action == "build") //build a new thread
                    {
                        this.Build();
                    }
                    else if (action == "reply")
                    {
                        this.Reply();
                    }
                    else if (action == "send") //send a comment to the thread
                    {
                        this.Send();
                    }
                    else if (action == "loadAll")
                    {
                        LoadAll();
                    }
                    else if (action == "createThread")
                    {
                        this.CreateThread();
                    }
                    else if (action == "replyThread")
                    {
                        ReplyThread();
                    }
                    else if (action == "sendThread")//前台客户再次发送feedback时调用，后台不使用
                    {
                        SendThread();
                    }
                    else if (action=="getAcc")
                    {
                        OrthRsp r = this.GetSysLoginAccount();
                        result = Newtonsoft.Json.JsonConvert.SerializeObject(r);
                    }
                    else
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "action not found!";
                    }

                    if (result.Length<=0)
                    {
                        result=Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                    }
                }
                catch (Exception err)
                {
                    rsp.err_code = -99;
                    rsp.err_msg = err.Message;
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                }               
                Response.Write(result);
                Response.End();
            }           
        }

        private OrthRsp GetSysLoginAccount()
        {
            OrthRsp rsp = new OrthRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.Sys_Login_Account> etys = bz.Retrieve<Model.Sys_Login_Account>();
                tx.CommitTrans();

                rsp.schema = this.GetSchema(etys[0],"Role");
                OrthSchemaItem item = new OrthSchemaItem();                                
                rsp.result = etys;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return rsp;
        }

        private List<OrthSchemaItem> GetSchema<T>(T ety, params string[] excludes)
        {
            List<OrthSchemaItem> schema = new List<OrthSchemaItem>();
            Type t = ety.GetType();
            IEnumerable<string> enums = excludes.AsEnumerable<string>();
            foreach (System.Reflection.FieldInfo fi in t.GetFields())
            {
                if (fi.Name == "TABLE_NAME")
                    continue;

                OrthSchemaItem item = new OrthSchemaItem();
                string propName = fi.GetValue(ety).ToString();
                item.label = "";
                item.field = propName;
                item.type = Type.GetTypeCode(t.GetProperty(propName).PropertyType).ToString();
                item.ignore = enums.Contains<string>(propName) ? true : false;
                item.vals = new SortedList<string, string>();
                schema.Add(item);                
            }
            return schema;
        }

        /// <summary>
        /// 加载所有主thread
        /// </summary>
        private void Loads()
        {
            this.rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false);
            try
            {
                ThreadBiz bz = new ThreadBiz(tx);
                bz.Clauses.Add(ThreadModel.FIELD_PID, new Helper.Evaluation(Helper.SqlOperator.EQ, 0));
                List<ThreadModel> etys = bz.Retrieve<ThreadModel>();
                tx.CommitTrans();

                rsp.result = etys;


                /// -- start --                 
                string url = System.Configuration.ConfigurationManager.AppSettings["PushUrl"];
                string result = HttpUtil.PushNotification(url, "test push", "this is a test push message", "18171adc03995d2319e");
                /// --  end  -- 
            }
            catch (Exception)
            {
                tx.AbortTrans();               
            }
        }

        /// <summary>
        /// 创建thread
        /// </summary>
        private void Build()
        {
            this.rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true);
            try
            {
                ThreadBiz bz = new ThreadBiz(tx);
                string title = Request.Form["title"];
                string content = Request.Form["content"];
                int type = Aid.DbNull2Int(Request.Form["type"]);
                int userid = Aid.DbNull2Int(Request.Form["userid"]);

                bz.Bulid(title.Trim(), content.Trim(), type, userid);
                tx.CommitTrans();
                rsp.result = true;
            }
            catch (Exception)
            {
                tx.AbortTrans();
                throw;
            }
        }

        /// <summary>
        /// 点选一个thread
        /// </summary>
        private void Tick()
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false);
            try
            {
                int id = Aid.DbNull2Int(Request.Form["id"]);
                ThreadBiz bz = new ThreadBiz(tx);
                bz.Clauses.Add("Pid", new Helper.Evaluation(Helper.SqlOperator.EQ, id));
                List<ThreadModel> etys = bz.Retrieve<ThreadModel>();
                etys.Sort((x, y) => { return x.Id < y.Id ? -1 : x.Id > y.Id ? 1 : 0; });                    
                tx.CommitTrans();
                rsp.result = etys;                
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
        }

        /// <summary>
        /// 发送一条指定了thread的comment
        /// </summary>
        private void Send()
        {
            this.rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true);
            try
            {
                ThreadBiz bz = new ThreadBiz(tx);
                ThreadModel ety = new ThreadModel();
                ety.Content = Request.Form["content"];
                ety.Create_Date = DateTime.Now;
                ety.Pid = Aid.DbNull2Int(Request.Form["pid"]);
                ety.Title = Request.Form["title"];
                ety.Type = Aid.DbNull2Int(Request.Form["type"]);
                ety.User_Id= Aid.DbNull2Int(Request.Form["userid"]);

                bz.Clauses.Add("UserId", new Helper.Evaluation(Helper.SqlOperator.EQ, ety.User_Id));
                List<Model.Sys_Login_Account> etys = bz.Retrieve<Model.Sys_Login_Account>();
                ety.Create_By = etys[0].LoginName;
                bz.Create(ety);
                tx.CommitTrans();
                rsp.result = true;
            }
            catch (Exception)
            {
                tx.AbortTrans();
                throw;
            }
        }

        /// <summary>
        /// 加载所有后台用户
        /// </summary>
        private void LoadBeu() //LoadBeu = Backend User
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false);
            try
            {               
                ThreadBiz bz = new ThreadBiz(tx);
                bz.Clauses.Add("Status", new Helper.Evaluation(Helper.SqlOperator.EQ, "A"));
                List<Model.Sys_Login_System> etys = bz.Retrieve<Model.Sys_Login_System>();
                rsp.result = etys;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
        }

        /// <summary>
        /// 后台用户回复一条thread
        /// </summary>
        private void Reply()
        {
            this.rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true);
            try
            {
                ThreadBiz bz = new ThreadBiz(tx);
                ThreadModel ety = new ThreadModel();
                ety.Content = Request.Form["content"];
                ety.Create_Date = DateTime.Now;
                ety.Pid = Aid.DbNull2Int(Request.Form["pid"]);
                ety.Title = Request.Form["title"];
                ety.Type = Aid.DbNull2Int(Request.Form["type"]);
                ety.User_Id = Aid.DbNull2Int(Request.Form["userid"]);

                bz.Clauses.Add("UserId", new Helper.Evaluation(Helper.SqlOperator.EQ, ety.User_Id));
                List<Model.Sys_Login_System> etys = bz.Retrieve<Model.Sys_Login_System>();
                ety.Create_By = etys[0].LoginName;
                bz.Create(ety);
                tx.CommitTrans();
                rsp.result = true;
            }
            catch (Exception)
            {
                tx.AbortTrans();
                throw;
            }
        }

        private void SearchRoot()
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false);
            try
            {
                ThreadBiz bz = new ThreadBiz(tx);
                bz.Clauses.Add("Create_By", new Helper.Evaluation(Helper.SqlOperator.EQ, Request.Form["Create_By"]));
                List<ThreadModel> etys = bz.Retrieve<ThreadModel>();

                List<ThreadModel> etys2 = new List<ThreadModel>();
                for (int i = 0; i < etys.Count; i++)
                {
                    ThreadModel ety = null;
                    if (etys[i].Pid == 0)
                        ety = etys[i];
                    else
                        ety = bz.GetRoot(etys[i].Pid);                
                    
                    if (!etys2.Contains(ety))
                        etys2.Add(ety);
                }
                tx.CommitTrans();
                rsp.result = etys2;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
        }

        private void SearchThreadChain(string id)
        {
            
        }
    
        private void Search()
        {
            /*
            foreach (JsonParaItem item in para.items)
            {
                if (item.type == "string")
                {
                    bz.Clauses.Add(item.key, new Helper.Evaluation(Helper.SqlOperator.EQ, item.val));
                }
                else if (item.type == "int" || item.type == "decimal")
                {
                    bz.Clauses.Add(item.key, new Helper.Evaluation(Helper.SqlOperator.EQ, Helper.Aid.Null2Int(item.val)));
                }
            } 
             */ 
        }

        private void SendThread()
        {
            Helper.DbTx tx = new Helper.DbTx(Commom.DbVendor.Oracle, GlobalUtilSystem.sdb_local, true);
            try
            {
                ThreadModel ety = new ThreadModel();
                ety.Pid = Aid.DbNull2Int(Request.Form["Pid"]);// pid;
                ety.Title = Aid.DbNull2Str(Request.Form["Title"]);
                ety.Type = 0;
                ety.User_Id = Aid.DbNull2Int(Request.Form["User_Id"]);
                ety.Content = Aid.DbNull2Str(Request.Form["Content"]);
                ety.Create_By = Aid.DbNull2Str(Request.Form["Create_By"]);
                ety.Create_Date = DateTime.Now;

                Helper.DbConsole biz = new Helper.DbConsole(tx);
                int cnt = biz.Create(ety);
                tx.CommitTrans();

                rsp.result = true;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                this.rsp.err_code = -1;
                this.rsp.err_msg = err.Message;
            }
        }

        private void ReplyThread()
        {
            Helper.DbTx tx = new Helper.DbTx(Commom.DbVendor.Oracle, GlobalUtilSystem.sdb_local, true);
            try
            {             
                ThreadModel ety = new ThreadModel();
                ety.Pid = Aid.DbNull2Int(Request.Form["Pid"]);// pid;
                ety.Title = Aid.DbNull2Str(Request.Form["Title"]);
                ety.Type = 0;
                ety.User_Id = Aid.DbNull2Int(Request.Form["User_Id"]);
                ety.Content = Aid.DbNull2Str(Request.Form["Content"]);
                ety.Create_By = Aid.DbNull2Str(Request.Form["Create_By"]);
                ety.Create_Date = DateTime.Now;

                Helper.DbConsole biz = new Helper.DbConsole(tx);
                int cnt = biz.Create(ety);
                tx.CommitTrans();

                rsp.result = true;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                this.rsp.err_code = -1;
                this.rsp.err_msg = err.Message;
            }
        }

        private void LoadAll()
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false);
            try
            {
                Helper.DbConsole biz = new Helper.DbConsole(tx);
                List<ThreadModel> etys = biz.Retrieve<ThreadModel>();
                tx.CommitTrans();
                rsp.result = etys;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                this.rsp.err_code = -1;
                this.rsp.err_msg = err.Message;
            }
        }

        private void CreateThread()
        {
            Helper.DbTx tx = new Helper.DbTx(Commom.DbVendor.Oracle, GlobalUtilSystem.sdb_local, true);
            try
            {
                string title = Request.Form["title"].ToString();
                string content = Request.Form["content"].ToString();
                int userid = int.Parse(Request.Form["userid"].ToString());
                string username = Request.Form["username"].ToString();
                string time = Request.Form["time"].ToString();

                ThreadModel ety = new ThreadModel();
                ety.Pid = 0;
                ety.Title = title;
                ety.Type = 0;
                ety.User_Id = userid;
                ety.Content = content;
                ety.Create_By = username;
                ety.Create_Date = DateTime.Now;

                Helper.DbConsole biz = new Helper.DbConsole(tx);
                int cnt = biz.Create(ety);
                tx.CommitTrans();

                rsp.result = true;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                this.rsp.err_code = -1;
                this.rsp.err_msg = err.Message;
            }
        }
    }

    /// <summary>
    /// Thread class business
    /// </summary>
    /// build thread => 
    public class ThreadBiz : Helper.DbConsole
    {
        public ThreadBiz(Helper.DbTx tx) : base(tx) { }

        public List<ThreadModel> Search(Dictionary<string, Helper.Evaluation> paras)
        {
            try
            {
                this.Clauses = paras;
                List<ThreadModel> etys = this.Retrieve<ThreadModel>();
                return etys;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Bulid(string title,string content,int type,int userid) 
        {
            try
            {
                ThreadModel ety = new ThreadModel();
                ety.Pid = 0;
                ety.Title = title.Trim();
                ety.Content = content.Trim();
                ety.User_Id = userid;
                ety.Type = type;
                ety.Create_Date = DateTime.Now;
                
                this.Clauses.Add("UserId", new Helper.Evaluation(Helper.SqlOperator.EQ, userid));
                List<Model.Sys_Login_Account> etys = Retrieve<Model.Sys_Login_Account>();
                ety.Create_By = etys[0].LoginName;

                int cnt = Create(ety);
                return true;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void Send() { 

        }

        public void Reply() { }

        public ThreadModel GetRoot(int id)
        {
            this.Clauses.Add("Id", new Helper.Evaluation(Helper.SqlOperator.EQ, id));
            List<ThreadModel> etys = this.Retrieve<ThreadModel>();
            if (etys[0].Pid != 0)
                GetRoot(etys[0].Pid);
            return etys[0];
        }

        public void GetChain(int id)
        {
            this.Clauses.Add("Id", new Helper.Evaluation(Helper.SqlOperator.EQ, id));
            List<ThreadModel> etys = this.Retrieve<ThreadModel>();

            ThreadModel root = GetRoot(id);
        }
    }

    public class JsonPara
    {
        public string action { get; set; }
        public JsonParaItem[] items { get; set; }
    }

    public class JsonParaItem
    {
        public string key { get; set; }
        public string val { get; set; }
        public string type { get; set; }
    }

    public class OrthRsp : JsonRsp
    {
        public List<OrthSchemaItem> schema = new List<OrthSchemaItem>();
    }

    public class OrthSchemaItem
    {
        public string label { get; set; }
        public string field { get; set; }
        public string type { get; set; }
        public bool ignore { get; set; }
        public SortedList<string, string> vals = new SortedList<string, string>();
    }
}

/*
 *  <!-- 前台提交 -->
        <div class="row">
            <div class="col-md-10">
                <div class="form-horizontal">                    
                    <div class="form-group">
                        <label for="inputUserId" class="col-sm-2 control-label">User Id:</label>
                        <div class="col-sm-3">
                            <input type="number" class="form-control" id="inputUserId" v-bind:value="threadChats.length>0?threadChats[0].userid:0">
                            <select class="form-control" id="selectCustId">
                                <option value="-1">--select--</option>
                                <option v-for="item in custs" v-bind:value="item.CustId">{{item.LoginName}}</option>
                            </select>
                        </div>
                        <label for="inputThreadId" class="col-sm-4 control-label">Thread Id(root):</label>
                        <div class="col-sm-2">
                          <input type="number" class="form-control" id="inputThreadId" v-bind:value="threadChats.length>0?threadChats[0].Id:0" >
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="inputContent" class="col-sm-2 control-label">Content:</label>
                        <div class="col-sm-8">
                            <textarea class="form-control" id="inputContent" rows="3"></textarea>
                        </div>
                        <button type="submit" class="btn btn-default bottom" v-on:click="invokeSend">Send</button>
                        <button type="submit" class="btn btn-default bottom" @click="invokeCreate">Create</button>
                    </div>                                         
                </div>
            </div>
        </div>  
        <div class="row"> 
            <div class="col-md-6">
                <button class="btn btn-primary " id="btnLoad" type="button" @click="loadAll">Load Chat</button>
            </div>                     
        </div>
        <!-- 后台回复 -->
        <div class="row">
            <div class="col-md-3 aborder" >
                <ul class="list-group">                  
                    <li class="list-group-item" v-for="item in threadsInDb" v-if="item.Pid==0" v-on:click="onTick(item.Id)">{{item.Id}}/{{item.Title}}</li>
                </ul>
            </div>
            <div class="col-md-9">
                <div class="panel panel-default fixedHeight18" style="overflow:auto;">
                    <div class="panel-body" id="scrollContent" >                       
                        <div class="well-sm" 
                            v-for="item in threadChats"
                            v-bind:class="item.User_Id==currentUserId?'text-right':'text-left'">
                            {{item.Create_By}} : {{item.Content}}
                        </div>
                    </div>                    
                </div>
                <div class="panel panel-default">
                    <div class="panel-body">
                        <textarea class="form-control" rows="3" id="replyContent" ></textarea>                  
                        <div class="form-inline pull-right">
                            <div class="form-group">
                                <label class="sr-only" for="exampleInputEmail3">Reply User Id</label>                               
                                <select class="form-control" id="selectReplyUserId">
                                    <option value='-1'>--select a user--</option>
                                    <option v-for="item in users" v-bind:value='item.UserId'>{{item.LoginName}}</option>
                                </select>
                            </div>
                            <button class="btn btn-primary" id="btnReply" type="button" v-on:click="invokeReply">Reply</button>                   
                            {{currentUserId}}
                        </div>
                    </div>
                </div>                
            </div>            
        </div>
        <div class="row" id="result">               
        </div>
        <div>
            <button type="submit" id="btnTestGateway" @click="gateway">Gateway</button>
        </div>
 */

