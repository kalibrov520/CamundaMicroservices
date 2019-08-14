using System;
using System.Numerics;

namespace FileLoader.FileSystem
{
    public class File : IFileSystemItem, IEquatable<File>
    {
        public double Size { get; set; }

        public string FullPath { get; set; }

        public string Name { get; set; }

        public DateTime LastModified { get; set; }

        public bool Equals(File other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FullPath, other.FullPath) && string.Equals(Name, other.Name) && LastModified.Equals(other.LastModified);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((File) obj);
        }
    }
}