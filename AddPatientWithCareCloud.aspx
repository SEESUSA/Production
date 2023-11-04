﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AddPatientWithCareCloud.aspx.cs" Inherits="AddPatientWithCareCloud" %>

<%@ Register Src="CheckLogon.ascx" TagName="CheckLogon" TagPrefix="uc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="x-ua-compatible" content="ie=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link href="styles.css" rel="stylesheet" />
      <script src="Scripts/jquery-3.6.0.min.js"></script>
    <script src="Scripts/jquery-ui-1.13.2.min.js"></script>
    <title><%=Application.Get("Title").ToString() %></title>
    <uc1:CheckLogon ID="CheckLogon1" runat="server" />
    <script type="text/javascript">
        $(document).ready(function () {
            $("#txtPhone").keypress(function () {
                var phonenumber = $("#txtPhone").val();
                //alert(phonenumber);
                //alert(phonenumber.length);
                if (phonenumber.length == 3) {
                  //  alert(1);
                    $("#txtPhone").val(phonenumber + '-');
                }
                else if (phonenumber.length == 7) {
                    $("#txtPhone").val(phonenumber + '-');
                }
                else {
                   
                }
            });

            $("#txtDOB").keypress(function () {
                var DOB = $("#txtDOB").val();
                if (DOB.length == 2) {
                    $("#txtDOB").val(DOB + '-');
                }
                else if (DOB.length == 5) {
                    $("#txtDOB").val(DOB + '-');
                }
                else {

                }
                //if (DOB.length == 4) {
                //    $("#txtDOB").val(DOB + '-');
                //}
                //else if (DOB.length == 7) {
                //    $("#txtDOB").val(DOB + '-');
                //}
                //else {

                //}
            });
            $("#txtSSN").keypress(function () {
                var SSN = $("#txtSSN").val();
                if (SSN.length == 3) {
                    $("#txtSSN").val(SSN + '-');
                }
                else if (SSN.length == 6) {
                    $("#txtSSN").val(SSN + '-');
                }
                else {

                }
            });
        });
       
    </script>
</head>
<body>
    <header>
        <img src="img/MicrosoftTeams-image.png" alt="eye health partners/vision america portal" width="397" height="44"/>
        <h2 class="PageTitle">New Patient</h2>
    </header>
    <form id="form1" runat="server">
        <div style="width:100%;">
            
            <label>First Name <i style="color:red">*</i>
                <asp:TextBox runat="server" ID="txtFirstName" CssClass="TextField" MaxLength="35" /></label>
            <label>Last Name  <i style="color:red">*</i>
                <asp:TextBox runat="server" ID="txtLastName" CssClass="TextField" MaxLength="60" /></label>
            <label>Gender  <i style="color:red">*</i>
                <asp:DropDownList runat="server" ID="cboGender" CssClass="TextField" /></label>
            <label>Date of Birth  <i style="color:red">*</i>
                <asp:TextBox runat="server" ID="txtDOB" MaxLength="10" CssClass="TextField" placeholder="MM-DD-YYYY"/></label>
            <label>Phone <i style="color:red">*</i>
                <asp:TextBox runat="server" ID="txtPhone" CssClass="TextField" MaxLength="12" />
                <%--<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="The PhoneNumber field is not a valid phone number." ControlToValidate="txtPhone" ValidationExpression="^[0-9]{3}-[0-9]{3}-[0-9]{4}$" ></asp:RegularExpressionValidator>--%>
            </label>
           
              <label>SSN
                <asp:TextBox runat="server" ID="txtSSN" CssClass="TextField" MaxLength="11" /></label>
            <div class="buttons">
                <asp:Button runat="server" ID="btnSave" CssClass="Button" Text="Save" OnClick="btnSave_Click"/>
                <asp:Button runat="server" id="btnCancel" CssClass="Button" Text="Cancel" OnClick="btnCancel_Click"/>
            </div>
        </div>
     
        <div class="Error">
            <asp:Literal runat="server" ID="litError" />
        </div>
        <br />
           <div>
            <asp:GridView ID="GVpatients" runat="server" AutoGenerateColumns="false" DataKeyNames="PID"  OnRowCommand="GVpatients_RowCommand">
                <Columns>  
                    <asp:BoundField DataField="PatientName" HeaderText="PatientName" /> 
                    <asp:BoundField DataField="Gender" HeaderText="Gender" /> 
                    <asp:BoundField DataField="DateOfBirth" HeaderText="DateOfBirth" /> 
                    <asp:BoundField DataField="PhoneNumber" HeaderText="PhoneNumber" DataFormatString="{0:###-###-####}" />
                   <%-- <asp:TemplateField>
    <ItemTemplate>
        <asp:Literal ID="litPhone" runat="server" Text='<%# string.Format("{0:(###) ###-####}", Int64.Parse(Eval("MainPhoneNumber").ToString())) %>' />
    </ItemTemplate>
</asp:TemplateField>--%>
                       <asp:TemplateField>
            <ItemTemplate>
                <asp:Button Text="Select" runat="server" CommandName="Select" CommandArgument="<%# Container.DataItemIndex %>" />
            </ItemTemplate>
        </asp:TemplateField>
                        </Columns>
            </asp:GridView>
        </div>

    </form>
</body>
</html>
