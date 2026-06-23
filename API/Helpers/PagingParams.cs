namespace API.Helpers;

public class PagingParams
{
    private const int MaxPageSize = 50;
    private int pageNumber = 1;
    private int pageSize = 9;

    public int PageNumber
    {
        get => pageNumber;
        set => pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => pageSize;
        set => pageSize = Math.Clamp(value, 1, MaxPageSize);
    }
}
