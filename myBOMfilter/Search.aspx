<%@ Page Title="BOM篩選" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myBOMfilter_Search" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">生產採購</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">BOM篩選 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
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
                    <div class="seven wide required field">
                        <label>指定品號</label>
                        <div class="ui fluid search ac-ModelNo">
                            <div class="ui left icon right labeled input">
                                <asp:TextBox ID="filter_ModelNo" runat="server" CssClass="prompt" MaxLength="20" autocomplete="off" placeholder="輸入完整品號,不區分大小寫"></asp:TextBox>
                                <i class="search icon"></i>
                                <asp:Panel ID="lb_ModelNo" runat="server" CssClass="ui label">輸入品號</asp:Panel>
                            </div>
                        </div>
                    </div>
                    <div class="four wide field">
                        <label>資料庫</label>
                        <asp:RadioButtonList ID="filter_DBS" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem Value="TW">台灣&nbsp;</asp:ListItem>
                            <asp:ListItem Value="SH">上海</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="four wide field">
                        <label>資料範圍</label>
                        <asp:RadioButtonList ID="filter_StopType" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem Value="" Selected="True">全部&nbsp;</asp:ListItem>
                            <asp:ListItem Value="Stop">排除已停售</asp:ListItem>
                        </asp:RadioButtonList>
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
                            <th class="grey-bg lighten-3">品號</th>
                            <th class="grey-bg lighten-3">工具組品號</th>
                            <th class="grey-bg lighten-3">工具組品名</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">用量</th>
                            <th class="grey-bg lighten-3">主供應商</th>
                            <th class="grey-bg lighten-3 center aligned">出貨地</th>
                            <th class="grey-bg lighten-3">產銷訊息</th>
                            <th class="grey-bg lighten-3">倉管屬性</th>
                            <th class="grey-bg lighten-3">目錄</th>
                            <th class="grey-bg lighten-3">頁次</th>
                            <th class="grey-bg lighten-3">上市日期</th>
                            <th class="grey-bg lighten-3">最近<br />
                                出貨時間</th>
                            <th class="grey-bg lighten-3">最近<br />
                                出貨客戶</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">最近<br />
                                出貨數量</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">近一年銷量</th>
                            <th class="grey-bg lighten-3">品號屬性</th>
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
    <%-- Search UI Start --%>
    <script>
        /* 品號 (使用category) */
        $('.ac-ModelNo').search({
            type: 'category',
            minCharacters: 1,
            searchFields: [
                'title',
                'description'
            ]
            , onSelect: function (result, response) {
                //console.log(result.title);
                $("#MainContent_filter_ModelNo").val(result.title);
                $("#MainContent_lb_ModelNo").text(result.description);
            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Prod_v1.ashx?show=Y&q={query}',
                onResponse: function (ajaxResp) {
                    //宣告空陣列
                    var response = {
                        results: {}
                    }
                    ;
                    // translate API response to work with search
                    /*
                      取得遠端資料後處理
                      .results = 物件名稱
                      item.Category = 要群組化的欄位
                      maxResults = 查詢回傳筆數

                    */
                    $.each(ajaxResp.results, function (index, item) {
                        var
                          categoryContent = item.Category || 'Unknown',
                          maxResults = 10
                        ;
                        if (index >= maxResults) {
                            return false;
                        }
                        // create new categoryContent category
                        if (response.results[categoryContent] === undefined) {
                            response.results[categoryContent] = {
                                name: categoryContent,
                                results: []
                            };
                        }

                        //重組回傳結果(指定顯示欄位)
                        response.results[categoryContent].results.push({
                            title: item.ID,
                            description: item.Label
                        });
                    });
                    return response;
                }
            }

        });

    </script>
    <%-- Search UI End --%>

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
                var _ModelNo = $("#MainContent_filter_ModelNo").val();
                var _DBS = $('input:radio:checked[name="ctl00$MainContent$filter_DBS"]').val();
                var _StopType = $('input:radio:checked[name="ctl00$MainContent$filter_StopType"]').val();


                /* Check required field */
                if (_ModelNo == "" || _DBS == "") {
                    alert('品號 / 資料庫為必填');
                    return false;
                }

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
                         "url": '<%=fn_Param.WebUrl%>' + 'myBOMfilter/Ashx_GetData.ashx',
                         "type": "POST",
                         "data": {
                             ModelNo: _ModelNo,
                             DBS: _DBS,
                             StopType: _StopType
                         }
                     },
                     "searching": false,  //搜尋
                     "ordering": false,   //排序
                     "paging": true,     //分頁
                     "info": true,      //頁數資訊
                     "lengthChange": false,  //是否顯示筆數選單
                     //自訂顯示欄位
                     "columns": [
                         { data: "PartModelNo", className: "collapsing" },
                         { data: "MainModelNo", className: "collapsing green-text text-darken-3" },
                         { data: "MainModelName", className: "" },
                         { data: "Qty", className: "right aligned" },
                         { data: "SupName", className: "" },
                         { data: "Ship_From", className: "center aligned" },
                         { data: "MarketMsg", className: "" },
                         { data: "StockProp", className: "" },
                         { data: "Vol", className: "" },
                         { data: "Page", className: "" },
                         { data: "Date_Of_Listing", className: "" },
                         { data: "SO_Date", className: "" },
                         { data: "CustName", className: "" },
                         { data: "SO_Qty", className: "right aligned" },
                         { data: "YearQty", className: "right aligned" },
                         { data: "ProdProp", className: "" }

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

