<%@ Page Title="訂單到貨狀況" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myOpcsStatus_Search" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
    <style>
        /* 自訂table
           - user要粗線#757575
           - 群組頭拎北不要粗線 rgba(34,36,38,.1) (原來CSS的)
           - user表頭要深色#26a69a
        */
        #myListTable {
            border-color: #757575;
        }

            #myListTable td {
                border-color: #757575;
            }

            #myListTable .groupData td {
                border-color: rgba(34,36,38,.1);
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
            <div class="section">訂單到貨狀況</div>
            <i class="right angle icon divider"></i>
            <div class="active section">
                <asp:Literal ID="lt_CorpName" runat="server" Text="公司別名稱"></asp:Literal>
            </div>
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
                    <div class="seven wide field">
                        <label>OPCS編號</label>
                        <input type="text" id="filter_OpcsNo" placeholder="輸入完整或部份編號" maxlength="20" autocomplete="off" />
                        <div class="ui fluid popup hidden">
                            <h4 class="ui red header">輸入方式如下</h4>
                            <ol class="ui list">
                                <li>完整編號不含「-」：2201L180651</li>
                                <li>完整編號含「-」：2201-L180651</li>
                                <li>只填單別：2201</li>
                                <li>只填單號：L180651</li>
                                <li class="orange-text text-darken-2">輸入的文字不分大小寫,皆可查詢</li>
                            </ol>
                        </div>
                    </div>
                    <div class="nine wide field">
                        <label>訂單預交日期</label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_sDate" placeholder="開始日" autocomplete="off" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_eDate" placeholder="結束日" autocomplete="off" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="fields">
                    <div class="seven wide field">
                        <label>客戶關鍵字</label>
                        <input type="text" id="filter_Cust" placeholder="輸入客戶代號或名稱關鍵字" maxlength="20" autocomplete="off" />
                    </div>
                    <div class="nine wide field">
                        <div class="two fields">
                            <div class="field">
                                <label>部門</label>
                                <select id="filter_Dept">
                                    <option value="">-- 全部 --</option>
                                    <option value="100">總經理室</option>
                                    <option value="120">外銷</option>
                                    <option value="130">內銷</option>
                                </select>
                            </div>
                            <div class="field">
                                <label class="orange-text text-darken-2"><b>快查選項</b></label>
                                <select id="filter_fastmenu" class="orange-text text-darken-4">
                                    <option value="">-- 全部 --</option>
                                    <option value="r1">欠料狀況</option>
                                    <option value="r2">庫存出貨</option>
                                    <option value="r3">產品無條碼</option>
                                    <option value="r4">產品無MIT</option>
                                </select>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="ui horizontal divider">
                    自訂篩選
                    <span class="grey-text text-darken-1">(下方選單避免與快查選項一同使用，有可能因條件衝突而查無資料)</span>
                </div>
                <div class="field">
                    <div class="six fields">
                        <div class="field">
                            <label>品項屬性</label>
                            <select id="filter_ProdProperty">
                                <option value="">-- 全部 --</option>
                                <option value="P">P</option>
                                <option value="M">M</option>
                                <option value="S">S</option>
                            </select>
                        </div>
                        <div class="field">
                            <label>完工狀態</label>
                            <select id="filter_MakeStatus">
                                <option value="">-- 全部 --</option>
                                <option value="N">未完工</option>
                                <option value="Y">已完成</option>
                                <option value="V">未開工單</option>
                            </select>
                        </div>
                        <div class="field">
                            <!-- 訂單結案碼 -->
                            <label>出貨狀態</label>
                            <select id="filter_ShipStatus">
                                <option value="">-- 全部 --</option>
                                <option value="N">未出貨</option>
                                <option value="Y">已出貨</option>
                            </select>
                        </div>
                        <div class="field">
                            <label>採購進貨狀態</label>
                            <select id="filter_GetInStatus">
                                <option value="">-- 全部 --</option>
                                <option value="A">已採購未進貨</option>
                                <option value="B">已進貨</option>
                            </select>
                        </div>
                        <div class="field">
                            <label>庫存狀態</label>
                            <select id="filter_StockStatus">
                                <option value="">-- 全部 --</option>
                                <option value="A">不足量 &lt; 0</option>
                            </select>
                        </div>
                        <div class="field">
                            <label>採購下單狀態</label>
                            <select id="filter_PurStatus">
                                <option value="">-- 全部 --</option>
                                <option value="A">P+S件</option>
                                <option value="B">P+S件,不足量+預計進 &lt; 0</option>
                            </select>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="section-filter ui bottom attached segment">
            <div class="ui two column grid">
                <div class="column">
                    <button type="button" id="doExport" class="ui green button"><i class="file excel outline icon"></i>Excel</button>
                    <a href="<%=FuncPath() %>" class="ui button"><i class="refresh icon"></i>清除條件</a>
                </div>
                <div class="column right aligned">
                    <button type="button" id="doSearch" class="ui blue button"><i class="search icon"></i>查詢</button>
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
            <div id="section-tip">
                請先篩選要查詢的條件，並按下<i class="material-icons">search</i>查詢。
                <!-- 搭配Sematic UI較美觀-->
            </div>
            <table id="myListTable" class="ui celled selectable compact small table">
                <thead>
                    <tr class="center aligned colorHead">
                        <asp:Literal ID="lt_TableHeader" runat="server"></asp:Literal>
                    </tr>
                </thead>
                <tbody class="remoteData">
                </tbody>
            </table>
        </div>
        <!-- data section End -->

        <!-- Opcs Modal Start -->
        <div id="opcsDT" class="ui fullscreen modal">
            <i class="close icon"></i>
            <div class="header">延遲出貨通知</div>
            <div class="scrolling content">
                <table class="ui striped compact table">
                    <thead>
                        <tr>
                            <th>品號</th>
                            <th>預交日</th>
                            <th>延遲原因</th>
                            <th>出貨指示</th>
                        </tr>
                    </thead>
                    <tbody></tbody>
                </table>
            </div>
            <div class="actions">
                <div class="ui close button">
                    Close
                </div>
            </div>
        </div>
        <!-- Opcs Modal End -->
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
            var s_msg = $("#section-message");
            var s_tip = $("#section-tip");
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
                var _OpcsNo = $("#filter_OpcsNo").val();
                var _Cust = $("#filter_Cust").val();
                var _sDate = $("#filter_sDate").val();
                var _eDate = $("#filter_eDate").val();
                var _ProdProperty = $("#filter_ProdProperty").val();
                var _MakeStatus = $("#filter_MakeStatus").val();
                var _ShipStatus = $("#filter_ShipStatus").val();
                var _GetInStatus = $("#filter_GetInStatus").val();
                var _StockStatus = $("#filter_StockStatus").val();
                var _PurStatus = $("#filter_PurStatus").val();
                var _Dept = $("#filter_Dept").val();
                var _fastmenu = $("#filter_fastmenu").val();

                //檢查資料 - 預交日(起訖日)
                if (_sDate != "" && _eDate != "") {
                    var d1 = new Date(_sDate);
                    var d2 = new Date(_eDate);

                    if (d2 < d1) {
                        alert("預交日:結束日期不可小於開始日期");
                        return false;
                    }
                }

                //畫面處理 - 顯示或隱藏
                btnToggle.hide();  //收合鈕
                s_filter.hide();    //條件區
                s_loading.show();   //載入區
                s_msg.hide();   //message
                s_data.hide();  //資料區            

                //要填入HTML的位置
                var tableBody = $("#myListTable tbody.remoteData");

                //*** 執行AJAX載入資料 Start ***                
                var request = $.ajax({
                    url: '<%=fn_Param.WebUrl%>' + "myOpcsStatus/Ashx_GetData.ashx",
                    method: "POST",
                    cache: false,
                    data: {
                        CompID: "<%=Req_CompID%>",
                        OpcsNo: _OpcsNo,
                        Cust: _Cust,
                        sDate: _sDate,
                        eDate: _eDate,
                        ProdProperty: _ProdProperty,
                        MakeStatus: _MakeStatus,
                        ShipStatus: _ShipStatus,
                        GetInStatus: _GetInStatus,
                        StockStatus: _StockStatus,
                        PurStatus: _PurStatus,
                        Dept: _Dept,
                        Fastmenu: _fastmenu,
                        menuID: "<%=Req_DeptID%>"
                    },
                    dataType: "html"

                });

                request.done(function (callback) {
                    if (callback.indexOf("#fail#") != -1) {
                        s_msg.show();
                        s_msg.html(callback.replace("#fail#", ""));
                        return false;
                    }

                    //顯示資料區
                    s_data.show();

                    //重置Datatable
                    table.destroy();

                    /* 載入遠端HTML - [呼叫DataTable] */
                    //取得元素 - table.tbody
                    tableBody.empty();  //清空tbody
                    tableBody.append(callback); //載入html

                    //設定datatable
                    table = $('#myListTable').DataTable({
                        searching: false,  //搜尋
                        ordering: false,   //排序
                        paging: false,     //分頁
                        info: false,      //筆數資訊
                        autoWidth: false,
                        language: {
                            //自訂筆數顯示選單
                            "lengthMenu": ''
                        },
                        columnDefs: [
                         { width: "40px", targets: 0 },
                         { width: "100px", targets: 1 }
                        ],
                        //捲軸設定
                        "scrollY": '60vh',
                        "scrollCollapse": true,
                        "scrollX": true
                    });


                    //Ajax完成後觸發, (Opcs表頭按鈕-展開收合)
                    $(".detailShown").on("click", function () {
                        var target = $(this).attr("data-target");

                        $("." + target).toggle()
                    });


                    //Ajax完成後觸發Modal, (按鈕-未出貨)顯示OPCS延遲出貨(BPM資料)
                    $(".detailOpcsFlow").on("click", function () {
                        var id = $(this).attr("data-id");

                        //load html
                        var url = '<%=fn_Param.WebUrl%>' + "myOpcsStatus/Ashx_GetOpcsFlowData.ashx?CompID=<%=Req_CompID%>&OpcsNo=" + id;
                        var datablock = $("#opcsDT .content tbody");
                        datablock.empty();
                        datablock.load(url);

                        //show modal
                        $('#opcsDT')
                            .modal({
                                selector: {
                                    close: '.close, .actions .button'
                                }
                            })
                            .modal('show');
                    });


                    //Ajax完成後觸發Modal, (按鈕-客戶注意事項)顯示OPCS Remark
                    $(".detailOpcsRemk").on("click", function () {
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


                    //整批理貨
                    $(".setAllShip").on("click", function () {
                        var _target = $(this).attr("data-target");
                        var _txt = $(this).text();

                        if (!confirm("確定要整批理貨 " + _txt + " ?\n「於執行完畢前,請不要做其它動作」!")) {
                            return false;
                        }

                        //取得目標
                        var _ele = $("." + _target);
                        var idx = 0;

                        //each巡覽
                        $(_ele).each(function () {

                            //trigger click
                            var _thisBtn = $(this);

                            //每隔 1 秒執行
                            (function (x) {
                                window.setTimeout(function () {
                                    _thisBtn.trigger("click");
                                }, 1000 * x);

                            })(idx);

                            idx++;
                        });

                    });


                    //Ajax完成後觸發, (按鈕-資材理貨)
                    $(".doShipment .btnShip").on("click", function () {
                        var id = $(this).attr("data-id");       //單別+單號+序號組成的編號
                        var act = $(this).attr("data-action");  //要執行的動作(A / B / Update)
                        var section = $(this).parent(".doShipment"); //按鈕區塊(尋找上一層)
                        var loadingCss = "ui loading form"; //loading 的css

                        //處理中-將按鈕區加入Loading
                        $(section).addClass(loadingCss);

                        //call ajax return
                        setShipment(id, act, function (output) {
                            //已處理-將按鈕區移除Loading
                            $(section).removeClass(loadingCss);

                            //判斷成功與否
                            if (output == "success") {
                                if (act == "Cancel") {
                                    $(section).empty(); //清空按鈕
                                    $(section).append("<small>重新查詢後可選擇</small>"); //show ok
                                } else {
                                    $(section).find(".result").remove(); //清除上一次選擇的文字
                                    $(section).append('<b class="yellow-bg result">' + act + '區</b>'); //show ok
                                }

                            } else {
                                alert('資料處理失敗!\n' + output);
                            }
                        });

                    });


                    //Ajax完成後觸發, (按鈕-包裝資料)
                    $(".doBoxing .btnBox").on("click", function () {
                        var id = $(this).attr("data-target");       //單別+單號+序號組成的編號
                        var inputVal = $("#" + id).val();   //取得填入的資料
                        var section = $(this).parentsUntil(".doBoxing"); //按鈕區塊
                        var loadingCss = "loading form"; //loading 的css

                        //處理中-將按鈕區加入Loading
                        $(section).addClass(loadingCss);

                        //call ajax return
                        setBoxing(id, inputVal, function (output) {
                            //已處理-將按鈕區移除Loading
                            $(section).removeClass(loadingCss);

                            //判斷成功與否
                            if (output == "success") {
                                
                                $(section).find(".saveIcon").removeClass("save").addClass("check");                               

                            } else {
                                alert('資料處理失敗!\n' + output);
                            }
                        });


                    });

                });

                request.fail(function (jqXHR, textStatus) {
                    //終止預設行為
                    event.preventDefault();
                    s_msg.show();
                    s_msg.html("<h5>發生未預期的錯誤，請聯絡系統管理人員</h5>");

                });

                request.always(function () {
                    //ajax執行完成後一定會跑的工作
                    btnToggle.show();  //收合鈕
                    s_loading.slideToggle("fast");   //載入區
                    s_tip.hide();

                });
                //*** 執行AJAX載入資料 End ***
            }

        });


        //資材理貨
        function setShipment(id, act, callback) {
            //取得ashx Response的HTML
            var url = '<%=fn_Param.WebUrl%>' + "myOpcsStatus/Ashx_UpdateData.ashx";

            var request = $.ajax({
                url: url,
                method: "POST",
                data: {
                    DataID: id,
                    Act: act,
                    Type: "Ship"
                },
                dataType: "html"
            });

            request.done(function (data) {
                return callback(data);

            });

            request.fail(function (jqXHR, textStatus) {
                return "";
            });
        }

        //資材包裝
        function setBoxing(id, act, callback) {
            //取得ashx Response的HTML
            var url = '<%=fn_Param.WebUrl%>' + "myOpcsStatus/Ashx_UpdateData.ashx";

            var request = $.ajax({
                url: url,
                method: "POST",
                data: {
                    DataID: id,
                    Act: act,
                    Type: "Box"
                },
                dataType: "html"
            });

            request.done(function (data) {
                return callback(data);

            });

            request.fail(function (jqXHR, textStatus) {
                return "";
            });
        }

    </script>
    <%-- DataTables End --%>
    <script>
        $(function () {
            //匯出Excel
            $("#doExport").on("click", function () {
                /* 取得資料 - 各欄位 */
                var _OpcsNo = $("#filter_OpcsNo").val();
                var _Cust = $("#filter_Cust").val();
                var _sDate = $("#filter_sDate").val();
                var _eDate = $("#filter_eDate").val();
                var _ProdProperty = $("#filter_ProdProperty").val();
                var _MakeStatus = $("#filter_MakeStatus").val();
                var _ShipStatus = $("#filter_ShipStatus").val();
                var _GetInStatus = $("#filter_GetInStatus").val();
                var _StockStatus = $("#filter_StockStatus").val();
                var _PurStatus = $("#filter_PurStatus").val();
                var _Dept = $("#filter_Dept").val();
                var _fastmenu = $("#filter_fastmenu").val();

                //檢查資料 - 預交日(起訖日)
                if (_sDate != "" && _eDate != "") {
                    var d1 = new Date(_sDate);
                    var d2 = new Date(_eDate);

                    if (d2 < d1) {
                        alert("預交日:結束日期不可小於開始日期");
                        return false;
                    }
                }

                //導向下載頁,帶入參數
                var url = "<%=fn_Param.WebUrl%>myOpcsStatus/Ashx_OutputExcel.ashx?CompID=<%=Req_CompID%>&OpcsNo=" + _OpcsNo + "&Cust=" + encodeURIComponent(_Cust) + "&sDate=" + encodeURIComponent(_sDate) +
                    "&eDate=" + encodeURIComponent(_eDate) + "&ProdProperty=" + _ProdProperty +
                    "&MakeStatus=" + _MakeStatus + "&ShipStatus=" + _ShipStatus + "&GetInStatus=" + _GetInStatus +
                    "&StockStatus=" + _StockStatus + "&PurStatus=" + _PurStatus + "&Dept=" + _Dept + "&Fastmenu=" + _fastmenu +
                    "&menuID=" + "<%=Req_DeptID%>";

                window.open(url);
            });
        });
    </script>
</asp:Content>

