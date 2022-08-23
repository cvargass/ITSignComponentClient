using CurrieTechnologies.Razor.SweetAlert2;
using System;

namespace ITSignerWebComponent.SignApp.Error
{
    public interface IErrorService
    {
        SweetAlertService _swal { get; set; }

        void ProcessError(Exception ex);
        void ProcessError(Exception exception, string customMessage);
    }
}