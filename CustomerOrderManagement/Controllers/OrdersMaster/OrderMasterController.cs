using BAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Reporting.NETCore;
using Microsoft.ReportingServices.Interfaces;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Security.Cryptography;


namespace CustomerOrderManagement.Controllers.OrdersMaster
{
  public class OrderMasterController : Controller
  {
    private readonly OrdersMasterBAL OrderBal;
    private readonly GlobalSessionBAL GlobalBal;
    private readonly IWebHostEnvironment _webHostEnvironment;
    // Constructor injection for LoginBAL
    public OrderMasterController(OrdersMasterBAL OrderMasterBAL, GlobalSessionBAL globalBal, IWebHostEnvironment webHostEnvironment)
    {
      OrderBal = OrderMasterBAL;
      GlobalBal = globalBal;
      _webHostEnvironment = webHostEnvironment;
    }
    public IActionResult Index()
    {
      TempData["UserRole"] = GlobalBal.GetSessionValue("RoleId");
      ViewBag.Subscription = OrderBal.CheckSubscriptionActiveOrNot();
      ViewBag.OrderID = GlobalBal.GetSessionValue("RoleId");
      ViewBag.BranchData = OrderBal.GetAllBranchData().Tables[0].AsEnumerable();
      ViewBag.ItemsData = OrderBal.GetAllItemsData().Tables[0].AsEnumerable();
      ViewBag.PurityData = OrderBal.GetAllPurityData().Tables[0].AsEnumerable();
      ViewBag.StonesData = OrderBal.GetAllActiveStones().Tables[0].AsEnumerable();
      ViewBag.StoneColorData = OrderBal.GetAllStoneColor().Tables[0].AsEnumerable();
      //ViewBag.CustomerData = OrderBal.GetAllCustomerList().Tables[0].AsEnumerable();
      return View();
    }

