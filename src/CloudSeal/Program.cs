var secretKeyManager = new SecretKeyManager("CloudSealSecretKey");
var secretKey = secretKeyManager.GetOrCreateSecretKey();

if (string.IsNullOrEmpty(secretKey))
{
    Console.WriteLine("Operation aborted.");
    return;
}

Console.WriteLine("Would you like to (1) Save data or (2) Read data?");
var choice = Console.ReadLine();

if (choice == "1")
{
    var fileProcessor = new FileProcessor(secretKey, "Local", "SealedCloud");
    fileProcessor.ProcessFiles();
}
else if (choice == "2")
{
    var fileProcessor = new FileProcessor(secretKey, "View", "SealedCloud");
    fileProcessor.ReadUserSelectedFiles();
}
else
{
    Console.WriteLine("Invalid option. Exiting.");
}