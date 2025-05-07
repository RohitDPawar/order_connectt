using BAL;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text;

namespace CustomerOrderManagement.Controllers.FilesManagement
{
  public class FilesManagementController : Controller
  {
    private readonly FilesManagementBAL ExportBAL;
    // Constructor injection for LoginBAL
    public FilesManagementController(FilesManagementBAL FileBAL)
    {
      ExportBAL = FileBAL;
    }

    public ActionResult DownloadExportFile(string TableName)
    {
      // Fetch the dataset
      DataSet ds = ExportBAL.GetMessageReport(TableName);
      if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
      {
        // Handle the case where the dataset is empty
        return RedirectToAction("Index", "ReportMaster");
      }

      DataTable dt = ds.Tables[0];

      // Build the CSV file data as a comma-separated string.
      StringBuilder csvBuilder = new StringBuilder();

      // Add column names as the first line.
      csvBuilder.AppendLine(string.Join(",", dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName)));

      // Add rows to the CSV.
      foreach (DataRow row in dt.Rows)
      {
        csvBuilder.AppendLine(string.Join(",", dt.Columns.Cast<DataColumn>().Select(col =>
        {
          // Escape commas and handle null values
          string value = row[col].ToString();
          return value.Contains(",") ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
        })));
      }

      // Convert the CSV data to a byte array for downloading.
      byte[] csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());

      // Return the file as a response.
      return File(csvBytes, "application/csv", $"{TableName}.csv");
    }
    //public void DownloadExportFile(string TableName)
    //{
    //  DataSet ds = new DataSet();
    //  ds = ExportBAL.GetMessageReport(TableName);

    //  //int ColumnCount = ds.Tables[0].Columns.Count;

    //  //THIS IS USED FOR COLUMNS NAME COLLECTION
    //  string[] names = ds.Tables.Cast<DataTable>()
    //  .SelectMany(x => x.Columns.Cast<DataColumn>()
    //      .Select(y => y.ColumnName)
    //  ).ToArray();

    //  //THIS IS USED FOR ARRAY CONVERT INTO STRING
    //  string ColumnsName = "";
    //  foreach (var item in names)
    //  {
    //    ColumnsName += item + ",";
    //  }
    //  //ds.Tables.Add(dt);
    //  DataTable dt = ds.Tables[0];

    //  //Build the CSV file data as a Comma separated string.
    //  string csv = ColumnsName + "\r\n";

    //  foreach (DataRow row in dt.Rows)
    //  {
    //    foreach (DataColumn column in dt.Columns)
    //    {
    //      string actual = "";
    //      var valu = column.ColumnName;// row[column.ColumnName].ToString();
    //      csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';

    //    }

    //    //Add new line.;
    //    csv += "\r\n";
    //  }

    //  //Download the CSV file.
    //  Response.Clear();
    //  Response.Buffer = true;
    //  Response.AddHeader("content-disposition", "attachment;filename=" + TableName + ".csv");
    //  Response.Charset = "";
    //  Response.ContentType = "application/text";
    //  Response.Output.Write(csv);
    //  Response.Flush();
    //  Response.End();
    //  return RedirectToAction("Index", "ReportMaster");
    //}


  }
}
