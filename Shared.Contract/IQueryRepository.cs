namespace Shared.Contract;

public interface IFindRepository<FQuery, FResult>
{
    FResult FirstOrDefault(FQuery query);
}

public interface IQueryRepository<TQuery, TResult>
{
    TResult Query(TQuery query);
}
