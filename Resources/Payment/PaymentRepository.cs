namespace Resources;

public class PaymentRepository : BaseRepository<Vsd_Payment, Payment>, IPaymentRepository
{
    private readonly DatabaseContext _databaseContext;

    public PaymentRepository(DatabaseContext databaseContext, IMapper mapper) : base(databaseContext, mapper) 
    {
        _databaseContext = databaseContext;
    }

    public PaymentResult Query(PaymentQuery paymentQuery)
    {
        var query = _databaseContext.Vsd_PaymentSet
            .WhereIf(paymentQuery.ProgramId != null, p => p.Vsd_ProgramId.Id == paymentQuery.ProgramId)
            .WhereIf(paymentQuery.ContractId != null, p => p.Vsd_ContractId.Id == paymentQuery.ContractId)
            .WhereIfNotIn(paymentQuery.ExcludeStatusCodes != null, x => (PaymentStatusCode)x.StatusCode, paymentQuery.ExcludeStatusCodes);

        var queryResults = query.ToList();
        var payments = _mapper.Map<IEnumerable<Payment>>(queryResults);
        return new PaymentResult(payments);
    }
}
