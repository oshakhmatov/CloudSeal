using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ApplicationManager
{
    private readonly SecretKeyManager _secretKeyManager;
    private string _secretKey;

    public ApplicationManager(SecretKeyManager secretKeyManager)
    {
        _secretKeyManager = secretKeyManager;
    }

    public void Run()
    {
        _secretKey = InitializeSecretKey();

        if (string.IsNullOrEmpty(_secretKey))
        {
            Console.WriteLine("Операция отменена.");
            return;
        }

        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    SyncLocalWithCloud();
                    break;
                case "2":
                    SyncCloudWithLocal();
                    break;
                case "3":
                    ManageSecretKey();
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    private void ShowMenu()
    {
        Console.WriteLine("\nВыберите действие:");
        Console.WriteLine("1: Синхронизировать локальные данные с облаком");
        Console.WriteLine("2: Синхронизировать облако с локальными данными");
        Console.WriteLine("3: Управление секретным ключом");
    }

    private string InitializeSecretKey()
    {
        var secretKey = _secretKeyManager.GetOrCreateSecretKey();

        while (string.IsNullOrEmpty(secretKey))
        {
            Console.WriteLine("Неверный пароль.");
            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("1: Повторить ввод пароля");
            Console.WriteLine("2: Удалить ключ и создать новый");
            var choice = Console.ReadLine();

            if (choice == "2")
            {
                _secretKeyManager.DeleteSecretKey();
                secretKey = _secretKeyManager.CreateNewSecretKey();
                Console.WriteLine($"Новый секретный ключ: {secretKey}");
                Console.WriteLine("Рекомендуется сохранить этот ключ в надёжном менеджере паролей на случай утери ПК.");
                break;
            }
            else
            {
                secretKey = _secretKeyManager.GetOrCreateSecretKey();
            }
        }

        return secretKey;
    }

    private void SyncLocalWithCloud()
    {
        var fileProcessor = new FileProcessor(_secretKey);
        fileProcessor.SyncLocalWithCloud();
    }

    private void SyncCloudWithLocal()
    {
        var fileProcessor = new FileProcessor(_secretKey);
        fileProcessor.SyncCloudWithLocal();
    }

    private void ManageSecretKey()
    {
        Console.WriteLine($"Текущий секретный ключ: {_secretKeyManager.GetSecretKeyName()}");
        Console.WriteLine($"Секретный ключ: {_secretKeyManager.GetSecretKey()}");

        Console.WriteLine("\nВыберите действие:");
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
                _secretKeyManager.DeleteSecretKey();
                var newSecretKey = _secretKeyManager.CreateNewSecretKey();
                Console.WriteLine($"Новый секретный ключ: {newSecretKey}");
                Console.WriteLine("Рекомендуется сохранить этот ключ в надёжном менеджере паролей на случай утери ПК.");
                _secretKey = newSecretKey; // обновляем текущий ключ
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