using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCRPdf.Data.Entities;
using OCRPdf.Helpers;
using OCRPdf.Service.Services;

namespace OCRPdf.Api.Controllers;
[ApiController, Route("[controller]/[action]"), Authorize]
public class UserController(IServiceProvider serviceProvider) : ControllerBase {
	private readonly IServiceProvider ServiceProvider = serviceProvider;
	[HttpPost]
	public ServiceResponse<User> Add(User user) {
		user.PASSWORD = BcryptHasher.HashPassword(user.PASSWORD);
		ServiceResponse<User> userResponse = ServiceProvider.GetService<UserService>().Add(user);
		return userResponse;
	}
	[HttpDelete]
	public ServiceResponse<User> Delete(int id) {
		ServiceResponse<User> userResponse = ServiceProvider.GetService<UserService>().Delete(id);
		return userResponse;
	}
	[HttpPut]
	public ServiceResponse<User> Update(User user) {
		ServiceResponse<User> userResponse = ServiceProvider.GetService<UserService>().Update(user);
		return userResponse;
	}
	[HttpGet]
	public ServiceResponse<User> Get(int id) {
		ServiceResponse<User> userResponse = ServiceProvider.GetService<UserService>().GetById(id);
		return userResponse;
	}
	[HttpGet]
	public ServiceResponse<IEnumerable<User>> GetAll() {
		ServiceResponse<IEnumerable<User>> userResponse = ServiceProvider.GetService<UserService>().GetAll();
		return userResponse;
	}
	[HttpPost]
	public ServiceResponse<IEnumerable<User>> Filter([FromBody]Dictionary<string, object> filters) {
		ServiceResponse<IEnumerable<User>> userResponse = ServiceProvider.GetService<UserService>().Filter(filters);
		return userResponse;
	}
	[HttpPost, AllowAnonymous]
	public ServiceResponse<User> Login(string userName, string password) {
		Dictionary<string, object> filters = new() { { "KULLANICI_ADI", userName } };
		ServiceResponse<IEnumerable<User>> user = ServiceProvider.GetService<UserService>().Filter(filters);
		if (user == null) { return ServiceResponse<User>.ErrorResponse("Kullanýcý bulunamadý"); }
		bool isPasswordValid = BcryptHasher.VerifyPassword(password, user.Data.First().PASSWORD);
		if (!isPasswordValid) { return ServiceResponse<User>.ErrorResponse("Geçersiz þifre"); }
		string token = TokenHelper.GenerateToken(user.Data.First().KULLANICI_ADI);
		return ServiceResponse<User>.SuccessResponse(user.Data.First(), token);
	}

}
