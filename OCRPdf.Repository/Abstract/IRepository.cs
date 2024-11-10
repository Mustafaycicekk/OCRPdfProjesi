using static Dapper.SqlMapper;

namespace OCRPdf.Repository.Abstract;
public interface IRepository<T> where T : class {
	T GetById(int id);
	IEnumerable<T> GetAll();
	int Add<TEntity>(TEntity entity);
	int Update(T entity);
	int Delete(int id);
	IEnumerable<T> Filter(Dictionary<string, object> filters);
}