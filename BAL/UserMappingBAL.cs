using CustomerOrderManagement.Helper;
using Microsoft.AspNetCore.Http;
using System.Data;


namespace BAL
{
	public class UserMappingBAL
	{
		private readonly MySqlService DBHelper;
		private readonly GlobalSessionBAL GlobalBal;
		// Constructor injection for MySqlService
		public UserMappingBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
		{
			DBHelper = mySqlService;
			GlobalBal = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
		}


		//THIS FUNCTION USED FOR GET ALL ITEMS
		public string CheckSubscriptionActiveOrNot()
		{
			string Status = "";
			try
			{
				DataSet ds = new DataSet();
				string query = "";
				query = "SELECT t.subscription_valid_till FROM ta_user_management AS u JOIN ta_tenant_master t ON t.id = u.tenant_id WHERE u.is_active = 1 AND u.id =" + GlobalBal.GetSessionValue("UserId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);

				if (ds != null && ds.Tables.Count > 0)
				{
					if (ds.Tables[0].Rows.Count > 0)
					{
						DateTime TodaysDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
						DateTime SubscriptionDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["subscription_valid_till"].ToString());

						if (SubscriptionDate > TodaysDate)
						{
							Status = "ACTIVE";
						}
					}
				}
			}
			catch (Exception ex)
			{

			}
			return Status;
		}

		// THIS FUNCTION IS USED TO GET ALL ACTIVE Tenant

		public DataSet GetUserData(Dictionary<string, object> data)
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

				string query = @"SELECT u.id, t.tenant_name, u.user_name, u.mobile_no, CASE WHEN u.branch_id = 0 THEN NULL ELSE b.branch_name END AS branch_name, r.role_name, u.is_active,(SELECT COUNT(*) FROM ta_user_mapping WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + @" AND user_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_user_mapping u LEFT JOIN ta_tenant_master AS t ON u.tenant_id = t.id LEFT JOIN ta_role_master AS r ON u.role_id = r.id LEFT JOIN ta_branch_master AS b ON u.branch_id = b.id WHERE u.tenant_id = " + GlobalBal.GetSessionValue("TenantId") + @" AND u.user_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY u.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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

