using StoreFiles.Core.DTOs.Grafic;
using StoreFiles.Core.Exceptions;
using System;
using System.IO;

namespace StoreFiles.Core.Services.GraficSigning
{
    public class GraficSigningService : IGraficSigningService
    {
        private string _graficsPath = "./Grafics/";
        public string StoreGrafic(PostGraficDto postGraficDto)
        {
            try
            {
                byte[] grafic = Convert.FromBase64String(postGraficDto.GraficBase64);

                ValidateExistingGraficFolder();

                string filePath = GeneratePathGrafic(postGraficDto.Guid);

                using var writer = new BinaryWriter(File.OpenWrite(filePath));
                writer.Write(grafic);

                return postGraficDto.Guid;

                //if (grafic.Length <= _allowedFileSizeMBComponent)
                //{

                //}
                //else
                //{
                //    var mbAllowed = (_allowedFileSizeMBComponent / _megaByte).ToString();
                //    throw new InternalErrorException("El tamaño del grafico máximo permitido es " + mbAllowed + "MB");
                //}
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The input is not a valid Base-64"))
                    throw new InternalErrorException("El string-base64 del archivo no es válido");
                else
                    throw new InternalErrorException(ex.Message);
            }
        }

        public (bool flag, byte[] bytesFile) GetGrafic(string guidFile)
        {
            bool existingFile = false;
            byte[] bytesFile = null;
            string pathFile = GeneratePathGrafic(guidFile);

            if (File.Exists(pathFile))
            {
                existingFile = true;
                bytesFile = File.ReadAllBytes(pathFile);
            }

            return (existingFile, bytesFile);
        }

        private void ValidateExistingGraficFolder()
        {
            if (!Directory.Exists(_graficsPath))
                Directory.CreateDirectory(_graficsPath);
        }

        private string GeneratePathGrafic(string guid)
        {
            return _graficsPath += $"{guid}.png";
        }
    }
}
