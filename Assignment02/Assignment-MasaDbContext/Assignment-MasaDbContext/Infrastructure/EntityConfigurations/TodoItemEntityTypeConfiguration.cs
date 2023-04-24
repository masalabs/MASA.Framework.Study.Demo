using Assignment_MasaDbContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assignment_MasaDbContext.Infrastructure.EntityConfigurations;

public class TodoItemEntityTypeConfiguration: IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("todo");
        builder.HasKey(t => t.Id);
    }
}