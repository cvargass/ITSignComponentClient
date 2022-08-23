using FirmaITSign.Infraestructure.Data;
using ITSignerWebComponent.Core.Interfaces.Repositories;
using ITSignerWebComponent.Core.Interfaces.Repositories.Base;

namespace ITSignerWebComponent.Infraestructure.Repositories.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ItsignContext _dbContext;

        public UnitOfWork(ItsignContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ICustomersRepository ClienteComponenteRepository => new CustomersRepository(_dbContext);
        public IParametersRepository ParametrosComponenteRepository => new ParametersRepository(_dbContext);

        public void Dispose()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }
    }
}
