using AttributeDI.Attributes;
using AttributeDI.Exceptions;
using AttributeDI.Internal.Collections;
using AttributeDI.Internal.Extensions;
using AttributeDI.Startup;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace AttributeDI;

public static partial class AttributeDIExtensions
{
    private const string INVALID_PARAMETERS = "Registration method must have at least the IServiceCollection parameter type.";

    private readonly struct ServiceResolutionContext
    {
        private readonly object[] _overload1;
        private readonly object[] _overload2;

        internal readonly bool AllowsDuplicates;
        internal readonly IConfiguration Configuration;
        internal readonly IServiceTypeExclusions Exclusions;
        internal readonly IServiceCollection Services;
        internal readonly Type MustImplement;
        internal readonly bool ThrowOnMultipleDynamic;
        internal readonly bool ThrowOnMissingDynamic;

        internal ServiceResolutionContext(IServiceCollection services, AttributedServiceOptions options)
        {
            AllowsDuplicates = options.AllowDuplicateServiceRegistrations;
            ThrowOnMultipleDynamic = !options.IgnoreMultipleDynamicRegistrations;
            ThrowOnMissingDynamic = options.ThrowOnMissingDynamicRegistrationMethod;
            MustImplement = typeof(IDependencyInjectionAttribute);
            Services = services;
            Configuration = options.Configuration;
            Exclusions = options.GetServiceTypeExclusions();
            _overload1 = new object[1] { services };
            _overload2 = new object[2] { services, options.Configuration };
        }

        /// <inheritdoc cref="MethodBase.Invoke(object, object[])" path="/exception"/>
        internal readonly void InvokeStaticMethod(MethodInfo method, in bool includeConfiguration)
        {
            object[] parameters = !includeConfiguration ? _overload1 : _overload2;
            _ = method.Invoke(null, parameters);
        }
    }

    #region GET / ENUMERATE METHODS
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
    private static MethodInfo? GetFirstDynamicMethodByName(Type type)
    {
        return type
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(x => x.IsDefined(typeof(DynamicServiceRegistrationMethodAttribute), inherit: false))
                .OrderBy(x => x.Name)
                .FirstOrDefault();
    }
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
    /// <exception cref="AttributeDIStartupException">More than one dynamic method was found.</exception>
    private static MethodInfo? GetSingleDynamicMethod(Type type)
    {
        try
        {
            return type
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(x => x.IsDefined(typeof(DynamicServiceRegistrationMethodAttribute), inherit: false))
                .SingleOrDefault();
        }
        catch (InvalidOperationException e)
        {
            throw new AttributeDIStartupException(type,
                "More than one (1) dynamic registration method were found on the specified type.", e);
        }
    }
    private static IEnumerable<Type> GetResolvableTypes(Assembly assembly, ServiceResolutionContext context)
    {
        Type mustHave = context.MustImplement;
        IServiceTypeExclusions exclusions = context.Exclusions;

        Type[] types = assembly.GetTypes();

        return types.Where(x => IsProperType(x)
                                &&
                                x.IsDefined(mustHave)
                                &&
                                !exclusions.IsExcluded(x));
    }

    #endregion

