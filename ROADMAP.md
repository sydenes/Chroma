# Chroma CRM — Geliştirme Yol Haritası & TODO

> Bu dosya projenin tek kaynak yol haritasıdır.
> Yeni özellik / tablo / pattern eklerken burayı güncelle.
> Durum işaretleri: `[ ]` todo · `[~]` devam ediyor · `[x]` tamamlandı · `[-]` ertelendi

---

## 0) Mevcut Durum (Bugün)

### Proje iskeleti
- [x] Modüler monolith katmanlar: `Chroma`, `Chroma.Application`, `Chroma.Domain`, `Chroma.Infrastructure`
- [x] .NET 8 + PostgreSQL (EF Core) bağlandı
- [x] Swagger / health endpoint
- [x] `NuGet.Config` (nuget.org)
- [x] Soft delete / UTC timestamp için `BaseEntity`

### Hazır entity / API
- [x] `contacts` + Contacts API (CRUD + search)
- [x] `contact_channels` (entity + DbContext mapping)
- [x] `companies` + Companies API (CRUD + search)

### Eksik ama kritik
- [ ] `Chroma.sln` solution dosyası (OneDrive/encoding kaynaklı eksik kalabiliyor — yeniden oluştur)
- [ ] İlk EF Core migration + DB oluşturma
- [ ] Seed (demo tenant / admin kullanıcı)
- [ ] JWT auth + RBAC
- [ ] Tenants / Users / Roles / Permissions tabloları

---

## 1) Mimari Kurallar (Değişmeyecek)

1. Mikroservis değil → **modüler monolith**, tek API, tek PostgreSQL.
2. Clean Architecture + CQRS (MediatR) hedefi.
3. Tüm PK = `uuid`, zaman = `timestamptz` (UTC).
4. Soft delete + audit metadata zorunlu (`BaseEntity` + audit log).
5. `tenant_id` business tablolarında zorunlu.
6. Provider-specific tablo YASAK (`facebook_messages` vb. yok).
7. Contact’a social id kolonu koyma → `contact_channels` kullan.
8. Tablo/kolon naming: `snake_case`, çoğul tablo adı.
9. Her FK için `ON DELETE` davranışı açık tanımla.
10. Yeni provider (TikTok/LinkedIn vb.) schema-breaking migration gerektirmemeli.

---

## 2) Klasör / Pattern Standartı

Her yeni modül şu yapıya uysun:

```text
Chroma.Domain/
  Entities/
  Enums/
  Common/

Chroma.Application/
  Modules/{ModuleName}/
    Commands/          # CreateXCommand + Handler + Validator
    Queries/           # GetXQuery / SearchXQuery + Handler
    Dtos/
    Services/          # (geçiş döneminde; CQRS sonrası isteğe bağlı)
  Abstractions/
  Common/

Chroma.Infrastructure/
  Persistence/
    Configurations/    # IEntityTypeConfiguration<T>
    ApplicationDbContext.cs
  Services/
  Integrations/        # Meta/WhatsApp provider adapter'ları

Chroma/
  Controllers/
  Middleware/
```

### Her entity için yapılacak checklist
- [ ] Domain entity + enum/value object
- [ ] Fluent API `IEntityTypeConfiguration`
- [ ] `DbSet` + `IApplicationDbContext` kaydı
- [ ] Soft delete query filter
- [ ] Index planı (nedeniyle)
- [ ] Unique constraint (tenant-aware)
- [ ] Migration
- [ ] DTO / Command / Query / Validator
- [ ] Service veya MediatR handler
- [ ] Controller endpoint’leri
- [ ] Swagger’da görünürlük
- [ ] ROADMAP.md durumunu güncelle

