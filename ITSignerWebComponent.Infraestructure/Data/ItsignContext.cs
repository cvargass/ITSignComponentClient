using ITSignerWebComponent.Core.Entities.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FirmaITSign.Infraestructure.Data
{
    public partial class ItsignContext : DbContext
    {
        public ItsignContext()
        {
        }

        public ItsignContext(DbContextOptions<ItsignContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ClienteComponente> ClienteComponente { get; set; }
        public virtual DbSet<ParametrosComponente> ParametrosComponente { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.UTF-8");

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}