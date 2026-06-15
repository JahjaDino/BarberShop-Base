namespace BarberShop.API.DTOs.Appointments;

public class AvailableSlotDto
{
    public string StartTime { get; set; } = string.Empty;

    public string EndTime { get; set; } = string.Empty;

    public bool Available { get; set; }
}
