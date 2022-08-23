using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ITSignerWebComponent.Core.Entities.Base_Entity;
using FirmaITSign.Infraestructure.Data;
using ITSignerWebComponent.Core.Interfaces.Repositories.Base;

namespace ITSignerWebComponent.Infraestructure.Repositories.Base
{
    public class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly ItsignContext _dbContext;
        internal readonly DbSet<T> _entities;

        public BaseRepository(ItsignContext dbContext)
        {
            _dbContext = dbContext;
            _entities = dbContext.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return _entities.ToList();
        }

        public T GetById(int id)
        {
            return _entities.Find(id);
        }

        public void Add(T entity)
        {
            _entities.Add(entity);
        }

        public void AddRange(T[] entities)
        {
            _entities.AddRange(entities);
        }

        public void Update(T entity)
        {
            _entities.Update(entity);
        }

        public void Delete(int id)
        {
            T entity = GetById(id);
            if (entity != null)
            {
                entity.Activo = false;
                _dbContext.Update(entity);
            }
        }

        public bool Exists(int id)
        {
            bool flag = false;

            flag = _entities.Any(x => x.Id == id);

            return flag;
        }
    }
}
