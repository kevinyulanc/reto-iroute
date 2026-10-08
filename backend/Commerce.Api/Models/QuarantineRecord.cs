namespace Commerce.Api.Models;

/// <summary>Registro rechazado en la validación, con el motivo.</summary>
public class QuarantineRecord
{
    public int Id { get; set; }
    public DateTime PcProcessdate { get; set; }
    public string? PcTipodoc { get; set; }
    public string? PcNumdoc { get; set; }
    public string? PcNomcomred { get; set; }
    public string? PcRazonsocial { get; set; }
    public string? PcDireccion { get; set; }
    public string? PcTelefono { get; set; }
    public string? PcEmail { get; set; }
    public string Motivo { get; set; } = "";
}