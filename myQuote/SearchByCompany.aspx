<%@ Page Title="集團報價" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SearchByCompany.aspx.cs" Inherits="myQuote_SearchByProd" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
    <style>
        /* datatable processing 文字顯示 */
        .dataTables_processing {
            top: 200px !important;
            z-index: 11000 !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">業務行銷</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">集團報價
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a href="#!" id="exportExcel" class="item"><i class="file excel icon"></i><span class="mobile hidden">匯出</span></a>
                <asp:Button ID="btn_Excel" runat="server" Text="excel trigger" OnClick="btn_Excel_Click" Style="display: none;" />
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
                        <label>銷售類別</label>
                        <asp:DropDownList ID="ddl_Class" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label>品號關鍵字</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" placeholder="輸入品號關鍵字" MaxLength="50" autocomplete="off"></asp:TextBox>
                    </div>
                    <div class="three wide field">
                        <label>貨號關鍵字</label>
                        <asp:TextBox ID="filter_ItemNo" runat="server" placeholder="輸入貨號關鍵字" MaxLength="50" autocomplete="off"></asp:TextBox>
                    </div>
                    <div class="three wide field">
                        <label>目錄</label>
                        <asp:DropDownList ID="ddl_Vol" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="fields">
                    <div class="eight wide field">
                        <div class="fields">
                            <div class="six wide field">
                                <asp:DropDownList ID="ddl_dateType" runat="server" CssClass="fluid">
                                    <asp:ListItem Value="A">上市日</asp:ListItem>
                                    <asp:ListItem Value="B">停售日</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="ten wide field">
                                <div class="two fields">
                                    <div class="field">
                                        <div class="ui left icon input datepicker">
                                            <asp:TextBox ID="filter_sDate" runat="server" placeholder="開始日" autocomplete="off"></asp:TextBox>
                                            <i class="calendar alternate outline icon"></i>
                                        </div>
                                    </div>
                                    <div class="field">
                                        <div class="ui left icon input datepicker">
                                            <asp:TextBox ID="filter_eDate" runat="server" placeholder="結束日" autocomplete="off"></asp:TextBox>
                                            <i class="calendar alternate icon"></i>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="ui grid">
                <div class="five wide column">
                    <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i>重置條件</a>
                </div>
                <div class="six wide column">
                    <span class="ui tag label">值：USD=NTD</span>
                    <span class="ui tag label">毛利率：NTD =
                        <asp:Label ID="lb_twRate" runat="server" CssClass="green-text text-darken-3"></asp:Label>
                        ; RMB =
                        <asp:Label ID="lb_shRate" runat="server" CssClass="teal-text text-darken-1"></asp:Label>
                    </span>
                </div>
                <div class="five wide column right aligned">
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
                <table id="myListTable" class="ui celled compact small structured table" style="width: 100%">
                    <thead>
                        <tr>
                            <th class="grey-bg lighten-3" rowspan="2">品號</th>
                            <th class="grey-bg lighten-3" rowspan="2">目錄</th>
                            <th class="grey-bg lighten-3" rowspan="2">頁次</th>
                            <th class="grey-bg lighten-3" rowspan="2">主要<br />
                                出貨地</th>
                            <th class="grey-bg lighten-3 center aligned" colspan="3">台灣成本(NTD)</th>
                            <th class="grey-bg lighten-3 center aligned" colspan="3">上海成本(RMB)</th>
                            <th class="grey-bg lighten-3 center aligned" colspan="6">外銷(USD)</th>
                            <th class="blue-bg lighten-3 center aligned" colspan="12">中國市場(RMB)</th>
                            <th class="green-bg lighten-3 center aligned" colspan="4">台灣市場(NTD)</th>

                            <th class="grey-bg lighten-3" rowspan="2">品名</th>
                            <th class="grey-bg lighten-3" rowspan="2">包裝方式</th>
                            <th class="grey-bg lighten-3" rowspan="2">上市日</th>
                            <th class="grey-bg lighten-3" rowspan="2">停售日</th>
                        </tr>
                        <tr>
                            <!-- 台灣成本 -->
                            <th class="grey-bg lighten-3 right aligned numFmt">標準成本</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">採購<br />
                                最新核價</th>
                            <th class="grey-bg lighten-3 center aligned">核價日</th>
                            <!-- 上海成本 -->
                            <th class="grey-bg lighten-3 right aligned numFmt">標準成本</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">採購<br />
                                最新核價</th>
                            <th class="grey-bg lighten-3 center aligned">核價日</th>

                            <!-- 外銷(USD) -->
                            <th class="grey-bg lighten-3 right aligned numFmt">台灣<br />
                                Agent價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
                            <th class="grey-bg lighten-3 center aligned">台灣<br />
                                生效日</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">上海<br />
                                Agent價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
                            <th class="grey-bg lighten-3 center aligned">上海<br />
                                生效日</th>

                            <!-- 中國市場 -->
                            <th class="grey-bg lighten-3 right aligned numFmt">業務底價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">中國<br />
                                經銷價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">中國<br />
                                網路價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">京東<br />
                                採購價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">京東<br />
                                頁面價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">零售價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
                            <!-- 台灣市場 -->
                            <th class="grey-bg lighten-3 right aligned numFmt">台灣<br />
                                網路價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">內銷<br />
                                經銷價</th>
                            <th class="grey-bg lighten-3 center aligned">利潤率</th>
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


            //匯出excel
            $("#exportExcel").click(function () {
                //trigger button
                $("#MainContent_btn_Excel").trigger("click");
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
            $('.datepicker').calendar(calendarOpts_Range_unLimit);
        });
    </script>
    <%-- 日期選擇器 End --%>

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
                //search job
                doSearch();
            });

            //點擊時變更背景色
            $('#myListTable tbody').on('click', 'tr', function () {
                var bgcolor = 'yellow-bg darken-1';
                var targetBg = 'tr.yellow-bg.darken-1';

                table.$(targetBg).removeClass(bgcolor); //移除其他列背景
                $(this).addClass(bgcolor); //此列新增背景
            });

            /* Search function */
            function doSearch() {
                /* 取得資料 - 各欄位 */
                var _ClassID = $("#MainContent_ddl_Class").val();
                var _Vol = $("#MainContent_ddl_Vol").val();
                var _keyword = $("#MainContent_filter_Keyword").val();
                var _ItemNo = $("#MainContent_filter_ItemNo").val();
                var _dateType = $("#MainContent_ddl_dateType").val();
                var _sDate = $("#MainContent_filter_sDate").val();
                var _eDate = $("#MainContent_filter_eDate").val();


                //畫面處理 - 顯示或隱藏
                s_data.addClass("loading");

                /* DataTables UI Start */

                //重置Datatable
                table.destroy();

                //Init
                table =
                 $('#myListTable').DataTable({
                     "responsive": false,
                     "processing": true,
                     "serverSide": true,
                     "ajax": {
                         "url": '<%=fn_Param.WebUrl%>' + 'myQuote/Ashx_GetDataByComp.ashx?v=1',
                         "type": "POST",
                         "data": {
                             ClassID: _ClassID,
                             Vol: _Vol,
                             Keyword: _keyword,
                             ItemNo: _ItemNo,
                             dateType: _dateType,
                             sDate: _sDate,
                             eDate: _eDate
                         }
                     },
                     "searching": false,  //搜尋
                     "ordering": false,   //排序
                     "paging": true,     //分頁
                     "info": true,      //頁數資訊
                     "lengthChange": false,  //是否顯示筆數選單
                     //自訂顯示欄位
                     "columns": [
                         { data: "Model_No", className: "collapsing center aligned green-text text-darken-2" },
                         { data: "CatVol", className: "collapsing center aligned" },
                         { data: "CatPage", className: "collapsing center aligned" },
                         { data: "Ship_From", className: "collapsing center aligned" },

                         /* TW成本 */
                         { data: "tw_StdCost", className: "right aligned" },
                         { data: "tw_PurPrice", className: "right aligned" },
                         { data: "tw_ChkDay", className: "center aligned" },


                         /* SH成本 */
                         { data: "sh_StdCost", className: "right aligned" },
                         { data: "sh_PurPrice", className: "right aligned" },
                         { data: "sh_ChkDay", className: "center aligned" },

                         { data: "tw_AgentPrice", className: "right aligned" }, //TW-Agent價
                         {
                             /* 利潤率(TW-Agent價) */
                             data: function (source, type, val) {
                                 var partX = source.tw_Rate_AgentPrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },
                         { data: "tw_ValidDate", className: "center aligned" }, //TW-生效日

                         { data: "sh_AgentPrice", className: "right aligned" }, //SH-Agent價
                         {
                             /* 利潤率(SH-Agent價) */
                             data: function (source, type, val) {
                                 var partX = source.sh_Rate_AgentPrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },
                         { data: "sh_ValidDate", className: "center aligned" }, //SH-生效日

                         /* SH市場 */
                         { data: "sh_LowestPrice", className: "right aligned" }, //業務底價
                         {
                             /* 利潤率(業務底價) */
                             data: function (source, type, val) {
                                 var partX = source.sh_Rate_LowestPrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },

                         { data: "sh_SellPrice", className: "right aligned" }, //中國經銷價
                         {
                             /* 利潤率(中國經銷價) */
                             data: function (source, type, val) {
                                 var partX = source.sh_Rate_SellPrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },

                         { data: "sh_NetPrice", className: "right aligned" }, //中國網路價
                         {
                             /* 利潤率(中國網路價) */
                             data: function (source, type, val) {
                                 var partX = source.sh_Rate_NetPrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },

                         { data: "PurPrice", className: "right aligned" }, //京東採購價
                         {
                             /* 利潤率(京東採購價) */
                             data: function (source, type, val) {
                                 var partX = source.sh_Rate_PurPrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },

                         { data: "ListPrice", className: "right aligned" }, //京東頁面價
                         {
                             /* 利潤率(京東頁面價) */
                             data: function (source, type, val) {
                                 var partX = source.sh_Rate_ListPrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },

                         { data: "sh_SalePrice", className: "right aligned" }, //零售價
                         {
                             /* 利潤率(零售價) */
                             data: function (source, type, val) {
                                 var partX = source.sh_Rate_SalePrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },

                         /* TW市場 */
                         { data: "tw_NetPrice", className: "right aligned" }, //台灣網路價
                         {
                             /* 利潤率(台灣網路價) */
                             data: function (source, type, val) {
                                 var partX = source.tw_Rate_NetPrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },

                         { data: "tw_InAgentPrice", className: "right aligned" }, //內銷經銷價
                         {
                             /* 利潤率(內銷經銷價) */
                             data: function (source, type, val) {
                                 var partX = source.tw_Rate_InAgentPrice;
                                 //顯示結果
                                 var result = partX == 0 ? '' : Math.round(partX) + ' %';

                                 return result;
                             }, className: "collapsing center aligned"
                         },

                         { data: "ModelName", className: "left aligned" },
                         { data: "Packing", className: "left aligned" },
                         { data: "onlineDate", className: "center aligned" },
                         { data: "offlineDate", className: "center aligned" }


                     ],
                     //自訂欄位格式
                     "columnDefs": [
                        {
                            //數字為0字體變色 / 千分位
                            "render": function (data, type, row) {
                                //呼叫格式化function
                                return formatNumber(data);
                            },
                            "targets": 'numFmt' //指定class(只能指定一次)
                        }
                     ],
                     "pageLength": 20,   //顯示筆數預設值
                     "language": {
                         //自訂頁數資訊
                         "info": '共 <b>_TOTAL_</b> 筆 ,目前頁次 <b>_PAGE_</b> / _PAGES_, 每頁 20 筆.',
                         //自訂processing文字
                         "processing": "<h2 class='red-text text-darken-2'>資料擷取中,請稍候...</h2>"
                     },
                     //捲軸設定
                     "scrollY": '60vh',
                     "scrollCollapse": true,
                     "scrollX": true,
                     "initComplete": function () {
                         //移除loading
                         s_data.removeClass("loading");
                     }
                 });

                /* table 重新後寫入觸發 */
                 table.on('draw', function () {
                     //*** UI載入完成後觸發 *** Message Modal
                     //$(".doShowMsg").on("click", function () {
                     //    //取資料
                     //    var name = $(this).attr("data-title"); //model no
                     //    var id = $(this).attr("data-id"); //id
                     //    var msgTW = $("#TWmsgDetail_" + id).val(); //get hidden field value
                     //    var msgSH = $("#SHmsgDetail_" + id).val(); //get hidden field value

                     //    //填入值
                     //    $("#itemID").text(name);
                     //    $("#twMsg").text(msgTW);
                     //    $("#shMsg").text(msgSH);

                     //    //顯示modal
                     //    $('#msgPage').modal('show');
                     //});
                 });

                //數字格式化(為0設為灰字 / 千分位)
                 function formatNumber(val) {
                     if (val == 0) {
                         val = '<span class="grey-text">' + val + '</span>';

                         return val;
                     } else {
                         var num = val.toString();
                         var pattern = /(-?\d+)(\d{3})/;

                         while (pattern.test(num)) {
                             num = num.replace(pattern, "$1,$2");

                         }
                         return num;
                     }
                 }


                /* DataTables UI End */
             }
        });
    </script>
    <%-- DataTables End --%>
</asp:Content>

