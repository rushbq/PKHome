<%@ Page Title="標準成本" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myPurProdCost_Search" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<%@ Import Namespace="Resources" %>
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
                    <div class="section"><%:resPublic.nav_4000 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        標準成本 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
            <div class="right menu">
                <a href="#!" id="exportExcel" class="item"><i class="file excel icon"></i><span class="mobile hidden">匯出</span></a>
                <asp:Button ID="btn_Excel" runat="server" Text="excel trigger" OnClick="btn_Excel_Click" Style="display: none;" />
                <a href="<%:fn_Param.WebUrl %>myPurProdCost/Search_Prod.aspx" class="item" target="_blank" title="開新視窗"><i class="window restore outline icon"></i><span class="mobile hidden">包材維護</span></a>
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
                        <label>廠商</label>
                        <select id="menuSup" class="ac-drpSup ui fluid search selection dropdown">
                            <option value="">請選擇</option>
                        </select>
                        <asp:TextBox ID="val_Sups" runat="server" Style="display: none"></asp:TextBox>
                    </div>
                    <div class="four wide field">
                        <label>品號關鍵字</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" autocomplete="off" placeholder="查詢:品號" MaxLength="20"></asp:TextBox>
                    </div>
                    <div class="eight wide field">
                        <label>指定品號</label>
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
        <!-- Advance Search End -->

        <!-- List Content Start -->
        <asp:PlaceHolder ID="ph_Data" runat="server">
            <div id="section-data" class="ui green attached segment">
                <table id="myListTable" class="ui celled compact structured table" style="width: 100%">
                    <thead>
                        <tr>
                            <th class="grey-bg lighten-3">品號</th>
                            <th class="grey-bg lighten-3 center aligned">主供應商</th>
                            <th class="grey-bg lighten-3 center aligned">幣別</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">品號核價單價</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">包材核價單價*數量</th>
                            <th class="grey-bg lighten-3 right aligned numFmt" title="品號核價單價+(包材核價單價*數量)">計算標準成本</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">ERP標準成本</th>
                            <th class="grey-bg lighten-3">包材品號</th>
                            <th class="grey-bg lighten-3 right aligned numFmt">包材核價單價</th>
                            <th class="grey-bg lighten-3 center aligned numFmt">包材數量</th>
                            <th class="grey-bg lighten-3">品號備註</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:PlaceHolder ID="ph_Items" runat="server" />
                    </tbody>
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
            備註 - <span id="itemID" class="green-text text-darken-2"></span>
        </div>
        <div class="content">
            <div class="ui message">
                <p id="twMsg"></p>
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
            $(".myContentBody").keypress(function (e) {
                code = (e.keyCode ? e.keyCode : e.which);
                if (code == 13) {
                    $("#doSearch").trigger("click");
                    //避免觸發submit
                    e.preventDefault();
                }
            });

            //匯出excel
            $("#exportExcel").click(function () {
                //trigger button
                $("#MainContent_btn_Excel").trigger("click");
            });
        });
    </script>
    <%-- 供應商選單 --%>
    <script>
        /*
          search dropdown
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
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Supplier.ashx?q={query}&dbs=<%=Req_CompID%>&v=1.0'
            }

        });
        //取得複選選單欄位值
        function doGetSupDrpVals() {
            //取得值
            var procValue = $("#menuSup").dropdown("get value");
            var $fldCust = $("#MainContent_val_Sups");

            if (procValue.length == 0) {
                $fldCust.val("");
            } else {

                $fldCust.val(procValue);
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
                //將陣列轉成含分隔符號的字串
                var myVals = procValue.join(",");
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
                //取得已選擇的產品
                doGetDrpVals();

                //取得已選擇的供應商
                doGetSupDrpVals();

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
                var _keyword = $("#MainContent_filter_Keyword").val();
                var _ModelNo = $("#MainContent_val_Prods").val();
                var _SupID = $("#MainContent_val_Sups").val();


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
                         "url": '<%=fn_Param.WebUrl%>' + 'myPurProdCost/Ashx_GetData.ashx?v=1',
                         "type": "POST",
                         "data": {
                             Keyword: _keyword,
                             ModelNo: _ModelNo,
                             SupID: _SupID,
                             DBS: '<%=Req_CompID%>'
                         }
                     },
                     "searching": false,  //搜尋
                     "ordering": false,   //排序
                     "paging": true,     //分頁
                     "info": true,      //頁數資訊
                     "lengthChange": false,  //是否顯示筆數選單
                     //自訂顯示欄位
                     "columns": [
                         { data: "ModelNo", className: "green-text text-darken-2" },
                         { data: "SupName", className: "center aligned" },
                         { data: "Currency", className: "collapsing center aligned" },
                         { data: "ModelPrice", className: "right aligned" },
                         { data: "PackSumPrice", className: "right aligned" },
                         { data: "ProdCost", className: "right aligned" },
                         /* ERP 標準成本 */
                         //{ data: "ERPStdCost", className: "right aligned" },
                         {
                             data: function (source, type, val) {
                                 var cntStdCost = source.ProdCost;
                                 var erpStdCost = source.ERPStdCost;
                                 var _css = "";

                                 if(cntStdCost != erpStdCost){
                                     _css = "red-text text-darken-1"
                                 }

                                 //組成html
                                 var html = '<span class="'+ _css +'">'+ erpStdCost +'</span>';

                                 return html;
                             }, className: "right aligned"
                         },

                         { data: "PackItemNo", className: "blue-text text-darken-2" },
                         { data: "PackPrice", className: "right aligned" },
                         { data: "PackQty", className: "center aligned" },
                         {
                             /* 產品備註 */
                             data: function (source, type, val) {
                                 var showID = source.RowIdx;
                                 var getMsgTW = htmlEncode(source.ProdNote);

                                 //modal用的hidden欄位
                                 var msgTW = '<input type="hidden" id="TWmsgDetail_' + showID + '" value="' + getMsgTW + '">';

                                 //組成html
                                 var html = '';
                                 if (getMsgTW == "") {
                                     html = '-';
                                 } else {
                                     html = '<a href="#!" class="doShowMsg ui small grey basic icon button" data-id="' + showID + '" data-title="' + source.ModelNo + '"><i class="bullhorn icon"></i></a>'
                                     + msgTW;
                                 }

                                 return html;
                             }, className: "collapsing center aligned"
                         },

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
                     $(".doShowMsg").on("click", function () {
                         //取資料
                         var name = $(this).attr("data-title"); //model no
                         var id = $(this).attr("data-id"); //id
                         var msgTW = $("#TWmsgDetail_" + id).val(); //get hidden field value

                         //填入值
                         $("#itemID").text(name);
                         $("#twMsg").html(msgTW);

                         //顯示modal
                         $('#msgPage').modal('show');
                     });
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

                 function htmlEncode(value) {
                     //create a in-memory div, set it's inner text(which jQuery automatically encodes)
                     //then grab the encoded contents back out. The div never exists on the page.
                     return $('<div/>').text(value).html();
                 }

                 function htmlDecode(value) {
                     return $('<div/>').html(value).text();
                 }
                /* DataTables UI End */
             }
        });
    </script>
    <%-- DataTables End --%>
</asp:Content>

