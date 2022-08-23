using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StoreFiles.API.Exceptions;
using System.Net;

namespace StoreFiles.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is InternalErrorException)
            {
                var exception = (InternalErrorException)context.Exception;
                var validation = new
                {
                    Status = 500,
                    Message = "Se presento un inconveniente durante la transaccion.",
                    Details = exception.Message
                };

                var jsonResponse = new { validation };

                context.Result = new JsonResult(validation) { StatusCode = (int)HttpStatusCode.InternalServerError };
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.ExceptionHandled = true;

            }
        }
    }
}
