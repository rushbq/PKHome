<%@ Page Title="客戶備註" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="CustSearch.aspx.cs" Inherits="CustSearch" %>

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
                    <div class="section">訂單作業</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">客戶備註 (<%:Req_DBS %>)
                    </h5>
                </div>
            </div>
            <div class="right menu">
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Advance Search Start -->
        <div class="ui orange attached segment">
            <div class="ui small form">
                <div class="fields">
                    <div class="five wide field">
                        <label>關鍵字</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" MaxLength="20" autocomplete="off" placeholder="客戶關鍵字"></asp:TextBox>
                    </div>
                    <div class="seven wide field"></div>
                    <div class="four wide field" style="text-align: right;">
                        <label>&nbsp;</label>
                        <a href="<%=thisPage %>?dbs=<%:Req_DBS %>" class="ui small button"><i class="refresh icon"></i>重置</a>
                        <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                        <asp:Button ID="btn_Search" runat="server" Text="search" OnClick="btn_Search_Click" Style="display: none" />
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
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound" OnItemCommand="lvDataList_ItemCommand">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3"></th>
                                    <th class="grey-bg lighten-3 collapsing center aligned">客戶編號</th>
                                    <th class="grey-bg lighten-3">客戶名</th>
                                    <th class="grey-bg lighten-3">公版備註</th>
                                    <th class="grey-bg lighten-3">2210備註</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                        </table>
                    </div>

                    <!-- List Pagination Start -->
                    <div class="ui mini bottom attached segment grey-bg lighten-4">
                        <asp:Literal ID="lt_Pager" runat="server"></asp:Literal>
                    </div>
                    <!-- List Pagination End -->
                </LayoutTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="center aligned collapsing">
                            <a class="ui small teal basic icon button" href="<%=FuncPath() %>CustEdit.aspx?dbs=<%=Req_DBS %>&id=<%#Eval("Data_ID") %>&Cust=<%#Eval("CustID") %>" title="編輯">
                                <i class="pencil icon"></i>
                            </a>

                            <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
                        </td>
                        <td class="center aligned">
                            <%#Eval("CustID") %>
                        </td>
                        <td>
                            <%#Eval("CustName") %>
                        </td>
                        <td class="showPopup" data-id="remk1_<%#Eval("Data_ID") %>" title="點擊放大" style="cursor: pointer;">
                            <%#Eval("Remk_Normal").ToString().Left(100) %>

                            <asp:PlaceHolder ID="ph_Modal_r1" runat="server">
                                <!-- Modal Start -->
                                <div id="remk1_<%#Eval("Data_ID") %>" class="ui modal">
                                    <div class="header">(<%#Eval("CustID") %>) - <%#Eval("CustName") %>&nbsp;公版備註</div>
                                    <div class="scrolling content"><%# Eval("Remk_Normal").ToString().Replace("\n","<br>").Replace("\r","<br>") %></div>
                                    <div class="actions">
                                        <div class="ui black deny button">
                                            Close
                                        </div>
                                    </div>
                                </div>
                                <!--  Modal End -->
                            </asp:PlaceHolder>
                        </td>
                        <td class="showPopup" data-id="remk2_<%#Eval("Data_ID") %>" title="點擊放大" style="cursor: pointer;">
                            <%#Eval("Remk_2210").ToString().Left(100) %>

                            <asp:PlaceHolder ID="ph_Modal_r2" runat="server">
                                <!-- Modal Start -->
                                <div id="remk2_<%#Eval("Data_ID") %>" class="ui modal">
                                    <div class="header">(<%#Eval("CustID") %>) - <%#Eval("CustName") %>&nbsp;2210備註</div>
                                    <div class="scrolling content"><%# Eval("Remk_2210").ToString().Replace("\n","<br>") %></div>
                                    <div class="actions">
                                        <div class="ui black deny button">
                                            Close
                                        </div>
                                    </div>
                                </div>
                                <!--  Modal End -->
                            </asp:PlaceHolder>
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


            //Modal-備註
            $(".showPopup").on("click", function () {
                var id = $(this).attr("data-id");
                //show modal
                $('#' + id)
                    .modal({
                        selector: {
                            close: '.close, .actions .button'
                        }
                    })
                    .modal('show');
            });

        });
    </script>
</asp:Content>

