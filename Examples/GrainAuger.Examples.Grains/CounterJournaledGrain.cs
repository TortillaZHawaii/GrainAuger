using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;

namespace GrainAuger.Examples.Grains;

public class CounterState
{
    private int Value { get; set; }

    void Apply(Increment increment)
    {
        Value += increment.Value;
    }
    
    void Apply(Decrement decrement)
    {
        Value -= decrement.Value;
    }
}

public abstract record CounterEvent;

public record Increment(int Value) : CounterEvent;

public record Decrement(int Value) : CounterEvent;

public interface ICounterJournaledGrain : IGrainWithStringKey
{
}

public class CounterJournaledGrain : JournaledGrain<CounterState, CounterEvent>
{
}