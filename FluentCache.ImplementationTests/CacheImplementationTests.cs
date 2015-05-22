using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

//******************************************************************
//*** THIS FILE HAS BEEN AUTO-GENERATED. DO NOT EDIT IT MANUALLY ***
//******************************************************************

namespace FluentCache.Test
{
	
	[TestClass]
    public partial class DictionaryCacheTests
    {
		
		[TestMethod]
		public void DictionaryCache_ThisMethod_GetValue_Default()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for ThisMethod_GetValue_Default
			tester.ThisMethod_GetValue_Default();
		}
		
		[TestMethod]
		public void DictionaryCache_Method_Get()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get
			tester.Method_Get();
		}
		
		[TestMethod]
		public void DictionaryCache_Method_GetValue()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_GetValue
			tester.Method_GetValue();
		}
		
		[TestMethod]
		public void DictionaryCache_Method_Get_InitialVersion()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get_InitialVersion
			tester.Method_Get_InitialVersion();
		}
		
		[TestMethod]
		public void DictionaryCache_Method_Get_MultipleSameVersion()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get_MultipleSameVersion
			tester.Method_Get_MultipleSameVersion();
		}
		
		[TestMethod]
		public void DictionaryCache_Method_Get_Invalidation()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get_Invalidation
			tester.Method_Get_Invalidation();
		}
		
		[TestMethod]
		public async Task DictionaryCache_Method_ExpireAfter_VersionReset()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_ExpireAfter_VersionReset
			await tester.Method_ExpireAfter_VersionReset();
		}
		
		[TestMethod]
		public async Task DictionaryCache_Method_ExpireAfter()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_ExpireAfter
			await tester.Method_ExpireAfter();
		}
		
		[TestMethod]
		public void DictionaryCache_Method_Parameterized()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Parameterized
			tester.Method_Parameterized();
		}
		
		[TestMethod]
		public async Task DictionaryCache_Method_Async_GetValue_NotDefault()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Async_GetValue_NotDefault
			await tester.Method_Async_GetValue_NotDefault();
		}
		
		[TestMethod]
		public async Task DictionaryCache_Method_Async_Parameters()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Async_Parameters
			await tester.Method_Async_Parameters();
		}
			}
	
	[TestClass]
    public partial class MemoryCacheTests
    {
		
		[TestMethod]
		public void MemoryCache_ThisMethod_GetValue_Default()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for ThisMethod_GetValue_Default
			tester.ThisMethod_GetValue_Default();
		}
		
		[TestMethod]
		public void MemoryCache_Method_Get()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get
			tester.Method_Get();
		}
		
		[TestMethod]
		public void MemoryCache_Method_GetValue()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_GetValue
			tester.Method_GetValue();
		}
		
		[TestMethod]
		public void MemoryCache_Method_Get_InitialVersion()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get_InitialVersion
			tester.Method_Get_InitialVersion();
		}
		
		[TestMethod]
		public void MemoryCache_Method_Get_MultipleSameVersion()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get_MultipleSameVersion
			tester.Method_Get_MultipleSameVersion();
		}
		
		[TestMethod]
		public void MemoryCache_Method_Get_Invalidation()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get_Invalidation
			tester.Method_Get_Invalidation();
		}
		
		[TestMethod]
		public async Task MemoryCache_Method_ExpireAfter_VersionReset()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_ExpireAfter_VersionReset
			await tester.Method_ExpireAfter_VersionReset();
		}
		
		[TestMethod]
		public async Task MemoryCache_Method_ExpireAfter()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_ExpireAfter
			await tester.Method_ExpireAfter();
		}
		
		[TestMethod]
		public void MemoryCache_Method_Parameterized()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Parameterized
			tester.Method_Parameterized();
		}
		
		[TestMethod]
		public async Task MemoryCache_Method_Async_GetValue_NotDefault()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Async_GetValue_NotDefault
			await tester.Method_Async_GetValue_NotDefault();
		}
		
		[TestMethod]
		public async Task MemoryCache_Method_Async_Parameters()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Async_Parameters
			await tester.Method_Async_Parameters();
		}
			}
	
	[TestClass]
    public partial class RedisCacheTests
    {
		
		[TestMethod]
		public void RedisCache_ThisMethod_GetValue_Default()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for ThisMethod_GetValue_Default
			tester.ThisMethod_GetValue_Default();
		}
		
		[TestMethod]
		public void RedisCache_Method_Get()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get
			tester.Method_Get();
		}
		
		[TestMethod]
		public void RedisCache_Method_GetValue()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_GetValue
			tester.Method_GetValue();
		}
		
		[TestMethod]
		public void RedisCache_Method_Get_InitialVersion()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get_InitialVersion
			tester.Method_Get_InitialVersion();
		}
		
		[TestMethod]
		public void RedisCache_Method_Get_MultipleSameVersion()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get_MultipleSameVersion
			tester.Method_Get_MultipleSameVersion();
		}
		
		[TestMethod]
		public void RedisCache_Method_Get_Invalidation()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Get_Invalidation
			tester.Method_Get_Invalidation();
		}
		
		[TestMethod]
		public async Task RedisCache_Method_ExpireAfter_VersionReset()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_ExpireAfter_VersionReset
			await tester.Method_ExpireAfter_VersionReset();
		}
		
		[TestMethod]
		public async Task RedisCache_Method_ExpireAfter()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_ExpireAfter
			await tester.Method_ExpireAfter();
		}
		
		[TestMethod]
		public void RedisCache_Method_Parameterized()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Parameterized
			tester.Method_Parameterized();
		}
		
		[TestMethod]
		public async Task RedisCache_Method_Async_GetValue_NotDefault()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Async_GetValue_NotDefault
			await tester.Method_Async_GetValue_NotDefault();
		}
		
		[TestMethod]
		public async Task RedisCache_Method_Async_Parameters()
		{
			CacheTester tester = new CacheTester(CreateCache);
			
			//Unit test for Method_Async_Parameters
			await tester.Method_Async_Parameters();
		}
			}
	
	//Test Helpers & Initializers
	public partial class MemoryCacheTests 
	{
		private static Cache CreateCache()
		{
			var memoryCache = new System.Runtime.Caching.MemoryCache(Guid.NewGuid().ToString());
			return new FluentCache.RuntimeCaching.FluentMemoryCache(memoryCache);
		}
	}

	public partial class DictionaryCacheTests
	{
		private static Cache CreateCache()
		{
			return new FluentCache.Simple.FluentDictionaryCache();
		}
	}

	public partial class RedisCacheTests
	{
        private static StackExchange.Redis.ConnectionMultiplexer Redis;

        [ClassInitialize]
        public static void InitializeRedis(TestContext context)
        {
            var config = StackExchange.Redis.ConfigurationOptions.Parse("localhost");
            config.AllowAdmin = true;

            Redis = StackExchange.Redis.ConnectionMultiplexer.Connect(config);

        }

        [TestInitialize]
        public void TestInitialize()
        {
            var endpoint = Redis.GetEndPoints().Single();
            Redis.GetServer(endpoint).FlushAllDatabases();
        }

		private static Cache CreateCache()
		{
			return new FluentCache.Redis.FluentRedisCache(Redis);
		}

	}

}