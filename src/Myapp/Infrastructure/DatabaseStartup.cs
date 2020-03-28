using System;
using MyCompany.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyCompany.Infrastructure {
    public static class DatabaseConfiguration {
        public static IServiceCollection AddDatabaseModule(this IServiceCollection @this, IConfiguration configuration)
        {
            var entityFrameworkConfiguration = configuration.GetSection("EntityFramework");

            var connection = configuration.GetConnectionString("AppDbContext");

            @this.AddDbContext<ApplicationDatabaseContext>(options =>
                options.UseMySql(connection, mySqlOptions =>
                    mySqlOptions.ServerVersion(new ServerVersion(new Version(8, 0, 19), ServerType.MySql))
            ));

            return @this;
        }

        public static IApplicationBuilder UseApplicationDatabase(this IApplicationBuilder @this,
            IServiceProvider serviceProvider, IHostEnvironment environment)
        {
            if (environment.IsDevelopment() || environment.IsProduction())
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
                    context.Database.OpenConnection();
                    context.Database.EnsureCreated();
                }
            }

            return @this;
        }
    }
}
