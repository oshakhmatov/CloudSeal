using Meziantou.Framework.Win32;
using System.Security.Cryptography;

namespace CloudSeal;

class SecretKeyManager
{
    private readonly string _credentialName;

    public SecretKeyManager(string credentialName)
    {
        _credentialName = credentialName;
    }

    public string GetOrCreateSecretKey()
    {
        var credential = CredentialManager.ReadCredential(_credentialName);

        if (credential != null)
        {
            Console.Write("Enter the password to access the secret key: ");
            var password = Console.ReadLine();
            if (!string.IsNullOrEmpty(password) && credential.Password == password)
                return credential.UserName;

            Console.WriteLine("Incorrect password.");
            return null;
        }

        Console.Write("Enter a password to secure the new secret key: ");
        var newPassword = Console.ReadLine();
        if (string.IsNullOrEmpty(newPassword))
        {
            Console.WriteLine("Password cannot be empty.");
            return null;
        }

        var secretKey = GenerateSecureKey(32);
        CredentialManager.WriteCredential(_credentialName, secretKey, newPassword, "Generated application secret key", CredentialPersistence.LocalMachine);
        Console.WriteLine($"Secret key '{_credentialName}' has been created and saved with the provided password.");
        return secretKey;
    }

    private static string GenerateSecureKey(int length)
    {
        var keyBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }
}
