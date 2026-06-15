using BarberShop.API.Data;
using BarberShop.API.DTOs.WorkingHours;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using BarberShop.API.SearchObjects.WorkingHours;
using BarberShop.API.Services.Base;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.WorkingHours;

public class WorkingHourService
    : BaseCRUDService<WorkingHour, WorkingHourDto, WorkingHourSearchObject, WorkingHourInsertRequest, WorkingHourUpdateRequest>,
        IWorkingHourService
{
    private readonly IOwnerAccessService _ownerAccessService;

    public WorkingHourService(BarberShopDbContext dbContext, IOwnerAccessService ownerAccessService)
        : base(dbContext)
    {
        _ownerAccessService = ownerAccessService;
    }

    public override async Task<PagedResult<WorkingHourDto>> GetAsync(WorkingHourSearchObject search)
    {
        var ownerShopId = await _ownerAccessService.GetOwnerShopIdAsync();
        if (!ownerShopId.HasValue)
        {
            throw new ForbiddenException("Current user does not have access to an owner shop.");
        }

        if (search.EmployeeId.HasValue)
        {
            var employee = await GetEmployeeAsync(search.EmployeeId.Value);
            await EnsureOwnerCanAccessShopAsync(employee.ShopId);
        }

        var page = Math.Max(0, search.Page);
        var pageSize = search.PageSize <= 0
            ? BaseSearchObject.DefaultPageSize
            : Math.Min(search.PageSize, BaseSearchObject.MaxPageSize);

        var query = DbContext.WorkingHours
            .AsNoTracking()
            .Where(workingHour => workingHour.Employee.ShopId == ownerShopId.Value);

        query = AddFilter(query, search)
            .OrderBy(workingHour => workingHour.DayOfWeek)
            .ThenBy(workingHour => workingHour.StartTime);

        var totalCount = search.IncludeTotalCount ? await query.CountAsync() : 0;

        query = search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize);

        var items = await query.ToListAsync();

        return new PagedResult<WorkingHourDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount
                ? (int)Math.Ceiling(totalCount / (double)pageSize)
                : 0
        };
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var entity = await DbContext.WorkingHours
            .Include(workingHour => workingHour.Employee)
            .FirstOrDefaultAsync(workingHour => workingHour.Id == id);

        if (entity is null)
        {
            throw new NotFoundException("Working hours do not exist.");
        }

        await BeforeDelete(entity);

        entity.Active = false;
        await DbContext.SaveChangesAsync();

        await AfterDelete(entity);

        return true;
    }

    protected override IQueryable<WorkingHour> AddFilter(IQueryable<WorkingHour> query, WorkingHourSearchObject search)
    {
        if (search.EmployeeId.HasValue)
        {
            query = query.Where(workingHour => workingHour.EmployeeId == search.EmployeeId.Value);
        }

        if (search.DayOfWeek.HasValue)
        {
            query = query.Where(workingHour => workingHour.DayOfWeek == search.DayOfWeek.Value);
        }

        if (search.Active.HasValue)
        {
            query = query.Where(workingHour => workingHour.Active == search.Active.Value);
        }

        return query;
    }

    protected override WorkingHourDto MapToDto(WorkingHour entity)
    {
        return new WorkingHourDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            DayOfWeek = entity.DayOfWeek,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Active = entity.Active
        };
    }

    protected override WorkingHour MapInsertToEntity(WorkingHourInsertRequest request)
    {
        return new WorkingHour
        {
            EmployeeId = request.EmployeeId,
            DayOfWeek = request.DayOfWeek ?? 0,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Active = true
        };
    }

    protected override void MapUpdateToEntity(WorkingHour entity, WorkingHourUpdateRequest request)
    {
        entity.EmployeeId = request.EmployeeId;
        entity.DayOfWeek = request.DayOfWeek ?? 0;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.Active = request.Active;
    }

    protected override async Task BeforeInsert(WorkingHour entity, WorkingHourInsertRequest request)
    {
        var employee = await GetActiveEmployeeAsync(request.EmployeeId);
        await EnsureOwnerCanAccessShopAsync(employee.ShopId);
        ValidateWorkingHourRequest(request.DayOfWeek, request.StartTime, request.EndTime);
        await EnsureNoOverlapAsync(request.EmployeeId, request.DayOfWeek!.Value, request.StartTime, request.EndTime);
    }

    protected override async Task BeforeUpdate(WorkingHour entity, WorkingHourUpdateRequest request)
    {
        var existingEmployee = await GetEmployeeAsync(entity.EmployeeId);
        await EnsureOwnerCanAccessShopAsync(existingEmployee.ShopId);

        var requestedEmployee = await GetActiveEmployeeAsync(request.EmployeeId);
        await EnsureOwnerCanAccessShopAsync(requestedEmployee.ShopId);

        ValidateWorkingHourRequest(request.DayOfWeek, request.StartTime, request.EndTime);
        await EnsureNoOverlapAsync(request.EmployeeId, request.DayOfWeek!.Value, request.StartTime, request.EndTime, entity.Id);
    }

    protected override async Task BeforeDelete(WorkingHour entity)
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

    private static void ValidateWorkingHourRequest(int? dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (!dayOfWeek.HasValue)
        {
            throw new BadRequestException("Day of week is required.");
        }

        if (dayOfWeek.Value < 0 || dayOfWeek.Value > 6)
        {
            throw new BadRequestException("Day of week must be between 0 and 6.");
        }

        if (startTime >= endTime)
        {
            throw new BadRequestException("Start time must be before end time.");
        }
    }

    private async Task EnsureNoOverlapAsync(
        int employeeId,
        int dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int? currentWorkingHourId = null)
    {
        var overlaps = await DbContext.WorkingHours.AnyAsync(workingHour =>
            workingHour.EmployeeId == employeeId
            && workingHour.DayOfWeek == dayOfWeek
            && workingHour.Active
            && (!currentWorkingHourId.HasValue || workingHour.Id != currentWorkingHourId.Value)
            && startTime < workingHour.EndTime
            && endTime > workingHour.StartTime);

        if (overlaps)
        {
            throw new ConflictException("Working hours overlap with existing working hours.");
        }
    }

    private async Task EnsureOwnerCanAccessShopAsync(int shopId)
    {
        if (!await _ownerAccessService.CanAccessShopAsync(shopId))
        {
            throw new ForbiddenException("Employee does not belong to your shop.");
        }
    }
}
