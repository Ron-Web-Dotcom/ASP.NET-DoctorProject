<%@ Page Title="Doctor Availability" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="DoctorAvailability.aspx.cs" Inherits="DoctorAvailability" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .avail-hero { background:linear-gradient(135deg,#2980b9,#1a5276); color:#fff; padding:30px 20px 22px; margin-bottom:28px; border-radius:0 0 6px 6px; }
    .avail-hero h2 { margin:0 0 4px; font-size:26px; }
    .avail-hero p  { margin:0; opacity:.85; font-size:14px; }
    .card { background:#fff; border:1px solid #dde3ea; border-radius:6px; padding:24px; margin-bottom:24px; box-shadow:0 1px 4px rgba(0,0,0,.06); }
    .card h4 { margin-top:0; color:#2980b9; border-bottom:1px solid #eef1f5; padding-bottom:10px; }
    .doctor-card { border:1px solid #d6eaf8; border-radius:6px; padding:18px 18px 14px; margin-bottom:16px; background:#f4f9ff; }
    .doctor-card h5 { margin:0 0 4px; color:#1a5276; font-size:16px; }
    .doctor-card .badge-specialty { background:#2980b9; color:#fff; border-radius:4px; padding:2px 8px; font-size:11px; margin-bottom:8px; display:inline-block; }
    .doctor-card .ai-summary { color:#555; font-size:13px; margin-top:8px; }
    .slots-list { margin:8px 0 0 0; padding:0; list-style:none; }
    .slots-list li { display:inline-block; background:#e8f4fd; color:#1a5276; border-radius:4px; padding:2px 8px; margin:2px 4px 2px 0; font-size:12px; }
</style>

<div class="avail-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-user"></span>&nbsp;Doctor Availability Viewer</h2>
        <p>Choose a specialty to see which doctors are available and get an AI-generated summary of each doctor's areas of focus.</p>
    </div>
</div>

<div class="container">
    <div class="card">
        <h4><span class="glyphicon glyphicon-search"></span> Select a Specialty</h4>
        <div class="row">
            <div class="col-sm-5">
                <asp:DropDownList ID="DdlSpecialty" runat="server" CssClass="form-control">
                    <asp:ListItem Value="">-- Choose a specialty --</asp:ListItem>
                    <asp:ListItem>Cardiology</asp:ListItem>
                    <asp:ListItem>General Practitioner</asp:ListItem>
                    <asp:ListItem>Gynaecology</asp:ListItem>
                    <asp:ListItem>Opticology</asp:ListItem>
                    <asp:ListItem>Paediatrics</asp:ListItem>
                    <asp:ListItem>Radiology</asp:ListItem>
                    <asp:ListItem>Surgery</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-sm-3">
                <asp:Button ID="BtnView" runat="server" Text="View Doctors"
                    CssClass="btn btn-primary" OnClick="BtnView_Click" />
            </div>
        </div>
        <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" style="display:block;margin-top:10px;" />
    </div>

    <asp:Panel ID="PanelResults" runat="server" Visible="false">
        <div class="card">
            <h4>
                <span class="glyphicon glyphicon-th-list"></span>
                Doctors in <asp:Literal ID="LitSpecialty" runat="server" />
            </h4>
            <asp:Repeater ID="RptDoctors" runat="server">
                <ItemTemplate>
                    <div class="doctor-card">
                        <h5>
                            <span class="glyphicon glyphicon-user"></span>&nbsp;
                            <%# System.Web.HttpUtility.HtmlEncode(Eval("Name").ToString()) %>
                        </h5>
                        <span class="badge-specialty"><%# System.Web.HttpUtility.HtmlEncode(Eval("Specialty").ToString()) %></span>
                        <ul class="slots-list">
                            <%# GetSlotBadges(Eval("Slots").ToString()) %>
                        </ul>
                        <div class="ai-summary">
                            <span class="glyphicon glyphicon-flash" style="color:#2980b9;"></span>
                            &nbsp;<em><%# System.Web.HttpUtility.HtmlEncode(Eval("AISummary").ToString()) %></em>
                        </div>
                        <div style="margin-top:12px;">
                            <a href="AppointmentForm.aspx" class="btn btn-primary btn-sm">Book with this Doctor</a>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>
</div>
</asp:Content>
