using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OCRPdf.Data.Entities;
using OCRPdf.Repository.Concrete;
using OCRPdf.Service.Abstract;
using OCRPdf.Service.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCRPdf.Api.Configurations;
public static class ServiceConfiguration {
	public static void ConfigureServices(this IServiceCollection services) {
		// JSON Serileştirme Ayarları
		services.AddControllers().AddNewtonsoftJson(options =>
			options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

		// Swagger Ayarları
		services.AddSwaggerGen(x => {
			x.SwaggerDoc("v1", new OpenApiInfo {
				Title = "OCR PDF REST API",
				Version = "Version: 1.0.0.1",
				Description = "OCR PDF General Service",
			});

			x.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme {
				Name = "ApiKey",
				Type = SecuritySchemeType.ApiKey,
				In = ParameterLocation.Header,
				Description = "Servisleri kullanabilmek için <b>Mustafa Ayçiçek</b> ile iletişime geçmeniz ve <b>ApiKey</b> elde etmeniz gerekmektedir. " +
							  "Key bilgisini \"Header\" içerisinde \"Apikey\" başlığı altına verdikten sonra API'den bilgi alabilirsiniz."
			});
			x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
				Name = "Authorization",
				Type = SecuritySchemeType.ApiKey,
				In = ParameterLocation.Header,
				Scheme = "Bearer",
				BearerFormat = "JWT",
				Description = "\"<b>/User/Login\"</b> metoduyla \"Token\" almanız ve <b>Bearer {Token}</b> şeklinde yazmanız gerekmektedir."
			});
			x.AddSecurityRequirement(new OpenApiSecurityRequirement {
				{ new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new List<string>() },
				{ new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }}, new List<string>() },
			});
		});

		// JWT Authentication Ayarları
		SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes("fgai2cWFRU7lyuWSl9ZKHHCS6vWDJuBIcexzgl4lRz2wsbiIEG3Ks9fYjbK888Yl"));
		services.AddAuthentication(options => {
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options => {
			options.TokenValidationParameters = new TokenValidationParameters {
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateIssuerSigningKey = true,
				ValidIssuer = "Dersigo",
				ValidAudience = "Dersigo App",
				IssuerSigningKey = symmetricSecurityKey,
				ClockSkew = TimeSpan.Zero
			};
		});

		services.AddAuthorization();

		// Servisler
		services.AddScoped<UserRepository>();
		services.AddScoped<UserService>();
		services.AddScoped<BaseService<UserRepository, User>>();

		// CORS Ayarları
		services.AddCors(options => {
			options.AddPolicy("AllowAll", builder => {
				builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
			});
		});

		// API Endpointler ve Swagger için gerekli servisler
		services.AddEndpointsApiExplorer();
	}
}

