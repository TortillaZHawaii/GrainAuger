namespace CloudNative.Workload;

[GenerateSerializer]
public class CardTransaction
{
    [Id(0)]
    public int Id { get; set; }
    [Id(1)]
    public string CardNumber { get; set; }
    [Id(2)]
    public int Amount { get; set; }
}