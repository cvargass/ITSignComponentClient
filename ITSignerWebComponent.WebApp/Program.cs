using CurrieTechnologies.Razor.SweetAlert2;
using ITSignerWebComponent.Core.Interfaces.Services;
using ITSignerWebComponent.Core.Services;
using ITSignerWebComponent.SignApp.APIStoreFiles;
using ITSignerWebComponent.SignApp.Data;
using ITSignerWebComponent.SignApp.Error;
using ITSignerWebComponent.SignApp.Pages;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StoreFiles.Core.Entities.HubSettings;
using StoreFiles.Core.Services.Utils;
using StoreFiles.Core.Services.Utils.QRGenerator;
using System;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// =============================
//   REGISTER SERVICES
// =============================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
    

builder.Services.AddHttpClient<IAPIStoreFilesService, APIStoreFilesService>(client =>
{
    client.BaseAddress = new Uri(configuration["APIStoreFiles:BaseUrl"]);
});

int megabytes = int.Parse(configuration["AllowedFileSizeMBComponent"] ?? "100");
long bytes = megabytes * 1024 * 1024;

builder.Services.AddSignalR(e =>
{
    e.MaximumReceiveMessageSize = bytes;
});

builder.Services.AddSweetAlert2();
builder.Services.AddBlazorBootstrap();

// Servicios propios
builder.Services.AddTransient<ILicenseService, LicenseService>();
builder.Services.AddTransient<ISignerService, SignerService>();
builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddTransient<IErrorService, ErrorService>();
builder.Services.AddTransient<IEncryptorService, EncryptorService>();
builder.Services.AddTransient<IQRGeneratorService, QRGeneratorService>();
builder.Services.AddTransient<IUtilsService, UtilsService>();
builder.Services.AddTransient<IComponentManagerService, ComponentManagerService>();

builder.Services.Configure<HubSettings>(configuration.GetSection("SignalR"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<HubSettings>>().Value);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// =============================
//   BUILD APP
// =============================
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

// =============================
//   MAP ENDPOINTS
// =============================
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<NotifyHubService>("/notifyHub");

app.Run();
