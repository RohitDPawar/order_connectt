using CustomerOrderManagement.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NLog;
using System.Collections.Specialized;
using System.Data;

namespace BAL
{
    public class VendorMasterBAL
    {
        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBal;

        // Constructor injection for MySqlService
        public VendorMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
        {
            DBHelper = mySqlService;
            GlobalBal = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }

        //this function is used to get tenant ID
        public DataSet GetTenantDetails()
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT t.id,t.tenant_name FROM ta_tenant_master AS t JOIN ta_user_management AS u ON u.tenant_id = t.id WHERE u.id = " + GlobalBal.GetSessionValue("UserId");
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }


        // THIS FUNCTION IS USED TO SAVE THE Vendor DETAILS
        public void SaveAddVendor(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", form["TenantID"].ToString());
                TagParam.Add("vendor_name", form["VendorName"].ToString());
                TagParam.Add("mobile_no", form["Mobileno"].ToString());
                TagParam.Add("email_id", form["EmailID"].ToString());
                TagParam.Add("gst_no", form["GstNo"].ToString());
                TagParam.Add("pancard_no", form["PanCard"].ToString());
                TagParam.Add("residential_address", form["RessidentialAddress"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_vendor_master", TagParam));
                // ADD DATA IN HISTORY TABLE
                AddHistory("ta_vendor_master", Id, "A");
                // THIS FUNCTION IS USED TO INSERT DATA IN USER MANAGEMENT TABLE
                // ALSO USED FOR LOGIN PURPOSE
                if (Id > 0)
                {
                    // CHECK USER MOBILE NO IS ALREDAY PRESENT IN USER MAPPING TABLE OR NOT 
                    string Query = "SELECT mobile_no FROM ta_user_management WHERE is_active = 1 AND mobile_no = '" + form["Mobileno"].ToString() + "'";
                    string UserData = DBHelper.ExecuteQueryReturnObject(Query);

                    if (UserData == "")
                    {
                        Dictionary<string, string> Param = new Dictionary<string, string>();
                        /* Param.Add("tenant_id", form["TenantID"].ToString());*/
                        Param.Add("mobile_no", form["Mobileno"].ToString());
                        Param.Add("is_active", form["IsActive"].ToString());
                        Param.Add("role_id", "5");
                        Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                        Param.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        Param.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                        DBHelper.ExecuteInsertQuery("ta_user_management", Param);
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        // THIS FUNCTION IS USED TO GET ALL ACTIVE Vendor

        public DataSet GetVendorData(Dictionary<string, object> data)
        {
            DataSet ds = new DataSet();
            int pageNumber = 0; int pageSize = 0;
            string searchText = string.Empty;
            try
            {
                if (data.ContainsKey("PageNumber") && !string.IsNullOrEmpty(data["PageNumber"]?.ToString()))
                {
                    pageNumber = Convert.ToInt32(data["PageNumber"].ToString());
                }

                if (data.ContainsKey("PageSize") && !string.IsNullOrEmpty(data["PageSize"]?.ToString()))
                {
                    pageSize = Convert.ToInt32(data["PageSize"].ToString());
                }

                if (data.ContainsKey("SearchText") && !string.IsNullOrEmpty(data["SearchText"]?.ToString()))
                {
                    searchText = data["SearchText"].ToString();
                }

                int offset = (pageNumber - 1) * pageSize;

                string query = @"SELECT v.id,t.tenant_name,v.vendor_name,v.mobile_no,v.email_id,v.gst_no,v.pancard_no,v.residential_address,v.is_active,(SELECT COUNT(*) FROM ta_vendor_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + @" AND vendor_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_vendor_master v LEFT JOIN ta_tenant_master AS t ON v.tenant_id = t.id WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + @" AND v.vendor_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY v.id DESC LIMIT " + pageSize + " OFFSET " + offset;

                ds = DBHelper.ExecuteQueryReturnDS(query);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataTable paginationInfo = new DataTable();
                    paginationInfo.Columns.Add("TotalRows", typeof(int));
                    paginationInfo.Columns.Add("TotalPages", typeof(int));
                    paginationInfo.Columns.Add("PageNumber", typeof(int));
                    paginationInfo.Columns.Add("PageSize", typeof(int));

                    int totalRows = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalRows"]);

                    paginationInfo.Rows.Add(totalRows, (int)Math.Ceiling((double)totalRows / pageSize), pageNumber, pageSize);
                    ds.Tables.Add(paginationInfo);
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                //logger.Error(ex.Message);
            }

            return ds;
        }

        // THIS FUNCTION IS USED TO GET ALL CITY DATA FOR EDITING
        public DataSet GetEditVendorData(int id)
        {

            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT id,tenant_id,vendor_name,mobile_no,email_id,gst_no,pancard_no,residential_address,remark,is_active FROM ta_vendor_master where id = " + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);


            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

        // THIS FUNCTION IS USED TO SAVE THE EDIT CITY DATA
        public void SaveEditVendor(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                DataSet Ds = new DataSet();
                TagParam.Add("tenant_id", form["TenantID"].ToString());
                TagParam.Add("vendor_name", form["VendorName"].ToString());
                TagParam.Add("mobile_no", form["Mobileno"].ToString());
                TagParam.Add("email_id", form["EmailID"].ToString());
                TagParam.Add("gst_no", form["GstNo"].ToString());
                TagParam.Add("pancard_no", form["PanCard"].ToString());
                TagParam.Add("residential_address", form["RessidentialAddress"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("remark", form["remark"].ToString());

                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                WhereParam.Add("id", form["UpdateRecordId"].ToString());
                DBHelper.ExecuteUpdateQuery("ta_vendor_master", TagParam, WhereParam);
                TagParam.Clear();
                WhereParam.Clear();
                AddHistory("ta_vendor_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
                Ds = GetEditVendorData(Convert.ToInt32(form["UpdateRecordId"]));
                if (Ds != null && Ds.Tables[0].Rows.Count > 0)
                {
                    if (form["Mobileno"].ToString() == Ds.Tables[0].Rows[0]["mobile_no"].ToString())
                    {

                    }
                    else
                    {
                        TagParam.Add("mobile_no", form["Mobileno"].ToString());
                        TagParam.Add("is_active", form["IsActive"].ToString());
                        TagParam.Add("role_id", "5");
                        TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                        TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                        DBHelper.ExecuteInsertQuery("ta_user_management", TagParam);
                    }
                }


            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }


        //THIS FUNCTION USED FOR CHECKING DUPLICATES
        public string CheckDuplicateRecord(string ColName, string Value)
        {
            string returnValue = "";
            try
            {
                if (ColName == "mobile_no")
                {
                    string query = "SELECT COUNT(" + ColName + ") FROM ta_vendor_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND " + ColName + "='" + Value + "'";
                    returnValue = DBHelper.ExecuteQueryReturnObject(query);

                    if (returnValue == "0")
                    {
                        string query1 = "SELECT COUNT(" + ColName + ") FROM ta_tenant_master WHERE " + ColName + "='" + Value + "'";
                        returnValue = DBHelper.ExecuteQueryReturnObject(query1);
                    }
                }
                else
                {
                    string query = "SELECT COUNT(" + ColName + ") FROM ta_vendor_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND " + ColName + "='" + Value + "'";
                    returnValue = DBHelper.ExecuteQueryReturnObject(query);
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

        public DataSet GetFilterVendorData(string selectedRowIds, string searchValue)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT t.tenant_name AS `Tenant Name`,v.vendor_name AS `Vendor Name`,v.mobile_no AS `Mobile No`,v.email_id as `Email`,v.gst_no AS `GST No`,v.pancard_no AS `Pan Card`,v.residential_address AS `Residential Address`, CASE WHEN v.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,v.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,v.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By` FROM ta_vendor_master AS v JOIN ta_tenant_master AS t ON v.tenant_id = t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = v.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = v.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no where t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
                // If selectedRowIds is not empty, filter by category_master_id
                if (!string.IsNullOrEmpty(selectedRowIds))
                {
                    query += "  AND v.id IN (" + selectedRowIds + ")" + " ORDER BY v.id DESC";
                }
                // If selectedRowIds is empty, filter by category_name using searchValue
                else if (!string.IsNullOrEmpty(searchValue))
                {
                    query += " AND v.vendor_name LIKE '%" + searchValue + "%'" + " ORDER BY v.id DESC";
                }
                else
                {
                    query += " ORDER BY v.id DESC";
                }

                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

        public void SaveAddVendorImport(Dictionary<string, string> form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
                TagParam.Add("vendor_name", form["vendor_name"]);
                TagParam.Add("mobile_no", form["mobile_no"]);
                TagParam.Add("email_id", form["email_id"]);
                TagParam.Add("gst_no", form["gst_no"]);
                TagParam.Add("pancard_no", form["pancard_no"]);
                TagParam.Add("residential_address", form["residential_address"]);
                TagParam.Add("is_active", form["is_active"]);
                //bool gstpanmatch = false;
                //if (form["gst_no"].Length == 15)
                //{
                //    if (form["gst_no"].Substring(2, 10) == form["pancard_no"])
                //    {
                //        gstpanmatch = true;
                //    }
                //}


                bool hasError = false;
                string errorMessage = "";

                // Duplicate checks only if values are provided
                if (!string.IsNullOrEmpty(form["mobile_no"]) &&
                    (CheckDuplicateRecord("mobile_no", form["mobile_no"]) != "0" ||
                     Convert.ToUInt32(CheckDuplicateRecordTemp("mobile_no", form["mobile_no"], form["unique_id"])) > 0))
                {
                    errorMessage = "Mobile_no is duplicate or invalid.";
                    hasError = true;
                }
                else if (!string.IsNullOrEmpty(form["gst_no"]) &&
                         (CheckDuplicateRecord("gst_no", form["gst_no"]) != "0" ||
                          Convert.ToUInt32(CheckDuplicateRecordTemp("gst_no", form["gst_no"], form["unique_id"])) > 0))
                {
                    errorMessage = "GST number is duplicate or invalid.";
                    hasError = true;
                }
                else if (!string.IsNullOrEmpty(form["pancard_no"]) &&
                         (CheckDuplicateRecord("pancard_no", form["pancard_no"]) != "0" ||
                          Convert.ToUInt32(CheckDuplicateRecordTemp("pancard_no", form["pancard_no"], form["unique_id"])) > 0))
                {
                    errorMessage = "PAN card is duplicate or invalid.";
                    hasError = true;
                }

                // Add error details if found
                if (hasError)
                {
                    TagParam.Add("Error_Message", errorMessage);
                    TagParam.Add("Error_Status", "1");
                }

                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("unique_id", form["unique_id"]);
                DBHelper.ExecuteInsertQuery("ta_vendor_master_temp", TagParam);

                //int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_vendor_master", TagParam));

                //// THIS FUNCTION IS USED TO INSERT DATA IN USER MANAGEMENT TABLE
                //// ALSO USED FOR LOGIN PURPOSE
                //if (Id > 0)
                //{
                //    // CHECK USER MOBILE NO IS ALREDAY PRESENT IN USER MAPPING TABLE OR NOT 
                //    string Query = "SELECT mobile_no FROM ta_user_management WHERE is_active = 1 AND mobile_no = '" + form["Mobileno"].ToString() + "'";
                //    string UserData = DBHelper.ExecuteQueryReturnObject(Query);

                //    if (UserData == "")
                //    {
                //        Dictionary<string, string> Param = new Dictionary<string, string>();
                //        /* Param.Add("tenant_id", form["TenantID"].ToString());*/
                //        Param.Add("mobile_no", form["Mobileno"].ToString());
                //        Param.Add("is_active", form["IsActive"].ToString());
                //        Param.Add("role_id", "5");
                //        Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //        Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                //        Param.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //        Param.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                //        DBHelper.ExecuteInsertQuery("ta_user_management", Param);
                //    }
                //}
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        // THIS FUNCTION IS USED GET DATA FROM HISTORY TABLE 
        public DataSet GetHistoryData(int id)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT v.id,t.tenant_name,v.vendor_name,v.mobile_no,v.email_id,v.gst_no,v.pancard_no,v.residential_address,v.is_active,v.action_name,v.ip_address,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by`,v.created_at,v.updated_at FROM ta_vendor_master_history v JOIN ta_tenant_master AS t ON v.tenant_id = t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = v.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = v.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE v.vendor_master_id = " + id + " order by v.updated_at desc";

                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

        // THIS FUNCTION IS USED INSERT DATA INTO HISTORY TABLE 
        public void AddHistory(string TableName, int Id, string Action)
        {
            try
            {
                string query = "";

                query = "SELECT v.id,t.tenant_name,v.vendor_name,v.tenant_id,v.mobile_no,v.email_id,v.gst_no,v.pancard_no,v.residential_address,v.remark,v.is_active,v.created_by,v.updated_by FROM " + TableName + " v LEFT JOIN ta_tenant_master AS t ON v.tenant_id = t.id WHERE v.id = " + Id;
                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("vendor_master_id", Id.ToString());
                HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
                HistoryParam.Add("vendor_name", ds.Tables[0].Rows[0]["vendor_name"].ToString());
                HistoryParam.Add("mobile_no", ds.Tables[0].Rows[0]["mobile_no"].ToString());
                HistoryParam.Add("email_id", ds.Tables[0].Rows[0]["email_id"].ToString());
                HistoryParam.Add("gst_no", ds.Tables[0].Rows[0]["gst_no"].ToString());
                HistoryParam.Add("pancard_no", ds.Tables[0].Rows[0]["pancard_no"].ToString());
                HistoryParam.Add("residential_address", ds.Tables[0].Rows[0]["residential_address"].ToString());
                HistoryParam.Add("action_name", Action);
                HistoryParam.Add("ip_address", GlobalBal.GetClientIpAddress());
                HistoryParam.Add("remark", ds.Tables[0].Rows[0]["remark"].ToString());
                HistoryParam.Add("is_active", ds.Tables[0].Rows[0]["is_active"].ToString());
                HistoryParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                HistoryParam.Add("created_by", ds.Tables[0].Rows[0]["created_by"].ToString());
                HistoryParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                HistoryParam.Add("updated_by", ds.Tables[0].Rows[0]["updated_by"].ToString());

                // Insert the history record into ta_item_master_history
                DBHelper.ExecuteInsertQuery(TableName + "_history", HistoryParam);
            }
            catch (Exception ex)
            {

            }
        }

        //THIS FUNCTION USED FOR CHECKING DUPLICATES
        public string CheckDuplicateRecordTemp(string ColName, string Value, string UniqueID)
        {
            string returnValue = "";
            try
            {
                if (ColName == "mobile_no")
                {
                    string query = "SELECT COUNT(" + ColName + ") FROM ta_vendor_master_temp WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND " + ColName + "='" + Value + "' AND unique_id = '" + UniqueID + "'  ";
                    returnValue = DBHelper.ExecuteQueryReturnObject(query);

                    if (returnValue == "0")
                    {
                        string query1 = "SELECT COUNT(" + ColName + ") FROM ta_tenant_master WHERE " + ColName + "='" + Value + "'";
                        returnValue = DBHelper.ExecuteQueryReturnObject(query1);
                    }
                }
                else
                {
                    string query = "SELECT COUNT(" + ColName + ") FROM ta_vendor_master_temp WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND " + ColName + "='" + Value + "'" + " AND unique_id = '" + UniqueID + "' ";
                    returnValue = DBHelper.ExecuteQueryReturnObject(query);
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

    }
}
