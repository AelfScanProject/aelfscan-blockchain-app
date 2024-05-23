using System.Runtime.Serialization;
using AeFinder.Sdk.Processor;
using AElf.CSharp.Core;
using AElfScan.BlockChainApp.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using AeFinder.Sdk.Processor;

namespace AElfScan.BlockChainApp.Processors;

public class TransactionProcessor : TransactionProcessorBase
{
    private IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();

    private const long BlockHeightDifference = 250000;

    public override async Task ProcessAsync(Transaction transaction, TransactionContext context)
    {
        var transactionInfo = ObjectMapper.Map<Transaction, TransactionInfo>(transaction);

        transactionInfo.BlockHeight = context.Block.BlockHeight;
        transactionInfo.Fee = GetTransactionFees(transaction.ExtraProperties);
        transactionInfo.Id = IdGenerateHelper.GetId(context.ChainId, transaction.TransactionId);

        await SaveEntityAsync(transactionInfo);
        await HandlerTransactionCountInfoAsync(context.ChainId);
        await HandlerBlockTransactionInfoAsync(context.ChainId, transaction.TransactionId,
            context.Block.BlockHeight);
        await DeleteNoUseTransactionInfoAsync(context.Block.BlockHeight, context.ChainId);
    }


    private async Task DeleteNoUseTransactionInfoAsync(long blockHeight, string chainId)
    {
        var deleteBlockHeight = blockHeight - BlockHeightDifference;
        if (deleteBlockHeight <= 0)
        {
            return;
        }

        var blockTransactionInfo =
            await GetEntityAsync<BlockTransactionInfo>(IdGenerateHelper.GetId(chainId, deleteBlockHeight));

        if (blockTransactionInfo == null)
        {
            return;
        }

        foreach (var transactionId in blockTransactionInfo.TransactionIds)
        {
            await DeleteEntityAsync<TransactionInfo>(IdGenerateHelper.GetId(chainId, transactionId));
        }
    }

    private async Task HandlerTransactionCountInfoAsync(string chainId)
    {
        var transactionCountInfo = await GetEntityAsync<TransactionCountInfo>(chainId);
        if (transactionCountInfo != null)
        {
            transactionCountInfo.Count++;
        }
        else
        {
            transactionCountInfo = new TransactionCountInfo()
            {
                Id = chainId,
                Count = 1
            };
        }

        await SaveEntityAsync(transactionCountInfo);
    }


    private async Task HandlerBlockTransactionInfoAsync(string chainId, string transactionId, long blockHeight)
    {
        var id = IdGenerateHelper.GetId(chainId, blockHeight);
        var blockTransactionInfo =
            await GetEntityAsync<BlockTransactionInfo>(id);
        if (blockTransactionInfo != null)
        {
            blockTransactionInfo.TransactionIds.Add(transactionId);
        }
        else
        {
            blockTransactionInfo = new BlockTransactionInfo()
            {
                Id = id,
                TransactionIds = new List<string> { transactionId }
            };
        }

        await SaveEntityAsync(blockTransactionInfo);
    }


    private static long GetTransactionFees(Dictionary<string, string> extraProperties)
    {
        var result = 0l;
        var feeMap = new Dictionary<string, long>();
        if (extraProperties == null)
        {
            return 0;
        }

        if (extraProperties.TryGetValue("TransactionFee", out var transactionFee))
        {
            feeMap = JsonConvert.DeserializeObject<Dictionary<string, long>>(transactionFee) ??
                     new Dictionary<string, long>();
            if (feeMap.TryGetValue("ELF", out var fee))
            {
                result += fee;
            }
        }

        if (extraProperties.TryGetValue("ResourceFee", out var resourceFee))
        {
            var resourceFeeMap = JsonConvert.DeserializeObject<Dictionary<string, long>>(resourceFee) ??
                                 new Dictionary<string, long>();
            if (resourceFeeMap.TryGetValue("ELF", out var fee))
            {
                result += fee;
            }
        }

        return result;
    }
}