using AeFinder.App.TestBase;
using AeFinder.Sdk;
using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.Entities;
using AElfScan.BlockChainApp.Processors;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using Newtonsoft.Json;
using Volo.Abp.ObjectMapping;
using Transaction = AeFinder.Sdk.Processor.Transaction;

namespace AElfScan.BlockChainApp;

public abstract class BlockChainAppTestBase :  AeFinderAppTestBase<AElfScanBlockChainAppTestModule>
{
    protected readonly IssuedProcessor IssuedProcessor;
    protected readonly IObjectMapper ObjectMapper;

    protected readonly IReadOnlyRepository<TransactionInfo> TransactionInfoReadOnlyRepository;
    protected readonly IReadOnlyRepository<TransactionCountInfo> TransactionCountInfoReadOnlyRepository;
    protected readonly IReadOnlyRepository<AddressTransactionCountInfo> AddressTransactionCountRepository;

    protected Address TestAddress = Address.FromBase58("ooCSxQ7zPw1d4rhQPBqGKB6myvuWbicCiw3jdcoWEMMpa54ea");
    protected string ChainId = "AELF";
    protected string BlockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
    protected string PreviousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
    protected string TransactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    protected long BlockHeight = 100;

    public BlockChainAppTestBase()
    {
        IssuedProcessor = GetRequiredService<IssuedProcessor>();
        ObjectMapper = GetRequiredService<IObjectMapper>();

        AddressTransactionCountRepository = GetRequiredService<IReadOnlyRepository<AddressTransactionCountInfo>>();
        TransactionInfoReadOnlyRepository = GetRequiredService<IReadOnlyRepository<TransactionInfo>>();
        TransactionCountInfoReadOnlyRepository = GetRequiredService<IReadOnlyRepository<TransactionCountInfo>>();
    }

    

    protected LogEventContext GenerateLogEventContext<T>(T eventData) where T : IEvent<T>
    {
        var logEvent = eventData.ToLogEvent().ToSdkLogEvent();

        return new LogEventContext
        {
            ChainId = ChainId,
            Block = new LightBlock
            {
                BlockHash = BlockHash,
                BlockHeight = BlockHeight,
                BlockTime = DateTime.UtcNow,
                PreviousBlockHash = PreviousBlockHash
            },
            Transaction = new AeFinder.Sdk.Processor.Transaction()
            {
                TransactionId = TransactionId,
                Status = TransactionStatus.Mined,
                ExtraProperties = new Dictionary<string, string>
                {
                    {
                        "TransactionFee", JsonConvert.SerializeObject(new Dictionary<string, long>
                        {
                            { "ELF", 123L }
                        })
                    }
                }
            },
            LogEvent = logEvent
        };
    }
    
    protected LogEventContext GenerateLogEventContext<T>(T eventData,string TransactionId) where T : IEvent<T>
    {
        var logEvent = eventData.ToLogEvent().ToSdkLogEvent();

        return new LogEventContext
        {
            ChainId = ChainId,
            Block = new LightBlock
            {
                BlockHash = BlockHash,
                BlockHeight = BlockHeight,
                BlockTime = DateTime.UtcNow,
                PreviousBlockHash = PreviousBlockHash
            },
            Transaction = new AeFinder.Sdk.Processor.Transaction()
            {
                TransactionId = TransactionId,
                Status = TransactionStatus.Mined,
                
                ExtraProperties = new Dictionary<string, string>
                {
                    {
                        "TransactionFee", JsonConvert.SerializeObject(new Dictionary<string, long>
                        {
                            { "ELF", 123L }
                        })
                    }
                },
                
            },
            LogEvent = logEvent
        };
    }

    protected TransactionContext GenerateTransactionContext(Transaction transaction)
    {
        return new TransactionContext
        {
            ChainId = ChainId,
            Block = new LightBlock
            {
                BlockHash = BlockHash,
                BlockHeight = BlockHeight,
                BlockTime = DateTime.UtcNow,
                PreviousBlockHash = PreviousBlockHash
            },
        };
    }
   
}