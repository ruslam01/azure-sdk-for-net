// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Azure.Core.TestFramework;
using Azure.Provisioning.Expressions;
using Azure.Provisioning.Tests;
using NUnit.Framework;

namespace Azure.Provisioning.Redis.Tests;

public class BasicRedisTests(bool async)
    : ProvisioningTestBase(async /*, skipTools: true, skipLiveCalls: true /**/)
{
    [Test]
    [Description("https://github.com/Azure/azure-quickstart-templates/blob/master/quickstarts/microsoft.cache/redis-cache/main.bicep")]
    public async Task CreateRedisCache()
    {
        await using Trycep test = CreateBicepTest();
        await test.Define(
            ctx =>
            {
                BicepParameter location =
                    new(nameof(location), typeof(string))
                    {
                        Value = BicepFunction.GetResourceGroup().Location,
                        Description = "The cache location."
                    };

                RedisResource cache =
                    new(nameof(cache), "2020-06-01")
                    {
                        Location = location,
                        EnableNonSslPort = false,
                        MinimumTlsVersion = RedisTlsVersion.Tls1_2,
                        Sku =
                            new RedisSku
                            {
                                Name = RedisSkuName.Standard,
                                Family = RedisSkuFamily.BasicOrStandard,
                                Capacity = 1
                            },
                    };
            })
        .Compare(
            """
            @description('The cache location.')
            param location string = resourceGroup().location

            resource cache 'Microsoft.Cache/redis@2020-06-01' = {
                name: take('cache-${uniqueString(resourceGroup().id)}', 63)
                location: location
                properties: {
                    sku: {
                        name: 'Standard'
                        family: 'C'
                        capacity: 1
                    }
                    enableNonSslPort: false
                    minimumTlsVersion: '1.2'
                }
            }
            """)
        .Lint()
        .ValidateAsync(); // Just validate...  Deploying takes a hot minute.
    }
}
