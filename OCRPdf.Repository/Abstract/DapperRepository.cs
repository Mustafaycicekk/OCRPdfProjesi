using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

namespace OCRPdf.Repository.Abstract;
public class DapperRepository<T>(IDbConnection dbConnection) : IRepository<T> where T : class
{
    private readonly IDbConnection DbConnection = dbConnection;

    public int Add(T entity)
    {
        string query = GenerateInsertQuery();
        return DbConnection.Execute(query, entity);
    }

    public int Delete(int id)
    {
		string query = $"DELETE FROM [{typeof(T).Name}] WHERE ID = @Id";
        return DbConnection.Execute(query, new { Id = id });
    }

    public IEnumerable<T> GetAll()
    {
        string query = $"SELECT * FROM [{typeof(T).Name}]";
        return DbConnection.Query<T>(query);
    }

    public T GetById(int id)
    {
        string query = $"SELECT * FROM [{typeof(T).Name}] WHERE ID = @Id";
        return DbConnection.QueryFirstOrDefault<T>(query, new { Id = id });
    }

    public int Update(T entity)
    {
        string query = GenerateUpdateQuery();
        return DbConnection.Execute(query, entity);
    }
	public IEnumerable<T> Filter(Dictionary<string, object> filters) {
		string tableName = typeof(T).Name;
		List<string> conditions = [];
		DynamicParameters parameters = new();

		foreach (KeyValuePair<string, object> filter in filters) {
			conditions.Add($"{filter.Key} = @{filter.Key}");
			parameters.Add(filter.Key, filter.Value);
		}

		string whereClause = conditions.Count != 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;
		string query = $"SELECT * FROM [{tableName}] {whereClause}";
		return DbConnection.Query<T>(query, parameters);
	}
	private static string GenerateInsertQuery() {
		string tableName = typeof(T).Name;
		PropertyInfo[] properties = typeof(T).GetProperties();
		IEnumerable<PropertyInfo> columns = properties.Where(p => p.Name != "ID");
		string columnNames = string.Join(", ", columns.Select(p =>
		{
			ColumnAttribute columnAttr = p.GetCustomAttribute<ColumnAttribute>();
			return columnAttr != null ? columnAttr.Name : p.Name;
		}));
		string parameterNames = string.Join(", ", columns.Select(p => "@" + p.Name));
		return $"INSERT INTO [{tableName}] ({columnNames}) VALUES ({parameterNames})";
	}

	private static string GenerateUpdateQuery()
    {
        string tableName = typeof(T).Name;
        PropertyInfo[] properties = typeof(T).GetProperties();
		IEnumerable<PropertyInfo> columns = properties.Where(p => p.Name != "ID");
		string setClause = string.Join(", ", columns.Select(p => $"{p.Name} = @{p.Name}"));
		return $"UPDATE [{tableName}] SET {setClause} WHERE ID = @Id";
    }
}