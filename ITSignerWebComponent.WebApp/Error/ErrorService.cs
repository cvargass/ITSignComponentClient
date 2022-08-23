using CurrieTechnologies.Razor.SweetAlert2;
using ITSignerWebComponent.Core.Interfaces.Services;
using System;

namespace ITSignerWebComponent.SignApp.Error
{
    public class ErrorService : IErrorService
    {
        public SweetAlertService _swal { get; set; }
        
        private readonly ILoggerService _logger;

        public ErrorService(SweetAlertService swal, ILoggerService logger)
        {
            _swal = swal;
            _logger = logger;
        }

        public async void ProcessError(Exception exception)
        {
            _logger.LogError(exception.Message, exception);
            await _swal.FireAsync("Error", "Ha ocurrido un problema interno en la aplicación", "error");
        }

        public async void ProcessError(Exception exception, string customMessage)
        {
            _logger.LogError(exception.Message, exception);
            await _swal.FireAsync("Error", customMessage, "error");
        }
    }
}
