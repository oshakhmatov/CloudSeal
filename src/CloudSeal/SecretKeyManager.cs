using Meziantou.Framework.Win32;
using System.Security.Cryptography;

class SecretKeyManager
{
    private const string CredentialName = "CloudSealSecretKey"; // Определяем константу

    public string GetOrCreateSecretKey()
    {
        var credential = CredentialManager.ReadCredential(CredentialName);

        if (credential != null)
        {
            Console.Write("Введите пароль для доступа к секретному ключу: ");
            var password = Console.ReadLine();
            if (!string.IsNullOrEmpty(password) && credential.Password == password)
            {
                return credential.UserName; // Возвращаем секретный ключ
            }
        }
        else
        {
            return CreateNewSecretKey();
        }

        return null; // Возвращаем null, если пароль неверный или нет ключа
    }

    public string CreateNewSecretKey()
    {
        Console.Write("Введите новый секретный ключ (оставьте пустым для автоматической генерации): ");
        var newSecretKey = Console.ReadLine();

        if (string.IsNullOrEmpty(newSecretKey))
        {
            newSecretKey = GenerateSecureKey(32);
            Console.WriteLine("Секретный ключ был автоматически сгенерирован.");
        }

        Console.Write("Введите пароль для защиты нового секретного ключа: ");
        var newPassword = Console.ReadLine();
        if (string.IsNullOrEmpty(newPassword))
        {
            Console.WriteLine("Пароль не может быть пустым.");
            return null;
        }

        SaveSecretKey(newSecretKey, newPassword);
        return newSecretKey;
    }

    public void DeleteSecretKey()
    {
        CredentialManager.DeleteCredential(CredentialName);
        Console.WriteLine("Секретный ключ удален.");
    }

    public string GetSecretKeyName()
    {
        return CredentialName;
    }

    public string GetSecretKey()
    {
        var credential = CredentialManager.ReadCredential(CredentialName);
        return credential?.UserName;
    }

    public void SaveSecretKey(string secretKey, string password)
    {
        CredentialManager.WriteCredential(CredentialName, secretKey, password, "Application secret key", CredentialPersistence.LocalMachine);
        Console.WriteLine("Секретный ключ сохранен.");
    }

    private static string GenerateSecureKey(int length)
    {
        var keyBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }
}
