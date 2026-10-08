using Commerce.Api.Models;

namespace Commerce.Api.Repositories;

public interface ICommerceRepository
{
    Task<int> InsertBatchAsync(IReadOnlyCollection<CommerceRecord> records, CancellationToken ct);
    Task<int> ProcessAsync(DateOnly date, CancellationToken ct);
    Task<IReadOnlyList<QuarantineRecord>> GetQuarantineAsync(CancellationToken ct);
}