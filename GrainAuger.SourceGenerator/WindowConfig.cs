using System;

namespace GrainAuger.SourceGenerator;

internal class WindowConfig
{
    internal TumblingWindowConfig? TumblingWindowConfig { get; init; }
    internal SlidingWindowConfig? SlidingWindowConfig { get; init; }
    internal SessionWindowConfig? SessionWindowConfig { get; init; }
}

internal class TumblingWindowConfig
{
    internal TimeSpan WindowSize { get; init; }
}

internal class SlidingWindowConfig
{
    internal TimeSpan WindowSize { get; init; }
    internal TimeSpan SlideSize { get; init; }
}

internal class SessionWindowConfig
{
    internal TimeSpan SessionTimeout { get; init; }
}