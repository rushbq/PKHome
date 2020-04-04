<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AuthSearch.aspx.cs" Inherits="mySystem_AuthSearch" %>

<asp:Content ID="myCss" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="myBody" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- Sub Header Start -->
    <div class="grey lighten-3">
        <div class="container">
            <div class="row">
                <div class="col s12 m12 l12">
                    <h5 class="breadcrumbs-title">權限查詢</h5>
                    <ol class="breadcrumb">
                        <li><a href="#!">系統管理</a></li>
                        <li><a href="#!">權限管理</a></li>
                        <li class="active">權限查詢</li>
                    </ol>
                </div>
            </div>
        </div>
    </div>
    <!-- Sub Header End -->
    <!-- Body Content Start -->
    <div class="row">
        <div class="col s12">
            <div class="card grey">
                <div class="card-content white-text">
                    <h5>權限查詢</h5>
                </div>
                <div class="card-content grey lighten-5">
                    <!-- filter start -->
                    <asp:PlaceHolder ID="ph_Require" runat="server" Visible="false">
                        <div class="card-panel red white-text">
                            <h5><i class="material-icons">warning</i>&nbsp;請確認以下條件是否達成:</h5>
                            <ul>
                                <li>請選擇資料庫</li>
                            </ul>
                        </div>
                    </asp:PlaceHolder>
                    <div class="row">
                        <div class="col s6">
                            <label for="MainContent_ddl_DB">選擇資料庫</label>
                            <asp:DropDownList ID="ddl_DB" runat="server" CssClass="browser-default red-text text-darken-1">
                            </asp:DropDownList>
                        </div>
                        <div class="col s6 right-align">
                            <asp:LinkButton ID="lbtn_Go" runat="server" CssClass="btn waves-effect waves-light blue" OnClick="lbtn_Go_Click"><i class="material-icons right">playlist_add_check</i>帶出權限表</asp:LinkButton>
                        </div>
                    </div>
                    <!-- filter end -->

                    <!-- setting start -->

                    <div class="row">
                        <div class="col s12 m6">
                            <asp:PlaceHolder ID="ph_treeHtml" runat="server">
                                <blockquote class="color-blue">
                                    <h6>權限表 (點選功能以查詢)</h6>
                                    <div>
                                        <button type="button" id="treeOpen" class="btn green lighten-1"><i class="material-icons right">sort</i>展開所有節點</button>
                                    </div>
                                    <div id="authList" class="ztree"></div>
                                </blockquote>
                            </asp:PlaceHolder>
                        </div>
                        <div class="col s12 m6">
                            <asp:PlaceHolder ID="ph_treeUser" runat="server">
                                <blockquote class="color-green">
                                    <h6>人員清單 (點選權限表以查詢)</h6>
                                    <div id="userList" class="ztree"></div>
                                </blockquote>
                            </asp:PlaceHolder>
                        </div>
                    </div>

                    <!-- setting end -->
                </div>
            </div>
        </div>
    </div>
    <!-- Body Content End -->

</asp:Content>
<asp:Content ID="myBottom" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="myScript" ContentPlaceHolderID="ScriptContent" runat="Server">
    <asp:PlaceHolder ID="ph_treeJS" runat="server">
        <link rel="stylesheet" href="<%=fn_Param.WebUrl %>plugins/zTree/css/style.min.css" />
        <script src="<%=fn_Param.WebUrl %>plugins/zTree/jquery.ztree.core-3.5.min.js"></script>
        <script>
            //--- zTree 設定 Start ---
            var setting = {
                view: {
                    dblClickExpand: false   //已使用onclick展開,故將雙擊展開關閉                    
                },
                callback: {
                    onClick: MMonClick
                },
                data: {
                    simpleData: {
                        enable: true
                    }
                }
            };

            //Event - onClick
            function MMonClick(e, treeId, treeNode) {
                //權限表:轉頁 , 人員表:expand
                if (treeId == 'userList') {
                    var zTree = $.fn.zTree.getZTreeObj(treeId);
                    zTree.expandNode(treeNode);

                } else {
                    var menuID = treeNode.id;

                    location.href = '<%=fn_Param.WebUrl%><%=Req_Lang%>/AuthSearch/<%=Param_dbID %>/' + menuID;

                }

            }
            //--- zTree 設定 End ---
        </script>
        <script>
            $(function () {
                /*
                    取得權限List
                    ref:http://api.jquery.com/jQuery.post/
                */
                var jqxhr = $.post("<%=fn_Param.WebUrl%>Ajax_Data/GetAuthList.ashx", {
                    id: 'new', //user guid
                    db: '<%=Param_dbID %>'
                })
                  .done(function (data) {
                      //載入選單
                      $.fn.zTree.init($("#authList"), setting, data)

                      //auto expandAll
                      var treeObj = $.fn.zTree.getZTreeObj("authList");
                      //treeObj.expandAll(true);


                      //auto selectnode
                      var node = treeObj.getNodeByParam('id', '<%=Param_thisID%>'); //取得目前的menuid
                      treeObj.selectNode(node); //將node selected  
                  })
                  .fail(function () {
                      alert("權限選單載入失敗");
                  });


                //全展開
                $("#treeOpen").on("click", function () {
                    var myTreeName = "authList";

                    var treeObj = $.fn.zTree.getZTreeObj(myTreeName);

                    treeObj.expandAll(true);
                });
            });
        </script>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="ph_treeUserJS" runat="server">
        <script>
            $(function () {
                /*
                    取得人員List
                */
                var jqxhr = $.post("<%=fn_Param.WebUrl%>Ajax_Data/GetAuthUserList.ashx", {
                    id: '<%=Param_thisID%>', //menu id
                    db: '<%=Param_dbID %>'
                })
                  .done(function (data) {
                      //載入選單
                      $.fn.zTree.init($("#userList"), setting, data)
                  })
                  .fail(function () {
                      alert("人員選單載入失敗");
                  });
            });
        </script>
    </asp:PlaceHolder>

</asp:Content>

