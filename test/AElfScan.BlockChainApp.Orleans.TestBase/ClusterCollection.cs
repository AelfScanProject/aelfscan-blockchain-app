using Xunit;

namespace AElfScan.BlockChainApp.Orleans.TestBase;

[CollectionDefinition(ClusterCollection.Name)]
public class ClusterCollection:ICollectionFixture<ClusterFixture>
{
    public const string Name = "ClusterCollection";
}