### Ortak pattern backlog
- [ ] MediatR + CQRS’e geçiş (service pattern → command/query)
- [ ] FluentValidation pipeline
- [ ] Global exception middleware + `ApiResponse` standardı
- [ ] Pagination / filter / sort ortak yardımcıları
- [ ] `ICurrentTenant` / `ICurrentUser` abstraction
- [ ] Unit of Work (gerektiğinde)
- [ ] Outbox pattern (sonra: automation / notification)
- [ ] Idempotency key (message send, deal create)
- [ ] Health checks (Postgres + Redis)
- [ ] Serilog structured logging
- [ ] docker-compose: `api + postgres + redis`

---

## 3) Veritabanı Tabloları — Tam Envanter

### 3.1 Identity & Tenant

| Tablo | Durum | Not |
|---|---|---|
| `tenants` | [ ] | Id, Name, Slug, Logo, Phone, Email, Website, TimeZone, Language, Currency, Status |
| `users` | [ ] | TenantId, FirstName, LastName, Email, Phone, PasswordHash, Avatar, LastLogin, Status |
| `roles` | [ ] | TenantId, Name |
| `permissions` | [ ] | Key, Description (global veya tenant-aware karar) |
| `role_permissions` | [ ] | RoleId, PermissionId |
| `user_roles` | [ ] | UserId, RoleId |
| `tenant_settings` | [ ] | Theme, Language, Currency, Timezone + JSONB extras |

**Index / constraint notları**
- `users`: unique `(tenant_id, email)` where not deleted
- `roles`: unique `(tenant_id, name)`
- `permissions.key` unique
- `tenants.slug` unique

### 3.2 CRM Core

| Tablo | Durum | Not |
|---|---|---|
| `contacts` | [x] | Social id kolonları YOK |
| `contact_channels` | [~] | Entity var, API yok |
| `companies` | [x] | CRUD hazır |
| `tags` | [ ] | TenantId, Name, Color |
| `contact_tags` | [ ] | ContactId, TagId |
| `company_tags` | [ ] | CompanyId, TagId |
| `pipelines` | [ ] | TenantId, Name, Order |
| `stages` | [ ] | PipelineId, Name, Order, Color, WinStage, LostStage |
| `deals` | [ ] | Pipeline/Stage/Company/Contact/Owner + Amount/Currency/Probability |
| `tasks` | [ ] | Activity ile karıştırma; ayrı aggregate |
| `activities` | [ ] | CALL/EMAIL/MEETING/NOTE/SMS/WHATSAPP/... |
| `notes` | [ ] | Contact/Company/Deal polymorphic bağ |
| `files` | [ ] | OwnerType + OwnerId + StorageProvider + Url |

**ContactChannels detay**
- ChannelType: `EMAIL | PHONE | MOBILE | INSTAGRAM | FACEBOOK | WHATSAPP | TELEGRAM | LINKEDIN | ...`
- Unique: `(tenant_id, channel_type, value)` (normalize edilmiş value)
- API: add/update/verify/set-primary

### 3.3 Communication / Social (provider-agnostic)

| Tablo | Durum | Not |
|---|---|---|
| `channels` | [ ] | Provider + ExternalAccountId + Settings(JSONB) |
| `conversations` | [ ] | ChannelId, ContactId, AssignedUserId, Status, UnreadCount |
| `conversation_participants` | [ ] | Grup chat geleceğine hazır |
| `messages` | [ ] | Direction IN/OUT, Type, ExternalId, Text/Media, status timestamps |

**Provider enum (tablo değil)**
`FACEBOOK | INSTAGRAM | WHATSAPP | EMAIL | SMS | TELEGRAM | WEBCHAT | LINKEDIN | TIKTOK`

**Messages büyüme planı**
- [ ] Partition stratejisi (aylık / tenant)
- [ ] Arşivleme politikası
- [ ] GIN / FTS index ihtiyaç analizi

### 3.4 Forms / Custom Fields

