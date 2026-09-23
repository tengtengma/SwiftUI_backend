using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SwiftUI_backend_demo.sources.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. 添加 Controllers 与 .NET 10 原生 OpenApi
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 2. 配置 DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. 配置 JWT 认证
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

// 4. 开发环境下挂载原生的 OpenAPI 文档与 Scalar 调试 UI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "SwiftUI Backend API (.NET 10)";
        options.WithTheme(ScalarTheme.Purple);
    });
}

app.UseAuthentication(); // 认证（你是谁）
app.UseAuthorization();  // 授权（你能做什么）

app.MapControllers();

app.Run();