<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.Admin.Clients" AutoEventWireup="true"  Codebehind="Clients.ascx.cs" %>
<asp:Repeater ID="rpSearch" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpDataH" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpData" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpDataF" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>
