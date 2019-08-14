using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using FileLoader.FileSystem;
using Directory = FileLoader.FileSystem.Directory;

namespace FileLoader.FTP
{
    public class FtpFileLoader : IFileLoader
    {
        private string RootPath { get; set; }
        private string UserName { get; set; }
        private string Password { get; set; }

        private const string PathDelimeter = "/";

        public FtpFileLoader(string rootPath, string userName, string password)
        {
            RootPath = rootPath;
            UserName = userName;
            Password = password;
        }

        public IEnumerable<IFileSystemItem> GetFiles()
        {
            return LoadFromPath(RootPath);
        }

        public IEnumerable<IFileSystemItem> GetFilesWithPattern(IEnumerable<string> patterns)
        {
            var items = GetFiles();
            var checker = new WildCardPatternChecker(patterns);
            return checker.CheckMatches(items);
        }

        public IEnumerable<IFileSystemItem> GetFilesWithFileExtensionPattern(IEnumerable<string> patterns)
        {
            var items = GetFiles();
            var checker = new WildCardPatternChecker(patterns);
            return checker.CheckFileExtensionMatches(items);
        }
        
        private IEnumerable<IFileSystemItem> LoadFromPath(string path)
        {
            var request = CreateRequest(path);
            var response = request.GetResponse();
            var result = new List<IFileSystemItem>();
            var items = GetItemsFromResponse(response.GetResponseStream()).ToList();

            foreach (var item in items)
            {
                item.FullPath = $"{path}{PathDelimeter}{item.Name}";
            }
            result.AddRange(items);
            foreach (var directory in result.Where(x=> x is Directory).Cast<Directory>())
            {
                var newPath = $"{path}{PathDelimeter}{directory.Name}";
                var subItems = LoadFromPath(newPath);
                directory.Items.AddRange(subItems);
            }
            return result;
        }

        private IEnumerable<IFileSystemItem> GetItemsFromResponse(Stream responseStream)
        {
            var result = new List<IFileSystemItem>();

            using (var streamReader = new StreamReader(responseStream))
            {
                var line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    var item = FtpResponseParcer.ParseLine(line);
                    result.Add(item);
                    line = streamReader.ReadLine();
                }
            }

            return result;
        }

        private FtpWebRequest CreateRequest(string path)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
            request.Credentials = new NetworkCredential(UserName, Password);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.EnableSsl = true;
            ServicePointManager.ServerCertificateValidationCallback =
                (s, certificate, chain, sslPolicyErrors) => true;
            return request;
        }
    }
}
