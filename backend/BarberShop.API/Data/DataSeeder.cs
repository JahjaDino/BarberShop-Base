using BarberShop.API.Constants;
using BarberShop.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Data;

public class DataSeeder
{
    private readonly BarberShopDbContext _dbContext;

    public DataSeeder(BarberShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roleNames = new[]
        {
            RoleNames.CLIENT,
            RoleNames.EMPLOYEE,
            RoleNames.OWNER
        };

        var normalizedRoleNames = roleNames.Select(roleName => roleName.ToUpper()).ToList();

        var existingRoleNames = await _dbContext.Roles
            .Where(role => normalizedRoleNames.Contains(role.Name.ToUpper()))
            .Select(role => role.Name.ToUpper())
            .ToListAsync();

        var missingRoles = roleNames
            .Except(existingRoleNames)
            .Select(roleName => new Role { Name = roleName })
            .ToList();

        if (missingRoles.Count == 0)
        {
            return;
        }

        _dbContext.Roles.AddRange(missingRoles);
        await _dbContext.SaveChangesAsync();
    }
}
