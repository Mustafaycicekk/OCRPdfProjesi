namespace OCRPdf.Repository.Abstract;
public interface IRepository<T> where T : class {
	T GetById(int id);
	IEnumerable<T> GetAll();
	int Add<TEntity>(TEntity entity);
	int Update<TEntity>(T entity);
	int Delete(int id);
	IEnumerable<T> Filter(Dictionary<string, object> filters);
	IEnumerable<T> GetColumns(IEnumerable<string> columnNames);
}