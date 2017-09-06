// ******************
//  Ruffer LLP 2017
//  Created by jirish
//  09 2017
// ******************

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ServiceConnect.Monitor
{
    public class ConsumerPool : IDisposable
    {
        private readonly Connection _connection;

        private ConcurrentBag<Consumer> _pool;

        public ConsumerPool(Connection connection)
        {
            _connection = connection;
        }

        public void StartConsuming(ConsumerEventHandler messageReceived, string queueName, string forwardQueue)
        {
            // Ensure connection open
            _connection.Connect();

            // Create threads & start them consuming
            _pool = new ConcurrentBag<Consumer>();

            for(var i = 0; i < Environment.ProcessorCount * 2; i++)
                new Thread(() =>
                {
                    var threadConsumer = new Consumer(_connection);
                    threadConsumer.StartConsuming(messageReceived, queueName, forwardQueue);

                    _pool.Add(threadConsumer);

                }).Start();
        }

        public void SetForwardQueue(string queue)
        {
            foreach (var consumer in _pool)
                consumer.SetForwardQueue(queue);
        }

        public void Dispose()
        {
            foreach (var consumer in _pool)
                consumer.Dispose();
        }
    }
}