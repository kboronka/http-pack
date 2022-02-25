using HttpPack.Json;

namespace Example;

/// <summary>
///     Description of Post.
/// </summary>
public class Post : IJsonObject
{
    public Post(string title, string body, int userID)
    {
        Title = title;
        Body = body;
        UserID = userID;
    }

    public string Title { get; }
    public string Body { get; }
    public int UserID { get; }

    public JsonKeyValuePairs KeyValuePairs
    {
        get
        {
            var kvp = new JsonKeyValuePairs
            {
                {"title", Title},
                {"body", Body},
                {"userId", UserID}
            };
            return kvp;
        }
    }
}