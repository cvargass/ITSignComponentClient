using System;

namespace ITSignerWebComponent.SignApp.Responses.APIITSignResponse
{
    public class GetCustomerResponse
    {
        public bool Flag { get; set; }
        public CustomerComponentResponse CustomerComponent { get; set; }

    }

    public partial class CustomerComponentResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Licencia { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
