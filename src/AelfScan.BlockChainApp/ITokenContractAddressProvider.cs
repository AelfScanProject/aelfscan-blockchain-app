
namespace AElfScan.BlockChainApp;

public interface ITokenContractAddressProvider
{
    string GetContractAddress(string chainId);
}

public class TokenContractAddressProvider : ITokenContractAddressProvider
{
    private readonly Dictionary<string, string> _contractAddresses = BlockChainAppConstants.ContractAddresses;
    
    public string GetContractAddress(string chainId)
    {
        return _contractAddresses[chainId];
    }
}