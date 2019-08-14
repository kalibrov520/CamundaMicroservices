using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileLoader.FileSystem;
using FileLoader.FTP;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace FileLoader.Tests
{
    [TestFixture]
    public class Tests
    {
        private static object[] _sourceLists =
        {
            new object[] {new List<string> {"*.txt"}},
            new object[] {new List<string> {"*.tsv"}},
            new object[] {new List<string> {"*.xls"}},
            new object[] {new List<string> {"*.csv"}}
        };

        [Test, TestCaseSource(nameof(_sourceLists))]
        public void PatternSearchFromFtpTest(List<string> patternsList)
        {
            var fileLoader = new FtpFileLoader("ftp://spb-mdspoc01.internal.corp/test", "ftpUser", "password123");

            var newFiles = fileLoader.GetFilesWithFileExtensionPattern(patternsList).ToList();

            Assert.AreEqual(1,newFiles.Count);
        }

        [Test, TestCaseSource(nameof(_sourceLists))]
        public void FileLoaderFullCycleTest(List<string> patternList)
        {
            var _updateStatus = false;

            var timer = new System.Timers.Timer(10000); // every 10 seconds

            var fileLoader = new FtpFileLoader("ftp://spb-mdspoc01.internal.corp/test", "ftpUser", "password123");

            var filesOnFtp = new List<IFileSystemItem>();

            //Update base file. It will guarantee, that event will be triggered, when there are actual changes on the Ftp
            using (var textWriter = new StreamWriter(".\\FilesList.txt"))
            {
                foreach (var item in fileLoader.GetFilesWithFileExtensionPattern(patternList))
                {
                    var lineToWrite = item.Name + " " + item.FullPath + " " + item.LastModified.ToString("g");
                    textWriter.WriteLineAsync(lineToWrite);
                }
            }

            using (var textReader = new StreamReader(".\\FilesList.txt"))
            {
                while (!textReader.EndOfStream)
                {
                    var lineProperties = textReader.ReadLine()?.Split(' ');
                    filesOnFtp.Add(new FileLoader.FileSystem.File
                    {
                        Name = lineProperties[0],
                        FullPath = lineProperties[1],
                        LastModified = Convert.ToDateTime(lineProperties[2] + " " + lineProperties[3] + lineProperties[4])
                    });
                }
            }

            timer.Elapsed += (source, e) =>
            {
                var newFiles = fileLoader.GetFilesWithFileExtensionPattern(patternList).ToList();

                if (!filesOnFtp.SequenceEqual(newFiles))
                {
                    using (var textWriter = new StreamWriter(".\\FilesList.txt"))
                    {
                        foreach (var item in newFiles)
                        {
                            var lineToWrite = item.Name + " " + item.FullPath + " " + item.LastModified.ToString("g");
                            textWriter.WriteLineAsync(lineToWrite);
                        }
                    }
                    _updateStatus = true;
                }
            };

            timer.Enabled = true;

            while (!_updateStatus)
            {

            }

            Assert.Pass();
        }
    }
}