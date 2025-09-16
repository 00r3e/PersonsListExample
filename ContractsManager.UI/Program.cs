using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;
using RepositoryContracts;
using Repositories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using PersonsListExample.Filters.ActionFilters;
using PersonsListExample;
using PersonsListExample.Middleware;

var builder = WebApplication.CreateBuilder(args);

//Logging
//builder.Logging.ClearProviders().AddConsole().AddDebug().AddEventLog();

//Serilog
builder.Host.UseSerilog( (HostBuilderContext context, IServiceProvider service, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration) // Read configuration settings from build-in IConfiguration
        .ReadFrom.Services(service); // Read app's services 
});


builder.Services.ConfigureServices(builder.Configuration);


if (!builder.Environment.IsEnvironment("Test"))
{
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseHsts();
app.UseHttpsRedirection();

app.UseSerilogRequestLogging();
app.UseHttpLogging();

//app.Logger.LogDebug("debug-message");
//app.Logger.LogInformation("information-message");
//app.Logger.LogWarning("warning-message");
//app.Logger.LogError("error-message");
//app.Logger.LogCritical("critical-message");

app.UseStaticFiles();  

app.UseRouting();  //Identifying action method based routee

app.UseAuthentication();  //Reading Identity cookie

app.UseAuthorization(); //Validates access permissions of the user

app.MapControllers();  //Execute the filter pipeline (actions + filters)

app.Run();

public partial class Program { } //make Program accessible programmatically