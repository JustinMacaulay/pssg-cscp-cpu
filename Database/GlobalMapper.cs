namespace Database;

public class GlobalMapper : Profile
{
    public GlobalMapper()
    {
        RecognizeDestinationPrefixes("Vsd_");
        RecognizePrefixes("Vsd_");

        RecognizeDestinationPostfixes("Id");
        RecognizePostfixes("Id");
    }
}
