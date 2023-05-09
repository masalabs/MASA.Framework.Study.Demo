using Microsoft.EntityFrameworkCore;

namespace Assignment_MasaDbContext.Infrastructure;

// public class TodoDbContext : MasaDbContext
public class TodoDbContext : MasaDbContext<TodoDbContext>
{
    // public DbSet<TodoItem> Todo { get; set; }
    
    public TodoDbContext(MasaDbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    #region 模型映射

    protected override void OnModelCreatingExecuting(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoDbContext).Assembly);

        // modelBuilder.Entity<TodoItem>();
    }

    #endregion

    // protected override void OnConfiguring(MasaDbContextOptionsBuilder optionsBuilder)
    // {
    //     // optionsBuilder.UseSqlite("Data Source=todo.db;");
    //     base.OnConfiguring(optionsBuilder);
    // }
}