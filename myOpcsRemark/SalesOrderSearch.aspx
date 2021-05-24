<%@ Page Title="訂單備註" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SalesOrderSearch.aspx.cs" Inherits="SalesOrderSearch" %>

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
                    <div class="section">訂單作業</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">訂單備註 (<%:Req_DBS %>)
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="item" href="<%=GetHistoryUrl(Req_DBS) %>" target="_blank">
                    <i class="list icon"></i>
                    <span class="mobile hidden">歷史記錄</span>
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
                    <div class="five wide field">
                        <label>訂單日期</label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_sDate" runat="server" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_eDate" runat="server" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="two wide field">
                        <label>單身結案碼</label>
                        <asp:DropDownList ID="filter_IsClose" runat="server" CssClass="fluid">
                            <asp:ListItem Value="N" Selected="True">未結案</asp:ListItem>
                            <asp:ListItem Value="Y">已結案</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="two wide field">
                        <label>確認碼</label>
                        <asp:DropDownList ID="filter_CfmCode" runat="server" CssClass="fluid">
                            <asp:ListItem Value="">- 不限 -</asp:ListItem>
                            <asp:ListItem Value="N">N:未確認</asp:ListItem>
                            <asp:ListItem Value="Y">Y:已確認</asp:ListItem>
                            <asp:ListItem Value="V">V:作廢</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="four wide field">
                        <label>關鍵字</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" MaxLength="20" autocomplete="off" placeholder="單別單號關鍵字"></asp:TextBox>
                    </div>
                    <div class="four wide field" style="text-align: right;">
                        <label>&nbsp;</label>
                        <a href="<%=thisPage %>?dbs=<%:Req_DBS %>" class="ui small button"><i class="refresh icon"></i>重置</a>
                        <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                        <asp:Button ID="btn_Search" runat="server" Text="search" OnClick="btn_Search_Click" Style="display: none" />
                    </div>
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
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound" OnItemCommand="lvDataList_ItemCommand">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3">&nbsp;</th>
                                    <th class="grey-bg lighten-3 center aligned">單別</th>
                                    <th class="grey-bg lighten-3 center aligned">單號</th>
                                    <th class="grey-bg lighten-3 center aligned">訂單日</th>
                                    <th class="grey-bg lighten-3 center aligned">預交日</th>
                                    <th class="grey-bg lighten-3 center aligned">客戶名</th>
                                    <th class="grey-bg lighten-3 center aligned">幣別</th>
                                    <th class="grey-bg lighten-3 center aligned">匯率</th>
                                    <th class="grey-bg lighten-3">價格條件</th>
                                    <th class="grey-bg lighten-3">付款條件</th>
                                    <th class="grey-bg lighten-3 center aligned">業務</th>
                                    <th class="grey-bg lighten-3 center aligned">確認碼</th>
                                    <th class="grey-bg lighten-3 center aligned">簽核狀態碼</th>
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
                        <td class="left aligned collapsing">
                            <!-- edit -->
                            <asp:PlaceHolder ID="ph_Edit" runat="server">
                                <a class="ui small teal basic icon button" href="<%=FuncPath() %>SalesOrderEdit.aspx?dbs=<%=Req_DBS %>&id=<%#Eval("Data_ID") %>&Fid=<%#Eval("SO_Fid") %>&Sid=<%#Eval("SO_Sid") %>" title="編輯">
                                    <i class="pencil icon"></i>
                                </a>
                            </asp:PlaceHolder>

                            <!-- excel -->
                            <asp:PlaceHolder ID="ph_Excel" runat="server">
                                <a class="ui small green basic icon button" href="<%=FuncPath() %>OPCS_form.aspx?dbs=<%=Req_DBS %>&id=<%#Eval("SO_Fid") %><%#Eval("SO_Sid") %>" target="_blank" title="一般訂單">
                                    <i class="file excel icon"></i>
                                </a>
                            </asp:PlaceHolder>
                            <!-- view remark -->
                            <asp:PlaceHolder ID="ph_RemarkSection" runat="server">
                                <a class="showPopup" title="點擊放大看備註" data-id="remk1_<%#Eval("Data_ID") %>">
                                    <asp:Label ID="lb_showMark" runat="server" CssClass="ui small grey basic icon button"><i class="file alternate icon"></i></asp:Label>
                                </a>

                                <asp:PlaceHolder ID="ph_Modal_r1" runat="server">
                                    <!-- Modal Start -->
                                    <div id="remk1_<%#Eval("Data_ID") %>" class="ui modal">
                                        <div class="header"><%#Eval("SO_FID") %>-<%#Eval("SO_SID") %>&nbsp;訂單備註</div>
                                        <div class="scrolling content"><%# Eval("Remk_Normal").ToString().Replace("\n","<br>") %></div>
                                        <div class="actions">
                                            <div class="ui black deny button">
                                                Close
                                            </div>
                                        </div>
                                    </div>
                                    <!--  Modal End -->
                                </asp:PlaceHolder>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="ph_PDF" runat="server">
                                <a class="ui small red basic icon button btn-OpenDetail" data-id="<%#Eval("SO_FID") %><%#Eval("SO_SID") %>" data-title="<%#Eval("SO_FID") %>-<%#Eval("SO_SID") %>" title="變更單">
                                    <i class="file pdf outline icon"></i>
                                </a>
                            </asp:PlaceHolder>
                        </td>
                        <td class="center aligned orange-text text-darken-4">
                            <strong><%#Eval("SO_FID") %></strong>
                        </td>
                        <td class="center aligned orange-text text-darken-4">
                            <strong><%#Eval("SO_SID") %></strong>
                        </td>
                        <td class="center aligned">
                            <%#Eval("SO_Date").ToString().ToDateString_ERP("/") %>
                        </td>
                        <td class="center aligned">
                            <%#Eval("PreDate").ToString().ToDateString_ERP("/") %>
                        </td>
                        <td class="center aligned">
                            <b class="green-text text-darken-2"><%#Eval("CustName") %></b>
                        </td>
                        <td class="center aligned">
                            <%#Eval("TradeCurrency") %>
                        </td>
                        <td class="center aligned">
                            <%#Math.Round(Convert.ToDecimal(Eval("Rate")), 2) %>
                        </td>
                        <td>
                            <%#Eval("TradeTerm") %>
                        </td>
                        <td>
                            <%#Eval("PayTerm") %>
                        </td>
                        <td class="center aligned collapsing">
                            <%#Eval("SalesWho") %>
                        </td>
                        <td class="center aligned">
                            <strong><%#Eval("CfmCode") %></strong>
                        </td>
                        <td class="center aligned collapsing">
                            <%#fn_Menu.GetERP_FlowStatus(Eval("FlowStatus").ToString()) %>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>
        </asp:PlaceHolder>
        <!-- List Content End -->

    </div>
    <!-- 內容 End -->
    <!-- Opcs Modal Start -->
    <div id="opcsDT" class="ui small modal">
        <div class="header">
            OPCS變更單&nbsp;<small>(訂單單號:<span class="grey-text text-darken-1" id="titleSoID"></span>)</small>
        </div>
        <div class="scrolling content">
            <table class="ui striped celled table">
                <thead>
                    <tr>
                        <th class="center aligned">單別-單號</th>
                        <th class="center aligned">版次</th>
                        <th class="center aligned">PDF</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
            <div class="red-text text-darken-1" style="text-align: right;"><b>PDF產生需要一點時間，請不要重複按下載。</b></div>
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
            //init dropdown list
            $('select').dropdown();


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


            //Modal-備註
            $(".showPopup").on("click", function () {
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

        });
    </script>
    <%-- Modal-載入Opcs單號 --%>
    <script>
        $(function () {
            $(".btn-OpenDetail").on("click", function () {
                var id = $(this).attr("data-id");
                var dbs = '<%=Req_DBS%>';
                var title = $(this).attr("data-title");

                console.log(id);

                $("#titleSoID").text(title);

                //load html
                var url = '<%=fn_Param.WebUrl%>' + "myOpcsRemark/Ashx_GetOpcsUpdData.ashx?DBS=" + dbs + "&SoID=" + id;
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
            $('.datepicker').calendar(calendarOpts_Range_unLimit);
        });
    </script>
    <%-- 日期選擇器 End --%>
</asp:Content>

