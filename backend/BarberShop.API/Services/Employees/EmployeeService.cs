using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.Employees;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.SearchObjects.Employees;
using BarberShop.API.Services.Base;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Employees;

public class EmployeeService : BaseCRUDService<Employee, EmployeeDto, EmployeeSearchObject, EmployeeInsertRequest, EmployeeUpdateRequest>, IEmployeeService
{
    private readonly IOwnerAccessService _ownerAccessService;

    public EmployeeService(BarberShopDbContext dbContext, IOwnerAccessService ownerAccessService)
        : base(dbContext)
    {
        _ownerAccessService = ownerAccessService;
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var entity = await DbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == id);
        if (entity is null)
        {
            throw new NotFoundException("Employee does not exist.");
        }

        await BeforeDelete(entity);

        entity.Active = false;
        await DbContext.SaveChangesAsync();

        await AfterDelete(entity);

        return true;
    }

    protected override IQueryable<Employee> AddFilter(IQueryable<Employee> query, EmployeeSearchObject search)
    {
        if (search.ShopId.HasValue)
        {
            query = query.Where(employee => employee.ShopId == search.ShopId.Value);
        }

        if (search.UserId.HasValue)
        {
            query = query.Where(employee => employee.UserId == search.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.Position))
        {
            var position = search.Position.Trim().ToLower();
            query = query.Where(employee => employee.Position.ToLower().Contains(position));
        }

        if (search.Active.HasValue)
        {
            query = query.Where(employee => employee.Active == search.Active.Value);
        }

        return query;
    }

    protected override EmployeeDto MapToDto(Employee entity)
    {
        return new EmployeeDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            ShopId = entity.ShopId,
            Position = entity.Position,
            Bio = entity.Bio,
            EmploymentDate = entity.EmploymentDate,
            Active = entity.Active
        };
    }

    protected override Employee MapInsertToEntity(EmployeeInsertRequest request)
    {
        return new Employee
        {
            UserId = request.UserId,
            ShopId = request.ShopId,
            Position = request.Position.Trim(),
            Bio = request.Bio?.Trim(),
            EmploymentDate = request.EmploymentDate,
            Active = true
        };
    }

    protected override void MapUpdateToEntity(Employee entity, EmployeeUpdateRequest request)
    {
        entity.UserId = request.UserId;
        entity.ShopId = request.ShopId;
        entity.Position = request.Position.Trim();
        entity.Bio = request.Bio?.Trim();
        entity.EmploymentDate = request.EmploymentDate;
        entity.Active = request.Active;
    }

    protected override async Task BeforeInsert(Employee entity, EmployeeInsertRequest request)
    {
        await EnsureOwnerCanAccessShopAsync(request.ShopId);
        await ValidateEmployeeRequestAsync(request.UserId, request.ShopId, request.Position, request.EmploymentDate);
        await EnsureUserIsNotActiveEmployeeInShopAsync(request.UserId, request.ShopId);
        await EnsureEmployeeRoleForShopAsync(request.UserId, request.ShopId);
    }

    protected override async Task BeforeUpdate(Employee entity, EmployeeUpdateRequest request)
    {
        await EnsureOwnerCanAccessShopAsync(entity.ShopId);
        await EnsureOwnerCanAccessShopAsync(request.ShopId);
        await ValidateEmployeeRequestAsync(request.UserId, request.ShopId, request.Position, request.EmploymentDate);

        if (request.Active)
        {
            await EnsureUserIsNotActiveEmployeeInShopAsync(request.UserId, request.ShopId, entity.Id);
        }
    }

    protected override async Task BeforeDelete(Employee entity)
    {
        await EnsureOwnerCanAccessShopAsync(entity.ShopId);
    }

    private async Task ValidateEmployeeRequestAsync(int userId, int shopId, string position, DateOnly employmentDate)
    {
        if (!await DbContext.Users.AnyAsync(user => user.Id == userId))
        {
            throw new BadRequestException("User does not exist.");
        }

        if (!await DbContext.Shops.AnyAsync(shop => shop.Id == shopId))
        {
            throw new BadRequestException("Shop does not exist.");
        }

        if (string.IsNullOrWhiteSpace(position))
        {
            throw new BadRequestException("Position is required.");
        }

        if (employmentDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new BadRequestException("Employment date cannot be in the future.");
        }
    }

    private async Task EnsureUserIsNotActiveEmployeeInShopAsync(int userId, int shopId, int? currentEmployeeId = null)
    {
        var exists = await DbContext.Employees.AnyAsync(employee =>
            employee.UserId == userId
            && employee.ShopId == shopId
            && employee.Active
            && (!currentEmployeeId.HasValue || employee.Id != currentEmployeeId.Value));

        if (exists)
        {
            throw new ConflictException("User is already an active employee in this shop.");
        }
    }

    private async Task EnsureEmployeeRoleForShopAsync(int userId, int shopId)
    {
        var employeeRoleId = await DbContext.Roles
            .Where(role => role.Name.ToUpper() == RoleNames.EMPLOYEE)
            .Select(role => (int?)role.Id)
            .FirstOrDefaultAsync();

        if (!employeeRoleId.HasValue)
        {
            throw new BadRequestException("Required EMPLOYEE role does not exist.");
        }

        var userAlreadyHasEmployeeRoleInShop = await DbContext.UserRoles.AnyAsync(userRole =>
            userRole.UserId == userId
            && userRole.RoleId == employeeRoleId.Value
            && userRole.ShopId == shopId);

        if (userAlreadyHasEmployeeRoleInShop)
        {
            return;
        }

        DbContext.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = employeeRoleId.Value,
            ShopId = shopId
        });
    }

    private async Task EnsureOwnerCanAccessShopAsync(int shopId)
    {
        if (!await _ownerAccessService.CanAccessShopAsync(shopId))
        {
            throw new ForbiddenException("Employee does not belong to your shop.");
        }
    }
}
