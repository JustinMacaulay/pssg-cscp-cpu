namespace Shared.Database;

public class SharedMapper : Profile
{
    public SharedMapper()
    {
        CreateMap<Money, decimal>()
            .ConvertUsing(src => src.Value);
        CreateMap<Money, decimal?>()
            .ConvertUsing(src => src != null ? src.Value : null);
        CreateMap<EntityReference, Guid>()
            .ConvertUsing(src => src.Id);
        CreateMap<EntityReference, Guid?>()
            .ConvertUsing(src => src.Id);
    }
}
