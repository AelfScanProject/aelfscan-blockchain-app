using AeFinder.Block.Dtos;
using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.GraphQL;
using NSubstitute.Core;
using Shouldly;
using Xunit;
using Query = AElfScan.BlockChainApp.GraphQL.Query;

namespace AElfScan.BlockChainApp.Processors;

public class TransactionProcessorTests : TokenContractAppTestBase
{
    private readonly TransactionProcessor _transactionProcessor;

    public TransactionProcessorTests()
    {
        _transactionProcessor = GetRequiredService<TransactionProcessor>();
    }

    [Fact]
    public async Task HandlerTransaction_Test()
    {
        var transaction1 = new Transaction()
        {
            TransactionId = "testId1",
            MethodName = "testMethod1",
            From = "testFrom1",
            To = "to1"
        };


        var transaction2 = new Transaction()
        {
            TransactionId = "testId2",
            MethodName = "testMethod2",
            From = "testFrom2",
            To = "to2"
        };

        var transaction3 = new Transaction()
        {
            TransactionId = "testId3",
            MethodName = "testMethod3",
            From = "testFrom3",
            To = "to3"
        };
        
        
        var transactionContext1 = GenerateTransactionContext(transaction1);
        transactionContext1.Block.BlockHeight = 1;
        transactionContext1.Block.BlockTime = DateTime.Today.AddDays(-2);


        var transactionContext2 = GenerateTransactionContext(transaction2);
        transactionContext2.Block.BlockHeight = 250001;
        transactionContext2.Block.BlockTime = DateTime.Today;
        
        var transactionContext3 = GenerateTransactionContext(transaction3);
        transactionContext3.Block.BlockHeight = 250002;
        transactionContext3.Block.BlockTime = DateTime.Today;

        await _transactionProcessor.ProcessAsync(transaction1, transactionContext1);
        await SaveDataAsync();

        var transactionResult = await Query.TransactionInfos(TransactionInfoReadOnlyRepository,
            TransactionCountInfoReadOnlyRepository, ObjectMapper,
            new GetTransactionInfosInput()
            {
                ChainId = "AELF",
                SkipCount = 0,
                MaxResultCount = 2,
            });

        transactionResult.Items.Count.ShouldBe(1);
        transactionResult.TotalCount.ShouldBe(1);

        await _transactionProcessor.ProcessAsync(transaction2, transactionContext2);
        await SaveDataAsync();


        var transactionResult2 = await Query.TransactionInfos(TransactionInfoReadOnlyRepository,
            TransactionCountInfoReadOnlyRepository, ObjectMapper,
            new GetTransactionInfosInput()
            {
                ChainId = "AELF",
                SkipCount = 0,
                MaxResultCount = 2,
            });

        transactionResult2.Items.Count.ShouldBe(1);
        transactionResult2.TotalCount.ShouldBe(2);


        await _transactionProcessor.ProcessAsync(transaction3, transactionContext3);
        await SaveDataAsync();

        
        var transactionResult3 = await Query.TransactionInfos(TransactionInfoReadOnlyRepository,
            TransactionCountInfoReadOnlyRepository, ObjectMapper,
            new GetTransactionInfosInput()
            {
                ChainId = "AELF",
                SkipCount = 0,
                MaxResultCount = 2,
                // StartTime = DateTime.Today.Millisecond,
                // EndTime = DateTime.Today.Millisecond
            });

        transactionResult3.Items.Count.ShouldBe(2);
        transactionResult3.TotalCount.ShouldBe(3);
    }
}