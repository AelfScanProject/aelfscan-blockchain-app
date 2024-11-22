using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.Entities;
using Newtonsoft.Json;
using Volo.Abp.ObjectMapping;

namespace AElfScan.BlockChainApp.Processors;

public class TransactionProcessor : TransactionProcessorBase
{
    private IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();

    private const long BlockHeightDifference = 500000;

    private static readonly List<string> SkipMethodList = new()
    {
        "DonateResourceToken", "UpdateTinyBlockInformation", "UpdateValue", "NextRound", "ApproveMultiProposals",
        "TestTransfer","ProposeCrossChainIndexing","ReleaseCrossChainIndexingProposal"
    };

    protected IAeFinderLogger Logger => this.LazyServiceProvider.LazyGetService<IAeFinderLogger>();

    public override async Task ProcessAsync(Transaction transaction, TransactionContext context)
    {
        await HandlerTransactionCountInfoAsync(context.ChainId);
        await HandlerAddressTransactionCountInfoAsync(context.ChainId, transaction.From);
        await HandlerAddressTransactionCountInfoAsync(context.ChainId, transaction.To);
        bool skip = IsContractAddress(context.ChainId, transaction.To) &&
                    SkipMethodList.Contains(transaction.MethodName);
        if (skip && context.Block.BlockHeight <= BlockChainAppConstants.TransactionBeginHeight[context.ChainId])
        {
            return;
        }

        var transactionInfo = ObjectMapper.Map<Transaction, TransactionInfo>(transaction);
        transactionInfo.BlockHeight = context.Block.BlockHeight;
        transactionInfo.Fee = GetTransactionFees(transaction.ExtraProperties);
        transactionInfo.Id = IdGenerateHelper.GetId(context.ChainId, transaction.TransactionId);
        await SaveEntityAsync(transactionInfo);

        await HandlerContractBlockTransactionRecordAsync(context.ChainId, transaction.TransactionId,
            context.Block.BlockHeight, skip);
        await DeleteNoUseTransactionInfoAsync(context.Block.BlockHeight, context.ChainId, skip);
    }


    private async Task DeleteNoUseTransactionInfoAsync(long blockHeight, string chainId, bool skip)
    {
        if (!skip)
        {
            return;
        }

        var deleteBlockHeight = blockHeight - BlockHeightDifference;
        if (deleteBlockHeight <= BlockChainAppConstants.TransactionBeginHeight[chainId])
        {
            return;
        }

        var key = IdGenerateHelper.GetId(chainId, deleteBlockHeight);
        var blockTransactionInfo =
            await GetEntityAsync<ContractBlockTransactionRecord>(key);

        if (blockTransactionInfo == null)
        {
            return;
        }

        foreach (var transactionId in blockTransactionInfo.TransactionIds)
        {
            var id = IdGenerateHelper.GetId(chainId, transactionId);

            var entityAsync = await GetEntityAsync<TransactionInfo>(id);
            if (entityAsync != null)
            {
                await DeleteEntityAsync<TransactionInfo>(IdGenerateHelper.GetId(chainId, transactionId));
            }
        }

        await DeleteEntityAsync<ContractBlockTransactionRecord>(key);
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


    private async Task HandlerAddressTransactionCountInfoAsync(string chainId, string address)
    {
        if (address.IsNullOrEmpty())
        {
            return;
        }

        var id = IdGenerateHelper.GetId(chainId, address);

        var transactionCountInfo =
            await GetEntityAsync<AddressTransactionCountInfo>(id);

        if (transactionCountInfo != null)
        {
            transactionCountInfo.Count++;
        }
        else
        {
            transactionCountInfo = new AddressTransactionCountInfo()
            {
                Id = id,
                ChainId = chainId,
                Count = 1,
                Address = address
            };
        }

        await SaveEntityAsync(transactionCountInfo);
    }


    private async Task HandlerContractBlockTransactionRecordAsync(string chainId, string transactionId,
        long blockHeight,
        bool skip)
    {
        if (!skip)
        {
            return;
        }

        var id = IdGenerateHelper.GetId(chainId, blockHeight);

        var blockTransactionInfo =
            await GetEntityAsync<ContractBlockTransactionRecord>(id);
        if (blockTransactionInfo != null)
        {
            blockTransactionInfo.TransactionIds.Add(transactionId);
        }
        else
        {
            blockTransactionInfo = new ContractBlockTransactionRecord()
            {
                Id = id,
                TransactionIds = new List<string> { transactionId },
                ContractAddress = "",
                BlockHeight = blockHeight
            };
        }

        await SaveEntityAsync(blockTransactionInfo);
    }


    private long GetTransactionFees(Dictionary<string, string> extraProperties)
    {
        try
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
        catch (Exception e)
        {
            Logger.LogWarning(e, "Get transaction fee error");
        }

        return 0;
    }


    public static bool IsContractAddress(string chainId, string address)
    {
        if (address.IsNullOrEmpty())
        {
            return false;
        }

        if (!BlockChainAppConstants.TransactionAddressListMap.TryGetValue(chainId, out var list))
        {
            return false;
        }


        if (!list.Contains(address))
        {
            return false;
        }

        return true;
    }
}