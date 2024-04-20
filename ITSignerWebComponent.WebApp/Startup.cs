using CurrieTechnologies.Razor.SweetAlert2;
using ITSignerWebComponent.Core.Interfaces.Services;
using ITSignerWebComponent.Core.Services;
using ITSignerWebComponent.SignApp.APIStoreFiles;
using ITSignerWebComponent.SignApp.Data;
using ITSignerWebComponent.SignApp.Error;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StoreFiles.Core.Services.Utils;
using StoreFiles.Core.Services.Utils.QRGenerator;
using System;

namespace ITSignerWebComponent.SignApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddHttpClient<IAPIStoreFilesService, APIStoreFilesService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["APIStoreFiles:BaseUrl"]);
            });

            services.AddSignalR(e => {
                e.MaximumReceiveMessageSize = (long?)1e+7;
            });

            services.AddSweetAlert2();


            //Services
            services.AddBlazorBootstrap();
            services.AddTransient<ILicenseService, LicenseService>();
            services.AddTransient<ISignerService, SignerService>();
            services.AddSingleton<ILoggerService, LoggerService>();
            services.AddTransient<IErrorService, ErrorService>();
            services.AddTransient<IEncryptorService, EncryptorService>();
            services.AddTransient<IQRGeneratorService, QRGeneratorService>();
            services.AddTransient<IUtilsService, UtilsService>();

            //Automapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddTransient<IComponentManagerService, ComponentManagerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            //});


            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
