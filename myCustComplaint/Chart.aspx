﻿<%@ Page Title="客訴統計" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Chart.aspx.cs" Inherits="myCustComplaint_Chart" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<%@ Import Namespace="Resources" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section"><%:resPublic.nav_2000 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="section"><%:resPublic.fun_客訴管理 %></div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">
                        <asp:Literal ID="lt_TypeName" runat="server" Text="功能名稱"></asp:Literal>
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="item" href="<%=Page_SearchUrl %>"><i class="undo icon"></i>返回</a>
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
                    <div class="ten wide field">
                        <label>開案日期</label>
                        <div class="fields">
                            <div class="four wide field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_sDate" autocomplete="off" placeholder="開始日" maxlength="10" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="four wide field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_eDate" autocomplete="off" placeholder="結束日" maxlength="10" />
                                    <i class="calendar alternate icon"></i>
                                </div>
                            </div>
                            <div class="eight wide field">
                                <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Search End -->

        <div class="ui attached segment grey-bg lighten-5">
            <!-- 客戶類別 -->
            <div class="ui segments">
                <div class="ui green segment">
                    <h5 class="ui header">客戶類別&nbsp;<small class="grey-text text-darken-1">流程狀態=結案</small></h5>
                </div>
                <div class="ui attached segment">
                    <div class="ui grid">
                        <div class="row">
                            <div class="eleven wide column">
                                <div id="PieChartbyClass">
                                    <div class="ui active centered inline loader"></div>
                                </div>
                            </div>
                            <div class="five wide column">
                                <div id="table_PieChartbyClass" class="ui celled striped small table">
                                    <div class="ui active centered inline loader"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <!-- 客服資料不良原因 -->
            <div class="ui segments">
                <div class="ui green segment">
                    <h5 class="ui header">客服資料不良原因&nbsp;<small class="grey-text text-darken-1">狀態=已開案</small></h5>
                </div>
                <div class="ui attached segment">
                    <div class="ui grid">
                        <div class="row">
                            <div class="eleven wide column">
                                <div id="PieChartbyF101">
                                    <div class="ui active centered inline loader"></div>
                                </div>
                            </div>
                            <div class="five wide column">
                                <div id="table_PieChartbyF101" class="ui celled striped small table">
                                    <div class="ui active centered inline loader"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <!-- 二線維修不良原因 -->
            <div class="ui segments">
                <div class="ui green segment">
                    <h5 class="ui header">二線維修不良原因&nbsp;<small class="grey-text text-darken-1">流程狀態=結案</small></h5>
                </div>
                <div class="ui attached segment">
                    <div class="ui grid">
                        <div class="row">
                            <div class="eleven wide column">
                                <div id="PieChartbyF301">
                                    <div class="ui active centered inline loader"></div>
                                </div>
                            </div>
                            <div class="five wide column">
                                <div id="table_PieChartbyF301" class="ui celled striped small table">
                                    <div class="ui active centered inline loader"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //tab menu
            //$('.menu .item').tab();

        });
    </script>
    <%-- 日期選擇器 Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.js"></script>
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/options.js"></script>
    <script>
        $(function () {
            //取得設定值(往前天數, 往後天數)
            var calOpt = getCalOptBydate(730, 7);
            //載入datepicker
            $('.datepicker').calendar(calOpt);
        });
    </script>
    <%-- 日期選擇器 End --%>
    <%-- Google Chart Start --%>
    <script type="text/javascript" src="https://www.gstatic.com/charts/loader.js"></script>
    <script type="text/javascript">
        //load google charts
        google.charts.load("current", { packages: ['corechart', 'table', 'bar'], 'callback': drawCharts });

        //預設載入的圖表, 此function必須與callback宣告放在同一個script
        function drawCharts() {
            //取得欄位值
            var getsDate = $("#filter_sDate").val();
            var geteDate = $("#filter_eDate").val();
            var getWho = $("#MainContent_filter_ProcWho").val();

            //設定日期
            var setsDate = getsDate;
            var seteDate = geteDate;

            //若為空值,帶入預設值
            if (getsDate == "" && geteDate == "") {
                //填入預設日期(當年開始~今日)
                var thisDate = new Date();
                var setsDate = thisDate.getFullYear() + '/01/01';
                var seteDate = thisDate.getFullYear() + '/' + ('0' + (thisDate.getMonth() + 1)).slice(-2) + '/' + ('0' + thisDate.getDate()).slice(-2);

                $("#filter_sDate").val(setsDate);
                $("#filter_eDate").val(seteDate);
            }

            //載入圖表 - 客戶類別
            SearchbyClass(setsDate, seteDate);
            //載入圖表 - F101
            SearchbyF101(setsDate, seteDate);
            //載入圖表 - F301
            SearchbyF301(setsDate, seteDate);
        }

        $("#doSearch").click(function () {
            drawCharts();
        });

    </script>

    <script>
        //-- 載入圖表 - 客戶類別(Pie Chart) --
        function SearchbyClass(StartDate, EndDate) {
            var request = $.ajax({
                url: '<%=fn_Param.WebUrl%>' + "Ajax_Data/GetData_ChartData_CCP.ashx",
                method: "POST",
                data: {
                    jobtype: 'A',
                    type: '<%=Req_TypeID%>',
                    sDate: StartDate,
                    eDate: EndDate
                },
                //contentType: 'application/json',    //傳送格式
                dataType: "html"    //遠端回傳格式
            });

            request.done(function (resp) {
                var myTitle = StartDate + ' ~' + EndDate;
                //繪製圖表(drawChart_Pie)
                drawChart_Pie(resp, myTitle, 900, 700, 'PieChartbyClass', '類別', '件數');
            });

            request.fail(function (jqXHR, textStatus) {
                event.preventDefault();
                alert('Pie Chart載入失敗,請聯絡MIS.');

            });

            request.always(function () {
                //do something
            });
        }

        //-- 載入圖表 - F101(Pie Chart) --
        function SearchbyF101(StartDate, EndDate) {
            var request = $.ajax({
                url: '<%=fn_Param.WebUrl%>' + "Ajax_Data/GetData_ChartData_CCP.ashx",
                method: "POST",
                data: {
                    jobtype: 'B',
                    type: '<%=Req_TypeID%>',
                    sDate: StartDate,
                    eDate: EndDate
                },
                //contentType: 'application/json',    //傳送格式
                dataType: "html"    //遠端回傳格式
            });

            request.done(function (resp) {
                var myTitle = StartDate + ' ~' + EndDate;
                //繪製圖表(drawChart_Pie)
                drawChart_Pie(resp, myTitle, 900, 700, 'PieChartbyF101', '類別', '件數');
            });

            request.fail(function (jqXHR, textStatus) {
                event.preventDefault();
                alert('Pie Chart載入失敗,請聯絡MIS.');

            });

            request.always(function () {
                //do something
            });
        }

        //-- 載入圖表 - F301(Pie Chart) --
        function SearchbyF301(StartDate, EndDate) {
            var request = $.ajax({
                url: '<%=fn_Param.WebUrl%>' + "Ajax_Data/GetData_ChartData_CCP.ashx",
                method: "POST",
                data: {
                    jobtype: 'C',
                    type: '<%=Req_TypeID%>',
                    sDate: StartDate,
                    eDate: EndDate
                },
                //contentType: 'application/json',    //傳送格式
                dataType: "html"    //遠端回傳格式
            });

            request.done(function (resp) {
                var myTitle = StartDate + ' ~' + EndDate;
                //繪製圖表(drawChart_Pie)
                drawChart_Pie(resp, myTitle, 900, 700, 'PieChartbyF301', '類別', '件數');
            });

            request.fail(function (jqXHR, textStatus) {
                event.preventDefault();
                alert('Pie Chart載入失敗,請聯絡MIS.');

            });

            request.always(function () {
                //do something
            });
        }


        //-- 繪製圖表 Start --
        /*
          [Pie Chart]
          dataValues : 資料值
          titleName : 圖表title
          width : 圖表寬度
          height : 圖表高度
          chartID : 圖表ID
          colName : 欄位名 - 類別
          valName : 欄位名 - 值

          ref:https://developers.google.com/chart/interactive/docs/gallery/piechart
        */
        function drawChart_Pie(_dataValues, _titleName, _width, _height, _chartID, _colName, _valName) {
            var data = new google.visualization.DataTable();
            var data_table = new google.visualization.DataTable();
            var totalCnt = 0;

            //新增欄-Chart
            data.addColumn('string', _colName);
            data.addColumn('number', _valName);

            //新增欄-Table
            data_table.addColumn('string', _colName);
            data_table.addColumn('number', _valName);

            //Json to Array
            var ary = $.parseJSON(_dataValues);
            //新增列值
            $.each(ary, function () {
                $.each(this, function (idx, val) {
                    var dtCol = val.Label;
                    var dtVal_cnt = val.Cnt;

                    //AddRow-Chart
                    data.addRow([dtCol, dtVal_cnt]);

                    //AddRow-Table
                    data_table.addRow([dtCol, dtVal_cnt]);

                    //計算總計
                    totalCnt += dtVal_cnt;

                });
            });

            //AddRow-Table(SUM)
            data_table.addRow(["Total", totalCnt]);

            //設定圖表選項
            var options = {
                title: _titleName,
                width: _width,
                height: _height,
                is3D: true,
                legend: 'right', /*labeled:自圖上拉線出來描述*/
                chartArea: { left: 20, top: 20, width: '80%', height: '75%' } /*配置圖表區域的位置和大小*/
            };

            //載入圖表
            var chart = new google.visualization.PieChart(document.getElementById(_chartID));
            chart.draw(data, options);

            //載入表格
            var table = new google.visualization.Table(document.getElementById('table_' + _chartID));
            var options = {
                showRowNumber: false,
                width: '100%',
                height: '100%',
                sort: 'disable'
            };
            table.draw(data_table, options);

        }


        //-- 繪製圖表 End --
    </script>
    <%-- Google Chart End --%>
</asp:Content>

