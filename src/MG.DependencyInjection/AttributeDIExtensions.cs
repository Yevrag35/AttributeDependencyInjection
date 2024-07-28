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
    public static IServiceCollection AddAttributedServices(this IServiceCollection services, Assembly[] assemblies, IConfiguration? configuration, Action<IAddServiceTypeExclusions>? configureExclusions = null)
    {
        IServiceTypeExclusions exclusions = ServiceTypeExclusions.ConfigureFromAction(configureExclusions);
        ServiceResolutionContext context = new(services, configuration!, in exclusions);

        foreach (Assembly assembly in assemblies.Where(IsServicableAssembly))
        {
            AddResolvedServicesFromAssembly(assembly, in context);
        }

        return services;
    }
}