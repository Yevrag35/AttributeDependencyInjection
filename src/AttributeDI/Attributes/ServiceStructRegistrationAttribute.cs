using AttributeDI.Internal;

namespace AttributeDI.Attributes;

/// <summary>
/// An attribute decorated on structs that will automatically register the type with the application's
/// dependency injection container at application startup binding it to the specified service type.
/// </summary>
/// <remarks>
/// Unlike the <see cref="ServiceRegistrationAttribute"/>, this attribute requires the service type to be defined and
/// only accepts interface types. This is because structs cannot be used as service types in default Dependency 
/// Injection containers.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class ServiceStructRegistrationAttribute : ServiceRegistrationAttribute
{
    /// <inheritdoc path="/*[not(self::exception)]"/>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not an interface type.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    [NotNull]
    [DisallowNull]
    public override Type ServiceType
    {
        [return: NotNull]
        get => base.ServiceType!;
        set
        {
            Guard.ThrowIfNull(value, nameof(this.ServiceType));
            if (!value.IsInterface)
            {
                throw new ArgumentException("Only interfaces can be used as service types for structs.", nameof(this.ServiceType));
            }

            base.ServiceType = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceStructRegistrationAttribute"/> attribute class indicating
    /// that the decorated struct type is the implementation type for the specified interface service type when 
    /// creating the <see cref="ServiceDescriptor"/>.
    /// </summary>
    /// <param name="serviceType"><inheritdoc cref="ServiceType" path="/summary"/></param>
    /// <exception cref="ArgumentException"><paramref name="serviceType"/> is not an interface type.</exception>
    /// <inheritdoc cref="ServiceRegistrationAttribute(Type)" path="/exception"/>
    public ServiceStructRegistrationAttribute(Type serviceType)
        : base(serviceType)
    {
    }
}