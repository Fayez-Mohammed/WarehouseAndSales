using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AccountantUserProfileConfiguration : IEntityTypeConfiguration<AccountantUserProfile>
{
    public void Configure(EntityTypeBuilder<AccountantUserProfile> builder)
    {
        builder.ToTable("AccountantUserProfiles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
               .IsRequired();

        // One-to-One with AspNetUsers
        builder.HasOne(a => a.User)
               .WithOne()
               .HasForeignKey<AccountantUserProfile>(a => a.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}




