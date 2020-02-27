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
            using var call = client.SendNumber();

            var t1 = Task.Run(async () =>
             {
                 for (var i = 0; i < 20; i++)
                 {
                     var number = RNG.Next(1,100);
                     Console.WriteLine($"Sending {number}");
                     await call.RequestStream.WriteAsync(new NumberRequest { Value = number,Index=i });
                     await Task.Delay(new Random().Next(1,5)*100);
                 }
             });

            var t2 = Task.Run(async () =>
             {
                 await foreach (var number in call.ResponseStream.ReadAllAsync())
                 {
                     Console.WriteLine($"Recieved [{number.Index}] By power {Math.Sqrt(number.Result)} -> {number.Result}");
                 }
             });

            await Task.WhenAll(t1, t2);




        }


    }
}
