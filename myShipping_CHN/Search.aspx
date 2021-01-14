<%@ Page Title="出貨明細表" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myShipping_Search" %>

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
                    <div class="active section red-text text-darken-2">
                        出貨明細表 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
            <div class="right menu">
                <a href="<%=fn_Param.WebUrl %>myShipping_CHN/CustSendCntStat.aspx" class="item" target="_blank"><i class="sync alternate icon"></i><span class="mobile hidden">代發統計</span></a>
                <a href="<%=fn_Param.WebUrl %><%:Req_Lang %>/<%:Req_RootID %>/ShipImportCHN?dt=<%=Req_DataType %>" class="item"><i class="sync alternate icon"></i><span class="mobile hidden">物流單轉入</span></a>
                <asp:LinkButton ID="lbtn_Export2" runat="server" OnClick="lbtn_Export2_Click" CssClass="item" ToolTip="查詢條件僅限銷貨日"><i class="file excel icon"></i><span class="mobile hidden">拼箱樣單</span></asp:LinkButton>
                <asp:LinkButton ID="lbtn_Export1" runat="server" OnClick="lbtn_Export1_Click" CssClass="item"><i class="file excel icon"></i><span class="mobile hidden">代發明細</span></asp:LinkButton>
                <asp:LinkButton ID="lbtn_ShipExcel" runat="server" OnClick="lbtn_ShipExcel_Click" CssClass="item"><i class="file excel icon"></i><span class="mobile hidden">德邦匯出</span></asp:LinkButton>
                <asp:LinkButton ID="lbtn_Excel" runat="server" OnClick="lbtn_Excel_Click" CssClass="item"><i class="file excel icon"></i><span class="mobile hidden">一般匯出</span></asp:LinkButton>

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
                    <div class="five wide field">
                        <label>銷貨日期</label>
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
                                    <i class="calendar alternate icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="five wide field">
                        <label>關鍵字查詢 (ERP單號, 物流單號, 收件人)</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" placeholder="輸入關鍵字" autocomplete="off"></asp:TextBox>
                    </div>
                    <div class="three wide field">
                        <label>物流途徑</label>
                        <asp:DropDownList ID="filter_ShipWay" runat="server" CssClass="fluid topSearch">
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label>運費方式</label>
                        <asp:DropDownList ID="filter_FreightWay" runat="server" CssClass="fluid topSearch">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="fields">
                    <div class="five wide field">
                        <label>銷貨單開立時間</label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datetimepicker">
                                    <asp:TextBox ID="filter_sDate_Ship" runat="server" placeholder="開始時間" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datetimepicker">
                                    <asp:TextBox ID="filter_eDate_Ship" runat="server" placeholder="結束時間" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate icon"></i>
                                </div>
                            </div>
                        </div>
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
                    <div class="three wide field">
                        <label>貨運公司</label>
                        <asp:DropDownList ID="filter_ShipComp" runat="server" CssClass="fluid topSearch">
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label>資材確認</label>
                        <asp:DropDownList ID="filter_IsCheck" runat="server" CssClass="fluid topSearch">
                            <asp:ListItem Value="">-- 全部 --</asp:ListItem>
                            <asp:ListItem Value="Y">已確認</asp:ListItem>
                            <asp:ListItem Value="N">未確認</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
            </div>
            <div class="ui two column grid">
                <div class="column">
                    <a href="<%=FuncPath() %>?dt=<%=Req_DataType %>" class="ui small button"><i class="refresh icon"></i>重置條件</a>
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
            <div id="formData" class="ui small form">
                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                    <LayoutTemplate>
                        <table id="tableList" class="ui celled compact small table nowrap">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3">銷貨日期</th>
                                    <th class="grey-bg lighten-3">&nbsp;</th>
                                    <th class="grey-bg lighten-3">資材確認</th>
                                    <th class="grey-bg lighten-3">銷貨單開立時間</th>
                                    <th class="grey-bg lighten-3 center aligned">客戶<br />
                                        銷貨單號</th>
                                    <th class="grey-bg lighten-3">銷貨金額</th>
                                    <th class="grey-bg lighten-3">備註</th>
                                    <th class="grey-bg lighten-3">貨運公司</th>
                                    <th class="grey-bg lighten-3">物流單號</th>
                                    <th class="grey-bg lighten-3">運費方式</th>
                                    <th class="grey-bg lighten-3">物流途徑</th>
                                    <th class="grey-bg lighten-3">件數</th>
                                    <th class="grey-bg lighten-3">運費金額</th>
                                    <th class="grey-bg lighten-3">收件人</th>
                                    <th class="grey-bg lighten-3">收件電話</th>
                                    <th class="grey-bg lighten-3">收件地址</th>
                                    <th class="grey-bg lighten-3">銷售員</th>
                                    <th class="grey-bg lighten-3">銷貨單<br />
                                        確認</th>
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
                                <strong><%#Eval("Erp_SO_Date") %></strong>
                            </td>
                            <td class="left aligned collapsing">
                                <asp:PlaceHolder ID="ph_Save" runat="server">
                                    <asp:LinkButton ID="lbtn_Save" runat="server" CssClass="ui small teal basic icon button" ValidationGroup="List" CommandName="doSave"><i class="save icon"></i></asp:LinkButton>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="ph_Del" runat="server">
                                    <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定重置?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                </asp:PlaceHolder>
                            </td>
                            <td class="center aligned">
                                <!-- 資材確認 -->
                                <asp:LinkButton ID="lbtn_CheckY" runat="server" CssClass="ui grey basic icon button" ValidationGroup="List" CommandName="DoCheck_YES" ToolTip="設為確認"><i class="truck icon"></i></asp:LinkButton>
                                <asp:LinkButton ID="lbtn_CheckN" runat="server" CssClass="ui circular green basic icon button" ValidationGroup="List" CommandName="DoCheck_NO" ToolTip="取消確認"><i class="check icon"></i></asp:LinkButton>
                            </td>
                            <td>
                                <span class="ui basic fluid label">
                                    <%#Eval("Create_Time").ToString().ToDateString("yyyy/MM/dd HH:mm") %>
                                </span>
                            </td>
                            <td>
                                <!-- 客戶/單號 -->
                                <h5 class="green-text text-darken-3" style="margin-bottom: 0px;"><%#Eval("CustName") %></h5>
                                <h5 class="blue-text text-darken-3" style="margin-top: 0.5rem;"><%#Eval("Erp_SO_FullID") %></h5>
                            </td>
                            <td class="right aligned red-text text-darken-1">
                                <strong><%#Eval("TotalPrice").ToString().ToMoneyString() %></strong>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_Remark" runat="server" Width="100px" Text='<%#Eval("Remark") %>' MaxLength="100" placeholder="最多 50 字"></asp:TextBox>
                            </td>
                            <td class="center aligned">
                                <!-- 貨運公司 -->
                                <asp:DropDownList ID="lst_ShipComp" runat="server" Width="110px"></asp:DropDownList>
                            </td>
                            <td>
                                <!-- 物流單號 -->
                                <asp:TextBox ID="tb_ShipNo" runat="server" Width="110px" Text='<%#Eval("ShipNo") %>' MaxLength="20" autocomplete="off"></asp:TextBox>
                            </td>
                            <td class="center aligned">
                                <!-- 運費方式 -->
                                <asp:DropDownList ID="lst_FreightWay" runat="server" Width="80px"></asp:DropDownList>
                            </td>
                            <td class="center aligned">
                                <!-- 物流途徑 -->
                                <asp:DropDownList ID="lst_ShipWay" runat="server" Width="80px"></asp:DropDownList>
                            </td>
                            <td class="center aligned">
                                <!-- 件數 -->
                                <asp:TextBox ID="tb_BoxCnt" runat="server" Width="40px" Text='<%#Eval("BoxCnt") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td class="right aligned blue-text text-darken-3">
                                <!-- 運費 -->
                                <asp:TextBox ID="tb_Freight" runat="server" Width="40px" Text='<%#Eval("Freight") %>' type="number" step="any" min="0"></asp:TextBox>
                            </td>
                            <td>
                                <!-- 收件人 -->
                                <asp:TextBox ID="tb_ShipWho" runat="server" Width="60px" Text='<%#Eval("ShipWho") %>' MaxLength="20" autocomplete="off"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_ShipTel" runat="server" Width="90px" Text='<%#Eval("ShipTel") %>' MaxLength="20" autocomplete="off"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_ShipAddr1" runat="server" Width="110px" Text='<%#Eval("ShipAddr1") %>' MaxLength="120" autocomplete="off" placeholder="地址1, 最多120字"></asp:TextBox><br />
                                <asp:TextBox ID="tb_ShipAddr2" runat="server" Width="110px" Text='<%#Eval("ShipAddr2") %>' MaxLength="120" autocomplete="off" placeholder="地址2, 最多120字" Style="margin-top: 3px;"></asp:TextBox>
                            </td>
                            <td class="center aligned">
                                <!-- 確認者 -->
                                <%#Eval("CfmWhoName") %>
                            </td>
                            <td class="center aligned">
                                <%#Eval("CfmCode") %>
                                <asp:HiddenField ID="hf_SO_FID" runat="server" Value='<%#Eval("Erp_SO_FID") %>' />
                                <asp:HiddenField ID="hf_SO_SID" runat="server" Value='<%#Eval("Erp_SO_SID") %>' />
                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
                                <asp:HiddenField ID="hf_UserCheck1" runat="server" Value='<%#Eval("UserCheck1") %>' />
                                <asp:HiddenField ID="hf_OldCheckTime1" runat="server" Value='<%#Eval("Check_Time1").ToString().ToDateString("yyyy/MM/dd HH:mm") %>' />
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
            $(".myContentBody").keypress(function (e) {
                code = (e.keyCode ? e.keyCode : e.which);
                if (code == 13) {
                    $("#doSearch").trigger("click");
                    //避免觸發submit
                    e.preventDefault();
                }
            });

            //Click:Search
            $("#doSearch").click(function () {
                //觸發查詢按鈕
                $("#MainContent_btn_Search").trigger("click");
            });


            //Click:Save
            $("#doSave").click(function () {
                //confirm
                var r = confirm("是否要存檔??\n「確定」:資料存檔\n「取消」:繼續編輯\n資材確認打勾後,資料則鎖定不可修改.");
                if (r == true) {

                } else {
                    return false;
                }

                //loading
                $("#formData").addClass("loading");
                $("#MainContent_btn_Save").trigger("click");
            });


            //init dropdown list
            $('select.topSearch').dropdown();

        });
    </script>
    <%-- 日期選擇器 Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.js"></script>
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/options.js"></script>
    <script>
        $(function () {
            //[Cal1]取得設定值(往前天數, 往後天數)
            var calOpt1 = getCalOptBydate(730, 0);
            //[Cal1]載入datepicker
            $('.datepicker').calendar(calOpt1);

            //[Cal2]取得設定值(往前天數, 往後天數)
            var calOpt2 = getCalOptByTime(60, 1);
            //[Cal2]載入datepicker
            $('.datetimepicker').calendar(calOpt2);

        });
    </script>
    <%-- 日期選擇器 End --%>
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
                var bgcolor = 'orange-bg lighten-4';
                var targetBg = 'tr.orange-bg.lighten-4';

                table.$(targetBg).removeClass(bgcolor); //移除其他列背景
                $(this).addClass(bgcolor); //此列新增背景
            });
        });
    </script>
    <%-- DataTables End --%>
</asp:Content>

