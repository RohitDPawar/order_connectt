using BAL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;
namespace CustomerOrderManagement.Controllers.UserMapping
{
  public class UserMappingController : Controller
  {
    private readonly UserMappingBAL UserBAL;
    // Constructor injection for LoginBAL
    public UserMappingController(UserMappingBAL UserBal)
    {
      UserBAL = UserBal;
    }
    public IActionResult Index()
    {
      ViewBag.Subscription = UserBAL.CheckSubscriptionActiveOrNot();
      ViewBag.Role = UserBAL.GetRole().Tables[0].AsEnumerable();
      ViewBag.TenantID = UserBAL.GetTenantDetails().Tables[0].AsEnumerable();
      ViewBag.BranchData = UserBAL.GetBranchDetails().Tables[0].AsEnumerable();
      return View();
    }

    public IActionResult GetUserData([FromBody] Dictionary<string, object> data)
    {
      try
      {
        var items = UserBAL.GetUserData(data);

        if (items.Tables.Count >= 2)
        {
          var userData = items.Tables[0];

          var paginationInfo = items.Tables[1];

          var param = new Dictionary<string, object>
            {
                { "UserData", userData },
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

    // THIS FUNCTION USED FOR SAVE Tenant DETAILS
    public IActionResult SaveAddUser()
    {
      var Form = HttpContext.Request.Form;
      UserBAL.SaveAddUser(Form);

      TempData["Message"] = "User Details Added successfully!";
      //ViewBag.SuccessMessage = "Tenant details saved successfully!";
      return RedirectToAction("Index", "UserMapping");
    }
    //THIS FUNCTION USED FOR DECK DUPLICATES
    [HttpPost]
    public JsonResult CheckDuplicateRecord(string ColName, string Username)
    {
      string result = UserBAL.CheckDuplicateRecord(ColName, Username);
      return Json(result);
    }

    // THIS FUNCTION IS USED TO SHOW HISTORY DATA
    public IActionResult GetHistoryData(int id)
    {

      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      Param.Add("HistoryData", UserBAL.GetHistoryData(id).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    // THIS FUNCTION IS USED TO EDIT THE User 
    public IActionResult EditUser(int ID, string Flag)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      Param.Add("UserData", UserBAL.GetEditUserData(ID).Tables[0]);
      Param.Add("Flag", Flag);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR UPDATE user DETAILS
    public IActionResult SaveEditUser()
    {
      var Form = HttpContext.Request.Form;
      UserBAL.SaveEditUser(Form);

      TempData["Message"] = "User Updated Successfully !!!!!";
      return RedirectToAction("Index", "UserMapping");
    }

    public IActionResult ExportdataExcelUser(string selectedRowIds, string searchValue, string tablename, string coloumname)
    {
      try
      {
        // Set the license context for EPPlus (required for non-commercial use)
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        // Retrieve data from the Business Logic Layer
        var ds = UserBAL.GetFilterUserData(selectedRowIds, searchValue);

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
            var worksheet = package.Workbook.Worksheets.Add("UserData");

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
          var fileName = "UserData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

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
  }
}
