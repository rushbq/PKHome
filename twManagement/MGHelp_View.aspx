<%@ Page Title="需求檢視頁" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="MGHelp_View.aspx.cs" Inherits="MGHelp_View" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">行政管理</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">工作需求登記
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="anchor" id="top" title="置頂用錨點"></a>
                <a href="<%:Page_SearchUrl %>" class="item"><i class="undo icon"></i><span class="mobile hidden">返回列表</span></a>
                <%--<a class="item" id="tips">
                    <i class="question circle icon"></i>
                </a>--%>
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
                        <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
                            <div class="ui negative message">
                                <div class="header">
                                    Oops..執行時發生了小狀況~
                                </div>
                                <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                            </div>
                        </asp:PlaceHolder>
                        <!-- [Section] 需求資料 Start -->
                        <div class="ui segments">
                            <asp:PlaceHolder ID="ph_AgreeArea" runat="server" Visible="false">
                                <div class="ui placeholder segment">
                                    <div class="ui icon header">
                                        <i class="gavel icon"></i>
                                        本案需要主管核准，請問是否同意?
                                    </div>
                                    <div class="inline">
                                        <asp:LinkButton ID="lbtn_No" runat="server" CssClass="ui grey button" OnClick="lbtn_No_Click"><i class="ban icon"></i>不同意</asp:LinkButton>
                                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                    <asp:LinkButton ID="lbtn_Yes" runat="server" CssClass="ui blue button" OnClick="lbtn_Yes_Click"><i class="check icon"></i>同意</asp:LinkButton>
                                    </div>
                                </div>
                            </asp:PlaceHolder>
                            <div class="ui green segment">
                                <h5 class="ui header"><a class="anchor" id="baseData"></a>需求資料</h5>
                            </div>
                            <div id="formBase" class="ui small form segment">
                                <!-- 基本資料 Start -->
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="two fields">
                                            <div class="field">
                                                <label>追蹤編號</label>
                                                <div class="ui red basic label">
                                                    <asp:Literal ID="lt_TraceID" runat="server"></asp:Literal>
                                                </div>
                                            </div>
                                            <div class="field">
                                                <label>登記日期</label>
                                                <div class="ui basic label">
                                                    <asp:Literal ID="lt_CreateDate" runat="server"></asp:Literal>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>報修方式</label>
                                                <div class="ui basic label">
                                                    <asp:Literal ID="rbl_Help_Way" runat="server"></asp:Literal>
                                                </div>
                                            </div>
                                            <div class="field">
                                                <label>需求者</label>
                                                <asp:Label ID="lb_Emp" runat="server" CssClass="ui large label"></asp:Label>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="two fields">
                                            <div class="field">
                                                <label>處理狀態</label>
                                                <asp:Literal ID="lt_ReqStatus" runat="server"></asp:Literal>
                                            </div>
                                            <div class="field">
                                                <label>申請類別</label>
                                                <div class="ui teal basic label">
                                                    <asp:Literal ID="lt_ReqClass" runat="server"></asp:Literal>
                                                </div>
                                            </div>
                                        </div>

                                        <!-- 主管同意 -->
                                        <asp:PlaceHolder ID="ph_Agree" runat="server" Visible="false">
                                            <div class="field">
                                                <label>主管同意狀態</label>
                                                <div class="ui grey label">
                                                    <asp:Literal ID="lt_AuthAgree" runat="server">未同意</asp:Literal>
                                                </div>
                                            </div>
                                        </asp:PlaceHolder>
                                    </div>
                                </div>
                                <!-- 主旨,內文 -->
                                <div class="fields">
                                    <div class="sixteen wide field" style="z-index: 1;">
                                        <div class="ui piled large segment">
                                            <h4 class="ui header">
                                                <asp:Literal ID="tb_ReqSubject" runat="server"></asp:Literal></h4>
                                            <strong class="brown-text text-darken-1" style="line-height: 1.6em;">
                                                <asp:Literal ID="tb_ReqContent" runat="server"></asp:Literal>
                                            </strong>
                                        </div>
                                    </div>
                                </div>
                                <asp:PlaceHolder ID="ph_Benefit" runat="server">
                                    <div class="fields">
                                        <div class="sixteen wide field">
                                            <div class="ui hidden divider"></div>
                                            <div class="ui raised segment">
                                                <h4 class="ui header">改善效益</h4>
                                                <p class="grey-text text-darken-3">
                                                    <asp:Literal ID="tb_Help_Benefit" runat="server"></asp:Literal>
                                                </p>
                                            </div>
                                        </div>
                                    </div>
                                </asp:PlaceHolder>

                                <!-- 基本資料 End -->
                                <!-- 附件&轉寄 Start -->
                                <div class="fields">
                                    <div class="sixteen wide field">
                                        <div class="ui secondary pointing small menu">
                                            <a class="item active" data-tab="tab-name1">
                                                <i class="paperclip icon"></i>附件清單
                                            </a>
                                            <a class="item" data-tab="tab-name2">
                                                <i class="envelope outline icon"></i>轉寄通知
                                            </a>
                                        </div>
                                        <!-- 附件清單 tab Start -->
                                        <div class="ui active tab" data-tab="tab-name1">
                                            <asp:ListView ID="lv_Attachment" runat="server" ItemPlaceholderID="ph_Items" GroupPlaceholderID="ph_Group" GroupItemCount="2" OnItemDataBound="lv_Attachment_ItemDataBound">
                                                <LayoutTemplate>
                                                    <table class="ui celled table">
                                                        <tbody>
                                                            <asp:PlaceHolder ID="ph_Group" runat="server" />
                                                        </tbody>
                                                    </table>
                                                </LayoutTemplate>
                                                <GroupTemplate>
                                                    <tr>
                                                        <asp:PlaceHolder ID="ph_Items" runat="server" />
                                                    </tr>
                                                </GroupTemplate>
                                                <ItemTemplate>
                                                    <td style="width: 50%">
                                                        <asp:Literal ID="lt_FileUrl" runat="server"></asp:Literal>
                                                    </td>
                                                </ItemTemplate>
                                                <EmptyItemTemplate>
                                                    <td>&nbsp;</td>
                                                </EmptyItemTemplate>
                                                <EmptyDataTemplate>
                                                    <div class="ui message">
                                                        <div class="content">
                                                            <div class="header">沒有附件可以參考</div>
                                                        </div>
                                                    </div>
                                                </EmptyDataTemplate>
                                            </asp:ListView>
                                        </div>
                                        <!-- 附件清單 tab End -->

                                        <!-- 轉寄通知 tab Start -->
                                        <div class="ui tab" data-tab="tab-name2">
                                            <div class="ui grid">
                                                <div class="row">
                                                    <div class="sixteen wide column">
                                                        <asp:ListView ID="lv_Inform" runat="server" ItemPlaceholderID="ph_Items">
                                                            <LayoutTemplate>
                                                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                                                            </LayoutTemplate>
                                                            <ItemTemplate>
                                                                <div class="ui basic label">
                                                                    <%#Eval("CC_Who") %>
                                                                </div>
                                                            </ItemTemplate>
                                                            <EmptyDataTemplate>
                                                                <p class="grey-text">沒有人需要轉寄通知</p>
                                                            </EmptyDataTemplate>
                                                        </asp:ListView>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- 轉寄通知 tab End -->
                                    </div>
                                </div>
                                <!-- 附件&轉寄 End -->
                            </div>
                        </div>
                        <!-- [Section] 需求資料 End -->


                        <!-- [Section] 回覆資料 Start -->
                        <asp:PlaceHolder ID="ph_section1" runat="server">
                            <div class="ui segments">
                                <div class="ui blue segment">
                                    <h5 class="ui header"><a class="anchor" id="section1"></a>回覆資料</h5>
                                </div>
                                <div id="section1-form" class="ui small form segment">
                                    <div class="fields">
                                        <div class="three wide field">
                                            <label>工時 (小時)</label>
                                            <div class="ui basic orange large label">
                                                <asp:Literal ID="lt_Finish_Hours" runat="server">案件處理中</asp:Literal>
                                            </div>
                                        </div>
                                        <div class="four wide field">
                                            <label>預計完成日</label>
                                            <div class="ui blue basic label">
                                                <asp:Literal ID="lt_Wish_Date" runat="server">案件處理中</asp:Literal>
                                            </div>
                                        </div>
                                        <div class="four wide field">
                                            <label>結案日</label>
                                            <div class="ui red basic label">
                                                <asp:Literal ID="lt_Finish_Time" runat="server">案件處理中</asp:Literal>
                                            </div>
                                        </div>
                                        <div class="four wide field">
                                            <label>結案人</label>
                                            <div class="ui red basic label">
                                                <asp:Literal ID="lt_Finish_Who" runat="server">案件處理中</asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="fields">
                                        <div class="sixteen wide field">
                                            <div class="ui raised segment">
                                                <asp:Literal ID="tb_ReplyContent" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </asp:PlaceHolder>
                        <!-- [Section] 回覆資料 End -->


                        <!-- [Section] 處理記錄 Start -->
                        <asp:PlaceHolder ID="ph_section2" runat="server">
                            <div class="ui segments">
                                <div class="ui orange segment">
                                    <h5 class="ui header"><a class="anchor" id="section2"></a>處理記錄</h5>
                                </div>
                                <div id="section2-form" class="ui segment">
                                    <div class="ui grid">
                                        <div class="sixteen wide column">
                                            <!-- Proc list -->
                                            <div class="ui feed timeline">
                                                <asp:ListView ID="lv_ProcList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lv_ProcList_ItemDataBound">
                                                    <LayoutTemplate>
                                                        <asp:PlaceHolder ID="ph_Items" runat="server" />
                                                    </LayoutTemplate>
                                                    <ItemTemplate>
                                                        <div class="event">
                                                            <div class="label">
                                                                <img src="<%:fn_Param.CDNUrl%>images/common/img-user.jpg" alt="user" />
                                                            </div>
                                                            <div class="content">
                                                                <div class="summary grey-text text-darken-2">
                                                                    <span class="ui blue basic label"><%#Eval("Class_Name") %></span>
                                                                    <span><%#Eval("Create_WhoName") %></span>&nbsp;
                                                        <div class="date">
                                                            <strong><%#Eval("Proc_Time") %></strong>
                                                        </div>
                                                                </div>
                                                                <div class="extra text">
                                                                    <%#Eval("Proc_Desc").ToString().Replace("\r\n","<br/>") %>
                                                                </div>
                                                                <div class="ui hidden divider"></div>
                                                                <div class="meta">
                                                                    <asp:Literal ID="lt_TimeDesc1" runat="server"></asp:Literal>
                                                                    <asp:Literal ID="lt_TimeDesc2" runat="server"></asp:Literal>
                                                                </div>
                                                                <div class="extra text">
                                                                    <asp:Literal ID="lt_AttachDesc" runat="server"></asp:Literal>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </ItemTemplate>
                                                    <EmptyDataTemplate>
                                                        <div class="ui placeholder segment">
                                                            <div class="ui icon header">
                                                                <i class="coffee icon"></i>
                                                                處理記錄未新增
                                                            </div>
                                                        </div>
                                                    </EmptyDataTemplate>
                                                </asp:ListView>
                                            </div>
                                        </div>

                                    </div>
                                </div>
                            </div>
                        </asp:PlaceHolder>
                        <!-- [Section] 處理記錄 End -->


                        <!-- [Section] 驗收意見 Start -->
                        <asp:PlaceHolder ID="ph_section3" runat="server">
                            <div class="ui segments">
                                <div class="ui olive segment">
                                    <h5 class="ui header"><a class="anchor" id="section3"></a>驗收意見
                                        <asp:PlaceHolder ID="ph_IsDoneWrite" runat="server" Visible="false">
                                            <span class="ui orange right corner label" title="填寫完成">
                                                <i class="check icon"></i>
                                            </span>
                                        </asp:PlaceHolder>
                                    </h5>
                                </div>
                                <asp:PlaceHolder ID="ph_unClose" runat="server">
                                    <div class="ui placeholder segment">
                                        <div class="ui icon header">
                                            <i class="smile outline icon"></i>
                                            結案後才能填寫
                                        </div>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="ph_section3_data" runat="server">
                                    <div id="section3-form" class="ui form segment">
                                        <div class="field">
                                            <label>滿意度&nbsp;(1 ~ 5分)</label>
                                            <asp:RadioButtonList ID="rbl_RateScore" runat="server" RepeatDirection="Horizontal">
                                                <asp:ListItem Value="1">&nbsp;1：不滿意&nbsp;&nbsp;</asp:ListItem>
                                                <asp:ListItem Value="2">&nbsp;2：勉強&nbsp;&nbsp;</asp:ListItem>
                                                <asp:ListItem Value="3">&nbsp;3：尚可&nbsp;&nbsp;</asp:ListItem>
                                                <asp:ListItem Value="4">&nbsp;4：不錯&nbsp;&nbsp;</asp:ListItem>
                                                <asp:ListItem Value="5" Selected="True">&nbsp;5：超級棒</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </div>
                                        <div class="required field">
                                            <label>驗收意見</label>
                                            <asp:TextBox ID="tb_RateContent" runat="server" Rows="3" TextMode="MultiLine" MaxLength="500" placeholder="填寫意見，最多 400 字"></asp:TextBox>
                                        </div>
                                        <div class="field">
                                            <label>填寫人&nbsp;<small class="grey-text text-darken-2">(存檔人)</small></label>
                                            <asp:Literal ID="lt_RateWho" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                    <div class="ui right aligned segment">
                                        <button id="doSaveS3" type="button" class="ui green small button">
                                            <i class="save icon"></i>確認填寫</button>
                                        <asp:Button ID="btn_doSaveRate" runat="server" Text="Save" OnClick="btn_doSaveRate_Click" Style="display: none;" />
                                    </div>
                                </asp:PlaceHolder>
                            </div>

                        </asp:PlaceHolder>
                        <!-- [Section] 驗收意見 End -->

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
                            <a href="#baseData" class="item">需求資料</a>
                            <asp:PlaceHolder ID="ph_naviBar" runat="server">
                                <a href="#section1" class="item">回覆資料</a>
                                <a href="#section2" class="item">處理記錄</a>
                                <a href="#section3" class="item">驗收意見</a>
                            </asp:PlaceHolder>
                            <a href="#top" class="item"><i class="angle double up icon"></i>到頂端</a>
                        </div>
                        <div class="ui vertical text menu">
                            <div class="header item">功能按鈕</div>
                            <asp:Panel ID="pl_doJob" runat="server" CssClass="item">
                                <a href="<%:fn_Param.WebUrl %>twManagement/MGHelp_Edit.aspx?id=<%:Req_DataID %>" class="ui small green button"><i class="fighter jet icon"></i>前往處理</a>
                            </asp:Panel>
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
    <%-- 共用 Start --%>
    <script>
        $(function () {
            //init dropdown list
            $('select').dropdown();
            //tab menu
            $('.menu .item').tab();

        });
    </script>
    <%-- 共用 End --%>


    <%-- 驗收意見 Start --%>
    <script>
        $(function () {
            //[驗收意見] Save Click
            $("#doSaveS3").click(function () {
                $("#section3-form").addClass("loading");
                $("#MainContent_btn_doSaveRate").trigger("click");
            });

        });
    </script>
    <%-- 驗收意見 End --%>

    <%-- 快速選單 --%>
    <script src="<%=fn_Param.WebUrl %>plugins/sticky.js"></script>


</asp:Content>

