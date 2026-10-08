using Commerce.Api.Models;
using Commerce.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Api.Controllers;

[ApiController]
[Route("api/commerce")]
public class CommerceController : ControllerBase
{
    private readonly ICommerceService _service;

    public CommerceController(ICommerceService service) => _service = service;

    /// <summary>Carga commerce_DDMMYYYY.csv (campo de formulario: "file").</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken ct)
    {
        try
        {
            var inserted = await _service.UploadAsync(file, ct);
            return Ok(new { registrosInsertados = inserted });
        }
        catch (InvalidFileException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Valida los registros de una fecha y mueve los inválidos a cuarentena.</summary>
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] ProcessRequest request, CancellationToken ct)
    {
        var count = await _service.ProcessAsync(request.Date, ct);
        return Ok(new { registrosEnCuarentena = count });
    }

    /// <summary>Lista los comercios en cuarentena con su motivo.</summary>
    [HttpGet("quarantine")]
    public async Task<ActionResult<IReadOnlyList<QuarantineRecord>>> GetQuarantine(CancellationToken ct) =>
        Ok(await _service.GetQuarantineAsync(ct));
}