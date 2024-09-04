using System.Diagnostics;

Console.Write("Password: ");
string password = Console.ReadLine();

if (string.IsNullOrEmpty(password))
{
    Console.WriteLine("Password is wrong");
    return;
}

string currentDirectory = Directory.GetCurrentDirectory();
string folderPath = Path.Combine(currentDirectory, "Local");

Directory.CreateDirectory(folderPath);

string sealedFolderPath = Path.Combine(currentDirectory, "SealedCloud");
Directory.CreateDirectory(sealedFolderPath);

var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
foreach (var file in files)
{
    if (Path.GetExtension(file) != ".7z" && Path.GetExtension(file) != ".exe")
    {
        // Определяем путь файла в структуре SealedCloud
        string relativePath = Path.GetRelativePath(folderPath, file);
        string outputFilePath = Path.Combine(
            sealedFolderPath,
            Path.GetDirectoryName(relativePath),
            Path.GetFileNameWithoutExtension(file) + "_sealed.7z"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

        try
        {
            ArchiveFile(file, outputFilePath, password);
            File.Delete(file);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}

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
