using System;

namespace FileLoader.FileSystem
{
    public interface IFileSystemItem
    {
        string FullPath { get; set; }

        string Name { get; set; }

        DateTime LastModified { get; set; }
    }
}