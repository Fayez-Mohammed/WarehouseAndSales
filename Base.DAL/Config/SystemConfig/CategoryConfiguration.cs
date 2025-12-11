using Base.DAL.Config.BaseConfig; // Ensure you import your Base Config namespace
using Base.DAL.Models.SystemModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Base.DAL.Config.SystemConfig
{
    public class CategoryConfiguration : BaseEntityConfigurations<Category>
    {
        public override void Configure(EntityTypeBuilder<Category> builder)
        {
            // 1. Base Configuration (ID, Audit, Soft Delete)
            base.Configure(builder);

            // 2. Properties
            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasMaxLength(500);

            // 3. Fix Audit Fields (Avoid Cycles)
            builder.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.UpdatedBy).WithMany().HasForeignKey(x => x.UpdatedById).OnDelete(DeleteBehavior.Restrict);
        }
    }
}