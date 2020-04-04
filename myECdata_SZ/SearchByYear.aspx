<%@ Page Title="年度" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SearchByYear.aspx.cs" Inherits="myECdata_SZ_SearchByYear" %>

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
                    <div class="section"><%:resPublic.nav_3000 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="section"><%:resPublic.fun_深圳電商平台業績 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        <asp:Literal ID="lt_TypeName" runat="server" Text="功能名稱"></asp:Literal>
                    </div>
                </div>
            </div>
            <div class="right menu">
                <a class="item" href="<%=thisPage %>/Edit">
                    <i class="plus icon"></i><%:resPublic.btn_New%>
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
                    <div class="three wide field">
                        <label>年度</label>
                        <asp:DropDownList ID="filter_Year" runat="server">
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label>平台</label>
                        <asp:DropDownList ID="filter_Mall" runat="server">
                        </asp:DropDownList>
                    </div>

                </div>
            </div>
            <div class="ui two column grid">
                <div class="column">
                    <a href="<%=thisPage %>" class="ui small button"><i class="refresh icon"></i><%:resPublic.btn_Reset%></a>
                </div>
                <div class="column right aligned">
                    <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i><%:resPublic.btn_Search%></button>
                    <asp:Button ID="btn_Search" runat="server" Text="Button" OnClick="btn_Search_Click" Style="display: none" />
                </div>
            </div>

        </div>
        <!-- Search End -->

        <!-- Empty Content Start -->
        <asp:PlaceHolder ID="ph_EmptyData" runat="server" Visible="false">
            <div class="ui secondary pointing menu">
                <a class="item active" href="<%=thisPage %>">年度
                </a>
                <a class="item" href="<%=FuncPath() %>/Month">每月
                </a>
                <a class="item" href="<%=FuncPath() %>/Date">每日
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
                            <a href="<%=thisPage %>/Edit" class="ui basic green button">新增資料</a>
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
                    <a class="item active" href="<%=thisPage %>">年度
                    </a>
                    <a class="item" href="<%=FuncPath() %>/Month">每月
                    </a>
                    <a class="item" href="<%=FuncPath() %>/Date">每日
                    </a>
                </div>
                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                    <LayoutTemplate>
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="center aligned">年度</th>
                                    <th class="center aligned">平台</th>
                                    <th class="right aligned">結算銷售金額</th>
                                    <th class="right aligned">結算返利</th>
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
                            <td class="center aligned red-text text-darken-2">
                                <b><%#Eval("setYear") %></b>
                            </td>
                            <td class="center aligned green-text text-darken-2">
                                <h4><%#Eval("MallName") %></h4>
                            </td>
                            <td class="right aligned">
                                <%#Eval("Price_Sales") %>
                            </td>
                            <td class="right aligned">
                                <%#Eval("Price_Rebate") %>
                            </td>
                            <td class="left aligned collapsing">
                                <%--<a class="ui small grey basic icon button" href="<%=thisPage %>/View/<%#Eval("Data_ID") %>" title="查看">
                                    <i class="file alternate icon"></i>
                                </a>--%>
                                <asp:PlaceHolder ID="ph_Edit" runat="server">
                                    <a class="ui small teal basic icon button" href="<%=thisPage %>/Edit/<%#Eval("Data_ID") %>" title="編輯">
                                        <i class="pencil icon"></i>
                                    </a>
                                </asp:PlaceHolder>
                                <%--<asp:PlaceHolder ID="ph_Del" runat="server">
                                    <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                </asp:PlaceHolder>--%>
                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
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

            //init dropdown list
            $('select').dropdown();

        });
    </script>

</asp:Content>

