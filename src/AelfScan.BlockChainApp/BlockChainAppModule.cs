using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.GraphQL;
using AElfScan.BlockChainApp.Processors;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AElfScan.BlockChainApp;

public class BlockChainAppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<BlockChainAppModule>(); });
        context.Services.AddSingleton<ISchema, BlockChainAppSchema>();

        context.Services.AddSingleton<ITokenContractAddressProvider, TokenContractAddressProvider>();
        context.Services.AddSingleton<ILogEventProcessor, CrossChainReceivedProcessor>();
        context.Services.AddSingleton<ILogEventProcessor, CrossChainTransferredProcessor>();
        context.Services.AddSingleton<ILogEventProcessor, IssuedProcessor>();
        context.Services.AddSingleton<ILogEventProcessor, RentalChargedProcessor>();
        context.Services.AddSingleton<ILogEventProcessor, ResourceTokenClaimedProcessor>();
        context.Services.AddSingleton<ILogEventProcessor, TransferredProcessor>();
        context.Services.AddSingleton<ITransactionProcessor, TransactionProcessor>();
    }
}