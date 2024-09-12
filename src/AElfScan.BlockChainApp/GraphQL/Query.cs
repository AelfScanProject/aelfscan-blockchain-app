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
            if (!input.ChainId.IsNullOrEmpty())
            {
                countQueryable = countQueryable.Where((c => c.Id == input.ChainId));
            }


            var countList = countQueryable.ToList();
            totalCount = countList.Sum(o => o.Count);
        }
        else
        {
            var queryableAsync = await addressCountRepository.GetQueryableAsync();
            queryableAsync = queryableAsync
                .Where(o => o.Address == input.Address);

            if (!input.ChainId.IsNullOrEmpty())
            {
                queryableAsync = queryableAsync.Where(o => o.ChainId == input.ChainId);
            }

            var addressTransactionCountInfos = queryableAsync.ToList();
            totalCount = addressTransactionCountInfos.Sum(o => o.Count);
        }


        transactionInfoPageResultDto.Items =
            objectMapper.Map<List<TransactionInfo>, List<TransactionInfoDto>>(transactionInfos);
        transactionInfoPageResultDto.TotalCount = totalCount;

        return transactionInfoPageResultDto;
    }


    public static async Task<TransactionInfoPageResultDto> TransactionByHash(
        [FromServices] IReadOnlyRepository<TransactionInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTransactionInfosByHashInput input)
    {
        var transactionInfoPageResultDto = new TransactionInfoPageResultDto();
        var queryable = await repository.GetQueryableAsync();


        if (!input.Hashs.IsNullOrEmpty())
        {
            var predicates = input.Hashs.Select(s =>
                (Expression<Func<Entities.TransactionInfo, bool>>)(o => o.TransactionId == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        var transactionInfos = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        transactionInfoPageResultDto.Items =
            objectMapper.Map<List<TransactionInfo>, List<TransactionInfoDto>>(transactionInfos);

        return transactionInfoPageResultDto;
    }


    public static async Task<TransactionCountDto> TransactionCount(
        [FromServices] IReadOnlyRepository<TransactionCountInfo> repository, GetTransactionCount input)
    {
        var transactionCountDto = new TransactionCountDto();
        var queryable = await repository.GetQueryableAsync();

        if (!input.ChainId.IsNullOrEmpty())
        {
            queryable = queryable.Where(o => o.Id == input.ChainId);
        }

        var transactionCountInfos = queryable.ToList();
        var totalCount = transactionCountInfos.Sum(o => o.Count);

        transactionCountDto.Count = totalCount;

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