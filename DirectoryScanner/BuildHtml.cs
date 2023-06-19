using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryScanner
{
    public static class BuildHtml
    {
        private static readonly ConcurrentDictionary<string, (int count, long size)> _dictionary = new();
        private static readonly ConcurrentQueue<Task> _tasks = new();
        private static readonly StringBuilder _stringBuilder = new();

        private static void CheckMimeTypes(FileSystemElement element)
        {
            foreach (var item in element.ChildrenElements)
            {
                if (item.MimeType == "folder/folder")
                {
                    _tasks.Enqueue(Task.Run(() => CheckMimeTypes(item)));
                    continue;
                }

                _dictionary.AddOrUpdate(item.MimeType, (1, item.Size),
                    (_, tuple) => (tuple.count += 1, tuple.size += item.Size));
            }
        }

        private static string FormatFileSize(long size)
        {
            return size > 1_099_511_627_776 ? $"{size / 1_099_511_627_776.0:f2} Тб"
                : size > 1_073_741_824 ? $"{size / 1_073_741_824.0:f2} Гб"
                : size > 1_048_576 ? $"{size / 1_048_576.0:f2} Мб"
                : size > 1024 ? $"{size / 1024.0:f2} Кб" : $"{size} б";
        }

        public static void Start(FileSystemElement element, bool includeMoreData = false)
        {
            _stringBuilder.Append(
                "<style> html{font-size:1.5em;font-family:sans-serif;} li{list-style-type: none;} li:before{content: '|- ';} </style>");

            if (includeMoreData)
            {
                CheckMimeTypes(element);
                _stringBuilder.Append(
                    "<table style='text-align: center;'><tr><th>MIME-тип</th><th>Файлов данного типа</th><th>Средний вес файла в категории</th><th>Процентное отношение</th></tr> <col width='40%'> <col span='3' width='20%'>");

                Task.WaitAll(_tasks.ToArray());

                foreach (var (key, (count, size)) in _dictionary)
                {
                    _stringBuilder.Append(
                        $"<tr> <td style='text-align: left;'>{key}</td> <td>{count}</td> <td>{FormatFileSize(size / count)}</td> <td>{100.0 * count / FileSystemElement.TotalCountElements:f4} %</td> </tr>");
                }

                _stringBuilder.Append("</table>\n");
            }

            _stringBuilder.Append("Файловая структура<br>");
            FileManager(element);
            File.WriteAllText("index.html", _stringBuilder.ToString());
        }

        private static void FileManager(FileSystemElement element)
        {
            _stringBuilder.Append(
                $"|- {element.Name} <b>-dir</b> <i>({FormatFileSize(element.Size)})</i> <ul style='margin:0;padding-left:20px'>");
            foreach (var item in element.ChildrenElements)
            {
                if (item.MimeType == "folder/folder")
                {
                    FileManager(item);
                }
                else
                {
                    _stringBuilder.Append($"<li>{item.Name} <b>-file</b> ({FormatFileSize(item.Size)})</li>");
                }
            }

            _stringBuilder.Append("</ul>");
        }
    }
}