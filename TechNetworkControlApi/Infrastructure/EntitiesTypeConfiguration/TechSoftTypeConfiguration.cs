using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNetworkControlApi.Infrastructure.Entities;

namespace TechNetworkControlApi.Infrastructure.EntitiesTypeConfiguration;

public class TechSoftTypeConfiguration : IEntityTypeConfiguration<TechSoft>
{
    public void Configure(EntityTypeBuilder<TechSoft> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Version).HasMaxLength(25);
    }
}