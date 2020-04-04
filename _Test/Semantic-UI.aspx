<%@ Page Title="Demo" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Semantic-UI.aspx.cs" Inherits="SemanticUI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
    <style>
        /* 自訂table
        */
        #myListTable {
            width: 100% !important;
        }

        table .colorHead th {
            background-color: #26a69a !important;
            color: #ffffff !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- breadcrumb Start -->
    <div class="ui vertical segment" style="background-color: #eee; padding-left: 15px;">
        <div class="ui breadcrumb">
            <div class="section">Demo</div>
        </div>
    </div>
    <!-- breadcrumb End -->

    <div class="myContainer">
        <!-- filter section Start -->
        <div class="ui top attached blue segment">
            <div class="ui two column grid">
                <div class="column">
                    <div class="ui small blue header">條件篩選</div>
                </div>
                <div class="column right aligned">
                    <button type="button" class="doToggle tiny ui icon blue button" data-target="section-filter" title="展開/收合"><i class="filter icon"></i></button>
                </div>
            </div>
        </div>
        <div class="section-filter ui attached segment">
            <div class="ui form">
                <div class="fields">
                    <div class="eight wide field">
                        <label>日期</label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_sDate" placeholder="開始日" autocomplete="off" value="<%=sDate %>" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <input type="text" id="filter_eDate" placeholder="結束日" autocomplete="off" value="<%=eDate %>" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="four wide field">
                        <label>公司</label>
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
                  
                </div>
                <div class="fields">
                    <div class="eight wide field">
                        <!-- Search UI -->
                        <div class="ui fluid search ac-Search">
                            <div class="ui left icon input">
                                <input class="prompt" type="text" placeholder="Search keword" />
                                <i class="search icon"></i>
                            </div>
                        </div>
                        <input id="val-search" type="text" readonly="readonly" />
                    </div>
                    <div class="eight wide field">
                        <select id="category" class="ui selection dropdown">
                            <option disabled>Category 1</option>
                            <option value="1">&nbsp;&nbsp; - Menu 1</option>
                            <option value="2">&nbsp;&nbsp; - Menu 2</option>
                            <option disabled>Category 2</option>
                            <option value="3">&nbsp;&nbsp; - Menu 3</option>
                            <option value="4">&nbsp;&nbsp; - Menu 4</option>
                        </select>
                    </div>

                </div>
            </div>
        </div>

        <!-- filter section End -->


    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //載入radio選單
            $('.ui.radio.checkbox').checkbox();
            $('select.dropdown').dropdown();

            //Search UI(ajax)
            $('.ac-Search').search({
                type: 'category',
                minCharacters: 2,
                searchFields: [
                    'title',
                    'description'
                ]
                , onSelect: function (result, response) {
                    //console.log(result.title);
                    $("#val-search").val(result.title);
                }
                , apiSettings: {
                    url: '<%=fn_Param.WebUrl%>_Test/GetData_Prod.ashx?q={query}',
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

                            //重組回傳結果(填入群組欄位)
                            response.results[categoryContent].results.push({
                                title: item.ID,
                                description: item.Label
                            });
                        });
                        return response;
                    }
                }

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

