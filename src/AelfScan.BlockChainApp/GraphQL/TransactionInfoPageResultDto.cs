using AeFinder.Sdk.Dtos;
using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.Entities;
using Nest;

namespace AElfScan.BlockChainApp.GraphQL;

public class TransactionInfoPageResultDto
{
    public long TotalCount { get; set; }
    public List<TransactionInfoDto> Items { get; set; }
}

public class GetTransactionInfosInput : PagedResultQueryDto
{
    public string ChainId { get; set; }

    public string Address { get; set; }

    public long StartTime { get; set; }

    public long EndTime { get; set; }
}

public class TransactionInfoDto : AeFinderEntityDto
{
    public string TransactionId { get; set; }
    public long BlockHeight { get; set; }
    public string ChainId { get; set; }
    public string MethodName { get; set; }
    public TransactionStatus Status { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public long TransactionValue { get; set; }

    public long Fee { get; set; }
}