using System.Text.Json.Serialization;

namespace NotionColumnUpdater.Abstractions;

public class NotionResponse
{
    [JsonPropertyName("object")] public string Object { get; set; }

    [JsonPropertyName("results")] public List<Page> Results { get; set; }

    [JsonPropertyName("next_cursor")] public string NextCursor { get; set; }

    [JsonPropertyName("has_more")] public bool HasMore { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("page_or_database")] public object PageOrDatabase { get; set; }

    [JsonPropertyName("request_id")] public string RequestId { get; set; }
}

public class Page
{
    [JsonPropertyName("object")] public string Object { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("created_time")] public DateTime CreatedTime { get; set; }

    [JsonPropertyName("last_edited_time")] public DateTime LastEditedTime { get; set; }

    [JsonPropertyName("created_by")] public User CreatedBy { get; set; }

    [JsonPropertyName("last_edited_by")] public User LastEditedBy { get; set; }

    [JsonPropertyName("cover")] public object Cover { get; set; }

    [JsonPropertyName("icon")] public object Icon { get; set; }

    [JsonPropertyName("parent")] public Parent Parent { get; set; }

    [JsonPropertyName("archived")] public bool Archived { get; set; }

    [JsonPropertyName("in_trash")] public bool InTrash { get; set; }

    [JsonPropertyName("properties")] public Dictionary<string, Property> Properties { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; }

    [JsonPropertyName("public_url")] public object PublicUrl { get; set; }
}

public class User
{
    [JsonPropertyName("object")] public string Object { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }
}

public class Parent
{
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("database_id")] public string DatabaseId { get; set; }
}

public class Property
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("relation")] public List<Relation> Relation { get; set; }
}

public class Relation
{
    [JsonPropertyName("id")] public string Id { get; set; }
}