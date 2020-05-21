<%@ Page Title="訂單庫存狀況" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myOrderingStock_Search" %>

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
                    <h5 class="active section red-text text-darken-2">訂單庫存狀況 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <%-- <a class="item">
                    <i class="file excel icon"></i>
                    <span class="mobile hidden">匯出</span>
                </a>--%>
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
                    <div class="six wide field">
                        <label>單據日</label>
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
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="four wide field">
                        <label>訂單單號</label>
                        <asp:TextBox ID="filter_BoNo" runat="server" MaxLength="20" placeholder="輸入不含「-」的完整訂單單號" autocomplete="off"></asp:TextBox>
                    </div>
                    <div class="six wide field">
                        <label>客戶</label>
                        <div class="ui fluid search ac-Cust">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Cust" runat="server" CssClass="prompt" placeholder="輸入客戶代號或名稱關鍵字"></asp:TextBox>
                                <asp:Panel ID="lb_Cust" runat="server" CssClass="ui label"><i class="angle left"></i>輸入關鍵字,選擇項目</asp:Panel>
                            </div>
                        </div>
                        <asp:TextBox ID="val_Cust" runat="server" Style="display: none"></asp:TextBox>
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
                <div class="ui icon header">
                    <i class="search icon"></i>
                    目前條件查無資料，請重新查詢。
                </div>
            </div>
        </asp:PlaceHolder>
        <!-- Empty Content End -->

        <!-- List Content Start -->
        <asp:PlaceHolder ID="ph_Data" runat="server">
            <div class="ui green attached segment">
                <table id="dtTable" class="ui celled selectable compact small structured table" style="width: 100%">
                    <thead>
                        <tr>
                            <th class="grey-bg lighten-3" rowspan="2">客戶</th>
                            <th class="grey-bg lighten-3" rowspan="2">單號</th>
                            <th class="grey-bg lighten-3" rowspan="2">品號</th>
                            <th class="grey-bg lighten-3" rowspan="2">確認碼</th>
                            <th class="grey-bg lighten-3 center aligned" rowspan="2">庫別</th>
                            <th class="grey-bg lighten-3 center aligned" rowspan="2">數量</th>

                            <asp:PlaceHolder ID="ph_headerTW" runat="server">
                                <th class="grey-bg lighten-3 center aligned" colspan="5">01倉</th>
                                <th class="grey-bg lighten-3 center aligned" colspan="5">20倉</th>
                                <th class="grey-bg lighten-3 center aligned" colspan="5">21倉</th>
                                <th class="grey-bg lighten-3 center aligned" colspan="5">22倉</th>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="ph_headerSH" runat="server">
                                <th class="grey-bg lighten-3 center aligned" colspan="5">12倉</th>
                                <th class="grey-bg lighten-3 center aligned" colspan="5">128倉</th>
                                <th class="grey-bg lighten-3 center aligned" colspan="5">A01倉</th>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="ph_headerSZ" runat="server">
                                <th class="grey-bg lighten-3 center aligned" colspan="5">A01倉</th>
                            </asp:PlaceHolder>

                            <th class="grey-bg lighten-3 center aligned" colspan="5">總計</th>
                        </tr>
                        <tr>
                            <asp:PlaceHolder ID="ph_subHeaderTW" runat="server">
                                <th class="grey-bg lighten-3 right aligned">01庫存</th>
                                <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                <th class="grey-bg lighten-3 right aligned">預計進</th>
                                <th class="grey-bg lighten-3 right aligned">預計生</th>
                                <th class="grey-bg lighten-3 right aligned">預計領</th>
                                <th class="grey-bg lighten-3 right aligned">20庫存</th>
                                <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                <th class="grey-bg lighten-3 right aligned">預計進</th>
                                <th class="grey-bg lighten-3 right aligned">預計生</th>
                                <th class="grey-bg lighten-3 right aligned">預計領</th>
                                <th class="grey-bg lighten-3 right aligned">21庫存</th>
                                <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                <th class="grey-bg lighten-3 right aligned">預計進</th>
                                <th class="grey-bg lighten-3 right aligned">預計生</th>
                                <th class="grey-bg lighten-3 right aligned">預計領</th>
                                <th class="grey-bg lighten-3 right aligned">22庫存</th>
                                <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                <th class="grey-bg lighten-3 right aligned">預計進</th>
                                <th class="grey-bg lighten-3 right aligned">預計生</th>
                                <th class="grey-bg lighten-3 right aligned">預計領</th>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="ph_subHeaderSH" runat="server">
                                <th class="grey-bg lighten-3 right aligned">12庫存</th>
                                <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                <th class="grey-bg lighten-3 right aligned">預計進</th>
                                <th class="grey-bg lighten-3 right aligned">預計生</th>
                                <th class="grey-bg lighten-3 right aligned">預計領</th>
                                <th class="grey-bg lighten-3 right aligned">128庫存</th>
                                <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                <th class="grey-bg lighten-3 right aligned">預計進</th>
                                <th class="grey-bg lighten-3 right aligned">預計生</th>
                                <th class="grey-bg lighten-3 right aligned">預計領</th>
                                <th class="grey-bg lighten-3 right aligned">A01庫存</th>
                                <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                <th class="grey-bg lighten-3 right aligned">預計進</th>
                                <th class="grey-bg lighten-3 right aligned">預計生</th>
                                <th class="grey-bg lighten-3 right aligned">預計領</th>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="ph_subHeaderSZ" runat="server">
                                <th class="grey-bg lighten-3 right aligned">A01庫存</th>
                                <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                <th class="grey-bg lighten-3 right aligned">預計進</th>
                                <th class="grey-bg lighten-3 right aligned">預計生</th>
                                <th class="grey-bg lighten-3 right aligned">預計領</th>
                            </asp:PlaceHolder>

                            <th class="grey-bg lighten-3 right aligned">總庫存</th>
                            <th class="grey-bg lighten-3 right aligned">總預計銷</th>
                            <th class="grey-bg lighten-3 right aligned">總預計進</th>
                            <th class="grey-bg lighten-3 right aligned">總預計生</th>
                            <th class="grey-bg lighten-3 right aligned">總預計領</th>
                        </tr>
                    </thead>

                    <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound">
                        <LayoutTemplate>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%#Eval("CustID") %></td>
                                <td><%#Eval("BO_Fid") %>-<%#Eval("BO_Sid") %></td>
                                <td class="green-text text-darken-2"><strong><%#Eval("ModelNo") %></strong></td>
                                <td class="center aligned"><strong><%#Eval("CfmCode") %></strong></td>
                                <td class="center aligned"><%#Eval("StockType") %></td>
                                <td class="center aligned warning"><%#Eval("BuyCnt") %></td>
                                <asp:PlaceHolder ID="ph_BodyTW" runat="server">
                                    <!-- 01 -->
                                    <td class="right aligned warning"><%#Eval("StockQty_01") %></td>
                                    <td class="right aligned"><%#Eval("PreSell_01") %></td>
                                    <td class="right aligned"><%#Eval("PreIN_01") %></td>
                                    <td class="right aligned"><%#Eval("PreSet_01") %></td>
                                    <td class="right aligned"><%#Eval("PreGet_01") %></td>
                                    <!-- 20 -->
                                    <td class="right aligned warning"><%#Eval("StockQty_20") %></td>
                                    <td class="right aligned"><%#Eval("PreSell_20") %></td>
                                    <td class="right aligned"><%#Eval("PreIN_20") %></td>
                                    <td class="right aligned"><%#Eval("PreSet_20") %></td>
                                    <td class="right aligned"><%#Eval("PreGet_20") %></td>
                                    <!-- 21 -->
                                    <td class="right aligned warning"><%#Eval("StockQty_21") %></td>
                                    <td class="right aligned"><%#Eval("PreSell_21") %></td>
                                    <td class="right aligned"><%#Eval("PreIN_21") %></td>
                                    <td class="right aligned"><%#Eval("PreSet_21") %></td>
                                    <td class="right aligned"><%#Eval("PreGet_21") %></td>
                                    <!-- 22 -->
                                    <td class="right aligned warning"><%#Eval("StockQty_22") %></td>
                                    <td class="right aligned"><%#Eval("PreSell_22") %></td>
                                    <td class="right aligned"><%#Eval("PreIN_22") %></td>
                                    <td class="right aligned"><%#Eval("PreSet_22") %></td>
                                    <td class="right aligned"><%#Eval("PreGet_22") %></td>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="ph_BodySH" runat="server">
                                    <!-- 12 -->
                                    <td class="right aligned warning"><%#Eval("StockQty_12") %></td>
                                    <td class="right aligned"><%#Eval("PreSell_12") %></td>
                                    <td class="right aligned"><%#Eval("PreIN_12") %></td>
                                    <td class="right aligned"><%#Eval("PreSet_12") %></td>
                                    <td class="right aligned"><%#Eval("PreGet_12") %></td>
                                    <!-- 128 -->
                                    <td class="right aligned warning"><%#Eval("StockQty_128") %></td>
                                    <td class="right aligned"><%#Eval("PreSell_128") %></td>
                                    <td class="right aligned"><%#Eval("PreIN_128") %></td>
                                    <td class="right aligned"><%#Eval("PreSet_128") %></td>
                                    <td class="right aligned"><%#Eval("PreGet_128") %></td>
                                    <!-- A01 -->
                                    <td class="right aligned warning"><%#Eval("StockQty_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreSell_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreIN_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreSet_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreGet_A01") %></td>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="ph_BodySZ" runat="server">
                                    <!-- A01 -->
                                    <td class="right aligned warning"><%#Eval("StockQty_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreSell_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreIN_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreSet_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreGet_A01") %></td>
                                </asp:PlaceHolder>

                                <!-- Total -->
                                <asp:PlaceHolder ID="ph_TotalTW" runat="server">
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("StockQty_01"))+Convert.ToInt32(Eval("StockQty_20"))+Convert.ToInt32(Eval("StockQty_21"))+Convert.ToInt32(Eval("StockQty_22"))%>
                                        </strong>
                                    </td>
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("PreSell_01"))+Convert.ToInt32(Eval("PreSell_20"))+Convert.ToInt32(Eval("PreSell_21"))+Convert.ToInt32(Eval("PreSell_22"))%>
                                        </strong>
                                    </td>
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("PreIN_01"))+Convert.ToInt32(Eval("PreIN_20"))+Convert.ToInt32(Eval("PreIN_21"))+Convert.ToInt32(Eval("PreIN_22"))%>
                                        </strong>
                                    </td>
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("PreSet_01"))+Convert.ToInt32(Eval("PreSet_20"))+Convert.ToInt32(Eval("PreSet_21"))+Convert.ToInt32(Eval("PreSet_22"))%>
                                        </strong>
                                    </td>
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("PreGet_01"))+Convert.ToInt32(Eval("PreGet_20"))+Convert.ToInt32(Eval("PreGet_21"))+Convert.ToInt32(Eval("PreGet_22"))%>
                                        </strong>
                                    </td>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="ph_TotalSH" runat="server">
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("StockQty_12")) + Convert.ToInt32(Eval("StockQty_128")) + Convert.ToInt32(Eval("StockQty_A01"))%>
                                        </strong>
                                    </td>
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("PreSell_12")) + Convert.ToInt32(Eval("PreSell_128")) + Convert.ToInt32(Eval("PreSell_A01"))%>
                                        </strong>
                                    </td>
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("PreIN_12")) + Convert.ToInt32(Eval("PreIN_128")) + Convert.ToInt32(Eval("PreIN_A01"))%>
                                        </strong>
                                    </td>
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("PreSet_12")) + Convert.ToInt32(Eval("PreSet_128")) + Convert.ToInt32(Eval("PreSet_A01"))%>
                                        </strong>
                                    </td>
                                    <td class="right aligned positive">
                                        <strong>
                                            <%#Convert.ToInt32(Eval("PreGet_12")) + Convert.ToInt32(Eval("PreGet_128")) + Convert.ToInt32(Eval("PreGet_A01"))%>
                                        </strong>
                                    </td>
                                </asp:PlaceHolder>

                                <asp:PlaceHolder ID="ph_TotalSZ" runat="server">
                                    <td class="right aligned positive"><%#Eval("StockQty_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreSell_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreIN_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreSet_A01") %></td>
                                    <td class="right aligned"><%#Eval("PreGet_A01") %></td>
                                </asp:PlaceHolder>
                            </tr>
                        </ItemTemplate>
                    </asp:ListView>
                </table>
            </div>

        </asp:PlaceHolder>
        <!-- List Content End -->

        <!-- remote Modal Start -->
        <%--<div id="detailDT" class="ui fullscreen modal">
            <i class="close icon"></i>
            <div class="header"></div>
            <div class="scrolling content">
                <table class="ui celled striped compact table">
                    <thead>
                        <tr>
                            <th class="center aligned">庫別</th>
                            <th class="center aligned">數量</th>
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
        </div>--%>
        <!-- remote Modal End -->
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


            /* Modal Start */
            <%--$(".detailPreSell").on("click", function () {
                var id = $(this).attr("data-id");
                var header = $(this).attr("data-head");
                $("#detailDT .header").text(header + ' : ' + id);

                //load html
                var url = '<%=fn_Param.WebUrl%>' + "myOrderStock/Ashx_GetPreSellList.ashx?CompID=<%=Req_CompID%>&ModelNo=" + id;
                var datablock = $("#detailDT .content tbody");
                datablock.empty();
                datablock.load(url);

                //show modal
                $('#detailDT').modal({
                    selector: {
                        close: '.close, .actions .button'
                    }
                }).modal('show');
            });--%>
            /* Modal End */
        });
    </script>
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
                $("#MainContent_val_Cust").val(result.ID);
                $("#MainContent_lb_Cust").text(result.Label);
            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?q={query}'
            }

        });
    </script>
    <%-- Search UI End --%>
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

    <%-- DataTable Start --%>
    <link href="https://cdn.datatables.net/1.10.13/css/jquery.dataTables.min.css" rel="stylesheet" />
    <link href="https://cdn.datatables.net/buttons/1.6.0/css/buttons.dataTables.min.css" rel="stylesheet" />
    <script src="https://cdn.datatables.net/1.10.11/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/buttons/1.6.0/js/dataTables.buttons.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jszip/3.1.3/jszip.min.js"></script>
    <script src="https://cdn.datatables.net/buttons/1.6.0/js/buttons.html5.min.js"></script>
    <script>
        $(function () {
            //使用DataTable (含Excel匯出)
            //ref: https://datatables.net/extensions/buttons/examples/initialisation/export.html
            var table = $('#dtTable').DataTable({
                fixedHeader: true,
                searching: true,  //搜尋
                ordering: false,   //排序
                paging: true,     //分頁
                info: true,      //頁數資訊
                language: {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                //捲軸設定
                "scrollY": '60vh',
                "scrollCollapse": true,
                "scrollX": true,
                dom: 'Bfrtip',
                buttons: [
                    'excel'
                ]
            });


        });
    </script>
    <%-- DataTable End --%>
</asp:Content>

