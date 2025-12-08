using AeFinder.Sdk.Processor;
using AElfScan.BlockChainApp.Entities;
using AElf.CSharp.Core;
using Volo.Abp.ObjectMapping;

namespace AElfScan.BlockChainApp.Processors;

public abstract class TokenProcessorBase<TEvent> : LogEventProcessorBase<TEvent> where TEvent : IEvent<TEvent>, new()
{
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();

    public ITokenContractAddressProvider ContractAddressProvider { get; set; }

    public override string GetContractAddress(string chainId)
    {
        return ContractAddressProvider.GetContractAddress(chainId);
    }
}