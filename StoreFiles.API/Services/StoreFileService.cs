﻿using Microsoft.Extensions.Configuration;
using StoreFiles.API.DTOs.PostFile;
using StoreFiles.API.DTOs.PostFileSigned;
using StoreFiles.API.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace StoreFiles.API.Services
{
    public class StoreFileService : IStoreFileService
    {
        private string _pendingFilesPath = "./PendingFiles/";
        private string _signedFilePath = "./SignedFiles/";
        private readonly IConfiguration _configuration;

        public StoreFileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string StoreFile(PostFileDto postFileDto)
        {
            try
            {
                string fileName = GenerateFileName();
                ValidateExistingPendingFolder();

                byte[] file = Convert.FromBase64String(postFileDto.FilePdfBase64);
                string filePath = GeneratePathPendingFile(fileName);

                using var writer = new BinaryWriter(File.OpenWrite(filePath));
                writer.Write(file);

                return fileName;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The input is not a valid Base-64"))
                    throw new InternalErrorException("El string-base64 del archivo no es válido");
                else
                    throw new InternalErrorException(ex.Message);
            }
        }

        private string GenerateFileName()
        {
            string fileName = string.Empty;

            string guidFile = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            string dateFile = DateTime.Now.ToString("ddMMMMyyyy-").ToUpper();

            fileName = dateFile + guidFile;

            return fileName;
        }

        public string[] GetPendingFiles()
        {
            try
            {
                //Elimina los archivos, manteniendo los archivos de los ultimos {n} dias según configuracion appsettings
                CleanFolderSignedFiles();
                ValidateExistingPendingFolder();

                string[] filesTemp = Directory.GetFiles(_pendingFilesPath, "*.pdf");
                List<String> files = new List<String>();

                for (int i = 0; i < filesTemp.Length; i++)
                {
                    files.Add(filesTemp[i].Replace(filesTemp[i].Substring(0, _pendingFilesPath.Length),""));
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

        private string GeneratePathPendingFile(string guidFile)
        {
            return _pendingFilesPath += $"{guidFile}.pdf";
        }

        private string GeneratePathSignedFile(string guidFile)
        {
            return _signedFilePath += $"{guidFile}.pdf";
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
