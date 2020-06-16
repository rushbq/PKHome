<%@ Page Title="發送對帳單Step1" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Step1.aspx.cs" Inherits="myARdata_ImportStep1" %>

<%@ Register Src="Ascx_StepMenu.ascx" TagName="Ascx_Menu" TagPrefix="ucMenu" %>
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
                    <div class="section">應收帳款對帳</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        發送對帳單 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
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
            <!-- Steps menu -->
            <ucMenu:Ascx_Menu ID="Ascx_Menu1" runat="server" nowIndex="1" />
            <!-- 基本資料 Start -->
            <div id="formData" class="ui small form attached green segment">
                <div class="two fields">
                    <div class="field">
                        <label>追蹤碼</label>
                        <asp:Label ID="lb_TraceID" runat="server" CssClass="ui red basic large label"></asp:Label>
                    </div>
                    <div class="field">
                        <label>資料庫</label>
                        <asp:Label ID="lb_DBS" runat="server" CssClass="ui blue basic large label"></asp:Label>
                    </div>
                </div>
                <div class="two fields">
                    <div class="required field">
                        <label>客戶</label>
                        <div class="ui fluid search ac-Cust">
                            <div class="ui right labeled input">
                                <asp:TextBox ID="filter_Cust" runat="server" CssClass="prompt" placeholder="輸入客戶代號或名稱關鍵字"></asp:TextBox>
                                <asp:Panel ID="lb_Cust" runat="server" CssClass="ui label">輸入關鍵字,選擇項目</asp:Panel>
                                <asp:TextBox ID="val_Cust" runat="server" Style="display: none;"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <div class="required field">
                        <label>單據日區間</label>
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
                                    <i class="calendar alternate icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="ui message">
                    <div class="header">注意事項</div>
                    <ul class="list">
                        <li>請先檢查發信名單已建立.(PKEF/客戶基本資料)</li>
                        <li>Step3為最終確認頁,發送後無法回溯.</li>
                        <li>結帳單資料皆為未確認.</li>
                    </ul>
                </div>

                <div class="ui grid">
                    <div class="six wide column">
                        <a href="<%=Page_SearchUrl %>" class="ui button"><i class="undo icon"></i>返回列表</a>
                    </div>
                    <div class="ten wide column right aligned">
                        <button id="doNext" type="button" class="ui green button">下一步<i class="chevron right icon"></i></button>
                        <asp:Button ID="btn_Next" runat="server" Text="next" OnClick="lbtn_Next_Click" Style="display: none;" />
                    </div>

                    <asp:HiddenField ID="hf_TraceID" runat="server" />
                </div>
            </div>
            <!-- 基本資料 End -->
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
            //$('select').dropdown();

            //Save Click
            $("#doNext").click(function () {
                $("#formData").addClass("loading");
                $("#MainContent_btn_Next").trigger("click");
            });

        });
    </script>
    <%-- Search UI Start --%>
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
                $("#MainContent_lb_Cust").text(result.Label);
                $("#MainContent_val_Cust").val(result.ID);
            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?dbs=<%=Req_CompID%>&q={query}'
            }

        });
    </script>
    <%-- Search UI End --%>
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
</asp:Content>

