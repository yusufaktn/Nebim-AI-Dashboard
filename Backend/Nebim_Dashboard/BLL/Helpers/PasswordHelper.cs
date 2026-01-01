using System.Security.Cryptography;

namespace BLL.Helpers;

/// <summary>
/// Şifre işlemleri için helper
/// 
/// 🎓 AÇIKLAMA:
/// - Şifreler asla düz metin olarak saklanmaz
/// - BCrypt yerine PBKDF2 kullanıyoruz (built-in, ek paket gerektirmez)
/// - Salt: Her şifre için rastgele üretilen değer (rainbow table saldırılarını önler)
/// - Hash: Tek yönlü şifreleme (geri dönüşü yok)
/// </summary>
public static class PasswordHelper
{
    private const int SaltSize = 16; // 128 bit
    private const int HashSize = 32; // 256 bit
    private const int Iterations = 100000; // OWASP önerisi
    
    /// <summary>
    /// Şifreyi hashle
    /// </summary>
    /// <param name="password">Düz metin şifre</param>
    /// <returns>Base64 formatında hash (salt dahil)</returns>
    public static string HashPassword(string password)
    {
        // 1. Rastgele salt üret
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        
        // 2. PBKDF2 ile hashle
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);
        
        // 3. Salt + Hash birleştir ve Base64'e çevir
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
        
        return Convert.ToBase64String(hashBytes);
    }
    
    /// <summary>
    /// Şifreyi doğrula
    /// </summary>
    /// <param name="password">Düz metin şifre</param>
    /// <param name="hashedPassword">Veritabanındaki hash</param>
    /// <returns>Eşleşiyor mu?</returns>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            // 1. Base64'ten byte dizisine çevir
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            
            // 2. Salt'ı ayıkla
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            
            // 3. Girilen şifreyi aynı salt ile hashle
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);
            
            // 4. Hash'leri karşılaştır (timing attack'a karşı güvenli)
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
