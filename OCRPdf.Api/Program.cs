using Microsoft.Data.SqlClient;
using OCRPdf.Api.Configurations;
using OCRPdf.Api.Model;
using System.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices();
builder.Services.Configure<TesseractSettings>(builder.Configuration.GetSection("Tesseract"));
builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

WebApplication app = builder.Build();

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI(c => {
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
	});
}

app.Run();
