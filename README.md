# FluentCache ![](https://raw.githubusercontent.com/cordialgerm/FluentCache/master/FluentCache_24.png)

## A Library for Fluent Caching in C&#35;

FluentCache is a simple, fluent library to help you write clean, legible caching code by reducing boilerplate.

```csharp
double ezResult = cache.Method(r => r.DoSomeHardParameterizedWork(parameter))
                       .GetValue();
```

|Package|Status|
|---|---|
|[FluentCache](https://www.nuget.org/packages/FluentCache) |![FluentCache](https://buildstats.info/nuget/FluentCache)|
|[FluentCache.RuntimeCaching](https://www.nuget.org/packages/FluentCache.RuntimeCaching) |![](https://buildstats.info/nuget/FluentCache.RuntimeCaching)|
|[FluentCache.Microsoft.Extensions.Caching.Abstractions](https://www.nuget.org/packages/FluentCache.Microsoft.Extensions.Caching.Abstractions) |![](https://buildstats.info/nuget/FluentCache.Microsoft.Extensions.Caching.Abstractions)|
|[FluentCache.Microsoft.Extensions.Caching.Memory](https://www.nuget.org/packages/FluentCache.Microsoft.Extensions.Caching.Memory) |![](https://buildstats.info/nuget/FluentCache.Microsoft.Extensions.Caching.Memory)|
|[FluentCache.Microsoft.Extensions.Caching.Redis](https://www.nuget.org/packages/FluentCache.Microsoft.Extensions.Caching.Redis) |![](https://buildstats.info/nuget/FluentCache.Microsoft.Extensions.Caching.Redis)|

**Features**:

* **Fluent API**: clean, simple API to encapsulate caching logic
* **Automatic Cache Key Generation**: refactor at will and never worry about magic strings. FluentCache automatically analyzes the expression tree and generates caching keys based on the type, method, and parameters
* **Caching policies**: specify caching policies like expiration, validation, and error handling
* **Cache Implementations**: FluentCache supports common caching implementations and has a simple ICache interface to support other providers

Here's an example of some typical caching code that can be replaced by FluentCache:

```csharp
//retrieve a value from the cache. If it's not there, load it from the repository 
var repository = new Repository();
int parameter = 5;
string region = "FluentCacheExamples";
string cacheKey = "Samples.DoSomeHardParameterizedWork." + parameter;

CachedValue<double> cachedValue = cache.Get<double>(cacheKey, region);
if (cachedValue == null)
{
    double val = repository.DoSomeHardParameterizedWork(parameter);
    cachedValue = cache.Set<double>(cacheKey, region, val, new CacheExpiration());
}
double result = cachedValue.Value;
```
There are several issues with this code:

* **boilerplate**: duplicating this code is tedious
* **magic strings**: the cache key is based on magic strings that won't automatically refactor as methods and parameters change
* **hard to read**: the intent is overwhelmed by the mechanics

## Examples:


### Cache Expiration Policies
```csharp
double ttlValue = cache.Method(r => r.DoSomeHardWork())
                       .ExpireAfter(TimeSpan.FromMinutes(5))
                       .GetValue();
```

### Async/Await Retrieval
```csharp
double asyncValue = await cache.Method(r => r.DoSomeHardWorkAsync())
                               .GetValueAsync();
```

### Validation Strategies
```csharp
double onlyCachePositiveValues = cache.Method(r => r.DoSomeHardWork())
                                      .InvalidateIf(cachedVal => cachedVal.Value <= 0d)
                                      .GetValue();
```

### Clearing Values
```csharp
cache.Method(r => r.DoSomeHardParameterizedWork(parameter))
     .ClearValue();
```


## Getting Started

### Hello World

To get started, we will use the `FluentDictionaryCache` to illustrate the various Fluent extension methods provided by the API

```csharp
//use the simplest cache, which wraps a dictionary
//other cache implementations are provided in additional nuget packages
ICache myCache = new FluentCache.Simple.FluentDictionaryCache();

//create a wrapper around our Repository
//wrapper will allow us to cache the results of various Repository methods
Repository repo = new Repository();
Cache<Repository> myRepositoryCache = myCache.WithSource(repo);

//create and execute a CacheStrategy using Fluent Extension methods
string resource = myRepositoryCache.Method(r => r.RetrieveResource())
                                   .ExpireAfter(TimeSpan.FromMinutes(30))
                                   .GetValue();
```

### Implementations

| Cache Implementation | FluentCache Type | NuGet Package |
|---|---|---|
|`ConcurrentDictionary` | `FluentDictionaryCache` | `FluentCache` |
|`System.Runtime.Caching.MemoryCache`|`FluentMemoryCache`|`FluentCache.RuntimeCaching`|
|`Microsoft.Extensions.Caching.Memory.MemoryCache`| `FluentMemoryCache` | `FluentCache.Microsoft.Extensions.Caching.Memory`|
|`Microsoft.Extensions.Caching.Redis.RedisCache` | `FluentRedisCache` | `FluentCache.Microsoft.Extensions.Caching.Redis`|
|`Microsoft.Extensions.Caching.Memory.IMemoryCache`| `FluentIMemoryCache` | `FluentCache.Microsoft.Extensions.Caching.Abstractions`|
|`Microsoft.Extensions.Caching.Distributed.IDistributedCache` | `FluentIDistributedCache` | `FluentCache.Microsoft.Extensions.Caching.Abstractions`|

Other caches can implement the `FluentCache.ICache` interface

## New in v4.0.2

1. `.ExpireAfter()` now supports a callback for determining sliding expiration based on cached value
1. `FluentCache` now only requires .NET Standard 1.1
1. Assemblies are signed

## New in v4

1. Support for .NET Standard 2.0
1. New implementations for `Microsoft.Extensions.Caching`
   1. `Microsoft.Extensions.Caching.Memory.IMemoryCache` added to `FluentCache.Microsoft.Extensions.Caching.Abstractions` nuget package
   1. `Microsoft.Extensions.Caching.Memory.MemoryCache` added to `FluentCache.Microsoft.Extensions.Caching.Memory` nuget package
   1. `Microsoft.Extensions.Caching.Distributed.IDistributedCache` added to `FluentCache.Microsoft.Extensions.Caching.Abstractions` nuget package
   1. `Microsoft.Extensions.Caching.Redis.RedisCache` added to `FluentCache.Microsoft.Extensions.Caching.Redis` nuget package
1. Support for caching methods on generic repositories

The previous `FluentCache.Redis` package is deprecated in favor of `FluentCache.Microsoft.Extensions.Caching.Redis`


