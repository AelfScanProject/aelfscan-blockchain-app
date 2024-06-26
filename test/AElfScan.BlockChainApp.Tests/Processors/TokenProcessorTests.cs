using AeFinder.Block.Dtos;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AElfScan.BlockChainApp.Entities;
using AElfScan.BlockChainApp.GraphQL;
using NSubstitute.Core;
using Shouldly;
using Xunit;
using Query = AElfScan.BlockChainApp.GraphQL.Query;
using Transaction = AeFinder.Sdk.Processor.Transaction;

namespace AElfScan.BlockChainApp.Processors;

public partial class TransactionProcessorTests
{
    private readonly string DefaultTransactionId = "testId1";
    [Fact]
    public async Task BurnProcessor_Test()
    {
       await handleTransaction_Test("From","To",100);
       // Deploy contract
       var burned = new Burned
       {
           Amount = 10,
           Symbol = "ELF",
           Burner = TestAddress
       };
       var logEventContext = GenerateLogEventContext(burned,DefaultTransactionId);
       await _burnedProcessor.ProcessAsync(logEventContext);
       await SaveDataAsync();
       var queryable = await _transactionInfoRepository.GetQueryableAsync(); 
       var result = queryable.Where(o => o.Metadata.ChainId == "AELF").Where(o => o.TransactionId == DefaultTransactionId)
           .ToList(); 
       result.Count.ShouldBe(1);
       result[0].TransactionValue.ShouldBe(burned.Amount);
    }
    
    [Fact]
    public async Task IssueProcessor_Test()
    {
        BlockChainAppConstants.TransactionBeginHeight[ChainId] = 1;
        await handleTransaction_Test("From",BlockChainAppConstants.TransactionAddressListMap[ChainId][0],100);
        var issued = new Issued
        {
            Amount = 100,
            Symbol = "ELF",
            To = TestAddress,
            Memo = "memo"
        };
        var logEventContext = GenerateLogEventContext(issued,DefaultTransactionId);
        await IssuedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        var queryable = await _transactionInfoRepository.GetQueryableAsync(); 
        var result = queryable.Where(o => o.Metadata.ChainId == "AELF").Where(o => o.TransactionId == DefaultTransactionId)
            .ToList(); 
        result.Count.ShouldBe(1);
        result[0].TransactionValue.ShouldBe(issued.Amount);
    }
    
    [Fact]
    public async Task RentalCharged_Test()
    {
        BlockChainAppConstants.TransactionBeginHeight[ChainId] = 1;
        await handleTransaction_Test("From",BlockChainAppConstants.TransactionAddressListMap[ChainId][0],100);
        var @event = new RentalCharged
        {
            Amount = 1,
            Symbol = "ELF",
            Payer = TestAddress,
            Receiver = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG")
        };
        var logEventContext = GenerateLogEventContext(@event,DefaultTransactionId);
        await _rentalChargedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        var queryable = await _transactionInfoRepository.GetQueryableAsync(); 
        var result = queryable.Where(o => o.Metadata.ChainId == "AELF").Where(o => o.TransactionId == DefaultTransactionId)
            .ToList(); 
        result.Count.ShouldBe(1);
        result[0].TransactionValue.ShouldBe(@event.Amount);
    }
    
    [Fact]
    public async Task ResourceTokenClaimedProcessor_Test()
    {
        BlockChainAppConstants.TransactionBeginHeight[ChainId] = 1;
        await handleTransaction_Test("From",BlockChainAppConstants.TransactionAddressListMap[ChainId][0],100);
        var @event = new ResourceTokenClaimed
        {
            Amount = 1,
            Symbol = "ELF",
            Payer = TestAddress,
            Receiver = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG")
        };
        var logEventContext = GenerateLogEventContext(@event,DefaultTransactionId);

        await _resourceTokenClaimedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        var queryable = await _transactionInfoRepository.GetQueryableAsync(); 
        var result = queryable.Where(o => o.Metadata.ChainId == "AELF").Where(o => o.TransactionId == DefaultTransactionId)
            .ToList(); 
        result.Count.ShouldBe(1);
        result[0].TransactionValue.ShouldBe(@event.Amount);
    }
    
    [Fact]
    public async Task TransferredProcessor_Test()
    {
        BlockChainAppConstants.TransactionBeginHeight[ChainId] = 1;
        await handleTransaction_Test("From",BlockChainAppConstants.TransactionAddressListMap[ChainId][0],100);
        var crossChainReceived = new CrossChainReceived
        {
            Amount = 1,
            From = TestAddress,
            Symbol = "ELF",
            To = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            Memo = "memo",
            IssueChainId = 1,
            FromChainId = 1,
            ParentChainHeight = 100,
            TransferTransactionId = Hash.LoadFromHex("cd29ff43ce541c76752638cbc67ce8d4723fd5142cacffa36a95a40c93d30a4c")
        };
        var logEventContext = GenerateLogEventContext(crossChainReceived,DefaultTransactionId);
        await _crossChainReceivedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        var queryable = await _transactionInfoRepository.GetQueryableAsync(); 
        var result = queryable.Where(o => o.Metadata.ChainId == "AELF").Where(o => o.TransactionId == DefaultTransactionId)
            .ToList(); 
        result.Count.ShouldBe(1);
        result[0].TransactionValue.ShouldBe(crossChainReceived.Amount);
    }
    
    [Fact]
    public async Task CrossChainReceivedProcessor_Test()
    {
        BlockChainAppConstants.TransactionBeginHeight[ChainId] = 1;
        await handleTransaction_Test("From",BlockChainAppConstants.TransactionAddressListMap[ChainId][0],100);
        var transferred = new Transferred
        {
            Amount = 1,
            From = TestAddress,
            Symbol = "ELF",
            To = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            Memo = "memo"
        };
        var logEventContext = GenerateLogEventContext(transferred,DefaultTransactionId);
        await _transferredProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        var queryable = await _transactionInfoRepository.GetQueryableAsync(); 
        var result = queryable.Where(o => o.Metadata.ChainId == "AELF").Where(o => o.TransactionId == DefaultTransactionId)
            .ToList(); 
        result.Count.ShouldBe(1);
        result[0].TransactionValue.ShouldBe(transferred.Amount);
    }
    private async Task handleTransaction_Test(string from,string to,long blockHeight)
    {
        var transaction1 = new Transaction()
        {
            TransactionId = DefaultTransactionId,
            MethodName = "testMethod1",
            From = from,
            To = to,
        };
        var transactionContext1 = GenerateTransactionContext(transaction1);
        transactionContext1.Block.BlockHeight = blockHeight;
        transactionContext1.Block.BlockTime = DateTime.Today.AddDays(-2);
       
        await _transactionProcessor.ProcessAsync(transaction1, transactionContext1);
        await SaveDataAsync();
    }
}   