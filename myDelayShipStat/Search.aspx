<%@ Page Title="延遲出貨分析" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myDelayShipStat_Search" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
    <style>
        /* 自訂table
        */
        #myListTable {
            width: 100% !important;
        }

        table .colorHead th {
            background-color: #26a69a !important;
            color: #ffffff !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- breadcrumb Start -->
    <div class="ui vertical segment" style="background-color: #eee; padding-left: 15px;">
        <div class="ui small breadcrumb">
            <div class="section">生產採購</div>
            <i class="right angle icon divider"></i>
            <div class="active section">延遲出貨分析</div>
        </div>
    </div>
    <!-- breadcrumb End -->

    <div class="myContainer">
        <!-- filter section Start -->
        <div class="ui attached blue segment">
            <div class="ui two column grid">
                <div class="column">
                    <div class="ui small blue header">條件篩選</div>
                </div>
                <div class="column right aligned">
                    <button type="button" class="doToggle tiny ui icon blue button" data-target="section-filter" title="展開/收合"><i class="filter icon"></i></button>
                </div>
            </div>
        </div>
        <div class="section-filter ui attached segment">
            <div class="ui small form">
                <div class="fields">
                    <div class="eight wide field">
                        <label>發延日期</label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_sDate" placeholder="開始日" autocomplete="off" value="<%=sDate %>" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_eDate" placeholder="結束日" autocomplete="off" value="<%=eDate %>" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="four wide field">
                        <label>流程公司別</label>
                        <select id="filter_Comp" class="dropdown">
                            <option value="">-- 全部 --</option>
                            <option value="A">台灣</option>
                            <option value="B">上海</option>
                            <option value="C">三角</option>
                        </select>
                    </div>
                    <div class="four wide field">
                        <label>延遲原因</label>
                        <select id="filter_Reason" class="dropdown">
                            <option value="">-- 全部 --</option>
                            <option value="A">採購端</option>
                            <option value="B">業務端</option>
                        </select>
                    </div>
                </div>
                <div class="four fields">
                    <div class="field">
                        <label>供應商</label>
                        <input type="text" id="filter_Supplier" placeholder="輸入供應商代號或名稱關鍵字" maxlength="20" autocomplete="off" />
                    </div>
                    <div class="field">
                        <label>品號</label>
                        <input type="text" id="filter_ModelNo" placeholder="輸入品號關鍵字" maxlength="20" autocomplete="off" />
                    </div>
                    <div class="field">
                        <label>客戶</label>
                        <input type="text" id="filter_Cust" placeholder="輸入客戶代號或名稱關鍵字" maxlength="20" autocomplete="off" />
                    </div>
                    <div class="field">
                        <label>OPCS單號</label>
                        <input type="text" id="filter_OpcsNo" placeholder="輸入不含「-」的單號" maxlength="20" autocomplete="off" />
                    </div>
                </div>
            </div>
        </div>
        <div class="section-filter ui bottom attached segment">
            <div class="ui two column grid">
                <div class="column">
                    <button type="button" id="doExport" class="ui green small button"><i class="file excel outline icon"></i>Excel</button>
                    <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i>清除條件</a>
                </div>
                <div class="column right aligned">
                    <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                </div>
            </div>
        </div>
        <!-- filter section End -->

        <!-- 訊息顯示區 Start -->
        <div id="section-loading" class="ui icon orange message" style="display: none;">
            <i class="notched circle loading icon"></i>
            <div class="content">
                <div class="header">
                    資料處理中,請稍候....
                </div>
                <p>請勿關閉瀏覽器或按其他連結，不然資料取得速度會更慢。</p>
            </div>
        </div>

        <div id="section-message" class="ui red inverted segment" style="display: none;">
            <h5>OOPS....</h5>
        </div>
        <!-- 訊息顯示區 Start -->

        <!-- data section Start -->
        <div class="ui attached green segment">
            <div class="ui small green header">資料列表</div>
        </div>
        <div id="section-data" class="ui attached segment">
            <table id="myListTable" class="ui celled selectable compact small table">
                <thead>
                    <tr class="center aligned colorHead">
                        <th>公司別</th>
                        <th>發延日期</th>
                        <th data-tooltip="ERP訂單日" data-inverted="">接單日期</th>
                        <th data-tooltip="ERP訂單原預交日" data-inverted="">原出貨日</th>
                        <th>交期天數</th>
                        <th>OPCS</th>
                        <th>客戶</th>
                        <th>品號</th>
                        <th>訂單數量</th>
                        <th data-tooltip="EFGP預計出貨數" data-inverted="">可出<br />
                            數量</th>
                        <th data-tooltip="訂單數量 - 可出數量" data-inverted="">未交<br />
                            數量</th>
                        <th>幣別</th>
                        <th data-tooltip="訂單單價 * 未出數量" data-inverted="">未出金額</th>
                        <th data-tooltip="ERP訂單預交日" data-inverted="">可出貨日期</th>
                        <th>供應商</th>
                        <th>採購人員</th>
                        <th>延遲原因</th>
                    </tr>
                </thead>
            </table>
        </div>
        <!-- data section End -->

    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //載入radio選單
            $('.ui.radio.checkbox').checkbox();
            $('select.dropdown').dropdown();

            //提示
            $('#filter_OpcsNo').popup({
                inline: true,
                on: 'click',
                position: 'top left'
            });

            /* 條件篩選-收合/展開 */
            $(".doToggle").on("click", function () {
                var target = $(this).attr("data-target");

                $("." + target).slideToggle()
            });


            /* Click事件 - 匯出 */
            $("#doExport").on("click", function () {
                /* 取得資料 - 各欄位 */
                var _OpcsNo = $("#filter_OpcsNo").val();
                var _Cust = $("#filter_Cust").val();
                var _sDate = $("#filter_sDate").val();
                var _eDate = $("#filter_eDate").val();
                var _Comp = $("#filter_Comp").val();
                var _Reason = $("#filter_Reason").val();
                var _Supplier = $("#filter_Supplier").val();
                var _ModelNo = $("#filter_ModelNo").val();


                //檢查資料 - 預交日(起訖日)
                if (_sDate != "" && _eDate != "") {
                    var d1 = new Date(_sDate);
                    var d2 = new Date(_eDate);

                    if (d2 < d1) {
                        alert("結束日期不可小於開始日期");
                        return false;
                    }
                }

                //導向下載頁,帶入參數
                var url = "<%=fn_Param.WebUrl%>myDelayShipStat/Ashx_OutputExcel.ashx?OpcsNo=" + encodeURIComponent(_OpcsNo) +
                    "&sDate=" + encodeURIComponent(_sDate) + "&eDate=" + encodeURIComponent(_eDate) +
                    "&Comp=" + _Comp + "&Reason=" + _Reason +
                    "&Cust=" + encodeURIComponent(_Cust) + "&Supplier=" + encodeURIComponent(_Supplier) + "&ModelNo=" + encodeURIComponent(_ModelNo)
                ;

                window.open(url);

            });

        });
    </script>
    <%-- 日期選擇器 Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.js"></script>
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/options.js"></script>
    <script>
        $(function () {
            //載入datepicker
            $('.datepicker').calendar(calendarOpts_Range);
        });
    </script>
    <%-- 日期選擇器 End --%>

    <%-- DataTables Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/dataTables-1.10.18/datatables.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/dataTables-1.10.18/datatables.min.js"></script>
    <script>
        $(function () {
            /* 宣告共用參數 */
            var btnToggle = $(".doToggle");
            var s_filter = $(".section-filter");
            var s_loading = $("#section-loading");
            //var s_msg = $("#section-message");
            //var s_data = $("#section-data");

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
                var _OpcsNo = $("#filter_OpcsNo").val();
                var _Cust = $("#filter_Cust").val();
                var _sDate = $("#filter_sDate").val();
                var _eDate = $("#filter_eDate").val();
                var _Comp = $("#filter_Comp").val();
                var _Reason = $("#filter_Reason").val();
                var _Supplier = $("#filter_Supplier").val();
                var _ModelNo = $("#filter_ModelNo").val();


                //檢查資料 - 預交日(起訖日)
                if (_sDate != "" && _eDate != "") {
                    var d1 = new Date(_sDate);
                    var d2 = new Date(_eDate);

                    if (d2 < d1) {
                        alert("結束日期不可小於開始日期");
                        return false;
                    }
                }

                //畫面處理 - 顯示或隱藏
                s_filter.hide();    //條件區
                s_loading.show();   //載入區


                /* DataTables UI Start */

                //重置Datatable
                table.destroy();

                //Init
                table =
                 $('#myListTable').DataTable({
                     "processing": true,
                     "serverSide": true,
                     "ajax": {
                         "url": '<%=fn_Param.WebUrl%>' + 'myDelayShipStat/Ashx_GetData.ashx',
                         "type": "POST",
                         "data": {
                             OpcsNo: _OpcsNo,
                             Cust: _Cust,
                             sDate: _sDate,
                             eDate: _eDate,
                             Comp: _Comp,
                             Reason: _Reason,
                             Supplier: _Supplier,
                             ModelNo: _ModelNo
                         }
                     },
                     "searching": false,  //搜尋
                     "ordering": false,   //排序
                     "paging": true,     //分頁
                     "info": true,      //頁數資訊
                     "lengthChange": false,  //是否顯示筆數選單
                     //自訂顯示欄位
                     "columns": [
                         { data: "CompName", className: "center aligned" },
                         { data: "PendingDate", className: "center aligned" },
                         { data: "OrderDate", className: "center aligned" },
                         { data: "ShipDateOld", className: "center aligned" },
                         { data: "RangeDays", className: "right aligned" },
                         {//OPCS No
                             data: function (source, type, val) {
                                 var showID = source.OrderNoType
                                 var showName = source.OrderNo
                                 var html = '<span>' + showID + showName + '</span>'

                                 return html;
                             }, className: "center aligned"
                         },
                         { data: "CustName" },
                         { data: "ModelNo", className: "center aligned" },
                         { data: "OrderNum", className: "right aligned" },
                         {//可出數量
                             data: function (source, type, val) {
                                 var getVal = source.NewQty
                                 var showVal = getVal == -999 ? '<small class="grey-text text-darken-2">(整批)</small>' : getVal;
                                 var html = showVal;

                                 return html;
                             }, className: "right aligned"
                         },
                         { data: "OrderNum_Pend", className: "right aligned" },
                         { data: "Currency", className: "center aligned" },
                         { data: "PendingPrice", className: "right aligned" },
                         { data: "ShipDateNew", className: "center aligned" },
                         { data: "Supplier", className: "center aligned" },
                         { data: "Purchaser", className: "center aligned" },
                         { data: "ReasonName" }
                     ],
                     "pageLength": 20,   //顯示筆數預設值
                     "language": {
                         //自訂頁數資訊
                         "info": 'Total <strong class="text-success">_TOTAL_</strong> ,Current page <strong class="text-success">_PAGE_</strong> / _PAGES_'
                     },
                     //捲軸設定
                     "scrollY": '55vh',
                     "scrollCollapse": true,
                     "scrollX": true,
                     "initComplete": function (settings, json) {
                         s_loading.hide();
                     }
                 });

                /* DataTables UI End */
                }
        });
    </script>
    <%-- DataTables End --%>
</asp:Content>

