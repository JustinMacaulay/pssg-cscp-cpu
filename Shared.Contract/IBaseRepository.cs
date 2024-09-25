namespace Shared.Contract;

public interface IBaseRepository<TDto> where TDto : IDto
{
    Guid Insert(TDto dto);
    Guid Upsert(TDto dto);
    bool TryDelete(Guid id);
    bool TryDelete(TDto dto);
    bool Delete(Guid id);
}
