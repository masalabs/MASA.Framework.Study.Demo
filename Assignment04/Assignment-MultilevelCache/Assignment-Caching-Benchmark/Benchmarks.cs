using Assignment_Caching_Benchmark.Entities;
using Assignment_Caching_Benchmark.Infrastructure;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Masa.BuildingBlocks.Caching;
using Masa.Contrib.Caching.Distributed.StackExchangeRedis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Assignment_Caching_Benchmark;

[MarkdownExporter, AsciiDocExporter, HtmlExporter]
[SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.Net60)]
public class Benchmarks
{
    [Params(10000)]
    public int IterationCount { get; set; }

    private int _id;
    private TodoDbContext _todoDbContext;
    private IMultilevelCacheClient _multilevelCacheClient;
    private IDistributedCacheClient _distributedCacheClient;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddMasaDbContext<TodoDbContext>(options =>
        {
            options.UseSqlServer("server=localhost;uid=sa;pwd=P@ssw0rd;database=test");
        });
        services.AddMultilevelCache(cacheBuilder => cacheBuilder.UseStackExchangeRedisCache(options =>
        {
            options.Servers = new List<RedisServerOptions>()
            {
                new("localhost", 6379)
            };
            options.Password = "";
        }));
        var serviceProvider = services.BuildServiceProvider();
        _multilevelCacheClient = serviceProvider.GetRequiredService<IMultilevelCacheClient>();
        _distributedCacheClient = serviceProvider.GetRequiredService<IDistributedCacheClient>();
        _todoDbContext = serviceProvider.GetRequiredService<TodoDbContext>();
        _todoDbContext.Database.EnsureCreated();
        if (_todoDbContext.Set<TodoItem>().Any())
            return;

        var todoItem = new TodoItem()
        {
            Title = "title",
            Describe = "describe",
            Done = false
        };
        _todoDbContext.Set<TodoItem>().Add(todoItem);
        _todoDbContext.SaveChanges();
        _id = todoItem.Id;
    }

    [Benchmark(Baseline = true)]
    public TodoItem? QueryByDb()
    {
        var todoItem = _todoDbContext.Set<TodoItem>().FirstOrDefault(t => t.Id == _id);
        return todoItem;
    }
    
    [Benchmark]
    public TodoItem? QueryByDistributedCacheClient()
    {
        var info = _distributedCacheClient.GetOrSet("1", () =>
        {
            var todoItem = _todoDbContext.Set<TodoItem>().FirstOrDefault(t => t.Id == _id);
            return new CacheEntry<TodoItem?>(todoItem);
        });
        return info;
    }

    [Benchmark]
    public TodoItem? QueryByMultilevelCacheClient()
    {
        var info = _multilevelCacheClient.GetOrSet("1", () =>
        {
            var todoItem = _todoDbContext.Set<TodoItem>().FirstOrDefault(t => t.Id == _id);
            return new CacheEntry<TodoItem?>(todoItem);
        });
        return info;
    }
}
