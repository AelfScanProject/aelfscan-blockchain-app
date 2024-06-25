using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.Entities;
using AElf.Contracts.MultiToken;

namespace AElfScan.BlockChainApp.Processors;

public class CrossChainReceivedProcessor : TokenProcessorBase<CrossChainReceived>
{
    public override async Task ProcessAsync(CrossChainReceived logEvent, LogEventContext context)
    {
        if (logEvent.Symbol != "ELF")
        {
            return;
        }
        if (BlockChainAppConstants.StartProcessBalanceEventHeight[context.ChainId] > context.Block.BlockHeight)
        {
            return;
        }
        var transactionInfo = await GetEntityAsync<TransactionInfo>(IdGenerateHelper.GetId(context.ChainId,context.Transaction.TransactionId));

        transactionInfo.TransactionValue += logEvent.Amount;
        await SaveEntityAsync(transactionInfo);
    }
}