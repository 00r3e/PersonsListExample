using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;
using RepositoryContracts;
using Repositories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using PersonsListExample.Filters.ActionFilters;

var builder = WebApplication.CreateBuilder(args);

//Logging
//builder.Logging.ClearProviders().AddConsole().AddDebug().AddEventLog();

//Serilog
builder.Host.UseSerilog( (HostBuilderContext context, IServiceProvider service, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration) // Read configuration settings from build-in IConfiguration
        .ReadFrom.Services(service); // Read app's services 
});

//adds controllers and views as services
builder.Services.AddControllersWithViews(options =>
{
    ////without argument values
    //options.Filters.Add<ResponseHeaderActionFilter>(5); 

    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

    options.Filters.Add(new ResponseHeaderActionFilter(logger) {Key = "X-Global-Custom-Key", Value = "Global-Custom-Value", Order = 2 });
});

//add services into IoC Container
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties |
    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))/*.EnableSensitiveDataLogging()*/;
});

builder.Services.AddTransient<ResponseHeaderActionFilter>();



if (!builder.Environment.IsEnvironment("Test"))
{
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSerilogRequestLogging();
app.UseHttpLogging();

//app.Logger.LogDebug("debug-message");
//app.Logger.LogInformation("information-message");
//app.Logger.LogWarning("warning-message");
//app.Logger.LogError("error-message");
//app.Logger.LogCritical("critical-message");

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program { } //make Program accessible programmatically