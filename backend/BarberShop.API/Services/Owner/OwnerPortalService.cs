using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.Owner;
using BarberShop.API.DTOs.ServiceCategories;
using BarberShop.API.DTOs.Services;
using BarberShop.API.DTOs.TimeOff;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using BarberShop.API.SearchObjects.Inventory;
using BarberShop.API.SearchObjects.Owner;
using BarberShop.API.Services.Notifications;
using BarberShop.API.Services.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceEntity = BarberShop.API.Entities.Service;
using TimeOffEntity = BarberShop.API.Entities.TimeOff;

namespace BarberShop.API.Services.Owner;

public class OwnerPortalService : IOwnerPortalService
{
    private const string AvailabilityAvailable = "AVAILABLE";
    private const string AvailabilityBusy = "BUSY";
    private const string AvailabilityAbsent = "ABSENT";
    private const string ActiveStatus = "ACTIVE";

    private readonly BarberShopDbContext _dbContext;
    private readonly IOwnerAccessService _ownerAccessService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly INotificationService _notificationService;

    public OwnerPortalService(
        BarberShopDbContext dbContext,
        IOwnerAccessService ownerAccessService,
        ICurrentUserService currentUserService,
        IPasswordHasher<User> passwordHasher,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _ownerAccessService = ownerAccessService;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _notificationService = notificationService;
    }

    public async Task<OwnerDashboardDto> GetDashboardAsync()
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var weekStart = todayStart.AddDays(-6);

        var todayAppointmentsQuery = ShopAppointments(shopId)
            .Where(appointment => appointment.StartTime >= todayStart && appointment.StartTime < tomorrowStart);

        var todayAppointments = await todayAppointmentsQuery
            .OrderBy(appointment => appointment.StartTime)
            .Select(appointment => new OwnerTodayAppointmentDto
            {
                AppointmentId = appointment.Id,
                Time = appointment.StartTime.ToString("HH:mm"),
                ClientName = appointment.Client.User.FirstName + " " + appointment.Client.User.LastName,
                ServiceName = appointment.AppointmentServices
                    .OrderBy(appointmentService => appointmentService.Id)
                    .Select(appointmentService => appointmentService.Service.Name)
                    .FirstOrDefault() ?? string.Empty,
                EmployeeName = appointment.Employee.User.FirstName + " " + appointment.Employee.User.LastName,
                Status = appointment.Status.ToUpper()
            })
            .ToListAsync();

        var employeesToday = await GetEmployeesTodayAsync(shopId, todayStart, tomorrowStart, now);

        var todayRevenue = await todayAppointmentsQuery
            .Where(appointment => appointment.Status == AppointmentStatuses.COMPLETED)
            .SumAsync(appointment => appointment.TotalPrice);

