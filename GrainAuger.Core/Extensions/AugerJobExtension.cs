using System.Reflection;
using GrainAuger.Abstractions;
using Orleans.Hosting;

namespace GrainAuger.Core.Extensions;

public static class AugerJobExtension
{
    public static void UseAugerJob(this ISiloBuilder app)
    {
        var jobTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetCustomAttributes(typeof(AugerJobConfigurationAttribute), true).Any())
            .ToList();

        foreach (var jobType in jobTypes)
        {
            var jobName = jobType.GetCustomAttribute<AugerJobConfigurationAttribute>()?.JobName;
            if (jobName == null)
            {
                throw new InvalidOperationException($"Job type {jobType.FullName} is missing a {nameof(AugerJobConfigurationAttribute)}");
            }
            // var jobBuilder = new AugerJobBuilder(jobName);
            // var jobConfiguration = (IAugerJobConfiguration)Activator.CreateInstance(jobType);
            // jobConfiguration.Configure(jobBuilder);
            // var job = jobBuilder.Build();
            //
            // app.ApplicationServices.GetRequiredService<IAugerJobManager>().AddJob(job);
        }
    }
}