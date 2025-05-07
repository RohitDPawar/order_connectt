using MySql.Data.MySqlClient;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Data;

namespace CustomerOrderManagement.Helper
{
	public class MySqlService
	{
		private readonly string _connectionString;
		private readonly string _connectionStringPadm;

		public MySqlService(string connectionString, string connectionStringPadm)
		{
			_connectionString = connectionString;
			_connectionStringPadm = connectionStringPadm;
		}

		// FUNCTION IS USED TO INSERT DATA IN DATABASE
		// ConnectionString -  CONNECTION STRING FROM CONSTATNT HELPER
		// TableName - TABLE NAME WHICH IS USED TO INSERT DATA
		// DICTIONARY FIRST PARAMERET IS FILED NAME IN DATABASE AND ANOTHER PARAMETER IS VALUE
		public int ExecuteInsertQuery(String TableName, Dictionary<string, string> FieldNValuesPara)
		{
			MySqlConnection con = new MySqlConnection(this._connectionString);

			String Query = "";
			String TableFields = "";
			String TableValues = "";

			foreach (KeyValuePair<string, string> para in FieldNValuesPara)
			{
				if (TableFields == "")
				{
					TableFields = para.Key;
					TableValues = "'" + para.Value + "'";
				}
				else
				{
					TableFields += ", " + para.Key;
					TableValues += ", '" + para.Value + "'";
				}
			}

			Query = "Insert Into " + TableName + " (" + TableFields + ") Values(" + TableValues + ");SELECT LAST_INSERT_ID();";

			MySqlCommand cmd = new MySqlCommand();
			cmd.Parameters.Clear();
			cmd.CommandText = Query;
			cmd.Connection = con;
			cmd.CommandType = System.Data.CommandType.Text;

			if (con.State == ConnectionState.Closed)
			{
				con.Open();
			}
			int id = Convert.ToInt32(cmd.ExecuteScalar());
			con.Close();
			return id;
		}

