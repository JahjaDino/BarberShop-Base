using BarberShop.API.DTOs.Reviews;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects.Reviews;

namespace BarberShop.API.Services.Reviews;

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(ReviewInsertRequest request);

    Task<PagedResult<ReviewDto>> GetMineAsync(ReviewSearchObject search);

    Task<IReadOnlyCollection<PendingReviewDto>> GetPendingAsync();

    Task<PagedResult<ReviewDto>> GetAsync(ReviewSearchObject search);

    Task<ReviewDto?> GetByIdAsync(int id);

    Task<bool> DeleteAsync(int id);
}