| Tablo | Durum | Not |
|---|---|---|
| `forms` | [ ] | |
| `form_fields` | [ ] | Options JSONB |
| `form_responses` | [ ] | JsonData JSONB + ContactId |
| `custom_fields` | [ ] | EntityType + Name + Type + Settings JSONB |
| `custom_field_values` | [ ] | FieldId + EntityId + Value |

### 3.5 Automation / Notifications / Audit

| Tablo | Durum | Not |
|---|---|---|
| `workflows` | [ ] | |
| `workflow_triggers` | [ ] | |
| `workflow_conditions` | [ ] | |
| `workflow_actions` | [ ] | |
| `notifications` | [ ] | UserId, Title, Body, Read, Type |
| `audit_logs` | [ ] | Entity, EntityId, Action, OldValue/NewValue JSONB |

---

## 4) Faz Planı

### Faz 0 — Stabilizasyon (şimdi)
- [ ] Solution dosyasını kalıcı oluştur (`Chroma.sln`)
- [ ] Placeholder class dosyalarını temizle (`Class1.cs`)
- [ ] Fluent configs’i `Configurations/` altına taşı
- [ ] İlk migration: `InitialCreate` (contacts, contact_channels, companies)
- [ ] `EnsureCreated` veya migrate-on-startup (sadece Development)
- [ ] docker-compose (postgres + optional redis)
- [ ] README: nasıl çalıştırılır

### Faz 1 — Identity + Tenant + Auth
- [ ] `tenants`, `users`, `roles`, `permissions`, `role_permissions`, `user_roles`, `tenant_settings`
- [ ] JWT login / refresh token modeli
- [ ] Password hashing (ASP.NET Identity veya custom bcrypt/argon2)
- [ ] Permission-based authorization attribute
- [ ] Seed: default tenant + admin + temel permission listesi
- [ ] `ICurrentUser` / `ICurrentTenant` middleware
- [ ] Controllers: `/api/auth`, `/api/users`, `/api/roles`

### Faz 2 — CRM satış omurgası
- [ ] `contact_channels` API tamamla
- [ ] `tags` + contact/company tag ilişkileri
- [ ] `pipelines` + `stages`
- [ ] `deals` CRUD + stage move endpoint
- [ ] `notes`
- [ ] `tasks` + `activities` (ayrı tut)
- [ ] Deal board query (stage bazlı liste)

### Faz 3 — Communication çekirdeği
- [ ] `channels`, `conversations`, `messages`, `conversation_participants`
- [ ] Inbound webhook adapter iskeleti (provider-agnostic)
- [ ] Outbound message send use-case
- [ ] Conversation assign / unread / status
- [ ] ContactChannel ↔ Conversation eşleştirme kuralları

### Faz 4 — Entegrasyonlar
- [ ] WhatsApp Business API adapter
- [ ] Meta (Facebook/Instagram/Messenger) adapter
- [ ] Email channel adapter (SMTP/IMAP veya provider)
- [ ] Telegram adapter
- [ ] Channel settings şifreleme (secrets vault / env)

### Faz 5 — Forms / Custom Fields / Files
- [ ] Custom fields definition + values
- [ ] Forms + responses → contact oluşturma opsiyonu
- [ ] Files (local/S3/minio) + OwnerType polymorphic

### Faz 6 — Automation / Notifications / Reports
- [ ] Workflow engine (trigger/condition/action)
- [ ] Outbox + background worker
- [ ] In-app notifications
- [ ] Temel report endpoint’leri (deal pipeline conversion, activity volume)

### Faz 7 — Sertleştirme
- [ ] Audit log middleware / interceptors
- [ ] Rate limiting
- [ ] Field-level PII koruma planı
- [ ] Integration tests + contract tests
- [ ] Performance index review
- [ ] Messages/audit archive jobs

---

## 5) API Endpoint Envanteri

### Hazır
- [x] `GET/POST/PUT/DELETE /api/contacts`
- [x] `GET/POST/PUT/DELETE /api/companies`
- [x] `GET /health`
- [~] `GET /api/auth/status` (placeholder)

