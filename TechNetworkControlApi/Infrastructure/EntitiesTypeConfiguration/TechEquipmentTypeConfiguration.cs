using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Infrastructure.EntitiesTypeConfiguration;

public class TechEquipmentTypeConfiguration : IEntityTypeConfiguration<TechEquipment>
{
    public void Configure(EntityTypeBuilder<TechEquipment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.IpAddress).HasMaxLength(20);

        builder.Property(x => x.Type)
            .HasConversion(x => x.ToString(),
                x => Enum.Parse<TechType>(x));
    }
}