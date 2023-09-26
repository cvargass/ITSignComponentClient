using Microsoft.Extensions.Configuration;
using StoreFiles.Core.DTOs.PostFile;
using StoreFiles.Core.DTOs.PostFileSigned;
using StoreFiles.Core.Exceptions;
using StoreFiles.Core.QueryFilters;
using StoreFiles.Core.Services.StoreFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StoreFiles.API.Services.StoreFiles
{
    public class StoreFileService : IStoreFileService
    {
        private string _pendingFilesPath = "./PendingFiles/";
        private string _signedFilePath = "./SignedFiles/";
        private string _signedFileAPIPath = "./SignedFiles/API/";
        private readonly IConfiguration _configuration;
        private readonly double _allowedFileSizeMBComponent;
        private readonly double _megaByte = 1e+6;

        public StoreFileService(IConfiguration configuration)
        {
            _configuration = configuration;
            _allowedFileSizeMBComponent =  Convert.ToInt32(configuration["AllowedFileSizeMBComponent"]) * _megaByte;
        }
        public string StoreFile(PostFileDto postFileDto)
        {
            try
            {
                if (postFileDto.FilePdfBase64.Length <= _allowedFileSizeMBComponent)
                {
                    string fileName = GenerateFileName(postFileDto.IdUser, postFileDto.IdApp);
                    ValidateExistingPendingFolder();

                    byte[] file = Convert.FromBase64String(postFileDto.FilePdfBase64);
                    string filePath = GeneratePathPendingFile(fileName);

                    using var writer = new BinaryWriter(File.OpenWrite(filePath));
                    writer.Write(file);

                    return fileName;
                }
                else
                {
                    var mbAllowed = (_allowedFileSizeMBComponent / _megaByte).ToString();
                    throw new InternalErrorException("El tamaño del archivo máximo permitido es " + mbAllowed + "MB");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The input is not a valid Base-64"))
                    throw new InternalErrorException("El string-base64 del archivo no es válido");
                else
                    throw new InternalErrorException(ex.Message);
            }
        }

        public string StoreFileCades(PostFileDto postFileDto)
        {
            try
            {
                if (postFileDto.FilePdfBase64.Length <= _allowedFileSizeMBComponent)
                {
                    string fileName = GenerateFileName(postFileDto.IdUser, postFileDto.IdApp);
                    ValidateExistingPendingFolder();

                    byte[] file = Convert.FromBase64String(postFileDto.FilePdfBase64);
                    string filePath = GeneratePathPendingFile(fileName, true);

                    using var writer = new BinaryWriter(File.OpenWrite(filePath));
                    writer.Write(file);

                    return fileName;
                }
                else
                {
                    var mbAllowed = (_allowedFileSizeMBComponent * _megaByte).ToString();
                    throw new InternalErrorException("El tamaño del archivo máximo permitido es " + mbAllowed + "MB");
                }
                
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The input is not a valid Base-64"))
                    throw new InternalErrorException("El string-base64 del archivo no es válido");
                else
                    throw new InternalErrorException(ex.Message);
            }
        }

        private string GenerateFileName(int idUser, int idApp)
        {
            //string placeholderName = "[DATE]-[GUID]-[ID_USER]-[ID-APP]";
            string placeholderName = "[DATE]-[GUID]-[ID-APP]-[ID_USER]";
            string fileName = string.Empty;
            string dateFile = DateTime.Now.ToString("ddMMMMyyyy").ToUpper();
            string guidFile = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            placeholderName = placeholderName.Replace("[DATE]", dateFile);
            placeholderName = placeholderName.Replace("[GUID]", guidFile);
            placeholderName = placeholderName.Replace("[ID_USER]", idUser.ToString());
            placeholderName = placeholderName.Replace("[ID-APP]", idApp.ToString());

            fileName = placeholderName;

            return fileName;
        }

        public string[] GetPendingFiles(PendingFileQueryFilter pendingFileQueryFilter)
        {
            try
            {
                //var placeholderSearch = "-[ID-USER]-[ID-APP]";
                var placeholderSearch = "-[ID-APP]-[ID-USER]";
                List<string> files = new List<string>();

                placeholderSearch = placeholderSearch.Replace("[ID-USER]", pendingFileQueryFilter.IdUser.ToString());
                placeholderSearch = placeholderSearch.Replace("[ID-APP]", pendingFileQueryFilter.IdApp.ToString());

                //Elimina los archivos, manteniendo los archivos de los ultimos {n} dias según configuracion appsettings
                CleanFolderSignedFiles();
                ValidateExistingPendingFolder();

                string[] filesTemp = Directory.GetFiles(_pendingFilesPath, "*.*").Where(s => s.EndsWith(".pdf")).ToArray();
                    
                //Selecciona los archivos por IdUser y IdApp
                filesTemp = filesTemp.Where(x => x.Contains(placeholderSearch)).ToArray();

                for (int i = 0; i < filesTemp.Length; i++)
                {
                    files.Add(filesTemp[i].Replace(filesTemp[i].Substring(0, _pendingFilesPath.Length), ""));
                }

                return files.ToArray();
            }
            catch (Exception ex)
            {
                throw new InternalErrorException(ex.Message);
            }
        }

        public string[] GetCadesPendingFiles(PendingFileQueryFilter pendingFileQueryFilter)
        {
            try
            {
                //var placeholderSearch = "-[ID-USER]-[ID-APP]";
                var placeholderSearch = "-[ID-APP]-[ID-USER]";
                List<string> files = new List<string>();

                placeholderSearch = placeholderSearch.Replace("[ID-USER]", pendingFileQueryFilter.IdUser.ToString());
                placeholderSearch = placeholderSearch.Replace("[ID-APP]", pendingFileQueryFilter.IdApp.ToString());

                //Elimina los archivos, manteniendo los archivos de los ultimos {n} dias según configuracion appsettings
                CleanFolderSignedFiles();
                ValidateExistingPendingFolder();

                string[] filesTemp = Directory.GetFiles(_pendingFilesPath, "*.*").Where(s => s.EndsWith(".txt")).ToArray();

                //Selecciona los archivos por IdUser y IdApp
                filesTemp = filesTemp.Where(x => x.Contains(placeholderSearch)).ToArray();

                for (int i = 0; i < filesTemp.Length; i++)
                {
                    files.Add(filesTemp[i].Replace(filesTemp[i].Substring(0, _pendingFilesPath.Length), ""));
                }

                return files.ToArray();
            }
            catch (Exception ex)
            {
                throw new InternalErrorException(ex.Message);
            }
        }

        public (bool flag, byte[] bytesFile) GetFile(string guidFile, bool isSigned)
        {
            bool existingFile = false;
            byte[] bytesFile = null;
            string pathFile = isSigned ? GeneratePathSignedFile(guidFile) : GeneratePathPendingFile(guidFile);

            if (File.Exists(pathFile))
            {
                existingFile = true;
                bytesFile = File.ReadAllBytes(pathFile);
            }

            return (existingFile, bytesFile);
        }

        public (bool flag, byte[] bytesFile) GetCadesFile(string guidFile, bool isSigned)
        {
            bool existingFile = false;
            byte[] bytesFile = null;
            string pathFile = isSigned ? GeneratePathSignedFile(guidFile, true) : GeneratePathPendingFile(guidFile, true);

            if (File.Exists(pathFile))
            {
                existingFile = true;
                bytesFile = File.ReadAllBytes(pathFile);
            }

            return (existingFile, bytesFile);
        }

        public void StoreFileSigned(PostFileSignedDto postFileSignedDto)
        {
            try
            {
                ValidateExistingSignedFolder();

                byte[] file = Convert.FromBase64String(postFileSignedDto.PdfSignedBase64);
                string filePendingPath = GeneratePathPendingFile(postFileSignedDto.PdfGuid);
                string fileSignedPath = GeneratePathSignedFile(postFileSignedDto.PdfGuid);

                File.Delete(filePendingPath);

                using var writer = new BinaryWriter(File.OpenWrite(fileSignedPath));
                writer.Write(file);
            }
            catch (Exception ex)
            {
                throw new InternalErrorException(ex.Message);
            }
        }

        public void StoreFileSignedAPI(byte[] pdfSignedBase64)
        {
            try
            {
                ValidateExistingSignedFolderForAPI();

                string fileSignedPath = GeneratePathSignedFileForAPI();

                using var writer = new BinaryWriter(File.OpenWrite(fileSignedPath));
                writer.Write(pdfSignedBase64);
            }
            catch (Exception ex)
            {
                throw new InternalErrorException(ex.Message);
            }
        }

        public void StoreCadesFileSigned(PostFileSignedDto postFileSignedDto)
        {
            try
            {
                ValidateExistingSignedFolder();

                byte[] file = Convert.FromBase64String(postFileSignedDto.PdfSignedBase64);
                string filePendingPath = GeneratePathPendingFile(postFileSignedDto.PdfGuid, true);
                string fileSignedPath = GeneratePathSignedFile(postFileSignedDto.PdfGuid, true);

                File.Delete(filePendingPath);

                using var writer = new BinaryWriter(File.OpenWrite(fileSignedPath));
                writer.Write(file);
            }
            catch (Exception ex)
            {
                throw new InternalErrorException(ex.Message);
            }
        }

        private string GeneratePathPendingFile(string guidFile, bool isCadesFile = false)
        {
            if (isCadesFile)
                return _pendingFilesPath += $"{guidFile}.txt";
            else
                return _pendingFilesPath += $"{guidFile}.pdf";
        }

        private string GeneratePathSignedFile(string guidFile, bool isCadesFile = false)
        {
            if (isCadesFile)
                return _signedFilePath += $"{guidFile}.p7z";
            else
                return _signedFilePath += $"{guidFile}.pdf";
        }

        private string GeneratePathSignedFileForAPI()
        {
            string guidFile = Guid.NewGuid().ToString().ToUpper();
            return _signedFileAPIPath += $"{guidFile}.pdf";
        }

        private void ValidateExistingPendingFolder()
        {
            if (!Directory.Exists(_pendingFilesPath))
                Directory.CreateDirectory(_pendingFilesPath);
        }

        private void ValidateExistingSignedFolder()
        {
            if (!Directory.Exists(_signedFilePath))
                Directory.CreateDirectory(_signedFilePath);
        }

        private void ValidateExistingSignedFolderForAPI()
        {
            if (!Directory.Exists(_signedFileAPIPath))
                Directory.CreateDirectory(_signedFileAPIPath);
        }

        private void CleanFolderSignedFiles()
        {
            int keepFileDays = 5;

            try
            {
                var daysConfigured = _configuration["KeepFiles:last-days"];

                if (daysConfigured != null)
                    keepFileDays = Convert.ToInt32(daysConfigured);

                if (Directory.Exists(_signedFilePath))
                {
                    string[] files = Directory.GetFiles(_signedFilePath, "*.pdf");
                    foreach (string name in files)
                    {
                        DateTime dateCreationFile = File.GetCreationTime(name);

                        //Si no se encuentra dentro de los ultimos {n} dias configurados se elimina
                        if (!(dateCreationFile > DateTime.Now.AddDays(-keepFileDays)))
                            File.Delete(name);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InternalErrorException(ex.Message);
            }
        }
    }
}
