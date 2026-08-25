using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Services;

namespace Pharmacy.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[EnableRateLimiting("fixed")]
[Route("api/v{version:apiVersion}/medicines")]
[Produces("application/json")]
public sealed class MedicinesController : ControllerBase
{
    private readonly IMedicineService _medicines;

    public MedicinesController(IMedicineService medicines)
    {
        _medicines = medicines;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MedicineListItemDto>>> List(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var items = await _medicines.ListAsync(search, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MedicineDetailDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var item = await _medicines.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<MedicineDetailDto>> Create(
        [FromBody] CreateMedicineRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _medicines.CreateAsync(request, GetIdempotencyKey(), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id, version = "1.0" }, created);
    }

    [HttpPost("{id:guid}/sell")]
    public async Task<ActionResult<SellMedicineResponse>> Sell(
        Guid id,
        [FromBody] SellMedicineRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _medicines.SellAsync(id, request, GetIdempotencyKey(), cancellationToken);
        return Ok(result);
    }

    private string? GetIdempotencyKey() =>
        Request.Headers[AppHeaders.IdempotencyKey].FirstOrDefault();
}
