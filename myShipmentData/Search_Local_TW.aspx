<%@ Page Title="出貨明細表-台灣內銷" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search_Local_TW.aspx.cs" Inherits="myShipmentData_Search_Local_TW" %>

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
                    <h5 class="active section red-text text-darken-2">出貨明細表-台灣內銷
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <div class="item" title="物流單號,運費">
                    <label for="MainContent_freightImport" style="cursor: pointer;">
                        <i class="sync alternate icon"></i>運費轉入
                    </label>
                    <asp:FileUpload ID="freightImport" runat="server" accept=".xlsx,.xls" Style="display: none" />
                    <asp:Button ID="btn_Import" runat="server" OnClick="btn_Import_Click" Style="display: none" />
                </div>

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
                        <label>銷貨日</label>
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
                        <asp:TextBox ID="filter_Keyword" runat="server" MaxLength="20" autocomplete="off" placeholder="客戶代號, 名稱"></asp:TextBox>
                    </div>
                    <div class="five wide field">
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
                                    <th class="grey-bg lighten-3 center aligned">日期</th>
                                    <th class="grey-bg lighten-3">客戶</th>
                                    <th class="grey-bg lighten-3">&nbsp;</th>
                                    <th class="grey-bg lighten-3 center aligned">客戶別</th>
                                    <th class="grey-bg lighten-3 center aligned">商品類別</th>
                                    <th class="grey-bg lighten-3">OPCS單號</th>
                                    <th class="grey-bg lighten-3">銷貨單號</th>
                                    <th class="grey-bg lighten-3 center aligned">出貨單金額<br />
                                        <span class="blue-text text-darken-1">(L)</span>
                                    </th>
                                    <th class="grey-bg lighten-3">OPCS筆數</th>
                                    <th class="grey-bg lighten-3">件數</th>
                                    <th class="grey-bg lighten-3">出貨方式</th>
                                    <th class="grey-bg lighten-3">托運單號</th>
                                    <th class="grey-bg lighten-3 center aligned">運費金額<br />
                                        <span class="blue-text text-darken-1">(G)</span>
                                    </th>
                                    <th class="grey-bg lighten-3">運費比(%)<br />
                                        <span class="blue-text text-darken-1">(M=L/G)</span>
                                    </th>
                                    <th class="grey-bg lighten-3">業務</th>
                                    <th class="grey-bg lighten-3">發票號碼(起)</th>
                                    <th class="grey-bg lighten-3">發票號碼(迄)</th>
                                    <th class="grey-bg lighten-3">發票寄送方式</th>
                                    <th class="grey-bg lighten-3">發票寄出單號</th>
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
                            <td class="center aligned red-text text-darken-1">
                                <strong><%#Eval("CustName") %></strong><br />
                                <span class="grey-text text-darken-2">(<%#Eval("CustID") %>)</span>
                            </td>
                            <td class="center aligned">
                                <asp:LinkButton ID="lbtn_Save" runat="server" CssClass="ui small teal basic icon button" ValidationGroup="List" CommandName="doSave"><i class="save icon"></i></asp:LinkButton>
                            </td>
                            <td>
                                <asp:DropDownList ID="ddl_CustType" runat="server" Width="80px"></asp:DropDownList>
                            </td>
                            <td>
                                <asp:DropDownList ID="ddl_ProdType" runat="server" Width="60px"></asp:DropDownList>
                            </td>
                            <td class="center aligned">
                                <a class="ui mini grey basic icon button btn-OpenDetail" data-id="<%#Eval("SO_FID") %><%#Eval("SO_SID") %>" data-title="<%#Eval("SO_FID") %>-<%#Eval("SO_SID") %>" title="展開明細">
                                    <i class="folder open icon"></i>
                                </a>
                            </td>
                            <td>
                                <%#Eval("SO_FID") %>-<%#Eval("SO_SID") %>
                            </td>
                            <td class="right aligned">
                                <div class="ui basic fluid label">
                                    <div class="detail"><%# Eval("Price").ToString().ToMoneyString() %></div>
                                </div>
                            </td>
                            <td class="center aligned"><%#Eval("OpcsCnt") %></td>

                            <td>
                                <!-- 件數 -->
                                <asp:TextBox ID="tb_BoxCnt" runat="server" Width="60px" Text='<%#Eval("BoxCnt") %>' type="number" step="1" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td>
                                <asp:DropDownList ID="ddl_Ship" runat="server" Width="120px"></asp:DropDownList>
                            </td>
                            <td>
                                <!-- 托運單號 -->
                                <asp:TextBox ID="tb_ShipNo" runat="server" Width="100px" Text='<%#Eval("ShipNo") %>' MaxLength="40" placeholder="最多 40 字"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_Freight" runat="server" Width="60px" Text='<%#Eval("Freight") %>' type="number" step="any" min="0" placeholder="填入數字"></asp:TextBox>
                            </td>
                            <td class="center aligned <%#Convert.ToDouble(Eval("Cnt_FreightPercent"))>0 ? "positive":"disabled" %>">
                                <%-- 運費比(%) --%>
                                <%#Eval("Cnt_FreightPercent") %>
                            </td>
                            <td class="center aligned"><%#Eval("SalesName") %></td>
                            <td class="center aligned"><%#Eval("InvNo_Start") %></td>
                            <td class="center aligned"><%#Eval("InvNo_End") %></td>
                            <td>
                                <asp:DropDownList ID="ddl_SendType" runat="server" Width="60px"></asp:DropDownList>
                            </td>
                            <td>
                                <!-- 發票寄出單號 -->
                                <asp:TextBox ID="tb_SendNo" runat="server" Width="100px" Text='<%#Eval("SendNo") %>' MaxLength="40" placeholder="最多 40 字"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_Remark" runat="server" Width="200px" Text='<%#Eval("Remark") %>' MaxLength="250" placeholder="最多 250 字"></asp:TextBox>
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


            //Click:Upload
            $("#MainContent_freightImport").change(function () {
                //confirm
                var r = confirm("請確認匯入的資料在「第一個工作表」?");
                if (r == true) {

                } else {
                    return false;
                }

                //trigger
                $("#MainContent_btn_Import").trigger("click");
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
                    /* 要凍結的窗格不可放要編輯的欄位 */
                    leftColumns: 3,
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

