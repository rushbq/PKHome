<%@ Page Title="部門資料維護" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Edit.aspx.cs" Inherits="myDepts_Edit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">系統管理</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">部門資料
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="anchor" id="top"></a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui grid">
            <div class="row">
                <!-- Left Body Content Start -->
                <div id="myStickyBody" class="thirteen wide column">
                    <div class="ui attached segment grey-bg lighten-5">
                        <!-- Section-基本資料 Start -->
                        <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
                            <div class="ui negative message">
                                <div class="header">
                                    Oops!!
                                </div>
                                <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                            </div>
                        </asp:PlaceHolder>
                        <div class="ui segments">
                            <div class="ui green segment">
                                <h5 class="ui header"><a class="anchor" id="baseData"></a>基本資料</h5>
                            </div>
                            <div id="formBase" class="ui small form segment">
                                <div class="fields">
                                    <div class="four wide field">
                                        <label>區域</label>
                                        <asp:DropDownList ID="ddl_Area" runat="server">
                                            <asp:ListItem Value="TW">台灣</asp:ListItem>
                                            <asp:ListItem Value="SH">上海</asp:ListItem>
                                            <asp:ListItem Value="SZ">深圳</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="four wide field">
                                        <label>部門代號</label>
                                        <asp:TextBox ID="tb_DeptID" runat="server" MaxLength="5" placeholder="部門代號" autocomplete="off"></asp:TextBox>
                                    </div>
                                    <div class="eight wide field">
                                        <label>部門名稱</label>
                                        <asp:TextBox ID="tb_DeptName" runat="server" MaxLength="20" placeholder="部門名稱" autocomplete="off"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="fields">
                                    <div class="four wide field">
                                        <label>是否顯示</label>
                                        <asp:RadioButtonList ID="rbl_Display" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="Y" Selected="True">顯示&nbsp;</asp:ListItem>
                                            <asp:ListItem Value="N">隱藏</asp:ListItem>
                                        </asp:RadioButtonList>
                                    </div>
                                    <div class="four wide field">
                                        <label>對應ERP代號</label>
                                        <asp:TextBox ID="tb_ErpID" runat="server" MaxLength="5" placeholder="部門代號" autocomplete="off"></asp:TextBox>
                                    </div>
                                    <div class="eight wide field">
                                        <label>部門Email</label>
                                        <asp:TextBox ID="tb_Email" runat="server" MaxLength="100" placeholder="部門Email" autocomplete="off"></asp:TextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="ui right aligned segment">
                                <button id="doSaveBase" type="button" class="ui green small button">
                                    <i class="save icon"></i>儲存設定
                                </button>
                                <asp:Button ID="btn_doSaveBase" runat="server" Text="Save" OnClick="btn_doSaveBase_Click" Style="display: none;" />

                            </div>
                            <%-- <div class="ui bottom attached info small message">
                                <ul>
                                    <li>設定狀態=資料設定中, 可任意修改資料。</li>
                                    <li>設定狀態=加入排程, 無法修改, 資料進入排程。</li>
                                    <li class="red-text"><b>所有資料設定完畢後, 記得將狀態設為「加入排程」, 並按下「修改設定」,才會正式進入發送Mail的排程。</b></li>
                                </ul>
                            </div>--%>
                        </div>
                        <!-- Section-基本資料 End -->

                        <asp:PlaceHolder ID="ph_Details" runat="server" Visible="false">
                            <!-- Section-主管清單 Start -->
                            <div class="ui segments">
                                <div class="ui brown segment">
                                    <h5 class="ui header"><a class="anchor" id="section1"></a>主管清單</h5>
                                </div>
                                <div class="ui segment">
                                    <div class="ui small form">
                                        <div class="two fields">
                                            <div class="field">
                                                <div class="ui fluid search ac-Employee">
                                                    <div class="ui right labeled input">
                                                        <asp:TextBox ID="filter_Emp" runat="server" CssClass="prompt"></asp:TextBox>
                                                        <asp:Panel ID="lb_Emp" runat="server" CssClass="ui label">查詢工號或名稱</asp:Panel>
                                                    </div>
                                                    <asp:TextBox ID="val_Emp" runat="server" Style="display: none"></asp:TextBox>
                                                </div>
                                            </div>
                                            <div class="field">
                                                <asp:Button ID="btn_SaveDetail" runat="server" Text="新增" CssClass="ui teal small button" OnClick="btn_SaveDetail_Click" />
                                            </div>
                                        </div>
                                    </div>

                                    <asp:ListView ID="lv_Suplist" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lv_Suplist_ItemCommand">
                                        <LayoutTemplate>
                                            <table class="ui celled selectable compact small table">
                                                <thead>
                                                    <tr>
                                                        <th class="grey-bg lighten-3 center aligned">工號</th>
                                                        <th class="grey-bg lighten-3 center aligned">人名</th>
                                                        <th class="grey-bg lighten-3"></th>
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
                                                    <%#Eval("AccountName") %>
                                                </td>
                                                <td class="center aligned"><%#Eval("DisplayName") %></td>

                                                <td class="center aligned collapsing">
                                                    <asp:LinkButton ID="lbtn_Del" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doDel" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                                                    <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("UID") %>' />
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                        <EmptyDataTemplate>
                                            <div class="ui placeholder segment">
                                                <div class="ui icon header">
                                                    <i class="coffee icon"></i>
                                                    查無資料
                                                </div>
                                            </div>
                                        </EmptyDataTemplate>
                                    </asp:ListView>
                                </div>
                            </div>
                            <!-- Section-主管清單 End -->

                            <!-- Section-部門成員 Start -->
                            <div class="ui segments">
                                <div class="ui blue segment">
                                    <h5 class="ui header"><a class="anchor" id="section2"></a>部門成員</h5>
                                </div>
                                <div class="ui segment">
                                    <asp:ListView ID="lv_Emplist" runat="server" ItemPlaceholderID="ph_Items">
                                        <LayoutTemplate>
                                            <table id="table1" class="ui celled selectable compact small table">
                                                <thead>
                                                    <tr>
                                                        <th class="grey-bg lighten-3 center aligned" colspan="5">人員</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <asp:PlaceHolder ID="ph_Items" runat="server" />
                                                </tbody>
                                            </table>
                                        </LayoutTemplate>
                                        <ItemTemplate>
                                            <tr>
                                                <td class="center aligned"><%#Eval("ProfID") %></td>
                                                <td class="center aligned"><%#Eval("ProfName") %></td>
                                                <td class="center aligned"><%#Eval("NickName") %></td>
                                                <td class="center aligned"><%#Eval("Email") %></td>
                                                <td class="center aligned">#<%#Eval("Tel_Ext") %></td>
                                            </tr>
                                        </ItemTemplate>
                                        <EmptyDataTemplate>
                                            <div class="ui placeholder segment">
                                                <div class="ui icon header">
                                                    <i class="coffee icon"></i>
                                                    查無資料
                                                </div>
                                            </div>
                                        </EmptyDataTemplate>
                                    </asp:ListView>
                                </div>
                            </div>
                            <!-- Section-部門成員 End -->

                        </asp:PlaceHolder>
                    </div>

                </div>
                <!-- Left Body Content End -->

                <!-- Right Navi Menu Start -->
                <div class="three wide column">
                    <div class="ui sticky">
                        <div id="fastjump" class="ui secondary vertical pointing fluid text menu">
                            <div class="header item">快速跳轉<i class="dropdown icon"></i></div>
                            <a href="#baseData" class="item">基本設定</a>
                            <a href="#section1" class="item">主管清單</a>
                            <a href="#section2" class="item">部門成員</a>
                            <a href="#top" class="item"><i class="angle double up icon"></i>到頂端</a>
                        </div>

                        <div class="ui vertical text menu">
                            <div class="header item">功能按鈕</div>
                            <div class="item">
                                <a href="<%:Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i>返回列表</a>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- Right Navi Menu End -->
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
            //Save Click
            $("#doSaveBase").click(function () {
                $("#formBase").addClass("loading");
                $("#MainContent_btn_doSaveBase").trigger("click");
            });

            //init dropdown list
            $('select').dropdown();
        });
    </script>

    <%-- 快速選單 --%>
    <script src="<%=fn_Param.WebUrl %>javascript/sticky.js"></script>

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
</asp:Content>

