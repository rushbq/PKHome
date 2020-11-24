<%@ Page Title="出貨明細表-台灣進口" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search_TW.aspx.cs" Inherits="myShipmentData_Search_TW" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
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
                    <h5 class="active section red-text text-darken-2">出貨明細表-台灣進口
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
                        <asp:TextBox ID="filter_Keyword" runat="server" MaxLength="20" autocomplete="off" placeholder="供應商, 報單號碼"></asp:TextBox>
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
                                    <th class="grey-bg lighten-3 center aligned">報關日期</th>
                                    <th class="grey-bg lighten-3 center aligned">贖單日期</th>
                                    <th class="grey-bg lighten-3">廠商</th>
                                    <th class="grey-bg lighten-3">贖單單號</th>
                                    <th class="grey-bg lighten-3">&nbsp;</th>
                                    <th class="grey-bg lighten-3">件數</th>
                                    <th class="grey-bg lighten-3">報關金額<br />
                                        (NTD)</th>
                                    <th class="grey-bg lighten-3">採購金額</th>
                                    <th class="grey-bg lighten-3">匯率</th>
                                    <th class="grey-bg lighten-3">進口報單號碼</th>
                                    <th class="grey-bg lighten-3 center aligned">報關費<br />
                                        (NTD)未稅
                                        <span class="blue-text text-darken-1">(J)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">Local Charge<br />
                                        (NTD)未稅
                                        <span class="blue-text text-darken-1">(K)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">營業稅<br />
                                        <span class="blue-text text-darken-1">(L)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">進口稅<br />
                                        <span class="blue-text text-darken-1">(M)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">貿推費<br />
                                        <span class="blue-text text-darken-1">(N)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">進口營業稅<br />
                                        <span class="blue-text text-darken-1">(O)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">代墊款<br />
                                        <span class="blue-text text-darken-1">(P=M+N+O)</span>
                                    </th>
                                    <th class="grey-bg lighten-3 center aligned">總計<br />
                                        <span class="blue-text text-darken-1">(Q=J+K+L+P)</span>
                                    </th>
                                    <th class="grey-bg lighten-3">商港費</th>
                                    <th class="grey-bg lighten-3 center aligned">進口費用(%)<br />
                                        <span class="blue-text text-darken-1">((J+K)/F)</span>
                                    </th>
                                    <th class="grey-bg lighten-3">採購單<br />
                                        張數</th>
                                    <th class="grey-bg lighten-3">採購單<br />
                                        Item數</th>
                                    <th class="grey-bg lighten-3">卡車</th>
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
                                <%#Eval("CustomsDate") %>
                            </td>
                            <td class="center aligned">
                                <%#Eval("RedeemDate") %>
                            </td>
                            <td class="center aligned red-text text-darken-1">
                                <strong><%#Eval("SupName") %></strong><br />
                                <span class="grey-text text-darken-2">(<%#Eval("SupID") %>)</span>
                            </td>
                            <td class="blue-text text-darken-1">
                                <strong><%#Eval("Redeem_FID") %>-<%#Eval("Redeem_SID") %></strong>
                            </td>
                            <td class="center aligned">
                                <asp:LinkButton ID="lbtn_Save" runat="server" CssClass="ui small teal basic icon button" ValidationGroup="List" CommandName="doSave"><i class="save icon"></i></asp:LinkButton>
                            </td>
                            <td>
                                <!-- 件數 -->
                                <%#Eval("QtyMark") %>
                            </td>
                            <td class="right aligned">
                                <!-- 報關金額 -->
                                <%#Eval("CustomsPrice").ToString().ToMoneyString() %>
                            </td>
                            <td class="right aligned">
                                <!-- 採購金額 -->
                                <div class="ui basic fluid label">
                                    <%#Eval("Currency") %>
                                    <div class="detail"><%# Eval("PurPrice").ToString().ToMoneyString() %></div>
                                </div>
                            </td>
                            <td class="center aligned">
                                <%#Eval("Tax") %>
                            </td>
                            <td>
                                <!-- 進口報單號碼 -->
                                <%#Eval("CustomsNo") %>
                            </td>
                            <td title="J">
                                <!-- 報關費 -->
                                <asp:TextBox ID="tb_Cost_Customs" runat="server" Width="60px" Text='<%#Eval("Cost_Customs") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td title="K">
                                <!-- LocalCharge -->
                                <asp:TextBox ID="tb_Cost_LocalCharge" runat="server" Width="60px" Text='<%#Eval("Cost_LocalCharge") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td title="L">
                                <!-- 營業稅 -->
                                <asp:TextBox ID="tb_Cost_LocalBusiness" runat="server" Width="60px" Text='<%#Eval("Cost_LocalBusiness") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td title="M">
                                <!-- 進口稅 -->
                                <asp:TextBox ID="tb_Cost_Imports" runat="server" Width="60px" Text='<%#Eval("Cost_Imports") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td title="N">
                                <!-- 貿推費 -->
                                <asp:TextBox ID="tb_Cost_Trade" runat="server" Width="60px" Text='<%#Eval("Cost_Trade") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td title="O">
                                <!-- 進口營業稅 -->
                                <asp:TextBox ID="tb_Cost_ImportsBusiness" runat="server" Width="60px" Text='<%#Eval("Cost_ImportsBusiness") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td class="right aligned <%#Convert.ToDouble( Eval("Cnt_CostFee"))>0 ? "negative":"disabled" %>" title="P=M+N+O">
                                <!-- 代墊款 -->
                                <%#Eval("Cnt_CostFee").ToString().ToMoneyString() %>
                            </td>
                            <td class="right aligned <%#Convert.ToDouble( Eval("Cnt_Total"))>0 ? "negative":"disabled" %>" title="Q=J+K+L+P">
                                <!-- 總計 -->
                                <%#Eval("Cnt_Total").ToString().ToMoneyString() %>
                            </td>
                            <td>
                                <!-- 商港費 -->
                                <asp:TextBox ID="tb_Cost_Service" runat="server" Width="60px" Text='<%#Eval("Cost_Service") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td class="center aligned <%#Convert.ToDouble( Eval("Cnt_CostPercent"))>0 ? "positive":"disabled" %>" title="(J+K)/F">
                                <%-- 進口費用(%) --%>
                                <%#Eval("Cnt_CostPercent") %>
                            </td>
                            <td class="center aligned"><%#Eval("PurCnt") %></td>
                            <td class="center aligned"><%#Eval("PurItemCnt") %></td>
                            <td>
                                <!-- 卡車 -->
                                <asp:TextBox ID="tb_Cost_Truck" runat="server" Width="60px" Text='<%#Eval("Cost_Truck") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_Remark" runat="server" Width="200px" Text='<%#Eval("Remark") %>' MaxLength="250"></asp:TextBox>
                                <asp:HiddenField ID="hf_Redeem_FID" runat="server" Value='<%#Eval("Redeem_FID") %>' />
                                <asp:HiddenField ID="hf_Redeem_SID" runat="server" Value='<%#Eval("Redeem_SID") %>' />
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
                    leftColumns: 5,
                    heightMatch: 'semiauto' /* [高度計算] semiauto:計算一次後暫存; auto:每次計算,較慢但準確度高; none:不計算 */
                }
            });

            //點擊時變更背景色
            $('#tableList tbody').on('click', 'tr', function () {
                var bgcolor = 'orange-bg lighten-2';
                var targetBg = 'tr.orange-bg.lighten-2';

                table.$(targetBg).removeClass(bgcolor); //移除其他列背景
                $(this).addClass(bgcolor); //此列新增背景
            });
        });
    </script>
    <%-- DataTables End --%>
</asp:Content>

