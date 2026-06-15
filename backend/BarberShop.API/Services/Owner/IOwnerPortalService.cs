using BarberShop.API.DTOs.Owner;
using BarberShop.API.DTOs.ServiceCategories;
using BarberShop.API.DTOs.Services;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects.Inventory;
using BarberShop.API.SearchObjects.Owner;

namespace BarberShop.API.Services.Owner;

public interface IOwnerPortalService
{
    Task<OwnerDashboardDto> GetDashboardAsync();
    Task<PagedResult<OwnerAppointmentListItemDto>> GetAppointmentsAsync(OwnerAppointmentSearchObject search);
    Task<IReadOnlyCollection<OwnerEmployeeListItemDto>> GetEmployeesAsync();
    Task<OwnerEmployeeDetailsDto?> GetEmployeeDetailsAsync(int id);
    Task<OwnerEmployeeListItemDto> CreateEmployeeAsync(OwnerEmployeeCreateRequest request);
    Task<OwnerEmployeeListItemDto> ActivateEmployeeAsync(int id);
    Task<OwnerEmployeeListItemDto> DeactivateEmployeeAsync(int id);
    Task<IReadOnlyCollection<ServiceCategoryDto>> GetServiceCategoriesAsync();
    Task<ServiceCategoryDto> CreateServiceCategoryAsync(OwnerServiceCategoryCreateRequest request);
    Task<ServiceCategoryDto> UpdateServiceCategoryAsync(int id, OwnerServiceCategoryUpdateRequest request);
    Task<PagedResult<OwnerServiceListItemDto>> GetServicesAsync(OwnerServiceSearchObject search);
    Task<ServiceDto> CreateServiceAsync(OwnerServiceCreateRequest request);
    Task<ServiceDto> UpdateServiceAsync(int id, OwnerServiceUpdateRequest request);
    Task<PagedResult<OwnerReviewListItemDto>> GetReviewsAsync(OwnerReviewSearchObject search);
    Task<OwnerReviewSummaryDto> GetReviewSummaryAsync();
    Task<PagedResult<OwnerInventoryListItemDto>> GetInventoryAsync(InventoryItemSearchObject search);
    Task<IReadOnlyCollection<OwnerInventoryListItemDto>> GetLowStockInventoryAsync();
    Task<PagedResult<OwnerPaymentListItemDto>> GetPaymentsAsync(OwnerPaymentSearchObject search);
    Task<PagedResult<OwnerTimeOffRequestDto>> GetTimeOffRequestsAsync(OwnerTimeOffRequestSearchObject search);
    Task<OwnerTimeOffRequestDto> ApproveTimeOffRequestAsync(int id, OwnerTimeOffReviewRequest request);
    Task<OwnerTimeOffRequestDto> RejectTimeOffRequestAsync(int id, OwnerTimeOffReviewRequest request);
    Task<OwnerAnalyticsDto> GetAnalyticsAsync();
    Task<OwnerShopSettingsDto> GetShopSettingsAsync();
    Task<OwnerShopSettingsDto> UpdateShopSettingsAsync(OwnerShopSettingsUpdateRequest request);
}
