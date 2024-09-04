using System;
using System.IO;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8; // Переключение на Unicode
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var secretKeyManager = new SecretKeyManager("CloudSealSecretKey");
        string secretKey = InitializeSecretKey(secretKeyManager);

        if (string.IsNullOrEmpty(secretKey))
        {
            Console.WriteLine("Операция отменена.");
            return;
        }

        while (true) // Цикл для повторного отображения меню
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1: Сохранить данные");
            Console.WriteLine("2: Прочитать данные");
            Console.WriteLine("3: Управление секретным ключом");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    var fileProcessor = new FileProcessor(secretKey, "Local", "SealedCloud");
                    fileProcessor.ProcessFiles();
                    break;
                case "2":
                    fileProcessor = new FileProcessor(secretKey, "View", "SealedCloud");
                    fileProcessor.ReadUserSelectedFiles();
                    break;
                case "3":
                    ManageSecretKey(secretKeyManager);
                    secretKey = InitializeSecretKey(secretKeyManager); // Перезапросить ключ после изменения
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    static string InitializeSecretKey(SecretKeyManager secretKeyManager)
    {
        string secretKey = secretKeyManager.GetOrCreateSecretKey();

        while (string.IsNullOrEmpty(secretKey))
        {
            Console.WriteLine("Неверный пароль.");
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1: Повторить ввод пароля");
            Console.WriteLine("2: Удалить ключ и создать новый");
            var choice = Console.ReadLine();

            if (choice == "2")
            {
                secretKeyManager.DeleteSecretKey();
                secretKey = secretKeyManager.CreateNewSecretKey();
                Console.WriteLine($"Новый секретный ключ: {secretKey}");
                Console.WriteLine("Рекомендуется сохранить этот ключ в надёжном менеджере паролей на случай утери ПК.");
                break;
            }
            else
            {
                secretKey = secretKeyManager.GetOrCreateSecretKey();
            }
        }

        return secretKey;
    }

    static void ManageSecretKey(SecretKeyManager secretKeyManager)
    {
        Console.WriteLine($"Текущий секретный ключ: {secretKeyManager.GetSecretKeyName()}");
        Console.WriteLine($"Секретный ключ: {secretKeyManager.GetSecretKey()}");

        Console.WriteLine("Выберите действие:");
        Console.WriteLine("1: Вернуться");
        Console.WriteLine("2: Удалить секретный ключ");
        var choice = Console.ReadLine();

        if (choice == "2")
        {
            Console.WriteLine("ВНИМАНИЕ: Удаление ключа сделает недоступными все зашифрованные им данные.");
            Console.Write("Удалить текущий ключ и создать новый? (да/нет): ");
            var confirmation = Console.ReadLine();

            if (confirmation.ToLower() == "да")
            {
                secretKeyManager.DeleteSecretKey();
                var newSecretKey = secretKeyManager.CreateNewSecretKey();
                Console.WriteLine($"Новый секретный ключ: {newSecretKey}");
                Console.WriteLine("Рекомендуется сохранить этот ключ в надёжном менеджере паролей на случай утери ПК.");
            }
            else
            {
                Console.WriteLine("Операция отменена. Возврат в меню.");
            }
        }
        else
        {
            Console.WriteLine("Возврат в главное меню.");
        }
    }
}
