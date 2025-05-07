using CustomerOrderManagement.Helper;
using Helper;
using Microsoft.AspNetCore.Http;
using Mysqlx.Crud;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;

namespace BAL
{
	public class OrdersMasterBAL
	{
		MessageHelper Msg = new MessageHelper();
		private readonly MySqlService DBHelper;
		private readonly GlobalSessionBAL GlobalBal;
        private static readonly HttpClient client = new HttpClient();
        // Constructor injection for MySqlService
        public OrdersMasterBAL(MySqlService mySqlService, GlobalSessionBAL sessionHelper)
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

		//THIS FUNCTION USED FOR GET ALL BRANCH LIST
		public DataSet GetAllBranchData()
		{
			DataSet ds = new DataSet();
			try
			{
				//string UserId = _sessionBAL.GetSessionValue("UserId");
				string query = "";
				if (GlobalBal.GetSessionValue("RoleId") == "4")
				{
					query = "SELECT b.id,b.branch_name FROM ta_branch_master AS b JOIN ta_user_mapping AS u ON u.branch_id = b.id JOIN ta_user_management AS up ON up.mobile_no = u.mobile_no WHERE b.is_active=1 AND b.tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " and up.id = " + GlobalBal.GetSessionValue("UserId");
				}
				else
				{
					query = "SELECT b.id,b.branch_name FROM ta_branch_master AS b WHERE b.is_active=1 AND b.tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";
				}
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION USED FOR GET ALL ITEMS
		public DataSet GetAllItemsData()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT id,item_name FROM ta_item_master WHERE tenant_id =" + GlobalBal.GetSessionValue("TenantId") + " AND is_active=1 AND is_stone=0";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{

			}
			return ds;
		}

