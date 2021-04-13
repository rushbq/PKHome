<%@ Page Title="需求查詢頁" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="MGHelp_Search.aspx.cs" Inherits="MGHelp_Search" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">行政管理</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">工作需求登記
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <div class="ui right pointing red basic label">新增需求,按這兒</div>
                <a href="MGHelp_Edit.aspx" class="item"><i class="plus icon"></i><span class="mobile hidden">新增需求</span></a>
                <asp:LinkButton ID="lbtn_Export" runat="server" CssClass="item" OnClick="btn_Excel_Click"><i class="file excel icon"></i><span class="mobile hidden">匯出</span></asp:LinkButton>
                <%--<a href="<%=fn_Param.WebUrl %>Recording/MG_HelpSummary.aspx" class="item"><i class="chart bar icon"></i><span class="mobile hidden">統計</span></a>--%>
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
                    <div class="two wide field">
                        <label>日期區間</label>
                        <asp:DropDownList ID="filter_dateType" runat="server" CssClass="fluid">
                            <asp:ListItem Value="A">登記日</asp:ListItem>
                            <asp:ListItem Value="B">結案日</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="five wide field">
                        <div class="two fields">
                            <div class="field">
                                <label>&nbsp;</label>
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_sDate" runat="server" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <label>&nbsp;</label>
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_eDate" runat="server" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="three wide field">
                        <label>需求類別</label>
                        <asp:DropDownList ID="filter_ReqClass" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="two wide field">
                        <label>處理狀態</label>
                        <asp:DropDownList ID="filter_ReqStatus" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="four wide field">
                        <label>關鍵字</label>
                        <asp:TextBox ID="filter_Keyword" runat="server" MaxLength="20" autocomplete="off" placeholder="追蹤編號, 主旨"></asp:TextBox>
                    </div>
                </div>

                <div class="fields">
                    <div class="three wide field">
                        <div class="ui fluid search ac-Dept">
                            <div class="ui left labeled input">
                                <asp:Panel ID="lb_Dept" runat="server" CssClass="ui label">需求部門</asp:Panel>
                                <asp:TextBox ID="filter_Dept" runat="server" CssClass="prompt" placeholder="查詢代號或名稱"></asp:TextBox>
                            </div>
                            <asp:TextBox ID="val_Dept" runat="server" Style="display: none"></asp:TextBox>
                        </div>
                    </div>
                    <div class="four wide field">
                        <div class="ui fluid search ac-Employee" data-label="lb_Emp" data-val="val_Emp">
                            <div class="ui left labeled input">
                                <asp:Panel ID="lb_Emp" runat="server" CssClass="ui label">需求者</asp:Panel>
                                <asp:TextBox ID="filter_Emp" runat="server" CssClass="prompt" placeholder="查詢工號,姓名,英文名"></asp:TextBox>
                            </div>
                            <asp:TextBox ID="val_Emp" runat="server" Style="display: none"></asp:TextBox>
                        </div>
                    </div>
                    <div class="five wide field">
                        <div class="ui fluid search ac-Employee" data-label="lb_FinishWho" data-val="val_FinishWho">
                            <div class="ui left labeled input">
                                <asp:Panel ID="lb_FinishWho" runat="server" CssClass="ui label">結案人</asp:Panel>
                                <asp:TextBox ID="filter_FinishWho" runat="server" CssClass="prompt" placeholder="查詢工號,姓名,英文名"></asp:TextBox>
                            </div>
                            <asp:TextBox ID="val_FinishWho" runat="server" Style="display: none"></asp:TextBox>
                        </div>
                    </div>
                    <div class="four wide field" style="text-align: right;">
                        <a href="<%=thisPage %>" class="ui small button"><i class="refresh icon"></i>重置</a>
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
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound" OnItemCommand="lvDataList_ItemCommand">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3 collapsing">系統序號</th>
                                    <th class="grey-bg lighten-3 center aligned">類別</th>
                                    <th class="grey-bg lighten-3">需求主旨</th>
                                    <th class="grey-bg lighten-3 center aligned">狀態</th>
                                    <th class="grey-bg lighten-3 center aligned collapsing">最新進度</th>
                                    <th class="grey-bg lighten-3 center aligned collapsing">預計完成</th>
                                    <th class="grey-bg lighten-3 center aligned">需求者</th>
                                    <th class="grey-bg lighten-3 center aligned">時間</th>
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
                            <%#Eval("SeqNo") %>
                        </td>
                        <td class="center aligned collapsing">
                            <span class="ui teal basic fluid label"><%#Eval("Req_ClassName") %></span>
                        </td>
                        <td>
                            <div style="margin-bottom: 8px;">
                                <asp:Literal ID="lt_onTop" runat="server"></asp:Literal>
                                <span class="ui red basic small label"><%#Eval("TraceID") %></span>
                            </div>
                            <strong class="grey-text text-darken-2" style="font-size: 1.2em;"><%#Eval("Help_Subject") %></strong>
                        </td>
                        <td class="center aligned collapsing">
                            <asp:Literal ID="lt_Status" runat="server"></asp:Literal>
                        </td>
                        <td class="center aligned collapsing">
                            <%#Eval("ProcInfo").ToString().Replace("_","<BR/>") %>
                        </td>
                        <td class="center aligned collapsing">
                            <%#Eval("Wish_Time").ToString().ToDateString("yyyy/MM/dd") %>
                        </td>
                        <td class="center aligned collapsing">
                            <div class="ui basic fluid large label"><%#Eval("Req_WhoName") %></div>
                        </td>
                        <td class="left aligned collapsing">
                            <div style="margin-bottom: 1px;">
                                <div class="ui basic fluid label">
                                    登記日<div class="detail"><%#Eval("Create_Time").ToString().ToDateString("yyyy/MM/dd") %></div>
                                </div>
                            </div>
                            <div>
                                <div class="ui basic fluid label">
                                    結案日<div class="detail"><%#Eval("Finish_Time").ToString().ToDateString("yyyy/MM/dd") %></div>
                                </div>
                            </div>
                        </td>
                        <td class="center aligned collapsing">
                            <a class="ui small grey basic icon button" href="<%=FuncPath() %>MGHelp_View.aspx?id=<%#Eval("DataID") %>" title="看明細">
                                <i class="file alternate icon"></i>
                            </a>
                            <asp:PlaceHolder ID="ph_Edit" runat="server">
                                <a class="ui small teal basic icon button" href="<%=FuncPath() %>MGHelp_Edit.aspx?id=<%#Eval("DataID") %>" title="編輯">
                                    <i class="pencil icon"></i>
                                </a>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="ph_Del" runat="server">
                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                            </asp:PlaceHolder>

                            <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("DataID") %>' />
                            <asp:HiddenField ID="hf_TraceID" runat="server" Value='<%#Eval("TraceID") %>' />
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

    <%-- Search UI Start --%>
    <script>
        /* 部門 (使用category) */
        $('.ac-Dept').search({
            type: 'category',
            minCharacters: 1,
            searchFields: [
                'title',
                'description'
            ]
            , onSelect: function (result, response) {
                $("#MainContent_val_Dept").val(result.title);
                $("#MainContent_lb_Dept").text(result.description);

            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Depts.ashx?q={query}',
                onResponse: function (ajaxResp) {
                    //宣告空陣列
                    var response = {
                        results: {}
                    }
                    ;
                    // translate API response to work with search
                    /*
                      取得遠端資料後處理
                      .results = 物件名稱
                      item.Category = 要群組化的欄位
                      maxResults = 查詢回傳筆數

                    */
                    $.each(ajaxResp.results, function (index, item) {
                        var
                          categoryContent = item.Category || 'Unknown',
                          maxResults = 20
                        ;
                        if (index >= maxResults) {
                            return false;
                        }
                        // create new categoryContent category
                        if (response.results[categoryContent] === undefined) {
                            response.results[categoryContent] = {
                                name: categoryContent,
                                results: []
                            };
                        }

                        //重組回傳結果(指定顯示欄位)
                        response.results[categoryContent].results.push({
                            title: item.ID,
                            description: item.Label,
                            email: item.Email
                        });
                    });
                    return response;
                }
            }

        });
    </script>
    <script>
        /* 人員 (使用category) */
        $('.ac-Employee').search({
            type: 'category',
            minCharacters: 1,
            searchFields: [
                'title',
                'description'
            ]
            , onSelect: function (result, response) {
                var _label = $(this).attr("data-label");
                var _val = $(this).attr("data-val");

                //儲存值
                $("#MainContent_" + _val).val(result.title);
                //顯示值
                $("#MainContent_" + _label).text(result.description);

            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Users.ashx?q={query}',
                onResponse: function (ajaxResp) {
                    //宣告空陣列
                    var response = {
                        results: {}
                    }
                    ;
                    // translate API response to work with search
                    /*
                      取得遠端資料後處理
                      .results = 物件名稱
                      item.Category = 要群組化的欄位
                      maxResults = 查詢回傳筆數

                    */
                    $.each(ajaxResp.results, function (index, item) {
                        var
                          categoryContent = item.Category || 'Unknown',
                          maxResults = 20
                        ;
                        if (index >= maxResults) {
                            return false;
                        }
                        // create new categoryContent category
                        if (response.results[categoryContent] === undefined) {
                            response.results[categoryContent] = {
                                name: categoryContent,
                                results: []
                            };
                        }

                        //重組回傳結果(指定顯示欄位)
                        response.results[categoryContent].results.push({
                            title: item.ID,
                            description: item.Label + ' (' + item.NickName + ')',
                            email: item.Email,
                            deptID: item.DeptID
                        });
                    });
                    return response;
                }
            }

        });
    </script>
    <%-- Search UI End --%>
</asp:Content>

