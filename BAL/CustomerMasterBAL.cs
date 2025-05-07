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
    public class CustomerMasterBAL
    {
        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBAL;

        // Constructor injection for MySqlService
        public CustomerMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
        {
            DBHelper = mySqlService;
            GlobalBAL = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }
        //this function is used to get tenant ID
        public DataSet GettenantID()
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT t.id,t.tenant_name FROM ta_tenant_master AS t JOIN ta_user_management AS u ON u.tenant_id = t.id WHERE u.id = " + GlobalBAL.GetSessionValue("UserId") + "";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO GET ALL Country ID
        public DataSet GetCountryID()
        {
            DataSet ds = new DataSet();

            try
            {
                string query = "select id,country_name from ta_country_master WHERE is_active = 1";

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
        // THIS FUNCTION IS USED TO GET ALL State ID
        public DataSet GetStateID()
        {
            DataSet ds = new DataSet();

            try
            {
                string query = "select id,state_name from ta_state_master WHERE is_active = 1";

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
                string query = "select id,city_name from ta_city_master WHERE is_active = 1";

                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE Vendor DETAILS
        public void SaveAddCustomer(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", form["TenantID"].ToString());
                TagParam.Add("customer_name", form["CustomerName"].ToString());
                TagParam.Add("mobile_no", form["Mobileno"].ToString());
                TagParam.Add("email_id", form["EmailID"].ToString());
                TagParam.Add("address", form["Address"].ToString());
                TagParam.Add("area_description", form["Area"].ToString());
                TagParam.Add("pincode", form["Pincode"].ToString());
                TagParam.Add("country_id", form["CountryID"].ToString());
                TagParam.Add("state_id", form["StateID"].ToString());
                TagParam.Add("city_id", form["CityID"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBAL.GetSessionValue("UserId").ToString());
                int id = DBHelper.ExecuteInsertQuery("ta_customer_master", TagParam);
                AddHistory("ta_customer_master", id, "A");

            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL CITY DATA FOR EDITING
        public DataSet GetEditCustomerData(int id)
        {

            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT id,tenant_id,customer_name,mobile_no,email_id,address,area_description,pincode,country_id,state_id,city_id,remark,is_active FROM ta_customer_master where id = " + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);


            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

        // THIS FUNCTION IS USED TO GET ALL ACTIVE Tenant

        public DataSet GetCustomerData(Dictionary<string, object> data)
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

                string query = @"SELECT cu.id,t.tenant_name,cu.customer_name,cu.mobile_no,cu.email_id,cu.address,cu.area_description,cu.pincode,c.country_name,s.state_name,cm.city_name,cu.is_active,cu.remark ,(SELECT COUNT(*) FROM ta_customer_master WHERE tenant_id = " + GlobalBAL.GetSessionValue("TenantId") + @" AND customer_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_customer_master cu LEFT JOIN ta_country_master AS c ON cu.country_id = c.id  LEFT JOIN ta_state_master AS s ON s.id = cu.state_id LEFT JOIN ta_city_master AS cm ON cm.id = cu.city_id LEFT JOIN ta_tenant_master AS t ON cu.tenant_id = t.id WHERE t.id = " + GlobalBAL.GetSessionValue("TenantId") + @" AND cu.customer_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY cu.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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
        public void SaveEditCustomer(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", form["TenantID"].ToString());
                TagParam.Add("customer_name", form["CustomerName"].ToString());
                TagParam.Add("mobile_no", form["Mobileno"].ToString());
                TagParam.Add("email_id", form["EmailID"].ToString());
                TagParam.Add("address", form["Address"].ToString());
                TagParam.Add("area_description", form["Area"].ToString());
                TagParam.Add("pincode", form["Pincode"].ToString());
                TagParam.Add("country_id", form["CountryID"].ToString());
                TagParam.Add("state_id", form["StateID"].ToString());
                TagParam.Add("city_id", form["CityID"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBAL.GetSessionValue("UserId").ToString());
                WhereParam.Add("id", form["UpdateRecordId"].ToString());
                DBHelper.ExecuteUpdateQuery("ta_customer_master", TagParam, WhereParam);
                AddHistory("ta_customer_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");

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
                string query = "SELECT COUNT(" + ColName + ") FROM ta_customer_master WHERE " + ColName + "='" + Value + "' AND tenant_id = " + GlobalBAL.GetSessionValue("TenantId") + "";
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
                string query = "SELECT COUNT(" + ColName + ") FROM ta_customer_master_temp WHERE " + ColName + "='" + Value + "' AND tenant_id = " + GlobalBAL.GetSessionValue("TenantId") + " AND unique_id = '" + UniqueID + "' ";
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

        // THIS FUNCTION IS USED TO GET HISTORY DATA
        public DataSet GetHistoryData(int id)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT cu.customer_master_id,t.tenant_name,cu.customer_name,cu.mobile_no,cu.email_id,cu.address,cu.area_description,cu.pincode,c.country_name,s.state_name,cm.city_name,cu.action_name,cu.ip_address,cu.client_detail,cu.remark,cu.is_active,cu.created_at,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,cu.updated_at,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by` FROM `ta_customer_master_history` AS cu LEFT JOIN `ta_tenant_master` AS t  ON cu.tenant_id=t.id  LEFT JOIN `ta_country_master` AS c ON cu.country_id = c.id  LEFT JOIN `ta_state_master` AS s ON s.id = cu.state_id LEFT JOIN `ta_city_master` AS cm ON cm.id = cu.city_id LEFT JOIN ta_user_management AS um_created ON um_created.id = cu.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = cu.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE cu.customer_master_id =" + id + " order by cu.updated_at desc";

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
                string query = "SELECT id, tenant_id, customer_name, mobile_no, email_id, address, area_description, pincode, country_id, state_id, city_id, remark, is_active FROM " + TableName + " where id= " + Id;



                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();
                HistoryParam.Add("customer_master_id", Id.ToString());
                HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
                HistoryParam.Add("customer_name", ds.Tables[0].Rows[0]["customer_name"].ToString());
                HistoryParam.Add("mobile_no", ds.Tables[0].Rows[0]["mobile_no"].ToString());
                HistoryParam.Add("email_id", ds.Tables[0].Rows[0]["email_id"].ToString());
                HistoryParam.Add("address", ds.Tables[0].Rows[0]["address"].ToString());
                HistoryParam.Add("area_description", ds.Tables[0].Rows[0]["area_description"].ToString());
                HistoryParam.Add("pincode", ds.Tables[0].Rows[0]["pincode"].ToString());
                HistoryParam.Add("country_id", ds.Tables[0].Rows[0]["country_id"].ToString());
                HistoryParam.Add("state_id", ds.Tables[0].Rows[0]["state_id"].ToString());
                HistoryParam.Add("city_id", ds.Tables[0].Rows[0]["city_id"].ToString());
                HistoryParam.Add("action_name", Action);
                HistoryParam.Add("ip_address", GlobalBAL.GetClientIpAddress());
                HistoryParam.Add("is_active", ds.Tables[0].Rows[0]["is_active"].ToString());
                HistoryParam.Add("remark", ds.Tables[0].Rows[0]["remark"].ToString());
                HistoryParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                HistoryParam.Add("created_by", GlobalBAL.GetSessionValue("UserId").ToString());
                HistoryParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                HistoryParam.Add("updated_by", GlobalBAL.GetSessionValue("UserId").ToString());
                // Insert the history record into ta_item_master_history
                DBHelper.ExecuteInsertQuery(TableName + "_history", HistoryParam);
            }
            catch (Exception ex)
            {
            }
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

        public DataSet GetFilterCustomersData(string selectedRowIds, string searchValue)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT t.tenant_name AS `Tenant Name`,cu.customer_name AS `Customer Name`,cu.mobile_no AS `Mobile No`,cu.email_id AS `Email`,cu.address AS `Address`,cu.area_description AS `Area Description`,cu.pincode AS `Pincode`,c.country_name,s.state_name AS `State Name`,cm.city_name AS `City Name`,CASE WHEN cu.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,cu.remark,cu.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,cu.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By` FROM `ta_customer_master` AS cu LEFT JOIN `ta_tenant_master` AS t  ON cu.tenant_id=t.id  LEFT JOIN `ta_country_master` AS c ON cu.country_id = c.id  LEFT JOIN `ta_state_master` AS s ON s.id = cu.state_id LEFT JOIN `ta_city_master` AS cm ON cm.id = cu.city_id LEFT JOIN ta_user_management AS um_created ON um_created.id = cu.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = cu.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE t.id = " + GlobalBAL.GetSessionValue("TenantId");
                // If selectedRowIds is not empty, filter by category_master_id
                if (!string.IsNullOrEmpty(selectedRowIds))
                {
                    query += "  AND cu.id IN (" + selectedRowIds + ")" + " ORDER BY cu.id DESC";
                }
                // If selectedRowIds is empty, filter by category_name using searchValue
                else if (!string.IsNullOrEmpty(searchValue))
                {
                    query += " AND cu.customer_name LIKE '%" + searchValue + "%'" + " ORDER BY cu.id DESC";
                }
                else
                {
                    query += " ORDER BY cu.id DESC";
                }

                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

        public void SaveAddCustomerImportTemp(Dictionary<string, string> form)
        {
            try
            {
                var cityRow = GetCityID().Tables[0].AsEnumerable()
                        .FirstOrDefault(row => row["city_name"].ToString().Equals(form["city_name"], StringComparison.OrdinalIgnoreCase));

                var CountryRow = GetCountryID().Tables[0].AsEnumerable()
                         .FirstOrDefault(row => row["country_name"].ToString().Equals(form["country_name"], StringComparison.OrdinalIgnoreCase));

                var stateRow = GetStateID().Tables[0].AsEnumerable()
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

                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("tenant_id", GlobalBAL.GetSessionValue("TenantId"));
                TagParam.Add("customer_name", form["customer_name"]);
                TagParam.Add("mobile_no", form["mobile_no"]);
                TagParam.Add("email_id", form["email_id"]);
                TagParam.Add("address", form["address"]);
                TagParam.Add("area_description", form["area_description"]);
                TagParam.Add("pincode", form["pincode"]);
                TagParam.Add("country_id", Country_Id);
                TagParam.Add("state_id", State_Id);
                TagParam.Add("city_id", City_Id);

                if (!countryExists || !cityExists || !stateExists || CheckDuplicateRecord("mobile_no", form["mobile_no"]) != "0" || Convert.ToInt32(CheckDuplicateRecordTemp("mobile_no", form["mobile_no"], form["unique_id"])) > 0 || string.IsNullOrEmpty(form["mobile_no"]))
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
                        TagParam.Add("Error_Message", "Mobile No is duplicate find or invalid");
                    }
                }
                TagParam.Add("country_name", form["country_name"]);
                TagParam.Add("state_name", form["state_name"]);
                TagParam.Add("city_name", form["city_name"]);
                TagParam.Add("unique_id", form["unique_id"]);
                TagParam.Add("is_active", form["is_active"]);
                TagParam.Add("remark", form["remark"]);
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBAL.GetSessionValue("UserId").ToString());
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBAL.GetSessionValue("UserId").ToString());
                DBHelper.ExecuteInsertQuery("ta_customer_master_temp", TagParam);

            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }



    }
}
