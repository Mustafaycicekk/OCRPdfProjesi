using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OCRPdf.Data.Entities;
using OCRPdf.Repository.Abstract;
using OCRPdf.Repository.Concrete;
using OCRPdf.Service.Abstract;
using OCRPdf.Service.Services;
using System.Text;

namespace OCRPdf.Api.Configurations {
	public static class ServiceConfiguration {
		public static void ConfigureServices(this IServiceCollection services) {
			services.AddControllers();

			#region Swagger
			services.AddSwaggerGen(x => {
				x.SwaggerDoc("v1", new OpenApiInfo {
					Title = "OCR PDF REST API",
					Version = "v1",
					Description = "OCR PDF Service"
				});
				x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					In = ParameterLocation.Header,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					Description = "JWT Token kullanarak erişim sağlamak için /User/Login endpoint'i ile Token alın. Sonra 'Authorization' başlığına Bearer Token girin."
				});

				x.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
						new List<string>()
					},
				});
			});
			#endregion
			#region JWT
			SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes("fgai2cWFRU7lyuWSl9ZKHHCS6vWDJuBIcexzgl4lRz2wsbiIEG3Ks9fYjbK888Yl"));
			services.AddAuthentication(options => {
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options => {
				options.TokenValidationParameters = new TokenValidationParameters {
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateIssuerSigningKey = true,
					ValidIssuer = "Mustafa",
					ValidAudience = "Mustafa App",
					IssuerSigningKey = symmetricSecurityKey,
					ClockSkew = TimeSpan.Zero,
				};
			});
			#endregion
			services.AddAuthorization();
			// Servisler
			services.AddScoped<UserRepository>();
			
			services.AddScoped<UserService>();
			services.AddScoped<BaseService<UserRepository, User>>();
			services.AddScoped<OptimizasyonRepository>();
			services.AddScoped<OptimizasyonService>();
			services.AddScoped<BaseService<OptimizasyonRepository, Optimizasyon>>();
			services.AddEndpointsApiExplorer();
		}
	}
}
