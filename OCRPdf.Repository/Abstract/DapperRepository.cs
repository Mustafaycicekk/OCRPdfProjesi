using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using static Dapper.SqlMapper;

namespace OCRPdf.Repository.Abstract;
public class DapperRepository<T> : IRepository<T> where T : class {
	protected readonly IDbConnection DbConnection;
	public DapperRepository() { }

	public DapperRepository(IDbConnection dbConnection) {
		DbConnection = dbConnection;
	}
	public int Add<TEntity>(TEntity entity) {
		string query = GenerateInsertQuery<TEntity>();
		return DbConnection.QuerySingle<int>(query, entity);
	}

	public int Delete(int id) {
		string tableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name;
		string query = $"DELETE FROM [{tableName}] WHERE ID = @Id";
		return DbConnection.Execute(query, new { Id = id });
	}

	public IEnumerable<T> GetAll() {
		string tableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name;
		string query = $"SELECT * FROM [{tableName}]";
		return DbConnection.Query<T>(query);
	}

	public T GetById(int id) {
		string tableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name;
		string query = $"SELECT * FROM [{tableName}] WHERE ID = @Id";
		return DbConnection.QueryFirstOrDefault<T>(query, new { Id = id });
	}

	public int Update<TEntity>(T entity) {
		string query = GenerateUpdateQuery<TEntity>();
		return DbConnection.Execute(query, entity);
	}
	public IEnumerable<T> Filter(Dictionary<string, object> filters) {
		string tableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name;
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
	public IEnumerable<T> GetColumns(IEnumerable<string> columnNames) {
		string tableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name;
		if (columnNames == null || !columnNames.Any()) throw new ArgumentException("İstenilen sütun girilmemiş");
		string columns = string.Join(", ", columnNames.Select(c => $"[{c}]"));
		string query = $"SELECT {columns} FROM [{tableName}]";
		return DbConnection.Query<T>(query);
	}

	private static string GenerateInsertQuery<TEntity>() {
		string tableName = typeof(TEntity).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(TEntity).Name;
		PropertyInfo[] properties = typeof(TEntity).GetProperties();
		IEnumerable<PropertyInfo> columns = properties.Where(p => p.Name != "ID");
		string columnNames = string.Join(", ", columns.Select(p => {
			ColumnAttribute columnAttr = p.GetCustomAttribute<ColumnAttribute>();
			string columnName = columnAttr != null ? columnAttr.Name : p.Name;
			return $"[{columnName}]";
		}));

		string parameterNames = string.Join(", ", columns.Select(p => "@" + p.Name));
		return $"INSERT INTO [{tableName}] ({columnNames}) VALUES ({parameterNames}); SELECT CAST(SCOPE_IDENTITY() as int);";
	}

	private static string GenerateUpdateQuery<TEntity>() {
		string tableName = typeof(TEntity).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(TEntity).Name;
		PropertyInfo[] properties = typeof(T).GetProperties();
		IEnumerable<PropertyInfo> columns = properties.Where(p => p.Name != "ID");
		string setClause = string.Join(", ", columns.Select(p => $"{p.Name} = @{p.Name}"));
		return $"UPDATE [{tableName}] SET {setClause} WHERE ID = @Id";
	}
}