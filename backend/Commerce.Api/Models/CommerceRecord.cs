using CsvHelper.Configuration.Attributes;

namespace Commerce.Api.Models;

/// <summary>Fila del CSV commerce_DDMMYYYY.csv (mismas columnas que la tabla commerce).</summary>
public class CommerceRecord
{
    [Name("pc_processdate")] public DateTime PcProcessdate { get; set; }
    [Name("pc_tipodoc")] public string? PcTipodoc { get; set; }
    [Name("pc_numdoc")] public string? PcNumdoc { get; set; }
    [Name("pc_nomcomred")] public string? PcNomcomred { get; set; }
    [Name("pc_razonsocial")] public string? PcRazonsocial { get; set; }
    [Name("pc_direccion")] public string? PcDireccion { get; set; }
    [Name("pc_telefono")] public string? PcTelefono { get; set; }
    [Name("pc_email")] public string? PcEmail { get; set; }
}