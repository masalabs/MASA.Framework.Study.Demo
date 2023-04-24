using Assignment_MasaDbContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assignment_MasaDbContext.Infrastructure;

// public class TodoDbContext : MasaDbContext
public class TodoDbContext : MasaDbContext<TodoDbContext>
{
    public TodoDbContext(MasaDbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    // public DbSet<TodoItem> Todo { get; set; }

    protected override void OnModelCreatingExecuting(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoDbContext).Assembly);

        // modelBuilder.Entity<TodoItem>();
    }
}