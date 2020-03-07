using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyServer
{
    public class NumbersService : Numerics.NumericsBase
    {
        private readonly ILogger<NumbersService> _logger;
        private readonly IWebHostEnvironment _webenv;
        public NumbersService(ILogger<NumbersService> logger, IWebHostEnvironment webenv)
        {
            _logger = logger;
            _webenv = webenv;
        }

        public override async Task<NumberResponse> SendNumber(IAsyncStreamReader<NumberRequest> requestStream, ServerCallContext context)
        {
            var total = 0;

            await foreach (var number in requestStream.ReadAllAsync())
            {
                _logger.LogInformation($"Recieved number -> {number.Value}");
                total += number.Value;

            }

            return new NumberResponse { Result = total };
        }

        public override async Task<SendResult> SendFile(Chunk request, ServerCallContext context)
        {
            var content = request.Content.ToArray();
            await System.IO.File.WriteAllBytesAsync(_webenv.ContentRootPath + "/Files/" + "storm1.jpg", content);
            return new SendResult { Success = true };
        }

        public override async Task<SendResult> SendFileStream(IAsyncStreamReader<Chunk> requestStream, ServerCallContext context)
        {
            var fileName = _webenv.ContentRootPath + "/Files/" + "storm100.jpg";
            using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            int c = 0;

            try
            {
                await foreach (var chunk in requestStream.ReadAllAsync())
                {

                    fs.Write(chunk.Content.ToArray(), 0, chunk.Content.Length);

                    await Task.Delay(200);
                    Console.WriteLine(c++);
                }
            }
            finally 
            {

                fs.Close();
            }


            return new SendResult { Success = true };
        }



    }
}
