namespace GrainAuger.Abstractions;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class AugerJobConfigurationAttribute(string jobName) : System.Attribute
{
    public string JobName { get; } = jobName;
}
