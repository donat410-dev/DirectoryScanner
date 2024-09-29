namespace DirectoryScanner;

public static class WriteToConsole
{
    public static void Start(FileSystemElement element)
    {
        Console.WriteLine("Файловая структура");
        BuildFileManager(element);
    }

    private static void BuildFileManager(FileSystemElement element, string indent = "")
    {
        PrintColoredMessages([
            ($"{indent}|- ", null),
            (element.Name, ConsoleColor.Blue),
            ($" ({FileSystemElement.FormatFileSize(element.Size)})", ConsoleColor.Cyan),
        ]);

        indent += "| ";
        foreach (var item in element.ChildrenElements)
        {
            if (item.MimeType == "folder/folder")
            {
                BuildFileManager(item, indent);
            }
            else
            {
                PrintColoredMessages([
                    ($"{indent}|- ", null),
                    (item.Name, ConsoleColor.Magenta),
                    ($" ({FileSystemElement.FormatFileSize(element.Size)}) ", ConsoleColor.Cyan),
                    (item.MimeType, ConsoleColor.Green),
                ]);
            }
        }
    }

    private static void PrintColoredMessages((string message, ConsoleColor? color)[] messagesWithColors)
    {
        var defaultColor = Console.ForegroundColor;

        foreach (var (message, color) in messagesWithColors)
        {
            Console.ForegroundColor = color ?? defaultColor;

            Console.Write(message);
        }
        Console.WriteLine();
        Console.ForegroundColor = defaultColor;
    }
}
