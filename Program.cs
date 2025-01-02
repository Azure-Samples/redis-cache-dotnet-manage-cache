// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information. 

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Redis;
using Azure.ResourceManager.Redis.Models;

namespace ManageRedis
{
    public class Program
    {
        /**
         * Azure Redis sample for managing Redis Cache:
         *  - Create a Redis Cache and print out hostname.
         *  - Get access keys.
         *  - Regenerate access keys.
         *  - Create another 2 Redis Caches with Premium Sku.
         *  - List all Redis Caches in a resource group – for each cache with Premium Sku:
         *     - set Redis patch schedule to Monday at 5 am.
         *     - update shard count.
         *     - enable non-SSL port.
         *     - modify max memory policy and reserved settings.
         *     - restart it.
         *  - Clean up all resources.
         */
        private static ResourceIdentifier? _resourceGroupId = null;
        public static async Task RunSample(ArmClient client)
        {
            try
            {
                // ============================================================
       
                // Get default subscription
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();

                // Create a resource group in the USCentral region
                var rgName = Utilities.CreateRandomName("RedisRG");
                Utilities.Log($"creating resource group with name:{rgName}");
                var rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.CentralUS));
                var resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;
                Utilities.Log("Created a resource group with name: " + resourceGroup.Data.Name);

                // Create a Redis cache
                Utilities.Log("Creating the 1st Cache...");
                var redisCacheName1 = Utilities.CreateRandomName("rc1");
                var redisCacheCollection = resourceGroup.GetAllRedis();
                var parameter = new RedisCreateOrUpdateContent(AzureLocation.CentralUS, new RedisSku(RedisSkuName.Basic, RedisSkuFamily.BasicOrStandard, 0));
                var task1 = redisCacheCollection.CreateOrUpdateAsync(WaitUntil.Completed, redisCacheName1, parameter);
              
                // ============================================================

                // Create another two Redis Caches
              
                // Create the 2nd Redis Caches
                Utilities.Log("Creating the 2nd Redis Caches...");
                var redisCacheName2 = Utilities.CreateRandomName("rc2");
                var parameter2 = new RedisCreateOrUpdateContent(AzureLocation.CentralUS, new RedisSku(RedisSkuName.Premium, RedisSkuFamily.Premium, 1))
                {
                    ShardCount = 3
                };
                var task2 = redisCacheCollection.CreateOrUpdateAsync(WaitUntil.Completed, redisCacheName2, parameter2);
              
                // Create the 3rd Redis cache
                Utilities.Log("Creating the 3rd Redis cache...");
                var redisCacheName3 = Utilities.CreateRandomName("rc3");
                var parameter3 = new RedisCreateOrUpdateContent(AzureLocation.CentralUS, new RedisSku(RedisSkuName.Premium, RedisSkuFamily.Premium, 2))
                {
                    ShardCount = 3
                };
                var task3 = redisCacheCollection.CreateOrUpdateAsync(WaitUntil.Completed, redisCacheName3, parameter3);
                await Task.WhenAll(task1, task2, task3);
                Utilities.Log($"Created all");

                // ============================================================

                // Get | regenerate Redis Cache access keys
                Utilities.Log("Getting Redis Cache access keys");
                var redisAccessKeys = task1.Result.Value.GetKeys();
                Utilities.Log("Got Redis Cache access keys");
                Utilities.Log("Regenerating secondary Redis Cache access key");
                var content = new RedisRegenerateKeyContent(RedisRegenerateKeyType.Secondary);
                _ = task1.Result.Value.RegenerateKey(content);
                Utilities.Log("Regenerated secondary Redis Cache access key");

                // ============================================================

                // List Redis Caches inside the resource group

                // Create a Patch Schedules
                Utilities.Log("Creating a Patch Schedules...");
                var scheduleCollection2 = task2.Result.Value.GetRedisPatchSchedules();
                var data2 = new RedisPatchScheduleData(new RedisPatchScheduleSetting[]
                {
                    new RedisPatchScheduleSetting(RedisDayOfWeek.Tuesday, 11)
                    {
                        MaintenanceWindow = TimeSpan.FromHours(11)
                    }
                });
                _ = (await scheduleCollection2.CreateOrUpdateAsync(WaitUntil.Completed, RedisPatchScheduleDefaultName.Default, data2)).Value;
                Utilities.Log("Created a Patch Schedules");

                // Create another Patch Schedules
                Utilities.Log("Creating another Patch Schedules...");
                var scheduleCollection3 = task3.Result.Value.GetRedisPatchSchedules();
                var data3 = new RedisPatchScheduleData(new RedisPatchScheduleSetting[]
                {
                    new RedisPatchScheduleSetting(RedisDayOfWeek.Tuesday, 11)
                    {
                        MaintenanceWindow = TimeSpan.FromHours(11)
                    }
                });
                _ = (await scheduleCollection3.CreateOrUpdateAsync(WaitUntil.Completed, RedisPatchScheduleDefaultName.Default, data3)).Value;
                Utilities.Log("Created another Patch Schedules");
                Utilities.Log("Listing Redis Caches");
                await foreach (var caches in redisCacheCollection.GetAllAsync())
                {
                    Utilities.Log("==================");
                    var premium = caches.Data.Sku.Name.Equals("Premium");
                    Utilities.Log(caches.Data.Sku.Name);
                    if (premium)
                    {
                        // Restart Redis Cache
                        Utilities.Log("Restarting updated Redis Cache");
                        var ForceRebootcontent = new RedisRebootContent()
                        {
                            RebootType = RedisRebootType.AllNodes,
                            ShardId = 1
                        };
                        _ = caches.ForceReboot(ForceRebootcontent);
                        Utilities.Log("Redis Cache restart scheduled");

                        // Update each Premium Sku Redis Cache instance
                        Utilities.Log("Updating Premium Redis Cache");
                        var patch = new RedisPatch()
                        {
                            ShardCount = 4,
                            EnableNonSslPort = true,
                            RedisConfiguration = new RedisCommonConfiguration()
                            {

                                MaxMemoryPolicy = "allkeys-random",
                                MaxMemoryReserved = "20"
                            }
                        };
                        _ = await caches.UpdateAsync(patch);
                        Utilities.Log("Updated Premium Redis Cache");

                        // Updating Redis Patch Schedule Setting
                        Utilities.Log("Updating Redis Patch Schedule Setting");
                        var scheduleEntries = new RedisPatchScheduleSetting[]
                        {
                            new RedisPatchScheduleSetting(RedisDayOfWeek.Monday, 5)
                            {
                                MaintenanceWindow = TimeSpan.FromHours(5)
                            }
                        };
                        var scheduleData = new RedisPatchScheduleData(scheduleEntries);
                        foreach(var schedule in caches.GetRedisPatchSchedules())
                        {
                            _ = await schedule.UpdateAsync(WaitUntil.Completed, scheduleData);
                        }
                        Utilities.Log("Updated Redis Patch Schedule Setting");
                        Utilities.Log("Deleting a Redis Cache  - " + task1.Result.Value.Data.Name);
                        _ =caches.DeleteAsync(WaitUntil.Completed);
                        Utilities.Log("Deleted Redis Cache");
                    }
                }
                // ============================================================

                // Delete a Redis Cache
                Utilities.Log("Deleting a Redis Cache  - " + task1.Result.Value.Data.Name);
                _ = task1.Result.Value.DeleteAsync(WaitUntil.Completed);
                Utilities.Log("Deleted Redis Cache");
            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Utilities.Log($"Deleting Resource Group: {_resourceGroupId}");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Utilities.Log($"Deleted Resource Group: {_resourceGroupId}");
                    }
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);
                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}