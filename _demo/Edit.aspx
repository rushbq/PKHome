<%@ Page Title="XXOOXX資料維護頁" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Edit.aspx.cs" Inherits="myDemo_Edit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
    <style>
        /*.ui.sticky.fixed{
            margin-top: 65px !important;
        }*/
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">

                <div class="ui breadcrumb">
                    <a class="section">功能ROOT</a>
                    <i class="right angle icon divider"></i>
                    <div class="active section">維護頁</div>
                </div>
            </div>
            <div class="right menu">
                <%--<a class="item">
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
        <!-- Content Start -->
        <div class="ui attached segment grey-bg lighten-5">
            <!-- 發貨資料 Start -->
            <div class="ui segments">
                <div class="ui green segment">
                    <h5 class="ui header">發貨基本資料
                    </h5>
                </div>
                <div class="ui small form segment">
                    <div class="fields">
                        <div class="six wide field">
                            <label>ERP單號</label>
                            <div class="ui green basic large label">
                                22B2-1108908223
                            </div>
                        </div>
                        <div class="seven wide field">
                            <label>客戶</label>
                            <div class="ui blue basic large label">
                                天貓商城棋見店123 (11800401)
                            </div>
                        </div>
                        <div class="three wide field">
                            <label>庫別</label>
                            <div class="ui basic large label">
                                深圳倉 (A01)
                            </div>
                        </div>
                    </div>
                    <div class="fields">
                        <div class="six wide field">
                            <label>發貨日期</label>
                            <div class="ui left icon input datepicker">
                                <input type="text" placeholder="發貨日期" autocomplete="off" value="2018/12/20" />
                                <i class="calendar alternate outline icon"></i>
                            </div>
                        </div>
                        <div class="ten wide required field">
                            <label>貨運公司</label>
                            <select>
                                <option value="A">德邦快递</option>
                                <option value="B">優速快递</option>
                                <option value="C">XX快递</option>
                                <option value="D">EMS</option>
                            </select>
                        </div>

                    </div>
                    <div class="fields">
                        <div class="six wide field">
                            <label>物流途徑</label>
                            <select>
                                <option value="A">客戶自提</option>
                                <option value="B">自行送貨</option>
                                <option value="C" selected="selected">其它</option>
                            </select>
                        </div>
                        <div class="seven wide required field">
                            <label>收貨人</label>
                            <input type="text" value="鋼鐵人">
                        </div>
                        <div class="three wide field">
                            <label>件數</label>
                            <input type="number" value="1">
                        </div>
                    </div>
                    <div class="fields">
                        <div class="sixteen wide field">
                            <label>備註</label>
                            <textarea rows="2"></textarea>
                        </div>
                    </div>

                    <div class="ui two column grid">
                        <div class="column">
                            <a href="#!" class="ui small button"><i class="undo icon"></i>返回列表</a>
                        </div>
                        <div class="column right aligned">
                            <button type="button" class="ui green small button"><i class="save icon"></i>存檔</button>
                        </div>
                    </div>
                </div>
            </div>
            <!-- 發貨資料 End -->

            <!-- 物流單號(多筆) Start -->
            <div class="ui segments">
                <div class="ui teal segment">
                    <h5 class="ui header">物流單號
                        <small class="grey-text text-darken-1">&nbsp;(此處可填寫多筆物流單號)</small>
                    </h5>
                </div>
                <div class="ui small form segment">
                    <div class="ui internally celled grid">
                        <div class="row">
                            <div class="ten wide column">
                                <table class="ui celled compact small table">
                                    <thead>
                                        <tr>
                                            <th>單號</th>
                                            <th>到付$</th>
                                            <th>自付$</th>
                                            <th>墊付$</th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr>
                                            <td>A1234567890</td>
                                            <td>15</td>
                                            <td>0</td>
                                            <td>30</td>
                                            <td class="center aligned collapsing">
                                                <button type="button" class="ui mini orange basic icon button">
                                                    <i class="trash alternate icon"></i>
                                                </button>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <div class="six wide column">
                                <div class="three fields">
                                    <div class="field">
                                        <label>到付$</label>
                                        <input type="text" value="0">
                                    </div>
                                    <div class="field">
                                        <label>自付$</label>
                                        <input type="text" value="0">
                                    </div>
                                    <div class="field">
                                        <label>墊付$</label>
                                        <input type="text" value="0">
                                    </div>
                                </div>
                                <div class="field">
                                    <label>物流單號</label>
                                    <input type="text" value="trigger enter">
                                </div>
                                <div class="field">
                                    <button type="button" class="ui teal tiny button">新增單號</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <!-- 物流單號(多筆) End -->

            <!-- 合併發貨 (ERP單號關聯) Start -->
            <div class="ui segments">
                <div class="ui teal segment">
                    <h5 class="ui header">合併發貨
                        <small class="grey-text text-darken-1">&nbsp;(多筆單據合併發貨時請填寫此處)</small>
                    </h5>
                </div>
                <div class="ui small form segment">
                    <div class="ui internally celled grid">
                        <div class="row">
                            <div class="ten wide column">
                                <label>已關聯列表</label>
                                <table class="ui celled compact small table">
                                    <tr>
                                        <td class="center aligned">2212-33344123123</td>
                                        <td class="center aligned collapsing">
                                            <button type="button" class="ui mini orange basic icon button">
                                                <i class="trash alternate icon"></i>
                                            </button>
                                        </td>
                                        <td class="center aligned">2212-33344123123</td>
                                        <td class="center aligned collapsing">
                                            <button type="button" class="ui mini orange basic icon button">
                                                <i class="trash alternate icon"></i>
                                            </button>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <div class="six wide column">
                                <div class="field">
                                    <label>關聯單據</label>
                                    <div class="ui fluid search ac-ModelNo">
                                        <div class="ui right labeled input">
                                            <asp:TextBox ID="filter_ModelNo" runat="server" CssClass="prompt" placeholder="輸入不含「-」的單號"></asp:TextBox>
                                            <asp:Panel ID="lb_ModelNo" runat="server" CssClass="ui label">輸入關鍵字,選擇項目</asp:Panel>
                                        </div>
                                        <asp:TextBox ID="val_ModelNo" runat="server" Style="display: none"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="ui message">
                                    <div class="header">
                                        如何使用?
                                    </div>
                                    <ul class="list">
                                        <li>於「關聯單據」欄位，輸入不含「-」的單號</li>
                                        <li>系統會自動帶出符合條件的清單</li>
                                        <li>選擇指定項目後，會自動新增至左方列表</li>
                                        <li>合併單的備註欄位自動備註上合併的單號</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>
            </div>
            <!-- ERP單號關聯 End -->
        </div>
        <!-- Content End -->

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