    //THIS FUNCTION USED FOR GET ITEM ID ALL CATEGORY AND ITEM PRODUCT GROUP
    public IActionResult GetItemIdAllCategory(string itemId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("CategoryData", OrderBal.GetAllCategoryItemId(itemId).Tables[0]);
      Param.Add("ProductData", OrderBal.GetProductNameId(itemId).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR SAVE ITEM DETAILS
    public IActionResult SaveItemDetails([FromBody] Dictionary<string, object> data)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("ItemAddedData", OrderBal.SaveOrderDetails(data).Tables[0]);
      //Param.Add("CategoryData", OrderBal.GetAllCategoryItemId(itemId).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR GET COPY AS NEW ITEM
    public IActionResult GetLastOrderItemForUsed(string OrderHeaderId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("LastAddedItemData", OrderBal.GetOrderlastAddedItemForUsed(OrderHeaderId).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR CREATE ORDER
    public IActionResult CreateOrder()
    {
      var Form = HttpContext.Request.Form;
      OrderBal.GenarateOrderNumber(Form);
      TempData["Message"] = "Order Created Successfully...!!!";
      return RedirectToAction("Dashboard", "Dashboard");
    }

    //THIS IS USED FOR EDIT ORDER DETAILS GET
    public IActionResult EditOrder(string OrderItemId)
    {
      ViewBag.OrderEditData = OrderBal.GetOrderItemIdData(OrderItemId);
      ViewBag.StoneEditData = OrderBal.GetOrderStoneData(OrderItemId);
      ViewBag.AttachmentEditData = OrderBal.GetOrderAttachmentData(OrderItemId);
      ViewBag.BranchData = OrderBal.GetAllBranchData().Tables[0].AsEnumerable();
      ViewBag.CategoryMaster = OrderBal.GetAllCategoryData().Tables[0].AsEnumerable();
      ViewBag.ItemsData = OrderBal.GetAllItemsData().Tables[0].AsEnumerable();
      ViewBag.PurityData = OrderBal.GetAllPurityData().Tables[0].AsEnumerable();
      ViewBag.StonesData = OrderBal.GetAllActiveStones().Tables[0].AsEnumerable();
      ViewBag.StoneColorData = OrderBal.GetAllStoneColor().Tables[0].AsEnumerable();
      ViewBag.CustomerData = OrderBal.GetAllCustomerList().Tables[0].AsEnumerable();
      ViewBag.VendorName = OrderBal.OrderSendVenodrsList().Tables[0].AsEnumerable();
      ViewBag.RoleId = GlobalBal.GetSessionValue("RoleId");
      return View();
    }

    //public IActionResult UpdateOrderSave()
    //{
    //  var Form = HttpContext.Request.Form;
    //  //OrderBal.OrderItemDetailsUpdate(Form);
    //  return RedirectToAction("Dashboard", "Dashboard");
    //}

    //THIS FUNCTION USED FOR CHECK CUSTOMER EXIST OR NOT
    public IActionResult CheckCustomerExistOrNot(string mobileNo)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("CustomerData", OrderBal.CustomerExistOrNot(mobileNo).Tables[0]);
      //Param.Add("CategoryData", OrderBal.GetAllCategoryItemId(itemId).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR UPDATE ORDER DETAILS
    public IActionResult UpdateOrderDetails([FromBody] Dictionary<string, object> data)
    {
      var jsondata = "";

      int result = OrderBal.UpdateOrderDetails(data);
      if (result == 0)
      {
        return Json(new { success = true });
      }
      else
      {
        return Json(new { success = false });
      }

    }

    // FETCH THE STONE DATA BASED ON THE PROVIDED ORDERITEMID
    public JsonResult GetStoneData(string orderItemId)
    {
      try
      {
        DataSet ds = OrderBal.GetOrderStoneData(orderItemId);

        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        {
          var stoneData = ds.Tables[0].AsEnumerable().Select(row => new
          {
            StoneId = row["item_name"].ToString(),
            StoneWeight = row["stone_wt"].ToString(),
            stone_category_id = row["stone_category_id"].ToString(),
            category_name = row["category_name"].ToString(),
            Pieces = row["stone_pcs"].ToString(),
            StoneColor = row["color_name"].ToString(),
            Remark = row["remark"].ToString(),
            IsActive = row["is_active"].ToString(),
            ColorID = row["ColorID"].ToString(),
            ItemID = row["ItemID"].ToString(),
            Id = row["id"].ToString(),
          }).ToList();

          return Json(stoneData);
        }
        else
        {
          return Json(new List<object>());
        }
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = ex.Message });
      }
    }

    //THIS IS USED FOR SEND ORER TO PREVIEW
    public IActionResult SendToHOForPreview([FromBody] Dictionary<string, object> data)
    {
      string HO_Name = OrderBal.SendToOrderPreview(data);
      TempData["SendPreviewToHO"] = "Order Send To " + HO_Name + " For Preview";
      // Ensure action and controller names are correct
      string redirectUrl = Url.Action("VendorDashboard", "Dashboard");
      if (string.IsNullOrEmpty(redirectUrl))
      {
        return Json(new { success = false, message = "URL generation failed." });
      }
      return Json(new { success = true, redirectUrl });
    }

    //THIS IS USED FOR SEND ORDER TO HO
    public IActionResult OrderSendToHO(string OrderHeaderId)
    {
      string Ho_Name = OrderBal.GetHOName(OrderHeaderId);
      OrderBal.OrderSendToHO(OrderHeaderId);
      GenerateInvoice(OrderHeaderId);
      TempData["SendPreviewToHO"] = "Order Completed...Send To " + Ho_Name + "";
      // Ensure action and controller names are correct
      string redirectUrl = Url.Action("VendorDashboard", "Dashboard");
      if (string.IsNullOrEmpty(redirectUrl))
      {
        return Json(new { success = false, message = "URL generation failed." });
      }
      return Json(new { success = true, redirectUrl });
    }

    //THIS IS USED FOR GENERATE DELIVERY CHALLAN
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

    //THIS IS USED FOR PARTIAL VIEW OPEN FOE ADD NEW CUSTOMER
    public IActionResult GetPartialView(string MobileNo)
    {
      TempData["MobileNoGet"] = MobileNo;
      ViewBag.CountryID = OrderBal.GetCountryID().Tables[0].AsEnumerable();
      ViewBag.StateID = OrderBal.GetStateID().Tables[0].AsEnumerable();
      ViewBag.CityID = OrderBal.GetCityID().Tables[0].AsEnumerable();
      ViewBag.TenantID = OrderBal.GettenantID().Tables[0].AsEnumerable();
      return PartialView("_CustomerAddPartialView");
    }

    //THIS IS USED FOR ADD NEW CUSTOMER FROM CREATE ORDER PAGE
    public ActionResult SaveAddCustomerNew()
    {
      var Form = HttpContext.Request.Form;
      int Id = OrderBal.SaveAddCustomer(Form);
      TempData["Message"] = "User Added Successfully !!!!!";

      TempData["CustomerId"] = Id.ToString();
      TempData["MobileNumber"] = Form["Mobileno"].ToString();
      TempData["CustomerName"] = Form["CustomerName"].ToString();
      return RedirectToAction("Index", "OrderMaster");
    }

    //THIS IS USED FOR GET HISTORY
    public IActionResult GetHistoryData(string OrderDetailsId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("HistoryData", OrderBal.GetHistoryData(OrderDetailsId).Tables[0]);
      //Param.Add("CategoryData", OrderBal.GetAllCategoryItemId(itemId).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);

    }

    //THIS IS USED FOR SEND JOB CARD TO CUSTOMER
    public IActionResult ShareJobCardDetailsTocustomer()
    {
      var Form = HttpContext.Request.Form;
      // Initialize the report
      var report = new LocalReport();

      DataSet ds = OrderBal.GetJobCardDetailsForSend(Form["OrderDetailsId"]);
      DataSet ds2 = OrderBal.GetStonePrintData(Form["OrderDetailsId"]);

      // Get the physical path of wwwroot directory
      string wwwRootPath = _webHostEnvironment.WebRootPath;

      using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomerOrderManagement.Reports.JobCard.rdlc");

      report.LoadReportDefinition(rs);
      report.EnableExternalImages = true;

      report.DataSources.Add(new ReportDataSource("JobCardDataTable", ds.Tables[0]));
      report.DataSources.Add(new ReportDataSource("JobCardStoneData", ds2.Tables[0]));

      var table = new DataTable();

      table.Columns.Add("Id", typeof(int));
      table.Columns.Add("imageBase64", typeof(string));

      if (ds != null && ds.Tables[0].Rows.Count > 0)
      {
        // Create a list to store report parameters
        var reportParameters = new List<ReportParameter>();

        // Loop through each row in the dataset
        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        {
          // Get the image path for the current row
          string relativePath = ds.Tables[0].Rows[i]["path"].ToString();

          if (!string.IsNullOrEmpty(relativePath))
          {
            // Combine the relative path with the web root path to get the full image path
            string imagePath = Path.Combine(wwwRootPath, relativePath.TrimStart('~', '/').Replace('/', '\\'));

            try
            {
              // Load the image and convert to base64
              using (var b = new Bitmap(imagePath))
              using (var ms = new MemoryStream())
              {
                b.Save(ms, ImageFormat.Bmp);

                string imageBase64 = Convert.ToBase64String(ms.ToArray());

                table.Rows.Add(i, imageBase64);
                // Add the base64 string as a report parameter
                //reportParameters.Add(new ReportParameter($"ImagePath{i + 1}", imageBase64));
              }
            }
            catch (Exception ex)
            {
              // Log or handle errors (e.g., image not found or invalid format)
              Console.WriteLine($"Error processing image at row {i}: {ex.Message}");
            }
          }
        }
        //if (table.Rows.Count > 0)
        //{
        report.DataSources.Add(new ReportDataSource("JobCardStoneImageData", table));
        //}
        // Set all the parameters at once
        if (reportParameters.Any())
        {
          report.SetParameters(reportParameters);
        }
      }

      byte[] pdfBytes = report.Render("PDF");

      string baseDirectoryPath = Path.Combine(wwwRootPath, "FileStorage", "JobCard");

      // Ensure the directory exists, create it if it doesn't
      if (!Directory.Exists(baseDirectoryPath))
      {
        Directory.CreateDirectory(baseDirectoryPath);
      }

      // Generate a random filename (e.g., using Guid)
      string fileName = $"JobCard_{Guid.NewGuid().ToString("N")}.pdf";

      // Define the full file path
      string pdfFilePath = Path.Combine(baseDirectoryPath, fileName);

      // Save the PDF file to disk
      System.IO.File.WriteAllBytes(pdfFilePath, pdfBytes);

      // Generate the URL to the stored file
      string fileUrl = $"/FileStorage/JobCard/{fileName}";


      OrderBal.GetCustomerMobileNumber(Form["OrderDetailsId"], fileUrl);

      if(GlobalBal.GetSessionValue("RoleId") == "4")
      {
        // Return the file URL as a JSON response
        return RedirectToAction("SalePersonDashboard", "Dashboard");
      }
      else
      {
        // Return the file URL as a JSON response
        return RedirectToAction("OrdersStatus", "Dashboard");
      }
       
    }

    //THIS IS USED FOR HO REJECTED ORDERS WITH REMARK/NARRATIONS
    public IActionResult HO_OrderRejected()
    {
      var Form = HttpContext.Request.Form;
      OrderBal.HO_OrderRejected(Form);
      TempData["Message"] = "Order Rejected Successfully !!!!!";
      return RedirectToAction("OrdersStatus", "Dashboard");
    }

    //THIS FUNCTION USED FOR VENDOR ORDER REJECTED WITH REMARK
    public IActionResult VendorRejected()
    {
      var Form = HttpContext.Request.Form;
      OrderBal.VendorOrderRejected(Form);
      TempData["VendorOrderReject"] = "Order Rejected Successfully...!!";
      return RedirectToAction("VendorOrderStatus", "Dashboard");
    }

    //THIS FUNCTION USED FOR VENDOR ORDER REJECTED WITH REMARK
    public IActionResult VendorFromNewOrdersRejected()
    {
      var Form = HttpContext.Request.Form;
      OrderBal.VendorOrderRejected(Form);
      TempData["VendorOrderReject1"] = "Order Rejected Successfully...!!";
      return RedirectToAction("VendorNewOrders", "Dashboard");
    }

    //VENDOR GET JOB CARD SHOW DETAILS
    public IActionResult GetJobcardDetails(string ItemId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("OrderData", OrderBal.GetJobCardDetails(ItemId).Tables[0]);
      Param.Add("ImageData", OrderBal.GetJobCardDetailsImage(ItemId).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR UPDATE ORDER ACCEPT OR REJECT
    public IActionResult UpdateOrderStatusInEdit(string itemId, string Flag)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      OrderBal.OrderAcceptOrRejectInEditView(itemId, Flag);
      if (Flag == "1")
      {
        TempData["EditOrderMessage"] = "Order Accepted Successfully...!!";
      }
      else
      {
        TempData["EditOrderMessage"] = "Order Rejected Successfully...!!";
      }
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR DIRECT ORDER SEND TO BACKOFFICE
    public IActionResult OrderDirectSentToBackOffice([FromBody] Dictionary<string, object> data)
    {
      var jsondata = "";
      int OrederItemId = OrderBal.OrderSendToBackOffice(data);
      GenerateInvoice(OrederItemId.ToString());
      string HO_Name = OrderBal.GetHOName(OrederItemId.ToString());
      TempData["SendPreviewToHO"] = "Order Completed Send To " + HO_Name + "...!!";
      return Json(jsondata);
    }

    //THIS IS USED FOR GET NOTIFICATION
    public IActionResult GetAllNotifications(string Flag)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("Notification", OrderBal.GetAllNotifications(Flag).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }


    //THIS IS USED FOR UPLAOD ORDER DATA FROM EXCEL
    [HttpPost]
    public async Task<IActionResult> UploadOrderData(IFormFile file, IList<IFormFile> imageFiles)
    {
      if (file == null || file.Length == 0)
      {
        TempData["ErrorMessage"] = "Please upload a valid Excel file.";
        return RedirectToAction("Index");
      }

      DateTime today2 = DateTime.UtcNow.Date;

      string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FileStorage", "Preview_Attachment", today2.ToString("yyyy-MM-dd"));


      // ENSURE THE FOLDER EXISTS
      if (!Directory.Exists(uploadFolder))
      {
        Directory.CreateDirectory(uploadFolder);
      }

      // Save each image from the imageFiles list
      Dictionary<string, string> savedImagePaths = new Dictionary<string, string>();

      foreach (var imageFile in imageFiles)
      {
        // Generate a path for the image file
        string imageFilePath = Path.Combine(uploadFolder, imageFile.FileName);
        string imgPath = Path.Combine(imageFilePath, imageFile.FileName.Replace(" ", "-"));
        string imageName = imageFile.FileName.ToString().Replace(" ", "_");
        string formattedDate = DateTime.Now.ToString("yyyy-MM-dd");
        string relativePath = "~/" + Path.Combine("FileStorage", "Preview_Attachment", formattedDate, imageName).Replace("\\", "/");
        // Save the image to the folder
        using (var stream = new FileStream(imageFilePath, FileMode.Create))
        {
          await imageFile.CopyToAsync(stream);
        }

        // Add the image path to the list (you may want to save these paths in the database)
        savedImagePaths.Add(imageFile.FileName.ToUpper(), relativePath);
      }

      try
      {
        // SET THE LICENSE CONTEXT FOR EPPLUS
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // CREATE A NEW EXCEL PACKAGE TO STORE RESULTS
        using (var package = new ExcelPackage(file.OpenReadStream()))
        {
          List<string> AllheaderRow = new List<string>();


          for (int i = 0; i <= 2; i++)
          {
            var worksheet = package.Workbook.Worksheets[i];

            // GET THE HEADER ROW (FIRST ROW OF THE SHEET)
            var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column]
                            .Select(cell => cell.Text?.Trim())
                            .ToList();
            AllheaderRow.AddRange(headerRow);
          }


          // DEFINE THE REQUIRED HEADERS
          var requiredHeaders = new[] { "Order_Sequence", "branch_name", "customer_mobileno", "order_delivery_date", "Item_Sequence", "item_name", "category_name", "purity", "net_wt", "size", "design_code", "pcs", "image_name", "reference_barcode", "Stone_Sequence", "stone_name", "stone_category", "stone_wt", "stone_pcs", "stone_color" };

          // CHECK IF ALL REQUIRED HEADERS ARE PRESENT
          if (!requiredHeaders.All(header => AllheaderRow.Contains(header, StringComparer.OrdinalIgnoreCase)))
          {
            return Json(new
            {
              success = false,
              message = "Invalid Excel format.Required headers are missing..........."
            });
          }
          List<string> SuccessOrder = new List<string>();

          #region All Tables

          //ORDER HEADER DATATABLE
          var OrderHeaderworksheet = package.Workbook.Worksheets[0];

          DataTable dtOrderHeader = new DataTable();

          int totalColumnsOrderHeader = OrderHeaderworksheet.Dimension.End.Column;


          // Add "Status" header
          OrderHeaderworksheet.Cells[1, totalColumnsOrderHeader + 1].Value = "Status";

          // Update lastColumn after adding "Status"
          //totalColumnsOrderHeader++;

          // Add "Order_No" header
          //OrderHeaderworksheet.Cells[1, totalColumnsOrderHeader + 1].Value = "Order_No";

          for (int col = 1; col <= totalColumnsOrderHeader; col++)
          {
            string columnName = OrderHeaderworksheet.Cells[1, col].Text;
            dtOrderHeader.Columns.Add(columnName);
          }

          if (!dtOrderHeader.Columns.Contains("SerialNo"))
          {
            dtOrderHeader.Columns.Add("SerialNo", typeof(int));
          }


          //ORDER DETAILS DATATABLE
          var OrderDetailsworksheet = package.Workbook.Worksheets[1];
          DataTable dtOrderDetails = new DataTable();
          DataTable dtOrderDetailsvalidate = new DataTable();

          int totalColumnsOrderDetails = OrderDetailsworksheet.Dimension.End.Column;

          // Add "Status" header
          OrderDetailsworksheet.Cells[1, totalColumnsOrderDetails + 1].Value = "Status";

          for (int col = 1; col <= totalColumnsOrderDetails; col++)
          {
            string columnName = OrderDetailsworksheet.Cells[1, col].Text;
            dtOrderDetails.Columns.Add(columnName);
            dtOrderDetailsvalidate.Columns.Add(columnName);
          }

          if (!dtOrderDetails.Columns.Contains("SerialNo"))
          {
            dtOrderDetails.Columns.Add("SerialNo", typeof(int));
          }
          if (!dtOrderDetails.Columns.Contains("gross_wt"))
          {
            dtOrderDetails.Columns.Add("gross_wt", typeof(decimal));
          }
          if (!dtOrderDetails.Columns.Contains("Image_path"))
          {
            dtOrderDetails.Columns.Add("Image_path", typeof(string));
          }

          //STONE DETAILS DATATABLE
          var StoneDetailsworksheet = package.Workbook.Worksheets[2];

          DataTable dtStoneDetails = new DataTable();
          DataTable dtStoneDetailsValidate = new DataTable();


          int totalColumnsStoneDetails = StoneDetailsworksheet.Dimension.End.Column;

          StoneDetailsworksheet.Cells[1, totalColumnsStoneDetails + 1].Value = "Status";

          for (int col = 1; col <= totalColumnsStoneDetails; col++)
          {
            string columnName = StoneDetailsworksheet.Cells[1, col].Text;
            dtStoneDetails.Columns.Add(columnName);
            dtStoneDetailsValidate.Columns.Add(columnName);
          }

          if (!dtStoneDetails.Columns.Contains("SerialNo"))
          {
            dtStoneDetails.Columns.Add("SerialNo", typeof(int));
          }

          #endregion

          for (int row = 2; row <= OrderHeaderworksheet.Dimension.End.Row; row++)
          {
            int errorcount = 0;
            string AllErrorMsg = "";
            DataRow RowOrderHeader = dtOrderHeader.NewRow();

            for (int col = 1; col <= totalColumnsOrderHeader; col++)
            {
              RowOrderHeader[col - 1] = OrderHeaderworksheet.Cells[row, col].Text;
            }

            // ADD THE POPULATED ROW TO THE DATATABLE
            dtOrderHeader.Rows.Add(RowOrderHeader);

            string id_Order_Header = OrderHeaderworksheet.Cells[row, 1].Text;
            List<string> list_id_order_details = new List<string>();


            //ADD ITEMS DATA IN DATATABLE WITH RESPECT TO HEADER ID
            for (int OrderDetailsrowCount = 2; OrderDetailsrowCount <= OrderDetailsworksheet.Dimension.End.Row; OrderDetailsrowCount++)
            {
              var cellValue = OrderDetailsworksheet.Cells[OrderDetailsrowCount, 2].Text;
              string id_order_details = OrderDetailsworksheet.Cells[OrderDetailsrowCount, 1].Text;

              if (cellValue == id_Order_Header)
              {
                DataRow RowOrderDetails = dtOrderDetails.NewRow();

                for (int col = 1; col <= totalColumnsOrderDetails; col++)
                {
                  RowOrderDetails[col - 1] = OrderDetailsworksheet.Cells[OrderDetailsrowCount, col].Text;

                }
                RowOrderDetails["SerialNo"] = OrderDetailsrowCount;
                dtOrderDetails.Rows.Add(RowOrderDetails);
                list_id_order_details.Add(id_order_details);
              }

              if (string.IsNullOrEmpty(cellValue) || string.IsNullOrEmpty(id_order_details))
              {
                OrderDetailsworksheet.Cells[OrderDetailsrowCount, 13].Value = "Order Sequence Or Item Sequence is invalid";
                var orderErrorRow = OrderDetailsworksheet.Cells[OrderDetailsrowCount, 1, OrderDetailsrowCount, 13];
                orderErrorRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                orderErrorRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                orderErrorRow.Style.Font.Color.SetColor(System.Drawing.Color.White);
              }
            }

            //ADD STONE DATA IN DATATABLE WITH RESPECT TO HEADER ID & ITEMHEADERID
            foreach (string id_order_detail in list_id_order_details)
            {
              for (int StoneDetailsrowCount = 2; StoneDetailsrowCount <= StoneDetailsworksheet.Dimension.End.Row; StoneDetailsrowCount++)
              {
                var cellValue1 = StoneDetailsworksheet.Cells[StoneDetailsrowCount, 3].Text;
                var cellValue2 = StoneDetailsworksheet.Cells[StoneDetailsrowCount, 2].Text;
                string id_order_details = StoneDetailsworksheet.Cells[StoneDetailsrowCount, 1].Text;

                if (cellValue1 == id_Order_Header && cellValue2 == id_order_detail)
                {
                  DataRow RowStoneDetails = dtStoneDetails.NewRow();

                  for (int col = 1; col <= totalColumnsStoneDetails; col++)
                  {
                    RowStoneDetails[col - 1] = StoneDetailsworksheet.Cells[StoneDetailsrowCount, col].Text;
                  }
                  RowStoneDetails["SerialNo"] = StoneDetailsrowCount;

                  dtStoneDetails.Rows.Add(RowStoneDetails);
                }
                if (string.IsNullOrEmpty(cellValue1) || string.IsNullOrEmpty(cellValue2) || string.IsNullOrEmpty(id_order_details))
                {
                  StoneDetailsworksheet.Cells[StoneDetailsrowCount, 8].Value = "Order Sequence,Stone_Sequence Or Item Sequence is invalid";
                  var errorRow = StoneDetailsworksheet.Cells[StoneDetailsrowCount, 1, StoneDetailsrowCount, 8];
                  errorRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                  errorRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                  errorRow.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
              }
            }

            //VALIDATE CUSTOMER DETAILS
            string BranchID = OrderBal.ValidateIDs("ta_branch_master", "branch_name", dtOrderHeader.Rows[0]["branch_name"].ToString());
            string customer_id = OrderBal.ValidateIDs("ta_customer_master", "mobile_no", dtOrderHeader.Rows[0]["customer_mobileno"].ToString());

            DateTime today = DateTime.UtcNow.Date;  // Get today's UTC date (without time)
            DateTime deliveryDate;
            bool isValidDate = false;  // Default to invalid

            // Define the expected date formats, including dd-MM-yyyy
            string[] expectedFormats = { "dd-MM-yyyy", "yyyy-MM-dd", "dd/MM/yyyy" };  // Add the formats that might be used
            string orderDeliveryDateString = dtOrderHeader.Rows[0]["order_delivery_date"].ToString();


            // Try parsing the date with the expected formats
            isValidDate = DateTime.TryParseExact(
                orderDeliveryDateString,  // The raw date string
                expectedFormats,          // Formats to try
                System.Globalization.CultureInfo.InvariantCulture,  // Ignore culture-specific settings
                System.Globalization.DateTimeStyles.None,  // No special date styles
                out deliveryDate          // Output parsed date
            );

            if (isValidDate)
            {
              // Now that the date is parsed, check if it's >= today
              if (deliveryDate >= today)
              {
                isValidDate = true;
                string formattedDate = deliveryDate.ToString("yyyy-MM-dd");
                dtOrderHeader.Rows[0]["order_delivery_date"] = formattedDate;
              }
              else
              {
                isValidDate = false;
              }
            }
            else
            {
              // If parsing failed, set the validation flag to false
              isValidDate = false;
            }

            if (!string.IsNullOrEmpty(BranchID) && !string.IsNullOrEmpty(BranchID) && !string.IsNullOrEmpty(customer_id) && !string.IsNullOrEmpty(dtOrderHeader.Rows[0]["Order_Sequence"].ToString()) && isValidDate)
            {
              dtOrderHeader.Rows[0]["branch_name"] = BranchID;
              dtOrderHeader.Rows[0]["customer_mobileno"] = customer_id;


              //VALIDATE STONE DETAILS
              if (dtStoneDetails.Rows.Count > 0 && dtStoneDetails != null)
              {
                foreach (DataRow currentRow in dtStoneDetails.Rows)
                {

                  Dictionary<string, string> dictextraField = new Dictionary<string, string>();
                  dictextraField.Add("is_stone", "1");
                  string stoneID = OrderBal.ValidateIDs("ta_item_master", "item_name", currentRow["stone_name"].ToString(), "0", dictextraField);
                  string StonecolorId = OrderBal.ValidateIDs("ta_stone_color_master", "color_name", currentRow["stone_color"].ToString());
                  dictextraField.Clear();
                  dictextraField.Add("category_name", currentRow["stone_category"].ToString());
                  string Stonecategory = OrderBal.ValidateIDs("ta_category_master", "item_id", stoneID, "0",dictextraField);

                  bool stonewtvalid = true;
                  bool stonewtpcs = true;
                  bool id_order_headerstone = true;
                  bool id_order_detailstone = true;

                  try
                  {
                    //THIS IS CHANGES BY ROHIT PAWAR
                    if (string.IsNullOrEmpty(currentRow["stone_wt"].ToString()) || Convert.ToDecimal(currentRow["stone_wt"]) < 0)
                    {
                      stonewtvalid = false;
                    }
                  }
                  catch (Exception ex)
                  {
                    stonewtvalid = false;

                  }

                  try
                  {
                    if (string.IsNullOrEmpty(currentRow["stone_pcs"].ToString()) || Convert.ToInt32(currentRow["stone_pcs"]) <= 0)
                    {
                      stonewtpcs = false;
                    }
                  }
                  catch (Exception ex)
                  {
                    stonewtpcs = false;

                  }

                  try
                  {
                    if (string.IsNullOrEmpty(currentRow["Order_Sequence"].ToString()) || Convert.ToInt32(currentRow["Order_Sequence"]) <= 0)
                    {
                      id_order_headerstone = false;
                    }
                  }
                  catch (Exception ex)
                  {
                    id_order_headerstone = false;

                  }

                  try
                  {
                    if (string.IsNullOrEmpty(currentRow["Item_Sequence"].ToString()) || Convert.ToInt32(currentRow["Item_Sequence"]) <= 0)
                    {
                      id_order_detailstone = false;
                    }
                  }
                  catch (Exception ex)
                  {
                    id_order_detailstone = false;

                  }

                  if (!string.IsNullOrEmpty(stoneID) && !string.IsNullOrEmpty(Stonecategory)
                  && stonewtvalid && stonewtpcs && id_order_headerstone && id_order_detailstone)
                  {
                    currentRow["stone_name"] = stoneID;
                    currentRow["stone_color"] = StonecolorId;
                    currentRow["stone_category"] = Stonecategory;
                  }
                  else
                  {
                    errorcount++;
                    string errorMessage = "Data validation failed:";
                    if (string.IsNullOrEmpty(stoneID))
                    {
                      errorMessage += " Invalid stone name: " + currentRow["stone_name"].ToString() + " ";
                    }

                    if (string.IsNullOrEmpty(Stonecategory))
                    {
                      errorMessage += " Invalid stone category: " + currentRow["stone_category"].ToString() + " ";
                    }

                    if (!stonewtvalid)
                    {
                      errorMessage += " Invalid or missing stone weight: " + currentRow["stone_wt"].ToString() + " ";
                    }

                    if (!stonewtpcs)
                    {
                      errorMessage += " Invalid or missing stone pieces: " + currentRow["stone_pcs"].ToString() + " ";
                    }

                    if (!id_order_headerstone)
                    {
                      errorMessage += " Invalid or missing Order Sequence. ";
                    }

                    if (!id_order_detailstone)
                    {
                      errorMessage += " Invalid or missing Item Sequence. ";
                    }
                    AllErrorMsg += errorMessage + ",";

                    StoneDetailsworksheet.Cells[Convert.ToInt32(currentRow["SerialNo"].ToString()), 9].Value = errorMessage;
                    var errorRow = StoneDetailsworksheet.Cells[Convert.ToInt32(currentRow["SerialNo"].ToString()), 1, Convert.ToInt32(currentRow["SerialNo"].ToString()), 9];
                    errorRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    errorRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                    errorRow.Style.Font.Color.SetColor(System.Drawing.Color.White);

                    OrderHeaderworksheet.Cells[row, 5].Value = errorMessage;
                    var orderErrorRow = OrderHeaderworksheet.Cells[row, 1, row, 5];
                    orderErrorRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    orderErrorRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                    orderErrorRow.Style.Font.Color.SetColor(System.Drawing.Color.White);

                    //goto skipiteration;
                  }
                }
              }

              // VALIDATE ORDER ITEM DETAILS
              if (dtOrderDetails.Rows.Count > 0 && dtOrderDetails != null)
              {
                foreach (DataRow currentRow in dtOrderDetails.Rows)
                {
                  string itemID = OrderBal.ValidateIDs("ta_item_master", "item_name", currentRow["item_name"].ToString());
                  Dictionary<string, string> dictextraField = new Dictionary<string, string>();
                  dictextraField.Add("item_id", itemID);
                  string categoryID = OrderBal.ValidateIDs("ta_category_master", "category_name", currentRow["category_name"].ToString(), "0", dictextraField);
                  string PurityID = OrderBal.ValidateIDs("ta_purity_master", "purity", currentRow["purity"].ToString());
                  string errorMessage = "Data validation failed:";
                  string[] ImageNames = currentRow["image_name"]?.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                  bool imgvalid = false;
                  int imgCount = ImageNames.Length;
                  if (ImageNames.Length > 0 && imgCount <= 5)
                  {
                    string combinedPaths = string.Empty;
                    imgvalid = true;

                    foreach (string name in ImageNames)
                    {
                      string upperName = name.Trim().ToUpper();
                      if (savedImagePaths.ContainsKey(upperName))
                      {
                        if (!string.IsNullOrEmpty(combinedPaths))
                        {
                          combinedPaths += ",";
                        }
                        combinedPaths += savedImagePaths[upperName];
                      }
                      else
                      {
                        imgvalid = false;
                        break;
                      }
                    }

                    if (imgvalid)
                    {
                      currentRow["Image_path"] = combinedPaths;
                    }
                  }
                  else
                  {
                    imgvalid = true;
                  }

                  bool StoneCountStatus = true;

                  var FindStones = dtStoneDetails.AsEnumerable()
                        .Where(row => row.Field<string>("Order_Sequence") == currentRow["Order_Sequence"].ToString()
                            && row.Field<string>("Item_Sequence") == currentRow["Item_Sequence"].ToString())
                        .ToList();

                  int StoneCount = FindStones.Count;

                  if (StoneCount > 10)
                  {
                    StoneCountStatus = false;
                  }

                  if (StoneCount > 0)
                  {
                    try
                    {
                      //THIS IS CHANGES BY ROHIT PAWAR
                      if (dtStoneDetails.Rows.Count > 0)
                      {
                        double StoneWt = 0;
                        for (int a = 0;a< dtStoneDetails.Rows.Count; a++)
                        {
                          StoneWt += OrderBal.StoreCaratConvertIntoGram(Convert.ToDouble(dtStoneDetails.Rows[a]["stone_wt"]), dtStoneDetails.Rows[a]["stone_name"].ToString());
                        }

                        double netWeight = Convert.ToDouble(currentRow["net_wt"]);
                        double grossWeight = netWeight + StoneWt;
                        currentRow["gross_wt"] = grossWeight;
                      }
                    }
                    catch(Exception ex)
                    {

                    }

                  }
                  else
                  {
                    if(!string.IsNullOrEmpty(currentRow["net_wt"].ToString()) && Convert.ToDecimal(currentRow["net_wt"]) > 0)
                    {
                      currentRow["gross_wt"] = currentRow["net_wt"];
                    }
                  }

                  bool isValid = !string.IsNullOrEmpty(itemID) && !string.IsNullOrEmpty(categoryID) && !string.IsNullOrEmpty(PurityID)
                                 && !string.IsNullOrEmpty(currentRow["net_wt"].ToString()) && Convert.ToDecimal(currentRow["net_wt"]) > 0
                                 && !string.IsNullOrEmpty(currentRow["pcs"].ToString()) && Convert.ToInt32(currentRow["pcs"]) > 0
                                 && !string.IsNullOrEmpty(currentRow["Item_Sequence"].ToString())
                                 && !string.IsNullOrEmpty(currentRow["Order_Sequence"].ToString())
                                 && Convert.ToDecimal(currentRow["net_wt"]) <= Convert.ToDecimal(currentRow["gross_wt"])
                                 && StoneCountStatus && imgvalid && imgCount <= 5;

                  if (isValid)
                  {
                    currentRow["item_name"] = itemID;
                    currentRow["category_name"] = categoryID;
                    currentRow["purity"] = PurityID;
                  }
                  else
                  {
                    errorcount++;


                    if (string.IsNullOrEmpty(itemID)) errorMessage += " Invalid item name: " + currentRow["item_name"].ToString() + " ";
                    if (string.IsNullOrEmpty(categoryID)) errorMessage += " Invalid category name: " + currentRow["category_name"].ToString() + " ";
                    if (string.IsNullOrEmpty(PurityID)) errorMessage += " Invalid purity: " + currentRow["purity"].ToString() + " ";

                    if (!string.IsNullOrEmpty(currentRow["net_wt"].ToString()))
                    {
                      try
                      {
                        if (string.IsNullOrEmpty(currentRow["net_wt"].ToString()) || Convert.ToDecimal(currentRow["net_wt"]) <= 0)
                          errorMessage += " Invalid or missing net weight: " + currentRow["net_wt"].ToString() + " ";
                      }
                      catch
                      {
                        errorMessage += " Invalid or missing net weight: " + currentRow["net_wt"].ToString() + " ";
                      }

                    }
                    else
                    {
                      errorMessage += " Invalid or missing net weight: " + currentRow["net_wt"].ToString() + " ";
                    }

                    //if (string.IsNullOrEmpty(currentRow["size"].ToString()) || Convert.ToDecimal(currentRow["size"]) <= 0)
                    //  errorMessage += " Invalid or missing size: " + currentRow["size"].ToString() + " ";

                    if (string.IsNullOrEmpty(currentRow["pcs"].ToString()) || Convert.ToInt32(currentRow["pcs"]) <= 0)
                      errorMessage += " Invalid or missing pcs: " + currentRow["pcs"].ToString() + " ";

                    if (string.IsNullOrEmpty(currentRow["Item_Sequence"].ToString()))
                      errorMessage += " Missing or invalid Item Sequence. ";

                    if (string.IsNullOrEmpty(currentRow["Order_Sequence"].ToString()))
                      errorMessage += " Missing or invalid Order Sequence. ";

                    //if (string.IsNullOrEmpty(currentRow["design_code"].ToString()))
                    //  errorMessage += " Missing or invalid design code. ";

                    if (!StoneCountStatus)
                      errorMessage += " More Than 10 Stone against this item. ";

                    if (!imgvalid)
                      errorMessage += " Images not found against this item. ";

                    if (imgCount > 5)
                      errorMessage += " Upload Image upto 5 not more.... ";

                    AllErrorMsg += errorMessage + ",";

                    int serialNo = Convert.ToInt32(currentRow["SerialNo"].ToString());
                    OrderDetailsworksheet.Cells[serialNo, 13].Value = errorMessage;
                    var errorRow = OrderDetailsworksheet.Cells[serialNo, 1, serialNo, 13];
                    errorRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    errorRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                    errorRow.Style.Font.Color.SetColor(System.Drawing.Color.White);

                    OrderHeaderworksheet.Cells[row, 5].Value = errorMessage;
                    var orderErrorRow = OrderHeaderworksheet.Cells[row, 1, row, 5];
                    orderErrorRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    orderErrorRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                    orderErrorRow.Style.Font.Color.SetColor(System.Drawing.Color.White);

                    //goto skipiteration;
                  }
                }
              }

            }
            else
            {
              errorcount++;
              string errorMessage = "Data validation failed:";

              if (string.IsNullOrEmpty(BranchID))
              {
                errorMessage += "Invalid Branch Name: " + dtOrderHeader.Rows[0]["branch_name"].ToString() + " ";
              }

              if (string.IsNullOrEmpty(customer_id))
              {
                errorMessage += "Invalid Customer Mobile Number: " + dtOrderHeader.Rows[0]["customer_mobileno"].ToString() + " ";
              }

              if (!isValidDate)
              {
                errorMessage += "Invalid Delivery Date: " + dtOrderHeader.Rows[0]["order_delivery_date"].ToString() + " ";
              }

              if (string.IsNullOrEmpty(dtOrderHeader.Rows[0]["Order_Sequence"].ToString()))
              {
                errorMessage += " Missing or invalid order sequence. ";
              }

              AllErrorMsg += errorMessage + ",";

              OrderHeaderworksheet.Cells[row, 5].Value = AllErrorMsg;
              var orderErrorRow = OrderHeaderworksheet.Cells[row, 1, row, 5];
              orderErrorRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
              orderErrorRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
              orderErrorRow.Style.Font.Color.SetColor(System.Drawing.Color.White);

              //goto skipiteration;

            }

            //if(er)

            if (errorcount == 0 && dtOrderHeader.Rows.Count > 0 && dtOrderDetails.Rows.Count > 0)
            {
              //AFTER VALIDATION STORED DATA IN DATABASE AND AFTER GENERATE ORDER
              string OrderHeaderID = OrderBal.InsertIntoOrderInHeaderTable(dtOrderHeader).ToString();
              dtOrderHeader.Rows[0]["Order_Sequence"] = OrderHeaderID;
              Dictionary<string, string> OrderDetailsID = OrderBal.InsertIntoOrderDetailsTable(dtOrderDetails, OrderHeaderID);
              OrderBal.InsertOrderStoneDetailsTable(dtStoneDetails, OrderHeaderID, OrderDetailsID);
              string OrderNo = OrderBal.GenerateOrderNumberTable(dtOrderHeader);
              //string OrderNo = "CO-" + OrderHeaderID;
              //OrderHeaderworksheet.Cells[row, 8].Value = OrderNo;
              //OrderHeaderworksheet.Cells[row, 4].Value = DateTime.Now.ToString("dd-MM-yyyy");
              SuccessOrder.Add(id_Order_Header);
            }
            else
            {
              if (string.IsNullOrEmpty(AllErrorMsg))
              {
                AllErrorMsg = "Item Is Not find against this order";
              }
              OrderHeaderworksheet.Cells[row, 5].Value = AllErrorMsg;
              var orderErrorRow = OrderHeaderworksheet.Cells[row, 1, row, 5];
              orderErrorRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
              orderErrorRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
              orderErrorRow.Style.Font.Color.SetColor(System.Drawing.Color.White);
            }


            //skipiteration:;

            //CLEAR ALL REQUIRED TABLE
            dtOrderHeader.Rows.Clear();
            dtStoneDetails.Rows.Clear();
            dtOrderDetails.Rows.Clear();
            dtOrderHeader.Rows.Clear();
            dtStoneDetailsValidate.Rows.Clear();
            dtOrderDetailsvalidate.Rows.Clear();
          }

          for (int OrderHeadersrowCount = 2; OrderHeadersrowCount <= OrderHeaderworksheet.Dimension.End.Row; OrderHeadersrowCount++)
          {
            string cellValue = OrderHeaderworksheet.Cells[OrderHeadersrowCount, 1].Text;

            if (SuccessOrder.Contains(cellValue))
            {
              OrderHeaderworksheet.DeleteRow(OrderHeadersrowCount);

              OrderHeadersrowCount--;
            }
          }

          for (int OrderDetailsrowCount = 2; OrderDetailsrowCount <= OrderDetailsworksheet.Dimension.End.Row; OrderDetailsrowCount++)
          {
            string cellValue = OrderDetailsworksheet.Cells[OrderDetailsrowCount, 2].Text;
            string id_order_details = OrderDetailsworksheet.Cells[OrderDetailsrowCount, 1].Text;

            if (SuccessOrder.Contains(cellValue))
            {
              OrderDetailsworksheet.DeleteRow(OrderDetailsrowCount);

              OrderDetailsrowCount--;
            }
          }

          for (int StoneDetailsrowCount = 2; StoneDetailsrowCount <= StoneDetailsworksheet.Dimension.End.Row; StoneDetailsrowCount++)
          {
            string cellValue = StoneDetailsworksheet.Cells[StoneDetailsrowCount, 3].Text;

            if (SuccessOrder.Contains(cellValue))
            {
              StoneDetailsworksheet.DeleteRow(StoneDetailsrowCount);

              StoneDetailsrowCount--;
            }
          }

          int totalColuOrderHeader = OrderHeaderworksheet.Dimension.End.Row;
          int totalColuOrderDetails = OrderDetailsworksheet.Dimension.End.Row;
          int totalColuStoneDetails = StoneDetailsworksheet.Dimension.End.Row;

          // THIS BELOW CODE IS USE FOR RETURN SHEET WITH STATUS 
          var memoryStream = new MemoryStream();
          package.SaveAs(memoryStream);
          memoryStream.Position = 0;
          memoryStream.Seek(0, SeekOrigin.Begin);

          // Convert the file content into a byte array
          byte[] fileBytes = memoryStream.ToArray();
          string fileName = "OrderStatus_" + DateTime.Now.ToString("dd-MM-yy_HHmmss") + ".xlsx";
          if (totalColuOrderHeader > 1 || totalColuOrderDetails > 1 || totalColuStoneDetails > 1)
          {
            return Json(new
            {
              success = true,
              fileContent = Convert.ToBase64String(fileBytes),
              fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
              fileName = fileName
            });
          }
          else
          {
            return Json(new
            {
              success = false,
              message = "File Import Succesfully...",
              flag = "0"
            });
          }


        }
      }
      catch (Exception ex)
      {
        return Json(new
        {
          success = false,
          message = "Error while processing the file. Please check the Excel format and ensure all required headers are present..........",
          flag = "1",
        });
      }
    }

    //THIS IS USED FOR DOWNLOAD BULT ORDER FORMAT
    [HttpGet]
    public IActionResult DownloadSampleSheet()
    {
      // Define the relative path to the sample sheet in wwwroot
      var fileUrl = "/FileStorage/Sample_Sheet_Order/New_Format.xlsx";

      // Check if the file exists in wwwroot
      var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileUrl.TrimStart('/'));

      if (System.IO.File.Exists(filePath))
      {
        // Return the relative URL for the file (client will use this)
        return Json(new { fileUrl });
      }
      else
      {
        return NotFound("Sample sheet not found.");
      }
    }

    //THIS FUNCTION USED FOR GET COPY AS NEW ITEM
    public IActionResult GetOrderData(string orderno)
    {
      var jsondata = "";
      string OrderHeaderId = "0";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      string[] OrderNumber = OrderNumber = orderno.Split('-');

      if (OrderNumber.Length == 2)
      {
        DataSet OrderData = OrderBal.GetOrderData(OrderNumber[0], OrderNumber[1]);
        if (OrderData != null && OrderData.Tables.Count > 0)
        {
          if (OrderData.Tables[0].Rows.Count > 0)
          {

            if (Convert.ToInt32(OrderBal.OrderNoExistOrNot(orderno)) == 0)
            {
              int RowCount = 0;
              for (int i = 0; i < OrderData.Tables[0].Rows.Count; i++)
              {
                RowCount++;
                ////THIS IS USED FOR BRANCH EXIST OR NOT
                string BranchId = OrderBal.ValidateIDs("ta_branch_master", "branch_name", OrderData.Tables[0].Rows[i]["branchName"].ToString());
                if (BranchId == "")
                {
                  TempData["ValidateMessage"] = "(" + OrderData.Tables[0].Rows[i]["branchName"].ToString() + ") Branch Not Valid...!!";
                  break;
                }

                ////THIS IS USED FOR CUSTOMER NOT EXIST THEN CREATE CUSTOMER
                string CustomerId = OrderBal.ValidateIDs("ta_customer_master", "mobile_no", OrderData.Tables[0].Rows[i]["mobileNo"].ToString());
                if (CustomerId == "")
                {
                  CustomerId = OrderBal.InsertIntoCustomerMaster(OrderData).ToString();
                }

                //THIS IS USED FOR ITEM EXIST OR NOT
                string ItemId = OrderBal.ValidateIDs("ta_item_master", "item_name", OrderData.Tables[0].Rows[i]["itemName"].ToString());
                if (ItemId == "")
                {
                  TempData["ValidateMessage"] = "(" + OrderData.Tables[0].Rows[i]["itemName"].ToString() + ") Item Name Not Exist...!!";
                  break;
                }

                string CategoryId = "";
                if (OrderData.Tables[0].Rows[i]["EntryClubPosition"].ToString() == "1")
                {
                  var CategoryName = OrderData.Tables[0].Rows[i]["categoryName"];
                  CategoryId = OrderBal.ItemIdAgainstCategoryIds(ItemId, CategoryName);

                  if (CategoryId == "")
                  {
                    TempData["ValidateMessage"] = "Item "+ OrderData.Tables[0].Rows[i]["itemName"].ToString() + " Against (" + OrderData.Tables[0].Rows[i]["categoryName"].ToString() + ") Category Not Exist...!!";
                    //TempData["ValidateMessage"] = "Item's category not found against ("+ OrderData.Tables[0].Rows[i]["itemName"].ToString() + "). Please add (NA) in category master to proceed....!!";
                    break;
                  }
                }
                else
                {
                  ////THIS IS USED FOR CATEGORY EXIST OR NOT
                  var CategoryValue = OrderData.Tables[0].Rows[i]["categoryName"];
                  if (CategoryValue != "")
                  {
                    CategoryId = OrderBal.ValidateIDs("ta_category_master", "category_name", OrderData.Tables[0].Rows[i]["categoryName"].ToString());
                    if (CategoryId == "")
                    {
                      TempData["ValidateMessage"] = "(" + OrderData.Tables[0].Rows[i]["categoryName"].ToString() + ") Stone Category Not Exist...!!";
                      break;
                    }
                  }
                  else
                  {
                    TempData["ValidateMessage"] = "Stone Category Not Found Against Item (" + OrderData.Tables[0].Rows[i]["itemName"].ToString() + ")... Please Add 'NA' In Category Master...!!";
                    break;
                  }
                }

                //THIS IS USED FOR PURITY EXIST OR NOT
                string PurityId = "";
                if(OrderData.Tables[0].Rows[i]["EntryClubPosition"].ToString() == "1")
                {
                  PurityId = OrderBal.ValidateIDs("ta_purity_master", "purity", OrderData.Tables[0].Rows[i]["purity"].ToString());
                  if (PurityId == "")
                  {
                    TempData["ValidateMessage"] = "(" + OrderData.Tables[0].Rows[i]["purity"].ToString() + ") Purity Not Exist. Item Name "+ OrderData.Tables[0].Rows[i]["itemName"].ToString() + " ..!!";
                    break;
                  }
                }

                decimal NetWt = Convert.ToDecimal(OrderData.Tables[0].Rows[i]["netWt"].ToString());
                decimal GrossWt = Convert.ToDecimal(OrderData.Tables[0].Rows[i]["grossWt"].ToString());
                //THIS IS USED FOR NEW WT NOT GREATER THEN GROSS WT
                if (Convert.ToDecimal(OrderData.Tables[0].Rows[i]["netWt"].ToString()) > Convert.ToDecimal(OrderData.Tables[0].Rows[i]["grossWt"].ToString()))
                {
                  TempData["ValidateMessage"] = "Net Wt Not Greater Than Gross Wt...!!";
                  break;
                }

                //THIS IS USED FOR PCS GREATER THAN ZERO
                if (Convert.ToDecimal(OrderData.Tables[0].Rows[i]["pcs"].ToString()) < 0 || string.IsNullOrEmpty(OrderData.Tables[0].Rows[i]["pcs"].ToString()))
                {
                  TempData["ValidateMessage"] = "Order Pcs Greater Than Zero...!!";
                  break;
                }

                //THIS IS USED FOR ORDER DELIVERY DATE LESS THAN TODAT DATE

                if (OrderData.Tables[0].Rows[i]["EntryClubPosition"].ToString() == "1")
                {
                  DateTime deliveryDate = Convert.ToDateTime(OrderData.Tables[0].Rows[i]["deliverydate"].ToString());
                  //DateTime OrderDate = Convert.ToDateTime(OrderData.Tables[0].Rows[i]["DocumentDate"].ToString());
                  if (deliveryDate < DateTime.Now.Date)
                  {
                    TempData["ValidateMessage"] = "Order delivery date should not be less than current date...!!";
                    break;
                  }
                }

                if (OrderData.Tables[0].Rows.Count == RowCount)
                {
                  OrderBal.CreatePADMOrder(OrderData);
                  TempData["ValidateMessage"] = "Order Created Successfully...!!!";
                  break;
                }
              }
            }
            else
            {
              TempData["ValidateMessage"] = "Order Number Is Already Exist...!!";
            }

          }
          else
          {
            TempData["ValidateMessage"] = "Order Not Found...!!";
          }
        }
        else
        {
          TempData["ValidateMessage"] = "Order Not Found...!!";
        }
      }
      else
      {
        TempData["ValidateMessage"] = "Invalid Order Number...!!";
      }
      Param.Add("ErrorMessage", TempData["ValidateMessage"]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR STONE CARAT CONVERT INTO GRAM
    public double ConvertStoneCaratIntoGram(double Carat)
    {
      double StoneInGram = 0;
      if(Carat > 0)
      {
        StoneInGram = Carat / 5;
      }

      return StoneInGram;
    }

    //THIS IS USED FOR GET ITEM UOM
    public JsonResult GetItemUOM(string ItemId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("ItemUOM", OrderBal.GetItemUOM(ItemId.Split('~')[0]).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS IS USED FOR GET ITEM UOM
    public JsonResult GetStoneCategory(string ItemId)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("StoneCategory", OrderBal.GetStoneCategory(ItemId.Split('~')[0]).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }


    //THIS IS USED FOR SEND JOB CARD TO CUSTOMER
    public IActionResult ShareJobCardDetailsDownload(string ItemDetailsId)
    {
      var wwwRootPath = _webHostEnvironment.WebRootPath;

      var report = new LocalReport();

      DataSet ds = OrderBal.GetJobCardDetailsForSend(ItemDetailsId);
      DataSet ds2 = OrderBal.GetStonePrintData(ItemDetailsId);

      using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomerOrderManagement.Reports.JobCard.rdlc");
      report.LoadReportDefinition(rs);
      report.EnableExternalImages = true;

      report.DataSources.Add(new ReportDataSource("JobCardDataTable", ds.Tables[0]));
      report.DataSources.Add(new ReportDataSource("JobCardStoneData", ds2.Tables[0]));

      var table = new DataTable();
      table.Columns.Add("Id", typeof(int));
      table.Columns.Add("imageBase64", typeof(string));

      for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
      {
        string relativePath = ds.Tables[0].Rows[i]["path"].ToString();
        if (!string.IsNullOrEmpty(relativePath))
        {
          string imagePath = Path.Combine(wwwRootPath, relativePath.TrimStart('~', '/').Replace('/', '\\'));
          try
          {
            using (var b = new Bitmap(imagePath))
            using (var ms = new MemoryStream())
            {
              b.Save(ms, ImageFormat.Bmp);
              string imageBase64 = Convert.ToBase64String(ms.ToArray());
              table.Rows.Add(i, imageBase64);
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Error processing image at row {i}: {ex.Message}");
          }
        }
      }

      report.DataSources.Add(new ReportDataSource("JobCardStoneImageData", table));

      byte[] pdfBytes = report.Render("PDF");

      string baseDirectoryPath = Path.Combine(wwwRootPath, "FileStorage", "JobCard");
      if (!Directory.Exists(baseDirectoryPath))
      {
        Directory.CreateDirectory(baseDirectoryPath);
      }

      string fileName = $"JobCard_{Guid.NewGuid():N}.pdf";
      string pdfFilePath = Path.Combine(baseDirectoryPath, fileName);
      System.IO.File.WriteAllBytes(pdfFilePath, pdfBytes);

      string fileUrl = $"/FileStorage/JobCard/{fileName}";

      // ✅ Return the file path as JSON
      return Json(new { fileUrl });
    }



  }

}
