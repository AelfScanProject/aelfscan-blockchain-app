using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.BlockChainApp.Entities;

public class AddressTransactionCountInfo : AeFinderEntity, IAeFinderEntity
{
    public long Count { get; set; }

    [Keyword] public string ChainId { get; set; }

    [Keyword] public string Address { get; set; }
}

public class GetAddressTransactionCountInput
{
    public string ChainId { get; set; }

    public List<string> AddressList { get; set; }
}