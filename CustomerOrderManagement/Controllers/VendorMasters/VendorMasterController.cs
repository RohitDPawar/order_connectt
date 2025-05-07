using BAL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;

namespace CustomerOrderManagement.Controllers.VendorMaster
{
  public class VendorMasterController : Controller
  {
    private readonly VendorMasterBAL VendorBAL;
    private readonly GlobalSessionBAL GlobalBal;
    // Constructor injection for LoginBAL
    public VendorMasterController(VendorMasterBAL VendorBaL, GlobalSessionBAL globalBal)
    {
      VendorBAL = VendorBaL;
      GlobalBal = globalBal;
    }
    public IActionResult Index()
    {
      ViewBag.TenantID = VendorBAL.GetTenantDetails().Tables[0].AsEnumerable();

      return View();
    }

    public IActionResult GetVendorData([FromBody] Dictionary<string, object> data)
    {
      try
      {
        var items = VendorBAL.GetVendorData(data);

        if (items.Tables.Count >= 2)
        {
          var vendorData = items.Tables[0];

          var paginationInfo = items.Tables[1];

          var param = new Dictionary<string, object>
            {
                { "VendorData", vendorData },
                { "PaginationInfo", paginationInfo }
            };

          var jsonResponse = JsonConvert.SerializeObject(param);

          return Content(jsonResponse, "application/json");
        }
        else
        {
          return StatusCode(500, "Unexpected data structure in the response.");
        }
      }
      catch (Exception ex)
      {
        return StatusCode(500, "An error occurred: " + ex.Message);
      }
    }

    // THIS FUNCTION USED FOR SAVE Vendor DETAILS
    public IActionResult SaveAddVendor()
    {
      var Form = HttpContext.Request.Form;
      VendorBAL.SaveAddVendor(Form);
      TempData["Message"] = "Vendor Added Successfully !!!!!";
      return RedirectToAction("Index", "VendorMaster");
    }


    // THIS FUNCTION IS USED TO EDIT THE VENDOR MASTER
    public IActionResult EditVendor(int ID, string Flag)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      Param.Add("VendorData", VendorBAL.GetEditVendorData(ID).Tables[0]);
      Param.Add("Flag", Flag);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR UPDATE Vendor DETAILS
    public IActionResult SaveEditVendor()
    {
      var Form = HttpContext.Request.Form;
      VendorBAL.SaveEditVendor(Form);
      TempData["Message"] = "Vendor Updated Successfully !!!!!";
      return RedirectToAction("Index", "VendorMaster");
    }

