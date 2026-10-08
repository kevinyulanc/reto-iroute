using Commerce.Api.Models;

namespace Commerce.Api.Services;

public interface ICommerceService
{
    Task<int> UploadAsync(IFormFile? file, CancellationToken ct);
    Task<int> ProcessAsync(DateOnly date, CancellationToken ct);
    Task<IReadOnlyList<QuarantineRecord>> GetQuarantineAsync(CancellationToken ct);
}