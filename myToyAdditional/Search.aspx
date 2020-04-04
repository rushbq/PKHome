<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myToyAdditional_Search" %>

<asp:Content ID="myCss" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="myBody" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- Sub Header Start -->
    <div class="grey lighten-3">
        <div class="container">
            <div class="row">
                <div class="col s12 m12 l12">
                    <ol class="breadcrumb">
                        <li><a>科學玩具補件登記簿</a></li>
                        <li class="active">資料列表</li>
                    </ol>
                </div>
            </div>
        </div>
    </div>
    <!-- Sub Header End -->
    <!-- Body Content Start -->
    <div class="row">
        <div class="col s12">
            <div class="card-panel">
                <div class="row">
                    <div class="input-field col s6 m5 l5">
                        <asp:TextBox ID="filter_Keyword" runat="server" placeholder="品號 / 聯繫人 / 電話 / 地址 / 貨運單號" MaxLength="50" autocomplete="off"></asp:TextBox>
                        <label for="MainContent_filter_Keyword">關鍵字查詢</label>
                    </div>
                    <div class="input-field col s6 m5 l6 right-align">
                        <a id="trigger-keySearch" class="btn waves-effect waves-light blue" title="查詢"><i class="material-icons">search</i></a>
                        <asp:PlaceHolder ID="ph_Edit" runat="server">
                            <a href="<%=FuncPath() %>/Edit" class="btn waves-effect waves-light red" title="新增"><i class="material-icons">add</i></a>
                        </asp:PlaceHolder>
                        <asp:LinkButton ID="lbtn_Export" runat="server" CssClass="btn waves-effect waves-light green" OnClick="lbtn_Export_Click">Excel</asp:LinkButton>
                        <asp:Button ID="btn_KeySearch" runat="server" Text="Search" OnClick="btn_KeySearch_Click" Style="display: none;" />
                    </div>
                </div>
                <div>
                    <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound" OnItemCommand="lvDataList_ItemCommand">
                        <LayoutTemplate>
                            <div>
                                <table id="myListTable" class="bordered highlight">
                                    <thead>
                                        <tr>
                                            <th class="center-align">ID</th>
                                            <th>聯繫資料</th>
                                            <th>產品</th>
                                            <th class="center-align">寄出日</th>
                                            <th class="center-align">貨運號</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:PlaceHolder ID="ph_Items" runat="server" />
                                    </tbody>
                                </table>
                            </div>
                            <asp:Literal ID="lt_Pager" runat="server"></asp:Literal>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr>
                                <td class="grey-text center-align"><%#Eval("SeqNo") %></td>
                                <td>
                                    <div>
                                        <i class="tiny material-icons grey-text text-darken-1">person_pin</i>&nbsp;<strong><%#Eval("CustName") %></strong>
                                        <span class="new badge grey darken-1" data-badge-caption="<%#Eval("CustTypeName") %>"></span>
                                    </div>
                                    <div>
                                        <i class="tiny material-icons grey-text text-darken-1">local_phone</i>&nbsp;<%#Eval("CustTel") %>
                                    </div>
                                    <div class="truncate">
                                        <i class="tiny material-icons grey-text text-darken-1">place</i><small>&nbsp;<%# Eval("CustAddr") %></small>
                                    </div>
                                </td>
                                <td>
                                    <div><%#Eval("ModelNo") %></div>
                                    <p><%#Eval("ModelName") %></p>
                                </td>
                                <td class="grey-text text-darken-2 center-align">
                                    <%#Eval("ShipDate") %>
                                </td>
                                <td class="grey-text text-darken-2 center-align">
                                    <%#Eval("ShipNo") %>
                                </td>
                                <td class="left-align">
                                    <a class="waves-effect waves-light waves-teal btn white grey-text text-darken-2" href="<%:FuncPath() %>/View/<%#Eval("Data_ID") %>" title="查看"><i class="material-icons">visibility</i></a>
                                    <asp:PlaceHolder ID="ph_Edit" runat="server">
                                        <a class="waves-effect waves-light btn green" href="<%:FuncPath() %>/Edit/<%#Eval("Data_ID") %>" title="編輯"><i class="material-icons">edit</i></a>
                                    </asp:PlaceHolder>
                                    <p>
                                        <asp:PlaceHolder ID="ph_Send" runat="server">
                                            <a class="waves-effect waves-light btn light-blue" href="<%:FuncPath() %>/Edit/<%#Eval("Data_ID") %>/#ShipInfo" title="備貨"><i class="material-icons">local_shipping</i></a>
                                        </asp:PlaceHolder>
                                        <asp:PlaceHolder ID="ph_Close" runat="server">
                                            <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="waves-effect waves-light btn orange lighten-1" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('是否確定刪除?')"><i class="material-icons">delete_forever</i></asp:LinkButton>
                                        </asp:PlaceHolder>
                                    </p>
                                    <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
                                </td>
                            </tr>
                        </ItemTemplate>
                        <EmptyDataTemplate>
                            <div class="section">
                                <div class="card-panel grey darken-1">
                                    <i class="material-icons flow-text white-text">error_outline</i>
                                    <span class="flow-text white-text">目前的篩選條件找不到資料，請重新篩選。</span>
                                </div>
                            </div>
                        </EmptyDataTemplate>
                    </asp:ListView>
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
        $(function () {
            //[搜尋][查詢鈕] - 觸發查詢
            $("#trigger-keySearch").click(function () {
                $("#MainContent_btn_KeySearch").trigger("click");
            });


            //[搜尋][Enter鍵] - 觸發查詢
            $("#MainContent_filter_Keyword").keypress(function (e) {
                code = (e.keyCode ? e.keyCode : e.which);
                if (code == 13) {
                    $("#MainContent_btn_KeySearch").trigger("click");

                    e.preventDefault();
                }
            });
        });
    </script>
</asp:Content>

