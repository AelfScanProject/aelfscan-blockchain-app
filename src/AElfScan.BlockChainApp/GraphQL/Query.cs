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
        [FromServices] IReadOnlyRepository<AddressTransactionCountInfo> addressCountRepository,
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

        if (!input.Address.IsNullOrEmpty())
        {
            queryable = queryable.Where(o => o.From == input.Address || o.To == input.Address);
        }

        queryable = QueryableExtensions.TransactionInfoSort(queryable, input);
        
        var transactionInfos = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        var countQueryable = await countRepository.GetQueryableAsync();
        var totalCount = 0l;

        if (input.Address.IsNullOrEmpty())
        {
            totalCount = countQueryable.Where(p => p.Id == input.ChainId).FirstOrDefault().Count;
        }
        else
        {
            var queryableAsync = await addressCountRepository.GetQueryableAsync();
            // queryable.Where(w => w.ChainId == input.ChainId);
            var transactionCountInfo = queryableAsync.Where(o => o.ChainId == input.ChainId && o.Address == input.Address)
                .FirstOrDefault();
            totalCount = transactionCountInfo == null ? 0 : transactionCountInfo.Count;
        }


        transactionInfoPageResultDto.Items =
            objectMapper.Map<List<TransactionInfo>, List<TransactionInfoDto>>(transactionInfos);
        transactionInfoPageResultDto.TotalCount = totalCount;

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

    public static async Task<AddressTransactionCountResultDto> AddressTransactionCount(
        [FromServices] IReadOnlyRepository<AddressTransactionCountInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetAddressTransactionCountInput input)
    {
        var transactionCountDto = new AddressTransactionCountInfo();
        var queryable = await repository.GetQueryableAsync();


        if (!input.ChainId.IsNullOrEmpty())
        {
            queryable = queryable.Where(c => c.ChainId == input.ChainId);
        }

        if (!input.AddressList.IsNullOrEmpty())
        {
            var predicates = input.AddressList.Select(s =>
                (Expression<Func<AddressTransactionCountInfo, bool>>)(o => o.Address == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));

            queryable = queryable.Where(predicate);
        }


        var addressTransactionCountInfos = queryable.ToList();


        var addressTransactionCountDtos =
            objectMapper.Map<List<AddressTransactionCountInfo>, List<AddressTransactionCountDto>>(
                addressTransactionCountInfos);


        return new AddressTransactionCountResultDto()
        {
            Items = addressTransactionCountDtos
        };
    }
}