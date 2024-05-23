using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AeFinder.Sdk;
using AElfScan.BlockChainApp.Entities;
using GraphQL;
using Volo.Abp.ObjectMapping;

namespace AElfScan.BlockChainApp.GraphQL;

public class Query
{
    public static async Task<TransactionInfoPageResultDto> TransactionInfos(
        [FromServices] IReadOnlyRepository<TransactionInfo> repository,
        [FromServices] IReadOnlyRepository<TransactionCountInfo> countRepository,
        [FromServices] IObjectMapper objectMapper, GetTransactionInfosInput input)
    {
        var transactionInfoPageResultDto = new TransactionInfoPageResultDto();
        var queryable = await repository.GetQueryableAsync();

        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        }

        if (input.StartTime > 0 && input.EndTime > 0)
        {
            queryable = queryable.Where(o =>
                o.Metadata.Block.BlockTime >=
                DateTimeOffset.FromUnixTimeMilliseconds(input.StartTime).UtcDateTime &&
                o.Metadata.Block.BlockTime <= DateTimeOffset.FromUnixTimeMilliseconds(input.EndTime).UtcDateTime);
        }

        var transactionInfos = queryable.OrderByDescending(p => p.BlockHeight).Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        var countQueryable = await countRepository.GetQueryableAsync();

        var count = countQueryable.Where(p => p.Id == input.ChainId).FirstOrDefault().Count;

        transactionInfoPageResultDto.Items =
            objectMapper.Map<List<TransactionInfo>, List<TransactionInfoDto>>(transactionInfos);
        transactionInfoPageResultDto.TotalCount = count;
        return transactionInfoPageResultDto;
    }


    public static async Task<TransactionCountDto> TransactionCount(
        [FromServices] IReadOnlyRepository<TransactionCountInfo> repository, GetTransactionCount input)
    {
        var transactionCountDto = new TransactionCountDto();
        var queryable = await repository.GetQueryableAsync();

        var transactionCountInfo = queryable.Where(o => o.Id == input.ChainId).FirstOrDefault();


        transactionCountDto.Count = transactionCountInfo.Count;

        return transactionCountDto;
    }
}