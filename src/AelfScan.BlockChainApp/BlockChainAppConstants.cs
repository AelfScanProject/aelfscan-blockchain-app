namespace AElfScan.BlockChainApp;

public class BlockChainAppConstants
{
    public static Dictionary<string, string> ContractAddresses = new()
    {
        { "AELF", "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE" },
        { "tDVV", "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX" }
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