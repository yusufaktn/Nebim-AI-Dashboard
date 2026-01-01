NEBIM V3 ENTEGRELİ YAPAY ZEKA DESTEKLİ TEKSTİL YÖNETİM PLATFORMU: PROJE TANIMI VE İŞLEYİŞ SENARYOSU
1. Proje Vizyonu ve Amacı
Bu proje, Türkiye'nin en yaygın tekstil perakende ERP yazılımı olan Nebim V3 veritabanı üzerinde çalışan, modern, web tabanlı ve Yapay Zeka (AI) güçlendirilmiş bir yönetim panelidir.

Mevcut ERP raporlarının karmaşıklığını ortadan kaldırarak mağaza yöneticilerine ve şirket sahiplerine şunları sunmayı amaçlar:

Tekstil Dikeyi: Standart satış verisi yerine, tekstil sektörünün gerçeği olan Renk, Beden, Sezon ve Varyant kırılımlı analizler sunmak.

Görselleştirme: Ciro, Kar Marjı ve Stok Devir Hızı gibi kritik verileri saniyeler içinde grafiklerle göstermek.

AI Sohbet Asistanı: Yöneticilerin SQL bilmesine gerek kalmadan, doğal dilde (Türkçe) soru sorarak veritabanından karmaşık analizler almasını sağlamak. (Örn: "Geçen sezon en çok iade edilen kırmızı elbiseler hangileri?")

2. Sistemin Mantıksal İşleyişi (Workflow)
Sistem, Klasik Raporlama ve Yapay Zeka Destekli Analiz olmak üzere iki ana kulvarda çalışır.

Senaryo A: Hızlı Veri İzleme (Dashboard Akışı)
Amaç: Yöneticinin sisteme girdiğinde anlık durumu (Ciro, Satış Adedi) beklemeden görmesi.

Talep: Kullanıcı arayüzü (React), açılışta günün özet verilerini ister.

Sorgulama: Backend sistemi, Nebim veritabanına önceden optimize edilmiş hızlı SQL sorguları (Read-Only) gönderir.

Görselleştirme: Gelen ham veri, arayüzde Grafiklere ve Kartlara dönüşür.

Örnek Veri: "Bugün 150.000 TL ciro yapıldı, dünden %10 fazla."

Senaryo B: Yapay Zeka Asistanı (AI Agent Akışı)
Amaç: Önceden tanımlanmamış, spesifik ve derinlemesine sorulara cevap vermek.

Kullanıcı Sorusu: Yönetici Chat ekranına yazar: "Hangi mağazalarda M beden stokları tükenmek üzere?"

Anlamlandırma (Semantic Kernel): Sistemdeki Yapay Zeka motoru soruyu analiz eder. Kullanıcının "Stok" ve "Beden" ile ilgili bir veri istediğini anlar.

Fonksiyon Çağırma (Tool Usage): AI, kendi hafızasındaki yeteneklere bakar ve StokAnaliziGetir(beden='M', durum='Kritik') isimli arka plan fonksiyonunu seçer.

Veri Çekme: Bu fonksiyon, gerçek Nebim veritabanında bir SQL sorgusu çalıştırır. AI kafadan sayı uydurmaz, veritabanından gelen gerçek sonucu alır.

Yorumlama ve Cevap: Veritabanından gelen "İzmir Şube: 0 Adet, Ankara Şube: 2 Adet" verisini alan AI, bunu kullanıcıya cümle olarak sunar:

"İzmir ve Ankara şubelerinde M beden stoklarınız kritik seviyede. İzmir şubesinde hiç kalmamış, transfer yapmanızı öneririm."

3. Temel Veri Kuralları
Sadece Okuma (Read-Only): Sistem, güvenlik gereği Nebim veritabanına asla veri yazmaz, silmez veya güncellemez. Sadece var olan veriyi okur.

Varyant Yönetimi: Tüm ürün analizleri, tekstil doğası gereği Renk ve Beden (S, M, L, XL) detayında işlenir.

Gizlilik: AI modeline gönderilen verilerde müşteri isimleri veya hassas kimlik bilgileri maskelenir.