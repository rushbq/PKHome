<%@ Page Title="部門資料" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myDepts_Search" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">系統管理</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">部門資料
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="item" href="<%=FuncPath() %>Edit/">
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
        <div class="ui orange attached segment">
            <div class="ui small form">
                <div class="three fields">
                    <div class="field">
                        <label>區域</label>
                        <asp:DropDownList ID="filter_Area" runat="server">
                            <asp:ListItem Value="ALL">-- 全部 --</asp:ListItem>
                            <asp:ListItem Value="TW">台灣</asp:ListItem>
                            <asp:ListItem Value="SH">上海</asp:ListItem>
                            <asp:ListItem Value="SZ">深圳</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="field">
                        <label>部門</label>
                        <div class="ui fluid search ac-Dept">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Dept" runat="server" CssClass="prompt" placeholder="輸入關鍵字"></asp:TextBox>
                                <asp:Panel ID="lb_Dept" runat="server" CssClass="ui label">輸入關鍵字,選擇項目</asp:Panel>
                            </div>
                            <asp:TextBox ID="val_Dept" runat="server" Style="display: none"></asp:TextBox>
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
            <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand">
                <LayoutTemplate>
                    <div class="ui green attached segment">
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3 center aligned">區域</th>
                                    <th class="grey-bg lighten-3 center aligned" colspan="3">部門</th>
                                    <th class="grey-bg lighten-3 center aligned">ERP ID</th>
                                    <th class="grey-bg lighten-3 center aligned">顯示</th>
                                    <th class="grey-bg lighten-3">Email</th>
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
                        <td class="center aligned"><%#Eval("GroupName") %></td>
                        <td class="center aligned green-text text-darken-2">
                            <h5><%#Eval("DeptID") %></h5>
                        </td>
                        <td class="left aligned blue-text text-darken-2">
                            <h5><%#Eval("DeptName") %></h5>
                        </td>
                        <td class="center aligned collapsing">
                            <a href="<%=FuncPath() %>/Edit/<%#Eval("DeptID") %>#section2" class="ui small basic icon button" target="_blank" title="查看部門成員"><i class="users icon"></i></a>
                        </td>
                        <td class="center aligned grey-text text-darken-1"><%#Eval("ERP_DeptID") %></td>
                        <td class="center aligned positive"><%#Eval("Display") %></td>
                        <td><%#Eval("Email") %></td>
                        <td class="center aligned collapsing">
                            <a class="ui small teal basic icon button" href="<%=FuncPath() %>/Edit/<%#Eval("DeptID") %>" title="編輯">
                                <i class="pencil icon"></i>
                            </a>
                            <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                            <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("DeptID") %>' />
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
            //[搜尋][查詢鈕] - 觸發查詢
            $("#doSearch").click(function () {
                $("#MainContent_btn_Search").trigger("click");
            });

            //init dropdown list
            $('select').dropdown();

        });
    </script>
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
                $("#MainContent_val_Dept").val(result.deptID);
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
                            title: item.ID + ' - ' + item.Label,
                            description: item.Email,
                            email: item.Email,
                            deptID: item.ID,
                            deptName: item.Label
                        });
                    });
                    return response;
                }
            }

        });
    </script>
    <%-- Search UI End --%>
</asp:Content>