### Faz 1
- [ ] `POST /api/auth/login`
- [ ] `POST /api/auth/refresh`
- [ ] `GET/POST/PUT /api/users`
- [ ] `GET/POST /api/roles`
- [ ] `PUT /api/roles/{id}/permissions`
- [ ] `GET/PUT /api/tenant/settings`

### Faz 2
- [ ] `/api/contacts/{id}/channels`
- [ ] `/api/tags`
- [ ] `/api/pipelines` + `/api/pipelines/{id}/stages`
- [ ] `/api/deals` (+ move-stage)
- [ ] `/api/tasks`
- [ ] `/api/activities`
- [ ] `/api/notes`

### Faz 3+
- [ ] `/api/channels`
- [ ] `/api/conversations`
- [ ] `/api/messages`
- [ ] `/api/forms`
- [ ] `/api/custom-fields`
- [ ] `/api/workflows`
- [ ] `/api/files`
- [ ] `/api/notifications`
- [ ] `/api/reports`
- [ ] `/webhooks/{provider}`

---

## 6) Index / Constraint Önerileri (Özet)

- Tenant izolasyonu için çoğu sorguda: `(tenant_id, ...)` composite index
- Soft delete partial unique: `WHERE is_deleted = false`
- `contact_channels`: unique `(tenant_id, channel_type, lower(value))`
- `deals`: `(tenant_id, pipeline_id, stage_id)`, `(tenant_id, owner_id)`
- `messages`: `(conversation_id, sent_at)`, unique `(channel_id/external)` için `external_id`
- `audit_logs`: `(tenant_id, entity, entity_id, created_at)`
- JSONB Settings alanlarında ihtiyaç oldukça GIN

---

## 7) Kararlar / Varsayımlar

1. İlk fazda service pattern kabul; CQRS’e fazla geçmeden domain büyümeyecek.
2. Auth gelene kadar endpoint’ler `TenantId` query/body ile çalışabilir (geçici); auth sonrası claim’den alınacak.
3. Permission key formatı: `module.action` örn. `contacts.read`, `deals.move_stage`.
4. Custom field values ilk etapta text/json string; tip özel kolonlara gerek yok.
5. Automation ve Chat entegrasyonları çekirdeğe gömülmez; `Integrations/` adapter sınırı korur.
6. Tek DB; polyglot (Mongo) açmayacağız.

---

## 8) Hemen Sonraki 10 İş (Sprint Checkpoint)

1. [ ] `Chroma.sln` oluştur / doğrula
2. [ ] Class1 temizliği
3. [ ] Entity configs’i `Configurations/` klasörüne taşı
4. [ ] Migration paketi + `InitialCreate`
5. [ ] Development’ta migrate/seed startup
6. [ ] `tenants` + `users` entity
7. [ ] JWT auth iskeleti
8. [ ] Permission seed + authorize attribute
9. [ ] `contact_channels` API
10. [ ] `pipelines` / `stages` / `deals` domain başlangıcı

---

## 9) Çalıştırma Notu

```powershell
# restore + build
dotnet restore .\Chroma\Chroma.csproj
dotnet build .\Chroma\Chroma.csproj

# run API
dotnet run --project .\Chroma\Chroma.csproj

# swagger
# https://localhost:{port}/swagger
```

Connection string: `Chroma/appsettings.json` → `ConnectionStrings:DefaultConnection`

---

## 10) Güncelleme Kuralı

Her PR / geliştirme oturumu sonunda:
1. Bu dosyada ilgili checkbox’ları güncelle
2. Yeni tablo veya karar eklendiyse “Kararlar / Varsayımlar”a yaz
3. Endpoint eklendiyse API envanterine işaretle

**Son güncelleme:** 2026-07-08  
**Durum:** Faz 0 devam ediyor — contacts/companies ayakta, auth ve migration sırada.
