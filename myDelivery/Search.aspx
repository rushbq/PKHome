<%@ Page Title="快遞貨運登記" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myDelivery_Search" %>

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
                    <div class="section">快遞貨運登記</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        登記清單
                    </div>
                </div>
            </div>
            <div class="right menu">
                <a href="<%=FuncPath() %>/Address" class="item"><i class="user icon"></i>收件人維護</a>
                <a href="#!" class="shipHCT item"><i class="shipping icon"></i>新竹單號處理</a>
                <a href="<%=FuncPath() %>/Edit" class="item"><i class="plus icon"></i>新增登記</a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Advance Search Start -->
        <div id="section-search" class="ui orange attached segment">
            <div class="ui small form">
                <div class="fields">
                    <div class="six wide field">
                        <label>建立日期</label>
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
                    <div class="three wide field">
                        <label>寄送方式</label>
                        <asp:DropDownList ID="filter_ShipWay" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="four wide field">
                        <label>關鍵字查詢</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" autocomplete="off" placeholder="查詢:登記單號, 收件公司, 收件人, 銷貨單號, 採購單號, INVOICE" MaxLength="20"></asp:TextBox>
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
        <!-- 新竹物流Steps Start -->
        <div id="section-shipHCT" class="ui red attached segment" style="display: none;">
            <div class="ui header">
                新竹物流單號回寫&nbsp;<a href="#!" class="shipHCT ui grey small button">返回查詢</a>
            </div>
            <!-- Steps 示意 -->
            <div class="ui fluid steps">
                <div class="step">
                    <i class="file excel icon"></i>
                    <div class="content">
                        <div class="title">下載已登記單號</div>
                        <div class="description">
                            <asp:LinkButton ID="lbtn_Export1" runat="server" CssClass="ui mini green basic button" OnClick="lbtn_Export1_Click">下載新竹格式</asp:LinkButton>
                        </div>
                    </div>
                </div>
                <div class="step">
                    <i class="truck icon"></i>
                    <div class="content">
                        <div class="title">上傳至貨運公司</div>
                        <div class="description">
                            將上一步轉出的Excel,<br />
                            上傳至貨運公司
                        </div>
                    </div>
                </div>
                <div class="step">
                    <i class="clipboard list icon"></i>
                    <div class="content">
                        <div class="title">下載物流單</div>
                        <div class="description">
                            1.下載貨運公司的物流單(<a href="<%=fn_Param.RefUrl %>PKHome/Delivery/Sample-1.xlsx" target="_blank">參考範例</a>)。<br />
                            2.匯入物流單號 (登記單號為對應)
                            <div class="ui small segments">
                                <div class="ui segment">
                                    請確認資料在<b class="red-text">第一個工作表</b>, 並檢查格式是否符合規定.
                                </div>
                                <div class="ui segment">
                                    <div class="ui grid">
                                        <div class="eight wide column">
                                            <asp:FileUpload ID="fu_ShipFile" runat="server" />
                                        </div>
                                        <div class="eight wide column center aligned">
                                            <asp:LinkButton ID="lbtn_JobUpload" runat="server" CssClass="ui mini teal basic button" OnClick="lbtn_JobUpload_Click" OnClientClick="return confirm('確認執行?\n匯入後將無法還原.')">匯入單號</asp:LinkButton>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="step">
                    <i class="dollar icon"></i>
                    <div class="content">
                        <div class="title">下載運費單</div>
                        <div class="description">
                            1.下載貨運公司的運費單(<a href="<%=fn_Param.RefUrl %>PKHome/Delivery/Sample-2.xlsx" target="_blank">參考範例</a>)。<br />
                            2.匯入運費 (物流單號為對應)
                            <div class="ui small segments">
                                <div class="ui segment">
                                    請確認資料在<b class="red-text">第一個工作表</b>, 並檢查格式是否符合規定.
                                </div>
                                <div class="ui segment">
                                    <div class="ui grid">
                                        <div class="eight wide column">
                                            <asp:FileUpload ID="fu_FreightFile" runat="server" />
                                        </div>
                                        <div class="eight wide column center aligned">
                                            <asp:LinkButton ID="lbtn_FreightUpload" runat="server" CssClass="ui mini teal basic button" OnClick="lbtn_FreightUpload_Click" OnClientClick="return confirm('確認執行?\n匯入後將無法還原.')">匯入運費</asp:LinkButton>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- 新竹物流Steps End -->
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
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3 center aligned">登記單號</th>
                                    <th class="grey-bg lighten-3 center aligned">寄件類別</th>
                                    <th class="grey-bg lighten-3 center aligned">寄件日</th>
                                    <th class="grey-bg lighten-3 center aligned">寄送方式</th>
                                    <th class="grey-bg lighten-3 center aligned">付款方式</th>
                                    <th class="grey-bg lighten-3 center aligned">登記人員</th>
                                    <th class="grey-bg lighten-3 center aligned">物流單號</th>
                                    <th class="grey-bg lighten-3 right aligned">運費</th>
                                    <th class="grey-bg lighten-3 center aligned">總箱數</th>
                                    <th class="grey-bg lighten-3 center aligned">內容物分類</th>
                                    <th class="grey-bg lighten-3 center aligned">ERP資料</th>
                                    <th class="grey-bg lighten-3"></th>
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
                        <td class="center aligned">
                            <b class="red-text text-darken-2"><%#Eval("TraceID") %></b>
                        </td>
                        <td class="center aligned">
                            <%#Eval("ShipTypeName") %>
                        </td>
                        <td class="center aligned">
                            <b class="green-text text-darken-2"><%#Eval("SendDate").ToString().ToDateString("yyyy/MM/dd") %></b>
                        </td>
                        <td class="center aligned">
                            <%#Eval("ShipWayName") %>
                        </td>
                        <td class="center aligned">
                            <%#Eval("PayWayName") %>
                        </td>
                        <td class="center aligned collapsing">
                            <%#Eval("ShipWho_Name") %>
                        </td>
                        <td class="center aligned">
                            <%#Eval("ShipNo") %>
                        </td>
                        <td class="right aligned">
                            <%#Eval("ShipPay") %>
                        </td>
                        <td class="center aligned">
                            <%#Eval("Box") %>
                        </td>
                        <td class="left aligned collapsing">
                            <div>
                                <div class="ui basic fluid label">
                                    分類1<div class="detail"><%#Eval("BoxClass1Name") %></div>
                                </div>
                            </div>
                            <div>
                                <div class="ui basic fluid label">
                                    分類2<div class="detail"><%#Eval("BoxClass2Name") %></div>
                                </div>
                            </div>
                        </td>
                        <td class="left aligned collapsing">
                            <div>
                                <div class="ui basic fluid label">
                                    採購<div class="detail"><%#Eval("PurNo") %></div>
                                </div>
                            </div>
                            <div>
                                <div class="ui basic fluid label">
                                    銷貨<div class="detail"><%#Eval("SaleNo") %></div>
                                </div>
                            </div>
                        </td>
                        <td class="center aligned collapsing">
                            <a class="ui small grey basic icon button" href="<%=FuncPath() %>/View/<%#Eval("Data_ID") %>" title="查看">
                                <i class="file alternate icon"></i>
                            </a>
                            <asp:PlaceHolder ID="ph_Edit" runat="server">
                                <a class="ui small teal basic icon button" href="<%#FuncPath() %>/Edit/<%#Eval("Data_ID") %>" title="編輯">
                                    <i class="pencil icon"></i>
                                </a>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="ph_Del" runat="server">
                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                            </asp:PlaceHolder>
                            <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
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


            //
            $(".shipHCT").click(function () {
                $('#section-search').transition('slide right');
                $('#section-shipHCT').transition('slide left');

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
            $('.datepicker').calendar(calendarOptsByTime_Range);
        });
    </script>
    <%-- 日期選擇器 End --%>
</asp:Content>

