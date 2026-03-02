<%@ Page Title="Recovery Tracker" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="RecoveryTracker.aspx.cs" Inherits="RecoveryTracker" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .rec-hero{background:linear-gradient(135deg,#117a65,#0b5345);color:#fff;padding:30px 20px 22px;margin-bottom:28px;border-radius:0 0 6px 6px;}
    .rec-hero h2{margin:0 0 4px;font-size:26px;} .rec-hero p{margin:0;opacity:.85;font-size:14px;}
    .card{background:#fff;border:1px solid #dde3ea;border-radius:6px;padding:24px;margin-bottom:24px;box-shadow:0 1px 4px rgba(0,0,0,.06);}
    .card h4{margin-top:0;color:#117a65;border-bottom:1px solid #eef1f5;padding-bottom:10px;}
    .entry-item{background:#f0faf8;border-left:4px solid #117a65;padding:10px 14px;border-radius:0 4px 4px 0;margin-bottom:8px;font-size:13px;}
    .entry-date{font-weight:bold;color:#117a65;margin-right:8px;}
    .result-output{white-space:pre-wrap;font-size:14px;line-height:1.8;color:#333;}
    .rec-on-track  {background:#DFF0D8;border-left:4px solid #3C763D;padding:12px 16px;border-radius:4px;color:#3C763D;font-weight:bold;}
    .rec-contact   {background:#FCF8E3;border-left:4px solid #8A6D3B;padding:12px 16px;border-radius:4px;color:#8A6D3B;font-weight:bold;}
    .rec-urgent    {background:#F2DEDE;border-left:4px solid #A94442;padding:12px 16px;border-radius:4px;color:#A94442;font-weight:bold;}
    .disclaimer{font-size:12px;color:#888;margin-top:14px;border-top:1px solid #eee;padding-top:10px;}
</style>
<div class="rec-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-stats"></span>&nbsp;Post-Treatment Recovery Tracker</h2>
        <p>Log your daily recovery notes after a procedure and let GPT-4 analyse your progress and flag anything that needs attention.</p>
    </div>
</div>
<div class="container">
    <div class="row">
        <!-- Log entry panel -->
        <div class="col-md-5">
            <div class="card">
                <h4><span class="glyphicon glyphicon-plus-sign"></span> Add Today's Entry</h4>
                <div class="form-group">
                    <label>Procedure / Treatment</label>
                    <asp:TextBox ID="TxtProcedure" runat="server" CssClass="form-control" placeholder="e.g. Knee arthroscopy, appendectomy..." />
                </div>
                <div class="form-group">
                    <label>Date</label>
                    <asp:TextBox ID="TxtDate" runat="server" CssClass="form-control" placeholder="e.g. Day 3 post-op, or 02 Mar 2026" />
                </div>
                <div class="form-group">
                    <label>How are you feeling today?</label>
                    <asp:TextBox ID="TxtNote" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control"
                        placeholder="e.g. Pain level 4/10, swelling reduced slightly, managed a short walk, wound looking clean..." />
                </div>
                <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" /><br />
                <asp:Button ID="BtnAddEntry" runat="server" Text="Add Entry" CssClass="btn btn-success" OnClick="BtnAddEntry_Click" />
                <asp:Button ID="BtnClear" runat="server" Text="Clear All" CssClass="btn btn-danger" OnClick="BtnClear_Click" style="margin-left:8px;" />
            </div>
        </div>
        <!-- Entries + analysis -->
        <div class="col-md-7">
            <div class="card">
                <h4><span class="glyphicon glyphicon-list"></span> Recovery Log
                    <span class="badge" style="background:#117a65;"><asp:Literal ID="LitCount" runat="server" /></span>
                </h4>
                <asp:Panel ID="PanelNoEntries" runat="server">
                    <p class="text-muted">No entries yet. Add your first recovery note on the left.</p>
                </asp:Panel>
                <asp:Repeater ID="RptEntries" runat="server">
                    <ItemTemplate>
                        <div class="entry-item">
                            <span class="entry-date"><%# System.Web.HttpUtility.HtmlEncode(Eval("Date").ToString()) %></span>
                            <%# System.Web.HttpUtility.HtmlEncode(Eval("Note").ToString()) %>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="PanelAnalyseBtn" runat="server" Visible="false" style="margin-top:12px;">
                    <asp:Button ID="BtnAnalyse" runat="server" Text="Analyse My Recovery"
                        CssClass="btn btn-info" OnClick="BtnAnalyse_Click" />
                </asp:Panel>
            </div>
            <asp:Panel ID="PanelResult" runat="server" Visible="false">
                <div class="card">
                    <h4><span class="glyphicon glyphicon-flash"></span> Recovery Analysis</h4>
                    <asp:Literal ID="LitRecommendBadge" runat="server" />
                    <div class="result-output" style="margin-top:14px;"><asp:Literal ID="LitResult" runat="server" /></div>
                    <p class="disclaimer"><span class="glyphicon glyphicon-exclamation-sign"></span> This analysis is based on self-reported notes only. If you have any clinical concerns, contact your care team directly.</p>
                </div>
            </asp:Panel>
        </div>
    </div>
</div>
</asp:Content>
