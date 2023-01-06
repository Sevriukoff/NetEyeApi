using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Infrastructure.EntitiesTypeConfiguration;

public class RepairRequestTypeConfiguration : IEntityTypeConfiguration<RepairRequest>
{
    public void Configure(EntityTypeBuilder<RepairRequest> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property<int>("UserToId");
        
        builder.HasOne(x => x.UserTo)
            .WithMany(x => x.RepairRequestsReceived)
            .HasForeignKey("UserToId")
            .IsRequired(false);

        builder.Property<int>("UserFromId");
        
        builder.HasOne(x => x.UserFrom)
            .WithMany(x => x.RepairRequestsSubmitted)
            .HasForeignKey("UserFromId");

        builder.HasOne(x => x.TechEquipment);
        builder.Navigation(x => x.TechEquipment).IsRequired();

        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property(x => x.CreatedDate)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP()");

        builder.Property(x => x.Status)
            .HasConversion(x => x.ToString(),
                x => (RepairRequestStatus) Enum.Parse(typeof(RepairRequestStatus), x));
    }
}