using CustomerOrderManagement.Helper;
using Microsoft.AspNetCore.Http;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL
{
    public class CityMasterBAL
    {
        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBal;

        // Constructor injection for MySqlService
        public CityMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
        {
            DBHelper = mySqlService;
            GlobalBal = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
        }

        public DataSet GetAllCountry()
        {
            DataSet ds = new DataSet();
            try
            {
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
        public void SaveAddCity(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("country_id", form["CountryId"].ToString());
                TagParam.Add("state_id", form["StateId"].ToString());
                TagParam.Add("city_name", form["Name"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());

                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_city_master", TagParam));
                // ADD DATA IN HISTORY TABLE
                //AddHistory("ta_city_master", Id, "A");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL CITY DATA FOR EDITING
        public DataSet GetEditCityData(int id)
        {

            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "select id,country_id,state_id,city_name,is_active,remark from ta_city_master where id = " + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);


            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE EDIT CITY DATA
        public void SaveEditCity(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                TagParam.Add("country_id", form["CountryId"].ToString());
                TagParam.Add("state_id", form["StateId"].ToString());
                TagParam.Add("city_name", form["Name"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                //TagParam.Add("password", form["Password"].ToString());
                WhereParam.Add("id", form["UpdateRecordId"].ToString());
                DBHelper.ExecuteUpdateQuery("ta_city_master", TagParam, WhereParam);

                //AddHistory("ta_city_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        // THIS FUNCTION IS USED TO GET ALL ACTIVE CITIES
        public DataSet GetCityData(Dictionary<string, object> data)
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

                string query = @"SELECT c.id,c.city_name,c.is_active,s.state_name AS state,country.country_name AS country,(SELECT COUNT(*) FROM ta_city_master WHERE  city_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_city_master c JOIN ta_state_master s ON s.id = c.state_id JOIN ta_country_master country ON country.id = c.country_id WHERE c.city_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY c.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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
        public string CheckDuplicateRecord(string Country, string State, string City)
        {
            string returnValue = "";
            try
            {
                string query = "SELECT city_name FROM ta_city_master WHERE city_name ='" + City + "' and country_id = '" + Country + "' and state_id = '" + State + "'";
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }
        // Get Filter City data
		public DataSet GetFilterCityData(string selectedRowIds, string searchValue)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT  country.country_name AS `Country Name`, s.state_name AS `State Name`,c.city_name AS `City Name`, CASE WHEN c.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`, s.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,s.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By` FROM ta_city_master c JOIN ta_state_master s ON s.id = c.state_id JOIN ta_country_master country ON country.id = c.country_id LEFT JOIN ta_user_management AS um_created ON um_created.id = c.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = c.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE 1=1 ";
				// If selectedRowIds is not empty, filter by category_master_id
				if (!string.IsNullOrEmpty(selectedRowIds))
				{
					query += "  AND c.id IN (" + selectedRowIds + ")" + " ORDER BY c.id DESC";
				}
				// If selectedRowIds is empty, filter by category_name using searchValue
				else if (!string.IsNullOrEmpty(searchValue))
				{
					query += " AND c.city_name LIKE '%" + searchValue + "%'" + " ORDER BY c.id DESC";
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


        //THIS FUNCTION USED FOR GET ALL STATE LIST connected to country
        public DataSet GetStateNames(int CountryId,String SearchItem)
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

        // THIS FUNCTION IS USED GET DATA FROM HISTORY TABLE 
        public DataSet GetHistoryData(int id)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT c.id,c.city_name,c.is_active,s.state_name AS state,s.id AS `state_id`,country.country_name AS country,country.id AS `country_id`,c.action_name,c.ip_address,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by`,c.created_at,c.updated_at FROM ta_city_master_history c JOIN ta_state_master s ON s.id = c.state_id JOIN ta_country_master country ON country.id = c.country_id LEFT JOIN ta_user_management AS um_created ON um_created.id = c.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = c.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE c.city_master_id = " + id;

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

                query = "SELECT c.id,c.city_name,c.is_active,s.state_name AS state,s.id AS `state_id`,country.country_name AS country,country.id AS `country_id`,c.remark,c.created_by,c.updated_by FROM " + TableName + " c JOIN ta_state_master s ON s.id = c.state_id JOIN ta_country_master country ON country.id = c.country_id WHERE c.id = " + Id;
                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("city_master_id", Id.ToString());
                HistoryParam.Add("country_id", ds.Tables[0].Rows[0]["country_id"].ToString());
                HistoryParam.Add("state_id", ds.Tables[0].Rows[0]["state_id"].ToString());
                HistoryParam.Add("city_name", ds.Tables[0].Rows[0]["city_name"].ToString());
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
