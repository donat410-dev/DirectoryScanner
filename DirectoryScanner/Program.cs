using System;
using System.IO;

namespace DirectoryScanner
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Начато сканирование директории");
            var tree = FileSystemElement.GenTree(Directory.GetCurrentDirectory());
            Console.WriteLine("Cканирование директории завершено");

            Console.WriteLine("Начато построение html документа");
            BuildHtml.Start(tree, true);
            Console.WriteLine("Построение html документа завершено");
            Console.ReadKey();
        }
    }
}