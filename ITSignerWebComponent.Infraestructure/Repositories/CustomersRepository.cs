using FirmaITSign.Infraestructure.Data;
using ITSignerWebComponent.Core.Entities.Domain;
using ITSignerWebComponent.Core.Interfaces.Repositories;
using ITSignerWebComponent.Infraestructure.Repositories.Base;
using System.Collections.Generic;
using System.Linq;

namespace ITSignerWebComponent.Infraestructure.Repositories
{
    public class CustomersRepository : BaseRepository<ClienteComponente>, ICustomersRepository
    {
        public CustomersRepository(ItsignContext dbContext) : base(dbContext)
        {
        }

        public ClienteComponente GetByPendingLicense(string license)
        {
            return _entities.Where(x => x.Licencia == license && x.FechaActivacion == null)
                .FirstOrDefault();
        }

        public IEnumerable<ClienteComponente> GetAllDescending()
        {
            return _entities.OrderByDescending(x => x.Id).ToList();
        }
    }
}