		// FUNCTION IS USED TO UPDATE DATA IN DATABASE
		// ConnectionString -  CONNECTION STRING FROM CONSTATNT HELPER
		// TableName - TABLE NAME WHICH IS USED TO INSERT DATA
		// FIRST DICTIONARY FIRST PARAMERET IS FILED NAME IN DATABASE AND ANOTHER PARAMETER IS VALUE (UPDATE FIELD DATA)
		// SECOND DICTIONARY FIRST PARAMERET IS FILED NAME IN DATABASE AND ANOTHER PARAMETER IS VALUE (WHERE FIELD DATA)
		public void ExecuteUpdateQuery(String TableNamesWithJoin, Dictionary<string, string> FieldNValuesPara, Dictionary<string, string> WhereConditionPara)
		{
			MySqlConnection con = new MySqlConnection(this._connectionString);

			String Query = "";
			String FieldNValue = "";
			String WhereCondition = "";

			foreach (KeyValuePair<string, string> para in FieldNValuesPara)
			{
				if (FieldNValue == "")
				{
					FieldNValue = para.Key + " = '" + para.Value + "'";
				}
				else
				{
					FieldNValue += ", " + para.Key + " = '" + para.Value + "'";
				}
			}

			foreach (KeyValuePair<string, string> para in WhereConditionPara)
			{
				if (WhereCondition == "")
				{
					WhereCondition = para.Key + " = '" + para.Value + "'";
				}
				else
				{
					WhereCondition += " AND " + para.Key + " = '" + para.Value + "'";
				}
			}

			Query = "Update " + TableNamesWithJoin + " SET " + FieldNValue + " Where " + WhereCondition;

			MySqlCommand cmd = new MySqlCommand();
			cmd.Parameters.Clear();
			cmd.CommandText = Query;
			cmd.Connection = con;
			cmd.CommandType = System.Data.CommandType.Text;

			if (con.State == ConnectionState.Closed)
			{
				con.Open();
			}
			cmd.ExecuteNonQuery();
			con.Close();
		}
		// THIS FUNCTION IS USED TO EXECUTE QUERY AND RETURN DATASET
		public DataSet ExecuteQueryReturnDS(string query)
		{
			DataSet ds = new DataSet();
			try
			{
				MySqlConnection con = new MySqlConnection(this._connectionString);
				MySqlCommand cmd = new MySqlCommand();
				cmd.Parameters.Clear();
				cmd.CommandText = query;
				cmd.Connection = con;
				cmd.CommandTimeout = 60;
				cmd.CommandType = System.Data.CommandType.Text;

				MySqlDataAdapter da = new MySqlDataAdapter();
				da.SelectCommand = cmd;
				da.Fill(ds);

			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return ds;
		}

		// THIS FUNCTION IS USED EXECUTE QUERY AND RETURN NOTHING
		public void ExecuteQuery(string query)
		{
			try
			{
				MySqlConnection con = new MySqlConnection(this._connectionString);
				MySqlCommand cmd = new MySqlCommand();
				cmd.Parameters.Clear();
				cmd.CommandText = query;
				cmd.Connection = con;
				cmd.CommandTimeout = 60;
				cmd.CommandType = System.Data.CommandType.Text;
				con.Open();
				cmd.ExecuteNonQuery();
				con.Close();

			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
		}
		// THIS FUNCTION IS USED TO EXECUTE QUERY AND RETURN OBJECT
		// THIS FUNCTION IS USED TO EXECUTE QUERY AND RETURN OBJECT
		public string ExecuteQueryReturnObject(string query)
		{
			string Result = "";
			try
			{
				MySqlConnection con = new MySqlConnection(this._connectionString);
				MySqlCommand cmd = new MySqlCommand();
				cmd.Parameters.Clear();
				cmd.CommandText = query;
				cmd.Connection = con;
				cmd.CommandTimeout = 60;
				cmd.CommandType = System.Data.CommandType.Text;
				if (con.State == ConnectionState.Closed)
				{
					con.Open();
				}
				Result = Convert.ToString(cmd.ExecuteScalar());
				con.Close();
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return Result;
		}


		public Object ExecuteQueryReturnObj(string query)
		{
			Object Obj = new Object();
			try
			{
				MySqlConnection con = new MySqlConnection(this._connectionString);
				MySqlCommand cmd = new MySqlCommand();
				cmd.Parameters.Clear();
				cmd.CommandText = query;
				cmd.Connection = con;
				cmd.CommandTimeout = 60;
				cmd.CommandType = System.Data.CommandType.Text;
				if (con.State == ConnectionState.Closed)
				{
					con.Open();
				}

				if (con.State == ConnectionState.Closed)
				{
					con.Open();
				}
				Obj = cmd.ExecuteScalar();
				con.Close();
				if (Obj == null)
				{
					Obj = "";
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex.Message);
			}
			return Obj;
		}

		//THIS IS USED FOR STORE PROCEDURE RETURN DATASET
		public DataSet ExecuteStoredProcedureReturnDS(string storedProcedure, Dictionary<string, object> parameters)
		{
			DataSet ds = new DataSet();
			try
			{
				using (MySqlConnection con = new MySqlConnection(this._connectionStringPadm))
				{
					using (MySqlCommand cmd = new MySqlCommand())
					{
						cmd.Parameters.Clear();
						cmd.CommandText = storedProcedure;
						cmd.Connection = con;
						cmd.CommandTimeout = 60;
						cmd.CommandType = System.Data.CommandType.StoredProcedure;

						// Add parameters from the dictionary to the command
						foreach (var param in parameters)
						{
							cmd.Parameters.AddWithValue(param.Key, param.Value);
						}

						using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
						{
							da.Fill(ds);
						}
					}
				}
			}
			catch (Exception ex)
			{
			}

			return ds;
		}

		public DataSet ExecuteStoredProcedureWithJsonInput(string uniqueId, string ip_address, string storedProcedure)
		{
			DataSet result = new DataSet();

			try
			{
				using (MySqlConnection connection = new MySqlConnection(_connectionString)) // Replace with your connection string variable
				{
					using (MySqlCommand command = new MySqlCommand(storedProcedure, connection))
					{
						command.CommandType = CommandType.StoredProcedure;

						// Add JSON input parameter
						command.Parameters.AddWithValue("@unique_id", uniqueId);
						command.Parameters.AddWithValue("@ip_address", ip_address);

						// MySQL does not support output parameters directly like SQL Server,
						// but we can handle results through multiple result sets.
						connection.Open();

						using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
						{
							adapter.Fill(result); // Fill result with all result sets (valid and invalid data)
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Handle errors (e.g., logging)
				Console.WriteLine($"Error: {ex.Message}");
			}

			return result;
		}
	}
}
