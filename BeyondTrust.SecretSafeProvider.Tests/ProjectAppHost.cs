using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Projects;

namespace BeyondTrust.SecretSafeProvider.Tests;

public class ProjectAppHost() : DistributedApplicationFactory(typeof(BeyondTrust_SecretSafeProvider))
{
    protected override void OnBuilderCreating(DistributedApplicationOptions opts, HostApplicationBuilderSettings hostOptions)
    {
        opts.EnableResourceLogging = false;
        opts.DisableDashboard = true;
        hostOptions.Args = ["--testmode=true"];
    }

    protected override void OnBuilderCreated(DistributedApplicationBuilder builder)
    {
        builder.Services.AddLogging(logging => logging
            .ClearProviders()
            .SetMinimumLevel(LogLevel.Critical));
    }
}
