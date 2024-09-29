using System.Collections.Concurrent;

namespace DirectoryScanner;

public class FileSystemElement
{
    public string Name { get; private init; }
    public string MimeType { get; private init; }
    public long Size { get; private set; }
    public ConcurrentBag<FileSystemElement> ChildrenElements { get; private init; } = [];
    public FileSystemElement ParentElement { get; init; }

    private void AddFiles(string dir)
    {
        try
        {
            Parallel.ForEach(Directory.GetFiles(dir), f =>
            {
                var tempFileInfo = new FileInfo(f);
                MimeTypes.TryGetMimeType(tempFileInfo.Name, out var mimeType);

                var fElement = new FileSystemElement
                {
                    ChildrenElements = null,
                    Size = tempFileInfo.Length,
                    Name = tempFileInfo.Name,
                    MimeType = mimeType ?? "undefined",
                    ParentElement = this
                };

                ChildrenElements.Add(fElement);
            });
        }
        catch (Exception _)
        {
            // ignored
        }
        finally
        {
            Size += ChildrenElements
                .Select(fElement => fElement.Size)
                .Sum();
        }
    }

    private void AddDirs(string dir)
    {
        try
        {
            Parallel.ForEach(Directory.GetDirectories(dir), d =>
            {
                var tempDirInfo = new DirectoryInfo(d);
                var dElement = new FileSystemElement
                {
                    MimeType = "folder/folder",
                    ParentElement = this,
                    Name = tempDirInfo.Name,
                };
                ChildrenElements.Add(dElement);
                dElement.AddDirs(d);
                dElement.AddFiles(d);
            });
        }
        catch (Exception _)
        {
            // ignored
        }
    }

    public static FileSystemElement GenTree(string dir)
    {
        var firstDir = new FileSystemElement
        {
            Name = dir,
            MimeType = "folder/folder"
        };
        firstDir.AddDirs(dir);
        firstDir.AddFiles(dir);

        return firstDir;
    }

    public static string FormatFileSize(long size)
        {
            return size switch
            {
                > 1_099_511_627_776 => $"{size / 1_099_511_627_776.0:F2} Тб",
                > 1_073_741_824 => $"{size / 1_073_741_824.0:F2} Гб",
                > 1_048_576 => $"{size / 1_048_576.0:F2} Мб",
                > 1024 => $"{size / 1024.0:F2} Кб",
                _ => $"{size} б"
            };
        }
}
