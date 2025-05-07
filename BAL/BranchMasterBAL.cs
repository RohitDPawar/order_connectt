//Testing Rohit Pawar
//Testing Push
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

    public class BranchMasterBAL
    {
        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBal;
        // Constructor injection for MySqlService
        public BranchMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
        {
            DBHelper = mySqlService;
            GlobalBal = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }

        //THIS FUNCTION USED FOR GET ALL COUNTRY LIST
        public DataSet GetAllCountry()
        {
            DataSet ds = new DataSet();
            try
            {
                //string UserId = _sessionBAL.GetSessionValue("UserId");
                string query = "";
                query = "SELECT id,country_name FROM ta_country_master WHERE is_active=1";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

        //THIS FUNCTION USED FOR GET ALL STATE LIST
        public DataSet GetAllState()
        {
            DataSet ds = new DataSet();
            try
            {
                //string UserId = _sessionBAL.GetSessionValue("UserId");
                string query = "";
                query = "SELECT id,state_name FROM ta_state_master WHERE is_active=1";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO GET FILTER  COUNTRY LIST
        public DataSet GetFilteredCountry(string SearchItem)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT id,country_name FROM ta_country_master WHERE is_active=1 AND country_name LIKE '%" + SearchItem + "%'";

                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        //THIS FUNCTION USED FOR GET FILTER STATE LIST connected to country
        public DataSet GetStateNames(int CountryId, String SearchItem)
        {
            DataSet ds = new DataSet();
            try
            {
                //string UserId = _sessionBAL.GetSessionValue("UserId");

                //string query = "SELECT s.id, s.state_name FROM ta_state_master AS s " +
                //"JOIN ta_country_master AS c ON s.country_id = c.id " +
                //"WHERE s.country_id = " + CountryId + " AND s.is_active = 1";
                string query = "SELECT s.id, s.state_name FROM ta_state_master AS s " +
                 "JOIN ta_country_master AS c ON s.country_id = c.id " +
                 "WHERE s.country_id = " + CountryId + " " +
                 "AND s.is_active = 1 " +
                 "AND s.state_name LIKE '%" + SearchItem + "%'";



                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        //THIS FUNCTION USED FOR GET FILTER City LIST connected to country 
        public DataSet GetCityNames(int StateId, String SearchItem)
        {
            DataSet ds = new DataSet();
            try
            {
                //string UserId = _sessionBAL.GetSessionValue("UserId");

                //string query = "SELECT s.id, s.state_name FROM ta_state_master AS s " +
                //"JOIN ta_country_master AS c ON s.country_id = c.id " +
                //"WHERE s.country_id = " + CountryId + " AND s.is_active = 1";
                string query = "SELECT c.id, c.city_name FROM ta_city_master AS c " +
                 "JOIN ta_state_master AS s ON c.state_id = s.id " +
                 "WHERE c.state_id = " + StateId + " " +
                 "AND c.is_active = 1 " +
                 "AND c.city_name LIKE '%" + SearchItem + "%'";



                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        //THIS FUNCTION USED FOR GET ALL CITY LIST
        public DataSet GetAllCity()
        {
            DataSet ds = new DataSet();
            try
            {
                //string UserId = _sessionBAL.GetSessionValue("UserId");
                string query = "";
                query = "SELECT id,city_name FROM ta_city_master WHERE is_active=1";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
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

        //THIS FUNCTION GET ALL BRANCHES
        public DataSet GetAllBranches(Dictionary<string, object> data)
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

                string query = @"SELECT b.id,t.tenant_name,c.country_name,s.state_name,ci.city_name,b.branch_name,b.address,b.is_active,(SELECT COUNT(*) FROM ta_branch_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + @" AND branch_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_branch_master b LEFT JOIN ta_country_master AS c ON c.id = b.country_id LEFT JOIN ta_state_master AS s ON b.state_id = s.id LEFT JOIN ta_city_master AS ci ON b.city_id = ci.id LEFT JOIN ta_tenant_master AS t ON t.id = b.tenant_id WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + @" AND b.branch_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY b.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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
       

        // THIS FUNCTION IS USED TO SAVE THE CATEGORY DETAILS
        public void SaveAddBranch(IFormCollection form)
        {
            try
            {
                string UserId = GlobalBal.GetSessionValue("UserId");
                // Retrieve UserId from session
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", form["TenantName"].ToString());
                TagParam.Add("branch_name", form["BranchName"].ToString());
                TagParam.Add("address", form["Address"].ToString());
                TagParam.Add("area_description", form["AreaDescription"].ToString());
                TagParam.Add("pincode", form["Pincode"].ToString());
                TagParam.Add("country_id", form["CountryName"].ToString());
                TagParam.Add("state_id", form["StateName"].ToString());
                TagParam.Add("city_id", form["CityName"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_branch_master", TagParam));
                // ADD DATA IN HISTORY TABLE
                AddHistory("ta_branch_master", Id, "A");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL CATEGORY DATA FOR EDITING
        public DataSet GetEditBranchData(int id)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "select * from ta_branch_master where id = " + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE EDIT CATEGORY DATA
        public void SaveEditBranch(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                //TagParam.Add("tenant_id", form["TenantName"].ToString());
                //TagParam.Add("branch_name", form["BranchName"].ToString());
                //TagParam.Add("address", form["Address"].ToString());
                //TagParam.Add("area_description", form["AreaDescription"].ToString());
                //TagParam.Add("pincode", form["Pincode"].ToString());
                //TagParam.Add("country_id", form["CountryName"].ToString());
                //TagParam.Add("state_id", form["StateName"].ToString());
                //TagParam.Add("city_id", form["CityyName"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                WhereParam.Add("id", form["UpdateRecordId"].ToString());

                DBHelper.ExecuteUpdateQuery("ta_branch_master", TagParam, WhereParam);
                AddHistory("ta_branch_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL ACTIVE CATEGORY
        public DataSet GetCategoryData()
        {
            DataSet ds = new DataSet();

            try
            {
                string query = "";
                query = "SELECT c.id,t.tenant_name,i.item_name,c.category_name,c.is_active FROM ta_category_master AS c JOIN ta_tenant_master AS t ON c.tenant_id = t.id JOIN ta_item_master AS i ON i.id = c.item_id";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

        //THIS FUNCTION USED FOR CHECKING DUPLICATES
        public string CheckDuplicateRecord(string ColName, string value)
        {
            string returnValue = "";
            try
            {
                string query = "SELECT COUNT(" + ColName + ") FROM ta_branch_master WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND " + ColName + " = '" + value.Replace("'", "''") + "'";
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

        //THIS FUNCTION USED FOR CHECKING DUPLICATES
        public string CheckDuplicateRecordTemp(string ColName, string value ,string UniqueID)
        {
            string returnValue = "";
            try
            {
                string query = "SELECT COUNT(" + ColName + ") FROM ta_branch_master_temp WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND unique_id = '" + UniqueID + "'  " + " AND " + ColName + " = '" + value.Replace("'", "''") + "'";
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

        public DataSet GetFilterBranchData(string selectedRowIds, string searchValue)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT b.id AS `ID`,t.tenant_name AS `Tenant Name`,c.country_name AS 'Country Name',s.state_name AS `State Name`,ci.city_name AS `City Name`,b.branch_name AS `Branch Name`,b.address AS `Address`,CASE WHEN b.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,b.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By` ,b.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By`  FROM ta_branch_master AS b JOIN ta_country_master AS c ON c.id = b.country_id JOIN ta_state_master AS s ON b.state_id = s.id JOIN ta_city_master AS ci ON b.city_id = ci.id JOIN ta_tenant_master AS t ON t.id = b.tenant_id LEFT JOIN ta_user_management AS um_created ON um_created.id = b.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = b.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
				// If selectedRowIds is not empty, filter by category_master_id
				if (!string.IsNullOrEmpty(selectedRowIds))
				{
					query += " AND b.id IN (" + selectedRowIds + ")" + " ORDER BY b.id DESC";
				}
				// If selectedRowIds is empty, filter by category_name using searchValue
				else if (!string.IsNullOrEmpty(searchValue))
				{
					query += " AND b.branch_name LIKE '%" + searchValue + "%'" + " ORDER BY b.id DESC";
				}
				else
				{
					query += " ORDER BY b.id DESC";
				}

				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

        // THIS FUNCTION IS USED TO SAVE THE CATEGORY DETAILS
        public void SaveAddBranchImport(Dictionary<string,string> form)
        {
            try
            {
                string UserId = GlobalBal.GetSessionValue("UserId");
                // Retrieve UserId from session
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
                TagParam.Add("branch_name", form["BranchName"].ToString());
                TagParam.Add("address", form["address"].ToString());
                TagParam.Add("area_description", form["area_description"].ToString());
                TagParam.Add("pincode", form["pincode"].ToString());
                TagParam.Add("country_id", form["country_id"].ToString());
                TagParam.Add("state_id", form["state_id"].ToString());
                TagParam.Add("city_id", form["city_id"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", (form["is_active"].ToString().Equals("Yes")) ? "1" : "0");
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));

                DBHelper.ExecuteInsertQuery("ta_branch_master", TagParam);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        // THIS FUNCTION IS USED TO SAVE THE CATEGORY DETAILS
        public void SaveAddBranchImporttemp(Dictionary<string, string> form)
        {
            try
            {
                var CountryRow = GetAllCountry().Tables[0].AsEnumerable()
                            .FirstOrDefault(row => row["country_name"].ToString().Equals(form["country_name"], StringComparison.OrdinalIgnoreCase));

                var cityRow = GetAllCity().Tables[0].AsEnumerable()
                            .FirstOrDefault(row => row["city_name"].ToString().Equals(form["city_name"], StringComparison.OrdinalIgnoreCase));

                var stateRow = GetAllState().Tables[0].AsEnumerable()
                            .FirstOrDefault(row => row["state_name"].ToString().Equals(form["state_name"], StringComparison.OrdinalIgnoreCase));

                string City_Id = "";
                string State_Id = "";
                string Country_Id = "";

                if (CountryRow != null && cityRow != null && stateRow != null)
                {
                    City_Id = cityRow["id"].ToString();
                    State_Id = stateRow["id"].ToString();
                    Country_Id = CountryRow["id"].ToString();
                }

                bool stateExists = stateRow != null;
                bool cityExists = cityRow != null;
                bool countryExists = CountryRow != null;

                string UserId = GlobalBal.GetSessionValue("UserId");
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
                TagParam.Add("branch_name", form["BranchName"]);
                TagParam.Add("address", form["address"]);
                TagParam.Add("area_description", form["area_description"]);
                TagParam.Add("pincode", form["pincode"]);

                if (!countryExists || !cityExists || !stateExists || CheckDuplicateRecord("branch_name", form["BranchName"]) != "0" || Convert.ToInt32(CheckDuplicateRecordTemp("branch_name", form["BranchName"], form["unique_id"])) > 0)
                {
                    TagParam.Add("Error_Status", "1");
                    if (!countryExists)
                    {
                        TagParam.Add("Error_Message", "Country not found or invalid");
                    }
                    else if (!stateExists)
                    {
                        TagParam.Add("Error_Message", "State not found or invalid");
                    }
                    else if (!cityExists)
                    {
                        TagParam.Add("Error_Message", "City not found or invalid");
                    }
                    else
                    {
                        TagParam.Add("Error_Message", "Branch Name duplicate find");
                    }
                }

                TagParam.Add("country_id", Country_Id);
                TagParam.Add("state_id", State_Id);
                TagParam.Add("city_id", City_Id);
                TagParam.Add("country_name", form["country_name"]);
                TagParam.Add("state_name", form["state_name"]);
                TagParam.Add("city_name", form["city_name"]);
                TagParam.Add("unique_id", form["unique_id"]);
                TagParam.Add("remark", form["remark"]);
                TagParam.Add("is_active", form["is_active"]);
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId").ToString());
                DBHelper.ExecuteInsertQuery("ta_branch_master_temp", TagParam);
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
                string query = "SELECT b.id,t.tenant_name,c.country_name,s.state_name,ci.city_name,b.branch_name,b.address,b.is_active,b.action_name,b.ip_address,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by`,b.created_at,b.updated_at FROM ta_branch_master_history b JOIN ta_country_master AS c ON c.id = b.country_id JOIN ta_state_master AS s ON b.state_id = s.id JOIN ta_city_master AS ci ON b.city_id = ci.id JOIN ta_tenant_master AS t ON t.id = b.tenant_id LEFT JOIN ta_user_management AS um_created ON um_created.id = b.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = b.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE b.branch_master_id = " + id + " order by b.created_at desc";

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

                query = "SELECT b.id,t.tenant_name,b.tenant_id,c.country_name,c.id AS `country_id`,s.state_name,s.id AS `state_id`,ci.city_name,ci.id AS `city_id`,b.branch_name,b.address,b.area_description,b.pincode,b.is_active,b.remark,b.created_by,b.updated_by FROM " + TableName + " b LEFT JOIN ta_country_master AS c ON c.id = b.country_id LEFT JOIN ta_state_master AS s ON b.state_id = s.id LEFT JOIN ta_city_master AS ci ON b.city_id = ci.id LEFT JOIN ta_tenant_master AS t ON t.id = b.tenant_id WHERE b.id = " + Id;
                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("branch_master_id", Id.ToString());
                HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
                HistoryParam.Add("branch_name", ds.Tables[0].Rows[0]["branch_name"].ToString());
                HistoryParam.Add("address", ds.Tables[0].Rows[0]["address"].ToString());
                HistoryParam.Add("area_description", ds.Tables[0].Rows[0]["area_description"].ToString());
                HistoryParam.Add("pincode", ds.Tables[0].Rows[0]["pincode"].ToString());
                HistoryParam.Add("country_id", ds.Tables[0].Rows[0]["country_id"].ToString());
                HistoryParam.Add("state_id", ds.Tables[0].Rows[0]["state_id"].ToString());
                HistoryParam.Add("city_id", ds.Tables[0].Rows[0]["city_id"].ToString());
                HistoryParam.Add("remark", ds.Tables[0].Rows[0]["remark"].ToString());
                HistoryParam.Add("is_active", ds.Tables[0].Rows[0]["is_active"].ToString());
                HistoryParam.Add("action_name", Action);
                HistoryParam.Add("ip_address", GlobalBal.GetClientIpAddress());
                HistoryParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                HistoryParam.Add("created_by", GlobalBal.GetSessionValue("UserId").ToString());
                HistoryParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                HistoryParam.Add("updated_by", GlobalBal.GetSessionValue("UserId").ToString());

                // Insert the history record into ta_item_master_history
                DBHelper.ExecuteInsertQuery(TableName + "_history", HistoryParam);
            }
            catch (Exception ex)
            {

            }
        }


    }
}
