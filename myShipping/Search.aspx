<%@ Page Title="發貨明細表" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myShipping_Search" %>

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
                    <div class="section">發貨/運費維護統計</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        發貨明細表
                    </div>
                </div>
            </div>
            <div class="right menu">
                <a href="<%=FuncPath_Import() %>" class="item"><i class="sync alternate icon"></i><span class="mobile hidden">物流單轉入</span></a>
                <asp:LinkButton ID="lbtn_Excel" runat="server" OnClick="lbtn_Excel_Click" CssClass="item"><i class="file excel icon"></i><span class="mobile hidden">匯出</span></asp:LinkButton>

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
                        <label>單據日期</label>
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
                        <label>關鍵字查詢 (ERP單號, 物流單號)</label>
                        <asp:TextBox ID="filter_ErpNo" runat="server" placeholder="輸入關鍵字" autocomplete="off"></asp:TextBox>
                    </div>
                    <div class="three wide field">
                        <label>物流途徑</label>
                        <asp:DropDownList ID="filter_ShipWay" runat="server">
                            <asp:ListItem Value="ALL">-- 全部 --</asp:ListItem>
                            <asp:ListItem Value="A">客戶自提</asp:ListItem>
                            <asp:ListItem Value="B">自行送貨</asp:ListItem>
                            <asp:ListItem Value="C">其它</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label>排序欄位</label>
                        <asp:DropDownList ID="sort_SortField" runat="server">
                            <asp:ListItem Value="">系統預設</asp:ListItem>
                            <asp:ListItem Value="A">單別+單號</asp:ListItem>
                            <asp:ListItem Value="B">發貨日期</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="fields">
                    <div class="five wide field">
                        <label>發貨日期</label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_sDate_Ship" runat="server" placeholder="開始日" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_eDate_Ship" runat="server" placeholder="結束日" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
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
                        <label>出貨地</label>
                        <asp:DropDownList ID="filter_ShipFrom" runat="server">
                            <asp:ListItem Value="ALL">-- 全部 --</asp:ListItem>
                            <asp:ListItem Value="SH">上海</asp:ListItem>
                            <asp:ListItem Value="SZ">深圳</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label>排序方式</label>
                        <asp:DropDownList ID="sort_SortWay" runat="server">
                            <asp:ListItem Value="A">遞增(小到大)</asp:ListItem>
                            <asp:ListItem Value="B" Selected="True">遞減(大到小)</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="inline fields">
                    <div class="field">
                        <div class="ui checkbox">
                            <asp:CheckBox ID="cb_Pay1" runat="server" />
                            <label>到付</label>
                        </div>
                    </div>
                    <div class="field">
                        <div class="ui checkbox">
                            <asp:CheckBox ID="cb_Pay2" runat="server" />
                            <label>自付</label>
                        </div>
                    </div>
                    <div class="field">
                        <div class="ui checkbox">
                            <asp:CheckBox ID="cb_Pay3" runat="server" />
                            <label>墊付</label>
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
                <div class="ui icon header">
                    <i class="search icon"></i>
                    目前條件查無資料，請重新查詢。
                </div>
            </div>
        </asp:PlaceHolder>
        <!-- Empty Content End -->

        <!-- List Content Start -->
        <asp:PlaceHolder ID="ph_Data" runat="server">
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3 center aligned" colspan="2">ERP資料</th>
                                    <th class="grey-bg lighten-3 center aligned" colspan="11">發貨資料</th>
                                    <th class="grey-bg lighten-2 center aligned" colspan="3">平均運費/箱</th>
                                    <th class="grey-bg lighten-3" rowspan="2">備註</th>
                                    <th class="grey-bg lighten-3" rowspan="2"></th>
                                </tr>
                                <tr>
                                    <th class="grey-bg lighten-3">客戶/單號</th>
                                    <th class="grey-bg lighten-3">單據日</th>
                                    <th class="grey-bg lighten-3">發貨日期</th>
                                    <th class="grey-bg lighten-3">銷貨金額</th>
                                    <th class="grey-bg lighten-3 center aligned">物流途徑</th>
                                    <th class="grey-bg lighten-3">貨運公司</th>
                                    <th class="grey-bg lighten-3">物流單號</th>
                                    <th class="grey-bg lighten-3">收貨人</th>
                                    <th class="grey-bg lighten-3 center aligned">出貨地</th>
                                    <th class="grey-bg lighten-3 center aligned">件數</th>
                                    <th class="grey-bg lighten-3 center aligned">到付$</th>
                                    <th class="grey-bg lighten-3 center aligned">自付$</th>
                                    <th class="grey-bg lighten-3 center aligned">墊付$</th>
                                    <!--平均運費-->
                                    <th class="grey-bg lighten-2 center aligned">到付$</th>
                                    <th class="grey-bg lighten-2 center aligned">自付$</th>
                                    <th class="grey-bg lighten-2 center aligned">墊付$</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                        </table>
                    </div>

                    <!-- List Pagination Start -->
                    <div class="ui mini bottom attached segment grey-bg lighten-4">
                        <asp:Literal ID="lt_Pager" runat="server"></asp:Literal>
                    </div>
                    <!-- List Pagination End -->
                </LayoutTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <p><%#Eval("CustName") %></p>
                            <strong class="green-text text-darken-3"><%#Eval("Erp_SO_FID") %>-<%#Eval("Erp_SO_SID") %></strong>
                        </td>
                        <td><%#Eval("Erp_SO_Date") %></td>
                        <td><%#Eval("ShipDate") %></td>
                        <td class="right aligned red-text text-darken-1">
                            <%#Eval("TotalPrice").ToString().ToMoneyString() %>
                        </td>
                        <td class="center aligned">
                            <asp:Literal ID="lt_ShipWay" runat="server"></asp:Literal>
                        </td>
                        <td><%#Eval("ShipCompName") %></td>
                        <td><%#Eval("ShipNo") %></td>
                        <td><%#Eval("ShipWho") %></td>
                        <td class="center aligned"><%#Eval("StockName") %></td>
                        <td class="center aligned"><%#showNumber(Eval("ShipCnt")) %></td>
                        <td class="center aligned"><%#showNumber(Eval("Pay1")) %></td>
                        <td class="center aligned"><%#showNumber(Eval("Pay2")) %></td>
                        <td class="center aligned"><%#showNumber(Eval("Pay3")) %></td>
                        <td class="center aligned"><%#showNumber(Eval("AvgPay1")) %></td>
                        <td class="center aligned"><%#showNumber(Eval("AvgPay2")) %></td>
                        <td class="center aligned"><%#showNumber(Eval("AvgPay3")) %></td>
                        <td><%#Eval("Remark") %></td>
                        <td class="center aligned collapsing">
                            <a class="ui small teal basic icon button" href="<%=FuncPath() %>/Edit/<%#Eval("Data_ID") %>?erpNo=<%#"{0}{1}".FormatThis(Eval("Erp_SO_FID").ToString(),Eval("Erp_SO_SID").ToString()) %>">
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
            $(".myContentBody").keypress(function (e) {
                code = (e.keyCode ? e.keyCode : e.which);
                if (code == 13) {
                    $("#doSearch").trigger("click");
                    //避免觸發submit
                    e.preventDefault();
                }
            });

            //[搜尋][查詢鈕] - 觸發查詢
            $("#doSearch").click(function () {
                $("#MainContent_btn_Search").trigger("click");
            });

            //init dropdown list
            $('select').dropdown();
            //init checkbox
            $('.ui.checkbox').checkbox();

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

