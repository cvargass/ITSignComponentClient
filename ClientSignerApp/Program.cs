using ClientSignerApp.Services.APIStoreFiles;
using ClientSignerApp.Services.Logger;
using ClientSignerApp.Services.Signer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;
namespace ClientSignerApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            // Registro del Protocolo
            if (args.Length > 0 && args[0].Equals("--registerProtocol", StringComparison.OrdinalIgnoreCase))
            {
                if (IsUserAdministrator())
                {
                    RegisterProtocol();
                }
                else
                {
                    MessageBox.Show("Debe ejecutar como administrador para registrar el protocolo.");
                }
                return;
            }

            var baseConfig = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
               .Build();

            var configurationManager = new ConfigurationManager();
            configurationManager.AddConfiguration(baseConfig);


            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configurationManager);
            services.AddTransient<SignerApp>();
            services.AddTransient<ILoggerService, LoggerService>();
            services.AddTransient<ISignerService, SignerService>();

            services.AddHttpClient<IAPIStoreFilesService, APIStoreFilesService>(client =>
            {
                client.BaseAddress = new Uri(configurationManager["APIStoreFiles:BaseUrl"]);
            });

            //services.Configure<SignOptions>(configurationManager.GetSection("SignConfigurations"));

            var serviceProvider = services.BuildServiceProvider();

            var mainForm = serviceProvider.GetRequiredService<SignerApp>();

            if (args.Length > 0)
            {
                args[0] = args[0].Replace("appsigner:", "");
                mainForm.SetGuidFiles(args[0].Split("_"));
            }
            else
            {
                mainForm.SetGuidFiles(new string[] { "27AGOSTO2025-0117CD24-2000-31", "true|6" });   //last param is signature configuration visible|idSignPosition
            }

            mainForm.Visible = false;
            Application.Run(mainForm);
        }

        private static bool IsUserAdministrator()
    {
        try
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        catch
        {
            return false;
        }
    }

        private static void RegisterProtocol()
        {
            try
            {
                string appPath = Process.GetCurrentProcess().MainModule.FileName;

                using (var key = Registry.ClassesRoot.CreateSubKey("appsigner"))
                {
                    key.SetValue("", "URL: Client Signer Application");
                    key.SetValue("URL Protocol", "");

                    using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue("", $"\"{appPath}\" \"%1\"");
                    }
                }

                var toast = new ToolTip();
                toast.Show("Protocolo registrado exitosamente", new Form(), 3000);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registrando protocolo: {ex.Message}");
            }
        }

        private static bool IsProtocolRegistered()
        {
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey("appsigner"))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}