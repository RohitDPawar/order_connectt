using CustomerOrderManagement.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NLog;
using System.Collections.Specialized;
using Google.Protobuf.WellKnownTypes;
namespace BAL
{
    public class SubVendorMasterBAL
    {
        private readonly MySqlService DBHelper;
		private readonly GlobalSessionBAL GlobalBal;

		// Constructor injection for MySqlService
		public SubVendorMasterBAL(MySqlService mySqlService, GlobalSessionBAL globalBAL)
        {
            DBHelper = mySqlService;
            GlobalBal = globalBAL ?? throw new ArgumentNullException(nameof(globalBAL));
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
        //this function is used to get Karagir ID
        public DataSet GetkaragirID()
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT id,vendor_name FROM ta_vendor_master WHERE is_active = 1 and tenant_id = "+ GlobalBal.GetSessionValue("TenantId");
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE Vendor DETAILS
        public void SaveAddSubVendor(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                //TagParam.Add("tenant_id", form["TenantID"].ToString());
                TagParam.Add("karagir_id", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("sub_vendor_name", form["SubVendorName"].ToString());
                TagParam.Add("mobile_no", form["Mobileno"].ToString());
                TagParam.Add("email_id", form["EmailID"].ToString());
                TagParam.Add("gst_no", "");
                TagParam.Add("pancard_no", "");
                TagParam.Add("residential_address", form["RessidentialAddress"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_sub_vendor_master", TagParam));
                // ADD DATA IN HISTORY TABLE
                AddHistory("ta_sub_vendor_master", Id, "A");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        // THIS FUNCTION IS USED TO GET ALL Sub Vendor DATA FOR EDITING
        public DataSet GetEditSubVendorData(int id)
        {

            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT id,tenant_id,karagir_id,sub_vendor_name,mobile_no,email_id,gst_no,pancard_no,residential_address,remark,is_active FROM ta_sub_vendor_master WHERE id = " + id;
             
                ds = DBHelper.ExecuteQueryReturnDS(query);


            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO GET ALL ACTIVE SubVendor
        public DataSet GetSubVendorData(Dictionary<string, object> data)
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

                string query = @"SELECT s.id,v.vendor_name,t.tenant_name,s.sub_vendor_name,s.mobile_no,s.email_id,s.gst_no,s.pancard_no,s.residential_address,s.is_active,(SELECT COUNT(*) FROM ta_sub_vendor_master WHERE karagir_id = " + GlobalBal.GetSessionValue("UserId") + @" AND sub_vendor_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_sub_vendor_master s LEFT JOIN ta_user_management AS u ON u.id = s.karagir_id LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no LEFT JOIN `ta_tenant_master` AS t ON t.id = s.karagir_id WHERE s.karagir_id = " + GlobalBal.GetSessionValue("UserId") + @" AND s.sub_vendor_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY s.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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

        // THIS FUNCTION IS USED TO SAVE THE EDIT CITY DATA
        public void SaveEditSubVendor(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                //TagParam.Add("tenant_id", form["TenantID"].ToString());
                TagParam.Add("karagir_id", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("sub_vendor_name", form["SubVendorName"].ToString());
                TagParam.Add("mobile_no", form["Mobileno"].ToString());
                TagParam.Add("email_id", form["EmailID"].ToString());
                TagParam.Add("gst_no", "");
                TagParam.Add("pancard_no", "");
                TagParam.Add("residential_address", form["RessidentialAddress"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                WhereParam.Add("id", form["UpdateRecordId"].ToString());
                DBHelper.ExecuteUpdateQuery("ta_sub_vendor_master", TagParam, WhereParam);
                AddHistory("ta_sub_vendor_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
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
                string query = "SELECT COUNT(" + ColName + ") FROM ta_sub_vendor_master WHERE " + ColName + "='" + Value + "' and karagir_id = " + GlobalBal.GetSessionValue("UserId");
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

        //THIS FUNCTION USED FOR CHECKING DUPLICATES
        public string CheckDuplicateRecordTemp(string ColName, string Value,string UniqueID)
        {
            string returnValue = "";
            try
            {
                string query = "SELECT COUNT(" + ColName + ") FROM ta_sub_vendor_master_temp WHERE " + ColName + "='" + Value + "' and karagir_id = " + GlobalBal.GetSessionValue("UserId")  +" AND unique_id = '" + UniqueID + "' "; ;
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }
        public DataSet GetFilterCategoryDataSubVendor(string selectedRowIds, string searchValue)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT v.vendor_name AS `Vendor Name`, s.sub_vendor_name AS `Sub Vendor Name`, s.mobile_no AS `Mobile No`, s.email_id AS `Email Id`, s.residential_address AS `Address`, CASE WHEN s.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`, s.created_at AS `Created At`, COALESCE(ump_created.user_name, tm_created.tenant_name, vm_created.vendor_name, 'Unknown') AS `Created By`, s.updated_at AS `Updated At`, COALESCE(ump_updated.user_name, tm_updated.tenant_name, vm_updated.vendor_name, 'Unknown') AS `Updated By` FROM ta_sub_vendor_master AS s LEFT JOIN `ta_user_management` AS u ON u.id = s.karagir_id LEFT JOIN `ta_vendor_master` AS v ON v.mobile_no = u.mobile_no LEFT JOIN `ta_tenant_master` AS t ON t.id = u.tenant_id LEFT JOIN ta_user_management AS um_created ON um_created.id = s.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = s.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_vendor_master AS vm_created ON vm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_vendor_master AS vm_updated ON vm_updated.mobile_no = um_updated.mobile_no WHERE s.karagir_id = " + GlobalBal.GetSessionValue("UserId");

                // If selectedRowIds is not empty, filter by category_master_id
                if (!string.IsNullOrEmpty(selectedRowIds))
				{
					query += " AND s.id IN (" + selectedRowIds + ")" + " ORDER BY s.id DESC"; ;
				}
				// If selectedRowIds is empty, filter by category_name using searchValue
				else if (!string.IsNullOrEmpty(searchValue))
				{
					query += " AND s.sub_vendor_name LIKE '%" + searchValue + "%'" + " ORDER BY s.id DESC"; ;
				}
				else
				{
					query += " ORDER BY s.id DESC";
				}

				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		public string CheckLoginUserPreviousVendorOrNot()
        {

			string returnValue = "0";
			try
			{
				string query = "SELECT v.vendor_name FROM ta_vendor_master AS v JOIN ta_user_management AS u ON v.mobile_no = u.mobile_no WHERE u.id = " + GlobalBal.GetSessionValue("UserId") + "";
				returnValue = DBHelper.ExecuteQueryReturnObject(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return returnValue;
		}

        public void SaveAddSubVendorImport(Dictionary<string, string> form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                //TagParam.Add("tenant_id", form["TenantID"].ToString());
                TagParam.Add("karagir_id", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("sub_vendor_name", form["SubVendorName"]);
                TagParam.Add("mobile_no", form["Mobileno"]);
                TagParam.Add("email_id", form["EmailID"]);
                if (CheckDuplicateRecord("mobile_no", form["Mobileno"]) != "0" || Convert.ToUInt32(CheckDuplicateRecordTemp("mobile_no", form["Mobileno"], form["unique_id"])) > 0)
                {
                    TagParam.Add("Error_Status", "1");
                    TagParam.Add("Error_Message", "Mobile no is duplicate find");
                }
                TagParam.Add("gst_no", "");
                TagParam.Add("pancard_no", "");
                TagParam.Add("unique_id", form["unique_id"]);
                TagParam.Add("residential_address", form["RessidentialAddress"]);
                TagParam.Add("is_active", form["is_active"]);
                TagParam.Add("remark", form["remark"]);
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                DBHelper.ExecuteInsertQuery("ta_sub_vendor_master_temp", TagParam);
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
                string query = "SELECT v.vendor_name, s.sub_vendor_name, s.mobile_no, s.email_id, s.residential_address,s.is_active,s.action_name,s.ip_address, s.created_at, COALESCE(ump_created.user_name, tm_created.tenant_name, vm_created.vendor_name, 'Unknown') AS `created_by`, s.updated_at, COALESCE(ump_updated.user_name, tm_updated.tenant_name, vm_updated.vendor_name, 'Unknown') AS `updated_by` FROM ta_sub_vendor_master_history AS s LEFT JOIN `ta_user_management` AS u ON u.id = s.karagir_id LEFT JOIN `ta_vendor_master` AS v ON v.mobile_no = u.mobile_no LEFT JOIN `ta_tenant_master` AS t ON t.id = u.tenant_id LEFT JOIN ta_user_management AS um_created ON um_created.id = s.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = s.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_vendor_master AS vm_created ON vm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_vendor_master AS vm_updated ON vm_updated.mobile_no = um_updated.mobile_no WHERE s.sub_vendor_master_id = " + id + " order by s.updated_at desc";

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

                query = "SELECT  s.id,v.vendor_name,t.tenant_name,s.sub_vendor_name,s.karagir_id,s.remark,s.mobile_no,s.email_id,s.gst_no,s.pancard_no,s.residential_address,s.is_active,s.created_by,s.updated_by FROM " + TableName + " s LEFT JOIN ta_user_management AS u ON u.id = s.karagir_id LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no LEFT JOIN `ta_tenant_master` AS t ON t.id = s.karagir_id  WHERE s.id = " + Id;
                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("sub_vendor_master_id", Id.ToString());
                HistoryParam.Add("karagir_id", ds.Tables[0].Rows[0]["karagir_id"].ToString());
                HistoryParam.Add("sub_vendor_name", ds.Tables[0].Rows[0]["sub_vendor_name"].ToString());
                HistoryParam.Add("mobile_no", ds.Tables[0].Rows[0]["mobile_no"].ToString());
                HistoryParam.Add("email_id", ds.Tables[0].Rows[0]["email_id"].ToString());
                HistoryParam.Add("gst_no", ds.Tables[0].Rows[0]["gst_no"].ToString());
                HistoryParam.Add("pancard_no", ds.Tables[0].Rows[0]["pancard_no"].ToString());
                HistoryParam.Add("residential_address", ds.Tables[0].Rows[0]["residential_address"].ToString());
                HistoryParam.Add("remark", ds.Tables[0].Rows[0]["remark"].ToString());
                HistoryParam.Add("is_active", ds.Tables[0].Rows[0]["is_active"].ToString());
                HistoryParam.Add("action_name", Action);
                HistoryParam.Add("ip_address", GlobalBal.GetClientIpAddress());
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

    }
}
