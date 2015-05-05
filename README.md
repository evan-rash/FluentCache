# FluentCache - A Fluent Library for Caching in C#

FluentCache is a simple, fluent library to help you write clean, legible caching code by reducing boilerplate.

Here's an example of some typical caching code:

```csharp
//I want to retrieve a value from my cache, and if it's not there load it from the repository 
Repository repository = new Repository();
int parameter = 5;
string region = "FluentCacheExamples";
string cacheKey = "Samples.DoSomeHardParameterizedWork." + parameter;

ICachedValue<double> cachedValue = cache.Get<double>(cacheKey, region);
if (cachedValue == null)
{
    double val = repository.DoSomeHardParameterizedWork(parameter);
    cachedValue = cache.Set<double>(cacheKey, region, val, new CachePolicy());
}
double result = cachedValue.Value;
```

This code is full of boilerplate, magic strings, and is hard to read. The *intent* of the code is overwhelmed by the mechanics of how to cache the value. I hope the method names and parameters don't change, otherwise I have to remember to update the cache key!

Here's the equivalent code using FluentCache

```csharp
double ezResult = cache.Method(r => r.DoSomeHardParameterizedWork(parameter))
                       .GetValue();
```

FluentCache automatically analyzes the expression tree and generates a unique cache key from the type, method, and any parameters and creates a caching policy that you can store or immediately execute. You can also specify various caching policies such as time-to-live, error handling, and cache invalidation.

# Examples:

```csharp
//You can specify cache expiration policies
double ttlValue = cache.Method(r => r.DoSomeHardWork())
                       .ExpireAfter(TimeSpan.FromMinutes(5))
                       .GetValue();

//It supports asyn/await natively
double asyncValue = await cache.Method(r => r.DoSomeHardWorkAsync())
                               .GetValueAsync();

//You can specify validation strategies to customize when caches should be updated
double onlyCachePositiveValues = cache.Method(r => r.DoSomeHardWork())
                                      .InvalidateIf(cachedVal => cachedVal.Value <= 0d)
                                      .GetValue();

//You can clear existing cached values
cache.Method(r => r.DoSomeHardParameterizedWork(parameter))
     .ClearValue();
```

# Getting Started

To get started, you need to choose a FluentCache implementation. FluentCache supports System.Runtime.Caching.MemoryCache out of the box. Other cache types can implement the FluentCache.ICache interface.

In this example, we will use the FluentMemoryCache, which is a wrapper around the System.Runtime.Caching.MemoryCache  

```csharp
ICache myCache = FluentCache.RuntimeCaching.FluentMemoryCache.Default();

//Now that we have our cache, we're going to create a wrapper around our Repository
//The wrapper will allow us to cache the results of various Repository methods
Repository repo = new Repository();
ICache<Repository> myRepositoryCache = myCache.WithSource(repo);

//Now that we have a wrapper, we can create and execute a CacheStrategy
string resource = myRepositoryCache.Method(r => r.RetrieveResource())
                                   .ExpireAfter(TimeSpan.FromMinutes(30))
                                   .GetValue();
```
