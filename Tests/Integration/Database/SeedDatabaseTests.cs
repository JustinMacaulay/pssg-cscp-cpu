public class SeedDatabaseTests(SeedDatabase seedDatabase, IProgramRepository programRepository, IContractRepository contractRepository)
{
    // WARNING!!! these are not real tests, this is a shortcut I used for load testing, do not use

    //[Fact]
    public void Initialize()
    {
        seedDatabase.Seed();
    }

    //[Fact]
    public void Clear_Fake_Data()
    {
        seedDatabase.Clear();
    }

    //[Fact]
    public void Get_Approved_Programs()
    {
        var contractQuery = new ContractQuery();
        var contracts = contractRepository.Query(contractQuery);

        var programQuery = new ProgramQuery();
        programQuery.StateCode = StateCode.Active;
        var programs = programRepository.Query(programQuery);

        var approvedPrograms = programRepository.GetApproved();
    }
}