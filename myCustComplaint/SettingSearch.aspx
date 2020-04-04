<%@ Page Title="開案中客訴" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SettingSearch.aspx.cs" Inherits="myCustComplaint_SettingSearch" %>

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
                    <div class="section"><%:resPublic.fun_客訴管理 %></div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">
                        <asp:Literal ID="lt_TypeName" runat="server" Text="功能名稱"></asp:Literal>
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <asp:LinkButton ID="lbtn_Excel" runat="server" CssClass="item" OnClick="lbtn_Excel_Click"><i class="file excel icon"></i><span class="mobile hidden">匯出</span></asp:LinkButton>
                <a class="item" href="<%=FuncPath() %>/SetEdit">
                    <i class="plus icon"></i>
                    <span class="mobile hidden"><%:resPublic.btn_New%></span>
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
                        <label><%:GetLocalResourceObject("sh_狀態")%></label>
                        <asp:DropDownList ID="filter_Status" runat="server">
                            <asp:ListItem Value="">所有資料</asp:ListItem>
                            <asp:ListItem Value="A">客服單位未填寫</asp:ListItem>
                            <asp:ListItem Value="B">收貨單位未填寫</asp:ListItem>
                            <asp:ListItem Value="C">商品資料未填寫</asp:ListItem>
                            <asp:ListItem Value="D">資料填畢,待開案</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_類別")%></label>
                        <asp:DropDownList ID="filter_CustType" runat="server">
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_收貨")%></label>
                        <asp:DropDownList ID="filter_FrieghtType" runat="server">
                        </asp:DropDownList>
                    </div>
                    <div class="five wide field">
                        <label><%:GetLocalResourceObject("sh_關鍵字查詢")%></label>
                        <asp:TextBox ID="filter_Keyword" runat="server" autocomplete="off" placeholder="客戶姓名/電話、品號、快遞單號、追蹤碼"></asp:TextBox>
                    </div>
                </div>
            </div>
            <div class="ui two column grid">
                <div class="column">
                    <a href="<%=FuncPath() %>/Set" class="ui small button"><i class="refresh icon"></i><%:resPublic.btn_Reset%></a>
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
                <a class="item active" href="<%=FuncPath() %>/Set">開案中客訴
                        <div class="ui label">0</div>
                </a>
                <a class="item" href="<%=FuncPath() %>">客訴清單
                </a>
            </div>
            <div class="ui placeholder segment">
                <div class="ui two column stackable center aligned grid">
                    <div class="ui vertical divider">Or</div>
                    <div class="middle aligned row">
                        <div class="column">
                            <div class="ui icon header">
                                <i class="search icon"></i>
                                <%:GetLocalResourceObject("txt_NoData")%>
                            </div>
                        </div>
                        <div class="column">
                            <div class="ui icon header">
                                <i class="plus icon"></i>
                                <%:GetLocalResourceObject("txt_新增資料")%>
                            </div>
                            <a href="<%=FuncPath() %>/SetEdit" class="ui basic green button">新增客訴</a>
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
                    <a class="item active" href="<%=FuncPath() %>/Set">開案中客訴
                        <div class="ui label">
                            <asp:Literal ID="lt_DataCnt" runat="server">0</asp:Literal>
                        </div>
                    </a>
                    <a class="item" href="<%=FuncPath() %>">客訴清單
                    </a>
                </div>
                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                    <LayoutTemplate>
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th>追蹤碼</th>
                                    <th>客戶類別</th>
                                    <th>收貨方式</th>
                                    <th>聯絡資料</th>
                                    <th>計畫處理</th>
                                    <th>狀態</th>
                                    <th colspan="2">維護資訊</th>
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
                            <td class="left aligned collapsing">
                                <b class="red-text text-darken-2"><%#Eval("TraceID") %></b>
                            </td>
                            <td>
                                <div><%#Eval("CustTypeName") %></div>
                                <div class="grey-text text-darken-2"><%#Eval("RefCustName") %></div>
                                <div class="grey-text text-darken-2"><%#Eval("RefMallName") %></div>
                            </td>
                            <td>
                                <%#Eval("FreightTypeName") %>
                            </td>
                            <td>
                                <div><%#Eval("BuyerName") %>&nbsp;( <%#Eval("BuyerPhone") %> )</div>
                                <small><%#Eval("BuyerAddr") %></small>
                            </td>
                            <td class="center aligned">
                                <strong>
                                    <%#Eval("PlanTypeName") %>
                                </strong>
                            </td>
                            <td class="center aligned collapsing">
                                <asp:Panel ID="pl_CS" runat="server">
                                    <div class="ui grey basic fluid label">客服單位未填寫</div>
                                </asp:Panel>
                                <asp:Panel ID="pl_Freight" runat="server">
                                    <div class="ui grey basic fluid label">收貨單位未填寫</div>
                                </asp:Panel>
                                <asp:Panel ID="pl_Cnt" runat="server" Style="margin-top: 1px;">
                                    <div class="ui red basic fluid label">商品資料未填寫</div>
                                </asp:Panel>
                                <asp:Panel ID="pl_Invoke" runat="server">
                                    <div class="ui orange basic fluid label">待確認開案</div>
                                </asp:Panel>
                            </td>
                            <td class="left aligned collapsing">
                                <div>
                                    <div class="ui small basic fluid label">
                                        建立<div class="detail"><%#Eval("Create_Time") %></div>
                                    </div>
                                </div>
                                <div>
                                    <div class="ui small basic fluid label">
                                        更新<div class="detail"><%#Eval("Update_Time") %></div>
                                    </div>
                                </div>
                            </td>
                            <td class="left aligned collapsing">
                                <div>
                                    <div class="ui small basic fluid label">
                                        建立<div class="detail"><%#Eval("Create_Name") %></div>
                                    </div>
                                </div>
                                <div>
                                    <div class="ui small basic fluid label">
                                        更新<div class="detail"><%#Eval("Update_Name") %></div>
                                    </div>
                                </div>
                            </td>
                            <td class="left aligned collapsing">
                                <a class="ui small grey basic icon button" href="<%=FuncPath() %>/SetView/<%#Eval("Data_ID") %>" title="查看">
                                    <i class="file alternate icon"></i>
                                </a>
                                <asp:PlaceHolder ID="ph_Edit" runat="server">
                                    <a class="ui small teal basic icon button" href="<%=FuncPath() %>/SetEdit/<%#Eval("Data_ID") %>" title="編輯">
                                        <i class="pencil icon"></i>
                                    </a>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="ph_Del" runat="server">
                                    <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                </asp:PlaceHolder>
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

