namespace BarberShop.API.DTOs.Public;

public class PublicEmployeeDto
{
    public int EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public decimal? Rating { get; set; }
}
