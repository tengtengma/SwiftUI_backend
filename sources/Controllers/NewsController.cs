using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SwiftUI_backend_demo.sources.Data;
using SwiftUI_backend_demo.sources.Models;
using SwiftUI_backend_demo.sources.Dtos;

namespace SwiftUI_backend_demo.sources.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly IDistributedCache _cache; // 注入 Redis 缓存接口

    // 你的 Mediastack Access Key
    private const string MediastackApiKey = "79ac869ea03fcf9b7dc9ed446db6ac8b";

    public NewsController(AppDbContext db, IHttpClientFactory httpClientFactory, IDistributedCache cache)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    /// <summary>
    /// 1. 后端主动调用 Mediastack 第三方 API 拉取最新新闻并存入数据库
    /// POST /api/news/fetch-external
    /// </summary>
    [HttpPost("fetch-external")]
    public async Task<ActionResult<ApiResponseDto<int>>> FetchAndSaveExternalNews()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // 构造请求 URL（可自定义参数，如 languages=de 或 en）
            var url = $"https://api.mediastack.com/v1/news?access_key={MediastackApiKey}";

            // 直接发起 GET 请求并反序列化为 DTO
            var externalData = await client.GetFromJsonAsync<NewsImportRequestDto>(url);

            if (externalData?.Data == null || !externalData.Data.Any())
            {
                return BadRequest(ApiResponseDto<int>.Fail(4000, "从第三方 API 未获取到有效数据。"));
            }

            // URL 自动去重检查
            var incomingUrls = externalData.Data.Select(d => d.Url).ToList();
            var existingUrls = await _db.Articles
                .Where(a => incomingUrls.Contains(a.Url))
                .Select(a => a.Url)
                .ToListAsync();

            // 过滤出未入库的新文章
            var newArticles = externalData.Data
                .Where(dto => !existingUrls.Contains(dto.Url))
                .Select(dto => new Article
                {
                    Author = dto.Author,
                    Title = dto.Title,
                    Description = dto.Description,
                    Url = dto.Url,
                    Source = dto.Source,
                    Image = dto.Image,
                    Category = dto.Category,
                    Language = dto.Language,
                    Country = dto.Country,
                    PublishedAt = DateTime.SpecifyKind(dto.PublishedAt, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (!newArticles.Any())
            {
                return Ok(ApiResponseDto<int>.Success(0, "第三方数据已是最新，无新文章落库。"));
            }

            // 存入数据库
            await _db.Articles.AddRangeAsync(newArticles);
            var insertedCount = await _db.SaveChangesAsync();

            return Ok(ApiResponseDto<int>.Success(insertedCount, $"成功从 Mediastack 拉取并入库 {insertedCount} 条新闻。"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<int>.Fail(5000, $"拉取第三方新闻失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 2. 提供给 SwiftUI 前端获取新闻列表(带Redis缓存)
    /// GET /api/news?page=1&pageSize=10
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<NewsResponseDto>>> GetNews(
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // 1. 生成唯一缓存 Key，例如: "news_category_all_page_1_size_10"
        var categoryKey = string.IsNullOrWhiteSpace(category) ? "all" : category.ToLower();
        var cacheKey = $"news_category_{categoryKey}_page_{page}_size_{pageSize}";

        // 2. 尝试从 Redis 读取
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            // 缓存命中！反序列化后直接返回
            var cachedResult = JsonSerializer.Deserialize<NewsResponseDto>(cachedData);
            if (cachedResult != null)
            {
                return Ok(ApiResponseDto<NewsResponseDto>.Success(cachedResult, "Fetch news success (From Redis Cache)"));
            }
        }

        // 3. Redis 缓存未命中，查 PostgreSQL 数据库
        var query = _db.Articles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(a => a.Category.ToLower() == category.ToLower());
        }

        var total = await query.CountAsync();

        var articles = await query
            .OrderByDescending(a => a.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleDto(
                a.Id,
                a.Author,
                a.Title,
                a.Description,
                a.Url,
                a.Source,
                a.Image,
                a.Category,
                a.Language,
                a.Country,
                a.PublishedAt
            ))
            .ToListAsync();

        var responseData = new NewsResponseDto(total, articles);

        // 4. 将数据库结果写入 Redis，设置过期时间（例如 10 分钟）
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // 10分钟后自动失效
        };

        var serializedData = JsonSerializer.Serialize(responseData);
        await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

        return Ok(ApiResponseDto<NewsResponseDto>.Success(responseData, "Fetch news success (From Database)"));
    }
}