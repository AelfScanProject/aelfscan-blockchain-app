using Shouldly;
using Xunit;

namespace AElfScan.BlockChainApp.Processors;

public class TokenProcessorBaseTests: TokenContractAppTestBase
{
    private readonly TransferredProcessor _transferredProcessor;

    public TokenProcessorBaseTests()
    {
        _transferredProcessor = GetRequiredService<TransferredProcessor>();
    }
    
    [Fact]
    public async Task GetContractAddressTest()
    {
        var contractAddress = _transferredProcessor.GetContractAddress(ChainId);
        // contractAddress.ShouldBe("AELFTokenContractAddress");
        
        contractAddress = _transferredProcessor.GetContractAddress("tDVV");
        // contractAddress.ShouldBe("tDVVTokenContractAddress");
    }
}