using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.Entities;
using Newtonsoft.Json;
using Volo.Abp.ObjectMapping;

namespace AElfScan.BlockChainApp.Processors;

public class TransactionProcessor : TransactionProcessorBase
{
    private IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();

    private const long BlockHeightDifference = 250000;

    public static readonly Dictionary<string, List<string>> AddressListMap = new()
    {
        {
            "AELF", new List<string>()
            {
                "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE",
                "pGa4e5hNGsgkfjEGm72TEvbF7aRDqKBd4LuXtab4ucMbXLcgJ"
            }
        },
        {
            "tDVV", new List<string>()
            {
                "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX",
                "BNPFPPwQ3DE9rwxzdY61Q2utU9FZx9KYUnrYHQqCR6N4LLhUE"
            }
        }
    };

    protected IAeFinderLogger Logger => this.LazyServiceProvider.LazyGetService<IAeFinderLogger>();

    public override async Task ProcessAsync(Transaction transaction, TransactionContext context)
    {
        Logger.LogInformation($"start processor transaction data:{context.Block.BlockHeight}");
        var transactionInfo = ObjectMapper.Map<Transaction, TransactionInfo>(transaction);

        Logger.LogInformation("step 1 {c}", context.ChainId);
        transactionInfo.BlockHeight = context.Block.BlockHeight;
        transactionInfo.Fee = GetTransactionFees(transaction.ExtraProperties);

        Logger.LogInformation("step 2 {c}", context.ChainId);
        transactionInfo.Id = IdGenerateHelper.GetId(context.ChainId, transaction.TransactionId);

        await SaveEntityAsync(transactionInfo);
        Logger.LogInformation("step 3 {c}", context.ChainId);
        await HandlerTransactionCountInfoAsync(context.ChainId);
        Logger.LogInformation("step 4 {c}", context.ChainId);
        await HandlerAddressTransactionCountInfoAsync(context.ChainId, transaction.From);
        Logger.LogInformation("step 5 {c}", context.ChainId);
        await HandlerAddressTransactionCountInfoAsync(context.ChainId, transaction.To);
        Logger.LogInformation("step 6 {c}", context.ChainId);

        await HandlerContractBlockTransactionRecordAsync(context.ChainId, transaction.TransactionId,
            context.Block.BlockHeight, transaction.From);
        Logger.LogInformation("step 7 {c}", context.ChainId);

        await HandlerContractBlockTransactionRecordAsync(context.ChainId, transaction.TransactionId,
            context.Block.BlockHeight, transaction.To);
        Logger.LogInformation("step 8 {c}", context.ChainId);

        await DeleteNoUseTransactionInfoAsync(context.Block.BlockHeight, context.ChainId, transaction.From);
        Logger.LogInformation("step 9 {c}", context.ChainId);
        await DeleteNoUseTransactionInfoAsync(context.Block.BlockHeight, context.ChainId, transaction.To);
        Logger.LogInformation("step 10 {c}", context.ChainId);
    }


    private async Task DeleteNoUseTransactionInfoAsync(long blockHeight, string chainId, string address)
    {
        if (!IsContractAddress(chainId, address))
        {
            return;
        }

        var deleteBlockHeight = blockHeight - BlockHeightDifference;
        if (deleteBlockHeight <= 0)
        {
            return;
        }

        var key = IdGenerateHelper.GetId(chainId, deleteBlockHeight,
            address);
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
        Logger.LogInformation("get  AddressTransactionCountInfo id:{c}", id);

        try
        {
            var transactionCountInfo1 =
                await GetEntityAsync<AddressTransactionCountInfo>(id);
        }
        catch (Exception e)
        {
            Logger.LogInformation(e, "get  AddressTransactionCountInfo id:{c}，err", id);
            throw e;
        }

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

        Logger.LogInformation($"Address:{address} transaction count incr to:{transactionCountInfo.Count}");

        await SaveEntityAsync(transactionCountInfo);
    }


    private async Task HandlerContractBlockTransactionRecordAsync(string chainId, string transactionId,
        long blockHeight,
        string address)
    {
        if (!IsContractAddress(chainId, address))
        {
            return;
        }

        var id = IdGenerateHelper.GetId(chainId, blockHeight, address);

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
                ContractAddress = address,
                BlockHeight = blockHeight
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


    public static bool IsContractAddress(string chainId, string address)
    {
        if (address.IsNullOrEmpty())
        {
            return false;
        }

        if (!AddressListMap.TryGetValue(chainId, out var list))
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