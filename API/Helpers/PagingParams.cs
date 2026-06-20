namespace API.Helpers;

public class PagingParams
{
    private const int MaxPageSize = 50;
    private int pageSize = 9;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => pageSize;
        set => pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
