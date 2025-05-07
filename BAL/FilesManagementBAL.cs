using CustomerOrderManagement.Helper;
using System.Data;

namespace BAL
{
	public class FilesManagementBAL
	{
		private readonly MySqlService DBHelper;

		// Constructor injection for MySqlService
		public FilesManagementBAL(MySqlService mySqlService)
		{
			DBHelper = mySqlService;
		}
		// THIS FUNCTION IS USED TO GET ALL BRANCH DATA FOR EDITING
		public DataSet GetMessageReport(string tableName)
		{
			DataSet ds = new DataSet();
			try
			{
				string query = "";
				query = "SELECT * from " + tableName + "";
				ds = DBHelper.ExecuteQueryReturnDS(query);
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}
	}
}