    // THIS FUNCTION IS USED TO SHOW HISTORY DATA
    public IActionResult GetHistoryData(int id)
    {

      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      Param.Add("HistoryData", VendorBAL.GetHistoryData(id).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR DECK DUPLICATES
    [HttpPost]
    public JsonResult CheckDuplicateRecord(string ColName, string Username)
    {
      string result = VendorBAL.CheckDuplicateRecord(ColName, Username);
      return Json(result);
    }

    public IActionResult ExportdataExcelVendor(string selectedRowIds, string searchValue, string tablename, string coloumname)
    {
      try
      {
        // Set the license context for EPPlus (required for non-commercial use)
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        // Retrieve data from the Business Logic Layer
        var ds = VendorBAL.GetFilterVendorData(selectedRowIds, searchValue);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        {
          return Json(new { success = false, message = "No data available to export." });
        }

        // Create a memory stream to write the Excel file to
        using (var memoryStream = new MemoryStream())
        {
          // Create a new Excel package
          using (var package = new ExcelPackage(memoryStream))
          {
            // Add a new worksheet to the Excel package
            var worksheet = package.Workbook.Worksheets.Add("VendorData");

            // Get the data table from the dataset
            var dataTable = ds.Tables[0];

            // Add a serial number column at the beginning of the data table
            DataColumn serialNumberColumn = new DataColumn("SR No", typeof(int));
            dataTable.Columns.Add(serialNumberColumn);

            // Insert the serial number column at index 0 (first column)
            dataTable.Columns["SR No"].SetOrdinal(0);

            // Populate the serial number column
            int serialNumber = 1;
            foreach (DataRow row in dataTable.Rows)
            {
              row["SR No"] = serialNumber++;
            }

            // Load the data into the worksheet
            worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

            // Format the "Created Date" column as Date
            int createdDateColumnIndex = dataTable.Columns.IndexOf("Created At") + 1; // Excel is 1-based indexing

            worksheet.Cells[2, createdDateColumnIndex, worksheet.Dimension.End.Row, createdDateColumnIndex].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss"; // Adjust format as needed

            // Format the "Created Date" column as Date
            int createdDateColumnIndex2 = dataTable.Columns.IndexOf("Updated At") + 1; // Excel is 1-based indexing

            worksheet.Cells[2, createdDateColumnIndex2, worksheet.Dimension.End.Row, createdDateColumnIndex2].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss"; // Adjust format as needed

            // Align all data cells to the left (except the header)
            worksheet.Cells[1, 1, worksheet.Dimension.End.Row, dataTable.Columns.Count].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            // Optional: Format columns
            worksheet.Cells.AutoFitColumns();

            // Save the package to the memory stream
            package.Save();
          }

          // Set the memory stream position to the beginning
          memoryStream.Position = 0;

          // Set the filename
          var fileName = "VendorData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

          byte[] fileBytes = memoryStream.ToArray();

          return Json(new
          {
            success = true,
            fileContent = Convert.ToBase64String(fileBytes),
            fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName = fileName
          });
        }
      }
      catch (Exception ex)
      {
        // Log the error
        Console.Error.WriteLine($"Error occurred while exporting data to Excel: {ex.Message}");

        // Return a friendly error message
        return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
      }
    }

    //########################## THIS IS USE FOR UPLOAD BULK DATA ######################################################3
    [HttpPost]
    public async Task<IActionResult> UploadVendorMaster(IFormFile file)
    {
      try
      {
        // Set EPPlus License Context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage(file.OpenReadStream()))
        {
          var dataTable = new DataTable();
          var outputPackage = new ExcelPackage();

          var worksheet = package.Workbook.Worksheets[0];

          var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column]
                           .Select(cell => cell.Text?.Trim())
                           .ToList();

          var requiredHeaders = new[] { "Vendor_Name", "Mobile_Number", "Email_ID", "Pan_Card_No", "Gst_Number", "Residential_Address", "Remark", "Status" };

          if (!requiredHeaders.All(header => headerRow.Contains(header, StringComparer.OrdinalIgnoreCase)))
          {
            return Json(new
            {
              success = false,
              message = $"Invalid Excel format. Required headers are missing in sheet {worksheet.Name}.",
              flag = '0'
            });
          }

          foreach (var header in headerRow)
          {
            dataTable.Columns.Add(header, typeof(string));
          }

          string uniqueId = DateTime.Now.Ticks.ToString();

          if (worksheet.Dimension == null || worksheet.Dimension.End.Row < 2)
          {
            return Json(new { success = false, message = "No data found.", flag = '0' });
          }

          for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
          {
            var dataRow = dataTable.NewRow();

            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
              string cellValue = worksheet.Cells[row, col].Text?.Trim();
              dataRow[col - 1] = cellValue;
            }
            dataTable.Rows.Add(dataRow);

            string GetSafeString(object value)
            {
              return string.IsNullOrWhiteSpace(value.ToString()) ? "" : value.ToString();
            }

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("vendor_name", GetSafeString(dataRow["Vendor_Name"]));
            para.Add("mobile_no", GetSafeString(dataRow["Mobile_Number"]));
            para.Add("email_id", GetSafeString(dataRow["Email_ID"]));
            para.Add("gst_no", GetSafeString(dataRow["Gst_Number"]));
            para.Add("pancard_no", GetSafeString(dataRow["Pan_Card_No"]));
            para.Add("residential_address", GetSafeString(dataRow["Residential_Address"]));
            para.Add("remark", GetSafeString(dataRow["Remark"]));
            para.Add("is_active", GetSafeString(dataRow["Status"]));
            para.Add("unique_id", uniqueId);

            VendorBAL.SaveAddVendorImport(para);

          }

          DataSet result = GlobalBal.GetImportErrorData(uniqueId, GlobalBal.GetClientIpAddress(), "sp_ValidateAndSaveVendorData");
          
          if (result != null && result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
          {
            DataTable errorDataTable = result.Tables[0];
            var errorWorksheet = outputPackage.Workbook.Worksheets.Add("Vendor Import Errors");

            for (int col = 1; col <= headerRow.Count; col++)
            {
              errorWorksheet.Cells[1, col].Value = headerRow[col - 1];
            }

            errorWorksheet.Cells[1, headerRow.Count + 1].Value = "Error Status";

            var errorArray = errorDataTable.AsEnumerable()
                                           .Select(row => new object[]
                                           {
                                                   row["vendor_name"],
                                                   row["mobile_no"],
                                                   row["email_id"],
                                                   row["pancard_no"],
                                                   row["gst_no"],
                                                   row["residential_address"],
                                                   row["remark"],
                                                   row["is_active"],
                                                   row["Error_Message"]
                                           })
                                           .ToArray();

            errorWorksheet.Cells[2, 1].LoadFromArrays(errorArray);

            var memoryStream = new MemoryStream();
            outputPackage.SaveAs(memoryStream);
            memoryStream.Position = 0;

            byte[] fileBytes = memoryStream.ToArray();
            string fileName = "VendorInvalidData_" + DateTime.Now.ToString("dd-MM-yy_HHmmss") + ".xlsx";

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
            return Json(new { success = false, message = "File uploaded and processed successfully.", flag = '1' });
          }
        }
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = "Error while processing the file: " + ex.Message, flag = '0' });
      }
    }

    //THIS US USED FOR DOWNLOAD FILE FORMAT
    [HttpGet]
    public IActionResult DownloadSampleSheet()
    {
      // Define the relative path to the sample sheet in wwwroot
      var fileUrl = "/FileStorage/Sample_Sheet_Order/Vendor_data_import.xlsx";

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
  }
}
