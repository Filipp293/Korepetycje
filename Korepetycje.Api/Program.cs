using Korepetycje.Api.Data;
using Korepetycje.Api.Services;
using Korepetycje.Api.Settings;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Ustawienia i połączenie z MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<MongoDbContext>();

// Rejestracja serwisów aplikacji
builder.Services.AddScoped<IAuthService, MongoAuthService>();
builder.Services.AddScoped<IEmailService, MockEmailService>();

// Kontrolery i autoryzacja
builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Dokumentacja OpenAPI (Scalar)
builder.Services.AddOpenApi();

// Polityka CORS dla Blazora
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();