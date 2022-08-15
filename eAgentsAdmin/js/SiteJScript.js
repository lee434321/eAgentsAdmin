/* 获取制定查询字符串的值 */
function qs(search, key) {    
    var patt = /(\w*)=([^&]+)(?=&|$)/g;
    var q = decodeURI(search).match(patt);
    if (q) {
        for (var i = 0; i < q.length; i++) {
            var kv = q[i].split("=");
            if (kv[0] == key) {
                return kv[1];
            }
        }
    }
    return "";
}

/* 去掉前后的空格 */
function trim(v) {   
    if (v)
        return v.replace(/(^\s*)|(\s*$)/g, "");
    else
        return "";
}
/* 遮罩层 */
function mask(selector, t) { //s=selector t=turn(on,off)
    $target = $(selector);
    if (t == "on") {
        $mask = $("<div class='mask'></div>").width($target.width()).height($target.height()).css("z-index", 100);
        $mask.css("position", "absolute").css("background", "gray");
        $mask.css("filter", "alpha(opacity=35)").css("opacity", "0.35");
        $mask.css("posLeft","0");
        $target.append($mask);
    } else {
        $target.find(".mask").remove();
    }
}

//修改“dd-mm-yyyy”格式的日期到 "yyyy-mm-dd"
function dmyr(v) {
    var out = "";
    if (v) {
        var arr = v.split('-');
        out = arr[2] + "-";
        out += arr[1] + "-";
        out += arr[0];
    } else {         
    }    
    return out;
}

//日期格式转换
function df(v, f) {
    if (v) {
        var d = new Date(v);
        var yy = d.getFullYear();
        var mm = d.getMonth() + 1;
        var dd = d.getDate();

        var hh = 0;
        var mi = 0;
        var ss = 0;
        if (v.length > 10) {
            hh = d.getHours();
            mi = d.getMinutes();
            ss = d.getSeconds();
        }
        var od = "";
        switch (f) {
            case "yyyy-mm-dd":
                od += yy.toString() + "-";
                od += (mm >= 10 ? mm.toString() : "0" + mm.toString()) + "-";
                od += (dd >= 10 ? dd.toString() : "0" + dd.toString());
                break;
            case "yyyy-mm-dd hh:mi":
                od += yy.toString() + "-";
                od += (mm >= 10 ? mm.toString() : "0" + mm.toString()) + "-";
                od += (dd >= 10 ? dd.toString() : "0" + dd.toString()) + " ";
                od += (hh >= 10 ? hh.toString() : "0" + hh.toString()) + ":";
                od += (mi >= 10 ? mi.toString() : "0" + mi.toString());
                break;
            case "yyyy-mm-dd hh:mi:ss":
                od += yy.toString() + "-";
                od += (mm >= 10 ? mm.toString() : "0" + mm.toString()) + "-";
                od += (dd >= 10 ? dd.toString() : "0" + dd.toString()) + " ";
                od += (hh >= 10 ? hh.toString() : "0" + hh.toString()) + ":";
                od += (mi >= 10 ? mi.toString() : "0" + mi.toString()) + ":";
                od += (ss >= 10 ? ss.toString() : "0" + ss.toString());
                break;
            case "dd-mm-yyyy":
                od += (dd >= 10 ? dd.toString() : "0" + dd.toString()) + "-";
                od += (mm >= 10 ? mm.toString() : "0" + mm.toString()) + "-";
                od += yy.toString();
                break;
            case "dd-mm-yyyy hh:mi":
                od += (dd >= 10 ? dd.toString() : "0" + dd.toString()) + "-";
                od += (mm >= 10 ? mm.toString() : "0" + mm.toString()) + "-";
                od += yy.toString() + " ";
                od += (hh >= 10 ? hh.toString() : "0" + hh.toString()) + ":";
                od += (mi >= 10 ? mi.toString() : "0" + mi.toString());
                break;
            case "dd-mm-yyyy hh:mi:ss":
                od += (dd >= 10 ? dd.toString() : "0" + dd.toString()) + "-";
                od += (mm >= 10 ? mm.toString() : "0" + mm.toString()) + "-";
                od += yy.toString() + " ";
                od += (hh >= 10 ? hh.toString() : "0" + hh.toString()) + ":";
                od += (mi >= 10 ? mi.toString() : "0" + mi.toString()) + ":";
                od += (ss >= 10 ? ss.toString() : "0" + ss.toString());
                break;
            case "hh:mi dd-mm-yyyy": // 由derek提出
                od += (hh >= 10 ? hh.toString() : "0" + hh.toString()) + ":";
                od += (mi >= 10 ? mi.toString() : "0" + mi.toString()) + " ";
                od += (dd >= 10 ? dd.toString() : "0" + dd.toString()) + "-";
                od += (mm >= 10 ? mm.toString() : "0" + mm.toString()) + "-";
                od += yy.toString();
                break;
            default:
                od = d.toLocaleString();
        }
        return od;
    } else {
        return "";
    }
}
/*浏览器类型*/
function wb() { //which browser
    var userAgent = navigator.userAgent; //取得浏览器的userAgent字符串
    if (userAgent.indexOf("Opera") > -1) {          //判断是否Opera浏览器
        return "Opera";
    } else if (userAgent.indexOf("Firefox") > -1) { //判断是否Firefox浏览器
        return "FF";
    } else if (userAgent.indexOf("Chrome") > -1) {  //判断是否Chrome浏览器
        return "Chrome";
    } else if (userAgent.indexOf("Safari") > -1) {  //判断是否Safari浏览器
        return "Safari";
    } else if (!!window.ActiveXObject || "ActiveXObject" in window) {  //判断是否IE浏览器
        return "IE";
    } else
        return "";
}

