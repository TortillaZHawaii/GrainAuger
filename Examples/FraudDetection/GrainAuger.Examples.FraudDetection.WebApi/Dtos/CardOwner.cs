namespace GrainAuger.Examples.FraudDetection.WebApi.Dtos;

[GenerateSerializer]
public record CardOwner(
    int Id,
    string FirstName,
    string LastName
);
