using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OCRPdf.Data.Entities;
using OCRPdf.Helpers;
using OCRPdf.Service.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCRPdf.Api.Controllers;
[ApiController, Route("[controller]/[action]")]
public class UserController(IServiceProvider serviceProvider, IConfiguration configuration) : ControllerBase {
	private readonly string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Default baðlantýya ulaþýlamadý");
	private readonly IServiceProvider ServiceProvider = serviceProvider;

	[HttpPost]
	public ServiceResponse<User> Add(User user) {
		user.PASSWORD = BcryptHasher.HashPassword(user.PASSWORD);
		UserService? userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<User> userAdded = userService.Add(user);
		return userAdded;
	}
	[HttpDelete]
	public ServiceResponse<User> Delete(int id) {
		UserService? userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<User> userDeleted = userService.Delete(id);
		return userDeleted;
	}
	[HttpPut]
	public ServiceResponse<User> Update(User user) {
		UserService? userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<User> userUpdated = userService.Update(user);
		return userUpdated;
	}
	[HttpGet]
	public ServiceResponse<User> Get(int id) {
		UserService? userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<User> userGet = userService.GetById(id);
		return userGet;
	}
	[HttpOptions]
	public ServiceResponse<IEnumerable<User>> GetAll() {
		UserService? userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<IEnumerable<User>> userGetAll = userService.GetAll();
		return userGetAll;
	}
	[HttpOptions]
	public ServiceResponse<IEnumerable<User>> Filter(Dictionary<string, object> filters) {
		UserService? userService = ServiceProvider.GetService<UserService>() ?? throw new InvalidOperationException("Service de hata");
		ServiceResponse<IEnumerable<User>> userFiltered = userService.Filter(filters);
		return userFiltered;
	}
	[HttpPost]
	public async Task<ServiceResponse<User>> Login(string email, string password) {
		using SqlConnection connection = new(connectionString);
		string query = "SELECT * FROM [User] WHERE EMAIL = @Email";
		await connection.OpenAsync();
		User? user = await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
		if (user == null) { return ServiceResponse<User>.ErrorResponse("Kullanýcý bulunamadý"); }
		bool isPasswordValid = BcryptHasher.VerifyPassword(password, user.PASSWORD);
		if (!isPasswordValid) { return ServiceResponse<User>.ErrorResponse("Geçersiz þifre"); }
		string token = TokenHelper.GenerateToken(user.EMAIL);
		return ServiceResponse<User>.SuccessResponse(user, token);
	}

}
