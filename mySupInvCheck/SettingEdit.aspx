<%@ Page Title="排程設定" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SettingEdit.aspx.cs" Inherits="mySupInvCheck_SettingEdit" %>

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
                    <h5 class="active section red-text text-darken-2">排程設定
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
                                        <asp:RadioButtonList ID="rbl_IsOnTask" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="N" Selected="True">資料設定中&nbsp;</asp:ListItem>
                                            <asp:ListItem Value="Y">加入排程 (加入後不可修改資料)</asp:ListItem>
                                        </asp:RadioButtonList>
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
                                        <asp:TextBox ID="tb_Subject" runat="server" MaxLength="100" placeholder="Email主旨(100字)" autocomplete="off"></asp:TextBox>
                                    </div>
                                    <div class="six wide field">
                                        <label>排程時間</label>
                                        <div class="ui left icon input datepicker">
                                            <asp:TextBox ID="tb_TaskTime" runat="server" MaxLength="20" autocomplete="off" placeholder="格式:西元年/月/日 時:分"></asp:TextBox>
                                            <i class="calendar alternate outline icon"></i>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="ui right aligned segment">
                                <button id="doSaveBase" type="button" class="ui green small button">
                                    <i class="save icon"></i>
                                    <asp:Literal ID="lt_SaveBase" runat="server">開始設定</asp:Literal></button>
                                <asp:Button ID="btn_doSaveBase" runat="server" Text="Save" OnClick="btn_doSaveBase_Click" Style="display: none;" />
                                <asp:HiddenField ID="hf_DataID" runat="server" />
                            </div>
                            <div class="ui bottom attached info small message">
                                <ul>
                                    <li>設定狀態=資料設定中, 可任意修改資料。</li>
                                    <li>設定狀態=加入排程, 無法修改, 資料進入排程。</li>
                                    <li class="red-text"><b>所有資料設定完畢後, 記得將狀態設為「加入排程」, 並按下「修改設定」,才會正式進入發送Mail的排程。</b></li>
                                </ul>
                            </div>
                        </div>
                        <!-- Section-基本資料 End -->

                        <asp:PlaceHolder ID="ph_Details" runat="server" Visible="false">
                            <!-- Section-選擇供應商 Start -->
                            <div class="ui segments">
                                <div class="ui brown segment">
                                    <h5 class="ui header"><a class="anchor" id="section1"></a>選擇供應商</h5>
                                </div>
                                <div class="ui segment">
                                    <asp:ListView ID="lv_SelectSup" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lv_SelectSup_ItemCommand">
                                        <LayoutTemplate>
                                            <table id="table1" class="ui celled selectable compact small table" style="width: 100%;">
                                                <thead>
                                                    <tr>
                                                        <th class="grey-bg lighten-3 center aligned">供應商代號</th>
                                                        <th class="grey-bg lighten-3 center aligned">供應商名稱</th>
                                                        <th class="grey-bg lighten-3 center aligned">採購人員</th>
                                                        <th class="grey-bg lighten-3">供應商Email</th>
                                                        <th class="grey-bg lighten-3 center aligned">寶工庫存顯示</th>
                                                        <th class="grey-bg lighten-3"></th>
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
                                                <td class="center aligned"><%#Eval("PurWhoName") %></td>
                                                <td>
                                                    <small>
                                                        <%#Eval("SupMails") %>
                                                    </small>
                                                </td>
                                                <td class="center aligned">
                                                    <asp:DropDownList ID="ddl_IsShow" runat="server">
                                                        <asp:ListItem Value="Y">顯示</asp:ListItem>
                                                        <asp:ListItem Value="N" Selected="True">不顯示</asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>
                                                <td class="center aligned collapsing">
                                                    <asp:LinkButton ID="lbtn_Plus" runat="server" CssClass="ui small blue basic icon button" ValidationGroup="List" CommandName="doAdd"><i class="plus icon"></i></asp:LinkButton>

                                                    <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("SupID") %>' />
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                        <EmptyDataTemplate>
                                            <div class="ui placeholder segment">
                                                <div class="ui icon header">
                                                    <i class="coffee icon"></i>
                                                    無法取得供應商資料
                                                </div>
                                            </div>
                                        </EmptyDataTemplate>
                                    </asp:ListView>
                                </div>
                            </div>
                            <!-- Section-選擇供應商 End -->

                            <!-- Section-供應商清單 Start -->
                            <div class="ui segments">
                                <div class="ui blue segment">
                                    <h5 class="ui header"><a class="anchor" id="section2"></a>供應商清單</h5>
                                </div>
                                <div class="ui segment">
                                    <asp:ListView ID="lv_CheckedSup" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lv_CheckedSup_ItemCommand" OnItemDataBound="lv_CheckedSup_ItemDataBound">
                                        <LayoutTemplate>
                                            <table id="table1" class="ui celled selectable compact small table" style="width: 100%;">
                                                <thead>
                                                    <tr>
                                                        <th class="grey-bg lighten-3 center aligned">供應商代號</th>
                                                        <th class="grey-bg lighten-3 center aligned">供應商名稱</th>
                                                        <th class="grey-bg lighten-3 center aligned">採購人員</th>
                                                        <th class="grey-bg lighten-3">供應商Email</th>
                                                        <th class="grey-bg lighten-3 center aligned">寶工庫存顯示</th>
                                                        <th class="grey-bg lighten-3 center aligned">檢查</th>
                                                        <th class="grey-bg lighten-3"></th>
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
                                                <td class="center aligned"><%#Eval("PurWhoName") %></td>
                                                <td><%#Eval("SupMails") %></td>
                                                <td class="center aligned"><%#Eval("StockShow") %></td>
                                                <td class="center aligned">
                                                    <asp:Literal ID="lt_ChkMsg" runat="server">OK</asp:Literal>
                                                </td>
                                                <td class="center aligned collapsing">
                                                    <asp:LinkButton ID="lbtn_Del" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doDel" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                                                    <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("SupID") %>' />
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
                        </asp:PlaceHolder>
                    </div>

                </div>
                <!-- Left Body Content End -->

                <!-- Right Navi Menu Start -->
                <div class="three wide column">
                    <div class="ui sticky">
                        <div id="fastjump" class="ui secondary vertical pointing fluid text menu">
                            <div class="header item">快速跳轉<i class="dropdown icon"></i></div>
                            <a href="#baseData" class="item">基本設定</a>
                            <a href="#section1" class="item">選擇供應商</a>
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
    <script>
        $(function () {
            //Save Click
            $("#doSaveBase").click(function () {
                $("#formBase").addClass("loading");
                $("#MainContent_btn_doSaveBase").trigger("click");
            });

        });
    </script>

    <%-- 快速選單 --%>
    <script src="<%=fn_Param.WebUrl %>javascript/sticky.js"></script>
    <%-- 日期選擇器 Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.js"></script>
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/options.js"></script>
    <script>
        $(function () {
            //載入datepicker
            $('.datepicker').calendar(calendarOptsByTime_Range);
        });
    </script>
    <%-- 日期選擇器 End --%>

    <%-- DataTable Start --%>
    <link href="https://cdn.datatables.net/1.10.13/css/jquery.dataTables.min.css" rel="stylesheet" />
    <script src="https://cdn.datatables.net/1.10.11/js/jquery.dataTables.min.js"></script>
    <script>
        $(function () {
            //使用DataTable
            var table = $('#table1').DataTable({
                fixedHeader: true,
                searching: true,  //搜尋
                ordering: false,   //排序
                paging: false,     //分頁
                info: false,      //頁數資訊
                language: {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                //捲軸設定
                "scrollY": '60vh',
                "scrollCollapse": true,
                "scrollX": true
            });


        });

    </script>
    <%-- DataTable End --%>
</asp:Content>