/*确认提示,基于bootstrap modal dialog*/
function promptEx(a, m, c) {//a=args; m:message; c=callback
    var modal_id = "prompt-modal" + Math.floor(Math.random() * 1000 % 1000);
    // modal
    var modal = document.createElement("div"); modal.setAttribute("class", "modal"); modal.setAttribute("tabindex", "-1"); modal.setAttribute("id", modal_id);
    // modal-dialog
    var modal_dialog = document.createElement("div"); modal_dialog.setAttribute("class", "modal-dialog modal-sm");
    // modal-content
    var modal_content = document.createElement("div"); modal_content.setAttribute("class", "modal-content");
    // modal-header
    var modal_header = document.createElement("div"); modal_header.setAttribute("class", "modal-header");

    var el_h5 = document.createElement("h5"); el_h5.setAttribute("class", "modal-title"); el_h5.innerHTML = "Hint";
    var el_btn_X = document.createElement("button"); el_btn_X.setAttribute("class", "close"); el_btn_X.setAttribute("type", "button");
    document.createAttribute("data-dismiss");
    el_btn_X.setAttribute("data-dismiss", "modal");
    var el_span = document.createElement("span"); el_span.setAttribute("aria-hidden", "true"); el_span.innerHTML = "&times;"
    //modal-body
    var modal_body = document.createElement("div"); modal_body.setAttribute("class", "modal-body");
    var el_p_msg = document.createElement("p"); el_p_msg.innerHTML = m;
    modal_body.appendChild(el_p_msg);
    //modal-footer
    var modal_footer = document.createElement("div"); modal_footer.setAttribute("class", "modal-footer");
    var el_btn_close = document.createElement("button"); el_btn_close.setAttribute("type", "button");
    el_btn_close.setAttribute("class", "btn btn-secondary btn-sm"); el_btn_close.setAttribute("data-dismiss", "modal");
    el_btn_close.innerHTML = "Close";
    //“确定”按钮及事件
    var el_btn_enter = document.createElement("button"); el_btn_enter.setAttribute("type", "button"); //元素
    el_btn_enter.setAttribute("class", "btn btn-primary btn-sm"); el_btn_close.setAttribute("data-dismiss", "modal");
    el_btn_enter.innerHTML = "Confirm";
    el_btn_enter.addEventListener('click', c); //事件绑定
    el_btn_enter.args = a; //外部数据放在按钮对象中的“args”变量里，事件触发时可以作为事件参数供回调函数使用。

    modal.appendChild(modal_dialog);
    modal_dialog.appendChild(modal_content);
    modal_content.appendChild(modal_header);    
    modal_header.appendChild(el_btn_X);
    el_btn_X.appendChild(el_span);
    modal_header.appendChild(el_h5);
    modal_content.appendChild(modal_body);
    modal_content.appendChild(modal_footer);
    modal_footer.appendChild(el_btn_close);
    modal_footer.appendChild(el_btn_enter);

    if (document.querySelector(".modal[id*=prompt-modal]")) {
        document.querySelectorAll(".modal[id*=prompt-modal]").forEach(function (i) {
            i.remove();
        });
    }

    document.body.appendChild(modal);
    $("#" + modal_id).modal("show");
    el_btn_enter.addEventListener('click', function (e) {
        $("#" + modal_id).modal("hide");
    });
}


