<%@ Page Title="開案中客訴-編輯" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SettingEdit.aspx.cs" Inherits="myCustComplaint_SettingEdit" %>

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
                    <h5 class="active section red-text text-darken-2">
                        <asp:Literal ID="lt_TypeName" runat="server" Text="功能名稱"></asp:Literal>
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a class="item" id="tips">
                    <i class="question circle icon"></i>
                </a>
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
                                <h5 class="ui header"><a class="anchor" id="baseData"></a>基本資料</h5>
                            </div>
                            <div class="ui small form segment">
                                <div class="fields">
                                    <div class="four wide field">
                                        <label>追蹤碼</label>
                                        <div class="ui red basic large label">
                                            <asp:Literal ID="lt_TraceID" runat="server">資料建立中</asp:Literal>
                                        </div>
                                    </div>
                                    <div class="eight wide field">
                                        <label>目前狀態</label>
                                        <asp:PlaceHolder ID="ph_CS" runat="server">
                                            <div class="ui grey basic label">客服單位未填寫</div>
                                        </asp:PlaceHolder>
                                        <asp:PlaceHolder ID="ph_Freight" runat="server">
                                            <div class="ui grey basic label">收貨單位未填寫</div>
                                        </asp:PlaceHolder>
                                        <asp:PlaceHolder ID="ph_ProdCnt" runat="server">
                                            <div class="ui red basic label">商品資料未填寫</div>
                                        </asp:PlaceHolder>
                                        <asp:PlaceHolder ID="ph_Status" runat="server">
                                            <div class="ui orange basic label">待確認開案</div>
                                        </asp:PlaceHolder>
                                    </div>
                                    <div class="four wide field">
                                        <label>建立日期</label>
                                        <div class="ui basic large label">
                                            <asp:Literal ID="lt_CreateDate" runat="server">資料建立中</asp:Literal>
                                        </div>
                                    </div>
                                </div>

                            </div>
                            <div class="ui bottom attached info small message">
                                <ul>
                                    <li>本頁為「未開案客訴」，資料填寫完整後才可開案。</li>
                                    <li>客服資料、收貨資料、商品資料，以上填寫完畢後，右方會出現「確認開案」的按鈕。</li>
                                    <li>「確認開案」按下後，會依商品資料的拆分規則，建立對應的客訴單。</li>
                                </ul>
                            </div>
                        </div>
                        <!-- Section-基本資料 End -->

                        <!-- Section-流程資料 Start -->
                        <div class="ui segments">
                            <!-- Flow_客服資料 Start -->
                            <div class="ui brown segment">
                                <h5 class="ui header"><a class="anchor" id="flow101"></a>客服資料&nbsp;
                                    <small class="grey-text">(F101)</small></h5>
                            </div>
                            <div id="form101" class="ui small form segment">
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide required field">
                                                <label>計劃處理方式</label>
                                                <asp:DropDownList ID="ddl_PlanType" runat="server">
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide required field">
                                                <label>選擇客戶類別</label>
                                                <asp:DropDownList ID="ddl_CustType" runat="server">
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="ten wide field">
                                                <label>客戶(經銷商)</label>
                                                <div class="ui fluid search ac-Cust">
                                                    <div class="ui left icon right labeled input">
                                                        <asp:TextBox ID="tb_Cust" runat="server" CssClass="prompt" placeholder="請填關鍵字查詢"></asp:TextBox>
                                                        <i class="search icon"></i>
                                                        <asp:Label ID="lb_CustName" runat="server" CssClass="ui label" Text="輸入關鍵字,選擇項目"></asp:Label>
                                                    </div>
                                                    <asp:TextBox ID="val_CustID" runat="server" Style="display: none"></asp:TextBox>
                                                </div>
                                            </div>
                                            <div class="six wide field">
                                                <label>&nbsp;</label>
                                                <asp:TextBox ID="tb_ERP_ID" runat="server" MaxLength="20" placeholder="ERP銷貨單號(單別+單號)"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="ten wide field">
                                                <label>商城</label>
                                                <asp:DropDownList ID="ddl_Mall" runat="server">
                                                </asp:DropDownList>
                                            </div>
                                            <div class="six wide field">
                                                <label>&nbsp;</label>
                                                <asp:TextBox ID="tb_Platform_ID" runat="server" MaxLength="20" placeholder="平台單號"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>備註</label>
                                                <asp:TextBox ID="tb_CustInput" runat="server" MaxLength="40" placeholder="備註"></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide required field">
                                                <label>是否有發票退回</label>
                                                <asp:DropDownList ID="ddl_InvoiceIsBack" runat="server" CssClass="fluid">
                                                    <asp:ListItem Value="Y">是</asp:ListItem>
                                                    <asp:ListItem Value="N" Selected="True">否</asp:ListItem>
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide required field">
                                                <label>聯絡人</label>
                                                <asp:TextBox ID="tb_BuyerName" runat="server" MaxLength="40" placeholder="聯絡人姓名"></asp:TextBox>
                                            </div>
                                            <div class="nine wide required field">
                                                <label>聯絡電話</label>
                                                <asp:TextBox ID="tb_BuyerPhone" runat="server" MaxLength="40" placeholder="市話或手機"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide required field">
                                                <label>聯絡地址</label>
                                                <asp:TextBox ID="tb_BuyerAddr" runat="server" MaxLength="200" placeholder="完整地址"></asp:TextBox>
                                            </div>

                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field grey-text text-darken-2">
                                                <label>填寫人員</label>
                                                <asp:Literal ID="lt_CS_Who" runat="server"></asp:Literal>
                                            </div>
                                            <div class="nine wide field grey-text text-darken-2">
                                                <label>填寫時間</label>
                                                <asp:Literal ID="lt_CS_Time" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="ui right aligned segment">
                                <div class="ui right pointing grey basic label">
                                    「客服資料」填寫完畢後，請按這裡
                                </div>
                                <button id="doSave101" type="button" class="ui green small button"><i class="save icon"></i>儲存客服資料</button>
                                <asp:Button ID="btn_doSave101" runat="server" Text="Save" OnClick="btn_doSave101_Click" Style="display: none;" />

                            </div>
                            <!-- Flow_客服資料 End -->
                            <!-- Flow_收貨資料 Start -->
                            <div class="ui brown segment">
                                <h5 class="ui header"><a class="anchor" id="flow102"></a>收貨資料&nbsp;
                                    <small class="grey-text">(F102)</small></h5>
                            </div>
                            <div id="form102" class="ui small form segment">
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide required field">
                                                <label>選擇收貨方式</label>
                                                <asp:DropDownList ID="ddl_FreightType" runat="server">
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="eight wide field">
                                                <label>若有發票,請填發票號碼</label>
                                                <asp:TextBox ID="tb_InvoiceNumber" runat="server" MaxLength="40" placeholder="輸入發票號碼"></asp:TextBox>
                                            </div>
                                            <div class="eight wide field">
                                                <label>發票金額</label>
                                                <asp:TextBox ID="tb_InvoicePrice" runat="server" MaxLength="20" placeholder="輸入發票金額" type="number" step="any" min="0">0</asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>快遞單號</label>
                                                <asp:TextBox ID="tb_FreightInput" runat="server" MaxLength="40" placeholder="快遞單號"></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="seven wide required field">
                                                <label>收貨時間</label>
                                                <div class="ui left icon input datepicker">
                                                    <asp:TextBox ID="tb_FreightGetDate" runat="server" MaxLength="20" autocomplete="off" placeholder="格式:西元年/月/日"></asp:TextBox>
                                                    <i class="calendar alternate outline icon"></i>
                                                </div>
                                            </div>
                                            <div class="nine wide field">
                                                <label>快遞公司</label>
                                                <asp:TextBox ID="tb_ShipComp" runat="server" MaxLength="40" placeholder="快遞公司名稱"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field">
                                                <label>客戶姓名</label>
                                                <asp:TextBox ID="tb_ShipWho" runat="server" MaxLength="40" placeholder="客戶姓名"></asp:TextBox>
                                            </div>
                                            <div class="nine wide field">
                                                <label>客戶電話</label>
                                                <asp:TextBox ID="tb_ShipTel" runat="server" MaxLength="50" placeholder="客戶電話"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>客戶地址</label>
                                                <asp:TextBox ID="tb_ShipAddr" runat="server" MaxLength="200" placeholder="客戶完整地址"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field grey-text text-darken-2">
                                                <label>填寫人員</label>
                                                <asp:Literal ID="lt_Freight_Who" runat="server"></asp:Literal>
                                            </div>
                                            <div class="nine wide field grey-text text-darken-2">
                                                <label>填寫時間</label>
                                                <asp:Literal ID="lt_Freight_Time" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="ui right aligned segment">
                                <div class="ui right pointing grey basic label">
                                    「收貨資料」填寫完畢後，請按這裡
                                </div>
                                <button id="doSave102" type="button" class="ui green small button"><i class="save icon"></i>儲存收貨資料</button>
                                <asp:Button ID="btn_doSave102" runat="server" Text="Save" OnClick="btn_doSave102_Click" Style="display: none;" />
                            </div>
                            <!-- Flow_收貨資料 End -->
                        </div>
                        <!-- Section-流程資料 End -->


                        <!-- Section-收貨圖片 Start -->
                        <div class="ui segments">
                            <div class="ui blue segment">
                                <h5 class="ui header"><a class="anchor" id="attfiles"></a>收貨圖片</h5>
                            </div>
                            <div class="ui segment">
                                <div class="ui small form">
                                    <div class="fields">
                                        <div class="sixteen wide field">
                                            <label>附加檔案&nbsp;<span class="grey-text text-darken-1">(單次上傳最多 5 筆, 大小為 50MB, 限 <%=FileExtLimit.Replace("|", ", ") %>)</span></label>
                                            <div class="fields">
                                                <div class="thirteen wide field">
                                                    <asp:FileUpload ID="fu_Attachment" runat="server" AllowMultiple="true" />
                                                </div>
                                                <div class="three wide field">
                                                    <asp:Button ID="btn_Upload" runat="server" Text="新增" CssClass="ui teal tiny button" OnClick="btn_Upload_Click" />
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
                                                <a href="<%#fn_Param.RefUrl %><%#UploadFolder %><%#Eval("FilePath") %>" target="_blank"><%#Eval("AttachFile_Org") %></a>
                                            </td>
                                            <td class="collapsing grey-text text-darken-2"><%#Eval("Create_Name") %></td>
                                            <td class="collapsing center aligned">
                                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui mini orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
                                                <asp:HiddenField ID="hf_FileName" runat="server" Value='<%#Eval("AttachFile") %>' />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:ListView>
                            </div>
                        </div>
                        <!-- Section-收貨圖片 End -->


                        <!-- Section-商品資料 Start -->
                        <div class="ui segments">
                            <div class="ui blue segment">
                                <h5 class="ui header"><a class="anchor" id="prodData"></a>商品資料</h5>
                            </div>
                            <div class="ui segment">
                                <div class="ui small form">
                                    <div class="fields">
                                        <div class="twelve wide field">
                                            <div class="fields">
                                                <div class="seven wide required field">
                                                    <label>品號</label>
                                                    <div class="ui fluid search ac-ModelNo">
                                                        <div class="ui left icon right labeled input">
                                                            <asp:TextBox ID="filter_ModelNo" runat="server" CssClass="prompt" placeholder="輸入品號或品名關鍵字"></asp:TextBox>
                                                            <i class="search icon"></i>
                                                            <asp:Panel ID="lb_ModelNo" runat="server" CssClass="ui label">輸入關鍵字,選擇項目</asp:Panel>
                                                        </div>
                                                        <asp:TextBox ID="val_ModelNo" runat="server" Style="display: none"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="three wide required field">
                                                    <label>數量</label>
                                                    <asp:TextBox ID="tb_Qty" runat="server" autocomplete="off" type="number" min="1" Style="text-align: center;">1</asp:TextBox>
                                                </div>
                                                <div class="three wide field">
                                                    <label>拆單?</label>
                                                    <asp:DropDownList ID="ddl_IsSplit" runat="server" CssClass="fluid">
                                                        <asp:ListItem Value="Y" Selected="True">是</asp:ListItem>
                                                        <asp:ListItem Value="N">否</asp:ListItem>
                                                    </asp:DropDownList>
                                                </div>
                                                <div class="three wide field">
                                                    <label>保固內?</label>
                                                    <asp:DropDownList ID="ddl_IsWarranty" runat="server" CssClass="fluid">
                                                        <asp:ListItem Value="Y" Selected="True">是</asp:ListItem>
                                                        <asp:ListItem Value="N">否</asp:ListItem>
                                                    </asp:DropDownList>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="four wide field">
                                            <label>Excel批次匯入&nbsp;<a href="<%=fn_Param.CDNUrl %>PKHome/CustComplaint/Import_ItemNo.xlsx" target="_blank">(範本下載)</a></label>
                                            <div class="ui action input">
                                                <asp:FileUpload ID="fu_File" runat="server" AllowMultiple="false" accept=".xlsx" />
                                                <asp:Button ID="btn_Import" runat="server" Text="批次匯入" CssClass="ui orange tiny button" OnClick="btn_Import_Click" OnClientClick="return confirm('確定執行匯入?\n商品資料會先清空後再匯入.')" />
                                            </div>
                                        </div>
                                    </div>
                                    <div class="fields">
                                        <div class="twelve wide field">
                                            <div class="fields">
                                                <div class="fourteen wide field">
                                                    <asp:TextBox ID="tb_Remark" runat="server" autocomplete="off" MaxLength="80" placeholder="填寫客訴內容"></asp:TextBox>
                                                </div>
                                                <div class="two wide field">
                                                    <asp:Button ID="btn_SaveDetail" runat="server" Text="單筆新增" CssClass="ui teal tiny fluid button" OnClick="btn_SaveDetail_Click" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="four wide field">
                                            
                                        </div>
                                    </div>
                                </div>

                                <div class="ui divider"></div>

                                <asp:ListView ID="lv_Detail" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lv_Detail_ItemCommand">
                                    <LayoutTemplate>
                                        <table class="ui celled selectable compact small table">
                                            <thead>
                                                <tr>
                                                    <th class="grey-bg lighten-3">品號</th>
                                                    <th class="grey-bg lighten-3 center aligned">數量</th>
                                                    <th class="grey-bg lighten-3 center aligned">是否拆單</th>
                                                    <th class="grey-bg lighten-3 center aligned">保固內</th>
                                                    <th class="grey-bg lighten-3">客訴內容</th>
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
                                            <td>
                                                <strong class="blue-text text-darken-3"><%#Eval("ModelNo") %></strong>
                                            </td>
                                            <td class="center aligned"><%#Eval("Qty") %></td>
                                            <td class="center aligned"><%#Eval("IsSplit") %></td>
                                            <td class="center aligned"><%#Eval("IsWarranty") %></td>
                                            <td><%#Eval("Remark") %></td>
                                            <td class="center aligned collapsing">
                                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("Data_ID") %>' />
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
                            </div>
                        </div>
                        <!-- Section-商品資料 End -->

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
                            <a href="#baseData" class="item">基本資料</a>
                            <a href="#flow101" class="item">客服資料</a>
                            <a href="#flow102" class="item">收貨資料</a>
                            <a href="#attfiles" class="item">收貨圖片</a>
                            <a href="#prodData" class="item">商品資料</a>
                            <a href="#top" class="item"><i class="angle double up icon"></i>到頂端</a>
                        </div>

                        <div class="ui vertical text menu">
                            <div class="header item">功能按鈕</div>
                            <div class="item">
                                <a href="<%:Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i>返回列表</a>
                            </div>
                            <asp:PlaceHolder ID="ph_Invoke" runat="server" Visible="false">
                                <div class="item">
                                    <button id="doInvoke" type="button" class="ui orange small button"><i class="gavel icon"></i>確認開案</button>
                                </div>
                            </asp:PlaceHolder>
                        </div>
                        <div class="serverController" style="display: none;">
                            <asp:Button ID="btn_Invoke" runat="server" Text="Next" OnClick="btn_Invoke_Click" Style="display: none;" />
                            <asp:Button ID="btn_NewJob" runat="server" Text="NewJob" OnClick="btn_NewJob_Click" Style="display: none;" />
                        </div>
                    </div>
                </div>
                <!-- Right Navi Menu End -->
            </div>
        </div>

        <!-- Lock Modal Start -->
        <asp:PlaceHolder ID="ph_LockModal" runat="server" Visible="false">
            <div id="lockPage" class="ui small basic modal">
                <div class="ui icon header">
                    <i class="info circle icon"></i>
                    新增客訴資料-確認視窗
                </div>
                <div class="content">
                    <p>按下「確定執行」後，系統會產生<u>追蹤編號</u>及系統資料。</p>
                    <p>完畢後，「客服單位」及「收貨單位」即可開始填寫資料。</p>
                    <p>若要取消，請按下「取消，返回列表」</p>
                </div>
                <div class="actions">
                    <div class="ui red basic cancel inverted button">
                        <i class="remove icon"></i>
                        取消，返回列表
                    </div>
                    <div class="ui green ok inverted button">
                        <i class="checkmark icon"></i>
                        確定執行
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>
        <!-- Lock Modal End -->

        <!-- Tips Start -->
        <div id="tipPage" class="ui modal">
            <div class="header">
                說明文件
            </div>
            <div class="content">
                <div class="ui header">
                    功能按鈕
                </div>
                <ul class="ui list">
                    <li><span>返回列表</span>：前往開案中客訴清單.</li>
                    <li><span class="green-text">資料存檔</span>：僅儲存資料.</li>
                    <li><span class="orange-text">確認開案</span>：拆分商品資料,建立客訴單,並發信通知一線人員.</li>
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
            //init radio
            $('.ui.radio.checkbox').checkbox();

            //init dropdown list
            $('select').dropdown();

            //說明視窗(Modal)
            $("#tips").click(function () {
                $('#tipPage').modal('show');
            });

            //Save Click
            $("#doSave101").click(function () {
                $("#form101").addClass("loading");
                $("#MainContent_btn_doSave101").trigger("click");
            });
            $("#doSave102").click(function () {
                $("#form102").addClass("loading");
                $("#MainContent_btn_doSave102").trigger("click");
            });
            $("#doInvoke").click(function () {
                if (confirm("確認開案?")) {
                    $("#form101").addClass("loading");
                    $("#form102").addClass("loading");
                    $(this).addClass("loading disabled");

                    $("#MainContent_btn_Invoke").trigger("click");
                }

            });

        });
    </script>
    <asp:PlaceHolder ID="ph_LockScript" runat="server" Visible="false">
        <script>
            //Lock顯示(Modal), 後端控制顯示與否.
            $('#lockPage').modal({
                closable: false,
                onDeny: function () {
                    window.location.href = '<%:Page_SearchUrl %>';
                },
                onApprove: function () {
                    //觸發button
                    $("#MainContent_btn_NewJob").trigger("click");
                }
            }).modal('show');
        </script>
    </asp:PlaceHolder>

    <%-- 快速選單 --%>
    <script src="<%=fn_Param.WebUrl %>javascript/sticky.js"></script>
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

    <%-- Search UI Start --%>
    <script>
        /* 品號 (使用category) */
        $('.ac-ModelNo').search({
            type: 'category',
            minCharacters: 1,
            searchFields: [
                'title',
                'description'
            ]
            , onSelect: function (result, response) {
                //console.log(result.title);
                $("#MainContent_val_ModelNo").val(result.title);
                $("#MainContent_lb_ModelNo").text(result.description);

            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Prod_v1.ashx?q={query}',
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
                          maxResults = 10
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
                            description: item.Label
                        });
                    });
                    return response;
                }
            }

        });

    </script>
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
                $("#MainContent_val_CustID").val(result.ID);
                $("#MainContent_lb_CustName").text(result.Label);
                $("#MainContent_tb_BuyerName").val(result.ContactWho);
                $("#MainContent_tb_BuyerPhone").val(result.Tel);
                $("#MainContent_tb_BuyerAddr").val(result.ContactAddr);

            }
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?q={query}'
            }

        });
    </script>
    <%-- Search UI End --%>
</asp:Content>

