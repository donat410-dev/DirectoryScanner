using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryScanner
{
    public static class BuildHtml
    {
        private static readonly ConcurrentDictionary<string, (int count, long size)> Dictionary = new();
        private static readonly ConcurrentQueue<Task> Tasks = new();
        private static readonly StringBuilder Sb = new();

        private static void CheckMimeTypes(FileSystemElement element)
        {
            foreach (var item in element.ChildrenElements)
            {
                if (item.MimeType == "folder/folder")
                {
                    Tasks.Enqueue(Task.Run(() => CheckMimeTypes(item)));
                    continue;
                }

                Dictionary.AddOrUpdate(item.MimeType, (1, item.Size),
                    (_, tuple) => (tuple.count += 1, tuple.size += item.Size));
            }
        }

        private static string FormatFileSize(long size)
        {
            if (size > 1_099_511_627_776) //тера
            {
                return $"{size / 1_099_511_627_776.0:f2} Тб";
            }

            if (size > 1_073_741_824) //гига
            {
                return $"{size / 1_073_741_824.0:f2} Гб";
            }

            if (size > 1_048_576) //мега
            {
                return $"{size / 1_048_576.0:f2} Мб";
            }

            if (size > 1024) //кило
            {
                return $"{size / 1024.0:f2} Кб";
            }

            return $"{size} б";
        }

        public static void Start(FileSystemElement element, bool includeMoreData = false)
        {
            Sb.Append(
                "<style> html{font-size:1.5em;font-family:sans-serif;} li{list-style-type: none;} li:before{content: '|- ';} </style>");

            if (includeMoreData)
            {
                CheckMimeTypes(element);
                Sb.Append(
                    "<table style='text-align: center;'><tr><th>MIME-тип</th><th>Файлов данного типа</th><th>Средний вес файла в категории</th><th>Процентное отношение</th></tr> <col width='40%'> <col span='3' width='20%'>");

                Task.WaitAll(Tasks.ToArray());

                foreach (var (key, (count, size)) in Dictionary)
                {
                    Sb.Append(
                        $"<tr> <td style='text-align: left;'>{key}</td> <td>{count}</td> <td>{FormatFileSize(size / count)}</td> <td>{100.0 * count / FileSystemElement.TotalCountElements:f4} %</td> </tr>");
                }

                Sb.Append("</table>\n");
            }

            Sb.Append("Файловая структура<br>");
            FileManager(element);
            File.Delete("index.html");
            File.AppendAllText("index.html", Sb.ToString());
        }

        private static void FileManager(FileSystemElement element)
        {
            Sb.Append(
                $"|- {element.Name} <b>-dir</b> <i>({FormatFileSize(element.Size)})</i> <ul style='margin:0;padding-left:20px'>");
            foreach (var item in element.ChildrenElements)
            {
                if (item.MimeType == "folder/folder")
                {
                    FileManager(item);
                }
                else
                {
                    Sb.Append($"<li>{item.Name} <b>-file</b> ({FormatFileSize(item.Size)})</li>");
                }
            }

            Sb.Append("</ul>");
        }
    }
}