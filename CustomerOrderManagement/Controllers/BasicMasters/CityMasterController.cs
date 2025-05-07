using BAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model.Map;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;

namespace CustomerOrderManagement.Controllers.BasicMasters
{
  public class CityMasterController : Controller
  {
    private readonly CityMasterBAL CityBAL;
    // Constructor injection for LoginBAL
    public CityMasterController(CityMasterBAL CityBaL)
    {
      CityBAL = CityBaL;
    }
    public IActionResult Index()
    {
      ViewBag.Country = CityBAL.GetAllCountry().Tables[0].AsEnumerable();
      ViewBag.State = CityBAL.GetAllState().Tables[0].AsEnumerable();
      return View();
    }
    //THIS FUNCTIONIS IS USED FOR GET FILTER State name
    [HttpGet]
    public JsonResult GetFilteredCountryGroups(string SearchItem)
    {
      var allGroups = CityBAL.GetFilteredCountry(SearchItem).Tables[0].AsEnumerable();

      var filtered = allGroups
          // .Where(row => row["item_name"].ToString().Contains(SearchItem, StringComparison.OrdinalIgnoreCase))
          .Select(row => new {
            id = row["id"],
            country_name = row["country_name"].ToString()
          })
          .ToList();

      return Json(filtered);
    }
    public IActionResult GetCityData([FromBody] Dictionary<string, object> data)
    {
      try
      {
        var cities = CityBAL.GetCityData(data);

        if (cities.Tables.Count >= 2)
        {
          var itemData = cities.Tables[0];

          var paginationInfo = cities.Tables[1];

          var param = new Dictionary<string, object>
            {
                { "CityData", itemData },
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

    // THIS FUNCTION IS USE FOR DISPLAY THE ADD CITY MASTER VIEW
    public IActionResult AddCity()
    {
      ViewBag.Country = CityBAL.GetAllCountry().Tables[0].AsEnumerable();
      ViewBag.State = CityBAL.GetAllState().Tables[0].AsEnumerable();
      return View();
    }
    // THIS FUNCTION USED FOR SAVE City DETAILS
    public IActionResult SaveAddCity()
    {
      var Form = HttpContext.Request.Form;

      string CityData = CityBAL.CheckDuplicateRecord(Form["CountryId"].ToString(), Form["StateId"].ToString(), Form["Name"].ToString());
      if (CityData == "")
      {
        CityBAL.SaveAddCity(Form);
        TempData["Message"] = "City Added Successfully !!!!!";
      }
      else
      {
        TempData["Message"] = "City Already Exit !!!!!";
      }

      return RedirectToAction("Index", "CityMaster");
    }
    // THIS FUNCTION IS USED TO EDIT THE CITY MASTER
    public IActionResult EditCity(int ID, string Flag)
    {
      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      Param.Add("CityData", CityBAL.GetEditCityData(ID).Tables[0]);
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

      Param.Add("HistoryData", CityBAL.GetHistoryData(id).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }
    //THIS FUNCTION USED FOR UPDATE CITY DETAILS
    public IActionResult SaveEditCity()
    {
      var Form = HttpContext.Request.Form;

      CityBAL.SaveEditCity(Form);

      TempData["Message"] = "City Updated Successfully !!!!!";
      return RedirectToAction("Index", "CityMaster");
    }

    //THIS FUNCTION USED FOR CHECK DUPLICATES
    [HttpPost]
    public JsonResult CheckDuplicateRecord(string City, string Country, string State)
    {
      string result = CityBAL.CheckDuplicateRecord(Country, State, City);
      return Json(result);
    }

    public IActionResult ExportdataExcelCity(string selectedRowIds, string searchValue, string tablename, string coloumname)
    {
      try
      {
        // Set the license context for EPPlus (required for non-commercial use)
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        // Retrieve data from the Business Logic Layer
        var ds = CityBAL.GetFilterCityData(selectedRowIds, searchValue);

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
            var worksheet = package.Workbook.Worksheets.Add("CategoryData");

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
          var fileName = "CityData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

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

    //THIS FUNCTION USED get state names of country
    [HttpGet]
    public JsonResult GetStateName(int CountryId, String SearchItem)
    {
      var allGroups = CityBAL.GetStateNames(CountryId,SearchItem).Tables[0].AsEnumerable();

      var filtered = allGroups
          // .Where(row => row["item_name"].ToString().Contains(SearchItem, StringComparison.OrdinalIgnoreCase))
          .Select(row => new {
            id = row["id"],
            state_name = row["state_name"].ToString()
          })
          .ToList();

      return Json(filtered);
    }

  }
}
