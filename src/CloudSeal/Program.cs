using Meziantou.Framework.Win32;
using System.Diagnostics;
using System.Security.Cryptography;

string credentialName = "CloudSealSecretKey";
string secretKey;

try
{
    // Check if the secret key already exists in the credential manager
    var credential = CredentialManager.ReadCredential(credentialName);

    if (credential != null)
    {
        // If the secret key exists, prompt for the password
        Console.Write("Enter the password to access the secret key: ");
        string password = Console.ReadLine();

        if (string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Password cannot be empty.");
            return;
        }

        if (credential.Password == password)
        {
            secretKey = credential.UserName; // Use UserName as the secret key
        }
        else
        {
            Console.WriteLine("Incorrect password.");
            return;
        }
    }
    else
    {
        // If no secret key exists, generate a new one
        Console.Write("Enter a password to secure the new secret key: ");
        string password = Console.ReadLine();

        if (string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Password cannot be empty.");
            return;
        }

        secretKey = GenerateSecureKey(32); // Generate a 32-byte (256-bit) key

        // Save the newly generated secret key to the Credential Manager
        CredentialManager.WriteCredential(credentialName, secretKey, password, "Generated application secret key", CredentialPersistence.LocalMachine);

        Console.WriteLine($"Secret key '{credentialName}' has been created and saved with the provided password.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error accessing the secret key: " + ex.Message);
    return;
}

string currentDirectory = Directory.GetCurrentDirectory();
string folderPath = Path.Combine(currentDirectory, "Local");

// Create the 'Local' directory if it does not exist
Directory.CreateDirectory(folderPath);

string sealedFolderPath = Path.Combine(currentDirectory, "SealedCloud");
// Create the 'SealedCloud' directory if it does not exist
Directory.CreateDirectory(sealedFolderPath);

// Recursively process all files and subfolders in the 'Local' directory
var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
foreach (var file in files)
{
    if (Path.GetExtension(file) != ".7z" && Path.GetExtension(file) != ".exe")
    {
        // Determine the file path in the 'SealedCloud' structure
        string relativePath = Path.GetRelativePath(folderPath, file);
        string outputFilePath = Path.Combine(
            sealedFolderPath,
            Path.GetDirectoryName(relativePath),
            Path.GetFileNameWithoutExtension(file) + "_sealed.7z"
        );

        // Create necessary subfolders
        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

        try
        {
            ArchiveFile(file, outputFilePath, secretKey);
            File.Delete(file); // Delete the file after archiving
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}

// Clean up empty directories after processing
DeleteEmptyDirectories(folderPath);

static void ArchiveFile(string inputFile, string outputFile, string password)
{
    var command = $".\\7z.exe a -t7z \"{outputFile}\" \"{inputFile}\" -mhe=on -mx=9 -p\"{password}\"";

    var processStartInfo = new ProcessStartInfo
    {
        FileName = "cmd.exe",
        Arguments = $"/C {command}",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = false
    };

    using var process = Process.Start(processStartInfo);
    process.WaitForExit();
}

static void DeleteEmptyDirectories(string startLocation)
{
    foreach (var directory in Directory.GetDirectories(startLocation))
    {
        DeleteEmptyDirectories(directory);
        if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
        {
            Directory.Delete(directory);
        }
    }
}

static string GenerateSecureKey(int length)
{
    using (var rng = RandomNumberGenerator.Create())
    {
        byte[] keyBytes = new byte[length];
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }
}
