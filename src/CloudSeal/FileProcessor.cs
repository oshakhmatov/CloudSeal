using System;
using System.Diagnostics;
using System.IO;

class FileProcessor
{
    private const string LocalDirectoryName = "Local";
    private const string CloudDirectoryName = "SealedCloud";

    private readonly string _secretKey;
    private readonly string _inputDirectory;
    private readonly string _outputDirectory;

    public FileProcessor(string secretKey)
    {
        _secretKey = secretKey;
        _inputDirectory = Path.Combine(Directory.GetCurrentDirectory(), LocalDirectoryName);
        _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), CloudDirectoryName);

        Directory.CreateDirectory(_inputDirectory);
        Directory.CreateDirectory(_outputDirectory);
    }

    public void SyncCloudWithLocal()
    {
        SyncFiles(_inputDirectory, _outputDirectory, isToCloud: false);
    }

    public void SyncLocalWithCloud()
    {
        SyncFiles(_inputDirectory, _outputDirectory, isToCloud: true);
    }

    private void SyncFiles(string sourceDirectory, string targetDirectory, bool isToCloud)
    {
        int addedFilesCount = 0;

        if (isToCloud)
        {
            // Синхронизация из локальной директории в облако
            var files = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".7z" || Path.GetExtension(file) == ".exe") continue;

                var relativePath = Path.GetRelativePath(sourceDirectory, file);
                var outputFilePath = Path.Combine(targetDirectory, Path.GetDirectoryName(relativePath), Path.GetFileName(file) + ".7z");

                Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
                ArchiveFile(file, outputFilePath);
                addedFilesCount++;
            }
            RemoveOrphanedFiles(sourceDirectory, targetDirectory, isToCloud);
        }
        else
        {
            // Синхронизация из облака в локальную директорию
            var sealedFiles = Directory.GetFiles(targetDirectory, "*.7z", SearchOption.AllDirectories);
            foreach (var sealedFile in sealedFiles)
            {
                var relativePath = Path.GetRelativePath(targetDirectory, sealedFile).Replace(".7z", "");
                var localFilePath = Path.Combine(sourceDirectory, relativePath);

                ExtractFile(sealedFile, localFilePath);
                addedFilesCount++;
            }
            RemoveOrphanedFiles(sourceDirectory, targetDirectory, isToCloud);
        }

        Console.WriteLine($"\nСинхронизация завершена. Добавлено файлов: {addedFilesCount}");
    }

    private void RemoveOrphanedFiles(string sourceDirectory, string targetDirectory, bool isToCloud)
    {
        if (isToCloud)
        {
            var sealedFiles = Directory.GetFiles(targetDirectory, "*.7z", SearchOption.AllDirectories);
            foreach (var sealedFile in sealedFiles)
            {
                var relativePath = Path.GetRelativePath(targetDirectory, sealedFile).Replace(".7z", "");
                var localFile = Path.Combine(sourceDirectory, relativePath);

                if (!File.Exists(localFile))
                {
                    File.Delete(sealedFile);
                    Console.WriteLine($"Удален архивированный файл из облака: {sealedFile}");
                }
            }
        }
        else
        {
            var localFiles = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);
            foreach (var localFile in localFiles)
            {
                var relativePath = Path.GetRelativePath(sourceDirectory, localFile);
                var cloudFilePath = Path.Combine(targetDirectory, relativePath + ".7z");

                if (!File.Exists(cloudFilePath))
                {
                    File.Delete(localFile);
                    Console.WriteLine($"Удален файл из локальной директории: {localFile}");
                }
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
