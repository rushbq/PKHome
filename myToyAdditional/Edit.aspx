<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Edit.aspx.cs" Inherits="myToyAdditional_Edit" %>

<asp:Content ID="myCss" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="myBody" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- Sub Header Start -->
    <div class="grey lighten-3">
        <div class="container">
            <div class="row">
                <div class="col s12 m12 l12">
                    <h5 class="breadcrumbs-title">編輯資料<asp:Literal ID="lt_topInfo" runat="server"></asp:Literal></h5>
                    <ol class="breadcrumb">
                        <li><a>科學玩具補件登記簿</a></li>
                        <li><a href="<%=Page_SearchUrl %>">資料列表</a></li>
                        <li class="active">編輯資料</li>
                    </ol>
                </div>
            </div>
        </div>
    </div>
    <!-- Sub Header End -->
    <!-- Body Content Start -->
    <div class="container">
        <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
            <div class="card-panel red darken-1 white-text">
                <h4><i class="material-icons right">error_outline</i>糟糕了!!...發生了一點小問題</h4>
                <p>若持續看到此訊息, 請回報 <strong class="flow-text">詳細操作狀況</strong>。</p>
                <p>
                    <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                </p>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="ph_Data" runat="server">
            <div class="row">
                <div class="col s12 m9 l10">
                    <!-- // 接洽窗口 // -->
                    <div id="CreateInfo" class="scrollspy">
                        <ul class="collection with-header">
                            <li class="collection-header grey">
                                <h5 class="white-text">接洽窗口填寫</h5>
                            </li>
                            <li class="collection-item">
                                <div class="section">
                                    <div class="row">
                                        <div class="col s6">
                                            <label>系統編號</label>
                                            <div class="red-text text-darken-2 center-align">
                                                <b>
                                                    <asp:Literal ID="lt_DataID" runat="server">資料建立中</asp:Literal></b>
                                            </div>
                                        </div>
                                        <div class="input-field col s6">
                                            <asp:DropDownList ID="ddl_CustType" runat="server">
                                                <asp:ListItem Value="1">京東POP</asp:ListItem>
                                                <asp:ListItem Value="3">京東VC</asp:ListItem>
                                                <asp:ListItem Value="4">經銷商(eService)</asp:ListItem>
                                            </asp:DropDownList>
                                            <label for="MainContent_ddl_CustType">來源</label>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="input-field col s6">
                                            <asp:TextBox ID="tb_CustName" runat="server" MaxLength="40" data-length="40" CssClass="validate" required="" aria-required="true"></asp:TextBox>
                                            <label for="MainContent_tb_CustName" data-error="wrong" data-success="">
                                                聯繫人*
                                            </label>
                                        </div>
                                        <div class="input-field col s6">
                                            <asp:TextBox ID="tb_CustTel" runat="server" MaxLength="20" data-length="20" CssClass="validate" required="" aria-required="true"></asp:TextBox>
                                            <label for="MainContent_tb_CustTel">
                                                聯繫電話*
                                            </label>
                                            <span class="helper-text" data-error="wrong" data-success=""></span>
                                        </div>
                                    </div>

                                    <div class="row">
                                        <div class="input-field col s12">
                                            <asp:TextBox ID="tb_CustAddr" runat="server" MaxLength="200" data-length="200" CssClass="validate" required="" aria-required="true"></asp:TextBox>
                                            <label for="MainContent_tb_CustAddr" data-error="wrong" data-success="">
                                                配送地址*
                                            </label>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="input-field col s9">
                                            <asp:TextBox ID="AC_ModelNo" runat="server" CssClass="AC-ModelNo" data-target="MainContent_Rel_ModelNo_Val" data-label="MainContent_lb_ModelName" ValidationGroup="AddProd"></asp:TextBox>
                                            <asp:Label ID="lb_ModelName" runat="server" CssClass="orange-text text-darken-2 flow-text"></asp:Label>
                                            <asp:TextBox ID="Rel_ModelNo_Val" runat="server" ValidationGroup="AddProd" Style="display: none"></asp:TextBox>
                                            <label for="MainContent_Rel_ModelNo">
                                                品號
                                            </label>
                                            <small class="grey-text text-darken-2">(輸入產品關鍵字, 出現選單後, <u class="pink-text text-lighten-2">選擇你要的項目</u>)</small>
                                        </div>
                                        <div class="input-field col s3">
                                            <asp:TextBox ID="tb_Qty" runat="server" MaxLength="3" CssClass="center-align validate" type="number" min="1" max="99" required="" aria-required="true">1</asp:TextBox>
                                            <label for="MainContent_tb_Qty" data-error="請填數字" data-success="">
                                                數量*
                                            </label>
                                        </div>
                                    </div>
                                    <div class="section row">
                                        <div class="input-field col s12">
                                            <asp:TextBox ID="tb_Remark1" runat="server" TextMode="MultiLine" CssClass="materialize-textarea" MaxLength="500" data-length="500"></asp:TextBox>
                                            <label for="MainContent_tb_Remark1">Step 1.客戶反映狀況描述</label>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="input-field col s12">
                                            <asp:TextBox ID="tb_Remark2" runat="server" TextMode="MultiLine" CssClass="materialize-textarea" MaxLength="500" data-length="500"></asp:TextBox>
                                            <label for="MainContent_tb_Remark2">Step 2.解決方式回覆</label>
                                        </div>
                                    </div>

                                    <div class="row">
                                        <div class="input-field col s12">
                                            <asp:TextBox ID="tb_Remark3" runat="server" TextMode="MultiLine" CssClass="materialize-textarea" MaxLength="500" data-length="500"></asp:TextBox>
                                            <label for="MainContent_tb_Remark3">Step 3.需補備品明細</label>
                                        </div>
                                    </div>
                                </div>
                                <asp:PlaceHolder ID="ph_SaveBase" runat="server">
                                    <div class="row">
                                        <div class="col s12 right-align">
                                            <a class="btn waves-effect waves-light blue trigger-Save"><i class="material-icons left">save</i>存檔</a>
                                        </div>
                                    </div>
                                </asp:PlaceHolder>
                            </li>
                        </ul>
                    </div>


                    <!-- // 備貨單位 // -->
                    <asp:PlaceHolder ID="ph_ShipInfo" runat="server" Visible="false">
                        <div id="ShipInfo" class="scrollspy">
                            <ul class="collection with-header">
                                <li class="collection-header grey">
                                    <h5 class="white-text">備貨單位填寫</h5>
                                </li>
                                <li class="collection-item">
                                    <div class="section">
                                        <div class="row">
                                            <div class="input-field col s3">
                                                <i class="material-icons prefix">today</i>
                                                <asp:TextBox ID="tb_ShipDate" runat="server" CssClass="datepicker"></asp:TextBox>
                                                <label for="MainContent_tb_ShipDate">
                                                    寄出日
                                                </label>
                                            </div>
                                            <div class="input-field col s6">
                                                <asp:TextBox ID="tb_ShipNo" runat="server" MaxLength="40" data-length="40"></asp:TextBox>
                                                <label for="MainContent_tb_ShipNo">
                                                    貨運及單號
                                                </label>
                                            </div>
                                            <div class="input-field col s3">
                                                <asp:TextBox ID="tb_Freight" runat="server" MaxLength="6" CssClass="center-align">0</asp:TextBox>
                                                <label for="MainContent_tb_Freight">
                                                    運費
                                                </label>
                                            </div>
                                        </div>
                                    </div>
                                    <asp:PlaceHolder ID="ph_SaveShip" runat="server">
                                        <div class="row">
                                            <div class="col s12 right-align">
                                                <a class="btn waves-effect waves-light blue trigger-Ship"><i class="material-icons left">save</i>存檔</a>
                                            </div>
                                        </div>
                                    </asp:PlaceHolder>
                                </li>
                            </ul>
                        </div>
                    </asp:PlaceHolder>

                    <!-- // 維護資訊 // -->
                    <div>
                        <ul class="collection with-header">
                            <li class="collection-header grey">
                                <h5 class="white-text">維護資訊</h5>
                            </li>
                            <li class="collection-item">
                                <table class="bordered striped responsive-table">
                                    <tbody>
                                        <tr>
                                            <th>建立者
                                            </th>
                                            <td>
                                                <asp:Literal ID="lt_Creater" runat="server"></asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="lt_CreateTime" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>最後更新
                                            </th>
                                            <td>
                                                <asp:Literal ID="lt_Updater" runat="server"></asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="lt_UpdateTime" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>出貨者
                                            </th>
                                            <td>
                                                <asp:Literal ID="lt_Shipper" runat="server"></asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="lt_ShipTime" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </li>
                        </ul>
                    </div>
                </div>
                <div class="col hide-on-small-only m3 l2">
                    <!-- // 快速導覽按鈕 // -->
                    <div class="table-Nav">
                        <ul class="table-of-contents">
                            <li><a href="#CreateInfo">接洽窗口填寫</a></li>
                            <asp:PlaceHolder ID="ph_ShipNav" runat="server" Visible="false">
                                <li><a href="#ShipInfo">備貨單位填寫</a></li>
                            </asp:PlaceHolder>
                            <li></li>
                            <li><a href="<%=Page_SearchUrl %>"><i class="material-icons left">list</i>回列表</a></li>
                        </ul>
                    </div>
                </div>

                <!-- // Hidden buttons // -->
                <div class="SrvSide-Buttons" style="display: none;">
                    <asp:HiddenField ID="hf_DataID" runat="server" />
                    <asp:Button ID="btn_Save" runat="server" Text="Save1" OnClick="btn_Save_Click" />
                    <asp:Button ID="btn_Ship" runat="server" Text="Save2" OnClick="btn_Ship_Click" />

                </div>
            </div>

        </asp:PlaceHolder>
    </div>
    <!-- Body Content End -->
