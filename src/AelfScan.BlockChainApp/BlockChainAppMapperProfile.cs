using AElfScan.BlockChainApp.Entities;
using AElfScan.BlockChainApp.GraphQL;
using AElf;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AutoMapper;
using Transaction = AeFinder.Sdk.Processor.Transaction;

namespace AElfScan.BlockChainApp;

public class BlockChainAppMapperProfile : Profile
{
    public BlockChainAppMapperProfile()
    {
        // Common
        CreateMap<Hash, string>().ConvertUsing(s => s == null ? string.Empty : s.ToHex());
        CreateMap<Address, string>().ConvertUsing(s => s == null ? string.Empty : s.ToBase58());


        CreateMap<Transaction, TransactionInfo>();
        CreateMap<TransactionInfo, TransactionInfoDto>();
    }
}