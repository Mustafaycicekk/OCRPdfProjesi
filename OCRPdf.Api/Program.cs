using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OCRPdf.Api.Configurations;
using OCRPdf.Api.Model;
using System.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Swagger ve CORS gibi yap�land�rmalar�n eklendi�i servisleri y�kle
builder.Services.ConfigureServices();
builder.Services.Configure<TesseractSettings>(builder.Configuration.GetSection("Tesseract"));
builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

WebApplication app = builder.Build();

// CORS konfig�rasyonu
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// API controller'lar� ekleyelim
app.MapControllers();

// Swagger UI sadece geli�tirme ortam�nda aktif olacak
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI(c => {
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dersigo API v1");
	});
}

app.Run();
