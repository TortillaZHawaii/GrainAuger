namespace GrainAuger.Examples.FraudDetection.WebApi.Dtos;

[GenerateSerializer]
public record Alert(
    CardTransaction Transaction,
    string Reason
);
