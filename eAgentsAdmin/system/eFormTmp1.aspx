<%@ Page Language="C#"  AutoEventWireup="true"  CodeBehind="eFormTmp1.aspx.cs" Inherits="PropertyOneAppWeb.system.eFormTmp1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=EDGE" />    
    <link rel="stylesheet" href="../js/bootstrap/css/bootstrap.css" media="screen ,print" />    
    <link rel="stylesheet" href="../Css/Menu.css" />
    <link rel="stylesheet" href="../Css/Site.css" />
    <link rel="stylesheet" href="../js/font-awesome-4.7.0/css/font-awesome.min.css" />
    <!--[if (gte IE 9)|!(IE)]><!-->
    <script type="text/javascript" src="../js/jQuery/jquery-3.2.1.min.js"></script>
    <!--<![endif]-->
    <!--[if lte IE 8]>
    <script src="/js/jQuery/jquery-1.11.1.min.js"></script>
    <script src="/js/modernizr.js"></script>
    <script src="/js/AmazeUI/assets/js/amazeui.ie8polyfill.min.js"></script>
    <![endif]-->    
    <script type="text/javascript" src="../js/bootstrap/js/bootstrap.min.js"></script>     
    <script type="text/javascript" src="../js/SiteJScript.js"></script>
    <title>template1</title>
    <style>
         .control-label-left
         {
             padding-top: 7px;
            margin-bottom: 0;
            text-align:left;
         }
         .form-control-reboot
         {
             border-top-style: none;
             border-left-style: none;
             border-right-style:none;
         }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-md-4 col-md-offset-8 text-right">                
                No.: AC/_______/_______
            </div>
        </div>
        <div class="row">
            <div class="col-md-12 text-center">
                <h1>ac 加時冷氣申請</h1>
                <h2>Requisition for Extra Air-conditioning Service</h2>
            </div>
        </div>

        <div class="row">
            <div class="col-md-12 ">
                 <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-2 control-label-left">日期 Date :</label>
                        <div class="col-sm-3">
                            <input type="text" class="form-control p-field " alt="date" v-bind:value="" />
                        </div>                                    
                    </div>        
                 </div>
            </div>
        </div>           

        <div class="row">
            <div class="col-md-12">
                 <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-2 control-label-left">商戶 Tenant :</label>
                        <div class="col-sm-3">
                            <input type="text" class="form-control p-field" alt="tenant" v-bind:value="" />
                        </div>                                    
                    </div>        
                 </div>
            </div>
        </div>
                             
        <div class="row">
            <div class="col-md-12">
                 <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-2 control-label-left">期數 Site No:</label>
                        <div class="col-sm-2">
                            <input type="text" class="form-control p-field" alt="SiteNo" v-bind:value="" />
                        </div>   
                        <label class="col-sm-2 control-label-left">層數 Floor:</label>
                        <div class="col-sm-2">
                            <input type="text" class="form-control p-field" alt="Floor" v-bind:value="" />
                        </div>   
                        <label class="col-sm-2 control-label-left">舖號 Shop No:</label>
                        <div class="col-sm-2">
                            <input type="text" class="form-control p-field" alt="ShopNo" v-bind:value="" />
                        </div>                                       
                    </div>                    
                 </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-5 control-label-left">聯絡人姓名及電話 Contact Person & Telephone No.:</label>
                        <div class="col-sm-5">
                            <input type="text" class="form-control p-field" alt="Contact" v-bind:value="" />
                        </div>   
                    </div>
                </div>
            </div>  
        </div>

        <div class="row">
            <div class="col-md-12">
                <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-5 control-label-left">請安排於下列時段安排加時冷氣服務至</label>
                        <div class="col-sm-3">
                        </div>
                        <label class="col-sm-4 control-label-left">(商舖名稱)</label>
                    </div>
                </div>
                <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-5 control-label-left">Please arrange extra air-conditioning service to</label>
                        <div class="col-sm-3">
                            <input type="text" class="form-control p-field" alt="Contact" v-bind:value="" />
                        </div>                    
                        <label class="col-sm-4 control-label-left">(Premises) as the following period:-</label>
                    </div>
                </div>                 
            </div>  
        </div>
        <div class="row">
            <div class="col-md-12">
                 <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-2 control-label-left">日期 Date:</label>
                        <div class="col-sm-4">
                            <input type="text" class="form-control p-field" alt="Contact" v-bind:value="" />
                        </div>
                    </div>
                 </div>
                 <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-3 control-label-left">時間 Time: &nbsp;&nbsp; 由 From</label>
                        <div class="col-sm-3">
                            <input type="text" class="form-control p-field" alt="Contact" v-bind:value="" />
                        </div>
                        <label class="col-sm-3 control-label-left">時 hours</label>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-3 control-label-left">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;至 To</label>
                        <div class="col-sm-3">
                            <input type="text" class="form-control p-field" alt="Contact" v-bind:value="" />
                        </div>
                        <label class="col-sm-3 control-label-left">時 hours</label>
                    </div>
                 </div>               
            </div>  
        </div>
        <div class="row">
            <div class="col-md-6 col-md-offset-7">                
                <div class="control-label-left">
                    <p>簽署及公司蓋印</p>                   
                    <p>Authorized Signature with Company Chop</p>
                    <p>日期 Date: _____________________________</p>
                </div>                 
            </div>  
        </div>
        <div class="row">
            <div class="col-md-12">
                <p>備註 Remark:</p>
                <p>1) 此加時冷氣申請表須於申請時段之最少24個辦公小時前提交至管理處。</p>
                <p>This form should be completed and returned to the Estate Management Office preferably 24 business hours before
                the requirement of extra hour air-conditioning service.</p>
                <p>2) 加時冷氣之費用將會於下期月結單顯示。</p>
                <p>Debit note will be sent to you in the next month to recover the cost incurred.</p>
                <p>3) 加時冷氣供應服務之最低收費為每日港幣一佰元正。</p>
                <p>There will be a minimum charge of HK$100 each day for the extra hour air-conditioning service.</p>
                <p>4) 如有需要，有關之最低收費可以隨時更改，而最終收費以管理處所發出之通告或信件通知為準。</p>
                <p> The minimum charge is subject to change as and when necessary and the Management Office will issue notice and/or
                letter to notify and/or confirm the actual charge.</p>               
            </div>
        </div>
        <div class="row">
            <div class="col-md-5 col-md-offset-7">
                <p>For Office Use Only</p>
                <div class="form-inline">
                   <div class="form-group">
                        <label for="tbxTotalCharge"> Total A/C Charge: HK$</label>
                        <input type="text" class="form-control" id="tbxTotalCharge" />
                    </div>
                </div>
                <div class="form-inline">
                    <div class="form-group">
                        <label for="rbno">R.B. no.:</label>
                        <input type="text" class="form-control" id="rbno" />
                    </div>
                </div>                
            </div>           
        </div>
        <div class="row">
            <div class="col-md-3">
                EMDFORM/ACC/0078/2
            </div>                               
        </div>
        <div class="row">
            <div class="col-md-4 col-md-offset-8 text-right">
                w.e.f. in 2017.12.1     
            </div>
        </div>
    </div>
    </form>
</body>
</html>
