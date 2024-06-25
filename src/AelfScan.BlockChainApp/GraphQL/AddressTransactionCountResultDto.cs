using JetBrains.Annotations;

namespace AElfScan.BlockChainApp.GraphQL;

public class AddressTransactionCountResultDto
{
    public List<AddressTransactionCountDto> Items { get; set; }
}

public class AddressTransactionCountDto
{
    public long Count { get; set; }

    public string ChainId { get; set; }

    public string Address { get; set; }
}