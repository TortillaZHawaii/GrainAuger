namespace GrainAuger.Examples.FraudDetection.WebApi.Dtos;

[GenerateSerializer]
public record CardTransaction(
    int Amount,
    int LimitLeft,
    Card Card,
    CardOwner Owner
);
