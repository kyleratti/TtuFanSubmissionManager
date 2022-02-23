using Microsoft.Extensions.Caching.Memory;

namespace AdminData.Services;

public class MetadataCache
{
	private readonly IMemoryCache _memoryCache;

	public MetadataCache(
		IMemoryCache memoryCache
	)
	{
		_memoryCache = memoryCache;
	}

	public async Task<T?> GetOrCreateAsync<T>(string key, Func<ICacheEntry, Task<T>> factory) =>
		await _memoryCache.GetOrCreateAsync(key, factory);
}