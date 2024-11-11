using Microsoft.Data.SqlClient;
using System.Data;

namespace OCRPdf.Helpers {
	public static class DbConnectionHelper {
		public static IDbConnection GetOpenConnection() {
			string connectionString = "Server=TB; Database=PDF; UID=sa; PWD=sasa; TrustServerCertificate=true;"; 
			SqlConnection connection = new(connectionString);
			connection.Open();
			return connection;
		}
	}
}
