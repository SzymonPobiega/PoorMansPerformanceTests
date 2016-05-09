using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

namespace Sql6
{
    class Program
    {
        const int MessageCount = 25000;

        static void Main(string[] args)
        {
            var busConfig = new EndpointConfiguration("PoorMansPerformance.Sql6");
            busConfig.UseTransport<SqlServerTransport>()
                .ConnectionString(@"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=nservicebus")
                .Transactions(TransportTransactionMode.SendsAtomicWithReceive);
            busConfig.UsePersistence<InMemoryPersistence>();
            busConfig.SendFailedMessagesTo("error");
            busConfig.SendOnly();
            var t = new Thread(_ =>
            {
               Run(busConfig).GetAwaiter().GetResult(); 
            });
            t.Start();
            t.Join();
        }

        static async Task Run(EndpointConfiguration busConfig)
        {
            var endpoint = await Endpoint.Start(busConfig);
            Console.WriteLine("Starting the test");
            var start = Stopwatch.GetTimestamp();
            
            for (var i = 0; i < MessageCount; i++)
            {
                var sendOptions = new SendOptions();
                sendOptions.SetDestination("DestinationQueue");
                await endpoint.Send(new MyMessage(), sendOptions).ConfigureAwait(false);
            }
            var end = Stopwatch.GetTimestamp();
            var duration = end - start;

            var durationInSeconds = duration/(double)Stopwatch.Frequency;

            var throughput = MessageCount/durationInSeconds;

            Console.WriteLine("Throughput: {0}", throughput);
            Console.ReadLine();

            await endpoint.Stop();
        }
    }

    class MyMessage : IMessage
    {
    }
}
