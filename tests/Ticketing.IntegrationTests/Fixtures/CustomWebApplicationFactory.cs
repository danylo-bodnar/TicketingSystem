using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Infrastructure.Contexts;

namespace Ticketing.IntegrationTests.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TicketingDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add test PostgreSQL DbContext
                services.AddDbContext<TicketingDbContext>(options =>
                {
                    options.UseNpgsql("Host=localhost;Port=5433;Database=ticketing_test;Username=postgres;Password=postgres");
                });






            });
        }
    }
}
