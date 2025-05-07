using CustomerOrderManagement.Helper;
using Helper;
using Microsoft.AspNetCore.Http;
using System.Data;
using BAL;


namespace BAL
{
	public class LoginBAL
	{
		MessageHelper MsgBal = new MessageHelper();
		private readonly MySqlService DBHelper; 
		private readonly GlobalSessionBAL GlobalBal;
        // Constructor injection for MySqlService
        public LoginBAL(MySqlService mySqlService, GlobalSessionBAL globalBal)
		{
			DBHelper = mySqlService;
            GlobalBal = globalBal;
        }

		//THIS FUNCTION USED FOR USER DETAILS SAVE
		public void saveUserDetails(IFormCollection FormData)
		{
			Dictionary<string, string> Param = new Dictionary<string, string>();
			Param.Add("tenant_id", FormData["TenantName"]);
			Param.Add("f_name", FormData["firstName"]);
			Param.Add("l_name", FormData["lastName"]);
			Param.Add("email_id", FormData["EmailNameId"]);
			Param.Add("mobile_no", FormData["mobileName"]);
			Param.Add("role_id", FormData["RoleName"]);
			Param.Add("branch_id", FormData["BranchName"]);
			Param.Add("username", FormData["Password"]);
			Param.Add("password", FormData["Password"]);
			Param.Add("remark", "");
			Param.Add("is_active", "1");
			Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd"));
			Param.Add("created_by", "1");
			Param.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd"));
			Param.Add("updated_by", "1");
			DBHelper.ExecuteInsertQuery("ta_user_master", Param);
		}

		//THIS FUNCTION USED FOR CHECK USER EXIST OR NOT
		public DataSet CheckUserExistOrNot(string MobileNo)
		{
			DataSet Ds = new DataSet();
			try
			{
				string Query = "SELECT id,mobile_no,role_id,tenant_id,is_active FROM ta_user_management WHERE is_active = 1 AND mobile_no ='" + MobileNo + "'";
				Ds = DBHelper.ExecuteQueryReturnDS(Query);
			}
			catch (Exception ex)
			{

			}
			return Ds;
		}
		public void SendOTPOnNumber(string mobileNumber, string OTP)
		{
			Dictionary<string, string> Param = new Dictionary<string, string>();

			try
			{
				string Message = "";
                string sms_provider = GlobalBal.CheckSettingMaster(mobileNumber, "sms_provider");
				if(!string.IsNullOrEmpty(sms_provider))
				{
					if(sms_provider == "Pinnacle")
					{
                        string apiURl = GlobalBal.CheckSettingMaster(mobileNumber, "api_url");

						string GetMessage = GlobalBal.CheckSettingMaster(mobileNumber, "text_message");
						MessageHelper.DynamicTextMessageSend(mobileNumber, GetMessage.Replace("CUSTOTP", OTP), apiURl);
					}
					else if(sms_provider == "TechneAi")
					{
						Message = "Dear Customer, Your OTP is CUSTOTP . Thank You Chandukaka Saraf & Sons Pvt Ltd";
                        string apiURl = GlobalBal.CheckSettingMaster(mobileNumber, "api_url");

                        MessageHelper.SendTextSmsCSPL(mobileNumber, Message, "", apiURl);
                    }
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			//DBHelper.ExecuteInsertQuery("ta_user_master", Param);
		}

		// THIS FUNCTION IS USED TI VALIDATE THE USER
		public DataSet ValidateLoginUser(IFormCollection FormData)
		{
			DataSet ds = new DataSet();
			Dictionary<string, string> Param = new Dictionary<string, string>();
			try
			{
				string query = "SELECT * FROM ta_user_master WHERE username='" + FormData["username"] + "' AND PASSWORD='" + FormData["password"].ToString() + "' AND is_active = 1";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//var logger = NLog.LogManager.GetCurrentClassLogger();
				//logger.Error(ex.Message);
			}
			return ds;
		}

		
    }
}
