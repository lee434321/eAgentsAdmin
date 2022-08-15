<%@ Page Title="" Language="C#" MasterPageFile="~/SiteSystem.Master" AutoEventWireup="true"
    CodeBehind="NoticeMaintenance.aspx.cs" Inherits="PropertyOneAppWeb.system.NoticeMaintenance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .boot-c-property
        {
            width: 250px;
        }
        .boot-c-date
        {
            width: 120px;
        }
        .boot-c-edit
        {
            width: 120px;
        }
    </style>
    <script type="text/javascript">
        var lang = '<% =Session["lang"] %>';
        function safeHtml(v) {
            if (v) {                
                var o = v.replace(/'/g, "&#39;");
                o = o.replace(/&/g, "&#38;");

                return o;
            } else
                return "";
        }

        //单引号包裹，外层是双引号
        function q1safe(v) {
            if (v) {
                // arr = o.match(/\n/g);
                var o = v.replace(/"/g, "&#34;").replace(/\'/g, "\\\'");
                o = o.replace(/\n/g, "\\n");
                return o;
            } else
                return "";
        }

        //双引号内安全转换
        function q2safe(v) {
            if (v) {
                return v.replace(/'/g, "&#39;");
            } else
                return "";
        }

        $(document).ready(function () {
            changeNav("../Image/icon_Notices.png", '<%= Resources.Lang.Res_NoticeMaintenance %>');
            search();
            LoadPropertyInfo();

            if (lang == "en-US") {
                $('#datapicker-from').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' }).on('changeDate.datepicker.amui', function (event) { });
                $('#datapicker-to').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' }).on('changeDate.datepicker.amui', function (event) { });
                $('#datapicker-search-from').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' }).on('changeDate.datepicker.amui', function (event) { });
            } else {

            }

            $('#datapicker-from').datepicker({ format: 'yyyy-mm-dd', locale: 'zh_CN' }).on('changeDate.datepicker.amui', function (event) { });
            $('#datapicker-to').datepicker({ format: 'yyyy-mm-dd', locale: 'zh_CN' }).on('changeDate.datepicker.amui', function (event) { });
            $('#datapicker-search-from').datepicker({ format: 'yyyy-mm-dd', locale: 'zh_CN' }).on('changeDate.datepicker.amui', function (event) { });

            $("#grid-data th[data-column-id='PROPERTY_CODE']").css('width', '108px');  //设置property_code列的宽度
            $("#grid-data th[data-column-id='edit']").css('width', '180px'); // 设置"edit"列宽度

            document.querySelector(".icon.glyphicon.glyphicon-refresh").parentElement.addEventListener('click', function (e) {
                if (qs(window.location.href, 'sp')) {
                    document.querySelector("#ul-notice #li-notice-maintenance a").click();
                }
            });
        });

        var firstPageload = true; /* 是否第一次加载页面 */
        function search() {
            if (firstPageload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-data").bootgrid({
                    navigation: 3,
                    rowCount: 10,
                    columnSelection: false,
                    ajax: true,
                    url: "./NoticeMaintenance.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    searchSettings: {
                        delay: 100,
                        characters: 3
                    },
                    post: function () {
                        return {
                            action: "search"
                        };
                    },
                    formatters: {
                        "IMGURLLARGE": function (column, row) {
                            var imgurl = row.IMGURLLARGE;
                            var website = '<% =Session["website"]%>';
                            //imgurl = website + imgurl;
                            return '<img alt="" src="..' + imgurl + '" width="100px" height="50px"/>';
                        },
                        "edit": function (column, row) {
                            var noticeid = row.NOTICEID;
                            var title = row.TITLE;
                            var startdate = row.STARTDATE;
                            var enddate = row.ENDDATE;
                            var detail = row.DETAIL;
                            var imgurl = row.IMGURLLARGE;
                            var website = '<% =Session["website"]%>';
                            //var fullurl = website + imgurl;
                            var fullurl = imgurl;
                            var propertyname = row.PROPERTY_NAME;
                            var type = row.TYPE;            // 类型（通知，活动等等）
                            var attachurl = row.ATTACH_URL; // 附件地址
                            var attachurl_encoded = row.attach_url_encoded;
                            /// -- start -- #20042 新增createby,updateby,approveby等信息
                            var createBy = row.CREATEBY;
                            var createDate = row.CREATEDATE;
                            var updateBy = row.UPDATEBY;
                            var updateDate = row.UPDATEDATE;
                            var approveBy = row.APPROVEBY;
                            var approveDate = row.APPROVEDATE;
                            /// --  end  --                                                        
                            //var c = '<a class="am-badge am-badge-primary am-radius" href="JavaScript:void(0);" onclick="ShowEditPanel(\'' + noticeid + '\', \'' + title + '\', \'' + startdate + '\', \'' + enddate + '\', \'' + detail + '\', \'' + fullurl + '\', \'' + imgurl + '\', \'' + propertyname + '\', \'' + type + '\',\'' + attachurl + '\',\'' + createdBy + '\' )">Edit</a> <a class="am-badge am-badge-danger am-radius" href="JavaScript:void(0);" onclick="showdeletemodal(\'' + noticeid + '\')">Delete</a>'; //content
                            //c += ' <a class="am-badge am-badge-secondary am-radius" href="Javascript:void(0);" onclick="Preview(\'' + title + '\',\'' + startdate + '\', \'' + detail + '\',\'' + imgurl + '\',\'' + attachurl + '\')">Preview</a>';


                            var d = '<a class="am-badge am-badge-primary am-radius" href="javascript:void(0);" onclick="ShowEditPanel(';
                            d += '\'' + noticeid + '\',';
                            // d += '\'' + title.replace("'","\\'") + '\','; ///blue/g
                            d += '\'' + q1safe(title) + '\',';
                            d += '\'' + startdate + '\',';
                            d += '\'' + enddate + '\',';
                            d += '\'' + q1safe(detail) + '\',';
                            d += '\'' + (typeof (fullurl) == "string" ? fullurl.replace('\'', '\\\'') : "") + '\',';
                            d += '\'' + imgurl + '\',';
                            d += '\'' + propertyname + '\',';
                            d += '\'' + type + '\',';
                            d += '\'' + (typeof (attachurl) == "string" ? attachurl.replace('\'', '\\\'') : "") + '\',';
                            d += '\'' + createBy + '\',';
                            d += '\'' + createDate + '\',';
                            d += '\'' + updateBy + '\',';
                            d += '\'' + updateDate + '\',';
                            d += '\'' + approveBy + '\',';
                            d += '\'' + approveDate + '\'';
                            d += ')">Edit</a>';

                            d += '<a class="am-badge am-badge-danger am-radius" href="JavaScript:void(0);" onclick="showdeletemodal(\'' + noticeid + '\')">Delete</a>';
                            d += '<a class="am-badge am-badge-secondary am-radius" href="Javascript:void(0);" onclick="Preview(\'' +q1safe(title) + '\',\'' + startdate + '\', \'' + q1safe(detail) + '\',\'' + (typeof (fullurl) == "string" ? fullurl.replace('\'', '\\\'') : "") + '\',\'';
                            d += (typeof (attachurl) == "string" ? q1safe(attachurl) : '') + '\')">Preview</a>';                           
                            
                            /// --  end  --                       
                            return d;
                        },
                        "approve": function (column, row) {
                            var approve = row.APPROVE;
                            if (approve == 'A') {
                                return '<span class="am-badge am-badge-success am-radius">Approved</span>';
                            }
                            else {
                                return '<span class="am-badge am-badge-warning am-radius">Submit</span>';
                            }
                        },
                        "type": function (column, row) {
                            /*
                            <option value="S">Notice</option>
                            <option value="P">Event</option>
                            <option value="C">Carpark Info</option>	  
                            <option value="V">Services</option>      
                            */
                            var result = "";
                            switch (row.TYPE) {
                                case "S":
                                    result = "Notice";
                                    break;
                                case "P":
                                    result = "Event";
                                    break;
                                case "C":
                                    result = "Carpark";
                                    break;
                                case "V":
                                    result = "Services";
                                    break;
                                default:
                                    result = "Unknown";
                                    break;

                            }
                            return result;
                        }
                    }
                });
                firstPageload = false;
                $("#grid-data").bootgrid("search", qs(window.location.href, "sp"));
            } else { /* 如果不是第一次 */
                $("#grid-data").bootgrid("reload");
            };
        };

        /* 加载Property */
        var firstPropertyload = true; /* 是否第一次加载Property页面 */
        function LoadPropertyInfo() {
            if (firstPropertyload == true) {  /* 如果是第一次 */
                /* 加载bootgird数据 */
                $("#grid-property").bootgrid({
                    navigation: 0,
                    rowCount: -1,
                    selection: true,
                    multiSelect: true,
                    rowSelect: true,
                    ajax: true,
                    url: "./NoticeMaintenance.aspx",
                    sorting: false,
                    ajaxSettings: {
                        cache: false
                    },
                    post: function () {
                        return {
                            action: "searchproperty"
                        };
                    }
                });
                firstPropertyload = false;
            } else { /* 如果不是第一次 */
                $("#grid-property").bootgrid("reload");
            };
        };

        function SaveNoticeEx(sendstr, isDefaultImg, paras) {
            $.ajax({
                cache: false,
                type: "POST",
                url: "./NoticeMaintenance.aspx",
                data: {
                    "action": "addnotice",
                    "saveinfo": sendstr,
                    /// -- start --
                    "title": paras.title,
                    "startdate": paras.startdate,
                    "enddate": paras.enddate,
                    "content": paras.content,
                    "imgurl": paras.imgurl,
                    "imgname": paras.imgname,
                    "arrayproperty": paras.arrayproperty,
                    "type": paras.type,
                    "attachurl": paras.attachurl,
                    /// --  end  --
                    "defaultImg": isDefaultImg                   
                },
                success: function (data) {
                    try {
                        if (data == "ok") {
                            showAlert("Notice", "Add new notice success");
                            ClearAddPanel();
                            search();
                        }
                        else {
                            showAlert("Notice", "error");
                        }
                    } catch (ex) {
                        showAlert("Notice", "error");
                    }
                    // 释放保存按钮
                    $("#button-new-save").removeAttr("disabled").text("Save New Notice");
                },
                error: function (data) {
                    showAlert("Notice", "error");
                    // 释放保存按钮
                    $("#button-new-save").removeAttr("disabled").text("Save New Notice");
                }
            });
        }

        function GetInputData() {
            var title = $("#input-edit-title").val(); if (title.length <= 0) { showAlert("Notice", "Error, [Title] is required!"); return; }
            var startdate = $('#datapicker-from').val(); if (title.length <= 0) { showAlert("Notice", "Error, [startdate] is required!"); return; }
            var enddate = $('#datapicker-to').val(); if (title.length <= 0) { showAlert("Notice", "Error, [enddate] is required!"); return; }
            var content = $("#textarea-edit-content").val(); if (title.length <= 0) { showAlert("Notice", "Error, [content] is required!"); return; }

            var upload = $("#doc-form-file")[0].files[0];
            var imgurl = $("#img-edit").attr("rpath");

            var imgname = $("#img-edit").attr("alt"); // 这里的alt属性保存的是图片的文件名     
            var arrayproperty = $("#grid-property").bootgrid("getSelectedRows");            
            var type = $("#ddlType").val();            
            var attachurl = $("#attachment-file").attr("rpath");
            var inputStr = title + "|" + startdate + "|" + enddate + "|" + content + "|" + imgurl + "|" + imgname + "|" + arrayproperty + "|" + type + "|" + attachurl; //这里要再获取一次下拉框的值
            return inputStr;
        }

        /* 保存新增的Notice */
        function SaveNewNotice() {
            var isDefaultImg = $("#ddlType").val() == "C" ? "Carpark.png" : "";

            var title = $("#input-edit-title").val(); if (title.length <= 0) { showAlert("Notice", "Error, [Title] is required!"); return; }
            var startdate = $('#datapicker-from').val(); if (title.length <= 0) { showAlert("Notice", "Error, [startdate] is required!"); return; }
            var enddate = $('#datapicker-to').val(); if (title.length <= 0) { showAlert("Notice", "Error, [enddate] is required!"); return; }
            var content = $("#textarea-edit-content").val(); if (title.length <= 0) { showAlert("Notice", "Error, [content] is required!"); return; }

            var upload = $("#doc-form-file")[0].files[0];
            var imgurl = $("#img-edit").attr("rpath");
           
            var imgname = $("#img-edit").attr("alt"); // 这里的alt属性保存的是图片的文件名     
            var arrayproperty = $("#grid-property").bootgrid("getSelectedRows");           

            /// -- start -- #19172
            var type = $("#ddlType").val();
            if (type == "-1") {
                showAlert("Notice", "please select an valid notice type!");
                return;
            }
            var attachurl = $("#attachment-file").attr("rpath");
            /// --  end  --
            /*
            1.检查是否提交了图片
            2.若无，则弹出提示框询问是否使用默认，
            3.若点击是，上传默认图片（实际不是上传，是有个参数告诉后台，复制一份默认图片到/Upload文件夹下，并且保存url到notice表）
            4.若点击否，则模拟点击上传按钮。
            */
            var imgrpath = $("#img-edit").attr("rpath");
            if ((imgrpath === undefined || imgrpath == "") && $("#ddlType").val() != "C") {
                //加载默认图片
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./NoticeMaintenance.aspx",
                    data: {
                        "action": "getdefaultimg",
                        "imgType": type
                    },
                    dataType: "json",
                    success: function (r) { //response sample:{"err_code":0,"err_msg":"","result":[{"name":"notice_01.png","url":"/notice_default_img/notice_01.png"}]}
                        if (r.err_code != 0) {
                            alert(r.err_msg);
                            return;
                        } else {
                            $("#doc-modal-prompt .imagelist div").remove();

                            for (var i = 0; i < r.result.length; i++) {
                                var $d = $("<div></div>").css("padding", "5px");
                                var $r = $("<input type='radio' name='rdoImg' style='width:58px;'/>").attr("alt", r.result[i].name);
                                var $e = $("<img />").attr("src", r.result[i].url).attr("alt", r.result[i].name);
                                if (r.result[i].height > 300) {
                                    var ratio = r.result[i].heigth / r.result[i].width;
                                    var h2 = 250;
                                    var w2 = Math.floor(250 / ratio);
                                    $e.css("height", h2 + "px").css("width", w2 + "px");
                                }
                                $d.append($r).append($e);
                                $("#doc-modal-prompt .imagelist").append($d);
                            }

                            $("#label-prompt-title").text("Notice");
                            $("#label-prompt-content").text("Use Default Image?");
                            $("#doc-modal-prompt").modal({ closeViaDimmer: true, // 点击外部空白处是否关闭弹窗，这里使用true
                                onConfirm: function (e) {
                                    var imgName = "";
                                    $("#doc-modal-prompt .imagelist input").each(function (x) {
                                        if (this.checked) {
                                            imgName = this.alt;
                                        }
                                    });

                                    if (imgName == "") {
                                        alert("Pleae select an image or click [No] button");
                                        return;
                                    }

                                    isDefaultImg = imgName;
                                    upload = "x";
                                    imgurl = "";
                                    var sendstr = GetInputData();

                                    /// -- start --
                                    var defParas = {
                                        title: title,
                                        startdate: startdate,
                                        enddate: enddate,
                                        content: content,
                                        imgurl: imgurl,
                                        imgname: imgname,
                                        arrayproperty: "" + arrayproperty + "",
                                        type: $("#ddlType").val(),
                                        attachurl: $("#attachment-file").attr("rpath")
                                    }
                                    /// --  end  --
                                    SaveNoticeEx(sendstr, isDefaultImg, defParas); //这里有潜在问题，可能存在必要字段为空的情况，后期要修正。     
                                    // 禁用保存按钮
                                    $("#button-new-save").attr("disabled", "disabled").text("Waiting...");
                                },
                                onCancel: function (e) {
                                    $("#doc-form-file").click();
                                    return;
                                }
                            });
                        }
                    }
                });
            } else {
                upload = $("#doc-form-file")[0].files[0];
                imgurl = $("#img-edit").attr("rpath");
                if (imgurl.length <= 0 && $("#ddlType").val() != "C") { showAlert("Notice", "Error, [Image] is required!"); return; }
                var sendstr = title + "|" + startdate + "|" + enddate + "|" + content + "|" + imgurl + "|" + imgname + "|" + arrayproperty + "|" + type + "|" + attachurl;
                var sendparas = {
                    title: title,
                    startdate: startdate,
                    enddate: enddate,
                    content: content,
                    imgurl: imgurl,
                    imgname: imgname,
                    arrayproperty: "" + arrayproperty + "",
                    type: type,
                    attachurl: attachurl
                }
                if (checknotnull(title) && checknotnull(content) && $("#ddlType").val() == "C" ? true : checknotnull(upload) && checknotnull(startdate) && checknotnull(enddate) && checknotnull(arrayproperty)) {
                    SaveNoticeEx(sendstr, isDefaultImg, sendparas);
                }
                else {
                    showAlert("Notice", "Required item can not be blank.");
                }
            }
        };

        /* 保存编辑后的Notice */
        function SaveEditNotice() {
            var noticeid = $("#input-edit-id").val();
            var title = $("#input-edit-title").val();
            var startdate = $('#datapicker-from').val();
            var enddate = $('#datapicker-to').val();
            var content = $("#textarea-edit-content").val();
            var upload = $("#doc-form-file")[0].files[0];
            var imgurl = $("#img-edit").attr("rpath");
            var imgname = $("#img-edit").attr("alt");
   
            /// -- start -- #19172
            var type = $("#ddlType").val();
            if (type == "-1") {
                showAlert("Notice", "please select an valid notice type!");
                return;
            }
            var attachurl = $("#attachment-file").attr("rpath");
            /// --  end  --
            if (checknotnull(noticeid) && checknotnull(title) && checknotnull(content) && checknotnull(startdate) && checknotnull(enddate)) {
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "./NoticeMaintenance.aspx",
                    data: {
                        "action": "editnotice",
                        "noticeid": noticeid,
                        "title": title,
                        "startdate": startdate,
                        "enddate": enddate,
                        "content": content,
                        "imgurl": imgurl,
                        "imgname": imgname,
                        /// -- start --
                        "type": type,
                        "attachurl":attachurl
                        /// --  end  --
                    },
                    success: function (data) {
                        try {
                            if (data == "ok") {
                                showAlert("Notice", "Edit Success");
                                ClearAddPanel();
                                search();
                            }
                            else {
                                showAlert("Notice", "Edit error");
                            }
                        } catch (ex) {
                            showAlert("Notice", "Edit error");
                        }
                    },
                    error: function (data) {
                        showAlert("Notice", "Edit error");
                    }
                });
            }
            else {
                showAlert("Notice", "Input information error");
            }
        };

        /* 显示Add窗口 */
        function ShowAddPanel() {
            ClearAddPanel();

            /* 更新Title */
            $("#p-title").text('<%= Resources.Lang.Res_Notice_Btn_New %>');
            /* 显示新增面板 */
            $("#div-add-notice").show();
            /* 隐藏保存编辑按钮 */
            $("#button-new-save").show();
            $("#button-edit-save").hide();
            /* 隐藏property输入框*/
            $("#tr-input-property").hide();
            /* 显示property select */
            $("#tr-grid-property").show();

            /* 滚动屏幕 */
            $('html, body').animate({ scrollTop: 600 }, '500');

        };
        /* 显示预览窗口 */
        function Preview(title, date, detail, imgurl, attach_url) {
            $p = $("#doc-modal-preview");
            $p.modal({ closeViaDimmer: false });
            $p.find(".media-heading").text(title);
            $p.find(".media-body span").text(date);
            $p.find(".media-body p").text(detail);
            $p.find("img").attr('src', '..'+imgurl);

            if (attach_url != null && attach_url != "null" && attach_url!="") {
                $p.find("#attach").attr("href", '..' + attach_url.replace("&", "%26"));
                $p.find("#attach").css("display", "block");
            } else {
                $p.find("#attach").css("display", "none");
            }
        }

        /* 显示Edit窗口 */
        function ShowEditPanel(noticeid, title, startdate, enddate, content, fullurl, imgurl, propertyname, type, attachurl, createBy, createDate, updateBy, updateDate,approveBy,approveDate) {
            /* 清空编辑面板 */
            ClearAddPanel();

            /* 更新Title */
            $("#p-title").text('<%= Resources.Lang.Res_Notice_Btn_Edit %>');

            /* 显示编辑内容 */
            $("#input-edit-id").val(noticeid);
            $("#input-edit-title").val(title);
            $('#datapicker-from').attr("value", startdate);
            $('#datapicker-to').attr("value", enddate);
            $("#textarea-edit-content").val(content);
            $("#img-edit").attr("src", ".." + fullurl);
            $("#img-edit").attr("alt", imgurl);
            $("#img-edit").attr("rpath", fullurl); //暂存图片的相对路径
            $("#input-property").val(propertyname);
            /// -- start --
            $("#ddlType").val(type);
            $("#attachment-file").html(attachurl == "null" ? "" : attachurl);
            $("#attachment-file").attr("rpath", attachurl);
            /// --  end  --
            /// -- start --
            $("#spanCreatedBy").html(createBy != "undefined" && createBy != "null" ? createBy : "");           
            $("#spanUpdateBy").html(updateBy != "undefined" && updateBy != "null" ? updateBy : "");
            $("#spanApproveBy").html(approveBy != "undefined" && approveBy != "null" ? approveBy : "");
            $("#spanCreateDate").html(createDate != "undefined" && createDate != "null" ? df(createDate, "yyyy-mm-dd") : "");
            $("#spanUpdateDate").html(updateDate != "undefined" && updateDate != "null" ? df(createDate, "yyyy-mm-dd") : "");
            $("#spanApproveDate").html(approveDate != "undefined" && approveDate != "null" ? df(createDate, "yyyy-mm-dd") : "");
            /// --  end  --
            /* 显示编辑面板 */
            $("#div-add-notice").show();

            /* 隐藏bootgrid */
            $("#tr-grid-property").hide();
            /* 显示property输入框*/
            $("#tr-input-property").show();

            /* 隐藏保存新增按钮 */
            $("#button-new-save").hide();
            $("#button-edit-save").show();

            /* 滚动屏幕 */
            $('html, body').animate({ scrollTop: 600 }, '500');
        };

        /* 显示删除窗口 */
        function showdeletemodal(noticeid) {
            $("#button-delete-notice").attr("onclick", "deletenotice(" + noticeid + ")");
            $("#doc-modal-delete").modal({ closeViaDimmer: false });
        };

        /* 删除Notice */
        function deletenotice(noticeid) {
            ClearAddPanel();
            $.ajax({
                cache: false,
                type: "POST",
                url: "./NoticeMaintenance.aspx",
                data: {
                    "action": "deletenotice",
                    "noticeid": noticeid
                },
                success: function (data) {
                    try {
                        if (data == "ok") {
                            /* 提交成功，关闭窗口 */
                            $("#doc-modal-delete").modal('close');
                            showAlert("Notice", "Delete Success");
                            /* 刷新bootgrid */
                            search();
                        }
                        else {
                            alert("fail");
                            $("#doc-modal-delete").modal('close');
                        }
                    } catch (ex) {
                        alert("fail");
                        $("#doc-modal-delete").modal('close');
                    }
                },
                error: function (data) {
                    alert("fail");
                    $("#doc-modal-delete").modal('close');
                }
            });
        };

        /* 清空edit窗口内容 */
        function ClearAddPanel() {
            /// -- start --
            $("#ddlType").val("-1");
            $("#attachment-file").empty();
            $("#attachment-file").attr("alt", "");
            $("#attachment-file").attr("rpath", "");
            /// --  end  --
            $("#input-edit-id").val("");
            $("#input-edit-title").val("");
            $("#textarea-edit-content").val("");
            $("#img-edit").attr("src", "");
            $("#img-edit").attr("alt", "");
            $("#img-edit").attr("rpath", ""); // relative path
            $("#file-list").empty();
            /* 清空upload */
            var file = $('#doc-form-file');
            file.after(file.clone().val(""));
            file.remove();
            /* 清空datapicker */
            $('#datapicker-from').attr("value", "");
            $('#datapicker-to').attr("value", "");
            /* 清空复选框 */
            $("#grid-property").bootgrid("deselect");
            /* 清空property输入框 */
            $("#input-property").val("");
            /* 隐藏edit面板 */
            $("#div-add-notice").hide();
        };

        /// 上传附件
        function uploadAttachment() {
            var attachFile = new FormData();
            attachFile.append("file", $("#attachment")[0].files[0]);
            attachFile.append("action", "upload");
            attachFile.append("isAttachment", 'Y'); //如果是附件，要加入‘Y’这个标识，避免后台期作为图片来处理。

            /// -- start -- 屏蔽保存按钮，上传完成后放开
            $("#button-new-save").attr("disabled", "disabled").text("uploading...");
            $("#button-edit-save").attr("disabled", "disabled").text("uploading...");
            /// --  end  --       
            $.ajax({
                url: "./NoticeMaintenance.aspx",
                type: "POST",
                data: attachFile,
                contentType: false, /* 必须false才会避开jQuery对 formdata 的默认处理 XMLHttpRequest会对 formdata 进行正确的处理 */
                processData: false, /* 必须false才会自动加上正确的Content-Type */
                success: function (data) {
                    if (checknotnull(data) == true) {
                        var json = jQuery.parseJSON(data);
                        if (json.data == "ok") {
                            var fileNames = '<span class="am-badge">' + $("#attachment")[0].files[0].name + ' upload success!' + '</span> ';
                            $('#attachment-file').html(fileNames);
                            $('#attachment-file').attr("alt", json.attachname);
                            $('#attachment-file').attr("rpath", json.attachurl);
                        }
                        else {
                            alert("upload attachment file fail: " + json.data);
                        }
                    }
                    $("#button-new-save").removeAttr("disabled").text("Save New Notice");
                    $("#button-edit-save").removeAttr("disabled").text("Save Edit Notice");
                    /// --  end  --       
                },
                error: function (data) {
                    $("#button-new-save").removeAttr("disabled").text("Save New Notice");
                    $("#button-edit-save").removeAttr("disabled").text("Save Edit Notice");
                    AlertJqueryAjaxError(data);
                }
            });
        }
        
        /* 上传文件 */
        function uploadfile() {        
            var formData = new FormData();
            //console.log($("#doc-form-file").val());
            formData.append("file", $("#doc-form-file")[0].files[0]);
            formData.append("action", "upload");

            /// -- start -- 屏蔽保存按钮，上传完成后放开
            $("#button-new-save").attr("disabled", "disabled").text("uploading...");
            $("#button-edit-save").attr("disabled", "disabled").text("uploading...");
            /// --  end  --
            $.ajax({
                url: "./NoticeMaintenance.aspx",
                type: "POST",
                data: formData,
                contentType: false, /* 必须false才会避开jQuery对 formdata 的默认处理 XMLHttpRequest会对 formdata 进行正确的处理 */
                processData: false, /* 必须false才会自动加上正确的Content-Type */
                success: function (data) {
                    if (checknotnull(data) == true) {
                        var json = jQuery.parseJSON(data);
                        if (json.data == "ok") {
                            var fileNames = '<span class="am-badge">' + $("#doc-form-file")[0].files[0].name + ' upload success!' + '</span> ';
                            $('#file-list').html(fileNames);
                            var imgurl = json.imgurl;
                            $("#img-edit").attr("src", '..' + imgurl + "?" + Math.floor((Math.random() * 1000)).toString());
                            var imgname = json.imgname;
                            $("#img-edit").attr("alt", imgname);
                            $("#img-edit").attr("rpath", imgurl); //相对路径
                        }
                        else {
                            alert("upload image fail: " + json.data);
                        }
                    }
                    $("#button-new-save").removeAttr("disabled").text("Save New Notice");
                    $("#button-edit-save").removeAttr("disabled").text("Save Edit Notice");
                },
                error: function (data) {
                    $("#button-new-save").removeAttr("disabled").text("Save New Notice");
                    $("#button-edit-save").removeAttr("disabled").text("Save Edit Notice");
                    AlertJqueryAjaxError(data);
                }
            });

        };

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">    
    <div class="am-g am-g-collapse">
        <%-- 外边框 --%>
        <div class="am-u-sm-12">
            <%-- 深色标题 --%>
            <div class="am-u-sm-12 global_table_head global_color_navyblue">
                <p>
                    <%= Resources.Lang.Res_NoticeMaintenance %>
                </p>
            </div>
            <%-- 内容 --%>
            <div class="am-u-sm-12 global_table_body global_color_lightblue">
                <%-- 边距内内容 --%>
                <div class="am-u-sm-12">                    
                    <%--bootgrid--%>
                    <table id="grid-data" class="table table-condensed table-hover table-striped global-bootgrid" data-toggle="bootgrid">
                        <thead>
                            <tr>
                                <th data-column-id="PROPERTY_CODE" data-header-css-class="boot-c-property" data-align="center"
                                    data-header-align="center">
                                    <%= Resources.Lang.Res_Notice_Grid_Property%>
                                </th>
                                <th data-column-id="TITLE" data-header-css-class="boot-header-1" data-align="left"
                                    data-header-align="center">
                                   <%= Resources.Lang.Res_Notice_Grid_Subject%>
                                </th>
                                <th data-column-id="STARTDATE" data-header-css-class="boot-c-data" data-align="center"
                                    data-header-align="center">
                                    <%= Resources.Lang.Res_Notice_Grid_Start%>
                                </th>
                                <th data-column-id="ENDDATE" data-header-css-class="boot-c-data" data-align="center"
                                    data-header-align="center">
                                   <%= Resources.Lang.Res_Notice_Grid_End%> 
                                </th>
                                <th data-column-id="APPROVE" data-formatter="approve" data-header-css-class="boot-c-data"
                                    data-align="center" data-header-align="center">
                                   <%= Resources.Lang.Res_Notice_Grid_Approve%> 
                                </th>
                                <th data-column-id="Type" data-formatter="type" data-header-css-class="boot-c-data"
                                    data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Notice_Grid_Type%> 
                                </th>
                                <th data-column-id="IMGURLLARGE" data-formatter="IMGURLLARGE" data-header-css-class="boot-header-1"
                                    data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Notice_Grid_Image%> 
                                </th>
                                <th data-column-id="edit" data-formatter="edit" data-header-css-class="boot-c-edit"
                                    data-align="center" data-header-align="center">
                                    <%= Resources.Lang.Res_Notice_Grid_Edit%> 
                                </th>
                            </tr>
                        </thead>
                    </table>
                </div>                
                <div class="am-u-sm-12">
                    <button id="button-add" type="button" class="am-btn am-btn-secondary am-round button-all" onclick="ShowAddPanel()" style="float: left"><%= Resources.Lang.Res_Notice_Btn_New%> </button> 
                </div>
                <div class="am-u-sm-12" style="height: 20px;">
                </div>
            </div>
            <div class="am-u-sm-12" style="height: 20px;">
            </div>                
            <%-- 新增Notice --%>            
            <div id="div-add-notice" class="am-g"  hidden="hidden">
                <%-- 深色标题 --%>
                <div class="am-u-sm-12 global_table_head global_color_navyblue">
                    <p id="p-title"></p>
                </div>
                <%-- 内容 --%>
                <div class="am-u-sm-12 global_table_body global_color_lightblue">
                    <%-- 边距内内容 --%>
                    <div class="am-u-sm-12">                        
                        <table style="width: 100%;">
                            <tr>
                                <td style="width: 100px;"><div style="width:100px;"></div>
                                </td>
                                <td>
                                </td>
                            </tr>
                            <tr id="tr-noticeid" hidden="hidden">
                                <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1">
                                        Notice ID:</label> 
                                </td>
                                <td>
                                    <input id="input-edit-id" type="text" class="am-form-field" readonly />
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"><%= Resources.Lang.Res_Notice_Grid_Subject%>:</label> 
                                </td>
                                <td style="width:100%">
                                    <input id="input-edit-title" type="text" maxlength="30" class="am-form-field" style="width: 300px;" />
                                </td>                              
                            </tr>
                            <!-- start #19172-->
                            <tr>
                                <td style="text-align: right; padding-right: 10px;">
	                                <label class="g-font-1"><%= Resources.Lang.Res_Notice_Grid_Type%></label>
	                            </td>
                                <td>
                                    <select id="ddlType" class="am-form-field " style="width:400px;" > 
	                                    <option value="-1">(Please select...)</option>
	                                    <option value="S">Notice</option>
	                                    <option value="P">Event</option>
	                                    <option value="C">Carpark Info</option>	  
	                                    <option value="V">Services</option>                                        
                                    </select>
                                </td>
                            </tr>
                            <!--  end  -->
                            <tr>
                                <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"><%= Resources.Lang.Res_Notice_Grid_Start%>:</label>
                                </td>
                                <td>
                                    <input id="datapicker-from" type="text" class="am-form-field" style="width: 300px;"  readonly />
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"> <%= Resources.Lang.Res_Notice_Grid_End%>:</label>
                                </td>
                                <td>
                                    <input id="datapicker-to" type="text" class="am-form-field" style="width: 300px;" readonly />
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"><%= Resources.Lang.Res_Notice_Content%>:</label>
                                </td>
                                <td>
                                    <textarea id="textarea-edit-content" class="textarea" maxlength="500" style="width: 100%;
                                        height: 200px"></textarea>
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"><%= Resources.Lang.Res_Notice_Grid_Image%>:</label>
                                </td>
                                <td style="padding-top: 10px;">
                                    <div class="am-form-group am-form-file" style="float: left;">
                                        <button type="button" class="am-btn am-btn-secondary am-btn-sm"> 
                                            <i class="am-icon-cloud-upload"></i><%= Resources.Lang.Res_Notice_ChooseUploadImage%></button><span class="small">(仅支持JPG/JPEG/PNG/BMP格式图片)</span>
                                        <input id="doc-form-file" type="file" name="file" onchange="uploadfile()" />                                        
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                </td>
                                <td>
                                    <label class="g-font-1">
                                        <%= Resources.Lang.Res_Notice_ImageResolutionIs%> 343*125</label> 
                                </td>
                            </tr>                            
                            <tr>
                                <td>
                                </td>
                                <td>
                                    <div id="file-list" style="float: left;">
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                </td>
                                <td>
                                    <img id="img-edit" style="float: left;" alt="" width="343px" height="125px" src="" /> <%--<img src="/upload/1709061511160463.jpg" />--%>
                                </td>
                            </tr>
                            <!-- start -->
                            <tr>
                                <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1">
                                         <%= Resources.Lang.Res_Notice_Attachment%>:</label> 
                                </td>
                                <td style="padding-top: 10px;">
                                    <div class="am-form-group am-form-file" style="float: left;">
                                        <button type="button" class="am-btn am-btn-secondary am-btn-sm">
                                            <i class="am-icon-cloud-upload"></i><%= Resources.Lang.Res_Notice_ChooseUploadFile%></button>
                                        <input id="attachment" type="file" name="file" onchange="uploadAttachment()" />
                                    </div>
                                </td>                                                              
                            </tr>
                            <tr>
                                <td></td>
                                <td id="attachment-file" style="text-align: left;" alt="" rpath=""></td>                                                                                        
                            </tr>
                            <!--  end  -->
                            <tr id="tr-input-property">
                                <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"> Property:</label>
                                </td>
                                <td>
                                    <input id="input-property" type="text" class="am-form-field" maxlength="50" readonly />
                                </td>
                            </tr>
                            <tr id="tr-grid-property">
                                <td style="vertical-align: top; text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"><%= Resources.Lang.Res_Notice_SelectProperty%>:</label> 
                                </td>
                                <td>
                                    <%--bootgrid--%>
                                    <table id="grid-property" class="table table-condensed table-hover table-striped global-bootgrid"
                                        data-toggle="bootgrid">
                                        <thead>
                                            <tr>
                                                <th data-column-id="PROPERTY_NAME" data-align="center" data-header-align="center">
                                                    Property Name
                                                </th>
                                                <th data-column-id="PROPERTY_CODE" data-identifier="true" data-align="center" data-header-align="center">
                                                    Property Code
                                                </th>
                                            </tr>
                                        </thead>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                               <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"><%= Resources.Lang.Res_Notice_CreateBy%>:</label>
                                </td>
                                <td style="padding-top: 10px;">
                                     <div class="am-form-group am-form-file" style="float: left;">
                                        <span id="spanCreatedBy"></span>&nbsp;&nbsp;&nbsp;&nbsp;<span id="spanCreateDate"></span>
                                    </div>
                                </td>    
                            </tr>
                            <tr>
                               <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"><%= Resources.Lang.Res_Notice_UpdateBy%>:</label> 
                                </td>
                                <td style="padding-top: 10px;">
                                     <div class="am-form-group am-form-file" style="float: left;">
                                        <span id="spanUpdateBy"></span>&nbsp;&nbsp;&nbsp;&nbsp;<span id="spanUpdateDate"></span>
                                    </div>
                                </td>    
                            </tr>
                            <tr>
                               <td style="text-align: right; padding-right: 10px;">
                                    <label class="g-font-1"><%= Resources.Lang.Res_Notice_ApproveBy%>:</label>
                                </td>
                                <td style="padding-top: 10px;">
                                     <div class="am-form-group am-form-file" style="float: left;">
                                        <span id="spanApproveBy"></span>&nbsp;&nbsp;&nbsp;&nbsp;<span id="spanApproveDate"></span>
                                    </div>
                                </td>    
                            </tr>
                            <tr>
                                <td>                                    
                                </td>
                                <td>
                                    <button id="button-edit-save" type="button" class="am-btn am-btn-secondary am-round button-all"
                                        onclick="SaveEditNotice()" style="float: right">
                                        <%= Resources.Lang.Res_Notice_SaveEdit%></button>
                                    <button id="button-new-save" type="button" class="am-btn am-btn-secondary am-round button-all"
                                        onclick="SaveNewNotice()" style="float: right">
                                        <%= Resources.Lang.Res_Notice_SaveNew%></button>
                                </td>
                            </tr>
                            </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <%-- 确认删除模态窗口 --%>
    <div class="am-modal am-modal-no-btn" tabindex="-1" id="doc-modal-delete">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                Delete Notice <a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>
                    &times;</a>
            </div>
            <div class="am-modal-bd">
                <div class="am-modal-bd-my">
                    <%-- 表单区域 --%>
                    <div class="am-u-sm-12 am-g-collapse">
                        <table style="width: 100%; padding: 20px;">
                            <tr>
                                <td>
                                    Delete this notice?
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <button id="button-delete-notice" type="button" class="am-btn am-btn-secondary am-round button-all">
                                        Delete</button>
                                </td>
                            </tr>
                            </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <%-- 预览模态窗口 --%>
    <div class="am-modal am-model-no-btn" tabindex="-1" id="doc-modal-preview">
        <div class="am-modal-dialog am-round">
            <div class="am-modal-hd am-modal-hd-my">
                Preview <a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>&times;</a>
            </div>
            <div class="am-modal-bd">
                <section class="am-margin-top-sm">                    
                    <div class="media">
                        <div class="media-left">
                            <img class="media-object" src="" alt="..." style="width:150px;height:150px">
                        </div>
                        <div class="media-body">
                            <h4 class="media-heading text-left"></h4>
                            <div  class="text-left"><span></span></div>
                            <p class="text-left am-margin-top-0"></p>                            
                        </div>
                        <div class="media-bottom">
                            <a id="attach" href="#">Attachment</a>
                        </div>
                    </div>         
                </section>                
            </div>            
        </div>
    </div>
    <%-- Prompt窗口 --%>
    <div class="am-modal am-modal-alert" tabindex="-1" id="doc-modal-prompt">
        <div class="am-modal-dialog">
            <div class="am-modal-hd am-modal-hd-my">
                <label id="label-prompt-title">
                </label>
            </div>
            <div class="am-modal-bd">
                <div class="am-modal-bd-my">
                    <label id="label-prompt-content" style="font-weight: normal;"></label>
                    <div class="imagelist" style="height:280px; overflow:scroll;"></div>  
                </div>              
            </div>
            <div class="am-modal-footer">
                <span class="am-modal-btn" data-am-modal-confirm>Yes</span>
                <span class="am-modal-btn"  data-am-modal-cancel>No</span>
            </div>
        </div>            
    </div> 
</asp:Content>
