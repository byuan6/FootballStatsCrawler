<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="viewschedule.aspx.cs" Inherits="FFToiletBowlWeb.viewschedule" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:GridView ID="ScheduleGrid" AutoGenerateColumns="true" ShowHeader="true" runat="server"/>
    </div>
    </form>
    <asp:Label ID="Hint" runat="server" />
</body>
</html>
