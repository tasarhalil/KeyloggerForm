# 🖥️ Keylogger Projesi (C# - Visual Studio 2022)

Bu proje, **C#** kullanılarak **Visual Studio 2022** üzerinde geliştirilmiş bir **Keylogger uygulamasıdır**.  
Amaç: Kullanıcının yaptığı tuşlamaları kaydetmek, hangi pencere/dosya üzerinde işlem yapıldığını ve saat bilgisini log dosyasında saklamaktır.  

---

## ⚙️ Özellikler

- ⌨️ **Tuşlama Kaydı** → Kullanıcının bastığı tüm tuşlar kaydedilir.  
- 🕒 **Zaman Bilgisi** → Her tuşlama için saat bilgisi tutulur.  
- 📂 **Pencere/Dosya Bilgisi** → Tuşlamanın yapıldığı aktif pencere veya dosya bilgisi log dosyasına eklenir.  
- 🖼️ **Form Açılmadan Çalışma** → Uygulama arka planda çalışır, herhangi bir form açılmaz.  
- 🪟 **Sistem Tepsisi Entegrasyonu**  
  - Sağ tık menüsü ile:  
    - **Log Dosyasını Aç**  
    - **Log Dosyasını Temizle**  
    - **Çıkış Yap** seçenekleri kullanılabilir.  
- 📧 **Saat Başı Mail Gönderme** → Uygulama, log dosyasını her saat başında otomatik olarak belirlenen e‑posta adresine gönderir.   

---

## ⚠️ Uyarı ve Kullanım Koşulları

- Bu proje **yalnızca eğitim, araştırma ve siber güvenlik farkındalığı** amacıyla geliştirilmiştir.  
- **Kötüye kullanım kesinlikle yasaktır.** Başkalarının izni olmadan bilgisayarlarında çalıştırmak, kişisel verilerini toplamak veya gizliliklerini ihlal etmek **suç teşkil eder**.  
- Geliştirici, bu yazılımın **izinsiz veya yasa dışı kullanımından sorumlu değildir**.  
- Lütfen yalnızca **kendi bilgisayarınızda ve kendi verileriniz üzerinde** test edin.
- ---

## ⚠️ Güvenlik Notu

Kodda mail adresi ve şifre kısımları boş bırakılmıştır.  
Programı çalıştırmak için:  
1. Gmail hesabınızda **2 Adımlı Doğrulama**’yı açın.  
2. Google hesabınızdan **"Uygulama Şifresi"** oluşturun.  
3. Kodda `gonderici`, `sifre` ve `alici` değişkenlerini kendi bilgilerinizle doldurun.  

---

## 📚 Önerilen Kullanım Alanları

- Klavye girişlerinin nasıl işlendiğini öğrenmek  
- Antivirüs yazılımlarının keylogger’ları nasıl tespit ettiğini anlamak  
- Güvenlik araştırmalarında “zararlı yazılım davranışlarını” analiz etmek  

---

## 🛠️ Gereksinimler

- Visual Studio 2022  
- .NET Framework / .NET 6+  
- Windows işletim sistemi  

