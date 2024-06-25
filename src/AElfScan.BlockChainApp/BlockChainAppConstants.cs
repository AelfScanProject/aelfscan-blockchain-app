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
    
    public static readonly Dictionary<string, List<string>> TransactionAddressListMap = new()
    {
        {
            "AELF", new List<string>()
            {
                "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE",
                "pGa4e5hNGsgkfjEGm72TEvbF7aRDqKBd4LuXtab4ucMbXLcgJ"
            }
        },
        {
            "tDVV", new List<string>()
            {
                "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX",
                "BNPFPPwQ3DE9rwxzdY61Q2utU9FZx9KYUnrYHQqCR6N4LLhUE"
            }
        }
    };
    
    public static  Dictionary<string, long> TransactionBeginHeight = new()
    {
        {
            "AELF", 217634830
        },
        {
            "tDVV", 205591099
        }
    };
}