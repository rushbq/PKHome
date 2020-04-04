<%@ Page Title="回覆明細" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="View.aspx.cs" Inherits="mySupInvCheck_View" %>

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
                    <div class="section">回覆明細</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">
                        <asp:Literal ID="lt_NavHeader" runat="server"></asp:Literal>
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <asp:LinkButton ID="lbtn_Excel" runat="server" CssClass="item" OnClick="lbtn_Excel_Click"><i class="file excel icon"></i><span class="mobile hidden">匯出</span></asp:LinkButton>
                <a class="item" href="<%=FuncPath() %>">
                    <i class="undo icon"></i>
                    <span class="mobile hidden">返回</span>
                </a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Search Start -->
        <%--<div class="ui orange attached segment">
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

        </div>--%>
        <!-- Search End -->

        <!-- Empty Content Start -->
        <asp:PlaceHolder ID="ph_EmptyData" runat="server" Visible="false">
            <div class="ui placeholder segment">
                <div class="ui icon header">
                    <i class="search icon"></i>
                    目前條件查無資料，請重新查詢。
                </div>
            </div>
        </asp:PlaceHolder>
        <!-- Empty Content End -->

        <!-- List Content Start -->
        <asp:PlaceHolder ID="ph_Data" runat="server">
            <div class="ui green attached segment">
                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items">
                    <LayoutTemplate>
                        <table class="ui celled selectable compact small structured table">
                            <thead>
                                <tr>
                                    <th rowspan="2">品號/品名</th>
                                    <th class="center aligned" rowspan="2">幣別</th>
                                    <th class="right aligned" rowspan="2">最近核價</th>
                                    <th class="right aligned" rowspan="2">寶工庫存</th>
                                    <th class="center aligned" colspan="3">廠商實盤數量</th>
                                    <th class="right aligned" rowspan="2" title="寶工庫存-總數">差額數量</th>
                                    <th class="center aligned" rowspan="2">最近入庫日</th>
                                    <th class="center aligned" rowspan="2">最近出庫日</th>
                                    <th rowspan="2">替代品號</th>
                                </tr>
                                <tr>
                                    <th class="right aligned">未包裝數</th>
                                    <th class="right aligned">已包裝<br />
                                        未出貨數</th>
                                    <th class="right aligned">總數</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                        </table>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <strong class="green-text text-darken-3"><%#Eval("ModelNo") %></strong>
                                <br />
                                <span class="grey-text text-darken-1"><%#Eval("ModelName") %></span>
                            </td>
                            <td class="center aligned">
                                <%#Eval("Currency") %>
                            </td>
                            <td class="right aligned">
                                <%#Eval("lastPurPrice") %>
                            </td>
                            <td class="right aligned red-text">
                                <%#Eval("StockNum") %>
                            </td>
                            <td class="right aligned">
                                <%#Eval("InputQty1") %>
                            </td>
                            <td class="right aligned">
                                <%#Eval("InputQty2") %>
                            </td>
                            <td class="right aligned">
                                <strong>
                                    <%#Convert.ToInt32(Eval("InputQty1"))+Convert.ToInt32(Eval("InputQty2")) %>
                                </strong>
                            </td>
                            <td class="right aligned">
                                <strong>
                                    <%#Convert.ToInt32(Eval("StockNum"))-(Convert.ToInt32(Eval("InputQty1"))+Convert.ToInt32(Eval("InputQty2"))) %>
                                </strong>
                            </td>
                            <td class="center aligned">
                                <%#Eval("inStockDate") %>
                            </td>
                            <td class="center aligned">
                                <%#Eval("outStockDate") %>
                            </td>
                            <td>
                                <%#Eval("anotherModel") %>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:ListView>
            </div>
        </asp:PlaceHolder>
        <!-- List Content End -->
    </div>
    <!-- 內容 End -->

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
</asp:Content>

