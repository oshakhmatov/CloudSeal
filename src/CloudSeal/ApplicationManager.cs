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
                    UnsealLocalData();
                    SyncLocalWithCloud();
                    break;
                case "2":
                    SyncCloudWithLocal();
                    break;
                case "3":
                    SealLocalData();
                    break;
                case "4":
                    UnsealLocalData();
                    break;
                case "5":
                    ManageSecretKey();
                    break;
                case "6":
                    ShowUsageInstructions();
                    break;
                case "7":
                    ShowAboutInformation();
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
        Console.WriteLine("3: Запечатать локальные данные");
        Console.WriteLine("4: Распечатать локальные данные");
        Console.WriteLine("5: Управление секретным ключом");
        Console.WriteLine("6: Как пользоваться");
        Console.WriteLine("7: О приложении");
    }

    private void ShowUsageInstructions()
    {
        Console.WriteLine("\nКак пользоваться:");
        Console.WriteLine("В папке SealedCloud будут храниться зашифрованные данные, укажите эту папку для любого облака.");
        Console.WriteLine("Например Яндекс.Диск, Google Диск или OneDrive от Microsoft. Все они позволяют выбрать локальную папку для синхронизации с облаком.");
        Console.WriteLine("a) Как работает синхронизация?");
        Console.WriteLine("Если мы синхронизируем локальные данные с облаком, это значит что мы целиком заменяем их на облычные.");
        Console.WriteLine("И наоборот. Поэтому не синхронизируйте локальные данные с облаком, если не хотите, чтобы они были удалены (заменены).");
        Console.WriteLine("b) Что значит запечатать данные?");
        Console.WriteLine("Запечатывание/распечатывание относится только к локальным данным. Облычные данные запечатаны всегда.");
        Console.WriteLine("Потому что нельзя допустить попадание открытых данных в любое облако.");
        Console.WriteLine("Эта функция полезна, если вы хотите защитить данные на своем ПК (и сэкономить место заодно).");
        Console.WriteLine("c) Вы можете управлять своим секретным ключом.");
        Console.WriteLine("Прежде всего - он хранится в защищенном хранилище Windows и закрыт вашим паролем.");
        Console.WriteLine("Рекомендуется сохранить его и сам пароль в надёжном менеджере паролей, ведь пароль можно легко забыть, а ключ потерять вместе с ПК.");
        Console.WriteLine("Если нужно по какой-то причине заменить ключ выполните следующие действия:");
        Console.WriteLine("Синхронизируйте локальные данные с облаком, распечатайте их и только после этого можно удалять ключ.");
        Console.WriteLine("Ни в коем случае не удаляйте ключ, если остались данные, которые запечатаны им. Сначала распечатайте их.");
    }

    private void ShowAboutInformation()
    {
        Console.WriteLine("\nЭто приложение - мощное средство для защиты и управления данными. Оно использует передовой алгоритм шифрования AES-256, который признан одним из самых безопасных в мире.");
        Console.WriteLine("Ваши данные надежно защищены даже в облаке благодаря этому сквозному шифрованию.");
        Console.WriteLine("Секретный ключ хранится с использованием самых современных методов защиты, что делает его недоступным для злоумышленников.");
        Console.WriteLine("Даже если ваши данные попадут в руки злоумышленников, они останутся полностью защищенными.");
        Console.WriteLine("Приложение предоставляет простоту использования и высочайший уровень безопасности для защиты данных.");
        Console.WriteLine("PS: всей жизни вселенной не хватит, чтобы расшифровать AES-256 даже при помощи квантового супер-компьютера.");
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

    private void SealLocalData()
    {
        var fileProcessor = new FileProcessor(_secretKey);
        fileProcessor.SealLocalData();
    }

    private void UnsealLocalData()
    {
        var fileProcessor = new FileProcessor(_secretKey);
        fileProcessor.UnsealLocalData();
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
                _secretKey = newSecretKey;
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
