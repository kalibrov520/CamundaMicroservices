using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using FileLoader.FileSystem;
using Directory = FileLoader.FileSystem.Directory;

namespace FileLoader
{
    public class WildCardPatternChecker
    {
        private readonly List<Regex> RegexList = new List<Regex>();

        public WildCardPatternChecker(IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                var regex = new Regex(Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", "."));
                RegexList.Add(regex);
            }
        }

        public bool IsMatch(string path)
        {
            return RegexList.Any(r => r.IsMatch(path));
        }

        public bool IsMatchExtension(string path)
        {
            return RegexList.Any(r => r.IsMatch(new FileInfo(path).Extension));
        }

        public IEnumerable<IFileSystemItem> CheckMatches(IEnumerable<IFileSystemItem> items)
        {
            var result = new List<IFileSystemItem>();
            foreach (var item in items)
            {
                if (IsMatch(item.FullPath))
                {
                    result.Add(item);
                }

                if (item is Directory directory)
                {
                    result.AddRange(CheckMatches(directory.Items));
                }
            }

            return result;
        }

        public IEnumerable<IFileSystemItem> CheckFileExtensionMatches(IEnumerable<IFileSystemItem> items)
        {
            var result = new List<IFileSystemItem>();
            foreach (var item in items)
            {
                if (IsMatchExtension(item.FullPath))
                {
                    result.Add(item);
                }

                if (item is Directory directory)
                {
                    result.AddRange(CheckFileExtensionMatches(directory.Items));
                }
            }

            return result;
        }
    }
}