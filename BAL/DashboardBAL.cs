using CustomerOrderManagement.Helper;
using Helper;
using Microsoft.AspNetCore.Http;
using Mysqlx.Crud;
using System.Data;

namespace BAL
{
	public class DashboardBAL
	{
		MessageHelper MsgBal = new MessageHelper();
		private readonly MySqlService DBHelper;
		private readonly GlobalSessionBAL GlobalBal;

		// Constructor injection for MySqlService
		public DashboardBAL(MySqlService mySqlService, GlobalSessionBAL globalBal)
		{
			DBHelper = mySqlService;
			GlobalBal = globalBal ?? throw new ArgumentNullException(nameof(globalBal));
		}

		//######################################################## SALES PERSON FLOW #######################################################3
		public string SalesPersonDashboardCounts(string Flag)
		{
			string Count = "";
			try
			{
				string query = "";
				//THIS FLAG USED FOR SALES PERSON CREATED NEWLY ORDER COUNT
				if (Flag == "1")
				{
					query = "SELECT COUNT(*) AS COUNT FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id WHERE order_status=0 AND h.order_no != '0' AND d.created_by = " + GlobalBal.GetSessionValue("UserId") + "";
				}
				//THIS IS USED FOR BACK OFFICE REJECTED COUNT
				else if (Flag == "2")
				{
					query = "SELECT COUNT(*) AS COUNT FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id WHERE order_status=3 AND h.order_no != '0' AND d.created_by ='" + GlobalBal.GetSessionValue("UserId") + "'";
				}
				//THIS IS USED FOR ORDER COMPLETED COUNT
				else if (Flag == "3")
				{
					query = "SELECT COUNT(*) AS COUNT FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id WHERE d.order_status=12 AND h.order_no != '0' AND d.created_by ='" + GlobalBal.GetSessionValue("UserId") + "'";
				}
				//THIS IS USED FOR ORDER READY TO SEND COUNT
				else
				{
					query = "SELECT COUNT(*) FROM ta_order_details AS d JOIN ta_order_header AS h ON d.order_header_id = h.id WHERE d.order_series_no != '0' AND d.order_status =13 AND d.created_by ='" + GlobalBal.GetSessionValue("UserId") + "'";
				}
				Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
			}
			catch (Exception ex)
			{
			}
			return Count;
		}

		//THIS FUNCTION USED FOR ALL ORDERS DATA
		public DataSet AllOrderData(Dictionary<string, object> AllOrderPara = null)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				string queryFields = "";
				string queryJoin = "";

				if (GlobalBal.GetSessionValue("RoleId").ToString() == "4")
				{
					string Query1 = "SELECT up.branch_id FROM ta_user_management AS u JOIN ta_user_mapping AS up ON up.mobile_no = u.mobile_no WHERE u.id =" + GlobalBal.GetSessionValue("UserId").ToString();
					string BranchId = DBHelper.ExecuteQueryReturnObj(Query1).ToString();

					queryFields = " h.id AS order_header_id,d.order_series_no,d.id AS order_item_id,b.branch_name,d.order_status,s.sales AS STATUS,cu.customer_name,h.order_no,i.item_name,c.category_name,p.purity as purity_name,d.net_wt,t.tenant_name,d.gross_wt,d.pcs,pr.product_group_name,IFNULL(v.vendor_name, '') AS vendor_name,h.order_date,d.expected_delivery_date,h.order_delivery_date ";

					queryJoin = " FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_user_management AS u ON u.id = d.assign_to_vendor_id LEFT JOIN ta_customer_master AS cu ON cu.id = h.customer_id LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no AND v.tenant_id = 1 LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status LEFT JOIN ta_tenant_master AS t ON t.id = h.tenant_id WHERE h.order_no != '0' AND h.tenant_id = " + GlobalBal.GetSessionValue("TenantId").ToString() + " AND h.branch_id = " + BranchId + " ";
				}
				else
				{

					queryFields = " h.id AS order_header_id,d.id AS order_item_id,d.order_series_no,b.branch_name,d.order_status,s.ho AS STATUS,h.order_no,i.item_name,cu.customer_name,c.category_name,p.purity as purity_name,d.net_wt,t.tenant_name,d.gross_wt,d.pcs,pr.product_group_name,IFNULL(v.vendor_name, '') AS vendor_name,h.order_date,d.expected_delivery_date,h.order_delivery_date ";

					queryJoin = " FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_user_management AS u ON u.id = d.assign_to_vendor_id LEFT JOIN ta_customer_master AS cu ON cu.id = h.customer_id LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no AND v.tenant_id = 1 LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status LEFT JOIN ta_tenant_master AS t ON t.id = h.tenant_id WHERE h.id != '0' AND h.tenant_id = " + GlobalBal.GetSessionValue("TenantId").ToString() + " ";
				}

				if (AllOrderPara != null && AllOrderPara.Count > 0)
				{

					if (AllOrderPara.ContainsKey("CustomerName") && !string.IsNullOrEmpty(AllOrderPara["CustomerName"]?.ToString()))
					{
						queryJoin += " AND  cu.customer_name LIKE '%" + AllOrderPara["CustomerName"].ToString().Trim() + "%'";
					}

					if (AllOrderPara.ContainsKey("BranchName") && !string.IsNullOrEmpty(AllOrderPara["BranchName"]?.ToString()))
					{
						queryJoin += " AND b.branch_name LIKE '%" + AllOrderPara["BranchName"].ToString().Trim() + "%'";
					}

					if (AllOrderPara.ContainsKey("ItemName") && !string.IsNullOrEmpty(AllOrderPara["ItemName"]?.ToString()))
					{
						queryJoin += " AND i.item_name LIKE '%" + AllOrderPara["ItemName"].ToString().Trim() + "%'";
					}

					if (AllOrderPara.ContainsKey("OrderNo") && !string.IsNullOrEmpty(AllOrderPara["OrderNo"]?.ToString()))
					{
						queryJoin += " AND d.order_series_no LIKE '%" + AllOrderPara["OrderNo"].ToString().Trim() + "%'";
                    }

					if (AllOrderPara.ContainsKey("Category") && !string.IsNullOrEmpty(AllOrderPara["Category"]?.ToString()))
					{
						queryJoin += " AND c.category_name LIKE '%" + AllOrderPara["Category"].ToString().Trim() + "%'";
					}

					if (AllOrderPara.ContainsKey("VendorName") && !string.IsNullOrEmpty(AllOrderPara["VendorName"]?.ToString()))
					{
						queryJoin += " AND v.vendor_name LIKE '%" + AllOrderPara["VendorName"].ToString().Trim() + "%'";
					}

					if (AllOrderPara.ContainsKey("OrderDateFrom") && !string.IsNullOrEmpty(AllOrderPara["OrderDateFrom"]?.ToString()) &&
						AllOrderPara.ContainsKey("OrderDateTo") && !string.IsNullOrEmpty(AllOrderPara["OrderDateTo"]?.ToString()))
					{
						queryJoin += " AND h.order_date BETWEEN '" + AllOrderPara["OrderDateFrom"].ToString() + "' AND '" + AllOrderPara["OrderDateTo"].ToString() + "'";
					}

					if (AllOrderPara.ContainsKey("DeliveryDateFrom") && !string.IsNullOrEmpty(AllOrderPara["DeliveryDateFrom"]?.ToString()) &&
						AllOrderPara.ContainsKey("DeliveryDateTo") && !string.IsNullOrEmpty(AllOrderPara["DeliveryDateTo"]?.ToString()))
					{
						query += " AND h.order_delivery_date BETWEEN '" + AllOrderPara["DeliveryDateFrom"].ToString() + "' AND '" + AllOrderPara["DeliveryDateTo"].ToString() + "'";
					}

					queryJoin += " ORDER BY h.created_at DESC";
				}
				string baseQuery = "SELECT " + queryFields + queryJoin;
				string countQuery = "SELECT COUNT(*) AS TotalRows " + queryJoin;

