<%@ Page Title="回覆清單" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="mySupInvCheck_Search" %>

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
                    <h5 class="active section red-text text-darken-2">回覆清單
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="item" href="<%=FuncPath() %>/SetEdit">
                    <i class="plus icon"></i>
                    <span class="mobile hidden">新增排程</span>
                </a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Search Start -->
        <div class="ui orange attached segment">
            <div class="ui small form">
                <div class="fields">
                    <div class="six wide field">
                        <label>回覆時間</label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_sDate" runat="server" placeholder="開始日" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_eDate" runat="server" placeholder="結束日" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="six wide field">
                        <label>關鍵字查詢</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" autocomplete="off" placeholder="查詢:供應商,採購人員" MaxLength="20"></asp:TextBox>
                    </div>
                </div>
            </div>
            <div class="ui two column grid">
                <div class="column">
                    <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i>重置條件</a>
                </div>
                <div class="column right aligned">
                    <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                    <asp:Button ID="btn_Search" runat="server" Text="Button" OnClick="btn_Search_Click" Style="display: none" />
                </div>
            </div>

        </div>
        <!-- Search End -->

        <!-- Empty Content Start -->
        <asp:PlaceHolder ID="ph_EmptyData" runat="server" Visible="false">
            <div class="ui secondary pointing menu">
                <a class="item" href="<%=FuncPath() %>/Set">發送清單
                </a>
                <a class="item active" href="<%=FuncPath() %>">回覆清單
                </a>
            </div>
            <div class="ui placeholder segment">
                <div class="ui two column stackable center aligned grid">
                    <div class="ui vertical divider">Or</div>
                    <div class="middle aligned row">
                        <div class="column">
                            <div class="ui icon header">
                                <i class="search icon"></i>
                                目前條件查無資料，請重新查詢。
                            </div>
                        </div>
                        <div class="column">
                            <div class="ui icon header">
                                <i class="plus icon"></i>
                                建立新資料
                            </div>
                            <a href="<%=FuncPath() %>/SetEdit" class="ui basic green button">新增排程</a>
                        </div>
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>
        <!-- Empty Content End -->

        <!-- List Content Start -->
        <asp:PlaceHolder ID="ph_Data" runat="server">
            <div class="ui green attached segment">
                <div class="ui secondary pointing menu">
                    <a class="item" href="<%=FuncPath() %>/Set">發送清單
                    </a>
                    <a class="item active" href="<%=FuncPath() %>">回覆清單
                    </a>
                </div>
                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                    <LayoutTemplate>
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="center aligned">回覆時間</th>
                                    <th class="center aligned" colspan="2">供應商</th>
                                    <th class="center aligned collapsing">顯示寶工庫存</th>
                                    <th class="center aligned">採購人員</th>
                                    <th>收件人</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                        </table>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="center aligned blue-text text-darken-2 collapsing">
                                <b><%#Eval("WriteTime") %></b>
                            </td>
                            <td class="center aligned green-text text-darken-2">
                                <strong><%#Eval("SupID") %></strong>
                            </td>
                            <td class="center aligned">
                                <h5><%#Eval("SupName") %></h5>
                            </td>
                            <td class="center aligned">
                                <%#Eval("IsWrite") %>
                            </td>
                            <td class="center aligned collapsing">
                                <%#Eval("PurWhoName") %>
                            </td>
                            <td>
                                <%#Eval("SupMails") %>
                            </td>
                            <td class="left aligned collapsing">
                                <a class="ui small grey basic icon button" href="<%=FuncPath() %>/View/<%#Eval("Parent_ID") %>/<%#Eval("SupID") %>" title="查看">
                                    <i class="file alternate icon"></i>
                                </a>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:ListView>
            </div>
            <!-- List Pagination Start -->
            <div class="ui mini bottom attached segment grey-bg lighten-4">
                <asp:Literal ID="lt_Pager" runat="server"></asp:Literal>
            </div>
            <!-- List Pagination End -->
        </asp:PlaceHolder>
        <!-- List Content End -->
    </div>
    <!-- 內容 End -->

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //[搜尋][查詢鈕] - 觸發查詢
            $("#doSearch").click(function () {
                //觸發查詢按鈕
                $("#MainContent_btn_Search").trigger("click");
            });


        });
    </script>
    <%-- 日期選擇器 Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.js"></script>
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/options.js"></script>
    <script>
        $(function () {
            //載入datepicker
            $('.datepicker').calendar(calendarOpts_Range);
        });
    </script>
    <%-- 日期選擇器 End --%>
</asp:Content>

