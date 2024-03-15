namespace GrainAuger.Examples.FraudDetection.WebApi.Dtos;

[GenerateSerializer]
public record Card(
    string Number,
    string Type,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv
);
