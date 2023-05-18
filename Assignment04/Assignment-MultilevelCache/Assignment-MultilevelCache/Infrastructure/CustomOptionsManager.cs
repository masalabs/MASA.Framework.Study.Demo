namespace Assignment_MasaDbContext.Infrastructure;

public class CustomOptionsManager
{
    private readonly CustomOptionsCache _customOptionsCache;
    private readonly IConfiguration _configuration;

    public CustomOptionsManager(CustomOptionsCache customOptionsCache, IConfiguration configuration)
    {
        _customOptionsCache = customOptionsCache;
        _configuration = configuration;
    }

    public string GetValue(string name)
    {
        return _customOptionsCache.GetOrAdd(name, (key) =>
        {
            return _configuration.GetSection(name).Value;
        });
    }
}
