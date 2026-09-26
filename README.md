# SwiftUI_backend

本项目是一个基于 **.NET 10** 构建的高性能 Web API 后端服务，专为移动端（如 SwiftUI 应用）设计。系统采用现代云原生架构，集成了 **Supabase PostgreSQL** 持久化存储、**Upstash Redis** 高速分布式缓存，并通过 **AWS ECS (Fargate)** 实现容器化托管，搭配 **GitHub Actions** 达成全自动化 CI/CD 流水线。

---

## 🚀 核心技术栈

* **后端框架**: .NET 10 Web API
* **API 文档与调试**: 原生 OpenAPI + **Scalar UI** (`/scalar/v1`)
* **主数据库**: **Supabase PostgreSQL**（关系型数据持久化）
* **缓存服务**: **Upstash Redis**（通过 SSL 安全连接实现高性能缓存）
* **容器化与托管**: **AWS ECS (Fargate)** 托管在悉尼大区 (`ap-southeast-2`)，使用 ECR 镜像仓库 (`swiftui-backend-demo`)
* **CI/CD 自动化**: **GitHub Actions**（实现代码构建、Docker 镜像推送及 ECS 自动滚动更新）

---

## 📂 项目架构与目录

```text
SwiftUI_backend_demo/
├── Controllers/         # API 控制器（如 NewsController 等业务接口）
├── Dockerfile           # 多阶段容器打包配置文件
├── Program.cs           # 应用入口、DI 注入、中间件与配置保底
├── cicd.yml             # GitHub Actions 自动化部署流水线配置
└── appsettings.json     # 本地开发配置文件

```

---

## ⚙️ 环境变量与配置说明

生产环境中通过 **AWS ECS Task Definition** 注入以下环境变量，确保敏感信息与代码解耦：

| 环境变量名 | 说明 | 示例 |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | Supabase PostgreSQL 数据库连接串 | `postgresql://postgres:password@db.xxx.supabase.co:5432/postgres` |
| `ConnectionStrings__Redis` | Upstash Redis 加密连接串（带 SSL） | `eager-basilisk-xxxx.upstash.io:6379,password=xxx,ssl=true` |

---

## 🔄 CI/CD 自动化部署流程

项目配置了完整的 GitHub Actions 流水线，每次向 `main` 分支推送代码时将自动执行：

1. **Build & Test**: 拉取 .NET 10 环境编译并运行单元测试。
2. **AWS Auth**: 使用 AWS 凭证登录 ECR 私有仓库 (`003571163976`)。
3. **Docker Push**: 构建镜像并推送到 `swiftui-backend-demo` 仓库。
4. **ECS Deployment**: 触发 AWS CLI 强制更新服务 (`--force-new-deployment`)，实现零停机滚动发布。

---

## 🛠️ 运维与成本优化（按需启停）

为了节省云端计算成本，本服务支持一键暂停 Fargate 实例运行，同时保留所有架构配置：

* **停止计费**：将 ECS Service 的 **Desired tasks（期望任务数）** 修改为 **`0`**，计算资源费立刻归零。
* **恢复运行**：将 Desired tasks 改回 **`1`**，服务即可快速恢复。
