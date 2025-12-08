using AeFinder.Sdk;

namespace AElfScan.BlockChainApp.GraphQL;

public class BlockChainAppSchema : AppSchema<Query>
{
    public BlockChainAppSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}