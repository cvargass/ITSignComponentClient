using ClientSignerApp.Services.APIStoreFiles;
using ClientSignerApp.Services.Logger;
using ClientSignerApp.Services.QRGenerator;
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
            services.AddTransient<IQRGeneratorService, QRGeneratorService>();

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
                var parameters = args[0].Split("_");

                string type = parameters[0]; // massive/individual

                if (type is not null
                    && (type == "massive" || type == "individual"))
                {
                    mainForm.SetSigningType(type);
                    mainForm.SetGuidFiles(parameters);
                }
                else
                {
                    MessageBox.Show("Parámetros inválidos para iniciar la aplicación. " + args[0]);
                    return;
                }
            }
            else
            {
                ///
                // FOR TESTING PURPOSES 
                ///

                string type = "individual"; // massive/individual
                mainForm.SetSigningType(type);

                if (type == "massive")
                {
                    //ESTRUCTURE type_file1_file2_file3
                    mainForm.SetGuidFiles(new string[] { "massive", "27AGOSTO2025-0117CD24-2000-31", "27AGOSTO2025-0117CD24-2000-32", "27AGOSTO2025-0117CD24-2000-33" });
                }
                else if (type == "individual")
                {
                    //ESTRUCTURE type_file1_page|x|y
                    mainForm.SetGuidFiles(new string[] { "individual", "29AGOSTO2025-726AEF3F-2000-31", "1|50|50" });
                } else
                {
                    MessageBox.Show("Parámetros inválidos para iniciar la aplicación.");
                    return;
                }
            }

            mainForm.SignDocument();
            Application.Run();
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