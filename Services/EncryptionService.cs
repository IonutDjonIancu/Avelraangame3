using System.Security.Cryptography;
using System.Text;

namespace Services;

public class EncryptionService
{
    private const string Key = "9b8e7a1d58c64f2a906b3d7f3e8a49c0"; // TODO: replace with Azure appsettings
    private const string IV = "b2a0d1c8e9f3c5a4"; // TODO: replace with Azure appsettings

    public static string EncryptString(string plainText)
    {
        byte[] encryptedBytes;
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(Key);
            aesAlg.IV = Encoding.UTF8.GetBytes(IV);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new();
            using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter swEncrypt = new(csEncrypt);
                swEncrypt.Write(plainText);
            }
            encryptedBytes = msEncrypt.ToArray();
        }
        return Convert.ToBase64String(encryptedBytes);
    }

    public static string DecryptString(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(Key);
        aesAlg.IV = Encoding.UTF8.GetBytes(IV);

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new(cipherBytes);
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new(csDecrypt);
        return srDecrypt.ReadToEnd();
    }
}