/**********************  Modal窗口相关  *********************************/
/* 弹出警告框 */
function showAlert(title, content) {
    $("#label-alert-title").text(title);
    $("#label-alert-content").text(content);
    $("#doc-modal-alert").modal({ closeViaDimmer: false });
};

/* 弹出Prompt框*/
function showPrompt(title, content) {
    $("#label-prompt-title").text(title);
    $("#label-prompt-content").text(content);
    var result = "";
    $("#doc-modal-prompt").modal({ closeViaDimmer: false,
        onConfirm: function (e) {
            result = "Y";
        },
        onCancel: function (e) {
            result = "N";
        }
    });
    return result;
};

/* 关闭后刷新页面的警告框 */
function showAlertRefresh(title, content) {
    $("#label-alert-title-refresh").text(title);
    $("#label-alert-content-refresh").text(content);
    $('#doc-modal-alert-refresh').on('closed.modal.amui', function () {
        location.replace(location.href);
    });
    $("#doc-modal-alert-refresh").modal({ closeViaDimmer: false });
};

/* 弹出Loading */
function showLoading() {
    $("#my-modal-loading").modal({ closeViaDimmer: false });
};

/* 关闭Loading */
function closeLoading() {
    $("#my-modal-loading").modal('close');
};

/* 显示模态窗口 */
function showModal(target, width, closeViaDimmer) {
    $("#" + target).modal({ closeViaDimmer: closeViaDimmer, width: width });
};

/* 关闭模态窗口 */
function closeModal(target) {
    $("#" + target).modal('close');
};

/* 显示Jquery的Ajax错误*/
function AlertJqueryAjaxError(data) {
    alert(data.status + " " + data.statusText + " " + data.responseText);
}
/**********************************************************************/

/* 切换导航图标和文字 */
function changeNav(imgurl, txt) {
    $("#img-nav").attr("src", imgurl);
    $("#label-nav").text(txt);
    $("#p-title").text(txt);
};

/* 将数值转换为待逗号的财务数值 */
function formatMoney(number, places, symbol, thousand, decimal) {
    try {
        number = Number(number);
        places = !isNaN(places = Math.abs(places)) ? places : 2;
        symbol = symbol !== undefined ? symbol : "$";
        thousand = thousand || ",";
        decimal = decimal || ".";
        var negative = number < 0 ? "-" : "",
            i = parseInt(number = Math.abs(+number || 0).toFixed(places), 10) + "",
            j = (j = i.length) > 3 ? j % 3 : 0;
        return symbol + negative + (j ? i.substr(0, j) + thousand : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousand) + (places ? decimal + Math.abs(number - i).toFixed(places).slice(2) : "");
    }
    catch (ex) {
        return 0.00;
    }

};

/* 清除格式中的逗号, 并返回数值格式 */
function clearFormatMoney(str) {
    try {
        var returnValue = str.replace(/,/g, "");
        return parseFloat(returnValue);
    }
    catch (ex) {
        return 0.00;
    }
};

