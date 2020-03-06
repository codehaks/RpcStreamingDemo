using Google.Protobuf;
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
            await StreamFile(client, @"d:\storm.jpg");
            Console.WriteLine("Done!");
            Console.ReadLine();

        }

        private static async Task StreamFile(NumericsClient client, string filePath)
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

            //await call.RequestStream.WriteAsync(new Chunk { Content = ByteString.CopyFrom(buffer)});

            //await call.RequestStream.CompleteAsync();
        }


        //static Random RNG = new Random();

        //private static async Task StreamNumbersFromClientToServer(NumericsClient client)
        //{
        //    using (var call = client.SendNumber())
        //    {
        //        for (var i = 0; i < 10; i++)
        //        {
        //            var number = RNG.Next(5);
        //            Console.WriteLine($"Sending {number}");
        //            await call.RequestStream.WriteAsync(new NumberRequest { Value = number });
        //            await Task.Delay(1000);
        //        }

        //        await call.RequestStream.CompleteAsync();

        //        var response = await call;
        //        Console.WriteLine($"Result: {response.Result}");
        //    }
        //}
    }
}
