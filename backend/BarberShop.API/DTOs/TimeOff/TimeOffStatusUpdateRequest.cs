namespace BarberShop.API.DTOs.TimeOff;

public class TimeOffStatusUpdateRequest
{
    public TimeOffStatus Status { get; set; }

    public string? ReviewNote { get; set; }
}
