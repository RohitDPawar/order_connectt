using BAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model.Map;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;

namespace CustomerOrderManagement.Controllers.BasicMasters
{
  public class ProductGroupMasterController : Controller
  {
    private readonly ProductGroupMasterBAL ProductGroupBAL;
    private readonly GlobalSessionBAL GlobalBal;
    // Constructor injection for LoginBAL
    public ProductGroupMasterController(ProductGroupMasterBAL ProductMasterMasterBal, GlobalSessionBAL globalBal)
    {
      ProductGroupBAL = ProductMasterMasterBal;
      GlobalBal = globalBal;
    }
    public IActionResult Index()
    {
      ViewBag.GetAllTenants = ProductGroupBAL.GetTenantDetails().Tables[0].AsEnumerable();
      return View();
    }

    public IActionResult GetProductGroupData([FromBody] Dictionary<string, object> data)
    {
      try
      {
        var products = ProductGroupBAL.GetAllProductGroups(data);

        if (products.Tables.Count >= 2)
        {
          var itemData = products.Tables[0];

          var paginationInfo = products.Tables[1];

          var param = new Dictionary<string, object>
            {
                { "ProductGroupData", itemData },
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

    public IActionResult AddProductGroup()
    {
      ViewBag.GetAllTenants = ProductGroupBAL.GetTenantDetails().Tables[0].AsEnumerable();
      return View();
    }

    // THIS FUNCTION IS USED TO SAVE PRODUCT DETAILS
    public IActionResult SaveAddProductGroup()
    {
      var Form = HttpContext.Request.Form;
      JsonResult response = CheckDuplicateRecord("product_group_name", Form["ProductGroupName"].ToString());
      var jsonData = response.Value; // Extract the JSON object
      int result = Convert.ToInt32(jsonData); // Convert to integer
      if (result == 1)
      {
        TempData["MessageWarning"] = "Product Group Already Exist...!!";
      }
      else
      {
        ProductGroupBAL.SaveAddProductGroup(Form);
        TempData["Message"] = "Product Group Added Successfully !!!!!";
      }
      return RedirectToAction("Index");
    }

    // THIS FUNCTION IS USED TO VEW EDIR PAGE
    public ActionResult EditProductGroup(int ID, string Flag)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      //ViewBag.EditCountryData = countryBAL.GetEditCountryData(ID);
      Param.Add("ProductGroupData", ProductGroupBAL.GetProductGroupData(ID).Tables[0]);
      Param.Add("Flag", Flag);
      //ViewBag.Flag = Flag;
      jsondata = JsonConvert.SerializeObject(Param);

      return Json(jsondata);
    }
    //THIS FUNCTION USED FOR UPDATE COUNTRY DETAILS
    public ActionResult SaveEditProductGroup()
    {
      var Form = HttpContext.Request.Form;
      ProductGroupBAL.SaveEditProductGroup(Form);
      TempData["Message"] = "Product Group Updated Successfully !!!!!";
      return RedirectToAction("Index", "ProductGroupMaster");
    }

    // THIS FUNCTION IS USED TO SHOW HISTORY DATA
    public IActionResult GetHistoryData(int id)
    {

      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      Param.Add("HistoryData", ProductGroupBAL.GetHistoryData(id).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR DECK DUPLICATES
    [HttpPost]
    public JsonResult CheckDuplicateRecord(string ColName, string value)
    {
      int result = ProductGroupBAL.CheckDuplicateRecord(ColName, value);
      return Json(result);
    }

    public IActionResult ExportdataExcelProductGroup(string selectedRowIds, string searchValue, string tablename, string coloumname)
    {
      try
      {
        // Set the license context for EPPlus (required for non-commercial use)
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        // Retrieve data from the Business Logic Layer
        var ds = ProductGroupBAL.GetFilterProductGroupData(selectedRowIds, searchValue);

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
            var worksheet = package.Workbook.Worksheets.Add("ProductGroupData");

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
          var fileName = "ProductGroupDataData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

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
    public async Task<IActionResult> UploadProductGroup(IFormFile file)
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

          // Read header row
          var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column]
                          .Select(cell => cell.Text?.Trim())
                          .ToList();

          // Required headers
          var requiredHeaders = new[] { "product_group_name", "remark", "is_active" };

          // Validate headers
          if (!requiredHeaders.All(header => headerRow.Contains(header, StringComparer.OrdinalIgnoreCase)))
          {
            return Json(new
            {
              success = false,
              message = $"Invalid Excel format. Required headers are missing in sheet {worksheet.Name}.",
              flag = '0'
            });
          }

          // Add columns to the DataTable
          foreach (var header in headerRow)
          {
            dataTable.Columns.Add(header, typeof(string));
          }

          string uniqueId = DateTime.Now.Ticks.ToString();
          var processedRows = new HashSet<string>();

          if (worksheet.Dimension == null || worksheet.Dimension.End.Row < 2)
          {
            return Json(new { success = false, message = "No data found.", flag = '0' });
          }

          for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
          {
            var dataRow = dataTable.NewRow();
            var rowContent = new List<string>();

            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
              string cellValue = worksheet.Cells[row, col].Text?.Trim();
              dataRow[col - 1] = cellValue;
              rowContent.Add(cellValue);
            }

            // Generate a unique key for each row (concatenate all values)
            string rowKey = string.Join("|", rowContent);

            if (!processedRows.Contains(rowKey)) // Avoid duplicates
            {
              dataTable.Rows.Add(dataRow);
              processedRows.Add(rowKey);

              // Save to database
              ProductGroupBAL.SaveImportDataProductGrp(
                  dataRow["product_group_name"].ToString(),
                  dataRow["remark"].ToString(),
                  dataRow["is_active"].ToString(),
                  uniqueId
              );
            }
          }

          // Get validation results
          DataSet result = GlobalBal.GetImportErrorData(uniqueId, GlobalBal.GetClientIpAddress(), "sp_ValidateAndSaveProductGroupData");
          DataTable errorDataTable = result.Tables[0];

          if (errorDataTable != null && errorDataTable.Rows.Count > 0)
          {
            var errorWorksheet = outputPackage.Workbook.Worksheets.Add("Product Group Import Errors");

            // Write headers
            for (int col = 1; col <= headerRow.Count; col++)
            {
              errorWorksheet.Cells[1, col].Value = headerRow[col - 1];
            }
            errorWorksheet.Cells[1, headerRow.Count + 1].Value = "Error Status";

            // Write data
            var errorArray = errorDataTable.AsEnumerable()
                                           .Select(row => new object[]
                                           {
                                                   row["product_group_name"],
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
            string fileName = "ProductGrpInvalidData_" + DateTime.Now.ToString("dd-MM-yy_HHmmss") + ".xlsx";

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
      var fileUrl = "/FileStorage/Sample_Sheet_Order/product_group_import.xlsx";

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
