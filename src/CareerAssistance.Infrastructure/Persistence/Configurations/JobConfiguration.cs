using CareerAssistance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerAssistance.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(j => j.Company)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(j => j.Description)
            .HasMaxLength(4000);

        builder.Property(j => j.Url)
            .HasMaxLength(1024);

        builder.Property(j => j.Notes)
            .HasMaxLength(2000);

        builder.Property(j => j.SalaryRange)
            .HasMaxLength(128);

        builder.Property(j => j.Status)
            .IsRequired()
            .HasConversion<int>(); // enum як int в БД

        builder.Property(j => j.CreatedAt)
            .IsRequired();

        // Створюємо індекс для UserId, оскільки за ним буде йти постійна фільтрація
        builder.HasIndex(j => j.UserId);

        builder.HasOne(j => j.User)
            .WithMany()
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}