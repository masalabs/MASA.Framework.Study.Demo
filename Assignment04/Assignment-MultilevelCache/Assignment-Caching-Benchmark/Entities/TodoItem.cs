using Masa.BuildingBlocks.Data;

namespace Assignment_Caching_Benchmark.Entities;

public class TodoItem : ISoftDelete
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public string? Describe { get; set; }

    public bool Done { get; set; }

    public bool IsDeleted { get; private set; }
}