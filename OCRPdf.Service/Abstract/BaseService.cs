using OCRPdf.Helpers;
using OCRPdf.Repository.Abstract;

namespace OCRPdf.Service.Abstract;
public class BaseService<TRepository, TEntity> : IBaseService<TEntity> where TRepository : IRepository<TEntity>, new() where TEntity : class, new() {
	protected TRepository Repository { get; set; }
	//public BaseService() { Repository = new TRepository();  }
	public BaseService(TRepository repository) {
		Repository = repository ?? throw new ArgumentNullException(nameof(repository));
	}

	public ServiceResponse<TEntity> Add(TEntity entity) {
		try {
			int affectedRows = Repository.Add(entity);
			if (affectedRows > 0) { return ServiceResponse<TEntity>.SuccessResponse(entity); }
			else { return ServiceResponse<TEntity>.ErrorResponse("Data Eklenemedi"); }
		}
		catch (Exception ex) { return ServiceResponse<TEntity>.ErrorResponse(ex.Message); }
	}


	public ServiceResponse<TEntity> Delete(int id) {
		try {
			int affectedRows = Repository.Delete(id);
			if (affectedRows > 0) { return ServiceResponse<TEntity>.SuccessResponse(null); }
			else { return ServiceResponse<TEntity>.ErrorResponse("Data Silinemedi"); }
		}
		catch (Exception ex) { return ServiceResponse<TEntity>.ErrorResponse(ex.Message); }

	}

	public ServiceResponse<IEnumerable<TEntity>> GetAll() {
		try {
			IEnumerable<TEntity> entities = Repository.GetAll();
			return ServiceResponse<IEnumerable<TEntity>>.SuccessResponse(entities);
		}
		catch (Exception ex) { return ServiceResponse<IEnumerable<TEntity>>.ErrorResponse(ex.Message); }
	}

	public ServiceResponse<TEntity> GetById(int id) {
		try {
			TEntity entity = Repository.GetById(id);
			if (entity != null) { return ServiceResponse<TEntity>.SuccessResponse(entity); }
			else { return ServiceResponse<TEntity>.ErrorResponse("Data Bulunamadı"); }
		}
		catch (Exception ex) { return ServiceResponse<TEntity>.ErrorResponse(ex.Message); }
	}

	public ServiceResponse<TEntity> Update(TEntity entity) {
		try {
			int affectedRows = Repository.Update(entity);
			if (affectedRows > 0) { return ServiceResponse<TEntity>.SuccessResponse(entity); }
			else { return ServiceResponse<TEntity>.ErrorResponse("Güncellenemedi"); }
		}
		catch (Exception ex) { return ServiceResponse<TEntity>.ErrorResponse(ex.Message); }
	}
	public ServiceResponse<IEnumerable<TEntity>> Filter(Dictionary<string, object> filters) {
		try {
			IEnumerable<TEntity> entities = Repository.Filter(filters);
			return ServiceResponse<IEnumerable<TEntity>>.SuccessResponse(entities);
		}
		catch (Exception ex) { return ServiceResponse<IEnumerable<TEntity>>.ErrorResponse(ex.Message); }
	}
}