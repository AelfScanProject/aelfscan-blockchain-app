namespace AElfScan.BlockChainApp.GraphQL;

public class TransactionCountDto
{
    public long Count { get; set; }
}

public class GetTransactionCount
{
    public string ChainId { get; set; }
}