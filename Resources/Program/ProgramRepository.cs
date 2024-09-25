namespace Resources;

public class ProgramRepository : BaseRepository<Vsd_Program, Program>, IProgramRepository
{
    private readonly DatabaseContext _databaseContext;

    public ProgramRepository(DatabaseContext databaseContext, IMapper mapper) : base(databaseContext, mapper) 
    {
        _databaseContext = databaseContext;
    }

    public ProgramResult Query(ProgramQuery programQuery)
    {
        var queryResults = _databaseContext.Vsd_ProgramSet
            .WhereIf(programQuery.Id != null, x => x.Id == programQuery.Id)
            .WhereIf(programQuery.StateCode != null, c => c.StateCode == _mapper.Map<Vsd_Program_StateCode>(programQuery.StateCode))
            .ToList();
        var programs = _mapper.Map<IEnumerable<Program>>(queryResults);
        return new ProgramResult(programs);
    }

    public ProgramResult GetApproved()
    {
        var queryResults = (
            from p in _databaseContext.Vsd_ProgramSet
            join c in _databaseContext.Vsd_ContractSet on p.Vsd_ContractId.Id equals c.Vsd_ContractId
            where p.StateCode == Vsd_Program_StateCode.Active
            where p.StatusCode != Vsd_Program_StatusCode.Draft && p.StatusCode != Vsd_Program_StatusCode.ApplicationInfoSent && p.StatusCode != Vsd_Program_StatusCode.ApplicationInfoReceived
            where c.StatusCode == Vsd_Contract_StatusCode.DulyExecuted
            where c.Vsd_Type != Vsd_ContractType.TuaCommunityAccountabilityPrograms
            select p)
                .ToList();
        var programs = _mapper.Map<IEnumerable<Program>>(queryResults);
        return new ProgramResult(programs);
    }
}
