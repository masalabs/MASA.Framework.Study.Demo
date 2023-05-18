using Microsoft.EntityFrameworkCore;

namespace Assignment_Caching_Benchmark.Infrastructure;

// public class TodoDbContext : MasaDbContext
public class TodoDbContext : MasaDbContext<TodoDbContext>
{
    public TodoDbContext(MasaDbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    #region model mapping

    protected override void OnModelCreatingExecuting(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoDbContext).Assembly);
    }

    #endregion
}