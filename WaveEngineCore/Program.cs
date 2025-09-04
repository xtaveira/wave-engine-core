using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microwave.Application;
using Microwave.Domain;
using Microwave.Domain.Interfaces;
using Microwave.Infrastructure.Repositories;
using Microwave.Infrastructure.Services;
using Microwave.Infrastructure.Logging;
using WaveEngineCore.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var jwtKey = Encoding.UTF8.GetBytes("MicrowaveWaveEngineCore2025SecretKey123!@#$%^&*()");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICryptographyService, CryptographyService>();
builder.Services.AddScoped<IAuthRepository>(provider =>
{
    var dataPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "auth-settings.json");
    return new JsonAuthRepository(dataPath);
});
builder.Services.AddScoped<IExceptionLogger>(provider =>
{
    var logPath = Path.Combine(builder.Environment.ContentRootPath, "Logs", "exceptions.log");
    return new FileExceptionLogger(logPath);
});

builder.Services.AddScoped<IMicrowaveService, MicrowaveService>();
builder.Services.AddScoped<ICustomProgramRepository>(provider =>
{
    var dataPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "custom-programs.json");
    return new JsonCustomProgramRepository(dataPath);
});
builder.Services.AddScoped<IProgramDisplayService, ProgramDisplayService>();
builder.Services.AddScoped<CustomProgramService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSession();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

public partial class Program { }