				// Handle pagination
				if (AllOrderPara != null && AllOrderPara.TryGetValue("PageNumber", out var pageNumberObj) && AllOrderPara.TryGetValue("PageSize", out var pageSizeObj) &&
					int.TryParse(pageNumberObj.ToString(), out int pageNumber) && int.TryParse(pageSizeObj.ToString(), out int pageSize))
				{
					int offset = (pageNumber - 1) * pageSize;
					string paginationQuery = " LIMIT " + pageSize + " OFFSET " + offset;

					ds = DBHelper.ExecuteQueryReturnDS(baseQuery + paginationQuery);

					DataSet countTable = DBHelper.ExecuteQueryReturnDS(countQuery);

					// Add pagination info
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						DataTable paginationInfo = new DataTable();
						paginationInfo.Columns.Add("TotalRows", typeof(int));
						paginationInfo.Columns.Add("TotalPages", typeof(int));
						paginationInfo.Columns.Add("PageNumber", typeof(int));
						paginationInfo.Columns.Add("PageSize", typeof(int));

						int totalRows = Convert.ToInt32(countTable.Tables[0].Rows[0]["TotalRows"]);
						paginationInfo.Rows.Add(totalRows, (int)Math.Ceiling((double)totalRows / pageSize), pageNumber, pageSize);
						ds.Tables.Add(paginationInfo);
					}
				}
				else
				{
					ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		public DataSet AllOrdersData(Dictionary<string, object> AllOrderPara)
		{
			DataSet ds = new DataSet();
			try
			{
				// Get the TenantId from session once to avoid repetition
				string tenantId = GlobalBal.GetSessionValue("TenantId")?.ToString();

				if (string.IsNullOrEmpty(tenantId))
				{
					throw new Exception("TenantId is not available in session.");
				}

                string queryField = " h.id AS order_header_id,d.id AS order_item_id,b.branch_name,cu.customer_name,d.order_status,d.order_series_no,h.order_no,i.item_name,t.tenant_name,c.category_name,p.purity as purity_name,d.net_wt,d.gross_wt,d.pcs,pr.product_group_name,IFNULL(v.vendor_name, '') AS vendor_name,d.expected_delivery_date, h.order_date,h.order_delivery_date,s.ho AS status,IFNULL(um.user_name, 'Admin') AS sales_person";

                // Build the common part of the query
                string queryJoin = " FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master AS c ON c.id = d.category_id LEFT JOIN ta_customer_master AS cu ON cu.id = h.customer_id JOIN ta_purity_master AS p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_user_management AS u_created ON u_created.id = h.created_by LEFT JOIN ta_user_mapping AS um ON um.mobile_no = u_created.mobile_no LEFT JOIN ta_user_management AS u_vendor ON u_vendor.id = d.assign_to_vendor_id LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u_vendor.mobile_no AND v.tenant_id = " + GlobalBal.GetSessionValue("TenantId").ToString() + " LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status LEFT JOIN ta_tenant_master AS t ON t.id = h.tenant_id WHERE h.order_no != '0' AND h.tenant_id = " + GlobalBal.GetSessionValue("TenantId").ToString() + ""; // common part

                if (AllOrderPara.ContainsKey("CustomerName") && !string.IsNullOrEmpty(AllOrderPara["CustomerName"]?.ToString()))
				{
					queryJoin += " AND  cu.customer_name LIKE '%" + AllOrderPara["CustomerName"].ToString().Trim() + "%'";
				}

				if (AllOrderPara.ContainsKey("BranchName") && !string.IsNullOrEmpty(AllOrderPara["BranchName"]?.ToString()))
				{
					queryJoin += " AND b.branch_name LIKE '%" + AllOrderPara["BranchName"].ToString().Trim() + "%'";
				}

				if (AllOrderPara.ContainsKey("ItemName") && !string.IsNullOrEmpty(AllOrderPara["ItemName"]?.ToString()))
				{
					queryJoin += " AND i.item_name LIKE '%" + AllOrderPara["ItemName"].ToString().Trim() + "%'";
				}

				if (AllOrderPara.ContainsKey("OrderNo") && !string.IsNullOrEmpty(AllOrderPara["OrderNo"]?.ToString()))
				{
					queryJoin += " AND d.order_series_no LIKE '%" + AllOrderPara["OrderNo"].ToString().Trim() + "%'";
                }

				if (AllOrderPara.ContainsKey("Category") && !string.IsNullOrEmpty(AllOrderPara["Category"]?.ToString()))
				{
					queryJoin += " AND c.category_name LIKE '%" + AllOrderPara["Category"].ToString().Trim() + "%'";
				}

				if (AllOrderPara.ContainsKey("VendorName") && !string.IsNullOrEmpty(AllOrderPara["VendorName"]?.ToString()))
				{
					queryJoin += " AND v.vendor_name LIKE '%" + AllOrderPara["VendorName"].ToString().Trim() + "%'";
				}

				if (AllOrderPara.ContainsKey("OrderDateFrom") && !string.IsNullOrEmpty(AllOrderPara["OrderDateFrom"]?.ToString()) &&
					AllOrderPara.ContainsKey("OrderDateTo") && !string.IsNullOrEmpty(AllOrderPara["OrderDateTo"]?.ToString()))
				{
					queryJoin += " AND h.order_date BETWEEN '" + AllOrderPara["OrderDateFrom"].ToString() + "' AND '" + AllOrderPara["OrderDateTo"].ToString() + "'";
				}

				if (AllOrderPara.ContainsKey("DeliveryDateFrom") && !string.IsNullOrEmpty(AllOrderPara["DeliveryDateFrom"]?.ToString()) &&
					AllOrderPara.ContainsKey("DeliveryDateTo") && !string.IsNullOrEmpty(AllOrderPara["DeliveryDateTo"]?.ToString()))
				{
					queryJoin += " AND h.order_delivery_date BETWEEN '" + AllOrderPara["DeliveryDateFrom"].ToString() + "' AND '" + AllOrderPara["DeliveryDateTo"].ToString() + "'";
				}

				if (AllOrderPara.ContainsKey("Flag") && AllOrderPara["Flag"]?.ToString() == "1") // All Orders
				{
					queryJoin += " ORDER BY h.created_at DESC";
				}
				else if (AllOrderPara.ContainsKey("Flag") && AllOrderPara["Flag"]?.ToString() == "2") // Accepted Orders
				{
					queryJoin += " AND d.order_status = 2 " +
							 "ORDER BY h.created_at DESC";
				}
				else if (AllOrderPara.ContainsKey("Flag") && AllOrderPara["Flag"]?.ToString() == "4") // WIP Orders
				{
					queryJoin += " AND d.order_status IN (4,5,6,7,8,9,10,11)" +
							 "ORDER BY h.created_at DESC";

				}
				else if (AllOrderPara.ContainsKey("Flag") && AllOrderPara["Flag"]?.ToString() == "3") // Rejected Orders
				{
					queryJoin += " AND d.order_status = 3 " +
							 "ORDER BY h.created_at DESC";
				}

				string baseQuery = "SELECT " + queryField + queryJoin;
				string countQuery = "SELECT COUNT(*) AS TotalRows " + queryJoin;

				// Handle pagination
				if (AllOrderPara != null && AllOrderPara.TryGetValue("PageNumber", out var pageNumberObj) && AllOrderPara.TryGetValue("PageSize", out var pageSizeObj) &&
					int.TryParse(pageNumberObj.ToString(), out int pageNumber) && int.TryParse(pageSizeObj.ToString(), out int pageSize))
				{
					int offset = (pageNumber - 1) * pageSize;
					string paginationQuery = " LIMIT " + pageSize + " OFFSET " + offset;

					ds = DBHelper.ExecuteQueryReturnDS(baseQuery + paginationQuery);

					DataSet countTable = DBHelper.ExecuteQueryReturnDS(countQuery);

					// Add pagination info
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						DataTable paginationInfo = new DataTable();
						paginationInfo.Columns.Add("TotalRows", typeof(int));
						paginationInfo.Columns.Add("TotalPages", typeof(int));
						paginationInfo.Columns.Add("PageNumber", typeof(int));
						paginationInfo.Columns.Add("PageSize", typeof(int));

						int totalRows = Convert.ToInt32(countTable.Tables[0].Rows[0]["TotalRows"]);
						paginationInfo.Rows.Add(totalRows, (int)Math.Ceiling((double)totalRows / pageSize), pageNumber, pageSize);
						ds.Tables.Add(paginationInfo);
					}
				}
				else
				{
					ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
				}
			}
			catch (Exception ex)
			{
				// Log the error
				// logger.Error(ex.Message); 
				// Handle exception properly (e.g., rethrow, return empty dataset, etc.)
			}
			return ds;
		}

		//THIS IS USED FOP UPDATE STATUS FROM HO
		public void OrderStatusUpdateFromHO(string SelectedDataIds, string Status)
		{
			try
			{
				string query = "";

				if (Status == "2")
				{
					query = "UPDATE ta_order_details SET order_status = '2' WHERE id IN (" + SelectedDataIds + ") AND tenant_id = " + GlobalBal.GetSessionValue("TenantId") + "";

					DataSet Orders = DBHelper.ExecuteQueryReturnDS("SELECT order_series_no,id FROM ta_order_details WHERE id IN (" + SelectedDataIds.Replace("on,", "") + ")");

					if (Orders != null && Orders.Tables.Count > 0)
					{
						for (int i = 0; i < Orders.Tables[0].Rows.Count; i++)
						{
							GlobalBal.InsertNotificationMessage("Order " + Orders.Tables[0].Rows[i]["order_series_no"] + " is Accepted by " + GlobalBal.GetTenantName(Orders.Tables[0].Rows[i]["id"].ToString()) + "", "4");
							GlobalBal.InsertNotificationMessage("Order " + Orders.Tables[0].Rows[i]["order_series_no"] + " is Accepted successfully..", "3");
						}
					}
				}

				DBHelper.ExecuteQuery(query);
				string[] IdsArray = SelectedDataIds.ToString().Replace("on,", "").Split(',');
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

		//THIS FUNCTION USED FOR CHECK ORD ID DETAISL STATUS
		public DataSet OrderDetailsIdChecks(string SelectedDataIds)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT d.id,d.order_status FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id WHERE d.id IN(" + SelectedDataIds.Replace("on,", "") + ")";
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

        //THIS IS USED FOR ORDER STATUS FOR HO
        public DataSet FilterOrderSendVenodrsList(String SearchItem)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "";
                query = "SELECT u.id,v.vendor_name FROM ta_vendor_master AS v JOIN ta_user_management AS u ON u.mobile_no = v.mobile_no join ta_tenant_master as t on t.id = v.tenant_id where t.id =" + GlobalBal.GetSessionValue("TenantId") +
						" AND vendor_name LIKE '%" + SearchItem + "%'";
                ds = DBHelper.ExecuteQueryReturnDS(query);
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return ds;
        }


        //THIS FUNCTION USED FOR ALL ORDERS DATA
        public DataSet AllVendorOrders()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT h.id AS order_header_id,d.id AS order_item_id,s.vendor as status,d.order_status,h.order_no,i.item_name,cu.customer_name,d.order_series_no,c.category_name,t.tenant_name,p.purity as purity_name,d.net_wt,d.gross_wt,d.pcs,pr.product_group_name,IFNULL(v.vendor_name, '0') AS vendor_name,IFNULL(sv.sub_vendor_name, '') AS sub_vendor_name,d.expected_delivery_date,h.order_date,h.order_delivery_date FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id LEFT JOIN ta_customer_master AS cu ON cu.id = h.customer_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_vendor_master AS v ON v.id = d.assign_to_vendor_id LEFT JOIN ta_sub_vendor_master AS sv ON sv.id = d.assign_to_sub_vendor_id LEFT JOIN ta_tenant_master AS t ON t.id = h.tenant_id LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status WHERE h.order_no != '0' AND d.assign_to_vendor_id = '" + GlobalBal.GetSessionValue("UserId") + "' ORDER BY h.created_at DESC";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//###################################################################################################################################################################

		//THIS IS USED FOP UPDATE STATUS
		public void VendorOrderStatusUpdate(string SelectedDataIds, string Status)
		{
			try
			{
				string query = "";

				if (Status == "1")
				{
					query = "UPDATE ta_order_details SET order_status = '6' WHERE id IN (" + SelectedDataIds + ")";
					//THIS IS USED FOR VENDOR REJECTED ORDER REJECTED
					string[] AllIds = SelectedDataIds.Replace("on,", "").Split(',');
					for (int i = 0; i < AllIds.Length; i++)
					{
						string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + AllIds[i] + ")";
						string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();

						GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " Accepted Successfully..", "5");
						GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is Accepted by Vendor..", "3");
					}

				}
				else if (Status == "5")
				{
					query = "UPDATE ta_order_details SET order_status = '14' WHERE id IN (" + SelectedDataIds + ")";
					//THIS IS USED FOR VENDOR RECEIVED ORDER FROM SUB VENDOR
					string[] AllIds = SelectedDataIds.Replace("on,", "").Split(',');
					for (int i = 0; i < AllIds.Length; i++)
					{
						string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + AllIds[i] + ")";
						string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();

						GlobalBal.InsertNotificationMessage("Your have received completed Order " + OrderSeriesNumber + " from SubVendor", "5");
					}
				}
				else
				{
					query = "UPDATE ta_order_details SET order_status = '5' WHERE id IN (" + SelectedDataIds + ")";

					//THIS IS USED FOR VENDOR REJECTED ORDER REJECTED
					string[] AllIds = SelectedDataIds.Replace("on,", "").Split(',');
					for (int i = 0; i < AllIds.Length; i++)
					{
						string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + AllIds[i] + ")";
						string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();

						GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " rejected Successfully..", "5");
						GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is rejected by Vendor..", "3");
					}

				}
				DBHelper.ExecuteQuery(query);

				string[] IdsArray = SelectedDataIds.ToString().Replace("on,", "").Split(',');
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

		public DataSet AllVendorOrders(Dictionary<string, object> AllOrderPara = null)
		{
			DataSet ds = new DataSet();
			try
			{
				string flag = "";
				// Define the common SELECT fields
				string queryField = "h.id AS order_header_id, d.id AS order_item_id, d.order_status, h.order_no, cu.customer_name, d.order_series_no, i.item_name, c.category_name, t.tenant_name, p.purity as purity_name, d.net_wt, d.gross_wt, d.pcs, pr.product_group_name, IFNULL(v.vendor_name, '0') AS vendor_name, IFNULL(sv.sub_vendor_name, '') AS sub_vendor_name, h.order_date, h.order_delivery_date, d.expected_delivery_date, s.vendor AS STATUS";

				// Define the common JOINs
				string queryJoin = " FROM ta_order_header AS h " +
								   "JOIN ta_order_details AS d ON h.id = d.order_header_id " +
								   "JOIN ta_item_master AS i ON d.item_id = i.id " +
								   "JOIN ta_category_master AS c ON c.id = d.category_id " +
								   "LEFT JOIN ta_customer_master AS cu ON cu.id = h.customer_id " +
								   "JOIN ta_purity_master AS p ON p.id = d.purity_id " +
								   "JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id " +
								   "LEFT JOIN ta_vendor_master AS v ON v.id = d.assign_to_vendor_id " +
								   "LEFT JOIN ta_sub_vendor_master AS sv ON sv.id = d.assign_to_sub_vendor_id " +
								   "LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status " +
								   "LEFT JOIN ta_tenant_master AS t ON t.id = h.tenant_id " +
								   "WHERE h.order_no != '0' AND d.assign_to_vendor_id = '" + GlobalBal.GetSessionValue("UserId") + "'";

				// Apply filters based on parameters
				if (AllOrderPara != null && AllOrderPara.Count > 0)
				{
					if (AllOrderPara.TryGetValue("CustomerName", out var customerName) && !string.IsNullOrEmpty(customerName?.ToString()))
						queryJoin += " AND cu.customer_name LIKE '%" + customerName.ToString().Trim() + "%'";

					if (AllOrderPara.TryGetValue("BranchName", out var branchName) && !string.IsNullOrEmpty(branchName?.ToString()))
						queryJoin += " AND b.branch_name LIKE '%" + branchName.ToString().Trim() + "%'";

					if (AllOrderPara.TryGetValue("ItemName", out var itemName) && !string.IsNullOrEmpty(itemName?.ToString()))
						queryJoin += " AND i.item_name LIKE '%" + itemName.ToString().Trim() + "%'";

					if (AllOrderPara.TryGetValue("OrderNo", out var OrderNo) && !string.IsNullOrEmpty(OrderNo?.ToString()))
						queryJoin += " AND d.order_series_no LIKE '%" + OrderNo.ToString().Trim() + "%'";

                    if (AllOrderPara.TryGetValue("Category", out var category) && !string.IsNullOrEmpty(category?.ToString()))
						queryJoin += " AND c.category_name LIKE '%" + category.ToString().Trim() + "%'";

					if (AllOrderPara.TryGetValue("VendorName", out var vendorName) && !string.IsNullOrEmpty(vendorName?.ToString()))
						queryJoin += " AND v.vendor_name LIKE '%" + vendorName.ToString().Trim() + "%'";

					if (AllOrderPara.TryGetValue("OrderDateFrom", out var orderDateFrom) && AllOrderPara.TryGetValue("OrderDateTo", out var orderDateTo) &&
						!string.IsNullOrEmpty(orderDateFrom?.ToString()) && !string.IsNullOrEmpty(orderDateTo?.ToString()))
					{
						queryJoin += " AND h.order_date BETWEEN '" + orderDateFrom.ToString() + "' AND '" + orderDateTo.ToString() + "'";
					}

					if (AllOrderPara.TryGetValue("DeliveryDateFrom", out var deliveryDateFrom) && AllOrderPara.TryGetValue("DeliveryDateTo", out var deliveryDateTo) &&
						!string.IsNullOrEmpty(deliveryDateFrom?.ToString()) && !string.IsNullOrEmpty(deliveryDateTo?.ToString()))
					{
						queryJoin += " AND h.order_delivery_date BETWEEN '" + deliveryDateFrom.ToString() + "' AND '" + deliveryDateTo.ToString() + "'";
					}

					if (AllOrderPara.TryGetValue("Flag", out var Flag) && !string.IsNullOrEmpty(Flag?.ToString()))
					{
						flag = Flag.ToString();
					}

				}

				// Apply conditional filters based on Flag
				switch (flag)
				{
					case "1": // All Orders
						queryJoin += " AND d.order_status !=3  ORDER BY h.created_at DESC";
						break;
					case "2": // Accepted Orders
						queryJoin += " AND d.order_status = 6 ORDER BY h.created_at DESC";
						break;
					case "3": // Rejected Orders
						queryJoin += " AND d.order_status = 3 ORDER BY h.created_at DESC";
						break;
					case "4": // In Progress
						queryJoin += " AND d.order_status = 4 ORDER BY h.created_at DESC";
						break;
					case "5": // Completed Orders
						queryJoin += " AND d.order_status = 7 ORDER BY h.created_at DESC";
						break;
					case "6": // Pending Orders
						queryJoin += " AND d.order_status = 5 ORDER BY h.created_at DESC";
						break;
					default: // Default to rejected orders for invalid flag
						queryJoin += " AND d.order_status = 3 ORDER BY h.created_at DESC";
						break;
				}


				// Construct the final query
				string baseQuery = "SELECT " + queryField + queryJoin;
				string countQuery = "SELECT COUNT(*) AS TotalRows " + queryJoin;

				// Pagination logic
				if (AllOrderPara != null && AllOrderPara.TryGetValue("PageNumber", out var pageNumberObj) && AllOrderPara.TryGetValue("PageSize", out var pageSizeObj) &&
					 int.TryParse(pageNumberObj.ToString(), out int pageNumber) && int.TryParse(pageSizeObj.ToString(), out int pageSize))
				{
					int offset = (pageNumber - 1) * pageSize;
					string paginationQuery = " LIMIT " + pageSize + " OFFSET " + offset;

					ds = DBHelper.ExecuteQueryReturnDS(baseQuery + paginationQuery);

					DataSet countTable = DBHelper.ExecuteQueryReturnDS(countQuery);

					// Add pagination info
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						DataTable paginationInfo = new DataTable();
						paginationInfo.Columns.Add("TotalRows", typeof(int));
						paginationInfo.Columns.Add("TotalPages", typeof(int));
						paginationInfo.Columns.Add("PageNumber", typeof(int));
						paginationInfo.Columns.Add("PageSize", typeof(int));

						int totalRows = Convert.ToInt32(countTable.Tables[0].Rows[0]["TotalRows"]);
						paginationInfo.Rows.Add(totalRows, (int)Math.Ceiling((double)totalRows / pageSize), pageNumber, pageSize);
						ds.Tables.Add(paginationInfo);
					}
					else
					{
						// Execute the query
						ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
					}
				}
				else
				{
					// Execute the query
					ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
				}

			}
			catch (Exception ex)
			{
				// Log the error for debugging purposes
				// logger.Error(ex.Message);
			}
			return ds;
		}


