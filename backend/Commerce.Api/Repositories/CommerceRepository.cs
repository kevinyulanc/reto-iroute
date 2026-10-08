using System.Data;
using Commerce.Api.Models;
using Microsoft.Data.SqlClient;

namespace Commerce.Api.Repositories;

/// <summary>Acceso a datos con ADO.NET puro, siempre mediante stored procedures.</summary>
public class CommerceRepository : ICommerceRepository
{
    private readonly string _connectionString;

    public CommerceRepository(IConfiguration config) =>
        _connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'Default'.");

    /// <summary>
    /// Inserta todas las filas en una sola llamada a la base de datos, enviándolas
    /// como parámetro tipo tabla a sp_create_commerce_batch (una sola sentencia, atómica).
    /// </summary>
    public async Task<int> InsertBatchAsync(IReadOnlyCollection<CommerceRecord> records, CancellationToken ct)
    {
        var table = new DataTable();
        table.Columns.Add("pc_processdate", typeof(DateTime));
        table.Columns.Add("pc_tipodoc", typeof(string));
        table.Columns.Add("pc_numdoc", typeof(string));
        table.Columns.Add("pc_nomcomred", typeof(string));
        table.Columns.Add("pc_razonsocial", typeof(string));
        table.Columns.Add("pc_direccion", typeof(string));
        table.Columns.Add("pc_telefono", typeof(string));
        table.Columns.Add("pc_email", typeof(string));

        foreach (var r in records)
        {
            table.Rows.Add(r.PcProcessdate.Date, Db(r.PcTipodoc), Db(r.PcNumdoc), Db(r.PcNomcomred),
                           Db(r.PcRazonsocial), Db(r.PcDireccion), Db(r.PcTelefono), Db(r.PcEmail));
        }

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("dbo.sp_create_commerce_batch", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        var p = cmd.Parameters.Add("@rows", SqlDbType.Structured);
        p.TypeName = "dbo.commerce_type";
        p.Value = table;

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    /// <summary>Ejecuta sp_process_commerce y devuelve cuántos registros fueron a cuarentena.</summary>
    public async Task<int> ProcessAsync(DateOnly date, CancellationToken ct)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("dbo.sp_process_commerce", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add("@pc_processdate", SqlDbType.Date).Value = date.ToDateTime(TimeOnly.MinValue);

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task<IReadOnlyList<QuarantineRecord>> GetQuarantineAsync(CancellationToken ct)
    {
        const string sql = @"SELECT id, pc_processdate, pc_tipodoc, pc_numdoc, pc_nomcomred,
                                    pc_razonsocial, pc_direccion, pc_telefono, pc_email, motivo
                             FROM dbo.commerce_quarantine ORDER BY id";
        var list = new List<QuarantineRecord>();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new QuarantineRecord
            {
                Id = rd.GetInt32(0),
                PcProcessdate = rd.GetDateTime(1),
                PcTipodoc = rd.IsDBNull(2) ? null : rd.GetString(2),
                PcNumdoc = rd.IsDBNull(3) ? null : rd.GetString(3),
                PcNomcomred = rd.IsDBNull(4) ? null : rd.GetString(4),
                PcRazonsocial = rd.IsDBNull(5) ? null : rd.GetString(5),
                PcDireccion = rd.IsDBNull(6) ? null : rd.GetString(6),
                PcTelefono = rd.IsDBNull(7) ? null : rd.GetString(7),
                PcEmail = rd.IsDBNull(8) ? null : rd.GetString(8),
                Motivo = rd.GetString(9)
            });
        }
        return list;
    }

    private static object Db(string? value) => value is null ? DBNull.Value : value;
}