using System;
using System.Diagnostics;
using System.IO;

if (args.Length < 2)
{
    Console.WriteLine("Usage: cloudseal <password> <file_path>");
    //return;
}

string password = args[0];
string filePath = args[1];
//string password = "1234";
//string filePath = @"C:\test\test.txt";

if (!File.Exists(filePath))
{
    Console.WriteLine("File not found: " + filePath);
    return;
}

string outputFilePath = Path.Combine(
    Path.GetDirectoryName(filePath),
    Path.GetFileNameWithoutExtension(filePath) + "_sealed.7z"
);

try
{
    ArchiveFile(filePath, outputFilePath, password);
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred: " + ex.Message);
}

static void ArchiveFile(string inputFile, string outputFile, string password)
{
    string command = $".\\7z.exe a -t7z \"{outputFile}\" \"{inputFile}\" -mhe=on -mx=9 -p\"{password}\"";

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
