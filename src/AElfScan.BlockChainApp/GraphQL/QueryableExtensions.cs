using System.Linq.Expressions;
using AElf.Contracts.MultiToken;
using AElfScan.BlockChainApp.Entities;

namespace AElfScan.BlockChainApp.GraphQL;

public class QueryableExtensions
{
    public static Task<int> CountAsync<T>(IQueryable<T> query)
    {
        return Task.Run(query.Count);
    }


    public static IQueryable<TransactionInfo> TransactionInfoSort(IQueryable<TransactionInfo> queryable,
        GetTransactionInfosInput input)
    {
        return ApplySortingAndPaging(queryable, input.GetAdaptableOrderInfos(), input.SearchAfter);
    }

    private static IQueryable<T> ApplySortingAndPaging<T>(IQueryable<T> queryable, List<OrderInfo> orderInfos,
        List<string> searchAfter)
    {
        if (!orderInfos.IsNullOrEmpty())
        {
            foreach (var orderInfo in orderInfos)
            {
                queryable = AddSort(queryable, orderInfo.OrderBy, orderInfo.Sort);
            }
        }

        if (searchAfter != null && searchAfter.Any())
        {
            queryable = queryable.After(searchAfter.Cast<object>().ToArray());
        }

        return queryable;
    }

    private static IQueryable<T> AddSort<T>(IQueryable<T> queryable, string orderBy, string sort)
    {
        var parameter = Expression.Parameter(typeof(T), "o");
        Expression property = null;
        switch (orderBy)
        {
            case "BlockHeight":
                property = GetNestedPropertyExpression(parameter, "BlockHeight");
                break;
            case "TransactionId":
                property = GetNestedPropertyExpression(parameter, "TransactionId");
                break;
            case "Index":
                property = GetNestedPropertyExpression(parameter, "Index");
                break;
            default:
                throw new Exception("Invalid order by field");
        }

        var lambda = Expression.Lambda(property, parameter);
        string methodName = sort == SortType.Asc.ToString() ? "OrderBy" : "OrderByDescending";
        var resultExpression = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), property.Type },
            queryable.Expression, Expression.Quote(lambda));

        return queryable.Provider.CreateQuery<T>(resultExpression);
    }

    private static Expression GetNestedPropertyExpression(Expression parameter, string propertyPath)
    {
        var properties = propertyPath.Split('.');
        Expression property = parameter;
        foreach (var prop in properties)
        {
            property = Expression.Property(property, prop);
        }

        return property;
    }
}