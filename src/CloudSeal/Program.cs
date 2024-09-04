using CloudSeal;

var secretKeyManager = new SecretKeyManager("CloudSealSecretKey");
var secretKey = secretKeyManager.GetOrCreateSecretKey();

if (string.IsNullOrEmpty(secretKey))
{
    Console.WriteLine("Operation aborted.");
    return;
}

var fileProcessor = new FileProcessor(secretKey, "Local", "SealedCloud");
fileProcessor.ProcessFiles();