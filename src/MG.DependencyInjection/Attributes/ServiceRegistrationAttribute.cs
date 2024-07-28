using MG.DependencyInjection.Internal;

namespace MG.DependencyInjection.Attributes;

/// <summary>
/// An attribute decorated on classes that will automatically register the class with the application's
/// dependency injection container at application startup.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ServiceRegistrationAttribute : ServiceRegistrationBaseAttribute
{
    /// <summary>
    /// The service type that the decorated type will be registered as in the <see cref="ServiceCollection"/>.
    /// </summary>
    [MaybeNull]
    public virtual Type ServiceType
    {
        [return: MaybeNull]
        get => base.Service!;
        set => base.Service = value;
    }

    public ServiceRegistrationAttribute()
    {
        this.ServiceType = null!;
        this.Lifetime = ServiceLifetime.Singleton;
    }
    public ServiceRegistrationAttribute(Type serviceType)
    {
        Guard.ThrowIfNull(serviceType, nameof(serviceType));
        this.ServiceType = serviceType;
        this.Lifetime = ServiceLifetime.Singleton;
    }
}