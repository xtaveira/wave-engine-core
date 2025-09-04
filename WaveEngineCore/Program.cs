using Microwave.Application;
using Microwave.Domain;
using Microwave.Domain.Interfaces;
using Microwave.Infrastructure.Repositories;
using Microwave.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
