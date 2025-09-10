using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;

namespace PersonsListTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Action<IServiceCollection>? ConfigureTestServicesAction { get; set; }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                

                var dbContextDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                                d.ImplementationType == typeof(ApplicationDbContext))
                    .ToList();

                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DatabaseForTest");
                });

                var descriptorIPersonService = services.SingleOrDefault(
                d => d.ServiceType == typeof(IPersonsGetterService));

                if (descriptorIPersonService != null)
                    services.Remove(descriptorIPersonService);

                // Apply test service overrides
                ConfigureTestServicesAction?.Invoke(services);

            });
        }
    }
}
