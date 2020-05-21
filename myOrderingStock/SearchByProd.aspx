<%@ Page Title="產品庫存狀況" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SearchByProd.aspx.cs" Inherits="myOrderingStock_SearchByProd" %>

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
                    <h5 class="active section red-text text-darken-2">產品庫存狀況
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <asp:LinkButton ID="lbtn_Excel" runat="server" CssClass="item" OnClick="lbtn_Excel_Click"><i class="file excel icon"></i><span class="mobile hidden">匯出</span></asp:LinkButton>
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
                        <label>產品類別</label>
                        <asp:DropDownList ID="filter_Class" runat="server">
                        </asp:DropDownList>
                    </div>
                    <div class="five wide field">
                        <label>產品關鍵字</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" MaxLength="20" autocomplete="off" placeholder="品號或品名關鍵字"></asp:TextBox>
                    </div>
                    <div class="five wide field">
                        <label>指定品號</label>
                        <asp:TextBox ID="filter_ModelNo" runat="server" MaxLength="20" autocomplete="off" placeholder="輸入完整品號,不區分大小寫"></asp:TextBox>
                    </div>
                </div>
            </div>
            <div class="ui two column grid">
                <div class="column">
                    <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i>重置條件</a>
                </div>
                <div class="column right aligned">
                    <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                </div>
            </div>

        </div>
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
            <div id="section-data" class="ui green attached segment">
                <table id="myListTable" class="ui celled selectable compact small structured table" style="width: 100%">
                    <thead>
                        <tr>
                            <th class="grey-bg lighten-3" rowspan="2">品號</th>
                            <th class="green-bg lighten-3 center aligned" colspan="5">台灣-01倉</th>
                            <th class="green-bg lighten-3 center aligned" colspan="5">台灣-20倉</th>
                            <th class="green-bg lighten-3 center aligned" colspan="5">台灣-21倉</th>
                            <th class="green-bg lighten-3 center aligned" colspan="5">台灣-22倉</th>
                            <th class="blue-bg lighten-3 center aligned" colspan="5">上海-12倉</th>
                            <th class="blue-bg lighten-3 center aligned" colspan="5">上海-128倉</th>
                            <th class="blue-bg lighten-3 center aligned" colspan="5">上海-A01倉</th>
                            <th class="orange-bg lighten-2 center aligned" colspan="5">深圳-A01倉</th>
                        </tr>
                        <tr>
                            <th class="grey-bg lighten-3 right aligned numFmt">01庫存</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計進</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計生</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計領</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">20庫存</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計進</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計生</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計領</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">21庫存</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計進</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計生</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計領</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">22庫存</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計進</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計生</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計領</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">12庫存</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計進</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計生</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計領</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">128庫存</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計進</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計生</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計領</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">A01庫存</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計進</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計生</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計領</th>
                            <!-- SZ -->
                            <th class="grey-bg lighten-3 right aligned numFmt">A01庫存</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計進</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計生</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">預計領</th>
                        </tr>
                    </thead>
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
            //偵測enter
            $(".myContentBody").keypress(function (e) {
                code = (e.keyCode ? e.keyCode : e.which);
                if (code == 13) {
                    $("#doSearch").trigger("click");
                    //避免觸發submit
                    e.preventDefault();
                }
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
            /* 宣告共用參數 */
            var s_data = $("#section-data");

            //宣告空的datatable
            var table = $('#myListTable').DataTable({
                "searching": false,  //搜尋
                "ordering": false,   //排序
                "paging": false,     //分頁
                "info": false      //筆數資訊
            });

            /* Click事件 - 查詢 */
            $("#doSearch").on("click", function () {
                doSearch();
            });

            /* Search function */
            function doSearch() {
                /* 取得資料 - 各欄位 */
                var _Keyword = $("#MainContent_filter_Keyword").val();
                var _ModelNo = $("#MainContent_filter_ModelNo").val();
                var _ClassID = $("#MainContent_filter_Class").val();
                var _Lang = '<%=Req_Lang%>';

                //畫面處理 - 顯示或隱藏
                s_data.addClass("loading");

                /* DataTables UI Start */

                //重置Datatable
                table.destroy();

                //Init
                table =
                 $('#myListTable').DataTable({
                     "processing": true,
                     "serverSide": true,
                     "ajax": {
                         "url": '<%=fn_Param.WebUrl%>' + 'myOrderingStock/Ashx_GetData.ashx',
                         "type": "POST",
                         "data": {
                             Keyword: _Keyword,
                             ModelNo: _ModelNo,
                             ClassID: _ClassID,
                             Lang: _Lang
                         }
                     },
                     "searching": false,  //搜尋
                     "ordering": false,   //排序
                     "paging": true,     //分頁
                     "info": true,      //頁數資訊
                     "lengthChange": false,  //是否顯示筆數選單
                     //自訂顯示欄位
                     "columns": [
                          {
                              data: function (source, type, val) {
                                  var showID = source.ModelNo
                                  var showName = source.ModelName
                                  var html = '<b class="green-text text-darken-2">' + showID + '</b><div class="grey-text text-darken-2"><small>' + showName + '</small></div>'

                                  return html;
                              }, className: "collapsing"
                          },
                         { data: "StockQty_01", className: "collapsing right aligned warning" },
                         { data: "PreSell_01", className: "right aligned" },
                         { data: "PreIN_01", className: "right aligned" },
                         { data: "PreSet_01", className: "right aligned" },
                         { data: "PreGet_01", className: "right aligned" },
                         { data: "StockQty_20", className: "right aligned warning" },
                         { data: "PreSell_20", className: "right aligned" },
                         { data: "PreIN_20", className: "right aligned" },
                         { data: "PreSet_20", className: "right aligned" },
                         { data: "PreGet_20", className: "right aligned" },
                         { data: "StockQty_21", className: "right aligned warning" },
                         { data: "PreSell_21", className: "right aligned" },
                         { data: "PreIN_21", className: "right aligned" },
                         { data: "PreSet_21", className: "right aligned" },
                         { data: "PreGet_21", className: "right aligned" },
                         { data: "StockQty_22", className: "right aligned warning" },
                         { data: "PreSell_22", className: "right aligned" },
                         { data: "PreIN_22", className: "right aligned" },
                         { data: "PreSet_22", className: "right aligned" },
                         { data: "PreGet_22", className: "right aligned" },
                         { data: "StockQty_12", className: "right aligned warning" },
                         { data: "PreSell_12", className: "right aligned" },
                         { data: "PreIN_12", className: "right aligned" },
                         { data: "PreSet_12", className: "right aligned" },
                         { data: "PreGet_12", className: "right aligned" },
                         { data: "StockQty_128", className: "right aligned warning" },
                         { data: "PreSell_128", className: "right aligned" },
                         { data: "PreIN_128", className: "right aligned" },
                         { data: "PreSet_128", className: "right aligned" },
                         { data: "PreGet_128", className: "right aligned" },
                         { data: "StockQty_A01", className: "right aligned warning" },
                         { data: "PreSell_A01", className: "right aligned" },
                         { data: "PreIN_A01", className: "right aligned" },
                         { data: "PreSet_A01", className: "right aligned" },
                         { data: "PreGet_A01", className: "right aligned" },
                         { data: "SZ_StockQty_A01", className: "right aligned warning" },
                         { data: "SZ_PreSell_A01", className: "right aligned" },
                         { data: "SZ_PreIN_A01", className: "right aligned" },
                         { data: "SZ_PreSet_A01", className: "right aligned" },
                         { data: "SZ_PreGet_A01", className: "right aligned" },
                     ],
                     //自訂欄位格式
                     "columnDefs": [
                        {
                            "render": function (data, type, row) {
                                return formatNumber(data);
                            },
                            "targets": 'numFmt' //指定class
                        },
                     ],
                     "pageLength": 20,   //顯示筆數預設值
                     "language": {
                         //自訂頁數資訊
                         "info": '共 <b>_TOTAL_</b> 筆 ,目前頁次 <b>_PAGE_</b> / _PAGES_, 每頁 20 筆.'
                     },
                     //捲軸設定
                     "scrollY": '60vh',
                     "scrollCollapse": true,
                     "scrollX": true,
                     "initComplete": function (settings, json) {
                         //移除loading
                         s_data.removeClass("loading");
                     }
                 });

                //數字格式化(為0設為灰字)
                function formatNumber(val) {
                    if (val == 0) {
                        val = '<span class="grey-text">' + val + '</span>';
                    }
                    
                    return val;
                }
                /* DataTables UI End */
             }
        });
    </script>
    <%-- DataTables End --%>
</asp:Content>

