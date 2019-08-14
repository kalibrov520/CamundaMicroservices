using System.Collections.Generic;
using FileLoader.FileSystem;

namespace FileLoader
{
    public interface IFileLoader
    {
        IEnumerable<IFileSystemItem> GetFiles();
    }
}
