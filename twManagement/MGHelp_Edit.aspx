<%@ Page Title="需求編輯頁" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="MGHelp_Edit.aspx.cs" Inherits="MGHelp_Edit" %>

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
                <a class="item" id="tips">
                    <i class="question circle icon"></i>
                </a>
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
                            <div class="ui green segment">
                                <h5 class="ui header"><a class="anchor" id="baseData"></a>需求資料</h5>
                            </div>
                            <div id="formBase" class="ui form segment">
                                <!-- 基本資料 Start -->
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="two fields">
                                            <div class="ten wide field">
                                                <label>追蹤編號</label>
                                                <div class="ui red basic label">
                                                    <asp:Literal ID="lt_TraceID" runat="server">資料建立中</asp:Literal>
                                                </div>
                                            </div>
                                            <div class="six wide field">
                                                <label>登記日期</label>
                                                <div class="ui basic label">
                                                    <asp:Literal ID="lt_CreateDate" runat="server">資料建立中</asp:Literal>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="three fields">
                                            <div class="required field">
                                                <label>申請類別</label>
                                                <asp:DropDownList ID="ddl_ReqClass" runat="server" CssClass="fluid"></asp:DropDownList>
                                            </div>
                                            <div class="field">
                                                <label>報修方式</label>
                                                <asp:DropDownList ID="rbl_Help_Way" runat="server" CssClass="fluid">
                                                    <asp:ListItem Value="1" Selected="True">即時通</asp:ListItem>
                                                    <asp:ListItem Value="2">電話</asp:ListItem>
                                                    <asp:ListItem Value="3">面談</asp:ListItem>
                                                </asp:DropDownList>
                                            </div>
                                            <div class="field">
                                                <label>處理狀態</label>
                                                <asp:DropDownList ID="ddl_ReqStatus" runat="server" CssClass="fluid" Enabled="false">
                                                </asp:DropDownList>
                                            </div>
                                        </div>

                                        <div class="fields">
                                            <div class="sixteen wide required field">
                                                <label class="red-text text-darken-1"><strong>主旨</strong></label>
                                                <asp:TextBox ID="tb_ReqSubject" runat="server" MaxLength="50" autocomplete="off" placeholder="填寫主旨, 最多 40 字"></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="two fields">
                                            <div class="field">
                                                <label>處理狀態</label>
                                                <asp:Literal ID="lt_ReqStatus" runat="server"><div class="ui basic label">資料建立中</div></asp:Literal>
                                            </div>
                                            <div class="field">
                                                <label>申請類別</label>
                                                <div class="ui teal basic label">
                                                    <asp:Literal ID="lt_ReqClass" runat="server">資料建立中</asp:Literal>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>需求者&nbsp;<span class="grey-text text-darken-1">(跳出清單後，選擇項目)</span></label>
                                                <div class="ui fluid search ac-Employee">
                                                    <div class="ui left labeled input">
                                                        <asp:Label ID="lb_Emp" runat="server" CssClass="ui label" Text="需求者"></asp:Label>
                                                        <asp:TextBox ID="filter_Emp" runat="server" CssClass="prompt" autocomplete="off" placeholder="可查詢:工號, 姓名, 英文名"></asp:TextBox>
                                                    </div>
                                                    <asp:TextBox ID="val_Emp" runat="server" Style="display: none"></asp:TextBox>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- 權限主管同意 -->
                                        <asp:PlaceHolder ID="ph_Agree" runat="server" Visible="false">
                                            <div class="fields">
                                                <div class="sixteen wide field">
                                                    <label>權限申請同意狀態</label>
                                                    <div class="ui grey large label">
                                                        <asp:Literal ID="lt_AuthAgree" runat="server">未同意</asp:Literal>
                                                    </div>
                                                </div>
                                            </div>
                                        </asp:PlaceHolder>
                                    </div>
                                </div>

                                <div class="fields">
                                    <div class="sixteen wide required field">
                                        <label class="red-text text-darken-1"><strong>詳細說明</strong></label>
                                        <asp:TextBox ID="tb_ReqContent" runat="server" Rows="8" TextMode="MultiLine" MaxLength="5000" placeholder="詳細描述需求內容, 最多 4000 字"></asp:TextBox>
                                    </div>
                                </div>
                                <asp:PlaceHolder ID="ph_Benefit" runat="server">
                                    <div class="fields">
                                        <div class="sixteen wide field">
                                            <!-- 狀態為 處理中 時鎖定 -->
                                            <label>改善效益&nbsp;<small class="grey-text text-darken-1">(開案後鎖定)</small></label>
                                            <asp:TextBox ID="tb_Help_Benefit" runat="server" Rows="4" TextMode="MultiLine" MaxLength="1100" placeholder="最多 1000 字"></asp:TextBox>
                                        </div>
                                    </div>
                                </asp:PlaceHolder>
                                <!-- 基本資料 End -->
                                <!-- 附件&轉寄 Start -->
                                <div class="fields">
                                    <div class="sixteen wide field">
                                        <label><i class="copy outline icon"></i>上傳附件&nbsp;<span class="grey-text text-darken-1">(最多5筆, 總大小為50MB)</span></label>
                                        <div class="two fields">
                                            <div class="field">
                                                <asp:FileUpload ID="fu_Attachment" runat="server" AllowMultiple="true" />
                                            </div>
                                            <div class="field">
                                                <div class="ui small info message">
                                                    副檔名：<%=FileExtLimit.Replace("|", ", ") %>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
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
                                            <asp:PlaceHolder ID="ph_fileEmpty" runat="server">
                                                <div class="ui message">
                                                    <div class="content">
                                                        <div class="header">請在上方點選要上傳的附件，讓需求說明更完整。</div>
                                                    </div>
                                                </div>
                                            </asp:PlaceHolder>
                                            <asp:ListView ID="lv_Attachment" runat="server" ItemPlaceholderID="ph_Items" GroupPlaceholderID="ph_Group" GroupItemCount="2" OnItemCommand="lv_Attachment_ItemCommand" OnItemDataBound="lv_Attachment_ItemDataBound">
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
                                                    <td style="width: 40%">
                                                        <asp:Literal ID="lt_FileUrl" runat="server"></asp:Literal>
                                                        <%--<a href="<%#fn_Param.RefUrl %><%#UploadFolder %><%=lt_TraceID.Text %>/<%#Eval("AttachFile") %>" target="_blank"><%#Eval("AttachFile_Org") %></a>--%>
                                                    </td>
                                                    <td style="width: 10%" class="center aligned">
                                                        <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui mini orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                                                        <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("AttachID") %>' />
                                                        <asp:HiddenField ID="hf_FileName" runat="server" Value='<%#Eval("AttachFile") %>' />
                                                    </td>
                                                </ItemTemplate>
                                                <EmptyItemTemplate>
                                                    <td>&nbsp;</td>
                                                    <td>&nbsp;</td>
                                                </EmptyItemTemplate>
                                                <EmptyDataTemplate>
                                                    <div class="ui message">
                                                        <div class="content">
                                                            <div class="header">附件未上傳</div>
                                                            <p>上傳截圖或參考檔案，讓需求說明更完整，問題能更快速地解決。</p>
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
                                                        <div class="ui message">
                                                            <div class="content">
                                                                <div class="header">轉寄通知</div>
                                                                <p>發送EMail通知其他人，讓他們能追蹤此案件。</p>
                                                                <p>此功能請謹慎使用，大量發送EMail有可能成為垃圾郵件。</p>
                                                            </div>
                                                        </div>
                                                        <asp:PlaceHolder ID="ph_InformWho" runat="server" Visible="true">
                                                            <!-- zTree js -->
                                                            <div id="userList" class="ztree"></div>
                                                            <asp:TextBox ID="tb_InfoWho" runat="server" Style="display: none"></asp:TextBox>
                                                        </asp:PlaceHolder>

                                                        <asp:PlaceHolder ID="ph_InformList" runat="server" Visible="false">
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
                                                        </asp:PlaceHolder>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- 轉寄通知 tab End -->
                                    </div>
                                </div>
                                <!-- 附件&轉寄 End -->
                            </div>
                            <div class="ui right aligned segment">
                                <button id="doSaveBase" type="button" class="ui green small button">
                                    <i class="save icon"></i>
                                    <asp:Literal ID="lt_SaveBase" runat="server">送出需求</asp:Literal>
                                </button>
                                <asp:Button ID="btn_doSaveBase" runat="server" Text="Save" OnClick="btn_doSaveBase_Click" Style="display: none;" />
                                <asp:HiddenField ID="hf_DataID" runat="server" />
                            </div>
                        </div>
                        <!-- [Section] 需求資料 End -->


                        <!-- [Section] 回覆資料 Start -->
                        <asp:PlaceHolder ID="ph_section1" runat="server">
                            <div class="ui segments">
                                <div class="ui blue segment">
                                    <h5 class="ui header"><a class="anchor" id="section1"></a>回覆資料</h5>
                                </div>
                                <div id="section1-form" class="ui form segment">
                                    <div class="five fields">
                                        <div class="field">
                                            <label>總工時&nbsp;<small>(與結案工時共用)</small></label>
                                            <asp:TextBox ID="tb_Finish_Hours" runat="server" type="number" step="0.5" min="0"></asp:TextBox>
                                        </div>
                                        <div class="field">
                                            <label>預計完成日</label>
                                            <div class="ui left icon input datepicker">
                                                <asp:TextBox ID="tb_Wish_Date" runat="server" autocomplete="off"></asp:TextBox>
                                                <i class="calendar alternate outline icon"></i>
                                            </div>
                                        </div>
                                        <div class="field">
                                            <label>結案日</label>
                                            <div class="ui red basic label">
                                                <asp:Literal ID="lt_Finish_Time" runat="server">案件處理中</asp:Literal>
                                            </div>
                                        </div>
                                        <div class="field">
                                            <label>結案人</label>
                                            <div class="ui red basic label">
                                                <asp:Literal ID="lt_Finish_Who" runat="server">案件處理中</asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="fields">
                                        <div class="sixteen wide field">
                                            <asp:TextBox ID="tb_ReplyContent" runat="server" Rows="4" TextMode="MultiLine" MaxLength="1000" placeholder="備註，最多 500 字"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                                <div class="ui right aligned segment">
                                    <!-- 管理者按鈕 -->
                                    <button id="doSaveS1" type="button" class="ui green small button"><i class="save icon"></i>回覆存檔</button>
                                    <asp:Button ID="btn_doSaveReply" runat="server" Text="Save" OnClick="btn_doSaveReply_Click" Style="display: none;" />

                                    <!-- 加入追蹤 -->
                                    <asp:LinkButton ID="lbtn_doTrace" runat="server" CssClass="ui grey small button" ToolTip="加入後會在列表頁置頂(只有本人)" OnClick="lbtn_doTrace_Click"><i class="heart icon"></i>加入追蹤</asp:LinkButton>

                                    <!-- 發通知信 -->
                                    <button id="showInform" type="button" class="ui blue small button" title="開窗後選擇類型"><i class="envelope icon"></i>發通知信</button>
                                    <asp:TextBox ID="val_InformType" runat="server" Style="display: none" ToolTip="通知類型"></asp:TextBox>
                                    <asp:TextBox ID="val_MailCont" runat="server" TextMode="MultiLine" Style="display: none" ToolTip="通知內文"></asp:TextBox>
                                    <asp:Button ID="btn_Inform" runat="server" Text="Button" OnClick="btn_Inform_Click" Style="display: none;" />

                                    <!-- 結案 -->
                                    <asp:PlaceHolder ID="ph_finish" runat="server">
                                        <button id="showFinish" type="button" class="ui red small button"><i class="archive icon"></i>結案</button>
                                        <asp:TextBox ID="val_FinishTime" runat="server" Style="display: none"></asp:TextBox>
                                        <asp:Button ID="btn_Finish" runat="server" Text="Button" OnClick="btn_Finish_Click" Style="display: none;" />
                                    </asp:PlaceHolder>
                                </div>
                            </div>
                        </asp:PlaceHolder>
                        <!-- [Section] 回覆資料 End -->


                        <!-- [Section] 處理記錄 Start -->
                        <asp:PlaceHolder ID="ph_section2" runat="server">
                            <div class="ui segments">
                                <div class="ui orange segment">
                                    <h5 class="ui header"><a class="anchor" id="section2"></a>處理記錄&nbsp;<small>(新增第一筆後，會將狀態設為處理中)</small></h5>
                                </div>
                                <div id="section2-form" class="ui segment">
                                    <div class="ui grid">
                                        <div class="ten wide column">
                                            <!-- Proc list -->
                                            <div class="ui feed timeline">
                                                <asp:ListView ID="lv_ProcList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lv_ProcList_ItemDataBound" OnItemCommand="lv_ProcList_ItemCommand">
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
                                                                    <div class="ui grid">
                                                                        <div class="row">
                                                                            <div class="thirteen wide column">
                                                                                <%#Eval("Proc_Desc").ToString().Replace("\r\n","<br/>") %>
                                                                            </div>
                                                                            <div class="three wide column right aligned">
                                                                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui mini orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                                                                <asp:HiddenField ID="hf_ID" runat="server" Value='<%#Eval("DetailID") %>' />
                                                                            </div>
                                                                        </div>
                                                                    </div>
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


                                        <!-- Add section -->
                                        <div class="six wide column">
                                            <div class="ui small form attached brown segment">
                                                <div class="required field">
                                                    <label>處理類別</label>
                                                    <asp:DropDownList ID="ddl_ProcClass" runat="server"></asp:DropDownList>
                                                </div>
                                                <div class="required field">
                                                    <label>說明</label>
                                                    <asp:TextBox ID="tb_ProcDesc" runat="server" Rows="6" TextMode="MultiLine"></asp:TextBox>
                                                </div>
                                                <div class="field">
                                                    <label>附件&nbsp;<small class="grey-text text-darken-1">(最多5筆, 限50MB)</small></label>
                                                    <asp:FileUpload ID="fu_ProcFiles" runat="server" AllowMultiple="true" />
                                                </div>
                                                <div class="required field">
                                                    <label>處理時間</label>
                                                    <div class="ui left icon input dateTimepicker">
                                                        <asp:TextBox ID="tb_ProcTime" runat="server" autocomplete="off"></asp:TextBox>
                                                        <i class="calendar alternate outline icon"></i>
                                                    </div>
                                                </div>
                                                <div class="field">
                                                    <label>需求確認時間</label>
                                                    <div class="ui left icon input dateTimepicker">
                                                        <asp:TextBox ID="tb_ConfirmTime" runat="server" autocomplete="off"></asp:TextBox>
                                                        <i class="calendar alternate outline icon"></i>
                                                    </div>
                                                </div>
                                                <div class="field">
                                                    <label>預計完成日</label>
                                                    <div class="ui left icon input datepicker">
                                                        <asp:TextBox ID="tb_WishTime" runat="server" autocomplete="off"></asp:TextBox>
                                                        <i class="calendar alternate outline icon"></i>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="ui right aligned bottom attached segment">
                                                <button id="doSaveS2" type="button" class="ui green small button">
                                                    <i class="save icon"></i>新增記錄</button>
                                                <asp:Button ID="btn_doSaveProc" runat="server" Text="Save" OnClick="btn_doSaveProc_Click" Style="display: none;" />
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
                                    <h5 class="ui header"><a class="anchor" id="section3"></a>驗收意見</h5>
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
                            <div class="item">
                                <a href="<%:Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i>返回列表</a>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- Right Navi Menu End -->
            </div>
        </div>
        <asp:PlaceHolder ID="ph_Modals" runat="server">
            <!-- 通知信 Modal Start -->
            <div id="informPage" class="ui modal">
                <div class="header">
                    自訂通知信
                </div>
                <div class="content">
                    <div class="ui info message">
                        <div class="header">
                            通知信說明
                        </div>
                        <ul class="list">
                            <%--<li>提供資料：缺資料或改善效益未填時可使用.</li>
                            <li>測試通知：專案交付測試後通知測試, 發送後狀態會變成「測試中」.</li>
                            <li>驗收通知：結案時會發第一則通知, 後續可繼續手動通知.</li>--%>
                            <li>此通知信會發給需求者.</li>
                            <li>內文連結會自動產生.</li>
                        </ul>
                    </div>
                    <div class="ui form">
                        <%--<div class="fields">
                            <div class="eight wide field">
                                <label>選擇類型</label>
                                <select id="dia-InformType" class="fluid">
                                    <option value="">- 請選擇 -</option>
                                    <option value="A">A:提供資料</option>
                                    <option value="B">B:測試通知</option>
                                    <option value="C">C:驗收通知</option>
                                </select>
                            </div>
                            <div class="eight wide field">
                                <label>快速片語</label>
                                <select id="dia-InformTxt" class="fluid">
                                    <option value="">- 請選擇 -</option>
                                    <option value="請點下方連結，回來填寫改善效益">請點下方連結，回來填寫改善效益</option>
                                    <option value="需求已經可以測試，點下方連結可看詳細說明">需求已經可以測試，點下方連結可看詳細說明</option>
                                    <option value="需求已結案，請點下方連結，填寫驗收意見">需求已結案，請點下方連結，填寫驗收意見</option>
                                </select>
                            </div>
                        </div>--%>
                        <div class="fields">
                            <div class="sixteen wide field">
                                <label>內文簡易說明</label>
                                <textarea id="dia-mailCont" rows="3" maxlength="200" placeholder="最多 100 字"></textarea>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="actions">
                    <div class="ui cancel button">
                        稍候處理，關閉視窗
                    </div>
                    <button id="doInform" type="button" class="ui blue small button"><i class="envelope icon"></i>確定發送通知信</button>
                </div>
            </div>
            <!-- 通知信 Modal End -->

            <!-- 結案 Modal Start -->
            <div id="finishPage" class="ui modal">
                <div class="header">
                    結案
                </div>
                <div class="content">
                    <div class="ui info message">
                        <div class="header">
                            結案說明
                        </div>
                        <ul class="list">
                            <li>Email通知：管理部、需求者、轉寄人員</li>
                            <li>內文告知結案，並要求填寫滿意度調查及驗收意見.</li>
                            <li>處理狀態設為「已結案」</li>
                        </ul>
                    </div>
                    <div class="ui form">
                        <div class="fields">
                            <div class="six wide field">
                                <label>工時</label>
                                <input type="number" id="dia-finishHours" step="0.5" />
                            </div>
                            <div class="ten wide field">
                                <label>結案時間</label>
                                <div class="ui left icon input">
                                    <input type="datetime-local" id="dia-finishDate" value="<%=DateTime.Now.ToString().ToDateString("yyyy-MM-ddTHH:mm") %>" />
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
                <div class="actions">
                    <div class="ui cancel button">
                        稍候處理，關閉視窗
                    </div>
                    <button id="doFinish" type="button" class="ui red small button"><i class="archive icon"></i>確定結案</button>
                </div>
            </div>
            <!-- 結案 Modal End -->

            <!-- Tips Modal Start -->
            <div id="tipPage" class="ui modal">
                <div class="header">
                    說明文件
                </div>
                <div class="content">
                    <%--<div class="ui header">
                        處理狀態說明
                    </div>
                    <ul class="ui list">
                        <li><span>待處理</span>：需求者發出新需求</li>
                        <li><span>處理中</span>：執行單位努力處理中。</li>
                        <li><span>測試中</span>：處理完畢，通知需求者測試。</li>
                        <li><span>已結案</span>：案件已結案。</li>
                    </ul>--%>
                    <div class="ui header">
                        通知信發送時機
                    </div>
                    <ul class="ui list">
                        <li><span>新需求</span>：通知執行單位(管理部)、轉寄人員(若有勾選)</li>
                        <li><span class="red-text text-darken-1">通知信</span>：由執行單位手動按通知, 內容自行定義.</li>
                        <li><span>驗收通知</span>：結案時會發第一則通知, 後續可繼續手動通知.</li>
                        <li><span>結案</span>：通知執行單位、轉寄人員、需求者。</li>
                    </ul>
                </div>
                <div class="actions">
                    <div class="ui cancel button">
                        關閉視窗
                    </div>
                </div>
            </div>
            <!-- Tips Modal End -->

            <!-- Lock Modal Start -->
            <asp:PlaceHolder ID="ph_LockModal" runat="server" Visible="false">
                <!-- 已結案Modal -->
                <div id="lockPage" class="ui small basic modal">
                    <div class="ui icon header">
                        <i class="archive icon"></i>
                        案件已結案
                    </div>
                    <div class="content">
                        <p>案件：<span class="pink-text text-lighten-2"><asp:Literal ID="show_TraceID" runat="server"></asp:Literal></span></p>
                        <p>
                            已於&nbsp;
                            <span class="yellow-text">
                                <asp:Literal ID="show_FinishTime" runat="server"></asp:Literal></span>&nbsp;,&nbsp;由&nbsp;
                            <span class="yellow-text">
                                <asp:Literal ID="show_FinishWho" runat="server"></asp:Literal></span>
                            &nbsp;結案。
                        </p>
                    </div>
                    <div class="actions">
                        <div class="ui red basic cancel inverted button">
                            <i class="pencil icon"></i>
                            繼續編輯
                        </div>
                        <div class="ui green ok inverted button">
                            <i class="undo icon"></i>
                            返回列表
                        </div>
                    </div>
                </div>
            </asp:PlaceHolder>
            <!-- Lock Modal End -->
        </asp:PlaceHolder>

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
    <%-- 需求資料 Start --%>
    <script>
        $(function () {
            //[需求資料] Save Click
            $("#doSaveBase").click(function () {
                var flag = $("#MainContent_hf_DataID").val();

                //[Add時] 取得zTree已勾選的值, 並填入指定欄位 Start -----
                if (flag == "") {
                    var myTreeName = "userList";
                    var valAry = [];

                    //宣告tree物件
                    var treeObj = $.fn.zTree.getZTreeObj(myTreeName);

                    //取得節點array
                    var nodes = treeObj.getCheckedNodes(true);

                    //將id丟入陣列
                    for (var row = 0; row < nodes.length; row++) {
                        //只取開頭為'v_'的值
                        var myval = nodes[row].id;
                        if (myval.substring(0, 2) == "v_") {
                            valAry.push(myval.replace("v_", ""));
                        }
                    }

                    //將陣列組成以','分隔的字串，並填入欄位(轉寄人員Guid)
                    $("#MainContent_tb_InfoWho").val(valAry.join(","));

                }
                //[Add時] 取得zTree已勾選的值, 並填入指定欄位 End -----

                $("#formBase").addClass("loading");
                $(this).addClass("loading");
                $("#MainContent_btn_doSaveBase").trigger("click");
            });

            //function:申請類別=3, 改善效益顯示必填
            $("input[name='ctl00$MainContent$rbl_ApplyType']").change(function () {
                var val = this.value;
                if (val == "3") {
                    $("#MainContent_tb_Help_Benefit").parent().addClass("required error");
                } else {
                    $("#MainContent_tb_Help_Benefit").parent().removeClass("required error");
                }
            });
        });
    </script>
    <%-- 需求資料 End --%>

    <%-- 回覆資料 Start --%>
    <script>
        $(function () {
            //[回覆資料] Save Click
            $("#doSaveS1").click(function () {
                $(this).addClass("loading");
                $("#section1-form").addClass("loading");
                $("#MainContent_btn_doSaveReply").trigger("click");
            });

        });
    </script>
    <%-- 回覆資料 End --%>

    <%-- 處理進度 Start --%>
    <script>
        $(function () {
            //[處理進度] Save Click
            $("#doSaveS2").click(function () {
                $(this).addClass("loading");
                $("#section2-form").addClass("loading");
                $("#MainContent_btn_doSaveProc").trigger("click");
            });

        });
    </script>
    <%-- 處理進度 End --%>

    <%-- 驗收意見 Start --%>
    <script>
        $(function () {
            //[驗收意見] Save Click
            $("#doSaveS3").click(function () {
                $(this).addClass("loading");
                $("#section3-form").addClass("loading");
                $("#MainContent_btn_doSaveRate").trigger("click");
            });

        });
    </script>
    <%-- 驗收意見 End --%>

    <%-- Modal Start --%>
    <script>
        $(function () {
            //說明視窗(Modal)
            $("#tips").click(function () {
                $('#tipPage').modal('show');
            });


            /* --- 結案 --- */
            //結案視窗(Modal)
            $("#showFinish").click(function () {
                //取工時欄位,填入結案的工時欄
                var _hours = $("#MainContent_tb_Finish_Hours").val();
                $("#dia-finishHours").val(_hours);

                $('#finishPage').modal('show');
            });

            //結案確認鈕
            $("#doFinish").click(function () {
                var hours = $("#dia-finishHours").val();
                var dt = $("#dia-finishDate").val();

                //填入隱藏欄位(傳遞時使用)
                $("#MainContent_tb_Finish_Hours").val(hours);
                $("#MainContent_val_FinishTime").val(dt);
                //loading
                $(this).addClass("loading");

                //觸發按鈕
                $("#MainContent_btn_Finish").trigger("click");

            });


            /* --- 通知 --- */
            //通知視窗(Modal)
            $("#showInform").click(function () {
                $('#informPage').modal('show');
            });

            //通知確認鈕
            $("#doInform").click(function () {
                //var _type = $("#dia-InformType").val();
                var _dt = $("#dia-mailCont").val();

                //填入隱藏欄位(傳遞時使用)
                //$("#MainContent_val_InformType").val(_type);
                $("#MainContent_val_MailCont").val(_dt);
                //loading
                $(this).addClass("loading");

                //觸發按鈕
                $("#MainContent_btn_Inform").trigger("click");

            });

            //快速片語自動填入欄位
            $("#dia-InformTxt").change(function () {
                var _val = $("#dia-InformTxt").val();

                $("#dia-mailCont").val(_val);
            });

        });
    </script>
    <!-- Modal end -->
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


    <%-- 快速選單 --%>
    <script src="<%=fn_Param.WebUrl %>plugins/sticky.js"></script>
    <%-- 日期選擇器 Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.js"></script>
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/options.js"></script>
    <script>
        $(function () {
            //載入datepicker (time)
            $('.dateTimepicker').calendar(calendarOptsByTime_Range);
            //載入datepicker (date)
            $('.datepicker').calendar(calendarOpts_Range);
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
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Users.ashx?q={query}&v=1.0',
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
                            deptID: item.DeptID
                        });
                    });
                    return response;
                }
            }

        });
    </script>
    <%-- Search UI End --%>

    <%-- zTree(舊版) Start --%>
    <link href="<%=fn_Param.WebUrl%>plugins/zTree/css/style.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.WebUrl%>plugins/zTree/jquery.ztree.core-3.5.min.js"></script>
    <script src="<%=fn_Param.WebUrl%>plugins/zTree/jquery.ztree.excheck-3.5.min.js"></script>
    <script>
        //zTree 設定
        var setting = {
            view: {
                dblClickExpand: false
            },
            callback: {
                onClick: MMonClick
            },
            check: {
                enable: true
            },
            data: {
                simpleData: {
                    enable: true
                }
            }
        };

        //Event - onClick
        function MMonClick(e, treeId, treeNode) {
            var zTree = $.fn.zTree.getZTreeObj("userList");
            zTree.expandNode(treeNode);
        }

        //宣告節點
        var zNodes;

        //取得資料
        function getUserList() {
            $.ajax({
                async: false,
                cache: false,
                type: 'POST',
                dataType: "json",
                url: "<%=fn_Param.WebUrl%>Ajax_Data/GetAuthUserList.ashx",
                data: {
                    block: 'Y'
                },
                error: function () {
                    alert('人員選單載入失敗!');
                },
                success: function (data) {
                    zNodes = data;
                }
            });
            //載入zTree
            $.fn.zTree.init($("#userList"), setting, zNodes);
        }

        // 所有節點的收合(true = 展開, false = 折疊)
        //function expandAll(objbool) {
        //    var treeObj = $.fn.zTree.getZTreeObj("userList");
        //    treeObj.expandAll(objbool);
        //}

        /* 取值(zTree名稱, 要放值的欄位名) */
        //function getCbValue(eleName, valName) {
        //    var treeObj = $.fn.zTree.getZTreeObj(eleName);
        //    var nodes = treeObj.getCheckedNodes(true);
        //    var ids = "";
        //    for (var i = 0; i < nodes.length; i++) {
        //        //只取開頭為'v_'的值
        //        var myval = nodes[i].id;

        //        if (myval.substring(0, 2) == "v_") {
        //            //字串組合, 加入分隔符號("||")
        //            if (ids != "") {
        //                ids += "||"
        //            }

        //            //取得id值
        //            ids += myval.replace("v_", "");
        //        }
        //    }


        //    //輸出組合完畢的字串值
        //    document.getElementById(valName).value = ids;
        //    return true;
        //}

        //Load
        $(document).ready(function () {
            getUserList();

        });


    </script>
    <%-- zTree End --%>
</asp:Content>

