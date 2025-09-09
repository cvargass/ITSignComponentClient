using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;

[RunInstaller(true)]
public class CustomInstaller : Installer
{
    public override void Install(IDictionary stateSaver)
    {
        base.Install(stateSaver);

        try
        {
            // Recupera parámetros pasados desde el instalador
            string installDir = Context.Parameters["targetdir"];
            string apiBaseUrl = Context.Parameters["URL_API"];
            string pageFilesUrl = Context.Parameters["URL_FRONT"];

            if (string.IsNullOrEmpty(apiBaseUrl))
                throw new InstallException("Debe ingresar un valor para Url API.");

            if (string.IsNullOrEmpty(pageFilesUrl))
                throw new InstallException("Debe ingresar un valor para Url Sitio Web.");

            if (string.IsNullOrEmpty(installDir))
                throw new Exception("No se recibió targetdir en parámetros.");

            string configPath = Path.Combine(installDir, "appsettings.json");

            if (!File.Exists(configPath))
                throw new FileNotFoundException("No se encontró el archivo appsettings.json", configPath);

            // Cargar JSON
            var json = JObject.Parse(File.ReadAllText(configPath));

            // Reemplazar valores
            json["APIStoreFiles"]["BaseUrl"] = apiBaseUrl;
            json["PageFiles"]["Url"] = pageFilesUrl;

            // Guardar cambios
            File.WriteAllText(configPath, json.ToString());

        }
        catch (Exception ex)
        {
            // Si falla, escribir log dentro de la carpeta de instalación
            string logPath = Path.Combine(
                Context.Parameters["targetdir"] ?? "C:\\Temp",
                "InstallerError.log"
            );
            File.WriteAllText(logPath, $"Error en CustomInstaller: {ex}");
            throw ex;
        }
    }
}