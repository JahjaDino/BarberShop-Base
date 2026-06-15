using BarberShop.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Data;

public class BarberShopDbContext : DbContext
{
    public BarberShopDbContext(DbContextOptions<BarberShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();
    public DbSet<AppointmentStatusHistory> AppointmentStatusHistories => Set<AppointmentStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<WorkingHour> WorkingHours => Set<WorkingHour>();
    public DbSet<TimeOff> TimeOffs => Set<TimeOff>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<ClientFavoriteService> ClientFavoriteServices => Set<ClientFavoriteService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(role => role.Name)
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasOne(userRole => userRole.Shop)
            .WithMany()
            .HasForeignKey(userRole => userRole.ShopId)
            .IsRequired(false);

        modelBuilder.Entity<UserRole>()
            .HasIndex(userRole => new { userRole.UserId, userRole.RoleId })
            .IsUnique()
            .HasFilter("\"ShopId\" IS NULL");

        modelBuilder.Entity<UserRole>()
            .HasIndex(userRole => new { userRole.UserId, userRole.RoleId, userRole.ShopId })
            .IsUnique()
            .HasFilter("\"ShopId\" IS NOT NULL");

        modelBuilder.Entity<Client>()
            .HasIndex(client => client.UserId)
            .IsUnique();

        modelBuilder.Entity<ClientFavoriteService>()
            .HasIndex(favoriteService => new { favoriteService.ClientId, favoriteService.ServiceId })
            .IsUnique();

        modelBuilder.Entity<ServiceCategory>()
            .HasIndex(category => category.ShopId);

        modelBuilder.Entity<ServiceCategory>()
            .HasIndex(category => new { category.ShopId, category.Active });

        modelBuilder.Entity<ServiceCategory>()
            .HasIndex(category => new { category.ShopId, category.Name })
            .IsUnique()
            .HasFilter("\"Active\" = TRUE");

        modelBuilder.Entity<Service>()
            .HasIndex(service => service.ShopId);

        modelBuilder.Entity<Service>()
            .HasIndex(service => service.CategoryId);

        modelBuilder.Entity<Service>()
            .HasIndex(service => new { service.ShopId, service.Active });

        modelBuilder.Entity<Service>()
            .HasIndex(service => new { service.ShopId, service.CategoryId, service.Active });

        modelBuilder.Entity<Service>()
            .HasIndex(service => new { service.ShopId, service.CategoryId, service.Name })
            .IsUnique()
            .HasFilter("\"Active\" = TRUE");

        modelBuilder.Entity<Service>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_Services_Price_NonNegative", "\"Price\" >= 0");
                table.HasCheckConstraint("CK_Services_DurationMinutes_Positive", "\"DurationMinutes\" > 0");
            });

        modelBuilder.Entity<Employee>()
            .HasIndex(employee => employee.ShopId);

        modelBuilder.Entity<Employee>()
            .HasIndex(employee => employee.UserId)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(employee => new { employee.ShopId, employee.Active });

        modelBuilder.Entity<Employee>()
            .HasIndex(employee => new { employee.ShopId, employee.UserId })
            .IsUnique();

        modelBuilder.Entity<WorkingHour>()
            .HasIndex(workingHour => workingHour.EmployeeId);

        modelBuilder.Entity<WorkingHour>()
            .HasIndex(workingHour => new { workingHour.EmployeeId, workingHour.DayOfWeek });

        modelBuilder.Entity<WorkingHour>()
            .HasIndex(workingHour => new { workingHour.EmployeeId, workingHour.DayOfWeek, workingHour.Active });

        modelBuilder.Entity<WorkingHour>()
            .HasIndex(workingHour => new
            {
                workingHour.EmployeeId,
                workingHour.DayOfWeek,
                workingHour.StartTime,
                workingHour.EndTime
            })
            .IsUnique();

        modelBuilder.Entity<WorkingHour>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_WorkingHours_StartBeforeEnd", "\"StartTime\" < \"EndTime\"");
            });

        modelBuilder.Entity<TimeOff>()
            .HasOne(timeOff => timeOff.ReviewedByUser)
            .WithMany()
            .HasForeignKey(timeOff => timeOff.ReviewedByUserId)
            .IsRequired(false);

        modelBuilder.Entity<TimeOff>()
            .HasIndex(timeOff => timeOff.EmployeeId);

        modelBuilder.Entity<TimeOff>()
            .HasIndex(timeOff => new { timeOff.EmployeeId, timeOff.StartTime, timeOff.EndTime });

        modelBuilder.Entity<TimeOff>()
            .HasIndex(timeOff => new { timeOff.EmployeeId, timeOff.Status });

        modelBuilder.Entity<TimeOff>()
            .HasIndex(timeOff => new { timeOff.EmployeeId, timeOff.StartTime, timeOff.EndTime })
            .IsUnique()
            .HasFilter("\"Status\" NOT IN ('CANCELLED', 'REJECTED')");

        modelBuilder.Entity<TimeOff>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_TimeOffs_StartBeforeEnd", "\"StartTime\" < \"EndTime\"");
            });

        modelBuilder.Entity<Review>()
            .HasIndex(review => review.AppointmentId)
            .IsUnique();

        modelBuilder.Entity<Review>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_Reviews_Rating_Range", "\"Rating\" BETWEEN 1 AND 5");
            });

        modelBuilder.Entity<InventoryItem>()
            .HasIndex(item => new { item.ShopId, item.Name })
            .IsUnique();

        modelBuilder.Entity<InventoryItem>()
            .HasOne(item => item.ReportedByEmployee)
            .WithMany()
            .HasForeignKey(item => item.ReportedByEmployeeId)
            .IsRequired(false);

        modelBuilder.Entity<InventoryItem>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_InventoryItems_Quantity_NonNegative", "\"Quantity\" >= 0");
                table.HasCheckConstraint("CK_InventoryItems_MinimumQuantity_NonNegative", "\"MinimumQuantity\" >= 0");
            });

        modelBuilder.Entity<Payment>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_Payments_Amount_NonNegative", "\"Amount\" >= 0");
            });

        modelBuilder.Entity<Notification>()
            .HasIndex(notification => new { notification.UserId, notification.Status });

        modelBuilder.Entity<Notification>()
            .HasIndex(notification => new { notification.UserId, notification.SentAt });

        modelBuilder.Entity<Appointment>()
            .HasIndex(appointment => appointment.ClientId);

        modelBuilder.Entity<Appointment>()
            .HasIndex(appointment => appointment.EmployeeId);

        modelBuilder.Entity<Appointment>()
            .HasIndex(appointment => new
            {
                appointment.EmployeeId,
                appointment.StartTime,
                appointment.EndTime
            });

        modelBuilder.Entity<Appointment>()
            .HasIndex(appointment => appointment.Status);

        modelBuilder.Entity<Appointment>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_Appointments_StartBeforeEnd", "\"StartTime\" < \"EndTime\"");
                table.HasCheckConstraint("CK_Appointments_TotalPrice_NonNegative", "\"TotalPrice\" >= 0");
            });

        modelBuilder.Entity<AppointmentService>()
            .HasIndex(appointmentService => appointmentService.AppointmentId);

        modelBuilder.Entity<AppointmentService>()
            .HasIndex(appointmentService => appointmentService.ServiceId);

        modelBuilder.Entity<AppointmentService>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_AppointmentServices_PriceAtBooking_NonNegative", "\"PriceAtBooking\" >= 0");
                table.HasCheckConstraint("CK_AppointmentServices_DurationAtBooking_Positive", "\"DurationAtBooking\" > 0");
            });

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(resetToken => resetToken.TokenHash)
            .IsUnique();
    }
}
