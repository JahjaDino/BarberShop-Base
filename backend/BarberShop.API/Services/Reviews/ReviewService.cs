using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.Reviews;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using BarberShop.API.SearchObjects.Reviews;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Reviews;

public class ReviewService : IReviewService
{
    private const int MaxCommentLength = 1000;

    private readonly BarberShopDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOwnerAccessService _ownerAccessService;

    public ReviewService(
        BarberShopDbContext dbContext,
        ICurrentUserService currentUserService,
        IOwnerAccessService ownerAccessService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _ownerAccessService = ownerAccessService;
    }

    public async Task<ReviewDto> CreateAsync(ReviewInsertRequest request)
    {
        ValidateReviewRequest(request);

        var clientId = await GetCurrentClientIdAsync();

        var appointment = await _dbContext.Appointments
            .AsNoTracking()
            .Where(currentAppointment => currentAppointment.Id == request.AppointmentId)
            .Select(currentAppointment => new
            {
                currentAppointment.Id,
                currentAppointment.ClientId,
                currentAppointment.Status
            })
            .FirstOrDefaultAsync();
        if (appointment is null)
        {
            throw new BadRequestException("Appointment does not exist.");
        }

        if (appointment.ClientId != clientId)
        {
            throw new ForbiddenException("You can review only your own appointment.");
        }

        if (!string.Equals(appointment.Status, AppointmentStatuses.COMPLETED, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Only completed appointments can be reviewed.");
        }

        var reviewExists = await _dbContext.Reviews.AnyAsync(review => review.AppointmentId == appointment.Id);
        if (reviewExists)
        {
            throw new BadRequestException("Review already exists for this appointment.");
        }

        var reviewEntity = new Review
        {
            AppointmentId = appointment.Id,
            Rating = request.Rating,
            Comment = NormalizeComment(request.Comment),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(reviewEntity);
        await _dbContext.SaveChangesAsync();

        return await GetReviewDtoByIdAsync(reviewEntity.Id)
            ?? throw new InvalidOperationException("Created review could not be loaded.");
    }

    public async Task<PagedResult<ReviewDto>> GetMineAsync(ReviewSearchObject search)
    {
        var clientId = await GetCurrentClientIdAsync();

        var query = _dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.Appointment.ClientId == clientId);

        query = AddFilter(query, search);

        return await ToPagedResultAsync(query, search);
    }

    public async Task<IReadOnlyCollection<PendingReviewDto>> GetPendingAsync()
    {
        var clientId = await GetCurrentClientIdAsync();

        return await _dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.ClientId == clientId
                && appointment.Status == AppointmentStatuses.COMPLETED
                && !_dbContext.Reviews.Any(review => review.AppointmentId == appointment.Id))
            .OrderByDescending(appointment => appointment.StartTime)
            .Select(appointment => new PendingReviewDto
            {
                AppointmentId = appointment.Id,
                ServiceName = appointment.AppointmentServices
                    .OrderBy(appointmentService => appointmentService.Id)
                    .Select(appointmentService => appointmentService.Service.Name)
                    .FirstOrDefault() ?? string.Empty,
                EmployeeName = appointment.Employee.User.FirstName + " " + appointment.Employee.User.LastName,
                AppointmentDate = appointment.StartTime
            })
            .ToListAsync();
    }

    public async Task<PagedResult<ReviewDto>> GetAsync(ReviewSearchObject search)
    {
        var ownerShopId = await _ownerAccessService.GetOwnerShopIdAsync();
        if (!ownerShopId.HasValue)
        {
            throw new ForbiddenException("You do not have access to reviews.");
        }

        var query = _dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.Appointment.Employee.ShopId == ownerShopId.Value);

        query = AddFilter(query, search);

