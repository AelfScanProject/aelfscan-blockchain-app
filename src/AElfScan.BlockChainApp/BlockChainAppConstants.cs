namespace AElfScan.BlockChainApp;

public class BlockChainAppConstants
{
    public static Dictionary<string, string> ContractAddresses = new()
    {
        { "AELF", "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE" },
        { "tDVV", "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX" }
    };
    public static Dictionary<string,long> TransactionBeginHeight = new Dictionary<string, long>
    {
        { "AELF", 217634830 },
        { "tDVV", 205591099 }
    };
    
    public static readonly Dictionary<string, List<string>> TransactionAddressListMap = new()
    {
        {
            "AELF", new List<string>()
            {
                "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE",
                "pGa4e5hNGsgkfjEGm72TEvbF7aRDqKBd4LuXtab4ucMbXLcgJ",
                "2JT8xzjR5zJ8xnBvdgBZdSjfbokFSbF5hDdpUCbXeWaJfPDmsK",
                "2SQ9LeGZYSWmfJcYuQkDQxgd3HzwjamAaaL4Tge2eFSXw2cseq"
            }
        },
        {
            "tDVV", new List<string>()
            {
                "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX",
                "BNPFPPwQ3DE9rwxzdY61Q2utU9FZx9KYUnrYHQqCR6N4LLhUE",
                "4SGo3CUj3PPh3hC5oXV83WodyUaPHuz4trLoSTGFnxe84nqNr",
                "2snHc8AMh9QMbCAa7XXmdZZVM5EBZUUPDdLjemwUJkBnL6k8z9"
            }
        }
    };
    
}