<%@ Page Title="上海訂貨計劃(業務版) - A01倉" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search_SH_A01_Sales.aspx.cs" Inherits="myPurPlan_Search_SH_A01_Sales" %>

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
                    <h5 class="active section red-text text-darken-2">上海訂貨計劃(業務版) - A01倉
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
                        <label>估算天數</label>
                        <asp:TextBox ID="filter_Days" runat="server" type="number" min="1" max="365">90</asp:TextBox>
                    </div>
                    <div class="five wide field">
                        <label>指定範圍條件</label>
                        <asp:DropDownList ID="menuFilter" runat="server" CssClass="fluid">
                            <asp:ListItem Value="">不限</asp:ListItem>
                            <%--<asp:ListItem Value="A">擬定數量 (庫存 + 預計進 + 計劃進 - 預計銷) < 安全存量</asp:ListItem>--%>
                            <asp:ListItem Value="B">催貨量 < 0</asp:ListItem>
                            <asp:ListItem Value="C">安全存量不為0</asp:ListItem>
                            <asp:ListItem Value="D">預計進 > 0</asp:ListItem>
                            <asp:ListItem Value="E">計劃進 > 0</asp:ListItem>
                            <asp:ListItem Value="F">虛擬預計銷 > 0</asp:ListItem>
                            <asp:ListItem Value="G">近N天用量 > 0</asp:ListItem>
                            <asp:ListItem Value="H">近N天用量 = 0</asp:ListItem>
                            <asp:ListItem Value="I">近N天用量 = 0, 可用量 > 0</asp:ListItem>
                            <asp:ListItem Value="J">安全存量 = 0, 可用量 > 0</asp:ListItem>
                            <asp:ListItem Value="K">可用量A01倉 > 0</asp:ListItem>
                            <%--<asp:ListItem Value="L">可用量12倉 > 0</asp:ListItem>--%>
                            <asp:ListItem Value="M">可用週轉月 < 1</asp:ListItem>
                            <asp:ListItem Value="N">可用週轉月 < 2</asp:ListItem>
                            <asp:ListItem Value="O">可用週轉月 < 2.5</asp:ListItem>
                            <asp:ListItem Value="P">可用週轉月 < 3.5</asp:ListItem>
                            <asp:ListItem Value="Q">可用週轉月 > 6</asp:ListItem>
                            <asp:ListItem Value="R">可用週轉月 > 12</asp:ListItem>
                            <asp:ListItem Value="S">可用週轉月 > 24</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <%--<div class="five wide field">
                        <label>廠商</label>
                        <asp:DropDownList ID="menuSup" runat="server" CssClass="ac-drpSup ui fluid search selection dropdown">
                            <asp:ListItem Value="">請選擇</asp:ListItem>
                        </asp:DropDownList>
                    </div>--%>
                </div>
                <div class="fields">
                    <div class="thirteen wide field">
                        <label>品號&nbsp;<small>(查詢品號,可選取多項目)</small></label>
                        <select id="menuProd" class="ac-drpProd ui fluid search selection dropdown" multiple="">
                            <option value="">請選擇</option>
                        </select>
                        <asp:TextBox ID="val_Prods" runat="server" Style="display: none"></asp:TextBox>
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
                <table id="myListTable" class="ui celled compact small structured table" style="width: 100%">
                    <thead>
                        <tr>
                            <th class="grey-bg lighten-3">品號</th>
                            <th class="grey-bg lighten-3">屬性</th>
                            <th class="grey-bg lighten-3">品名</th>
                            <th class="grey-bg lighten-3">目錄</th>
                            <th class="grey-bg lighten-3">頁次</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">庫存</th>
                            <th class="grey-bg lighten-3 right aligned">預計進</th> <!-- numFmt 不適用於有Url的欄位, 因為品號有數字... -->
                            <th class="grey-bg lighten-3 right aligned numFmt">虛擬入</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">計劃進</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">待驗收</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">現有周轉月</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">催貨量</th>
                            <th class="grey-bg lighten-3 right aligned">預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">虛擬預計銷</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">實際預計銷</th>
                            <th class="grey-bg lighten-3 right aligned">近N天用量</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">全年平均<br />
                                月用量(</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">去年當季<br />
                                平均用量</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">可用週轉月</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">可用量</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">安全存量</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">內盒數量</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">一箱數量</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">整箱材積</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">內銷MOQ</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">產銷訊息</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">12倉庫存</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">供應商</th>
                        </tr>
                    </thead>
                </table>
            </div>
        </asp:PlaceHolder>
        <!-- List Content End -->
    </div>
    <!-- 內容 End -->

    <!--// Modal,觸發語法在table.on('draw')裡 //-->
    <!-- Msg Modal Start -->
    <div id="msgPage" class="ui modal">
        <div class="header">
            產銷訊息 - <span class="titleName green-text text-darken-2"></span>
        </div>
        <div class="content">
            <div class="ui message">
                <p id="Msg"></p>
            </div>
        </div>
        <div class="actions">
            <div class="ui cancel button">
                關閉視窗
            </div>
        </div>
    </div>
    <!-- Msg Modal End -->

    <!-- 預計進 Modal Start -->
    <div id="detail_PreIN" class="ui small modal">
        <div class="header">
            預計進 - <span class="titleName green-text text-darken-2"></span>
        </div>
        <div class="scrolling content">
            <table class="ui striped celled table">
                <thead>
                    <tr>
                        <th class="center aligned">庫別</th>
                        <th class="center aligned">數量</th>
                        <th class="center aligned">預交日</th>
                        <th>單別</th>
                        <th>單號</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
        <div class="actions">
            <div class="ui cancel button">
                關閉視窗
            </div>
        </div>
    </div>
    <!-- 預計進 Modal End -->

    <!-- 預計銷 Modal Start -->
    <div id="detail_PreSell" class="ui small modal">
        <div class="header">
            預計銷 - <span class="titleName green-text text-darken-2"></span>
        </div>
        <div class="scrolling content">
            <table class="ui striped celled table">
                <thead>
                    <tr>
                        <th class="center aligned">庫別</th>
                        <th class="center aligned">數量</th>
                        <th class="center aligned">預交日</th>
                        <th>單別</th>
                        <th>單號</th>
                        <th class="center aligned">客戶代號</th>
                        <th class="center aligned">客戶名稱</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
        <div class="actions">
            <div class="ui cancel button">
                關閉視窗
            </div>
        </div>
    </div>
    <!-- 預計銷 Modal End -->

    <!-- 品號異動明細 Modal Start -->
    <div id="detail_InvHistory" class="ui small modal">
        <div class="header">
            品號歷史異動 - <span class="titleName green-text text-darken-2"></span>
        </div>
        <div class="scrolling content">
            <table class="ui striped celled table">
                <thead>
                    <tr>
                        <th class="center aligned">日期</th>
                        <th>單別</th>
                        <th>單號</th>
                        <th class="center aligned">庫別</th>
                        <th class="center aligned">數量</th>
                        <th class="center aligned">出入別</th>
                        <th>備註</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
        <div class="actions">
            <div class="ui cancel button">
                關閉視窗
            </div>
        </div>
    </div>
    <!-- 預計銷 Modal End -->
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
                //Get values of models
                doGetDrpVals();


                //trigger button
                $("#MainContent_btn_Excel").trigger("click");

            });


        });
    </script>

    <%-- 供應商選單 --%>
