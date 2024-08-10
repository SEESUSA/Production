﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

public partial class ScheduleApptWithCareCloud : System.Web.UI.Page
{
    static  DataSet dsLocation = new DataSet();
    static DataSet dsApptType =new DataSet();
    static DataSet dsApptSlots = new DataSet();
    static int apptTemplateId = 0;
    static string resouceId = "";
    static string patientCity = string.Empty;
  
    protected void Page_Load(object sender, EventArgs e)
    {
        //Response.Cache.SetCacheability(HttpCacheability.NoCache);
        API.Session.GetLocationsError += Err;
        API.Session.GetEHPFacilityListError += Err;
        API.Session.GetEHPDoctorListError += Err;
        API.Session.GetEHPApptTypeListError += Err;
        API.Session.GetCPSScheduleIDsError += Err;
        API.Session.GetOpenApptSlotsError += Err;
        API.Session.GetRefDrPatientsError += Err;
        API.Session.GetApptStartError += Err;
        API.Session.GetApptStopError += Err;
        API.Session.GetCompanyIDError += Err;
        API.Session.BookAppointmentError += Err;
        API.Session.GetFacilityContactListError += Err;
        API.Session.SendEmailError += Err;
        API.Session.GetFacilityContactDetailsError += Err;
        API.Session.UpdatePatientPhoneNumberError += Err;
        API.Session.GetPatientPhoneNumberError += Err;

        litError.Text = "";
        Label1.Text = "";
        //Added by Uma D.
        string url = HttpContext.Current.Request.Url.AbsoluteUri;

      
        if (!IsPostBack)
        {
            
            RefreshLocations();
           
            if (API.Session.ReferralDoctorValue > 0) SetLocation(API.Session.ReferralDoctorValue);
            if (API.Session.FacilityValue > 0) SetFacility(API.Session.FacilityValue);
            if (API.Session.DoctorValue > 0) SetDoctor(API.Session.DoctorValue);
            if (API.Session.AppointmentTypeValue > 0) SetApptType(API.Session.AppointmentTypeValue);
            if (API.Session.SloatValue > 0) SetApptSlot(API.Session.SloatValue);
            SetEye(API.Session.SWEye);
            
            if (url.Contains("?"))
            {
                var result = url.Substring(url.LastIndexOf('=') + 1);
                API.Session.AutherisationCode = result;
                if (API.Session.AccessToken == "")
                    GetAccessTokenByAutherisationCode();
            }
        }
        else
        {
            if (txtPhone.Text == "" && hdnpId.Value != "")
            {
                List<PatientModel> list = new List<PatientModel>();
                DataTable dtPatient = new DataTable();
                dtPatient = API.Session.GetPatientList();

                if (dtPatient.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtPatient.Rows)
                    {
                        list.Add(new PatientModel { PatientName = dr["PatientName"].ToString(), PID = dr["PID"].ToString(), PhoneNumber = dr["PhoneNumber"].ToString() });
                    }
                }

                string phone = list.FirstOrDefault(x => x.PID == hdnpId.Value).PhoneNumber;
                string phonenumber = String.Empty;
                if (phone != "")
                {
                    string case1 = phone.Substring(0, 3);
                    string case2 = phone.Substring(3, 3);
                    string case3 = phone.Substring(6);
                     phonenumber = string.Format("{0}-{1}-{2}", case1, case2, case3);
                }
                else
                {
                    phonenumber = phone;
                }
                txtPhone.Text = phonenumber;
                Label2.Text = "Please verify the contact number. If different, please provide correct contact number in " + "\"Reason for Visit\"" + " section.";

            }
        }
    }
    protected void ClearSteps(string startingpoint)
    {
        switch (startingpoint)
        {
            case "Location":
                cboEHPFacility.Items.Clear();
                goto case "Facility";
            case "Facility":
                cboEHPDoctor.Items.Clear();
                goto case "Doctor";
            case "Doctor":
                cboApptType.Items.Clear();
                goto case "ApptType";
            case "ApptType":
                cboApptSlot.Items.Clear();
                goto case "ApptSlot";
            case "ApptSlot":
              //  cboPatient.Items.Clear();
                goto case "Patient";
            case "Patient":
                rbBoth.Checked = true;
                rbEyeLeft.Checked = false;
                rbEyeRight.Checked = false;
                txtPhone.Text = "";
                break;
            default:
                break;
        }
    }
    protected void RefreshLocations()
    {

        ClearSteps("Locations");
        cboLocation.Items.Clear();
        cboLocation.Items.Add(new ListItem("Select Your Location", "0"));
        

        API.Location[] data1 = API.Session.GetLocationsforTN(API.Session.Email);
        if (data1 != null)
        {
            for (int a = 0; a < data1.Length; a++)
            {
                string state = "";
                if(API.Session.IsMiddleTN==true && API.Session.IsAL == true && API.Session.IsEastTN==true)
                {
                    if (data1[a].Location1 == "TN" && API.Session.IsState=="TN")
                    {
                        state = "TN";

                        cboLocation.Items.Add(new ListItem(data1[a].Display, data1[a].State + "^" + data1[a].Address1.ToString()));
                    }
                    else if (data1[a].Location1 == "AL" && API.Session.IsState == "AL")
                    {
                        state = "AL";

                        cboLocation.Items.Add(new ListItem(data1[a].Display, data1[a].State + "^" + data1[a].Address1.ToString()));
                    }
                    else if (data1[a].Location1 == "ETN" && API.Session.IsState == "ETN")
                    {
                        state = "ETN";

                        cboLocation.Items.Add(new ListItem(data1[a].Display, data1[a].Location1 + "^" + data1[a].Address1.ToString()));
                    }
                }
                else if (API.Session.IsMiddleTN == true && API.Session.IsAL == true && API.Session.IsEastTN != true)
                {
                    if (data1[a].Location1 == "TN" && API.Session.IsState == "TN")
                    {
                        state = "TN";

                        cboLocation.Items.Add(new ListItem(data1[a].Display, data1[a].State + "^" + data1[a].Address1.ToString()));
                    }
                    else if (data1[a].Location1 == "AL" && API.Session.IsState == "AL")
                    {
                        state = "AL";

                        cboLocation.Items.Add(new ListItem(data1[a].Display, data1[a].State + "^" + data1[a].Address1.ToString()));
                    }
                    
                }
                else if (API.Session.IsMiddleTN == true && API.Session.IsAL != true && API.Session.IsEastTN != true)
                {
                    if (data1[a].Location1 == "TN" && API.Session.IsState == "TN")
                    {
                        state = "TN";

                        cboLocation.Items.Add(new ListItem(data1[a].Display, data1[a].State + "^" + data1[a].Address1.ToString()));
                    }
                    
                }
                else if (API.Session.IsMiddleTN != true && API.Session.IsAL == true && API.Session.IsEastTN != true)
                {
                    if (data1[a].Location1 == "AL" && API.Session.IsState == "AL")
                    {
                        state = "AL";

                        cboLocation.Items.Add(new ListItem(data1[a].Display, data1[a].State + "^" + data1[a].Address1.ToString()));
                    }
                     else if (data1[a].Location1 == "ETN" && API.Session.IsState == "ETN")
                    {
                        state = "ETN";

                        cboLocation.Items.Add(new ListItem(data1[a].Display, data1[a].Location1 + "^" + data1[a].Address1.ToString()));
                    }
                }
                else if (API.Session.IsMiddleTN != true && API.Session.IsAL == true && API.Session.IsEastTN == true)
                {
                    if (data1[a].Location1 == "ETN" && API.Session.IsState == "ETN")
                    {
                        state = "ETN";

                        cboLocation.Items.Add(new ListItem(data1[a].Display, data1[a].Location1 + "^" + data1[a].Address1.ToString()));
                    }
                }

            }
        }
        lbApptSlotMore.Visible = false;
        lbNewPatient.Visible = false;
    }

    protected void RefreshLocations1()
    {

        ClearSteps("Locations");
        cboLocation.Items.Clear();
        cboLocation.Items.Add(new ListItem("Select Your Location", "0"));
        RefreshEHPLocation_API("");

        lbApptSlotMore.Visible = false;
        lbNewPatient.Visible = false;
    }

    protected void RefreshEHPLocation_API(string OrgName)
    {
        var responseString = "";
        WebRequest request = WebRequest.Create(Statics.URL_GETRefLocation+"name="+ OrgName);
        DataSet ds = new DataSet();
        request.Method = "GET";
        request.ContentType = "application/json";
        request.Headers.Add("Authorization", API.Session.AccessToken);
        HttpWebResponse httpWebResponse = null;
        httpWebResponse = (HttpWebResponse)request.GetResponse();

        using (Stream reader = httpWebResponse.GetResponseStream())
        {
            StreamReader sr = new StreamReader(reader);
            responseString = sr.ReadToEnd();
            sr.Close();
        }

        try
        {
            XmlDocument xd = new XmlDocument();
            responseString = "{ \"rootNode\": {\"root\":" + responseString.Trim() + "} }";
            xd = (XmlDocument)JsonConvert.DeserializeXmlNode(responseString);

            ds.ReadXml(new XmlNodeReader(xd));
            if (ds != null)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    foreach(DataRow dataRow in ds.Tables[1].Rows)
                    {
                        if(dr["root_Id"].ToString()== dataRow["root_Id"].ToString())
                        {
                            cboLocation.Items.Add(new ListItem((dr["location_name"].ToString()+' '+ dataRow["line1"].ToString()+' '+ dataRow["city"].ToString()+' '+ dataRow["state"].ToString() + ' ' + dataRow["zip"].ToString()), dataRow["state"].ToString() + "^" + dr["location_id"].ToString()));
                            break;
                        }
                    }
                }
            }
            // return ds; 
        }
        catch (Exception ex)
        { throw new ArgumentException(ex.Message); }


    }
    //protected void RefreshEHPFacilities_API()
    //{
    //    string[] Locarray = cboLocation.SelectedItem.Value.Split('^');
    //    API.Session.ChState = Locarray[0].ToString();
    //    if (Locarray[0].ToString() == "ETN")
    //    {
    //        Label3.Text = "Appointment Slot (Eastern Time)";
    //    }
    //    else
    //    {
    //        Label3.Text = "Appointment Slot (Central Time)";
    //    }
    //    //if (Locarray[0] == "AL")
    //    //{
    //    //    Response.Redirect("~/ScheduleAppt.aspx");
    //    //}
    //    //else { 

    //    #region Get Facility

    //    var responseString = "";
    //    WebRequest request = WebRequest.Create(Statics.URL_GETFAcility);
    //    DataSet ds = new DataSet();
    //    request.Method = "GET";
    //    request.ContentType = "application/json";
    //    request.Headers.Add("Authorization", API.Session.AccessToken);
    //    HttpWebResponse httpWebResponse = null;
    //    httpWebResponse = (HttpWebResponse)request.GetResponse();

    //    using (Stream reader = httpWebResponse.GetResponseStream())
    //    {
    //        StreamReader sr = new StreamReader(reader);
    //        responseString = sr.ReadToEnd();
    //        sr.Close();
    //    }

    //    try
    //    {
    //        XmlDocument xd = new XmlDocument();
    //        responseString = "{ \"rootNode\": {" + responseString.Trim().TrimStart('{').TrimEnd('}') + "} }";
    //        xd = (XmlDocument)JsonConvert.DeserializeXmlNode(responseString);

    //        ds.ReadXml(new XmlNodeReader(xd));
    //        dsLocation = ds;
    //        //  return ds; 
    //    }
    //    catch (Exception ex)
    //    { throw new ArgumentException(ex.Message); }



    //    #endregion



    //    ClearSteps("Facility");
    //    cboEHPFacility.Items.Clear();
    //    cboEHPFacility.Items.Add(new ListItem("Select Facility", "0"));

    //    if (dsLocation != null)
    //    {
    //            DataTable dt = dsLocation.Tables["locations"];
    //            DataView dv = dt.DefaultView;
    //            dv.Sort = "name";
    //            dt = dv.ToTable();
    //            if (dt.Rows.Count > 0)
    //        {
    //            foreach (DataRow dr in dt.Rows)
    //            {//Remaining to check enable.need to check when get details.
    //             // if (ds.Tables["address"].Rows[0]["state_name"].ToString() != API.Session.SWState) continue;
    //                    if (dr["id"].ToString()== "46679" )// || dr["id"].ToString() == "39341") //|| dr["id"].ToString() == "44736"
    //                {


    //                    }
    //                else if (dr["name"].ToString().Trim() == "OFFICE ONEONTA" || dr["name"].ToString().Trim() == "OFFICE HAMILTON DR SHOTTS" || dr["name"].ToString().Trim() == "OFFICE HAMILTON DR COBB" || dr["name"].ToString().Trim() == "OFFICE GREENVILLE" || dr["name"].ToString().Trim() == "OFFICE GARDENDALE" || dr["name"].ToString().Trim() == "OFFICE CENTREVILLE")
    //                {

    //                }
    //                else
    //                {
    //                    cboEHPFacility.Items.Add(new ListItem(dr["name"].ToString(), dr["id"].ToString()));
    //                }
    //            }
    //        }
    //    }
    // //   }
    //    lbApptSlotMore.Visible = false;
    //    lbNewPatient.Visible = false;
    //}

    protected void RefreshEHPFacilities_API()
    {
        int entityid = 0;
        string[] Locarray = cboLocation.SelectedItem.Value.Split('^');
        API.Session.SWLocationID = Convert.ToInt32(Locarray[1]);
        API.Session.ChState = Locarray[0].ToString();
        if (Locarray[0].ToString() == "ETN")
        {
            Label3.Text = "Appointment Slot (Eastern Time)";
        }
        else
        {
            Label3.Text = "Appointment Slot (Central Time)";
        }
        #region Get Facility

        var responseString = "";
        WebRequest request = WebRequest.Create(Statics.URL_GETFAcility);
        DataSet ds = new DataSet();
        request.Method = "GET";
        request.ContentType = "application/json";
        request.Headers.Add("Authorization", API.Session.AccessToken);
        HttpWebResponse httpWebResponse = null;
        httpWebResponse = (HttpWebResponse)request.GetResponse();

        using (Stream reader = httpWebResponse.GetResponseStream())
        {
            StreamReader sr = new StreamReader(reader);
            responseString = sr.ReadToEnd();
            sr.Close();
        }

        try
        {
            XmlDocument xd = new XmlDocument();
            responseString = "{ \"rootNode\": {" + responseString.Trim().TrimStart('{').TrimEnd('}') + "} }";
            xd = (XmlDocument)JsonConvert.DeserializeXmlNode(responseString);

            ds.ReadXml(new XmlNodeReader(xd));
            dsLocation = ds;
            //  return ds; 
        }
        catch (Exception ex)
        { throw new ArgumentException(ex.Message); }



        #endregion
        ClearSteps("Facility");
        cboEHPFacility.Items.Clear();
        cboEHPFacility.Items.Add(new ListItem("Select Facility", "0"));

            DataTable dt = new DataTable();

            if (Locarray[0].ToString() == "AL")
            {
                API.Session.EntityID = 1;
            }
            else if (Locarray[0].ToString() == "ETN")
            {
                API.Session.EntityID = 2;
            }
            else if (Locarray[0].ToString() == "TN")
            {
                API.Session.EntityID = 3;
            }
            dt = API.Session.GetFacilities(API.Session.EntityID);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                        cboEHPFacility.Items.Add(new ListItem(dr["FacilityName"].ToString(), dr["CCFacilityID"].ToString()));
                }
            }
        lbApptSlotMore.Visible = false;
        lbNewPatient.Visible = false;
    }

    protected void RefreshApptTypes_API()
    {
        var responseString1 = "";
        DataSet ds = new DataSet();
        string templateId = "";
        string url = cboEHPDoctor.SelectedItem.Value;
    https://api.carecloud.com/v2/appointment_resources/32012/visit_reasons?request_types_only=false

        WebRequest request1 = WebRequest.Create(Statics.URL_GETApptType + url + "/visit_reasons?request_types_only=false");
        request1.Method = "GET";
        request1.ContentType = "application/json";
        request1.Headers.Add("Authorization", API.Session.AccessToken);

        HttpWebResponse httpWebResponse1 = null;
        httpWebResponse1 = (HttpWebResponse)request1.GetResponse();

        using (Stream reader = httpWebResponse1.GetResponseStream())
        {
            StreamReader sr = new StreamReader(reader);
            responseString1 = sr.ReadToEnd();
            sr.Close();
        }

        try
        {
            if (!(responseString1.Trim().StartsWith("{")) && (!responseString1.Trim().EndsWith("}")))
            {
                responseString1 = "{" + responseString1 + "}";
            }
            XmlDocument xd = new XmlDocument();
            responseString1 = "{ \"rootNode\": {\"root\":" + responseString1.Trim().TrimStart('{').TrimEnd('}') + "} }";
            xd = (XmlDocument)JsonConvert.DeserializeXmlNode(responseString1);

            dsApptType.Clear();
            dsApptType.ReadXml(new XmlNodeReader(xd));
        }
        catch (Exception ex)
        { throw new ArgumentException(ex.Message); }

        ClearSteps("ApptType");
                cboApptType.Items.Clear();
                cboApptType.Items.Add(new ListItem("Select Appointment Type", "0"));

                DataTable dt1 = new DataTable();
                dt1 = API.Session.GetAppointmentType(cboEHPDoctor.SelectedItem.Value.ToString(),cboEHPFacility.SelectedItem.Value.ToString(),API.Session.EntityID);

                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow dr1 in dt1.Rows)
                    {
                        if (dr1["CCApptTypeID"].ToString() != "")
                        {
                            cboApptType.Items.Add(new ListItem(dr1["DisplayName"].ToString(), dr1["CCApptTypeID"].ToString()));
                        }
                    }
                }

        lbApptSlotMore.Visible = false;
        lbNewPatient.Visible = false;
    }

    //protected void RefreshApptTypes_API()
    //{
    //    var responseString1 = "";
    //    // DataSet ds = new DataSet();
    //    string templateId = "";
    //    string url = cboEHPDoctor.SelectedItem.Value;
    //    //https://api.carecloud.com/v2/appointment_resources/32012/visit_reasons?request_types_only=false

    //    WebRequest request1 = WebRequest.Create(Statics.URL_GETApptType + url + "/visit_reasons?request_types_only=false");
    //    request1.Method = "GET";
    //    request1.ContentType = "application/json";
    //    request1.Headers.Add("Authorization", API.Session.AccessToken);

    //    HttpWebResponse httpWebResponse1 = null;
    //    httpWebResponse1 = (HttpWebResponse)request1.GetResponse();

    //    using (Stream reader = httpWebResponse1.GetResponseStream())
    //    {
    //        StreamReader sr = new StreamReader(reader);
    //        responseString1 = sr.ReadToEnd();
    //        sr.Close();
    //    }

    //    try
    //    {
    //        if (!(responseString1.Trim().StartsWith("{")) && (!responseString1.Trim().EndsWith("}")))
    //        {
    //            responseString1 = "{" + responseString1 + "}";
    //        }
    //        XmlDocument xd = new XmlDocument();
    //        //responseString1 = "{\"resource\":" + doctorResponse.Trim() + "}";
    //        //responseString1 = "{ \"rootNode\": {" + doctorResponse.Trim().TrimStart('{').TrimEnd('}') + "} }";

    //        responseString1 = "{ \"rootNode\": {\"root\":" + responseString1.Trim().TrimStart('{').TrimEnd('}') + "} }";
    //        xd = (XmlDocument)JsonConvert.DeserializeXmlNode(responseString1);


    //        dsApptType.Clear();
    //        dsApptType.ReadXml(new XmlNodeReader(xd));
    //        //  return ds; 
    //    }
    //    catch (Exception ex)
    //    { throw new ArgumentException(ex.Message); }

    //    ClearSteps("ApptType");
    //    cboApptType.Items.Clear();
    //    cboApptType.Items.Add(new ListItem("Select Appointment Type", "0"));


    //    if (dsApptType != null)
    //    {
    //        //if (ds.Tables["resources"].Rows.Count > 0)
    //        //{
    //        //    foreach (DataRow dr in ds.Tables["resources"].Rows)
    //        //    {
    //        //        if (dr["id"].ToString() == cboEHPDoctor.SelectedValue)
    //        //        {

    //        //        }


    //        //        //cboApptType.Items.Add(new ListItem(dr["name"].ToString(), dr["id"].ToString()));
    //        //    }
    //        //}
    //        DataTable dt = dsApptType.Tables["root"];
    //        DataView dv = dt.DefaultView;
    //        dv.Sort = "name";
    //        dt = dv.ToTable();
    //        if (dt.Rows.Count > 0)
    //        {
    //            foreach (DataRow dr in dt.Rows)
    //            {
    //                //API.Session.CreateAppointmentType(dr["name"].ToString(), dr["id"].ToString());
    //                 cboApptType.Items.Add(new ListItem(dr["name"].ToString(), dr["id"].ToString()));

    //            }

    //            //DataTable dt1 = new DataTable();
    //            //dt1 = API.Session.GetAppointmentType();
    //            //foreach (DataRow dr in dt.Rows)
    //            //{
    //            //    if (dt1.Rows.Count > 0)
    //            //    {

    //            //        foreach (DataRow dr1 in dt1.Rows)
    //            //        {
    //            //            if (dr["name"].ToString() == dr1["ApptType"].ToString())
    //            //            {
    //            //                if (dr1["DisplayName"].ToString() != "")
    //            //                {
    //            //                    cboApptType.Items.Add(new ListItem(dr1["DisplayName"].ToString(), dr1["ApptTypeId"].ToString()));
    //            //                }
    //            //                else
    //            //                {
    //            //                    cboApptType.Items.Add(new ListItem(dr1["ApptType"].ToString(), dr1["ApptTypeId"].ToString()));
    //            //                }

    //            //            }

    //            //        }

    //            //    }

    //            //}

    //        }
    //    }

    //    lbApptSlotMore.Visible = false;
    //    lbNewPatient.Visible = false;
    //}

    protected string GetProvider()
    {
        var responseString1 = "";
         DataSet ds = new DataSet();
        string templateId = "";
        string ProviderId = "";
        string url = cboEHPDoctor.SelectedItem.Value;
        //https://api.carecloud.com/v2/appointment_resources/32012

        WebRequest request1 = WebRequest.Create(Statics.URL_GETDoctors +"/"+ url );
        request1.Method = "GET";
        request1.ContentType = "application/json";
        request1.Headers.Add("Authorization", API.Session.AccessToken);

        HttpWebResponse httpWebResponse1 = null;
        httpWebResponse1 = (HttpWebResponse)request1.GetResponse();

        using (Stream reader = httpWebResponse1.GetResponseStream())
        {
            StreamReader sr = new StreamReader(reader);
            responseString1 = sr.ReadToEnd();
            sr.Close();
        }

        try
        {
            
            XmlDocument xd = new XmlDocument();
            responseString1 = "{\"resource\":" + responseString1.Trim() + "}";
            responseString1 = "{ \"rootNode\": {" + responseString1.Trim().TrimStart('{').TrimEnd('}') + "} }";
            xd = (XmlDocument)JsonConvert.DeserializeXmlNode(responseString1);


            ds.Clear();
            ds.ReadXml(new XmlNodeReader(xd));
        }
        catch (Exception ex)
        { throw new ArgumentException(ex.Message); }

        if (ds != null)
        {
           
            if (ds.Tables["default_provider"].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables["default_provider"].Rows)
                {
                    ProviderId = dr["id"].ToString();
                }
            }
        }
        return ProviderId;


    }


    protected void refreshDoctor_API()
    {

        ClearSteps("Doctor");
        cboEHPDoctor.Items.Clear();
        cboEHPDoctor.Items.Add(new ListItem("Select Doctor", "0"));

        DataTable dt1 = new DataTable();
        dt1 = API.Session.GetDoctors(cboEHPFacility.SelectedItem.Value.ToString(),API.Session.EntityID);

        if (dt1.Rows.Count > 0)
        {

            foreach (DataRow dr1 in dt1.Rows)
            {

                if (dr1["CCDoctorId"].ToString() != "")
                {
                    cboEHPDoctor.Items.Add(new ListItem(dr1["DisplayName"].ToString(), dr1["CCDoctorId"].ToString()));
                }


            }

        }

        lbApptSlotMore.Visible = false;
        lbNewPatient.Visible = false;

    }
    //protected void refreshDoctor_API()
    //{

    //    var doctorResponse = "";
    //    DataSet ds = new DataSet();
    //    WebRequest doctorRequest = WebRequest.Create(Statics.URL_GETDoctors);
    //    doctorRequest.Method = "GET";
    //    doctorRequest.ContentType = "application/json";
    //    doctorRequest.Headers.Add("Authorization", API.Session.AccessToken);
    //    HttpWebResponse doctorHttpWebResponse = null;
    //    doctorHttpWebResponse = (HttpWebResponse)doctorRequest.GetResponse();

    //    using (Stream reader = doctorHttpWebResponse.GetResponseStream())
    //    {
    //        StreamReader sr = new StreamReader(reader);
    //        doctorResponse = sr.ReadToEnd();
    //        sr.Close();
    //    }


    //    try
    //    {
    //        XmlDocument xd = new XmlDocument();
    //        //doctorResponse = "{ \"rootNode\": {" + doctorResponse.Trim().TrimStart('{').TrimEnd('}') + "} }";
    //        //doctorResponse = doctorResponse.Trim().TrimStart('[').TrimEnd(']');
    //        doctorResponse = "{\"resource\":" + doctorResponse.Trim() + "}";
    //        doctorResponse = "{ \"rootNode\": {" + doctorResponse.Trim().TrimStart('{').TrimEnd('}') + "} }";
    //        xd = (XmlDocument)JsonConvert.DeserializeXmlNode(doctorResponse);

    //        ds.ReadXml(new XmlNodeReader(xd));
    //        //  return ds; 
    //    }
    //    catch (Exception ex)
    //    { throw new ArgumentException(ex.Message); }
    //    ClearSteps("Doctor");
    //    cboEHPDoctor.Items.Clear();
    //    cboEHPDoctor.Items.Add(new ListItem("Select Doctor", "0"));

    //    if (ds != null)
    //    {
    //        DataTable dt = ds.Tables["resource"];
    //        DataView dv = dt.DefaultView;
    //        dv.Sort = "name";
    //        dt = dv.ToTable();
    //        if (dt.Rows.Count > 0) //if (ds.Tables["providers"].Rows.Count > 0)
    //        {
    //            foreach (DataRow dr in dt.Rows)
    //            {//Remaining to check enable.need to check when get details.
    //             // if (ds.Tables["address"].Rows[0]["state_name"].ToString() != API.Session.SWState) continue;
    //                if (dr["name"].ToString() != "")
    //                {
    //                    if (dr["name"].ToString().Contains("testforinfo"))
    //                    {

    //                    }
    //                    else if (dr["name"].ToString().Contains("LUDWIG"))
    //                    {

    //                    }
    //                    else
    //                    {
    //                        cboEHPDoctor.Items.Add(new ListItem(dr["name"].ToString(), dr["id"].ToString()));
    //                    }

    //                }

    //            }
    //        }
    //    }

    //    lbApptSlotMore.Visible = false;
    //    lbNewPatient.Visible = false;

    //}

    //protected void refreshPatient_API()
    //{
    //    int locationId = 0;
    //    DataTable dtPatient = new DataTable();
    //    ClearSteps("Patient");
    //    cboPatient.Items.Clear();
    //    cboPatient.Items.Add(new ListItem("Select Patient", "0"));
    //    //cboPatient.Items.Add(new ListItem("Uma Dixit", "3d29e42e-97d4-40a4-a36a-a76111bdca9d"));
    //    //if(API.Session.PatientId!="")
    //    //ShowPatient(API.Session.PatientId);
    //    //else
    //    //{
    //    if (dsLocation != null && dsLocation.Tables["locations"].Rows.Count > 0)
    //    {
    //        foreach (DataRow dr in dsLocation.Tables["locations"].Rows)
    //        {
    //            if (dr["id"].ToString() == cboEHPFacility.SelectedItem.Value.ToString())
    //            {
    //                locationId = int.Parse(dr["locations_id"].ToString());

    //                foreach (DataRow dr1 in dsLocation.Tables["address"].Rows)
    //                {
    //                    if (dr1["locations_id"].ToString() == locationId.ToString())
    //                    {
    //                        patientCity = dr1["city"].ToString();
    //                        API.Session.CityName = dr1["city"].ToString();
    //                        //patientCity = "Tullahoma";//Added only for testing purpose // to lowercser only
    //                        dtPatient = API.Session.GetLocationwisePatientList(patientCity);
    //                    }
    //                }
    //            }
    //        }
    //        if (dtPatient.Rows.Count > 0)
    //        {
    //            foreach (DataRow dr in dtPatient.Rows)
    //            {
    //                cboPatient.Items.Add(new ListItem(dr["PatientName"].ToString(), dr["PID"].ToString()));
    //            }
    //        }

    //    }
    //    // }
    //    lbApptSlotMore.Visible = true;
    //    lbNewPatient.Visible = true;
    //}

    //public void ShowPatient(string patientId)
    //{
    //    var Response = "";
    //    DataSet ds = new DataSet();
    //    WebRequest Request = WebRequest.Create(Statics.URL_ShowPatient + patientId);
    //    Request.Method = "GET";
    //    Request.ContentType = "application/json";
    //    Request.Headers.Add("Authorization", API.Session.AccessToken);
    //    HttpWebResponse HttpWebResponse = null;
    //    HttpWebResponse = (HttpWebResponse)Request.GetResponse();

    //    using (Stream reader = HttpWebResponse.GetResponseStream())
    //    {
    //        StreamReader sr = new StreamReader(reader);
    //        Response = sr.ReadToEnd();
    //        sr.Close();
    //    }


    //    try
    //    {
    //        XmlDocument xd = new XmlDocument();
    //        Response = "{ \"rootNode\": {" + Response.Trim().TrimStart('{').TrimEnd('}') + "} }";
    //        xd = (XmlDocument)JsonConvert.DeserializeXmlNode(Response);

    //        ds.ReadXml(new XmlNodeReader(xd));
    //        //  return ds; 
    //    }
    //    catch (Exception ex)
    //    { throw new ArgumentException(ex.Message); }
    //    ClearSteps("Patient");
    //    cboPatient.Items.Clear();
    //    cboPatient.Items.Add(new ListItem("Select Patient", "0"));

    //    if (ds != null)
    //    {
    //        if (ds.Tables["patient"].Rows.Count > 0)
    //        {
    //            foreach (DataRow dr in ds.Tables["patient"].Rows)
    //            {
    //                cboPatient.Items.Add(new ListItem(dr["name"].ToString(), dr["id"].ToString()));

    //                foreach (DataRow dr1 in ds.Tables["phones"].Rows)
    //                {
    //                    API.Session.PatientPhone = dr1["phone_number"].ToString();
    //                    break;
    //                }

    //            }
    //        }
    //    }


    //}



    protected void refreshApptSlots_API()
    {

        ClearSteps("ApptSlot");
        cboApptSlot.Items.Clear();
        cboApptSlot.Items.Add(new ListItem("Select Appointment Slot", "0"));

        var ApptSlotsResponse = "";
        DataSet ds = new DataSet();
       CultureInfo provider = new CultureInfo("en-US");
       DateTime date = DateTime.ParseExact(DateTime.Now.ToString(), "yyyy-MM-dd", provider,
        DateTimeStyles.AdjustToUniversal);
        string url = "start_date="+ DateTime.Now.AddDays(5).ToString("yyyy-MM-dd") +"&visit_reason_id=" + cboApptType.SelectedItem.Value + "&location_ids=" + cboEHPFacility.SelectedItem.Value + "&resource_ids=" + cboEHPDoctor.SelectedItem.Value+ "&end_date="+ DateTime.Now.AddDays(95).ToString("yyyy-MM-dd");

        WebRequest ApptSlotsRequest = WebRequest.Create(Statics.URL_GETApptSlots+url);
        ApptSlotsRequest.Method = "GET";
        ApptSlotsRequest.ContentType = "application/json";
        ApptSlotsRequest.Headers.Add("Authorization", API.Session.AccessToken);
        HttpWebResponse ApptSlotsHttpWebResponse = null;
        ApptSlotsHttpWebResponse = (HttpWebResponse)ApptSlotsRequest.GetResponse();

        using (Stream reader = ApptSlotsHttpWebResponse.GetResponseStream())
        {
            StreamReader sr = new StreamReader(reader);
            ApptSlotsResponse = sr.ReadToEnd();
            sr.Close();
        }


        try
        {
            XmlDocument xd = new XmlDocument();
            ApptSlotsResponse = "{ \"rootNode\": {\"root\":" + ApptSlotsResponse.Trim().TrimStart('{').TrimEnd('}') + "} }";
            var settings = new JsonSerializerSettings
            {
                Converters = { new Newtonsoft.Json.Converters.XmlNodeConverter() },
                DateParseHandling = DateParseHandling.None,
            };
            var doc = JsonConvert.DeserializeObject<XmlDocument>(ApptSlotsResponse, settings);
             ds.ReadXml(new XmlNodeReader(doc));
            dsApptSlots = ds;
            API.Session.Data = ds;  
        }
        catch (Exception ex)
        { throw new ArgumentException(ex.Message); }

        if (ds.Tables.Count != 0)
        {
          
            SloatCount(ds, 8);

        }
        lbApptSlotMore.Visible = true;
        lbNewPatient.Visible = false;
    }

    private void SloatCount(DataSet ds,int count )
    {
        if (ds.Tables.Count != 0)
        {
            if (ds.Tables["slots"].Rows.Count > 0)
            {
                ClearSteps("ApptType");
                cboApptSlot.Items.Clear();
                cboApptSlot.Items.Add(new ListItem("Select Appointment Slot", "0"));
                foreach (DataRow dr in ds.Tables["slots"].Rows)
                {//Remaining to check enable.need to check when get details.
                    string date = Convert.ToDateTime(dr["start_time"].ToString()).ToString("MM-dd-yyyy");
                    string[] AppSlotsTime = (dr["start_time"].ToString().Remove(0, 11)).Split('-');
                    string[] AppSlotsTime2 = (dr["end_time"].ToString().Remove(0, 11)).Split('-');
                    string[] appttime = AppSlotsTime[0].ToString().Split(':');
                    cboApptSlot.Items.Add(Convert.ToDateTime(date + " " + AppSlotsTime[0]).ToString("MM-dd-yyyy h:mm:ss tt"));
                    
                }
            }
        }
    }
    protected void Err(int num, string msg)
    {
        litError.Text += "Error "+ msg + "<br />";// + num.ToString() + " - " 
    }
    protected bool ValidateFields()
    {
        if (cboLocation.Items.Count <= 1) { Err(1099, "You must select a Referring From location first."); return false; }
        if (cboLocation.SelectedIndex == 0) { Err(1100, "You must select a Referring From location first."); return false; }
        if (cboEHPFacility.Items.Count <= 1) { Err(1101, "You must select a Facility first."); return false; }
        if (cboEHPFacility.SelectedIndex == 0) { Err(1102, "You must select a Facility first."); return false; }
        if (cboEHPDoctor.Items.Count <= 1) { Err(1103, "You must select a Doctor first."); return false; }
        if (cboEHPDoctor.SelectedIndex == 0) { Err(1104, "You must select a Doctor first."); return false; }
        if (cboApptType.Items.Count <= 1) { Err(1105, "You must select an Appointment Type first."); return false; }
        if (cboApptType.SelectedIndex == 0) { Err(1106, "You must select an Appointment Type first."); return false; }
        if (cboApptSlot.Items.Count <= 1) { Err(1107, "You must select an Appointment Slot first."); return false; }
        if (cboApptSlot.SelectedIndex == 0) { Err(1108, "You must select an Appointment Slot first."); return false; }
        //if (cboPatient.Items.Count <= 1) { Err(1109, "You must select a Patient first."); return false; }
        //if (cboPatient.SelectedIndex == 0) { Err(1110, "You must select a Patient first."); return false; }
        if (cboPatient1.Text=="") { Err(1109, "You must select a Patient first."); return false; }
        if (hdnpId.Value == "") { Err(1109, "You must select a Patient first."); return false; }
        if (!rbBoth.Checked && !rbEyeLeft.Checked && !rbEyeRight.Checked) { Err(1111, "You must select an Eye first."); return false; }
        //if (txtPhone.Text == "") { Err(1133, "You must provide the patient's current phone number."); return false; }
        
        // if (txtPhone.Text.Replace("(", "").Replace(" ", "").Replace(")", "").Replace("-", "").Length < 10) { Err(1134, "You must provide the patient's current phone number."); return false; }
        if (txtReason.Text == "") { Err(1135, "You must provide the Reason for Visit."); return false; }
        int DoctorIdCnt = API.Session.ValidateDoctor(cboEHPFacility.SelectedItem.Value.ToString(), API.Session.EntityID, cboEHPDoctor.SelectedValue);
        if (DoctorIdCnt==0) { Err(1135, "Invalid Doctor Provided."); return false; }
        return true;
    }

    protected void CreateAppointment()
    {
        try
        {
          //  Literal1.Text = "Please wait, Appointment is creating.";
            if (ValidateFields())
            {
                string ProviderId = GetProvider();
                string eye = "Eye: ";
                if (API.Session.SWEye == API.Eye.Both) eye += "Both";
                if (API.Session.SWEye == API.Eye.Left) eye += "Left";
                if (API.Session.SWEye == API.Eye.Right) eye += "Right";


                ////Post API 
                var responseString = "";
                WebRequest PostRequest = WebRequest.Create(Statics.URL_POSTCreateAppointment);
                PostRequest.Method = "POST";
                PostRequest.ContentType = "application/json";
                PostRequest.Headers.Add("Authorization", API.Session.AccessToken);//HH:mm:ss
                string date = Convert.ToDateTime(cboApptSlot.SelectedItem.ToString()).AddMonths(14).ToString("yyyy-MM-dd");
                string enddate = Convert.ToDateTime(cboApptSlot.SelectedItem.ToString()).ToString("yyyy-MM-dd");                                                                

                string Locationst = API.Session.ChState.ToString();
                if (API.Session.patientId != null)
                {
                    hdnpId.Value = API.Session.patientId;
                }
                string postData = "{\"appointment\": {\"start_time\":" + "\"" + date + "\"" + ", \"end_time\": " + "\"" + enddate + "\"" + ", \"appoint ment_status_id\": \"1\", \"location_id\":" + cboEHPFacility.SelectedValue + ", \"provider_id\":" + ProviderId + ", \"visit_reason_id\":" + cboApptType.SelectedValue + ", \"resource_id\":" + resouceId + ", \"chief_complaint\":" + "\"" + eye + " // Reason: " + txtReason.Text + "\"" + " ,\"comments\": \"string\", \"patient\": {\"id\":" + "\"" + hdnpId.Value + "\"" + "}}}";

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


                    API.Session.ReferralDoctor = cboLocation.SelectedItem.Text;
                    API.Session.AppointmentType = cboApptType.SelectedItem.Text;
                    API.Session.Datetime = cboApptSlot.SelectedItem.Text + " " + (API.Session.ChState.ToString() == "ETN" ? "EST" : "CST");
                    API.Session.Doctor = cboEHPDoctor.SelectedItem.Text;
                    API.Session.EyeDetails = eye + " // Reason: " + txtReason.Text;
                    API.Session.SWEHPFacilityID = Convert.ToInt32(cboEHPFacility.SelectedValue);
                    API.Session.SWFacility = cboEHPFacility.SelectedItem.Text;
                    //Facility

                    int locationId = 0;

                    string lacation = string.Empty;
                    string address = string.Empty;
                    string address2 = string.Empty;
                    string phone = string.Empty;
                    string fax = string.Empty;



                    foreach (DataRow row in dsLocation.Tables["locations"].Rows)
                    {
                        if (row["id"].ToString() == cboEHPFacility.SelectedValue.ToString())
                        {
                            locationId = int.Parse(row["locations_Id"].ToString());
                            foreach (DataRow row1 in dsLocation.Tables["address"].Rows)
                            {

                                if (row1["locations_Id"].ToString() == locationId.ToString())
                                {
                                    address = row1["line1"].ToString();
                                    address2 = row1["city"].ToString() + ", " + row1["state_name"].ToString() + ", " + row1["zip_code"].ToString();
                                }
                            }
                            foreach (DataRow row2 in dsLocation.Tables["phones"].Rows)
                            {

                                if (row2["locations_Id"].ToString() == locationId.ToString())
                                {
                                    if (row2["phone_type"].ToString() == "Business") { phone = row2["phone_number"].ToString(); }
                                    if (row2["phone_type"].ToString() == "Fax") { fax = row2["phone_number"].ToString(); }
                                }
                            }
                            API.Session.Phone = phone;
                            API.Session.Fax = fax;
                            API.Session.FacilityDetails = address + ", " + address2;
                            API.Session.FacilityContact = address2;
                            break;
                        }

                    }

                    string[] Locarray = cboLocation.SelectedItem.Value.Split('^');

                    string AppointmentId = string.Empty;
                    Regex regex = new Regex(@"[^\d]");
                    string phonenum = !string.IsNullOrEmpty(txtPhone.Text) ? regex.Replace(txtPhone.Text, "") : "";
                    AppointmentId = API.Session.CreateAppointmentLog(API.Session.Email, Convert.ToInt32(Locarray[1].ToString())
                        , cboEHPFacility.SelectedItem.Text, cboEHPDoctor.SelectedItem.Text, cboApptType.SelectedItem.Text
                        , Convert.ToDateTime(cboApptSlot.SelectedItem.ToString()), (API.Session.ChState.ToString() == "ETN" ? "EST" : "CST")
                        , hdnpId.Value, phonenum, eye, txtReason.Text, "Successful", API.Session.PracticeName, API.Session.ChState.ToString());

                    string PatientId = "";
                    if (!string.IsNullOrEmpty(API.Session.PatientId))
                    {
                        PatientId = API.Session.PatientId;
                    }
                    //Update Appointment Create Request And Response
                    API.Session.UpdateAppointmentReqResponse(postData, responseString, "Appointment", AppointmentId, PatientId);

                    SendClinicEmail();
                    SendReferringEmail();
                    Response.Redirect("~/CareCloudApptDetails.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();

                }
                catch (Exception ex)
                {

                    if (!string.IsNullOrEmpty(hdnpId.Value))
                    {
                        string[] Locarray1 = cboLocation.SelectedItem.Value.Split('^');
                        string AppointmentId = string.Empty;
                        Regex regex = new Regex(@"[^\d]");
                        string phonenum = !string.IsNullOrEmpty(txtPhone.Text) ? regex.Replace(txtPhone.Text, "") : "";
                        AppointmentId = API.Session.CreateAppointmentLog(API.Session.Email, Convert.ToInt32(Locarray1[1].ToString())
                                , cboEHPFacility.SelectedItem.Text, cboEHPDoctor.SelectedItem.Text, cboApptType.SelectedItem.Text
                                , Convert.ToDateTime(cboApptSlot.SelectedItem.ToString()), (API.Session.ChState.ToString() == "ETN" ? "EST" : "CST")
                                , hdnpId.Value, phonenum, eye, txtReason.Text, "Failed", API.Session.PracticeName, API.Session.ChState.ToString());
                        //Update Appointment Create Request And Response
                        string PatientId = "";
                        if (!string.IsNullOrEmpty(API.Session.PatientId))
                        {
                            PatientId = API.Session.PatientId;
                        }
                        API.Session.UpdateAppointmentReqResponse(postData, ex.Message, "Appointment", AppointmentId, PatientId);

                        API.Session.ReferralDoctor = cboLocation.SelectedItem.Text;
                        API.Session.AppointmentType = cboApptType.SelectedItem.Text;
                        API.Session.Datetime = cboApptSlot.SelectedItem.Text + " " + (API.Session.ChState.ToString() == "ETN" ? "EST" : "CST");
                        API.Session.Doctor = cboEHPDoctor.SelectedItem.Text;
                        API.Session.EyeDetails = eye + " // Reason: " + txtReason.Text;
                        API.Session.SWEHPFacilityID = Convert.ToInt32(cboEHPFacility.SelectedValue);
                        API.Session.SWFacility = cboEHPFacility.SelectedItem.Text;
                        //Facility

                        int locationId = 0;

                        string lacation = string.Empty;
                        string address = string.Empty;
                        string address2 = string.Empty;
                        string phone = string.Empty;
                        string fax = string.Empty;



                        foreach (DataRow row in dsLocation.Tables["locations"].Rows)
                        {
                            if (row["id"].ToString() == cboEHPFacility.SelectedValue.ToString())
                            {
                                locationId = int.Parse(row["locations_Id"].ToString());
                                foreach (DataRow row1 in dsLocation.Tables["address"].Rows)
                                {

                                    if (row1["locations_Id"].ToString() == locationId.ToString())
                                    {
                                        address = row1["line1"].ToString();
                                        address2 = row1["city"].ToString() + ", " + row1["state_name"].ToString() + ", " + row1["zip_code"].ToString();
                                    }
                                }
                                foreach (DataRow row2 in dsLocation.Tables["phones"].Rows)
                                {

                                    if (row2["locations_Id"].ToString() == locationId.ToString())
                                    {
                                        if (row2["phone_type"].ToString() == "Business") { phone = row2["phone_number"].ToString(); }
                                        if (row2["phone_type"].ToString() == "Fax") { fax = row2["phone_number"].ToString(); }
                                    }
                                }

                                API.Session.Phone = phone;
                                API.Session.Fax = fax;
                                API.Session.FacilityDetails = address + ", " + address2;
                                API.Session.FacilityContact = address2;
                                break;
                            }

                        }
                        //SendClinicEmail();
                        //SendReferringEmail();
                        Response.Redirect("~/CareCloudApptDetails.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                        Err(1108, "Appointment Creation Failed.");
                        Label1.Text = "";
                    }
                    
                }

            }
            else
            {
                Label1.Text = "";
            }
        }
        catch (Exception ex)
        {
            Err(1108, "Appointment Creation Failed.");
            Label1.Text = "";
        }
    }
    protected void SendClinicEmail()
    {
        string[] destinations = API.Session.GetFacilityContactList(API.Session.SWEHPFacilityID);
        string subject = "[SECURE] New Online Appointment Scheduled";
        string msg = "<html><body>";
        string eye = "";
        EHP.Formatting f = new EHP.Formatting();

        if (rbBoth.Checked) eye = "both";
        if (rbEyeLeft.Checked) eye = "left";
        if (rbEyeRight.Checked) eye = "right";
        msg += "An online appointment was just created in the CareCloud PMS for your clinic.<br><br>";
        msg += "Please verify the information is correct and there are no conflicts or other issues with this appointment.  ";
        msg += "You may also need to collect more information from the patient before their appointment.<BR><BR>";
        msg += "Date / Time the Appointment was created: " + DateTime.Now.ToString() +" "+"CST"+ "<BR>";
        msg += "Referring From: " + cboLocation.SelectedItem.Text + "<br>";
        msg += "Your Facility: " + cboEHPFacility.SelectedItem.Text + "<br>";
        msg += "Your Doctor: " + cboEHPDoctor.SelectedItem.Text + "<br>";
        msg += "Appointment Type: " + cboApptType.SelectedItem.Text + "<br>";
        msg += "Appointment Slot: " + cboApptSlot.SelectedItem.Text + " " + (API.Session.ChState.ToString() == "ETN" ? "EST" : "CST") + "<br>";
        msg += "Patient: " + cboPatient1.Text + "<br>";// cboPatient.SelectedItem.Text
        msg += "Patient Contact Number: " + f.PhoneNumber(txtPhone.Text) + "<br>";
        msg += "Email Address: " + API.Session.PtEmailAddress + " <br>";
        msg += "Eye: " + eye + "<br>";
        msg += "Reason for visit: " + txtReason.Text + "<br>";
        msg += "<BR><BR>If you feel that the program is in error, please forward this email to  RPPSupport@theseesgroup.com.<BR><BR>";
        msg += "</body></html>";
        if (destinations != null)
        {
            for (int a = 0; a < destinations.Length; a++)
            {
                string[] fields = destinations[a].Split('^');
                if (fields != null)
                {
                    if (fields.Length > 0)
                    {
                        /*
                        fields[0] = ID #
                        fields[1] = Name
                        fields[2] = Email Address
                        */

                        API.Session.SendEmail(fields[1], fields[2], subject, msg);

                    }
                }
            }
        }
       // API.Session.SendEmail("Uma Dixit", "udixit@pipartners.com", subject, msg);
    }
    protected void SendReferringEmail()
    {
        string subject = "[SECURE] Online Appointment Confirmation";
        string eye = "";
        if (rbBoth.Checked) eye = "both";
        if (rbEyeLeft.Checked) eye = "left";
        if (rbEyeRight.Checked) eye = "right";
        string msg = "<html><body>";
        msg += "This email is to confirm that your appointment has been scheduled in our system for your patient.  ";
        msg += "Due to HIPAA, PHI, and HiTech laws, the patient name will not be disclosed in this email.  If you need ";
        msg += "to cancel or reschedule this appointment, please contact our clinic.<BR><BR>";
        msg += "If you did not schedule this appointment and believe it was made in error, please contact our clinic.<BR><BR>";
        msg += "Date / Time the Appointment was created: " + DateTime.Now.ToString() +" "+ "CST" + "<BR>";
        msg += "Referring From: " + cboLocation.SelectedItem.Text + "<br>";
        msg += "Facility: " + cboEHPFacility.SelectedItem.Text + "<br>";
        msg += "Doctor: " + cboEHPDoctor.SelectedItem.Text + "<br>";
        msg += "Appointment Type: " + cboApptType.SelectedItem.Text + "<br>";
        msg += "Appointment Slot: " + cboApptSlot.SelectedItem.Text +" "+ (API.Session.ChState.ToString() == "ETN" ? "EST" : "CST") + "<br>";
        msg += "Patient: " + API.Session.SWPatient.ToString() + "<br>";
        msg += "Eye: " + eye + "<br>";
        msg += "Reason for visit: " + txtReason.Text + "<br>";
        msg += "<BR><BR>Please fax your referral note to our office.<BR>";
        msg += "<BR><BR>Facility Contact Information:<BR>";
        //string details = API.Session.GetFacilityContactDetails(API.Session.SWState, API.Session.SWCPSFacilityID);
        //if (details != null)
        //{
        //    string[] fields = details.Split('^');
        //    if (fields != null)
        //    {
        //        if (fields.Length > 0)
        //        {
        //            EHP.Formatting h = new EHP.Formatting();
        //            msg += "Address: " + fields[0] + "<br>";
        //            msg += "City, State, Zip: " + fields[1] + ", " + fields[2] + " " + fields[3] + "<br>";
        //            if (fields[4] != "") msg += "Phone: " + h.PhoneNumber(fields[4]) + "<BR>";
        //            if (fields[5] != "") msg += "Fax: " + h.PhoneNumber(fields[5]) + "<BR>";
        //        }
        //    }
        //}
        string address = API.Session.FacilityDetails;
        string [] val= address.Split(',');
        msg += "Address: " +val[0] + "<br>"; 
        msg += "City, State, Zip: "+API.Session.FacilityContact+ "<br/>";
        msg += "Phone: " + API.Session.Phone +"<br/>";
        msg += "Fax: " + API.Session.Fax +"<br/>";
        msg += "</body></html>";
        API.Session.SendEmail("", API.Session.Email, subject, msg);
    }
    protected void cboLocation_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshEHPFacilities_API();
    }

    protected void cboEHPFacility_SelectedIndexChanged(object sender, EventArgs e)
    {
        refreshDoctor_API();
    }

    protected void cboEHPDoctor_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshApptTypes_API();
       
    }

    protected void cboApptType_SelectedIndexChanged(object sender, EventArgs e)
    {
        
       int apptId = int.Parse(cboApptType.SelectedValue.ToString());
        resouceId= cboEHPDoctor.SelectedItem.Value;
        refreshApptSlots_API();
    }

    

    //protected void cboPatient_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //   // ShowPatient(API.Session.PatientId);
    //    txtPhone.Text = API.Session.PatientPhone;
    //}

    protected void btnBookAppt_Click(object sender, EventArgs e)
    {
        CreateAppointment();
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        cboLocation.Items.Clear();
        cboEHPFacility.Items.Clear();
        cboEHPDoctor.Items.Clear();
        cboApptType.Items.Clear();
        cboApptSlot.Items.Clear();
        cboPatient1.Text=string.Empty;
        txtPhone.Text=string.Empty;
        txtReason.Text=string.Empty;
        API.Session.ReferralDoctorValue = 0;
        API.Session.FacilityValue = 0;
        API.Session.DoctorValue = 0;
        API.Session.AppointmentTypeValue = 0;
        API.Session.SloatValue = 0;
        Response.Redirect("~/");
    }

    protected void lbApptSlotMore_Click(object sender, EventArgs e)
    {
        SloatCount(API.Session.Data, 500);
    }

    protected void lbNewPatient_Click(object sender, EventArgs e)
    {
        API.Session.SloatValue=cboApptSlot.SelectedIndex;
        API.Session.FacilityValue= cboEHPFacility.SelectedIndex;
        API.Session.DoctorValue = cboEHPDoctor.SelectedIndex;
        API.Session.AppointmentTypeValue = cboApptType.SelectedIndex;
        API.Session.ReferralDoctorValue = cboLocation.SelectedIndex;
        API.Session.refDocval= cboLocation.SelectedValue;
        Response.Redirect("~/AddPatientWithCareCloud.aspx");
       
    }

    protected void rbEyeLeft_CheckedChanged(object sender, EventArgs e)
    {
        API.Session.SWEye = API.Eye.Right;
       // SetEye(API.Eye.Left);
    }

    protected void rbEyeRight_CheckedChanged(object sender, EventArgs e)
    {
        API.Session.SWEye = API.Eye.Right;
        //SetEye(API.Eye.Right);
    }

    protected void rbBoth_CheckedChanged(object sender, EventArgs e)
    {

        API.Session.SWEye = API.Eye.Both;
        //SetEye(API.Eye.Both);
    }

    private void GetAccessTokenByAutherisationCode()
    {
        try
        {
            string URI = Statics.URL_GETAccessToken;
            string myParameters = "code=" + API.Session.AutherisationCode + "&grant_type=authorization_code&redirect_uri=" + Statics.RedirectUrl;
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("Authorization", Statics.AuthorizationKey);
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string HtmlResult = wc.UploadString(URI, myParameters);
                API.Session.LoggedOn = true;
                DataTable dt = JsonConvert.DeserializeObject<DataTable>("[" + HtmlResult + "]");

                API.Session.AccessToken = "Bearer " + dt.Rows[0][0].ToString();
            }
        } catch (Exception ex) 
        {

        }
    }




    protected void cboApptSlot_SelectedIndexChanged(object sender, EventArgs e)
    {
        ClearSteps("ApptSlot");
        if (cboApptSlot.Items.Count > 0)
        {
            if (cboApptSlot.SelectedIndex > 0)
            {
                //  refreshPatient_API();
                lbApptSlotMore.Visible = true;
                lbNewPatient.Visible = true;
                if (cboPatient1.Text != "")
                {
                    API.Session.patientId = hdnpId.Value.ToString();
                    SetPatient();

                }
                //else
                //{
                //    API.Session.patientId = "";
                //    API.Session.PatientPhone = "";
                //}
                int locationId = 0;
                if (dsLocation != null && dsLocation.Tables["locations"].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsLocation.Tables["locations"].Rows)
                    {
                        if (dr["id"].ToString() == cboEHPFacility.SelectedItem.Value.ToString())
                        {
                            locationId = int.Parse(dr["locations_id"].ToString());

                            foreach (DataRow dr1 in dsLocation.Tables["address"].Rows)
                            {
                                if (dr1["locations_id"].ToString() == locationId.ToString())
                                {
                                    API.Session.CityName = dr1["city"].ToString();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [System.Web.Services.WebMethod]
    public static  List<PatientModel> GetPatientList(string SearchParam, string cboEHPFacilityitem,string PVId)
    {
        List<PatientModel> list = new List<PatientModel>();
        int locationId = 0;
        DataTable dtPatient = new DataTable();
        dtPatient = API.Session.GetLocationwisePatientList(PVId);
        if (dtPatient.Rows.Count > 0)
        {
            foreach (DataRow dr in dtPatient.Rows)
            {
                list.Add(new PatientModel { PatientName = dr["PatientName"].ToString(), PID = dr["PID"].ToString(),PhoneNumber= dr["PhoneNumber"].ToString() });
            }
        }
        return list.Where(rd => rd.PatientName.ToLower().Contains(SearchParam.ToLower())).ToList();
    }

    public class PatientModel
    {
        public string PatientName { get; set; }
        public string PID { get; set; }
        public string PhoneNumber { get; set; }
    }
    protected void SetEye(API.Eye eye)
    {
        switch (eye)
        {
            case API.Eye.Both:
                rbBoth.Checked = true;
                rbEyeLeft.Checked = false;
                rbEyeRight.Checked = false;
                break;
            case API.Eye.Left:
                rbBoth.Checked = false;
                rbEyeLeft.Checked = true;
                rbEyeRight.Checked = false;
                break;
            case API.Eye.Right:
                rbBoth.Checked = false;
                rbEyeLeft.Checked = false;
                rbEyeRight.Checked = true;
                break;
            default:
                break;
        }
        if (txtPhone.Text == "" && hdnpId.Value != "")
        {
            List<PatientModel> list = new List<PatientModel>();
            DataTable dtPatient = new DataTable();
            dtPatient = API.Session.GetPatientList();

            if (dtPatient.Rows.Count > 0)
            {
                foreach (DataRow dr in dtPatient.Rows)
                {
                    //cboPatient.Items.Add(new ListItem(dr["PatientName"].ToString(), dr["PID"].ToString()));
                    list.Add(new PatientModel { PatientName = dr["PatientName"].ToString(), PID = dr["PID"].ToString(), PhoneNumber = dr["PhoneNumber"].ToString() });
                }
            }

            string phone = list.FirstOrDefault(x => x.PID == hdnpId.Value).PhoneNumber;
            string phonenumber = String.Empty;
            if (phone != "")
            {
                string case1 = phone.Substring(0, 3);
                string case2 = phone.Substring(3, 3);
                string case3 = phone.Substring(6);
                 phonenumber = string.Format("{0}-{1}-{2}", case1, case2, case3);
            }
            else
            {
                phonenumber = phone;
            }
            
            txtPhone.Text = phonenumber;
            Label2.Text = "Please verify the contact number. If different, please provide correct contact number in " + "\"Reason for Visit\"" + " section.";

        }
       
    }
    protected void SetLocation(int locationid)
    {
        if (cboLocation.Items.Count > 1)
        {
           cboLocation.SelectedIndex = locationid;
           // RefreshEHPFacilities();
           RefreshEHPFacilities_API();
        }
    }
    protected void SetFacility(int cpsfacilityid)
    {
        if (cboEHPFacility.Items.Count > 1)
        {
             cboEHPFacility.SelectedIndex = cpsfacilityid;
             refreshDoctor_API();
        }
    }
    protected void SetDoctor(int cpsdoctorid)
    {
        if (cboEHPDoctor.Items.Count > 1)
        {
           cboEHPDoctor.SelectedIndex = cpsdoctorid;
           RefreshApptTypes_API();
        }

    }
    protected void SetApptType(int appttypeid)
    {
                    cboApptType.SelectedIndex = appttypeid;
                    refreshApptSlots_API();
    }
    protected void SetApptSlot(int apptslotid)
    {
        cboApptSlot.SelectedIndex = apptslotid;
        lbNewPatient.Visible = true;
        SetPatient();
     
        //   refreshPatient_API();
    }
    //protected void SetPatient(int patientid)
    //{
    //                cboPatient.SelectedIndex = patientid;
    //}

    protected void SetPatient()
    {
        List<PatientModel> list = new List<PatientModel>();
        int locationId = 0;
        DataTable dtPatient = new DataTable();
         dtPatient = API.Session.GetPatientList();
                       
            if (dtPatient.Rows.Count > 0)
            {
                foreach (DataRow dr in dtPatient.Rows)
                {
                    list.Add(new PatientModel { PatientName = dr["PatientName"].ToString(), PID = dr["PID"].ToString() ,PhoneNumber=dr["PhoneNumber"].ToString()});
                }
            }

        if(list != null && !string.IsNullOrEmpty( API.Session.patientId))
        {
            cboPatient1.Text = list.FirstOrDefault(x => x.PID == API.Session.patientId).PatientName;
            hdnpId.Value = API.Session.patientId; // list.FirstOrDefault(x => x.PID == API.Session.patientId).PatientName;


            //  ShowPatient(API.Session.patientId);
            if (API.Session.PatientPhone == "")
            {
                string phonenum = list.FirstOrDefault(x => x.PID == API.Session.patientId).PhoneNumber;
                //string Phonenumber = String.Format("{0:###-###-####}", nnumber);
                //txtPhone.Text = Phonenumber; 
                string phonenumber = String.Empty;
                if (phonenum != "")
                {
                    string case1 = phonenum.Substring(0, 3);
                    string case2 = phonenum.Substring(3, 3);
                    string case3 = phonenum.Substring(6);
                     phonenumber = string.Format("{0}-{1}-{2}", case1, case2, case3);
                }
                else
                {
                    phonenumber = phonenum;
                }
               
                txtPhone.Text = phonenumber;
                Label2.Text = "Please verify the contact number. If different, please provide correct contact number in " + "\"Reason for Visit\"" + " section.";
            }
            else
            {
                //string Phonenum = String.Format("{0:###-###-####}", API.Session.PatientPhone);
                //txtPhone.Text = Phonenum; 
                string phone = API.Session.PatientPhone;
                string case1 = phone.Substring(0, 3);
                string case2 = phone.Substring(3, 3);
                string case3 = phone.Substring(6);
                string phonenumber = string.Format("{0}-{1}-{2}", case1, case2, case3);
                txtPhone.Text = phonenumber;
                Label2.Text = "Please verify the contact number. If different, please provide correct contact number in " + "\"Reason for Visit\"" + " section.";
            }
            

        }
        else
        {
            cboPatient1.Text = "";
            hdnpId.Value = "";
            txtPhone.Text = "";
        }
    }


    //protected void cboPatient1_TextChanged(object sender, EventArgs e)
    //{


    //    //ShowPatient(hdnpId.Value.ToString());
    //    //txtPhone.Text = API.Session.PatientPhone;
    //}

    protected void cboPatient1_TextChanged(object sender, EventArgs e)
    {
        if (txtPhone.Text == "" && hdnpId.Value != "")
        {
            List<PatientModel> list = new List<PatientModel>();
            DataTable dtPatient = new DataTable();
            dtPatient = API.Session.GetPatientList();

            if (dtPatient.Rows.Count > 0)
            {
                foreach (DataRow dr in dtPatient.Rows)
                {
                    //cboPatient.Items.Add(new ListItem(dr["PatientName"].ToString(), dr["PID"].ToString()));
                    list.Add(new PatientModel { PatientName = dr["PatientName"].ToString(), PID = dr["PID"].ToString(), PhoneNumber = dr["PhoneNumber"].ToString() });
                }
            }

            string phone = list.FirstOrDefault(x => x.PID == hdnpId.Value).PhoneNumber;
            string case1 = phone.Substring(0, 3);
            string case2 = phone.Substring(3, 3);
            string case3 = phone.Substring(6);
            string phonenumber = string.Format("{0}-{1}-{2}", case1, case2, case3);
            txtPhone.Text = phonenumber;
            Label2.Text = "Please verify the contact number. If different, please provide correct contact number in " + "\"Reason for Visit\"" + " section.";

        }
    }
    public void ShowPatient(string patientId)
    {

        var Response = "";
        DataSet ds = new DataSet();
        WebRequest Request = WebRequest.Create(Statics.URL_ShowPatient + patientId);
        Request.Method = "GET";
        Request.ContentType = "application/json";
        Request.Headers.Add("Authorization", API.Session.AccessToken);
        HttpWebResponse HttpWebResponse = null;
        HttpWebResponse = (HttpWebResponse)Request.GetResponse();

        using (Stream reader = HttpWebResponse.GetResponseStream())
        {
            StreamReader sr = new StreamReader(reader);
            Response = sr.ReadToEnd();
            sr.Close();
        }


        try
        {
            XmlDocument xd = new XmlDocument();
            Response = "{ \"rootNode\": {" + Response.Trim().TrimStart('{').TrimEnd('}') + "} }";
            xd = (XmlDocument)JsonConvert.DeserializeXmlNode(Response);

            ds.ReadXml(new XmlNodeReader(xd));
            //  return ds; 
        }
        catch (Exception ex)
        { throw new ArgumentException(ex.Message); }
        ClearSteps("Patient");
        //cboPatient.Items.Clear();
        //cboPatient.Items.Add(new ListItem("Select Patient", "0"));

        if (ds != null)
        {
            if (ds.Tables["patient"].Rows.Count > 0)
            {
                foreach (DataRow dr1 in ds.Tables["phones"].Rows)
                {
                    API.Session.PatientPhone = dr1["phone_number"].ToString();
                    break;
                }
            }
        }


    }

}