		//THIS FUNCTION USED FOR GET ALL PURITY DATA
		public DataSet GetAllPurityData()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT p.id,p.purity FROM ta_purity_master AS p WHERE p.tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND is_active=1 ";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION USED FOR SELECTED ITEM ID AGAINST ALL CATEGORY
		public DataSet GetAllCategoryItemId(string itemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT c.id,c.category_name FROM ta_category_master AS c JOIN ta_tenant_master AS t ON t.id = c.tenant_id WHERE item_id=" + itemId + " AND c.is_active = 1 AND t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION USED FOR SELECTED ITEM ID AGAINST ALL CATEGORY
		public DataSet GetProductNameId(string itemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT p.product_group_name FROM ta_item_master AS i JOIN ta_product_group_master AS p ON i.product_group_id = p.id JOIN ta_tenant_master AS t ON t.id = p.tenant_id WHERE i.id = " + itemId + " AND t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION ISED FOR GET ALL ACTIVE STONES
		public DataSet GetAllActiveStones()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT i.id,i.item_name FROM ta_item_master AS i JOIN ta_tenant_master AS t ON t.id = i.tenant_id WHERE i.is_stone=1 AND i.is_active =1 AND t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION ISED FOR GET ALL ACTIVE STONES COLOR
		public DataSet GetAllStoneColor()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT id,color_name FROM ta_stone_color_master WHERE is_active =1 ";
				query = "SELECT s.id,s.color_name FROM ta_stone_color_master AS s JOIN ta_tenant_master AS t ON t.id = s.tenant_id WHERE s.is_active =1 AND t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION ISED FOR GET ALL ACTIVE STONES COLOR
		public DataSet GetAllCustomerList()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT s.id,s.customer_name FROM ta_customer_master AS s JOIN ta_tenant_master AS t ON t.id = s.tenant_id WHERE s.is_active =1 AND t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}


		//THIS FUNCTION ISED FOR GET ALL ACTIVE STONES COLOR
		public DataSet GetAllCategoryData()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT s.id,s.category_name FROM ta_category_master AS s JOIN ta_tenant_master AS t ON t.id = s.tenant_id WHERE s.is_active =1 AND t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION USED FOR GET ALL ORDER DETAILS
		public DataSet GetOrderDetailsData(int OrderHeaderId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT h.id AS order_header_id,d.id AS order_details_id,d.order_series_no,i.item_name,t.tenant_name,c.category_name,d.pcs,d.design_code,p.purity as purity_name,d.net_wt,d.gross_wt,d.size FROM ta_order_header AS h JOIN ta_order_details d ON h.id = d.order_header_id JOIN ta_item_master AS i ON i.id = d.item_id JOIN ta_purity_master AS p ON p.id = d.purity_id JOIN ta_category_master AS c ON c.id = d.category_id JOIN ta_tenant_master AS t ON t.id = d.tenant_id WHERE h.id =" + OrderHeaderId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}
		//THIS FUNCTION USED FOR SAVE ORDER DETAILS
		public DataSet SaveOrderDetails(Dictionary<string, object> orderDetails)
		{
			DataSet ds = new DataSet();
			try
			{
				int OrderHeaderId = InsertIntoOrderInHeaderTable(orderDetails);
				int OrderDetailsId = InsertIntoOrderDetailsTable(orderDetails, OrderHeaderId);
				int StoneTableId = InsertOrderStoneDetails(orderDetails, OrderHeaderId, OrderDetailsId);
				if (orderDetails.ContainsKey("AttachmentData"))
				{
					InsertUploadImagesInTable(orderDetails, OrderHeaderId, OrderDetailsId);
				}
				ds = GetOrderDetailsData(OrderHeaderId);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION USED FOR ATTACHMENT IMAGE SAVE
		public void InsertUploadImagesInTable(Dictionary<string, object> orderDetails, int OrderHeaderId, int OrderDetailsId)
		{
			if (orderDetails.ContainsKey("AttachmentData"))
			{
				Dictionary<string, string> para = new Dictionary<string, string>();


				string value = orderDetails["AttachmentData"].ToString();
				// Deserialize JSON into a List of Dictionary<string, object>
				List<Dictionary<string, object>> AttachmentDatas = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(value);
				for (int i = 0; i < AttachmentDatas.Count; i++)
				{
					var AttachmentData = AttachmentDatas[i];

					string cleanedUrl = AttachmentData["base64"].ToString().Replace("data:image/png;base64,", "")
											.Replace("data:image/jpeg;base64,", "")
											.Replace("data:image/jpg;base64,", "");
					string imageName = AttachmentData["name"].ToString();
					string formattedDate = DateTime.Now.ToString("yyyy-MM-dd");

					// Define the base directory path inside wwwroot
					string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
					string baseDirectoryPath = Path.Combine(wwwRootPath, "FileStorage", "Item_Attachment");

					// Combine the directory path with the formatted date
					string directoryPath = Path.Combine(baseDirectoryPath, formattedDate);

					// Create directory if it doesn't exist
					if (!Directory.Exists(directoryPath))
					{
						Directory.CreateDirectory(directoryPath);
					}

					// Full path for the image
					string imgPath = Path.Combine(directoryPath, imageName);

					// Convert the cleaned base64 string to a byte array
					byte[] imageBytes = Convert.FromBase64String(cleanedUrl);

					// Save the image as a file in wwwroot/FileStorage/Item_Attachment/2024-12-02/
					File.WriteAllBytes(imgPath, imageBytes);

					// Get the relative path to the wwwroot folder, starting from wwwroot
					string relativePath = Path.Combine("FileStorage", "Item_Attachment", formattedDate, imageName).Replace("\\", "/");

					para.Add("order_header_id", OrderHeaderId.ToString());
					para.Add("order_detail_id", OrderDetailsId.ToString());
					para.Add("path", "~/" + relativePath);
					para.Add("remark", "ImageUploaded");
					para.Add("is_active", "1");

					// Insert into the database table (assuming DBHelper is correctly set up)
					DBHelper.ExecuteInsertQuery("ta_attachment_master", para);
					para.Clear();
				}
			}
		}
		//INSERT ORDER DETAILS IN ORDER HEADER TABLE
		public int InsertIntoOrderInHeaderTable(Dictionary<string, object> orderDetails)
		{
			int OrderHeaderId = 0;
			try
			{
				//orderDetails["orderDeliveryDate"].ToString()
				if (orderDetails["OrderHeaderId"].ToString() == "0")
				{
					Dictionary<string, string> Param = new Dictionary<string, string>();
					Param.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
					Param.Add("branch_id", orderDetails["Branch"].ToString());

					if (orderDetails.ContainsKey("PadmReference"))
					{
						Param.Add("order_no", orderDetails["OrderNo"].ToString());
						Param.Add("role_id", GlobalBal.GetSessionValue("RoleId"));
						Param.Add("order_date", orderDetails["OrderDate"].ToString());
					}
					else
					{
						Param.Add("order_no", "0");
						Param.Add("order_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
					}

					Param.Add("customer_id", orderDetails["CustName"].ToString());   //orderDetails["CustName"] != "0"? orderDetails["CustName"].ToString() :  "0";
					Param.Add("order_delivery_date", orderDetails["orderDeliveryDate"].ToString());
					Param.Add("remark", orderDetails["Remark"].ToString());
					Param.Add("is_active", "1");
					Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
					Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));

					OrderHeaderId = DBHelper.ExecuteInsertQuery("ta_order_header", Param);
				}
				else
				{
					OrderHeaderId = Convert.ToInt32(orderDetails["OrderHeaderId"].ToString());
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return OrderHeaderId;
		}

		//INSERT INTO ORDER DETAILS TABLE
		public int InsertIntoOrderDetailsTable(Dictionary<string, object> orderDetails, int OrderHeaderId)
		{
			int OrderDetailsId = 0;
			try
			{
				string query = "SELECT CASE WHEN COUNT(line_number) = 0 THEN 1 ELSE COUNT(line_number) + 1 END AS next_line_number FROM ta_order_details WHERE order_header_id = " + OrderHeaderId + "";
				string LineNumber = DBHelper.ExecuteQueryReturnObj(query).ToString();
				Dictionary<string, string> Param = new Dictionary<string, string>();
                string designCode = (orderDetails.ContainsKey("design_code") && !string.IsNullOrEmpty(orderDetails["design_code"].ToString()))
                ? orderDetails["design_code"].ToString(): " ";

                string size = (orderDetails.ContainsKey("Size") && !string.IsNullOrEmpty(orderDetails["Size"].ToString()))
                    ? orderDetails["Size"].ToString(): "0";

                Param.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
				Param.Add("order_header_id", OrderHeaderId.ToString());
				Param.Add("line_number", LineNumber.ToString());
				Param.Add("item_id", orderDetails["ItemId"].ToString());
				Param.Add("category_id", orderDetails["CategoryId"].ToString());
				Param.Add("purity_id", orderDetails["PurityId"].ToString());
				Param.Add("net_wt", orderDetails["NetWt"].ToString());
				Param.Add("gross_wt", orderDetails["GrossWt"].ToString());
                Param.Add("design_code", designCode);
                Param.Add("size", size);
                Param.Add("pcs", orderDetails["pieces"].ToString());
				Param.Add("reference_barcode", orderDetails["BarcodeReference"].ToString());
				if (GlobalBal.GetSessionValue("RoleId").ToString() == "3" || GlobalBal.GetSessionValue("RoleId").ToString() == "2")
				{
					if (orderDetails.ContainsKey("PadmReference"))
					{
						Param.Add("order_status", "0");
						Param.Add("order_series_no", orderDetails["OrderNo"] + "-" + LineNumber);
					}
					else
					{
						Param.Add("order_status", "2");
					}
				}
				else
				{
					Param.Add("order_status", "0");
				}

				Param.Add("assign_to_vendor_id", "0");
				//DateTime orderDeliveryDate = DateTime.Parse(orderDetails["orderDeliveryDate"].ToString());
				//Param.Add("expected_delivery_date", "orderDeliveryDate.ToString("yyyy-MM-dd HH:mm:ss")");
				Param.Add("assign_to_sub_vendor_id", "0");
				Param.Add("remark", orderDetails["Remark"].ToString());
				Param.Add("is_active", "1");
				Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));

				OrderDetailsId = DBHelper.ExecuteInsertQuery("ta_order_details", Param);

				//OrderDetailsHistory("ta_order_details", OrderDetailsId, "A");
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return OrderDetailsId;
		}

		// THIS FUNCTION IS USED INSERT DATA INTO HISTORY TABLE 
		public void OrderDetailsHistory(string TableName, int Id, string Action)
		{
			try
			{
				string query = "";
				query = "SELECT id,tenant_id,order_header_id,order_series_no,line_number,challan_no,challan_pdf_path,item_id ,category_id,purity_id,net_wt,gross_wt,size,design_code,pcs,actual_item_gross_wt,actual_item_net_wt ,upload_preview_image_path,reference_barcode,order_status,assign_to_vendor_id,assign_to_sub_vendor_id,expected_delivery_date,remark,rejected_remark,is_active FROM " + TableName + " where id = " + Id;

				DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
				// Capture the necessary information for the history record
				Dictionary<string, string> HistoryParam = new Dictionary<string, string>();
				HistoryParam.Add("order_details_id ", Id.ToString());
				HistoryParam.Add("tenant_id", ds.Tables[0].Rows[0]["tenant_id"].ToString());
				HistoryParam.Add("order_header_id", ds.Tables[0].Rows[0]["order_header_id"].ToString());
				HistoryParam.Add("action_name", Action);
				HistoryParam.Add("ip_address", GlobalBal.GetClientIpAddress());
				HistoryParam.Add("challan_no", ds.Tables[0].Rows[0]["challan_no"].ToString());
				HistoryParam.Add("challan_pdf_path", ds.Tables[0].Rows[0]["challan_pdf_path"].ToString());
				//HistoryParam.Add("client_detail", ds.Tables[0].Rows[0]["order_header_id"].ToString());
				//HistoryParam.Add("order_no", ds.Tables[0].Rows[0]["tenant_id"].ToString());
				//HistoryParam.Add("order_line_number", ds.Tables[0].Rows[0]["tenant_id"].ToString());
				HistoryParam.Add("item_id", ds.Tables[0].Rows[0]["item_id"].ToString());
				HistoryParam.Add("category_id", ds.Tables[0].Rows[0]["category_id"].ToString());
				HistoryParam.Add("purity_id", ds.Tables[0].Rows[0]["purity_id"].ToString());
				HistoryParam.Add("net_wt", ds.Tables[0].Rows[0]["net_wt"].ToString());
				HistoryParam.Add("gross_wt", ds.Tables[0].Rows[0]["gross_wt"].ToString());
				HistoryParam.Add("size", ds.Tables[0].Rows[0]["size"].ToString());
				HistoryParam.Add("design_code", ds.Tables[0].Rows[0]["design_code"].ToString());
				HistoryParam.Add("pcs", ds.Tables[0].Rows[0]["pcs"].ToString());
				HistoryParam.Add("actual_item_gross_wt", ds.Tables[0].Rows[0]["actual_item_gross_wt"].ToString());
				HistoryParam.Add("actual_item_net_wt", ds.Tables[0].Rows[0]["actual_item_net_wt"].ToString());
				HistoryParam.Add("upload_preview_image_path", ds.Tables[0].Rows[0]["upload_preview_image_path"].ToString());
				HistoryParam.Add("reference_barcode", ds.Tables[0].Rows[0]["reference_barcode"].ToString());
				HistoryParam.Add("order_status", ds.Tables[0].Rows[0]["order_status"].ToString());
				HistoryParam.Add("assign_to_vendor_id", ds.Tables[0].Rows[0]["assign_to_vendor_id"].ToString());
				HistoryParam.Add("assign_to_sub_vendor_id", ds.Tables[0].Rows[0]["assign_to_sub_vendor_id"].ToString());
				HistoryParam.Add("expected_delivery_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				HistoryParam.Add("rejected_remark", ds.Tables[0].Rows[0]["rejected_remark"].ToString());
				HistoryParam.Add("remark", ds.Tables[0].Rows[0]["remark"].ToString());
				HistoryParam.Add("is_active", ds.Tables[0].Rows[0]["is_active"].ToString());
				HistoryParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				HistoryParam.Add("created_by", GlobalBal.GetSessionValue("UserId"));
				HistoryParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				HistoryParam.Add("updated_by", GlobalBal.GetSessionValue("UserId"));

				// Insert the history record into ta_item_master_history
				DBHelper.ExecuteInsertQuery(TableName + "_history", HistoryParam);
			}
			catch (Exception ex)
			{

			}
		}

		//THIS FUNCTION USED FOR INSERT STONE DETAILS IN STONE DETAILS TABLE
		public int InsertOrderStoneDetails(Dictionary<string, object> orderDetails, int OrderHeaderId, int OrderDetailsId)
		{
			int OrderStoneTableId = 0;
			try
			{

				if (orderDetails.ContainsKey("PadmReference"))
				{

				}
				else
				{
					// Access the value from the dictionary
					string value = orderDetails["StoneData"].ToString();
					// Deserialize JSON into a Dictionary<string, object>
					List<Dictionary<string, object>> stones = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(value);
					for (int i = 0; i < stones.Count; i++)
					{
						var stone = stones[i];
						string val = stone["type"].ToString().Split('~')[0];
						Dictionary<string, string> Param = new Dictionary<string, string>();
						Param.Add("order_header_id", OrderHeaderId.ToString());
						Param.Add("order_detail_item_id", OrderDetailsId.ToString());
						Param.Add("stone_id", stone["type"].ToString().Split('~')[0]);
						Param.Add("stone_category_id", stone["Stonecategory"].ToString().Split('~')[0]);
						Param.Add("stone_wt", stone["weight"].ToString());
						Param.Add("stone_pcs", stone["pieces"].ToString());
						Param.Add("stone_color_id", stone["color"].ToString().Split('~')[0]);
						Param.Add("remark", "");
						Param.Add("is_active", "1");
						Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
						Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));

						OrderStoneTableId = DBHelper.ExecuteInsertQuery("ta_order_stone_details", Param);
					}
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return OrderStoneTableId;
		}

		//THIS FUNCTION ISED FOR GET LAST ADDED DADA COPY TO USED
		public DataSet GetOrderlastAddedItemForUsed(string OrderHeaderId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT d.item_id,p.product_group_name,d.category_id,d.purity_id,d.net_wt,d.gross_wt,d.size,d.design_code,d.pcs,d.reference_barcode,d.remark FROM ta_order_details AS d JOIN ta_item_master AS t ON t.id = d.item_id JOIN ta_product_group_master AS p ON p.id = t.product_group_id WHERE d.order_header_id = " + OrderHeaderId + " ORDER BY d.id DESC LIMIT 1";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION USED FOR GENERATE ORDER NUMBER
		public void GenarateOrderNumber(IFormCollection Form)
		{
			string SeriesNumberReturn = "";
			try
			{
				Dictionary<string, string> TagParam = new Dictionary<string, string>();

				string Query = "UPDATE ta_order_header SET branch_id='" + Form["BranchName"] + "',role_id='" + Form["RoleId"] + "',order_date='" + Form["OrderDate"] + "',order_delivery_date='" + Form["DeliveryDate"] + "',customer_id='" + Form["CustomerNameId"] + "' WHERE id='" + Form["OrderHeader"] + "'";

				DBHelper.ExecuteQuery(Query);

				//THIS IS USED FOR UPDATE ORDER'S LINES
				UpdateOrderDetailsLines(Form["OrderHeader"], Form["CustomerNameId"], Form["BranchName"]);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		//THIS FUNCTION USED FOR GET ORDER DETAILS FOR EDIT
		public string UpdateOrderDetailsLines(string OrderHeaderId, string CustomerId = null, string BranchId = null)
		{
			string OrderSeriesNumber = "";
			string NextOrderNo = "";

			try
			{

				//THIS IS USED FOR GET PREFIX OF TENANT AGAINST
				string query = "SELECT prefix FROM ta_tenant_master WHERE id =" + GlobalBal.GetSessionValue("TenantId") + "";
				string GetPreFixTenant = DBHelper.ExecuteQueryReturnObj(query).ToString();

				//THIS IS USED FOR GENERATE NEXT ORDER NUMBER WITH PREFIX
				string Query = "SELECT CONCAT('" + GetPreFixTenant + "', '-', COALESCE(MAX(CAST(SUBSTRING_INDEX(order_no, '-', -1) AS UNSIGNED)) + 1, 1)) AS next_order_no FROM ta_order_header WHERE order_no LIKE CONCAT('" + GetPreFixTenant + "', '-%'); ";
				NextOrderNo = DBHelper.ExecuteQueryReturnObj(Query).ToString();

				string UpdateOrderNo = "UPDATE ta_order_header SET order_no = '" + NextOrderNo + "' WHERE id =" + OrderHeaderId + "";
				DBHelper.ExecuteQuery(UpdateOrderNo);

				//GET ORDER AGAINST CUSTOMER MOBILE NUMBER FOR SEND WHATS APP MESSAGE
				string WhatsQuery = "SELECT mobile_no,customer_name FROM ta_customer_master AS c JOIN ta_order_header AS h WHERE c.id = " + CustomerId + "";
				DataSet DS = DBHelper.ExecuteQueryReturnDS(WhatsQuery);

				//THIS IS USED FOR SEND WHATSAPP MESSAGE WHEN ORDER CREATES
				if (DS != null && DS.Tables[0].Rows.Count > 0)
				{
					Msg.SendWhatsappMessage(DS.Tables[0].Rows[0]["mobile_no"].ToString(), "670451", DS.Tables[0].Rows[0]["customer_name"].ToString() + "," + NextOrderNo + "");
				}

				string OrderLinesQuery = "SELECT line_number,id FROM ta_order_details WHERE order_header_id = " + OrderHeaderId + "";
				DataSet OrderSeq = DBHelper.ExecuteQueryReturnDS(OrderLinesQuery);

				if (OrderSeq != null && OrderSeq.Tables.Count > 0)
				{
					for (int i = 0; i < OrderSeq.Tables[0].Rows.Count; i++)
					{
						string Update = "UPDATE ta_order_details SET order_series_no = '" + NextOrderNo + "-" + OrderSeq.Tables[0].Rows[i]["line_number"].ToString() + "' WHERE id = " + OrderSeq.Tables[0].Rows[i]["id"].ToString() + "";
						DBHelper.ExecuteQuery(Update);

						//THIS IS USED FOR ADD IN HOSTORY TABLE
						OrderDetailsHistory("ta_order_details", Convert.ToInt32(OrderSeq.Tables[0].Rows[i]["id"].ToString()), "A");

						//THIS IS USED FOR SEND NOTIFICATIONS
						if (GlobalBal.GetSessionValue("RoleId") == "4")
						{
							string Query1 = "SELECT branch_name FROM ta_branch_master WHERE id = " + BranchId + "";
							string BranchName = DBHelper.ExecuteQueryReturnObj(Query1).ToString();
							GlobalBal.InsertNotificationMessage("Order " + NextOrderNo + "-" + OrderSeq.Tables[0].Rows[i]["line_number"].ToString() + " Submitted Successfully", "4");
							GlobalBal.InsertNotificationMessage("You have Received new Order " + NextOrderNo + "-" + OrderSeq.Tables[0].Rows[i]["line_number"].ToString() + " from " + BranchName + "", "3");
						}
						else
						{
							GlobalBal.InsertNotificationMessage("Order " + NextOrderNo + "-" + OrderSeq.Tables[0].Rows[i]["line_number"].ToString() + " Submitted Successfully", "3");
						}


					}
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return NextOrderNo;


		}

		//THIS FUNCTION USED FOR GET ORDER DETAILS FOR EDIT
		public DataSet GetOrderItemIdData(string OrderItemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT d.id,d.order_header_id AS header_id,h.order_no,d.order_series_no,h.branch_id,h.customer_id,c.customer_name,h.order_date,h.order_delivery_date,d.item_id,d.category_id,d.purity_id,d.gross_wt,d.net_wt,d.size,d.design_code,d.pcs,d.reference_barcode,d.remark,d.order_status FROM ta_order_details AS d JOIN ta_order_header AS h ON d.order_header_id = h.id JOIN ta_customer_master AS c ON c.id = h.customer_id WHERE d.id =" + OrderItemId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION USED FOR GET ORDER DETAILS FOR EDIT
		public DataSet GetDeliveryData(string OrderItemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT d.order_series_no,d.id,i.item_name,c.category_name,d.order_series_no,d.challan_no,d.pcs,d.actual_item_gross_wt,d.actual_item_net_wt,d.net_wt,d.gross_wt,t.tenant_name,t.address AS tenant_address,t.mobile_no AS tenat_mobile_no,t.gst_no AS tenant_gst,t.pancard_no AS tenant_pan_no,v.vendor_name,v.mobile_no AS vendor_mobile,v.residential_address AS vendor_address,v.gst_no AS vendor_gst,v.pancard_no AS vendor_pan_no FROM ta_order_details AS d LEFT JOIN ta_item_master AS i ON i.id = d.item_id LEFT JOIN ta_category_master AS c ON c.id =d.category_id LEFT JOIN ta_tenant_master AS t ON t.id = d.tenant_id LEFT JOIN `ta_user_management` AS u ON u.id = d.assign_to_vendor_id LEFT JOIN `ta_vendor_master` AS v ON v.mobile_no = u.mobile_no WHERE d.id IN (" + OrderItemId + ")";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION USED FOR GET ORDER DETAILS FOR EDIT
		public DataSet CustomerExistOrNot(string MobileNumber)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT id,customer_name FROM ta_customer_master WHERE mobile_no = '" + MobileNumber + "' and tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//UPDATE ORDER DETAILS
		public int UpdateOrderDetails(Dictionary<string, object> orderDetailsupdate)
		{
			DataSet ds = new DataSet();
			try
			{
				UpdateOrderInHeaderTable(orderDetailsupdate);
				UpdateOrderDetailsTable(orderDetailsupdate);
				int orderHeaderId = Convert.ToInt32(orderDetailsupdate["OrderHeaderId"].ToString());
				int OrderItemId = Convert.ToInt32(orderDetailsupdate["OrderItemId"].ToString());
				UpdateOrderStoneDetails(orderDetailsupdate, orderHeaderId, OrderItemId);
				if (orderDetailsupdate.ContainsKey("AttachmentData"))
				{
					string query = "DELETE FROM ta_attachment_master WHERE order_detail_id = " + OrderItemId + " AND order_header_id = " + orderHeaderId;
					DBHelper.ExecuteQuery(query);
					InsertUploadImagesInTable(orderDetailsupdate, orderHeaderId, OrderItemId);

				}
				string Query1 = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + OrderItemId + ")";
				string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query1).ToString();
				GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " updated successfully..", "3");
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
				return -1;
			}
			return 0;
		}

		//UPDATE ORDER DETAILS IN HEADER
		public int UpdateOrderInHeaderTable(Dictionary<string, object> orderDetails)
		{
			try
			{
				Dictionary<string, string> Param = new Dictionary<string, string>();
				Dictionary<string, string> WhereParam = new Dictionary<string, string>();
				Param.Add("order_delivery_date", orderDetails["orderDeliveryDate"].ToString());
				Param.Add("remark", orderDetails["Remark"].ToString());
				Param.Add("is_active", "1");
				Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));
				WhereParam.Add("id", orderDetails["OrderHeaderId"].ToString());

				DBHelper.ExecuteUpdateQuery("ta_order_header", Param, WhereParam);

			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
				return -1;
			}
			return 0;
		}

		//UPDATE ORDER DETAILS IN ORDER DETAILS TABLE
		public int UpdateOrderDetailsTable(Dictionary<string, object> orderDetails)
		{
			try
			{
				Dictionary<string, string> Param = new Dictionary<string, string>();
				Dictionary<string, string> WhereParam = new Dictionary<string, string>();
                string designCode = (orderDetails.ContainsKey("design_code") && !string.IsNullOrEmpty(orderDetails["design_code"].ToString()))
                ? orderDetails["design_code"].ToString() : " ";

                string size = (orderDetails.ContainsKey("Size") && !string.IsNullOrEmpty(orderDetails["Size"].ToString()))
                    ? orderDetails["Size"].ToString() : "0";

                Param.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
				Param.Add("item_id", orderDetails["ItemId"].ToString());
				Param.Add("category_id", orderDetails["CategoryId"].ToString());
				Param.Add("purity_id", orderDetails["PurityId"].ToString());
				Param.Add("net_wt", orderDetails["NetWt"].ToString());
				Param.Add("gross_wt", orderDetails["GrossWt"].ToString());
				Param.Add("size", size);
				Param.Add("design_code", designCode);
				Param.Add("pcs", orderDetails["pieces"].ToString());
				Param.Add("reference_barcode", orderDetails["BarcodeReference"].ToString());
				//if (GlobalBal.GetSessionValue("RoleId").ToString() == "3" || GlobalBal.GetSessionValue("RoleId").ToString() == "2")
				//{
				//	Param.Add("order_status", "2");
				//}
				//else
				//{
				//	Param.Add("order_status", "0");
				//}

				Param.Add("assign_to_vendor_id", "0");
				Param.Add("assign_to_sub_vendor_id", "0");
				Param.Add("remark", orderDetails["Remark"].ToString());
				Param.Add("is_active", "1");
				Param.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				Param.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
				WhereParam.Add("id", orderDetails["OrderItemId"].ToString());

				DBHelper.ExecuteUpdateQuery("ta_order_details", Param, WhereParam);

				OrderDetailsHistory("ta_order_details", Convert.ToInt32(orderDetails["OrderItemId"].ToString()), "U");
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
				return -1;
			}
			return 0;
		}

		//UPDATE STONE DETAILS
		public int UpdateOrderStoneDetails(Dictionary<string, object> orderDetails, int OrderHeaderId, int OrderDetailsId)
		{
			int OrderStoneTableId = 0;
			try
			{
				// Access the value from the dictionary
				string value = orderDetails["StoneData"].ToString();

				string query = "DELETE FROM ta_order_stone_details WHERE order_detail_item_id = " + OrderDetailsId + " AND order_header_id = " + OrderHeaderId;
				DBHelper.ExecuteQuery(query);
				// Deserialize JSON into a Dictionary<string, object>
				List<Dictionary<string, object>> stones = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(value);
				for (int i = 0; i < stones.Count; i++)
				{
					var stone = stones[i];
					string val = stone["type"].ToString().Split('~')[0];
					Dictionary<string, string> Param = new Dictionary<string, string>();
					Dictionary<string, string> WhereParam = new Dictionary<string, string>();
					Param.Add("order_header_id", OrderHeaderId.ToString());
					Param.Add("order_detail_item_id", OrderDetailsId.ToString());
					Param.Add("stone_id", stone["type"].ToString().Split('~')[0]);
					Param.Add("stone_wt", stone["weight"].ToString());
					Param.Add("stone_pcs", stone["pieces"].ToString());
					Param.Add("stone_color_id", stone["color"].ToString().Split('~')[0]);
                    Param.Add("stone_category_id", stone["stoneCategory"].ToString().Split('~')[0]);
                    Param.Add("remark", "");
					Param.Add("is_active", "1");
					Param.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
					Param.Add("updated_by", GlobalBal.GetSessionValue("UserId"));
					//if (stone["DataID"].ToString() == "0")
					//{

					DBHelper.ExecuteInsertQuery("ta_order_stone_details", Param);
					
					Param.Clear();
					WhereParam.Clear();
				}

			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return OrderStoneTableId;
		}

		//get order stone data
		public DataSet GetOrderStoneData(string OrderItemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT  sd.id,im.item_name,im.id AS `ItemID`,COALESCE(sm.color_name,'NA') AS `color_name`,sd.stone_category_id,sd.stone_color_id AS `ColorID`,sd.stone_wt,sd.stone_pcs,sd.remark,sd.stone_category_id,c.category_name,sd.is_active FROM ta_order_stone_details  AS sd LEFT JOIN ta_stone_color_master AS sm ON sd.stone_color_id = sm.id JOIN ta_item_master AS im ON im.id=sd.stone_id LEFT JOIN ta_category_master AS c ON c.id =sd.stone_category_id WHERE order_detail_item_id =" + OrderItemId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//get order attachement
		public DataSet GetOrderAttachmentData(string OrderItemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT path FROM ta_attachment_master WHERE order_detail_id =" + OrderItemId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}


		//THIS IS USED FOR ORDER STATUS FOR HO
		public DataSet OrderSendVenodrsList()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT u.id,v.vendor_name FROM ta_vendor_master AS v JOIN ta_user_management AS u ON u.mobile_no = v.mobile_no join ta_tenant_master as t on t.id = v.tenant_id where t.id = " + GlobalBal.GetSessionValue("TenantId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION USED FOR SAVE ORDER DETAILS
		public string SendToOrderPreview(Dictionary<string, object> orderDetails)
		{
			string HO_Name = "";
			try
			{
				int OrderDetailsId = Convert.ToInt32(orderDetails["OrderHeaderId"].ToString());
				if (orderDetails.ContainsKey("AttachmentData"))
				{
					InsertUploadImagesInDetailsTable(orderDetails, OrderDetailsId);
				}

				UpdateOrderDetailsStatus(OrderDetailsId);
				HO_Name = GetHOName(OrderDetailsId.ToString());
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return HO_Name;
		}

		//order details status
		public void UpdateOrderDetailsStatus(int OrderItemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "UPDATE ta_order_details SET order_status = 8 WHERE id =" + OrderItemId + "";
				DBHelper.ExecuteQuery(query);

				OrderDetailsHistory("ta_order_details", OrderItemId, "U");
				string Query1 = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + OrderItemId + ")";
				string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query1).ToString();

				GlobalBal.InsertNotificationMessage("You have received Order " + OrderSeriesNumber + " for preview", "3");
				GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " sent to " + GlobalBal.GetTenantName(OrderItemId.ToString()) + " for preview", "5");
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		//THIS FUNCTION USED FOR ATTACHMENT IMAGE SAVE
		//THIS FUNCTION USED FOR ATTACHMENT IMAGE SAVE
		public void InsertUploadImagesInDetailsTable(Dictionary<string, object> orderDetails, int OrderDetailsId)
		{
			if (orderDetails.ContainsKey("AttachmentData"))
			{
				Dictionary<string, string> para = new Dictionary<string, string>();
				//string query = "DELETE FROM ta_attachment_master WHERE order_detail_id = " + OrderDetailsId + " AND order_header_id = " + OrderHeaderId;
				//DBHelper.ExecuteQuery(query);

				string value = orderDetails["AttachmentData"].ToString();
				// Deserialize JSON into a List of Dictionary<string, object>
				List<Dictionary<string, object>> AttachmentDatas = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(value);
				for (int i = 0; i < AttachmentDatas.Count; i++)
				{
					var AttachmentData = AttachmentDatas[i];

					string cleanedUrl = AttachmentData["base64"].ToString().Replace("data:image/png;base64,", "")
											.Replace("data:image/jpeg;base64,", "")
											.Replace("data:image/jpg;base64,", "");
					string imageName = AttachmentData["name"].ToString().Replace(" ", "_");
					string formattedDate = DateTime.Now.ToString("yyyy-MM-dd");

					// Define the base directory path inside wwwroot
					string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
					string baseDirectoryPath = Path.Combine(wwwRootPath, "FileStorage", "Preview_Attachment");

					// Combine the directory path with the formatted date
					string directoryPath = Path.Combine(baseDirectoryPath, formattedDate);

					// Create directory if it doesn't exist
					if (!Directory.Exists(directoryPath))
					{
						Directory.CreateDirectory(directoryPath);
					}

					// Full path for the image
					string imgPath = Path.Combine(directoryPath, imageName.Replace(" ", "-"));

					// Convert the cleaned base64 string to a byte array
					byte[] imageBytes = Convert.FromBase64String(cleanedUrl);

					// Save the image as a file in wwwroot/FileStorage/Item_Attachment/2024-12-02/
					File.WriteAllBytes(imgPath, imageBytes);

					// Get the relative path to the wwwroot folder, starting from wwwroot
					string relativePath = Path.Combine("FileStorage", "Preview_Attachment", formattedDate, imageName).Replace("\\", "/");

					Dictionary<string, string> WhereParam = new Dictionary<string, string>();
					para.Add("actual_item_gross_wt", orderDetails["GrossWt"].ToString());
					para.Add("actual_item_net_wt", orderDetails["NetWt"].ToString());
					para.Add("upload_preview_image_path", "~/" + relativePath);

					WhereParam.Add("id", OrderDetailsId.ToString());

					// Insert into the database table (assuming DBHelper is correctly set up)
					DBHelper.ExecuteUpdateQuery("ta_order_details", para, WhereParam);
					para.Clear();
				}
			}
		}

        //public void OrderSendToHO(string OrderDetailsId)
        //{
        //	try
        //	{
        //		string Query = "SELECT CONCAT('ChallanNo-', MAX(CAST(SUBSTRING_INDEX(challan_no, '-', -1) AS UNSIGNED)) + 1) AS next_challan_no FROM ta_order_details";
        //		string ChallanCount = DBHelper.ExecuteQueryReturnObj(Query).ToString();
        //		string query = "";
        //		query = "UPDATE ta_order_details SET order_status = 11,challan_no='" + ChallanCount + "' WHERE id =" + OrderDetailsId + "";
        //		DBHelper.ExecuteQuery(query);

        //		OrderDetailsHistory("ta_order_details", Convert.ToInt32(OrderDetailsId), "U");
        //		string Query1 = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + OrderDetailsId + ")";
        //		string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query1).ToString();
        //		GlobalBal.InsertNotificationMessage("Your have received completed Order " + OrderSeriesNumber + " from vendor with Delivery Challan", "3");
        //		GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " Send to " + GlobalBal.GetTenantName(OrderDetailsId.ToString()) + " with Delivery Challan...", "5");
        //	}
        //	catch (Exception ex)
        //	{
        //		//logger.Error(ex.Message);
        //	}
        //}

        //THIS FUNCTION IS USED FOR RECEICED ORDER FROM VENDOR & SUBMIT WEIGHT DATA (ANIKET 21-02-2025)
        public void OrderSendToHO(string OrderDetailsId, string NetWt = "", string GrossWt = "")
        {
            try
            {
                string Query = "SELECT CONCAT('ChallanNo-', MAX(CAST(SUBSTRING_INDEX(challan_no, '-', -1) AS UNSIGNED)) + 1) AS next_challan_no FROM ta_order_details";
                string ChallanCount = DBHelper.ExecuteQueryReturnObj(Query).ToString();
                string query = "";
                string UpdateWt = "";

				//THIS IS USE FOR UPDATE ACTUAL NET & GROSS WT
                if (!string.IsNullOrEmpty(NetWt) && !string.IsNullOrEmpty(GrossWt))
                {
                    UpdateWt = "actual_item_gross_wt=" + GrossWt + ",actual_item_net_wt=" + NetWt + ",";
                }

                query = "UPDATE ta_order_details SET order_status = 11," + UpdateWt + "challan_no='" + ChallanCount + "' WHERE id =" + OrderDetailsId + "";
                DBHelper.ExecuteQuery(query);

                OrderDetailsHistory("ta_order_details", Convert.ToInt32(OrderDetailsId), "U");
                string Query1 = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + OrderDetailsId + ")";
                string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query1).ToString();
                GlobalBal.InsertNotificationMessage("Your have received completed Order " + OrderSeriesNumber + " from vendor with Delivery Challan", "3");
                GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " Send to " + GlobalBal.GetTenantName(OrderDetailsId.ToString()) + " with Delivery Challan...", "5");
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        //this function is used to get tenant ID
        public DataSet GettenantID()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT t.id,t.tenant_name FROM ta_tenant_master AS t JOIN ta_user_management AS u ON u.tenant_id = t.id WHERE u.id = " + GlobalBal.GetSessionValue("UserId") + "";
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
		public int SaveAddCustomer(IFormCollection form)
		{
			int Id = 0;
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
				TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId").ToString());
				Id = DBHelper.ExecuteInsertQuery("ta_customer_master", TagParam);
				AddHistory("ta_customer_master", Id, "A");

			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return Id;
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
				HistoryParam.Add("country_id", ds.Tables[0].Rows[0]["country_name"].ToString());
				HistoryParam.Add("state_id", ds.Tables[0].Rows[0]["state_name"].ToString());
				HistoryParam.Add("city_id", ds.Tables[0].Rows[0]["city_name"].ToString());
				HistoryParam.Add("action_name", Action);
				HistoryParam.Add("ip_address", GlobalBal.GetClientIpAddress());
				HistoryParam.Add("is_active", ds.Tables[0].Rows[0]["is_active"].ToString());
				HistoryParam.Add("remark", ds.Tables[0].Rows[0]["remark"].ToString());
				HistoryParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				HistoryParam.Add("created_by", GlobalBal.GetSessionValue("UserId").ToString());
				HistoryParam.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				HistoryParam.Add("updated_at_by", GlobalBal.GetSessionValue("UserId").ToString());
				// Insert the history record into ta_item_master_history
				DBHelper.ExecuteInsertQuery(TableName + "_history", HistoryParam);
			}
			catch (Exception ex)
			{
			}
		}

		// THIS FUNCTION IS USED TO GET HISTORY DATA
		public DataSet GetHistoryData(string id)
		{
			DataSet ds = new DataSet();
			try
			{
				//string query = "SELECT t.tenant_name,h.action_name,h.ip_address,i.item_name,h.order_series_no,c.category_name,p.purity_name,h.gross_wt,h.net_wt,h.actual_item_gross_wt,h.actual_item_net_wt,s.status_name,v.vendor_name,h.created_at,h.expected_delivery_date,h.updated_at,h.remark,h.rejected_remark FROM ta_order_details_history AS h LEFT JOIN ta_tenant_master AS t ON t.id=h.tenant_id LEFT JOIN ta_item_master AS i ON i.id = h.item_id LEFT JOIN ta_category_master AS c ON c.id = h.category_id LEFT JOIN ta_purity_master AS p ON p.id = h.purity_id LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = h.order_status LEFT JOIN ta_vendor_master AS v ON v.id = h.assign_to_vendor_id WHERE h.order_details_id =" + id;

				string Query = "SELECT h.id, h.assign_to_vendor_id, COALESCE( v.vendor_name, (SELECT vendor_name FROM ta_vendor_master AS v LEFT JOIN ta_user_management AS u ON u.mobile_no = v.mobile_no WHERE u.id = h.assign_to_vendor_id LIMIT 1) ) AS vendor_name, t.tenant_name, h.action_name, h.ip_address, i.item_name, h.order_series_no, c.category_name, p.purity as purity_name, h.gross_wt, h.net_wt, h.actual_item_gross_wt, h.actual_item_net_wt, s.status_name, h.created_at, h.expected_delivery_date, h.updated_at, h.created_by, h.remark, h.rejected_remark, CASE WHEN u.mobile_no IN (SELECT mobile_no FROM ta_tenant_master) THEN (SELECT tenant_name FROM ta_tenant_master WHERE mobile_no = u.mobile_no LIMIT 1) WHEN u.mobile_no IN (SELECT mobile_no FROM ta_vendor_master) THEN (SELECT vendor_name FROM ta_vendor_master WHERE mobile_no = u.mobile_no LIMIT 1) WHEN u.mobile_no IN (SELECT mobile_no FROM ta_user_mapping) THEN (SELECT user_name FROM ta_user_mapping WHERE mobile_no = u.mobile_no LIMIT 1) ELSE 'Techne AI' END AS updated_name FROM ta_order_details_history AS h LEFT JOIN ta_tenant_master AS t ON t.id = h.tenant_id LEFT JOIN ta_item_master AS i ON i.id = h.item_id LEFT JOIN ta_category_master AS c ON c.id = h.category_id LEFT JOIN ta_purity_master AS p ON p.id = h.purity_id LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = h.order_status LEFT JOIN ta_user_management AS u ON u.id = h.updated_by LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no WHERE h.order_details_id = " + id + " ORDER BY h.created_at DESC";
				ds = DBHelper.ExecuteQueryReturnDS(Query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		public void UpdatePdfPath(string OrderDetailsId, string fileUrl)
		{
			try
			{
				string query = "";
				query = "UPDATE ta_order_details SET challan_pdf_path = '" + fileUrl + "' WHERE id IN (" + OrderDetailsId + ")";
				DBHelper.ExecuteQuery(query);

				string[] IdsArray = OrderDetailsId.ToString().Replace("on,", "").Split(',');
				for (int i = 0; i < IdsArray.Length; i++)
				{
					OrderDetailsHistory("ta_order_details", Convert.ToInt32(IdsArray[i]), "U");
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		public DataSet GetJobCardDetailsForSend(string OrderDetailsId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT d.id,d.order_series_no,h.order_no,h.order_date,h.order_delivery_date,d.gross_wt,pm.product_group_name,pum.purity,c.category_name,d.net_wt,d.gross_wt,h.remark,d.reference_barcode,'ABCNew123' AS NewBarcode,t.item_name,d.size,d.pcs,d.order_series_no,c.category_name,a.path FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS t ON t.id = d.item_id JOIN ta_category_master AS c ON c.id = d.category_id JOIN ta_product_group_master AS pm ON pm.id =t.product_group_id JOIN ta_purity_master AS pum ON pum.id =d.purity_id LEFT JOIN ta_attachment_master AS a ON a.order_detail_id = d.id WHERE d.id =" + OrderDetailsId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS IS USED FOR ORDER REJECTED BY HO WITH REMARK OR NARRATION
		public void HO_OrderRejected(IFormCollection Form)
		{
			try
			{
				string query = "";
				query = "UPDATE ta_order_details SET order_status=3,rejected_remark='" + Form["RejectedRemark"].ToString() + "' WHERE id IN (" + Form["SelectedOrderItemId"].ToString().Replace("on,", "") + ")";
				DBHelper.ExecuteQuery(query);

				string[] IdsArray = Form["SelectedOrderItemId"].ToString().Replace("on,", "").Split(',');
				for (int i = 0; i < IdsArray.Length; i++)
				{
					OrderDetailsHistory("ta_order_details", Convert.ToInt32(IdsArray[i]), "U");
					string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + IdsArray[i] + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();

					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is rejected by " + "+ GlobalBal.GetTenantName(IdsArray[i]) + " + "", "4");
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is rejected by Successfully..", "3");
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		public void VendorOrderRejected(IFormCollection Form)
		{
			try
			{
				string query = "";
				query = "UPDATE ta_order_details SET order_status=5,rejected_remark='" + Form["RejectedRemark"].ToString() + "' WHERE id IN (" + Form["SelectedOrderItemId"].ToString().Replace("on,", "") + ")";
				DBHelper.ExecuteQuery(query);

				string[] IdsArray = Form["SelectedOrderItemId"].ToString().Replace("on,", "").Split(',');
				for (int i = 0; i < IdsArray.Length; i++)
				{
					OrderDetailsHistory("ta_order_details", Convert.ToInt32(IdsArray[i]), "U");
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		public DataSet GetStonePrintData(string OrderDetailsId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT t.item_name AS `stone_name`,c.category_name AS stone_category_name,COALESCE(st.color_name, 'NA') AS `stone_color`,s.stone_wt,s.stone_pcs FROM ta_order_details AS d JOIN ta_order_stone_details AS s ON d.id = s.order_detail_item_id JOIN ta_item_master AS t ON t.id = s.stone_id LEFT JOIN ta_stone_color_master AS st ON st.id = s.stone_color_id LEFT JOIN ta_category_master AS c ON c.id=s.stone_category_id WHERE d.id =" + OrderDetailsId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		public void GetCustomerMobileNumber(string OrderDetailsId, string PdfPath)
		{
			string MobileNo = "";
			try
			{
				string query = "";
				query = "SELECT c.mobile_no FROM ta_order_details AS d JOIN ta_order_header AS h ON d.order_header_id = h.id JOIN ta_customer_master AS c ON c.id = h.customer_id WHERE d.id = " + OrderDetailsId + "";
				MobileNo = DBHelper.ExecuteQueryReturnObj(query).ToString();

                string Url_Ip = GlobalBal.CheckSettingMaster(MobileNo, "public_ip");

                Msg.SendPdfWhatsappMessage(MobileNo, "670317", Url_Ip + PdfPath, "JobCard.pdf");
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}

		}

		public DataSet GetJobCardDetails(string ItemDetailsId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT d.id AS order_details_id,h.order_no,h.order_date,d.reference_barcode,'NewBarcode123',d.order_series_no,i.item_name,c.category_name,d.size,d.design_code,d.remark,d.pcs,pr.product_group_name,s.stone_wt,s.stone_pcs FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id LEFT JOIN ta_order_stone_details AS s ON s.order_detail_item_id = d.id WHERE d.id =" + ItemDetailsId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//AND d.assign_to_vendor_id = 56
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS IS USED FOR GET JOB CARD IMAGES
		public DataSet GetJobCardDetailsImage(string ItemDetailsId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT path FROM ta_attachment_master WHERE order_detail_id =" + ItemDetailsId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//AND d.assign_to_vendor_id = 56
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS IS USED FOR ORDER ACCEPT AND REJECT PREVIEW
		public void OrderAcceptOrRejectInEditView(string ItemId, string Flag)
		{
			try
			{
				string query = "";

				if (Flag == "1")
				{
					query = "UPDATE ta_order_details SET order_status=2 WHERE id=" + ItemId + "";

					string Query1 = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + ItemId + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query1).ToString();
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " accepted successfully..", "3");
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + "Accepted by " + GlobalBal.GetTenantName(ItemId.ToString()) + "", "5");
				}
				else
				{
					query = "UPDATE ta_order_details SET order_status=3 WHERE id=" + ItemId + "";
					//THIS IS USED FOR ORDER ACCEPTED IN EDIT VIEW
					string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + ItemId + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();

					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is rejected by " + GlobalBal.GetTenantName(ItemId.ToString()) + "", "4");
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is rejected by Successfully..", "5");
				}
				DBHelper.ExecuteQuery(query);

				OrderDetailsHistory("ta_order_details", Convert.ToInt32(ItemId), "U");
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		//THIS FUNCTION USED FOR SAVE ORDER DETAILS
		public int OrderSendToBackOffice(Dictionary<string, object> orderDetails)
		{
			int OrderDetailsId = 0;
			try
			{
				OrderDetailsId = Convert.ToInt32(orderDetails["OrderHeaderId"].ToString());
				if (orderDetails.ContainsKey("AttachmentData"))
				{
					InsertUploadImagesInDetailsTable(orderDetails, OrderDetailsId);
				}

				OrderDetailsHistory("ta_order_details", OrderDetailsId, "U");
				//THIS IS USED FOR GENERATE CHALLAN NO
				OrderSendToHO(OrderDetailsId.ToString());
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return OrderDetailsId;
		}

		public DataSet GetAllNotifications(string Flag)
		{
			DataSet ds = new DataSet();
			try
			{
				if (Flag == "1")
				{
					string query = "";
					if (GlobalBal.GetSessionValue("TenantId").ToString() != "")
					{
						query = "SELECT notification FROM ta_notification_master WHERE flag =1 AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND role_id = " + GlobalBal.GetSessionValue("RoleId").ToString() + " ORDER BY created_at DESC ";
					}
					else
					{
						string GetTenant = "SELECT v.tenant_id FROM ta_vendor_master AS v JOIN ta_user_management AS u ON u.mobile_no = v.mobile_no WHERE u.id = " + GlobalBal.GetSessionValue("UserId").ToString() + "";
						string GetVendorTenantId = DBHelper.ExecuteQueryReturnObj(GetTenant).ToString();

						query = "SELECT notification FROM ta_notification_master WHERE flag =1 AND tenant_id = " + GetVendorTenantId + " AND role_id = " + GlobalBal.GetSessionValue("RoleId").ToString() + " ORDER BY created_at DESC";
					}

					ds = DBHelper.ExecuteQueryReturnDS(query);

				}
				else
				{
					string GetTenant = "SELECT v.tenant_id FROM ta_vendor_master AS v JOIN ta_user_management AS u ON u.mobile_no = v.mobile_no WHERE u.id = " + GlobalBal.GetSessionValue("UserId").ToString() + "";
					string GetVendorTenantId = DBHelper.ExecuteQueryReturnObj(GetTenant).ToString();
					if (GlobalBal.GetSessionValue("TenantId").ToString() != "")
					{
						string query1 = "UPDATE ta_notification_master SET flag = 0 WHERE tenant_id = " + GlobalBal.GetSessionValue("TenantId").ToString() + " AND role_id = " + GlobalBal.GetSessionValue("RoleId").ToString() + "";
						DBHelper.ExecuteQuery(query1);
					}
					else
					{
						string query1 = "UPDATE ta_notification_master SET flag = 0 WHERE tenant_id = " + GetVendorTenantId + " AND role_id = " + GlobalBal.GetSessionValue("RoleId").ToString() + "";
						DBHelper.ExecuteQuery(query1);
					}

					string query = "";
					string TenantId = GetVendorTenantId == "" ? GlobalBal.GetSessionValue("TenantId").ToString() : GetVendorTenantId;
					query = "SELECT notification FROM ta_notification_master WHERE flag = 1 AND tenant_id = " + TenantId + " AND role_id = " + GlobalBal.GetSessionValue("RoleId").ToString() + " ORDER BY created_at DESC";
					ds = DBHelper.ExecuteQueryReturnDS(query);
				}
			}
			catch (Exception ex)
			{
				//AND d.assign_to_vendor_id = 56
				//logger.Error(ex.Message);
			}
			return ds;
		}

		public string ValidateIDs(string tablename, string fieldname, string value, string flagtenant = "0", Dictionary<string, string> flagextraField = null)
		{
			string obj = "";
			if (string.IsNullOrEmpty(value))
			{
				return obj;
			}
			try
			{
				string query = "SELECT id FROM " + tablename + " WHERE is_active = 1  " + " AND " + fieldname + " = '" + value + "'";
				if (flagtenant == "0")
				{
					query = query + " AND tenant_id = " + GlobalBal.GetSessionValue("TenantId");

				}

				if (flagextraField != null && flagextraField.Count > 0)
				{
					foreach (var kvp in flagextraField)
					{
						// Ensure both key and value are not null or empty before adding to query
						if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
						{
							query += " AND " + kvp.Key + " = '" + kvp.Value + "'";
						}
					}
				}

				DataSet ds = DBHelper.ExecuteQueryReturnDS(query);
				if (ds != null && ds.Tables[0].Rows.Count > 0)
				{
					obj = ds.Tables[0].Rows[0]["id"].ToString();
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return obj;
		}

		public int InsertIntoOrderInHeaderTable(DataTable orderHeader)
		{
			int OrderHeaderId = 0;

			try
			{
				// Ensure the DataTable is not empty
				if (orderHeader != null && orderHeader.Rows.Count > 0)
				{
					foreach (DataRow row in orderHeader.Rows)
					{
						Dictionary<string, string> Param = new Dictionary<string, string>();

						Param.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
						Param.Add("branch_id", row["branch_name"].ToString());
						Param.Add("order_no", "0");
						Param.Add("customer_id", row["customer_mobileno"].ToString());
						Param.Add("order_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
						Param.Add("order_delivery_date", row["order_delivery_date"] == DBNull.Value ? null : Convert.ToDateTime(row["order_delivery_date"]).ToString("yyyy-MM-dd"));
						//Param.Add("remark", row["Narration"].ToString());
						Param.Add("role_id", GlobalBal.GetSessionValue("RoleId"));
						Param.Add("is_active", "1");
						Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
						Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));

						// Insert a new order header and retrieve the OrderHeaderId
						OrderHeaderId = DBHelper.ExecuteInsertQuery("ta_order_header", Param);

					}
				}
			}
			catch (Exception ex)
			{
				// Log the error here
				//logger.Error(ex.Message);
			}

			return OrderHeaderId;
		}

		public Dictionary<string, string> InsertIntoOrderDetailsTable(DataTable orderDetailsTable, string OrderHeaderId)
		{
			string OrderDetailsId = "";
			Dictionary<string, string> OrderDetailsIdList = new Dictionary<string, string>();

			try
			{
				// Loop through each row in the DataTable
				foreach (DataRow row in orderDetailsTable.Rows)
				{
					// Generate a new line number for the history table (similar logic as for order details)
					string query = $"SELECT CASE WHEN COUNT(line_number) = 0 THEN 1 ELSE COUNT(line_number) + 1 END AS next_line_number FROM ta_order_details WHERE order_header_id = {OrderHeaderId}";
					string LineNumber = DBHelper.ExecuteQueryReturnObj(query).ToString();

                    string designCode = (!string.IsNullOrEmpty(row["design_code"].ToString())) ? row["design_code"].ToString() : " ";

                    string size = (!string.IsNullOrEmpty(row["size"].ToString())) ? row["size"].ToString() : "0";

                    // Create a dictionary for parameters to insert into the specified table
                    Dictionary<string, string> Param = new Dictionary<string, string>();
					Param.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
					Param.Add("order_header_id", OrderHeaderId);
					Param.Add("line_number", LineNumber);
					Param.Add("item_id", row["item_name"].ToString());
					Param.Add("category_id", row["category_name"].ToString());
					Param.Add("purity_id", row["purity"].ToString());
					Param.Add("net_wt", row["net_wt"].ToString());
					Param.Add("gross_wt", row["gross_wt"].ToString());
					Param.Add("size", size);
					Param.Add("design_code", designCode);
					Param.Add("pcs", row["pcs"].ToString());
					Param.Add("reference_barcode", row["reference_barcode"].ToString());
					Param.Add("order_status", "2");
					Param.Add("assign_to_vendor_id", "0");
					Param.Add("assign_to_sub_vendor_id", "0");
					Param.Add("remark", row["Narration"].ToString());
					Param.Add("is_active", "1");
					Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
					Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));

					// Insert into the specified table
					OrderDetailsId = DBHelper.ExecuteInsertQuery("ta_order_details", Param).ToString();

					// Record history action (you may choose to log each insert)
					//OrderDetailsHistory("ta_order_details", Convert.ToInt32(OrderDetailsId), "A");

					OrderDetailsIdList.Add(row["Item_Sequence"].ToString(), OrderDetailsId);
					Param.Clear();
					string[] ImagePaths = row["Image_path"]?.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

					if (ImagePaths.Length > 0)
					{
						foreach (string Imagepath in ImagePaths)
						{
							Param.Add("order_header_id", OrderHeaderId.ToString());
							Param.Add("order_detail_id", OrderDetailsId.ToString());
							Param.Add("path", Imagepath);
							Param.Add("remark", "ImageUploaded");
							Param.Add("is_active", "1");

							// Insert into the database table (assuming DBHelper is correctly set up)
							DBHelper.ExecuteInsertQuery("ta_attachment_master", Param);
							Param.Clear();
						}
					}

				}
			}
			catch (Exception ex)
			{
				// Handle the exception (log the error if necessary)
				//logger.Error(ex.Message);
			}

			// Return the total number of rows inserted
			return OrderDetailsIdList;
		}

		public void InsertOrderStoneDetailsTable(DataTable orderStonesTable, string OrderHeaderId, Dictionary<string, string> order_details_ids)
		{
			int OrderStoneTableId = 0;
			try
			{
				foreach (DataRow row in orderStonesTable.Rows)
				{
					double StoneWeigthInGram = StoreCaratConvertIntoGram(Convert.ToDouble(row["stone_wt"].ToString()), row["stone_name"].ToString());
					string orderDetailsid = order_details_ids.Where(kvp => kvp.Key == row["Item_Sequence"].ToString()).Select(kvp => kvp.Value).FirstOrDefault();
					Dictionary<string, string> Param = new Dictionary<string, string>();
					Param.Add("order_header_id", OrderHeaderId);
					Param.Add("order_detail_item_id", orderDetailsid);
					Param.Add("stone_id", row["stone_name"].ToString());
					Param.Add("stone_wt", StoneWeigthInGram.ToString());
					Param.Add("stone_pcs", row["stone_pcs"].ToString());
                    Param.Add("stone_color_id", string.IsNullOrEmpty(row["stone_color"]?.ToString()) ? "0" : row["stone_color"].ToString());
                    Param.Add("stone_category_id", row["stone_category"].ToString());
                    Param.Add("remark", "");
					Param.Add("is_active", "1");
					Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
					Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));

					OrderStoneTableId = DBHelper.ExecuteInsertQuery("ta_order_stone_details", Param);

				}

			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		//THIS IS USED FOR CARAT CONVERTED INTO GRAM
		public double StoreCaratConvertIntoGram(double StoneWt,string ItemId)
		{
			double ConvertedGram = 0;

			if(StoneWt > 0)
			{
				DataSet DS = GetItemUOM(ItemId);
				if (DS != null && DS.Tables.Count > 0 && DS.Tables[0].Rows.Count > 0 && DS.Tables[0].Columns.Contains("uom"))
				{
					if (DS.Tables[0].Rows[0]["uom"].ToString().ToUpper() == "CT")
					{
						ConvertedGram = StoneWt / 5;
					}
					else
					{
						ConvertedGram = StoneWt;
					}
				}
				
			}

			return ConvertedGram;
		}


		public string GenerateOrderNumberTable(DataTable orderHeaderTable)
		{
			string NextOrderNo = "";
			try
			{
				if (orderHeaderTable.Rows.Count == 0)
				{
					throw new Exception("Order details are missing.");
				}

				// Extract values from the first row of the DataTable (assuming there is only one order record)
				DataRow row = orderHeaderTable.Rows[0];

				// Assuming the DataTable has the following columns: OrderHeader, BranchName, RoleId, OrderDate, DeliveryDate, CustomerNameId
				string orderHeader = row["Order_Sequence"].ToString();
				string customerNameId = row["customer_mobileno"].ToString();


				// Update order details lines (assuming this is a method to handle order details)
				NextOrderNo = UpdateOrderDetailsLines(orderHeader, customerNameId);

			}
			catch (Exception ex)
			{
				// Log error or handle exception as needed
				//logger.Error(ex.Message);
			}
			return NextOrderNo;
		}

		//THIS FUNCTION ISED FOR GET LAST ADDED DADA COPY TO USED
		public DataSet GetOrderData(string OrderSeriesNo, string OrderNo)
		{
			DataSet ds = new DataSet();
			try
			{
				string apiurl = GlobalBal.GetSettingValue(GlobalBal.GetSessionValue("TenantId"), "base_url");
				string apifunction = GlobalBal.GetSettingValue(GlobalBal.GetSessionValue("TenantId"), "order_api");
                string URL = apiurl + apifunction;

                var requestData = new
                {
                    doc_series = OrderSeriesNo,
                    doc_no = OrderNo
                };

                string jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.PostAsync(URL, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = response.Content.ReadAsStringAsync().Result;

                        JObject jsonObject = JObject.Parse(jsonResponse);
                        JArray dataArray = (JArray)jsonObject["data"];

                        if (dataArray != null)
                        {
                            DataTable dt = JsonConvert.DeserializeObject<DataTable>(dataArray.ToString());
                            ds.Tables.Add(dt);
                        }
                        else
                        {
                            Console.WriteLine("Error: 'data' field is missing in API response.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("API Error: " + response.StatusCode);
                    }
                }

                //Dictionary<string, object> Params = new Dictionary<string, object>();
                //Params.Add("p_voucherseries", OrderSeriesNo);
                //Params.Add("p_voucherno", OrderNo);
                //ds = DBHelper.ExecuteStoredProcedureReturnDS("customerorder_getdata_oc", Params);
                //ds = DBHelper.ExecuteStoredProcedureReturnDS("customerorder_getdata", Params);
            }
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		public int InsertIntoCustomerMaster(DataSet OrderData)
		{
			int CustId = 0;
			try
			{
				Dictionary<string, string> TagParam = new Dictionary<string, string>();
				TagParam.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
				TagParam.Add("customer_name", OrderData.Tables[0].Rows[0]["CustomerName"].ToString());
				TagParam.Add("mobile_no", OrderData.Tables[0].Rows[0]["mobileNo"].ToString());
				//TagParam.Add("email_id", OrderData.Tables[0].Rows[0]["CustomerName"].ToString());
				TagParam.Add("address", OrderData.Tables[0].Rows[0]["cust_Address2"].ToString());
				TagParam.Add("area_description", OrderData.Tables[0].Rows[0]["cust_Address1"].ToString());
				TagParam.Add("pincode", OrderData.Tables[0].Rows[0]["pincode"].ToString());
				//TagParam.Add("country_id", OrderData.Tables[0].Rows[0]["CustomerName"].ToString());
				//TagParam.Add("state_id", OrderData.Tables[0].Rows[0]["CustomerName"].ToString());
				//TagParam.Add("city_id", OrderData.Tables[0].Rows[0]["CustomerName"].ToString());
				TagParam.Add("is_active", "1");
				//TagParam.Add("remark", OrderData.Tables[0].Rows[0]["CustomerName"].ToString());
				TagParam.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				TagParam.Add("created_by", GlobalBal.GetSessionValue("UserId").ToString());
				CustId = DBHelper.ExecuteInsertQuery("ta_customer_master", TagParam);
				AddHistory("ta_customer_master", CustId, "A");
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return CustId;
		}

		//THIS FUNCTION USED FOR CHECK ORDER EXIST OR NOT
		public int OrderNoExistOrNot(string OrderNo)
		{
			int Count = 0;
			try
			{
				string query = "";
				query = "SELECT COUNT(*) AS cnt FROM ta_order_header WHERE order_no = '" + OrderNo + "'" + " and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
				Count = Convert.ToInt32(DBHelper.ExecuteQueryReturnObject(query));
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return Count;
		}


		public string GetHOName(string OrderDetailsId)
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


		public void CreatePADMOrder(DataSet PadmOrderData)
		{
			int OrderHeaderId = 0;
			string OrderDetailsId = "0";
			for (int i = 0; i < PadmOrderData.Tables[0].Rows.Count; i++)
			{

				if (PadmOrderData.Tables[0].Rows[i]["EntryClubPosition"].ToString() == "1")
				{
					Dictionary<string, object> Param1 = new Dictionary<string, object>();
					DateTime deliveryDate = Convert.ToDateTime(PadmOrderData.Tables[0].Rows[i]["deliverydate"].ToString());
					DateTime OrderDate = Convert.ToDateTime(PadmOrderData.Tables[0].Rows[i]["DocumentDate"].ToString());

					Param1.Add("OrderNo", PadmOrderData.Tables[0].Rows[i]["orderNumber"].ToString());
					Param1.Add("Branch", ValidateIDs("ta_branch_master", "branch_name", PadmOrderData.Tables[0].Rows[i]["branchName"].ToString()));
					Param1.Add("CustName", ValidateIDs("ta_customer_master", "mobile_no", PadmOrderData.Tables[0].Rows[i]["mobileNo"].ToString()));
					Param1.Add("ItemId", ValidateIDs("ta_item_master", "item_name", PadmOrderData.Tables[0].Rows[i]["itemName"].ToString()));
					Param1.Add("CategoryId", ValidateIDs("ta_category_master", "category_name", PadmOrderData.Tables[0].Rows[i]["categoryName"].ToString()));
					Param1.Add("PurityId", ValidateIDs("ta_purity_master", "purity", PadmOrderData.Tables[0].Rows[i]["purity"].ToString()));
					Param1.Add("NetWt", PadmOrderData.Tables[0].Rows[i]["netWt"].ToString());
					Param1.Add("GrossWt", PadmOrderData.Tables[0].Rows[i]["grossWt"].ToString());
					Param1.Add("pieces", PadmOrderData.Tables[0].Rows[i]["pcs"]);
					Param1.Add("Size", PadmOrderData.Tables[0].Rows[i]["size"].ToString() == "" ? 0 : PadmOrderData.Tables[0].Rows[i]["size"]);
					Param1.Add("design_code", "");
					Param1.Add("OrderDate", Convert.ToString(OrderDate.ToString("yyyy-MM-dd")));
					Param1.Add("BarcodeReference", "0");
					Param1.Add("orderDeliveryDate", Convert.ToString(deliveryDate.ToString("yyyy-MM-dd")));
					Param1.Add("PadmReference", "1");
					Param1.Add("OrderHeaderId", OrderHeaderId.ToString());
					Param1.Add("Remark", "");
					Param1.Add("StoneData", "");

					OrderHeaderId = Convert.ToInt32(InsertIntoOrderInHeaderTable(Param1));
					OrderDetailsId = InsertIntoOrderDetailsTablePADM(Param1, OrderHeaderId.ToString());
				}

				if (PadmOrderData.Tables[0].Rows[i]["EntryClubPosition"].ToString() == "2")
				{
					Dictionary<string, string> Param = new Dictionary<string, string>();
					Param.Add("order_header_id", OrderHeaderId.ToString());
					Param.Add("order_detail_item_id", OrderDetailsId);
					Param.Add("stone_id", ValidateIDs("ta_item_master", "item_name", PadmOrderData.Tables[0].Rows[i]["itemName"].ToString()));
					Param.Add("stone_category_id", ValidateIDs("ta_category_master", "category_name", PadmOrderData.Tables[0].Rows[i]["categoryName"].ToString()));
					Param.Add("stone_wt", PadmOrderData.Tables[0].Rows[i]["grossWt"].ToString());
					Param.Add("stone_pcs", PadmOrderData.Tables[0].Rows[i]["pcs"].ToString());
					Param.Add("stone_color_id", "0");

					DBHelper.ExecuteInsertQuery("ta_order_stone_details", Param);
				}
				
			}

		}

		//INSERT INTO ORDER DETAILS TABLE
		public string InsertIntoOrderDetailsTablePADM(Dictionary<string, object> orderDetails, string OrderHeaderId)
		{
			string OrderDetailsId = "0";
			try
			{
				string query = "SELECT CASE WHEN COUNT(line_number) = 0 THEN 1 ELSE COUNT(line_number) + 1 END AS next_line_number FROM ta_order_details WHERE order_header_id = " + OrderHeaderId + "";
				string LineNumber = DBHelper.ExecuteQueryReturnObj(query).ToString();
				Dictionary<string, string> Param = new Dictionary<string, string>();
				Param.Add("tenant_id", GlobalBal.GetSessionValue("TenantId"));
				Param.Add("order_header_id", OrderHeaderId.ToString());
				Param.Add("line_number", LineNumber.ToString());
				Param.Add("item_id", orderDetails["ItemId"].ToString());
				Param.Add("category_id", orderDetails["CategoryId"].ToString());
				Param.Add("purity_id", orderDetails["PurityId"].ToString());
				Param.Add("net_wt", orderDetails["NetWt"].ToString());
				Param.Add("gross_wt", orderDetails["GrossWt"].ToString());
				Param.Add("size", orderDetails["Size"].ToString());
				Param.Add("design_code", orderDetails["design_code"].ToString());
				Param.Add("pcs", orderDetails["pieces"].ToString());
				Param.Add("reference_barcode", orderDetails["BarcodeReference"].ToString());
				Param.Add("order_status", "0");
				Param.Add("order_series_no", orderDetails["OrderNo"] + "-" + LineNumber);
				Param.Add("assign_to_vendor_id", "0");
				Param.Add("assign_to_sub_vendor_id", "0");
				Param.Add("remark", orderDetails["Remark"].ToString());
				Param.Add("is_active", "1");
				Param.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				Param.Add("created_by", GlobalBal.GetSessionValue("UserId"));

				OrderDetailsId = DBHelper.ExecuteInsertQuery("ta_order_details", Param).ToString();

				OrderDetailsHistory("ta_order_details", Convert.ToInt32(OrderDetailsId), "A");
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return OrderDetailsId;
		}

		//THIS FUNCTION USED FOR GET ITEM UOM
		public DataSet GetItemUOM(string ItemId)
		{
			DataSet dataSet = new DataSet();
			try
			{
				string query = "";
				query = "SELECT uom FROM ta_item_master WHERE  id='" + ItemId + "'" + " and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
				dataSet = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return dataSet;
		}

		//THIS FUNCTION USED FOR GET ITEM UOM
		public DataSet GetStoneCategory(string ItemId)
		{
			DataSet dataSet = new DataSet();
			try
			{
				string query = "";
				query = "SELECT id,category_name FROM ta_category_master WHERE item_id ='" + ItemId + "'" + " and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
				dataSet = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return dataSet;
		}

        //THIS FUNCTION USED FOR GET ITEM UOM
        public dynamic ItemIdAgainstCategoryIds(string ItemId,dynamic CategoryName)
        {
			dynamic Id = "";
            try
            {
                string query = "";
                query = "SELECT id FROM ta_category_master WHERE item_id ='" + ItemId + "'" + " and category_name='"+CategoryName+"' and tenant_id = " + GlobalBal.GetSessionValue("TenantId");
                Id = DBHelper.ExecuteQueryReturnObj(query).ToString();
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return Id;
        }
		
		
    }
}


