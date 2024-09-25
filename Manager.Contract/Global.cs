namespace Manager.Contract;

// NOTE these values need to match the Dynamics values and also need to be updated concurrently with Dynamics
// these are brittle and a better solution should be considered
public enum StatusCode
{
    Active = 0,
    Inactive = 1
}

public enum YesNo
{
    No = 100000000,
    Yes = 100000001
}

public enum MethodOfPayment
{
    Cheque = 100000001,
    CreditCard = 100000003,
    Eft = 100000000,
    WireTransfer = 100000002,
}

public enum CpuInvoiceType
{
    Deprecated = 100000002,
    OneTimePayment = 100000001,
    ScheduledPayment = 100000000
}

public enum InvoiceType
{
    CounsCourtPsychEd = 100000000,
    DoNotUseMedicalSessions = 100000002,
    OtherPayments = 100000001
}

public enum ProgramUnit
{
    Cpu = 100000003,
    Csu = 100000002,
    Cvap = 100000000,
    Gangs = 100000005,
    Rest = 100000004,
    Vsu = 100000001
}

public enum Quarter
{
    _1StQuarter = 100000000,
    _2NdQuarter = 100000001,
    _3RdQuarter = 100000002,
    _4ThQuarter = 100000003
}