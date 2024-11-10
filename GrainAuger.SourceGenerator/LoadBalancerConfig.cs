using System;

namespace GrainAuger.SourceGenerator;

internal class LoadBalancerConfig
{
    RoundRobinLoadBalancerConfig? RoundRobinLoadBalancerConfig { get; init; }
    RandomLoadBalancerConfig? RandomLoadBalancerConfig { get; init; }
    // KeyByConfig? KeyByConfig { get; init; }
}

internal class RoundRobinLoadBalancerConfig
{
    private int BucketCount { get; init; }
}

internal class RandomLoadBalancerConfig
{
    private int BucketCount { get; init; }
}

internal class KeyByConfig<T, TKey>
{
    private Func<T, TKey> KeySelector { get; init; }
}