namespace AElfScan.BlockChainApp;

public class BlockChainAppConstants
{
    public static Dictionary<string, string> ContractAddresses = new()
    {
        { "AELF", "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE" },
        { "tDVW", "ASh2Wt7nSEmYqnGxPPzp4pnVDU4uhj1XW9Se5VeZcX2UDdyjx" }
    };
    
    public static Dictionary<string,long> InitialBalanceEndHeight = new Dictionary<string, long>
    {
        { "AELF", 4100 },
        { "tDVV", 5500 }
    };
    public static Dictionary<string,long> StartProcessBalanceEventHeight = new Dictionary<string, long>
    {
        { "AELF", 193837741 },
        { "tDVV", 182214194 }
    };
}