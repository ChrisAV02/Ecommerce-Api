using Ecommerce_Api.Features.Incidents.Application.Dtos.Response;
using Ecommerce_Api.Features.Incidents.Application.Services;
using Ecommerce_Api.Shared.Presentation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Api.Features.Incidents.Controllers;

[ApiController, Route("api/v1/incidents"), Tags("Incidents"), WorkflowKey, ServiceFilter(typeof(WorkflowExceptionFilter))]
public sealed class IncidentsController(IIncidentService incidents) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public Task<PickingIncidentResponse> Get(Guid id, CancellationToken ct) => incidents.GetAsync(id, ct);
}
