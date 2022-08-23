using AutoMapper;
using ITSignerWebComponent.Core.DTOs.Customers;
using ITSignerWebComponent.Core.Entities.Domain;

namespace ITSignerWebComponent.Infraestructure.Mappings
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<ClienteComponente, CustomerForCreationDto>().ReverseMap();
            CreateMap<ClienteComponente, CustomerForUpdateDto>().ReverseMap();
            CreateMap<ClienteComponente, CustomerDto>().ReverseMap();
        }
    }
}
