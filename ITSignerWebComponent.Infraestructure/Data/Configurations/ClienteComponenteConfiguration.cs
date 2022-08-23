using ITSignerWebComponent.Core.Entities.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITSignerWebComponent.Infraestructure.Data.Configurations
{
    public class ClienteComponenteConfiguration : IEntityTypeConfiguration<ClienteComponente>
    {
        public void Configure(EntityTypeBuilder<ClienteComponente> builder)
        {
            builder.HasKey(e => e.Id)
                    .HasName("cliente_componente_pkey");

            builder.ToTable("cliente_componente");

            builder.HasComment("Tabla de cleintes del componente de firma");

            builder.Property(e => e.Id)
                .HasColumnName("idcliente")
                .HasDefaultValueSql("nextval('sec_cliente_componente'::regclass)");

            builder.Property(e => e.Activo).HasColumnName("activo");

            builder.Property(e => e.FechaActivacion).HasColumnName("fecha_activacion");

            builder.Property(e => e.FechaCreacion).HasColumnName("fecha_crea");

            builder.Property(e => e.Licencia)
                .IsRequired()
                .HasColumnName("licencia");

            builder.Property(e => e.Email)
                .IsRequired()
                .HasColumnName("email");

            builder.Property(e => e.Nombre)
                .IsRequired()
                .HasColumnName("nombre");
        }
    }
}
