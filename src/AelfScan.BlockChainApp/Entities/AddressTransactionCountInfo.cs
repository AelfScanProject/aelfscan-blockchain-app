using AeFinder.Sdk.Entities;

namespace AElfScan.BlockChainApp.Entities;

public class AddressTransactionCountInfo : AeFinderEntity, IAeFinderEntity
{
    public long Count { get; set; }

    public string ChainId { get; set; }

    public string Address { get; set; }
}