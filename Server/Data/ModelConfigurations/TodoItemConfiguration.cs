using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Data.ModelConfigurations
{
    public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
    {
        public void Configure(EntityTypeBuilder<TodoItem> builder)
        {
            builder.ToTable("TodoItem");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(t => t.Title)
                .HasColumnName("Title")
                .IsUnicode()
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .IsUnicode()
                .IsRequired();

            builder.Property(t => t.DueDate)
                .HasColumnName("DueDate")
                .IsRequired();

            builder.Property(t => t.IsCompleted)
                .HasColumnName("Complete")
                .HasDefaultValue(false)
                .IsRequired();
        }
    }
}