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