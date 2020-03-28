using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using MyServer;
using System;
using System.IO;
using System.Linq;
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

            var client = new NumericsClient(channel);

            Console.WriteLine("Sending");
            await StreamFilePercent(client, @"d:\bloom.jpg");
            Console.WriteLine("Done!");
            Console.ReadLine();

        }

        private static async Task StreamFilePercent(NumericsClient client, string filePath)
        {

            using Stream source = File.OpenRead(filePath);
            using var call = client.SendFileStreamProgress();
            var size = source.Length / 100;
            byte[] buffer = new byte[size];
            int bytesRead;
            var t1 = Task.Run(async () =>
            {
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {

                    await call.RequestStream.WriteAsync(new Chunk { Content = Google.Protobuf.ByteString.CopyFrom(buffer) });
                    await Task.Delay(100);
                }

                await call.RequestStream.CompleteAsync();
            });

            var t2 = Task.Run(async () =>
            {
                await foreach (var number in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine($" Progress : {number.Percent} %");
                }
            });

            await Task.WhenAll(t1, t2);

        }



    }
}
