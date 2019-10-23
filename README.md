---
page_type: sample
languages:
- csharp
products:
- azure
- dotnet
- azure-redis-cache
extensions:
- services: Redis-Cache
- platforms: dotnet
urlFragment: "getting-started-on-managing-redis-cache-in-c"
description: "Azure Redis sample for managing Redis Cache."
---

# Get started managing Azure Redis Cache (C#)

Azure Redis sample for managing Redis Cache.

- Create a Redis Cache and print out hostname.
- Get access keys.
- Regenerate access keys.
- Create another 2 Redis Caches with Premium Sku.
- List all Redis Caches in a resource group – for each cache with Premium Sku:
   - set Redis patch schedule to Monday at 5 am.
   - update shard count.
   - enable non-SSL port.
   - modify max memory policy and reserved settings.
   - restart it.
- Clean up all resources.

## Running this sample

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

```bash
git clone https://github.com/Azure-Samples/redis-cache-dotnet-manage-cache.git
cd redis-cache-dotnet-manage-cache
dotnet build
bin\Debug\net452\ManageRedis.exe
```

## More information

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
