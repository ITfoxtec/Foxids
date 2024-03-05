using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace FoxIDs.Logic;

public class RedisQueueProcessor(IConnectionMultiplexer redisConnectionMultiplexer, string queue, ChannelMessageQueue channelMessageQueue) : IQueueProcessor
{
    ChannelMessageQueue ChannelMessageQueue { get; } = channelMessageQueue;

    public event Func<string, Task> ProcessAsync
    {
        add => ChannelMessageQueue.OnMessage(async channelMessage => await value(await GetEnvelope()));
        remove => throw new Exception(nameof(RedisQueueProcessor) + " does not support removing handlers");
    }

    private async Task<string> GetEnvelope()
    {
        var db = redisConnectionMultiplexer.GetDatabase();
        var envelope = await db.ListRightPopAsync(queue);
        return envelope;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}