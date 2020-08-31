<%@ Page Title="客戶歷史報價" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SearchByProd.aspx.cs" Inherits="myQuote_SearchByProd" %>

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
                    <h5 class="active section red-text text-darken-2">客戶歷史報價
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
                    <div class="five wide field">
                        <label>客戶</label>
                        <select id="menuCust" class="ac-drpCust ui fluid search selection dropdown" multiple="">
                            <option value="">請選擇</option>
                        </select>
                        <asp:TextBox ID="val_Custs" runat="server" Style="display: none"></asp:TextBox>
                    </div>
                    <div class="eight wide field">
                        <label>品號&nbsp;<small>輸入品號,選擇項目</small></label>
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
                            <th class="grey-bg lighten-3" rowspan="2">報價資料來源</th>
                            <th class="grey-bg lighten-3" rowspan="2">客戶代號</th>
                            <th class="grey-bg lighten-3" rowspan="2">客戶名</th>
                            <th class="grey-bg lighten-3" rowspan="2">品號</th>
                            <th class="grey-bg lighten-3" rowspan="2">類別</th>
                            <th class="grey-bg lighten-3" rowspan="2">品名</th>
                            <th class="grey-bg lighten-3" rowspan="2">外包裝<br />
                                含商品數</th>
                            <th class="grey-bg lighten-3" rowspan="2">內盒<br />
                                產品數量</th>
                            <th class="grey-bg lighten-3" rowspan="2">主要<br />
                                出貨地</th>
                            <th class="grey-bg lighten-3" rowspan="2">幣別</th>
                            <th class="grey-bg lighten-3 right aligned numFmt" rowspan="2">最近報價</th>
                            <th class="grey-bg lighten-3 right aligned" rowspan="2">報價日期</th>
                            <th class="green-bg lighten-3 center aligned" colspan="4">台灣</th>
                            <th class="blue-bg lighten-3 center aligned" colspan="4">上海</th>
                            <th class="grey-bg lighten-3" rowspan="2">產銷訊息</th>
                        </tr>
                        <tr>
                            <!-- TW -->
                            <th class="grey-bg lighten-3 right aligned numFmt">AGENT價</th>
                            <th class="grey-bg lighten-3 right aligned">利潤率</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">最近訂單價</th>
                            <th class="grey-bg lighten-3 right aligned">最近訂單日</th>
                            <!-- SH -->
                            <th class="grey-bg lighten-3 right aligned numFmt">AGENT價</th>
                            <th class="grey-bg lighten-3 right aligned">利潤率</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">最近訂單價</th>
                            <th class="grey-bg lighten-3 right aligned">最近訂單日</th>
                        </tr>
                    </thead>
                </table>
            </div>
        </asp:PlaceHolder>
        <!-- List Content End -->
    </div>
    <!-- 內容 End -->
    <!-- Msg Modal Start -->
    <!--// 觸發語法在DT initComplete裡 //-->
    <div id="msgPage" class="ui modal">
        <div class="header">
            產銷訊息 - <span id="itemID" class="green-text text-darken-2"></span>
        </div>
        <div class="content">
            <div class="ui message">
                <div class="header">
                    (台灣)
                </div>
                <p id="twMsg"></p>
            </div>

            <div class="ui message">
                <div class="header">
                    (上海)
                </div>
                <p id="shMsg"></p>
            </div>
        </div>
        <div class="actions">
            <div class="ui cancel button">
                關閉視窗
            </div>
        </div>
    </div>
    <!-- Msg Modal End -->
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
                alert('測試完畢後開發');
                return;

                //Get values of models
                doGetDrpVals();
                doGetCustDrpVals();

                //Check Null
                var _ModelNo = $("#MainContent_val_Prods").val();
                var _Cust = $("#MainContent_val_Custs").val();
                if (_ModelNo == "" && _Cust == "") {
                    alert('至少要輸入一個條件');
                    return;
                }

                //trigger button
                $("#MainContent_btn_Excel").trigger("click");

            });


        });
    </script>

    <%-- 客戶選單 --%>
    <script>
        /*
          search dropdown多選
          注意事項:
          需使用Html Controller, 不能使用 .Net元件
          , 因選項會變動, 會被視為安全性漏洞, 所以要用另一個ServerSide元件接收值
        */
        $('.ac-drpCust').dropdown({
            fields: {
                remoteValues: 'results',
                name: 'FullLabel',
                value: 'ID'
            },
            apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?q={query}&v=1.0'
            }

        });

        //取得複選選單欄位值
        function doGetCustDrpVals() {
            //取得多選的值
            var procValue = $("#menuCust").dropdown("get value");
            var $fldCust = $("#MainContent_val_Custs");

            if (procValue.length == 0) {
                $fldCust.val("");
            } else {
                //將陣列轉成以#分隔的字串
                var myVals = procValue.join("#");
                //填入隱藏欄位(傳遞時使用)
                $fldCust.val(myVals);
                //console.log(myVals);
            }
        }

    </script>

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
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Prod_v1.ashx?q={query}&v=1.0'
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
                doGetCustDrpVals();

                //Check Null
                var _ModelNo = $("#MainContent_val_Prods").val();
                var _Cust = $("#MainContent_val_Custs").val();
                if (_ModelNo == "" && _Cust == "") {
                    alert('至少要輸入一個條件');
                    return;
                }

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
                var _Cust = $("#MainContent_val_Custs").val();
                var _ModelNo = $("#MainContent_val_Prods").val();

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
                         "url": '<%=fn_Param.WebUrl%>' + 'myQuote/Ashx_GetData.ashx?v=1',
                         "type": "POST",
                         "data": {
                             Cust: _Cust,
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
                         { data: "DBS", className: "collapsing center aligned" },
                         { data: "CustID", className: "collapsing center aligned green-text text-darken-2" },
                         { data: "CustName", className: "collapsing" },
                         { data: "ModelNo", className: "collapsing center aligned" },
                         { data: "ClsaaName", className: "collapsing center aligned" },
                         { data: "ProdName_TW", className: "" },
                         { data: "OuterBox", className: "center aligned" },
                         { data: "InnerBox", className: "center aligned" },
                         { data: "Ship_From", className: "collapsing center aligned" },
                         { data: "Currency", className: "collapsing center aligned" },
                         { data: "UnitPrice", className: "right aligned error" },
                         { data: "QuoteDate", className: "center aligned" },

                         /* TW */
                         { data: "AgentPrice_TW", className: "right aligned warning" },
                         {
                             /* 利潤率 */
                             data: function (source, type, val) {
                                 var showVal = source.ProfitTW;

                                 //組成html
                                 var html = showVal == 0 ? '' : Math.round(showVal * 100) + ' %';

                                 return html;
                             }, className: "collapsing center aligned"
                         },
                         { data: "LastOrderPrice_TW", className: "right aligned" },
                         { data: "LastOrderDate_TW", className: "center aligned" },

                         /* SH */
                         { data: "AgentPrice_SH", className: "right aligned warning" },
                         {
                             /* 利潤率 */
                             data: function (source, type, val) {
                                 var showVal = source.ProfitSH;

                                 //組成html
                                 var html = showVal == 0 ? '' : showVal * 100 + ' %';

                                 return html;
                             }, className: "collapsing center aligned"
                         },
                         { data: "LastOrderPrice_SH", className: "right aligned" },
                         { data: "LastOrderDate_SH", className: "center aligned" },
                         {
                             /* 產銷訊息 */
                             data: function (source, type, val) {
                                 var showID = source.RowIdx;
                                 var getMsgTW = source.ProdMsg_TW;
                                 var getMsgSH = source.ProdMsg_SH;

                                 //產銷訊息modal用的hidden欄位
                                 var msgTW = '<input type="hidden" id="TWmsgDetail_' + showID + '" value="' + getMsgTW + '">';
                                 var msgSH = '<input type="hidden" id="SHmsgDetail_' + showID + '" value="' + getMsgSH + '">';

                                 //組成html
                                 var html = '';
                                 if (getMsgTW == "" && getMsgSH == "") {
                                     html = '-';
                                 } else {
                                     html = '<a href="#!" class="doShowMsg ui small grey basic icon button" data-id="' + showID + '" data-title="' + source.ModelNo + '"><i class="bullhorn icon"></i></a>'
                                     + msgTW + msgSH;
                                 }

                                 return html;
                             }, className: "collapsing center aligned"
                         },
                     ],
                     //自訂欄位格式
                     "columnDefs": [
                        {
                            "render": function (data, type, row) {
                                //呼叫格式化function
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
                     "initComplete": function () {
                         //移除loading
                         s_data.removeClass("loading");
                     }
                 });

                /* table 重新後寫入觸發 */
                 table.on('draw', function () {
                     //*** UI載入完成後觸發 *** Message Modal
                     $(".doShowMsg").on("click", function () {
                         //取資料
                         var name = $(this).attr("data-title"); //model no
                         var id = $(this).attr("data-id"); //id
                         var msgTW = $("#TWmsgDetail_" + id).val(); //get hidden field value
                         var msgSH = $("#SHmsgDetail_" + id).val(); //get hidden field value

                         //填入值
                         $("#itemID").text(name);
                         $("#twMsg").text(msgTW);
                         $("#shMsg").text(msgSH);

                         //顯示modal
                         $('#msgPage').modal('show');
                     });
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

