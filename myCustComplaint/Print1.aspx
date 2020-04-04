<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Print1.aspx.cs" Inherits="myCustComplaint_Print1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>客訴處理單</title>

    <asp:PlaceHolder ID="ph_Css" runat="server">
        <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-2.4.0/semantic.min.css" rel="stylesheet" />
        <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-2.4.0/_override.css?v=20190620" rel="stylesheet" />
    </asp:PlaceHolder>

</head>
<body>
    <form id="form1" runat="server">
        <div style="width: 840px;">
            <table class="ui celled large table">
                <tr>
                    <td colspan="2" class="center aligned">
                        <h2>客诉装箱单</h2>
                    </td>
                </tr>
                <tr>
                    <td class="right aligned" style="width: 25%"><b>客诉编号</b></td>
                    <td>
                        <h4>
                            <asp:Literal ID="lt_CCUID" runat="server"></asp:Literal></h4>
                    </td>
                </tr>
                <tr>
                    <td class="right aligned"><b>产品型号</b></td>
                    <td>
                        <h3>
                            <asp:Literal ID="lt_ModelNo" runat="server"></asp:Literal></h3>
                    </td>
                </tr>
                <tr>
                    <td class="right aligned"><b>数量</b></td>
                    <td>
                        <h3>
                            <asp:Literal ID="lt_Qty" runat="server"></asp:Literal></h3>
                    </td>
                </tr>
                <tr>
                    <td class="right aligned"><b>客户名称</b></td>
                    <td>
                        <asp:Literal ID="lt_CustTypeName" runat="server"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td class="right aligned"><b>單據日期</b></td>
                    <td>
                        <asp:Literal ID="info_CreateTime" runat="server"></asp:Literal>
                    </td>
                </tr>
            </table>
        </div>

    </form>
    <script src="<%=fn_Param.CDNUrl %>plugin/jQuery/jquery-3.3.1.min.js"></script>
    <script>
        $(function () {
            window.print();
        });
    </script>
</body>
</html>
