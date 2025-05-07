using CustomerOrderManagement.Helper;
using Microsoft.AspNetCore.Http;
using Mysqlx.Crud;
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
    public class StateMasterBAL
    {
        private readonly MySqlService DBHelper;
        private readonly GlobalSessionBAL GlobalBal;

        // Constructor injection for MySqlService
        public StateMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
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
                query = "select id,country_name from ta_country_master where is_active = '1'";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO GET FILTER  ITEMS LIST
        public DataSet GetFilteredCountryGroup(string SearchItem)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "select id,country_name from ta_country_master where is_active = '1' AND country_name LIKE '%" + SearchItem + "%'";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }

        // THIS FUNCTION IS USED TO SAVE THE STATE DETAILS
        public void SaveAddState(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                TagParam.Add("country_id", form["CountryId"].ToString());
                TagParam.Add("state_name", form["Name"].ToString());
                TagParam.Add("remark", form["remark"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());
                TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

                int Id = Convert.ToInt32(DBHelper.ExecuteInsertQuery("ta_state_master", TagParam));
                // ADD DATA IN HISTORY TABLE
                //AddHistory("ta_state_master", Id, "A");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL STATE DATA FOR EDITING
        public DataSet GetEditStateData(int id)
        {

            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "select id,country_id,state_name,is_active,remark from ta_state_master where id = " + id;
                ds = DBHelper.ExecuteQueryReturnDS(query);


            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }
        // THIS FUNCTION IS USED TO SAVE THE EDIT STATE DATA
        public void SaveEditState(IFormCollection form)
        {
            try
            {
                Dictionary<string, string> TagParam = new Dictionary<string, string>();
                Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                TagParam.Add("country_id", form["CountryId"].ToString());
                TagParam.Add("state_name", form["Name"].ToString());
                TagParam.Add("is_active", form["IsActive"].ToString());

                TagParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TagParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
                WhereParam.Add("id", form["UpdateRecordId"].ToString());

                DBHelper.ExecuteUpdateQuery("ta_state_master", TagParam, WhereParam);
                //AddHistory("ta_state_master", Convert.ToInt32(form["UpdateRecordId"].ToString()), "U");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }
        // THIS FUNCTION IS USED TO GET ALL ACTIVE STATES

        public DataSet GetStateData(Dictionary<string, object> data)
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

                string query = @"SELECT s.id,s.state_name,c.country_name,s.is_active,(SELECT COUNT(*) FROM ta_state_master WHERE  state_name LIKE '%" + searchText.ToString().Trim() + @"%') AS TotalRows FROM ta_state_master s JOIN ta_country_master c ON c.id = s.country_id WHERE s.state_name LIKE '%" + searchText.ToString().Trim() + @"%' ORDER BY s.id DESC LIMIT " + pageSize + " OFFSET " + offset;

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
        public string CheckDuplicateRecord(string ColName, string value)
        {
            string returnValue = "";
            try
            {
                string query = "SELECT COUNT(" + ColName + ") FROM ta_state_master WHERE " + ColName + "='" + value + "'";
                returnValue = DBHelper.ExecuteQueryReturnObject(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return returnValue;
        }

		public DataSet GetFilterStateData(string selectedRowIds, string searchValue)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "SELECT c.country_name AS `Country Name`,s.state_name AS `State Name`, CASE WHEN s.is_active = 1 THEN 'Active' ELSE 'Deactive' END AS `Status`,s.created_at AS`Created At`,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `Created By`,s.updated_at AS `Updated At`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `Updated By` FROM ta_state_master AS s JOIN ta_country_master c ON c.id = s.country_id LEFT JOIN ta_user_management AS um_created ON um_created.id = s.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = s.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no";
				// If selectedRowIds is not empty, filter by category_master_id
				if (!string.IsNullOrEmpty(selectedRowIds))
				{
					query += " where s.id IN (" + selectedRowIds + ")" + " ORDER BY s.id DESC";
				}
				// If selectedRowIds is empty, filter by category_name using searchValue
				else if (!string.IsNullOrEmpty(searchValue))
				{
					query += " where s.state_name LIKE '%" + searchValue + "%'" + " ORDER BY s.id DESC";
				}
				else
				{
					query += " ORDER BY s.id DESC";
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
                string query = "SELECT s.id,s.state_name,c.country_name,s.action_name,s.ip_address,s.is_active,COALESCE(ump_created.user_name, tm_created.tenant_name, 'Unknown') AS `created_by`,COALESCE(ump_updated.user_name, tm_updated.tenant_name, 'Unknown') AS `updated_by`,s.created_at,s.updated_at FROM ta_state_master_history AS s JOIN ta_country_master c ON c.id = s.country_id LEFT JOIN ta_user_management AS um_created ON um_created.id = s.created_by LEFT JOIN ta_user_management AS um_updated ON um_updated.id = s.updated_by LEFT JOIN ta_user_mapping AS ump_created ON ump_created.mobile_no = um_created.mobile_no LEFT JOIN ta_user_mapping AS ump_updated ON ump_updated.mobile_no = um_updated.mobile_no LEFT JOIN ta_tenant_master AS tm_created ON tm_created.mobile_no = um_created.mobile_no LEFT JOIN ta_tenant_master AS tm_updated ON tm_updated.mobile_no = um_updated.mobile_no WHERE s.state_master_id = " + id;

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

                query = "SELECT s.id,s.state_name,c.country_name,c.id as `countryid`,s.is_active,s.created_by,s.updated_by FROM " + TableName + " s JOIN ta_country_master c ON c.id = s.country_id WHERE s.id = " + Id;
                DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
                // Capture the necessary information for the history record
                Dictionary<string, string> HistoryParam = new Dictionary<string, string>();

                // Populate the history parameters with the updated values
                HistoryParam.Add("state_master_id", Id.ToString());
                HistoryParam.Add("country_id", ds.Tables[0].Rows[0]["countryid"].ToString());
                HistoryParam.Add("state_name", ds.Tables[0].Rows[0]["state_name"].ToString());
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
