using OCRPdf.Helpers;

namespace OCRPdf.Service.Abstract;
public interface IBaseService<TEntity> where TEntity : class
{
    ServiceResponse<IEnumerable<TEntity>> GetAll();
    ServiceResponse<TEntity> GetById(int id);
    ServiceResponse<TEntity> Add(TEntity entity);
    ServiceResponse<TEntity> Update(TEntity entity);
    ServiceResponse<TEntity> Delete(int id);
    ServiceResponse<IEnumerable<TEntity>> Filter(Dictionary<string, object> filters);
}