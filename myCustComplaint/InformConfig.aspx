<%@ Page Title="通知群組設定" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="InformConfig.aspx.cs" Inherits="myCustComplaint_InformConfig" %>

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
                    <h5 class="active section">
                        <asp:Literal ID="lt_TypeName" runat="server" Text="功能名稱"></asp:Literal>
                        - 通知群組設定
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="item" href="<%=Page_SearchUrl %>">
                    <i class="undo icon"></i>
                    <span class="mobile hidden">返回</span>
                </a>
                <a class="item" id="tips">
                    <i class="question circle icon"></i>
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
                    <div class="three wide field">
                        <label><%:GetLocalResourceObject("sh_狀態")%></label>
                        <asp:DropDownList ID="filter_FlowStatus" runat="server" CssClass="fluid">
                        </asp:DropDownList>
                    </div>
                    <div class="five wide field">
                        <label>通知者</label>
                        <div class="ui fluid search ac-Employee">
                            <div class="ui left icon right labeled input">
                                <asp:TextBox ID="filter_Emp" runat="server" CssClass="prompt" placeholder="輸入關鍵字:部門 / 人名 / 工號"></asp:TextBox>
                                <i class="search icon"></i>
                                <asp:Panel ID="lb_Emp" runat="server" CssClass="ui label">輸入關鍵字,選擇項目</asp:Panel>
                            </div>
                            <asp:TextBox ID="val_Emp" runat="server" Style="display: none"></asp:TextBox>
                        </div>
                    </div>
                    <div class="three wide field">
                        <label>Email</label>
                        <asp:TextBox ID="val_EMail" runat="server"></asp:TextBox>
                    </div>
                    <div class="five wide field">
                        <label>&nbsp;</label>
                        <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i><%:resPublic.btn_Search%></button>
                        <a href="<%=FuncPath() %>/Inform" class="ui small button"><i class="refresh icon"></i><%:resPublic.btn_Reset%></a>
                        <asp:Button ID="btn_Search" runat="server" Text="search" OnClick="btn_Search_Click" Style="display: none" />
                        <asp:Button ID="btn_Insert" runat="server" Text="新增" CssClass="ui green small button" OnClick="btn_Insert_Click" />
                    </div>
                </div>
            </div>
        </div>
        <!-- Search End -->

        <!-- Empty Content Start -->
        <asp:PlaceHolder ID="ph_EmptyData" runat="server" Visible="false">
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
                        </div>
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>
        <!-- Empty Content End -->

        <!-- List Content Start -->
        <asp:PlaceHolder ID="ph_Data" runat="server">
            <div class="ui green attached segment">
                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                    <LayoutTemplate>
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th class="collapsing">系統編號</th>
                                    <th>所屬流程</th>
                                    <th>通知者</th>
                                    <th>Email</th>
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
                            <td class="center aligned">
                                <%#Eval("Data_ID") %>
                            </td>
                            <td class="center aligned">
                                <b class="blue-text text-darken-2"><%#Eval("FlowName") %> (<%#Eval("FlowID") %>)</b>
                            </td>
                            <td>
                                <strong><%#Eval("DisplayName") %></strong>
                            </td>
                            <td>
                                <div><%#Eval("Email") %></div>
                            </td>
                            <td class="collapsing">
                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
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

        <!-- Tips Start -->
        <div id="tipPage" class="ui modal">
            <div class="header">
                說明文件
            </div>
            <div class="content">
                <ul class="ui list">
                    <li class="red-text text-darken-1">不同的來源需各自設定名單(台灣工具/中國工具/....)</li>
                    <li>一線判斷 / 二線維修 / 業務確認 / 資材寄貨：通知該關卡人員</li>
                    <li>結案關卡：通知所有人</li>
                </ul>
            </div>
            <div class="actions">
                <div class="ui cancel button">
                    關閉視窗
                </div>
            </div>
        </div>
        <!-- Tips End -->
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

            $("#MainContent_filter_Emp").focus(function () {
                $(this).select();
            });

            //說明視窗(Modal)
            $("#tips").click(function () {
                $('#tipPage').modal('show');
            });

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
                $("#MainContent_val_Emp").val(result.Guid);
                $("#MainContent_val_EMail").val(result.email);
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
                            Guid: item.Guid
                        });
                    });
                    return response;
                }
            }

        });
    </script>
</asp:Content>

