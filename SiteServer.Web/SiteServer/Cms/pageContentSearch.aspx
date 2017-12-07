﻿<%@ Page Language="C#" Inherits="SiteServer.BackgroundPages.Cms.PageContentSearch" %>
  <%@ Register TagPrefix="bairong" Namespace="SiteServer.BackgroundPages.Controls" Assembly="SiteServer.BackgroundPages" %>
    <!DOCTYPE html>
    <html>

    <head>
      <meta charset="utf-8">
      <!--#include file="../inc/header.aspx"-->
    </head>

    <body>
      <!--#include file="../inc/openWindow.html"-->
      <form class="form-inline" runat="server">
        <asp:Literal id="LtlBreadCrumb" runat="server" />
        <bairong:alerts runat="server" />

        <script type="text/javascript">
          $(document).ready(function () {
            loopRows(document.getElementById('contents'), function (cur) {
              cur.onclick = chkSelect;
            });
            $(".popover-hover").popover({
              trigger: 'hover',
              html: true
            });
          });
        </script>

        <div class="well well-small">
          <table class="table table-noborder">
            <tr>
              <td>
                栏目：
                <asp:DropDownList ID="DdlChannelId" AutoPostBack="true" OnSelectedIndexChanged="Search_OnClick" runat="server"></asp:DropDownList>
                内容状态：
                <asp:DropDownList ID="DdlState" AutoPostBack="true" OnSelectedIndexChanged="Search_OnClick" class="input-small" runat="server"></asp:DropDownList>
                <asp:CheckBox ID="CbIsDuplicate" class="checkbox inline" Text="包含重复标题" runat="server"></asp:CheckBox>
              </td>
            </tr>
            <tr>
              <td>
                时间：从
                <bairong:DateTimeTextBox id="TbDateFrom" class="input-small" Columns="12" runat="server" /> &nbsp;到&nbsp;
                <bairong:DateTimeTextBox id="TbDateTo" class="input-small" Columns="12" runat="server" /> 目标：
                <asp:DropDownList ID="DdlSearchType" class="input-small" runat="server"> </asp:DropDownList>
                关键字：
                <asp:TextBox id="TbKeyword" MaxLength="500" Size="37" runat="server" />
                <asp:Button class="btn" OnClick="Search_OnClick" id="Search" text="搜 索" runat="server" />
              </td>
            </tr>
          </table>
        </div>

        <table id="contents" class="table table-bordered table-hover">
          <tr class="info thead">
            <td>内容标题(点击查看) </td>
            <td>栏目</td>
            <asp:Literal ID="LtlColumnHeadRows" runat="server"></asp:Literal>
            <td width="50"> 状态 </td>
            <td width="30">&nbsp;</td>
            <asp:Literal ID="LtlCommandHeadRows" runat="server"></asp:Literal>
            <td width="20">
              <input type="checkbox" onClick="selectRows(document.getElementById('contents'), this.checked);">
            </td>
          </tr>
          <asp:Repeater ID="RptContents" runat="server">
            <itemtemplate>
              <tr>
                <td>
                  <asp:Literal ID="ltlItemTitle" runat="server"></asp:Literal>
                </td>
                <td>
                  <asp:Literal ID="ltlChannel" runat="server"></asp:Literal>
                </td>
                <asp:Literal ID="ltlColumnItemRows" runat="server"></asp:Literal>
                <td class="center" nowrap>
                  <asp:Literal ID="ltlItemStatus" runat="server"></asp:Literal>
                </td>
                <td class="center">
                  <asp:Literal ID="ltlItemEditUrl" runat="server"></asp:Literal>
                </td>
                <asp:Literal ID="ltlCommandItemRows" runat="server"></asp:Literal>
                <td class="center">
                  <asp:Literal ID="ltlSelect" runat="server"></asp:Literal>
                </td>
              </tr>
            </itemtemplate>
          </asp:Repeater>
        </table>

        <bairong:sqlPager id="SpContents" runat="server" class="table table-pager" />

        <ul class="breadcrumb breadcrumb-button">
          <asp:Button class="btn" id="BtnAddContent" OnClick="AddContent_OnClick" Text="添加信息" runat="server" />
          <asp:Button class="btn" id="BtnSelect" Text="选择显示项" runat="server" />
          <asp:Button class="btn" id="BtnAddToGroup" Text="添加到内容组" runat="server" />
          <asp:Button class="btn" id="BtnTranslate" Text="转 移" runat="server" />
          <asp:PlaceHolder ID="PhCheck" runat="server">
            <asp:Button class="btn" id="BtnCheck" Text="审 核" runat="server" />
          </asp:PlaceHolder>
          <asp:Button class="btn" id="BtnDelete" Text="删 除" runat="server" />
        </ul>

      </form>
    </body>

    </html>