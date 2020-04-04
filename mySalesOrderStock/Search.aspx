<%@ Page Title="銷貨單庫存狀況" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="mySalesOrderStock_Search" %>

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
                    <h5 class="active section">
                        銷貨單庫存狀況 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
                        (未確認單據)
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <%-- <a class="item">
                    <i class="file excel icon"></i>
                    <span class="mobile hidden">匯出</span>
                </a>
                <a class="item">
                    <i class="plus icon"></i>
                    <span class="mobile hidden">新增</span>
                </a>--%>
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
                        <label>銷貨單號</label>
                        <asp:TextBox ID="filter_SoNo" runat="server" MaxLength="20" placeholder="輸入不含「-」的完整銷貨單號" autocomplete="off"></asp:TextBox>
                    </div>
                    <div class="field">
                        <label>來源平台</label>
                        <asp:DropDownList ID="filter_Where" runat="server">
                            <asp:ListItem Value="ALL">-- 全部 --</asp:ListItem>
                            <asp:ListItem Value="天貓">天貓</asp:ListItem>
                            <asp:ListItem Value="京東POP">京東POP</asp:ListItem>
                            <asp:ListItem Value="京東VC">京東VC</asp:ListItem>
                            <asp:ListItem Value="京東廠送">京東廠送</asp:ListItem>
                            <asp:ListItem Value="唯品會">唯品會</asp:ListItem>
                            <asp:ListItem Value="官網線上下單" Selected="True">官網線上下單</asp:ListItem>
                            <asp:ListItem Value="eService">eService</asp:ListItem>
                            <asp:ListItem Value="ERP打單">ERP打單</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="field">
                        <label>平台單號</label>
                        <asp:TextBox ID="filter_OrderNo" runat="server" MaxLength="20" placeholder="輸入的完整平台單號" autocomplete="off"></asp:TextBox>
                    </div>
                </div>

                <div class="two fields">
                    <div class="field">
                        <label>客戶</label>
                        <div class="ui fluid search ac-Cust">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Cust" runat="server" CssClass="prompt" placeholder="輸入客戶代號或名稱關鍵字"></asp:TextBox>
                                <asp:Panel ID="lb_Cust" runat="server" CssClass="ui label"><i class="angle left"></i>輸入關鍵字,選擇項目</asp:Panel>
                            </div>
                        </div>
                        <asp:TextBox ID="val_Cust" runat="server" Style="display: none"></asp:TextBox>
                    </div>
                    <div class="field">
                        <label>品號</label>
                        <div class="ui fluid search ac-ModelNo">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_ModelNo" runat="server" CssClass="prompt" placeholder="輸入品號關鍵字"></asp:TextBox>
                                <asp:Panel ID="lb_ModelNo" runat="server" CssClass="ui label">輸入關鍵字,選擇項目</asp:Panel>
                            </div>
                            <asp:TextBox ID="val_ModelNo" runat="server" Style="display: none"></asp:TextBox>
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
                        <table class="ui celled selectable compact small structured table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3" rowspan="2">客戶</th>
                                    <th class="grey-bg lighten-3" rowspan="2">銷貨單號</th>
                                    <th class="grey-bg lighten-3" rowspan="2">品號</th>
                                    <th class="grey-bg lighten-3 center aligned" rowspan="2">庫別</th>
                                    <th class="grey-bg lighten-3 center aligned" rowspan="2">數量</th>
                                    <th class="grey-bg lighten-3 center aligned" rowspan="2">來源</th>
                                    <%--<th class="grey-bg lighten-3" rowspan="2">平台單號</th>--%>
                                    <th class="grey-bg lighten-3 center aligned" colspan="4">A01</th>
                                    <th class="grey-bg lighten-3 center aligned" colspan="4">B01</th>
                                </tr>
                                <tr>
                                    <th class="grey-bg lighten-3 right aligned">庫存</th>
                                    <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                    <th class="grey-bg lighten-3 right aligned">預計進</th>
                                    <th class="grey-bg lighten-3 right aligned">差異</th>
                                    <th class="grey-bg lighten-3 right aligned">庫存</th>
                                    <th class="grey-bg lighten-3 right aligned">預計銷</th>
                                    <th class="grey-bg lighten-3 right aligned">預計進</th>
                                    <th class="grey-bg lighten-3 right aligned">差異</th>
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
                        <td><%#Eval("CustID") %></td>
                        <td><%#Eval("SO_Fid") %>-<%#Eval("SO_Sid") %></td>
                        <td class="green-text text-darken-1"><strong><%#Eval("ModelNo") %></strong></td>
                        <td class="center aligned"><%#Eval("StockType") %></td>
                        <td class="center aligned warning"><%#Eval("BuyCnt") %></td>
                        <td class="center aligned"><%#Eval("ShopWhere") %></td>
                        <%--<td><%#Eval("ShopOrderID") %></td>--%>
                        <td class="right aligned"><%#Eval("StockQty_A01") %></td>
                        <td class="right aligned">
                            <a href="#!" class="detailPreSell" data-id="<%#HttpUtility.UrlEncode(Eval("ModelNo").ToString()) %>" data-head="預計銷"><%#Eval("PreSell_A01") %></a>
                        </td>
                        <td class="right aligned"><%#Eval("PreIN_A01") %></td>
                        <td class="right aligned <%#formatNumberString(Eval("gapA01").ToString(),"td") %>">
                            <%#formatNumberString(Eval("gapA01").ToString(),"val") %>
                        </td>
                        <td class="right aligned"><%#Eval("StockQty_B01") %></td>
                        <td class="right aligned">
                            <a href="#!" class="detailPreSell" data-id="<%#HttpUtility.UrlEncode(Eval("ModelNo").ToString()) %>" data-head="預計銷"><%#Eval("PreSell_B01") %></a>
                        </td>
                        <td class="right aligned"><%#Eval("PreIN_B01") %></td>
                        <td class="right aligned <%#formatNumberString(Eval("gapB01").ToString(),"td") %>">
                            <%#formatNumberString(Eval("gapB01").ToString(),"val") %>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>
        </asp:PlaceHolder>
        <!-- List Content End -->

        <!-- remote Modal Start -->
        <div id="detailDT" class="ui fullscreen modal">
            <i class="close icon"></i>
            <div class="header"></div>
            <div class="scrolling content">
                <table class="ui celled striped compact table">
                    <thead>
                        <tr>
                            <th class="center aligned">庫別</th>
                            <th class="center aligned">數量</th>
                        </tr>
                    </thead>
                    <tbody></tbody>
                </table>
            </div>
            <div class="actions">
                <div class="ui close button">
                    Close
                </div>
            </div>
        </div>
        <!-- remote Modal End -->
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

            /* Modal Start */
            $(".detailPreSell").on("click", function () {
                var id = $(this).attr("data-id");
                var header = $(this).attr("data-head");
                $("#detailDT .header").text(header + ' : ' + id);

                //load html
                var url = '<%=fn_Param.WebUrl%>' + "myOrderStock/Ashx_GetPreSellList.ashx?CompID=<%=Req_CompID%>&ModelNo=" + id;
                var datablock = $("#detailDT .content tbody");
                datablock.empty();
                datablock.load(url);

                //show modal
                $('#detailDT').modal({
                    selector: {
                        close: '.close, .actions .button'
                    }
                }).modal('show');
            });
            /* Modal End */
        });
    </script>
    <%-- Search UI Start --%>
    <script>
        /* 品號 (使用category) */
        $('.ac-ModelNo').search({
            type: 'category',
            minCharacters: 1,
            searchFields: [
                'title',
                'description'
            ]
            , onSelect: function (result, response) {
                //console.log(result.title);
                $("#MainContent_val_ModelNo").val(result.title);
                $("#MainContent_lb_ModelNo").text(result.description);

            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Prod_v1.ashx?q={query}',
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
                            description: item.Label
                        });
                    });
                    return response;
                }
            }

        });


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
                $("#MainContent_val_Cust").val(result.ID);
                $("#MainContent_lb_Cust").text(result.Label);
            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?q={query}'
            }

        });
    </script>
    <%-- Search UI End --%>
</asp:Content>

