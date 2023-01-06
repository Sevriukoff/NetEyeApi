using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNetworkControlApi.Infrastructure.Entities;

namespace TechNetworkControlApi.Infrastructure.EntitiesTypeConfiguration;

public class TechEquipmentTechSoftTypeConfiguration : IEntityTypeConfiguration<TechEquipmentTechSoft>
{
    public void Configure(EntityTypeBuilder<TechEquipmentTechSoft> builder)
    {
        builder.HasKey(x => new {x.TechEquipmentId, x.TechSoftId});
        
        builder.HasOne(x => x.TechEquipment)
            .WithMany(x => x.Softs)
            .HasForeignKey(x => x.TechEquipmentId);

        builder.HasOne(x => x.TechSoft)
            .WithMany(x => x.TechEquipments)
            .HasForeignKey(x => x.TechSoftId);
    }
}