using AttributeDI.Exceptions;
using AttributeDI.Startup;
using Microsoft.Extensions.Configuration;

namespace AttributeDI;

/// <summary>
/// An extension class for adding services to a given <see cref="IServiceCollection"/> through the use
/// of AttributeDI attributes.
/// </summary>
public static partial class AttributeDIExtensions
{
    /// <summary>
    /// Adds attributed services to the specified <see cref="IServiceCollection"/> using the provided assemblies and configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for attributed services.</param>
    /// <param name="configuration">The configuration for the attributed services.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <inheritdoc cref="AddResolvedServicesFromAssembly(Assembly, in ServiceResolutionContext)"
    ///  path="/exception"/>
    public static IServiceCollection AddAttributedServices(this IServiceCollection services, Assembly[] assemblies, IConfiguration? configuration)
    {
        AttributedServiceOptions options = new()
        {
            AssembliesToScan = assemblies,
            Configuration = configuration!,
        };

        return AddAttributedServicesFromOptions(services, options);
    }
    /// <summary>
    /// Adds attributed services to the specified <see cref="IServiceCollection"/> using the provided configuration action.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">The action to configure the <see cref="AttributedServiceOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <inheritdoc cref="AddResolvedServicesFromAssembly(Assembly, in ServiceResolutionContext)"
    ///  path="/exception"/>
    public static IServiceCollection AddAttributedServices(this IServiceCollection services, Action<AttributedServiceOptions> configureOptions)
    {
        AttributedServiceOptions options = new();
        configureOptions(options);

        return AddAttributedServicesFromOptions(services, options);
    }

    /// <inheritdoc cref="AddResolvedServicesFromAssembly(Assembly, in ServiceResolutionContext)"
    ///  path="/exception"/>
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