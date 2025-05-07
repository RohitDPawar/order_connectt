using CustomerOrderManagement.Helper;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BAL
{
    public class CategoryMasterBAL
    {
        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBal;
        // Constructor injection for MySqlService
        public CategoryMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
        {
            DBHelper = mySqlService;
            GlobalBal = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }

        //THIS FUNCTION USED FOR GET ALL ITEMS LIST
        public DataSet GetAllItems()
        {
            DataSet ds = new DataSet();
            try
            {
                //string UserId = _sessionBAL.GetSessionValue("UserId");
                string query = "";
                //query = "SELECT id,item_name FROM ta_item_master WHERE is_stone=0 AND is_active=1 and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
                query = "SELECT id,item_name FROM ta_item_master WHERE is_active=1 and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO GET FILTER  ITEMS LIST
        public DataSet GetFilteredItemGroup(string SearchItem)
      {
            DataSet ds = new DataSet();
            try
            {
                string query = "";

                //query = "SELECT id, product_group_name, is_active FROM ta_product_group_master WHERE is_active = 1 AND tenant_id = " + GlobalBal.GetSessionValue("TenantId")+ "   AND product_group_name LIKE '% +" SearchItem "+ %";
                query = "SELECT id,item_name FROM ta_item_master WHERE is_active=1 and tenant_id = " + GlobalBal.GetSessionValue("TenantId") +
                      " AND item_name LIKE '%" + SearchItem + "%'";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
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

        // THIS FUNCTION IS USED TO SAVE THE CATEGORY DETAILS
        public void SaveAddCategory(IFormCollection form)
        {
            try
            {
                string UserId = GlobalBal.GetSessionValue("UserId");
                // Retrieve UserId from session
                //string userId = _httpContextAccessor.HttpContext.Session.GetString(GlobalSessionBAL.SessionKeys.UserId);
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", form["TenantName"].ToString());
                TagParam.Add("item_id", form["ItemName"].ToString());
                TagParam.Add("category_name", form["CategoryName"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_category_master", TagParam));

                // ADD DATA IN HISTORY TABLE
                AddHistory("ta_category_master", Id, "A");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL CATEGORY DATA FOR EDITING
        public DataSet GetEditCategoryData(int id)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "select * from ta_category_master where id = " + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE EDIT CATEGORY DATA
        public void SaveEditItem(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                //TagParam.Add("tenant_id", form["TenantName"].ToString());
                //TagParam.Add("item_id", form["ItemName"].ToString());
                //TagParam.Add("category_name", form["CategoryName"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                WhereParam.Add("id", form["UpdateRecordId"].ToString());

                DBHelper.ExecuteUpdateQuery("ta_category_master", TagParam, WhereParam);

                // THIS FUNCTION ISUSED TO INSERT DATA INTO HISTORY TABLE
                AddHistory("ta_category_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL ACTIVE CATEGORY

        public DataSet GetCategoryData(Dictionary<string, object> data)
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

                string query = @"SELECT c.id,t.tenant_name,i.item_name,c.category_name,c.is_active,(SELECT COUNT(*) FROM ta_category_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + @" AND category_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_category_master c LEFT JOIN     ta_item_master AS i ON i.id = c.item_id LEFT JOIN ta_tenant_master AS t ON c.tenant_id = t.id WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + @" AND c.category_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY c.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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

        //THIS FUNCTION USED FOR CHECKING DUPLICATES
        public string CheckDuplicateRecord(string ColName, string value, string ItemId)
        {
            string returnValue = "";
            try
            {
                string query = "SELECT COUNT(" + ColName + ") FROM ta_category_master WHERE " + ColName + "='" + value + "' and item_id = " + ItemId + " and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

		public string CheckDuplicateRecordTemp(string ColName, string value, string ItemId, string UniqueID)
		{
			string returnValue = "";
			try
			{
				string query = "SELECT COUNT(" + ColName + ") FROM ta_category_master_temp WHERE " + ColName + "='" + value + "' and item_id = " + ItemId + " and tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND unique_id = '" + UniqueID + "' ";
				returnValue = DBHelper.ExecuteQueryReturnObject(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return returnValue;
		}

		// THIS FUNCTION IS USED INSERT DATA INTO HISTORY TABLE 
		public void AddHistory(string TableName, int Id, string Action)
        {
            try
            {
                string query = "";
                query = "SELECT tenant_id,item_id,category_name,remark,is_active,created_by,updated_by from " + TableName + " where id = " + Id;

                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("category_master_id", Id.ToString());
                HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
                HistoryParam.Add("category_name", ds.Tables[0].Rows[0]["category_name"].ToString());
                HistoryParam.Add("item_id", ds.Tables[0].Rows[0]["item_id"].ToString());
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

        // THIS FUNCTION IS USED TO GET HISTORY DATA
        public DataSet GetHistoryData(int id)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT category_master_id,c.tenant_id,category_name,action_name,ip_address,client_detail,c.remark,c.is_active, i.item_name,t.tenant_name, c.created_at,c.updated_at,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by`  FROM `ta_category_master_history` AS c  LEFT JOIN ta_item_master AS i ON i.id = c.item_id LEFT JOIN ta_tenant_master AS t ON t.id = c.tenant_id LEFT JOIN ta_user_management AS um_created ON um_created.id = c.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = c.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE c.category_master_id = " + id + " order by c.updated_at  desc ";

                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

		public DataSet GetFilterCategoryData(string selectedRowIds, string searchValue)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT t.tenant_name AS `Tenant Name`, i.item_name AS `Item Name`, c.category_name AS `Category Name`, c.remark AS `Remark`, CASE WHEN c.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status` ,c.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,c.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By` FROM ta_category_master AS c LEFT JOIN ta_item_master AS i ON i.id = c.item_id LEFT JOIN ta_tenant_master AS t ON t.id = c.tenant_id LEFT JOIN ta_user_management AS um_created ON um_created.id = c.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = c.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE 1=1 " + "and t.id = " + GlobalBal.GetSessionValue("TenantId");
				// If selectedRowIds is not empty, filter by category_master_id
				if (!string.IsNullOrEmpty(selectedRowIds))
				{
					query += "  AND c.id IN (" + selectedRowIds + ")" + " ORDER BY c.id DESC"; ;
				}
				// If selectedRowIds is empty, filter by category_name using searchValue
				else if (!string.IsNullOrEmpty(searchValue))
				{
					query += " AND c.category_name LIKE '%" + searchValue + "%'" + " ORDER BY c.id DESC"; ;
				}
				else
				{
					query += " ORDER BY c.id DESC";
				}

				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}
        //THIS FUNCTION USED FOR SAVE IMPORT DATA SAVE
        public void SaveImportData(string itemid, string Category_Name, string Remark, string Status)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
                TagParam.Add("item_id", itemid);
                TagParam.Add("category_name", Category_Name);
                TagParam.Add("remark", Remark);
                // Check the Status value and convert to 1 (active) or 0 (inactive)
                string statusValue = (Status?.ToUpper() == "YES") ? "1" : "0";
                TagParam.Add("is_active", statusValue);
           
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                DBHelper.ExecuteInsertQuery("ta_category_master", TagParam);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

		//THIS FUNCTION USED FOR SAVE IMPORT DATA SAVE
		public void SaveImportDataCategory(string item_name, string Category_Name, string Remark, string Status, string Unique_id)
		{
			try
			{
				var itemsTable = GetAllItems().Tables[0];

				var itemLookup = itemsTable.AsEnumerable().GroupBy(row => row["item_name"].ToString(), StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => group.First()["id"].ToString(), StringComparer.OrdinalIgnoreCase);


				var itemId = itemLookup.GetValueOrDefault(item_name);
				var validItemNames = itemLookup.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

				Dictionary<string, string> TagParam = new Dictionary<string, string>();
				TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
				TagParam.Add("unique_id", Unique_id);
				if (!string.IsNullOrEmpty(itemId))
				{
					TagParam.Add("item_id", itemId);
				}
				TagParam.Add("item_name", item_name);
				if (!string.IsNullOrEmpty(itemId))
				{
					if (CheckDuplicateRecord("Category_Name", Category_Name, itemId) != "0" || !validItemNames.Contains(item_name) || Convert.ToInt32(CheckDuplicateRecordTemp("Category_Name", Category_Name, itemId, Unique_id)) > 0)
					{
						TagParam.Add("Error_Status", "1");

						if (!validItemNames.Contains(item_name))
						{
							TagParam.Add("Error_Message", "Item name is not found or invalid.");
						}
						else
						{
							TagParam.Add("Error_Message", "Category name is duplicate.");
						}
					}
				}
				else
				{
					TagParam.Add("Error_Status", "1");
					TagParam.Add("Error_Message", "Item name is not found or invalid.");
				}
				TagParam.Add("category_name", Category_Name);
				TagParam.Add("remark", Remark);
				TagParam.Add("is_active", Status);
				TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
				TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

				DBHelper.ExecuteInsertQuery("ta_category_master_temp", TagParam);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		// THIS FUNCTION IS USED TO CHECK DUPLICATE
		public string CheckDuplicateCategory(string Category_Name)
        {
            string Count = "";
            try
            {
                string query = "";
                query = "SELECT COUNT(*) AS cnt FROM ta_category_master WHERE category_name ='" + Category_Name + "' AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";
                Count = DBHelper.ExecuteQueryReturnObject(query).ToString();

            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Count;
        }

        
        public void AddImportFileDataIntoHistory(string UniqueKey)
        {
			try
			{
				string query = "";
				query = "UPDATE ta_category_master_history SET ip_address = '"+ GlobalBal.GetClientIpAddress() + "' WHERE unique_id = "+UniqueKey+"";
				DBHelper.ExecuteQuery(query);

			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}
    }
}
