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

        static Random RNG = new Random();

        public override async Task SendNumber(NumberResponse request, IServerStreamWriter<NumberRequest> responseStream, ServerCallContext context)
        {
            for (var i = 0; i < 10; i++)
            {
                var number = RNG.Next(5);
                _logger.LogInformation($"Sending {number}");
                await responseStream.WriteAsync((new NumberRequest { Value = number }));
                await Task.Delay(300);
            }

            _logger.LogWarning(request.Result.ToString());
        }



    }
}
