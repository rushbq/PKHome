<%@ Page Title="標準成本包材維護" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search_Prod.aspx.cs" Inherits="myPurProdCost_Search_Prod" %>

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
                    <div class="section"><%:resPublic.nav_4000 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="section">標準成本</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        包材維護
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Advance Search Start -->
        <div class="ui orange attached segment">
            <div class="ui two column grid">
                <div class="column">
                    <div class="ui small form">
                        <div class="field">
                            <asp:TextBox ID="filter_Keyword" runat="server" autocomplete="off" placeholder="查詢關鍵字:品號" MaxLength="20"></asp:TextBox>
                        </div>
                    </div>
                </div>
                <div class="column right aligned">
                    <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i>重置條件</a>
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
                        <table class="ui celled selectable table">
                            <thead>
                                <tr>
                                    <th class="grey-bg lighten-3">品號/品名</th>
                                    <th class="grey-bg lighten-3">包材</th>
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
                    <asp:PlaceHolder ID="ph_DataGroup" runat="server">
                        <tr id="row<%#Eval("RowIdx") %>">
                            <td class="left aligned">
                                <h4 class="red-text text-darken-4"><%#Eval("ModelNo") %></h4>
                                <div class="grey-text text-darken-2">
                                    <%#Eval("ModelName") %>
                                </div>
                            </td>
                            <td>
                                <asp:PlaceHolder ID="ph_RelItems" runat="server">
                                    <table class="ui celled compact small grey-bg lighten-4 table">
                                        <thead>
                                            <tr>
                                                <th class="grey-text text-darken-1 center aligned collapsing">資料庫別</th>
                                                <th class="grey-text text-darken-1">包材品號</th>
                                                <th class="grey-text text-darken-1 center aligned collapsing">數量</th>
                                                <th>&nbsp;</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <asp:Literal ID="lt_Items" runat="server"></asp:Literal>
                                        </tbody>
                                        <tfoot class="full-width">
                                            <!-- New Item Section -->
                                            <tr>
                                                <th colspan="4">
                                                    <div class="ui small form">
                                                        <div class="fields">
                                                            <div class="two wide field">
                                                                <select id="dbs_<%#Eval("RowIdx") %>" class="fluid">
                                                                    <option value="TW">TW</option>
                                                                    <option value="SH">SH</option>
                                                                </select>
                                                            </div>
                                                            <div class="eight wide field">
                                                                <select id="item_<%#Eval("RowIdx") %>" class="ac-drpProd ui fluid search selection dropdown">
                                                                    <option value="">新增:填入關鍵字,選擇項目</option>
                                                                </select>
                                                            </div>
                                                            <div class="two wide field">
                                                                <input type="number" id="qty_<%#Eval("RowIdx") %>" placeholder="數量" step="any">
                                                            </div>
                                                            <div class="four wide field">
                                                                <input type="hidden" id="model_<%#Eval("RowIdx") %>" value="<%#Eval("ModelNo") %>" />
                                                                <button type="button" class="ui small teal basic button doCreate" data-id="<%#Eval("RowIdx") %>"><i class="plus icon"></i>New</button>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </th>
                                            </tr>
                                        </tfoot>
                                    </table>
                                </asp:PlaceHolder>
                            </td>
                        </tr>
                    </asp:PlaceHolder>

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
    <%-- 資料處理Ajax --%>
    <script>
        $(function () {
            /*
              功能按鈕:新增品項
            */
            //New Prod Click
            $(".doCreate").click(function () {
                //取得編號
                var id = $(this).attr("data-id");
                var ts = $.now();


                //成品品號
                var _model = $("#model_" + id).val();
                //數量
                var _qty = $("#qty_" + id).val();
                //DBS
                var _dbs = $("#dbs_" + id).dropdown("get value");
                //卡片品號
                var _itemID = $("#item_" + id).dropdown("get value");
                if (_itemID == '' || _qty == '') {
                    alert('未正確選擇產品,或數量未填.');
                    return false;
                }


                /* Ajax Start */
                var saveBtn = $(this);
                //button 加入loading
                saveBtn.addClass("loading");


                var request = $.ajax({
                    url: '<%=fn_Param.WebUrl%>' + "myPurProdCost/Ashx_SetData.ashx",
                    method: "POST",
                    //contentType: 'application/json',    //傳送格式
                    dataType: "html",   //遠端回傳格式
                    data: {
                        jobType: "ADD",
                        Model: _model,
                        Qty: _qty,
                        DBS: _dbs,
                        ItemID: _itemID
                    },
                });

                request.done(function (msg) {
                    //button 移除loading
                    saveBtn.removeClass("loading");

                    if (msg == "success") {
                        //顯示成功訊息
                        location.replace('<%=filterUrl()%>&v=' + ts + '#row' + id);

                } else {
                    event.preventDefault();
                    alert(_model + ':資料建立失敗!');
                    console.log(msg);
                }
                });

                request.fail(function (jqXHR, textStatus) {
                    event.preventDefault();
                    alert(_model + ':存檔時發生錯誤,請連絡MIS.');
                    saveBtn.removeClass("loading");
                });

                request.always(function () {
                    //do something
                });
                /* Ajax End */
            });


            /*
              功能按鈕:刪除品項
            */
            //New Prod Click
            $(".doDel").click(function () {
                //doDel
                if (!confirm("確定刪除?")) {
                    return false;
                }

                //取得編號
                var id = $(this).attr("data-id");
                var ts = $.now();

                /* Ajax Start */
                var saveBtn = $(this);
                //button 加入loading
                saveBtn.addClass("loading");

                var request = $.ajax({
                    url: '<%=fn_Param.WebUrl%>' + "myPurProdCost/Ashx_SetData.ashx",
                    method: "POST",
                    //contentType: 'application/json',    //傳送格式
                    dataType: "html",   //遠端回傳格式
                    data: {
                        jobType: "DEL",
                        ItemID: id
                    },
                });

                request.done(function (msg) {
                    //button 移除loading
                    saveBtn.removeClass("loading");

                    if (msg == "success") {
                        //顯示成功訊息
                        location.replace('<%=filterUrl()%>&v=' + ts);

                } else {
                    event.preventDefault();
                    alert('資料刪除失敗!');
                    console.log(msg);
                }
                });

                request.fail(function (jqXHR, textStatus) {
                    event.preventDefault();
                    alert('刪除時發生錯誤,請連絡MIS.');
                    saveBtn.removeClass("loading");
                });

                request.always(function () {
                    //do something
                });
                /* Ajax End */
            });
        });
    </script>
    <%-- 產品選單 --%>
    <script>
        /*
          search dropdown多選
          注意事項:
          需使用Html Controller, 不能使用 .Net元件
          , 因選項會變動, 會被視為安全性漏洞, 所以要用另一個ServerSide元件接收值
        */
        $('.ac-drpProd').dropdown({
            fields: {
                remoteValues: 'results',
                name: 'FullLabel',
                value: 'ID'
            },
            apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Prod_v1.ashx?show=Y&q={query}&v=1.0'
            }

        });

        //取得複選選單欄位值
        function doGetDrpVals() {
            //取得多選品號的值
            var procValue = $("#menuProd").dropdown("get value");
            var $fldProd = $("#MainContent_val_Prods");

            if (procValue.length == 0) {
                $fldProd.val("");
            } else {
                //將陣列轉成含分隔符號的字串
                var myVals = procValue.join(",");
                //填入隱藏欄位(傳遞時使用)
                $fldProd.val(myVals);
                //console.log(myVals);
            }
        }

    </script>
    <script>
        /* 當網址有#錨點時,觸發以下動作 */
        if (window.location.hash) {
            var
              $element = $(window.location.hash),
              position = $element.offset().top - 60
            ;
            //$element.addClass('active');
            $('html, body')
              .stop()
              .animate({
                  scrollTop: position
              }, 400)
            ;
        }
    </script>
</asp:Content>

