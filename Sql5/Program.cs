using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

namespace Sql5
{
    class Program
    {
        const int ThreadCount = 1;
        const int MessageCount = 25000;

        static void Main(string[] args)
        {
            var busConfig = new BusConfiguration();
            busConfig.EndpointName("PoorMansPerformance.Sql5");
            busConfig.UseTransport<SqlServerTransport>()
                .ConnectionString(@"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=PerformanceTests");
            busConfig.UsePersistence<InMemoryPersistence>();

            Run(busConfig);
        }

        static void Run(BusConfiguration busConfig)
        {
            var endpoint = Bus.CreateSendOnly(busConfig);
            var start = Stopwatch.GetTimestamp();
            var address = Address.Parse("PoorMansPerformance.Sql6.Receiver");

            var threads = Enumerable.Range(0, ThreadCount).Select(t => new Thread(_ =>
            {
                var countPerThread = MessageCount/ThreadCount;
                for (var i = 0; i < countPerThread; i++)
                {
                    endpoint.Send(address, new MyMessage());
                }
            })).ToArray();

            foreach (var thread in threads)
            {
                thread.Start();
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }
            
            var end = Stopwatch.GetTimestamp();
            var duration = end - start;

            var durationInSeconds = duration / (double)Stopwatch.Frequency;

            var throughput = MessageCount / durationInSeconds;

            Console.WriteLine("Throughput: {0}", throughput);
            Console.ReadLine();

            endpoint.Dispose();
        }
    }

    class MyMessage : IMessage
    {
    }
}
