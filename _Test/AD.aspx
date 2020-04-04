<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AD.aspx.cs" Inherits="_Test_AD" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="btn_showGroup" runat="server" Text="Show Groups" OnClick="btn_showGroup_Click" />
            <asp:Button ID="btn_showUser" runat="server" Text="Show Users" OnClick="btn_showUser_Click" />
            <hr />
            Group:<asp:TextBox ID="tb_Group" runat="server"></asp:TextBox>
            <asp:Button ID="btn_showGrpUsers" runat="server" Text="Show Group Users" OnClick="btn_showGrpUsers_Click" />
            <hr />
            <asp:Button ID="btn_Clear" runat="server" Text="Clear" OnClick="btn_Clear_Click" Width="200px" /><br />
            <asp:TextBox ID="tb_Result" runat="server" TextMode="MultiLine" Width="90%" Height="400"></asp:TextBox>
        </div>
    </form>
</body>
</html>
