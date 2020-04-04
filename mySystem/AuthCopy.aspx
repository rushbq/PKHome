<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AuthCopy.aspx.cs" Inherits="mySystem_AuthCopy" %>

<asp:Content ID="myCss" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="myBody" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- Sub Header Start -->
    <div class="grey lighten-3">
        <div class="container">
            <div class="row">
                <div class="col s12 m12 l12">
                    <h5 class="breadcrumbs-title">權限複製</h5>
                    <ol class="breadcrumb">
                        <li><a href="#!">系統管理</a></li>
                        <li><a href="#!">權限管理</a></li>
                        <li class="active">權限複製</li>
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
                        <h5>一對一權限複製 <small>(複製目標的權限會被清空再複製，請謹慎使用)</small></h5>
                    </div>
                    <div class="right">
                        <a id="openHelp" class="waves-effect waves-light btn"><i class="material-icons right">help</i>HELP</a>
                    </div>
                    <div class="clearfix"></div>
                </div>
                <div style="display: none" class="serversidecontroller">
                    <asp:Button ID="btn_Setting" runat="server" Text="Set" OnClick="btn_Setting_Click" />
                </div>
                <div class="card-content grey lighten-5">
                    <asp:PlaceHolder ID="ph_Require" runat="server" Visible="false">
                        <div class="card-panel red white-text">
                            <h5><i class="material-icons">warning</i>&nbsp;請確認以下條件是否達成:</h5>
                            <ul>
                                <li>請選擇資料庫</li>
                                <li>請選擇要複製的人員</li>
                            </ul>
                        </div>
                    </asp:PlaceHolder>
                    <asp:Panel ID="pl_Msg" runat="server" CssClass="card-panel red white-text" Visible="false">
                        <h5><i class="material-icons">announcement</i>&nbsp;權限設定失敗，請通知資訊人員</h5>
                    </asp:Panel>
                    <asp:PlaceHolder ID="ph_SetDone" runat="server" Visible="false">
                        <div class="card-panel orange darken-1 white-text">
                            <h5>
                                <i class="material-icons">thumb_up</i>&nbsp;權限複製完成
                                <small class="right"><i class="material-icons">access_time</i>&nbsp;<asp:Literal ID="lt_done_Time" runat="server"></asp:Literal></small>
                            </h5>
                            <ul>
                                <li>複製來源：<asp:Literal ID="lt_done_Source" runat="server"></asp:Literal></li>
                                <li>複製目標：<asp:Literal ID="lt_done_Target" runat="server"></asp:Literal></li>
                            </ul>
                        </div>
                    </asp:PlaceHolder>

                    <div class="row">
                        <div class="col s12 m6">
                            <blockquote class="color-blue">
                                <h6>選擇複製來源</h6>
                                <div class="row">
                                    <div class="col s12">
                                        <label for="MainContent_ddl_Who">選擇部門 (可不選)</label>
                                        <asp:DropDownListGP ID="ddl_Dept" runat="server" CssClass="browser-default" AutoPostBack="true" OnSelectedIndexChanged="ddl_Dept_SelectedIndexChanged">
                                        </asp:DropDownListGP>
                                    </div>
                                    <div class="col s12">
                                        <label for="MainContent_ddl_Who">[來源資料] - 人員</label>
                                        <asp:DropDownListGP ID="ddl_Who" runat="server" CssClass="browser-default green-text text-darken-2">
                                        </asp:DropDownListGP>
                                    </div>
                                    <div class="col s12">
                                        <label for="MainContent_ddl_DB">選擇資料庫</label>
                                        <asp:DropDownList ID="ddl_DB" runat="server" CssClass="browser-default red-text text-darken-1">
                                        </asp:DropDownList>
                                    </div>
                                </div>
                            </blockquote>
                        </div>
                        <div class="col s12 m6">
                            <blockquote class="color-green">
                                <h6>選擇複製目標</h6>
                                <div class="row">
                                    <div class="col s12">
                                        <label for="MainContent_ddl_tarDept">選擇部門 (可不選)</label>
                                        <asp:DropDownListGP ID="ddl_tarDept" runat="server" CssClass="browser-default" AutoPostBack="true" OnSelectedIndexChanged="ddl_tarDept_SelectedIndexChanged">
                                        </asp:DropDownListGP>
                                    </div>
                                    <div class="col s12">
                                        <label for="MainContent_ddl_tarWho">[目標資料] - 人員</label>
                                        <asp:DropDownListGP ID="ddl_tarWho" runat="server" CssClass="browser-default blue-text text-darken-2">
                                        </asp:DropDownListGP>
                                    </div>
                                </div>
                            </blockquote>
                        </div>
                        <asp:PlaceHolder ID="ph_SettingBtn" runat="server">
                            <div class="fixed-action-btn">
                                <a id="setAuth" class="btn-floating btn-large waves-effect waves-light red"><i class="material-icons">mode_edit</i></a>
                            </div>
                        </asp:PlaceHolder>
                        <div class="help-tip tap-target red lighten-1" data-activates="setAuth">
                            <div class="tap-target-content white-text">
                                <h5>這是什麼??</h5>
                                <p>把人及資料庫選項選好，按我就可以設定啦~~</p>
                                <br />
                                <div><a href="#!" id="closeHelp" class="blue-text text-lighten-4">OK，我知道了<i class="material-icons">touch_app</i></a></div>
                            </div>
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

                //設定
                $("#setAuth").on("click", function () {

                    //觸發設定 click
                    $("#MainContent_btn_Setting").trigger("click");

                });

            }); // end of document ready
        })(jQuery); // end of jQuery name space
    </script>
</asp:Content>

