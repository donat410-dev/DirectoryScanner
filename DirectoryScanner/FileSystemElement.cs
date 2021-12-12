using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DirectoryScanner
{
    public class FileSystemElement
    {
        public string Name { get; private init; }
        public string MimeType { get; private init; }
        public long Size { get; private set; }
        public ConcurrentBag<FileSystemElement> ChildrenElements { get; private init; } = new();
        private FileSystemElement ParentElement { get; init; }

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
            catch (Exception e)
            {
                // ignored
            }
            finally
            {
                Size += ChildrenElements
                    .Select(fElement => fElement.Size).Sum();
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
            catch (Exception e)
            {
                // ignored
            }
        }

        public static FileSystemElement GenTree(string dir)
        {
            var firstDir = new FileSystemElement
            {
                Name = dir.Split('\\').Last(),
                MimeType = "folder/folder"
            };
            firstDir.AddDirs(dir);
            firstDir.AddFiles(dir);

            return firstDir;
        }
    }
}