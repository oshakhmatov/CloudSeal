using System.Diagnostics;

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

    public void ReadFiles()
    {
        var files = Directory.GetFiles(_outputDirectory, "*.7z", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(_outputDirectory, file);
            var extractPath = Path.Combine(_inputDirectory, Path.GetDirectoryName(relativePath));

            Directory.CreateDirectory(extractPath);
            ExtractFile(file, extractPath);
        }
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

    private void ExtractFile(string inputFile, string extractPath)
    {
        var command = $".\\7z.exe x \"{inputFile}\" -o\"{extractPath}\" -p\"{_secretKey}\" -y";
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

    public void ReadUserSelectedFiles()
    {
        var directories = Directory.GetDirectories(_outputDirectory)
            .Where(dir => !Path.GetFileName(dir).StartsWith("."))
            .ToArray();

        if (directories.Length == 0 || (directories.Length == 1 && Directory.GetFiles(_outputDirectory).Length == 0))
        {
            ReadFiles(_outputDirectory);
        }
        else
        {
            Console.WriteLine("Select a folder to view:");
            Console.WriteLine("0: View all");

            for (int i = 0; i < directories.Length; i++)
            {
                var dir = directories[i];
                var fileCount = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).Length;
                Console.WriteLine($"{i + 1}: {Path.GetFileName(dir)} ({fileCount} files)");
            }

            if (int.TryParse(Console.ReadLine(), out int selectedIndex))
            {
                if (selectedIndex == 0)
                {
                    ReadFiles(_outputDirectory);
                }
                else if (selectedIndex > 0 && selectedIndex <= directories.Length)
                {
                    ReadFiles(directories[selectedIndex - 1]);
                }
                else
                {
                    Console.WriteLine("Invalid selection. Exiting.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Exiting.");
                return;
            }
        }

        OpenFolderInExplorer(_inputDirectory);
    }

    private void ReadFiles(string pathToRead)
    {
        var files = Directory.GetFiles(pathToRead, "*.7z", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(_outputDirectory, file);
            var extractPath = Path.Combine(_inputDirectory, Path.GetDirectoryName(relativePath));

            Directory.CreateDirectory(extractPath);
            ExtractFile(file, extractPath);
        }
    }

    private void OpenFolderInExplorer(string path)
    {
        Process.Start("explorer.exe", path);
    }
}

