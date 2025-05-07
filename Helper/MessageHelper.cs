using K4os.Hash.xxHash;
using NLog;
using RestSharp;
using System.Net;
using System.Net.Mail;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace Helper
{
	public class MessageHelper
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		//USED TO SEND TEST SMS TO THE CUSTOMER
		public static string SendTextSmsCSPL(string mobile_no, string msg, string template_id = "", string APIurl = "")
		{
			string response = "";
			try
			{
				string user = "DevelopmentTeam";
				string pass = "74@CU$dt";
				string receipientno = mobile_no;
				string senderID = "CSPLHO";
				string msgtxt = msg;
				string url;

				url = APIurl +"?username=" + HttpUtility.UrlEncode(user) + "&pass=" + HttpUtility.UrlEncode(pass) + "&senderid=" + HttpUtility.UrlEncode(senderID) + "&dest_mobileno=" + HttpUtility.UrlEncode(receipientno) + "&message=" + HttpUtility.UrlEncode(msgtxt) + "&response=Y";

				using (WebClient client = new WebClient())
				{
					response = client.DownloadString(url);
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return response;
		}

		//THIS IS USED FOR PINACLE SEND SMS
        public static string DynamicTextMessageSend(string mobile_no, string msg,string APIurl="")
        {
            string response = "";
            try
            {
				string url = APIurl.Replace("CUSTMESSAGE", HttpUtility.UrlEncode(msg)).Replace("CUSTNUMBER", HttpUtility.UrlEncode(mobile_no));

                using (WebClient client = new WebClient())
                {
                    response = client.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return response;
        }

        public static string SendTextSmsTechneAI(string mobile_no, string msg, string template_id = "", string APIurl = "")
        {
            string response = "";
            try
            {
                string user = "DevelopmentTeam";
                string pass = "74@CU$dt";
                string receipientno = mobile_no;
                string senderID = "CSPLHO";
                string msgtxt = msg;
                string url;

                url = APIurl + "?username=" + HttpUtility.UrlEncode(user) + "&pass=" + HttpUtility.UrlEncode(pass) + "&senderid=" + HttpUtility.UrlEncode(senderID) + "&dest_mobileno=" + HttpUtility.UrlEncode(receipientno) + "&message=" + HttpUtility.UrlEncode(msgtxt) + "&response=Y";

                using (WebClient client = new WebClient())
                {
                    response = client.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return response;
        }

        // USED TO SEND MAIL TO THE CUSTOMER EMAIL ADDRESS
        public void SendEmail(string EmailAddress, string message)
		{
			try
			{
				// Get SMTP details from environment or configuration
				string senderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL");
				string senderPassword = Environment.GetEnvironmentVariable("SMTP_SENDER_PASSWORD");
				string smtpHost = "smtp.office365.com";
				int smtpPort = 587;

				// Create and configure the email message
				var mailMessage = new MailMessage(senderEmail, EmailAddress, "Customer Message", message);

				// Configure the SMTP client
				using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
				{
					smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
					smtpClient.EnableSsl = true;

					// Send the email
					smtpClient.Send(mailMessage);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, "Error sending email");
			}
		}

		//USED TO SEND WHATSAPP TEXT MESSAGE USING TECHNEAI PINNACLE
		public string SendWhatsappMessage(string MobileNo, string Template, string Placeholders = "")
		{
			string response = "";
			try
			{
				var client = new RestClient("https://api.pinbot.ai/v2/wamessage/sendMessage");
				var request = new RestRequest();
				request.Method = Method.Post;
				request.AddHeader("Content-Type", "application/json");
				request.AddHeader("apikey", "ba719e14-d208-11ee-b22d-92672d2d0c2d");
				request.RequestFormat = DataFormat.Json;
				var placeholderArray = Placeholders.Split(',');
				request.AddJsonBody(new
				{
					from = "917030110088", // Should be configurable
					to = "91" + MobileNo,
					type = "template",
					message = new
					{
						templateid = Template,
						//placeholders = new[] { Placeholders }

						placeholders = placeholderArray
					}
				});

				var result = client.Execute(request);
				if (result.IsSuccessful)
				{
					response = result.Content;
				}
				else
				{
					logger.Error($"Failed to send WhatsApp message: {result.Content}");
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, "Error sending WhatsApp message");
			}
			return response;
		}

		//USED TO SEND DOCUMENT IN WHATSAPP USING TECHNEAI PINNACLE
		public string SendPdfWhatsappMessage(string MobileNo, string Template, string UrlPath, string FileName)
		{
			string response = "";
			try
			{
				var client = new RestClient("https://api.pinbot.ai/v2/wamessage/sendMessage");
				var request = new RestRequest();
				request.Method = Method.Post;
				request.AddHeader("Content-Type", "application/json");
				request.AddHeader("apikey", "ba719e14-d208-11ee-b22d-92672d2d0c2d");
				request.RequestFormat = DataFormat.Json;
				request.AddJsonBody(new
				{
					from = "917030110088", // Should be configurable
					to = "91" + MobileNo,
					type = "template",
					message = new
					{
						templateid = Template,
						url = UrlPath,
						filename = FileName
					}
				});

				var result = client.Execute(request);
				if (result.IsSuccessful)
				{
					response = result.Content;
				}
				else
				{
					logger.Error($"Failed to send WhatsApp message: {result.Content}");
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, "Error sending WhatsApp message");
			}
			return response;
		}
	}
}

