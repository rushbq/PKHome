<%@ Page Title="出貨明細表-台灣外銷" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search_TW.aspx.cs" Inherits="myShipmentData_Search_TW" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
    <style>
        /* 複寫dropdown, 讓高度在表格裡正常顯示 */
        .ui.selection.dropdown {
            /*min-width: 8em !important;*/
            min-height: 1em !important;
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
                    <h5 class="active section red-text text-darken-2">出貨明細表-台灣外銷
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
                    <div class="six wide field">
                        <label>報關日</label>
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
                    <div class="five wide field">
                        <label>關鍵字</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" MaxLength="20" autocomplete="off" placeholder="客戶,InvoiceNo"></asp:TextBox>
                    </div>
                    <div class="five wide field">
                        <%-- <label>指定品號</label>
                        <asp:TextBox ID="filter_ModelNo" runat="server" MaxLength="20" autocomplete="off" placeholder="輸入完整品號,不區分大小寫"></asp:TextBox>
                        --%>
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
            <div id="formData" class="ui small form">
                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                    <LayoutTemplate>
                        <table id="tableList" class="ui celled compact small table nowrap">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3 center aligned">進倉日</th>
                                    <th class="grey-bg lighten-3 center aligned">報關日</th>
                                    <th class="grey-bg lighten-3">客戶</th>
                                    <th class="grey-bg lighten-3">INVOICE NO.</th>
                                    <th class="grey-bg lighten-3">&nbsp;</th>
                                    <th class="grey-bg lighten-3">OPCS單號</th>
                                    <th class="grey-bg lighten-3">銷貨單號</th>
                                    <th class="grey-bg lighten-3">箱數</th>
                                    <th class="grey-bg lighten-3">棧板</th>
                                    <th class="grey-bg lighten-3">重量(KGS)</th>
                                    <th class="grey-bg lighten-3">材積(CUFT)</th>
                                    <th class="grey-bg lighten-3">交易條件</th>
                                    <th class="grey-bg lighten-3">付款條件</th>
                                    <th class="grey-bg lighten-3">報關金額(原幣)</th>
                                    <th class="grey-bg lighten-3">客戶金額(本幣)</th>
                                    <th class="grey-bg lighten-3 center aligned">報關費<br />
                                        (NTD)未稅<br />
                                        <span class="blue-text text-darken-1">(N)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">Local Charge<br />
                                        (NTD)未稅<br />
                                        <span class="blue-text text-darken-1">(O)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">產證費用<br />
                                        <span class="blue-text text-darken-1">(O1)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">海運費<br />
                                        (NTD)未稅<br />
                                        <span class="blue-text text-darken-1">(P)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">營業稅<br />
                                        (NTD)<br />
                                        <span class="blue-text text-darken-1">(Q)</span>
                                    </th>
                                    <th class="grey-bg lighten-3">出口費用小計<br />
                                        (NTD)含稅<br />
                                        <span class="blue-text text-darken-1">(R=N+O+O1+P+Q)</span>
                                    </th>
                                    <th class="grey-bg lighten-3">貨運公司</th>
                                    <th class="grey-bg lighten-3">提單編號</th>
                                    <th class="grey-bg lighten-3">卡車/物流費用<br />
                                        (NTD)未稅<br />
                                        <span class="blue-text text-darken-1">(T)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">代收費用<br />
                                        (原幣)<br />
                                        <span class="blue-text text-darken-1">(U)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">代收費用<br />
                                        (本幣)<br />
                                        <span class="blue-text text-darken-1">(V=U*W)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">匯率<br />
                                        <span class="blue-text text-darken-1">(W)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">實際出口費用<br />
                                        (NTD)未稅<br />
                                        <span class="blue-text text-darken-1">(X=N+O+O1+P+T-V)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">費用比率(%)<br />
                                        <span class="blue-text text-darken-1">(Y=X/M)</span>
                                    </th>
                                    <th class="grey-bg lighten-3">FWD</th>
                                    <th class="grey-bg lighten-3">櫃場/機場</th>
                                    <th class="grey-bg lighten-3">CLS</th>
                                    <th class="grey-bg lighten-3">ETD</th>
                                    <th class="grey-bg lighten-3">ETA</th>
                                    <th class="grey-bg lighten-3">貿推費<br />
                                        (NTD)</th>
                                    <th class="grey-bg lighten-3">商港服務費<br />
                                        (NTD)</th>
                                    <th class="grey-bg lighten-3">機械使用費</th>
                                    <th class="grey-bg lighten-3">報單號碼</th>
                                    <th class="grey-bg lighten-3">海關查驗</th>
                                    <th class="grey-bg lighten-3">OPCS<br />
                                        張數</th>
                                    <th class="grey-bg lighten-3">OPCS<br />
                                        Item數</th>
                                    <th class="grey-bg lighten-3">實際報關<br />
                                        差異天數</th>
                                    <th class="grey-bg lighten-3">業務</th>
                                    <th class="grey-bg lighten-3">備註</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                        </table>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="center aligned">
                                <%#Eval("SO_Date") %>
                            </td>
                            <td class="center aligned">
                                <%#Eval("BoxDate") %>
                            </td>
                            <td class="center aligned red-text text-darken-1">
                                <strong><%#Eval("CustName") %></strong><br />
                                <span class="grey-text text-darken-2">(<%#Eval("CustID") %>)</span>
                            </td>
                            <td class="blue-text text-darken-1">
                                <strong><%#Eval("InvNo") %></strong>
                            </td>
                            <td class="center aligned">
                                <asp:LinkButton ID="lbtn_Save" runat="server" CssClass="ui small teal basic icon button" ValidationGroup="List" CommandName="doSave"><i class="save icon"></i></asp:LinkButton>
                            </td>
                            <td class="center aligned">
                                <a class="ui mini grey basic icon button btn-OpenDetail" data-id="<%#Eval("SO_FID") %><%#Eval("SO_SID") %>" data-title="<%#Eval("SO_FID") %>-<%#Eval("SO_SID") %>" title="展開明細">
                                    <i class="folder open icon"></i>
                                </a>
                            </td>
                            <td>
                                <%#Eval("SO_FID") %>-<%#Eval("SO_SID") %>
                            </td>
                            <td>
                                <!-- 箱數 -->
                                <asp:TextBox ID="tb_BoxCnt" runat="server" Width="60px" Text='<%#Eval("BoxCnt") %>' type="number" step="1" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td>
                                <!-- 棧板 -->
                                <asp:TextBox ID="tb_Pallet" runat="server" Width="60px" Text='<%#Eval("Pallet") %>' MaxLength="4" placeholder="最多 4 字"></asp:TextBox>
                            </td>
                            <td>
                                <!-- 重量 -->
                                <asp:TextBox ID="tb_Weight" runat="server" Width="60px" Text='<%#Eval("Weight") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td>
                                <!-- 材積 -->
                                <asp:TextBox ID="tb_Cuft" runat="server" Width="60px" Text='<%#Eval("Cuft") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td>
                                <!-- 交易條件 -->
                                <asp:TextBox ID="tb_TradeTerms" runat="server" Width="60px" Text='<%#Eval("TradeTerms") %>' MaxLength="20" placeholder="最多 20 字"></asp:TextBox>
                            </td>
                            <td>
                                <%#Eval("PayTerms") %>
                            </td>
                            <td class="right aligned">
                                <div class="ui basic fluid label">
                                    <%#Eval("Currency") %>
                                    <div class="detail"><%# Eval("Price").ToString().ToMoneyString() %></div>
                                </div>
                            </td>
                            <td class="right aligned">
                                <div class="ui basic fluid label">
                                    <div class="detail"><%# Eval("localPrice").ToString().ToMoneyString() %></div>
                                </div>
                            </td>
                            <td title="N">
                                <!-- 報關費N -->
                                <asp:TextBox ID="tb_Cost_Customs" runat="server" Width="60px" Text='<%#Eval("Cost_Customs") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td title="O">
                                <!-- LocalCharge(O) -->
                                <asp:TextBox ID="tb_Cost_LocalCharge" runat="server" Width="60px" Text='<%#Eval("Cost_LocalCharge") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td title="O1">
                                <!-- 產證費用(O1) -->
                                <asp:TextBox ID="tb_Cost_Cert" runat="server" Width="60px" Text='<%#Eval("Cost_Cert") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td title="P">
                                <!-- 海運費P -->
                                <asp:TextBox ID="tb_Cost_Freight" runat="server" Width="60px" Text='<%#Eval("Cost_Freight") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td title="Q">
                                <!-- 營業稅Q -->
                                <asp:TextBox ID="tb_Cost_Business" runat="server" Width="60px" Text='<%#Eval("Cost_Business") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td class="right aligned <%#Convert.ToDouble( Eval("Cnt_CostExport"))>0 ? "negative":"disabled" %>">
                                <!-- 出口費用小計 -->
                                <%#Eval("Cnt_CostExport").ToString().ToMoneyString() %>
                            </td>
                            <td>
                                <asp:DropDownList ID="ddl_Ship" runat="server" Width="120px"></asp:DropDownList>
                            </td>
                            <td>
                                <!-- 提單編號 -->
                                <asp:TextBox ID="tb_TrackingNo" runat="server" Width="100px" Text='<%#Eval("TrackingNo") %>' MaxLength="40" placeholder="最多 40 字"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_Cost_Shipment" runat="server" Width="60px" Text='<%#Eval("Cost_Shipment") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td class="right aligned" title="U">
                                <asp:TextBox ID="tb_Cost_Fee" runat="server" Width="60px" Text='<%#Eval("Cost_Fee") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td class="right aligned <%#Convert.ToDouble( Eval("Cnt_CostLocalFee"))>0 ? "negative":"disabled" %>">
                                <%-- 代收費用(本幣) V=U*W --%>
                                <%#Eval("Cnt_CostLocalFee").ToString().ToMoneyString() %>
                            </td>
                            <td class="center aligned" title="W"><%#Eval("Tax") %></td>
                            <td class="right aligned <%#Convert.ToDouble(Eval("Cnt_CostFullExport"))>0 ? "negative":"disabled" %>">
                                <%-- 實際出口費用 X=N+O+P-V --%>
                                <%#Eval("Cnt_CostFullExport").ToString().ToMoneyString() %>
                            </td>
                            <td class="center aligned <%#Convert.ToDouble(Eval("Cnt_CostPercent"))>0 ? "positive":"disabled" %>">
                                <%-- 費用比率(%) Y=X/M --%>
                                <%#Eval("Cnt_CostPercent") %>
                            </td>
                            <td>
                                <!-- 貨代公司 -->
                                <asp:TextBox ID="tb_FWD" runat="server" Width="60px" Text='<%#Eval("FWD") %>' MaxLength="20" placeholder="最多 20 字"></asp:TextBox>
                            </td>
                            <td>
                                <asp:DropDownList ID="ddl_Place" runat="server" Width="100px"></asp:DropDownList>
                            </td>
                            <td><%#Eval("CLS") %></td>
                            <td><%#Eval("ETD") %></td>
                            <td><%#Eval("ETA") %></td>
                            <td>
                                <!-- 貿推費 -->
                                <asp:TextBox ID="tb_Cost_Trade" runat="server" Width="60px" Text='<%#Eval("Cost_Trade") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td>
                                <!-- 商港服務費 -->
                                <asp:TextBox ID="tb_Cost_Service" runat="server" Width="60px" Text='<%#Eval("Cost_Service") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td>
                                <!-- 機械使用費 -->
                                <asp:TextBox ID="tb_Cost_Use" runat="server" Width="60px" Text='<%#Eval("Cost_Use") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td><%#Eval("Ship_NO") %></td>
                            <td>
                                <asp:DropDownList ID="ddl_Check" runat="server" Width="60px"></asp:DropDownList>
                            </td>
                            <td class="center aligned"><%#Eval("OpcsCnt") %></td>
                            <td class="center aligned"><%#Eval("OpcsItemCnt") %></td>
                            <td class="center aligned"><%#Eval("diffDays") %></td>
                            <td class="center aligned"><%#Eval("SalesName") %></td>
                            <td>
                                <asp:TextBox ID="tb_Remark" runat="server" Width="200px" Text='<%#Eval("Remark") %>' MaxLength="250" placeholder="最多 250 字"></asp:TextBox>
                                <asp:HiddenField ID="hf_Ship_FID" runat="server" Value='<%#Eval("Ship_FID") %>' />
                                <asp:HiddenField ID="hf_Ship_SID" runat="server" Value='<%#Eval("Ship_SID") %>' />
                                <asp:HiddenField ID="hf_SO_FID" runat="server" Value='<%#Eval("SO_FID") %>' />
                                <asp:HiddenField ID="hf_SO_SID" runat="server" Value='<%#Eval("SO_SID") %>' />
                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <EmptyDataTemplate>
                        <div class="ui placeholder segment">
                            <div class="ui icon header">
                                <i class="search icon"></i>
                                目前條件查無資料
                            </div>
                        </div>
                    </EmptyDataTemplate>
                </asp:ListView>
            </div>
            <!-- List Pagination Start -->
            <div class="ui mini bottom attached segment grey-bg lighten-4">
                <asp:Literal ID="lt_Pager" runat="server"></asp:Literal>
            </div>
            <!-- List Pagination End -->
        </asp:PlaceHolder>
        <!-- List Content End -->
    </div>
    <!-- 內容 End -->
    <!-- Opcs Modal Start -->
    <div id="opcsDT" class="ui small modal">
        <div class="header">
            OPCS單號&nbsp;<small>(銷貨單號:<span class="grey-text text-darken-1" id="titleSoID"></span>)</small>
        </div>
        <div class="scrolling content">
            <table class="ui striped celled table">
                <thead>
                    <tr>
                        <th>單別</th>
                        <th>單號</th>
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
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //Click:Search
            $("#doSearch").click(function () {
                //觸發查詢按鈕
                $("#MainContent_btn_Search").trigger("click");
            });


            //Click:Save
            $("#doSave").click(function () {
                //confirm
                var r = confirm("是否要存檔??\n「確定」:資料存檔\n「取消」:繼續編輯");
                if (r == true) {

                } else {
                    return false;
                }

                //loading
                $("#formData").addClass("loading");
                $("#MainContent_btn_Save").trigger("click");
            });
        });
    </script>
    <%-- Modal-載入Opcs單號 --%>
    <script>
        $(function () {
            $(".btn-OpenDetail").on("click", function () {
                var id = $(this).attr("data-id");
                var title = $(this).attr("data-title");

                $("#titleSoID").text(title);

                //load html
                var url = '<%=fn_Param.WebUrl%>' + "myShipmentData/Ashx_GetOpcsData.ashx?DBS=TW&SoID=" + id;
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

    <link href="<%=fn_Param.CDNUrl %>plugin/dataTables-1.10.20/datatables.min.css?v=1.1" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/dataTables-1.10.20/datatables.min.js?v=1.1"></script>
    <%--<link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/v/se/dt-1.10.20/fc-3.3.0/fh-3.1.6/r-2.2.3/rg-1.1.1/rr-1.2.6/sc-2.0.1/datatables.min.css" />
    <script type="text/javascript" src="https://cdn.datatables.net/v/se/dt-1.10.20/fc-3.3.0/fh-3.1.6/r-2.2.3/rg-1.1.1/rr-1.2.6/sc-2.0.1/datatables.min.js"></script>--%>
    <script>
        $(function () {
            //使用DataTable
            var table = $('#tableList').DataTable({
                //fixedHeader: true,
                searching: false,  //搜尋
                ordering: false,   //排序
                paging: false,     //分頁
                info: false,      //頁數資訊
                //pageLength: 10,   //顯示筆數預設值
                language: {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                //捲軸設定
                "scrollY": '70vh',
                "scrollCollapse": true,
                "scrollX": true,
                fixedColumns: {
                    leftColumns: 5,
                    heightMatch: 'semiauto'
                }
            });

            //點擊時變更背景色
            $('#tableList tbody').on('click', 'tr', function () {
                var bgcolor = 'orange-bg lighten-2';
                var targetBg = 'tr.orange-bg.lighten-2';

                table.$(targetBg).removeClass(bgcolor); //移除其他列背景
                $(this).addClass(bgcolor); //此列新增背景
            });
            ////點擊時移除背景色
            //$('#tableList tbody').on('dblclick', 'tr', function () {
            //    var bgcolor = 'orange-bg lighten-2';

            //    if ($(this).hasClass(bgcolor)) {
            //        $(this).removeClass(bgcolor);
            //    }
            //});
        });
    </script>
    <%-- DataTables End --%>
</asp:Content>

