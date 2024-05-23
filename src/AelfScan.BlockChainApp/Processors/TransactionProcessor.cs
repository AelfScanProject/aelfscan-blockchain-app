using System.Runtime.Serialization;
using AeFinder.Sdk.Logging;
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

    public static readonly Dictionary<string, List<string>> AddressListMap = new()
    {
        {
            "AELF", new List<string>()
            {
                "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE",
                "pykr77ft9UUKJZLVq15wCH8PinBSjVRQ12sD1Ayq92mKFsJ1i"
            }
        },
        {
            "tDVV", new List<string>()
            {
                "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX",
                "2dtnkWDyJJXeDRcREhKSZHrYdDGMbn3eus5KYpXonfoTygFHZm"
            }
        }
    };


    protected IAeFinderLogger Logger => this.LazyServiceProvider.LazyGetService<IAeFinderLogger>();

    public override async Task ProcessAsync(Transaction transaction, TransactionContext context)
    {
        Logger.LogInformation($"start processor transaction data:{context.Block.BlockHeight}");
        var transactionInfo = ObjectMapper.Map<Transaction, TransactionInfo>(transaction);

        transactionInfo.BlockHeight = context.Block.BlockHeight;
        transactionInfo.Fee = GetTransactionFees(transaction.ExtraProperties);
        transactionInfo.Id = IdGenerateHelper.GetId(context.ChainId, transaction.TransactionId);

        await SaveEntityAsync(transactionInfo);
        await HandlerTransactionCountInfoAsync(context.ChainId);
        await HandlerAddressTransactionCountInfoAsync(context.ChainId, transaction.From);
        await HandlerAddressTransactionCountInfoAsync(context.ChainId, transaction.To);

        await HandlerContractBlockTransactionRecordAsync(context.ChainId, transaction.TransactionId,
            context.Block.BlockHeight, transaction.From);

        await HandlerContractBlockTransactionRecordAsync(context.ChainId, transaction.TransactionId,
            context.Block.BlockHeight, transaction.To);

        await DeleteNoUseTransactionInfoAsync(context.Block.BlockHeight, context.ChainId, transaction.From);
        await DeleteNoUseTransactionInfoAsync(context.Block.BlockHeight, context.ChainId, transaction.To);
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