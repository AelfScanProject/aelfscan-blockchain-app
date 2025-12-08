using AeFinder.Sdk.Entities;

namespace AElfScan.BlockChainApp.Entities;

public class TransactionCountInfo: AeFinderEntity, IAeFinderEntity
{
    public long Count { get; set; }
}


