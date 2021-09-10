using System;
using System.Diagnostics;
using System.IO;

namespace DirectoryScanner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Начато сканирование директории");
            var stopwatch = Stopwatch.StartNew();
            var tree = FilesystemElement.GenTree(Directory.GetCurrentDirectory());
            stopwatch.Stop();
            Console.WriteLine($"Cканирование директории завершено, затрачено {stopwatch.ElapsedMilliseconds / 1000.0} sec");

            Console.WriteLine("Начато построение html документа");
            stopwatch.Restart();
            BuildHtml.Start(tree, true);
            stopwatch.Stop();
            Console.WriteLine($"Построение html документа завершено, затрачено {stopwatch.ElapsedMilliseconds / 1000.0}sec");
        }
    }
}
