using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace FileStorage
{
    public class Program
    {
        public static int Main(string[] args)
        {
            //日志的使用太简单，重新规划
//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json")
//                .AddEnvironmentVariables()
//                .Build();

//            Log.Logger = new LoggerConfiguration()
//#if DEBUG
//                .MinimumLevel.Debug()
//#else
//                .MinimumLevel.Information()
//#endif
//                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
//                .Enrich.FromLogContext()
//                .WriteTo.Async(c => c.File("Logs/logs.txt"))
//                .WriteTo.Console()
//                .CreateLogger();

            try
            {
                Log.Information("Starting FileStorage.Host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseAutofac()
                     //.UseSerilog();
                .UseSerilog((context, logger) =>
                {
                    logger.ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext();
                });
    }
}
