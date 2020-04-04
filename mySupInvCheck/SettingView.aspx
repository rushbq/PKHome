<%@ Page Title="排程設定檢視" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SettingView.aspx.cs" Inherits="mySupInvCheck_SettingView" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<%@ Import Namespace="Resources" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section"><%:resPublic.nav_4000 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="section">外廠包材庫存盤點</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">排程設定檢視
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="anchor" id="top"></a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui grid">
            <div class="row">
                <!-- Left Body Content Start -->
                <div id="myStickyBody" class="thirteen wide column">
                    <div class="ui attached segment grey-bg lighten-5">
                        <!-- Section-基本資料 Start -->
                        <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
                            <div class="ui negative message">
                                <div class="header">
                                    <%:resPublic.error_oops %>
                                </div>
                                <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                            </div>
                        </asp:PlaceHolder>
                        <div class="ui segments">
                            <div class="ui green segment">
                                <h5 class="ui header"><a class="anchor" id="baseData"></a>基本資料</h5>
                            </div>
                            <div id="formBase" class="ui small form segment">
                                <div class="fields">
                                    <div class="ten wide field">
                                        <label>設定狀態</label>
                                        <div class="ui basic label">
                                            <asp:Literal ID="lt_IsOnTask" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                    <div class="six wide field">
                                        <label>系統編號</label>
                                        <div class="ui red basic large label">
                                            <asp:Literal ID="lt_SeqNo" runat="server">資料建立中</asp:Literal>
                                        </div>
                                    </div>
                                </div>
                                <div class="fields">
                                    <div class="ten wide field">
                                        <label>Email主旨</label>
                                        <div class="ui basic label">
                                            <asp:Literal ID="lt_Subject" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                    <div class="six wide field">
                                        <label>排程時間</label>
                                        <div class="ui basic label">
                                            <asp:Literal ID="lt_TaskTime" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- Section-基本資料 End -->

                        <!-- Section-供應商清單 Start -->
                        <div class="ui segments">
                            <div class="ui blue clearing segment">
                                <h5 class="ui left floated header"><a class="anchor" id="section2"></a>供應商清單</h5>
                                <h5 class="ui right floated header">
                                    <asp:Button ID="btn_Export" runat="server" Text="匯出" CssClass="ui green small button" OnClick="btn_Export_Click" />
                                </h5>
                            </div>
                            <div class="ui segment">
                                <asp:ListView ID="lv_CheckedSup" runat="server" ItemPlaceholderID="ph_Items">
                                    <LayoutTemplate>
                                        <table id="table1" class="ui celled selectable compact small table" style="width: 100%;">
                                            <thead>
                                                <tr>
                                                    <th class="grey-bg lighten-3 center aligned">供應商代號</th>
                                                    <th class="grey-bg lighten-3 center aligned">供應商名稱</th>
                                                    <th class="grey-bg lighten-3 center aligned">採購人員</th>
                                                    <th class="grey-bg lighten-3">供應商Email</th>
                                                    <th class="grey-bg lighten-3 center aligned">寶工庫存顯示</th>
                                                    <th class="grey-bg lighten-3 center aligned">Mail發送</th>
                                                    <th class="grey-bg lighten-3 center aligned">發送時間</th>
                                                    <th class="grey-bg lighten-3 center aligned">填寫?</th>
                                                    <th class="grey-bg lighten-3 center aligned">表單</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                                            </tbody>
                                        </table>
                                    </LayoutTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td class="center aligned green-text text-darken-2">
                                                <strong><%#Eval("SupID") %></strong>
                                            </td>
                                            <td class="center aligned">
                                                <h5><%#Eval("SupName") %></h5>
                                            </td>
                                            <td class="center aligned">
                                                <%#Eval("PurWhoName") %>
                                            </td>
                                            <td><%#Eval("SupMails") %></td>
                                            <td class="center aligned">
                                                <%#Eval("StockShow") %>
                                            </td>
                                            <td class="center aligned">
                                                <%#Eval("IsSend") %>
                                            </td>
                                            <td class="center aligned">
                                                <%#Eval("SendTime") %>
                                            </td>
                                            <td class="center aligned" title="回覆明細頁">
                                                <a href="<%=FuncPath() %>/View/<%#Eval("Parent_ID") %>/<%#Eval("SupID") %>"><%#Eval("IsWrite") %></a>
                                            </td>
                                            <td class="center aligned" title="填寫頁">
                                                <a href="https://www.prokits.com.tw/Stock/<%#Eval("Token") %>" target="_blank"><i class="ui icon external alternate"></i></a>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <div class="ui placeholder segment">
                                            <div class="ui icon header">
                                                <i class="coffee icon"></i>
                                                尚未加入
                                            </div>
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:ListView>
                            </div>
                        </div>
                        <!-- Section-供應商清單 End -->

                        <!-- Section-維護資訊 Start -->
                        <div class="ui segments">
                            <div class="ui grey segment">
                                <h5 class="ui header"><a class="anchor" id="infoData"></a>維護資訊</h5>
                            </div>
                            <div class="ui segment">
                                <table class="ui celled small four column table">
                                    <thead>
                                        <tr>
                                            <th colspan="2" class="center aligned">建立</th>
                                            <th colspan="2" class="center aligned">最後更新</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr class="center aligned">
                                            <td>
                                                <asp:Literal ID="info_Creater" runat="server">資料建立中...</asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="info_CreateTime" runat="server">資料建立中...</asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="info_Updater" runat="server"></asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="info_UpdateTime" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                        <!-- Section-維護資訊 End -->
                    </div>

                </div>
                <!-- Left Body Content End -->

                <!-- Right Navi Menu Start -->
                <div class="three wide column">
                    <div class="ui sticky">
                        <div id="fastjump" class="ui secondary vertical pointing fluid text menu">
                            <div class="header item">快速跳轉<i class="dropdown icon"></i></div>
                            <a href="#baseData" class="item">基本設定</a>
                            <a href="#section2" class="item">供應商清單</a>
                            <a href="#top" class="item"><i class="angle double up icon"></i>到頂端</a>
                        </div>

                        <div class="ui vertical text menu">
                            <div class="header item">功能按鈕</div>
                            <div class="item">
                                <a href="<%:Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i>返回列表</a>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- Right Navi Menu End -->
            </div>
        </div>

    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <%-- 快速選單 --%>
    <script src="<%=fn_Param.WebUrl %>javascript/sticky.js"></script>

</asp:Content>

