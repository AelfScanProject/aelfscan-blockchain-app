namespace AElfScan.BlockChainApp;

public class BlockChainAppConstants
{
    public static Dictionary<string, string> ContractAddresses = new()
    {
        { "AELF", "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE" },
        { "tDVW", "ASh2Wt7nSEmYqnGxPPzp4pnVDU4uhj1XW9Se5VeZcX2UDdyjx" }
    };


    public static Dictionary<string, long> TransactionBeginHeight = new Dictionary<string, long>
    {
        { "AELF", 193837741 },
        { "tDVW", 133412351 }
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
            "tDVW", new List<string>()
            {
                "2KPUA5wG78nnNmK9JsRWbFiEFUEfei9WKniZuKaVziDKZRwchM",
                "ASh2Wt7nSEmYqnGxPPzp4pnVDU4uhj1XW9Se5VeZcX2UDdyjx",
                "2Z2cfgvH4QkL8KsEKegYJRJ4JC3QYkCVVptF1mvtQVNgZXRL6u",
                "2PC7Jhb5V6iZXxz8uQUWvWubYkAoCVhtRGSL7VhTWX85R8DBuN"
            }
        }
    };
}