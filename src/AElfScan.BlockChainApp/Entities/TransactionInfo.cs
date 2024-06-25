using AeFinder.Sdk.Entities;
using AeFinder.Sdk.Processor;
using Nest;

namespace AElfScan.BlockChainApp.Entities;

public class TransactionInfo : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public string TransactionId { get; set; }
    public long BlockHeight { get; set; }
    [Keyword] public string ChainId { get; set; }
    [Keyword] public string MethodName { get; set; }
    [Keyword] public TransactionStatus Status { get; set; }
    [Keyword] public string From { get; set; }
    [Keyword] public string To { get; set; }
    public long TransactionValue { get; set; }


    public long Fee { get; set; }
}