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
    public class ProductGroupMasterBAL
    {
        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBal;
        // Constructor injection for MySqlService
        public ProductGroupMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
        {
            DBHelper = mySqlService;
            GlobalBal = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }

        //THIS FUNCTION USED FOR SAVE PRODUCT GROUP
        public void SaveAddProductGroup(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", form["TenantName"].ToString());
                TagParam.Add("product_group_name", form["ProductGroupName"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_product_group_master", TagParam));
                // ADD DATA IN HISTORY TABLE
                AddHistory("ta_product_group_master", Id, "A");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL COUNTRY DATA FOR EDITING
        public DataSet GetProductGroupData(int id)
        {

            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT id,tenant_id,product_group_name,remark,is_active FROM ta_product_group_master WHERE id=" + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE EDIT COUNTRY DATA
        public void SaveEditProductGroup(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                //TagParam.Add("tenant_id", form["TenantName"].ToString());
                //TagParam.Add("product_group_name", form["ProductGroupName"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                WhereParam.Add("id", form["UpdateRecordId"].ToString());

                DBHelper.ExecuteUpdateQuery("ta_product_group_master", TagParam, WhereParam);

                AddHistory("ta_product_group_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL ACTIVE COUNTRIES

        public DataSet GetAllProductGroups(Dictionary<string, object> data)
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

                string query = @"SELECT p.id,t.tenant_name,p.product_group_name,p.is_active,(SELECT COUNT(*) FROM ta_product_group_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + @" AND product_group_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_product_group_master p LEFT JOIN ta_tenant_master AS t ON p.tenant_id = t.id WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + @" AND p.product_group_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY p.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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

        // THIS FUNCTION IS USED TO DELETE RECORDS
        public void DeleteCountryRecord(int id)
        {
            try
            {
                string query = "";
                query = "delete from tai_country_master where id=" + id;
                DBHelper.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        //THIS FUNCTION USED FOR CHECKING DUPLICATES
        public int CheckDuplicateRecord(string ColName, string Value)
        {
            int returnValue = 0;
            try
            {
                string query = "SELECT COUNT(" + ColName + ") FROM ta_product_group_master WHERE " + ColName + "='" + Value + "' and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
                returnValue = Convert.ToInt32(DBHelper.ExecuteQueryReturnObject(query));
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }


        //THIS FUNCTION IS USED TO GET TENANT DETAILS
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

        public DataSet GetFilterProductGroupData(string selectedRowIds, string searchValue)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT t.tenant_name AS `Tenant Name`,p.product_group_name AS `Product Group Name`,p.remark AS `Remark`, CASE WHEN p.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,p.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,p.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By` FROM ta_product_group_master AS p JOIN ta_tenant_master AS t ON p.tenant_id = t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = p.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = p.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no where p.tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";
                // If selectedRowIds is not empty, filter by category_master_id
                if (!string.IsNullOrEmpty(selectedRowIds))
                {
                    query += " AND p.id IN (" + selectedRowIds + ")" + " ORDER BY p.id DESC";
                }
                // If selectedRowIds is empty, filter by category_name using searchValue
                else if (!string.IsNullOrEmpty(searchValue))
                {
                    query += " AND p.product_group_name LIKE '%" + searchValue + "%'" + " ORDER BY p.id DESC";
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
        public string CheckDuplicateProductGroupRecord(string ProductGroupName)
        {

            string Count = "";
            try
            {
                string query = "";
                query = "SELECT COUNT(*) AS cnt FROM ta_product_group_master WHERE product_group_name ='" + ProductGroupName + "' AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";
                Count = DBHelper.ExecuteQueryReturnObject(query).ToString();
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Count;
        }

        public string CheckDuplicateProductGroupRecordTemp(string ProductGroupName,string uniqueId)
        {

            string Count = "";
            try
            {
                string query = "";
                query = "SELECT COUNT(*) AS cnt FROM ta_product_group_master_temp WHERE product_group_name ='" + ProductGroupName + "' AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND unique_id = '" + uniqueId + "'  ";
                Count = DBHelper.ExecuteQueryReturnObject(query).ToString();
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Count;
        }
		public void SaveImportDataProductGrp(string ProductGroupName, string remark, string Is_Active, string Unique_id)
		{
			try
			{
				Dictionary<string, string> TagParam = new Dictionary<string, string>();
				TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
				TagParam.Add("product_group_name", ProductGroupName);
				TagParam.Add("unique_id", Unique_id);
				TagParam.Add("remark", remark);
				if (!string.IsNullOrEmpty(ProductGroupName))
				{
					if (CheckDuplicateProductGroupRecord(ProductGroupName).ToString() != "0" || Convert.ToInt32(CheckDuplicateProductGroupRecordTemp(ProductGroupName, Unique_id)) > 0)
					{
						TagParam.Add("Error_Status", "1");
						TagParam.Add("Error_Message", "Product Group Name Is duplicate find..");
					}
				}
				else
				{
					TagParam.Add("Error_Status", "1");
					TagParam.Add("Error_Message", "Product Group Name Is Invalid..");
				}
				TagParam.Add("is_active", Is_Active);
				TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
				TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

				DBHelper.ExecuteInsertQuery("ta_product_group_master_temp", TagParam);
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
                string query = "SELECT p.id,t.tenant_name,p.product_group_name,p.ip_address,p.action_name,p.is_active,p.created_at,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,p.updated_at,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by` FROM ta_product_group_master_history p JOIN ta_tenant_master AS t ON p.tenant_id = t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = p.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = p.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE p.product_group_master_id = " + id + " order by p.created_at desc";

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

                query = "SELECT p.id,p.tenant_id,t.tenant_name,p.product_group_name,p.is_active,p.created_by,p.updated_by FROM " + TableName + " p  LEFT JOIN ta_tenant_master AS t ON p.tenant_id = t.id WHERE p.id = " + Id;
                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("product_group_master_id", Id.ToString());
                HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
                HistoryParam.Add("product_group_name", ds.Tables[0].Rows[0]["product_group_name"].ToString());
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
