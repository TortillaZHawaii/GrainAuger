using System;

namespace GrainAuger.SourceGenerator;

internal class WindowConfig
{
    private TumblingWindowConfig? TumblingWindowConfig { get; init; }
    private SlidingWindowConfig? SlidingWindowConfig { get; init; }
    private SessionWindowConfig? SessionWindowConfig { get; init; }
}

internal class TumblingWindowConfig
{
    private TimeSpan WindowSize { get; init; }
}

internal class SlidingWindowConfig
{
    private TimeSpan WindowSize { get; init; }
    private TimeSpan SlideSize { get; init; }
}

internal class SessionWindowConfig
{
    private TimeSpan SessionTimeout { get; init; }
}