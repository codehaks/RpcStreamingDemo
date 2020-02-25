using Grpc.Core;
using Grpc.Net.Client;
using MyServer;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MyServer.Numerics;

namespace MyClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var client = new MyServer.Numerics.NumericsClient(channel);


            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            var response = new NumberResponse() { Result= -5 };
            using var streamingCall = client.SendNumber(response, cancellationToken: cts.Token);

            try
            {
                await foreach (var number in streamingCall.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                {
                 
                    Console.WriteLine($"{number.Value}");
                }


            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Stream cancelled.");
            }


            Console.ReadLine();

        }

       
    }
}
