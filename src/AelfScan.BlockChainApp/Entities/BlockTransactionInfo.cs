using AeFinder.Sdk.Entities;

namespace AElfScan.BlockChainApp.Entities;

public class BlockTransactionInfo : AeFinderEntity, IAeFinderEntity
{
    public List<string> TransactionIds { get; set; }
}