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

            await StreamNumbersFromClientToServer(client);

            Console.ReadLine();

        }

        static Random RNG = new Random();

        private static async Task StreamNumbersFromClientToServer(NumericsClient client)
        {
            using (var call = client.SendNumber())
            {
                for (var i = 0; i < 10; i++)
                {
                    var number = RNG.Next(5);
                    Console.WriteLine($"Sending {number}");
                    await call.RequestStream.WriteAsync(new NumberRequest { Value = number });
                    await Task.Delay(1000);
                }

                await call.RequestStream.CompleteAsync();

                //var response = await call;
                //Console.WriteLine($"Result: {response.Result}");
            }
        }


    }
}
