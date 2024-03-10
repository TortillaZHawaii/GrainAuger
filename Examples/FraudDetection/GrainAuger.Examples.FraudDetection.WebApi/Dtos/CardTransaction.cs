namespace GrainAuger.Examples.FraudDetection.WebApi.Dtos;

public record CardTransaction(
    int Amount,
    int LimitLeft,
    Card Card,
    CardOwner Owner
);
