<%@ Page Title="年度(編輯)" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="EditByYear.aspx.cs" Inherits="myECdata_SH_EditByYear" %>

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
                    <div class="section"><%:resPublic.fun_上海電商平台業績 %></div>
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
                        <div class="eight wide field">
                            <label>年度</label>
                            <asp:DropDownList ID="ddl_Year" runat="server" CssClass="fluid">
                            </asp:DropDownList>
                        </div>
                        <div class="eight wide field">
                            <label>
                                <asp:Literal ID="lt_PlatformType" runat="server"></asp:Literal>平台</label>
                            <asp:DropDownList ID="ddl_Mall" runat="server" CssClass="fluid">
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="fields">
                        <div class="eight wide field">
                            <label>結算銷售金額</label>
                            <asp:TextBox ID="tb_Price_Sales" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any"></asp:TextBox>
                        </div>
                        <div class="eight wide field">
                            <label>結算返利</label>
                            <asp:TextBox ID="tb_Price_Rebate" runat="server" MaxLength="10" placeholder="" autocomplete="off" type="number" step="any"></asp:TextBox>
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

            //lock
            $('input').on("cut copy paste", function (e) {
                e.preventDefault();
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
            $('.datepicker').calendar(calendarOptsByTime_Range);
        });
    </script>
    <%-- 日期選擇器 End --%>
</asp:Content>