<%--    <script>
        /*
          search dropdown多選
          注意事項:
          需使用Html Controller, 不能使用 .Net元件
          , 因選項會變動, 會被視為安全性漏洞, 所以要用另一個ServerSide元件接收值
        */
        $('.ac-drpSup').dropdown({
            fields: {
                remoteValues: 'results',
                name: 'FullLabel',
                value: 'ID'
            },
            apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Supplier.ashx?q={query}&dbs=SH&v=1.0'
            }
        });
    </script>--%>

    <%-- 產品選單 --%>
    <script>
        /*
          search dropdown多選
          注意事項:
          需使用Html Controller, 不能使用 .Net元件
          , 因選項會變動, 會被視為安全性漏洞, 所以要用另一個ServerSide元件接收值
        */
        $('.ac-drpProd').dropdown({
            fields: {
                remoteValues: 'results',
                name: 'FullLabel',
                value: 'ID'
            },
            apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Prod_v1.ashx?show=Y&q={query}&v=1.0'
            }

        });

        //取得複選選單欄位值
        function doGetDrpVals() {
            //取得多選品號的值
            var procValue = $("#menuProd").dropdown("get value");
            var $fldProd = $("#MainContent_val_Prods");

            if (procValue.length == 0) {
                $fldProd.val("");
            } else {
                //將陣列轉成以#分隔的字串
                var myVals = procValue.join("#");
                //填入隱藏欄位(傳遞時使用)
                $fldProd.val(myVals);
                //console.log(myVals);
            }
        }

    </script>

    <%-- DataTables Start --%>
    <style type="text/css">
        .dataTables_processing {
            top: 100px !important;
            z-index: 11000 !important;
        }
    </style>
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
                //判斷多選選單,填入隱藏欄位
                doGetDrpVals();


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
                var _stockType = "B"; /* 指定A01 (A=12, B=A01, C=合併倉) */
                var _ModelNo = $("#MainContent_val_Prods").val();
                var _nDays = $("#MainContent_filter_Days").val();
                var _CustomFilter = $("#MainContent_menuFilter").dropdown("get value");

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
                         "url": '<%=fn_Param.WebUrl%>' + 'myPurPlan/Ashx_GetData_SH.ashx?v=1',
                         "type": "POST",
                         "data": {
                             stock: _stockType,
                             ModelNo: _ModelNo,
                             nDays: _nDays,
                             CustomFilter: _CustomFilter /* 指定範圍條件 */
                         }
                     },
                     "searching": false,  //搜尋
                     "ordering": false,   //排序
                     "paging": true,     //分頁
                     "info": true,      //頁數資訊
                     "lengthChange": false,  //是否顯示筆數選單
                     //自訂顯示欄位
                     "columns": [
                         { data: "ModelNo", className: "collapsing positive" },
                         { data: "Item_Type", className: "collapsing" },
                         { data: "ModelName", className: "collapsing" },
                         { data: "ProdVol", className: "collapsing center aligned" },
                         { data: "ProdPage", className: "collapsing center aligned" },
                         { data: "StockQty_A01", className: "collapsing center aligned" },  /* 庫存<StockQty_A01> */
                         {
                             /* 預計進<PreIN_A01> */
                             data: function (source, type, val) {
                                 var showID = source.RowIdx;
                                 var getVal = formatNumber(source.PreIN_A01);
                                 //response
                                 var html = getVal == 0 ? "0" : '<a href="#!" class="showPreIn" data-id="' + source.ModelNo + '" data-title="' + source.ModelNo + '" data-stock="A01">' + getVal + '</a>';

                                 return html;
                             }, className: "collapsing center aligned"
                         },
                         { data: "VirIn_A01", className: "collapsing center aligned" }, /* 虛擬入<VirIn_A01> */
                         { data: "PlanIN_A01", className: "collapsing center aligned" },    /* 計劃進<PlanIN_A01> */
                         { data: "WaitQty", className: "collapsing center aligned" },   /* 待驗收<WaitQty> */
                         { data: "NowMonthTurn_A01", className: "collapsing center aligned" },   /* 現有周轉月<NowMonthTurn_A01> */
                         {
                             /* 催貨量<PushQty> */
                             data: function (source, type, val) {
                                 var showID = source.RowIdx;
                                 var getVal = source.PushQty;
                                 var css = getVal < 0 ? "red-text text-darken-1" : "";
                                 //response
                                 var html = '<span class="' + css + '">' + getVal + '</span>';

                                 return html;
                             }, className: "collapsing center aligned"
                         },
                         {
                             /* 預計銷<PreSell_A01> */
                             data: function (source, type, val) {
                                 var showID = source.RowIdx;
                                 var getVal = formatNumber(source.PreSell_A01);
                                 //response
                                 var html = getVal == 0 ? "0" : '<a href="#!" class="showPreSell" data-id="' + source.ModelNo + '" data-title="' + source.ModelNo + '" data-stock="A01">' + getVal + '</a>';

                                 return html;
                             }, className: "collapsing center aligned"
                         },
                         { data: "VirPreSell", className: "collapsing center aligned" },    /* 虛擬預計銷<VirPreSell> */
                         { data: "RealPreSell", className: "collapsing center aligned" },   /* 實際預計銷<RealPreSell> */
                         {
                             /* 近N天用量<Qty_Days> */
                             data: function (source, type, val) {
                                 var showID = source.RowIdx;
                                 var getVal = formatNumber(source.Qty_Days);
                                 //response
                                 var html = getVal == 0 ? "0" : '<a href="#!" class="showInvHistory" data-id="' + source.ModelNo + '" data-title="' + source.ModelNo + '" data-stock="A01">' + getVal + '</a>';

                                 return html;
                             }, className: "collapsing center aligned"
                         },

                         { data: "Qty_Year", className: "collapsing center aligned" },  /* 全年平均月用量<Qty_Year> */
                         { data: "Qty_Season", className: "collapsing center aligned" },    /* 去年當季平均用量<Qty_Season> */
                         { data: "MonthTurn_A01", className: "collapsing center aligned" },   /* 可用週轉月<MonthTurn_A01> */
                         { data: "UsefulQty_A01", className: "collapsing center aligned" },   /* 可用量<UsefulQty_A01> */
                         { data: "SafeQty_A01", className: "collapsing center aligned" },   /* 安全存量<SafeQty_A01> */
                         { data: "InBox_Qty", className: "collapsing center aligned" }, /* 內盒數量<InBox_Qty> */
                         { data: "Qty_Packing", className: "collapsing center aligned" },   /* 一箱數量<Qty_Packing> */
                         { data: "OutBox_Cuft", className: "collapsing center aligned" },   /* 整箱材積<OutBox_Cuft> */
                         { data: "MOQ", className: "collapsing center aligned" },   /* 內銷MOQ<MOQ> */
                         {
                             /* 產銷訊息 */
                             data: function (source, type, val) {
                                 var showID = source.RowIdx;
                                 var getMsg = source.ProdMsg;

                                 //產銷訊息modal用的hidden欄位
                                 var msg = '<input type="hidden" id="msgDetail_' + showID + '" value="' + getMsg + '">';

                                 //組成html
                                 var html = '';
                                 if (getMsg == "") {
                                     html = '';
                                 } else {
                                     html = '<a href="#!" class="doShowMsg ui small grey basic icon button" data-id="' + showID + '" data-title="' + source.ModelNo + '"><i class="bullhorn icon"></i></a>'
                                     + msg;
                                 }

                                 return html;
                             }, className: "collapsing center aligned"
                         },
                         { data: "StockQty_12", className: "collapsing center aligned" },  /* 庫存<StockQty_12> */
                         { data: "Supplier", className: "collapsing left aligned" }   /* 供應商Supplier */
                     ],
                     //自訂欄位格式
                     "columnDefs": [
                        {
                            "render": function (data, type, row) {
                                //呼叫格式化function
                                return formatNumber(data);
                            },
                            "targets": 'numFmt' //指定class
                        }
                     ],
                     "pageLength": 20,   //顯示筆數預設值
                     "language": {
                         //自訂頁數資訊
                         "info": '共 <b>_TOTAL_</b> 筆 ,目前頁次 <b>_PAGE_</b> / _PAGES_, 每頁 20 筆.',
                         "processing": '<h2>資料載入中,請稍候...</h2>'
                     },

                     //捲軸設定
                     "scrollY": '60vh',
                     "scrollCollapse": true,
                     "scrollX": true,
                     "initComplete": function () {
                         //移除loading
                         s_data.removeClass("loading");
                     },
                     fixedColumns: {
                         /* 要凍結的窗格不可放要編輯的欄位 */
                        leftColumns: 2,
                        heightMatch: 'semiauto'
                     }
                 });

                /* table refresh後寫入觸發 S */
                 table.on('draw', function () {
                     //*** UI載入完成後觸發 *** 
                     //===== 產銷訊息 Modal =====
                     $(".doShowMsg").on("click", function () {
                         //取資料
                         var name = $(this).attr("data-title"); //model no
                         var id = $(this).attr("data-id"); //id
                         var msg = $("#msgDetail_" + id).val(); //get hidden field value
                         var $msgPage = $("#msgPage"); //get modal id

                         //填入值
                         $msgPage.find('.titleName').text(name); //set title description
                         $("#Msg").text(msg); //set message content

                         //顯示modal
                         $msgPage.modal('show');
                     });


                     //===== 預計進 Modal =====
                     $(".showPreIn").on("click", function () {
                         //取資料
                         var name = $(this).attr("data-title"); //title = ModelNo
                         var id = $(this).attr("data-id"); //id = ModelNo
                         var stock = $(this).attr("data-stock"); //庫別
                         var dbs = '<%=Req_CompID%>';
                         var $msgPage = $("#detail_PreIN"); //get modal id

                         //load html
                         var url = '<%=fn_Param.WebUrl%>' + 'myPurPlan/Ashx_GetPreIn.ashx?DBS=' + dbs + '&stock=' + stock + '&id=' + id;
                         var datablock = $msgPage.find(".content tbody");
                         datablock.empty();
                         datablock.load(url);

                         //填入值
                         $msgPage.find('.titleName').text(name); //set title description

                         //顯示modal
                         $msgPage.modal('show');
                     });


                     //===== 預計銷 Modal =====
                     $(".showPreSell").on("click", function () {
                         //取資料
                         var name = $(this).attr("data-title"); //title = ModelNo
                         var id = $(this).attr("data-id"); //id = ModelNo
                         var stock = $(this).attr("data-stock"); //庫別
                         var dbs = '<%=Req_CompID%>';
                         var $msgPage = $("#detail_PreSell"); //get modal id

                         //load html
                         var url = '<%=fn_Param.WebUrl%>' + 'myPurPlan/Ashx_GetPreSell.ashx?DBS=' + dbs + '&stock=' + stock + '&id=' + id;
                         var datablock = $msgPage.find(".content tbody");
                         datablock.empty();
                         datablock.load(url);

                         //填入值
                         $msgPage.find('.titleName').text(name); //set title description

                         //顯示modal
                         $msgPage.modal('show');
                     });


                     //===== 品號歷史異動 Modal =====
                     $(".showInvHistory").on("click", function () {
                         //取資料
                         var name = $(this).attr("data-title"); //title = ModelNo
                         var id = $(this).attr("data-id"); //id = ModelNo
                         var stock = $(this).attr("data-stock"); //庫別
                         var dbs = '<%=Req_CompID%>';
                         var _nDays = $("#MainContent_filter_Days").val();
                         var $msgPage = $("#detail_InvHistory"); //get modal id

                         //load html
                         var url = '<%=fn_Param.WebUrl%>' + 'myPurPlan/Ashx_GetInvHistory.ashx?DBS=' + dbs + '&nDays=' + _nDays + '&stock=' + stock + '&id=' + id;
                         var datablock = $msgPage.find(".content tbody");
                         datablock.empty();
                         datablock.load(url);

                         //填入值
                         $msgPage.find('.titleName').text(name); //set title description

                         //顯示modal
                         $msgPage.modal('show');
                     });

                 });
                /* table refresh後寫入觸發 E */

                //[function] 數字格式化(等於0設為灰字) / 千分位
                 function formatNumber(val) {
                     if (val == 0) {
                         val = '<span class="grey-text">' + val + '</span>';

                         return val;

                     } else {
                         //千分位
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

