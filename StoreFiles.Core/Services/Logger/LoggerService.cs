﻿using log4net;
using log4net.Config;
using log4net.Core;
using Microsoft.Extensions.Configuration;
using StoreFiles.Core.Exceptions;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace StoreFiles.Core.Services.Logger
{
    public class LoggerService : ILoggerService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(LoggerManager));

        public LoggerService(IConfiguration configurations)
        {
            try
            {
                XmlDocument log4netConfig = new XmlDocument();

                using (var fs = File.OpenRead(@configurations["Logger:UrlConfigFile"]))
                {
                    log4netConfig.Load(fs);

                    var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);

                    GlobalContext.Properties["FilePath"] = path.Substring(6, path.Length - 6);
                    XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error", ex);
            }
        }

        public void LogInformation(string message)
        {
            _logger.Info(message);
        }
        public void LogInformation(string message, Exception ex)
        {
            _logger.Info(message, ex);
        }
        public void LogAdvertencia(string message)
        {
            _logger.Warn(message);
        }
        public void LogAdvertencia(string message, Exception ex)
        {
            _logger.Warn(message, ex);
        }
        public void LogError(string message)
        {
            _logger.Error(message);
        }
        public void LogError(string message, Exception ex)
        {
            _logger.Error(message, ex);
        }

        public void LogExceptionError(string message, Exception ex)
        {
            _logger.Error(message, ex);
            throw new InternalErrorException(ex.Message);
        }
    }
}
