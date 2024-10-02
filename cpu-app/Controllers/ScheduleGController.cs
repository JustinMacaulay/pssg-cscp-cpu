using Gov.Cscp.Victims.Public.Background;
using Gov.Cscp.Victims.Public.Services;
using Manager;
using Manager.Contract;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convert = Manager.Contract.Convert;

namespace Gov.Cscp.Victims.Public.Controllers
{
    [Route("api/[controller]")]
    [JwtAuthorize]
    public class ScheduleGController(IMediator mediator, IBackgroundTaskQueue taskQueue, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory) : Controller
    {
        private readonly CancellationToken _cancellationToken = applicationLifetime.ApplicationStopping;
        private readonly ILogger _logger = loggerFactory.CreateLogger<ScheduleGController>();

        [HttpGet("CreateScheduleGTasks/{quarter}")]
        public async Task<IActionResult> CreateScheduleGTasks(int quarter)
        {
            var quarterPeriod = Convert.ToQuarter(quarter);

            var contractQuery = new ContractQuery();
            contractQuery.StateCode = StateCode.Active;
            contractQuery.StatusCode = ContractStatusCode.DulyExecuted;
            contractQuery.NotNullCustomer = true;
            contractQuery.NotNullFiscalStartDate = true;
            contractQuery.NotNullFiscalEndDate = true;
            contractQuery.NotEqualType = ContractType.TuaCommunityAccountabilityPrograms;
            var contractResult = await mediator.Send(contractQuery, _cancellationToken);
            _logger.LogInformation("Found {0} contracts", contractResult.Contracts.Count());

            var scheduleGTasks = new List<(ScheduleG, Manager.Contract.Task)>();
            foreach (var contract in contractResult.Contracts)
            {
                var programQuery = new ProgramQuery();
                programQuery.StateCode = StateCode.Active;
                programQuery.StatusCode = ProgramStatusCode.Completed;
                programQuery.ContractId = contract.Id;
                var programResult = await mediator.Send(programQuery, _cancellationToken);
                _logger.LogInformation("Found {0} programs", programResult.Programs.Count());

                foreach (var program in programResult.Programs)
                {
                    var scheduleGQuery = new ScheduleGQuery();
                    scheduleGQuery.ProgramId = program.Id;
                    scheduleGQuery.Quarter = quarterPeriod;
                    var scheduleGCount = (await mediator.Send(scheduleGQuery, _cancellationToken))?.ScheduleGs.Count();
                    _logger.LogInformation("Found {0} schedule g's", scheduleGCount);

                    if (scheduleGCount == 0)
                    {
                        _logger.LogInformation(string.Format("Creating a new Schedule G for reporting period {0} and program '{1}'..", quarter, program.Id));
                        var scheduleG = new ScheduleG();
                        scheduleG.ServiceProviderId = contract.CustomerId;
                        scheduleG.ProgramId = program.Id;
                        scheduleG.ContractId = contract.Id;
                        scheduleG.Quarter = quarterPeriod;
                        var insertScheduleGCommand = new InsertCommand<ScheduleG>(scheduleG);
                        scheduleG.Id = await mediator.Send(insertScheduleGCommand, _cancellationToken);

                        _logger.LogInformation(string.Format("Creating a new Task to Schedule G '{0}'..", scheduleG.Id));
                        var task = new Manager.Contract.Task
                        {
                            Subject = string.Format("Schedule G - {0} - {1} Q{2}", program.Name, DateTime.Today.Year, quarter)
                        };
                        task.TaskTypeId = Constant.QuarterlyScheduleG;
                        task.ScheduledEnd = DateTime.Today.AddMonths(1);
                        task.RegardingObjectId = contract.Id;
                        task.ProgramId = program.Id;
                        task.ScheduleGId = scheduleG.Id;
                        var insertTaskCommand = new InsertCommand<Manager.Contract.Task>(task);
                        task.Id = await mediator.Send(insertTaskCommand, _cancellationToken);
                        _logger.LogInformation(string.Format("New task created with id '{0}'..", task.Id));

                        scheduleGTasks.Add((scheduleG, task));
                    }
                }
            }

            return Json(scheduleGTasks);
        }
    }
}
