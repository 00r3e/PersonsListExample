using Entities;
using Microsoft.EntityFrameworkCore;
using PersonsListExample.Filters.ActionFilters;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace PersonsListExample
{
    public static class ConfigureServicesExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {

            //adds controllers and views as services
            services.AddControllersWithViews(options =>
            {
                ////without argument values
                //options.Filters.Add<ResponseHeaderActionFilter>(5); 

                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

                options.Filters.Add(new ResponseHeaderActionFilter(logger) { Key = "X-Global-Custom-Key", Value = "Global-Custom-Value", Order = 2 });
            });

            //add services into IoC Container
            services.AddScoped<ICountriesService, CountriesService>();
            services.AddScoped<IPersonsService, PersonsService>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();
            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties |
                Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))/*.EnableSensitiveDataLogging()*/;
            });

            services.AddTransient<ResponseHeaderActionFilter>();

            return services;
        }
    }
}
