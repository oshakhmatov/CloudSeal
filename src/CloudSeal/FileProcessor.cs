using System.Diagnostics;

namespace CloudSeal;

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

    public void ProcessFiles()
    {
        var files = Directory.GetFiles(_inputDirectory, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (Path.GetExtension(file) == ".7z" || Path.GetExtension(file) == ".exe") continue;

            var relativePath = Path.GetRelativePath(_inputDirectory, file);
            var outputFilePath = Path.Combine(
                _outputDirectory,
                Path.GetDirectoryName(relativePath),
                Path.GetFileNameWithoutExtension(file) + "_sealed.7z"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
            ArchiveFile(file, outputFilePath);
            File.Delete(file);
        }

        DeleteEmptyDirectories(_inputDirectory);
    }

    private void ArchiveFile(string inputFile, string outputFile)
    {
        var command = $".\\7z.exe a -t7z \"{outputFile}\" \"{inputFile}\" -mhe=on -mx=9 -p\"{_secretKey}\"";
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

    private void DeleteEmptyDirectories(string startLocation)
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
}
