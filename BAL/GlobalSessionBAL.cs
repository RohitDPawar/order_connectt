using CustomerOrderManagement.Helper;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Net;
using System.Text;

namespace BAL
{
    public class GlobalSessionBAL
    {
        private readonly MySqlService DBHelper;
        /* private readonly MySqlService DBHelper;
		 public GlobalSessionBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
		 {
			 DBHelper = mySqlService;
		 }*/


        private static IHttpContextAccessor _httpContextAccessor;

        //public static dynamic name = new { FirstName = "Niki", LastName = "Kostov" };
        //public static dynamic nameAsString = JsonConvert.SerializeObject(name);
        //public static dynamic nameAsByteArray = Encoding.UTF8.GetBytes(nameAsString);
        //public static dynamic myObject = JsonConvert.DeserializeObject<MyObject>(session.GetString("MyObject"));
        // Initialize the IHttpContextAccessor through dependency injection
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public GlobalSessionBAL(IHttpContextAccessor httpContextAccessor, MySqlService mySqlService)
        {
            _httpContextAccessor = httpContextAccessor;
            DBHelper = mySqlService;
        }

        // Set session value as a string
        public void SetSessionValue(string key, string value)
        {

            var session = _httpContextAccessor.HttpContext.Session;
            var nameAsByteArray = Encoding.UTF8.GetBytes(value);
            session.Set(key, nameAsByteArray); // Correct method to set a string in the session
        }


        public string GetSessionValue(string key)
        {
            var session = _httpContextAccessor.HttpContext.Session;

            // Attempt to get the value from the session using TryGetValue
            if (session.TryGetValue(key, out var byteArray))
            {
                // Convert the byte array back to a string if the value exists
                return Encoding.UTF8.GetString(byteArray);
            }

            // Return null if the session value does not exist
            return null;
        }

        // THIS FUNCTION IS USED TO GET CLIENT IP ADDRESS
        public string GetClientIpAddress()
        {
            // Get the hostname of the system
            string hostName = Dns.GetHostName();

            // Get all the IP addresses associated with the hostname
            var ipAddresses = Dns.GetHostAddresses(hostName);

            // Filter to get IPv4 addresses (optional, to exclude IPv6 addresses)
            var ipv4Addresses = ipAddresses.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            // Display the first available IPv4 address or indicate no addresses found
            string systemIpAddress = ipv4Addresses.FirstOrDefault()?.ToString() ?? "No IPv4 address found";

            return systemIpAddress;
        }
        //THIS FUNCTION IS USED TO GET CLIENT DETAILS(BROWSER NAME,HOST,URL LINK)
        public string GetClientBrowserDetail()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            var host = httpContext.Request.Host.ToString();
            var referer = httpContext.Request.Headers["Referer"].ToString();

            string clientDetails = $"BrowserDetails:\"{userAgent}\",Host:\"{host}\",Referer:\"{referer}\"";
            // return JsonConvert.SerializeObject(clientDetails);
            return clientDetails;
        }