/* 截断文字 */
function limit(str, limit) {
    try {
        if (str != null && str != "") {
            if (str.length <= limit) {
                return str;
            }
            else {
                return str.substr(0, limit) + "...";
            }
        }
        else {
            return "";
        }
    }
    catch (ex) {
        return "";
    }
};

/* 将json中的日期转换为 dd-MM-yyyy等样式 */
Date.prototype.format = function (fmt) {
    var o = {
        "M+": this.getMonth() + 1,                 //月份 
        "d+": this.getDate(),                    //日 
        "h+": this.getHours(),                   //小时 
        "m+": this.getMinutes(),                 //分 
        "s+": this.getSeconds(),                 //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds()             //毫秒 
    };
    if (/(y+)/.test(fmt)) {
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(fmt)) {
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        }
    }
    return fmt;
};

function formatDate(date) {
    try {
        if (date == null || date == "") {
            return "";
        }
        else {
            var oldTime = (new Date(date)).getTime();            
            var curTime = new Date(oldTime).format("dd-MM-yyyy");

            return curTime;
        }
    }
    catch (ex) {
        return "";
    }
};

function formatDateByType(date, type) {
    /* type is "dd-MM-yyyy" or others */
    try {
        var oldTime = (new Date(date)).getTime();
        var curTime = new Date(oldTime).format(type);

        return curTime;
    }
    catch (ex) {
        return "";
    }
};

/**
* 将字符串解析成日期
* @param str 输入的日期字符串，如'2014-09-13'
* @param fmt 字符串格式，默认'yyyy-MM-dd'，支持如下：y、M、d、H、m、s、S，不支持w和q
* @returns 解析后的Date类型日期
*/
function parseDate(str, fmt) {
    fmt = fmt || 'yyyy-MM-dd';
    var obj = { y: 0, M: 1, d: 0, H: 0, h: 0, m: 0, s: 0, S: 0 };
    fmt.replace(/([^yMdHmsS]*?)(([yMdHmsS])\3*)([^yMdHmsS]*?)/g, function (m, $1, $2, $3, $4, idx, old) {
        str = str.replace(new RegExp($1 + '(\\d{' + $2.length + '})' + $4), function (_m, _$1) {
            obj[$3] = parseInt(_$1);
            return '';
        });
        return '';
    });
    obj.M--; // 月份是从0开始的，所以要减去1
    var date = new Date(obj.y, obj.M, obj.d, obj.H, obj.m, obj.s);
    if (obj.S !== 0) date.setMilliseconds(obj.S); // 如果设置了毫秒
    return date;
};

/* 校验是否为空值 空值返回false 非空值返回true*/
function checknotnull(data) {
    try {
        if (data != null && data != "" && data != "undefined" && data != undefined) {
            return true;
        }
        else {
            return false;
        }
    } catch (ex) {
        return false;
    }
};

/* 获取feedback类型 */
function getfeedbacktype(target, typejson) {
    try {
        var jsonData = jQuery.parseJSON(typejson);
        for (var i = 0; i < jsonData.typenum; i++) {
            $("#" + target).append('<option value="' + jsonData.typeinfo[i].id + '">' + jsonData.typeinfo[i].nameeng + '</option>');
        }
    }
    catch (e) {

    }
};

/* 根据typeid返回type名称 */
function getfeedbacktypebyid(id, typejson) {
    try {
        var jsonData = jQuery.parseJSON(typejson);
        /*
        for (var i = 0; i < jsonData.typenum; i++) {
            if (id == jsonData.typeinfo[i].id) {
                return jsonData.typeinfo[i].nameeng;
            }
        }*/

        for (var i = 0; i < jsonData.length; i++) {
            if (id == jsonData[i].FeedbackTypeId)
                return jsonData[i].TypeName;
        }
        return "unknow";
    }
    catch (e) {
        return "unknow";
    }
};

/****************** form相关js ***************************/
/* 显示form警告 */
function showformalert(divid, labelid, str) {
    $("#" + labelid).text(str);
    $("#" + divid).show(100);
};

