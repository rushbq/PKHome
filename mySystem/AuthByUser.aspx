<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AuthByUser.aspx.cs" Inherits="mySystem_AuthByUser" %>

<asp:Content ID="myCss" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="myBody" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- Sub Header Start -->
    <div class="grey lighten-3">
        <div class="container">
            <div class="row">
                <div class="col s12 m12 l12">
                    <h5 class="breadcrumbs-title">設定 - 單一使用者</h5>
                    <ol class="breadcrumb">
                        <li><a href="#!">系統管理</a></li>
                        <li><a href="#!">權限管理</a></li>
                        <li class="active">設定 - 單一使用者</li>
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
                    <div class="left">
                        <h5>權限設定</h5>
                    </div>
                    <div class="right">
                        <a id="openHelp" class="waves-effect waves-light btn"><i class="material-icons right">help</i>HELP</a>
                    </div>
                    <div class="clearfix"></div>
                </div>
                <div style="display: none" class="serversidecontroller">
                    <asp:Button ID="btn_Setting" runat="server" Text="Set" OnClick="btn_Setting_Click" />
                    <asp:TextBox ID="tb_Values" runat="server"></asp:TextBox>
                </div>
                <div class="card-content grey lighten-5">
                    <div class="row">
                        <div class="col s12 m5 l4">
                            <asp:PlaceHolder ID="ph_Require" runat="server" Visible="false">
                                <div class="card-panel red white-text">
                                    <h5><i class="material-icons">warning</i>&nbsp;請確認以下條件是否達成:</h5>
                                    <ul>
                                        <li>請選擇人員</li>
                                        <li>請選擇資料庫</li>
                                    </ul>
                                </div>
                            </asp:PlaceHolder>
                            <div class="row">
                                <div class="col s12">
                                    <label for="MainContent_ddl_Who">選擇部門 (可不選)</label>
                                    <asp:DropDownListGP ID="ddl_Dept" runat="server" CssClass="browser-default" AutoPostBack="true" OnSelectedIndexChanged="ddl_Dept_SelectedIndexChanged">
                                    </asp:DropDownListGP>
                                </div>
                                <div class="col s12">
                                    <label for="MainContent_ddl_Who">選擇人員</label>
                                    <asp:DropDownListGP ID="ddl_Who" runat="server" CssClass="browser-default green-text text-darken-2">
                                    </asp:DropDownListGP>
                                </div>
                                <div class="col s12">
                                    <label for="MainContent_ddl_DB">選擇資料庫</label>
                                    <asp:DropDownList ID="ddl_DB" runat="server" CssClass="browser-default red-text text-darken-1">
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col s12 right-align">
                                    <asp:LinkButton ID="lbtn_Go" runat="server" CssClass="btn waves-effect waves-light blue" OnClick="lbtn_Go_Click"><i class="material-icons right">playlist_add_check</i>帶出權限</asp:LinkButton>
                                </div>
                            </div>
                        </div>
                        <div class="col s12 m7 l8">
                            <asp:Panel ID="pl_Msg1" runat="server" CssClass="card-panel green lighten-1 white-text">
                                <h5><i class="material-icons">announcement</i>&nbsp;請先完成條件篩選</h5>
                            </asp:Panel>
                            <asp:Panel ID="pl_Msg2" runat="server" CssClass="card-panel red white-text" Visible="false">
                                <h5><i class="material-icons">announcement</i>&nbsp;至少要勾選一項權限</h5>
                            </asp:Panel>
                            <asp:Panel ID="pl_Msg3" runat="server" CssClass="card-panel red white-text" Visible="false">
                                <h5><i class="material-icons">announcement</i>&nbsp;權限設定失敗，請通知資訊人員</h5>
                            </asp:Panel>

                            <asp:PlaceHolder ID="ph_treeHtml" runat="server">
                                <div class="row">
                                    <div class="s12">
                                        <asp:PlaceHolder ID="ph_SettingBtn" runat="server">
                                            <div class="fixed-action-btn">
                                                <a id="setAuth" class="btn-floating btn-large waves-effect waves-light red"><i class="material-icons">mode_edit</i></a>
                                            </div>
                                        </asp:PlaceHolder>

                                        <h5>
                                            <asp:Literal ID="lt_UserInfo" runat="server"></asp:Literal></h5>
                                        <div>
                                            <button type="button" id="treeOpen" class="btn green lighten-1"><i class="material-icons right">sort</i>展開所有節點</button>
                                        </div>
                                        <div id="authList" class="ztree"></div>
                                    </div>
                                </div>

                                <div class="help-tip tap-target red lighten-1" data-activates="setAuth">
                                    <div class="tap-target-content white-text">
                                        <h5>這是什麼??</h5>
                                        <p>權限勾好，按我就可以設定啦~~</p>
                                        <br />
                                        <div><a href="#!" id="closeHelp" class="blue-text text-lighten-4">OK，我知道了<i class="material-icons">touch_app</i></a></div>
                                    </div>
                                </div>
                            </asp:PlaceHolder>
                        </div>
                    </div>
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
        <script src="<%=fn_Param.WebUrl %>plugins/zTree/jquery.ztree.excheck-3.5.min.js"></script>
        <script>
            //--- zTree 設定 Start ---
            var setting = {
                view: {
                    dblClickExpand: false
                },
                callback: {
                    onClick: MMonClick
                },
                check: {
                    enable: true
                },
                data: {
                    simpleData: {
                        enable: true
                    }
                }
            };

            //Event - onClick
            function MMonClick(e, treeId, treeNode) {
                var zTree = $.fn.zTree.getZTreeObj(treeId);
                zTree.expandNode(treeNode);
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
                    id: '<%=Param_thisID %>',
                    db: '<%=Param_dbID %>'
                })
                  .done(function (data) {
                      //載入選單
                      $.fn.zTree.init($("#authList"), setting, data)
                  })
                  .fail(function () {
                      alert("權限選單載入失敗");
                  });


                /*
                    取得已勾選的項目ID
                */
                $("#setAuth").on("click", function () {
                    var myTreeName = "authList";
                    var valAry = [];

                    //宣告tree物件
                    var treeObj = $.fn.zTree.getZTreeObj(myTreeName);

                    //取得節點array
                    var nodes = treeObj.getCheckedNodes(true);

                    //將id丟入陣列
                    for (var row = 0; row < nodes.length; row++) {
                        valAry.push(nodes[row].id);
                    }

                    //將陣列組成以','分隔的字串，並填入欄位
                    $("#MainContent_tb_Values").val(valAry.join(","));

                    //觸發設定 click
                    $("#MainContent_btn_Setting").trigger("click");

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


    <script>
        (function ($) {
            $(function () {
                //載入選單
                //$('select').material_select();

                //顯示說明
                $("#openHelp").on("click", function () {
                    $('.help-tip').tapTarget('open');
                });
                $("#closeHelp").on("click", function () {
                    $('.help-tip').tapTarget('close');
                });


            }); // end of document ready
        })(jQuery); // end of jQuery name space
    </script>
</asp:Content>

