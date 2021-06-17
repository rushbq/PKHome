<%@ Page Title="製物工單查詢" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myMarketingHelp_Search" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
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
                    <div class="section"><%:resPublic.fun_製物工單 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        <asp:Literal ID="lt_CorpName" runat="server" Text="公司別名稱"></asp:Literal>
                    </div>
                </div>
            </div>
            <div class="right menu">
                <asp:LinkButton ID="lbtn_Excel" runat="server" CssClass="item" OnClick="lbtn_Excel_Click"><i class="file excel icon"></i><span class="mobile hidden">匯出</span></asp:LinkButton>
                <%--<div class="ui right pointing red basic label"><%=GetLocalResourceObject("tip1").ToString()%></div>--%>
                <a class="item" href="<%=FuncPath() %>/Edit">
                    <i class="plus icon"></i>
                    <span class="mobile hidden"><%=GetLocalResourceObject("txt_新增需求").ToString()%></span>
                </a>
                <a class="item" href="<%=FuncPath() %>/Chart">
                    <i class="chart bar icon"></i>
                    <span class="mobile hidden">統計圖表</span>
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
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_關鍵字查詢")%></label>
                        <asp:TextBox ID="filter_Keyword" runat="server"></asp:TextBox>
                    </div>
                    <div class="two wide field">
                        <label>日期區間</label>
                        <asp:DropDownList ID="filter_dateType" runat="server" CssClass="fluid">
                            <asp:ListItem Value="A">登記日</asp:ListItem>
                            <asp:ListItem Value="B">結案日</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="five wide field">
                        <label><%:GetLocalResourceObject("sh_登記日期")%></label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_sDate" runat="server" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_eDate" runat="server" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_需求類別")%></label>
                        <asp:DropDownList ID="filter_ReqClass" runat="server">
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_需求資源")%></label>
                        <asp:DropDownList ID="filter_ReqRes" runat="server">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="fields">
                    <div class="four wide field">
                        <label><%:GetLocalResourceObject("sh_需求部門")%>&nbsp;<span class="grey-text text-darken-1"><%:GetLocalResourceObject("sh_tip1")%></span></label>
                        <div class="ui fluid search ac-Dept">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Dept" runat="server" CssClass="prompt"></asp:TextBox>
                                <asp:Panel ID="lb_Dept" runat="server" CssClass="ui label"><%:GetLocalResourceObject("sh_關鍵字查詢")%></asp:Panel>
                            </div>
                            <asp:TextBox ID="val_Dept" runat="server" Style="display: none"></asp:TextBox>
                        </div>
                    </div>
                    <div class="six wide field">
                        <label><%:GetLocalResourceObject("sh_需求者")%>&nbsp;<span class="grey-text text-darken-1"><%:GetLocalResourceObject("sh_tip1")%></span></label>
                        <div class="ui fluid search ac-Employee">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Emp" runat="server" CssClass="prompt"></asp:TextBox>
                                <asp:Panel ID="lb_Emp" runat="server" CssClass="ui label"><%:GetLocalResourceObject("sh_關鍵字查詢")%></asp:Panel>
                            </div>
                            <asp:TextBox ID="val_Emp" runat="server" Style="display: none"></asp:TextBox>
                        </div>
                    </div>
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_緊急度")%></label>
                        <asp:DropDownList ID="filter_EmgStatus" runat="server">
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_處理狀態")%></label>
                        <asp:DropDownList ID="filter_ReqStatus" runat="server">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="fields">
                    <div class="ten wide field">
                        <label><%:GetLocalResourceObject("sh_處理人員")%></label>
                        <asp:ListBox runat="server" ID="ddl_ProcWho" SelectionMode="Multiple"></asp:ListBox>
                        <asp:TextBox ID="val_Proc" runat="server" Style="display: none"></asp:TextBox>
                    </div>
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_排序欄位")%></label>
                        <asp:DropDownList ID="sort_SortField" runat="server">
                            <asp:ListItem Value="">系統預設</asp:ListItem>
                            <asp:ListItem Value="A">登記日</asp:ListItem>
                            <asp:ListItem Value="B">希望完成日</asp:ListItem>
                            <asp:ListItem Value="C">預計完成日</asp:ListItem>
                            <asp:ListItem Value="D">結案日</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_排序方式")%></label>
                        <asp:DropDownList ID="sort_SortWay" runat="server">
                            <asp:ListItem Value="A">遞增(小到大)</asp:ListItem>
                            <asp:ListItem Value="B" Selected="True">遞減(大到小)</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
            </div>
            <div class="ui two column grid">
                <div class="column">
                    <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i><%:GetLocalResourceObject("txt_重置條件")%></a>
                </div>
                <div class="column right aligned">
                    <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i><%:GetLocalResourceObject("txt_查詢")%></button>
                    <asp:Button ID="btn_Search" runat="server" Text="Button" OnClick="btn_Search_Click" Style="display: none" />
                </div>
            </div>

        </div>
        <!-- Advance Search End -->

        <!-- Empty Content Start -->
        <asp:PlaceHolder ID="ph_EmptyData" runat="server">
            <div class="ui placeholder segment">
                <div class="ui two column stackable center aligned grid">
                    <div class="ui vertical divider">Or</div>
                    <div class="middle aligned row">
                        <div class="column">
                            <div class="ui icon header">
                                <i class="search icon"></i>
                                <%:GetLocalResourceObject("txt_NoData")%>
                            </div>
                        </div>
                        <div class="column">
                            <div class="ui icon header">
                                <i class="plus icon"></i>
                                <%:GetLocalResourceObject("txt_新增需求")%>
                            </div>
                            <a href="<%=FuncPath() %>/Edit" class="ui basic green button">開始建立</a>
                        </div>
                    </div>
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
                                    <th class="grey-bg lighten-3 center aligned collapsing">
                                        <asp:Literal ID="lt_header1" runat="server"></asp:Literal></th>
                                    <th class="grey-bg lighten-3">
                                        <asp:Literal ID="lt_header2" runat="server"></asp:Literal></th>
                                    <!--狀態-->
                                    <th class="grey-bg lighten-3 center aligned">
                                        <asp:Literal ID="lt_header3" runat="server"></asp:Literal></th>
                                    <th class="grey-bg lighten-3 center aligned">
                                        <asp:Literal ID="lt_header10" runat="server"></asp:Literal></th>
                                    <th class="grey-bg lighten-3 center aligned">
                                        <asp:Literal ID="lt_header4" runat="server"></asp:Literal></th>
                                    <th class="grey-bg lighten-3 center aligned">
                                        <asp:Literal ID="lt_header5" runat="server"></asp:Literal></th>
                                    <th class="grey-bg lighten-3 center aligned">
                                        <asp:Literal ID="lt_header6" runat="server"></asp:Literal></th>
                                    <th class="grey-bg lighten-3">
                                        <asp:Literal ID="lt_header7" runat="server"></asp:Literal></th>
                                    <th class="grey-bg lighten-3 center aligned">
                                        <asp:Literal ID="lt_header8" runat="server"></asp:Literal></th>
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
                    <tr id="trItem" runat="server">
                        <td class="center aligned">
                            <a href="<%=FuncPath() %>/View/<%#Eval("Data_ID") %>">
                                <h5><%#Eval("TraceID") %></h5>
                            </a>
                        </td>
                        <td>
                            <asp:Literal ID="lt_IsTimeout" runat="server"></asp:Literal>
                            <strong><%#Eval("Req_Subject") %></strong>
                        </td>
                        <td class="center aligned collapsing">
                            <%#Get_Status(Eval("StDisp").ToString(),Eval("StName").ToString()) %>
                        </td>
                        <td class="center aligned">
                            <strong><%#Eval("Req_Qty") %></strong>
                        </td>
                        <td class="center aligned">
                            <%#Eval("EmgName") %>
                        </td>
                        <td class="center aligned">
                            <div>
                                <div class="ui basic fluid label"><%#Eval("TypeName") %></div>
                            </div>
                            <div>
                                <div class="ui basic fluid label"><%#Eval("ResName") %></div>
                            </div>
                        </td>
                        <td class="center aligned collapsing">
                            <%#Eval("Create_Time").ToString().ToDateString("yyyy/MM/dd") %>
                        </td>
                        <td>
                            <%#Eval("Req_Name") %>
                        </td>
                        <td class="left aligned collapsing">
                            <div>
                                <div class="ui basic fluid label">
                                    <%:GetLocalResourceObject("txt_結案日")%>
                                    <div class="detail"><%#Eval("Finish_Date") %></div>
                                </div>
                            </div>
                            <div>
                                <div class="ui basic fluid label">
                                    <%:GetLocalResourceObject("txt_預計完成")%>
                                    <div class="detail"><%#Eval("Est_Date") %></div>
                                </div>
                            </div>
                        </td>
                        <td class="left aligned collapsing">
                            <a class="ui small grey basic icon button" href="<%=FuncPath() %>/View/<%#Eval("Data_ID") %>" title="看明細">
                                <i class="file alternate icon"></i>
                            </a>
                            <asp:PlaceHolder ID="ph_ProcWho" runat="server">
                                <a class="ui small green basic icon button btn-OpenDetail" data-id="<%#Eval("TraceID") %>" title="處理人員">
                                    <i class="user icon"></i>
                                </a>
                                <input type="hidden" id="linkID<%#Eval("TraceID") %>" value="<%#Eval("Data_ID") %>" />
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="ph_Edit" runat="server">
                                <a class="ui small teal basic icon button" href="<%=FuncPath() %>/Edit/<%#Eval("Data_ID") %>" title="編輯">
                                    <i class="pencil icon"></i>
                                </a>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="ph_Del" runat="server">
                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                            </asp:PlaceHolder>
                            <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
                        </td>
                    </tr>
                    <%-- 帶出處理人員名單 --%>
                    <tr id="tar-Detail-<%#Eval("TraceID") %>" class="grey-bg lighten-5" style="display: none;">
                        <td class="right aligned"></td>
                        <td colspan="7">
                            <div class="Detail-<%#Eval("TraceID") %>">
                                <div class="ui icon message">
                                    <i class="notched circle loading icon"></i>
                                    <div class="content">
                                        <div class="header">
                                            資料擷取,請稍候....
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </td>
                        <td class="center aligned">
                            <a class="ui small grey button btn-CloseDetail" data-id="<%#Eval("TraceID") %>">CLOSE</a>
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
                /*
                  取得處理人員,複選清單的值(工號)
                  ref:https://semantic-ui.com/modules/dropdown.html#/usage
                  return:陣列
                */
                var procValue = $("#MainContent_ddl_ProcWho").dropdown("get value");
                if (procValue.length > 0) {
                    //將陣列轉成以逗號分隔的字串
                    var myVals = procValue.join(",");
                    //填入隱藏欄位(傳遞時使用)
                    $("#MainContent_val_Proc").val(myVals);
                }

                //觸發查詢按鈕
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
            $('.datepicker').calendar(calendarOpts_Range);
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
    <%-- Search UI End --%>
    <%-- 處理人員名單 Start --%>
    <script>
        $(function () {
            //按鈕 - 開明細
            $(".btn-OpenDetail").click(function () {
                var id = $(this).attr("data-id");

                boxDetail(id, true);
            });

            //按鈕 - 關明細
            $('.btn-CloseDetail').click(function () {
                var id = $(this).attr("data-id");

                boxDetail(id, false);
            });

            //FUNCTION - 明細開關
            function boxDetail(id, isOpen) {
                var myBox = $("#tar-Detail-" + id);

                if (isOpen) {
                    myBox.show();

                    loadDetail(id);

                } else {
                    myBox.hide();
                }
            }

            //Ajax - 讀取明細
            function loadDetail(id) {
                //取得目標容器
                var container = $(".Detail-" + id);

                //取得輸入值
                var _dataID = $("#linkID" + id).val();

                //填入Ajax Html
                var url = "<%=fn_Param.WebUrl%>myMarketingHelp/GetHtml_AssignedList.ashx?id=" + _dataID;
                container.load(url);
            }

        });
    </script>
    <%-- 處理人員名單 End --%>
</asp:Content>

