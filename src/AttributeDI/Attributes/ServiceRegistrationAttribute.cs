using AttributeDI.Internal;

namespace AttributeDI.Attributes;

/// <summary>
/// An attribute decorated on classes that will automatically register the class with the application's
/// dependency injection container at application startup.
/// </summary>
/// <remarks>
/// The default lifetime of the service is <see cref="ServiceLifetime.Singleton"/> unless specified in the
/// attribute declaration. If no <see cref="ServiceType"/> is specified, the class this attribute is decorated on will
/// be registered as itself.
/// <para>
/// If the <see cref="ServiceType"/> is a generic type, the generic type definition will be registered. An example of 
/// this:
/// <code>
/// [ServiceRegistration(typeof(IMyList&lt;&gt;))]
/// internal class MyList&lt;T&gt; : IMyList&lt;T&gt;
/// </code>
/// </para>
/// </remarks>
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
        get => this.Service!;
        set => this.Service = value;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceRegistrationAttribute"/> with its default values.
    /// </summary>
    public ServiceRegistrationAttribute()
    {
        this.ServiceType = null!;
        this.Lifetime = ServiceLifetime.Singleton;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="ServiceRegistrationAttribute"/> with the specified type as
    /// the service type.
    /// </summary>
    /// <param name="serviceType">The service type that this decorated class is derived from or implements.</param>
    /// <exception cref="ArgumentNullException"><paramref name="serviceType"/> is null.</exception>
    public ServiceRegistrationAttribute(Type serviceType)
    {
        Guard.ThrowIfNull(serviceType, nameof(serviceType));
        this.ServiceType = serviceType;
        this.Lifetime = ServiceLifetime.Singleton;
    }
}