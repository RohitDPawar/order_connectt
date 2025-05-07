using CustomerOrderManagement.Helper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace BAL
{
    public class PurityMasterBAL
    {
        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBal;
        // Constructor injection for MySqlService
        public PurityMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
        {
            DBHelper = mySqlService;
            GlobalBal = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }

        //THIS FUNCTION GET ALL TENANTS
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

        public DataSet GetAllPurity(Dictionary<string, object> data)
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

                string query = @"SELECT p.id,p.purity_name,p.purity,t.tenant_name,p.is_active,(SELECT COUNT(*) FROM ta_purity_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + @" AND purity LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_purity_master p LEFT JOIN ta_tenant_master AS t ON p.tenant_id = t.id WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + @" AND p.purity LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY p.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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

        // THIS FUNCTION IS USED TO GET ALL STATE NAME FROM COUNTRY MASTER
        public DataSet GetAllState()
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT id,state_name FROM ta_state_master WHERE is_active = 1";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE CITY DETAILS
        public void SaveAddPurity(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", form["TenantName"].ToString());
                TagParam.Add("purity", form["Purity"].ToString());
                TagParam.Add("purity_name", form["PurityName"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_purity_master", TagParam));
                // ADD DATA IN HISTORY TABLE
                AddHistory("ta_purity_master", Id, "A");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL CITY DATA FOR EDITING
        public DataSet GetEditPurityData(int id)
        {

            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT id,tenant_id,purity,purity_name,is_active,remark FROM ta_purity_master WHERE id=" + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE EDIT CITY DATA
        public void SaveEditPurity(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                //TagParam.Add("tenant_id", form["TenantName"].ToString());
                //TagParam.Add("purity", form["Purity"].ToString());
                //TagParam.Add("purity_name", form["PurityName"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());

                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                //TagParam.Add("password", form["Password"].ToString());
                WhereParam.Add("id", form["UpdateRecordId"].ToString());
                DBHelper.ExecuteUpdateQuery("ta_purity_master", TagParam, WhereParam);
                AddHistory("ta_purity_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        //THIS FUNCTION USED FOR CHECKING DUPLICATES
        public string CheckDuplicateRecord(string Purity)
        {
            string returnValue = "";
            try
            {
                string query = "SELECT purity FROM ta_purity_master WHERE purity ='" + Purity + "' and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

        public DataSet GetFilterPurityData(string selectedRowIds, string searchValue)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT p.purity_name AS `Purity Name`,p.purity AS `Purity`,t.tenant_name AS `Tenant Name`,CASE WHEN p.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,p.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,p.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By`  FROM ta_purity_master AS p JOIN ta_tenant_master AS t ON p.tenant_id = t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = p.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = p.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no where p.tenant_id  = " + GlobalBal.GetSessionValue("TenantId") + "";
                // If selectedRowIds is not empty, filter by category_master_id
                if (!string.IsNullOrEmpty(selectedRowIds))
                {
                    query += "  AND p.id IN (" + selectedRowIds + ")" + " ORDER BY p.id DESC";
                }
                // If selectedRowIds is empty, filter by category_name using searchValue
                else if (!string.IsNullOrEmpty(searchValue))
                {
                    query += " AND p.purity LIKE '%" + searchValue + "%'" + " ORDER BY p.id DESC";
                }
                else
                {
                    query += " ORDER BY p.id DESC";
                }

                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

        // THIS FUNCTION IS USED TO CHECK DUPLICATE
        public string CheckDuplicatePurity(string Purity)
        {
            string Count = "";
            try
            {
                string query = "";
                query = "SELECT COUNT(*) AS cnt FROM ta_purity_master WHERE purity ='" + Purity + "' AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";
                Count = DBHelper.ExecuteQueryReturnObject(query).ToString();
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Count;
        }

        public string CheckDuplicatePurityTemp(string Purity,string UniqueID)
        {
            string Count = "";
            try
            {
                string query = "";
                query = "SELECT COUNT(*) AS cnt FROM ta_purity_master_temp WHERE purity ='" + Purity + "' AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND unique_id = '" + UniqueID + "'  ";
                Count = DBHelper.ExecuteQueryReturnObject(query).ToString();
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Count;
        }

        public void SaveImportData(string purity, string purity_name, string Is_Active, string remark, string Unique_id)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
                TagParam.Add("purity", purity);
                TagParam.Add("purity_name", purity_name);
                TagParam.Add("unique_id", Unique_id);
                TagParam.Add("remark", remark);
                if (CheckDuplicatePurity(purity) != "0" || Convert.ToInt32(CheckDuplicatePurityTemp(purity, Unique_id)) > 0)
                {
                    TagParam.Add("Error_Status", "1");
                    TagParam.Add("Error_Message", "Purity Is duplicate find..");
                }
                TagParam.Add("is_active", Is_Active);
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                DBHelper.ExecuteInsertQuery("ta_purity_master_temp", TagParam);




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
                string query = "SELECT p.id,p.purity_name,p.purity,t.tenant_name,p.is_active,p.ip_address,p.action_name,p.created_at,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,p.updated_at,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by` FROM ta_purity_master_history p JOIN ta_tenant_master AS t ON p.tenant_id = t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = p.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = p.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE p.purity_master_id = " + id + " order by p.updated_at desc ";

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

                query = "SELECT p.id,p.purity_name,p.purity,t.tenant_name,p.tenant_id,p.is_active,p.created_by,p.updated_by FROM " + TableName + " p LEFT JOIN ta_tenant_master AS t ON p.tenant_id = t.id WHERE p.id = " + Id;
                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("purity_master_id", Id.ToString());
                HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
                HistoryParam.Add("purity", ds.Tables[0].Rows[0]["purity"].ToString());
                HistoryParam.Add("purity_name", ds.Tables[0].Rows[0]["purity_name"].ToString());
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
