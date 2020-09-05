using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SensorData.Api.Data;
using SensorData.Api.Data.CosmosDB;
using SensorData.Api.Data.SqlServer;
using SensorData.Api.infrastructure;
using SensorData.Api.Security;
using SensorDataApi;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SensorData.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register the Swagger services
            services.AddSwaggerDocument();

            // Auto Mapper Configurations
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.AddScoped(typeof(ISensorDataRepository), typeof(SqlSensorDataRepository));
            services.AddScoped(typeof(IDataSourceRepository), typeof(SqlDataSourceRepository));
            services.AddScoped(typeof(IDataTypeRepository), typeof(SqlDataTypeRepository));
            services.AddScoped(typeof(INotificationRepository), typeof(SqlNotificationRepository));

            //services.AddScoped(typeof(ISensorDataRepository), typeof(CdbSensorDataRepository));
            //services.AddScoped(typeof(IDataSourceRepository), typeof(CdbDataSourceRepository));
            //services.AddScoped(typeof(IDataTypeRepository), typeof(CdbDataTypeRepository));
            //services.AddScoped(typeof(INotificationRepository), typeof(CdbNotificationRepository));

            services.AddSignalR();
            services.AddControllers();

            var connectionstring = Configuration.GetConnectionString("SensorDataSqlEntities");
            services.AddDbContext<SensorDataSqlContext>(options => options.UseSqlServer(connectionstring));

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                Debug.WriteLine($"AppIdUri: {Configuration["Authentication:AppIdUri"]}");
                Debug.WriteLine($"AppIdUri: {Configuration["Authentication:ClientId"]}");
                    //https://joonasw.net/view/azure-ad-authentication-aspnet-core-api-part-1
                    o.Authority = Configuration["Authentication:Authority"];
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                        // Both App ID URI and client id are valid audiences in the access token                        
                        ValidAudiences = new List<string>
                    {
                        Configuration["Authentication:AppIdUri"],
                        Configuration["Authentication:ClientId"],
                        "1af2a885-6aed-4fbf-8866-51321da47926"
                            //  var cors = new EnableCorsAttribute("http://localhost:4200,https://localhost:4200,http://sensordataapp.azurewebsites.net,https://sensordataapp.azurewebsites.net", "*", "*");
                        }
                };
                    //https://medium.com/agilix/asp-net-core-supporting-multiple-authorization-6502eb79f934
                    //https://jasonwatmore.com/post/2018/09/08/aspnet-core-21-basic-authentication-tutorial-with-example-api
                }).AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                var origins = Configuration.GetSection("Origins").Get<List<string>>();
                builder.WithOrigins(origins.ToArray()).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            }));

            services.AddMvc(config => config.Filters.Add(typeof(ExceptionFilter)))
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        }

        private async Task<string> AuthenticateVault(string authority, string resource, string scope)
        {
            var clientCredential = new ClientCredential("769b1a7c-737d-4bc8-942f-5f2c305e087c", "36NZ3FZiO5biqrqbWugmh?WKOamiJ=/=");
            var authenticateContext = new AuthenticationContext(authority);
            var result = await authenticateContext.AcquireTokenAsync(resource, clientCredential);
            return result.AccessToken;
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            app.UseCors("CorsPolicy");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication(); //moet na UseRouting

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<SensorDataHub>("/sensordatahub");
            });
        }
    }
}
