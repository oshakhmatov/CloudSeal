Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var secretKeyManager = new SecretKeyManager();
var appManager = new ApplicationManager(secretKeyManager);
appManager.Run();