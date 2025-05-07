//THIS IS TESTING DEMO VINASHREE

using BAL;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model.Map;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;
using System.Diagnostics.Metrics;

namespace CustomerOrderManagement.Controllers.BasicMasters
{
  public class BranchMasterController : Controller
  {
    private readonly BranchMasterBAL BranchBAL;
    private readonly TenantMasterBAL TenantBAL;
    private readonly GlobalSessionBAL GlobalBal;
    // Constructor injection for LoginBAL
    public BranchMasterController(BranchMasterBAL BranchMasterBal, TenantMasterBAL TenantBal, GlobalSessionBAL globalBal)
    {
      BranchBAL = BranchMasterBal;
      TenantBAL = TenantBal;
      GlobalBal = globalBal;
    }
    public IActionResult Index()
    {
      ViewBag.AllCountry = BranchBAL.GetAllCountry().Tables[0].AsEnumerable();
      ViewBag.AllState = BranchBAL.GetAllState().Tables[0].AsEnumerable();
      ViewBag.AllCity = BranchBAL.GetAllCity().Tables[0].AsEnumerable();
      ViewBag.GetAllTenants = BranchBAL.GetTenantDetails().Tables[0].AsEnumerable();
      return View();
    }

    //THIS FUNCTIONIS IS USED FOR GET FILTER State name
    [HttpGet]
    public JsonResult GetFilteredCountryGroups(string SearchItem)
    {
      var allGroups = BranchBAL.GetFilteredCountry(SearchItem).Tables[0].AsEnumerable();

      var filtered = allGroups
          // .Where(row => row["item_name"].ToString().Contains(SearchItem, StringComparison.OrdinalIgnoreCase))
          .Select(row => new {
            id = row["id"],
            country_name = row["country_name"].ToString()
          })
          .ToList();

      return Json(filtered);
    }
    //THIS FUNCTION USED get state names of country
    [HttpGet]
    public JsonResult GetStateName(int CountryId, String SearchItem)
    {
      var allGroups = BranchBAL.GetStateNames(CountryId, SearchItem).Tables[0].AsEnumerable();

      var filtered = allGroups
          // .Where(row => row["item_name"].ToString().Contains(SearchItem, StringComparison.OrdinalIgnoreCase))
          .Select(row => new {
            id = row["id"],
            state_name = row["state_name"].ToString()
          })
          .ToList();

      return Json(filtered);
    }
    //THIS FUNCTION USED get state names of country
    [HttpGet]
    public JsonResult GetCityName(int StateId, String SearchItem)
   {
      var allGroups = BranchBAL.GetCityNames(StateId, SearchItem).Tables[0].AsEnumerable();

      var filtered = allGroups
          // .Where(row => row["item_name"].ToString().Contains(SearchItem, StringComparison.OrdinalIgnoreCase))
          .Select(row => new {
            id = row["id"],
            city_name = row["city_name"].ToString()
          })
          .ToList();

      return Json(filtered);
    }
    public IActionResult GetBranchData([FromBody] Dictionary<string, object> data)
    {
      try
      {
        var items = BranchBAL.GetAllBranches(data);

        if (items.Tables.Count >= 2)
        {
          var branchData = items.Tables[0];

          var paginationInfo = items.Tables[1];

          var param = new Dictionary<string, object>
            {
                { "BranchData", branchData },
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

    // THIS FUNCTION USED FOR SAVE ADD CATEGORY DATA
    public IActionResult SaveAddBranch()
    {
      var Form = HttpContext.Request.Form;
      BranchBAL.SaveAddBranch(Form);
      TempData["Message"] = "Branch Added Successfully !!!!!";
      return RedirectToAction("Index", "BranchMaster");
    }
    // THIS FUNCTION IS USED TO EDIT THE CATEGORYDATA
    public IActionResult EditBranch(int ID, string Flag)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();
      Param.Add("BranchData", BranchBAL.GetEditBranchData(ID).Tables[0]);
      Param.Add("Flag", Flag);
      //ViewBag.Flag = Flag;
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    // THIS FUNCTION IS USED TO SHOW HISTORY DATA
    public IActionResult GetHistoryData(int id)
    {

      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      Param.Add("HistoryData", BranchBAL.GetHistoryData(id).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }


    //THIS FUNCTION USED FOR UPDATE CATEGORY DETAILS
    public IActionResult SaveEditBranch()
    {
      var Form = HttpContext.Request.Form;
      BranchBAL.SaveEditBranch(Form);
      TempData["Message"] = "Branch Updated Successfully !!!!!";
      return RedirectToAction("Index", "BranchMaster");
    }

    //THIS FUNCTION USED FOR DECK DUPLICATES
    [HttpPost]
    public JsonResult CheckDuplicateRecord(string ColName, string value)
    {
      string result = BranchBAL.CheckDuplicateRecord(ColName, value);
      return Json(result);
    }

    public IActionResult ExportdataExcelBranch(string selectedRowIds, string searchValue, string tablename, string coloumname)
    {
      try
      {
        // Set the license context for EPPlus (required for non-commercial use)
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        // Retrieve data from the Business Logic Layer
        var ds = BranchBAL.GetFilterBranchData(selectedRowIds, searchValue);

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
            var worksheet = package.Workbook.Worksheets.Add("BranchData");

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
          var fileName = "BranchData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

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
    public async Task<IActionResult> UploadBranchMaster(IFormFile file)
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

          var requiredHeaders = new[] { "branch_name", "address", "area", "pincode", "country_Name", "state_name", "city_name", "remark", "is_active" };

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

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("BranchName", dataRow["branch_name"].ToString());
            para.Add("address", dataRow["address"].ToString());
            para.Add("area_description", dataRow["area"].ToString());
            para.Add("pincode", dataRow["pincode"].ToString());
            para.Add("country_name", dataRow["country_Name"].ToString());
            para.Add("state_name", dataRow["state_name"].ToString());
            para.Add("city_name", dataRow["city_name"].ToString());
            para.Add("remark", dataRow["remark"].ToString());
            para.Add("is_active", dataRow["is_active"].ToString());
            para.Add("unique_id", uniqueId);

            BranchBAL.SaveAddBranchImporttemp(para);
          }

          DataSet result = GlobalBal.GetImportErrorData(uniqueId, GlobalBal.GetClientIpAddress(), "sp_ValidateAndSaveBranchData");
          DataTable errorDataTable = result.Tables[0];

          if (errorDataTable != null && errorDataTable.Rows.Count > 0)
          {
            var errorWorksheet = outputPackage.Workbook.Worksheets.Add("Branch Import Errors");

            for (int col = 1; col <= headerRow.Count; col++)
            {
              errorWorksheet.Cells[1, col].Value = headerRow[col - 1];
            }

            errorWorksheet.Cells[1, headerRow.Count + 1].Value = "Error Status";

            var errorArray = errorDataTable.AsEnumerable()
                                           .Select(row => new object[]
                                           {
                                                   row["branch_name"],
                                                   row["address"],
                                                   row["area_description"],
                                                   row["pincode"],
                                                   row["country_Name"],
                                                   row["state_name"],
                                                   row["city_name"],
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
            string fileName = "BranchInvalidData_" + DateTime.Now.ToString("dd-MM-yy_HHmmss") + ".xlsx";

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
      var fileUrl = "/FileStorage/Sample_Sheet_Order/Branch_data_import.xlsx";

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
