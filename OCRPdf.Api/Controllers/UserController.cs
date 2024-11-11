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
		UserService userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<User> userAdded = userService.Add(user);
		return userAdded;
	}
	[HttpDelete]
	public ServiceResponse<User> Delete(int id) {
		UserService userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<User> userDeleted = userService.Delete(id);
		return userDeleted;
	}
	[HttpPut]
	public ServiceResponse<User> Update(User user) {
		UserService userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<User> userUpdated = userService.Update(user);
		return userUpdated;
	}
	[HttpGet]
	public ServiceResponse<User> Get(int id) {
		UserService userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<User> userGet = userService.GetById(id);
		return userGet;
	}
	[HttpGet]
	public ServiceResponse<IEnumerable<User>> GetAll() {
		UserService userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<IEnumerable<User>> userGetAll = userService.GetAll();
		return userGetAll;
	}
	[HttpGet]
	public ServiceResponse<IEnumerable<User>> Filter(Dictionary<string, object> filters) {
		UserService userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<IEnumerable<User>> userFiltered = userService.Filter(filters);
		return userFiltered;
	}
	[HttpPost, AllowAnonymous]
	public ServiceResponse<User> Login(string email, string password) {
		Dictionary<string, object> filters = new() { { "EMAIL", email } };
		ServiceResponse<IEnumerable<User>> user = ServiceProvider.GetService<UserService>().Filter(filters);
		if (user == null) { return ServiceResponse<User>.ErrorResponse("Kullanýcý bulunamadý"); }
		bool isPasswordValid = BcryptHasher.VerifyPassword(password, user.Data.First().PASSWORD);
		if (!isPasswordValid) { return ServiceResponse<User>.ErrorResponse("Geçersiz þifre"); }
		string token = TokenHelper.GenerateToken(user.Data.First().EMAIL);
		return ServiceResponse<User>.SuccessResponse(user.Data.First(), token);
	}

}
