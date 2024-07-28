using MG.DependencyInjection.Attributes;
using MG.DependencyInjection.Internal;
using MG.DependencyInjection.Startup;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace MG.DependencyInjection;

/// <summary>
/// An extension class for adding services to a given <see cref="IServiceCollection"/> through the use
/// of <see cref="ServiceRegistrationBaseAttribute"/>-derived attributes.
/// </summary>
public static partial class AttributeDIExtensions
{
    //public static IServiceCollection AddAttributedServices(this IServiceCollection services, IConfiguration? configuration, Action<>)
    public static IServiceCollection AddAttributedServices(this IServiceCollection services, Action<AttributedServiceOptions> configureOptions)
    {
        AttributedServiceOptions options = new();
        configureOptions(options);

        IServiceTypeExclusions exclusions = options.GetServiceTypeExclusions();
        ServiceResolutionContext context = new(services, options.Configuration, in exclusions);

        foreach (Assembly assembly in options.GetAssemblies())
        {
            AddResolvedServicesFromAssembly(assembly, in context);
        }

        return services;
    }
}