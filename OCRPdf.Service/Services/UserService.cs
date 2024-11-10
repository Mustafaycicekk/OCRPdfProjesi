using OCRPdf.Data.Entities;
using OCRPdf.Repository.Concrete;
using OCRPdf.Service.Abstract;

namespace OCRPdf.Service.Services;
public class UserService(UserRepository repository) : BaseService<UserRepository, User>(repository) {
}