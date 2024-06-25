using Orleans.TestingHost;
using AElfScan.BlockChainApp.TestBase;
using Volo.Abp.Modularity;

namespace AElfScan.BlockChainApp.Orleans.TestBase;

public abstract class AElfScanBlockChainAppOrleansTestBase<TStartupModule>:AElfScanBlockChainAppTestBase<TStartupModule> 
    where TStartupModule : IAbpModule
{
    protected readonly TestCluster Cluster;

    public AElfScanBlockChainAppOrleansTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}