/* 隐藏form警告 */
function hideformalert(divid, labelid) {
    $("#" + labelid).text("");
    $("#" + divid).hide(100);
};

/* 校验两次输入是否一致 */
function checkinputsame(txt1, txt2) {
    if (checknotnull(txt1) && checknotnull(txt2)) {
        if (txt1 == txt2) {
            return true;
        }
    }
    return false;
};

/* 校验密码：只能输入6-20个字母、数字、下划线 */
function isPasswd(s) {
    var patrn = /^(\w){6,20}$/;
    if (!patrn.exec(s)) return false
    return true
};
/*********************************************************/

//单引号包裹，外层是双引号
function q1safe(v) {
    if (v) {
        var o = v.replace(/"/g, "&#34;").replace(/\'/g, "\\\'");
        o = o.replace(/\n/g, "\\n");
        return o;
    } else
        return "";
}

/****************** 用户校验相关 **************************/
/* 初始化用户校验界面 */
function InitUserCheck() {
    /* 初始化日历 */
    $('#datapicker-q1').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });
    $('#datapicker-q2').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });

    /* 添加问题 */
    AddQuestions("select-question-1");

    /* 问题1事件 */
    $("#select-question-1").on('change', function () {
        ChangeQuestion('select-question-1', 'select-question-2')
        $("#select-question-2").selected('enable');
        $("#input-q2").prop("disabled", false);

        var val = $("#select-question-1").val();
        if (val == "ed") {
            $("#td-input-q1").hide();

            $("#datapicker-q1").remove();
            $("#td-datapicker-q1").append('<input id="datapicker-q1" type="text" class="am-form-field" placeholder="DD-MM-YYYY" readonly />');
            $('#datapicker-q1').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });
            $("#td-datapicker-q1").show();
        }
        else {
            $("#input-q1").val("");
            $("#td-input-q1").show();
            $("#td-datapicker-q1").hide();
        }

        /* 清除问题2的内容 */
        $("#input-q2").val("");
        $("#datapicker-q2").remove();
        $("#td-datapicker-q2").append('<input id="datapicker-q2" type="text" class="am-form-field" placeholder="DD-MM-YYYY" readonly />');
        $('#datapicker-q2').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });
    });

    /* 问题2事件 */
    $("#select-question-2").on('change', function () {
        var val = $("#select-question-2").val();
        if (val == "ed") {
            $("#td-input-q2").hide();

            $("#datapicker-q2").remove();
            $("#td-datapicker-q2").append('<input id="datapicker-q2" type="text" class="am-form-field" placeholder="DD-MM-YYYY" readonly />');
            $('#datapicker-q2').datepicker({ format: 'dd-mm-yyyy', locale: 'en_US' });
            $("#td-datapicker-q2").show();
        }
        else {
            $("#input-q2").val("");
            $("#td-input-q2").show();
            $("#td-datapicker-q2").hide();
        }
    });

    /* 禁用问题2 */
    $("#select-question-2").selected('disable');
    $("#input-q2").prop("disabled", true);
};

/*********************************************************/

/* 通用ajax处理（post方式） */
function httpost2(url, param, rtype, callback) {
    $.ajax({
        url: url,
        method: "POST",
        data: param,
        contentType: rtype != "" ? rtype : 'application/x-www-form-urlencoded; charset=UTF-8',
        dataType: "json",
        async: true,
        success: callback,
        error: callback
    });
}

