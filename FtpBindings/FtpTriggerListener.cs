using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FtpBindings
{
    internal class FtpTriggerListener : GenericTriggerListener<ConnectionOptions, FtpMessage, FtpActionResult>
    {
        public FtpTriggerListener(ITriggeredFunctionExecutor executor, string functionId, ILogger logger, ConnectionOptions options, IValueProvider resultValueProvider)
            : base(executor, functionId, logger, options, resultValueProvider)
        {
        }

        /// <summary>
        /// call on each loop, and wait 10 seconds
        /// </summary>
        /// <param name="peviousReadElapsed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task PreviousReadMessages(TimeSpan peviousReadElapsed, CancellationToken cancellationToken)
        {
            //We define a polling to 10 seconds
            //wait 10 seconds between loop
            if (peviousReadElapsed.TotalSeconds < 10)
            {
                var timeToWait = TimeSpan.FromSeconds(10).Subtract(peviousReadElapsed);
                cancellationToken.WaitHandle.WaitOne(timeToWait);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// read file from ftp depending on user configuration
        /// </summary>
        /// <returns></returns>
        protected override async IAsyncEnumerable<FtpMessage> ReadMessages()
        {
            var creds = new NetworkCredential(_options.Username, _options.Password);
            //list files on FTP
            var fileNames = await ReadFileNamesAsync(creds);
            foreach (var fileName in fileNames)
            {
                //foreach each file read content

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{_options.Host}/{_options.InFolder}/{fileName}");
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;
                request.Credentials = creds;
                request.EnableSsl = _options.secureFTP;
                using FtpWebResponse responseContent = (FtpWebResponse)await request.GetResponseAsync();
                using (Stream responseContentStream = responseContent.GetResponseStream())
                {
                    //TODO: read on demand => implement async reader
                    var memStm = new MemoryStream();
                    responseContentStream.CopyTo(memStm);
                    yield return new FtpMessage()
                    {
                        FileName = fileName,
                        FileContent = memStm.ToArray()
                    };
                }
            }

        }

        protected override Task ProceedWithSuccessAsync(FtpMessage? message, FtpActionResult fnResult)
        {
            //delete or move file to OutFolder
            return base.ProceedWithSuccessAsync(message, fnResult);
        }

        protected override Task ProceedWithFailureAsync(FtpMessage? message)
        {
            //move file to ErrFolder
            return base.ProceedWithFailureAsync(message);
        }


        private async Task<IEnumerable<string>> ReadFileNamesAsync(NetworkCredential creds)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{_options.Host}/{_options.InFolder}");
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = creds;
            request.EnableSsl = _options.secureFTP;

            using FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();

            Stream responseStream = response.GetResponseStream();
            using StreamReader reader = new StreamReader(responseStream);
            var lsResult = reader.ReadToEnd();
            return lsResult.Split(System.Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }

    }
}