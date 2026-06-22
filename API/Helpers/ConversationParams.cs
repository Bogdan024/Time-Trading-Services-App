using API.Entities;

namespace API.Helpers;

public class ConversationParams : PagingParams
{
    public ConversationType? Type { get; set; }
}
