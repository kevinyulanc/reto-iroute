namespace Commerce.Api.Models;

/// <summary>Body del endpoint de procesamiento. Ej: { "date": "2026-10-07" }</summary>
public record ProcessRequest(DateOnly Date);