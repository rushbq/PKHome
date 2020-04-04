<%@ Page Title="客戶返利資料維護" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Edit.aspx.cs" Inherits="myRebate_Edit" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
    <style>
        .ui.search > .results .result .description {
            color: rgba(0, 0, 0, 0.7) !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">業務行銷</div>
                    <i class="right angle icon divider"></i>
                    <div class="section">客戶返利統計</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        資料維護
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
            <!-- 資料 Start -->
            <div class="ui segments">
                <div class="ui green segment">
                    <h5 class="ui header">基本資料
                    </h5>
                </div>
                <div class="ui small form segment">
                    <div class="fields">
                        <div class="five wide field">
                            <label>年度</label>
                            <asp:DropDownList ID="ddl_Year" runat="server">
                            </asp:DropDownList>
                        </div>
                        <div class="seven wide required field">
                            <label>客戶</label>
                            <div class="ui fluid search ac-Cust">
                                <div class="ui right labeled input">
                                    <asp:TextBox ID="tb_Cust" runat="server" CssClass="prompt" placeholder="輸入客戶代號或名稱關鍵字"></asp:TextBox>
                                    <asp:Panel ID="lb_Cust" runat="server" CssClass="ui label">
                                        <asp:Literal ID="lt_CustName" runat="server">輸入關鍵字,選擇項目</asp:Literal>
                                    </asp:Panel>
                                    <asp:TextBox ID="val_Cust" runat="server" Style="display: none"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                        <div class="four wide field">
                            <label>計算方式</label>
                            <asp:DropDownList ID="ddl_Formula" runat="server">
                                <asp:ListItem Value="A">算式A</asp:ListItem>
                                <asp:ListItem Value="B">算式B</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="fields">
                        <div class="eight wide field">
                            <div class="fields">
                                <div class="ten wide required field">
                                    <label>業績目標</label>
                                    <asp:TextBox ID="tb_RespMoney" runat="server" MaxLength="10" placeholder="填寫業績目標" autocomplete="off"></asp:TextBox>
                                </div>
                                <div class="six wide required field">
                                    <label>業績目標-回饋方式</label>
                                    <div class="ui right labeled input">
                                        <asp:TextBox ID="tb_RespPercent" runat="server" MaxLength="4" placeholder="填寫數字" autocomplete="off"></asp:TextBox>
                                        <div class="ui basic label">
                                            %
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="ten wide required field">
                                    <label>挑戰目標</label>
                                    <asp:TextBox ID="tb_FightMoney" runat="server" MaxLength="10" placeholder="填寫挑戰目標" autocomplete="off"></asp:TextBox>
                                </div>
                                <div class="six wide required field">
                                    <label>挑戰目標-回饋方式</label>
                                    <div class="ui right labeled input">
                                        <asp:TextBox ID="tb_FightPercent" runat="server" MaxLength="4" placeholder="填寫數字" autocomplete="off"></asp:TextBox>
                                        <div class="ui basic label">
                                            %
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="eight wide field">
                            <label>備註</label>
                            <asp:TextBox ID="tb_Remark" runat="server" Rows="5" TextMode="MultiLine" MaxLength="200" placeholder="最多200字"></asp:TextBox>
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
            <!-- 資料 End -->
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
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?q={query}'
            }

        });
    </script>
    <%-- Search UI End --%>
</asp:Content>

