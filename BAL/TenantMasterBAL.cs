using CustomerOrderManagement.Helper;
using Microsoft.AspNetCore.Http;
using System.Data;


namespace BAL
{
	public class TenantMasterBAL
	{
		private readonly MySqlService DBHelper;
		private readonly GlobalSessionBAL GlobalBal;
		// Constructor injection for MySqlService
		public TenantMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
		{
			DBHelper = mySqlService;
			GlobalBal = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
		}
		// THIS FUNCTION IS USED TO GET ALL Country ID
		public DataSet GetCountryID()
		{
			DataSet ds = new DataSet();

			try
			{
				string query = "select id,country_name from ta_country_master where is_active =1";

				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}
		//THIS FUNCTION USED FOR GET ALL STATE LIST connected to country
		public DataSet GetStateNames(int CountryId)
		{
			DataSet ds = new DataSet();
			try
			{
				//string UserId = _sessionBAL.GetSessionValue("UserId");

				string query = "SELECT s.id, s.state_name FROM ta_state_master AS s " +
				"JOIN ta_country_master AS c ON s.country_id = c.id " +
				"WHERE s.country_id = " + CountryId + " AND s.is_active = 1";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}
		// THIS FUNCTION IS USED TO GET ALL State ID
		public DataSet GetStateID()
		{
			DataSet ds = new DataSet();

			try
			{
				string query = "select id,state_name from ta_state_master";

				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}
		//THIS FUNCTION USED FOR GET ALL STATE LIST connected to country
		public DataSet GetCityNames(int StateId)
		{
			DataSet ds = new DataSet();
			try
			{
				//string UserId = _sessionBAL.GetSessionValue("UserId");

				string query = "SELECT c.id, c.city_name FROM `ta_city_master` AS c JOIN `ta_state_master` AS s ON c.state_id = s.id WHERE c.state_id=" + StateId + " AND c.is_active=1";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}
		// THIS FUNCTION IS USED TO GET ALL City ID
		public DataSet GetCityID()
		{
			DataSet ds = new DataSet();

			try
			{
				string query = "select id,city_name from ta_city_master";

				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}


		// THIS FUNCTION IS USED TO SAVE THE Tenant DETAILS
		public void SaveAddTenant(IFormCollection form)
		{
			try
			{
				Dictionary<string, string> TagParam = new Dictionary<string, string>();
				TagParam.Add("tenant_name", form["TenantName"].ToString());
				TagParam.Add("email_id", form["EmailID"].ToString());
				TagParam.Add("mobile_no", form["Mobileno"].ToString());
				TagParam.Add("contact_person_name", form["ContactPersonName"].ToString());
				TagParam.Add("address", form["RessidentialAddress"].ToString());
				TagParam.Add("area_description", form["area"].ToString());
				TagParam.Add("pincode", form["pincode"].ToString());
				TagParam.Add("country_id", form["CountryID"].ToString());
				TagParam.Add("state_id", form["StateID"].ToString());
				TagParam.Add("city_id", form["CityID"].ToString());
				TagParam.Add("gst_no", form["gstno"].ToString());
				TagParam.Add("pancard_no", form["pancardno"].ToString());
				TagParam.Add("prefix", form["Prefix"].ToString());
				TagParam.Add("subscription_valid_till", form["subscription"].ToString());
				TagParam.Add("is_active", form["IsActive"].ToString());
				TagParam.Add("remark", form["remark"].ToString());
				TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
				TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

				int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_tenant_master", TagParam));
				// ADD DATA IN HISTORY TABLE
				AddHistory("ta_tenant_master", Id, "A");
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
						Param.Add("tenant_id", Id.ToString());
						Param.Add("mobile_no", form["Mobileno"].ToString());
						Param.Add("is_active", form["IsActive"].ToString());
						Param.Add("role_id", "2");
						Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
						Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));
						Param.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
						Param.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

						DBHelper.ExecuteInsertQuery("ta_user_management", Param);
					}
					else
					{
						string Query1 = "SELECT id FROM ta_user_management WHERE is_active = 1 AND role_id = 5 AND mobile_no  = '" + form["Mobileno"].ToString() + "'";
						string UserId = DBHelper.ExecuteQueryReturnObject(Query1);

						if (UserData != "")
						{
							Dictionary<string, string> Param = new Dictionary<string, string>();
							Dictionary<string, string> WhereParam = new Dictionary<string, string>();
							Param.Add("tenant_id", Id.ToString());
							//Param.Add("mobile_no", form["Mobileno"].ToString());
							//Param.Add("is_active", form["IsActive"].ToString());
							//Param.Add("role_id", "3");
							Param.Add("role_id", "2");

							Param.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
							Param.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

							WhereParam.Add("id", UserId);
							DBHelper.ExecuteUpdateQuery("ta_user_management", Param, WhereParam);
						}
					}
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		// THIS FUNCTION IS USED TO GET ALL ACTIVE Tenant

		public DataSet GetTenantData(Dictionary<string, object> data)
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

				string query = @"SELECT t.id,t.tenant_name,t.email_id,t.mobile_no,t.contact_person_name,t.address,t.area_description,t.pincode,t.prefix,c.country_name, s.state_name,cm.city_name,t.gst_no,t.pancard_no,t.is_active,t.remark,(SELECT COUNT(*) FROM ta_tenant_master WHERE tenant_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_tenant_master t LEFT JOIN ta_country_master AS c ON t.country_id = c.id  LEFT JOIN ta_state_master AS s ON s.id = t.state_id LEFT JOIN ta_city_master AS cm ON cm.id = t.city_id WHERE t.tenant_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY t.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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
		public DataSet GetEditTenantData(int id)
		{

			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT id,tenant_name,email_id,mobile_no,contact_person_name,address,area_description,pincode,country_id,state_id,city_id,prefix,gst_no,pancard_no,subscription_valid_till,remark,is_active FROM `ta_tenant_master` WHERE id =" + id;
				ds = DBHelper.ExecuteQueryReturnDS(query);


			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}
		// THIS FUNCTION IS USED TO SAVE THE EDIT CITY DATA
		public void SaveEditTenant(IFormCollection form)
		{
			try
			{
				Dictionary<string, string> TagParam = new Dictionary<string, string>();
				Dictionary<string, string> WhereParam = new Dictionary<string, string>();
				TagParam.Add("tenant_name", form["TenantName"].ToString());
				TagParam.Add("email_id", form["EmailID"].ToString());
				TagParam.Add("mobile_no", form["Mobileno"].ToString());
				TagParam.Add("contact_person_name", form["ContactPersonName"].ToString());
				TagParam.Add("address", form["RessidentialAddress"].ToString());
				TagParam.Add("area_description", form["area"].ToString());
				TagParam.Add("pincode", form["pincode"].ToString());
				TagParam.Add("country_id", form["CountryID"].ToString());
				TagParam.Add("state_id", form["StateID"].ToString());
				TagParam.Add("city_id", form["CityID"].ToString());
				TagParam.Add("gst_no", form["gstno"].ToString());
				TagParam.Add("prefix", form["PrefixUpdate"].ToString());
				TagParam.Add("pancard_no", form["pancardno"].ToString());
				TagParam.Add("subscription_valid_till", form["subscription"].ToString());
				TagParam.Add("is_active", form["IsActive"].ToString());
				TagParam.Add("remark", form["remark"].ToString());

				TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

				WhereParam.Add("id", form["UpdateRecordId"].ToString());
				DBHelper.ExecuteUpdateQuery("ta_tenant_master", TagParam, WhereParam);
				AddHistory("ta_tenant_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
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
				string query = "SELECT COUNT(" + ColName + ") FROM ta_tenant_master WHERE " + ColName + "='" + Value + "'";
				returnValue = DBHelper.ExecuteQueryReturnObject(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return returnValue;
		}

		public DataSet GetFilterTenantData(string selectedRowIds, string searchValue)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT t.tenant_name AS `Tenant Name`,t.mobile_no AS `Mobile No`,t.email_id AS `Email Id`,t.contact_person_name AS `Contact Person Name`,t.address AS `Address`,c.country_name AS `Country Name`, s.state_name AS `State Name`,cm.city_name AS `City Name`,t.gst_no AS `GST No`,CASE WHEN t.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,t.pancard_no AS `Pan Card No`, t.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'TechneAI') AS `Created By`,t.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'TechneAI') AS `Updated By` FROM `ta_tenant_master` AS t LEFT JOIN `ta_country_master` AS c ON t.country_id = c.id  LEFT JOIN `ta_state_master` AS s ON s.id = t.state_id LEFT JOIN `ta_city_master` AS cm ON cm.id = t.city_id LEFT JOIN ta_user_management AS um_created ON um_created.id = t.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = t.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE 1=1 ";
				// If selectedRowIds is not empty, filter by category_master_id
				if (!string.IsNullOrEmpty(selectedRowIds))
				{
					query += "  AND t.id IN (" + selectedRowIds + ")" + " ORDER BY t.id DESC";
				}
				// If selectedRowIds is empty, filter by category_name using searchValue
				else if (!string.IsNullOrEmpty(searchValue))
				{
					query += " AND t.tenant_name LIKE '%" + searchValue + "%'" + " ORDER BY t.id DESC";
				}
				else
				{
					query += " ORDER BY t.id DESC";
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
				string query = "SELECT t.tenant_name,t.mobile_no ,t.email_id,t.contact_person_name,t.address,c.country_name, s.state_name,t.action_name,t.ip_address,cm.city_name,t.gst_no, t.is_active,t.pancard_no,t.created_at,COALESCE(ump_created.user_name, tm_created.tenant_name, 'TechneAI') AS `created_by`,t.updated_at,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'TechneAI') AS `updated_by` FROM `ta_tenant_master_history` AS t LEFT JOIN `ta_country_master` AS c ON t.country_id = c.id  LEFT JOIN `ta_state_master` AS s ON s.id = t.state_id LEFT JOIN `ta_city_master` AS cm ON cm.id = t.city_id LEFT JOIN ta_user_management AS um_created ON um_created.id = t.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = t.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE t.tenant_master_id = " + id;

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

				query = "SELECT t.id,t.tenant_name,t.email_id,t.mobile_no,t.contact_person_name,t.address,t.subscription_valid_till,t.contact_person_name,t.area_description,t.pincode,t.prefix,c.country_name,c.id AS `country_id`, s.state_name,s.id AS `state_id`,cm.id AS `city_id`,cm.city_name,t.gst_no,t.pancard_no,t.is_active,t.remark,t.created_by,t.updated_by FROM " + TableName + " t LEFT JOIN ta_country_master AS c ON t.country_id = c.id  LEFT JOIN ta_state_master AS s ON s.id = t.state_id LEFT JOIN ta_city_master AS cm ON cm.id = t.city_id WHERE t.id = " + Id;
				DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
				// Capture the necessary information for the history record
				Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

				// Populate the history parameters with the updated values
				HistoryParam.Add("tenant_master_id", Id.ToString());
				HistoryParam.Add("tenant_name", ds.Tables[0].Rows[0]["tenant_name"].ToString());
				HistoryParam.Add("email_id", ds.Tables[0].Rows[0]["email_id"].ToString());
				HistoryParam.Add("mobile_no", ds.Tables[0].Rows[0]["mobile_no"].ToString());
				HistoryParam.Add("contact_person_name", ds.Tables[0].Rows[0]["contact_person_name"].ToString());
				HistoryParam.Add("address", ds.Tables[0].Rows[0]["address"].ToString());
				HistoryParam.Add("area_description", ds.Tables[0].Rows[0]["area_description"].ToString());
				HistoryParam.Add("pincode", ds.Tables[0].Rows[0]["pincode"].ToString());
				HistoryParam.Add("country_id", ds.Tables[0].Rows[0]["country_id"].ToString());
				HistoryParam.Add("state_id", ds.Tables[0].Rows[0]["state_id"].ToString());
				HistoryParam.Add("city_id", ds.Tables[0].Rows[0]["city_id"].ToString());
				HistoryParam.Add("gst_no", ds.Tables[0].Rows[0]["gst_no"].ToString());
				HistoryParam.Add("pancard_no", ds.Tables[0].Rows[0]["pancard_no"].ToString());
				HistoryParam.Add("subscription_valid_till", ds.Tables[0].Rows[0]["subscription_valid_till"].ToString());
				HistoryParam.Add("prefix", ds.Tables[0].Rows[0]["prefix"].ToString());
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
