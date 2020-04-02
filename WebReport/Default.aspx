
<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" EnableSessionState="True" %>

<%@ Register Assembly="Reportman.Web" Namespace="Reportman.Web" TagPrefix="cc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Página sin título</title>
</head>
<body>
    <form id="fmain" defaultfocus="DropDownList1" runat="server">
    <div>
				&nbsp; &nbsp; &nbsp;
				<cc1:PreviewWeb ID="PreviewWeb1" runat="server" BorderStyle="None" />
		</div>
    </form>
</body>
</html>
