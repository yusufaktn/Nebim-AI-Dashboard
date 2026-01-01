# Nebim-AI-Dashboard


Nebim V3 ERP sistemleri ile tam entegre çalışan, tekstil perakendeciliği için özel olarak tasarlanmış, yapay zeka destekli bir yönetim ve analiz platformudur.

## 🚀 Proje Hakkında
Geleneksel raporlama yöntemlerinin ötesine geçerek, yöneticilerin verileriyle "konuşmasını" sağlar. Karmaşık SQL sorguları yerine doğal dilde sorular sorarak (Gemini 2.5) anlık ticari kararlar alınmasına yardımcı olur.

### ✨ Temel Özellikler
- **AI Asistan (Gemini Integration):** "Geçen sezon en çok iade edilen kırmızı elbiseler hangileri?" gibi sorulara anlık yanıtlar.
- **Tekstil Odaklı Varyant Analizi:** Renk ve beden bazlı stok/satış takibi.
- **Performans Dashboardları:** Anlık ciro, kâr marjı ve mağaza bazlı KPI takibi.
- **N-Tier Architecture:** .NET 9 ile inşa edilmiş, ölçeklenebilir ve güvenli katmanlı mimari.

### 🛠 Teknik Stack
- **Backend:** .NET 9 Web API, Dapper, EF Core,
- **Frontend:** React, TypeScript, Mantine UI, Zustand, TanStack Query.
- **AI:** Google Gemini 2.5 Flash API.
- **Database:** PostgreSQL (App Data).

### 🏗 Mimari Yapı
Proje **N-Tier (Layered) Architecture** prensiplerine uygun olarak geliştirilmiştir:
- `Api`: Sunum katmanı.
- `BLL`: İş mantığı ve AI entegrasyonu.
- `DAL`: Dapper ve EF Core ile veri erişimi.
- `Entity`: Ortak veri modelleri.


