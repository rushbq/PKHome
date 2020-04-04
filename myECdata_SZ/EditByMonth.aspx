<%@ Page Title="每月(編輯)" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="EditByMonth.aspx.cs" Inherits="myECdata_SZ_EditByMonth" %>

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
                    <div class="section"><%:resPublic.fun_深圳電商平台業績 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        <asp:Literal ID="lt_TypeName" runat="server" Text="功能名稱"></asp:Literal>
                    </div>
                </div>
            </div>
            <div class="right menu">
                <%--<a class="item" id="tips">
                    <i class="question circle icon"></i>
                </a>
                <a class="anchor" id="top"></a>--%>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui attached segment grey-bg lighten-5">
            <!-- Section-基本資料 Start -->
            <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
                <div class="ui negative message">
                    <div class="header">
                        <%:resPublic.error_oops %>
                    </div>
                    <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                </div>
            </asp:PlaceHolder>

            <!-- Section-基本資料 End -->

            <!-- Section-資料填寫 Start -->
            <div class="ui segments">
                <div class="ui green segment">
                    <h5 class="ui header">資料填寫
                    </h5>
                </div>
                <div class="ui small form segment">
                    <div class="fields">
                        <div class="four wide field">
                            <label>年度</label>
                            <asp:DropDownList ID="ddl_Year" runat="server" CssClass="fluid">
                            </asp:DropDownList>
                        </div>
                        <div class="four wide field">
                            <label>月份</label>
                            <asp:DropDownList ID="ddl_Month" runat="server" CssClass="fluid">
                            </asp:DropDownList>
                        </div>
                        <div class="four wide field">
                            <label>
                                <asp:Literal ID="lt_PlatformType" runat="server"></asp:Literal>平台</label>
                            <asp:DropDownList ID="ddl_Mall" runat="server" CssClass="fluid">
                            </asp:DropDownList>
                        </div>
                        <div class="four wide field">
                            <label>促銷費用</label>
                            <div class="ui basic fluid large label">
                                <asp:Literal ID="lt_Price_Promo" runat="server">存檔後填寫單身</asp:Literal>
                            </div>
                        </div>
                    </div>
                    <div class="four fields">
                        <div class="field">
                            <label>平台收入</label>
                            <asp:TextBox ID="tb_Price_Income" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any">0</asp:TextBox>
                        </div>
                        <div class="field">
                            <label>平台銷售返利</label>
                            <asp:TextBox ID="tb_Price_SalesRebate" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any">0</asp:TextBox>
                        </div>
                        <div class="field">
                            <label>平台成本每月</label>
                            <asp:TextBox ID="tb_Price_Cost" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any">0</asp:TextBox>
                        </div>
                        <div class="field">
                            <label>平台毛利保護</label>
                            <asp:TextBox ID="tb_Price_Profit" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any">0</asp:TextBox>
                        </div>
                    </div>
                    <div class="four fields">
                        <div class="field">
                            <label>平台採購金額</label>
                            <asp:TextBox ID="tb_Price_Purchase" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any">0</asp:TextBox>
                        </div>
                        <div class="field">
                            <label>平台退貨金額</label>
                            <asp:TextBox ID="tb_Price_Back" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any">0</asp:TextBox>
                        </div>
                        <div class="field">
                            <label>平台採購返利</label>
                            <asp:TextBox ID="tb_Price_PurchaseRebate" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any">0</asp:TextBox>
                        </div>
                        <div class="field">
                            <label>TC運費</label>
                            <asp:TextBox ID="tb_Price_Freight" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any">0</asp:TextBox>
                        </div>
                    </div>
                    <div class="ui two column grid">
                        <div class="column">
                            <a href="<%=Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i>返回列表</a>
                        </div>
                        <div class="column right aligned">
                            <button id="doSaveThenStay" type="button" class="ui green small button"><i class="save icon"></i>存檔後,留在本頁</button>
                            <button id="doSave" type="button" class="ui green small button"><i class="save icon"></i>存檔後,返回列表</button>
                            <asp:Button ID="btn_Save" runat="server" Text="Button" OnClick="btn_Save_Click" Style="display: none;" />
                            <asp:Button ID="btn_SaveStay" runat="server" Text="Button" OnClick="btn_SaveStay_Click" Style="display: none;" />
                            <asp:HiddenField ID="hf_DataID" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
            <!-- Section-資料填寫 End -->

            <!-- Section-促銷費用 Start -->
            <asp:PlaceHolder ID="ph_Promo" runat="server">
                <div class="ui segments">
                    <div class="ui blue segment">
                        <h5 class="ui header"><a class="anchor" id="promoData"></a>促銷費用</h5>
                    </div>
                    <div class="ui segment">
                        <!-- Edit block S -->
                        <div class="ui small form">
                            <div class="four fields">
                                <div class="field">
                                    <label>記錄日期</label>
                                    <div class="ui left icon input datepicker">
                                        <asp:TextBox ID="tb_RecordDate" runat="server" autocomplete="off"></asp:TextBox>
                                        <i class="calendar alternate outline icon"></i>
                                    </div>
                                </div>
                                <div class="field">
                                    <label>定額費用類型</label>
                                    <asp:DropDownList ID="ddl_RecordType" runat="server">
                                        <asp:ListItem Value="1">促銷費</asp:ListItem>
                                        <asp:ListItem Value="2">優惠卷</asp:ListItem>
                                        <asp:ListItem Value="3">非結構化返利</asp:ListItem>
                                        <asp:ListItem Value="999">其他</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="field">
                                    <label>金額欄位</label>
                                    <asp:TextBox ID="tb_RecordMoney" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any">0</asp:TextBox>
                                </div>
                                <div class="field">
                                </div>
                            </div>
                            <div class="four fields">
                                <div class="field">
                                    <label>財務對帳日</label>
                                    <div class="ui left icon input datepicker">
                                        <asp:TextBox ID="tb_CheckDate" runat="server" autocomplete="off"></asp:TextBox>
                                        <i class="calendar alternate outline icon"></i>
                                    </div>
                                </div>
                                <div class="field">
                                    <label>財務對帳金額</label>
                                    <asp:TextBox ID="tb_CheckMoney" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any"></asp:TextBox>
                                </div>
                                <div class="field">
                                    <label>財務開票時間</label>
                                    <div class="ui left icon input datepicker">
                                        <asp:TextBox ID="tb_CheckInvoiceDate" runat="server" autocomplete="off"></asp:TextBox>
                                        <i class="calendar alternate outline icon"></i>
                                    </div>
                                </div>
                                <div class="field">
                                    <asp:Button ID="btn_SaveDetail" runat="server" Text="新增資料" CssClass="ui teal small button" OnClick="btn_SaveDetail_Click" />
                                    <asp:HiddenField ID="hf_DetailID" runat="server" />
                                </div>
                            </div>
                        </div>
                        <!-- Edit block E -->
                        <div class="ui divider"></div>
                        <!-- List block S -->
                        <asp:ListView ID="lv_Detail" runat="server" ItemPlaceholderID="ph_Items">
                            <LayoutTemplate>
                                <table class="ui celled selectable compact small table">
                                    <thead>
                                        <tr>
                                            <th class="grey-bg lighten-3 center aligned">記錄日期</th>
                                            <th class="grey-bg lighten-3 center aligned">定額費用類型</th>
                                            <th class="grey-bg lighten-3 right aligned">金額欄位</th>
                                            <th class="grey-bg lighten-3 center aligned">財務對帳日</th>
                                            <th class="grey-bg lighten-3 right aligned">財務對帳金額</th>
                                            <th class="grey-bg lighten-3 center aligned">財務開票時間</th>
                                            <th class="grey-bg lighten-3" colspan="2"></th>
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
                                        <%#Eval("RecordDate") %>
                                    </td>
                                    <td class="center aligned">
                                        <%#fn_Menu.GetECData_PromoType(Convert.ToInt32(Eval("RecordType"))) %>
                                    </td>
                                    <td class="right aligned">
                                        <%#Eval("RecordMoney") %>
                                    </td>
                                    <td class="center aligned">
                                        <%#Eval("CheckDate") %>
                                    </td>
                                    <td class="right aligned">
                                        <%#Eval("CheckMoney") %>
                                    </td>
                                    <td class="center aligned">
                                        <%#Eval("CheckInvoiceDate") %>
                                    </td>
                                    <td class="left aligned collapsing">
                                        <div>
                                            <div class="ui small basic fluid label">
                                                建立<div class="detail"><%#Eval("Create_Time") %></div>
                                            </div>
                                        </div>
                                        <div>
                                            <div class="ui small basic fluid label">
                                                更新<div class="detail"><%#Eval("Update_Time") %></div>
                                            </div>
                                        </div>
                                    </td>
                                    <td class="center aligned collapsing">
                                        <a class="ui small teal basic icon button" href="<%=thisPage %>?sub=<%#Eval("Data_ID") %>#promoData" title="編輯">
                                            <i class="pencil icon"></i>
                                        </a>
                                        <%--<asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                    <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />--%>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <EmptyDataTemplate>
                                <div class="ui placeholder segment">
                                    <div class="ui icon header">
                                        <i class="coffee icon"></i>
                                        資料尚未填寫...
                                    </div>
                                </div>
                            </EmptyDataTemplate>
                        </asp:ListView>
                        <!-- List block E -->
                    </div>
                </div>
            </asp:PlaceHolder>
            <!-- Section-促銷費用 End -->

            <!-- Section-維護資訊 Start -->
            <div class="ui segments">
                <div class="ui grey segment">
                    <h5 class="ui header"><a class="anchor" id="infoData"></a>維護資訊</h5>
                </div>
                <div class="ui segment">
                    <table class="ui celled small four column table">
                        <thead>
                            <tr>
                                <th colspan="2" class="center aligned">建立</th>
                                <th colspan="2" class="center aligned">最後更新</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr class="center aligned">
                                <td>
                                    <asp:Literal ID="info_Creater" runat="server">資料建立中...</asp:Literal>
                                </td>
                                <td>
                                    <asp:Literal ID="info_CreateTime" runat="server">資料建立中...</asp:Literal>
                                </td>
                                <td>
                                    <asp:Literal ID="info_Updater" runat="server"></asp:Literal>
                                </td>
                                <td>
                                    <asp:Literal ID="info_UpdateTime" runat="server"></asp:Literal>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <!-- Section-維護資訊 End -->
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

            //[觸發][SAVE鈕]
            $("#doSaveThenStay").click(function () {
                $("#MainContent_btn_SaveStay").trigger("click");
            });
            //[觸發][SAVE鈕]
            $("#doSave").click(function () {
                $("#MainContent_btn_Save").trigger("click");
            });

            //focus then select text
            $("input").focus(function () {
                $(this).select();
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

