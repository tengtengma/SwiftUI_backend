using System.Text.Json.Serialization;

namespace SwiftUI_backend_demo.sources.Dtos;

public class NewsImportRequestDto
{
    [JsonPropertyName("data")]
    public List<NewsItemDto> Data { get; set; } = new();
}

public class NewsItemDto
{
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }
}

// --- Return DTO to Frontend ---
public record ArticleDto(
    int Id,
    string? Author,
    string Title,
    string Description,
    string Url,
    string Source,
    string? Image,
    string Category,
    string Language,
    string Country,
    DateTime PublishedAt
);

public record NewsResponseDto(
    int Total,
    List<ArticleDto> Articles
);