using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.BlockChainApp.Entities;

public class ContractBlockTransactionRecord : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public List<string> TransactionIds { get; set; }

    public long BlockHeight { get; set; }

    [Keyword] public string ContractAddress { get; set; }
}