using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryScanner
{
    public static class BuildHtml
    {
        private static readonly ConcurrentDictionary<string, (int count, long size)> Dictionary = new();
        private static readonly ConcurrentBag<Task> Tasks = new();
        private static int _totalCount;
        private static string _html = "";

        private static void CheckMimeTypes(FileSystemElement element)
        {
            foreach (var item in element.GetChildrenElements())
            {
                if (item.MimeType == "folder/folder")
                {
                    Tasks.Add(Task.Run(() => CheckMimeTypes(item)));
                    continue;
                }
                Dictionary.AddOrUpdate(item.MimeType ?? "undefined", (1, item.Size),
                    (_, tuple) => (tuple.count += 1, tuple.size += item.Size));
                Interlocked.Increment(ref _totalCount);
            }
        }

        private static string FormatFileSize(long size)
        {
            if (size > 1_099_511_627_776)//тера
            {
                return $"{size / 1_099_511_627_776.0:f2} Тб";
            }
            if (size > 1_073_741_824)//гига
            {
                return $"{size / 1_073_741_824.0:f2} Гб";
            }
            if (size > 1_048_576)//мега
            {
                return $"{size / 1_048_576.0:f2} Мб";
            }
            if (size > 1024)//кило
            {
                return $"{size / 1024.0:f2} Кб";
            }
            return $"{size} б";
        }
        public static void Start(FileSystemElement element, bool includeMoreData = false)
        {
            _html += "<style> " +
                     "html{font-size:1.5em;font-family:sans-serif;}" +
                     "li{list-style-type: none;}" +
                     "li:before{content: '|- ';}" +
                     "</style>";
            if (includeMoreData)
            {
                CheckMimeTypes(element);
                _html += "<table style='text-align: center;'><tr><th>MIME-тип</th><th>Файлов данного типа</th><th>Средний вес файла в категории</th><th>Процентное отношение</th></tr>";
                _html += "<col width='40%'>";
                _html += "<col span='3' width='20%'>";

                Task.WaitAll(Tasks.ToArray());

                foreach (var (key, (count, size)) in Dictionary)
                {
                    _html += "<tr>";
                    _html += $"<td style='text-align: left;'>{key}</td>";
                    _html += $"<td>{count}</td>";
                    _html += $"<td>{FormatFileSize(size / count)}</td>";
                    _html += $"<td>{100.0 * count / _totalCount:f4} %</td>";
                    _html += "</tr>";
                }
                _html += "</table>\n";
            }

            _html += "Файловая структура<br>";
            FileManager(element);
            File.Delete("index.html");
            File.AppendAllText("index.html", _html);
        }

        private static void FileManager(FileSystemElement element)
        {
            _html += $"|- {element.Name} <b>-dir</b> <i>({FormatFileSize(element.Size)})</i> <ul style='margin:0;padding-left:20px'>";
            foreach (var item in element.GetChildrenElements())
            {
                if (item.MimeType == "folder/folder")
                {
                    FileManager(item);
                }
                else
                {
                    _html += $"<li>{item.Name} <b>-file</b> ({FormatFileSize(item.Size)})</li>";
                }
            }
            _html += "</ul>";
        }
    }
}