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
    public class ItemMasterBAL
    {

        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBal;

        // Constructor injection for MySqlService
        public ItemMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
        {
            DBHelper = mySqlService;
            GlobalBal = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }

        //THIS FUNCTION IS USED TO GET ITEMS DETAILS
        public DataSet GetAllItems(Dictionary<string, object> data)
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

                string query = @"SELECT i.id,t.tenant_name,p.product_group_name,i.remark,i.is_active,i.item_name,i.uom,i.is_stone,(SELECT COUNT(*) FROM ta_item_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + @" AND item_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_item_master i LEFT JOIN     ta_product_group_master AS p ON i.product_group_id = p.id LEFT JOIN ta_tenant_master AS t ON i.tenant_id = t.id WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + @" AND i.item_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY i.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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
        // THIS FUNCTION IS USED TO GET ALL PRODUCT GROUPS
        public DataSet GetAllProductGroup()
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT id,product_group_name,is_active FROM ta_product_group_master WHERE is_active = 1 and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
                //query = "SELECT id, product_group_name, is_active FROM ta_product_group_master WHERE is_active = 1 AND tenant_id =  + GlobalBal.GetSessionValue(TenantId) +  AND product_group_name LIKE '% + searchItem + %'";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO GET FILTER PRODUCT GROUPS
        public DataSet GetFilteredProductGroup(string SearchItem)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";

                //query = "SELECT id, product_group_name, is_active FROM ta_product_group_master WHERE is_active = 1 AND tenant_id = " + GlobalBal.GetSessionValue("TenantId")+ "   AND product_group_name LIKE '% +" SearchItem "+ %";
                 query = "SELECT id, product_group_name, is_active " +
                       "FROM ta_product_group_master " +
                       "WHERE is_active = 1 " +
                       "AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") +
                       " AND product_group_name LIKE '%" + SearchItem + "%'";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE CITY DETAILS
        public void SaveAddItem(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                string IsStone = (form.ContainsKey("IsStone") && !string.IsNullOrEmpty(form["IsStone"].ToString()))
                    ? form["IsStone"].ToString() : "0";

                TagParam.Add("tenant_id", form["TenantName"].ToString());
                TagParam.Add("product_group_id", form["ProductGroupName"].ToString());
                TagParam.Add("item_name", form["ItemName"].ToString());
                TagParam.Add("uom", form["UOM"].ToString());
                TagParam.Add("is_stone", IsStone);
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_item_master", TagParam));
                // ADD DATA IN HISTORY TABLE
                AddHistory("ta_item_master", Id, "A");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL CITY DATA FOR EDITING
        public DataSet GetEditItemData(int id)
        {

            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "select * from ta_item_master where id = " + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);


            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE EDIT CITY DATA
        public void SaveEditItem(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                //TagParam.Add("tenant_id", form["TenantName"].ToString());
                //TagParam.Add("product_group_id", form["ProductGroupName"].ToString());
                //TagParam.Add("item_name", form["ItemName"].ToString());
                //TagParam.Add("uom", form["UOM"].ToString());
                //TagParam.Add("is_stone", form["Is_Stone"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());

                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                //TagParam.Add("password", form["Password"].ToString());
                WhereParam.Add("id", form["UpdateRecordId"].ToString());
                DBHelper.ExecuteUpdateQuery("ta_item_master", TagParam, WhereParam);

                AddHistory("ta_item_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL ACTIVE CITIES
        public DataSet GetCityData()
        {
            DataSet ds = new DataSet();

            try
            {
                string query = "";
                query = "SELECT c.id,c.city_name,c.is_active,s.state_name AS state,country.country_name AS country FROM ta_city_master c JOIN ta_state_master s ON s.id = c.state_id JOIN ta_country_master country ON country.id = c.country_id ";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

		//THIS FUNCTION USED FOR CHECKING DUPLICATES
		public string CheckDuplicateRecordTemp(string ColName, string value, string ProductGroupId, string UniqueID)
		{
			string returnValue = "";
			try
			{
				string query = "SELECT COUNT(" + ColName + ") FROM ta_item_master_temp WHERE " + ColName + "='" + value + "' and  tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " and product_group_id=" + ProductGroupId + " AND unique_id = '" + UniqueID + "' ";
				returnValue = DBHelper.ExecuteQueryReturnObject(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return returnValue;
		}

		//THIS FUNCTION USED FOR CHECKING DUPLICATES
		public string CheckDuplicateRecord(string ColName, string value, string ProductGroupId)
        {
            string returnValue = "";
            try
            {
                string query = "SELECT COUNT(" + ColName + ") FROM ta_item_master WHERE " + ColName + "='" + value + "' and  tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

		public DataSet GetFilterItemData(string selectedRowIds, string searchValue)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT t.tenant_name AS `Tenant Name`,i.item_name AS `Item Name`,p.product_group_name AS `Product Group Name`,i.uom AS `UOM`,CASE WHEN i.is_stone = 1 THEN 'Yes' ELSE 'No' END AS `IS Stone`,CASE WHEN i.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,i.remark AS `Remark`,i.created_at AS `Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By`,i.updated_at AS `Updated At`  FROM ta_item_master i LEFT JOIN ta_product_group_master AS p ON i.product_group_id = p.id LEFT JOIN ta_tenant_master AS t ON i.tenant_id = t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = i.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = i.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + " ";
				// If selectedRowIds is not empty, filter by category_master_id
				if (!string.IsNullOrEmpty(selectedRowIds))
				{
					query += "  AND i.id IN (" + selectedRowIds + ")" + " ORDER BY i.id DESC";
				}
				// If selectedRowIds is empty, filter by category_name using searchValue
				else if (!string.IsNullOrEmpty(searchValue))
				{
					query += " AND i.item_name LIKE '%" + searchValue + "%'" + " ORDER BY i.id DESC";
				}
				else
				{
					query += " ORDER BY i.id DESC";
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
		public void SaveImportDataItem(string Product_Group_Name, string Item_Name, string UOM, string Is_Stone, string Remark, string Status, string Unique_id)
		{
			try
			{
				var validProductNames = GetAllProductGroup().Tables[0].AsEnumerable()
									  .Select(row => row["product_group_name"].ToString())
									  .ToHashSet(StringComparer.OrdinalIgnoreCase);

				var ProductLookup = GetAllProductGroup().Tables[0].AsEnumerable().ToDictionary(row => row["product_group_name"].ToString(), row => row["id"].ToString(), StringComparer.OrdinalIgnoreCase);

				var productId = ProductLookup.GetValueOrDefault(Product_Group_Name);

				Dictionary<string, string> TagParam = new Dictionary<string, string>();
				TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
				TagParam.Add("item_name", Item_Name);
				if (!string.IsNullOrEmpty(productId))
				{
					TagParam.Add("product_group_id", productId);
				}
				TagParam.Add("product_group_name", Product_Group_Name);
				TagParam.Add("uom", UOM);
				TagParam.Add("is_stone", Is_Stone);
				TagParam.Add("remark", Remark);
				TagParam.Add("unique_id", Unique_id);
				if (!string.IsNullOrEmpty(productId))
				{
					if (CheckDuplicateRecord("item_name", Item_Name, productId.ToString()) != "0" || !validProductNames.Contains(Product_Group_Name) || Convert.ToInt32(CheckDuplicateRecordTemp("Item_Name", Item_Name, productId.ToString(), Unique_id)) > 0)
					{
						TagParam.Add("Error_Status", "1");
						if (!validProductNames.Contains(Product_Group_Name))
						{
							TagParam.Add("Error_Message", "Product Group Name Is not find Or Invalid");
						}
						else
						{
							TagParam.Add("Error_Message", "Item Name Is duplicate find..");
						}
					}
				}
				else
				{
					TagParam.Add("Error_Status", "1");
					TagParam.Add("Error_Message", "Product Group Name Is not find Or Invalid");
				}
				TagParam.Add("is_active", Status);
				TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
				TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

				DBHelper.ExecuteInsertQuery("ta_item_master_temp", TagParam);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}
		// THIS FUNCTION IS USED TO CHECK DUPLICATE
		public string CheckDuplicateItem(string Item_Name)
        {
            string Count = "";
            try
            {
                string query = "";
                query = "SELECT COUNT(*) AS cnt FROM ta_item_master WHERE item_name ='" + Item_Name + "' AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";
                Count = DBHelper.ExecuteQueryReturnObject(query).ToString();

            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Count;
        }

        // THIS FUNCTION IS USED TO CHECK DUPLICATE
        public string CheckDuplicateItemTemp(string Item_Name,string UniqueID)
        {
            string Count = "";
            try
            {
                string query = "";
                query = "SELECT COUNT(*) AS cnt FROM ta_item_mastertemp WHERE item_name ='" + Item_Name + "' AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND unique_id = '" + UniqueID + "' ";
                Count = DBHelper.ExecuteQueryReturnObject(query).ToString();

            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Count;
        }

        // THIS FUNCTION IS USED GET DATA FROM HISTORY TABLE 
        public DataSet GetHistoryData(int id)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT i.id,i.action_name,t.tenant_name,p.product_group_name,i.remark,i.is_active,i.item_name,i.ip_address,i.uom,i.is_stone,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by`,i.created_at,i.updated_at FROM ta_item_master_history i LEFT JOIN ta_product_group_master AS p ON i.product_group_id = p.id LEFT JOIN ta_tenant_master AS t ON i.tenant_id = t.id LEFT JOIN ta_user_management AS um_created ON um_created.id = i.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = i.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE i.item_master_id = " + id + " order by i.updated_at desc ";

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

                query = "SELECT i.id,i.product_group_id,i.tenant_id,t.tenant_name,p.product_group_name,i.remark,i.is_active,i.item_name,i.uom,i.is_stone,i.created_by,i.updated_by FROM " + TableName + " i LEFT JOIN ta_product_group_master AS p ON i.product_group_id = p.id LEFT JOIN ta_tenant_master AS t ON i.tenant_id = t.id WHERE i.id = " + Id;
                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("item_master_id", Id.ToString());
                HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
                HistoryParam.Add("item_name", ds.Tables[0].Rows[0]["item_name"].ToString());
                HistoryParam.Add("product_group_id", ds.Tables[0].Rows[0]["product_group_id"].ToString());
                HistoryParam.Add("uom", ds.Tables[0].Rows[0]["uom"].ToString());
                HistoryParam.Add("is_stone", ds.Tables[0].Rows[0]["is_stone"].ToString());
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
