using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace MyServer
{
    public class NumbersService : WebAnalytics.WebAnalyticsBase
    {
        private readonly ILogger<NumbersService> _logger;
        public NumbersService(ILogger<NumbersService> logger)
        {
            _logger = logger;
        }

        public override async Task SendLink(IAsyncStreamReader<LinkRequest> requestStream, IServerStreamWriter<LinkResponse> responseStream, ServerCallContext context)
        {
            var subject = new Subject<string>();

            subject.Subscribe(async (url) =>
            {

                var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                try
                {
                    var content = await client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
                    var pageSize = content.Content.Headers.ContentLength.Value;

                    await responseStream.WriteAsync(new LinkResponse { Url = url.ToString(), PageSize = Convert.ToInt32(pageSize) });
                }
                catch (Exception)
                {

                    await responseStream.WriteAsync(new LinkResponse { Url = url.ToString(), PageSize = Convert.ToInt32(-1) });
                }
            });


            await foreach (var link in requestStream.ReadAllAsync())
            {
                subject.OnNext(link.Url);
                _logger.LogInformation($"Recieved Link -> {link.Url}");
            }

            subject.OnCompleted();
        }



    }
}
