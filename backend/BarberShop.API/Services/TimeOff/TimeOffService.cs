using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.TimeOff;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using BarberShop.API.SearchObjects.TimeOff;
using BarberShop.API.Services.Base;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;
using TimeOffEntity = BarberShop.API.Entities.TimeOff;

namespace BarberShop.API.Services.TimeOff;

public class TimeOffService
    : BaseCRUDService<TimeOffEntity, TimeOffDto, TimeOffSearchObject, TimeOffInsertRequest, TimeOffUpdateRequest>,
        ITimeOffService
{
    private readonly IOwnerAccessService _ownerAccessService;
    private readonly ICurrentUserService _currentUserService;

    public TimeOffService(
        BarberShopDbContext dbContext,
        IOwnerAccessService ownerAccessService,
        ICurrentUserService currentUserService)
        : base(dbContext)
    {
        _ownerAccessService = ownerAccessService;
        _currentUserService = currentUserService;
    }

    public override async Task<PagedResult<TimeOffDto>> GetAsync(TimeOffSearchObject search)
    {
        var ownerShopId = await GetOwnerShopIdOrThrowAsync();
        var page = Math.Max(0, search.Page);
        var pageSize = search.PageSize <= 0
            ? BaseSearchObject.DefaultPageSize
            : Math.Min(search.PageSize, BaseSearchObject.MaxPageSize);

        var query = DbContext.TimeOffs
            .AsNoTracking()
            .Include(timeOff => timeOff.Employee)
            .Include(timeOff => timeOff.ReviewedByUser)
            .Where(timeOff => timeOff.Employee.ShopId == ownerShopId);

        query = AddFilter(query, search)
            .OrderByDescending(timeOff => timeOff.StartTime);

        var totalCount = search.IncludeTotalCount ? await query.CountAsync() : 0;
        var items = await (search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize)).ToListAsync();

        return new PagedResult<TimeOffDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0
        };
    }

    public override async Task<TimeOffDto?> GetByIdAsync(int id)
    {
        var ownerShopId = await GetOwnerShopIdOrThrowAsync();

        var entity = await DbContext.TimeOffs
            .AsNoTracking()
            .Include(timeOff => timeOff.Employee)
            .Include(timeOff => timeOff.ReviewedByUser)
            .FirstOrDefaultAsync(timeOff => timeOff.Id == id && timeOff.Employee.ShopId == ownerShopId);

        return entity is null ? null : MapToDto(entity);
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var entity = await DbContext.TimeOffs
            .Include(timeOff => timeOff.Employee)
            .FirstOrDefaultAsync(timeOff => timeOff.Id == id);

        if (entity is null)
        {
            throw new NotFoundException("Time off does not exist.");
        }

        await BeforeDelete(entity);

        entity.Status = TimeOffStatuses.CANCELLED;
        await DbContext.SaveChangesAsync();

        await AfterDelete(entity);

        return true;
    }

    public async Task<TimeOffDto?> UpdateStatusAsync(int id, TimeOffStatusUpdateRequest request)
    {
        var entity = await DbContext.TimeOffs
            .Include(timeOff => timeOff.Employee)
            .FirstOrDefaultAsync(timeOff => timeOff.Id == id);

        if (entity is null)
        {
            return null;
        }

        await EnsureOwnerCanAccessShopAsync(entity.Employee.ShopId);

        var status = MapStatus(request.Status);
        ApplyReviewedStatus(entity, status, request.ReviewNote);

        await DbContext.SaveChangesAsync();

        return MapToDto(entity);
    }

    protected override IQueryable<TimeOffEntity> AddFilter(IQueryable<TimeOffEntity> query, TimeOffSearchObject search)
    {
        if (search.EmployeeId.HasValue)
        {
            query = query.Where(timeOff => timeOff.EmployeeId == search.EmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.Status))
        {
            var status = search.Status.Trim().ToUpper();
            query = query.Where(timeOff => timeOff.Status.ToUpper() == status);
        }

        if (search.DateFrom.HasValue)
        {
            var dateFrom = ToUtcDateTime(search.DateFrom.Value);
            query = query.Where(timeOff => timeOff.EndTime >= dateFrom);
        }

        if (search.DateTo.HasValue)
        {
            var dateTo = ToUtcDateTime(search.DateTo.Value);
            query = query.Where(timeOff => timeOff.StartTime <= dateTo);
        }

        return query;
    }

    protected override TimeOffDto MapToDto(TimeOffEntity entity)
    {
        return new TimeOffDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Reason = entity.Reason,
            Status = entity.Status,
            ReviewedByUserId = entity.ReviewedByUserId,
            ReviewedAt = entity.ReviewedAt,
            ReviewNote = entity.ReviewNote,
            ReviewedByName = entity.ReviewedByUser is null
                ? null
                : entity.ReviewedByUser.FirstName + " " + entity.ReviewedByUser.LastName
        };
    }

    protected override TimeOffEntity MapInsertToEntity(TimeOffInsertRequest request)
    {
        return new TimeOffEntity
        {
            EmployeeId = request.EmployeeId,
            StartTime = ToUtcDateTime(request.StartTime),
            EndTime = ToUtcDateTime(request.EndTime),
            Reason = request.Reason?.Trim(),
            Status = TimeOffStatuses.PENDING
        };
    }

    protected override void MapUpdateToEntity(TimeOffEntity entity, TimeOffUpdateRequest request)
    {
        entity.EmployeeId = request.EmployeeId;
        entity.StartTime = ToUtcDateTime(request.StartTime);
        entity.EndTime = ToUtcDateTime(request.EndTime);
        entity.Reason = request.Reason?.Trim();

        var requestedStatus = MapStatus(request.Status);
        if (requestedStatus == entity.Status)
        {
            return;
        }

        if (requestedStatus == TimeOffStatuses.PENDING)
        {
            throw new BadRequestException("Reviewed time off request cannot be moved back to pending.");
        }

        ApplyReviewedStatus(entity, requestedStatus, entity.ReviewNote);
    }

    protected override async Task BeforeInsert(TimeOffEntity entity, TimeOffInsertRequest request)
    {
        var employee = await GetActiveEmployeeAsync(request.EmployeeId);
        await EnsureOwnerCanAccessShopAsync(employee.ShopId);
        ValidateTimeRange(request.StartTime, request.EndTime);
        await EnsureNoOverlapAsync(request.EmployeeId, ToUtcDateTime(request.StartTime), ToUtcDateTime(request.EndTime));
    }

    protected override async Task BeforeUpdate(TimeOffEntity entity, TimeOffUpdateRequest request)
    {
        var existingEmployee = await GetEmployeeAsync(entity.EmployeeId);
        await EnsureOwnerCanAccessShopAsync(existingEmployee.ShopId);

        var requestedEmployee = await GetActiveEmployeeAsync(request.EmployeeId);
        await EnsureOwnerCanAccessShopAsync(requestedEmployee.ShopId);

        ValidateTimeRange(request.StartTime, request.EndTime);
        await EnsureNoOverlapAsync(request.EmployeeId, ToUtcDateTime(request.StartTime), ToUtcDateTime(request.EndTime), entity.Id);
    }

    protected override async Task BeforeDelete(TimeOffEntity entity)
    {
        var employee = entity.Employee ?? await GetEmployeeAsync(entity.EmployeeId);
        await EnsureOwnerCanAccessShopAsync(employee.ShopId);
    }

    private async Task<Employee> GetEmployeeAsync(int employeeId)
    {
        var employee = await DbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(currentEmployee => currentEmployee.Id == employeeId);
        if (employee is null)
        {
            throw new BadRequestException("Employee does not exist.");
        }

        return employee;
    }

    private async Task<Employee> GetActiveEmployeeAsync(int employeeId)
    {
        var employee = await GetEmployeeAsync(employeeId);
        if (!employee.Active)
        {
            throw new BadRequestException("Employee is not active.");
        }

        return employee;
    }

    private static void ValidateTimeRange(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        if (startTime == default || endTime == default)
        {
            throw new BadRequestException("Start time and end time are required.");
        }

        if (startTime >= endTime)
        {
            throw new BadRequestException("Start time must be before end time.");
        }
    }

    private async Task EnsureNoOverlapAsync(
        int employeeId,
        DateTime startTime,
        DateTime endTime,
        int? currentTimeOffId = null)
    {
        var overlaps = await DbContext.TimeOffs.AnyAsync(timeOff =>
            timeOff.EmployeeId == employeeId
            && !TimeOffStatuses.NonBlocking.Contains(timeOff.Status)
            && (!currentTimeOffId.HasValue || timeOff.Id != currentTimeOffId.Value)
            && startTime < timeOff.EndTime
            && endTime > timeOff.StartTime);

        if (overlaps)
        {
            throw new ConflictException("Time off overlaps with existing time off.");
        }
    }

    private static string MapStatus(TimeOffStatus status)
    {
        return status switch
        {
            TimeOffStatus.PENDING => TimeOffStatuses.PENDING,
            TimeOffStatus.APPROVED => TimeOffStatuses.APPROVED,
            TimeOffStatus.REJECTED => TimeOffStatuses.REJECTED,
            _ => throw new BadRequestException("Status is not valid.")
        };
    }

    private void ApplyReviewedStatus(TimeOffEntity entity, string status, string? reviewNote)
    {
        if (entity.Status != TimeOffStatuses.PENDING)
        {
            throw new ConflictException("Time off request is already reviewed and cannot be changed.");
        }

        if (status == TimeOffStatuses.PENDING)
        {
            throw new BadRequestException("Time off request is already pending.");
        }

        var currentUserId = _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        entity.Status = status;
        entity.ReviewedByUserId = currentUserId;
        entity.ReviewedAt = DateTime.UtcNow;
        entity.ReviewNote = string.IsNullOrWhiteSpace(reviewNote) ? null : reviewNote.Trim();
    }

    private static DateTime ToUtcDateTime(DateTimeOffset dateTime)
    {
        return dateTime.UtcDateTime;
    }

    private async Task EnsureOwnerCanAccessShopAsync(int shopId)
    {
        if (!await _ownerAccessService.CanAccessShopAsync(shopId))
        {
            throw new ForbiddenException("Employee does not belong to your shop.");
        }
    }

    private async Task<int> GetOwnerShopIdOrThrowAsync()
    {
        var shopId = await _ownerAccessService.GetOwnerShopIdAsync();
        if (!shopId.HasValue)
        {
            throw new ForbiddenException("Current user does not have access to an owner shop.");
        }

        return shopId.Value;
    }
}
