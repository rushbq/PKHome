<%@ Page Title="代發次數統計" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="CustSendCntStat.aspx.cs" Inherits="myShipping_ImportList" %>

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
                    <div class="section">出貨明細</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        代發次數統計
                    </div>
                </div>
            </div>
            <div class="right menu">
                <div class="item" title="">
                    <label for="MainContent_shipNoImport" style="cursor: pointer;">
                        <i class="sync alternate icon"></i>資料轉入
                    </label>
                    <asp:FileUpload ID="shipNoImport" runat="server" accept=".xlsx,.xls" Style="display: none" />
                    <asp:Button ID="btn_ImportShipNo" runat="server" OnClick="btn_ImportShipNo_Click" Style="display: none" />
                    &nbsp;
                    (<a href="<%:fn_Param.WebUrl %>myShipping_CHN/excel/Shipment_CustCnt.xlsx">範例</a>)
                </div>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Advance Search Start -->
        <div class="ui orange attached segment">
            <div id="searchForm" class="ui small form">
                <div class="fields">
                    <div class="six wide field">
                        <label>指定年月</label>
                        <div class="two fields">
                            <div class="field">
                                <asp:DropDownList ID="filter_Year" runat="server" CssClass="fluid topSearch">
                                </asp:DropDownList>
                            </div>
                            <div class="field">
                                <asp:DropDownList ID="filter_Month" runat="server" CssClass="fluid topSearch">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                    <div class="ten wide field">
                        <label>&nbsp;</label>
                        <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                        <asp:Button ID="btn_Search" runat="server" Text="Button" OnClick="btn_Search_Click" Style="display: none" />
                    </div>
                </div>
            </div>
        </div>
        <!-- Advance Search End -->

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
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound" OnDataBound="lvDataList_DataBound">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table class="ui celled selectable compact table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3 center aligned">年月</th>
                                    <th class="grey-bg lighten-3 center aligned">客戶代號</th>
                                    <th class="grey-bg lighten-3 center aligned">客戶名稱</th>
                                    <th class="grey-bg lighten-3 center aligned">代發筆數</th>
                                    <th class="grey-bg lighten-3 center aligned">限制筆數</th>
                                </tr>
                                <tr class="grey-bg lighten-4">
                                    <td colspan="3" class="center aligned">總計</td>
                                    <td class="center aligned"><b>
                                        <asp:Label ID="head_Tot1" runat="server"></asp:Label></b>
                                    </td>
                                    <td class="center aligned"><b>
                                        <asp:Label ID="head_Tot2" runat="server"></asp:Label></b>
                                    </td>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                            <tfoot>
                                <tr class="grey-bg lighten-4">
                                    <td colspan="3" class="center aligned">總計</td>
                                    <td class="center aligned"><b>
                                        <asp:Label ID="lb_Tot1" runat="server"></asp:Label></b>
                                    </td>
                                    <td class="center aligned"><b>
                                        <asp:Label ID="lb_Tot2" runat="server"></asp:Label></b>
                                    </td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>

                </LayoutTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="center aligned">
                            <%#Eval("setYear") %> / <%#Eval("setMonth") %>
                        </td>
                        <td class="center aligned">
                            <%#Eval("CustID") %>
                        </td>
                        <td class="center aligned">
                            <%#Eval("CustName") %>
                        </td>
                        <td class="center aligned green-text">
                            <b><%#Eval("doSentCnt") %></b>
                        </td>
                        <td class="center aligned red-text">
                            <b><%#Eval("SendCnt") %></b>
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
            $(".myContentBody").keypress(function (e) {
                code = (e.keyCode ? e.keyCode : e.which);
                if (code == 13) {
                    $("#doSearch").trigger("click");
                    //避免觸發submit
                    e.preventDefault();
                }
            });

            //[搜尋][查詢鈕] - 觸發查詢
            $("#doSearch").click(function () {
                $("#MainContent_btn_Search").trigger("click");
            });

            //init dropdown list
            $('select.topSearch').dropdown();

            //Click:Import
            $("#MainContent_shipNoImport").change(function () {
                //confirm
                var r = confirm("請確認匯入的資料在「第一個工作表」?");
                if (r == true) {

                } else {
                    return false;
                }
                //loading
                $("#searchForm").addClass("loading");
                //trigger
                $("#MainContent_btn_ImportShipNo").trigger("click");
            });
        });
    </script>
</asp:Content>

