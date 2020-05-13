<%@ Page Title="週統計" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="StatWeek.aspx.cs" Inherits="myShipping_StatWeek" %>

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
                        週統計 - 
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
        <!-- Advance Search Start -->
        <div class="ui orange attached segment">
            <div class="ui small form">
                <div class="fields">
                    <div class="four wide field">
                        <label>年份</label>
                        <asp:TextBox ID="filter_Year" runat="server" autocomplete="off" type="number"></asp:TextBox>
                    </div>
                    <div class="four wide field">
                        <label>出貨地</label>
                        <asp:DropDownList ID="filter_ShipFrom" runat="server">
                            <asp:ListItem Value="ALL">-- 全部 --</asp:ListItem>
                            <asp:ListItem Value="SH">上海</asp:ListItem>
                            <asp:ListItem Value="SZ">深圳</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="eight wide field">
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
            <div class="ui green attached segment">
                <table id="myListTable" class="ui celled selectable compact small table">
                    <thead>
                        <asp:Literal ID="lt_header" runat="server"></asp:Literal>
                    </thead>
                    <tbody>
                        <asp:Literal ID="lt_body" runat="server"></asp:Literal>
                    </tbody>
                </table>
            </div>
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
                $("#MainContent_btn_Search").trigger("click");
            });

            //init dropdown list
            $('select').dropdown();

        });
    </script>
    <%-- DataTables Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/dataTables-1.10.18/datatables.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/dataTables-1.10.18/datatables.min.js"></script>
    <script>
        $(function () {
            //宣告空的datatable
            var table = $('#myListTable').DataTable({
                searching: false,  //搜尋
                ordering: false,   //排序
                paging: false,     //分頁
                info: false,      //筆數資訊
                autoWidth: true,  //有設定columnDefs,要將autoWidth設為true
                language: {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                columnDefs: [
                 { width: "100px", targets: 0 },
                 { width: "40px", targets: 53 }
                ],
                //捲軸設定
                "scrollY": '55vh',
                "scrollCollapse": true,
                "scrollX": true
            });

        });
    </script>
    <%-- DataTables End --%>
</asp:Content>

