using System.Security.Cryptography;

namespace Pro4Soft.BackgroundWorker.Execution.Common;

public static class SymmetricCrypt
{
    private static string Key => "OFLamiqqu8DbHUwTtZNiB5xXuR0nFXo40zaxJfBBBO0=";
    private static string Iv => "V0r9woi/jlH5WlSVhn1Eaw==";

    public static (string, string) CreateNewKey()
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        return (Convert.ToBase64String(aes.Key), Convert.ToBase64String(aes.IV));
    }

    public static byte[] EncryptToBytes(string textToEncrypt, string key = null, string iv = null)
    {
        var actualKey = key ?? Key;
        var actualIv = iv ?? Iv;

        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(actualKey);
        aes.IV = Convert.FromBase64String(actualIv);

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using (var streamWriter = new StreamWriter(cryptoStream))
            streamWriter.Write(textToEncrypt);
        return memoryStream.ToArray();
    }

    public static string Encrypt(string textToEncrypt, string key = null, string iv = null)
    {
        if (string.IsNullOrWhiteSpace(textToEncrypt))
            return textToEncrypt;

        return Convert.ToBase64String(EncryptToBytes(textToEncrypt, key, iv));
    }

    public static string DecryptFromBytes(byte[] encryptedBytes, string key = null, string iv = null)
    {
        var actualKey = key ?? Key;
        var actualIv = iv ?? Iv;

        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(actualKey);
        aes.IV = Convert.FromBase64String(actualIv);

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream(encryptedBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        return streamReader.ReadToEnd();
    }

    public static string Decrypt(string encryptedText, string key = null, string iv = null)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
            return encryptedText;

        return DecryptFromBytes(Convert.FromBase64String(encryptedText), key, iv);
    }

    public static bool TryDecrypt(string data, out string res)
    {
        try
        {
            res = Decrypt(data, Key);
            return true;
        }
        catch
        {
            res = null;
            return false;
        }
    }
}