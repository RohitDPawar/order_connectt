using BAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Elfie.Model.Map;
using Newtonsoft.Json;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities.Collections;
using System.Data;
using System.Drawing.Drawing2D;
using System.Text;

namespace CustomerOrderManagement.Controllers.BasicMasters
{
  public class ItemMasterController : Controller
  {
    private readonly ItemMasterBAL ItemBAL;
    private readonly GlobalSessionBAL GlobalBal;
    // Constructor injection for LoginBAL
    public ItemMasterController(ItemMasterBAL itemMasterBAL, GlobalSessionBAL globalBal)
    {
      ItemBAL = itemMasterBAL;
      GlobalBal = globalBal;
    }
    public IActionResult Index()
    {
      //ViewBag.AllItems = ItemBAL.GetAllItems().Tables[0].AsEnumerable();
      ViewBag.AllProductGroups = ItemBAL.GetAllProductGroup().Tables[0].AsEnumerable();
      ViewBag.GetAllTenants = ItemBAL.GetTenantDetails().Tables[0].AsEnumerable();
      return View();
    }
    //THIS FUNCTION IS USED TO GET FILTERED PRODUCT GROUPS
    [HttpGet]
    public JsonResult GetFilteredProductGroups(string SearchItem)
    {
      var product = ItemBAL.GetFilteredProductGroup(SearchItem).Tables[0].AsEnumerable();

      var filtered = product
          .Select(row => new
          {
            id = row["id"],
            product_group_name = row["product_group_name"].ToString()
          })
          .ToList();

      return Json(filtered);
    }
    //[HttpGet]
    //public JsonResult GetFilteredProductGroups(string SearchItem)
    //{
    //  var product = ItemBAL.GetFilteredProductGroup(SearchItem).Tables[0].AsEnumerable();
    //  var productList = product.ToList(); // Just convert to List<DataRow>
    //  return Json(productList);
    //}

    //vina code
    public IActionResult GetItemData([FromBody] Dictionary<string, object> data)
    {
      try
      {
        var items = ItemBAL.GetAllItems(data);

        if (items.Tables.Count >= 2)
        {
          var itemData = items.Tables[0];

          var paginationInfo = items.Tables[1];

          var param = new Dictionary<string, object>
            {
                { "ItemData", itemData },
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

    // THIS FUNCTION USED FOR SAVE City DETAILS
    public IActionResult SaveAddItem()
    {
      var Form = HttpContext.Request.Form;
      ItemBAL.SaveAddItem(Form);
      TempData["Message"] = "Item Added Successfully !!!!!";
      return RedirectToAction("Index", "ItemMaster");
    }
    // THIS FUNCTION IS USED TO EDIT THE CITY MASTER
    public IActionResult EditItem(int ID, string Flag)
    {

      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      Param.Add("ItemData", ItemBAL.GetEditItemData(ID).Tables[0]);
      Param.Add("Flag", Flag);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }
    //THIS FUNCTION USED FOR UPDATE CITY DETAILS
    public IActionResult SaveEditItem()
    {
      var Form = HttpContext.Request.Form;
      ItemBAL.SaveEditItem(Form);

      TempData["Message"] = "Item Updated Successfully !!!!!";
      return RedirectToAction("Index", "ItemMaster");
    }

    // THIS FUNCTION IS USED TO SHOW HISTORY DATA
    public IActionResult GetHistoryData(int id)
    {

      var jsondata = "";
      Dictionary<string, object> Param = new Dictionary<string, object>();

      Param.Add("HistoryData", ItemBAL.GetHistoryData(id).Tables[0]);
      jsondata = JsonConvert.SerializeObject(Param);
      return Json(jsondata);
    }

    //THIS FUNCTION USED FOR DECK DUPLICATES
    [HttpPost]
    public JsonResult CheckDuplicateRecord(string ColName, string value, string selectedProductGroupId)
    {
      string result = ItemBAL.CheckDuplicateRecord(ColName, value, selectedProductGroupId);
      return Json(result);
    }

    public IActionResult ExportDataCsv(string selectedRowIds, string searchValue, string tablename, string coloumname)
    {
      try
      {
        // Retrieve data from the Business Logic Layer
        var ds = ItemBAL.GetFilterItemData(selectedRowIds, searchValue);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        {
          return Json(new { success = false, message = "No data available to export." });
        }

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

        // Create a StringBuilder to store CSV content
        var csv = new StringBuilder();

        // Add column headers to the CSV (first row)
        var columnNames = string.Join(",", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        csv.AppendLine(columnNames);

        // Add the data rows to the CSV
        foreach (DataRow row in dataTable.Rows)
        {
          var rowValues = string.Join(",", row.ItemArray.Select(field => field.ToString().Replace(",", ";"))); // Replacing commas in data to avoid breaking CSV
          csv.AppendLine(rowValues);
        }

        // Set the file name
        var fileName = "ItemData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

        // Return the CSV file as a downloadable file
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
      }
      catch (Exception ex)
      {
        // Log the error
        Console.Error.WriteLine($"Error occurred while exporting data to CSV: {ex.Message}");

        // Return a friendly error message
        return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
      }
    }

    //THIS US USED FOR DOWNLOAD FILE FORMAT
    [HttpGet]
    public IActionResult DownloadSampleSheet()
    {
      // Define the relative path to the sample sheet in wwwroot
      var fileUrl = "/FileStorage/Sample_Sheet_Order/Item_data_import.xlsx";

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
    //########################## THIS IS USE FOR UPLOAD BULK DATA ######################################################3
    [HttpPost]
    public async Task<IActionResult> UploadItemMaster(IFormFile file)
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

          var requiredHeaders = new[] { "Product_Group_Name", "Item_Name", "UOM", "Is_Stone", "Remark", "Status" };

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

            ItemBAL.SaveImportDataItem(dataRow["Product_Group_Name"].ToString(), dataRow["Item_Name"].ToString(), dataRow["UOM"].ToString().ToUpper(), dataRow["Is_Stone"].ToString(), dataRow["Remark"].ToString(), dataRow["Status"].ToString(), uniqueId);
          }

          DataSet result = GlobalBal.GetImportErrorData(uniqueId, GlobalBal.GetClientIpAddress(), "sp_ValidateAndSaveItemData");
          DataTable errorDataTable = result.Tables[0];

          if (errorDataTable != null && errorDataTable.Rows.Count > 0)
          {
            var errorWorksheet = outputPackage.Workbook.Worksheets.Add("Item Import Errors");

            for (int col = 1; col <= headerRow.Count; col++)
            {
              errorWorksheet.Cells[1, col].Value = headerRow[col - 1];
            }

            errorWorksheet.Cells[1, headerRow.Count + 1].Value = "Error Status";

            var errorArray = errorDataTable.AsEnumerable()
                                           .Select(row => new object[]
                                           {
                                                   row["product_group_name"],
                                                   row["item_name"],
                                                   row["uom"],
                                                   row["is_stone"],
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
            string fileName = "ItemInvalidData_" + DateTime.Now.ToString("dd-MM-yy_HHmmss") + ".xlsx";

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
  }
}