		//THIS IS USED FOR ORDER STATUS FOR HO
		public DataSet OrderSendSubVenodrsList()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				//query = "SELECT v.id,v.sub_vendor_name FROM ta_sub_vendor_master AS v JOIN ta_tenant_master AS t ON t.id = v.tenant_id WHERE t.id =" + GlobalBal.GetSessionValue("TenantId") + "";
				query = "SELECT v.id,v.sub_vendor_name FROM ta_sub_vendor_master AS v WHERE v.karagir_id = " + GlobalBal.GetSessionValue("UserId") + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//ORDER SEND TO VENDOR
		public void OrderSendtoSubVendor(IFormCollection Form)
		{
			try
			{
				string query = "";
				query = "UPDATE ta_order_details SET order_status = '7',assign_to_sub_vendor_id=" + Form["SubVendorName"] + ",expected_delivery_date='" + Form["ExpectedDeliveryDate"] + "' WHERE id IN (" + Form["SelectedOrderItemId"] + ")";
				DBHelper.ExecuteQuery(query);

				string[] IdsArray = Form["SelectedOrderItemId"].ToString().Replace("on,", "").Split(',');
				for (int i = 0; i < IdsArray.Length; i++)
				{
					OrderDetailsHistory("ta_order_details", Convert.ToInt32(IdsArray[i]), "U");
					string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + IdsArray[i] + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();

					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " assigned to sub vendor", "5");
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		// THIS FUNCTION IS USED TO GET SUBSCRIPTION VALID DATE OF LAOGIN TENANT
		public string GetSubscriptionDetails()
		{
			string SubscriptionStatus = "";
			try
			{
				string query = "";
				query = "SELECT subscription_valid_till FROM `ta_tenant_master` WHERE id = " + GlobalBal.GetSessionValue("TenantId") + "";
				string SubscriptionDateGFromDb = DBHelper.ExecuteQueryReturnObject(query).ToString();

				DateTime subscriptionDate;
				bool isDateValid = DateTime.TryParse(SubscriptionDateGFromDb, out subscriptionDate);

				DateTime currentDate = DateTime.Today;
				if (subscriptionDate > currentDate)
				{
					SubscriptionStatus = "true";
				}
				else
				{
					SubscriptionStatus = "false";
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return SubscriptionStatus;
		}
		public DataSet GetOrderDetailsOnTheBasesPriority(Dictionary<string, object> data)
		{
			DataSet ds = new DataSet();

			string querypara = " h.id AS order_header_id, d.id AS order_item_id, b.branch_name, d.order_status, h.order_no, cu.customer_name, i.item_name, s.ho AS STATUS, d.order_series_no, c.category_name, p.purity as purity_name, d.net_wt, d.gross_wt, d.pcs, pr.product_group_name, IFNULL(v.vendor_name, '') AS vendor_name, h.order_date, h.order_delivery_date ";

			String queryJoin = " FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status LEFT JOIN ta_customer_master AS cu ON cu.id = h.customer_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_user_management AS u ON u.id = d.assign_to_vendor_id LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id WHERE h.order_no != '0' AND h.tenant_id = " + GlobalBal.GetSessionValue("TenantId").ToString() + " ";

			if (data != null && data.Count > 0)
			{

				if (data.ContainsKey("CustomerName") && !string.IsNullOrEmpty(data["CustomerName"]?.ToString()))
				{
					queryJoin += " AND  cu.customer_name LIKE '%" + data["CustomerName"].ToString().Trim() + "%'";
				}

				if (data.ContainsKey("ItemName") && !string.IsNullOrEmpty(data["ItemName"]?.ToString()))
				{
					queryJoin += " AND i.item_name LIKE '%" + data["ItemName"].ToString().Trim() + "%'";
				}

				if (data.ContainsKey("OrderNo") && !string.IsNullOrEmpty(data["OrderNo"]?.ToString()))
				{
                    queryJoin += " AND d.order_series_no LIKE '%" + data["OrderNo"].ToString().Trim() + "%'";
                }

				if (data.ContainsKey("Category") && !string.IsNullOrEmpty(data["Category"]?.ToString()))
				{
					queryJoin += " AND c.category_name LIKE '%" + data["Category"].ToString().Trim() + "%'";
				}

				if (data.ContainsKey("VendorName") && !string.IsNullOrEmpty(data["VendorName"]?.ToString()))
				{
					queryJoin += " AND v.vendor_name LIKE '%" + data["VendorName"].ToString().Trim() + "%'";
				}
				if (data.ContainsKey("BranchName") && !string.IsNullOrEmpty(data["BranchName"]?.ToString()))
				{
					queryJoin += " AND b.branch_name LIKE '%" + data["BranchName"].ToString().Trim() + "%'";
				}

				if (data.ContainsKey("OrderDateFrom") && !string.IsNullOrEmpty(data["OrderDateFrom"]?.ToString()) &&
					data.ContainsKey("OrderDateTo") && !string.IsNullOrEmpty(data["OrderDateTo"]?.ToString()))
				{
					queryJoin += " AND h.order_date BETWEEN '" + data["OrderDateFrom"].ToString() + "' AND '" + data["OrderDateTo"].ToString() + "'";
				}

				if (data.ContainsKey("DeliveryDateFrom") && !string.IsNullOrEmpty(data["DeliveryDateFrom"]?.ToString()) &&
					data.ContainsKey("DeliveryDateTo") && !string.IsNullOrEmpty(data["DeliveryDateTo"]?.ToString()))
				{
					queryJoin += " AND h.order_delivery_date BETWEEN '" + data["DeliveryDateFrom"].ToString() + "' AND '" + data["DeliveryDateTo"].ToString() + "'";
				}
			}

			string orderStatusCondition = string.Empty;
			if (data.ContainsKey("Flag"))
			{
				string Flag = data["Flag"]?.ToString();
				switch (Flag)
				{
					case "1":
						orderStatusCondition = "d.order_status = 0";
						break;
					case "2":
						orderStatusCondition = "d.order_status = 4";
						break;
					case "3":
						orderStatusCondition = "d.order_status = 6";
						break;
					case "4":
						orderStatusCondition = "d.order_status = 5";
						break;
					case "5":
						orderStatusCondition = "d.order_status IN (0, 1, 2)";
						break;
					case "6":
						orderStatusCondition = "d.order_status = 11 AND h.role_id = 4";
						break;

				}
			}

			queryJoin += " AND " + orderStatusCondition + " ORDER BY h.created_at DESC ";

			String paginationapplyQuery = "";

			string baseQuery = "SELECT " + querypara + queryJoin;

			// Count query
			string countQuery = "SELECT COUNT(*) AS TotalRows " + queryJoin;

			if (data != null && data.Count > 0)
			{
				if (data.ContainsKey("PageNumber") && !string.IsNullOrEmpty(data["PageNumber"]?.ToString()) && data.ContainsKey("PageSize") && !string.IsNullOrEmpty(data["PageSize"]?.ToString()))
				{
					int pageNumber = 0, pageSize = 0, offset = 0;
					pageNumber = Convert.ToInt32(data["PageNumber"].ToString());
					pageSize = Convert.ToInt32(data["PageSize"].ToString());
					offset = (pageNumber - 1) * pageSize;
					paginationapplyQuery = " LIMIT " + pageSize + " OFFSET " + offset;

					ds = DBHelper.ExecuteQueryReturnDS(baseQuery + paginationapplyQuery);

					DataSet countTable = DBHelper.ExecuteQueryReturnDS(countQuery);

					// Add pagination info
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						DataTable paginationInfo = new DataTable();
						paginationInfo.Columns.Add("TotalRows", typeof(int));
						paginationInfo.Columns.Add("TotalPages", typeof(int));
						paginationInfo.Columns.Add("PageNumber", typeof(int));
						paginationInfo.Columns.Add("PageSize", typeof(int));

						int totalRows = Convert.ToInt32(countTable.Tables[0].Rows[0]["TotalRows"]);

						paginationInfo.Rows.Add(totalRows, (int)Math.Ceiling((double)totalRows / pageSize), pageNumber, pageSize);
						ds.Tables.Add(paginationInfo);
					}
				}
				else
				{
					ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
				}

			}

			return ds;
		}

		//THIS IS USED FOR BACK OFFICE DASHBOARD COUNT SHOW
		public string BackOfficeDashboardCardCounts(string Flag)
		{
			string Count = "0";
			try
			{
				string query = "";

				if (GlobalBal.GetSessionValue("RoleId").ToString() != "1")
				{
					//THIS IS USED FOR ORDER GET NEW ORDER COUNT
					if (Flag == "1")
					{
						query = "SELECT COUNT(*) AS Cnt FROM ta_order_details AS d JOIN ta_order_header AS h ON d.order_header_id = h.id WHERE d.order_status IN (0,1,2) AND h.order_no != '0' AND h.tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
					}
					//THIS IS USED FOR ORDER ASSIGN VENDOR COUNT
					else if (Flag == "2")
					{
						query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=4 AND tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
					}
					//THIS IS USED FOR ORDER REJECTED VENDOR COUNT
					else if (Flag == "3")
					{
						query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=5 AND tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
					}
					//THIS IS USED FOR VENDOR ORDER SEND TO PREVIEW
					else if (Flag == "4")
					{
						query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=8 AND tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
					}
					//THIS IS USED FOR VENDOR ORDER ASSIGN TO HO
					else if (Flag == "5")
					{
						query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=11 AND tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
					}
					//HO PREVIEW REJECTED
					else if (Flag == "6")
					{
						query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=12 AND assign_to_vendor_id=" + GlobalBal.GetSessionValue("UserId") + "";
					}

					Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
				}

			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return Count;
		}

		//######################################################################################################################################
		//THIS IS USED FOR ORDER GET NEW ORDER COUNT
		//public string NewOrderCount()
		//{
		//	string Count = "0";
		//	try
		//	{
		//		string query = "";

		//		if (GlobalBal.GetSessionValue("RoleId").ToString() != "1")
		//		{
		//			query = "SELECT COUNT(*) AS Cnt FROM ta_order_details AS d JOIN ta_order_header AS h ON d.order_header_id = h.id WHERE d.order_status IN (0,1,2) AND h.order_no != '0' AND h.tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
		//			Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		//logger.Error(ex.Message);
		//	}
		//	return Count;
		//}

		////THIS IS USED FOR ORDER ASSIGN VENDOR COUNT
		//public string AssignOrderCount()
		//{
		//	string Count = "0";
		//	try
		//	{
		//		string query = "";
		//		if (GlobalBal.GetSessionValue("RoleId").ToString() != "1")
		//		{
		//			query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=4 AND tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
		//			Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		//logger.Error(ex.Message);
		//	}
		//	return Count;
		//}

		////THIS IS USED FOR ORDER REJECTED VENDOR COUNT
		//public string RejectedByVendorCount()
		//{
		//	string Count = "0";
		//	try
		//	{
		//		string query = "";
		//		if (GlobalBal.GetSessionValue("RoleId").ToString() != "1")
		//		{
		//			query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=5 AND tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
		//			Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		//logger.Error(ex.Message);
		//	}
		//	return Count;
		//}

		////THIS IS USED FOR ORDER REJECTED VENDOR COUNT
		//public string VendorOrderSendToPreview()
		//{
		//	string Count = "0";
		//	try
		//	{
		//		string query = "";
		//		if (GlobalBal.GetSessionValue("RoleId").ToString() != "1")
		//		{
		//			query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=8 AND tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
		//			Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		//logger.Error(ex.Message);
		//	}
		//	return Count;
		//}

		////THIS IS USED FOR ORDER REJECTED VENDOR COUNT
		//public string VendorOrderComplete()
		//{
		//	string Count = "0";
		//	try
		//	{
		//		string query = "";
		//		if (GlobalBal.GetSessionValue("RoleId").ToString() != "1")
		//		{
		//			query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=11 AND tenant_id=" + GlobalBal.GetSessionValue("TenantId") + "";
		//			Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		//logger.Error(ex.Message);
		//	}
		//	return Count;
		//}


		//THIS IS USED FOR VENDOR DASHBOARD CARD COUNTS
		public string VendorDashboardCardsCount(string Flag)
		{
			string Count = "0";
			try
			{
				string query = "";

				if (Flag == "1")
				{
					query = "SELECT COUNT(*) AS cnt FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id WHERE h.order_no != '0' AND d.order_status = 4 AND d.assign_to_vendor_id = '" + GlobalBal.GetSessionValue("UserId") + "'";
				}
				else if (Flag == "2")
				{
					query = "SELECT COUNT(*) AS cnt FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id WHERE h.order_no != '0' AND d.order_status = 7 AND d.assign_to_vendor_id = " + GlobalBal.GetSessionValue("UserId").ToString();
				}
				else if (Flag == "3")
				{
					query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=10 AND assign_to_vendor_id=" + GlobalBal.GetSessionValue("UserId") + "";
				}
				else if (Flag == "4")
				{
					query = "SELECT COUNT(*) AS cnt FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id WHERE h.order_no != '0' AND d.order_status = 14 AND d.assign_to_vendor_id = '" + GlobalBal.GetSessionValue("UserId") + "'";
				}
				else if (Flag == "5")
				{
					query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=8 AND assign_to_vendor_id=" + GlobalBal.GetSessionValue("UserId") + "";
				}
				else if (Flag == "6")
				{
					query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=11 AND assign_to_vendor_id=" + GlobalBal.GetSessionValue("UserId") + "";
				}

				Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
			}
			catch (Exception ex)
			{
			}
			return Count;
		}


		////THIS IS USED FOR ORDER REJECTED VENDOR COUNT
		//public string PreviewRejectedCount()
		//{
		//	string Count = "0";
		//	try
		//	{
		//		string query = "";
		//		if (GlobalBal.GetSessionValue("RoleId").ToString() != "1")
		//		{
		//			query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=10 AND assign_to_vendor_id=" + GlobalBal.GetSessionValue("UserId") + "";
		//			Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		//logger.Error(ex.Message);
		//	}
		//	return Count;
		//}

		////THIS IS USED FOR ORDER REJECTED VENDOR COUNT
		//public string OrderSendToHOPreview()
		//{
		//	string Count = "0";
		//	try
		//	{
		//		string query = "";
		//		if (GlobalBal.GetSessionValue("RoleId").ToString() != "1")
		//		{
		//			query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=8 AND assign_to_vendor_id=" + GlobalBal.GetSessionValue("UserId") + "";
		//			Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		//logger.Error(ex.Message);
		//	}
		//	return Count;
		//}


		////THIS IS USED FOR ORDER REJECTED VENDOR COUNT
		//public string OrderCompleteByVendor()
		//{
		//	string Count = "0";
		//	try
		//	{
		//		string query = "";
		//		if (GlobalBal.GetSessionValue("RoleId").ToString() != "1")
		//		{
		//			query = "SELECT COUNT(*) AS Cnt FROM ta_order_details WHERE order_status=11 AND assign_to_vendor_id=" + GlobalBal.GetSessionValue("UserId") + "";
		//			Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		//logger.Error(ex.Message);
		//	}
		//	return Count;
		//}


		//######################################################################################################################################

		//ORDER SEND TO VENDOR
		public void DirectOrderSendToVendor(string VendorId, string ExpectedDate, string Ids)
		{
			try
			{
				string query = "";
				query = "UPDATE ta_order_details SET order_status = '4',assign_to_vendor_id=" + VendorId + ",expected_delivery_date='" + ExpectedDate + "' WHERE id IN (" + Ids + ")";
				DBHelper.ExecuteQuery(query);

				string[] IdsArray = Ids.ToString().Replace("on,", "").Split(',');
				for (int i = 0; i < IdsArray.Length; i++)
				{
					OrderDetailsHistory("ta_order_details", Convert.ToInt32(IdsArray[i]), "U");
					string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + IdsArray[i] + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is assigned to Vendor..", "3");
					GlobalBal.InsertNotificationMessage("You have received new Order " + OrderSeriesNumber + " from " + GlobalBal.GetTenantName(IdsArray[i].ToString()) + "", "5");
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}


		public DataSet GetSalesPersonPriority(Dictionary<string, object> data)
		{
			DataSet ds = new DataSet();
			try
			{
				// Define the common SELECT fields
				string queryField = "h.id AS order_header_id, d.id AS order_item_id, b.branch_name, d.order_status, s.sales AS STATUS, " +
									"h.order_no, cu.customer_name, i.item_name, d.order_series_no, c.category_name, p.purity as purity_name, " +
									"d.net_wt, d.gross_wt, d.pcs, pr.product_group_name, IFNULL(v.vendor_name, '') AS vendor_name, " +
									"h.order_date, d.expected_delivery_date, h.order_delivery_date";

				// Define the common JOINs
				string queryJoin = " FROM ta_order_header AS h " +
								   "JOIN ta_order_details AS d ON h.id = d.order_header_id " +
								   "JOIN ta_item_master AS i ON d.item_id = i.id " +
								   "JOIN ta_category_master AS c ON c.id = d.category_id " +
								   "JOIN ta_purity_master AS p ON p.id = d.purity_id " +
								   "JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id " +
								   "LEFT JOIN ta_user_management AS u ON u.id = d.assign_to_vendor_id " +
								   "LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status " +
								   "LEFT JOIN ta_vendor_master AS v ON v.id = d.assign_to_vendor_id " +
								   "LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id " +
								   "LEFT JOIN ta_customer_master AS cu ON cu.id = h.customer_id " +
								   "WHERE h.order_no != '0' AND d.created_by = " + GlobalBal.GetSessionValue("UserId").ToString();

				// Apply filters based on Flag
				if (data.TryGetValue("Flag", out var flagObj) && !string.IsNullOrEmpty(flagObj?.ToString()))
				{
					string flag = flagObj.ToString();
					switch (flag)
					{
						case "1": // Pending Orders
							queryJoin += " AND d.order_status = 0";
							break;

						case "2": // Vendor Rejected
							queryJoin += " AND d.order_status = 3";
							break;

						case "3": // Vendor Sent to Preview
							queryJoin += " AND d.order_status = 12";
							break;

						default: // Default case for other statuses
							queryJoin += " AND d.order_status = 13";
							break;
					}
				}

				// Apply additional filters
				if (data.TryGetValue("BranchName", out var branchName) && !string.IsNullOrEmpty(branchName?.ToString()))
				{
					queryJoin += " AND b.branch_name LIKE '%" + branchName.ToString().Trim() + "%'";
				}

				if (data.TryGetValue("CustomerName", out var customerName) && !string.IsNullOrEmpty(customerName?.ToString()))
				{
					queryJoin += " AND cu.customer_name LIKE '%" + customerName.ToString().Trim() + "%'";
				}

				if (data.TryGetValue("OrderDateFrom", out var orderDateFrom) && data.TryGetValue("OrderDateTo", out var orderDateTo) &&
					!string.IsNullOrEmpty(orderDateFrom?.ToString()) && !string.IsNullOrEmpty(orderDateTo?.ToString()))
				{
					queryJoin += " AND h.order_date BETWEEN '" + orderDateFrom.ToString() + "' AND '" + orderDateTo.ToString() + "'";
				}

				if (data.TryGetValue("ExpectedDeliveryFrom", out var expectedDeliveryFrom) && data.TryGetValue("ExpectedDeliveryTo", out var expectedDeliveryTo) &&
					!string.IsNullOrEmpty(expectedDeliveryFrom?.ToString()) && !string.IsNullOrEmpty(expectedDeliveryTo?.ToString()))
				{
					queryJoin += " AND d.expected_delivery_date BETWEEN '" + expectedDeliveryFrom.ToString() + "' AND '" + expectedDeliveryTo.ToString() + "'";
				}

				if (data.TryGetValue("ItemName", out var itemName) && !string.IsNullOrEmpty(itemName?.ToString()))
				{
					queryJoin += " AND i.item_name LIKE '%" + itemName.ToString().Trim() + "%'";
				}

				if (data.TryGetValue("OrderNo", out var OrderNo) && !string.IsNullOrEmpty(OrderNo?.ToString()))
				{
					queryJoin += " AND d.order_series_no LIKE '%" + OrderNo.ToString().Trim() + "%'";
                }

				if (data.TryGetValue("Category", out var category) && !string.IsNullOrEmpty(category?.ToString()))
				{
					queryJoin += " AND c.category_name LIKE '%" + category.ToString().Trim() + "%'";
				}

				queryJoin += " ORDER BY h.created_at DESC";

				string baseQuery = "SELECT " + queryField + queryJoin;
				string countQuery = "SELECT COUNT(*) AS TotalRows " + queryJoin;

				// Handle pagination
				if (data != null && data.TryGetValue("PageNumber", out var pageNumberObj) && data.TryGetValue("PageSize", out var pageSizeObj) &&
					int.TryParse(pageNumberObj.ToString(), out int pageNumber) && int.TryParse(pageSizeObj.ToString(), out int pageSize))
				{
					int offset = (pageNumber - 1) * pageSize;
					string paginationQuery = " LIMIT " + pageSize + " OFFSET " + offset;

					ds = DBHelper.ExecuteQueryReturnDS(baseQuery + paginationQuery);

					DataSet countTable = DBHelper.ExecuteQueryReturnDS(countQuery);

					// Add pagination info
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						DataTable paginationInfo = new DataTable();
						paginationInfo.Columns.Add("TotalRows", typeof(int));
						paginationInfo.Columns.Add("TotalPages", typeof(int));
						paginationInfo.Columns.Add("PageNumber", typeof(int));
						paginationInfo.Columns.Add("PageSize", typeof(int));

						int totalRows = Convert.ToInt32(countTable.Tables[0].Rows[0]["TotalRows"]);
						paginationInfo.Rows.Add(totalRows, (int)Math.Ceiling((double)totalRows / pageSize), pageNumber, pageSize);
						ds.Tables.Add(paginationInfo);
					}
				}
				else
				{
					ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
				}
			}
			catch (Exception ex)
			{
				// Log the error for debugging purposes
				// logger.Error(ex.Message);
			}

			return ds;
		}

		//THIS IS USED FOR ASSIGN ORDER AFTER REJECT 
		public void AssignOrderAfterRejectedOrders(IFormCollection form/*string SelectedDataIds*/)
		{
			try
			{

				string query = "";
				query = "UPDATE ta_order_details SET order_status = '3',rejected_remark='" + form["RejectedRemark"] + "' WHERE id IN (" + form["SelectedOrderItemId"] + ")";
				DBHelper.ExecuteQuery(query);

				string[] IdsArray = form["SelectedOrderItemId"].ToString().Replace("on,", "").Split(',');
				for (int i = 0; i < IdsArray.Length; i++)
				{
					OrderDetailsHistory("ta_order_details", Convert.ToInt32(IdsArray[i]), "U");
					string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + IdsArray[i] + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is rejected successfully..", "3");
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " rejected by " + GlobalBal.GetTenantName(IdsArray[i].ToString()) + "", "4");
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}


		//##############################################################################################################################################
		//THIS IS USED FOR VENDOR RECEIVED ORDER COUNT
		public string VendorReceivedOrderCount()
		{
			string Count = "0";
			try
			{
				string query = "";
				query = "SELECT COUNT(*) AS cnt FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id WHERE h.order_no != '0' AND d.order_status = 4 AND d.assign_to_vendor_id = '" + GlobalBal.GetSessionValue("UserId") + "'";
				Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return Count;
		}

		//THIS IS USED FOR GET COUNT SUB VENDOR ORDERS
		public string AssignSubVendorOrderCount()
		{
			string Count = "0";
			try
			{
				string query = "";
				/*query = "SELECT COUNT(*) AS cnt FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id WHERE h.order_no != '0' AND d.order_status = 7 AND assign_to_sub_vendor_id != '0'";*/
				query = "SELECT COUNT(*) AS cnt FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id WHERE h.order_no != '0' AND d.order_status = 7 AND d.assign_to_vendor_id = " + GlobalBal.GetSessionValue("UserId").ToString();

				Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return Count;
		}

		//THIS IS USED FOR RECEIVED ORDER FROM SUB VENDOR COUNT
		public string ReceivedFromSubVendor()
		{
			string Count = "0";
			try
			{
				string query = "";
				query = "SELECT COUNT(*) AS cnt FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id WHERE h.order_no != '0' AND d.order_status = 14 AND d.assign_to_vendor_id = '" + GlobalBal.GetSessionValue("UserId") + "'";
				Count = DBHelper.ExecuteQueryReturnObj(query).ToString();
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return Count;
		}

		//THIS IS USED FOR GET STONE DETAILS FOR SHOW
		public DataSet GetOrderStoneData(string OrderItemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT  sd.id,im.item_name,im.id AS `ItemID`,sm.color_name,sm.id AS `ColorID`,sd.stone_wt,sd.stone_pcs,sd.remark,sd.is_active FROM ta_order_stone_details  AS sd JOIN ta_stone_color_master AS sm ON sd.stone_color_id =sm.id JOIN ta_item_master AS im ON im.id=sd.stone_id WHERE order_detail_item_id =" + OrderItemId + " AND is_stone=1";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS IS USE FOR ATTACHED IMAGES PATH
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

		//THIS FUNCTION ISED FOR GET ALL ACTIVE STONES COLOR
		public DataSet GetOrderIdDetails(string OrderDeatilsId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT h.id AS order_header_id, d.id AS order_item_id, b.branch_name, d.order_status, h.order_no,d.order_series_no, i.item_name, c.category_name, p.purity as purity_name, d.net_wt, d.gross_wt, d.pcs, d.size,pr.product_group_name,d.expected_delivery_date,h.order_date,d.order_series_no, h.order_delivery_date FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id WHERE d.order_status IN(6,7,9,10,14) AND h.order_no != '0' AND d.id = " + OrderDeatilsId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//AND d.assign_to_vendor_id = 56
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS FUNCTION ISED FOR GET ALL RECEIVED FOR PREVIEW
		public DataSet AllOrderReceivedForPreview()
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT h.id AS order_header_id, d.id AS order_item_id, b.branch_name,v.vendor_name, d.order_status,s.ho AS STATUS,h.order_no, i.item_name, c.category_name,p.purity as purity_name,d.actual_item_gross_wt,d.order_series_no,d.expected_delivery_date,d.actual_item_net_wt,d.upload_preview_image_path, d.net_wt, d.gross_wt, d.pcs, d.size,pr.product_group_name,h.order_date, h.order_delivery_date FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status LEFT JOIN ta_user_management AS u ON u.id = d.assign_to_vendor_id LEFT JOIN ta_vendor_master AS v ON u.mobile_no = v.mobile_no WHERE d.order_status = 8 AND h.tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " ORDER BY h.created_at DESC";

				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//AND d.assign_to_vendor_id = 56
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS IS USED FOR GET ORDER FOR PREVIEW
		public DataSet GetOrderForPreview(string OrderItemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT h.id AS order_header_id, d.id AS order_item_id,d.order_series_no, b.branch_name, d.order_status, h.order_no, i.item_name, c.category_name, p.purity as purity_name,d.actual_item_gross_wt,d.expected_delivery_date,d.actual_item_net_wt,d.upload_preview_image_path, d.net_wt, d.gross_wt, d.pcs, d.size,pr.product_group_name, h.order_date, h.order_delivery_date FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id WHERE d.order_status = 8 AND h.order_no != '0' AND d.id = " + OrderItemId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//AND d.assign_to_vendor_id = 56
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS IS USED FOR UPDATE STATUS ACCEPT PREVIEW AND REJECT PREVIEW
		public int OrderPreviewAcceptOrReject(string Flag, string ItemdetailsId)
		{
			int res = 0;
			try
			{
				string query = "";

				if (Flag == "1")
				{
					query = "UPDATE ta_order_details SET order_status = 9 WHERE id = " + ItemdetailsId + "";
					string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + ItemdetailsId + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " preview accepted successfully..", "3");
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " preview accepted by " + GlobalBal.GetTenantName(ItemdetailsId.ToString()) + "", "5");
				}
				else
				{
					query = "UPDATE ta_order_details SET order_status = 10 WHERE id = " + ItemdetailsId + "";
					string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + ItemdetailsId + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " preview rejected successfully..", "3");
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " preview rejected by " + GlobalBal.GetTenantName(ItemdetailsId.ToString()) + "", "5");
				}
				DBHelper.ExecuteQueryReturnDS(query);
				string[] IdsArray = ItemdetailsId.ToString().Replace("on,", "").Split(',');
				for (int i = 0; i < IdsArray.Length; i++)
				{
					OrderDetailsHistory("ta_order_details", Convert.ToInt32(IdsArray[i]), "U");
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return res;
		}

		//THIS IS USED FOR ALL ORDERS DATA WITH DELIVERY CHALLAN
		public DataSet AllOrderWithDeliveryChallan(Dictionary<string, object> data)
		{
			DataSet ds = new DataSet();
			try
			{

				string querypara = "SELECT h.id AS order_header_id,v.vendor_name, d.id AS order_item_id,d.order_series_no,s.ho AS STATUS, b.branch_name, d.order_status, h.order_no, i.item_name, c.category_name, p.purity as purity_name,d.actual_item_gross_wt,d.actual_item_net_wt,d.upload_preview_image_path, d.net_wt, d.gross_wt, d.pcs, d.size,pr.product_group_name, h.order_date, h.order_delivery_date ";

				String queryJoin = " FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status LEFT JOIN ta_user_management AS u ON u.id = d.assign_to_vendor_id LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no WHERE d.order_status = 11 AND h.order_no != '0' AND h.tenant_id =" + GlobalBal.GetSessionValue("TenantId");

				if (data != null && data.Count > 0)
				{

					if (data.ContainsKey("CustomerName") && !string.IsNullOrEmpty(data["CustomerName"]?.ToString()))
					{
						queryJoin += " AND  cu.customer_name LIKE '%" + data["CustomerName"].ToString().Trim() + "%'";
					}

					if (data.ContainsKey("ItemName") && !string.IsNullOrEmpty(data["ItemName"]?.ToString()))
					{
						queryJoin += " AND i.item_name LIKE '%" + data["ItemName"].ToString().Trim() + "%'";
					}

					if (data.ContainsKey("OrderNo") && !string.IsNullOrEmpty(data["OrderNo"]?.ToString()))
					{
						queryJoin += " AND d.order_series_no LIKE '%" + data["OrderNo"].ToString().Trim() + "%'";
                    }

					if (data.ContainsKey("Category") && !string.IsNullOrEmpty(data["Category"]?.ToString()))
					{
						queryJoin += " AND c.category_name LIKE '%" + data["Category"].ToString().Trim() + "%'";
					}

					if (data.ContainsKey("VendorName") && !string.IsNullOrEmpty(data["VendorName"]?.ToString()))
					{
						queryJoin += " AND v.vendor_name LIKE '%" + data["VendorName"].ToString().Trim() + "%'";
					}
					if (data.ContainsKey("BranchName") && !string.IsNullOrEmpty(data["BranchName"]?.ToString()))
					{
						queryJoin += " AND b.branch_name LIKE '%" + data["BranchName"].ToString().Trim() + "%'";
					}

					if (data.ContainsKey("OrderDateFrom") && !string.IsNullOrEmpty(data["OrderDateFrom"]?.ToString()) &&
						data.ContainsKey("OrderDateTo") && !string.IsNullOrEmpty(data["OrderDateTo"]?.ToString()))
					{
						queryJoin += " AND h.order_date BETWEEN '" + data["OrderDateFrom"].ToString() + "' AND '" + data["OrderDateTo"].ToString() + "'";
					}

					if (data.ContainsKey("DeliveryDateFrom") && !string.IsNullOrEmpty(data["DeliveryDateFrom"]?.ToString()) &&
						data.ContainsKey("DeliveryDateTo") && !string.IsNullOrEmpty(data["DeliveryDateTo"]?.ToString()))
					{
						queryJoin += " AND h.order_delivery_date BETWEEN '" + data["DeliveryDateFrom"].ToString() + "' AND '" + data["DeliveryDateTo"].ToString() + "'";
					}
				}

				queryJoin += "  ORDER BY h.created_at DESC ";

				String paginationapplyQuery = "";

				string baseQuery = querypara + queryJoin;

				// Count query
				string countQuery = "SELECT COUNT(*) AS TotalRows " + queryJoin;

				if (data != null && data.Count > 0)
				{
					if (data.ContainsKey("PageNumber") && !string.IsNullOrEmpty(data["PageNumber"]?.ToString()) && data.ContainsKey("PageSize") && !string.IsNullOrEmpty(data["PageSize"]?.ToString()))
					{
						int pageNumber = 0, pageSize = 0, offset = 0;
						pageNumber = Convert.ToInt32(data["PageNumber"].ToString());
						pageSize = Convert.ToInt32(data["PageSize"].ToString());
						offset = (pageNumber - 1) * pageSize;
						paginationapplyQuery = " LIMIT " + pageSize + " OFFSET " + offset;

						ds = DBHelper.ExecuteQueryReturnDS(baseQuery + paginationapplyQuery);

						DataSet countTable = DBHelper.ExecuteQueryReturnDS(countQuery);

						// Add pagination info
						if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
						{
							DataTable paginationInfo = new DataTable();
							paginationInfo.Columns.Add("TotalRows", typeof(int));
							paginationInfo.Columns.Add("TotalPages", typeof(int));
							paginationInfo.Columns.Add("PageNumber", typeof(int));
							paginationInfo.Columns.Add("PageSize", typeof(int));

							int totalRows = Convert.ToInt32(countTable.Tables[0].Rows[0]["TotalRows"]);

							paginationInfo.Rows.Add(totalRows, (int)Math.Ceiling((double)totalRows / pageSize), pageNumber, pageSize);
							ds.Tables.Add(paginationInfo);
						}
					}
					else
					{
						ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
					}

				}

			}
			catch (Exception ex)
			{
				//AND d.assign_to_vendor_id = 56
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//ORDER SEND TO STORE
		public int OrderSendToStore(string SelectedIds)
		{
			int res = 0;
			try
			{
				string query = "";

				query = "UPDATE ta_order_details SET order_status = 12 WHERE id IN (" + SelectedIds + ")";
				DBHelper.ExecuteQueryReturnDS(query);

				string[] IdsArray = SelectedIds.ToString().Replace("on,", "").Split(',');
				for (int i = 0; i < IdsArray.Length; i++)
				{
					OrderDetailsHistory("ta_order_details", Convert.ToInt32(IdsArray[i]), "U");
					string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + IdsArray[i] + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();
					GlobalBal.InsertNotificationMessage("You have received completed Order " + OrderSeriesNumber + "from " + GlobalBal.GetTenantName(IdsArray[i]) + "", "4");
					GlobalBal.InsertNotificationMessage("You Have send to order " + OrderSeriesNumber + " successfully..", "5");

					//string QueryData = "SELECT d.id,um.user_name,um.mobile_no,d.order_series_no FROM ta_order_details AS d LEFT JOIN ta_user_management AS u ON u.id = d.created_by LEFT JOIN ta_user_mapping AS um ON um.mobile_no = u.mobile_no WHERE d.id = " + IdsArray[i] + "";

					//DataSet DbObject = DBHelper.ExecuteQueryReturnDS(QueryData);
					//if (DbObject != null)
					//{

					//}

				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return res;
		}

		//#################################################################################################################################################
		//THIS FUNCTION USED FOR GET JOB CARD RELATED DATA
		public DataSet GetJobCardDetails(string ItemDetailsId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT d.id AS order_details_id,h.order_no,h.order_date,h.order_delivery_date,d.reference_barcode,d.order_series_no,i.item_name,c.category_name,d.net_wt,d.gross_wt,d.size,d.design_code,d.remark,d.pcs,pr.product_group_name,p.purity FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id WHERE d.id =" + ItemDetailsId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//AND d.assign_to_vendor_id = 56
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS IS USED FOR GET ATTACHMENT IMAGE DOR SHOW ON JOB CARD
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

		//THIS IS USED FOR GET STONE DATA SHOW ON JOB CARD
		public DataSet StoneRelatedData(string ItemDetailsId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT i.item_name,COALESCE(st.color_name, 'NA') AS color_name,c.category_name AS stone_category_name,s.stone_pcs,s.stone_wt FROM ta_order_stone_details AS s JOIN ta_item_master AS i ON i.id = s.stone_id LEFT JOIN ta_stone_color_master AS st ON st.id = s.stone_color_id LEFT JOIN ta_category_master AS c ON c.id=s.stone_category_id WHERE order_detail_item_id =" + ItemDetailsId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//AND d.assign_to_vendor_id = 56
				//logger.Error(ex.Message);
			}
			return ds;
		}
		//#################################################################################################################################################

		//THIS IS USED FOR ORDER STATUS FOR HO
		public DataSet GetAllOrdersData(string SelectedDataIds)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT id,order_status FROM ta_order_details WHERE id IN(" + SelectedDataIds + ")";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS IS USED FOP UPDATE STATUS
		public void UpdateOrderComplete(string SelectedDataIds)
		{
			try
			{
				string query = "";

				string CheckQueryData = "SELECT id,order_status FROM ta_order_details WHERE id IN (" + SelectedDataIds + ")";
				DataSet Ds = DBHelper.ExecuteQueryReturnDS(CheckQueryData);

				if (Ds != null && Ds.Tables[0].Rows.Count > 0)
				{
					for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
					{
						if (Ds.Tables[0].Rows[i]["order_status"].ToString() == "12" && GlobalBal.GetSessionValue("RoleId").ToString() == "4")
						{
							query = "UPDATE ta_order_details SET order_status = '13' WHERE id IN (" + Ds.Tables[0].Rows[i]["id"].ToString() + ")";
							DBHelper.ExecuteQuery(query);

							OrderDetailsHistory("ta_order_details", Convert.ToInt32(Ds.Tables[0].Rows[i]["id"].ToString()), "U");

							string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + Ds.Tables[0].Rows[i]["id"].ToString() + ")";
							string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();
							GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " send successfully....", "4");
						}
						else
						{
							if (GlobalBal.GetSessionValue("RoleId").ToString() == "2" || GlobalBal.GetSessionValue("RoleId").ToString() == "3")
							{
								query = "UPDATE ta_order_details SET order_status = '12' WHERE id IN (" + Ds.Tables[0].Rows[i]["id"].ToString() + ")";
								DBHelper.ExecuteQuery(query);

								OrderDetailsHistory("ta_order_details", Convert.ToInt32(Ds.Tables[0].Rows[i]["id"].ToString()), "U");

								string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + Ds.Tables[0].Rows[i]["id"].ToString() + ")";
								string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();
								GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " send successfully....", "4");
							}
						}
					}
				}

			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
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

		public DataSet GetLiveryChallanNumberPath(string ItemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";

				query = "SELECT challan_pdf_path FROM ta_order_details WHERE id=" + ItemId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		public DataSet GetStoneDetails(string ItemId)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";

				//query = "SELECT t.item_name,st.color_name,s.stone_wt,s.stone_pcs FROM ta_order_details AS d JOIN ta_order_stone_details AS s ON d.id = s.order_detail_item_id JOIN ta_item_master AS t ON t.id = s.stone_id JOIN ta_stone_color_master AS st ON st.id = s.stone_color_id WHERE d.id = " + ItemId + "";
				query = "SELECT t.item_name,COALESCE(st.color_name, 'NA') AS color_name,c.category_name AS stone_category_name,s.stone_wt,s.stone_pcs FROM ta_order_details AS d JOIN ta_order_stone_details AS s ON d.id = s.order_detail_item_id JOIN ta_item_master AS t ON t.id = s.stone_id LEFT JOIN ta_stone_color_master AS st ON st.id = s.stone_color_id LEFT JOIN ta_category_master AS c ON c.id=s.stone_category_id WHERE d.id = " + ItemId + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		public DataSet GetUserName(string UserId)
		{
			DataSet ds = new DataSet();
			try
			{
				string Query = "SELECT um.id, COALESCE(up.user_name, t.tenant_name, v.vendor_name, 'TechneAI') AS user_name, um.mobile_no, um.role_id, um.tenant_id, um.is_active FROM ta_user_management AS um LEFT JOIN ta_user_mapping AS up ON um.mobile_no = up.mobile_no LEFT JOIN ta_tenant_master AS t ON t.mobile_no = um.mobile_no LEFT JOIN ta_vendor_master AS v ON v.mobile_no = um.mobile_no WHERE um.is_active = 1 AND um.id ='" + UserId + "'";
				ds = DBHelper.ExecuteQueryReturnDS(Query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		//THIS IS USED FOR ORDER REJECTD FROM BACKOFFICE PAGE
		public void OrderRejectedHOFromNewOrdersPage(IFormCollection Form)
		{
			try
			{
				string query = "";
				query = "UPDATE ta_order_details SET order_status = 3, rejected_remark='" + Form["RejectedRemark"] + "' WHERE id IN (" + Form["SelectedOrderItemId"].ToString().Replace("on,", "") + ")";
				DBHelper.ExecuteQuery(query);

				string[] IdsArray = Form["SelectedOrderItemId"].ToString().Replace("on,", "").Split(',');
				for (int i = 0; i < IdsArray.Length; i++)
				{
					OrderDetailsHistory("ta_order_details", Convert.ToInt32(IdsArray[i]), "U");
					//THIS IS USED FOR REJECTED ORDER FROM NEW ORDERS PAGE
					string Query = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + IdsArray[i] + ")";
					string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query).ToString();

					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is rejected by " + GlobalBal.GetTenantName(IdsArray[i].ToString()) + "", "4");
					GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " is rejected by Successfully..", "3");
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}

		//THIS FUNCTION USED FOR GET PRIVORITY BASED DATA 
		public DataSet GetVendorPriorityData(Dictionary<string, object> filters)
		{
			DataSet ds = new DataSet();
			string queryPara = " h.id AS order_header_id, d.id AS order_item_id, s.vendor AS STATUS, b.branch_name, d.order_status, h.order_no, cu.customer_name, i.item_name, d.order_series_no, c.category_name, p.purity as purity_name, d.net_wt, d.gross_wt, d.pcs, pr.product_group_name, IFNULL(v.vendor_name, '') AS vendor_name, d.expected_delivery_date, h.order_date, d.expected_delivery_date as order_delivery_date";

			string joinQuery = "  FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_user_management AS u ON u.id = d.assign_to_vendor_id LEFT JOIN ta_customer_master AS cu ON cu.id = h.customer_id LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id WHERE d.order_status IN (4, 7, 14, 8, 10, 11) AND h.order_no != '0' AND d.assign_to_vendor_id = " + GlobalBal.GetSessionValue("UserId").ToString();


			// Apply additional filters if provided
			if (filters != null && filters.Count > 0)
			{
				if (filters.ContainsKey("Flag") && !string.IsNullOrEmpty(filters["Flag"]?.ToString()))
				{
					string Flag = filters["Flag"]?.ToString();

					if (Flag == "1")
					{
						joinQuery = joinQuery + " AND d.order_status = 4";
					}
					else if (Flag == "2")
					{
						joinQuery = joinQuery + " AND d.order_status = 7";
					}
					else if (Flag == "3")
					{
						joinQuery = joinQuery + " AND d.order_status = 14";
					}
					else if (Flag == "4")
					{
						joinQuery = joinQuery + " AND d.order_status = 8";
					}
					else if (Flag == "5")
					{
						joinQuery = joinQuery + " AND d.order_status = 10";
					}
					else if (Flag == "6")
					{
						joinQuery = joinQuery + " AND d.order_status = 11";
					}
				}

				if (filters.ContainsKey("CustomerName") && !string.IsNullOrEmpty(filters["CustomerName"]?.ToString()))
				{
					joinQuery += " AND cu.customer_name LIKE '%" + filters["CustomerName"].ToString().Trim() + "%'";
				}

				if (filters.ContainsKey("ItemName") && !string.IsNullOrEmpty(filters["ItemName"]?.ToString()))
				{
					joinQuery += " AND i.item_name LIKE '%" + filters["ItemName"].ToString().Trim() + "%'";
				}

				if (filters.ContainsKey("OrderNo") && !string.IsNullOrEmpty(filters["OrderNo"]?.ToString()))
				{
					joinQuery += " AND d.order_series_no LIKE '%" + filters["OrderNo"].ToString().Trim() + "%'";
                }

				if (filters.ContainsKey("Category") && !string.IsNullOrEmpty(filters["Category"]?.ToString()))
				{
					joinQuery += " AND c.category_name LIKE '%" + filters["Category"].ToString().Trim() + "%'";
				}

				if (filters.ContainsKey("VendorName") && !string.IsNullOrEmpty(filters["VendorName"]?.ToString()))
				{
					joinQuery += " AND v.vendor_name LIKE '%" + filters["VendorName"].ToString().Trim() + "%'";
				}

				if (filters.ContainsKey("BranchName") && !string.IsNullOrEmpty(filters["BranchName"]?.ToString()))
				{
					joinQuery += " AND b.branch_name LIKE '%" + filters["BranchName"].ToString().Trim() + "%'";
				}

				if (filters.ContainsKey("OrderDateFrom") && !string.IsNullOrEmpty(filters["OrderDateFrom"]?.ToString()) &&
					filters.ContainsKey("OrderDateTo") && !string.IsNullOrEmpty(filters["OrderDateTo"]?.ToString()))
				{
					joinQuery += " AND h.order_date BETWEEN '" + filters["OrderDateFrom"].ToString() + "' AND '" + filters["OrderDateTo"].ToString() + "'";
				}

				if (filters.ContainsKey("DeliveryDateFrom") && !string.IsNullOrEmpty(filters["DeliveryDateFrom"]?.ToString()) &&
					filters.ContainsKey("DeliveryDateTo") && !string.IsNullOrEmpty(filters["DeliveryDateTo"]?.ToString()))
				{
					joinQuery += " AND h.order_delivery_date BETWEEN '" + filters["DeliveryDateFrom"].ToString() + "' AND '" + filters["DeliveryDateTo"].ToString() + "'";
				}
			}

			joinQuery += "  ORDER BY h.created_at DESC ";

			String paginationapplyQuery = "";

			string baseQuery = "SELECT " + queryPara + joinQuery;

			// Count query
			string countQuery = "SELECT COUNT(*) AS TotalRows " + joinQuery;

			if (filters != null && filters.Count > 0)
			{
				if (filters.ContainsKey("PageNumber") && !string.IsNullOrEmpty(filters["PageNumber"]?.ToString()) && filters.ContainsKey("PageSize") && !string.IsNullOrEmpty(filters["PageSize"]?.ToString()))
				{
					int pageNumber = 0, pageSize = 0, offset = 0;
					pageNumber = Convert.ToInt32(filters["PageNumber"].ToString());
					pageSize = Convert.ToInt32(filters["PageSize"].ToString());
					offset = (pageNumber - 1) * pageSize;
					paginationapplyQuery = " LIMIT " + pageSize + " OFFSET " + offset;

					ds = DBHelper.ExecuteQueryReturnDS(baseQuery + paginationapplyQuery);

					DataSet countTable = DBHelper.ExecuteQueryReturnDS(countQuery);

					// Add pagination info
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						DataTable paginationInfo = new DataTable();
						paginationInfo.Columns.Add("TotalRows", typeof(int));
						paginationInfo.Columns.Add("TotalPages", typeof(int));
						paginationInfo.Columns.Add("PageNumber", typeof(int));
						paginationInfo.Columns.Add("PageSize", typeof(int));

						int totalRows = Convert.ToInt32(countTable.Tables[0].Rows[0]["TotalRows"]);

						paginationInfo.Rows.Add(totalRows, (int)Math.Ceiling((double)totalRows / pageSize), pageNumber, pageSize);
						ds.Tables.Add(paginationInfo);
					}
				}
				else
				{
					ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
				}
			}
			return ds;
		}

		//THIS IS USED FOR CHECK EXPECTED DATE NOT GREATER THAN DELIVERY DATE
		public int ExpectedDateValideOrNot(string Ids, string ExpectedDate)
		{
			int res = 0;
			try
			{
				//string query = "SELECT CASE WHEN EXISTS (SELECT 1 FROM ta_order_header AS h JOIN ta_order_details AS d ON d.order_header_id = h.id WHERE h.order_delivery_date > '"+ ExpectedDate + "' AND d.id IN ("+Ids+")) THEN 1 ELSE 0 END AS result";

				//res = Convert.ToInt32(DBHelper.ExecuteQueryReturnObj(query));

				string Query = "SELECT h.order_delivery_date FROM ta_order_header AS h JOIN ta_order_details AS d ON d.order_header_id = h.id WHERE d.id IN (" + Ids.Replace("on,", "") + ")";
				DataSet Ds = DBHelper.ExecuteQueryReturnDS(Query);

				if (Ds != null && Ds.Tables.Count > 0)
				{
					for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
					{
						if (Convert.ToDateTime(Ds.Tables[0].Rows[i]["order_delivery_date"]) > Convert.ToDateTime(ExpectedDate))
						{
							res = 1;
						}
						else
						{
							res = 0;
							break;
						}
					}
				}

			}
			catch (Exception Ex)
			{

			}
			return res;
		}

		//THIS IS USED FOR UPDATE EXPECTED DATE AND IGNORE GREATER THAN DELIVERY DATE RECORDS
		public void UpdateExpectedDateInOrderItem(string ExpectedDate, string Ids, string VendorId)
		{
			try
			{
				string Query = "SELECT d.id,h.order_delivery_date FROM ta_order_header AS h JOIN ta_order_details AS d ON d.order_header_id = h.id WHERE d.id IN (" + Ids.Replace("on,", "") + ")";
				DataSet Ds = DBHelper.ExecuteQueryReturnDS(Query);

				if (Ds != null && Ds.Tables.Count > 0)
				{
					for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
					{
						if (Convert.ToDateTime(Ds.Tables[0].Rows[i]["order_delivery_date"]) > Convert.ToDateTime(ExpectedDate))
						{
							string query = "UPDATE ta_order_details SET order_status=4,expected_delivery_date = '" + ExpectedDate + "',assign_to_vendor_id='" + VendorId + "' WHERE id IN(" + Ds.Tables[0].Rows[i]["id"] + ")";
							DBHelper.ExecuteQuery(query);

							//THIS IS USED FOR INSERT NOTIFICATION IN NOTIFICARION TABLE CODE
							OrderDetailsHistory("ta_order_details", Convert.ToInt32(Ds.Tables[0].Rows[i]["id"]), "U");
							string Query1 = "SELECT order_series_no FROM ta_order_details WHERE id IN (" + Ds.Tables[0].Rows[i]["id"] + ")";
							string OrderSeriesNumber = DBHelper.ExecuteQueryReturnObj(Query1).ToString();
							GlobalBal.InsertNotificationMessage("Order " + OrderSeriesNumber + " assigned to vendor successfully..", "3");
							GlobalBal.InsertNotificationMessage("You have received new Order " + OrderSeriesNumber + " from " + GlobalBal.GetTenantName(Ds.Tables[0].Rows[i]["id"].ToString()) + "", "5");

							//USE THIS CODE FOR SEND WHATSAPP MESSAGE
							string Query2 = "SELECT v.vendor_name,v.mobile_no FROM ta_order_details AS d JOIN ta_user_management AS u ON d.assign_to_vendor_id = u.id JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no WHERE u.id = " + VendorId + " LIMIT 1";
							DataSet VendorData = DBHelper.ExecuteQueryReturnDS(Query2);

							//THIS IS USED FOR CHECK DATA
							if (VendorData != null && VendorData.Tables.Count > 0)
							{
								string PlaceHolder = VendorData.Tables[0].Rows[0]["vendor_name"].ToString() + "," + OrderSeriesNumber + "," + "http://114.29.232.154:90/";
								MsgBal.SendWhatsappMessage(VendorData.Tables[0].Rows[0]["mobile_no"].ToString(), "670457", PlaceHolder);
							}
						}
					}
				}


			}
			catch (Exception Ex)
			{

			}
		}


		//THIS IS USED FOR CHECK EXPECTED DATE NOT GREATER THAN EXPECTED DATE
		public int ExpectedDateValideOrNotSubVendor(string Ids, string ExpectedDate)
		{
			int res = 0;
			try
			{
				//string query = "SELECT CASE WHEN EXISTS (SELECT 1 FROM ta_order_header AS h JOIN ta_order_details AS d ON d.order_header_id = h.id WHERE h.order_delivery_date > '"+ ExpectedDate + "' AND d.id IN ("+Ids+")) THEN 1 ELSE 0 END AS result";

				//res = Convert.ToInt32(DBHelper.ExecuteQueryReturnObj(query));

				string Query = "SELECT id,expected_delivery_date FROM ta_order_details WHERE id IN (" + Ids.Replace("ON,", "") + ")";
				DataSet Ds = DBHelper.ExecuteQueryReturnDS(Query);

				if (Ds != null && Ds.Tables.Count > 0)
				{
					for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
					{
						if (Convert.ToDateTime(Ds.Tables[0].Rows[i]["expected_delivery_date"]) > Convert.ToDateTime(ExpectedDate))
						{
							res = 1;
						}
						else
						{
							res = 0;
							break;
						}
					}
				}

			}
			catch (Exception Ex)
			{

			}
			return res;
		}

		//THIS IS USED FOR UPDATE EXPECTED DATE AND IGNORE GREATER THAN EXPECTED
		public void UpdateExpectedDateInOrderItemSubVendor(string ExpectedDate, string Ids, string VendorId)
		{
			try
			{
				string Query = "SELECT id,expected_delivery_date FROM ta_order_details WHERE id IN (" + Ids.Replace("ON,", "") + ")";
				DataSet Ds = DBHelper.ExecuteQueryReturnDS(Query);

				if (Ds != null && Ds.Tables.Count > 0)
				{
					for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
					{
						if (Convert.ToDateTime(Ds.Tables[0].Rows[i]["expected_delivery_date"]) > Convert.ToDateTime(ExpectedDate))
						{
							string query = "UPDATE ta_order_details SET order_status=7,expected_delivery_date = '" + ExpectedDate + "',expected_delivery_date_sub_vendor='" + VendorId + "' WHERE id IN(" + Ds.Tables[0].Rows[i]["id"] + ")";
							DBHelper.ExecuteQuery(query);

							OrderDetailsHistory("ta_order_details", Convert.ToInt32(Ds.Tables[0].Rows[i]["id"]), "U");
						}
						else
						{

						}
					}
				}
			}
			catch (Exception Ex)
			{

			}
		}

		public DataSet GetOrderPreviewPriority(Dictionary<string, object> data)
		{
			DataSet ds = new DataSet();
			try
			{
				// Define the common SELECT fields
				string queryField = "h.id AS order_header_id, d.id AS order_item_id, b.branch_name, v.vendor_name, " +
									"d.order_status, s.ho AS STATUS, h.order_no, i.item_name, c.category_name, p.purity as purity_name, " +
									"d.actual_item_gross_wt, d.order_series_no, d.expected_delivery_date, d.actual_item_net_wt, " +
									"d.upload_preview_image_path, d.net_wt, d.gross_wt, d.pcs, d.size, pr.product_group_name, " +
									"h.order_date, h.order_delivery_date";

				// Define the common JOINs
				string queryJoin = " FROM ta_order_header AS h " +
								   "JOIN ta_order_details AS d ON h.id = d.order_header_id " +
								   "JOIN ta_item_master AS i ON d.item_id = i.id " +
								   "JOIN ta_category_master AS c ON c.id = d.category_id " +
								   "JOIN ta_purity_master AS p ON p.id = d.purity_id " +
								   "JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id " +
								   "LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id " +
								   "LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status " +
								   "LEFT JOIN ta_user_management AS u ON u.id = d.assign_to_vendor_id " +
								   "LEFT JOIN ta_vendor_master AS v ON u.mobile_no = v.mobile_no " +
								   "WHERE h.tenant_id = " + GlobalBal.GetSessionValue("TenantId");

				// Apply filters based on the Flag
				if (data.TryGetValue("Flag", out var flagObj) && !string.IsNullOrEmpty(flagObj?.ToString()))
				{
					string flag = flagObj.ToString();
					switch (flag)
					{
						case "1": // Filter for specific order statuses
							queryJoin += " AND d.order_status IN (8, 9, 10)";
							break;
						case "2":
							queryJoin += " AND d.order_status = 9"; // Pending status
							break;
						case "3":
							queryJoin += " AND d.order_status = 10"; // Rejected status
							break;
						default:
							break;
					}
				}

				if (data.ContainsKey("CustomerName") && !string.IsNullOrEmpty(data["CustomerName"]?.ToString()))
				{
					queryJoin += " AND  cu.customer_name LIKE '%" + data["CustomerName"].ToString().Trim() + "%'";
				}

				if (data.ContainsKey("ItemName") && !string.IsNullOrEmpty(data["ItemName"]?.ToString()))
				{
					queryJoin += " AND i.item_name LIKE '%" + data["ItemName"].ToString().Trim() + "%'";
				}

				if (data.ContainsKey("OrderNo") && !string.IsNullOrEmpty(data["OrderNo"]?.ToString()))
				{
                    queryJoin += " AND d.order_series_no LIKE '%" + data["OrderNo"].ToString().Trim() + "%'";
                }

				if (data.ContainsKey("Category") && !string.IsNullOrEmpty(data["Category"]?.ToString()))
				{
					queryJoin += " AND c.category_name LIKE '%" + data["Category"].ToString().Trim() + "%'";
				}

				if (data.ContainsKey("VendorName") && !string.IsNullOrEmpty(data["VendorName"]?.ToString()))
				{
					queryJoin += " AND v.vendor_name LIKE '%" + data["VendorName"].ToString().Trim() + "%'";
				}
				if (data.ContainsKey("BranchName") && !string.IsNullOrEmpty(data["BranchName"]?.ToString()))
				{
					queryJoin += " AND b.branch_name LIKE '%" + data["BranchName"].ToString().Trim() + "%'";
				}

				if (data.ContainsKey("OrderDateFrom") && !string.IsNullOrEmpty(data["OrderDateFrom"]?.ToString()) &&
					data.ContainsKey("OrderDateTo") && !string.IsNullOrEmpty(data["OrderDateTo"]?.ToString()))
				{
					queryJoin += " AND h.order_date BETWEEN '" + data["OrderDateFrom"].ToString() + "' AND '" + data["OrderDateTo"].ToString() + "'";
				}

				if (data.ContainsKey("DeliveryDateFrom") && !string.IsNullOrEmpty(data["DeliveryDateFrom"]?.ToString()) &&
					data.ContainsKey("DeliveryDateTo") && !string.IsNullOrEmpty(data["DeliveryDateTo"]?.ToString()))
				{
					queryJoin += " AND h.order_delivery_date BETWEEN '" + data["DeliveryDateFrom"].ToString() + "' AND '" + data["DeliveryDateTo"].ToString() + "'";
				}

				// Combine the final query and count query
				string baseQuery = "SELECT " + queryField + queryJoin + " ORDER BY h.created_at DESC";
				string countQuery = "SELECT COUNT(*) AS TotalRows " + queryJoin;

				// Execute the query with pagination if available
				if (data.TryGetValue("PageNumber", out var pageNumberObj) && data.TryGetValue("PageSize", out var pageSizeObj) &&
					int.TryParse(pageNumberObj.ToString(), out int pageNumber) && int.TryParse(pageSizeObj.ToString(), out int pageSize))
				{
					ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
					// Execute the count query to get the total row count
					DataSet countTable = DBHelper.ExecuteQueryReturnDS(countQuery);

					// Add pagination info to the result
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						string paginationQuery = "";
						int offset = (pageNumber - 1) * pageSize;
						paginationQuery = " LIMIT " + pageSize + " OFFSET " + offset;

						ds = DBHelper.ExecuteQueryReturnDS(baseQuery + paginationQuery);

						DataTable paginationInfo = new DataTable();
						paginationInfo.Columns.Add("TotalRows", typeof(int));
						paginationInfo.Columns.Add("TotalPages", typeof(int));
						paginationInfo.Columns.Add("PageNumber", typeof(int));
						paginationInfo.Columns.Add("PageSize", typeof(int));

						int totalRows = Convert.ToInt32(countTable.Tables[0].Rows[0]["TotalRows"]);
						paginationInfo.Rows.Add(totalRows, (int)Math.Ceiling((double)totalRows / pageSize), pageNumber, pageSize);
						ds.Tables.Add(paginationInfo);
					}
				}
				else
				{
					ds = DBHelper.ExecuteQueryReturnDS(baseQuery);
				}
			}
			catch (Exception ex)
			{
				// Log the exception for debugging purposes
				// logger.Error(ex.Message);
			}

			return ds;
		}

		public int CheckingOrderCreatesOwner(string OrderItemIds)
		{
			int Result = 0;
			try
			{
				string Query = "SELECT h.role_id FROM ta_order_header AS h JOIN ta_order_details AS d ON d.order_header_id = h.id WHERE d.id IN (" + OrderItemIds + ")";
				DataSet DS = DBHelper.ExecuteQueryReturnDS(Query);

				if (DS != null && DS.Tables.Count > 0)
				{
					for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
					{
						if (DS.Tables[0].Rows[i]["role_id"].ToString() == "4")
						{
							Result = 1;
							break;
						}
					}
				}

			}
			catch (Exception Ex)
			{

			}
			return Result;
		}

		public string OrderCompletedHO(string Ids)
		{
			try
			{
				string Query = "SELECT h.role_id,d.id FROM ta_order_header AS h JOIN ta_order_details AS d ON d.order_header_id = h.id WHERE d.id IN (" + Ids.Replace("ON,", "") + ")";
				DataSet Ds = DBHelper.ExecuteQueryReturnDS(Query);

				if (Ds != null && Ds.Tables.Count > 0)
				{
					for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
					{
						if (Ds.Tables[0].Rows[i]["role_id"].ToString() != "4")
						{
							string query = "UPDATE ta_order_details SET order_status=12 WHERE id IN(" + Ds.Tables[0].Rows[i]["id"] + ")";
							DBHelper.ExecuteQuery(query);

							OrderDetailsHistory("ta_order_details", Convert.ToInt32(Ds.Tables[0].Rows[i]["id"]), "U");
						}
						else
						{

						}
					}
				}
			}
			catch (Exception Ex)
			{

			}
			return "";
		}

		//THIS IS USED FOR CHECKING LOGIN USED TENANT/HO AND VENDOR
		public string CheckingLoginUserHOASVendor()
		{
			string Count = "0";
			try
			{
				string query = "";
				query = "SELECT COUNT(v.id) AS cnt FROM ta_user_management AS u LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no WHERE u.id = " + GlobalBal.GetSessionValue("UserId") + "";
				Count = DBHelper.ExecuteQueryReturnObject(query).ToString();
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return Count;
		}

		//THIS FUNCTION USED FOR CHECK ORD ID DETAISL STATUS
		public DataSet GetOrderWeightData(string SelectedDataIds)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT  h.id AS order_header_id,d.id AS order_item_id,d.order_series_no,d.net_wt,d.gross_wt,h.order_delivery_date,s.ho AS STATUS  FROM ta_order_header AS h JOIN ta_order_details AS d ON h.id = d.order_header_id JOIN ta_item_master AS i ON d.item_id = i.id JOIN ta_category_master c ON c.id = d.category_id LEFT JOIN ta_customer_master AS cu ON cu.id = h.customer_id JOIN ta_purity_master p ON p.id = d.purity_id JOIN ta_product_group_master AS pr ON pr.id = i.product_group_id LEFT JOIN ta_user_management AS u ON u.id = d.assign_to_vendor_id LEFT JOIN ta_vendor_master AS v ON v.mobile_no = u.mobile_no AND v.tenant_id =4 LEFT JOIN ta_branch_master AS b ON b.id = h.branch_id LEFT JOIN ta_rolewise_status_code AS s ON s.status_code = d.order_status LEFT JOIN ta_tenant_master AS t ON t.id = h.tenant_id WHERE h.order_no != '0' AND h.tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND d.id = " + SelectedDataIds + " ORDER BY h.created_at DESC ";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

        //THIS FUNCTION USED FOR GET ITEM UOM
        public dynamic OrderEditSettingSalesPerson(string setting_name)
        {
            dynamic value = "0";
            try
            {
                string query = "";
                query = "SELECT CASE WHEN EXISTS ( SELECT 1 FROM ta_tenant_wise_setting_master TV2 WHERE TV2.setting_id = SM.id AND TV2.tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " AND TV2.value IS NOT NULL ) THEN ( SELECT TV2.value FROM ta_tenant_wise_setting_master TV2 WHERE TV2.setting_id = SM.id AND TV2.tenant_id = " + GlobalBal.GetSessionValue("TenantId") + " LIMIT 1 ) ELSE SM.default_value END AS VALUE FROM ta_setting_master SM WHERE SM.setting_name = '" + setting_name + "'";
                DataSet DS = DBHelper.ExecuteQueryReturnDS(query);

				if (DS.Tables.Count > 0 && DS.Tables[0].Rows.Count > 0)
				{
					value = DS.Tables[0].Rows[0]["VALUE"];
				}
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
            return value;
        }
    }
}





