namespace AElfScan.BlockChainApp.GraphQL;

public class PagedResultQueryDto
{
    public static int DefaultMaxResultCount { get; set; } = 10;
    public static int MaxMaxResultCount { get; set; } = 25000;
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = DefaultMaxResultCount;
    
    public string OrderBy { get; set; }
    
    public string Sort { get; set; }
    
    public List<OrderInfo> OrderInfos { get; set; }
    public List<string> SearchAfter { get; set; }

    public virtual void Validate()
    {
        if (MaxResultCount > MaxMaxResultCount)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxResultCount),
                $"Max allowed value for {nameof(MaxResultCount)} is {MaxMaxResultCount}.");
        }
    }
    
    public List<OrderInfo> GetAdaptableOrderInfos()
    {
        if (OrderBy.IsNullOrEmpty())
        {
            return OrderInfos;
        }

        return new List<OrderInfo>
        {
            new()
            {
                OrderBy = OrderBy,
                Sort = Sort
            }
        };
    }
}

public class OrderInfo
{
    public string OrderBy { get; set; }
    
    public string Sort { get; set; }
}

public enum SortType
{
    Asc,
    Desc
}