        return await ToPagedResultAsync(query, search);
    }

    public async Task<ReviewDto?> GetByIdAsync(int id)
    {
        var access = await _dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.Id == id)
            .Select(review => new
            {
                review.Appointment.ClientId,
                review.Appointment.Employee.ShopId
            })
            .FirstOrDefaultAsync();
        if (access is null)
        {
            return null;
        }

        await EnsureCanAccessReviewAsync(access.ClientId, access.ShopId);

        return await GetReviewDtoByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var review = await _dbContext.Reviews
            .Include(currentReview => currentReview.Appointment)
            .FirstOrDefaultAsync(currentReview => currentReview.Id == id);
        if (review is null)
        {
            return false;
        }

        var clientId = await GetCurrentClientIdAsync();
        if (review.Appointment.ClientId != clientId)
        {
            throw new ForbiddenException("You can delete only your own review.");
        }

        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task EnsureCanAccessReviewAsync(int reviewClientId, int reviewShopId)
    {
        if (_currentUserService.Roles.Contains(RoleNames.CLIENT, StringComparer.OrdinalIgnoreCase))
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == reviewClientId)
            {
                return;
            }
        }

        if (_currentUserService.Roles.Contains(RoleNames.OWNER, StringComparer.OrdinalIgnoreCase)
            && await _ownerAccessService.CanAccessShopAsync(reviewShopId))
        {
            return;
        }

        throw new ForbiddenException("You do not have access to this review.");
    }

    private IQueryable<Review> AddFilter(IQueryable<Review> query, ReviewSearchObject search)
    {
        if (search.AppointmentId.HasValue)
        {
            query = query.Where(review => review.AppointmentId == search.AppointmentId.Value);
        }

        if (search.Rating.HasValue)
        {
            query = query.Where(review => review.Rating == search.Rating.Value);
        }

        if (search.DateFrom.HasValue)
        {
            var dateFrom = ToUtcDateTime(search.DateFrom.Value);
            query = query.Where(review => review.CreatedAt >= dateFrom);
        }

        if (search.DateTo.HasValue)
        {
            var dateTo = ToUtcDateTime(search.DateTo.Value);
            query = query.Where(review => review.CreatedAt <= dateTo);
        }

        return query.OrderByDescending(review => review.CreatedAt);
    }

    private static async Task<PagedResult<ReviewDto>> ToPagedResultAsync(IQueryable<Review> query, ReviewSearchObject search)
    {
        var page = Math.Max(0, search.Page);
        var pageSize = NormalizePageSize(search.PageSize);

        var totalCount = search.IncludeTotalCount
            ? await query.CountAsync()
            : 0;

        query = search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize);

        var items = await ProjectToDto(query).ToListAsync();

        return new PagedResult<ReviewDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount
                ? (int)Math.Ceiling(totalCount / (double)pageSize)
                : 0
        };
    }

    private async Task<ReviewDto?> GetReviewDtoByIdAsync(int id)
    {
        return await ProjectToDto(_dbContext.Reviews.AsNoTracking().Where(review => review.Id == id))
            .FirstOrDefaultAsync();
    }

    private static IQueryable<ReviewDto> ProjectToDto(IQueryable<Review> query)
    {
        return query.Select(review => new ReviewDto
        {
            Id = review.Id,
            AppointmentId = review.AppointmentId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            ServiceName = review.Appointment.AppointmentServices
                .OrderBy(appointmentService => appointmentService.Id)
                .Select(appointmentService => appointmentService.Service.Name)
                .FirstOrDefault() ?? string.Empty,
            ClientName = review.Appointment.Client.User.FirstName + " " + review.Appointment.Client.User.LastName,
            EmployeeName = review.Appointment.Employee.User.FirstName + " " + review.Appointment.Employee.User.LastName,
            AppointmentStartTime = review.Appointment.StartTime
        });
    }

    private async Task<int> GetCurrentClientIdAsync()
    {
        var currentUserId = _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var clientId = await _dbContext.Clients
            .AsNoTracking()
            .Where(client => client.UserId == currentUserId)
            .Select(client => (int?)client.Id)
            .FirstOrDefaultAsync();

        if (!clientId.HasValue)
        {
            throw new ForbiddenException("Current user does not have a client profile.");
        }

        return clientId.Value;
    }

    private static void ValidateReviewRequest(ReviewInsertRequest request)
    {
        if (request is null)
        {
            throw new BadRequestException("Review request is required.");
        }

        if (request.AppointmentId <= 0)
        {
            throw new BadRequestException("Appointment is required.");
        }

        if (request.Rating is < 1 or > 5)
        {
            throw new BadRequestException("Rating must be between 1 and 5.");
        }

        if (request.Comment is not null && request.Comment.Length > MaxCommentLength)
        {
            throw new BadRequestException($"Comment cannot be longer than {MaxCommentLength} characters.");
        }
    }

    private static string? NormalizeComment(string? comment)
    {
        var normalizedComment = comment?.Trim();

        return string.IsNullOrWhiteSpace(normalizedComment) ? null : normalizedComment;
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
        {
            return BaseSearchObject.DefaultPageSize;
        }

        return Math.Min(pageSize, BaseSearchObject.MaxPageSize);
    }

    private static DateTime ToUtcDateTime(DateTimeOffset dateTime)
    {
        return dateTime.UtcDateTime;
    }
}