        //THIS IS USED FOR INSERT NOTIFICATION MESSAGE IN NOTIFICATION TABLE
        public void InsertNotificationMessage(string Message, string RoleId)
        {
            try
            {
                if (GetSessionValue("TenantId").ToString() != "")
                {
                    string Query = "INSERT INTO ta_notification_master(tenant_id,notification,role_id) VALUES (" + GetSessionValue("TenantId") + ",'" + Message + "'," + RoleId + ")";
                    DBHelper.ExecuteQuery(Query);
                }
                else
                {
                    string GetTenant = "SELECT v.tenant_id FROM ta_vendor_master AS v JOIN ta_user_management AS u ON u.mobile_no = v.mobile_no WHERE u.id = " + GetSessionValue("UserId").ToString() + "";
                    string GetVendorTenantId = DBHelper.ExecuteQueryReturnObj(GetTenant).ToString();
                    string Query = "INSERT INTO ta_notification_master(tenant_id,notification,role_id) VALUES (" + GetVendorTenantId + ",'" + Message + "'," + RoleId + ")";
                    DBHelper.ExecuteQuery(Query);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public string GetTenantName(string OrderDetailsId)
        {
            string HO_Name = "";
            try
            {
                string query = "";
                query = "SELECT tenant_name FROM ta_tenant_master AS t LEFT JOIN ta_user_management AS u ON u.mobile_no = t.mobile_no LEFT JOIN ta_order_details AS d ON d.tenant_id = t.id WHERE d.id = " + OrderDetailsId + "";
                HO_Name = DBHelper.ExecuteQueryReturnObject(query).ToString();
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return HO_Name;
        }

        public DataSet GetImportErrorData(string Unique_id, string ip_address, string name)
        {

            DataSet ds = new DataSet();
            try
            {

                ds = DBHelper.ExecuteStoredProcedureWithJsonInput(Unique_id, ip_address, name);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }


        //THIS FUNCTION USE FOR CHECK SETTING VALUE AGAINST TENAT UISNG MOBILE NO
        public string CheckSettingMaster(string mobileNumber, string settingName)
        {
            DataSet ds = new DataSet();
            int tenant_id = 0;
            int role_id = 0;
            string value = "";
            Dictionary<string, string> Param = new Dictionary<string, string>();
            try
            {
                string queryRole = "SELECT role_id,tenant_id FROM `ta_user_management` WHERE mobile_no='" + mobileNumber + "'";
                ds = DBHelper.ExecuteQueryReturnDS(queryRole);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    role_id = ds.Tables[0].Rows[0]["role_id"] != DBNull.Value ? Convert.ToInt32(ds.Tables[0].Rows[0]["role_id"]) : 0;
                    tenant_id = ds.Tables[0].Rows[0]["tenant_id"] != DBNull.Value ? Convert.ToInt32(ds.Tables[0].Rows[0]["tenant_id"]) : 0;
                }

                if (role_id == 5)
                {
                    string queryVendor = "SELECT tenant_id FROM `ta_vendor_master` WHERE mobile_no='" + mobileNumber + "'";
                    tenant_id = Convert.ToInt32(DBHelper.ExecuteQueryReturnObj(queryVendor));
                }

                string query = "SELECT COALESCE(NULLIF(tssm.value, ''), tsm.default_value) AS `value` FROM `ta_setting_master` AS `tsm` LEFT JOIN `ta_tenant_wise_setting_master` AS `tssm` ON tsm.id = tssm.setting_id AND tssm.tenant_id = '" + tenant_id + "' AND tssm.is_active=1 WHERE tsm.setting_name = '" + settingName + "'";

                value = DBHelper.ExecuteQueryReturnObj(query).ToString();

            }
            catch (Exception ex)
            {
                //var logger = NLog.LogManager.GetCurrentClassLogger();
                //logger.Error(ex.Message);
            }
            return value;
        }

        //THIS FUNCTION USE FOR CHECK SETTING VALUE AGAINST TENANT USING TENANT NO
        public string GetSettingValue(string TenantID, string settingName)
        {
            string value = "";
            Dictionary<string, string> Param = new Dictionary<string, string>();
            try
            {
                string query = "SELECT COALESCE(NULLIF(tssm.value, ''), tsm.default_value) AS `value` FROM `ta_setting_master` AS `tsm` LEFT JOIN `ta_tenant_wise_setting_master` AS `tssm` ON tsm.id = tssm.setting_id AND tssm.tenant_id = '" + TenantID + "' AND tssm.is_active=1 WHERE tsm.setting_name = '" + settingName + "'";

                value = DBHelper.ExecuteQueryReturnObj(query).ToString();

            }
            catch (Exception ex)
            {
                //var logger = NLog.LogManager.GetCurrentClassLogger();
                //logger.Error(ex.Message);
            }
            return value;
        }
    }
}
