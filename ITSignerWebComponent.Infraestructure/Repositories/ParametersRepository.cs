using FirmaITSign.Infraestructure.Data;
using ITSignerWebComponent.Core.Entities.Domain;
using ITSignerWebComponent.Core.Interfaces.Repositories;
using ITSignerWebComponent.Infraestructure.Repositories.Base;
using System.Linq;

namespace ITSignerWebComponent.Infraestructure.Repositories
{
    public class ParametersRepository : BaseRepository<ParametrosComponente>, IParametersRepository
    {
        public ParametersRepository(ItsignContext dbContext) : base(dbContext)
        {
        }

        public ParametrosComponente GetByParameter(string parameter)
        {
            return _entities.Where(x => x.Parametro == parameter).FirstOrDefault();
        }
    }
}
