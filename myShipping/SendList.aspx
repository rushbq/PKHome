<%@ Page Title="發貨資料傳送" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SendList.aspx.cs" Inherits="myShipping_SendList" %>

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
                        發貨資料傳送
                    </div>
                </div>
            </div>
            <div class="right menu">
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
                    <div class="four wide field">
                        <label>物流途徑</label>
                        <asp:DropDownList ID="filter_ShipWay" runat="server">
                            <asp:ListItem Value="ALL">-- 全部 --</asp:ListItem>
                            <asp:ListItem Value="A">客戶自提</asp:ListItem>
                            <asp:ListItem Value="B">自行送貨</asp:ListItem>
                            <asp:ListItem Value="C">其它</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="four wide field">
                        <label>出貨地</label>
                        <asp:DropDownList ID="filter_ShipFrom" runat="server">
                            <asp:ListItem Value="ALL">-- 全部 --</asp:ListItem>
                            <asp:ListItem Value="SH">上海</asp:ListItem>
                            <asp:ListItem Value="SZ">深圳</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                    </div>
                </div>
            </div>
            <div class="ui two column grid">
                <div class="column">
                    <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i>重置條件</a>
                </div>
                <div class="column right aligned">
                    <button type="button" id="doSend" class="ui orange small button"><i class="envelope icon"></i>發送</button>
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
                                </tr>
                                <tr>
                                    <th class="grey-bg lighten-3">客戶</th>
                                    <th class="grey-bg lighten-3">單號</th>
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
                            <%#Eval("CustName") %>
                        </td>
                        <td><strong class="green-text text-darken-3"><%#Eval("Erp_SO_FID") %>-<%#Eval("Erp_SO_SID") %></strong></td>
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
                    </tr>
                </ItemTemplate>
            </asp:ListView>
        </asp:PlaceHolder>
        <!-- List Content End -->
    </div>

    <!-- 詢問框 Start -->
    <div id="confirmPage" class="ui small modal">
        <div class="header">
            即將發送資料&nbsp;(依部門代號取得Email)
        </div>
        <div class="content">
            <p>下一步將會以目前的查詢條件, 將發貨資料寄到以下部門Email.</p>
            <div class="ui bulleted list">
                <div class="item">深圳營業部</div>
                <div class="item">上海資材部</div>
                <div class="item">深圳業務部</div>
                <div class="item">深圳電子商務部</div>
                <div class="item">台灣營業部</div>
            </div>
        </div>
        <div class="actions">
            <div class="ui negative button">
                取消
            </div>
            <asp:LinkButton ID="lbtn_SendMail" runat="server" OnClick="lbtn_SendMail_Click" CssClass="ui positive right labeled icon button">開始傳送<i class="chevron right icon"></i></asp:LinkButton>
          
        </div>
    </div>
    <!-- 詢問框 End -->

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


            //發送MAIL(Modal)
            $("#doSend").click(function () {
                $('#confirmPage').modal('show');
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
</asp:Content>

