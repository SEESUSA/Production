using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Timers;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

public partial class AddPatientWithCareCloud : System.Web.UI.Page
{
   static DataTable dt = new DataTable();

    protected void Page_Load(object sender, EventArgs e)
    {
        API.Session.PatientExistsError += Err;
        API.Session.CreateNewPatientError += Err;
        API.Session.TransferPatientReferringDoctorError += Err;
        API.Session.GetSignatureSourceMIDError += Err;//ValidationErr
        API.Session.ValidateNewPatientError += ValidationErr;

        litError.Text = "";
        if (!IsPostBack)
        {
            RefreshGender();
            GVpatients.DataSource = null;
            GVpatients.DataBind();
        }
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        API.Session.PtEmailAddress = null;
        if (ValidateData())
        {
            CreatePatient();
        }
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        txtFirstName.Text = string.Empty;
        txtLastName.Text = string.Empty;
        cboGender.SelectedIndex = 0;
        txtDOB.Text = string.Empty;
        txtPhone.Text = string.Empty;
        txtSSN.Text = string.Empty;
        Response.Redirect("~/ScheduleApptWithCareCloud.aspx");
    }
    protected void RefreshGender()
    {
        cboGender.Items.Clear();
        cboGender.Items.Add(new ListItem("Select Gender", "0"));
        cboGender.Items.Add(new ListItem("Male", "Male"));
        cboGender.Items.Add(new ListItem("Female", "Female"));
        cboGender.Items.Add(new ListItem("Unknown", "Unknown"));
    }

    protected void CreatePatient()
    {
        Regex regex = new Regex(@"[^\d]");
        string phonenum = regex.Replace(txtPhone.Text, "");
        string GetSSN = regex.Replace(txtSSN.Text, "");
        ////Post API 
        var responseString = "";
            WebRequest PostRequest = WebRequest.Create(Statics.URL_CreatePatient);
            PostRequest.Method = "POST";
            PostRequest.ContentType = "application/json";
            PostRequest.Headers.Add("Authorization", API.Session.AccessToken);
            string address = string.Empty;
        string StLocation = string.Empty;
        string location = API.Session.ChState.ToString();// "AL";
        if (API.Session.ChState.ToString() == "ETN")
        {
             StLocation = "TN";
        }
        else
        {
             StLocation = API.Session.ChState.ToString();
        }
       
            string gender = string.Empty;
        //API.Session.GUID
        string LoginEmail = API.Session.Email;
        string[] data = API.Session.GetNPIByEmail(API.Session.Email, API.Session.SWLocationID);
        string NPI1 = "";
        if (data != null)
        {
            NPI1 = data[0];
        }
        DateTime originaldate;
        DateTime.TryParse(txtDOB.Text, out originaldate);
        API.Session.SWPatient = (txtFirstName.Text.Trim().Length > 0 ? txtFirstName.Text.Trim()[0].ToString() + "***  " : " ") + (txtLastName.Text.Trim().Length > 0 ? txtLastName.Text.Trim()[0].ToString() + "***  " : " ");
        string postData = "{\"patient\":{\"first_name\":" + "\"" + txtFirstName.Text + "\",\"last_name\":" + "\"" + txtLastName.Text + "\",\"email\":" + "\"" + txtEmailAddresss.Text.ToString() + "\",\"gender_code\":" + "\"" + cboGender.SelectedValue.ToString() + "\",\"date_of_birth\":" + "\"" + originaldate.ToString("yyyy-MM-dd") + "\" "+ (txtSSN.Text=="" ? "" : ",\"ssn\":" + "\"" + GetSSN + "\"") + ",\"referring_physician_npi\":" + (data[0]!=""?Convert.ToInt32(data[0]).ToString() : "\""+"\"" )+ "},\"addresses\":[{\"line1\":\"abc\",\"line2\":\"\",\"city\":\"ABC\",\"state\":" + "\"" + StLocation + "\",\"zip_code\":\"00000\",\"country_name\":\"USA\",\"is_primary\":true}],\"phones\":[{\"phone_number\":" + "\"" + phonenum + "\",\"phone_type_code\":\"M\",\"is_primary\":true}]}";
        try
        {
            using (var streamWriter = new StreamWriter(PostRequest.GetRequestStream()))
            {
                streamWriter.Write(postData);
                streamWriter.Flush();
                streamWriter.Close();

                var httpWebResponse2 = (HttpWebResponse)PostRequest.GetResponse();
                using (Stream reader = httpWebResponse2.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(reader);
                    responseString = sr.ReadToEnd();
                    sr.Close();
                }
            }
            DataSet dataSet = new DataSet();
            XmlDocument xd = new XmlDocument();
            xd = (XmlDocument)JsonConvert.DeserializeXmlNode(responseString);

            API.Session.PatientId = xd.InnerText;
            var Result = API.Session.CreatePatient(xd.InnerText, txtFirstName.Text, txtLastName.Text, cboGender.SelectedValue.ToString(), Convert.ToDateTime(txtDOB.Text), Convert.ToString(NPI1), "abc", API.Session.CityName.ToString(), location, "00000", "USA", phonenum, txtSSN.Text, txtEmailAddresss.Text);
            //Update Appointment Create Request And Response
            API.Session.UpdateAppointmentReqResponse(postData, responseString, "Patient", "","");
            if (Result == "true" || Result == "")
            {
                API.Session.PatientPhone = phonenum;
                API.Session.PtEmailAddress = txtEmailAddresss.Text;
                Server.Transfer("~/ScheduleApptWithCareCloud.aspx", false);

            }
        }
        catch(Exception ex)
        {
            //Update Appointment Create Request And Response
            API.Session.UpdateAppointmentReqResponse(postData, ex.Message, "Patient", "","");
        }
    }

