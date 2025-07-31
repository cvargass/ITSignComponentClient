using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StoreFiles.API.Services.StoreFiles;
using StoreFiles.Core.Filters;
using StoreFiles.Core.Options;
using StoreFiles.Core.Services.Cades;
using StoreFiles.Core.Services.Logger;
using StoreFiles.Core.Services.Sign;
using StoreFiles.Core.Services.StoreFiles;
using StoreFiles.Core.Services.Utils;
using StoreFiles.Core.Services.Utils.QRGenerator;
using StoreFiles.Core.Services.Utils.WriterQR;
using StoreFiles.Core.Services.Xades;
using System;

namespace StoreFiles.API
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
            services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });

            int megabytes = int.Parse(Configuration["AllowedFileSizeMBComponent"] ?? "100");
            long bytes = megabytes * 1024 * 1024;

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = bytes;
            });

            //Services
            services.AddTransient<IStoreFileService, StoreFileService>();
            services.AddTransient<ISignService, SignService>();
            services.AddTransient<ILoggerService, LoggerService>();
            services.AddTransient<ICadesService, CadesService>();
            services.AddTransient<IXadesService, XadesService>();

            services.AddTransient<IUtilsService, UtilsService>();
            services.AddTransient<IQRGeneratorService, QRGeneratorService>();
            services.AddTransient<IWriterQRService, WriterQRService>();

            services.AddServerSideBlazor(options => {
                 options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(10); // 10 minutos
                 options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(30); // Retención de circuito
             });

            services.Configure<SignOptions>(Configuration.GetSection("SignConfigurations"));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "StoreFiles.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StoreFiles.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
