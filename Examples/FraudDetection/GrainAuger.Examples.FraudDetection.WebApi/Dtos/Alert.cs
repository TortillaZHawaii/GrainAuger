namespace GrainAuger.Examples.FraudDetection.WebApi.Dtos;

public record Alert(
    CardTransaction Transaction,
    string Reason
);
