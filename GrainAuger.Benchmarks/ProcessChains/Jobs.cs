using GrainAuger.Abstractions;

namespace GrainAuger.Benchmarks.ProcessChains;

public class Jobs
{
    // [AugerJobConfiguration("SplitJob")]
    // public void ConfigureSplit(IAugerJobBuilder builder)
    // {
    //     var input1 = builder.FromStream<int, int>("Memory", "InputSplit");
    //     var split1 = input1.Process<PassProcessor>("Split1");
    //     var split2 = split1.Process<PassProcessor>("Split2");
    // }
    //
    // [AugerJobConfiguration("JoinedJob")]
    // public void ConfigureJoined(IAugerJobBuilder builder)
    // {
    //     var input2 = builder.FromStream<int, int>("Memory", "InputJoin");
    //     var joined = input2.Process<PassProcessor, PassProcessor>("Joined");
    // }
}