		// THIS FUNCTION IS USED TO SAVE THE Tenant DETAILS
		public void SaveAddUser(IFormCollection form)
		{
			try
			{
				Dictionary<string, string> TagParam = new Dictionary<string, string>();
                string branch_id = (form.ContainsKey("BarnchId") && !string.IsNullOrEmpty(form["BarnchId"].ToString()))
                ? form["BarnchId"].ToString() : "0";
                TagParam.Add("tenant_id", form["TenantID"].ToString());
				TagParam.Add("mobile_no", form["Mobileno"].ToString());
				TagParam.Add("branch_id", branch_id);
				TagParam.Add("role_id", form["Role"].ToString());
				TagParam.Add("user_name", form["UserName"].ToString());
				TagParam.Add("is_active", form["IsActive"].ToString());
				TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
				TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

				int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_user_mapping", TagParam));
				// ADD DATA IN HISTORY TABLE
				AddHistory("ta_user_mapping", Id, "A");

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
						Param.Add("tenant_id", form["TenantID"].ToString());
						Param.Add("mobile_no", form["Mobileno"].ToString());
						Param.Add("is_active", form["IsActive"].ToString());
						Param.Add("role_id", form["Role"].ToString());
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


		// THIS FUNCTION IS USED TO GET TENANT DETAILS
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
		// THIS FUNCTION IS USED TO GET Branch Details
		public DataSet GetBranchDetails()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT id,branch_name FROM `ta_branch_master` WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " " + "AND is_active = 1;";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}
		public DataSet GetRole()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT id,role_name FROM ta_role_master WHERE id IN (3,4) AND is_active = 1;";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;

		}
		//THIS FUNCTION USED FOR CHECKING DUPLICATES
		public string CheckDuplicateRecord(string ColName, string Value)
		{
			string returnValue = "";
			try
			{
				string query = "SELECT COUNT(" + ColName + ") FROM ta_user_mapping WHERE " + ColName + "='" + Value + "'";
				returnValue = DBHelper.ExecuteQueryReturnObject(query);

				if (ColName == "mobile_no" && returnValue == "0")
				{
					string Query = "SELECT COUNT(*) AS cnt FROM ta_vendor_master WHERE mobile_no = '" + Value + "'";
					returnValue = DBHelper.ExecuteQueryReturnObject(Query);

					string Query1 = "SELECT COUNT(*) AS cnt FROM ta_tenant_master WHERE mobile_no = '" + Value + "'";
					returnValue = DBHelper.ExecuteQueryReturnObject(Query1);
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return returnValue;
		}
		// THIS FUNCTION IS USED TO GET ALL CITY DATA FOR EDITING
		public DataSet GetEditUserData(int id)
		{

			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT id,tenant_id,user_name,mobile_no,role_id,branch_id,is_active FROM `ta_user_mapping` where id = " + id;

				ds = DBHelper.ExecuteQueryReturnDS(query);


			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION IS FOR UPDATE USER DETAILS

		public void SaveEditUser(IFormCollection form)
		{
			try
			{
				Dictionary<string, string> TagParam = new Dictionary<string, string>();
				Dictionary<string, string> WhereParam = new Dictionary<string, string>();
				TagParam.Add("tenant_id", form["TenantID"].ToString());
				TagParam.Add("mobile_no", form["Mobileno"].ToString());
				TagParam.Add("branch_id", form["BarnchId"].ToString());
				TagParam.Add("role_id", form["Role"].ToString());
				TagParam.Add("user_name", form["UserName"].ToString());
				TagParam.Add("is_active", form["IsActive"].ToString());
				TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
				TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
				WhereParam.Add("id", form["UpdateRecordId"].ToString());
				AddHistory("ta_user_mapping", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
				DBHelper.ExecuteUpdateQuery("ta_user_mapping", TagParam, WhereParam);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		public DataSet GetFilterUserData(string selectedRowIds, string searchValue)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT t.tenant_name AS `Tenant Name`,u.user_name AS `User Name`,u.mobile_no AS `Mobile No`, b.branch_name AS `Branch Name`, r.role_name AS `Role Name`, CASE WHEN u.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,u.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,u.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By` " +
						 "FROM `ta_user_mapping` AS u " +
						 "JOIN `ta_tenant_master` AS t ON u.tenant_id = t.id " +
						 "LEFT JOIN `ta_branch_master` AS b ON u.branch_id = b.id " +
						 "JOIN `ta_role_master` AS r ON u.role_id = r.id " +
						 "LEFT JOIN ta_tenant_master AS t_created ON u.created_by = t_created.id LEFT JOIN ta_user_management AS um_created ON um_created.id = u.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = u.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no " +
						 "WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + " ";
				// If selectedRowIds is not empty, filter by category_master_id
				if (!string.IsNullOrEmpty(selectedRowIds))
				{
					query += "  AND u.id IN (" + selectedRowIds + ")" + " ORDER BY u.id DESC";
				}
				// If selectedRowIds is empty, filter by category_name using searchValue
				else if (!string.IsNullOrEmpty(searchValue))
				{
					query += " AND u.user_name LIKE '%" + searchValue + "%'" + " ORDER BY u.id DESC";
				}
				else
				{
					query += " ORDER BY u.id DESC";
				}

				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		// THIS FUNCTION IS USED GET DATA FROM HISTORY TABLE 
		public DataSet GetHistoryData(int id)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT t.tenant_name,u.user_name,u.mobile_no, b.branch_name, r.role_name, u.is_active,u.created_at,u.action_name,u.ip_address,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,u.updated_at,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by` FROM `ta_user_mapping_history` AS u JOIN `ta_tenant_master` AS t ON u.tenant_id = t.id LEFT JOIN `ta_branch_master` AS b ON u.branch_id = b.id JOIN `ta_role_master` AS r ON u.role_id = r.id LEFT JOIN ta_tenant_master AS t_created ON u.created_by = t_created.id LEFT JOIN ta_user_management AS um_created ON um_created.id = u.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = u.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE u.user_mapping_master_id = " + id + " order by u.updated_at desc ";

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

				query = "SELECT t.tenant_name,t.id AS `tenant_id`,u.user_name,u.mobile_no, b.branch_name, b.id AS `branch_id`,r.role_name,r.id AS `role_id`, u.is_active,u.created_by,u.updated_by FROM " + TableName + " u JOIN `ta_tenant_master` AS t ON u.tenant_id = t.id LEFT JOIN `ta_branch_master` AS b ON u.branch_id = b.id JOIN `ta_role_master` AS r ON u.role_id = r.id LEFT JOIN ta_tenant_master AS t_created ON u.created_by = t_created.id LEFT JOIN ta_user_management AS um_created ON um_created.id = u.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = u.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE u.id = " + Id;
				DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
				// Capture the necessary information for the history record
				Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

				// Populate the history parameters with the updated values
				HistoryParam.Add("user_mapping_master_id", Id.ToString());
				HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
				HistoryParam.Add("user_name", ds.Tables[0].Rows[0]["user_name"].ToString());
				HistoryParam.Add("mobile_no", ds.Tables[0].Rows[0]["mobile_no"].ToString());
				HistoryParam.Add("role_id", ds.Tables[0].Rows[0]["role_id"].ToString());
				if (ds.Tables[0].Rows[0]["branch_id"].ToString() != null)
				{
					HistoryParam.Add("branch_id", ds.Tables[0].Rows[0]["branch_id"].ToString());
				}
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
