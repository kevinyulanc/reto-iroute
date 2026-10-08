using System.Globalization;
using System.Text.RegularExpressions;
using Commerce.Api.Models;
using Commerce.Api.Repositories;
using CsvHelper;
using CsvHelper.Configuration;

namespace Commerce.Api.Services;

/// <summary>Reglas de negocio: validación del archivo y lectura del CSV.</summary>
public class CommerceService : ICommerceService
{
    private static readonly Regex FileNameRegex =
        new(@"^commerce_\d{8}\.csv$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] DateFormats = { "yyyy-MM-dd", "dd/MM/yyyy", "ddMMyyyy", "yyyyMMdd" };

    private readonly ICommerceRepository _repo;

    public CommerceService(ICommerceRepository repo) => _repo = repo;

    public async Task<int> UploadAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new InvalidFileException("El archivo está vacío o no fue enviado.");

        if (!FileNameRegex.IsMatch(file.FileName))
            throw new InvalidFileException("El nombre debe tener el formato commerce_DDMMYYYY.csv.");

        var records = ReadCsv(file);
        if (records.Count == 0)
            throw new InvalidFileException("El archivo no contiene registros.");

        return await _repo.InsertBatchAsync(records, ct);
    }

    public Task<int> ProcessAsync(DateOnly date, CancellationToken ct) => _repo.ProcessAsync(date, ct);

    public Task<IReadOnlyList<QuarantineRecord>> GetQuarantineAsync(CancellationToken ct) =>
        _repo.GetQuarantineAsync(ct);

    private static List<CommerceRecord> ReadCsv(IFormFile file)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            DetectDelimiter = true,                    // acepta coma o punto y coma
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
        };

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, config);
            csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = DateFormats;
            return csv.GetRecords<CommerceRecord>().ToList();
        }
        catch (CsvHelperException ex)
        {
            throw new InvalidFileException($"El CSV no es válido: {ex.Message}");
        }
    }
}