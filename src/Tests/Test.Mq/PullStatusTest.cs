using System;
using System.Threading.Tasks;
using Test.Mq.Internal;
using Test.Mq.Models;
using Twino.Client.TMQ;
using Twino.MQ;
using Twino.MQ.Queues;
using Xunit;

namespace Test.Mq
{
    public class PullStatusTest
    {
        [Fact]
        public async Task SendAndPull()
        {
            int port = 47411;
            TestMqServer server = new TestMqServer();
            server.Initialize(port);
            server.Start(300, 300);

            bool msgReceived = false;
            TmqClient consumer = new TmqClient();
            consumer.ClientId = "consumer";
            await consumer.ConnectAsync("tmq://localhost:" + port);
            Assert.True(consumer.IsConnected);
            consumer.MessageReceived += (c, m) => msgReceived = true;
            bool joined = await consumer.Join("ch-pull", true);
            Assert.True(joined);

            TmqClient producer = new TmqClient();
            await producer.ConnectAsync("tmq://localhost:" + port);
            Assert.True(producer.IsConnected);

            await producer.Push("ch-pull", MessageA.ContentType, "Hello, World!", false);
            await Task.Delay(700);

            Channel channel = server.Server.FindChannel("ch-pull");
            ChannelQueue queue = channel.FindQueue(MessageA.ContentType);
            Assert.NotNull(channel);
            Assert.NotNull(queue);
            Assert.Single(queue.RegularMessages);
            Assert.False(msgReceived);

            bool requested = await consumer.Pull("ch-pull", MessageA.ContentType);
            Assert.True(requested);
            await Task.Delay(200);
            Assert.True(msgReceived);

            msgReceived = false;
            requested = await consumer.Pull("ch-pull", MessageA.ContentType);
            Assert.True(requested);
            await Task.Delay(200);
            Assert.False(msgReceived);
        }

        [Fact]
        public async Task RequestAcknowledge()
        {
            int port = 47412;
            TestMqServer server = new TestMqServer();
            server.Initialize(port);
            server.Start(300, 300);
            
            Channel channel = server.Server.FindChannel("ch-pull");
            ChannelQueue queue = channel.FindQueue(MessageA.ContentType);
            Assert.NotNull(channel);
            Assert.NotNull(queue);
            queue.Options.RequestAcknowledge = true;
            queue.Options.AcknowledgeTimeout = TimeSpan.FromSeconds(15);

            TmqClient consumer = new TmqClient();
            consumer.AutoAcknowledge = true;
            consumer.ClientId = "consumer";
            
            await consumer.ConnectAsync("tmq://localhost:" + port);
            Assert.True(consumer.IsConnected);
            
            bool msgReceived = false;
            consumer.MessageReceived += (c, m) => msgReceived = true;
            bool joined = await consumer.Join("ch-pull", true);
            Assert.True(joined);

            TmqClient producer = new TmqClient();
            producer.AcknowledgeTimeout = TimeSpan.FromSeconds(15);
            await producer.ConnectAsync("tmq://localhost:" + port);
            Assert.True(producer.IsConnected);

            Task<bool> taskAck = producer.Push("ch-pull", MessageA.ContentType, "Hello, World!", true);

            await Task.Delay(500);
            Assert.False(taskAck.IsCompleted);
            Assert.False(msgReceived);
            Assert.Single(queue.RegularMessages);

            bool requested = await consumer.Pull("ch-pull", MessageA.ContentType);
            Assert.True(requested);
            await Task.Delay(200);
            Assert.True(msgReceived);
            Assert.True(taskAck.IsCompleted);
            Assert.True(taskAck.Result);
        }
    }
}