    protected void Err(int number, string msg)
    {
        litError.Text += "Error: "+ msg + "<br /><br />";//" + number.ToString() + " - " 
    }

    protected void ValidationErr(string msg)
    {
        litError.Text += msg + "<br />";
    }

    protected bool ValidateData()
    {

        if (txtFirstName.Text == "") { Err(1112, "You must provide the First Name."); return false; }
        if (txtLastName.Text == "") { Err(1113, "You must provide the Last Name."); return false; }
        if (cboGender.SelectedIndex == 0) { Err(1114, "You must select the Gender first."); return false; }
        if (txtDOB.Text == "") { Err(1115, "You must provide the Date of Birth."); return false; }
        if (txtPhone.Text == "") { Err(1116, "You must provide the Phone number."); return false; }
        Regex regex = new Regex(@"[^\d]");
        string phonenum = regex.Replace(txtPhone.Text, "");
        string location = API.Session.ChState.ToString();
        if (Convert.ToInt32(phonenum.Length) < 10 || Convert.ToInt32(phonenum.Length) > 10) { Err(1116, "You must provide the valid Phone number."); return false; }

        if (txtEmailAddresss.Text != "" && !IsValidEmail(txtEmailAddresss.Text)) { Err(1116, "You must provide the valid Email Address."); return false; }
        if (txtSSN.Text != "" && txtSSN.Text.Length<11) { Err(1116, "You must provide the valid SSN."); return false; }
        var Result = API.Session.ValidatePatient(txtFirstName.Text, txtLastName.Text, cboGender.SelectedValue.ToString(), Convert.ToDateTime(txtDOB.Text), phonenum, txtSSN.Text, API.Session.CityName.ToString(), location);
        if (Result == "false" || Result=="")
        {

            API.Session.SWPatient = (txtFirstName.Text.Trim().Length>0 ? txtFirstName.Text.Trim()[0].ToString()+ "***  " : " ")+ (txtLastName.Text.Trim().Length > 0 ? txtLastName.Text.Trim()[0].ToString() + "***  " : " ");
            DataTable dt1 = new DataTable();
            dt = null;
            dt = API.Session.GetPatient(txtFirstName.Text, txtLastName.Text, cboGender.SelectedValue.ToString(), Convert.ToDateTime(txtDOB.Text), phonenum, txtSSN.Text, API.Session.CityName.ToString(), location);
            GVpatients.DataSource = dt;
            GVpatients.DataBind();
            return false;
        }
        else
        {
            return true;
        }

    }
    static bool IsValidEmail(string emailAddress)
    {
        // Regular expression for validating email addresses
        string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        // Check if the email address matches the regex pattern
        return Regex.IsMatch(emailAddress, emailRegex);
    }

    protected void GVpatients_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Select")
        {

            API.Session.PtEmailAddress = null;
            API.Session.SWPatient=null;
            //Determine the RowIndex of the Row whose Button was clicked.
            int rowIndex = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = GVpatients.Rows[rowIndex]; 
            //Get the value of column from the DataKeys using the RowIndex.
            string PatientId= Convert.ToString(GVpatients.DataKeys[rowIndex].Values[0]);
            API.Session.patientId = Convert.ToString(GVpatients.DataKeys[rowIndex].Values[0]);
            API.Session.PatientPhone = Convert.ToString(row.Cells[3].Text);
            if (dt != null)
            {
                foreach(DataRow dr in dt.Rows)
                {
                    if (PatientId == dr["PID"].ToString())
                    {
                        API.Session.PtEmailAddress = Convert.ToString(dr["EmailAddress"].ToString());
                        API.Session.SWPatient = (dr["FirstName"].ToString().Trim().Length > 0 ? dr["FirstName"].ToString().Trim()[0].ToString() + "***  " : " ") + (dr["LastName"].ToString().Trim().Length > 0 ? dr["LastName"].ToString().Trim()[0].ToString() + "***  " : " ");
                        break;
                    }
                }
            }
            Response.Redirect("~/ScheduleApptWithCareCloud.aspx");
        }
        
    }
    
}