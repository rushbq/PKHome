<%@ Page Title="客訴清單" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myCustComplaint_Search" %>

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
                    <div class="section"><%:resPublic.nav_3000 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="section"><%:resPublic.fun_客訴管理 %></div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">
                        <asp:Literal ID="lt_TypeName" runat="server" Text="功能名稱"></asp:Literal>
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <asp:PlaceHolder ID="ph_ExportAll" runat="server">
                    <asp:LinkButton ID="lbtn_Excel_All" runat="server" CssClass="item" OnClick="lbtn_Excel_All_Click"><i class="file excel icon"></i><span class="mobile hidden">匯出客訴清單</span></asp:LinkButton>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="ph_ExportFlow301" runat="server">
                    <asp:LinkButton ID="btn_Excel_301" runat="server" CssClass="item" OnClick="btn_Excel_301_Click"><i class="file excel icon"></i><span class="mobile hidden">匯出二線清單</span></asp:LinkButton>
                </asp:PlaceHolder>
                
                <a class="item" href="<%=FuncPath() %>/Chart">
                    <i class="chart bar icon"></i>
                    <span class="mobile hidden">分析圖表</span>
                </a>
                <a class="item" href="<%=FuncPath() %>/Inform">
                    <i class="envelope icon"></i>
                    <span class="mobile hidden">通知設定</span>
                </a>
                <a class="item" href="<%=FuncPath() %>/SetEdit">
                    <i class="plus icon"></i>
                    <span class="mobile hidden"><%:resPublic.btn_New%></span>
                </a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Search Start -->
        <div class="ui orange attached segment">
            <div class="ui small form">
                <div class="fields">
                    <div class="eight wide field">
                        <label>日期查詢</label>
                        <div class="three fields">
                            <div class="field">
                                <asp:DropDownList ID="filter_DateType" runat="server" CssClass="fluid">
                                    <asp:ListItem Value="A">開案時間</asp:ListItem>
                                    <asp:ListItem Value="B">結案時間</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_sDate" runat="server" placeholder="開始日" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_eDate" runat="server" placeholder="結束日" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="eight wide field">
                        <label><%:GetLocalResourceObject("sh_關鍵字查詢")%></label>
                        <asp:TextBox ID="filter_Keyword" runat="server" autocomplete="off" placeholder="客訴編號, 品號, 經銷商客戶, 物流快遞單號, 收貨客户/備註"></asp:TextBox>
                    </div>
                </div>
                <div class="fields">
                    <div class="four wide field">
                        <label><%:GetLocalResourceObject("sh_狀態")%></label>
                        <asp:DropDownList ID="filter_FlowStatus" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="four wide field">
                        <label><%:GetLocalResourceObject("sh_類別")%></label>
                        <asp:DropDownList ID="filter_CustType" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="four wide field">
                        <label>計畫處理方式</label>
                        <asp:DropDownList ID="filter_PlanType" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="four wide field">
                        <label><%:GetLocalResourceObject("sh_追蹤碼")%></label>
                        <asp:TextBox ID="filter_TraceID" runat="server" autocomplete="off" placeholder="追蹤碼"></asp:TextBox>
                    </div>
                </div>
            </div>
            <div class="ui three column grid">
                <div class="column">
                    <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i><%:resPublic.btn_Reset%></a>
                </div>
                <div class="column center aligned" title="僅限「業務確認」">
                    <div class="ui small buttons">
                        <div class="ui left labeled button" tabindex="0">
                            <span id="checkCount" class="ui green basic right pointing label">0</span>
                            <button type="button" id="batchReply" class="ui green button">批量回覆</button>
                        </div>
                        <div class="or"></div>
                        <button type="button" class="ui button" id="clearReply">清空勾選</button>
                    </div>

                </div>
                <div class="column right aligned">
                    <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i><%:resPublic.btn_Search%></button>
                    <asp:Button ID="btn_Search" runat="server" Text="search" OnClick="btn_Search_Click" Style="display: none" />
                </div>
            </div>

        </div>
        <!-- Search End -->

        <!-- Empty Content Start -->
        <asp:PlaceHolder ID="ph_EmptyData" runat="server" Visible="false">
            <div class="ui secondary pointing menu">
                <a class="item" href="<%=FuncPath() %>/Set">開案中客訴</a>
                <a class="item active" href="<%=FuncPath() %>">客訴清單
                </a>
            </div>
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
                                <%:GetLocalResourceObject("txt_新增資料")%>
                            </div>
                            <a href="<%=FuncPath() %>/SetEdit" class="ui basic green button">新增客訴</a>
                        </div>
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>
        <!-- Empty Content End -->

        <!-- List Content Start -->
        <asp:PlaceHolder ID="ph_Data" runat="server">
            <div class="ui green attached segment">
                <div class="ui secondary pointing menu">
                    <a class="item" href="<%=FuncPath() %>/Set">開案中客訴
                        <div class="ui label">
                            <asp:Literal ID="lt_DataCnt" runat="server">0</asp:Literal>
                        </div>
                    </a>
                    <a class="item active" href="<%=FuncPath() %>">客訴清單
                    </a>
                </div>
                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                    <LayoutTemplate>
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th>客訴編號</th>
                                    <th>品號</th>
                                    <th>數量</th>
                                    <th>客戶類別</th>
                                    <th>客訴內容</th>
                                    <th>流程狀態</th>
                                    <th class="center aligned">一線</th>
                                    <th class="center aligned">二線</th>
                                    <th class="center aligned">業務</th>
                                    <th class="center aligned">資材</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                        </table>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="collapsing">
                                <asp:PlaceHolder ID="ph_Checkbox" runat="server">
                                    <div id="<%#Eval("CC_UID") %>" class="ui checkbox">
                                        <input type="checkbox" name="<%#Eval("CC_UID") %>" value="<%#Eval("Data_ID") %>">
                                        <label><b class="blue-text text-darken-2"><%#Eval("CC_UID") %></b></label>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="ph_NoCheckbox" runat="server">
                                    <h5 class="blue-text text-darken-2"><%#Eval("CC_UID") %></h5>
                                </asp:PlaceHolder>
                            </td>
                            <td class="collapsing">
                                <strong><%#Eval("ModelNo") %></strong>
                            </td>
                            <td class="center aligned collapsing">
                                <strong><%#Eval("Qty") %></strong>
                            </td>
                            <td>
                                <div><%#Eval("CustTypeName") %></div>
                                <div class="grey-text text-darken-2"><%#Eval("RefCustName") %></div>
                                <div class="grey-text text-darken-2"><%#Eval("RefMallName") %></div>
                            </td>
                            <td>
                                <asp:Literal ID="lt_Content" runat="server"></asp:Literal>
                            </td>
                            <td class="center aligned collapsing">
                                <div class="ui <%#showStatus(Eval("FlowStatus").ToString()) %> basic fluid label"><%#Eval("FlowStatusName") %></div>
                            </td>
                            <td class="center aligned collapsing">
                                <p>
                                    <%#GetRefClassName(Convert.ToInt16(Eval("Flow201_Type"))) %>
                                </p>
                                <p class="grey-text text-darken-1"><%#Eval("Flow201_Time") %></p>
                            </td>
                            <td class="center aligned collapsing">
                                <p>
                                    <%#GetRefClassName(Convert.ToInt16(Eval("Flow301_Type"))) %>
                                </p>
                                <p class="grey-text text-darken-1"><%#Eval("Flow301_Time") %></p>
                            </td>
                            <td class="center aligned collapsing">
                                <p>
                                    <%#GetRefClassName(Convert.ToInt16(Eval("Flow401_Type"))) %>
                                </p>
                                <p class="grey-text text-darken-1"><%#Eval("Flow401_Time") %></p>
                            </td>
                            <td class="center aligned collapsing">
                                <p>
                                    <%#GetRefClassName(Convert.ToInt16(Eval("Flow501_Type"))) %>
                                </p>
                                <p class="grey-text text-darken-1"><%#Eval("Flow501_Time") %></p>
                            </td>
                            <td class="collapsing">
                                <%-- <a class="ui small grey basic icon button" href="<%=fn_Param.WebUrl %>myCustComplaint/Print1.aspx?id=<%#Eval("Data_ID") %>&typeID=<%=Req_TypeID %>" title="列印客訴處理單" target="_blank">
                                    <i class="dolly flatbed icon"></i>
                                </a>--%>
                                <a class="ui small grey basic icon button" href="<%=FuncPath() %>/View/<%#Eval("Data_ID") %>" title="查看">
                                    <i class="file alternate icon"></i>
                                </a>
                                <asp:PlaceHolder ID="ph_Edit" runat="server">
                                    <a class="ui small teal basic icon button" href="<%=FuncPath() %>/Edit/<%#Eval("Data_ID") %>#flow<%#Eval("FlowStatus") %>" title="編輯">
                                        <i class="pencil icon"></i>
                                    </a>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="ph_Del" runat="server">
                                    <button type="button" class="ui small orange basic icon button showClose" data-id="<%#Eval("Data_ID") %>" data-ccid="<%#Eval("CC_UID") %>" data-item="<%#Eval("ModelNo") %>" title="終止流程"><i class="ban alternate icon"></i></button>
                                </asp:PlaceHolder>
                            </td>
                        </tr>

                    </ItemTemplate>
                </asp:ListView>
            </div>
            <!-- List Pagination Start -->
            <div class="ui mini bottom attached segment grey-bg lighten-4">
                <asp:Literal ID="lt_Pager" runat="server"></asp:Literal>
            </div>
            <!-- List Pagination End -->
        </asp:PlaceHolder>
        <!-- List Content End -->
    </div>
    <!-- 內容 End -->
    <!-- Close Modal Start -->
    <div id="closePage" class="ui modal">
        <div class="header">
            流程終止後不可回復,請確認是否執行
        </div>
        <div class="content">
            <div class="ui form">
                <div class="two fields">
                    <div class="field">
                        <label>客訴編號</label>
                        <span id="valCCID" class="ui blue basic large label"></span>
                    </div>
                    <div class="field">
                        <label>品號</label>
                        <span id="valItem" class="ui green basic large label"></span>
                    </div>

                </div>
                <div class="required field">
                    <label>填寫原因</label>
                    <textarea id="tb_closeReason" placeholder="原因最多填寫200字" maxlength="200" rows="5"></textarea>
                </div>
            </div>

        </div>
        <div class="actions">
            <div class="ui cancel button">
                稍候處理，關閉視窗
            </div>
            <button id="doClose" type="button" class="ui positive right labeled icon button" onclick="return confirm('確認原因填寫完畢?')">確認執行<i class="checkmark icon"></i></button>
        </div>
    </div>
    <!-- Close Modal End -->
    <div style="display: none">
        <asp:TextBox ID="val_closeReason" runat="server" TextMode="MultiLine"></asp:TextBox>
        <asp:TextBox ID="val_DataID" runat="server" Style="display: none"></asp:TextBox>
        <asp:Button ID="btn_doClose" runat="server" OnClick="btn_doClose_Click" Style="display: none;" />
    </div>

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
        });

    </script>

    <%-- 批量回覆 Start --%>
    <script>
        $(function () {
            //init check items
            getCookieItems();

            //init checknumbers
            getCookieCount();


            //init checkbox
            $('.ui.checkbox').checkbox({
                onChecked: function () {
                    //勾選, 加入cookie
                    var _dataUid = $(this).val();
                    var _ccUid = $(this).attr("name");

                    //----- Ajax 處理 Start -----
                    //[Ajax return], 設定cookie
                    var req = setCookie(_dataUid, _ccUid, "set");

                    //[Ajax done]
                    req.done(function (callback) {
                        //init check items
                        getCookieItems();

                        //init checknumbers
                        getCookieCount();
                    });

                    //[Ajax fail]
                    req.fail(function (jqXHR, textStatus) {
                        event.preventDefault();
                        alert('無法加入項目，請連絡系統管理人員!');
                    });
                    //----- Ajax 處理 End -----
                },
                onUnchecked: function () {
                    //取消勾選, 移除cookie
                    var _ccUid = $(this).attr("name");
                    //var _dataUid = $(this).val();

                    //----- Ajax 處理 Start -----
                    //[Ajax return], 移除cookie
                    var req = setCookie("", _ccUid, "remove");

                    //[Ajax done]
                    req.done(function (callback) {
                        //init check items
                        getCookieItems();

                        //init checknumbers
                        getCookieCount();
                    });

                    //[Ajax fail]
                    req.fail(function (jqXHR, textStatus) {
                        event.preventDefault();
                        alert('無法移除項目，請連絡系統管理人員!');
                    });
                    //----- Ajax 處理 End -----

                }
            });


            //[Click] 執行批量回覆
            $("#batchReply").click(function () {
                //show confirm
                if (!confirm('確定要開始回覆?')) {
                    return;
                }

                //Check count
                var cnt = $("#checkCount").text();
                if (cnt == "0") {
                    alert('至少勾選一個項目');
                    return;
                }

                //Redirect batch reply page
                location.href = '<%=FuncPath() %>/BatchReply';

            });

            //[Click] 清空勾選
            $("#clearReply").click(function () {
                //show confirm
                if (!confirm('確定要清空已勾選項目?')) {
                    return;
                }

                //----- Ajax 處理 Start -----
                //[Ajax return], 取得已勾選數量
                var chkCount = setCookie("", "", "clear");

                //[Ajax done]
                chkCount.done(function (callback) {
                    window.location.replace(location.href);
                });

                //[Ajax fail]
                chkCount.fail(function (jqXHR, textStatus) {
                    event.preventDefault();
                    alert('無法清空勾選，請連絡系統管理人員!');
                });

                //----- Ajax 處理 End -----
            });

        });


        //[Function] 取得已勾選的項目
        function getCookieItems() {
            //----- Ajax 處理 Start -----
            //[Ajax return], 取得已勾選的項目
            var chkItem = setCookie("", "", "readChecked");

            //[Ajax done]
            chkItem.done(function (callback) {
                if (callback != "") {
                    //取得字串值
                    var data = callback.split(",");

                    //將有勾選的項目設為checked
                    $.each(data, function (index, value) {
                        $('#' + value).checkbox("set checked");
                    });
                }
            });

            //[Ajax fail]
            chkItem.fail(function (jqXHR, textStatus) {
                event.preventDefault();
                alert('無法取得已勾選的項目，請連絡系統管理人員!');
            });
            //----- Ajax 處理 End -----
        }


        //[Function] 取得已勾選數量
        function getCookieCount() {
            //----- Ajax 處理 Start -----
            //[Ajax return], 取得已勾選數量
            var chkCount = setCookie("", "", "readCount");

            //[Ajax done]
            chkCount.done(function (callback) {
                $("#checkCount").text(callback);
            });

            //[Ajax fail]
            chkCount.fail(function (jqXHR, textStatus) {
                event.preventDefault();
                alert('無法取得已勾選的數量，請連絡系統管理人員!');
            });

            //----- Ajax 處理 End -----
        }


        //[Function] Cookie Ajax呼叫主體
        function setCookie(guid, ccpid, setType) {
            var request = $.ajax({
                url: '<%=fn_Param.WebUrl%>' + "myCustComplaint/Ashx_CookieSet.ashx",
                method: "POST",
                data: {
                    guid: guid,
                    ccpid: ccpid,
                    setType: setType,
                    typeID: '<%=Req_TypeID%>'
                },
                dataType: "html"
            });

            return request;
        }
    </script>
    <%-- 批量回覆 End --%>

    <%-- 日期選擇器 Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.js"></script>
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/options.js?v=200804"></script>
    <script>
        $(function () {
            //取得設定值(往前天數, 往後天數)
            var calOpt = getCalOptBydate(730, 7);
            //載入datepicker
            $('.datepicker').calendar(calOpt);
        });
    </script>
    <%-- 日期選擇器 End --%>
    <%-- Modal Start --%>
    <script>
        $(function () {
            //流程作廢視窗(Modal)
            $(".showClose").click(function () {
                //取得按鈕上設定的編號/值
                var id = $(this).attr("data-id");
                var cid = $(this).attr("data-CCID");
                var model = $(this).attr("data-item");

                //填入值
                $("#valCCID").text(cid);
                $("#valItem").text(model);
                $("#MainContent_val_DataID").val(id); //將值傳到接收欄位

                //顯示modal
                $('#closePage').modal('show');
            });

            //作廢確認鈕
            //serverside的欄位/按鈕,要放在本頁,不可放在modal
            $("#doClose").click(function () {
                //取得Modal輸入值
                var value = $("#tb_closeReason").val();
                if (value == '') {
                    alert('請填入原因!')
                    return false;
                }

                //將輸入值傳到接收欄位
                $("#MainContent_val_closeReason").val(value);

                //觸發按鈕
                $("#MainContent_btn_doClose").trigger("click");
            });

        });
    </script>
    <%-- Modal End --%>
</asp:Content>

