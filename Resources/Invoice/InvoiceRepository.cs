namespace Resources;

public class InvoiceRepository : BaseRepository<Vsd_Invoice, Invoice>, IInvoiceRepository
{
    private readonly DatabaseContext _databaseContext;

    public InvoiceRepository(DatabaseContext databaseContext, IMapper mapper) : base(databaseContext, mapper) 
    {
        _databaseContext = databaseContext;
    }

    public InvoiceResult Query(InvoiceQuery invoiceQuery)
    {
        var queryResults = _databaseContext.Vsd_InvoiceSet
            .WhereIf(invoiceQuery.ProgramId != null, c => c.Vsd_ProgramId.Id == invoiceQuery.ProgramId)
            .WhereIf(invoiceQuery.Origin != null, c => c.Vsd_Origin == (Vsd_Invoice_Vsd_Origin?)invoiceQuery.Origin)
            .WhereIf(invoiceQuery.InvoiceDate != null, c => c.Vsd_InvoicedAte == invoiceQuery.InvoiceDate)
            .ToList();
        var invoices = _mapper.Map<IEnumerable<Invoice>>(queryResults);
        return new InvoiceResult(invoices);
    }
}
