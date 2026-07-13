# Chroma CRM

Modüler monolith CRM API — .NET 8, Clean Architecture, PostgreSQL.

## Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (PostgreSQL için önerilir)
- [EF Core CLI](https://learn.microsoft.com/ef/core/cli/dotnet) (migration oluşturmak için)

```powershell
dotnet tool install --global dotnet-ef
```

## Hızlı Başlangıç

### 1. PostgreSQL'i ayağa kaldır

```powershell
docker compose up -d
```

Connection string (`Chroma/appsettings.json`):

```
Host=localhost;Port=5432;Database=chroma_crm;Username=postgres;Password=postgres
```

### 2. Projeyi derle ve çalıştır

```powershell
dotnet restore .\Chroma.sln
dotnet build .\Chroma.sln
dotnet run --project .\Chroma\Chroma.csproj
```

Development ortamında API başlarken bekleyen migration'lar otomatik uygulanır.

### 3. Swagger

Tarayıcıda: `https://localhost:{port}/swagger`

Health check: `GET /health` (anonim)

### 4. İlk giriş (Development seed)

API Development modda başlarken demo tenant ve admin kullanıcı oluşturulur:

| Alan | Değer |
|------|-------|
| Tenant slug | `demo` |
| Email | `admin@demo.local` |
| Password | `Admin123!` |

Login:

```http
POST /api/auth/login
{
  "tenantSlug": "demo",
  "email": "admin@demo.local",
  "password": "Admin123!"
}
```

Dönen `accessToken` ile Swagger'da **Authorize** → `Bearer {token}` girin.

Seed ayrıca demo **Sales Pipeline** (Lead → Qualified → Proposal → Won/Lost) oluşturur.

## Modüller (API)

| Modül | Endpoint prefix |
|-------|-------------------|
| Auth & Users | `/api/auth`, `/api/users`, `/api/roles`, `/api/tenant` |
| CRM | `/api/contacts`, `/api/companies`, `/api/tags`, `/api/pipelines`, `/api/deals`, `/api/tasks`, `/api/activities`, `/api/notes` |
| Communication | `/api/channels`, `/api/conversations`, `/api/messages`, `/api/webhooks` |
| Forms & Custom | `/api/forms`, `/api/custom-fields`, `/api/files` |
| Automation | `/api/workflows`, `/api/notifications`, `/api/reports` |

Tüm endpoint'ler JWT + permission gerektirir (`contacts.read`, `deals.move_stage` vb.).

## Çözüm Yapısı

| Proje | Rol |
|-------|-----|
| `Chroma` | API host (Controllers, middleware) |
| `Chroma.Application` | Use-case sözleşmeleri, DTO'lar |
| `Chroma.Domain` | Entity'ler, domain kuralları |
| `Chroma.Infrastructure` | EF Core, servis implementasyonları |

## Migration Komutları

Yeni migration oluşturmak için:

```powershell
dotnet ef migrations add <MigrationName> `
  --project .\Chroma.Infrastructure\Chroma.Infrastructure.csproj `
  --startup-project .\Chroma\Chroma.csproj `
  --output-dir Persistence\Migrations
```

Veritabanını manuel güncellemek için:

```powershell
dotnet ef database update `
  --project .\Chroma.Infrastructure\Chroma.Infrastructure.csproj `
  --startup-project .\Chroma\Chroma.csproj
```

## Yol Haritası

Geliştirme planı ve TODO listesi için `ROADMAP.md` dosyasına bakın.
