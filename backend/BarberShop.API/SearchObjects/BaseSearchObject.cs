namespace BarberShop.API.SearchObjects;

public class BaseSearchObject
{
    public const int DefaultPageSize = 10;

    public const int MaxPageSize = 100;

    public int Page { get; set; } = 0;

    public int PageSize { get; set; } = DefaultPageSize;

    public string? OrderBy { get; set; }

    public string? SortDirection { get; set; }

    public bool IncludeTotalCount { get; set; } = true;

    public bool GetAll { get; set; } = false;
}
