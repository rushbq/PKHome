<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AuthByFunc.aspx.cs" Inherits="mySystem_AuthByFunc" %>

<asp:Content ID="myCss" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="myBody" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- Sub Header Start -->
    <div class="grey lighten-3">
        <div class="container">
            <div class="row">
                <div class="col s12 m12 l12">
                    <h5 class="breadcrumbs-title">設定 - 依功能設置</h5>
                    <ol class="breadcrumb">
                        <li><a href="#!">系統管理</a></li>
                        <li><a href="#!">權限管理</a></li>
                        <li class="active">設定 - 依功能設置</li>
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
                    <asp:TextBox ID="tb_Values_User" runat="server"></asp:TextBox>
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

                    <div class="container">
                        <asp:Panel ID="pl_Msg1" runat="server" CssClass="card-panel green lighten-1 white-text">
                            <h5><i class="material-icons">announcement</i>&nbsp;請先完成條件篩選</h5>
                        </asp:Panel>
                        <asp:Panel ID="pl_Msg2" runat="server" CssClass="card-panel red white-text" Visible="false">
                            <h5><i class="material-icons">announcement</i>&nbsp;請確認權限及人員至少勾選一筆</h5>
                        </asp:Panel>
                        <asp:Panel ID="pl_Msg3" runat="server" CssClass="card-panel red white-text" Visible="false">
                            <h5><i class="material-icons">announcement</i>&nbsp;權限設定失敗，請通知資訊人員</h5>
                        </asp:Panel>
                        <asp:Panel ID="pl_Msg4" runat="server" CssClass="card-panel orange darken-1 white-text" Visible="false">
                            <h5><i class="material-icons">announcement</i>&nbsp;請注意下列事項：</h5>
                            <ul>
                                <li>1. 本功能只有「新增」動作</li>
                                <li>2. 設定時只會新增不具有「已勾選功能」的人員</li>
                                <li>3. 若(功能項目*人員)大於200筆，則無法設定</li>
                                <li>4. 筆數的計算，只要有「勾勾」即是1筆</li>
                                <li>5. 設定完成會直接跳回本頁, 並清空勾選</li>
                            </ul>
                        </asp:Panel>
                    </div>
                    <asp:PlaceHolder ID="ph_treeHtml" runat="server">
                        <div class="row">
                            <div class="col s12 m6">
                                <blockquote class="color-blue">
                                    <h6>權限表</h6>
                                    <div id="authList" class="ztree"></div>
                                </blockquote>

                            </div>
                            <div class="col s12 m6">
                                <blockquote class="color-green">
                                    <h6>人員清單</h6>
                                    <div id="userList" class="ztree"></div>
                                </blockquote>

                            </div>
                        </div>

                        <!-- Set button -->
                        <asp:PlaceHolder ID="ph_SettingBtn" runat="server">
                            <div class="fixed-action-btn">
                                <a id="setAuth" class="btn-floating btn-large waves-effect waves-light red"><i class="material-icons">mode_edit</i></a>
                            </div>
                        </asp:PlaceHolder>
                        <div class="help-tip tap-target red lighten-1" data-activates="setAuth">
                            <div class="tap-target-content white-text">
                                <h5>這是什麼??</h5>
                                <p>權限勾好，按我就可以設定啦~~</p>
                                <br />
                                <div><a href="#!" id="closeHelp" class="blue-text text-lighten-4">OK，我知道了<i class="material-icons">touch_app</i></a></div>
                            </div>
                        </div>
                    </asp:PlaceHolder>

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
        <script src="<%=fn_Param.WebUrl %>plugins/zTree/jquery.ztree.excheck-3.5.min.js"></script>
        <script>
            //--- zTree 設定 Start ---
            var setting = {
                view: {
                    dblClickExpand: false   //已使用onclick展開,故將雙擊展開關閉                    
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
                    id: 'new',
                    db: '<%=Param_dbID %>'
                })
                  .done(function (data) {
                      //載入選單
                      $.fn.zTree.init($("#authList"), setting, data)

                      //auto expandAll
                      var treeObj = $.fn.zTree.getZTreeObj("authList");
                      treeObj.expandAll(true);
                  })
                  .fail(function () {
                      alert("權限選單載入失敗");
                  });

                /*
                    取得人員List
                */
                var jqxhr = $.post("<%=fn_Param.WebUrl%>Ajax_Data/GetAuthUserList.ashx")
                  .done(function (data) {
                      //載入選單
                      $.fn.zTree.init($("#userList"), setting, data)
                  })
                  .fail(function () {
                      alert("人員選單載入失敗");
                  });


                /*
                    取得已勾選的項目ID
                */
                $("#setAuth").on("click", function () {
                    var myTreeName = "authList";
                    var myTreeName_User = "userList";
                    var valAry = [];
                    var valAry_User = [];

                    //宣告tree物件
                    var treeObj = $.fn.zTree.getZTreeObj(myTreeName);
                    var treeObj_User = $.fn.zTree.getZTreeObj(myTreeName_User);

                    //取得節點array
                    var nodes = treeObj.getCheckedNodes(true);
                    var nodes_User = treeObj_User.getCheckedNodes(true);

                    /* 限制勾選過多, 避免loading過重 */
                    var cntA = nodes.length;
                    var cntB = nodes_User.length;
                    if (cntA * cntB > 200) {
                        alert('筆數過多, 請重新勾選!')
                        return false;
                    }

                    //將id丟入陣列
                    for (var row = 0; row < nodes.length; row++) {
                        valAry.push(nodes[row].id);
                    }
                    for (var row = 0; row < nodes_User.length; row++) {
                        //只取開頭為'v_'的值
                        var myval = nodes_User[row].id;
                        if (myval.substring(0, 2) == "v_") {
                            valAry_User.push(myval.replace("v_", ""));
                        }
                    }

                    //將陣列組成以','分隔的字串，並填入欄位
                    $("#MainContent_tb_Values").val(valAry.join(","));
                    $("#MainContent_tb_Values_User").val(valAry_User.join(","));

                    //觸發設定 click
                    $("#MainContent_btn_Setting").trigger("click");

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

