using System.Security.Cryptography;
using System.Text.Json;

namespace Api.Extensions;

public class AesEncryptionUtility
{
    /// <summary>
    /// Encrypts data using AES encryption with embedded IV
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <param name="base64Key">Base64 encoded encryption key</param>
    /// <returns>Base64 encrypted string containing IV and ciphertext</returns>
    public static string Encrypt(string plainText, string base64Key)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        if (string.IsNullOrEmpty(base64Key))
            throw new ArgumentNullException(nameof(base64Key));

        var key = Convert.FromBase64String(base64Key);

        if (key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes long for AES-256", nameof(base64Key));

        byte[] iv;
        byte[] encryptedBytes;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.GenerateIV();
            iv = aesAlg.IV;
            aesAlg.Key = key;
            aesAlg.IV = aesAlg.IV;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
            {
                // Encrypt
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    encryptedBytes = msEncrypt.ToArray();
                }
            }
        }

        // Combine IV and encrypted bytes
        var combinedBytes = new byte[iv.Length + encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, combinedBytes, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, combinedBytes, iv.Length, encryptedBytes.Length);

        // Return base64 encrypted string with embedded IV
        return Convert.ToBase64String(combinedBytes);
    }

    /// <summary>
    /// Decrypts AES encrypted data with embedded IV
    /// </summary>
    /// <param name="cipherText">Base64 encrypted text containing IV and ciphertext</param>
    /// <param name="base64Key">Base64 encoded decryption key</param>
    /// <returns>Decrypted string</returns>
    public static string Decrypt(string cipherText, string base64Key)
    {
        // Validate inputs
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentNullException(nameof(cipherText));

        if (string.IsNullOrEmpty(base64Key))
            throw new ArgumentNullException(nameof(base64Key));

        // Convert base64 key to byte array
        var key = Convert.FromBase64String(base64Key);

        // Validate key length
        if (key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes long for AES-256", nameof(base64Key));

        // Decode base64 cipher text
        var combinedBytes = Convert.FromBase64String(cipherText);

        // Extract IV (first 16 bytes)
        var iv = new byte[16];
        Buffer.BlockCopy(combinedBytes, 0, iv, 0, iv.Length);

        // Extract encrypted bytes
        var encryptedBytes = new byte[combinedBytes.Length - iv.Length];
        Buffer.BlockCopy(combinedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

        // Decrypt
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            // Create decryptor
            using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
            {
                using (var msDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read decrypted text
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public static string Encrypt(object obj, string base64Key)
    {
        var json = JsonSerializer.Serialize(obj);
        return Encrypt(json, base64Key);
    }

    public static T? Decrypt<T>(string cipherText, string base64Key)
    {
        var data = Decrypt(cipherText, base64Key);
        return JsonSerializer.Deserialize<T>(data);
    }
}