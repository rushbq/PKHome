<%@ Page Title="XXOOXX資料列表" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myDemo_Search" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui transparent icon input popups" data-content="查詢範圍:品號/品名" data-variation="tiny inverted">
                    <input class="prompt" placeholder="關鍵字查詢..." type="text" autocomplete="off" />
                    <i class="search link icon"></i>
                </div>
            </div>
            <a class="item showAdvSearch">
                <i class="filter icon"></i>
                <span class="mobile hidden">進階查詢</span>
            </a>
            <div class="right menu">
                <a class="item">
                    <i class="file excel icon"></i>
                    <span class="mobile hidden">匯出</span>
                </a>
                <a class="item">
                    <i class="plus icon"></i>
                    <span class="mobile hidden">新增</span>
                </a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Advance Search Start -->
        <div id="advSearch" class="ui orange attached segment transition hidden">
            <div class="ui small form">
                <div class="fields">
                    <div class="eight wide field">
                        <label>發延日期</label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_sDate" placeholder="開始日" autocomplete="off" value="" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_eDate" placeholder="結束日" autocomplete="off" value="" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="four wide field">
                        <label>流程公司別</label>
                        <select id="filter_Comp">
                            <option value="">-- 全部 --</option>
                            <option value="A">台灣</option>
                            <option value="B">上海</option>
                            <option value="C">三角</option>
                        </select>
                    </div>
                    <div class="four wide field">
                        <label>延遲原因</label>
                        <select id="filter_Reason">
                            <option value="">-- 全部 --</option>
                            <option value="A">採購端</option>
                            <option value="B">業務端</option>
                        </select>
                    </div>
                </div>
                <div class="four fields">
                    <div class="field">
                        <label>供應商</label>
                        <input type="text" id="filter_Supplier" placeholder="輸入供應商代號或名稱關鍵字" maxlength="20" autocomplete="off" />
                    </div>
                    <div class="field">
                        <label>品號</label>
                        <input type="text" id="filter_ModelNo" placeholder="輸入品號關鍵字" maxlength="20" autocomplete="off" />
                    </div>
                    <div class="field">
                        <label>客戶</label>
                        <input type="text" id="filter_Cust" placeholder="輸入客戶代號或名稱關鍵字" maxlength="20" autocomplete="off" />
                    </div>
                    <div class="field">
                        <label>OPCS單號</label>
                        <input type="text" id="filter_OpcsNo" placeholder="輸入不含「-」的單號" maxlength="20" autocomplete="off" />
                    </div>
                </div>
            </div>
            <div class="ui two column grid">
                <div class="column">
                    <a href="Search.aspx" class="ui small button"><i class="refresh icon"></i>清除條件</a>
                </div>
                <div class="column right aligned">
                    <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                </div>
            </div>

        </div>
        <!-- Advance Search End -->

        <!-- Empty Content Start -->
        <div class="ui placeholder segment">
            <div class="ui two column stackable center aligned grid">
                <div class="ui vertical divider"><span class="mobile hidden">或</span></div>
                <div class="middle aligned row">
                    <div class="column">
                        <div class="ui icon header">
                            <i class="search icon"></i>
                            目前條件查無資料，請重新查詢。
                        </div>
                        <div class="ui orange button showAdvSearch">
                            <i class="recycle icon"></i>
                            重新查詢
                        </div>
                    </div>
                    <div class="column mobile hidden">
                        <div class="ui icon header">
                            <i class="edit icon"></i>
                            建立新資料
                        </div>
                        <div class="ui primary button">
                            <i class="plus icon"></i>
                            Create
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Empty Content End -->

        <!-- List Content Start -->
        <div class="ui green attached segment">
            <div class="ui breadcrumb">
                <a class="section">Home</a>
                <i class="right angle icon divider"></i>
                <a class="section">Registration</a>
                <i class="right angle icon divider"></i>
                <div class="active section">Personal Information</div>
            </div>
            <table class="ui celled striped selectable compact small table">
                <thead>
                    <tr>
                        <th class="grey-bg lighten-3">名稱</th>
                        <th class="grey-bg lighten-3">Registration Date</th>
                        <th class="grey-bg lighten-3">E-mail address</th>
                        <th class="grey-bg lighten-3">Premium Plan</th>
                        <th class="grey-bg lighten-3 center aligned">狀態</th>
                        <th class="grey-bg lighten-3"></th>
                    </tr>
                </thead>
                <tbody>
                    <%for (int row = 0; row < 10; row++)
                        { %>
                    <tr>
                        <td>John Lilki</td>
                        <td>September 1<%=row %>, 2013</td>
                        <td>jhlilk22@yahoo.com</td>
                        <td>No<%=row %></td>
                        <td class="center aligned positive">Approved</td>
                        <td class="center aligned collapsing">
                            <div class="ui small icon buttons">
                                <button class="ui button cyan-bg darken-1 white-text">
                                    <i class="pencil alternate icon"></i>
                                    編輯
                                </button>
                                <button class="ui red button red-bg lighten-1 white-text">
                                    <i class="trash alternate icon"></i>
                                    刪除
                                </button>
                            </div>
                        </td>
                    </tr>
                    <%} %>
                </tbody>
            </table>


        </div>
        <!-- List Content End -->

        <!-- List Pagination Start -->
        <div class="ui mini right aligned bottom attached segment grey-bg lighten-4">
            <div class="ui small pagination menu">
                <a class="icon item">
                    <i class="left chevron icon"></i>
                </a>
                <a class="item">1</a>
                <a class="item">2</a>
                <a class="item">3</a>
                <a class="item">4</a>
                <a class="icon item">
                    <i class="right chevron icon"></i>
                </a>
            </div>
        </div>
        <!-- List Pagination End -->
    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //init dropdown list
            $('select').dropdown();

            //init tooltip
            $('.ui.popups').popup({
                inline: true,
                hoverable: true
            });

            //進階查詢區show on/off
            $(".showAdvSearch").click(function () {
                $("#advSearch").transition('fly down')
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

