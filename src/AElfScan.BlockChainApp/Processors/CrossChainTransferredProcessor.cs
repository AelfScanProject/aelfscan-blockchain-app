using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.Entities;
using AElf.Contracts.MultiToken;

namespace AElfScan.BlockChainApp.Processors;

public class CrossChainTransferredProcessor : TokenProcessorBase<CrossChainTransferred>
{
    public override async Task ProcessAsync(CrossChainTransferred logEvent, LogEventContext context)
    {
        if (logEvent.Symbol != "ELF")
        {
            return;
        }

        var transactionInfo = await GetEntityAsync<TransactionInfo>(IdGenerateHelper.GetId(context.ChainId,context.Transaction.TransactionId));

        transactionInfo.TransactionValue += logEvent.Amount;
        await SaveEntityAsync(transactionInfo);
    }
}