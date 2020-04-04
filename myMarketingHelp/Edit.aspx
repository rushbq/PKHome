<%@ Page Title="製物工單明細" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Edit.aspx.cs" Inherits="myMarketingHelp_Edit" %>

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
                    <div class="section"><%:resPublic.nav_2000 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="section"><%:resPublic.fun_製物工單 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        <asp:Literal ID="lt_CorpName" runat="server" Text="公司別名稱"></asp:Literal>
                    </div>
                </div>
            </div>
            <div class="right menu">
                <a class="item" id="tips">
                    <i class="question circle icon"></i>
                </a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui attached segment grey-bg lighten-5">
            <!-- 需求資料 Start -->
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
                    <h5 class="ui header"><%:GetLocalResourceObject("txt_需求資料")%>
                    </h5>
                </div>
                <div class="ui small form attached segment">
                    <!-- 基本資料 Start -->
                    <div class="fields">
                        <!-- Left Block -->
                        <div class="eight wide field">
                            <div class="three fields">
                                <div class="field">
                                    <label><%:GetLocalResourceObject("txt_追蹤編號")%></label>
                                    <div class="ui red basic large label">
                                        <asp:Literal ID="lt_TraceID" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div class="field">
                                    <label><%:GetLocalResourceObject("txt_處理狀態")%></label>
                                    <div class="ui blue basic large label">
                                        <asp:Literal ID="lt_ReqStatus" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div class="field">
                                    <label><%:GetLocalResourceObject("txt_登記日期")%></label>
                                    <div class="ui basic label">
                                        <asp:Literal ID="lt_CreateDate" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="sixteen wide required field">
                                    <label><%:GetLocalResourceObject("txt_主旨")%></label>
                                    <asp:TextBox ID="tb_ReqSubject" runat="server" MaxLength="40" autocomplete="off"></asp:TextBox>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="sixteen wide required field">
                                    <label><%:GetLocalResourceObject("txt_詳細說明")%></label>
                                    <asp:TextBox ID="tb_ReqContent" runat="server" Rows="6" TextMode="MultiLine" MaxLength="3000"></asp:TextBox>
                                </div>
                            </div>
                        </div>

                        <!-- Right Block -->
                        <div class="eight wide field">
                            <div class="fields">
                                <div class="six wide required field">
                                    <label><%:GetLocalResourceObject("txt_緊急度")%></label>
                                    <asp:DropDownList ID="ddl_EmgStatus" runat="server" CssClass="fluid">
                                    </asp:DropDownList>
                                </div>
                                <div class="six wide field">
                                    <label><%:GetLocalResourceObject("txt_希望完成日")%></label>
                                    <div class="ui left icon input datepicker">
                                        <asp:TextBox ID="tb_WishDate" runat="server" MaxLength="10" autocomplete="off"></asp:TextBox>
                                        <i class="calendar alternate outline icon"></i>
                                    </div>
                                </div>
                                <div class="four wide field">
                                    <label><%:GetLocalResourceObject("txt_預計完成日")%>&nbsp;<span class="grey-text text-darken-1"><%:GetLocalResourceObject("tip1")%></span></label>
                                    <asp:TextBox ID="tb_EstDate" runat="server" autocomplete="off" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="six wide required field">
                                    <label><%:GetLocalResourceObject("txt_需求類別")%></label>
                                    <asp:DropDownList ID="ddl_ReqClass" runat="server" CssClass="fluid">
                                    </asp:DropDownList>
                                </div>
                                <div class="six wide required field">
                                    <label><%:GetLocalResourceObject("txt_需求資源")%></label>
                                    <asp:DropDownList ID="ddl_ReqRes" runat="server" CssClass="fluid">
                                    </asp:DropDownList>
                                </div>
                                <div class="four wide required field">
                                    <label><%:GetLocalResourceObject("txt_需求數量")%></label>
                                    <asp:TextBox ID="tb_ReqQty" runat="server" MaxLength="3" type="number" min="1" max="999999999">1</asp:TextBox>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="sixteen wide field">
                                    <label><%:GetLocalResourceObject("txt_需求者")%>&nbsp;<span class="grey-text text-darken-1"><%:GetLocalResourceObject("tip4")%></span></label>
                                    <div class="ui fluid search ac-Employee">
                                        <div class="ui right labeled input">
                                            <asp:TextBox ID="filter_Emp" runat="server" CssClass="prompt"></asp:TextBox>
                                            <div class="ui label">
                                                <asp:Label ID="lb_Emp" runat="server"></asp:Label>
                                            </div>
                                        </div>
                                        <asp:TextBox ID="val_Emp" runat="server" Style="display: none"></asp:TextBox>
                                        <asp:TextBox ID="val_Dept" runat="server" Style="display: none"></asp:TextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="sixteen wide field">
                                    <label><%:GetLocalResourceObject("txt_附加檔案")%>&nbsp;<span class="grey-text text-darken-1">(<%:GetLocalResourceObject("tip5")%> <%=FileExtLimit.Replace("|", ", ") %>)</span></label>
                                    <asp:FileUpload ID="fu_Attachment" runat="server" AllowMultiple="true" />
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- 基本資料 End -->
                    <!-- 附件&轉寄 Start -->
                    <div class="fields">
                        <div class="sixteen wide field">
                            <div class="ui secondary pointing small menu">
                                <a class="item active" data-tab="tab-name1">
                                    <i class="paperclip icon"></i><%:GetLocalResourceObject("txt_附件清單")%>
                                </a>
                                <a class="item" data-tab="tab-name2">
                                    <i class="envelope outline icon"></i><%:GetLocalResourceObject("txt_轉寄通知")%>
                                </a>
                            </div>
                            <!-- 附件清單 tab Start -->
                            <div class="ui active tab" data-tab="tab-name1">
                                <asp:PlaceHolder ID="ph_fileEmpty" runat="server">
                                    <div class="ui message">
                                        <div class="content">
                                            <%=GetLocalResourceObject("msg1")%>
                                        </div>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:ListView ID="lv_Attachment" runat="server" ItemPlaceholderID="ph_Items" GroupPlaceholderID="ph_Group" GroupItemCount="2" OnItemCommand="lv_Attachment_ItemCommand">
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
                                            <a href="<%#fn_Param.RefUrl %><%#UploadFolder %><%=Req_DataID %>/<%#Eval("AttachFile") %>" target="_blank"><%#Eval("AttachFile_Org") %></a>
                                        </td>
                                        <td style="width: 10%" class="center aligned">
                                            <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui mini orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                                            <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
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
                                                <%=GetLocalResourceObject("msg2")%>
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
                                                    <%=GetLocalResourceObject("msg3")%>
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
                                                        <p><%=GetLocalResourceObject("txt_無另外通知")%></p>
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

                    <div class="ui hidden divider"></div>
                    <!-- 功能按鈕 Start -->
                    <asp:PlaceHolder ID="ph_ControlBtns" runat="server">
                        <div class="ui grid">
                            <div class="row">
                                <div class="four wide column">
                                    <a href="<%=Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i><%:GetLocalResourceObject("txt_返回列表")%></a>
                                </div>
                                <div class="eight wide column center aligned">
                                    <asp:PlaceHolder ID="ph_Assign" runat="server" Visible="false">
                                        <asp:Panel ID="pl_AssignMsg" runat="server" CssClass="ui right pointing red basic label">
                                            <%:GetLocalResourceObject("txt_請指派處理人員")%>
                                        </asp:Panel>

                                        <button id="showAssign" type="button" class="ui teal small button" title="指派處理人員"><i class="users icon"></i><%:GetLocalResourceObject("txt_派案")%></button>

                                        <asp:TextBox ID="val_Proc" runat="server" Style="display: none"></asp:TextBox>
                                        <asp:Button ID="btn_Assign" runat="server" OnClick="btn_Assign_Click" Style="display: none;" />
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="ph_FinishCase" runat="server" Visible="false">
                                        <button id="showFinish" type="button" class="ui red small button" title="結案,通知所有人"><i class="archive icon"></i><%:GetLocalResourceObject("txt_結案")%></button>

                                        <asp:TextBox ID="val_FinishHours" runat="server" Style="display: none"></asp:TextBox>
                                        <asp:TextBox ID="val_FinishDate" runat="server" Style="display: none"></asp:TextBox>
                                        <asp:Button ID="btn_Finish" runat="server" Text="Button" OnClick="btn_Finish_Click" Style="display: none;" />
                                    </asp:PlaceHolder>
                                </div>
                                <div class="four wide column right aligned">
                                    <button id="doSave" type="button" class="ui green small button"><i class="save icon"></i><%:GetLocalResourceObject("txt_存檔")%></button>
                                    <asp:Button ID="btn_Save" runat="server" Text="Button" OnClick="btn_Save_Click" Style="display: none;" />

                                    <asp:HiddenField ID="hf_AssignWho" runat="server" />
                                    <asp:HiddenField ID="hf_DataID" runat="server" />
                                    <asp:HiddenField ID="hf_Flag" runat="server" Value="Add" />
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>
                    <!-- 功能按鈕 End -->
                </div>
                <div class="ui bottom attached info small message">
                    <%=GetLocalResourceObject("tipList1")%>
                </div>
            </div>
            <!-- 需求資料 End -->

            <!-- 處理進度 Start -->
            <asp:PlaceHolder ID="ph_Comments" runat="server" Visible="false">
                <div id="replyComments" class="ui segments">
                    <div class="ui orange segment">
                        <h5 class="ui header"><%:GetLocalResourceObject("txt_處理進度")%>
                        </h5>
                    </div>
                    <div class="ui small form segment">
                        <div class="two fields">
                            <div class="field">
                                <label><%:GetLocalResourceObject("txt_工作時數")%></label>
                                <div class="ui red basic large label">
                                    <asp:Literal ID="lt_FinishHours" runat="server"></asp:Literal>
                                </div>
                            </div>
                            <div class="field">
                                <label><%:GetLocalResourceObject("txt_結案日期")%></label>
                                <div class="ui basic label">
                                    <asp:Literal ID="lt_FinishDate" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="sixteen wide field">
                                <label><%:GetLocalResourceObject("txt_處理人員")%></label>
                                <asp:ListView ID="lv_Assigned" runat="server" ItemPlaceholderID="ph_Items">
                                    <LayoutTemplate>
                                        <asp:PlaceHolder ID="ph_Items" runat="server" />
                                    </LayoutTemplate>
                                    <ItemTemplate>
                                        <div class="ui basic label">
                                            <%#Eval("Who") %>
                                        </div>
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <div class="ui placeholder segment">
                                            <div class="ui icon header">
                                                <i class="coffee icon"></i>
                                                <%:GetLocalResourceObject("txt_未指派")%>
                                            </div>
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:ListView>
                            </div>
                        </div>
                        <div id="processList" class="field">
                            <label><%:GetLocalResourceObject("txt_進度說明")%></label>
                            <div class="ui segment">
                                <div class="ui minimal comments">
                                    <asp:ListView ID="lv_Reply" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lv_Reply_ItemCommand">
                                        <LayoutTemplate>
                                            <asp:PlaceHolder ID="ph_Items" runat="server" />
                                        </LayoutTemplate>
                                        <ItemTemplate>
                                            <div class="comment">
                                                <div class="content">
                                                    <a><%#Eval("Create_Name") %></a>
                                                    <div class="metadata">
                                                        <span class="date"><%#Eval("Create_Time") %></span>
                                                    </div>
                                                    <div class="text">
                                                        <%#Eval("Reply_Content").ToString().Replace("\r","<br/>") %>
                                                    </div>
                                                    <div class="actions">
                                                        <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui mini orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                                        <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
                                                    </div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:ListView>

                                    <!-- reply form -->
                                    <asp:PlaceHolder ID="ph_ReplyForm" runat="server" Visible="false">
                                        <div class="ui reply form">
                                            <div class="field">
                                                <asp:TextBox ID="tb_ReplyContent" runat="server" Rows="6" TextMode="MultiLine"></asp:TextBox>
                                            </div>
                                            <button id="doReply" type="button" class="ui primary small button"><i class="edit icon"></i><%:GetLocalResourceObject("txt_新增進度說明")%></button>
                                            <asp:Button ID="btn_Reply" runat="server" Text="Button" OnClick="btn_Reply_Click" Style="display: none;" />
                                        </div>
                                    </asp:PlaceHolder>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>
            </asp:PlaceHolder>

            <!-- 處理進度 Start -->
        </div>

        <asp:PlaceHolder ID="ph_LockModal" runat="server" Visible="false">
            <!-- 已結案，不可編輯 -->
            <div id="lockPage" class="ui small basic modal">
                <div class="ui icon header">
                    <i class="archive icon"></i>
                    此案件已結案
                </div>
                <div class="content">
                    <p>案件已結案，若要查看更多，<a href="<%=FuncPath() %>/View/<%#Eval("Data_ID") %> %>">請點我前往明細檢視頁</a>，或是返回列表頁。</p>
                </div>
                <div class="actions">
                    <div class="ui green ok inverted button">
                        <i class="undo icon"></i>
                        返回列表
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>

        <!-- 派案人員 Start -->
        <div id="assignPage" class="ui modal">
            <div class="header">
                指派處理人員
            </div>
            <div class="content">
                <div class="ui info message">
                    <div class="header">
                        設定說明
                    </div>
                    <ul class="list">
                        <li>可從下拉選單中，選擇多個人員.</li>
                        <li>「確認派案」後，會發通知信給指定人員.</li>
                        <li>設定完成後，因信件已發，為避免爭議，不可修改已指定名單.</li>
                    </ul>
                </div>
                <div class="ui form">
                    <div class="field">
                        <label>選擇人員</label>
                        <asp:ListBox runat="server" ID="ddl_ProcWho" SelectionMode="Multiple"></asp:ListBox>
                    </div>
                </div>

            </div>
            <div class="actions">
                <div class="ui cancel button">
                    稍候處理，關閉視窗
                </div>
                <button id="doAssign" type="button" class="ui positive right labeled icon button" onclick="return confirm('確認名單正確無誤?')">確認派案<i class="checkmark icon"></i></button>
            </div>
        </div>
        <!-- 派案人員 End -->

        <!-- 結案 Start -->
        <div id="finishPage" class="ui modal">
            <div class="header">
                結案
            </div>
            <div class="content">
                <div class="ui small form">
                    <div class="two fields">
                        <div class="field">
                            <label>工作時數</label>
                            <input type="number" id="dia-finishHours" min="0" />
                        </div>
                        <div class="field">
                            <label>結案日期</label>
                            <div class="ui left icon input">
                                <input type="date" id="dia-finishDate" value="<%=DateTime.Today.ToString().ToDateString("yyyy-MM-dd") %>" />
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
        <!-- 結案 End -->

        <!-- Tips Start -->
        <div id="tipPage" class="ui modal">
            <div class="header">
                說明文件
            </div>
            <div class="content">
                <div class="ui header">
                    處理狀態說明
                </div>
                <ul class="ui list">
                    <li><span>未處理</span>：需求者發出新需求</li>
                    <li><span>派案中</span>：派案人員(主管)進入查看，尚未指定處理人員。</li>
                    <li><span>處理中</span>：已指定處理人員，案件正在處理中。</li>
                    <li><span>已完成</span>：案件已結案。</li>
                </ul>
                <div class="ui header">
                    通知信發送時機
                </div>
                <ul class="ui list">
                    <li><span>新需求</span>：通知執行單位(行企)、轉寄人員(若有勾選)</li>
                    <li><span>主管派案</span>：通知處理人員</li>
                    <li><span>結案</span>：通知執行單位、轉寄人員、需求者。</li>
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
    <script>
        $(function () {
            //init dropdown list
            $('select').dropdown();
            //tab menu
            $('.menu .item').tab();

            //[觸發][SAVE鈕]
            $("#doSave").click(function () {
                var flag = $("#MainContent_hf_Flag").val();

                //[Add時] 取得zTree已勾選的值, 並填入指定欄位 Start -----
                if (flag == "Add") {
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

                    //將陣列組成以','分隔的字串，並填入欄位
                    $("#MainContent_tb_InfoWho").val(valAry.join(","));
                }
                //[Add時] 取得zTree已勾選的值, 並填入指定欄位 End -----


                //觸發save
                $("#MainContent_btn_Save").trigger("click");
            });

            //[觸發][進度說明鈕]
            $("#doReply").click(function () {
                //觸發save
                $("#MainContent_btn_Reply").trigger("click");
            });


        });
    </script>
    <%-- 派案/結案 Modal --%>
    <script>
        $(function () {
            //說明視窗(Modal)
            $("#tips").click(function () {
                $('#tipPage').modal('show');
            });

            //派案人員視窗(Modal)
            $("#showAssign").click(function () {
                $('#assignPage').modal('show');
            });

            //派案人員確認鈕
            //serverside的欄位/按鈕,要放在本頁,不可放在modal
            $("#doAssign").click(function () {
                /*
                  取得處理人員,複選清單的值(工號)
                  ref:https://semantic-ui.com/modules/dropdown.html#/usage
                  return:陣列
                */
                var procValue = $("#MainContent_ddl_ProcWho").dropdown("get value");
                if (procValue.length > 0) {
                    //將陣列轉成以逗號分隔的字串
                    var myVals = procValue.join(",");
                    //填入隱藏欄位(傳遞時使用)
                    $("#MainContent_val_Proc").val(myVals);
                }

                //觸發按鈕
                $("#MainContent_btn_Assign").trigger("click");
            });


            //結案視窗(Modal)
            $("#showFinish").click(function () {
                $('#finishPage').modal('show');
            });
            //結案確認鈕
            $("#doFinish").click(function () {
                var hours = $("#dia-finishHours").val();
                var dt = $("#dia-finishDate").val();

                //填入隱藏欄位(傳遞時使用)
                $("#MainContent_val_FinishHours").val(hours);
                $("#MainContent_val_FinishDate").val(dt);

                //觸發按鈕
                $("#MainContent_btn_Finish").trigger("click");

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
                $("#MainContent_val_Dept").val(result.deptID);


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
                            description: item.Label + ' (' + item.Email + ')',
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

    <%-- zTree Start --%>
    <link rel="stylesheet" href="<%=fn_Param.WebUrl %>plugins/zTree/css/style.min.css" />
    <script src="<%=fn_Param.WebUrl %>plugins/zTree/jquery.ztree.core-3.5.min.js"></script>
    <script src="<%=fn_Param.WebUrl %>plugins/zTree/jquery.ztree.excheck-3.5.min.js"></script>
    <script>
        //--- zTree 設定 Start ---
        var setting = {
            view: {
                dblClickExpand: false   //已使用onclick展開,故將雙擊展開關閉                    
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
            var zTree = $.fn.zTree.getZTreeObj(treeId);
            zTree.expandNode(treeNode);
        }
        //--- zTree 設定 End ---
    </script>
    <script>
        $(function () {
            /*
                取得人員List
            */
            var jqxhr = $.post("<%=fn_Param.WebUrl%>Ajax_Data/GetAuthUserList.ashx", {
                block: 'Y',
            })
                  .done(function (data) {
                      //載入選單
                      $.fn.zTree.init($("#userList"), setting, data)
                  })
                  .fail(function () {
                      alert("人員選單載入失敗");
                  });

        });

    </script>
    <%-- zTree End --%>
</asp:Content>

