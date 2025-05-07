using BAL;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json;
using System.Data;
using System.Reflection;

namespace CustomerOrderManagement.Controllers.Dashboard
{
  public class DashboardController : Controller
  {
    private readonly DashboardBAL DashBAL;
    private readonly GlobalSessionBAL GlobalBal;
    private readonly OrdersMasterBAL OrderBal;

    // Constructor injection for LoginBAL
    public DashboardController(DashboardBAL DashBal, GlobalSessionBAL globalBal, OrdersMasterBAL orderBal)
    {
      DashBAL = DashBal;
      GlobalBal = globalBal;
      OrderBal = orderBal;
    }
    public IActionResult Index()
    {
      return View();
    }

    //THIS IS USED FOR MAIN DASHBOARDS
    public IActionResult Dashboard()
    {
      string roleId = GlobalBal.GetSessionValue("RoleId")?.ToString();

      if (string.IsNullOrEmpty(roleId))
      {
        // Handle case when roleId is null or empty (e.g., redirect to login or show an error)
        return RedirectToAction("Login", "StartPage");
      }

      switch (roleId)
      {
        case "1":
        case "2":

        case "3":
          return RedirectToAction("BackOfficeDashboard", "Dashboard"); // Example case
        case "4":
          return RedirectToAction("SalePersonDashboard", "Dashboard");
        case "5":
          return RedirectToAction("VendorDashboard", "Dashboard"); // Example case
        case "6":

        case "7":
          return RedirectToAction("SubVendorDashboard", "Dashboard"); // Example case
        default:
          // Handle default case (e.g., redirect to an error page)
          return RedirectToAction("Error", "Home");

      }
    }

    //########################################################################################### SALES PERSON ###########################################################################33
    //THIS US USED FOR SALES PERSON DASHBOARD
    public IActionResult SalePersonDashboard()
    {
      ViewBag.OrderEditStatus = DashBAL.OrderEditSettingSalesPerson("order_edit_sales"); 
      ViewBag.NewOrderCount = DashBAL.SalesPersonDashboardCounts("1");
      ViewBag.BackOfficeRejectedCount = DashBAL.SalesPersonDashboardCounts("2");
      ViewBag.OrderReadyCount = DashBAL.SalesPersonDashboardCounts("3");
      ViewBag.OrderSendCount = DashBAL.SalesPersonDashboardCounts("4");
      return View();
    }

    //THIS IS USED FOR BACK OFFICE DASHBOARD
    public IActionResult BackOfficeDashboard()
    {
      
      //MessageHelper.SendTextSmsPinnacle("","");
      //ViewBag.NewOrderCounts = DashBAL.NewOrderCount();
      //ViewBag.PendingOrdersCount = DashBAL.AssignOrderCount();
      //ViewBag.RejectedByVendor = DashBAL.RejectedByVendorCount();
      //ViewBag.RecivedbyVendor = DashBAL.VendorOrderSendToPreview();
      //ViewBag.Completed = DashBAL.VendorOrderComplete();
      //ViewBag.OrderData = DashBAL.AllOrderData();
      //ViewBag.OrderSendToStore = DashBAL.OrderReadyToSendStore();

      ViewBag.NewOrderCounts = DashBAL.BackOfficeDashboardCardCounts("1");
      ViewBag.PendingOrdersCount = DashBAL.BackOfficeDashboardCardCounts("2");
      ViewBag.RejectedByVendor = DashBAL.BackOfficeDashboardCardCounts("3");
      ViewBag.RecivedbyVendor = DashBAL.BackOfficeDashboardCardCounts("4");
      ViewBag.Completed = DashBAL.BackOfficeDashboardCardCounts("5");
      ViewBag.OrderSendToStore = DashBAL.BackOfficeDashboardCardCounts("6");

      ViewBag.VendorName = DashBAL.OrderSendVenodrsList();
      return View();
    }

    //THISIS USED FOR ORDER STATUS VIEW SHOWS ALL ORDERS, ACCEPTED, REJECTED
    public IActionResult OrdersStatus()
    {
      // THIS FUNCTION IS USED TO GET SUBSCRIPTION VALID DATE OF LAOGIN TENANT
      ViewBag.SubsciptionStatus = DashBAL.GetSubscriptionDetails();
      ViewBag.VendorName = DashBAL.OrderSendVenodrsList().Tables[0].AsEnumerable();
      //ViewBag.OrderData = DashBAL.AllOrderData();
      return View();
    }

