# Chroma CRM — Geliştirme Yol Haritası & TODO

> Bu dosya projenin tek kaynak yol haritasıdır.
> Durum işaretleri: `[ ]` todo · `[~]` devam ediyor · `[x]` tamamlandı · `[-]` ertelendi

---

## 0) Mevcut Durum

**Tüm fazlar (0–7) tamamlandı.** Modüler monolith CRM API production-hardening iskeletiyle hazır.

### Proje iskeleti
- [x] Modüler monolith katmanlar
- [x] .NET 8 + PostgreSQL (EF Core)
- [x] Swagger / health endpoint (+ PostgreSQL health check)
- [x] JWT auth + RBAC
- [x] Serilog structured logging
- [x] Global exception middleware
- [x] Audit log middleware
- [x] Rate limiting
- [x] Outbox + background worker iskeleti

---

## 4) Faz Planı

### Faz 0 — Stabilizasyon [x]
### Faz 1 — Identity + Tenant + Auth [x]
### Faz 2 — CRM satış omurgası [x]
- [x] `contact_channels` API
- [x] `tags` + contact/company tag ilişkileri
- [x] `pipelines` + `stages`
- [x] `deals` CRUD + stage move + board query
- [x] `notes`, `tasks`, `activities`

### Faz 3 — Communication çekirdeği [x]
- [x] `channels`, `conversations`, `messages`, `conversation_participants`
- [x] Inbound webhook adapter iskeleti
- [x] Outbound message send
- [x] Conversation assign / unread / status

### Faz 4 — Entegrasyonlar [x]
- [x] WhatsApp, Meta, Email, Telegram adapter iskeletleri
- [x] Channel settings şifreleme (AES)

### Faz 5 — Forms / Custom Fields / Files [x]
- [x] Custom fields + values
- [x] Forms + responses (+ contact oluşturma opsiyonu)
- [x] Files (local storage)

### Faz 6 — Automation / Notifications / Reports [x]
- [x] Workflow engine iskeleti (trigger/condition/action)
- [x] Outbox + background worker
- [x] In-app notifications
- [x] Report endpoint'leri (pipeline conversion, activity volume)

### Faz 7 — Sertleştirme [x]
- [x] Audit log middleware
- [x] Rate limiting
- [x] Health checks (Postgres)
- [x] Messages/audit archive job iskeleti
- [~] Integration tests (ertelendi — manuel test yeterli şimdilik)
- [~] MediatR/CQRS geçişi (ertelendi — service pattern yeterli)

---

## 5) API Endpoint Envanteri

### Auth & Identity [x]
- `POST /api/auth/login`, `POST /api/auth/refresh`, `GET /api/auth/status`
- `GET/POST/PUT /api/users`, `GET/POST /api/roles`, `PUT /api/roles/{id}/permissions`
- `GET/PUT /api/tenant/settings`

### CRM Core [x]
- `GET/POST/PUT/DELETE /api/contacts`, `/api/companies`
- `/api/contacts/{id}/channels`
- `/api/tags`
- `/api/pipelines` + `/api/pipelines/{id}/stages`
- `/api/deals` + `GET board` + `POST {id}/move-stage`
- `/api/tasks`, `/api/activities`, `/api/notes`

### Communication [x]
- `/api/channels`, `/api/conversations`, `/api/messages`
- `/api/webhooks/{provider}`

### Forms & Custom [x]
- `/api/forms`, `/api/custom-fields`, `/api/files`

### Automation & Reports [x]
- `/api/workflows`, `/api/notifications`, `/api/reports`

### System [x]
- `GET /health` (PostgreSQL dahil)

---

## 9) Çalıştırma

```powershell
dotnet run --project .\Chroma\Chroma.csproj
```

**Demo giriş:** tenant `demo` · `admin@demo.local` · `Admin123!`

**Son güncelleme:** 2026-07-13  
**Durum:** Faz 0–7 tamamlandı. UI geliştirmesi başladı (ChromaUI).

---

## 10) UI — ChromaUI (React)

### Faz 8 — Web Arayüzü [~]
- [x] Vite + React + TypeScript + Tailwind iskeleti
- [x] JWT auth (login, refresh, protected routes)
- [x] App shell (sidebar, layout)
- [x] Dashboard (özet istatistikler)
- [x] Kişiler sayfası (liste, arama, CRUD)
- [x] Şirketler sayfası (liste, arama, oluşturma)
- [x] Fırsatlar Kanban board (pipeline, aşama taşıma)
- [ ] Görevler, aktiviteler, notlar
- [ ] Mesajlaşma / konuşmalar
- [ ] Bildirimler
- [ ] Ayarlar (tenant, kullanıcılar, roller)
- [ ] Formlar, özel alanlar, dosyalar
- [ ] Raporlar dashboard

```powershell
cd ChromaUI
npm install
npm run dev
```
