<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PdfViewer.aspx.cs" Inherits="PropertyOneAppWeb.Web.PdfViewer" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Pdf Viewer</title>
    <meta http-equiv="X-UA-Compatible" content="IE=EDGE" />
    <link rel="stylesheet" href="/js/AmazeUI/assets/css/amazeui.css" />
    <!--[if (gte IE 9)|!(IE)]><!-->
    <script type="text/javascript" src="/js/jQuery/jquery-3.2.1.min.js"></script>
    <!--<![endif]-->
    <!--[if lte IE 8]>
    <script src="/js/jQuery/jquery-1.11.1.min.js"></script>
    <script src="http://cdn.staticfile.org/modernizr/2.8.3/modernizr.js"></script>
    <script src="/js/AmazeUI/assets/js/amazeui.ie8polyfill.min.js"></script>
    <![endif]-->
    <script type="text/javascript" src="/js/AmazeUI/assets/js/amazeui.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            /*
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Web/PdfViewer.aspx",
                data: "action=loadpdf",
                success: function (data) {
                    alert('OK');
                },
                error: function (data) {
                    alert('error');
                }
            });
            */
        });
    </script>
</head>
<body>

</body>
</html>