    //THIS IS USED FOR GET ALL ORDERS
    public IActionResult GetAllOrder([FromBody] Dictionary<string, object> data)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.AllOrdersData(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //##################################################### ORDER ACCEPT AND REJCET FROM DASHBOARD ###########################################################################
    //THIS IS USED FOR ORDER UPDATES
    public IActionResult OrderUpdatedStatus(string SelectedDataIds, string Status)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      DataSet Ds = DashBAL.OrderDetailsIdChecks(SelectedDataIds);
      if (Ds != null && Ds.Tables[0].Rows.Count > 0)
      {
        for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
        {
          if (Ds.Tables[0].Rows[i]["order_status"].ToString() == "0" || Ds.Tables[0].Rows[i]["order_status"].ToString() == "2")
          {
            DashBAL.OrderStatusUpdateFromHO(Ds.Tables[0].Rows[i]["id"].ToString(), Status);
          }
        }
      }

      Dictionary<string, object> data = new Dictionary<string, object>();
      data.Add("Flag", "1");
      Param.Add("OrderData", DashBAL.AllOrdersData(data).Tables[0]);
      //Param.Add("ErrorMesssage", Message);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR ORDER UPDATES
    public IActionResult OrderUpdatedsStatusFromOrders(string SelectedDataIds, string Status)
    {
      var jsondata = "";

      Dictionary<string, object> Param = new Dictionary<string, object>();

      DataSet Ds = DashBAL.OrderDetailsIdChecks(SelectedDataIds);
      if (Ds != null && Ds.Tables[0].Rows.Count > 0)
      {
        for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
        {
          if (Ds.Tables[0].Rows[i]["order_status"].ToString() == "0" || Ds.Tables[0].Rows[i]["order_status"].ToString() == "2")
          {
            DashBAL.OrderStatusUpdateFromHO(Ds.Tables[0].Rows[i]["id"].ToString(), Status);
          }
        }
      }

      Dictionary<string, object> data = new Dictionary<string, object>();
      data.Add("Flag", Status);
      Param.Add("OrderData", DashBAL.AllOrdersData(data).Tables[0]);
      TempData["HoReject"] = "Order Accepted Successfully...!!!";
      //Param.Add("ErrorMesssage", Message);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    ////################################################################################# VENDOR DASHBOARD ###########################################################################
    //THIS FUNCTION USED FOR VENDOR DASHBOARD
    public IActionResult VendorDashboard()
    {
      //ViewBag.NewOrderCount = DashBAL.VendorReceivedOrderCount();
      //ViewBag.AssignSubVendorOrderCount = DashBAL.AssignSubVendorOrderCount();
      //ViewBag.PreviewRejectCount = DashBAL.PreviewRejectedCount();
      //ViewBag.ReceivedFromSubVendor = DashBAL.ReceivedFromSubVendor();
      //ViewBag.OrderSendToPreview = DashBAL.OrderSendToHOPreview();
      //ViewBag.OrderCompleted = DashBAL.OrderCompleteByVendor();

      ViewBag.NewOrderCount = DashBAL.VendorDashboardCardsCount("1");
      ViewBag.AssignSubVendorOrderCount = DashBAL.VendorDashboardCardsCount("2");
      ViewBag.PreviewRejectCount = DashBAL.VendorDashboardCardsCount("3");
      ViewBag.ReceivedFromSubVendor = DashBAL.VendorDashboardCardsCount("4");
      ViewBag.OrderSendToPreview = DashBAL.VendorDashboardCardsCount("5");
      ViewBag.OrderCompleted = DashBAL.VendorDashboardCardsCount("6");

      //THIS IS USED FOR ALL VENDOR DATA
      ViewBag.OrderData = DashBAL.AllVendorOrders();

      return View();
    }
    ////########################################### VENDOR ORDER STATUS VIEW #####################################################
    //THIS FUNCTION USED FOR VENDOR ALL ORDERS
    public IActionResult VendorOrderStatus()
    {
      ViewBag.VendorOrderMenu = DashBAL.CheckingLoginUserHOASVendor();
      ViewBag.SubVendorName = DashBAL.OrderSendSubVenodrsList().Tables[0].AsEnumerable();
      ViewBag.OrderData = DashBAL.AllVendorOrders();
      return View();
    }

    //////################################## VENDOR ORDERS ACCEPTED AND REJECTED ORDERS SHOW ###########################################
    //THIS FUNCTION USED FOR GET VENDOR ALL ORDERS
    public IActionResult GetAllVendorOrder([FromBody] Dictionary<string, object> data)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      DataSet ds = DashBAL.AllVendorOrders(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //////##################################### VENDOR ACCEPTED AND REJECTED ORDER ####################################################
    //THIS FUNCTION USED FOR UPDATE ORDER STATUS
    public IActionResult VendorOrderUpdatedStatus(string SelectedDataIds, string Status)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet DS = DashBAL.GetAllOrdersData(SelectedDataIds.Replace("on,", ""));
      if (DS != null)
      {
        for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
        {
          if (DS.Tables[0].Rows[i]["order_status"].ToString() != "5" || DS.Tables[0].Rows[i]["order_status"].ToString() != "6")
          {
            DashBAL.VendorOrderStatusUpdate(DS.Tables[0].Rows[i]["id"].ToString(), Status);
          }
        }
      }
      Dictionary<string, object> data = new Dictionary<string, object>();
      data.Add("Flag", "1");
      Param.Add("OrderData", DashBAL.AllVendorOrders(data).Tables[0]);
      //Param.Add("ErrorMesssage", Message);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR ORDER SEND TO VENDOR
    public IActionResult OrderSendToSubVendor()                    //########################### SEND SUBVENDOR DATA GET ##################################        
    {
      var Form = HttpContext.Request.Form;
      DashBAL.OrderSendtoSubVendor(Form);
      TempData["Message"] = "Order Assign To Sub Vendor...!!";
      return RedirectToAction("Dashboard", "Dashboard");
    }

    //#################################################################################### THIS IS DASHBOARD DIV BUTTON ################################################################################
    //THIS FUNCTION USED FOR GET PRIVOIRTY BASES ORDER DATA
    public IActionResult GetOrdersOnPrivorityBases(string Flag)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Dictionary<string, object> Data = new Dictionary<string, object>();
      Data.Add("Flag", Flag);
      Param.Add("OrderData", DashBAL.GetOrderDetailsOnTheBasesPriority(Data).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }


    //THIS FUNCTION USED FOR ORDER SEND TO VENDOR
    public IActionResult DirectOrderSendToVendor(string VendorId, string ExpectedDate, string Ids)    //########## THIS IS EDIT ORDER SEND TO VENDOR ACTION BUTTON
    {
      var Form = HttpContext.Request.Form;
      DashBAL.DirectOrderSendToVendor(VendorId, ExpectedDate, Ids);
      TempData["Message"] = "Order Assign To Vendor...!!";
      return RedirectToAction("Dashboard", "Dashboard");
    }

    //################################################################################SALES PERSON DASHBOARD DIV BUTTON ###############################################################################
    //THIS IS USE USED FOR DASHBOARD BUTTON CLICK THAT TIME DATA FETCH
    public IActionResult GetSalesPersonPriorityOrders([FromBody] Dictionary<string, object> data)
    {

      DataSet ds = new DataSet();

      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      if (data != null && data.ContainsKey("Flag") && string.IsNullOrEmpty(data["Flag"]?.ToString()))
      {
        ds = DashBAL.AllOrderData(data);
      }
      else
      {
        ds = DashBAL.GetSalesPersonPriority(data);
      }

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);

      return Json(jsondata);
    }

    //THIS IS USED FOR NEW ORDER BACKOFFICE VIEW
    public IActionResult NewOrderBackOffice()
    {
      return View();
    }

    //THIS IS USED FOR NEW ORDER VIEW ON HO SIDE USED FOR IF APPLY FILTER
    public IActionResult NewOrderBackOfficeFilter([FromBody] Dictionary<string, object> data)
    {
      Dictionary<string, object> Data = new Dictionary<string, object>();
      data.Add("Flag", "5");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetOrderDetailsOnTheBasesPriority(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //########################## HO DASHBOARD VENDOR REJECTED ORDERS VIEW ######################################################
    public IActionResult VendorOrderRejected()
    {
      //Dictionary<string, object> Data = new Dictionary<string, object>();
      //Data.Add("Flag", "4");
      //ViewBag.AllRejected = DashBAL.GetOrderDetailsOnTheBasesPriority(Data);
      ViewBag.VendorName = DashBAL.OrderSendVenodrsList().Tables[0].AsEnumerable();
      return View();
    }
    //THIS FUNCTIONIS IS USED FOR GET FILTER VENDOR GROUP
    [HttpGet]
    public JsonResult GetFilteredVendorGroups(string SearchItem)
    {
      var allGroups = DashBAL.FilterOrderSendVenodrsList(SearchItem).Tables[0].AsEnumerable();
      var filtered = allGroups
          .Select(row => new {
            id = row["id"],
            vendor_name = row["vendor_name"].ToString()
          })
          .ToList();
      return Json(filtered);
    }
    //THIS IS USED FOR VENDOR ORDER REJECTED VIEW APPLY FILTER
    public IActionResult VendorOrderRejectedfilter([FromBody] Dictionary<string, object> data)
    {
      Dictionary<string, object> Data = new Dictionary<string, object>();
      data.Add("Flag", "4");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetOrderDetailsOnTheBasesPriority(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //######################################### ORDER SEND VENDOR VIEW #############################################################
    //VENDOR ASSIGN VIEW
    public IActionResult AssignToVendor()
    {
      return View();
    }

    //THIS IS  USED FOR ASSIGN ORDER TO VENDOR APPLY FILTER ON THAT PAGE
    public IActionResult AssignToVendorFilter([FromBody] Dictionary<string, object> data)
    {
      Dictionary<string, object> Data = new Dictionary<string, object>();
      data.Add("Flag", "2");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetOrderDetailsOnTheBasesPriority(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }

      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //#################################################################################### VENDOR ASSIGN AFTER REJECT ORDERS #################################################################################
    //THIS IS USED FOR VENDOR ASSIGN AFTER REJECT ORDERS
    public IActionResult VendorAssignAfterHOReject()
    {
      var Form = HttpContext.Request.Form;
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DashBAL.AssignOrderAfterRejectedOrders(Form);
      TempData["Message"] = "Order Rejected Successfully...!!";
      return RedirectToAction("AssignToVendor", "Dashboard");
    }

    //#################################################################################### ORDER SEND VENDOR VIEW #################################################################################
    //THIS FUNCTION USED FOR UPDATE ORDER STATUS
    public IActionResult VendorNewOrderReceived(string Status)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Dictionary<string, object> data = new Dictionary<string, object>();
      data.Add("Flag", "4");
      Param.Add("OrderData", DashBAL.AllVendorOrders(data).Tables[0]);
      //Param.Add("ErrorMesssage", Message);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR SEND ORDER PREVIEW TO HO
    public IActionResult VendorProcess(string OrderItemId)
    {
      ViewBag.OrderEditData = DashBAL.GetOrderIdDetails(OrderItemId);
      ViewBag.StoneEditData = DashBAL.GetOrderStoneData(OrderItemId);
      ViewBag.AttachmentEditData = DashBAL.GetOrderAttachmentData(OrderItemId);

      return View();
    }

    //THSI FUNCTION USED FOR PREVIEW SEND ORDER
    public IActionResult VendorOrderRecivedForPreview([FromBody] Dictionary<string, object> data = null)
    {
      ViewBag.ReceivedOrderForPreview = DashBAL.AllOrderReceivedForPreview();
      return View();
    }

    //THIS FUNCTION USED FOR GET ORDER DETAILS FOR PREVIEW
    public IActionResult GetOrderItemIddetailsForPreview(string OrderItemId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("OrderData", DashBAL.GetOrderForPreview(OrderItemId).Tables[0]);
      //Param.Add("ErrorMesssage", Message);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOPR PREVIEW ACCEPT AND REJECT
    public IActionResult PreviewAcceptAndReject(string Flag, string ItemDetailsId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("OrderData", DashBAL.OrderPreviewAcceptOrReject(Flag, ItemDetailsId));

      //ACCEPT
      if (Flag == "1")
      {
        TempData["PreviewAcceptReject"] = "Order Preview Accepted...!!!";
      }
      else
      {
        TempData["PreviewAcceptReject"] = "Order Preview Rejected...!!!";
      }

      Param.Add("ErrorMessage", TempData["PreviewAcceptReject"]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR GET ALL ORDERS DATA WITH DELIVERY CHALLAN
    public IActionResult OrderReceivedWithDelivery()
    {
      Dictionary<string, object> Param = new Dictionary<string, object>();
      ViewBag.ReceivedOrderWithDelivery = DashBAL.AllOrderWithDeliveryChallan(Param);
      return View();
    }

    //THIS IS USED FOR ORDER RECIEVED FOR DELVERY CHALLAN APPLY FILTER GETR DATA
    public IActionResult OrderReceivedWithDeliveryFilter([FromBody] Dictionary<string, object> data)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.AllOrderWithDeliveryChallan(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }


    //THIS FUNCTION USED FOR SEND TO STORE SELECTED
    public IActionResult OrderSendToStore(string SelectedDataIds)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("OrderData", DashBAL.OrderSendToStore(SelectedDataIds));
      TempData["HoSendToStore"] = "Order Send Successfully To Store......!!!";
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THOS IS USED FOR GET JOB CARD DETAILS FOR SHOW
    public IActionResult GetJobcardDetails(string ItemId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("OrderData", DashBAL.GetJobCardDetails(ItemId).Tables[0]);
      Param.Add("ImageData", DashBAL.GetJobCardDetailsImage(ItemId).Tables[0]);
      Param.Add("StoneData", DashBAL.StoneRelatedData(ItemId).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }


    //THIS IS USED FOR GET ORDERS DETAILS
    public IActionResult OrderDetails()
    {
      ViewBag.OrderEditStatus = DashBAL.OrderEditSettingSalesPerson("order_edit_sales");
      ViewBag.VendorName = DashBAL.OrderSendVenodrsList();
      return View();
    }

    //THIS IS USED FOR GET ORDERS DETAILS FILTER
    public IActionResult OrderDetailsFilter([FromBody] Dictionary<string, object> data)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.AllOrderData(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);

      return Json(jsondata);
    }


    //THSI IS USED FOR ORDER COMPLETE DATA
    public IActionResult OrderCompleteStatus(string SelectedDataIds)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DashBAL.UpdateOrderComplete(SelectedDataIds);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR GET DELIVERY CHALLAN PATH
    public IActionResult GetDeliveryChallanPath(string ItemDetailsId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("DeliveryChallan", DashBAL.GetLiveryChallanNumberPath(ItemDetailsId).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR GET STONE DETAILS FOR SHOW
    public IActionResult GetStoneDetails(string itemId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("StoneDetails", DashBAL.GetStoneDetails(itemId).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR ORDER COMPLETE AND SEND TO THAT ORDERS TO STORE
    public IActionResult SendToStore()
    {
      //Dictionary<string, object> Data = new Dictionary<string, object>();
      //Data.Add("Flag", "6");
      //ViewBag.AllSendToStoreOrder = DashBAL.GetOrderDetailsOnTheBasesPriority(Data);
      return View();
    }

    //THIS IS USED FOR SEND ORDER TO STORE
    public IActionResult SendToStoreFilter([FromBody] Dictionary<string, object> data)
    {
      data.Add("Flag", "6");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetOrderDetailsOnTheBasesPriority(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);

      return Json(jsondata);
    }

    //######################################################################## BACK OFFICE NEW ORDER FROM REJECT ################################3
    //THIS FUNCTION USED FOR BACK OFFICE REJECTED ON NEW ORDERS PAGE
    public IActionResult BackOfficeOrderReject()
    {
      var Form = HttpContext.Request.Form;
      DashBAL.OrderRejectedHOFromNewOrdersPage(Form);
      TempData["HoReject"] = "Order Rejected Successfully...!!!";
      return RedirectToAction("NewOrderBackOffice", "Dashboard");
    }

    //####################################################################### VENDOR DASHBOARD COUNTS BUTTON CLICK VIEW OPEN PARTS ###########################################################
    //THIS IS USED FOR VENDOR NEW ORDERS COUNT BUTTON TAB CLICK OPEN VIEW
    public IActionResult VendorNewOrders()
    {
      Dictionary<string, object> Data = new Dictionary<string, object>();
      Data.Add("Flag", "1");
      ViewBag.NewOrders = DashBAL.GetVendorPriorityData(Data);
      return View();
    }

    //THIS IS USED FOR ASSIGN TO SUBVENDOR ORDER COUNT TAB BUTTON VIEW
    public IActionResult AssignToSubVendor()
    {
      return View();
    }

    //THIS IS USED FOR ASSIGN TO SUB VENDOR ORDER 
    public IActionResult AssignToSubVendorFilter([FromBody] Dictionary<string, object> data)
    {
      data.Add("Flag", "2");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetVendorPriorityData(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);

      return Json(jsondata);
    }
    //THIS IS USED FOR RECEIVED ORDER FROM SUB VENDOR COUNT BUTTON VIEW
    public IActionResult ReceivedOrderFromSubVendor()
    {
      return View();
    }

    //THIS IS USED FOR RECIEVED FROM SUB VENDOR PAGE APPLY FILTER
    public IActionResult ReceivedOrderFromSubVendorFilter([FromBody] Dictionary<string, object> data)
    {
      data.Add("Flag", "3");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetVendorPriorityData(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);

      return Json(jsondata);
    }

    //THIS IS USED FOR ORDER SEND TO PREVIEW FOR HO
    public IActionResult OrderSendtoPreview()
    {
      return View();
    }

    //THIS IS USED FOR ORDER SEND TO PREVIEW
    public IActionResult OrderSendtoPreviewFilter([FromBody] Dictionary<string, object> data)
    {
      data.Add("Flag", "4");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetVendorPriorityData(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);

      return Json(jsondata);
    }

    //THIS IS USED FOR ORDER PREVIEW AND REJECETD PREVIEW COUNT BUTTON VIEW
    public IActionResult OrderPreviewReject()
    {
      return View();
    }

    //THIS IS USED FOR PREVIEW REJECTED PAGE APPLY FILTER FUNCTIONALITY
    public IActionResult OrderPreviewRejectFilter([FromBody] Dictionary<string, object> data)
    {
      data.Add("Flag", "5");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetVendorPriorityData(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);

      return Json(jsondata);
    }
    //THIS IS USED FOR VENDOR ORDER COMPLETED
    public IActionResult OrderComplete()
    {
      //Dictionary<string, object> Data = new Dictionary<string, object>();
      //Data.Add("Flag", "6");
      //ViewBag.OrderComplete = DashBAL.GetVendorPriorityData(Data);
      return View();
    }

    //THIS IS USED FOR ORDER COMPLTED PAGE APPLY FILTER
    public IActionResult OrderCompleteFilter([FromBody] Dictionary<string, object> data)
    {
      data.Add("Flag", "6");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetVendorPriorityData(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);

      return Json(jsondata);
    }

    //THIS IS USED FOR VENDOR RECEIVED NEW ORDER
    public IActionResult VendorNewOrdersfilter([FromBody] Dictionary<string, object> data)
    {
      data.Add("Flag", "1");
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetVendorPriorityData(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }
    //############################################################################################### END ###################################################################################3
    public IActionResult CheckDeliveryDateValidOrNot(string ItemDetailsId, string ExpectedDate)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("Result", DashBAL.ExpectedDateValideOrNot(ItemDetailsId, ExpectedDate));
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR GET LOGIN USER NAME
    public IActionResult GetUserName()
    {
      string UserId = GlobalBal.GetSessionValue("UserId");
      DataSet ds = DashBAL.GetUserName(UserId);
      var userName = ds.Tables[0].Rows[0]["user_name"].ToString();

      // Return user data as JSON
      var user = new
      {
        userName = userName
      };

      return Json(user); // This returns a JSON response
    }

    //THIS IS USED FOR SEND TO VENDOR CHECK DATES
    public IActionResult SendToVendor(string ExpectedDate, string ItemDetailsId, string VendorId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DashBAL.UpdateExpectedDateInOrderItem(ExpectedDate, ItemDetailsId, VendorId);
      TempData["EditOrderMessage"] = "Order Send To Vendor Successfully...!!";
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED CHECK EXPECTED AND GET ORDER DATE IS LESS THAN EXPECTED DATE
    public IActionResult CheckExpectedDateValidOrNot(string ItemDetailsId, string ExpectedDate)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("Result", DashBAL.ExpectedDateValideOrNotSubVendor(ItemDetailsId, ExpectedDate));
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR ASSIGN ORDER TO SUB VENDOR
    public IActionResult SendToSubVendor(string ExpectedDate, string ItemDetailsId, string VendorId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DashBAL.UpdateExpectedDateInOrderItemSubVendor(ExpectedDate, ItemDetailsId, VendorId);
      TempData["EditOrderMessage"] = "Order Send To Vendor Successfully...!!";
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR GET ORDER PREVIEW RELATED PRIORITY
    public IActionResult GetOrderForPreviewRelatedPriority([FromBody] Dictionary<string, object> data)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      DataSet ds = DashBAL.GetOrderPreviewPriority(data);

      if (ds != null && ds.Tables.Count > 0) // Check if dataset and tables are not null
      {
        DataTable orderData = ds.Tables[0];
        DataTable paginationInfo = ds.Tables.Count > 1 ? ds.Tables[1] : null; // Ensure the second table exists

        // Add to Param dictionary if data exists
        if (orderData != null && orderData.Rows.Count > 0)
        {
          Param.Add("OrderData", orderData);
        }

        if (paginationInfo != null && paginationInfo.Rows.Count > 0)
        {
          Param.Add("PaginationInfo", paginationInfo);
        }
      }
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }


    //THIS IS USED FOR CHECK ORDER CREATE OWNER ROLE ID
    public IActionResult OrderCreatedByChecked(string selectedRowIds)
    {
      var jsondata = "";
      jsondata = DashBAL.CheckingOrderCreatesOwner(selectedRowIds).ToString();
      return Json(jsondata);
    }

    //THIS IS USED FOR ORDER COMPLTED BY HO
    public IActionResult OrderCompletedByHO(string OrderItemIds)
    {
      var jsondata = "";
      jsondata = DashBAL.OrderCompletedHO(OrderItemIds).ToString();
      return Json(jsondata);
    }

    //THIS FUNCTION IS USED FOR GET ORDER WEIGHT DATA AGAIST ORDER (ANIKET 21-02-2025)
    public IActionResult GetOrderWeightData(string SelectedDataIds)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("OrderData", DashBAL.GetOrderWeightData(SelectedDataIds).Tables[0]);
      //Param.Add("ErrorMesssage", Message);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION IS USED FOR RECEICED ORDER FROM VENDOR & SUBMIT WEIGHT DATA (ANIKET 21-02-2025)
    public IActionResult ReceiveOrderPreSubmit()
    {
      var Form = HttpContext.Request.Form;

      var orderNos = Form["OrderItemNo[]"];
      var GrossWeightNow = Form["GrossWeightNow[]"];
      var NetWeightNow = Form["NetWeightNow[]"];
      OrderBal.OrderSendToHO(orderNos.ToString(), NetWeightNow, GrossWeightNow);
      GenerateInvoice(orderNos.ToString());
      string HO_Name = OrderBal.GetHOName(orderNos.ToString());
      TempData["SendPreviewToHO"] = "Order Completed Send To " + HO_Name + "...!!";
      return RedirectToAction("OrdersStatus");
    }

    //THIS FUNCTION IS USED FOR GENERATE INVOICE AFTER RECEIVED ORDER FROM VENDOR (ANIKET 21-02-2025)
    public void GenerateInvoice(string OrderDetailsId)
    {
      // Initialize the report
      var report = new LocalReport();

      DataSet ds = new DataSet();
      ds = OrderBal.GetDeliveryData(OrderDetailsId);

      using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomerOrderManagement.Reports.DeliveryChallan.rdlc");
      //  report.LoadReportDefinition(rs);
      report.LoadReportDefinition(rs);
      report.EnableExternalImages = true;
      report.DataSources.Add(new ReportDataSource("DeliveryChallanDataTable", ds.Tables[0]));

      byte[] pdfBytes = report.Render("PDF");

      // Define the directory to store the PDF file
      string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
      string baseDirectoryPath = Path.Combine(wwwRootPath, "FileStorage", "DeliveryChallan");

      // Ensure the directory exists, create it if it doesn't
      if (!Directory.Exists(baseDirectoryPath))
      {
        Directory.CreateDirectory(baseDirectoryPath);
      }

      // Generate a random filename (e.g., using Guid)
      string fileName = $"DeliveryChallan_{Guid.NewGuid().ToString("N")}.pdf";

      // Define the full file path
      string pdfFilePath = Path.Combine(baseDirectoryPath, fileName);

      // Save the PDF file to disk
      System.IO.File.WriteAllBytes(pdfFilePath, pdfBytes);

      // Generate the URL to the stored file
      string fileUrl = $"/FileStorage/DeliveryChallan/{fileName}";

      OrderBal.UpdatePdfPath(OrderDetailsId, fileUrl);
      // Return the file URL as a JSON response
      //return Json(new { fileUrl = fileUrl });
    }
  }
}


