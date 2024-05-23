using AeFinder.App.BlockProcessing;
using AeFinder.App.BlockState;
using AeFinder.App.OperationLimits;
using AeFinder.Block.Dtos;
using AeFinder.Grains.Grain.BlockStates;
using AeFinder.Sdk;
using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.Entities;
using AElfScan.BlockChainApp.Orleans.TestBase;
using AElfScan.BlockChainApp.Processors;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using Newtonsoft.Json;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Threading;
using Transaction = AeFinder.Sdk.Processor.Transaction;

namespace AElfScan.BlockChainApp;

public abstract class TokenContractAppTestBase : AElfScanBlockChainAppOrleansTestBase<AElfScanBlockChainAppTestModule>
{
    private readonly IAppDataIndexManagerProvider _appDataIndexManagerProvider;
    private readonly IAppBlockStateSetProvider _appBlockStateSetProvider;
    private readonly IOperationLimitManager _operationLimitManager;
    private readonly IBlockProcessingContext _blockProcessingContext;
    protected readonly IssuedProcessor IssuedProcessor;
    protected readonly IObjectMapper ObjectMapper;

    protected readonly IReadOnlyRepository<Entities.TransactionInfo> TransactionInfoReadOnlyRepository;
    protected readonly IReadOnlyRepository<Entities.TransactionCountInfo> TransactionCountInfoReadOnlyRepository;

    protected Address TestAddress = Address.FromBase58("ooCSxQ7zPw1d4rhQPBqGKB6myvuWbicCiw3jdcoWEMMpa54ea");
    protected string ChainId = "AELF";
    protected string BlockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
    protected string PreviousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
    protected string TransactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    protected long BlockHeight = 100;

    public TokenContractAppTestBase()
    {
        _appDataIndexManagerProvider = GetRequiredService<IAppDataIndexManagerProvider>();
        _appBlockStateSetProvider = GetRequiredService<IAppBlockStateSetProvider>();
        _operationLimitManager = GetRequiredService<IOperationLimitManager>();
        _blockProcessingContext = GetRequiredService<IBlockProcessingContext>();

        IssuedProcessor = GetRequiredService<IssuedProcessor>();
        ObjectMapper = GetRequiredService<IObjectMapper>();


        TransactionInfoReadOnlyRepository = GetRequiredService<IReadOnlyRepository<TransactionInfo>>();
        TransactionCountInfoReadOnlyRepository = GetRequiredService<IReadOnlyRepository<TransactionCountInfo>>();

        AsyncHelper.RunSync(async () => await InitializeBlockStateSetAsync());
    }

    protected async Task InitializeBlockStateSetAsync()
    {
        await _appBlockStateSetProvider.AddBlockStateSetAsync(ChainId, new BlockStateSet
        {
            Block = new BlockWithTransactionDto
            {
                ChainId = ChainId,
                BlockHash = BlockHash,
                PreviousBlockHash = PreviousBlockHash,
                BlockHeight = BlockHeight
            },
            Changes = new(),
            Processed = false
        });
        await _appBlockStateSetProvider.SetLongestChainBlockStateSetAsync(ChainId, BlockHash);

        _operationLimitManager.ResetAll();
        _blockProcessingContext.SetContext(ChainId, BlockHash, BlockHeight,
            DateTime.UtcNow, false);
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

    protected async Task SaveDataAsync()
    {
        await _appDataIndexManagerProvider.SavaDataAsync();
        await _appBlockStateSetProvider.SetBestChainBlockStateSetAsync(ChainId, BlockHash);
    }
}