        var averageWeeklyRating = await _dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.Appointment.Employee.ShopId == shopId && review.CreatedAt >= weekStart)
            .Select(review => (decimal?)review.Rating)
            .AverageAsync() ?? 0;

        var pendingPaymentsCount = await _dbContext.Payments
            .AsNoTracking()
            .CountAsync(payment => payment.Appointment.Employee.ShopId == shopId
                && payment.Status.ToUpper() == "PENDING");

        return new OwnerDashboardDto
        {
            Summary = new OwnerDashboardSummaryDto
            {
                TodayAppointmentsCount = await todayAppointmentsQuery.CountAsync(),
                ConfirmedTodayAppointmentsCount = await todayAppointmentsQuery.CountAsync(appointment => appointment.Status == AppointmentStatuses.CONFIRMED),
                ActiveEmployeesCount = await _dbContext.Employees.AsNoTracking().CountAsync(employee => employee.ShopId == shopId && employee.Active),
                AvailableEmployeesCount = employeesToday.Count(employee => employee.AvailabilityStatus == AvailabilityAvailable),
                TodayRevenue = todayRevenue,
                AverageWeeklyRating = Math.Round(averageWeeklyRating, 2)
            },
            TodayAppointments = todayAppointments,
            EmployeesToday = employeesToday,
            BusinessOverview = new OwnerBusinessOverviewDto
            {
                LowStockItemsCount = await _dbContext.InventoryItems.AsNoTracking().CountAsync(item => item.ShopId == shopId && item.Quantity <= item.MinimumQuantity),
                PendingPaymentsCount = pendingPaymentsCount
            }
        };
    }

    public async Task<PagedResult<OwnerAppointmentListItemDto>> GetAppointmentsAsync(OwnerAppointmentSearchObject search)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var query = ShopAppointments(shopId);

        if (search.Date.HasValue)
        {
            var start = search.Date.Value.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);
            query = query.Where(appointment => appointment.StartTime >= start && appointment.StartTime < end);
        }

        if (search.EmployeeId.HasValue)
        {
            query = query.Where(appointment => appointment.EmployeeId == search.EmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.Status))
        {
            var status = search.Status.Trim().ToUpperInvariant();
            query = query.Where(appointment => appointment.Status.ToUpper() == status);
        }

        query = query.OrderBy(appointment => appointment.StartTime);

        return await ToPagedResultAsync(query.Select(appointment => new OwnerAppointmentListItemDto
        {
            AppointmentId = appointment.Id,
            Date = DateOnly.FromDateTime(appointment.StartTime),
            Time = TimeOnly.FromDateTime(appointment.StartTime),
            ClientName = appointment.Client.User.FirstName + " " + appointment.Client.User.LastName,
            EmployeeName = appointment.Employee.User.FirstName + " " + appointment.Employee.User.LastName,
            ServiceName = appointment.AppointmentServices
                .OrderBy(appointmentService => appointmentService.Id)
                .Select(appointmentService => appointmentService.Service.Name)
                .FirstOrDefault() ?? string.Empty,
            DurationMinutes = appointment.AppointmentServices.Sum(appointmentService => appointmentService.DurationAtBooking),
            Price = appointment.TotalPrice,
            Status = appointment.Status.ToUpper()
        }), search);
    }

    public async Task<IReadOnlyCollection<OwnerEmployeeListItemDto>> GetEmployeesAsync()
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);

        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.ShopId == shopId)
            .OrderBy(employee => employee.User.FirstName)
            .ThenBy(employee => employee.User.LastName)
            .Select(employee => new OwnerEmployeeListItemDto
            {
                EmployeeId = employee.Id,
                FullName = employee.User.FirstName + " " + employee.User.LastName,
                Position = employee.Position,
                Email = employee.User.Email,
                PhoneNumber = employee.User.Phone,
                Active = employee.Active,
                AvailabilityStatus = _dbContext.TimeOffs.Any(timeOff => timeOff.EmployeeId == employee.Id && timeOff.StartTime < tomorrowStart && timeOff.EndTime > todayStart && timeOff.Status == TimeOffStatuses.APPROVED)
                    ? AvailabilityAbsent
                    : _dbContext.Appointments.Any(appointment => appointment.EmployeeId == employee.Id && AppointmentStatuses.Blocking.Contains(appointment.Status) && appointment.StartTime <= now && appointment.EndTime > now)
                        ? AvailabilityBusy
                        : AvailabilityAvailable,
                AppointmentsCountToday = _dbContext.Appointments.Count(appointment => appointment.EmployeeId == employee.Id && appointment.StartTime >= todayStart && appointment.StartTime < tomorrowStart),
                AverageRating = _dbContext.Reviews
                    .Where(review => review.Appointment.EmployeeId == employee.Id)
                    .Select(review => (decimal?)review.Rating)
                    .Average()
            })
            .ToListAsync();
    }

    public async Task<OwnerEmployeeListItemDto> CreateEmployeeAsync(OwnerEmployeeCreateRequest request)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _dbContext.Users.AnyAsync(user => user.Email.ToLower() == email))
        {
            throw new ConflictException("User with the same email already exists.");
        }

        var employeeRole = await _dbContext.Roles
            .FirstOrDefaultAsync(role => role.Name.ToUpper() == RoleNames.EMPLOYEE);
        if (employeeRole is null)
        {
            throw new BadRequestException("Required EMPLOYEE role does not exist.");
        }

        if (string.IsNullOrWhiteSpace(request.Position))
        {
            throw new BadRequestException("Position is required.");
        }

        var employmentDate = request.EmploymentDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (employmentDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new BadRequestException("Employment date cannot be in the future.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Phone = request.PhoneNumber.Trim(),
            Status = ActiveStatus,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var employee = new Employee
        {
            User = user,
            ShopId = shopId,
            Position = request.Position.Trim(),
            Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim(),
            EmploymentDate = employmentDate,
            Active = true
        };

        _dbContext.Users.Add(user);
        _dbContext.Employees.Add(employee);
        _dbContext.UserRoles.Add(new UserRole
        {
            User = user,
            Role = employeeRole,
            ShopId = shopId
        });

        await _dbContext.SaveChangesAsync();

        return new OwnerEmployeeListItemDto
        {
            EmployeeId = employee.Id,
            FullName = user.FirstName + " " + user.LastName,
            Position = employee.Position,
            Email = user.Email,
            PhoneNumber = user.Phone,
            Active = employee.Active,
            AvailabilityStatus = AvailabilityAvailable,
            AppointmentsCountToday = 0,
            AverageRating = null
        };
    }

    public async Task<OwnerEmployeeListItemDto> ActivateEmployeeAsync(int id)
    {
        return await SetEmployeeActiveStateAsync(id, true);
    }

    public async Task<OwnerEmployeeListItemDto> DeactivateEmployeeAsync(int id)
    {
        return await SetEmployeeActiveStateAsync(id, false);
    }

    public async Task<IReadOnlyCollection<ServiceCategoryDto>> GetServiceCategoriesAsync()
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();

        return await _dbContext.ServiceCategories
            .AsNoTracking()
            .Where(category => category.ShopId == shopId)
            .OrderBy(category => category.Name)
            .Select(category => new ServiceCategoryDto
            {
                Id = category.Id,
                ShopId = category.ShopId,
                Name = category.Name,
                Description = category.Description,
                Active = category.Active
            })
            .ToListAsync();
    }

    public async Task<ServiceCategoryDto> CreateServiceCategoryAsync(OwnerServiceCategoryCreateRequest request)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        var normalizedName = name.ToLower();
        var nameExists = await _dbContext.ServiceCategories.AnyAsync(category =>
            category.ShopId == shopId && category.Name.ToLower() == normalizedName);

        if (nameExists)
        {
            throw new ConflictException("Service category with the same name already exists.");
        }

        var category = new ServiceCategory
        {
            ShopId = shopId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Active = true
        };

        _dbContext.ServiceCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        return new ServiceCategoryDto
        {
            Id = category.Id,
            ShopId = category.ShopId,
            Name = category.Name,
            Description = category.Description,
            Active = category.Active
        };
    }

    public async Task<ServiceCategoryDto> UpdateServiceCategoryAsync(int id, OwnerServiceCategoryUpdateRequest request)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var category = await _dbContext.ServiceCategories
            .FirstOrDefaultAsync(currentCategory =>
                currentCategory.Id == id && currentCategory.ShopId == shopId);

        if (category is null)
        {
            throw new NotFoundException("Service category was not found.");
        }

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        var normalizedName = name.ToLower();
        var nameExists = await _dbContext.ServiceCategories.AnyAsync(currentCategory =>
            currentCategory.Id != id
            && currentCategory.ShopId == shopId
            && currentCategory.Name.ToLower() == normalizedName);

        if (nameExists)
        {
            throw new ConflictException("Service category with the same name already exists.");
        }

        category.Name = name;
        category.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        category.Active = request.Active;

        await _dbContext.SaveChangesAsync();

        return new ServiceCategoryDto
        {
            Id = category.Id,
            ShopId = category.ShopId,
            Name = category.Name,
            Description = category.Description,
            Active = category.Active
        };
    }

    public async Task<OwnerEmployeeDetailsDto?> GetEmployeeDetailsAsync(int id)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();

        var employee = await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.Id == id && employee.ShopId == shopId)
            .Select(employee => new OwnerEmployeeDetailsDto
            {
                EmployeeId = employee.Id,
                FullName = employee.User.FirstName + " " + employee.User.LastName,
                Position = employee.Position,
                Email = employee.User.Email,
                PhoneNumber = employee.User.Phone,
                Bio = employee.Bio,
                Active = employee.Active,
            })
            .FirstOrDefaultAsync();

        if (employee is null)
        {
            return null;
        }

        employee.WorkingHours = await _dbContext.WorkingHours
            .AsNoTracking()
            .Where(workingHour => workingHour.EmployeeId == id)
            .OrderBy(workingHour => workingHour.DayOfWeek)
            .ThenBy(workingHour => workingHour.StartTime)
            .Select(workingHour => new OwnerEmployeeWorkingHourDto
            {
                DayOfWeek = workingHour.DayOfWeek,
                StartTime = workingHour.StartTime.ToString("HH:mm"),
                EndTime = workingHour.EndTime.ToString("HH:mm"),
                Active = workingHour.Active
            })
            .ToListAsync();

        employee.TimeOff = await _dbContext.TimeOffs
            .AsNoTracking()
            .Where(timeOff => timeOff.EmployeeId == id)
            .OrderByDescending(timeOff => timeOff.StartTime)
            .Take(10)
            .Select(timeOff => new OwnerEmployeeTimeOffDto
            {
                StartTime = timeOff.StartTime,
                EndTime = timeOff.EndTime,
                Status = timeOff.Status,
                Reason = timeOff.Reason
            })
            .ToListAsync();

        employee.RecentAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.EmployeeId == id)
            .OrderByDescending(appointment => appointment.StartTime)
            .Take(10)
            .Select(appointment => new OwnerEmployeeRecentAppointmentDto
            {
                AppointmentId = appointment.Id,
                StartTime = appointment.StartTime,
                ClientName = appointment.Client.User.FirstName + " " + appointment.Client.User.LastName,
                ServiceName = appointment.AppointmentServices
                    .OrderBy(appointmentService => appointmentService.Id)
                    .Select(appointmentService => appointmentService.Service.Name)
                    .FirstOrDefault() ?? string.Empty,
                Status = appointment.Status
            })
            .ToListAsync();

        return employee;
    }

    public async Task<PagedResult<OwnerServiceListItemDto>> GetServicesAsync(OwnerServiceSearchObject search)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var query = _dbContext.Services.AsNoTracking().Where(service => service.ShopId == shopId);

        if (search.CategoryId.HasValue)
        {
            query = query.Where(service => service.CategoryId == search.CategoryId.Value);
        }

        query = query.OrderBy(service => service.Category.Name).ThenBy(service => service.Name);

        return await ToPagedResultAsync(query.Select(service => new OwnerServiceListItemDto
        {
            ServiceId = service.Id,
            CategoryId = service.CategoryId,
            Name = service.Name,
            Description = service.Description,
            CategoryName = service.Category.Name,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            BookingsCount = _dbContext.AppointmentServices.Count(appointmentService => appointmentService.ServiceId == service.Id),
            IsActive = service.Active
        }), search);
    }

    public async Task<ServiceDto> CreateServiceAsync(OwnerServiceCreateRequest request)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (request.DurationMinutes <= 0)
        {
            throw new BadRequestException("Duration must be greater than 0 minutes.");
        }

        if (request.DurationMinutes > 480)
        {
            throw new BadRequestException("Duration cannot be greater than 480 minutes.");
        }

        if (request.Price < 0)
        {
            throw new BadRequestException("Price cannot be negative.");
        }

        if (request.Price > 1000)
        {
            throw new BadRequestException("Price cannot be greater than 1000.");
        }

        var category = await _dbContext.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(currentCategory =>
                currentCategory.Id == request.CategoryId && currentCategory.ShopId == shopId);

        if (category is null)
        {
            throw new BadRequestException("Service category does not exist.");
        }

        if (!category.Active)
        {
            throw new BadRequestException("Service category is not active.");
        }

        var normalizedName = name.ToLower();
        var nameExists = await _dbContext.Services.AnyAsync(service =>
            service.ShopId == shopId && service.Name.ToLower() == normalizedName);

        if (nameExists)
        {
            throw new ConflictException("Service with the same name already exists in this shop.");
        }

        var service = new ServiceEntity
        {
            ShopId = shopId,
            CategoryId = request.CategoryId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            Active = true
        };

        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();

        return new ServiceDto
        {
            Id = service.Id,
            ShopId = service.ShopId,
            CategoryId = service.CategoryId,
            CategoryName = category.Name,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            Active = service.Active
        };
    }

    public async Task<ServiceDto> UpdateServiceAsync(int id, OwnerServiceUpdateRequest request)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var service = await _dbContext.Services
            .FirstOrDefaultAsync(currentService =>
                currentService.Id == id && currentService.ShopId == shopId);

        if (service is null)
        {
            throw new NotFoundException("Service was not found.");
        }

        var category = await _dbContext.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(currentCategory =>
                currentCategory.Id == request.CategoryId && currentCategory.ShopId == shopId);

        ValidateOwnerServiceRequest(request, category);

        var name = request.Name.Trim();
        var normalizedName = name.ToLower();
        var nameExists = await _dbContext.Services.AnyAsync(currentService =>
            currentService.Id != id
            && currentService.ShopId == shopId
            && currentService.Name.ToLower() == normalizedName);

        if (nameExists)
        {
            throw new ConflictException("Service with the same name already exists in this shop.");
        }

        service.CategoryId = request.CategoryId;
        service.Name = name;
        service.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        service.DurationMinutes = request.DurationMinutes;
        service.Price = request.Price;
        service.Active = request.Active;

        await _dbContext.SaveChangesAsync();

        return new ServiceDto
        {
            Id = service.Id,
            ShopId = service.ShopId,
            CategoryId = service.CategoryId,
            CategoryName = category!.Name,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            Active = service.Active
        };
    }

    public async Task<PagedResult<OwnerReviewListItemDto>> GetReviewsAsync(OwnerReviewSearchObject search)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var query = _dbContext.Reviews.AsNoTracking().Where(review => review.Appointment.Employee.ShopId == shopId);

        if (search.Rating.HasValue)
        {
            query = query.Where(review => review.Rating == search.Rating.Value);
        }

        if (search.EmployeeId.HasValue)
        {
            query = query.Where(review => review.Appointment.EmployeeId == search.EmployeeId.Value);
        }

        query = query.OrderByDescending(review => review.CreatedAt);

        return await ToPagedResultAsync(query.Select(review => new OwnerReviewListItemDto
        {
            ReviewId = review.Id,
            ClientName = review.Appointment.Client.User.FirstName + " " + review.Appointment.Client.User.LastName,
            EmployeeName = review.Appointment.Employee.User.FirstName + " " + review.Appointment.Employee.User.LastName,
            ServiceName = review.Appointment.AppointmentServices
                .OrderBy(appointmentService => appointmentService.Id)
                .Select(appointmentService => appointmentService.Service.Name)
                .FirstOrDefault() ?? string.Empty,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            AppointmentDate = review.Appointment.StartTime
        }), search);
    }

    public async Task<OwnerReviewSummaryDto> GetReviewSummaryAsync()
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var weekStart = DateTime.UtcNow.Date.AddDays(-6);
        var reviews = _dbContext.Reviews.AsNoTracking().Where(review => review.Appointment.Employee.ShopId == shopId);

        var topEmployee = await reviews
            .GroupBy(review => new
            {
                review.Appointment.EmployeeId,
                review.Appointment.Employee.User.FirstName,
                review.Appointment.Employee.User.LastName
            })
            .OrderByDescending(group => group.Average(review => review.Rating))
            .ThenByDescending(group => group.Count())
            .Select(group => group.Key.FirstName + " " + group.Key.LastName)
            .FirstOrDefaultAsync();

        return new OwnerReviewSummaryDto
        {
            AverageRating = Math.Round(await reviews.Select(review => (decimal?)review.Rating).AverageAsync() ?? 0, 2),
            TotalReviews = await reviews.CountAsync(),
            AverageRatingThisWeek = Math.Round(await reviews.Where(review => review.CreatedAt >= weekStart).Select(review => (decimal?)review.Rating).AverageAsync() ?? 0, 2),
            NewReviewsThisWeek = await reviews.CountAsync(review => review.CreatedAt >= weekStart),
            TopRatedEmployeeName = topEmployee
        };
    }

    public async Task<PagedResult<OwnerInventoryListItemDto>> GetInventoryAsync(InventoryItemSearchObject search)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var query = _dbContext.InventoryItems.AsNoTracking().Where(item => item.ShopId == shopId).OrderBy(item => item.Name);

        return await ToPagedResultAsync(query.Select(item => new OwnerInventoryListItemDto
        {
            ItemId = item.Id,
            Name = item.Name,
            Quantity = item.Quantity,
            MinimumQuantity = item.MinimumQuantity,
            Status = item.Quantity <= 0
                ? "OUT_OF_STOCK"
                : item.Quantity <= item.MinimumQuantity
                    ? "LOW_STOCK"
                    : "OK",
            UpdatedAt = item.LastUpdated,
            ReportedByEmployeeName = item.ReportedByEmployee == null
                ? null
                : item.ReportedByEmployee.User.FirstName + " " + item.ReportedByEmployee.User.LastName,
            ReportedAt = item.ReportedAt,
            ReportNote = item.ReportNote
        }), search);
    }

    public async Task<IReadOnlyCollection<OwnerInventoryListItemDto>> GetLowStockInventoryAsync()
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();

        return await _dbContext.InventoryItems
            .AsNoTracking()
            .Where(item => item.ShopId == shopId && item.Quantity <= item.MinimumQuantity)
            .OrderBy(item => item.Name)
            .Select(item => new OwnerInventoryListItemDto
            {
                ItemId = item.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                MinimumQuantity = item.MinimumQuantity,
                Status = item.Quantity <= 0
                    ? "OUT_OF_STOCK"
                    : item.Quantity <= item.MinimumQuantity
                        ? "LOW_STOCK"
                        : "OK",
                UpdatedAt = item.LastUpdated,
                ReportedByEmployeeName = item.ReportedByEmployee == null
                    ? null
                    : item.ReportedByEmployee.User.FirstName + " " + item.ReportedByEmployee.User.LastName,
                ReportedAt = item.ReportedAt,
                ReportNote = item.ReportNote
            })
            .ToListAsync();
    }

    public async Task<PagedResult<OwnerPaymentListItemDto>> GetPaymentsAsync(OwnerPaymentSearchObject search)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var query = ShopAppointments(shopId);

        if (search.Date.HasValue)
        {
            var start = search.Date.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(appointment => appointment.StartTime >= start && appointment.StartTime < start.AddDays(1));
        }

        if (search.From.HasValue)
        {
            query = query.Where(appointment => appointment.StartTime >= search.From.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (search.To.HasValue)
        {
            query = query.Where(appointment => appointment.StartTime < search.To.Value.ToDateTime(TimeOnly.MinValue).AddDays(1));
        }

        query = query.OrderByDescending(appointment => appointment.StartTime);

        return await ToPagedResultAsync(query.Select(appointment => new OwnerPaymentListItemDto
        {
            AppointmentId = appointment.Id,
            Date = DateOnly.FromDateTime(appointment.StartTime),
            Time = TimeOnly.FromDateTime(appointment.StartTime).ToString("HH:mm"),
            ClientName = appointment.Client.User.FirstName + " " + appointment.Client.User.LastName,
            EmployeeName = appointment.Employee.User.FirstName + " " + appointment.Employee.User.LastName,
            ServiceName = appointment.AppointmentServices
                .OrderBy(appointmentService => appointmentService.Id)
                .Select(appointmentService => appointmentService.Service.Name)
                .FirstOrDefault() ?? string.Empty,
            Amount = appointment.TotalPrice,
            PaymentMethod = _dbContext.Payments
                .Where(payment => payment.AppointmentId == appointment.Id)
                .Select(payment => payment.PaymentMethod)
                .FirstOrDefault() ?? PaymentMethods.CASH_ON_SITE,
            PaymentStatus = _dbContext.Payments
                .Where(payment => payment.AppointmentId == appointment.Id)
                .Select(payment => payment.Status)
                .FirstOrDefault() ?? PaymentStatuses.PENDING,
            AppointmentStatus = appointment.Status
        }), search);
    }

    public async Task<PagedResult<OwnerTimeOffRequestDto>> GetTimeOffRequestsAsync(OwnerTimeOffRequestSearchObject search)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var query = OwnerTimeOffRequests(shopId);

        if (search.Status.HasValue)
        {
            var status = MapTimeOffStatus(search.Status.Value);
            query = query.Where(timeOff => timeOff.Status == status);
        }

        if (search.EmployeeId.HasValue)
        {
            query = query.Where(timeOff => timeOff.EmployeeId == search.EmployeeId.Value);
        }

        if (search.From.HasValue)
        {
            var from = DateTime.SpecifyKind(search.From.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            query = query.Where(timeOff => timeOff.EndTime >= from);
        }

        if (search.To.HasValue)
        {
            var to = DateTime.SpecifyKind(search.To.Value.ToDateTime(TimeOnly.MinValue).AddDays(1), DateTimeKind.Utc);
            query = query.Where(timeOff => timeOff.StartTime < to);
        }

        query = query.OrderByDescending(timeOff => timeOff.StartTime);

        return await ToPagedResultAsync(query.Select(timeOff => new OwnerTimeOffRequestDto
        {
            TimeOffId = timeOff.Id,
            EmployeeId = timeOff.EmployeeId,
            EmployeeName = timeOff.Employee.User.FirstName + " " + timeOff.Employee.User.LastName,
            StartDate = timeOff.StartTime,
            EndDate = timeOff.EndTime,
            Reason = timeOff.Reason,
            Status = timeOff.Status,
            ReviewedAt = timeOff.ReviewedAt,
            ReviewNote = timeOff.ReviewNote
        }), search);
    }

    public async Task<OwnerTimeOffRequestDto> ApproveTimeOffRequestAsync(int id, OwnerTimeOffReviewRequest request)
    {
        return await ReviewTimeOffRequestAsync(id, TimeOffStatuses.APPROVED, request);
    }

    public async Task<OwnerTimeOffRequestDto> RejectTimeOffRequestAsync(int id, OwnerTimeOffReviewRequest request)
    {
        return await ReviewTimeOffRequestAsync(id, TimeOffStatuses.REJECTED, request);
    }

    public async Task<OwnerAnalyticsDto> GetAnalyticsAsync()
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var todayStart = DateTime.SpecifyKind(today.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var tomorrowStart = todayStart.AddDays(1);
        var weekStart = todayStart;
        var weekEnd = weekStart.AddDays(7);
        var monthStart = DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);
        var appointments = ShopAppointments(shopId);

        var workingHourIntervals = await _dbContext.WorkingHours
            .AsNoTracking()
            .Where(workingHour => workingHour.Employee.ShopId == shopId && workingHour.Active)
            .Select(workingHour => new
            {
                workingHour.StartTime,
                workingHour.EndTime
            })
            .ToListAsync();
        var workingMinutes = workingHourIntervals
            .Sum(workingHour => (workingHour.EndTime.ToTimeSpan() - workingHour.StartTime.ToTimeSpan()).TotalMinutes);
        var bookedMinutes = await _dbContext.AppointmentServices
            .AsNoTracking()
            .Where(appointmentService => appointmentService.Appointment.Employee.ShopId == shopId
                && appointmentService.Appointment.StartTime >= weekStart
                && appointmentService.Appointment.StartTime < weekEnd
                && (appointmentService.Appointment.Status.ToUpper() == AppointmentStatuses.CONFIRMED
                    || appointmentService.Appointment.Status.ToUpper() == AppointmentStatuses.COMPLETED))
            .SumAsync(appointmentService => (double?)appointmentService.DurationAtBooking) ?? 0;

        var appointmentClientIds = await appointments
            .Select(appointment => appointment.ClientId)
            .ToListAsync();
        var totalClients = appointmentClientIds.Distinct().Count();
        var returningClients = appointmentClientIds
            .GroupBy(clientId => clientId)
            .Count(group => group.Count() > 1);

        var topServices = await _dbContext.AppointmentServices
            .AsNoTracking()
            .Where(appointmentService => appointmentService.Appointment.Employee.ShopId == shopId)
            .GroupBy(appointmentService => new { appointmentService.ServiceId, appointmentService.Service.Name })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.Name)
            .Take(5)
            .Select(group => new OwnerAnalyticsServiceDto
            {
                ServiceId = group.Key.ServiceId,
                Name = group.Key.Name,
                AppointmentsCount = group.Count()
            })
            .ToListAsync();

        var mostActiveEmployees = await appointments
            .GroupBy(appointment => new
            {
                appointment.EmployeeId,
                appointment.Employee.User.FirstName,
                appointment.Employee.User.LastName
            })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.FirstName)
            .Take(5)
            .Select(group => new OwnerAnalyticsEmployeeDto
            {
                EmployeeId = group.Key.EmployeeId,
                FullName = group.Key.FirstName + " " + group.Key.LastName,
                AppointmentsCount = group.Count()
            })
            .ToListAsync();

        var popularServiceThisMonth = await _dbContext.AppointmentServices
            .AsNoTracking()
            .Where(appointmentService => appointmentService.Service.ShopId == shopId
                && appointmentService.Appointment.StartTime >= monthStart
                && appointmentService.Appointment.StartTime < nextMonthStart)
            .GroupBy(appointmentService => new { appointmentService.ServiceId, appointmentService.Service.Name })
            .OrderByDescending(group => group.Count())
            .Select(group => new { group.Key.Name, Count = group.Count() })
            .FirstOrDefaultAsync();

        return new OwnerAnalyticsDto
        {
            TotalAppointmentsCount = await appointments.CountAsync(),
            TodayAppointmentsCount = await appointments.CountAsync(appointment =>
                appointment.StartTime >= todayStart && appointment.StartTime < tomorrowStart),
            PendingAppointmentsCount = await appointments.CountAsync(appointment =>
                appointment.Status.ToUpper() == AppointmentStatuses.PENDING),
            ConfirmedAppointmentsCount = await appointments.CountAsync(appointment =>
                appointment.Status.ToUpper() == AppointmentStatuses.CONFIRMED),
            CompletedAppointmentsCount = await appointments.CountAsync(appointment =>
                appointment.Status.ToUpper() == AppointmentStatuses.COMPLETED),
            CancelledAppointmentsCount = await appointments.CountAsync(appointment =>
                appointment.Status.ToUpper() == AppointmentStatuses.CANCELLED
                || appointment.Status.ToUpper() == AppointmentStatuses.NO_SHOW),
            TodayRevenue = await appointments
                .Where(appointment => appointment.Status.ToUpper() == AppointmentStatuses.COMPLETED
                    && appointment.StartTime >= todayStart
                    && appointment.StartTime < tomorrowStart)
                .SumAsync(appointment => (decimal?)appointment.TotalPrice) ?? 0,
            MonthRevenue = await appointments
                .Where(appointment => appointment.Status.ToUpper() == AppointmentStatuses.COMPLETED
                    && appointment.StartTime >= monthStart
                    && appointment.StartTime < nextMonthStart)
                .SumAsync(appointment => (decimal?)appointment.TotalPrice) ?? 0,
            ActiveEmployeesCount = await _dbContext.Employees.CountAsync(employee =>
                employee.ShopId == shopId && employee.Active),
            ActiveServicesCount = await _dbContext.Services.CountAsync(service =>
                service.ShopId == shopId && service.Active),
            OccupancyRate = workingMinutes <= 0 ? 0 : Math.Round((decimal)(bookedMinutes / workingMinutes * 100), 2),
            ReturningClientsRate = totalClients == 0 ? 0 : Math.Round(returningClients / (decimal)totalClients * 100, 2),
            MostPopularService = popularServiceThisMonth?.Name ?? topServices.FirstOrDefault()?.Name,
            MostPopularServiceAppointmentsThisMonth = popularServiceThisMonth?.Count ?? 0,
            TopServices = topServices,
            MostActiveEmployees = mostActiveEmployees
        };
    }

    public async Task<OwnerShopSettingsDto> GetShopSettingsAsync()
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var shop = await _dbContext.Shops.AsNoTracking().FirstAsync(currentShop => currentShop.Id == shopId);
        return MapShopSettings(shop);
    }

    public async Task<OwnerShopSettingsDto> UpdateShopSettingsAsync(OwnerShopSettingsUpdateRequest request)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var shop = await _dbContext.Shops.FirstAsync(currentShop => currentShop.Id == shopId);

        shop.Name = request.Name.Trim();
        shop.Phone = request.PhoneNumber.Trim();
        shop.Email = request.Email.Trim();
        shop.Address = request.Address.Trim();
        shop.City = request.City.Trim();
        shop.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await _dbContext.SaveChangesAsync();

        return MapShopSettings(shop);
    }

    private IQueryable<Appointment> ShopAppointments(int shopId)
    {
        return _dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.Employee.ShopId == shopId);
    }

    private IQueryable<TimeOffEntity> OwnerTimeOffRequests(int shopId)
    {
        return _dbContext.TimeOffs
            .AsNoTracking()
            .Where(timeOff => timeOff.Employee.ShopId == shopId);
    }

    private async Task<OwnerTimeOffRequestDto> ReviewTimeOffRequestAsync(
        int id,
        string newStatus,
        OwnerTimeOffReviewRequest request)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var timeOff = await _dbContext.TimeOffs
            .Include(currentTimeOff => currentTimeOff.Employee)
            .ThenInclude(employee => employee.User)
            .FirstOrDefaultAsync(currentTimeOff => currentTimeOff.Id == id);
        if (timeOff is null)
        {
            throw new NotFoundException("Time off request does not exist.");
        }

        if (timeOff.Employee.ShopId != shopId)
        {
            throw new ForbiddenException("Time off request does not belong to your shop.");
        }

        if (timeOff.Status != TimeOffStatuses.PENDING)
        {
            throw new ConflictException("Only pending time off requests can be reviewed.");
        }

        var currentUserId = _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        timeOff.Status = newStatus;
        timeOff.ReviewedByUserId = currentUserId;
        timeOff.ReviewedAt = DateTime.UtcNow;
        timeOff.ReviewNote = string.IsNullOrWhiteSpace(request.ReviewNote) ? null : request.ReviewNote.Trim();

        await _dbContext.SaveChangesAsync();
        await _notificationService.CreateForTimeOffReviewedAsync(timeOff.Id, newStatus);

        return MapTimeOffRequest(timeOff);
    }

    private async Task<IReadOnlyCollection<OwnerEmployeeTodayDto>> GetEmployeesTodayAsync(
        int shopId,
        DateTime todayStart,
        DateTime tomorrowStart,
        DateTime now)
    {
        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.ShopId == shopId && employee.Active)
            .OrderBy(employee => employee.User.FirstName)
            .Select(employee => new OwnerEmployeeTodayDto
            {
                EmployeeId = employee.Id,
                FullName = employee.User.FirstName + " " + employee.User.LastName,
                AppointmentsCountToday = _dbContext.Appointments.Count(appointment => appointment.EmployeeId == employee.Id && appointment.StartTime >= todayStart && appointment.StartTime < tomorrowStart),
                AvailabilityStatus = _dbContext.TimeOffs.Any(timeOff => timeOff.EmployeeId == employee.Id && timeOff.StartTime < tomorrowStart && timeOff.EndTime > todayStart && timeOff.Status == TimeOffStatuses.APPROVED)
                    ? AvailabilityAbsent
                    : _dbContext.Appointments.Any(appointment => appointment.EmployeeId == employee.Id && AppointmentStatuses.Blocking.Contains(appointment.Status) && appointment.StartTime <= now && appointment.EndTime > now)
                        ? AvailabilityBusy
                        : AvailabilityAvailable
            })
            .ToListAsync();
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

    private static void ValidateOwnerServiceRequest(
        OwnerServiceUpdateRequest request,
        ServiceCategory? category)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (request.DurationMinutes <= 0)
        {
            throw new BadRequestException("Duration must be greater than 0 minutes.");
        }

        if (request.DurationMinutes > 480)
        {
            throw new BadRequestException("Duration cannot be greater than 480 minutes.");
        }

        if (request.Price < 0)
        {
            throw new BadRequestException("Price cannot be negative.");
        }

        if (request.Price > 1000)
        {
            throw new BadRequestException("Price cannot be greater than 1000.");
        }

        if (category is null)
        {
            throw new BadRequestException("Service category does not exist.");
        }

        if (!category.Active)
        {
            throw new BadRequestException("Service category is not active.");
        }
    }

    private async Task<OwnerEmployeeListItemDto> SetEmployeeActiveStateAsync(int id, bool active)
    {
        var shopId = await GetOwnerShopIdOrThrowAsync();
        var employee = await _dbContext.Employees
            .Include(currentEmployee => currentEmployee.User)
            .FirstOrDefaultAsync(currentEmployee =>
                currentEmployee.Id == id && currentEmployee.ShopId == shopId);

        if (employee is null)
        {
            throw new NotFoundException("Employee does not exist.");
        }

        employee.Active = active;
        await _dbContext.SaveChangesAsync();

        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        return new OwnerEmployeeListItemDto
        {
            EmployeeId = employee.Id,
            FullName = employee.User.FirstName + " " + employee.User.LastName,
            Position = employee.Position,
            Email = employee.User.Email,
            PhoneNumber = employee.User.Phone,
            Active = employee.Active,
            AvailabilityStatus = employee.Active ? AvailabilityAvailable : AvailabilityAbsent,
            AppointmentsCountToday = await _dbContext.Appointments.CountAsync(appointment =>
                appointment.EmployeeId == employee.Id
                && appointment.StartTime >= todayStart
                && appointment.StartTime < tomorrowStart),
            AverageRating = await _dbContext.Reviews
                .Where(review => review.Appointment.EmployeeId == employee.Id)
                .Select(review => (decimal?)review.Rating)
                .AverageAsync()
        };
    }

    private static string MapTimeOffStatus(TimeOffStatus status)
    {
        return status switch
        {
            TimeOffStatus.PENDING => TimeOffStatuses.PENDING,
            TimeOffStatus.APPROVED => TimeOffStatuses.APPROVED,
            TimeOffStatus.REJECTED => TimeOffStatuses.REJECTED,
            _ => throw new BadRequestException("Time off status is not valid.")
        };
    }

    private static OwnerTimeOffRequestDto MapTimeOffRequest(TimeOffEntity timeOff)
    {
        return new OwnerTimeOffRequestDto
        {
            TimeOffId = timeOff.Id,
            EmployeeId = timeOff.EmployeeId,
            EmployeeName = timeOff.Employee.User.FirstName + " " + timeOff.Employee.User.LastName,
            StartDate = timeOff.StartTime,
            EndDate = timeOff.EndTime,
            Reason = timeOff.Reason,
            Status = timeOff.Status,
            ReviewedAt = timeOff.ReviewedAt,
            ReviewNote = timeOff.ReviewNote
        };
    }

    private static async Task<PagedResult<T>> ToPagedResultAsync<T>(IQueryable<T> query, BaseSearchObject search)
    {
        var page = Math.Max(0, search.Page);
        var pageSize = search.PageSize <= 0
            ? BaseSearchObject.DefaultPageSize
            : Math.Min(search.PageSize, BaseSearchObject.MaxPageSize);

        var totalCount = search.IncludeTotalCount ? await query.CountAsync() : 0;
        var items = await (search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize)).ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0
        };
    }

    private static OwnerShopSettingsDto MapShopSettings(Shop shop)
    {
        return new OwnerShopSettingsDto
        {
            Name = shop.Name,
            PhoneNumber = shop.Phone,
            Email = shop.Email,
            Address = shop.Address,
            City = shop.City,
            Description = shop.Description
        };
    }
}
