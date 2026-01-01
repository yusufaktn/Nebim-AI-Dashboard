# PROJE PLANI: Nebim V3 Entegreli AI Dashboard (NebimAI)

Bu belge, Nebim V3 (ERP) verilerini analiz eden, .NET 8 tabanlı Clean Architecture backend ve React tabanlı frontend içeren, Yapay Zeka (Google Gemini) destekli bir dashboard projesinin teknik planıdır.

---

## 1. INITIALIZATION PROMPT (Sistem Başlatma Komutu)
*(Bu metni AI asistanına projenin başında verin)*

**Role:** You are a Senior .NET Solution Architect and a React Expert.
**Project Goal:** Build a "Textile Retail Management Dashboard" that visualizes Nebim V3 ERP data and includes an AI Chat Assistant capable of executing SQL queries via Semantic Kernel function calling.
**Key Constraint:** You must strictly follow Clean Architecture (Onion Architecture) principles.
**Language:** The codebase (variable names, comments) should be in English, but the UI text (labels, buttons) must be in TURKISH.

**Core Requirements:**
Backend: .NET 9 Web API.

Architecture: N-Tier (Solution logic: Entities, DataAccess, Business, WebAPI).

Data Access:

Use Dapper for high-performance READ operations (Reports, Dashboard Metrics) from Nebim Database.

Use EF Core for WRITE operations (User management, App Settings) to a separate App Database.

AI Engine: Microsoft Semantic Kernel utilizing Google Gemini 1.5 Flash API.

Frontend: React (Vite) + TypeScript + Mantine UI + Zustand + TanStack Query.

Mocking: Since you don't have access to the real Nebim MSSQL database, you must create a MockNebimService in the DAL or BLL that returns realistic textile retail data (Sizes: S, M, L, XL / Colors: Red, Blue / Sales data).

---

## 2. DETAYLI TECH STACK (Teknoloji Yığını)

### Backend (.NET Ecosystem)
Framework: .NET 9.0 (Latest)

Architecture: N-Tier (Layered)

API Type: ASP.NET Core Web API

AI Orchestration: Microsoft.SemanticKernel

AI Provider: Google Gemini API

ORM 1 (Read-Heavy / Reports): Dapper

ORM 2 (App Data / CRUD): Entity Framework Core

Dependency Injection: Built-in .NET DI

Validation: FluentValidation

Logging: Serilog

Documentation: Swagger / OpenAPI
### Frontend (React Ecosystem)
* **Build Tool:** Vite
* **Language:** TypeScript
* **UI Library:** Mantine UI v7 (Components, Hooks, Notifications)
* **State Management:** Zustand (Global State)
* **Data Fetching:** TanStack Query (React Query) v5
* **Routing:** React Router DOM v6
* **HTTP Client:** Axios
* **Icons:** Tabler Icons or Lucide React
* **Charts:** Recharts or Mantine Charts

### Database & Infrastructure
* **Main DB (ERP):** MSSQL Server (Existing Nebim V3 Structure - Read Only)
* **App DB:** MSSQL or PostgreSQL (For App-specific data)
* **Cache:** Redis (Optional for V1, but architecture should support it)

---

## 3. PROJE DOSYA YAPISI (Clean Architecture)

Backend yapısı katmanlı mimariye tam uyumlu olmalıdır.

```text
NebimAI_Solution
│
├── src
│   ├── NebimAI.Entities            # (Shared Layer) POCO Classes, DTOs, Enums
│   │   ├── Concrete                # (AppUser, Product, SaleRecord)
│   │   ├── DTOs                    # (SalesSummaryDto, ChatMessageDto)
│   │   └── Enums                   # (ColorType, SizeVariant)
│   │
│   ├── NebimAI.DataAccess          # (DAL) Database Operations
│   │   ├── Contexts                # (AppDbContext - EF Core)
│   │   ├── Abstract                # (IRepository Interfaces)
│   │   ├── Concrete                # (EfEntityRepositoryBase)
│   │   │   ├── EntityFramework     # (EfUserDal)
│   │   │   └── Dapper              # (DapperReportDal - Raw SQL Queries for Nebim)
│   │   └── Mock                    # (MockNebimDal - For Development)
│   │
│   ├── NebimAI.Business            # (BLL) Business Logic, Validations, AI
│   │   ├── Abstract                # (IService Interfaces)
│   │   ├── Concrete                # (DashboardManager, AuthManager)
│   │   ├── AI                      # (Semantic Kernel Integration)
│   │   │   ├── Plugins             # (NebimInfoPlugin.cs)
│   │   │   └── ChatService.cs      # (Main AI Logic)
│   │   ├── ValidationRules         # (FluentValidation)
│   │   └── DependencyResolvers     # (Service Registration)
│   │
│   ├── NebimAI.WebAPI              # (Presentation) Controllers
│       ├── Controllers             # (DashboardController, ChatController)
│       └── Program.cs              # (Middleware & DI Setup)
│
├── client                          # React Application
│   ├── src
│   │   ├── components              # (StatCard, CustomTable)
│   │   ├── layout                  # (MainLayout, Sidebar, Header)
│   │   ├── pages                   # (Dashboard, Inventory, Chat)
│   │   ├── services                # (Axios calls to API)
│   │   ├── store                   # (Zustand Stores)
│   │   ├── types                   # (TS Interfaces matching Backend DTOs)
│   │   └── App.tsx
│
└── NebimAI.sln