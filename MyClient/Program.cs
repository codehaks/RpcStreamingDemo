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

            var client = new MyServer.Numerics.NumericsClient(channel);

            Console.WriteLine("Sending");
            //await SendFile(client, @"d:\storm.jpg");
            //await StreamFile(client, @"d:\storm.jpg");
            await StreamFilePercent(client, @"d:\storm.jpg");
            Console.WriteLine("Done!");
            Console.ReadLine();

        }

        private static async Task SendFile(NumericsClient client, string filePath)
        {
            byte[] buffer;
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = await fileStream.ReadAsync(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }

            client.SendFile(new Chunk
            {
                Content = ByteString.CopyFrom(buffer)
            });
        }

        private static async Task StreamFile(NumericsClient client, string filePath)
        {

            using Stream source = File.OpenRead(filePath);
            using var call = client.SendFileStreamProgress();
            var size = source.Length / 100;
            byte[] buffer = new byte[size];
            int bytesRead;

            var t1 = Task.Run(async () =>
            {

                int c = 0;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {

                    await call.RequestStream.WriteAsync(new Chunk { Content = Google.Protobuf.ByteString.CopyFrom(buffer) });
                    Console.WriteLine(c++);
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

        private static async Task StreamFilePercent(NumericsClient client, string filePath)
        {

            using Stream fileStreamData = File.OpenRead(filePath);
            using var call = client.SendFileStreamProgress();
            var size = 1024;
            byte[] buffer = new byte[size];
            int bytesRead;
            var t1 = Task.Run(async () =>
            {
                while ((bytesRead = fileStreamData.Read(buffer, 0, buffer.Length)) > 0)
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
