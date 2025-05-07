using CustomerOrderManagement.Helper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL
{
    public class StoneColorMasterBAL
    {
        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBal;

        // Constructor injection for MySqlService
        public StoneColorMasterBAL(MySqlService mySqlService, GlobalSessionBAL globalBAL)
        {
            DBHelper = mySqlService;
            GlobalBal = globalBAL ?? throw new ArgumentNullException(nameof(globalBAL));
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

        //THIS IS USED FOR GET ALL STONE COLOR
        public DataSet GetAllStoneColor(Dictionary<string, object> data)
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

                string query = @"SELECT s.id,s.color_name,t.tenant_name,s.is_active,(SELECT COUNT(*) FROM ta_stone_color_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + @" AND color_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_stone_color_master s  LEFT JOIN ta_tenant_master AS t ON s.tenant_id = t.id WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + @" AND s.color_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY s.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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


        // THIS FUNCTION IS USED TO SAVE THE Stone DETAILS
        public void SaveAddStoneColor(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", form["TenantName"].ToString());
                TagParam.Add("color_name", form["StoneColor"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_stone_color_master", TagParam));
                AddHistory("ta_stone_color_master", Id, "A");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL CITY DATA FOR EDITING
        public DataSet GetEditStoneColorData(int id)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT id,tenant_id,color_name,is_active,remark FROM ta_stone_color_master WHERE id=" + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE EDIT CITY DATA
        public void SaveEditStoneColor(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                //TagParam.Add("tenant_id", form["TenantName"].ToString());
                //TagParam.Add("color_name", form["StoneColor"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());

                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                //TagParam.Add("password", form["Password"].ToString());
                WhereParam.Add("id", form["UpdateRecordId"].ToString());
                DBHelper.ExecuteUpdateQuery("ta_stone_color_master", TagParam, WhereParam);
                AddHistory("ta_stone_color_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        //THIS FUNCTION USED FOR CHECKING DUPLICATES
        public string CheckDuplicateRecord(string ColName, string value)
        {
            string returnValue = "";
            try
            {
                string query = "SELECT COUNT(" + ColName + ") FROM ta_stone_color_master WHERE " + ColName + "='" + value + "'and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

        public DataSet GetFilterStoneColorData(string selectedRowIds, string searchValue)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT t.tenant_name AS `Tenant Name`,p.color_name AS `Color Name`, CASE WHEN p.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,p.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,p.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By`  FROM ta_stone_color_master AS p JOIN ta_tenant_master AS t ON p.tenant_id=t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = p.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = p.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no where t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
                // If selectedRowIds is not empty, filter by category_master_id
                if (!string.IsNullOrEmpty(selectedRowIds))
                {
                    query += "  AND p.id IN (" + selectedRowIds + ")" + " ORDER BY p.id DESC";
                }
                // If selectedRowIds is empty, filter by category_name using searchValue
                else if (!string.IsNullOrEmpty(searchValue))
                {
                    query += " AND p.color_name LIKE '%" + searchValue + "%'" + " ORDER BY p.id DESC";
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
        public string CheckDuplicateStoneColor(string ColorName)
        {
            string Count = "";
            try
            {
                string query = "";
                query = "SELECT COUNT(*) AS cnt FROM ta_stone_color_master WHERE color_name ='" + ColorName + "' AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";
                Count = DBHelper.ExecuteQueryReturnObject(query).ToString();
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Count;
        }

        public string CheckDuplicateStoneColorTemp(string ColorName,string UniqueID)
        {
            string Count = "";
            try
            {
                string query = "";
                query = "SELECT COUNT(*) AS cnt FROM ta_stone_color_master_temp WHERE color_name ='" + ColorName + "' AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND unique_id = '" + UniqueID + "'  ";
                Count = DBHelper.ExecuteQueryReturnObject(query).ToString();
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Count;
        }

        //THIS FUNCTION USED FOR SAVE IMPORT DATA SAVE
        public void SaveImportDataStoneColor(string colorName, string remark,string Is_Active,String UniqueID)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
                TagParam.Add("color_name", colorName);
                TagParam.Add("remark", remark);
                TagParam.Add("unique_id", UniqueID);
                if (CheckDuplicateStoneColor(colorName) != "0" || Convert.ToInt32(CheckDuplicateStoneColorTemp(colorName, UniqueID)) > 0)
                {
                    TagParam.Add("Error_Status", "1");
                    TagParam.Add("Error_Message", "Stone Color Is duplicate find..");
                }
                TagParam.Add("is_active", Is_Active);
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                DBHelper.ExecuteInsertQuery("ta_stone_color_master_temp", TagParam);
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
                string query = "SELECT  s.id,s.color_name,t.tenant_name,s.ip_address,s.action_name,s.tenant_id,s.is_active,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by`,s.created_at,s.updated_at FROM ta_stone_color_master_history AS s JOIN ta_tenant_master AS t ON s.tenant_id=t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = s.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = s.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE s.stone_color_master_id = " + id + " order by s.updated_at desc";

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

                query = "SELECT s.id,s.color_name,t.tenant_name,s.tenant_id,s.is_active,s.created_by,s.updated_by FROM " + TableName + " s  LEFT JOIN ta_tenant_master AS t ON s.tenant_id = t.id WHERE s.id = " + Id;
                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("stone_color_master_id", Id.ToString());
                HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
                HistoryParam.Add("color_name", ds.Tables[0].Rows[0]["color_name"].ToString());
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
