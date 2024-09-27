using System.Diagnostics;

class FileProcessor
{
    private const string LocalDirectoryName = "Local";
    private const string CloudDirectoryName = "SealedCloud";

    private readonly string _secretKey;
    private readonly string localDir;
    private readonly string cloudDir;

    public FileProcessor(string secretKey)
    {
        _secretKey = secretKey;
        localDir = Path.Combine(Directory.GetCurrentDirectory(), LocalDirectoryName);
        cloudDir = Path.Combine(Directory.GetCurrentDirectory(), CloudDirectoryName);

        Directory.CreateDirectory(localDir);
        Directory.CreateDirectory(cloudDir);
    }

    public void SyncLocalWithCloud()
    {
        ClearDirectory(localDir);
        CopyDirectory(cloudDir, localDir);
        Console.WriteLine("\nСинхронизация локальных данных с облаком завершена.");
    }

    public void SyncCloudWithLocal()
    {
        ClearDirectory(cloudDir, preserveSpecial: true);
        SealLocalData();
        CopyDirectory(localDir, cloudDir);
        Console.WriteLine("\nСинхронизация облачных данных с локальными завершена.");
    }

    public void SealLocalData()
    {
        long totalOriginalSize = 0;
        long totalSealedSize = 0;

        ProcessFiles(localDir, file =>
        {
            if (Path.GetExtension(file) != ".7z")
            {
                totalOriginalSize += new FileInfo(file).Length;

                var sealedFilePath = file + ".7z";
                ArchiveFile(file, sealedFilePath);
                File.Delete(file);

                totalSealedSize += new FileInfo(sealedFilePath).Length;

                var fileName = Path.GetFileName(file);
                var sealedFileName = Path.GetFileName(sealedFilePath);
                Console.WriteLine($"{fileName} -> {sealedFileName}");
            }
        });

        long spaceSaved = totalOriginalSize - totalSealedSize;
        Console.WriteLine($"Общий размер после запечатывания: {totalSealedSize / 1024.0 / 1024.0:F2} MB");
        Console.WriteLine($"Сэкономлено места на диске: {spaceSaved / 1024.0 / 1024.0:F2} MB");
    }


    public void UnsealLocalData()
    {
        ProcessFiles(localDir, sealedFile =>
        {
            if (Path.GetExtension(sealedFile) == ".7z")
            {
                var originalFilePath = sealedFile.Substring(0, sealedFile.Length - 3);
                if (!File.Exists(originalFilePath))
                {
                    ExtractFile(sealedFile, originalFilePath);
                    File.Delete(sealedFile);
                    var sealedFileName = Path.GetFileName(sealedFile);
                    var originalFileName = Path.GetFileName(originalFilePath);
                    Console.WriteLine($"{sealedFileName} -> {originalFileName}");
                }
            }
        });
    }

    private static void ProcessFiles(string directory, Action<string> fileAction)
    {
        var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            fileAction(file);
        }
    }

    private static void ClearDirectory(string directory, bool preserveSpecial = false)
    {
        foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
        {
            if (preserveSpecial && (File.GetAttributes(file) & FileAttributes.Hidden) != 0 || Path.GetFileName(file).StartsWith("."))
                continue;

            File.Delete(file);
        }

        foreach (var dir in Directory.GetDirectories(directory))
        {
            if (preserveSpecial && new DirectoryInfo(dir).Name.StartsWith("."))
                continue;

            Directory.Delete(dir, true);
        }
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var destDir = Path.Combine(targetDir, Path.GetFileName(directory));
            CopyDirectory(directory, destDir);
        }
    }

    private void ArchiveFile(string inputFile, string outputFile)
    {
        ExecuteCommand($".\\7z.exe a \"{outputFile}\" \"{inputFile}\" -mhe=on -mx=9 -p\"{_secretKey}\"");
    }

    private void ExtractFile(string inputFile, string outputFile)
    {
        ExecuteCommand($".\\7z.exe x \"{inputFile}\" -o\"{Path.GetDirectoryName(outputFile)}\" -p\"{_secretKey}\" -y");
    }

    private static void ExecuteCommand(string command)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {command}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        process.WaitForExit();
    }
}
