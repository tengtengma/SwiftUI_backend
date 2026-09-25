# ==========================================
# 1. 编译构建阶段 (Build Stage)
# 使用官方 .NET 10.0 SDK 镜像
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 先复制 .csproj 文件还原依赖，利用 Docker 缓存
COPY ["SwiftUI_backend_demo.csproj", "./"]
RUN dotnet restore "SwiftUI_backend_demo.csproj"

# 复制其余源码并进行 Release 模式编译
COPY . .
RUN dotnet publish "SwiftUI_backend_demo.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# 2. 运行时运行阶段 (Runtime Stage)
# 使用轻量级的 .NET 10.0 ASP.NET Runtime 镜像
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# 从构建阶段复制编译产物
COPY --from=build /app/publish .

# 暴露容器内部端口（.NET 10 默认监听 8080 端口）
EXPOSE 8080

# 启动容器
ENTRYPOINT ["dotnet", "SwiftUI_backend_demo.dll"]