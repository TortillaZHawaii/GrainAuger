using Orleans.Runtime;

namespace GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore.Windows;

public class GroupByCountAuger<T> : Auger<T, List<T>>
{
    private readonly IPersistentState<List<T>> _state;

    private int WindowCount { get; }

    public GroupByCountAuger(
        // XXX should be replaced by code generation to some unique name
        [PersistentState("GroupByCountXXX", "AugerStore")]
        IPersistentState<List<T>> state, int windowCount)
    {
        WindowCount = windowCount;
        _state = state;
    }
    
    public override async Task ProcessAsync(T input, Func<List<T>, Task> collect)
    {
        _state.State ??= new List<T>();
        
        _state.State.Add(input);
        
        if (_state.State.Count >= WindowCount)
        {
            await collect(_state.State);
            _state.State = new List<T>();
        }

        await _state.WriteStateAsync();
    }
}