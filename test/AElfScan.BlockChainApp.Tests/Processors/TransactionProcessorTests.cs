using AeFinder.Sdk;
using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;
using AElfScan.BlockChainApp.Entities;
using AElfScan.BlockChainApp.GraphQL;
using Shouldly;
using Xunit;
using Query = AElfScan.BlockChainApp.GraphQL.Query;

namespace AElfScan.BlockChainApp.Processors;

public partial class TransactionProcessorTests : BlockChainAppTestBase
{
    private readonly TransactionProcessor _transactionProcessor;
    private readonly BurnedProcessor _burnedProcessor;
    private readonly CrossChainReceivedProcessor _crossChainReceivedProcessor;
    private readonly IssuedProcessor _issuedProcessor;
    private readonly RentalChargedProcessor _rentalChargedProcessor;
    private readonly ResourceTokenClaimedProcessor _resourceTokenClaimedProcessor;
    private readonly TransferredProcessor _transferredProcessor;
    private readonly IReadOnlyRepository<TransactionInfo> _transactionInfoRepository;
    private readonly IReadOnlyRepository<ContractBlockTransactionRecord> _blockTransactionInfoRepository;

    public TransactionProcessorTests()
    {
        _transactionProcessor = GetRequiredService<TransactionProcessor>();
        _burnedProcessor = GetRequiredService<BurnedProcessor>();
        _crossChainReceivedProcessor = GetRequiredService<CrossChainReceivedProcessor>();
        _issuedProcessor = GetRequiredService<IssuedProcessor>();
        _rentalChargedProcessor = GetRequiredService<RentalChargedProcessor>();
        _resourceTokenClaimedProcessor = GetRequiredService<ResourceTokenClaimedProcessor>();
        _transferredProcessor = GetRequiredService<TransferredProcessor>();
        _transferredProcessor = GetRequiredService<TransferredProcessor>();
        _transactionInfoRepository = GetRequiredService<IReadOnlyRepository<TransactionInfo>>();
        _blockTransactionInfoRepository = GetRequiredService<IReadOnlyRepository<ContractBlockTransactionRecord>>();
    }

