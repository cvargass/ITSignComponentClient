using System;

namespace ITSignerWebComponent.Core.Interfaces.Services
{
    public interface ILoggerService
    {
        void LogAdvertencia(string message);
        void LogAdvertencia(string message, Exception ex);
        void LogError(string message);
        void LogError(string message, Exception ex);
        void LogInformation(string message);
        void LogInformation(string message, Exception ex);
    }
}