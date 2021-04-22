<%@ Page Title="訂單備註內容" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SalesOrderEdit.aspx.cs" Inherits="myOpcsRemark_SalesOrderEdit" %>

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
                    <div class="section">訂單作業</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">訂單備註&nbsp;(<%:Req_DBS %>)
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
                                    <%:resPublic.error_oops %>
                                </div>
                                <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                            </div>
                        </asp:PlaceHolder>
                        <div class="ui segments">
                            <div class="ui green segment">
                                <h5 class="ui header"><a class="anchor" id="baseData"></a>備註資料</h5>
                            </div>
                            <div class="ui form segment">
                                <div class="two fields">
                                    <div class="field">
                                        <label>單號</label>
                                        <div class="ui orange basic large label">
                                            <asp:Literal ID="lt_SO_FID" runat="server"></asp:Literal>
                                            -
                                            <asp:Literal ID="lt_SO_SID" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                    <div class="field">
                                        <label>客戶</label>
                                        <div class="ui green basic large label">
                                            <asp:Literal ID="lt_CustID" runat="server"></asp:Literal>
                                            -
                                            <asp:Literal ID="lt_CustName" runat="server"></asp:Literal>
                                        </div>
                                        <a href="<%=FuncPath() %>CustEdit.aspx?dbs=<%:Req_DBS %>&Cust=<%:lt_CustID.Text %>" target="_blank">
                                            查看
                                        </a>
                                    </div>
                                </div>
                                <div class="field">
                                    <label>訂單備註</label>
                                    <asp:TextBox ID="tb_Remk_Normal" runat="server" MaxLength="200" TextMode="MultiLine" Rows="14"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                        <!-- Section-基本資料 End -->

                        <!-- Section-檔案附件 Start -->
                        <div class="ui segments">
                            <div class="ui blue segment">
                                <h5 class="ui header"><a class="anchor" id="attfiles"></a>檔案附件</h5>
                            </div>
                            <div class="ui segment">
                                <div class="ui small form">
                                    <div class="fields">
                                        <div class="sixteen wide field">
                                            <label>附加檔案&nbsp;<span class="grey-text text-darken-1">(單次上傳最多 5 筆, 大小為 10MB, 限 <%=FileExtLimit.Replace("|", ", ") %>)</span></label>
                                            <div class="fields">
                                                <div class="thirteen wide field">
                                                    <asp:FileUpload ID="fu_Attachment" runat="server" AllowMultiple="true" />
                                                </div>
                                                <div class="three wide field">
                                                    <asp:Button ID="btn_SaveDetail" runat="server" Text="新增" CssClass="ui teal tiny button" OnClick="btn_SaveDetail_Click" />
                                                </div>
                                            </div>

                                        </div>
                                    </div>
                                </div>
                                <asp:ListView ID="lv_Attachment" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lv_Attachment_ItemCommand">
                                    <LayoutTemplate>
                                        <table class="ui celled striped small table">
                                            <thead>
                                                <tr>
                                                    <th>原始檔名</th>
                                                    <th>上傳者</th>
                                                    <th>&nbsp;</th>
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
                                                <a href="<%#fn_Param.RefUrl %><%#UploadFolder %><%=hf_SO_FID.Value %><%=hf_SO_SID.Value %>/<%#Eval("AttachFile") %>" target="_blank"><%#Eval("AttachFile_Org") %></a>
                                            </td>
                                            <td class="collapsing grey-text text-darken-2"><%#Eval("Create_Name") %></td>
                                            <td class="collapsing center aligned">
                                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui mini orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("AttachID") %>' />
                                                <asp:HiddenField ID="hf_FileName" runat="server" Value='<%#Eval("AttachFile") %>' />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:ListView>
                            </div>
                        </div>
                        <!-- Section-檔案附件 End -->

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
                <!-- Left Body Content End -->

                <!-- Right Navi Menu Start -->
                <div class="three wide column">
                    <div class="ui sticky">
                        <div id="fastjump" class="ui secondary vertical pointing fluid text menu">
                            <div class="header item">快速跳轉<i class="dropdown icon"></i></div>
                            <a href="#baseData" class="item">備註資料</a>
                            <a href="#attfiles" class="item">檔案附件</a>
                            <a href="#top" class="item"><i class="angle double up icon"></i>到頂端</a>
                        </div>

                        <div class="ui vertical text menu">

                            <div class="header item">功能按鈕</div>
                            <div class="item">
                                <a href="<%:Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i>返回列表</a>
                            </div>
                            <div class="item">
                                <button id="doSave" type="button" class="ui green small button"><i class="save icon"></i>資料存檔</button>
                                <asp:Button ID="btn_doSave" runat="server" Text="Save" OnClick="btn_doSave_Click" Style="display: none;" />
                                <asp:HiddenField ID="hf_SO_FID" runat="server" />
                                <asp:HiddenField ID="hf_SO_SID" runat="server" />
                                <asp:HiddenField ID="hf_DataID" runat="server" />
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
            //[觸發][Save按鈕]
            $("#doSave").click(function () {
                //lock button
                $(this).addClass('loading').addClass('disabled');
                //lock page
                $("#myStickyBody").addClass("loading");

                //觸發ServerControll Button
                $("#MainContent_btn_doSave").trigger("click");
            });

        });
    </script>

    <%-- 快速選單 --%>
    <script src="<%=fn_Param.WebUrl %>javascript/sticky.js"></script>
</asp:Content>

