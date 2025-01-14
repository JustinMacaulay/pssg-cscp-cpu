﻿using Manager;
using Manager.Contract;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Gov.Cscp.Victims.Public.Background;
using Microsoft.Extensions.Hosting;
using MediatR;
using Gov.Cscp.Victims.Public.Services;

namespace Gov.Cscp.Victims.Public.Controllers
{
    [Route("api/[controller]")]
    [JwtAuthorize]
    public class ProgramController(IBackgroundTaskQueue taskQueue, IHostApplicationLifetime applicationLifetime, IMediator mediator) : Controller
    {
        private readonly CancellationToken _cancellationToken = applicationLifetime.ApplicationStopping;

        // TODO add ErrorHandler ActionFilter that returns 500 status code and logs the exception
        [HttpGet("Approved/{quarter}")]
        public async Task<IActionResult> GetApproved(int quarter)
        {
            var token = _cancellationToken;
            //await taskQueue.QueueBackgroundWorkItemAsync(GetApprovedWorkItem);
            //return new OkResult();
            //}

            //private async ValueTask GetApprovedWorkItem(CancellationToken token)
            //{
            var provinceBc = Constant.ProvinceBc;

            // query the database for CAD currency id
            //var currencyQuery = new FindCurrencyQuery();
            //currencyQuery.StateCode = StateCode.Active;
            //currencyQuery.IsoCurrencyCode = IsoCurrencyCode.CAD.ToString();
            //var currencyResult = await mediator.Send(currencyQuery, token);
            // CAD currency is static and the same on both DEV and TEST, if you need to retrieve from database, use above code instead
            var currency = new Currency
            {
                Id = Constant.CadCurrency,
                IsoCurrencyCode = IsoCurrencyCode.CAD.ToString()
            };
            var currencyResult = new FindCurrencyResult(currency);
            
            var invoiceDate = GetInvoiceDate(quarter);

            var dummy = new GetApprovedCommand();
            var programResult = await mediator.Send(dummy, token);

            var invoices = new List<(Invoice invoice, InvoiceLineDetail invoiceLineDetail)>();
            foreach (var program in programResult.Programs) 
            {
                var invoiceQuery = new InvoiceQuery();
                invoiceQuery.ProgramId = program.Id;
                invoiceQuery.InvoiceDate = invoiceDate;
                invoiceQuery.Origin = Origin.AutoGenerated;
                // check if invoice has been already created
                var invoiceResult = await mediator.Send(invoiceQuery, token);
                if (invoiceResult.Invoices.Any())
                {
                    continue;
                }

                var paymentQuery = new PaymentQuery();
                paymentQuery.ProgramId = program.Id;
                paymentQuery.ContractId = program.ContractId;
                paymentQuery.ExcludeStatusCodes = new List<PaymentStatusCode> { PaymentStatusCode.Negative, PaymentStatusCode.Canceled };
                var paymentResult = await mediator.Send(paymentQuery, token);
                var paymentTotal = paymentResult.Payments.Sum(p => p.PaymentTotal);
                var scheduledPaymentAmount = program.CpuSubtotal - paymentTotal / (5 - quarter);
                if (scheduledPaymentAmount == 0)
                {
                    throw new Exception("Line item is zero.");
                }

                var invoice = new Invoice();
                invoice.Id = Guid.NewGuid();
                invoice.Origin = Origin.AutoGenerated;
                var contractQuery = new FindContractQuery();
                contractQuery.Id = program.ContractId;
                var customerResults = await mediator.Send(contractQuery, token);
                invoice.PayeeId = customerResults.Contract.Id;
                invoice.ContractId = program.ContractId;
                invoice.ProgramId = program.Id;
                invoice.CurrencyId = currencyResult.Currency.Id;
                invoice.ProgramUnit = ProgramUnit.Cpu;
                invoice.CvapInvoiceType = InvoiceType.OtherPayments;
                invoice.OwnerId = program.OwnerId;
                invoice.TaxExemption = TaxExemption.NoTax;
                invoice.InvoiceDate = invoiceDate;
                invoice.CpuScheduledPaymentDate = invoiceDate.AddDays(3);
                invoice.MethodOfPayment = customerResults.Contract.MethodOfPayment;
                invoice.CpuInvoiceType = CpuInvoiceType.ScheduledPayment;
                invoice.ProvinceStateId = provinceBc;
                invoice.PaymentAdviceComments = string.Format("{0}, {1}-{2}-{3}", program.ContractName, invoiceDate.AddDays(3).Day.ToString(), invoiceDate.AddDays(3).Month.ToString(), invoiceDate.AddDays(3).Year.ToString());
                await mediator.Send(invoice);

                var invoiceLineDetail = new InvoiceLineDetail();
                invoiceLineDetail.Id = Guid.NewGuid();
                invoiceLineDetail.InvoiceId = invoice.Id;
                invoiceLineDetail.OwnerId = program.OwnerId ?? default;
                invoiceLineDetail.InvoiceType = InvoiceType.OtherPayments;
                invoiceLineDetail.ProgramUnit = ProgramUnit.Cpu;
                invoiceLineDetail.Approved = YesNo.Yes;
                invoiceLineDetail.AmountSimple = scheduledPaymentAmount ?? 0;
                invoiceLineDetail.ProvinceStateId = provinceBc;
                invoiceLineDetail.TaxExemption = invoice.TaxExemption;
                var invoiceLineDetailId = await mediator.Send(invoiceLineDetail);

                invoices.Add((invoice, invoiceLineDetail));
            }

            return Json(invoices);
        }

