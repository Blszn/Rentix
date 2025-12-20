🚗 Rentix - Araç Kiralama Sistemi
Rentix, modern web teknolojileri kullanılarak geliştirilmiş, kullanıcı dostu bir araç kiralama (Rent a Car) platformudur. Kullanıcıların tarih ve konum bazlı araç aramasına, güvenli bir şekilde rezervasyon yapmasına olanak tanırken; yöneticiler için gelişmiş bir yönetim paneli sunar.

🌟 Özellikler
👤 Kullanıcı Paneli (Müşteri)
Gelişmiş Arama: Alış/İade tarihi ve konuma göre müsait araçları listeleme.

Araç Filtreleme: Sadece seçilen tarihlerde uygun (çakışmayan) araçları görüntüleme.

Üyelik Sistemi: E-posta doğrulama (Email Confirmation) ile güvenli kayıt ve giriş.

Profil Yönetimi: Ad, soyad, telefon ve şifre güncelleme işlemleri.

Kiralamalarım: Geçmiş ve aktif kiralama durumlarını takip etme, iptal edebilme.

🛡️ Yönetici Paneli (Admin)
Dashboard: Toplam araç, aktif kiralamalar ve üye sayısı gibi özet istatistikler.

Kiralama Takibi: Şu an kimin hangi araçta olduğunu, kalan süreyi ve plaka bilgisini anlık görme.

Araç Yönetimi: Yeni araç ekleme, silme, düzenleme ve resim yükleme.

Akıllı Silme Koruması: Kirada olan araçların silinmesini engelleme, geçmiş kiralamalarla birlikte temizleme.

Teslim Alma: Kiradaki aracı tek tıkla teslim alıp tekrar müsaite çıkarma.

🛠️ Teknolojiler
Bu proje aşağıdaki teknolojiler kullanılarak geliştirilmiştir:

Backend: ASP.NET Core 8.0 MVC

Veritabanı: PostgreSQL (Entity Framework Core ile)

Kimlik Doğrulama: ASP.NET Core Identity (Rol tabanlı: Admin/User)

Frontend: HTML5, CSS3, Bootstrap 5, JavaScript

DevOps: Docker & Docker Compose

Bulut (Cloud): Render.com üzerinde deploy edilmiştir.

🚀 Kurulum (Local)
Projeyi kendi bilgisayarınızda çalıştırmak için adımları takip edin:

Projeyi Klonlayın:

Bash

git clone https://github.com/kullaniciadiniz/rentix-web.git
cd rentix-web
Veritabanı Ayarları: appsettings.json dosyasındaki Connection String'i kendi PostgreSQL bilgilerinizle güncelleyin.

Veritabanını Oluşturun: Terminalde proje dizinine gidip şu komutu çalıştırın:

Bash

dotnet ef database update
Projeyi Başlatın:

Bash

dotnet run
Tarayıcıda https://localhost:7112 adresine gidin.

🐳 Docker ile Çalıştırma
Proje Docker uyumludur. Docker Desktop kuruluysa tek komutla ayağa kaldırabilirsiniz:

Bash

docker build -t rentix-app .
docker run -d -p 8080:8080 rentix-app
