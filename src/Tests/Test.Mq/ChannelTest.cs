using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test.Mq.Internal;
using Twino.Client.TMQ;
using Twino.MQ;
using Twino.MQ.Clients;
using Xunit;

namespace Test.Mq
{
    public class ServerMessageTest
    {
        /// <summary>
        /// Client sends a channel join message to server
        /// </summary>
        [Fact]
        public async Task JoinChannel()
        {
            TestMqServer server = new TestMqServer();
            server.Initialize(41201);
            server.Start();

            TmqClient client = new TmqClient();
            client.Connect("tmq://localhost:41201");

            TmqResponseCode joined = await client.Join("ch-1", false);
            Assert.Equal(TmqResponseCode.Ok, joined);
            await Task.Delay(1000);

            Channel channel = server.Server.Channels.FirstOrDefault();
            Assert.NotNull(channel);

            List<ChannelClient> clients = channel.ClientsClone;
            Assert.Single(clients);
        }

        /// <summary>
        /// Client sends a channel join message to server and waits response
        /// </summary>
        [Fact]
        public async Task JoinChannelWithResponse()
        {
            TestMqServer server = new TestMqServer();
            server.Initialize(41202);
            server.Start();

            TmqClient client = new TmqClient();
            client.Connect("tmq://localhost:41202");

            TmqResponseCode joined = await client.Join("ch-1", true);
            Assert.Equal(TmqResponseCode.Ok, joined);

            Channel channel = server.Server.Channels.FirstOrDefault();
            Assert.NotNull(channel);

            List<ChannelClient> clients = channel.ClientsClone;
            Assert.Single(clients);
        }

        /// <summary>
        /// Client sends a channel leave message to server
        /// </summary>
        [Fact]
        public async Task LeaveChannel()
        {
            TestMqServer server = new TestMqServer();
            server.Initialize(41203);
            server.Start();

            TmqClient client = new TmqClient();
            client.Connect("tmq://localhost:41203");

            TmqResponseCode joined = await client.Join("ch-1", true);
            Assert.Equal(TmqResponseCode.Ok, joined);

            TmqResponseCode left = await client.Leave("ch-1", false);
            Assert.Equal(TmqResponseCode.Ok, left);
            await Task.Delay(1000);

            Channel channel = server.Server.Channels.FirstOrDefault();
            Assert.NotNull(channel);

            List<ChannelClient> clients = channel.ClientsClone;
            Assert.Empty(clients);
        }

        /// <summary>
        /// Client sends a channel leave message to server and waits response
        /// </summary>
        [Fact]
        public async Task LeaveChannelWithResponse()
        {
            TestMqServer server = new TestMqServer();
            server.Initialize(41204);
            server.Start();

            TmqClient client = new TmqClient();
            client.Connect("tmq://localhost:41204");

            TmqResponseCode joined = await client.Join("ch-1", true);
            Assert.Equal(TmqResponseCode.Ok, joined);

            TmqResponseCode left = await client.Leave("ch-1", true);
            Assert.Equal(TmqResponseCode.Ok, left);

            Channel channel = server.Server.Channels.FirstOrDefault();
            Assert.NotNull(channel);

            List<ChannelClient> clients = channel.ClientsClone;
            Assert.Empty(clients);
        }


        /// <summary>
        /// Client sends a queue creation message
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Create(bool verifyResponse)
        {
            int port = verifyResponse ? 35905 : 35904;
            TestMqServer server = new TestMqServer();
            server.Initialize(port);
            server.Start();

            TmqClient client = new TmqClient();
            client.Connect("tmq://localhost:" + port);

            TmqResponseCode created = await client.CreateChannel("new-channel", verifyResponse);
            if (verifyResponse)
                Assert.Equal(TmqResponseCode.Ok, created);
            else
            {
                await Task.Delay(1000);
                Assert.Equal(TmqResponseCode.Ok, created);
            }
        }

        [Fact]
        public async Task CreateWithProperties()
        {
            TestMqServer server = new TestMqServer();
            server.Initialize(41206);
            server.Start();

            TmqClient client = new TmqClient();
            await client.ConnectAsync("tmq://localhost:41206");
            Assert.True(client.IsConnected);

            TmqResponseCode created = await client.CreateChannel("new-channel", o =>
            {
                o.AllowMultipleQueues = false;
                o.SendOnlyFirstAcquirer = true;
                o.AcknowledgeTimeout = TimeSpan.FromSeconds(33);
                o.Status = MessagingQueueStatus.Pull;
            });
            Assert.Equal(TmqResponseCode.Ok, created);

            Channel found = server.Server.FindChannel("new-channel");
            Assert.NotNull(found);
            Assert.False(found.Options.AllowMultipleQueues);
            Assert.True(found.Options.SendOnlyFirstAcquirer);
        }
    }
}