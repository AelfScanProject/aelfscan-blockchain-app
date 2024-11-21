using AeFinder.Sdk;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AElfScan.BlockChainApp.Entities;
using AElfScan.BlockChainApp.Processors;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;
using Transaction = AeFinder.Sdk.Processor.Transaction;

namespace AElfScan.BlockChainApp.GraphQL;

public class QueryTests : BlockChainAppTestBase
{
    private readonly IObjectMapper _objectMapper;
    private readonly IReadOnlyRepository<TransactionInfo> _transactionRepository;
    private IReadOnlyRepository<TransactionCountInfo> _transactionCountRepository;
    private IReadOnlyRepository<AddressTransactionCountInfo> _addressCountRepository;
    private readonly TransactionProcessor _transactionProcessor;
    private readonly IssuedProcessor _issuedProcessor;
    public QueryTests()
    {
        _objectMapper = GetRequiredService<IObjectMapper>();
        _transactionRepository = GetRequiredService<IReadOnlyRepository<TransactionInfo>>();
        _transactionCountRepository = GetRequiredService<IReadOnlyRepository<TransactionCountInfo>>();
        _addressCountRepository = GetRequiredService<IReadOnlyRepository<AddressTransactionCountInfo>>();
        _transactionProcessor = GetRequiredService<TransactionProcessor>();
        _issuedProcessor = GetRequiredService<IssuedProcessor>();
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
        var  list = await Query.TransactionInfos(_transactionRepository, _transactionCountRepository, _addressCountRepository,
            _objectMapper, new GetTransactionInfosInput
            {
                SkipCount = 0,
                MaxResultCount = 10,
                OrderInfos =  new List<OrderInfo>()
                {
                    new OrderInfo()
                    {
                        OrderBy = "BlockHeight",
                        Sort = "Asc"
                    }
                },
                SearchAfter = new List<string> { "25000" },
                ChainId = "AELF",
                Address = transaction2.From,
                StartTime = ConvertDateTimeToMilliseconds(DateTime.UtcNow.AddDays(-1)),
                EndTime = ConvertDateTimeToMilliseconds(DateTime.UtcNow.AddDays(1))
            });
       
       list.TotalCount.ShouldBe(1);
       list.Items.Count.ShouldBe(1);
       
        list = await Query.TransactionInfos(_transactionRepository, _transactionCountRepository, _addressCountRepository,
           _objectMapper, new GetTransactionInfosInput
           {
               SkipCount = 0,
               MaxResultCount = 10,
               OrderInfos =  new List<OrderInfo>()
               {
                   new OrderInfo()
                   {
                       OrderBy = "BlockHeight",
                       Sort = "Asc"
                   }
               },
               SearchAfter = new List<string> { "25000" },
               ChainId = "AELF",
               StartTime = ConvertDateTimeToMilliseconds(DateTime.UtcNow.AddDays(-1)),
               EndTime = ConvertDateTimeToMilliseconds(DateTime.UtcNow.AddDays(1))
           });
       
       list.TotalCount.ShouldBe(1);
       list.Items.Count.ShouldBe(1);
    }
    
    
     [Fact]
    public async Task HandlerTransactionByHash_Test()
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
        var  list = await Query.TransactionByHash(_transactionRepository, 
            _objectMapper, new GetTransactionInfosByHashInput
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Hashs = new List<string>()
                {
                    transaction2.TransactionId
                }
            });
       list.Items.Count.ShouldBe(1);
       var transactionCount =  await Query.TransactionCount(_transactionCountRepository, new GetTransactionCount
       {
           ChainId = "AELF"
       });
       transactionCount.Count.ShouldBe(1);
       
       var addressTransactionCount =  await Query.AddressTransactionCount(_addressCountRepository,_objectMapper, new GetAddressTransactionCountInput()
       {
           ChainId = "AELF",
           AddressList = new List<string>()
           {
               transaction2.To
           }
       });
       addressTransactionCount.Items[0].Count.ShouldBe(1);
    }
    
    private long ConvertDateTimeToMilliseconds(DateTime dateTime)
    {
        DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(dateTime - unixEpoch).TotalMilliseconds;
    }

}