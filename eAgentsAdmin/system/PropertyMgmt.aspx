<%@ Page Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true" 
    CodeBehind="PropertyMgmt.aspx.cs" Inherits="PropertyOneAppWeb.system.PropertyMgmt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            changeNav("../Image/icon_Notices.png", '<%= Resources.Lang.Res_Property_Maintenance %>');
            search();
        });
        var firstPageload = true; /* 是否第一次加载页面 */
        function search() { 
            /* 加载bootgird数据 */
            $("#grid-data").bootgrid({
                navigation: 3,
                rowCount: 10,
                ajax: true,
                url: "./PropertyMgmt.aspx",
                sorting: false,               
                ajaxSettings: {
                    cache: false
                },
                searchSettings: {
                    delay: 100,
                    characters: 1
                },
                post: function () {
                    return {
                        action: "search"                        
                    };
                },
                formatters: {
                    "edit": function (column, row) {
                        var pcode = row.PROPERTY_CODE;
                        return '<a class="am-badge am-badge-primary am-radius" href="JavaScript:void(0);" onclick="ShowEditor(\'' + pcode + '\')">Edit</a>';
                    }
                }
            });
        }

        /* 保存编辑*/
        function Save() {
            var paras = {};
            paras["propertycode"] = $("#input-propertycode").val();
            paras["propertyname"] = $("#input-propertyname").val();
            paras["emphone"] = $("#input-emphone").val();
            paras["ememail"] = $("#input-ememail").val();
            paras["lephone"] = $("#input-lephone").val();
            paras["leemail"] = $("#input-leemail").val();
            paras["blphone"] = $("#input-blphone").val();
            paras["blemail"] = $("#input-blemail").val()
            paras["action"] = "save";
            Httpost("./PropertyMgmt.aspx", paras, function (rsp) {
                console.log(rsp);
                if (rsp.err_code == 0) {
                    $("#div-editor").hide();
                    showAlert("Notice", "Save successfully!");
                    $("#grid-data").bootgrid("reload");
                } else
                    showAlert("Notice", rsp.result_msg);
            }); 
        }

        /* 显示编辑 */
        function ShowEditor(code) {
            Httpost("./PropertyMgmt.aspx", { "action": "edit", "propertycode": code }, function (rsp) {
                if (rsp.err_code != 0) {
                    showAlert("Error", rsp.err_msg);
                } else {
                    var propinfo = rsp.result[0];
                    $("#div-editor").show();
                    $("#input-propertycode").val(propinfo.Property_Code);
                    $("#input-propertyname").val(propinfo.Property_Name);
                    $("#input-emphone").val(propinfo.EM_Phone);
                    $("#input-ememail").val(propinfo.EM_Email);
                    $("#input-lephone").val(propinfo.LE_Phone);
                    $("#input-leemail").val(propinfo.LE_Email);
                    $("#input-blphone").val(propinfo.BL_Phone);
                    $("#input-blemail").val(propinfo.BL_Email);
                }                
            });
        };

        function Httpost(url,data,callback) {
            $.ajax({
                cache: false,
                type: "POST",
                url: url,
                dataType:"json",
                data: data,
                success:callback,
                error:callback
            });
        }       
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="am-g am-g-collapse">
        <%-- 外边框 --%>
        <div class="am-u-sm-12">
            <%-- 深色标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue"><p><%= Resources.Lang.Res_Property_Maintenance%></p></div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">
                    <%--bootgrid--%>
                    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid" data-toggle="bootgrid">
                        <thead>
                            <tr>
                                <th data-column-id="PROPERTY_CODE" data-identifier="true" data-align="center" data-header-align="center">
                                   <%= Resources.Lang.Res_Property_Code%>
                                </th>
                                <th data-column-id="PROPERTY_NAME" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Property_Name%>
                                </th>
                                <th data-column-id="EM_PHONE" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Em_Phone_Number%> 
                                </th>
                                <th data-column-id="EDIT" data-formatter="edit" data-header-css-class="boot-header-1" data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Operation%>  
                                </th>
                            </tr>
                        </thead>
                    </table>
                </div>
            </div>
            <div class="am-u-sm-12" style="height: 30px;">
            </div>
            <%-- 编辑 --%>
            <div id="div-editor"  class="am-u-sm-12" hidden="hidden">
                <%-- 深色标题 --%>
                <div class="am-u-sm-12 global_table_head global_color_navyblue">
                    <p id="p-title">
                        <%= Resources.Lang.Res_Property_Editor%>   
                    </p>
                </div>
                <%-- 内容 --%>
                <div class="am-u-sm-12 global_table_body global_color_lightblue">
                    <%-- 边距内内容 --%>
                    <div class="am-u-sm-12">
                         <table style="width: 100%">
                            <tr>
                                <th style="width: 15%;">
                                </th>
                                <th style="width: 34%;">
                                </th>
                                <th style="width: 2%;">
                                </th>
                                <th style="width: 15%;">
                                </th>
                                <th style="width: 34%;">
                                </th>
                            </tr>
                            <tr>
                                <td>
                                </td>
                                <td>
                                </td>
                                <td>
                                </td>
                                <td>
                                </td>
                                <td>
                                    <button id="button-save" type="button" class="am-btn am-btn-secondary am-radius button-all" onclick="Save()" style="float: right;"><%= Resources.Lang.Res_Save%></button>                                                                                                         
                                </td>
                            </tr>
                            <tr style="height: 20px;"></tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                   <%= Resources.Lang.Res_Property_Code%>: 
                                </td>
                                <td>
                                    <input id="input-propertycode" type="text" class="am-form-field" maxlength="20" readonly="true" />
                                </td>
                                <td>
                                </td>
                                <td style="text-align: right; padding-right: 20px;">
                                   <%= Resources.Lang.Res_Property_Name%>:
                                </td>
                                <td>                                    
                                    <input id="input-propertyname" type="text" class="am-form-field" maxlength="20" readonly="true" />
                                </td>
                            </tr>
                            <tr style="height: 20px;"></tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                   <%= Resources.Lang.Res_Em_Phone_Number%>:
                                </td>
                                <td>
                                    <input id="input-emphone" type="text" class="am-form-field" maxlength="20"/>
                                </td>
                                <td></td>
                                <td style="text-align: right; padding-right: 20px;">
                                   <%= Resources.Lang.Res_Property_EM_Email%>:
                                </td>
                                <td>
                                    <input id="input-ememail" type="text" class="am-form-field" maxlength="20"/>
                                </td>
                            </tr>
                            <tr style="height: 20px;"></tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                  <%= Resources.Lang.Res_Property_Leasing_Phone%>:
                                </td>
                                <td>
                                    <input id="input-lephone" type="text" class="am-form-field" maxlength="20"/>
                                </td>
                                <td></td>
                                <td style="text-align: right; padding-right: 20px;">
                                   <%= Resources.Lang.Res_Property_Leasing_Email%>:
                                </td>
                                <td>
                                    <input id="input-leemail" type="text" class="am-form-field" maxlength="20"/>
                                </td>
                            </tr>
                            <tr style="height: 20px;"></tr>
                            <tr>
                                <td style="text-align: right; padding-right: 20px;">
                                  <%= Resources.Lang.Res_Property_Billing_Phone%>:
                                </td>
                                <td>
                                    <input id="input-blphone" type="text" class="am-form-field" maxlength="20"/>
                                </td>
                                <td></td>
                                <td style="text-align: right; padding-right: 20px;">
                                  <%= Resources.Lang.Res_Property_Billing_Email%>:
                                </td>
                                <td>
                                    <input id="input-blemail" type="text" class="am-form-field" maxlength="20"/>
                                </td>
                            </tr>
                         </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>