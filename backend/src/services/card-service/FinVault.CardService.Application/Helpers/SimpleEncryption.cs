using System.Security.Cryptography;
using System.Text;

namespace FinVault.CardService.Application.Helpers;

/// <summary>
/// Simple encryption helper for demo purposes
/// In production, use Azure Key Vault or AWS KMS
/// </summary>
public static class SimpleEncryption
{
    // Demo encryption key - in production, store in Azure Key Vault
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("FinVault2024Demo"); // 16 bytes for AES-128
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("InitVector123456"); // 16 bytes

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}
