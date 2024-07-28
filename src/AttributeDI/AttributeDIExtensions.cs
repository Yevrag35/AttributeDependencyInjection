using AttributeDI.Startup;
using Microsoft.Extensions.Configuration;

namespace AttributeDI;

/// <summary>
/// An extension class for adding services to a given <see cref="IServiceCollection"/> through the use
/// of AttributeDI attributes.
/// </summary>
public static partial class AttributeDIExtensions
{
    public static IServiceCollection AddAttributedServices(this IServiceCollection services, Assembly[] assemblies, IConfiguration? configuration)
    {
        AttributedServiceOptions options = new()
        {
            AssembliesToScan = assemblies,
            Configuration = configuration!,
        };

        return AddAttributedServicesFromOptions(services, options);
    }
    public static IServiceCollection AddAttributedServices(this IServiceCollection services, Action<AttributedServiceOptions> configureOptions)
    {
        AttributedServiceOptions options = new();
        configureOptions(options);

        return AddAttributedServicesFromOptions(services, options);
    }

    private static IServiceCollection AddAttributedServicesFromOptions(IServiceCollection services, AttributedServiceOptions configuredOptions)
    {
        IServiceTypeExclusions exclusions = configuredOptions.GetServiceTypeExclusions();
        ServiceResolutionContext context = new(services, configuredOptions);

        foreach (Assembly assembly in configuredOptions.GetAssemblies())
        {
            AddResolvedServicesFromAssembly(assembly, in context);
        }

        return services;
    }
}