<%@ Page Title="客戶返利統計-China" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myRebate_Search" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
    <style>
        table tfoot tr th {
            font-weight: bold !important;
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
                    <div class="active section">
                        客戶返利統計-China
                    </div>
                </div>
            </div>
            <div class="right menu">
                <a href="<%=FuncPath() %>/CustRel" class="item"><i class="users icon"></i>多客戶設定</a>
                <asp:LinkButton ID="lbtn_Excel" runat="server" CssClass="item" OnClick="lbtn_Excel_Click"><i class="file excel icon"></i><span class="mobile hidden">匯出</span></asp:LinkButton>
                <a class="item" href="<%=FuncPath() %>/Edit">
                    <i class="plus icon"></i>
                    <span class="mobile hidden">新增客戶目標</span>
                </a>
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
                    <div class="three wide field">
                        <label>年度</label>
                        <asp:DropDownList ID="filter_Year" runat="server">
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label>月份</label>
                        <asp:DropDownList ID="filter_Month" runat="server">
                        </asp:DropDownList>
                    </div>
                    <div class="five wide field">
                        <label>客戶</label>
                        <div class="ui fluid search ac-Cust">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Cust" runat="server" CssClass="prompt" placeholder="輸入客戶代號或名稱關鍵字"></asp:TextBox>
                                <asp:Panel ID="lb_Cust" runat="server" CssClass="ui label"><i class="angle left"></i>輸入關鍵字,選擇項目</asp:Panel>
                            </div>
                        </div>
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
                <div class="ui two column stackable center aligned grid">
                    <div class="ui vertical divider">Or</div>
                    <div class="middle aligned row">
                        <div class="column">
                            <div class="ui icon header">
                                <i class="search icon"></i>
                                目前條件查無資料，請重新查詢。
                            </div>
                        </div>
                        <div class="column">
                            <div class="ui icon header">
                                <i class="plus icon"></i>
                                新增客戶目標
                            </div>
                            <a href="<%=FuncPath() %>/Edit" class="ui basic green button">開始建立</a>
                        </div>
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>
        <!-- Empty Content End -->

        <!-- List Content Start -->
        <asp:PlaceHolder ID="ph_Data" runat="server">
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table id="listTable" class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3 center aligned">部門</th>
                                    <th class="grey-bg lighten-3 center aligned">客戶</th>
                                    <th class="grey-bg lighten-3 center aligned" data-content="e">責任目標 <span class="blue-text text-darken-1">(e)</span></th>
                                    <th class="grey-bg lighten-3 center aligned" data-content="f">回饋方式 <span class="blue-text text-darken-1">(f)</span></th>
                                    <th class="grey-bg lighten-3 center aligned" data-content="g">挑戰目標 <span class="blue-text text-darken-1">(g)</span></th>
                                    <th class="grey-bg lighten-3 center aligned" data-content="h">回饋方式 <span class="blue-text text-darken-1">(h)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="A">目前系統業績 <span class="blue-text text-darken-1">(A)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="F">單別2341 <span class="blue-text text-darken-1">(F)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="Fa">B009 <span class="blue-text text-darken-1">(Fa)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="a">
                                        <asp:Literal ID="lt_headerYear" runat="server"></asp:Literal>實際返利業績(含已返利) <span class="blue-text text-darken-1">(a)</span></th>
                                    <th class="grey-bg lighten-3">備註</th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="D">與挑戰目標<br />
                                        差額 <span class="blue-text text-darken-1">(D)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="E">與責任目標<br />
                                        差額 <span class="blue-text text-darken-1">(E)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="c">應回饋<br />
                                        金額 <span class="blue-text text-darken-1">(c)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="B">已回饋<br />
                                        金額 <span class="blue-text text-darken-1">(B)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="d">剩餘回饋<br />
                                        金額 <span class="blue-text text-darken-1">(d)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="C">
                                        <asp:Literal ID="lt_headerMonth" runat="server"></asp:Literal>月銷售<br />
                                        金額 <span class="blue-text text-darken-1">(C)</span></th>
                                    <th class="grey-bg lighten-3 right aligned" data-content="b">當月最高<br />
                                        返利金額 <span class="blue-text text-darken-1">(b)</span></th>
                                    <th class="grey-bg lighten-3">返利前<br />
                                        毛利率</th>
                                    <th class="grey-bg lighten-3">返利後<br />
                                        毛利率</th>
                                    <th class="grey-bg lighten-3">全返後<br />
                                        毛利率</th>
                                    <th class="grey-bg lighten-3"></th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                            <%--<tfoot>
                                <asp:Literal ID="lt_tableFooter" runat="server"></asp:Literal>
                            </tfoot>--%>
                            <tfoot>
                                <tr>
                                    <th colspan="2" class="red-bg lighten-5 center aligned">總計</th>
                                    <th class="red-bg lighten-5 center aligned" title="e" aria-label="責任目標"></th>
                                    <th class="red-bg lighten-5 center aligned"></th>
                                    <th class="red-bg lighten-5 center aligned" title="g" aria-label="挑戰目標"></th>
                                    <th class="red-bg lighten-5 center aligned"></th>
                                    <th class="red-bg lighten-5 right aligned" title="A" aria-label="目前系統業績"></th>
                                    <th class="red-bg lighten-5 right aligned" title="F" aria-label="單別2341"></th>
                                    <th class="red-bg lighten-5 right aligned" title="Fa" aria-label="B009"></th>
                                    <th class="red-bg lighten-5 right aligned" title="a" aria-label="實際返利業績"></th>
                                    <th class="red-bg lighten-5 center aligned"></th>
                                    <th class="red-bg lighten-5 right aligned" title="D" aria-label="與挑戰目標差額"></th>
                                    <th class="red-bg lighten-5 right aligned" title="E" aria-label="與責任目標差額"></th>
                                    <th class="red-bg lighten-5 right aligned" title="c" aria-label="應回饋金額"></th>
                                    <th class="red-bg lighten-5 right aligned" title="B" aria-label="已回饋金額"></th>
                                    <th class="red-bg lighten-5 right aligned" title="d" aria-label="剩餘回饋金額"></th>
                                    <th class="red-bg lighten-5 right aligned" title="C" aria-label="月銷售金額"></th>
                                    <th class="red-bg lighten-5 right aligned" title="b" aria-label="當月最高返利"></th>
                                    <th class="red-bg lighten-5"></th>
                                    <th class="red-bg lighten-5"></th>
                                    <th class="red-bg lighten-5"></th>
                                    <th class="red-bg lighten-5 center aligned"></th>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                </LayoutTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="center aligned"><%#Eval("DeptName") %>
                        </td>
                        <td class="center aligned" title="<%#Eval("CustID") %>">
                            <strong class="green-text text-darken-3"><%#Eval("CustName") %></strong>
                        </td>
                        <td class="center aligned" data-content="e">
                            <%#Eval("Cnt_e").ToString().ToMoneyString() %> 
                        </td>
                        <td class="center aligned" data-content="f">
                            <%#(Convert.ToDouble(Eval("Cnt_f"))*100) %>%
                        </td>
                        <td class="center aligned" data-content="g">
                            <%#Eval("Cnt_g").ToString().ToMoneyString() %>
                        </td>
                        <td class="center aligned" data-content="h">
                            <%#(Convert.ToDouble(Eval("Cnt_h"))*100) %>%
                        </td>
                        <td class="right aligned" data-content="A">
                            <%#Eval("CntBase_A").ToString().ToMoneyString() %>
                        </td>
                        <td class="right aligned" data-content="F">
                            <%#Eval("CntBase_F").ToString().ToMoneyString() %>
                        </td>
                        <td class="right aligned" data-content="Fa">
                            <%#Eval("CntBase_Fa").ToString().ToMoneyString() %>
                        </td>
                        <td class="right aligned" data-content="a" title="a = A + B - F">
                            <%#Eval("Cnt_a").ToString().ToMoneyString() %>
                        </td>
                        <td>
                            <asp:Literal ID="lt_Remark" runat="server"></asp:Literal></td>
                        <td class="right aligned <%#showNumber(Eval("CntBase_D")) %>" data-content="D" title="D = A + B - g">
                            <%#Eval("CntBase_D").ToString().ToMoneyString() %>
                        </td>
                        <td class="right aligned <%#showNumber(Eval("CntBase_E")) %>" data-content="E" title="E = A + B - e">
                            <%#Eval("CntBase_E").ToString().ToMoneyString() %>
                        </td>
                        <td class="right aligned" data-content="c">
                            <%#Eval("Cnt_c").ToString().ToMoneyString() %>
                        </td>
                        <td class="right aligned" data-content="B" title="W003">
                            <%#Eval("CntBase_B").ToString().ToMoneyString() %>
                        </td>
                        <td class="right aligned <%#showNumber(Eval("Cnt_d")) %>" data-content="d" title="d = c - B">
                            <%#Eval("Cnt_d").ToString().ToMoneyString() %>
                        </td>
                        <td class="right aligned" data-content="C">
                            <%#Eval("CntBase_C").ToString().ToMoneyString() %>
                        </td>
                        <td class="right aligned" data-content="b" title="b = (C+當月W003) * 50%">
                            <%#Eval("Cnt_b").ToString().ToMoneyString() %>
                        </td>
                        <td class="center aligned" title="(未稅A - 成本)/未稅A">
                            <%#Math.Round(Convert.ToDouble(Eval("ProfitA")),2) %>%
                        </td>
                        <td class="center aligned" title="((未稅A - 未稅已回饋B) - 成本)/(未稅A - 未稅已回饋B)">
                            <%#Math.Round(Convert.ToDouble(Eval("ProfitB")),2) %>%
                        </td>
                        <td class="center aligned" title="((未稅A - 未稅應回饋c) - 成本)/(未稅A - 未稅應回饋c)">
                            <%#Math.Round(Convert.ToDouble(Eval("ProfitC")),2) %>%
                        </td>
                        <td class="center aligned collapsing">
                            <a class="ui small teal basic icon button" href="<%=FuncPath() %>/Edit/<%#Eval("Data_ID") %>">
                                <i class="pencil icon"></i>
                            </a>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>
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

            //提示
            //$('#filter_OpcsNo').popup({
            //    inline: true,
            //    on: 'click',
            //    position: 'top left'
            //});

        });
    </script>
    <%-- DataTable Start --%>
    <link href="https://cdn.datatables.net/1.10.13/css/jquery.dataTables.min.css" rel="stylesheet" />
    <script src="https://cdn.datatables.net/1.10.11/js/jquery.dataTables.min.js"></script>
    <script>
        $(function () {
            //使用DataTable
            var table = $('#listTable').DataTable({
                fixedHeader: true,
                searching: false,  //搜尋
                ordering: false,   //排序
                paging: false,     //分頁
                info: false,      //頁數資訊
                language: {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                //捲軸設定
                "scrollY": '60vh',
                "scrollCollapse": true,
                "scrollX": true
                ,
                "footerCallback": function (row, data, start, end, display) {
                    var api = this.api(), data;

                    // Remove the formatting to get integer data for summation
                    var intVal = function (i) {
                        return typeof i === 'string' ?
                            i.replace(/[\$,]/g, '') * 1 :
                            typeof i === 'number' ?
                            i : 0;
                    };

                    //責任目標 (e)
                    total_e = api
                        .column(2)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);

                    //挑戰目標(f)
                    total_f = api
                        .column(4)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);

                    //目前系統業績
                    total_A = api
                        .column(6)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);

                    //單別2341
                    total_F = api
                        .column(7)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);

                    //實際返利業績(a)
                    total_a = api
                        .column(9)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);

                    //與挑戰目標差額
                    total_D = api
                        .column(11)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);

                    //與責任目標差額
                    total_E = api
                        .column(12)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);


                    //應回饋金額
                    total_c = api
                        .column(13)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);

                    //已回饋金額
                    total_B = api
                        .column(14)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);

                    //剩餘回饋金額
                    total_d = api
                        .column(15)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);


                    //月銷售金額
                    total_C = api
                        .column(16)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);

                    //當月最高返利
                    total_b = api
                        .column(17)
                        .data()
                        .reduce(function (a, b) {
                            return Math.round((intVal(a) + intVal(b)) * 100) / 100;
                        }, 0);


                    // Update footer
                    $(api.column(2).footer()).html(thousandComma(total_e));
                    $(api.column(4).footer()).html(thousandComma(total_f));
                    $(api.column(6).footer()).html(thousandComma(total_A));
                    $(api.column(7).footer()).html(thousandComma(total_F));
                    $(api.column(9).footer()).html(thousandComma(total_a));
                    $(api.column(11).footer()).html(thousandComma(total_D));
                    $(api.column(12).footer()).html(thousandComma(total_E));
                    $(api.column(13).footer()).html(thousandComma(total_c));
                    $(api.column(14).footer()).html(thousandComma(total_B));
                    $(api.column(15).footer()).html(thousandComma(total_d));
                    $(api.column(16).footer()).html(thousandComma(total_C));
                    $(api.column(17).footer()).html(thousandComma(total_b));
                }
            });


        });

        //數字格式化:千分位
        var thousandComma = function (number) {
            var num = number.toString();
            var pattern = /(-?\d+)(\d{3})/;

            while (pattern.test(num)) {
                num = num.replace(pattern, "$1,$2");

            }
            return num;
        }
    </script>

    <style>
        /*#listTable td {
            word-break: keep-all;
            word-wrap: break-word;
        }*/
    </style>
    <%-- DataTable End --%>
    <%-- Search UI Start --%>
    <script>
        /* 客戶 (一般查詢) */
        $('.ac-Cust').search({
            minCharacters: 1,
            fields: {
                results: 'results',
                title: 'ID',
                description: 'Label'
            },
            searchFields: [
                'title',
                'description'
            ]
            , onSelect: function (result, response) {
                $("#MainContent_lb_Cust").text(result.Label);
            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?q={query}'
            }

        });
    </script>
    <%-- Search UI End --%>
</asp:Content>

