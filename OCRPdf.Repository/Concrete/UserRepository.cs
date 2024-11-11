using OCRPdf.Data.Entities;
using OCRPdf.Repository.Abstract;
using System.Data;

namespace OCRPdf.Repository.Concrete;
public class UserRepository : DapperRepository<User> {
	public UserRepository(IDbConnection dbConnection) : base(dbConnection) { }
	public UserRepository() : base(null) { }
}
