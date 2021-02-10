using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using num4.Data;
using num4.Models;
using num4.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;

namespace num4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            try
            {
                var scope = host.Services.CreateScope();

                var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userMng = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleMng = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                ctx.Database.EnsureCreated();

                var userRole = new IdentityRole("User");
                if (!ctx.Roles.Contains(userRole))
                {
                    roleMng.CreateAsync(userRole).GetAwaiter().GetResult();
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
.ConfigureAppConfiguration((context, config) =>
{
var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
config.AddAzureKeyVault(
keyVaultEndpoint,
new DefaultAzureCredential());
})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
