<%@ Page Title="XXOOXX資料列表" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search_v1.aspx.cs" Inherits="myDemo_Search" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">

                <div class="ui breadcrumb">
                    <a class="section">功能ROOT</a>
                    <i class="right angle icon divider"></i>
                    <div class="active section">功能資料列表</div>
                </div>
            </div>
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
        <div class="ui orange attached segment">
            <div class="ui small form">
                <div class="fields">
                    <div class="five wide field">
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
                    <div class="three wide field">
                        <label>流程公司別</label>
                        <select id="filter_Comp">
                            <option value="">-- 全部 --</option>
                            <option value="A">台灣</option>
                            <option value="B">上海</option>
                            <option value="C">三角</option>
                        </select>
                    </div>
                    <div class="four wide field">
                        <label>客戶</label>
                        <div class="ui fluid search ac-Cust">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Cust" runat="server" CssClass="prompt" placeholder="輸入客戶代號或名稱關鍵字"></asp:TextBox>
                               <asp:Label ID="lb_Cust" runat="server" CssClass="ui label">輸入關鍵字,選擇項目</asp:Label>
                            </div>
                        </div>
                        <asp:TextBox ID="val_Cust" runat="server" Style="display: none"></asp:TextBox>
                    </div>
                    <div class="inline fields">
                        <label></label>
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
                <div class="three fields">
                    <div class="field">
                        <label>品號</label>
                        <div class="ui fluid search ac-ModelNo">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_ModelNo" runat="server" CssClass="prompt" placeholder="輸入品號關鍵字"></asp:TextBox>
                                <asp:Label ID="lb_ModelNo" runat="server" CssClass="ui label">輸入關鍵字,選擇項目</asp:Label>
                            </div>
                            <asp:TextBox ID="val_ModelNo" runat="server" Style="display: none"></asp:TextBox>
                        </div>
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
                    <div class="field">
                        <label>員工</label>
                        <div class="ui fluid search ac-Employee">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Emp" runat="server" CssClass="prompt" placeholder="輸入關鍵字"></asp:TextBox>
                                <asp:Panel ID="lb_Emp" runat="server" CssClass="ui label">輸入關鍵字,選擇項目</asp:Panel>
                            </div>
                            <asp:TextBox ID="val_Emp" runat="server" Style="display: none"></asp:TextBox>
                        </div>
                    </div>
                </div>

                <div class="three fields">
                    <div class="field">
                        <label>多選</label>
                        <select id="multi-select" class="ui fluid search dropdown" multiple="">
                            <option value="">State</option>
                            <option value="AL">Alabama</option>
                            <option value="AK">Alaska</option>
                            <option value="AZ">Arizona</option>
                            <option value="AR">Arkansas</option>
                            <option value="CA">California</option>
                            <option value="CO">Colorado</option>
                            <option value="CT">Connecticut</option>
                            <option value="DE">Delaware</option>

                        </select>
                    </div>
                    <div class="field">

                        <label>選單式Search UI</label>
                        <select class="ac-drpCust ui fluid search selection dropdown">
                            <option value="">請選擇</option>
                        </select>
                        <asp:TextBox ID="val_CustID" runat="server" ToolTip="實際儲存值" Style="display: block;"></asp:TextBox>
                        <asp:TextBox ID="val_CustName" runat="server" ToolTip="設定元件的Text用" Style="display: block;"></asp:TextBox>
                    </div>
                    <div class="field">
                    </div>
                </div>

            </div>
            <div class="ui two column grid">
                <div class="column">
                    <a href="Search.aspx" class="ui small button"><i class="refresh icon"></i>重置條件</a>
                </div>
                <div class="column right aligned">
                    <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                    <asp:Button ID="btn_Search" runat="server" Text="Button" OnClick="btn_Search_Click" Style="display: none" />
                </div>
            </div>

        </div>
        <!-- Advance Search End -->

        <!-- Empty Content Start -->
        <%--<div class="ui placeholder segment">
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
        </div>--%>
        <!-- Empty Content End -->

        <!-- List Content Start -->
        <div class="ui green attached segment">
            <table class="ui celled selectable compact small table">
                <thead>
                    <tr>
                        <th class="grey-bg lighten-3 center aligned">追蹤編號</th>
                        <th class="grey-bg lighten-3">主旨</th>
                        <th class="grey-bg lighten-3 center aligned">狀態</th>
                        <th class="grey-bg lighten-3 center aligned">緊急度</th>
                        <th class="grey-bg lighten-3 center aligned">類別</th>
                        <th class="grey-bg lighten-3 center aligned">登記日期</th>
                        <th class="grey-bg lighten-3">需求者</th>
                        <th class="grey-bg lighten-3 center aligned">時間</th>
                        <th class="grey-bg lighten-3 center aligned">結案日</th>
                        <th class="grey-bg lighten-3"></th>
                    </tr>
                </thead>
                <tbody>
                    <%for (int row = 0; row < 2; row++)
                        { %>
                    <tr>
                        <td class="center aligned collapsing">
                            <a href="#view"><strong>1902151002</strong></a>
                        </td>
                        <td>
                            <strong>SS-969說明書修改-說明SLP代表意義</strong>
                        </td>
                        <td class="center aligned collapsing">
                            <div class="ui orange label">未處理</div>
                            <%-- <div class="ui yellow label">派案中</div>
                            <div class="ui blue label">處理中</div>
                            <div class="ui green basic label">已結案</div>
                            <div class="ui red large label">逾期</div>--%>
                        </td>
                        <td class="center aligned">一般件
                        </td>
                        <td class="center aligned">工具 / 舊包材修改
                        </td>
                        <td class="center aligned collapsing">2019/01/25
                        </td>
                        <td>10255<br />
                            資訊部-高得桂
                        </td>
                        <td class="center aligned collapsing">
                            <div>
                                <div class="ui basic label">
                                    希望完成
                                <div class="detail">2019/02/25</div>
                                </div>
                            </div>
                            <div>
                                <div class="ui basic label">
                                    預計完成
                                <div class="detail">2019/03/03</div>
                                </div>
                            </div>
                        </td>
                        <td class="center aligned positive">2019/03/03
                        </td>
                        <td class="center aligned collapsing">
                            <button type="button" class="ui small teal basic icon button">
                                <i class="pencil alternate icon"></i>
                            </button>
                            <button type="button" class="ui small orange basic icon button">
                                <i class="trash alternate icon"></i>
                            </button>
                        </td>
                    </tr>
                    <%} %>

                    <tr class="negative">
                        <td class="center aligned collapsing">
                            <a href="#view"><strong>1902151024</strong></a>
                        </td>
                        <td>
                            <strong>CP-376N 更新產品中心照片,卡片更新.</strong>
                        </td>
                        <td class="center aligned collapsing">
                            <div class="ui blue label">處理中</div>
                            <div class="ui red large label">逾期</div>
                        </td>
                        <td class="center aligned">急件
                        </td>
                        <td class="center aligned">工具 / 行銷活動文宣
                        </td>
                        <td class="center aligned collapsing">2019/01/27
                        </td>
                        <td>10255<br />
                            資訊部-高得桂
                        </td>
                        <td class="center aligned collapsing">
                            <div>
                                <div class="ui basic label">
                                    希望完成
                                <div class="detail">2019/02/14</div>
                                </div>
                            </div>
                            <div>
                                <div class="ui basic label">
                                    預計完成
                                <div class="detail">2019/02/15</div>
                                </div>
                            </div>
                        </td>
                        <td class="center aligned positive">2019/03/03
                        </td>
                        <td class="center aligned collapsing">
                            <button type="button" class="ui small teal basic icon button">
                                <i class="pencil alternate icon"></i>
                            </button>
                            <button type="button" class="ui small orange basic icon button">
                                <i class="trash alternate icon"></i>
                            </button>
                        </td>
                    </tr>

                    <tr>
                        <td class="center aligned collapsing">
                            <a href="#view"><strong>1902100112</strong></a>
                        </td>
                        <td>
                            <strong>EC 包材上傳900-098-BK / 902-499 / 902-035(Prop65 更新)</strong>
                        </td>
                        <td class="center aligned collapsing">
                            <div class="ui green basic label">已結案</div>
                        </td>
                        <td class="center aligned">急件
                        </td>
                        <td class="center aligned">工具 / 行銷活動文宣
                        </td>
                        <td class="center aligned collapsing">2019/01/27
                        </td>
                        <td>10255<br />
                            資訊部-高得桂
                        </td>
                        <td class="center aligned collapsing">
                            <div>
                                <div class="ui basic label">
                                    希望完成
                                <div class="detail">2019/02/14</div>
                                </div>
                            </div>
                            <div>
                                <div class="ui basic label">
                                    預計完成
                                <div class="detail">2019/02/15</div>
                                </div>
                            </div>
                        </td>
                        <td class="center aligned positive">2019/03/03
                        </td>
                        <td class="center aligned collapsing">view
                        </td>
                    </tr>
                </tbody>
            </table>


        </div>
        <!-- List Content End -->

        <!-- List Pagination Start -->
        <div class="ui mini bottom attached segment grey-bg lighten-4">
            <asp:Literal ID="lt_Pager" runat="server"></asp:Literal>
        </div>
        <!-- List Pagination End -->
        <!-- remote Modal Start -->
        <div id="detailDT" class="ui fullscreen modal">
            <i class="close icon"></i>
            <div class="header">延遲出貨通知</div>
            <div class="scrolling content">
                <table class="ui striped compact table">
                    <thead>
                        <tr>
                            <th>品號</th>
                            <th>預交日</th>
                            <th>延遲原因</th>
                            <th>出貨指示</th>
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
            //init dropdown list
            $('select').dropdown();

            //init checkbox
            $('.ui.checkbox').checkbox();

            //init tooltip
            $('.ui.popups').popup({
                inline: true,
                hoverable: true
            });

            //test modal
            $(".detail").on("click", function () {
                //var id = $(this).attr("data-id");
                var id = 1;

                //load html
                var url = '<%=fn_Param.WebUrl%>' + "_demo/Ashx_GetData.ashx?CompID=TW&OpcsNo=" + id;
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


            //[搜尋][查詢鈕] - 觸發查詢
            $("#doSearch").click(function () {
                //取得複選清單的值
                var drpValue = $("#multi-select").dropdown("get value");
                console.log(drpValue);

                //$("#MainContent_btn_Search").trigger("click");
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
    </script>
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
                            description: item.Label + ' (' + item.Email + ')',
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
                //console.log("所屬部門:" + result.deptID);
                $("#MainContent_val_Emp").val(result.title);
                $("#MainContent_lb_Emp").text(result.description);

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
                $("#MainContent_val_Cust").val(result.ID);
                $("#MainContent_lb_Cust").text(result.Label);
            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?q={query}'
            }

        });
    </script>
    <script>
        /*
          客戶 -dropdown
          注意事項:
          需使用Html Controller, 不能使用 .Net元件
          , 因選項會變動, 會被視為安全性漏洞, 所以要用另一個ServerSide元件接收值
        */
        $('.ac-drpCust').dropdown({
            fields: {
                remoteValues: 'results',
                name: 'FullLabel',
                value: 'ID'
            },
            onChange: function (value, text, $selectedItem) {
                // custom action
                $("#MainContent_val_CustID").val(value);
            },
            apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?q={query}&v=1.1'
            }

        });


    $(function () {
        //帶入填寫值
        var _custVal = $("#MainContent_val_CustID").val();
        var _custText = $("#MainContent_val_CustName").val();
        $('.ac-drpCust').dropdown("set value", _custVal).dropdown("set text", _custText);
    });

    </script>
    <%-- Search UI End --%>
</asp:Content>

