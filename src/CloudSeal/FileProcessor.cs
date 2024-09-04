using System;
using System.Diagnostics;
using System.IO;

class FileProcessor
{
    private readonly string _secretKey;
    private readonly string _inputDirectory;
    private readonly string _outputDirectory;

    public FileProcessor(string secretKey, string inputDirectory, string outputDirectory)
    {
        _secretKey = secretKey;
        _inputDirectory = Path.Combine(Directory.GetCurrentDirectory(), inputDirectory);
        _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), outputDirectory);

        Directory.CreateDirectory(_inputDirectory);
        Directory.CreateDirectory(_outputDirectory);
    }

    // Синхронизация облачных данных с локальными
    public void SyncCloudWithLocal()
    {
        int addedFilesCount = 0;
        var files = Directory.GetFiles(_inputDirectory, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (Path.GetExtension(file) == ".7z" || Path.GetExtension(file) == ".exe") continue;

            var relativePath = Path.GetRelativePath(_inputDirectory, file);
            var outputFilePath = Path.Combine(_outputDirectory, Path.GetDirectoryName(relativePath), Path.GetFileName(file) + ".7z");

            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
            ArchiveFile(file, outputFilePath);
            addedFilesCount++;
        }

        // Удаляем из облака файлы, которых больше нет в локальной директории
        RemoveOrphanedFilesInCloud();

        Console.WriteLine($"\nСинхронизация завершена. Добавлено файлов: {addedFilesCount}");
    }

    // Синхронизация локальных данных с облаком
    public void SyncLocalWithCloud()
    {
        int addedFilesCount = 0;
        var sealedFiles = Directory.GetFiles(_outputDirectory, "*.7z", SearchOption.AllDirectories);
        foreach (var sealedFile in sealedFiles)
        {
            var relativePath = Path.GetRelativePath(_outputDirectory, sealedFile).Replace(".7z", "");
            var localFilePath = Path.Combine(_inputDirectory, relativePath);

            if (!File.Exists(localFilePath))
            {
                ExtractFile(sealedFile, localFilePath);
                addedFilesCount++;
            }
        }

        // Удаляем из локальной директории файлы, которые отсутствуют в облаке
        RemoveOrphanedFilesLocally();

        Console.WriteLine($"\nСинхронизация завершена. Добавлено файлов: {addedFilesCount}");
    }

    private void RemoveOrphanedFilesInCloud()
    {
        var sealedFiles = Directory.GetFiles(_outputDirectory, "*.7z", SearchOption.AllDirectories);
        foreach (var sealedFile in sealedFiles)
        {
            var relativePath = Path.GetRelativePath(_outputDirectory, sealedFile).Replace(".7z", "");
            var localFile = Path.Combine(_inputDirectory, relativePath);

            if (!File.Exists(localFile))
            {
                File.Delete(sealedFile); // Удалить архив, если исходный файл отсутствует
                Console.WriteLine($"Удален архивированный файл из облака: {sealedFile}");
            }
        }
    }

    private void RemoveOrphanedFilesLocally()
    {
        var localFiles = Directory.GetFiles(_inputDirectory, "*.*", SearchOption.AllDirectories);
        foreach (var localFile in localFiles)
        {
            var relativePath = Path.GetRelativePath(_inputDirectory, localFile);
            var cloudFilePath = Path.Combine(_outputDirectory, relativePath + ".7z");

            if (!File.Exists(cloudFilePath))
            {
                File.Delete(localFile); // Удалить локальный файл, если его нет в облаке
                Console.WriteLine($"Удален файл из локальной директории: {localFile}");
            }
        }
    }

    private void ArchiveFile(string inputFile, string outputFile)
    {
        var command = $".\\7z.exe a \"{outputFile}\" \"{inputFile}\" -mhe=on -mx=9 -p\"{_secretKey}\"";
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

    private void ExtractFile(string inputFile, string outputFile)
    {
        var outputPath = Path.GetDirectoryName(outputFile);
        Directory.CreateDirectory(outputPath);

        var command = $".\\7z.exe x \"{inputFile}\" -o\"{outputPath}\" -p\"{_secretKey}\" -y";
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
