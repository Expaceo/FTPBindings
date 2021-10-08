using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FtpBindings;

namespace AFFtpBindings
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static Task Run([FtpTrigger("myFtpConfig")] FtpMessage req, ILogger log)
        {
            log.LogInformation($"C# FTP trigger function processed a file {req.FileName}, size:{req.FileContent.Length}.");
            return Task.CompletedTask;
        }


        [FunctionName("Function2")]
        public static Task<FtpActionResult> Function2(
            [FtpTrigger("myFtpConfig")] FtpMessage req,
            ILogger log)
        {
            log.LogInformation($"C# FTP trigger function processed a file {req.FileName}, size:{req.FileContent.Length}.");
            throw new Exception("tagada");
            return Task.FromResult(new FtpActionResult(FtpActionResultEnum.MoveFileToOut));
        }
    }
}
