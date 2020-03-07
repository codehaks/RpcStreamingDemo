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
            await SendFile(client, @"d:\storm.jpg");
            Console.WriteLine("Done!");
            Console.ReadLine();

        }

        private static async Task SendFile(NumericsClient client, string filePath)
        {
            byte[] buffer;
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;  
                buffer = new byte[length];            
                int count;                            
                int sum = 0;                          

                //System.IO.File.ReadAllBytes()

                 while ((count = await fileStream.ReadAsync(buffer, sum, length - sum)) > 0)
                    sum += count; 
            }
            finally
            {
                fileStream.Close();
            }

            var result =await client.SendFileAsync(new Chunk
            {
                Content = ByteString.CopyFrom(buffer)
            });

            Console.WriteLine(result.Success);
         
        }
    }
}
