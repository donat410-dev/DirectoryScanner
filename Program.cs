using DirectoryScanner;

if (args.Length == 0)
{
    Console.WriteLine("Ошибка: не указан путь до директории.");
    Console.WriteLine("Использование: программа.exe <path_to_directory> [--console | --html <path_to_file>]");
    return;
}

var directoryPath = args[0];
Console.WriteLine("Начато сканирование директории");
var tree = FileSystemElement.GenTree(directoryPath);
Console.WriteLine("Cканирование директории завершено");

var htmlMode = false;
string htmlFilePath = null;

for (var i = 1; i < args.Length; i++)
{
    if (args[i] == "--html")
    {
        htmlMode = true;
        if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
        {
            htmlFilePath = args[i + 1];
            i++; // пропустить следующий аргумент, т.к. это путь до файла
        }
    }
    else if (args[i] == "--console")
    {
        WriteToConsole.Start(tree);
    }
    else
    {
        Console.WriteLine("Ошибка: не указан путь до директории.");
        Console.WriteLine("Использование: программа.exe <path_to_directory> [--console | --html <path_to_file>]");
        return;
    }
}

if (htmlMode)
{
    Console.WriteLine("Начато построение html документа");
    if (htmlFilePath != null)
    {
        WriteToHtml.Start(tree, htmlFilePath);
    }
    else
    {
        WriteToHtml.Start(tree);
    }
    Console.WriteLine("Построение html документа завершено");
}
