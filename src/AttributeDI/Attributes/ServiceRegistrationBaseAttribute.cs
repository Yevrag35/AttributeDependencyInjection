using AttributeDI.Exceptions;
using AttributeDI.Internal.Collections;
using AttributeDI.Startup;

namespace AttributeDI.Attributes;

#nullable enable
/// <summary>
/// A base attribute for defining services that will be automatically added to the dependency injection container
/// with derived types being able to access/define the service type, implementation type.
/// </summary>
public abstract class ServiceRegistrationBaseAttribute : AttributeDIAttribute
{
    /// <summary>
    /// The <see cref="Type"/> implementing the service.
    /// </summary>
    /// <remarks>
    /// Will default to the decorated type if not set or <see langword="null"/>.
    /// </remarks>
    protected Type? Implementation { get; set; }
    /// <summary>
    /// Gets or sets the lifetime of the service. Defaults to <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; }
    /// <summary>
    /// The <see cref="Type"/> of the service.
    /// </summary>
    /// <remarks>
    /// Will default to the decorated type if not set or <see langword="null"/>.
    /// </remarks>
    protected Type? Service { get; set; }

    /// <summary>
    /// Constructs and enumerates all <see cref="ServiceDescriptor"/> objects from the specified type.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="ServiceDescriptor"/> objects that were created from the 
    /// specified type.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <inheritdoc 
    ///     cref="TryCreateDescriptorFromAttribute(ServiceRegistrationBaseAttribute, Type, in IServiceTypeExclusions, out ServiceDescriptor)"
    ///     path="/exception"/>
    [DebuggerStepThrough]
    public static IEnumerable<ServiceDescriptor> CreateDescriptorsFromType(Type type, IServiceTypeExclusions exclusions)
    {
        foreach (var attribute in type.GetCustomAttributes<ServiceRegistrationBaseAttribute>(inherit: false))
        {
            if (TryCreateDescriptorFromAttribute(attribute, type, in exclusions, out ServiceDescriptor? descriptor))
            {
                yield return descriptor;
            }
        }
    }

    [DebuggerStepThrough]
    private static bool AnyGenericInterfaceMatches(Type serviceType, Type implementationType)
    {
        ArrayRefEnumerator<Type> enumerator = new(implementationType.GetInterfaces());
        bool flag = false;

        while (enumerator.MoveNext(in flag))
        {
            Type type = enumerator.Current;
            if (!type.IsGenericTypeDefinition && type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            flag = type.IsGenericTypeDefinition
                   &&
                   type.Equals(serviceType);
        }

        return flag;
    }

    [DebuggerStepThrough]
    private static bool ImplementsType(Type serviceType, Type implementationType)
    {
        if (serviceType.IsAssignableFrom(implementationType))
        {
            return true;
        }
        else if (serviceType.IsGenericTypeDefinition)
        {
            if (!implementationType.IsGenericTypeDefinition)
            {
                implementationType = implementationType.GetGenericTypeDefinition();
            }

            if (serviceType.IsInterface)
            {
                return AnyGenericInterfaceMatches(serviceType, implementationType);
            }
            else
            {
                return implementationType.IsSubclassOf(serviceType);
            }
        }

        return false;
    }

    /// <inheritdoc cref="Type.GetGenericTypeDefinition" path="/exception"/>
    /// <inheritdoc cref="ValidateImplementationType(Type, Type)" path="/exception"/>
    private static bool TryCreateDescriptorFromAttribute(ServiceRegistrationBaseAttribute attribute, Type type, in IServiceTypeExclusions exclusions, [NotNullWhen(true)] out ServiceDescriptor? descriptor)
    {
        attribute.Service ??= type;
        attribute.Implementation ??= type;

        if (exclusions.IsExcluded(attribute.Service))
        {
            descriptor = null;
            return false;
        }

        if (attribute.Service.IsGenericTypeDefinition && !attribute.Implementation.IsGenericTypeDefinition)
        {
            attribute.Implementation = attribute.Implementation.GetGenericTypeDefinition();
        }

        ValidateImplementationType(attribute.Service, attribute.Implementation);
        descriptor = new(attribute.Service, attribute.Implementation, attribute.Lifetime);
        return true;
    }

    /// <exception cref="MissingConstructorException"></exception>
    /// <exception cref="TypeNotAssignableException"></exception>
    [DebuggerStepThrough]
    protected static void ValidateImplementationType(Type serviceType, Type implementationType)
    {
        //BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        //ConstructorInfo[] ctors = implementationType.GetConstructors(flags);
        //if (ctors.Length <= 0)
        //{
        //    throw new MissingConstructorException(implementationType, flags);
        //}
        if (!ReferenceEquals(serviceType, implementationType) && !ImplementsType(serviceType, implementationType))
        {
            throw new TypeNotAssignableException(serviceType, implementationType);
        }
    }
}