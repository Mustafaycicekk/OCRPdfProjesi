using OCRPdf.Data.Entities;
using OCRPdf.Repository.Concrete;
using OCRPdf.Service.Abstract;

namespace OCRPdf.Service.Services;
public class UserService : BaseService<UserRepository, User> {
	public UserService(UserRepository repository) : base(repository) { }
}