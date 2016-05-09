using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NUnit.Framework;

namespace Sql6.NUnit
{
    [TestFixture]
    class Program
    {
        const int MessageCount = 25000;

        [Test]
        public async Task Main()
        {
            var busConfig = new EndpointConfiguration("PoorMansPerformance.Sql6");
            busConfig.UseTransport<SqlServerTransport>()
                .ConnectionString(@"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=nservicebus")
                .Transactions(TransportTransactionMode.SendsAtomicWithReceive);
            busConfig.UsePersistence<InMemoryPersistence>();
            busConfig.SendFailedMessagesTo("error");
            busConfig.SendOnly();

            await Run(busConfig).ConfigureAwait(false);
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

            var durationInSeconds = duration / (double)Stopwatch.Frequency;

            var throughput = MessageCount / durationInSeconds;

            Console.WriteLine("Throughput: {0}", throughput);
            Console.ReadLine();

            await endpoint.Stop();
        }
    }

    class MyMessage : IMessage
    {
    }
}
