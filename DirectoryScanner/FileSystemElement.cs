using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryScanner
{
    public class FileSystemElement
    {
        public string Name { get; private set; }
        public string MimeType { get; private set; }
        public long Size { get; private set; }
        private ConcurrentBag<FileSystemElement> _childrenElements = new();
        private FileSystemElement ParentElement { get; set; }

        public ConcurrentBag<FileSystemElement> GetChildrenElements()
        {
            return _childrenElements;
        }

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
                        _childrenElements = null,
                        Size = tempFileInfo.Length,
                        Name = tempFileInfo.Name,
                        MimeType = mimeType,
                        ParentElement = this
                    };

                    this._childrenElements.Add(fElement);
                });
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        private void AddDirs(string dir)
        {
            try
            {
                Parallel.ForEach(Directory.GetDirectories(dir), new ParallelOptions {MaxDegreeOfParallelism = 10}, d =>
                {
                    var tempDirInfo = new DirectoryInfo(d);
                    var dElement = new FileSystemElement
                    {
                        MimeType = "folder/folder",
                        ParentElement = this,
                        Name = tempDirInfo.Name,
                        Size = GetDirSize(d),
                    };
                    dElement.AddFiles(d);
                    dElement.AddDirs(d);

                    this._childrenElements.Add(dElement);
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
                Size = GetDirSize(dir),
                MimeType = "folder/folder"
            };

            firstDir.AddFiles(dir);
            firstDir.AddDirs(dir);
            return firstDir;
        }

        private static long GetDirSize(string path)
        {
            long size = 0;
            try
            {
                var files = Directory.GetFiles(path);

                Parallel.ForEach(files, file =>
                    Interlocked.Add(ref size, new FileInfo(file).Length));

                var dirs = Directory.GetDirectories(path);

                Parallel.ForEach(dirs, dir =>
                    Interlocked.Add(ref size, GetDirSize(dir)));
                return size;
            }
            catch (Exception e)
            {
                // ignored
            }

            return size;
        }
    }
}