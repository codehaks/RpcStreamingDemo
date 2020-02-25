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


    }
}
