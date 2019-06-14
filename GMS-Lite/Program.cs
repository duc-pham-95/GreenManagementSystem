using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GMS.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GMS_Lite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args);

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var serviceProvider = services.GetRequiredService<IServiceProvider>();
                    var configuration = services.GetRequiredService<IConfiguration>();
                    Seed.CreateRoles(serviceProvider, configuration).Wait();

                }
                catch (Exception exception)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    string s = exception.Message;
                    logger.LogError(exception, exception.ToString());
                }
            }

            host.Run();
        }

        //public static IWebHost CreateWebHostBuilder(string[] args)
        //{
        //    //var config = new ConfigurationBuilder()
        //    //            .SetBasePath(Directory.GetCurrentDirectory())
        //    //            .AddJsonFile("config.json", optional: true, reloadOnChange: true)
        //    //            .Build();
        //    var host = WebHost.CreateDefaultBuilder(args)
        //        .ConfigureAppConfiguration((hostingContext, config) =>
        //        {
        //            //config.AddJsonFile("config.json", optional: true, reloadOnChange: false);
        //            config.AddCommandLine(args);
        //        })
        //        .UseStartup<Startup>();
        //    return host;
        //}

        public static IWebHost CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

    }
}
