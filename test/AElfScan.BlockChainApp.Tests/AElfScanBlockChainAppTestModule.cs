using System.Reflection;
using AeFinder.App;
using AeFinder.App.BlockChain;
using AeFinder.BlockScan;
using AeFinder.Sdk.Entities;
using AElfScan.BlockChainApp.Orleans.TestBase;
using AElfScan.BlockChainApp.Processors;
using AElf.EntityMapping.Elasticsearch;
using AElf.EntityMapping.Elasticsearch.Options;
using AElf.EntityMapping.Elasticsearch.Services;
using AElf.EntityMapping.Options;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace AElfScan.BlockChainApp;

[DependsOn(
    typeof(AElfScanBlockChainAppOrleansTestBaseModule),
    typeof(BlockChainAppModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpAutofacModule),
    typeof(AeFinderAppModule))]
public class AElfScanBlockChainAppTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AElfScanBlockChainAppTestModule>(); });

        context.Services.AddSingleton<CrossChainReceivedProcessor>();
        context.Services.AddSingleton<BurnedProcessor>();
        context.Services.AddSingleton<IssuedProcessor>();
        context.Services.AddSingleton<RentalChargedProcessor>();
        context.Services.AddSingleton<ResourceTokenClaimedProcessor>();
        context.Services.AddSingleton<TransferredProcessor>();
        context.Services.AddSingleton<TransactionProcessor>();


        context.Services.Configure<ChainNodeOptions>(o =>
        {
            o.ChainNodes = new Dictionary<string, string>()
            {
                { "AELF", "http://mainchain.io" },
                { "tDVV", "http://sidechain.io" }
            };
        });

        context.Services.Configure<AElfEntityMappingOptions>(options => { options.CollectionPrefix = "AElfScanTest"; });

        context.Services.Configure<ElasticsearchOptions>(options =>
        {
            options.NumberOfReplicas = 0;
            options.NumberOfShards = 1;
            options.Refresh = Refresh.True;
        });

        context.Services.Configure<AppInfoOptions>(o =>
        {
            o.AppId = "TestAppId";
            o.Version = "TestVersion";
            o.ClientType = ClientType.Query;
        });

        var applicationBuilder = new ApplicationBuilder(context.Services.BuildServiceProvider());
        context.Services.AddObjectAccessor<IApplicationBuilder>(applicationBuilder);
        var mockBlockScanAppService = new Mock<IBlockScanAppService>();
        mockBlockScanAppService.Setup(p => p.GetMessageStreamIdsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(new List<Guid>()));
        context.Services.AddSingleton<IBlockScanAppService>(mockBlockScanAppService.Object);
    }

    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(async () =>
            await CreateIndexAsync(context.ServiceProvider)
        );
    }

    private async Task CreateIndexAsync(IServiceProvider serviceProvider)
    {
        var appInfoOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<AppInfoOptions>>().Value;
        var types = GetTypesAssignableFrom<IAeFinderEntity>(typeof(BlockChainAppModule).Assembly);
        var elasticIndexService = serviceProvider.GetRequiredService<IElasticIndexService>();
        foreach (var t in types)
        {
            var indexName = $"{appInfoOptions.AppId}-{appInfoOptions.Version}.{t.Name}".ToLower();
            await elasticIndexService.CreateIndexAsync(indexName, t, 1, 0);
        }
    }

    private List<Type> GetTypesAssignableFrom<T>(Assembly assembly)
    {
        var compareType = typeof(T);
        return assembly.DefinedTypes
            .Where(type => compareType.IsAssignableFrom(type) && !compareType.IsAssignableFrom(type.BaseType) &&
                           !type.IsAbstract && type.IsClass && compareType != type)
            .Cast<Type>().ToList();
    }

    private async Task DeleteIndexAsync(IServiceProvider serviceProvider)
    {
        var appInfoOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<AppInfoOptions>>().Value;
        var types = GetTypesAssignableFrom<IAeFinderEntity>(typeof(BlockChainAppModule).Assembly);
        var clientProvider = serviceProvider.GetRequiredService<IElasticsearchClientProvider>();
        var client = clientProvider.GetClient();

        foreach (var t in types)
        {
            var indexName = $"{appInfoOptions.AppId}-{appInfoOptions.Version}.{t.Name}".ToLower();
            // TODO: Add delete index api in EntityMapping
            client.Indices.Delete(indexName);
        }
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        AsyncHelper.RunSync(async () =>
            await DeleteIndexAsync(context.ServiceProvider)
        );
    }
}