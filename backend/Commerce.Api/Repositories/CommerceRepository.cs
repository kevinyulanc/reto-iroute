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
    /// Inserta todas las filas con sp_create_commerce usando una sola conexión,
    /// un solo comando reutilizado y una transacción (todo o nada).
    /// </summary>
    public async Task<int> InsertBatchAsync(IReadOnlyCollection<CommerceRecord> records, CancellationToken ct)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);

        await using var cmd = new SqlCommand("dbo.sp_create_commerce", conn, tx)
        {
            CommandType = CommandType.StoredProcedure
        };
        var pDate = cmd.Parameters.Add("@pc_processdate", SqlDbType.Date);
        var pTipo = cmd.Parameters.Add("@pc_tipodoc", SqlDbType.NVarChar, 20);
        var pNumdoc = cmd.Parameters.Add("@pc_numdoc", SqlDbType.NVarChar, 50);
        var pNomcom = cmd.Parameters.Add("@pc_nomcomred", SqlDbType.NVarChar, 200);
        var pRazon = cmd.Parameters.Add("@pc_razonsocial", SqlDbType.NVarChar, 200);
        var pDir = cmd.Parameters.Add("@pc_direccion", SqlDbType.NVarChar, 300);
        var pTel = cmd.Parameters.Add("@pc_telefono", SqlDbType.NVarChar, 50);
        var pEmail = cmd.Parameters.Add("@pc_email", SqlDbType.NVarChar, 150);

        try
        {
            foreach (var r in records)
            {
                pDate.Value = r.PcProcessdate.Date;
                pTipo.Value = Db(r.PcTipodoc);
                pNumdoc.Value = Db(r.PcNumdoc);
                pNomcom.Value = Db(r.PcNomcomred);
                pRazon.Value = Db(r.PcRazonsocial);
                pDir.Value = Db(r.PcDireccion);
                pTel.Value = Db(r.PcTelefono);
                pEmail.Value = Db(r.PcEmail);
                await cmd.ExecuteNonQueryAsync(ct);
            }
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
        return records.Count;
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