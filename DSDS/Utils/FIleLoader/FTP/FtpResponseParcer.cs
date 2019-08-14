using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using FileLoader.FileSystem;

namespace FileLoader.FTP
{
    public class FtpResponseParcer
    {
        private const char ParserDelimeter = ' ';
        private const string DirString = "<DIR>";


        public static IFileSystemItem ParseLine(string responseLine)
        {
            if (string.IsNullOrEmpty(responseLine))
                return null;

            var items = responseLine.Split(ParserDelimeter).Where(x => !string.IsNullOrEmpty(x)).ToList();

            if (items.Count < 4)
                return null;
            
            var result = items[2] == DirString ? CreateDirectory(items) : CreateFile(items);
            var dateString = $"{items[0]} {items[1]}";
            var lastModifiedDateTime = DateTime.ParseExact(dateString, "MM-dd-yy hh:mmtt", CultureInfo.InvariantCulture);
            result.LastModified = lastModifiedDateTime;
            return result;
        }


        private static IFileSystemItem CreateDirectory(IList<string> items)
        {
            var directory = new Directory
            {
                Name = items[3],
                Items = new List<IFileSystemItem>()
            };
            return directory;
        }

        private static IFileSystemItem CreateFile(IList<string> items)
        {
            var file = new File
            {
                Name = items[3],
                Size = double.Parse(items[2])
            };
            return file;
        }
    }
}