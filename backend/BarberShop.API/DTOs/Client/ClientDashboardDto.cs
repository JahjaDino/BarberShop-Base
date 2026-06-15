namespace BarberShop.API.DTOs.Client;

public class ClientDashboardDto
{
    public ClientAppointmentCardDto? NextAppointment { get; set; }

    public IReadOnlyCollection<ClientServiceCardDto> PopularServices { get; set; } = [];

    public int UnreadNotificationsCount { get; set; }
}