    #region VALIDATION
    /// <exception cref="InvalidOperationException"></exception>
    private static void CheckParameters(Type type, MethodInfo method, ref BoolCounter flags)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length <= 0)
        {
            throw new InvalidOperationException("Registration method must have at least the IServiceCollection parameter type");
        }
        else if (parameters.Length > 2)
        {
            throw new InvalidOperationException("Registration method must have at most two parameters");
        }

        ArrayRefEnumerator<ParameterInfo> enumerator = new(parameters);
        bool tripped = false;

        while (enumerator.MoveNext(in tripped))
        {
            ParameterInfo parameter = enumerator.Current;
            switch (parameter.Position)
            {
                case 0:
                    flags.MarkFlag(0, typeof(IServiceCollection).Equals(parameter.ParameterType));
                    break;

                case 1:
                    flags.MarkFlag(1, typeof(IConfiguration).Equals(parameter.ParameterType));
                    break;

                default:
                    break;
            }

            tripped = flags.Count == 2;
        }

        if (0 == flags.Count)
        {
            throw new AttributeDIStartupException(type, INVALID_PARAMETERS);
        }
    }
    private static bool IsProperType(Type type)
    {
        if (type.IsClass)
        {
            // Must not be static class.
            return !(type.IsAbstract && type.IsSealed);
        }
        else
        {
            // Must be interface or ValueType (struct).
            return type.IsValueType || type.IsInterface;
        }
    }

    #endregion

    #region ADD SERVICE

    /// <exception cref="AttributeDIStartupException"></exception>
    private static void AddFromRegistration(in ServiceResolutionContext context, Type type)
    {
        MethodInfo? method;
        try
        {
            method = context.ThrowOnMultipleDynamic
                ? GetSingleDynamicMethod(type)
                : GetFirstDynamicMethodByName(type);
        }
        catch (Exception e) when (e is not AttributeDIStartupException)
        {
            throw new AttributeDIStartupException(type, 
                $"An exception occurred scanning the type for {nameof(DynamicServiceRegistrationMethodAttribute)} - {e.Message}", e);
        }

        if (method is null)
        {
            if (context.ThrowOnMissingDynamic)
            {
                throw new AttributeDIStartupException(type, "No dynamic registration method was found on the specified type.");
            }

            return;
        }

        try
        {
            Span<bool> twoBools = stackalloc bool[2] { false, false };
            BoolCounter counter = new(twoBools);
            CheckParameters(type, method, ref counter);

            context.InvokeStaticMethod(method, in twoBools[twoBools.Length - 1]);
        }
        catch (Exception e)
        {
            throw new AttributeDIStartupException(typeof(AttributeDIExtensions), $"Failed to invoke registration method \"{method.Name}\" - {e.Message}", e);
        }
    }

    /// <exception cref="DuplicatedServiceException"/>
    /// <exception cref="AttributeDIStartupException"></exception>
    private static void AddResolvedServicesFromAssembly(Assembly assembly, in ServiceResolutionContext context)
    {
        foreach (Type type in GetResolvableTypes(assembly, context))
        {
            if (!type.IsInterface && type.IsDefined(typeof(DynamicServiceRegistrationAttribute), inherit: false))
            {
                AddFromRegistration(in context, type);
            }
            else if (type.IsDefined(typeof(ServiceRegistrationBaseAttribute), inherit: false))
            {
                try
                {
                    foreach (var descriptor in ServiceRegistrationBaseAttribute.CreateDescriptorsFromType(type, context.Exclusions))
                    {
                        AddService(context.Services, descriptor, in context.AllowsDuplicates);
                    }
                }
                catch (Exception e) when (e is not DuplicatedServiceException)
                {
                    throw new AttributeDIStartupException(typeof(AttributeDIExtensions), $"Failed to create descriptor for type \"{type.GetName()}\".", e);
                }
            }
        }
    }

#if DEBUG
    /// <exception cref="DuplicatedServiceException"/>
    /// <exception cref="InvalidOperationException">
    ///     <paramref name="services"/> is read-only.
    /// </exception>
    private static void AddService(IServiceCollection services, ServiceDescriptor descriptor, in bool allowsDuplicates)
    {
        if (!allowsDuplicates && !_addedViaAttributes.Add(descriptor))
        {
            throw new DuplicatedServiceException(
                serviceType: descriptor.ServiceType,
                diType: typeof(AttributeDIExtensions));
        }

        services.Add(descriptor);
    }

    static readonly HashSet<ServiceDescriptor> _addedViaAttributes = new(new ServiceDescriptorComparer());
    private sealed class ServiceDescriptorComparer : IEqualityComparer<ServiceDescriptor>
    {
        public bool Equals(ServiceDescriptor? x, ServiceDescriptor? y)
        {
            return ReferenceEquals(x, y)
                   ||
                   (x is not null && y is not null
                    &&
                    x.ServiceType == y.ServiceType
                    &&
                    x.ImplementationType == y.ImplementationType);
        }

        public int GetHashCode([DisallowNull] ServiceDescriptor obj)
        {
            if (obj is null)
            {
                return 0;
            }

            return HashCode.Combine(obj.ServiceType, obj.ImplementationType);
        }
    }
#else
        /// <exception cref="InvalidOperationException">
        ///     <paramref name="services"/> is read-only.
        /// </exception>
        private static void AddService(IServiceCollection services, ServiceDescriptor descriptor, in bool allowsDuplicates)
        {
            services.Add(descriptor);
        }
#endif

    #endregion
}