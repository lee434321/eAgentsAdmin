using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace PropertyOneAppWeb.Model
{
    public class DataModel
    {
        public class SysLogModel
        {
            public const string TABLE_NAME = "Sys_Log";

            [Helper.DbField(true, false, false)]
            public int LogId { get; set; }
            public int Type { get; set; }
            public int UserId { get; set; }
            public string Msg { get; set; }
            public DateTime CreateDate { get; set; }
        }

        /// <summary>
        /// 已选中的待支付trans
        /// </summary>
        public class ModelSelectedTrans
        {
            public string leasenum { get; set; }
            public string transno { get; set; }
            public string chargeitem { get; set; }
            public string amount { get; set; }
            public string outstanding { get; set; }
            public string invoicelinenum { get; set; }
        }

        /// <summary>
        /// 登录账号对应的租约号
        /// </summary>
        public class ModelLeaseGroup
        {
            public string leasenum { get; set; }
            public string outstanding { get; set; }
            public string premises { get; set; }
        }

        /// <summary>
        /// 根据Group Id，返回给User Group Maintenance页面的User Group信息
        /// </summary>
        public class ModelUserGroupInfo
        {
            public string groupid { get; set; }
            public string groupname { get; set; }
            public string status { get; set; }
            public string dept { get; set; }
            public string approver { get; set; }
            public List<ModelUserGroupInfo_AuthInfo> authinfo { get; set; }
            public List<ModelUserGroupInfo_PropertyInfo> propertyinfo { get; set; }
        }
        public class ModelUserGroupInfo_AuthInfo
        {
            public int authid { get; set; }
        }
        public class ModelUserGroupInfo_PropertyInfo
        {
            public string propertycode { get; set; }
        }
        
        /// <summary>
        /// 根据User Id，返回给User Manintenance页面的User信息
        /// </summary>
        public class ModelUserInfo
        {
            public string userid { get; set; }
            public string loginname { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string status { get; set; }
            public List<ModelUserInfo_GroupInfo> groupinfo { get; set; }
        }
        public class ModelUserInfo_GroupInfo
        {
            public int groupid { get; set; }
        }

        /// <summary>
        /// 用户权限列表
        /// </summary>
        public class ModelUserAuthList
        {
            public string loginName { get; set; }
            public List<ModelUserAuthList_AuthInfo> authInfo { get; set; }
        }
        public class ModelUserAuthList_AuthInfo
        {
            public string authId { get; set; }
        }
    }

    #region estatement后台数据库表ORM定义

    /*  
     */
    public class Sys_Users_Group_Lease
    {
        public const string TABLE_NAME = "Sys_Users_Group_Lease";

        [Helper.DbField(true, false, false)]
        public int USERID { get; set; }
        public string LEASENUMBER { get; set; }
        public string CREATEBY { get; set; }
        public DateTime CREATEDATE { get; set; }
        public string CREATE_SRC { get; set; }
    }

    public class Pone_Customer
    {
        public const string TABLE_NAME = "Pone_Customer";

        public string CUSTOMER_NUMBER { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string CONTACT_ADDRESS_1 { get; set; }
        public string CONTACT_ADDRESS_2 { get; set; }
        public string CONTACT_ADDRESS_3 { get; set; }
        
    }

    public class Pone_Lm_Lease
    { 
        public const string TABLE_NAME = "Pone_Lm_Lease";

        public string LEASE_NUMBER { get; set;  }
        public string CUSTOMER_NUMBER { get; set; }
        public DateTime Lease_Term_From { get; set; }
        public DateTime Lease_Term_To { get; set; }
        public string Premise_Name1 { get; set; }
        public string Premise_Name2 { get; set; }
        public string Premise_Name3 { get; set; }
        public string Premise_Name4 { get; set; }
    }

    public class SysLoginSystemModel
    {
        public const string TABLE_NAME = "Sys_Login_System";
        public const string FIELD_USERID = "UserId";
        public const string FIELD_LOGINNAME = "LoginName";

        [Helper.DbField(true)]
        public int UserId { get; set; }

        public string LoginName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string SiteNumber { get; set; }
        public string TempPsw { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }

    }

    public class Sys_Login_System
    {
        public const string TABLE_NAME = "Sys_Login_System";

        [Helper.DbField(true, false, false)]        
        public int UserId { get; set; }

        public string LoginName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string SiteNumber { get; set; }
        public string TempPsw { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }

        public const string FIELD_USERID = "UserId";
    }

    public class Sys_Login_System_Group
    {
        public const string TABLE_NAME = "Sys_Login_System_Group";
       
        public int UserId { get; set; }
        public int GroupId { get; set; }
    }

    public class Sys_Users_Group
    {
        public const string TABLE_NAME = "Sys_Users_Group";

        [Helper.DbField(true, false, false)]
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Status { get; set; }
        public string CreateBy{ get; set; }
        public DateTime CreateDate{ get; set; }
        public string Dept { get; set; }
        public string Approver { get; set; }
        public string UpdateBy{get;set;}
        public DateTime UpdateDate{get;set;}
    }

    public class Sys_Users_Group_Authority
    {
        public const string TABLE_NAME = "Sys_Users_Group_Authority";
        
        public int GroupId { get; set; }
        public int AuthTypeId { get; set; }
    }

    public class Sys_Login_Account
    {
        public const string TABLE_NAME = "Sys_Login_Account";
        
        public const string FIELD_USERID = "UserId";
        public const string FIELD_LOGINNAME = "LoginName";
        public const string FIELD_EMAIL = "Email";
        public const string FIELD_STATUS = "Status";
        public const string FIELD_CREATEBY = "CreateBy";
        public const string FIELD_ROLE = "Role";

        [Helper.DbField(true, false, false)]
        public int UserId { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string CU_Status { get; set; } //Customer User Status; 0-Pending;1-Sent Email;2-Active' ,MKT或者FIN创建的customer user状态
        public string Role { get; set; }
        public string Contact_Name { get; set; }
        public string Contact_Number { get; set; }
        public string CustName { get; set; }
        public string Registration_Id { get; set; }
        public string Device_Id { get; set; }
        public string Position { get; set; }
        public string IsPrimary { get; set; }
        [Helper.DbField(false, false, true)]
        public string Create_From_Lease { get; set; }
        public string Temppsw { get; set; }
        public string Wx_OpenId { get; set; }
    }

    public class pone_pd_property
    {
        public const string TABLE_NAME = "pone_pd_property";

        [Helper.DbField(true, false, false)]
        public string Property_Code { get; set; }
        public string Property_Name { get; set; }
        public string Active { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public string Address_3 { get; set; }
        public string EM_Phone { get; set; }
        public string EM_Email { get; set; }
        public string BL_Phone { get; set; }
        public string BL_Email { get; set; }
        public string LE_Phone { get; set; }
        public string LE_Email { get; set; }
    }

    public class T_Feedback_Type
    {
        public const string TABLE_NAME = "T_Feedback_Type";

        [Helper.DbField(true, false, false)]
        public int FeedbackTypeId { get; set; }
        public string TypeName { get; set; }
    }

    public class T_FeedBack
    {
        public const string TABLE_NAME = "T_FeedBack";
        public const string FIELD_FEEDBACKID = "FeedbackId";

        [Helper.DbField(true, false, false)]
        public int FeedbackId { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }      //sub content反馈内容
        public int STATUS { get; set; }         //
        public string CreateBy { get; set; }    //HUHS00639	
        public DateTime CreateDate { get; set; }//	8/2/2019 11:31:36 AM	
        public string LEASENUMBER { get; set; } //	HUHS00639 	租约号
        public int TYPE { get; set; }           //600	反馈类型
        public string PROPERTYCODE { get; set; }//<None>         	

        public Nullable<DateTime> Last_Update { get; set; } //最后更新(因项目早期设计原因，大部分的表没有这个字段，故使用可空类型)
        public string Update_By { get; set; }

        [Helper.DbField(false, false, true)]
        public string CustomerName { get; set; }//(数据库里不含该字段)

        [Helper.DbField(false, false, true)]
        public string Premise { get; set; } //(数据库里不含该字段)

        [Helper.DbField(false, false, true)]
        public bool NoReply { get; set; } // 这里是个标志位，用于在发送统计邮件时标识本条feedback是否为未回复(数据库里不含该字段)

        [Helper.DbField(false, false, true)]
        public List<T_Feedback_Res> feedback_res = new List<T_Feedback_Res>(); //(数据库里不含该字段)
    }

    public class T_Feedback_Res
    {
        public const string TABLE_NAME = "T_Feedback_Res";
        public const string FIELD_FEEDBACKID = "FeedbackId";

        [Helper.DbField(true, false, false)]
        public int Id { get; set; }
        public int FeedbackId { get; set; }
        public string Status { get; set; }
        public string Approve { get; set; }
        public DateTime CreateDate { get; set; }
        public string ApproveBy { get; set; }
        public Nullable<DateTime> ApproveDate { get; set; }
        public string CreateBy { get; set; }
        public string Detail { get; set; }
        public string Leasenum { get; set; }
        public string ReplyPerson { get; set; }
        public string ReplyType { get; set; }
        [Helper.DbField(false, false, true)]
        public string Src { get; set; }
    }

    public class T_Notice
    {
        public const string TABLE_NAME = "T_Notice";

        public int NoticeId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PropertyCode { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class Sys_Users_Group_Property
    {
        public const string TABLE_NAME = "Sys_Users_Group_Property";

        public int GroupId { get; set; }
        public string PropertyCode { get; set; }

        [Helper.DbField(false, false, true)] //用于前端界面选中时的数据绑定
        public bool Ticked { get; set; }
    }

    public class T_Statement_Date_Property
    {
        public const string TABLE_NAME = "T_Statement_Date_Property";

        [Helper.DbField(true, true)]
        public int Id { get; set; }
        public int Statement_Date_Id { get; set; }
        public string Email_Sent { get; set; }
        public string PROPERTY_CODE { get; set; } //为了bootgrid控件，这里必须要大写。
    }

    public class Profile
    {
        public const string TABLE_NAME = "Profile";
        public string Key { get; set; }
        public string Val { get; set; }        
    }
    #endregion    
}