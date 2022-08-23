using ITSignerWebComponent.Core.Entities.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITSignerWebComponent.Infraestructure.Data.Configurations
{
    public class ParametrosComponenteConfiguration : IEntityTypeConfiguration<ParametrosComponente>
    {
        public void Configure(EntityTypeBuilder<ParametrosComponente> builder)
        {
            builder.HasKey(e => e.Id)
                    .HasName("parametros_componente_pkey");

            builder.ToTable("parametros_componente");

            builder.HasComment("Tabla de parametros componente de firma");

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("idparametro");

            builder.Property(e => e.Activo).HasColumnName("activo");

            builder.Property(e => e.FechaCrea).HasColumnName("fecha_crea");

            builder.Property(e => e.Parametro)
                .IsRequired()
                .HasColumnName("parametro");
        }
    }
}
