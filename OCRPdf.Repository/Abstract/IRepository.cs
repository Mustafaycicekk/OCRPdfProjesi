namespace OCRPdf.Repository.Abstract;
public interface IRepository<T> where T : class {
	T GetById(int id);
	IEnumerable<T> GetAll();
	int Add(T entity);
	int Update(T entity);
	int Delete(int id);
	IEnumerable<T> Filter(Dictionary<string, object> filters);
}