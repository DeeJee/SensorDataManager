using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace SensorData.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
              //.ConfigureAppConfiguration((context, config) =>
              //{
              //    if (context.HostingEnvironment.IsProduction())
              //    {
              //        var root = config.Build();

              //        config.AddAzureKeyVault(
              //        root["KeyVault:Vault"],
              //        root["KeyVault: ClientId"],
              //        root["KeyVault: ClientSecret"]);
              //    }
              //})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

}
