<%@ Page Title="郵件寄送登記 (管理者)" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myPostal_Search" %>

<%@ Import Namespace="Resources" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section"><%:resPublic.nav_2000 %></div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">郵件寄送登記 (管理者)
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="item" href="<%=fn_Param.WebUrl %>myPostal/Print.aspx" target="_blank">
                    <i class="print icon"></i>
                    <span class="mobile hidden">列印登記表</span>
                </a>
                <a class="item" href="<%=fn_Param.WebUrl %>myPostal/StatMonth.aspx" target="_blank">
                    <i class="dollar sign icon"></i>
                    <span class="mobile hidden">郵資申請統計</span>
                </a>
                <asp:LinkButton ID="lbtn_Excel" runat="server" CssClass="item" OnClick="lbtn_Excel_Click"><i class="file excel icon"></i><span class="mobile hidden">郵寄明細表</span></asp:LinkButton>

            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Section-資料編輯區 Start -->
        <div class="ui attached segment grey-bg lighten-5">
            <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
                <div class="ui negative message">
                    <div class="header">
                        <%:resPublic.error_oops %>
                    </div>
                    <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                </div>
            </asp:PlaceHolder>
            <div class="ui segments">
                <div class="ui red segment">
                    <div class="ui accordion">
                        <div class="title active">
                            <i class="icon dropdown"></i>
                            資料填寫區
                        </div>
                        <div class="content">
                            <div id="formBase" class="ui small form">
                                <div class="fields">
                                    <div class="four wide field">
                                        <label>系統編號</label>
                                        <div class="ui red basic large label">
                                            <asp:Literal ID="lt_SeqNo" runat="server">資料建立中</asp:Literal>
                                        </div>
                                    </div>
                                    <div class="four wide required field">
                                        <label>日期</label>
                                        <div class="ui left icon input datepicker">
                                            <asp:TextBox ID="tb_PostDate" runat="server" MaxLength="10" autocomplete="off" placeholder="格式:西元年/月/日"></asp:TextBox>
                                            <i class="calendar alternate outline icon"></i>
                                        </div>
                                    </div>
                                    <div class="four wide required field">
                                        <label>寄件人</label>
                                        <div class="ui fluid search ac-Employee">
                                            <div class="ui right labeled input">
                                                <asp:TextBox ID="filter_Emp" runat="server" CssClass="prompt"></asp:TextBox>
                                                <span id="lb_Emp" class="ui label">查詢工號或人名</span>
                                            </div>
                                            <asp:TextBox ID="val_Emp" runat="server" Style="display: none"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="four wide field">
                                    </div>
                                </div>
                                <div class="fields">
                                    <div class="four wide required field">
                                        <label>郵式</label>
                                        <asp:DropDownList ID="ddl_PostType" runat="server" CssClass="fluid">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="four wide field">
                                        <label>重量</label>
                                        <asp:TextBox ID="tb_PackageWeight" runat="server" type="number" min="0" step="any" MaxLength="5">0</asp:TextBox>
                                    </div>
                                    <div class="four wide field">
                                        <label>郵資</label>
                                        <asp:TextBox ID="tb_PostPrice" runat="server" type="number" min="0" step="any" MaxLength="6">0</asp:TextBox>
                                    </div>
                                    <div class="four wide field">
                                        <label>郵務號碼</label>
                                        <asp:TextBox ID="tb_PostNo" runat="server" MaxLength="20" autocomplete="off"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="fields">
                                    <div class="four wide required field">
                                        <label>收件人&nbsp;<span class="grey-text text-darken-2">(可使用自動填寫或手動填寫)</span></label>
                                        <div class="ui fluid search ac-ShipData">
                                            <div class="ui icon input">
                                                <asp:TextBox ID="tb_ToWho" runat="server" CssClass="prompt" MaxLength="50" placeholder="輸入關鍵字,快速查詢並自動填寫"></asp:TextBox>
                                                <i class="search icon"></i>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="eight wide required field">
                                        <label>收件地址</label>
                                        <asp:TextBox ID="tb_ToAddr" runat="server" MaxLength="200" autocomplete="off"></asp:TextBox>
                                    </div>
                                    <div class="four wide required field">
                                        <label>內容</label>
                                        <asp:TextBox ID="tb_PostDesc" runat="server" MaxLength="50"></asp:TextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="ui padded grid">
                                <div class="right aligned column">
                                    <%--<a id="doShipping" href="<%=fn_Param.WebUrl %><%=Req_Lang %>/<%=Req_RootID %>/Postal/Address" class="ui orange small button"><i class="user icon"></i>收件資料維護</a>--%>
                                    <button id="doSave" type="button" class="ui green small button"><i class="save icon"></i>資料存檔</button>
                                    <asp:PlaceHolder ID="ph_Close_No" runat="server">
                                        <button id="doClose" type="button" class="ui red small button"><i class="power off icon"></i>截止今日登記</button>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="ph_Close_Yes" runat="server">
                                        <button type="button" class="ui grey small disabled button"><i class="coffee icon"></i>今日登記已截止</button>
                                    </asp:PlaceHolder>

                                    <asp:HiddenField ID="hf_DataID" runat="server" />
                                    <asp:Button ID="btn_doSave" runat="server" Text="Save" OnClick="btn_doSave_Click" Style="display: none;" />
                                    <asp:Button ID="btn_doClose" runat="server" Text="Close" OnClick="btn_doClose_Click" Style="display: none;" />
                                </div>

                            </div>
                        </div>
                    </div>
                </div>

            </div>
        </div>
        <!-- Section-資料編輯區 End -->

        <!-- Section-資料查詢區 Start -->
        <div class="ui padded divided grid">
            <div class="row">
                <!-- Left Section Start -->
                <div class="four wide column">
                    <div class="ui small form">
                        <div class="field">
                            <label>日期</label>
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
                        <div class="field">
                            <label>郵式</label>
                            <asp:DropDownList ID="filter_PostType" runat="server" CssClass="fluid">
                            </asp:DropDownList>
                        </div>
                        <div class="field">
                            <label>關鍵字查詢</label>
                            <asp:TextBox ID="filter_Keyword" runat="server" autocomplete="off" placeholder="查詢:郵務號碼,收件人" MaxLength="20"></asp:TextBox>
                        </div>
                    </div>
                    <div class="ui two column vertically padded grid">
                        <div class="column">
                            <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i>重置</a>
                        </div>
                        <div class="column right aligned">
                            <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                            <asp:Button ID="btn_Search" runat="server" Text="Button" OnClick="btn_Search_Click" Style="display: none" />
                        </div>
                    </div>
                    <div>
                        <a href="http://postserv.post.gov.tw/pstmail/main_mail.html?targetTxn=EB500100" class="ui green small fluid button" target="_blank"><i class="address card icon"></i>掛號查詢</a>
                    </div>
                </div>
                <!-- Left Section End -->

                <!-- Right Section Start -->
                <div class="twelve wide column">
                    <!-- List Content Start -->
                    <asp:PlaceHolder ID="ph_Data" runat="server">
                        <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                            <LayoutTemplate>
                                <table id="tableList" class="ui celled selectable compact small table display nowrap" style="width: 100%;">
                                    <thead>
                                        <tr>
                                            <th class="center aligned no-sort">編號</th>
                                            <th class="center aligned">日期</th>
                                            <%--<th class="center aligned">部門</th>--%>
                                            <th class="center aligned">寄件人</th>
                                            <th class="center aligned">郵式</th>
                                            <th class="center aligned">號碼</th>
                                            <th class="no-sort">收件人</th>
                                            <th class="no-sort">地址</th>
                                            <th class="center aligned no-sort">重量</th>
                                            <th class="center aligned no-sort">郵資</th>
                                            <th class="no-sort">內容</th>
                                            <th class="no-sort"></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:PlaceHolder ID="ph_Items" runat="server" />
                                    </tbody>
                                </table>
                            </LayoutTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td class="center aligned">
                                        <b><%#Eval("SeqNo") %></b>
                                    </td>
                                    <td class="center aligned collapsing">
                                        <%#Eval("PostDate") %>
                                    </td>
                                    <%-- <td class="center aligned collapsing">
                                        <%#Eval("Post_DeptName") %>
                                    </td>--%>
                                    <td class="center aligned collapsing">
                                        <%#Eval("Post_WhoName") %>
                                    </td>
                                    <td class="center aligned blue-text text-darken-2 collapsing">
                                        <strong>
                                            <%#Eval("PostTypeName") %>
                                        </strong>
                                    </td>
                                    <td class="center aligned green-text text-darken-2 collapsing">
                                        <strong>
                                            <%#Eval("PostNo") %>
                                        </strong>
                                    </td>
                                    <td>
                                        <%#Eval("ToWho") %>
                                    </td>
                                    <td>
                                        <%#Eval("ToAddr") %>
                                    </td>
                                    <td class="center aligned">
                                        <%#Eval("PackageWeight") %>
                                    </td>
                                    <td class="center aligned">
                                        <%#Eval("PostPrice") %>
                                    </td>
                                    <td>
                                        <%#Eval("PostDesc") %>
                                    </td>
                                    <td class="left aligned collapsing">
                                        <asp:PlaceHolder ID="ph_Edit" runat="server">
                                            <a class="ui small teal basic icon button" href="<%=FuncPath() %>?edit=Y&id=<%#Eval("Data_ID") %>" title="編輯">
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
                            <EmptyDataTemplate>
                                <div class="ui placeholder segment">
                                    <div class="ui icon header">
                                        <i class="search icon"></i>
                                        目前條件查無資料
                                    </div>
                                </div>
                            </EmptyDataTemplate>
                        </asp:ListView>
                    </asp:PlaceHolder>
                    <!-- List Content End -->
                </div>
                <!-- Right Section End -->

            </div>
        </div>
        <!-- Section-資料查詢區 End -->
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
                //觸發查詢按鈕
                $("#MainContent_btn_Search").trigger("click");
            });

            //init dropdown list
            $('select').dropdown();


            //清單收合(讀取完畢後關閉, 第0個accordion)
            var formSection = '<%=Req_Edit.Equals("Y")?"open":"close"%>';
            $('.ui.accordion').accordion(formSection, 0);


            //存檔按鈕
            $("#doSave").click(function () {
                $("#formBase").addClass("loading");
                $("#MainContent_btn_doSave").trigger("click");
            });

            //截止按鈕
            $("#doClose").click(function () {
                if (confirm("確定截止登記?")) {
                    $(this).addClass("loading disabled");
                    $("#MainContent_btn_doClose").trigger("click");
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

    <%-- Search UI Start --%>
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
                $("#lb_Emp").text(result.description);

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
        /* 收件人 */
        $('.ac-ShipData').search({
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
                $("#MainContent_tb_ToAddr").val(result.Label);
            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_PostalAddress.ashx?q={query}'
            }

        });
    </script>
    <%-- Search UI End --%>


    <%-- DataTable Start --%>
    <link href="https://cdn.datatables.net/1.10.13/css/jquery.dataTables.min.css" rel="stylesheet" />
    <script src="https://cdn.datatables.net/1.10.11/js/jquery.dataTables.min.js"></script>
    <script>
        $(function () {
            //使用DataTable
            var table = $('#tableList').DataTable({
                fixedHeader: true,
                searching: true,  //搜尋
                ordering: true,   //排序
                paging: true,     //分頁
                info: false,      //頁數資訊
                pageLength: 10,   //顯示筆數預設值
                language: {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                //讓不排序的欄位在初始化時不出現排序圖
                "order": [],
                //自訂欄位
                "columnDefs": [{
                    "targets": 'no-sort',
                    "orderable": false,
                }],
                //捲軸設定
                "scrollY": '60vh',
                "scrollCollapse": true,
                "scrollX": true
            });


        });

    </script>
    <%-- DataTable End --%>
</asp:Content>