function md5(string) {
    function md5_RotateLeft(lValue, iShiftBits) {
        return (lValue << iShiftBits) | (lValue >>> (32 - iShiftBits));
    }
    function md5_AddUnsigned(lX, lY) {
        var lX4, lY4, lX8, lY8, lResult;
        lX8 = (lX & 0x80000000);
        lY8 = (lY & 0x80000000);
        lX4 = (lX & 0x40000000);
        lY4 = (lY & 0x40000000);
        lResult = (lX & 0x3FFFFFFF) + (lY & 0x3FFFFFFF);
        if (lX4 & lY4) {
            return (lResult ^ 0x80000000 ^ lX8 ^ lY8);
        }
        if (lX4 | lY4) {
            if (lResult & 0x40000000) {
                return (lResult ^ 0xC0000000 ^ lX8 ^ lY8);
            } else {
                return (lResult ^ 0x40000000 ^ lX8 ^ lY8);
            }
        } else {
            return (lResult ^ lX8 ^ lY8);
        }
    }
    function md5_F(x, y, z) {
        return (x & y) | ((~x) & z);
    }
    function md5_G(x, y, z) {
        return (x & z) | (y & (~z));
    }
    function md5_H(x, y, z) {
        return (x ^ y ^ z);
    }
    function md5_I(x, y, z) {
        return (y ^ (x | (~z)));
    }
    function md5_FF(a, b, c, d, x, s, ac) {
        a = md5_AddUnsigned(a, md5_AddUnsigned(md5_AddUnsigned(md5_F(b, c, d), x), ac));
        return md5_AddUnsigned(md5_RotateLeft(a, s), b);
    };
    function md5_GG(a, b, c, d, x, s, ac) {
        a = md5_AddUnsigned(a, md5_AddUnsigned(md5_AddUnsigned(md5_G(b, c, d), x), ac));
        return md5_AddUnsigned(md5_RotateLeft(a, s), b);
    };
    function md5_HH(a, b, c, d, x, s, ac) {
        a = md5_AddUnsigned(a, md5_AddUnsigned(md5_AddUnsigned(md5_H(b, c, d), x), ac));
        return md5_AddUnsigned(md5_RotateLeft(a, s), b);
    };
    function md5_II(a, b, c, d, x, s, ac) {
        a = md5_AddUnsigned(a, md5_AddUnsigned(md5_AddUnsigned(md5_I(b, c, d), x), ac));
        return md5_AddUnsigned(md5_RotateLeft(a, s), b);
    };
    function md5_ConvertToWordArray(string) {
        var lWordCount;
        var lMessageLength = string.length;
        var lNumberOfWords_temp1 = lMessageLength + 8;
        var lNumberOfWords_temp2 = (lNumberOfWords_temp1 - (lNumberOfWords_temp1 % 64)) / 64;
        var lNumberOfWords = (lNumberOfWords_temp2 + 1) * 16;
        var lWordArray = Array(lNumberOfWords - 1);
        var lBytePosition = 0;
        var lByteCount = 0;
        while (lByteCount < lMessageLength) {
            lWordCount = (lByteCount - (lByteCount % 4)) / 4;
            lBytePosition = (lByteCount % 4) * 8;
            lWordArray[lWordCount] = (lWordArray[lWordCount] | (string.charCodeAt(lByteCount) << lBytePosition));
            lByteCount++;
        }
        lWordCount = (lByteCount - (lByteCount % 4)) / 4;
        lBytePosition = (lByteCount % 4) * 8;
        lWordArray[lWordCount] = lWordArray[lWordCount] | (0x80 << lBytePosition);
        lWordArray[lNumberOfWords - 2] = lMessageLength << 3;
        lWordArray[lNumberOfWords - 1] = lMessageLength >>> 29;
        return lWordArray;
    };
    function md5_WordToHex(lValue) {
        var WordToHexValue = "",
		WordToHexValue_temp = "",
		lByte, lCount;
        for (lCount = 0; lCount <= 3; lCount++) {
            lByte = (lValue >>> (lCount * 8)) & 255;
            WordToHexValue_temp = "0" + lByte.toString(16);
            WordToHexValue = WordToHexValue + WordToHexValue_temp.substr(WordToHexValue_temp.length - 2, 2);
        }
        return WordToHexValue;
    };
    function md5_Utf8Encode(string) {
        string = string.replace(/\r\n/g, "\n");
        var utftext = "";
        for (var n = 0; n < string.length; n++) {
            var c = string.charCodeAt(n);
            if (c < 128) {
                utftext += String.fromCharCode(c);
            } else if ((c > 127) && (c < 2048)) {
                utftext += String.fromCharCode((c >> 6) | 192);
                utftext += String.fromCharCode((c & 63) | 128);
            } else {
                utftext += String.fromCharCode((c >> 12) | 224);
                utftext += String.fromCharCode(((c >> 6) & 63) | 128);
                utftext += String.fromCharCode((c & 63) | 128);
            }
        }
        return utftext;
    };
    var x = Array();
    var k, AA, BB, CC, DD, a, b, c, d;
    var S11 = 7,
	S12 = 12,
	S13 = 17,
	S14 = 22;
    var S21 = 5,
	S22 = 9,
	S23 = 14,
	S24 = 20;
    var S31 = 4,
	S32 = 11,
	S33 = 16,
	S34 = 23;
    var S41 = 6,
	S42 = 10,
	S43 = 15,
	S44 = 21;
    string = md5_Utf8Encode(string);
    x = md5_ConvertToWordArray(string);
    a = 0x67452301;
    b = 0xEFCDAB89;
    c = 0x98BADCFE;
    d = 0x10325476;
    for (k = 0; k < x.length; k += 16) {
        AA = a;
        BB = b;
        CC = c;
        DD = d;
        a = md5_FF(a, b, c, d, x[k + 0], S11, 0xD76AA478);
        d = md5_FF(d, a, b, c, x[k + 1], S12, 0xE8C7B756);
        c = md5_FF(c, d, a, b, x[k + 2], S13, 0x242070DB);
        b = md5_FF(b, c, d, a, x[k + 3], S14, 0xC1BDCEEE);
        a = md5_FF(a, b, c, d, x[k + 4], S11, 0xF57C0FAF);
        d = md5_FF(d, a, b, c, x[k + 5], S12, 0x4787C62A);
        c = md5_FF(c, d, a, b, x[k + 6], S13, 0xA8304613);
        b = md5_FF(b, c, d, a, x[k + 7], S14, 0xFD469501);
        a = md5_FF(a, b, c, d, x[k + 8], S11, 0x698098D8);
        d = md5_FF(d, a, b, c, x[k + 9], S12, 0x8B44F7AF);
        c = md5_FF(c, d, a, b, x[k + 10], S13, 0xFFFF5BB1);
        b = md5_FF(b, c, d, a, x[k + 11], S14, 0x895CD7BE);
        a = md5_FF(a, b, c, d, x[k + 12], S11, 0x6B901122);
        d = md5_FF(d, a, b, c, x[k + 13], S12, 0xFD987193);
        c = md5_FF(c, d, a, b, x[k + 14], S13, 0xA679438E);
        b = md5_FF(b, c, d, a, x[k + 15], S14, 0x49B40821);
        a = md5_GG(a, b, c, d, x[k + 1], S21, 0xF61E2562);
        d = md5_GG(d, a, b, c, x[k + 6], S22, 0xC040B340);
        c = md5_GG(c, d, a, b, x[k + 11], S23, 0x265E5A51);
        b = md5_GG(b, c, d, a, x[k + 0], S24, 0xE9B6C7AA);
        a = md5_GG(a, b, c, d, x[k + 5], S21, 0xD62F105D);
        d = md5_GG(d, a, b, c, x[k + 10], S22, 0x2441453);
        c = md5_GG(c, d, a, b, x[k + 15], S23, 0xD8A1E681);
        b = md5_GG(b, c, d, a, x[k + 4], S24, 0xE7D3FBC8);
        a = md5_GG(a, b, c, d, x[k + 9], S21, 0x21E1CDE6);
        d = md5_GG(d, a, b, c, x[k + 14], S22, 0xC33707D6);
        c = md5_GG(c, d, a, b, x[k + 3], S23, 0xF4D50D87);
        b = md5_GG(b, c, d, a, x[k + 8], S24, 0x455A14ED);
        a = md5_GG(a, b, c, d, x[k + 13], S21, 0xA9E3E905);
        d = md5_GG(d, a, b, c, x[k + 2], S22, 0xFCEFA3F8);
        c = md5_GG(c, d, a, b, x[k + 7], S23, 0x676F02D9);
        b = md5_GG(b, c, d, a, x[k + 12], S24, 0x8D2A4C8A);
        a = md5_HH(a, b, c, d, x[k + 5], S31, 0xFFFA3942);
        d = md5_HH(d, a, b, c, x[k + 8], S32, 0x8771F681);
        c = md5_HH(c, d, a, b, x[k + 11], S33, 0x6D9D6122);
        b = md5_HH(b, c, d, a, x[k + 14], S34, 0xFDE5380C);
        a = md5_HH(a, b, c, d, x[k + 1], S31, 0xA4BEEA44);
        d = md5_HH(d, a, b, c, x[k + 4], S32, 0x4BDECFA9);
        c = md5_HH(c, d, a, b, x[k + 7], S33, 0xF6BB4B60);
        b = md5_HH(b, c, d, a, x[k + 10], S34, 0xBEBFBC70);
        a = md5_HH(a, b, c, d, x[k + 13], S31, 0x289B7EC6);
        d = md5_HH(d, a, b, c, x[k + 0], S32, 0xEAA127FA);
        c = md5_HH(c, d, a, b, x[k + 3], S33, 0xD4EF3085);
        b = md5_HH(b, c, d, a, x[k + 6], S34, 0x4881D05);
        a = md5_HH(a, b, c, d, x[k + 9], S31, 0xD9D4D039);
        d = md5_HH(d, a, b, c, x[k + 12], S32, 0xE6DB99E5);
        c = md5_HH(c, d, a, b, x[k + 15], S33, 0x1FA27CF8);
        b = md5_HH(b, c, d, a, x[k + 2], S34, 0xC4AC5665);
        a = md5_II(a, b, c, d, x[k + 0], S41, 0xF4292244);
        d = md5_II(d, a, b, c, x[k + 7], S42, 0x432AFF97);
        c = md5_II(c, d, a, b, x[k + 14], S43, 0xAB9423A7);
        b = md5_II(b, c, d, a, x[k + 5], S44, 0xFC93A039);
        a = md5_II(a, b, c, d, x[k + 12], S41, 0x655B59C3);
        d = md5_II(d, a, b, c, x[k + 3], S42, 0x8F0CCC92);
        c = md5_II(c, d, a, b, x[k + 10], S43, 0xFFEFF47D);
        b = md5_II(b, c, d, a, x[k + 1], S44, 0x85845DD1);
        a = md5_II(a, b, c, d, x[k + 8], S41, 0x6FA87E4F);
        d = md5_II(d, a, b, c, x[k + 15], S42, 0xFE2CE6E0);
        c = md5_II(c, d, a, b, x[k + 6], S43, 0xA3014314);
        b = md5_II(b, c, d, a, x[k + 13], S44, 0x4E0811A1);
        a = md5_II(a, b, c, d, x[k + 4], S41, 0xF7537E82);
        d = md5_II(d, a, b, c, x[k + 11], S42, 0xBD3AF235);
        c = md5_II(c, d, a, b, x[k + 2], S43, 0x2AD7D2BB);
        b = md5_II(b, c, d, a, x[k + 9], S44, 0xEB86D391);
        a = md5_AddUnsigned(a, AA);
        b = md5_AddUnsigned(b, BB);
        c = md5_AddUnsigned(c, CC);
        d = md5_AddUnsigned(d, DD);
    }
    return (md5_WordToHex(a) + md5_WordToHex(b) + md5_WordToHex(c) + md5_WordToHex(d)).toLowerCase();
}

function primes(n) {
    var outs = [];
    if (n == 2) {
        outs = [1, 2];
    } else {
        for (var i = 1; i <= n; i++) {
            var f = false
            for (var j = 2; j <= Math.floor(i / 2); j++) {
                if (i % j == 0) {
                    f = true;
                    break;
                }
            }
            if (!f) {
                outs.push(i);
            }
        }
    }
    return outs;
}