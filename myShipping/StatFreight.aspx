<%@ Page Title="運費統計" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="StatFreight.aspx.cs" Inherits="myShipping_StatFreight" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">業務行銷</div>
                    <i class="right angle icon divider"></i>
                    <div class="section">發貨/運費維護統計</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        運費統計 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
            <div class="right menu">
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Empty Content Start -->
        <asp:PlaceHolder ID="ph_EmptyData" runat="server">
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
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3 center aligned">年月</th>
                                    <th class="grey-bg lighten-3 right aligned">銷貨金額</th>
                                    <th class="grey-bg lighten-3 center aligned">品項數</th>
                                    <th class="grey-bg lighten-3 center aligned">發貨件數</th>
                                    <th class="grey-bg lighten-3 right aligned">到貨運費</th>
                                    <th class="grey-bg lighten-3 right aligned">自付運費</th>
                                    <th class="grey-bg lighten-3 right aligned">墊付運費</th>
                                    <th class="grey-bg lighten-3 center aligned">佔銷售比例(%)</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                        </table>
                    </div>

                </LayoutTemplate>
                <ItemTemplate>
                    <tr class="<%#setBg(Eval("Month")) %>">
                        <td class="center aligned">
                            <strong class="green-text text-darken-3">
                                <a href="<%:fn_Param.WebUrl %><%:Req_Lang %>/3000/ShipFreightSend/<%:Req_CompID %>?tab=2&sDate_Ship=<%#Server.UrlEncode(Eval("sDate").ToString()) %>&eDate_Ship=<%#Server.UrlEncode(Eval("eDate").ToString()) %>">
                                    <%#Eval("showYM") %>
                                </a>
                            </strong>
                        </td>
                        <td class="right aligned">
                            <%#Eval("TotalPrice").ToString().ToMoneyString() %>
                        </td>
                        <td class="center aligned">
                            <%#Eval("ItemCnt") %>
                        </td>
                        <td class="center aligned">
                            <%#Eval("ShipCnt") %>
                        </td>
                        <td class="right aligned">
                            <%#showNumber(Eval("Pay1")) %>
                        </td>
                        <td class="right aligned">
                            <%#showNumber(Eval("Pay2")) %>
                        </td>
                        <td class="right aligned">
                            <%#showNumber(Eval("Pay3")) %>
                        </td>
                        <td class="center aligned">
                            <%#Eval("avgPercent") %> %
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>
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

        });
    </script>
</asp:Content>