    [Fact]
    public async Task HandlerTransaction_Test()
    {
        var transaction1 = new Transaction()
        {
            TransactionId = "testId1",
            MethodName = "testMethod1",
            From = "testFrom1",
            To = "to1",
            Index = 1
        };


        var transaction2 = new Transaction()
        {
            TransactionId = "testId2",
            MethodName = "testMethod2",
            From = "testFrom2",
            To = "to2",
            Index = 2
        };

        var transaction3 = new Transaction()
        {
            TransactionId = "testId3",
            MethodName = "testMethod3",
            From = "testFrom3",
            To = "to3",
            Index = 3
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

            
        var transactionResult = await Query.TransactionInfos(TransactionInfoReadOnlyRepository,
            TransactionCountInfoReadOnlyRepository, AddressTransactionCountRepository, ObjectMapper,
            new GetTransactionInfosInput()
            {
                ChainId = "AELF",
                SkipCount = 0,
                MaxResultCount = 2,
                Address = "testFrom1"
            });

        transactionResult.Items.Count.ShouldBe(1);
        transactionResult.TotalCount.ShouldBe(1);
        transactionResult.Items[0].Index.ShouldBe(1);
        await _transactionProcessor.ProcessAsync(transaction2, transactionContext2);


        var transactionResult2 = await Query.TransactionInfos(TransactionInfoReadOnlyRepository,
            TransactionCountInfoReadOnlyRepository, AddressTransactionCountRepository, ObjectMapper,
            new GetTransactionInfosInput()
            {
                ChainId = "AELF",
                SkipCount = 0,
                MaxResultCount = 2,
            });

        transactionResult2.Items.Count.ShouldBe(2);
        transactionResult2.TotalCount.ShouldBe(2);


        await _transactionProcessor.ProcessAsync(transaction3, transactionContext3);


        var transactionResult3 = await Query.TransactionInfos(TransactionInfoReadOnlyRepository,
            TransactionCountInfoReadOnlyRepository, AddressTransactionCountRepository, ObjectMapper,
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

    [Fact]
    public async Task SkipTransaction_Test()
    {
        await handleTransaction_Test("From", BlockChainAppConstants.TransactionAddressListMap[ChainId][0], 100,
            "UpdateTinyBlockInformation");
        var queryable = await _transactionInfoRepository.GetQueryableAsync();
        var result = queryable.Where(o => o.Metadata.ChainId == "AELF")
            .Where(o => o.TransactionId == DefaultTransactionId)
            .ToList();
        result.Count.ShouldBe(0);
        await handleTransaction_Test("From", BlockChainAppConstants.TransactionAddressListMap[ChainId][0], 100, "aa");
        queryable = await _transactionInfoRepository.GetQueryableAsync();
        result = queryable.Where(o => o.Metadata.ChainId == "AELF").Where(o => o.TransactionId == DefaultTransactionId)
            .ToList();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task SaveSkipTransaction_Test()
    {
        BlockChainAppConstants.TransactionBeginHeight[ChainId] = 1;
        await handleTransaction_Test("From", BlockChainAppConstants.TransactionAddressListMap[ChainId][0], 100,
            "UpdateTinyBlockInformation");
        var queryable = await _transactionInfoRepository.GetQueryableAsync();
        var result = queryable.Where(o => o.Metadata.ChainId == "AELF")
            .Where(o => o.TransactionId == DefaultTransactionId)
            .ToList();
        result.Count.ShouldBe(1);
        var queryable2 = await _blockTransactionInfoRepository.GetQueryableAsync();
        var result2 = queryable2.Where(o => o.Metadata.ChainId == "AELF")
            .ToList();
        result2.Count.ShouldBe(1);
    }

    [Fact]
    public async Task SaveSkipTransactionTwice_Test()
    {
        await SaveSkipTransaction_Test();
        var transaction1 = new Transaction()
        {
            TransactionId = "test2",
            MethodName = "UpdateTinyBlockInformation",
            From = "From",
            To = BlockChainAppConstants.TransactionAddressListMap[ChainId][0],
        };
        var transactionContext1 = GenerateTransactionContext(transaction1);
        transactionContext1.Block.BlockHeight = 100;
        transactionContext1.Block.BlockTime = DateTime.Today.AddDays(-2);

        await _transactionProcessor.ProcessAsync(transaction1, transactionContext1);

        var queryable = await _transactionInfoRepository.GetQueryableAsync();
        var result = queryable.Where(o => o.Metadata.ChainId == "AELF")
            .ToList();
        result.Count.ShouldBe(2);
        var queryable2 = await _blockTransactionInfoRepository.GetQueryableAsync();
        var result2 = queryable2.Where(o => o.Metadata.ChainId == "AELF")
            .ToList();
        result2.Count.ShouldBe(1);
    }

    [Fact]
    public async Task DelSkipTransaction_Test()
    {
        await SaveSkipTransaction_Test();
        var transaction1 = new Transaction()
        {
            TransactionId = "test2",
            MethodName = "UpdateTinyBlockInformation",
            From = "From",
            To = BlockChainAppConstants.TransactionAddressListMap[ChainId][0],
        };
        var transactionContext1 = GenerateTransactionContext(transaction1);
        transactionContext1.Block.BlockHeight = 100 + 500000;
        transactionContext1.Block.BlockTime = DateTime.Today.AddDays(-2);

        await _transactionProcessor.ProcessAsync(transaction1, transactionContext1);

        var queryable = await _transactionInfoRepository.GetQueryableAsync();
        var result = queryable.Where(o => o.Metadata.ChainId == "AELF")
            .Where(o => o.TransactionId == DefaultTransactionId)
            .ToList();
        result.Count.ShouldBe(0);
        var queryable2 = await _blockTransactionInfoRepository.GetQueryableAsync();
        var result2 = queryable2.Where(o => o.Metadata.ChainId == "AELF")
            .ToList();
        result2.Count.ShouldBe(1);
       
    }
    
    
     [Fact]
    public async Task HandlerTransactionAmount_Test()
    {
        var transaction2 = new Transaction()
        {
            TransactionId = "testId2",
            MethodName = "testMethod2",
            From = "testFrom2",
            To = "to2",
            Index = 2
        };
        var transactionContext2 = GenerateTransactionContext(transaction2);
        transactionContext2.Block.BlockHeight = 250001;
        transactionContext2.Block.BlockTime = DateTime.Today;
        await _transactionProcessor.ProcessAsync(transaction2, transactionContext2);

        var issued = new Issued
        {
            Symbol = "ELF",
            Amount = 10,
            To = TestAddress
        };
        
        await _issuedProcessor.ProcessAsync(GenerateLogEventContext(issued,transaction2.TransactionId));

        var transactionResult2 = await Query.TransactionInfos(TransactionInfoReadOnlyRepository,
            TransactionCountInfoReadOnlyRepository, AddressTransactionCountRepository, ObjectMapper,
            new GetTransactionInfosInput()
            {
                ChainId = "AELF",
                SkipCount = 0,
                MaxResultCount = 2,
            });

        transactionResult2.Items.Count.ShouldBe(1);
        transactionResult2.TotalCount.ShouldBe(1);
        transactionResult2.Items[0].TransactionValue.ShouldBe(issued.Amount) ;
        
        var burned = new Burned
        {
            Amount = 10,
            Symbol = "ELF",
            Burner = TestAddress
        };
        await _burnedProcessor.ProcessAsync(GenerateLogEventContext(burned,transaction2.TransactionId));
        transactionResult2 = await Query.TransactionInfos(TransactionInfoReadOnlyRepository,
            TransactionCountInfoReadOnlyRepository, AddressTransactionCountRepository, ObjectMapper,
            new GetTransactionInfosInput()
            {
                ChainId = "AELF",
                SkipCount = 0,
                MaxResultCount = 2,
            });

        transactionResult2.Items.Count.ShouldBe(1);
        transactionResult2.Items[0].TransactionValue.ShouldBe(issued.Amount +burned.Amount); 

        var crossChainReceived = new CrossChainReceived
        {
            From = TestAddress,
            To = TestAddress,
            Amount = 10,
            Symbol = "ELF",

        };
        await _crossChainReceivedProcessor.ProcessAsync(GenerateLogEventContext(crossChainReceived,transaction2.TransactionId));
        transactionResult2 = await Query.TransactionInfos(TransactionInfoReadOnlyRepository,
            TransactionCountInfoReadOnlyRepository, AddressTransactionCountRepository, ObjectMapper,
            new GetTransactionInfosInput()
            {
                ChainId = "AELF",
                SkipCount = 0,
                MaxResultCount = 2,
            });

        transactionResult2.Items.Count.ShouldBe(1);
        transactionResult2.Items[0].TransactionValue.ShouldBe(issued.Amount +burned.Amount+crossChainReceived.Amount) ;
       
    }
}