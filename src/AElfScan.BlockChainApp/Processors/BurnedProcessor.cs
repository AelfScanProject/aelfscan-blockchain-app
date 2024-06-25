using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;
using AElfScan.BlockChainApp.Entities;

namespace AElfScan.BlockChainApp.Processors;

public class BurnedProcessor : TokenProcessorBase<Burned>
{
    public override async Task ProcessAsync(Burned logEvent, LogEventContext context)
    {
        if (logEvent.Symbol != "ELF")
        {
            return;
        }
        if (BlockChainAppConstants.TransactionBeginHeight[context.ChainId] > context.Block.BlockHeight)
        {
            return;
        }
        var transactionInfo = await GetEntityAsync<TransactionInfo>(IdGenerateHelper.GetId(context.ChainId,context.Transaction.TransactionId));

        transactionInfo.TransactionValue += logEvent.Amount;
        await SaveEntityAsync(transactionInfo);
    }
}