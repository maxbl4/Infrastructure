﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure.MessageHub;
using Xunit;

namespace maxbl4.Infrastructure.Tests
{
    public class MessageHubTests
    {
        readonly List<ChannelMessageHub.TestHubMessage> messages = new List<ChannelMessageHub.TestHubMessage>();
        public MessageHubTests()
        {
            ChannelMessageHub.TestHubMessage.StartIndex = 1;
        }

        [Fact]
        public void Subscribe_sync()
        {
            using var hub = GetMessageHub();
            using var sub = hub.Subscribe<ChannelMessageHub.TestHubMessage>(messages.Add);
            hub.Publish(new ChannelMessageHub.TestHubMessage());
            hub.Publish(new ChannelMessageHub.TestHubMessage());
            VerifyMessages(2);
        }
        
        [Fact]
        public async Task Unsubscribe_sync()
        {
            using var hub = GetMessageHub();
            using (hub.Subscribe<ChannelMessageHub.TestHubMessage>(messages.Add))
            {
                hub.Publish(new ChannelMessageHub.TestHubMessage());
                await Task.Delay(50);
            }
            hub.Publish(new ChannelMessageHub.TestHubMessage());
            VerifyMessages(1);
        }
        
        [Fact]
        public void Subscribe_async()
        {
            using var hub = GetMessageHub();
            using var sub = hub.SubscribeAsync<ChannelMessageHub.TestHubMessage>(async x =>
            {
                await Task.Delay(10);
                messages.Add(x);
            });
            const int count = 20;
            for (var i = 0; i < count; i++)
                hub.Publish(new ChannelMessageHub.TestHubMessage());
            
            VerifyMessages(count);
        }
        
        [Fact]
        public void Subscribe_both()
        {
            using var hub = GetMessageHub();
            using var sub = hub.SubscribeAsync<ChannelMessageHub.TestHubMessage>(async x =>
            {
                await Task.Delay(10);
                if (x.Index % 2 == 0)
                    throw new Exception();
                messages.Add(x);
            });
            using var sub2 = hub.Subscribe<ChannelMessageHub.TestHubMessage>(x =>
            {
                if (x.Index % 2 == 0)
                    throw new Exception();
                messages.Add(x);
            });
            const int count = 20;
            for (var i = 0; i < count; i++)
                hub.Publish(new ChannelMessageHub.TestHubMessage());
            
            new Timing()
                .FailureDetails(() => $"Actual message count {messages.Count}")
                .Expect(() => messages.Count == count);
            for (var i = 0; i < count / 2; i++)
            {
                messages[i * 2].Index.Should().Be(i * 2 + 1);
                messages[i * 2 + 1].Index.Should().Be(i * 2 + 1);
            }
        }
        
        [Fact]
        public void Error_in_subscribe()
        {
            using var hub = GetMessageHub();
            using var sub = hub.SubscribeAsync<ChannelMessageHub.TestHubMessage>(async x =>
            {
                await Task.Delay(10);
                if (x.Index == 2)
                    throw new Exception();
                messages.Add(x);
            });
            const int count = 3;
            for (var i = 0; i < count; i++)
                hub.Publish(new ChannelMessageHub.TestHubMessage());
            
            VerifyMessages(count - 1);
        }

        void VerifyMessages(int expectedCount)
        {
            new Timing()
                .Timeout(1000)
                .FailureDetails(() => $"Actual message count {messages.Count}")
                .Expect(() => messages.Count == expectedCount);
            messages.Should().HaveCount(expectedCount);
            messages.Should().BeInAscendingOrder(x => x.Index);
        }

        IMessageHub GetMessageHub()
        {
            return new ChannelMessageHub();
        }
    }
}