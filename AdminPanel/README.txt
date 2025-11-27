============================================
KUAFÖR RANDEVU SİSTEMİ - YÖNETİCİ PANELİ
ASP.NET Core MVC Projesi
============================================

GEREKLİ YAZILIMLAR:
------------------
1. .NET 8.0 SDK veya üzeri
   İndirme: https://dotnet.microsoft.com/download

2. PostgreSQL veritabanı (Docker'da çalışıyor olmalı)

3. Visual Studio 2022 veya Visual Studio Code (önerilir)


KURULUM ADIMLARI:
-----------------

1. Veritabanı Bağlantı Ayarları:
   
   appsettings.json dosyasını açın ve bağlantı bilgilerini güncelleyin:
   
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=hairdresser_booking;Username=KULLANICI_ADI;Password=ŞIFRE"
   }


2. Admin Giriş Bilgileri (İsteğe bağlı değiştirin):
   
   appsettings.json dosyasında:
   
   "AdminCredentials": {
     "Username": "admin",
     "Password": "admin123"
   }


3. Projeyi Çalıştırma:

   Terminal veya CMD'de proje klasörüne gidin:
   
   cd HairdresserAdmin
   
   NuGet paketlerini yükleyin:
   
   dotnet restore
   
   Projeyi derleyin:
   
   dotnet build
   
   Projeyi çalıştırın:
   
   dotnet run
   
   Tarayıcıda açılacak adres: https://localhost:5001 veya http://localhost:5000


4. Visual Studio'da Çalıştırma:
   
   - HairdresserAdmin.csproj dosyasına çift tıklayın
   - Visual Studio açılacak
   - F5'e basarak projeyi çalıştırın


GİRİŞ BİLGİLERİ:
---------------
Kullanıcı Adı: admin
Şifre: admin123


ÖZELLİKLER:
----------
✓ Güvenli giriş sistemi
✓ Günlük randevuları görüntüleme
✓ Tarih bazlı randevu arama
✓ Randevu durumu güncelleme (Onayla, Tamamla, İptal)
✓ Müşteri iletişim bilgileri
✓ Randevu notları ekleme/düzenleme
✓ İstatistikler (Toplam, Onaylı, Bekleyen, İptal)
✓ Responsive tasarım (mobil uyumlu)
✓ Modern ve kullanıcı dostu arayüz


VERİTABANI:
----------
Proje, ana proje klasöründeki database_schema.sql ile oluşturulan
PostgreSQL veritabanını kullanır. Veritabanının çalışır durumda
olduğundan emin olun.


SORUN GİDERME:
-------------

1. "Npgsql" hatası alıyorsanız:
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

2. "Connection refused" hatası:
   - PostgreSQL'in çalıştığından emin olun
   - Bağlantı bilgilerini kontrol edin
   - Host, Port, Database adı, kullanıcı ve şifreyi doğrulayın

3. "Table does not exist" hatası:
   - database_schema.sql dosyasını çalıştırdığınızdan emin olun

4. Port zaten kullanımda hatası:
   - launchSettings.json dosyasından port numarasını değiştirebilirsiniz
   - Properties/launchSettings.json


GÜVENLİK ÖNERİLERİ:
------------------
⚠️ Üretim ortamına geçmeden önce:
   - Admin şifresini değiştirin
   - appsettings.json'daki hassas bilgileri ortam değişkenlerine taşıyın
   - HTTPS kullanın
   - Güçlü şifre politikası uygulayın
   - Veritabanı kullanıcısına minimum gerekli yetkileri verin


PROJE YAPISI:
------------
HairdresserAdmin/
├── Controllers/           # MVC Controllers
│   ├── AccountController.cs
│   ├── DashboardController.cs
│   └── HomeController.cs
├── Models/               # Data Models
│   ├── User.cs
│   ├── Appointment.cs
│   └── ViewModels/
├── Views/                # Razor Views
│   ├── Account/
│   ├── Dashboard/
│   └── Shared/
├── Data/                 # Database Context
│   └── ApplicationDbContext.cs
├── wwwroot/             # Static Files
│   ├── css/
│   └── js/
├── appsettings.json
└── Program.cs


DESTEK:
------
Sorun yaşarsanız veya özellik talepleriniz varsa:
- GitHub Issues
- E-posta: support@example.com


LİSANS:
------
Bu proje özel kullanım içindir.


Son Güncelleme: 27 Kasım 2025

