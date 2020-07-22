<%@ Page Title="快遞貨運登記" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Edit.aspx.cs" Inherits="myDelivery_Edit" %>

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
                    <div class="section">快遞貨運登記</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        登記內容
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui attached segment grey-bg lighten-5">
            <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
                <div class="ui negative message">
                    <div class="header">
                        Oops...發生了一點小問題
                    </div>
                    <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                </div>
            </asp:PlaceHolder>


            <div id="formData" class="ui small form green segment">
                <!-- 基本資料 Start -->
                <div class="fields">
                    <div class="five wide field">
                        <label>登記單號</label>
                        <asp:Label ID="lb_TraceID" runat="server" CssClass="ui red basic large label">系統自動產生</asp:Label>
                    </div>
                    <div class="eleven wide field">
                        <label>類別</label>
                        <asp:RadioButtonList ID="rbl_ShipType" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem Value="1" Selected="True">&nbsp;寄件</asp:ListItem>
                            <asp:ListItem Value="2">&nbsp;收件</asp:ListItem>
                            <asp:ListItem Value="3">&nbsp;來回件</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                </div>
                <div class="fields">
                    <div class="five wide required field">
                        <label>寄送方式</label>
                        <asp:DropDownList ID="ddl_ShipWay" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="five wide required field">
                        <label>運費付款方式</label>
                        <asp:DropDownList ID="ddl_PayWay" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="six wide required field">
                        <label>登記人員</label>
                        <div class="ui fluid search ac-Employee">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Emp" runat="server" CssClass="prompt"></asp:TextBox>
                                <div class="ui label">
                                    <asp:Label ID="lb_Emp" runat="server"></asp:Label>
                                </div>
                            </div>
                            <asp:TextBox ID="val_Emp" runat="server" Style="display: none"></asp:TextBox>
                        </div>
                    </div>
                </div>
                <!-- 基本資料 End -->
                <!-- 收件者資料 Start -->
                <h4 class="ui horizontal divider header pink-text text-darken-2">
                    <i class="shipping fast icon"></i>
                    收件者資料
                </h4>
                <div class="two fields">
                    <!-- left section -->
                    <div class="field">
                        <div class="fields">
                            <div class="eight wide field">
                                <label>指定寄件日</label>
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="tb_SendDate" runat="server" MaxLength="10" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="eight wide field">
                                <label>公司名稱</label>
                                <asp:TextBox ID="tb_SendComp" runat="server" MaxLength="50" placeholder="公司名稱"></asp:TextBox>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="eight wide field">
                                <label>物流單號</label>
                                <asp:TextBox ID="tb_ShipNo" runat="server" MaxLength="40" placeholder="填入物流單號"></asp:TextBox>
                            </div>
                            <div class="eight wide field">
                                <label>運費</label>
                                <asp:TextBox ID="tb_ShipPay" runat="server" MaxLength="5" type="number" min="0" placeholder="填入數字">0</asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <!-- right section -->
                    <div class="field">
                        <div class="fields">
                            <div class="eight wide field">
                                <label>收件者</label>
                                <asp:TextBox ID="tb_SendWho" runat="server" MaxLength="40" placeholder="名稱"></asp:TextBox>
                            </div>
                            <div class="eight wide field">
                                <label>收件電話</label>
                                <asp:TextBox ID="tb_SendAddr" runat="server" MaxLength="10" placeholder="電話"></asp:TextBox>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="sixteen wide field">
                                <label>收件地址</label>
                                <asp:TextBox ID="tb_SendTel" runat="server" MaxLength="80" placeholder="地址"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- 收件者資料 End -->

                <!-- 內容物資料 Start -->
                <h4 class="ui horizontal divider header green-text text-darken-3">
                    <i class="archive icon"></i>
                    內容物資料
                </h4>
                <div class="two fields">
                    <!-- left section -->
                    <div class="field">
                        <div class="fields">
                            <div class="eight wide field">
                                <label>總寄件箱數</label>
                                <asp:TextBox ID="tb_Box" runat="server" MaxLength="5" type="number" min="0" placeholder="填入數字"></asp:TextBox>
                            </div>
                            <div class="eight wide field">
                                <label>對象分類</label>
                                <asp:DropDownList ID="ddl_TargetClass" runat="server" CssClass="fluid"></asp:DropDownList>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="eight wide field">
                                <label>內容分類1</label>
                                <asp:DropDownList ID="ddl_BoxClass1" runat="server" CssClass="fluid"></asp:DropDownList>
                            </div>
                            <div class="eight wide field">
                                <label>內容分類2</label>
                                <asp:DropDownList ID="ddl_BoxClass2" runat="server" CssClass="fluid"></asp:DropDownList>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="sixteen wide field">
                                <label>說明</label>
                                <asp:TextBox ID="tb_Remark1" runat="server" Rows="4" TextMode="MultiLine" MaxLength="500"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <!-- right section -->
                    <div class="field">
                        <div class="fields">
                            <div class="eight wide field">
                                <label>採購單號(單別-單號)</label>
                                <asp:TextBox ID="tb_PurNo" runat="server" MaxLength="30"></asp:TextBox>
                            </div>
                            <div class="eight wide field">
                                <label>銷貨單號(單別-單號)</label>
                                <asp:TextBox ID="tb_SaleNo" runat="server" MaxLength="30"></asp:TextBox>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="eight wide field">
                                <label>INVOICE</label>
                                <asp:TextBox ID="tb_InvoiceNo" runat="server" MaxLength="30"></asp:TextBox>
                            </div>
                            <div class="eight wide field">
                            </div>
                        </div>
                        <div class="fields">
                            <div class="sixteen wide field">
                                <label>備註</label>
                                <asp:TextBox ID="tb_Remark2" runat="server" Rows="4" TextMode="MultiLine" MaxLength="500"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                </div>
                <!-- 內容物資料 End -->
            </div>

            <!-- button -->
            <div class="ui grid">
                <div class="three wide column">
                    <a href="<%=Page_SearchUrl %>" class="ui button"><i class="undo icon"></i>返回列表</a>
                </div>
                <div class="four wide column">
                    <asp:Button ID="btn_Close" runat="server" Text="結案" OnClientClick="return confirm('結案後使用者無法編輯,是否確定?')" CssClass="ui orange button" OnClick="btn_Close_Click" Visible="false" />
                </div>
                <div class="nine wide column right aligned">
                    <button id="doSave" type="button" class="ui green button"><i class="save icon"></i>存檔後,返回列表</button>
                    <button id="doSaveThenStay" type="button" class="ui teal button"><i class="save icon"></i>存檔後,留在本頁</button>
                    <a href="<%=FuncPath() %>/Edit" class="ui blue button">新增下一筆</a>
                    <asp:Button ID="btn_Save" runat="server" Text="next" OnClick="btn_Save_Click" Style="display: none;" />
                    <asp:Button ID="btn_SaveThenStay" runat="server" Text="next" OnClick="btn_SaveStay_Click" Style="display: none;" />
                </div>
                <asp:HiddenField ID="hf_DataID" runat="server" />
            </div>

        </div>
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

            //Save Click
            $("#doSave").click(function () {
                $("#formData").addClass("loading");
                $("#MainContent_btn_Save").trigger("click");
            });

            $("#doSaveThenStay").click(function () {
                $("#formData").addClass("loading");
                $("#MainContent_btn_SaveThenStay").trigger("click");
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
            $('.datepicker').calendar(calendarOpts_Range_unLimit);
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
                            description: item.Label,
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
</asp:Content>

