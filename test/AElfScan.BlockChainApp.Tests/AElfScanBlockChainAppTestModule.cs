using System.Reflection;
using AeFinder.App.TestBase;
using AElfScan.BlockChainApp.Processors;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace AElfScan.BlockChainApp;

[DependsOn(
    typeof(AeFinderAppTestBaseModule),
    typeof(BlockChainAppModule))]
public class AElfScanBlockChainAppTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AeFinderAppEntityOptions>(options => { options.AddTypes<BlockChainAppModule>(); });

        context.Services.AddSingleton<CrossChainReceivedProcessor>();
        context.Services.AddSingleton<BurnedProcessor>();
        context.Services.AddSingleton<IssuedProcessor>();
        context.Services.AddSingleton<RentalChargedProcessor>();
        context.Services.AddSingleton<ResourceTokenClaimedProcessor>();
        context.Services.AddSingleton<TransferredProcessor>();
        context.Services.AddSingleton<TransactionProcessor>();
    }
}