        private DateTime GetInvoiceDate(int quarter)
        {
            var firstQuarterDate = new DateTime(DateTime.Today.Year, 1, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th January
            var secondQuarterDate = new DateTime(DateTime.Today.Year, 4, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th April
            var thirdQuarterDate = new DateTime(DateTime.Today.Year, 7, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th July
            var fourthQuarterDate = new DateTime(DateTime.Today.Year, 10, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th October
            var fifthQuarterDate = new DateTime(DateTime.Today.Year + 1, 1, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th January next year

            if (quarter == 1)
                return secondQuarterDate.AddDays(-3); //15-April-current year
            else if (quarter == 2)
                return thirdQuarterDate.AddDays(-3); //15-July-current year
            else if (quarter == 3)
                return fourthQuarterDate.AddDays(-3); //15-October-current year
            else if (quarter == 4)
                return firstQuarterDate.AddDays(-3); //15-January-current year
            else
                throw new ArgumentException(string.Format("Invalid Quarter number '{0}'..", quarter));
            }

        // NOTE use this version if quarter is not provided
        // what happens with dates Oct 16 to Dec 31, should it trigger quarter 1 and use next year's quarter?
        //private Tuple<short, DateTime> GetInvoiceDate()
        //{
        //    var firstQuarterDate = new DateTime(DateTime.Today.Year, 1, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th January
        //    var secondQuarterDate = new DateTime(DateTime.Today.Year, 4, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th April
        //    var thirdQuarterDate = new DateTime(DateTime.Today.Year, 7, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th July
        //    var fourthQuarterDate = new DateTime(DateTime.Today.Year, 10, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th October
        //    var fifthQuarterDate = new DateTime(DateTime.Today.Year + 1, 1, 15, DateTime.Today.Hour, DateTime.Today.Minute, DateTime.Today.Second, DateTimeKind.Local); //15th January next year

        //    if (DateTime.Today > firstQuarterDate && DateTime.Today <= secondQuarterDate)
        //    {
        //        return new Tuple<short, DateTime>(1, secondQuarterDate.AddDays(-3));
        //    }
        //    else if (DateTime.Today > secondQuarterDate && DateTime.Today <= thirdQuarterDate)
        //    {
        //        return new Tuple<short, DateTime>(2, thirdQuarterDate.AddDays(-3));
        //    }
        //    else if (DateTime.Today > thirdQuarterDate && DateTime.Today <= fourthQuarterDate)
        //    {
        //        return new Tuple<short, DateTime>(3, fourthQuarterDate.AddDays(-3));
        //    }
        //    else if (DateTime.Today <= firstQuarterDate)
        //    {
        //        return new Tuple<short, DateTime>(4, firstQuarterDate.AddDays(-3));
        //    }
        //    else
        //    {
        //        //throw new Exception("Invalid quarter number");
        //        return new Tuple<short, DateTime>(1, fifthQuarterDate.AddDays(-3));
        //    }
        //}
    }
}
