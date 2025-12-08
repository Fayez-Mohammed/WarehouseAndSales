using Base.DAL.Models.SystemModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(e => e.Description)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(e => e.DateOfCreation)
               .HasDefaultValueSql("GETUTCDATE()")
               .IsRequired();

        // Relation to ApplicationUser
        builder.HasOne(e => e.AccountantUser)
               .WithMany()  // this user may create many expenses
               .HasForeignKey(e => e.AccountantUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
