using AeFinder.Sdk.Entities;

namespace AElfScan.BlockChainApp.Entities;

public class ContractBlockTransactionRecord : AeFinderEntity, IAeFinderEntity
{
    public List<string> TransactionIds { get; set; }

    public long BlockHeight { get; set; }

    public string ContractAddress { get; set; }
}