using Gov.Cscp.Victims.Public.Services;
﻿using Manager;
using Manager.Contract;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Shared.Contract;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gov.Cscp.Victims.Public.Controllers
{
    [Route("api/[controller]")]
    [JwtAuthorize]
    public class ContractController(IHostApplicationLifetime applicationLifetime, IMediator mediator) : Controller
    {
        private readonly CancellationToken _cancellationToken = applicationLifetime.ApplicationStopping;

        [HttpGet("Clone")]
        public async Task<IActionResult> Clone()
        {
            var query = new ContractQuery();
            query.StateCode = StateCode.Active;
            query.StatusCode = ContractStatusCode.DulyExecuted;
            query.CpuCloneFlag = true;
            var result = await mediator.Send(query, _cancellationToken);

            List<Guid> ids = new List<Guid>();
            foreach (var contract in result.Contracts)
            {
                // if not cloned
                var isClonedCommand = new IsClonedCommand(contract.Id);
                if (!await mediator.Send(isClonedCommand, _cancellationToken))
                {
                    // clone the contract
                    var command = new CloneCommand(contract);
                    var id = await mediator.Send(command, _cancellationToken);
                    if (id != null) 
                        ids.Add(id.Value);
                }
            }

            return Json(ids);
        }
    }
}
