namespace Database;

public class GlobalMapper : Profile
{
    public GlobalMapper()
    {
        RecognizeDestinationPrefixes("Vsd_");
        RecognizePrefixes("Vsd_");

        RecognizeDestinationPostfixes("Id");
        RecognizePostfixes("Id");

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
