using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Infrastructure.EntitiesTypeConfiguration;

public class UserTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Email)
            .IsUnique();
        
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.Patronymic).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.Password).HasMaxLength(50);
        builder.Property(x => x.Phone).HasMaxLength(11);
        builder.Ignore(x => x.FullName);

        //builder.Property(x => x.RepairRequestsSubmitted).IsRequired(false);
        //builder.Property(x => x.RepairRequestsReceived).IsRequired(false);

        builder.Property(x => x.Role).HasMaxLength(50).HasConversion(
            x => x.ToString(),
            x => (UserRole)Enum.Parse(typeof(UserRole), x));

        //builder.HasMany(x => x.RepairRequestsReceived).WithOne(x => x.UserTo);
        //builder.HasMany(x => x.RepairRequestsSubmitted).WithOne(x => x.UserFrom);
        
        
    }
}