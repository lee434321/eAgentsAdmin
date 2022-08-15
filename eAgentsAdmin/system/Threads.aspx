<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Threads.aspx.cs" Inherits="PropertyOneAppWeb.system.Threads" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Threads</title>
    <script src="../js/jQuery/jquery-3.2.1.min.js" type="text/javascript"></script>
    <link rel="Stylesheet" href="../js/bootstrap-4.3.1-dist/css/bootstrap.min.css" />
    <script src="../js/bootstrap-4.3.1-dist/js/bootstrap.min.js" type="text/javascript"></script>
    <!-- vue.js -->
    <script src="../js/vue.js" type="text/javascript"></script>
    <script src="https://cdn.staticfile.org/vue-resource/1.5.1/vue-resource.min.js" type="text/javascript"></script>
    <script src="https://unpkg.com/axios/dist/axios.min.js"></script>

    <style type="text/css">
     html{
         font-size:62.5%;
     }
     body
     {
         font-size:1.4rem;
         }
    </style>
</head>
<body scroll="no">
    <form id="form1" runat="server">        
    </form>
    <div class="container-fluid" id="vu-det">
        <div class="row">
            <div class="col-3">
                <div class="card mt-3">
                    <div class="card-header text-info">
                        List of threads
                    </div>
                    <ul class="list-group list-group-flush " style="height:400px; overflow-y:scroll; scrollbar:none;}">     
                        <li class="list-group-item" v-for="item in threadsInDb" v-on:click="tick(item)" v-bind:class="item.Id==threadTicked.Id?'active':''" >{{item.Title}}</li>
                    </ul>
                </div>
            </div>
            <div class="col-9">
                <div class="d-flex flex-column">
                    <div class="d-flex flex-row bd-highlight mb-3">
                        <div class="p-1 flex-fill">
                            <div class="form-group">
                                <label for="inputUserContent">Feedback Title: </label>                                                 
                                <input class="form-control" type="text" id="input-user-title" v-bind:value="threadTicked.Title"/>  
                            </div>
                            <div class="form-group">
                                <label for="inputUserContent">Feedback Content: </label>                                                 
                                <textarea class="form-control"  id="text-user-content"cols="5" rows="3"></textarea>
                            </div>
                            <button class="btn btn-dark" id="btnBuild" v-on:click="build">New Feedback</button>        
                            <button class="btn btn-primary" id="btnSend" v-on:click="send">Send</button>        
                        </div>
                        <div class="p-1 flex-fill bd-highlight">
                            <div class="form-group">
                                <label for="selectUserContent">Backend User:</label>                    
                                <select class="form-control" id="select-User-Content">
                                    <option value="-1">-- Please Select --</option>
                                    <option v-for="item in backendUsers" v-bind:value="item.UserId">{{item.LoginName}}</option>
                                </select>
                            </div>
                            <div class="form-group">
                                <label for="inputUserContent">Backend User Input Content: </label>                    
                                <textarea class="form-control"  id="text-sysUser-Content"cols="5" rows="3" ></textarea>
                            </div>
                            <button class="btn btn-primary" id="btnUserSend" v-on:click="reply">Reply</button>         
                        </div>  
                    </div>                   
                    <div class="p-1">
                        <div class="card">                          
                            <div class="card-header" data-toggle="collapse" href="#card-content">
                                <h5>Chats</h5>
                            </div>
                            <div class="card-body collapse" id="card-content" >
                                <div id="wrapper" style="height:250px; overflow:auto;">
                                    <div id="scrollContent" >
                                        <p class="card-text" v-for="item in threadsInChat" v-bind:class='isBackend(item.Create_By)?"text-right text-primary":"text-left"'>{{chatExchange(item.Create_By,item.Content)}}</p>
                                    </div>                                
                                </div>
                            </div>
                        </div>
                        <v-datable url="threads.aspx"></v-datable>
                    </div>
                </div>
            </div>            
        </div>  
        <div class="row">
            <div class="col-3">
                <div id="chessboard" style="width:160px;height:160px">                      
                </div>    
            </div>
            <div class="col-9">
             
                <%--<v-datable url="data.json" ></v-datable>--%>    
                    
            </div>
        </div>        
    </div>
    <script src="app.js" type="text/javascript"></script>    
    <script type="text/javascript">
        /*
        for (var i = 0; i < 8; i++) {
            for (var j = 0; j < 8; j++) {
                var $e = $("<div></div>");
                $e.css("width", "20px").css("height", "20px").css("float", "left").addClass("bg-light border border-info");
                $e.attr("id", "i" + i + "j" + j);
                $("#chessboard").append($e);
            }
        }
        var arr = [1, 4, 7, 5, 2, 6, 3, 0];
        for (var k = 0; k < 8; k++) {
            var sel = "#" + "i" + k + "j" + arr[k];
            $(sel).removeClass("bg-light").css("background", "gray");
        }
        */
    </script>    
</body>
</html>
