namespace Resources;

public class CurrencyRepository : BaseRepository<TransactionCurrency, Currency>, ICurrencyRepository
{
    private readonly DatabaseContext _databaseContext;

    public CurrencyRepository(DatabaseContext databaseContext, IMapper mapper) : base(databaseContext, mapper) 
    {
        _databaseContext = databaseContext;
    }

    public FindCurrencyResult FirstOrDefault(FindCurrencyQuery currencyQuery)
    {
        var queryResults = _databaseContext.TransactionCurrencySet
            .WhereIf(currencyQuery.StateCode != null, c => c.StateCode == (TransactionCurrency_StateCode?)currencyQuery.StateCode)
            .WhereIf(currencyQuery.IsoCurrencyCode == null, p => p.IsoCurrencyCode == currencyQuery.IsoCurrencyCode)
            .FirstOrDefault();
        var currency = _mapper.Map<Currency>(queryResults);
        return new FindCurrencyResult(currency);
    }

    public CurrencyResult Query(CurrencyQuery currencyQuery)
    {
        var queryResults = _databaseContext.TransactionCurrencySet
            .WhereIf(currencyQuery.StateCode == null, c => c.StateCode == (TransactionCurrency_StateCode?)currencyQuery.StateCode)
            .WhereIf(currencyQuery.IsoCurrencyCode == null, c => c.IsoCurrencyCode == currencyQuery.IsoCurrencyCode)
            .ToList();
        var currencies = _mapper.Map<IEnumerable<Currency>>(queryResults);
        return new CurrencyResult(currencies);
    }
}
