using AutoMapper;

namespace Resources;

public class FakeRepository : IFakeRepository
{
    protected readonly DatabaseContext _databaseContext;
    protected readonly IMapper _mapper;

    public FakeRepository(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }

    public virtual Guid Insert(FakeDto dto)
    {
        return new Guid();
    }

    public virtual Guid Upsert(FakeDto dto)
    {
        return new Guid();
    }

    public virtual bool TryDelete(Guid id)
    {
        return true;
    }

    public virtual bool TryDelete(FakeDto dto)
    {
        return true;
    }

    public virtual bool Delete(Guid id)
    {
        return true;
    }
}
