using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MS.API.Customers.Constants;
using MS.API.Customers.Extensions;
using MS.API.Extensions;
using MS.API.Middlewares;
using MS.API.Services.Logging;
using MS.Core.Extensions;

namespace MS.API.Customers
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var logger = ConfigurationHelper.GetBaseConfiguration(Configuration, ApplicationConstants.APPLICATION_NAME);
            services.AddSingleton(logger);
            services.AddCoreOptions();
            services.AddAutoMapper(new List<Assembly> {Assembly.GetExecutingAssembly()});
            services.AddDataAccessLayer(Configuration);
            services.AddAPI(new List<Assembly> {Assembly.GetExecutingAssembly()});

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "MS", Version = "v1"});
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<LoggingMiddleware>(ApplicationConstants.APPLICATION_NAME);
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MS v1"));

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}