<%@ Page Title="發貨明細維護" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Edit.aspx.cs" Inherits="myShipping_Edit" %>

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
                    <div class="section">發貨/運費維護統計</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        發貨明細維護
                    </div>
                </div>
            </div>
            <div class="right menu">
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui attached segment grey-bg lighten-5">
            <!-- 發貨資料 Start -->
            <div class="ui segments">
                <div class="ui green segment">
                    <h5 class="ui header">發貨基本資料
                    </h5>
                </div>
                <div class="ui small form attached segment">
                    <div class="fields">
                        <div class="six wide field">
                            <label>ERP單號</label>
                            <div class="ui green basic large label">
                                <asp:Literal ID="lt_ErpNo" runat="server"></asp:Literal>
                            </div>
                        </div>
                        <div class="seven wide field">
                            <label>客戶</label>
                            <div class="ui blue basic large label">
                                <asp:Literal ID="lt_Cust" runat="server"></asp:Literal>
                            </div>
                        </div>
                        <div class="three wide field">
                            <label>出貨地</label>
                            <div class="ui basic large label">
                                <asp:Literal ID="lt_StockType" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </div>
                    <div class="fields">
                        <div class="twelve wide field">
                            <div class="fields">
                                <div class="six wide field">
                                    <label>發貨日期</label>
                                    <div class="ui left icon input datepicker">
                                        <asp:TextBox ID="tb_ShipDate" runat="server" MaxLength="10" placeholder="發貨日期" autocomplete="off"></asp:TextBox>
                                        <i class="calendar alternate outline icon"></i>
                                    </div>
                                </div>
                                <div class="ten wide required field">
                                    <label>貨運公司&nbsp;<span class="grey-text text-darken-2">(跳出清單後，請選擇清單中的項目)</span></label>
                                    <div class="ui fluid search ac-ShipComp">
                                        <div class="ui icon input">
                                            <asp:TextBox ID="tb_ShipComp" runat="server" CssClass="prompt" placeholder="輸入關鍵字,並選擇清單中的項目"></asp:TextBox>
                                            <i class="search icon"></i>
                                        </div>
                                    </div>
                                    <asp:TextBox ID="val_ShipComp" runat="server" Style="display: none"></asp:TextBox>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="six wide field">
                                    <label>物流途徑</label>
                                    <asp:DropDownList ID="ddl_ShipWay" runat="server">
                                        <asp:ListItem Value="A">客戶自提</asp:ListItem>
                                        <asp:ListItem Value="B">自行送貨</asp:ListItem>
                                        <asp:ListItem Value="C">其它</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="ten wide required field">
                                    <label>收貨人</label>
                                    <asp:TextBox ID="tb_ShipWho" runat="server" MaxLength="20" placeholder="填寫收貨人" autocomplete="off"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                        <div class="four wide field">
                            <label>備註</label>
                            <asp:TextBox ID="tb_Remark" runat="server" Rows="5" TextMode="MultiLine" MaxLength="200" placeholder="最多200字"></asp:TextBox>
                        </div>
                    </div>

                    <div class="ui grid">
                        <div class="four wide column">
                            <a href="<%=Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i>返回列表</a>
                        </div>
                        <div class="twelve wide column right aligned">
                            <button id="goShipComp" type="button" class="ui orange small button"><i class="shipping icon"></i>貨運公司維護</button>
                            <button id="doSaveThenStay" type="button" class="ui green small button"><i class="save icon"></i>存檔後,留在本頁</button>
                            <button id="doSave" type="button" class="ui green small button"><i class="save icon"></i>存檔後,返回列表</button>
                            <asp:Button ID="btn_Save" runat="server" Text="Button" OnClick="btn_Save_Click" Style="display: none;" />
                            <asp:Button ID="btn_SaveStay" runat="server" Text="Button" OnClick="btn_SaveStay_Click" Style="display: none;" />
                            <asp:HiddenField ID="hf_DataID" runat="server" />
                            <asp:HiddenField ID="hf_CustID" runat="server" />
                            <asp:HiddenField ID="hf_ErpFid" runat="server" />
                            <asp:HiddenField ID="hf_ErpSid" runat="server" />
                            <asp:HiddenField ID="hf_ShipFrom" runat="server" />

                        </div>
                    </div>
                </div>
                <div class="ui bottom attached red small message">
                    <ul class="list">
                        <li>以上資料若有變更，記得按下「存檔」，避免資料遺失。</li>
                    </ul>
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
                <div id="shipNoList" class="ui small form segment">
                    <div class="ui internally celled grid">
                        <div class="row">
                            <div class="ten wide column">
                                <asp:ListView ID="lvDetailList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDetailList_ItemCommand">
                                    <LayoutTemplate>
                                        <table class="ui celled compact small table">
                                            <thead>
                                                <tr>
                                                    <th>單號</th>
                                                    <th>件數</th>
                                                    <th>到付$</th>
                                                    <th>自付$</th>
                                                    <th>墊付$</th>
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
                                            <td>
                                                <input id="ShipNo<%#Eval("Data_ID") %>" type="text" maxlength="50" value="<%#Eval("ShipNo") %>" />
                                            </td>
                                            <td class="center aligned">
                                                <input id="ShipCnt<%#Eval("Data_ID") %>" type="number" autocomplete="off" maxlength="6" min="1" value="<%#Eval("ShipCnt") %>" />
                                            </td>
                                            <td class="center aligned">
                                                <input id="Pay1<%#Eval("Data_ID") %>" type="number" maxlength="8" value="<%#Eval("Pay1") %>" step="any" />
                                            </td>
                                            <td class="center aligned">
                                                <input id="Pay2<%#Eval("Data_ID") %>" type="number" maxlength="8" value="<%#Eval("Pay2") %>" step="any" />
                                            </td>
                                            <td class="center aligned">
                                                <input id="Pay3<%#Eval("Data_ID") %>" type="number" maxlength="8" value="<%#Eval("Pay3") %>" step="any" />
                                            </td>
                                            <td class="center aligned collapsing">
                                                <button type="button" class="btn-ShipNoSave ui small blue basic icon button" data-id="<%#Eval("Data_ID") %>" title="Save"><i id="icon<%#Eval("Data_ID") %>" class="save icon"></i></button>
                                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui mini orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <div class="ui placeholder segment">
                                            <div class="ui icon header">
                                                <i class="shipping fast icon"></i>
                                                尚未加入物流單號，請於「右方欄位」填寫
                                            </div>
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:ListView>
                            </div>
                            <div class="six wide column">
                                <div class="three fields">
                                    <div class="field">
                                        <label>到付$</label>
                                        <asp:TextBox ID="tb_Pay1" runat="server" MaxLength="8" placeholder="輸入數字" autocomplete="off" type="number" step="any">0</asp:TextBox>
                                    </div>
                                    <div class="field">
                                        <label>自付$</label>
                                        <asp:TextBox ID="tb_Pay2" runat="server" MaxLength="8" placeholder="輸入數字" autocomplete="off" type="number" step="any">0</asp:TextBox>
                                    </div>
                                    <div class="field">
                                        <label>墊付$</label>
                                        <asp:TextBox ID="tb_Pay3" runat="server" MaxLength="8" placeholder="輸入數字" autocomplete="off" type="number" step="any">0</asp:TextBox>
                                    </div>
                                </div>
                                <div class="fields">
                                    <div class="four wide field">
                                        <label>件數</label>
                                        <asp:TextBox ID="tb_ShipCnt" runat="server" MaxLength="6" placeholder="件數" autocomplete="off" type="number" min="1">1</asp:TextBox>
                                    </div>
                                    <div class="twelve wide required field">
                                        <label>物流單號</label>
                                        <asp:TextBox ID="tb_ShipNo" runat="server" MaxLength="40" placeholder="輸入物流單號" autocomplete="off"></asp:TextBox>
                                    </div>
                                </div>

                                <div class="field">
                                    <asp:Button ID="btn_SaveDetail" runat="server" Text="新增單號" CssClass="ui teal tiny button" OnClick="btn_SaveDetail_Click" />
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
                <div id="relList" class="ui small form segment">
                    <div class="ui internally celled grid">
                        <div class="row">
                            <div class="ten wide column">
                                <asp:ListView ID="lvRelList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvRelList_ItemCommand">
                                    <LayoutTemplate>
                                        <table class="ui celled compact small table">
                                            <thead>
                                                <tr>
                                                    <th>單號</th>
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
                                            <td>
                                                <strong><%#Eval("Erp_SO_FID") %>-<%#Eval("Erp_SO_SID") %></strong>
                                            </td>
                                            <td class="center aligned collapsing">
                                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui mini orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
                                                <asp:HiddenField ID="hf_RelID" runat="server" Value='<%#Eval("Rel_ID") %>' />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <div class="ui placeholder segment">
                                            <div class="ui icon header">
                                                <i class="dolly flatbed icon"></i>
                                                目前未有合併單
                                            </div>
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:ListView>
                            </div>
                            <div class="six wide column">
                                <div class="field">
                                    <label>關聯單據</label>
                                    <div class="ui fluid search ac-ErpNo">
                                        <div class="ui icon input">
                                            <asp:TextBox ID="filter_ErpNo" runat="server" CssClass="prompt" placeholder="輸入ERP單號"></asp:TextBox>
                                            <i class="search icon"></i>
                                        </div>
                                    </div>
                                    <asp:TextBox ID="val_ErpNo" runat="server" Style="display: none"></asp:TextBox>
                                    <asp:Button ID="btn_SaveRel" runat="server" OnClick="btn_SaveRel_Click" Style="display: none;" />
                                </div>
                                <div class="ui message">
                                    <div class="header">
                                        如何使用?
                                    </div>
                                    <ul class="list">
                                        <li>於「關聯單據」欄位, 輸入ERP單號, 系統會帶出符合條件的清單</li>
                                        <li class="red-text text-darken-1">清單內容為單據日30日內 / 未合併過 / 未輸入發貨資料</li>
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
    </div>
    <asp:PlaceHolder ID="ph_LockModal" runat="server" Visible="false">
        <!-- 已合併發貨，不可編輯 -->
        <div id="lockPage" class="ui small basic modal">
            <div class="ui icon header">
                <i class="archive icon"></i>
                此單已合併發貨
            </div>
            <div class="content">
                <p>此單已與「<asp:Literal ID="lt_CombineID" runat="server"></asp:Literal>」合併發貨,故不可編輯, 請按下綠色按鈕回到列表頁.</p>
            </div>
            <div class="actions">
                <div class="ui green ok inverted button">
                    <i class="undo icon"></i>
                    返回
                </div>
            </div>
        </div>
    </asp:PlaceHolder>

    <!-- 貨運維護詢問框 Start -->
    <div id="confirmPage" class="ui small modal">
        <div class="header">
            確認是否已存檔
        </div>
        <div class="content">
            <p>即將離開本頁, 若有未存檔的資料, 請按下「<strong class="red-text">關閉視窗</strong>」, 並執行存檔.</p>
            <p>若要繼續前往貨運公司維護，請按下「<strong class="green-text">繼續前往</strong>」.</p>
        </div>
        <div class="actions">
            <div class="ui negative button">
                關閉視窗
            </div>
            <a class="ui positive right labeled icon button" href="<%=fn_Param.WebUrl %>myShipping/ShipComp.aspx?comp=<%=Req_CompID %>&back=<%=Server.UrlEncode(thisPage) %>">繼續前往<i class="chevron right icon"></i>
            </a>
        </div>
    </div>
    <!-- 貨運維護詢問框 End -->

    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <asp:PlaceHolder ID="ph_LockScript" runat="server" Visible="false">
        <script>
            //Lock顯示(Modal), 後端控制顯示與否.
            $('#lockPage').modal({
                closable: false,
                onApprove: function () {
                    window.location.href = '<%=Page_SearchUrl%>';
                }
            }).modal('show');

        </script>
    </asp:PlaceHolder>
    <script>
        $(function () {
            //init dropdown list
            $('select').dropdown();

            //[觸發][SAVE鈕]
            $("#doSave").click(function () {
                $("#MainContent_btn_Save").trigger("click");
            });
            $("#doSaveThenStay").click(function () {
                $("#MainContent_btn_SaveStay").trigger("click");
            });

            //貨運維護連結(Modal)
            $("#goShipComp").click(function () {
                $('#confirmPage').modal('show');
            });
        });
    </script>

    <%-- 物流單Update Ajax Start --%>
    <script>
        $(function () {
            //按鈕 - 儲存
            $(".btn-ShipNoSave").click(function () {
                //取得編號
                var id = $(this).attr("data-id");

                //取得輸入值
                var _ParentID = '<%=Req_DataID%>';
                var _DataID = id;
                var _ShipNo = $("#ShipNo" + id).val();
                var _ShipCnt = $("#ShipCnt" + id).val();
                var _Pay1 = $("#Pay1" + id).val();
                var _Pay2 = $("#Pay2" + id).val();
                var _Pay3 = $("#Pay3" + id).val();

                //Check null
                if (_ShipNo == "" || _ShipCnt == "") {
                    alert('資料未填寫');
                    return false;
                }

                //其他欄位
                var saveBtn = $(this);
                var saveIcon = $("#icon" + id);
                //button 加入loading
                saveBtn.addClass("loading");

                var request = $.ajax({
                    url: '<%=fn_Param.WebUrl%>' + "myShipping/Ashx_UpdateShipNo.ashx",
                    method: "POST",
                    data: {
                        ParentID: _ParentID,
                        DataID: _DataID,
                        ShipNo: _ShipNo,
                        ShipCnt: _ShipCnt,
                        Pay1: _Pay1,
                        Pay2: _Pay2,
                        Pay3: _Pay3
                    },
                    dataType: "html"
                });

                request.done(function (msg) {
                    //button 移除loading
                    saveBtn.removeClass("loading");

                    if (msg == "success") {
                        //顯示成功訊息
                        saveBtn.removeClass("blue").addClass("green");
                        saveIcon.removeClass("save").removeClass("exclamation").addClass("check");

                    } else {
                        event.preventDefault();
                        alert('資料存檔失敗!');
                        saveBtn.removeClass("blue").addClass("red");
                        saveIcon.removeClass("save").removeClass("check").addClass("exclamation");
                    }
                });

                request.fail(function (jqXHR, textStatus) {
                    event.preventDefault();
                    alert('存檔時發生錯誤，請連絡系統管理人員!');
                    saveBtn.removeClass("loading");
                });

                request.always(function () {
                    //do something
                });

            });
        });
    </script>
    <%-- 物流單Update Ajax Start --%>

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
        var pubCustID = $("#MainContent_hf_CustID").val();
        /* 關聯單號 (一般查詢) */
        $('.ac-ErpNo').search({
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
                $("#MainContent_val_ErpNo").val(result.ID);
                //trigger save
                $("#MainContent_btn_SaveRel").trigger("click");

            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_ErpSno.ashx?q={query}&comp=<%=Req_CompID%>&custID=' + pubCustID
            }

        });
    </script>
    <script>
        /* 貨運公司 */
        $('.ac-ShipComp').search({
            minCharacters: 1,
            fields: {
                results: 'results',
                title: 'Label'
            },
            searchFields: [
                'title'
            ]
            , onSelect: function (result, response) {
                $("#MainContent_val_ShipComp").val(result.ID);

            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_ShipComp.ashx?q={query}&comp=<%=Req_CompID%>&v=<%=Req_ErpNo%>'
            }

        });
    </script>
    <%-- Search UI End --%>
</asp:Content>

