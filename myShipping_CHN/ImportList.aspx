<%@ Page Title="發貨-物流單轉入" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="ImportList.aspx.cs" Inherits="myShipping_ImportList" %>

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
                    <div class="active section red-text text-darken-2">
                        發貨-物流單轉入 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
            <div class="right menu">
                <a href="<%=Page_SearchUrl %>" class="item"><i class="undo icon"></i><span class="mobile hidden">返回發貨明細</span></a>
                <a href="<%=FuncPath() %>/Step1?dt=<%=Req_DataType %>" class="item"><i class="plus icon"></i><span class="mobile hidden">新增匯入</span></a>

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
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>

                    </div>
                    <div class="ten wide field">
                        <label>&nbsp;</label>
                        <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                        <asp:Button ID="btn_Search" runat="server" Text="Button" OnClick="btn_Search_Click" Style="display: none" />
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
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3">追蹤編號</th>
                                    <th class="grey-bg lighten-3 center aligned">狀態</th>
                                    <th class="grey-bg lighten-3 center aligned">單據日期</th>
                                    <th class="grey-bg lighten-3 center aligned">時間</th>
                                    <th class="grey-bg lighten-3 center aligned">人員</th>
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
                        <td class="left aligned collapsing">
                            <b class="red-text text-darken-2"><%#Eval("TraceID") %></b>
                        </td>
                        <td class="center aligned collapsing">
                            <div class="ui green basic fluid label">
                                <%#Eval("StatusName") %>
                            </div>
                        </td>
                        <td class="center aligned collapsing">
                            <%#Eval("erpSDate") %> ~ <%#Eval("erpEDate") %>
                        </td>
                        <td class="left aligned collapsing">
                            <div>
                                <div class="ui basic fluid label">
                                    建立<div class="detail"><%#Eval("Create_Time") %></div>
                                </div>
                            </div>
                            <div>
                                <div class="ui basic fluid label">
                                    更新<div class="detail"><%#Eval("Update_Time") %></div>
                                </div>
                            </div>
                        </td>
                        <td class="left aligned collapsing">
                            <div>
                                <div class="ui basic fluid label">
                                    建立<div class="detail"><%#Eval("Create_Name") %></div>
                                </div>
                            </div>
                            <div>
                                <div class="ui basic fluid label">
                                    更新<div class="detail"><%#Eval("Update_Name") %></div>
                                </div>
                            </div>
                        </td>
                        <td class="center aligned collapsing">
                            <asp:PlaceHolder ID="ph_KeepGo" runat="server">
                                <a class="ui small teal basic icon button" href="<%#keepGoUrl(Convert.ToInt16(Eval("Status")),Eval("Data_ID").ToString()) %>" title="繼續匯入">
                                    <i class="pencil icon"></i>
                                </a>
                            </asp:PlaceHolder>
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

