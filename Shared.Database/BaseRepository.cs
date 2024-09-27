namespace Shared.Database;

public abstract class BaseRepository<TEntity, TDto> 
    where TEntity : Entity
    where TDto : IDto
{
    private readonly OrganizationServiceContext _databaseContext;
    protected readonly IMapper _mapper;

    public BaseRepository(OrganizationServiceContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }

    public virtual Guid Insert(TDto dto)
    {
        var entity = Map(dto);
        _databaseContext.AddObject(entity);
        _databaseContext.SaveChanges();
        return entity.Id;
    }

    public virtual Guid Upsert(TDto dto)
    {
        var entity = Map(dto);
        var existingEntity = _databaseContext
            .CreateQuery<TEntity>()
            .FirstOrDefault(x => x.Id == entity.Id);
        if (existingEntity != null)
        {
            _databaseContext.Detach(existingEntity);
            _databaseContext.Attach(entity);
            _databaseContext.UpdateObject(entity);
        }
        else
        {
            _databaseContext.AddObject(entity);
        }
        _databaseContext.SaveChanges();
        return entity.Id;
    }

    public virtual bool TryDelete(Guid id)
    {
        var dto = Activator.CreateInstance<TDto>();
        dto.Id = id;
        return TryDelete(dto);
    }

    public virtual bool TryDelete(TDto dto)
    {
        try
        {
            var entity = Map(dto);
            _databaseContext.Attach(entity);
            _databaseContext.DeleteObject(entity);
            _databaseContext.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // safe delete, use TryDelete for faster deletes
    public virtual bool Delete(Guid id)
    {
        var entity = _databaseContext
            .CreateQuery<TEntity>()
            .FirstOrDefault(x => x.Id == id);
        if (entity == null)
        {
            return false;
        }
        _databaseContext.DeleteObject(entity);
        _databaseContext.SaveChanges();
        return true;
    }

    private TEntity Map(TDto dto)
    {
        return _mapper.Map<TEntity>(dto);
    }
}