</asp:Content>
<asp:Content ID="myBottom" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="myScript" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script src="<%=fn_Param.CDNUrl %>plugin/Materialize/v0.97.8/lib/pickadate/translation/zh_TW.js"></script>
    <script>
        (function ($) {
            $(function () {
                //載入選單
                $('select').material_select();


                //scrollSpy
                $('.scrollspy').scrollSpy();

                //pushpin
                $('.table-Nav').pushpin({
                    top: 97
                });

                //載入datepicker
                $('.datepicker').pickadate({
                    selectMonths: true, // Creates a dropdown to control month
                    selectYears: 5, // Creates a dropdown of 15 years to control year
                    format: 'yyyy-mm-dd'
                }).on('change', function () {
                    $(this).next().find('.picker__close').click();
                });


                //trigger Save1
                $(".trigger-Save").click(function () {
                    $("#MainContent_btn_Save").trigger("click");
                });
                //trigger Save2
                $(".trigger-Ship").click(function () {
                    $("#MainContent_btn_Ship").trigger("click");
                });

            }); // end of document ready
        })(jQuery); // end of jQuery name space


    </script>

    <link href="<%=fn_Param.CDNUrl %>plugin/jqueryUI-1.12.1/jquery-ui.min.css" rel="stylesheet" />
    <link href="<%=fn_Param.CDNUrl %>plugin/jqueryUI-1.12.1/catcomplete/catcomplete.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/jqueryUI-1.12.1/jquery-ui.min.js"></script>
    <script src="<%=fn_Param.CDNUrl %>plugin/jqueryUI-1.12.1/catcomplete/catcomplete.js"></script>
    <%-- Catcomplete Start --%>
    <script>
        /* Autocomplete 品號關聯 */
        $(".AC-ModelNo").catcomplete({
            minLength: 1,  //至少要輸入 n 個字元
            source: function (request, response) {
                $.ajax({
                    url: "<%=fn_Param.WebUrl %>Ajax_Data/GetData_Prod.ashx",
                    data: {
                        keyword: request.term
                    },
                    type: "POST",
                    dataType: "json",
                    success: function (data) {
                        if (data != null) {
                            response($.map(data, function (item) {
                                return {
                                    id: item.ID,
                                    label: '(' + item.ID + ') ' + item.Label,
                                    name: item.Label,
                                    category: item.Category
                                }
                            }));
                        }
                    }
                });
            },
            select: function (event, ui) {
                //目前欄位
                $(this).val(ui.item.value);

                //實際欄位-儲存值
                var targetID = $(this).attr("data-target");
                $("#" + targetID).val(ui.item.id);
                var targetName = $(this).attr("data-label");
                $("#" + targetName).text(ui.item.name);

                event.preventDefault();
            }
        });
    </script>

</asp:Content>

