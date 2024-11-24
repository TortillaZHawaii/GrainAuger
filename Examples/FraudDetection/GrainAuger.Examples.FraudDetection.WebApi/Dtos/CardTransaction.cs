namespace GrainAuger.Examples.FraudDetection.WebApi.Dtos;

[GenerateSerializer]
public record CardTransaction(
    int Amount,
    int LimitLeft,
    Card Card,
    double Latitude,
    double Longitude,
    CardOwner Owner
);
