using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace MyServer
{
    public class NumbersService : Numerics.NumericsBase
    {
        private readonly ILogger<NumbersService> _logger;
        public NumbersService(ILogger<NumbersService> logger)
        {
            _logger = logger;
        }

        public override async Task SendNumber(IAsyncStreamReader<NumberRequest> requestStream, IServerStreamWriter<NumberResponse> responseStream, ServerCallContext context)
        {

            await foreach (var number in requestStream.ReadAllAsync())
            {
                _logger.LogInformation($"Recieved number -> {number.Value}");

                await Task.Delay(new Random().Next(1, 5) * 100);
                var response = new NumberResponse
                {
                    Result = number.Value * number.Value
                };
                await responseStream.WriteAsync(response);

            }

        }





    }
}
