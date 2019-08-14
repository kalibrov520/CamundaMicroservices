using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Camunda.Worker;
using FileLoader.FileSystem;
using FileLoader.FTP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using File = System.IO.File;

namespace FtpWatcherService.Handlers
{
    [HandlerTopics("FtpWatcher")]
    public class FtpWatcherHandler : ExternalTaskHandler
    {
        private bool _updateStatus = false;

        public override Task<IExecutionResult> Process(ExternalTask externalTask)
        {
            var timer = new System.Timers.Timer(10000); // every 10 seconds

            var fileLoader = new FtpFileLoader(externalTask.Variables["rootPath"].AsString(),externalTask.Variables["userName"].AsString(),externalTask.Variables["password"].AsString());

            var patterns = externalTask.Variables["patternsForExtension"].AsString().Trim().Split(',').ToList();

            var filesOnFtp = new List<IFileSystemItem>();

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
                var newFiles = fileLoader.GetFilesWithFileExtensionPattern(patterns).ToList();

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

            timer.Enabled = false;

            return Task.FromResult<IExecutionResult>(new CompleteResult(new Dictionary<string, Variable>()
            {
                ["downloadFile"] = Variable.Bytes(File.ReadAllBytes("C:\\Users\\ikalibrov\\Desktop\\SS_Download.bat")),
                ["reformatFile"] = Variable.Bytes(File.ReadAllBytes("C:\\Users\\ikalibrov\\Desktop\\Reformat_Process.bat")),
                ["autoloadFile"] = Variable.Bytes(File.ReadAllBytes("C:\\Users\\ikalibrov\\Desktop\\Autoload_Process.bat"))
            }));
        }
    }
}