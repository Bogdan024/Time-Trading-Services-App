using API.Entities;

namespace API.Helpers;

public class TaskParams : PagingParams
{
    public string? CurrentMemberId { get; set; }
    public int? ServiceCategoryId { get; set; }
    public TaskLocationMode? LocationMode { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
    public int? MinCredits { get; set; }
    public int? MaxCredits { get; set; }
    public bool DueSoon { get; set; }
    public int? MinPosterRating { get; set; }
    public string OrderBy { get; set; } = "newest";
}

