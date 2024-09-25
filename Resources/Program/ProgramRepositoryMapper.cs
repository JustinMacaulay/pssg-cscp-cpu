namespace Resources;

public class ProgramRepositoryMapper : Profile
{
    public ProgramRepositoryMapper()
    {
        CreateMap<Vsd_Program, Program>()
            .ForMember(dest => dest.ContractId, opts => opts.MapFrom(src => src.Vsd_ContractId.Id))
            .ForMember(dest => dest.ContractName, opts => opts.MapFrom(src => src.Vsd_ContractIdName))
            .ForMember(dest => dest.CpuSubtotal, opts => opts.MapFrom(src => src.Vsd_Cpu_SubtotalComponentValue != null ? src.Vsd_Cpu_SubtotalComponentValue.Value : (decimal?)null))
            .ForMember(dest => dest.BudgetProposalSignatureDate, opts => opts.MapFrom(src => src.Vsd_BudgetProposalSignaturedAte))
            .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Vsd_Name));

        CreateMap<Program, Vsd_Program>()
            .ForMember(dest => dest.Vsd_ContractIdName, opts => opts.MapFrom(src => src.ContractName))
            .ForMember(dest => dest.Vsd_Cpu_SubtotalComponentValue, opts => opts.MapFrom(src => src.CpuSubtotal))
            .ForMember(dest => dest.Vsd_BudgetProposalSignaturedAte, opts => opts.MapFrom(src => src.BudgetProposalSignatureDate))
            // NOTE in theory, this shouldn't be necessary
            .ForMember(dest => dest.Vsd_ProvinceState, opts => opts.MapFrom(src => src.ProvinceState))
            .ForMember(dest => dest.Vsd_Name, opts => opts.MapFrom(src => src.Name))
            // reference keys
            .ForMember(dest => dest.OwnerId, opts => opts.MapFrom(src => src.OwnerId != null ? new EntityReference("systemuser", src.OwnerId.Value) : null))
            .ForMember(dest => dest.Vsd_ContractId, opts => opts.MapFrom(src => src.ContractId != null ? new EntityReference(Vsd_Contract.EntityLogicalName, src.ContractId.Value) : null));

        CreateMap<StateCode, Vsd_Program_StateCode>();
    }
}
