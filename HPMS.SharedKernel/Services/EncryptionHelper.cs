using System.Security.Cryptography;
using System.Text;

namespace HPMS.SharedKernel.Services;

public static class EncryptionHelper
{
    // In production, this MUST come from a secure environment variable or Key Vault.
    // It must be exactly 32 bytes (256 bits).
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("7db8f9a2e3c4b5d6e7f8a9b0c1d2e3f4"); 

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = Aes.Create();
        aes.Key = Key;
        // The IV is automatically generated here. 
        // We must store the IV alongside the ciphertext to decrypt it later.

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        // Write the IV to the start of the stream so it's preserved
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = Key;

        // Extract the IV from the beginning of the ciphertext
        var iv = new byte[aes.BlockSize / 8];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}