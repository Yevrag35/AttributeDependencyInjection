using MG.DependencyInjection.Attributes;
using MG.DependencyInjection.Exceptions;
using MG.DependencyInjection.Internal.Collections;
using MG.DependencyInjection.Internal.Extensions;
using MG.DependencyInjection.Startup;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace MG.DependencyInjection;

public static partial class AttributeDIExtensions
{
    private readonly struct ServiceResolutionContext
    {
        private readonly object[] _overload1;
        private readonly object[] _overload2;

        internal readonly IConfiguration Configuration;
        internal readonly IServiceTypeExclusions Exclusions;
        internal readonly IServiceCollection Services;
        internal readonly Type MustHaveAttribute;

        internal ServiceResolutionContext(IServiceCollection services, IConfiguration configuration, in IServiceTypeExclusions exclusions)
        {
            MustHaveAttribute = typeof(ServiceRegistrationBaseAttribute);
            Services = services;
            Configuration = configuration;
            Exclusions = exclusions;
            _overload1 = new object[1] { services };
            _overload2 = new object[2] { services, configuration };
        }

        /// <inheritdoc cref="MethodBase.Invoke(object, object[])" path="/exception"/>
        internal readonly void InvokeStaticMethod(MethodInfo method, in bool includeConfiguration)
        {
            object[] parameters = !includeConfiguration ? _overload1 : _overload2;
            _ = method.Invoke(null, parameters);
        }
    }

    #region GET TYPES
    private static IEnumerable<Type> GetResolvableTypes(Assembly assembly, ServiceResolutionContext context)
    {
        Type mustHave = context.MustHaveAttribute;
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
            throw new AdApiStartupException(type, Errors.Exception_InvalidMethodParameters);
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
    private static bool IsServicableAssembly(Assembly assembly)
    {
        return !assembly.IsDynamic
            && assembly.IsDefined(typeof(DependencyAssemblyAttribute), inherit: false);
    }

    #endregion

    #region ADD SERVICE

    /// <exception cref="AttributeDIStartupException"></exception>
    private static void AddFromRegistration(in ServiceResolutionContext context, Type type)
    {
        MethodInfo? method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(x => x.IsDefined(typeof(DynamicDependencyRegistrationMethodAttribute), inherit: false))
            .OrderBy(x => x.Name)
            .FirstOrDefault();

        if (method is null)
        {
            Debug.Fail("No registration method found.");
            return;
        }

        try
        {
            Span<bool> twoBools = [false, false];
            BoolCounter counter = new(twoBools);
            CheckParameters(type, method, ref counter);

            context.InvokeStaticMethod(method, in twoBools[twoBools.Length - 1]);
        }
        catch (Exception e)
        {
            throw new AttributeDIStartupException(typeof(AttributeDIExtensions), $"Failed to invoke registration method \"{method.Name}\".", e);
        }
    }

    /// <exception cref="DuplicatedServiceException"/>
    /// <exception cref="AdApiStartupException"></exception>
    private static void AddResolvedServicesFromAssembly(Assembly assembly, in ServiceResolutionContext context)
    {
        foreach (Type type in GetResolvableTypes(assembly, context))
        {
            if (!type.IsInterface && type.IsDefined(typeof(DynamicDependencyRegistrationAttribute), inherit: false))
            {
                AddFromRegistration(in context, type);
            }
            else if (type.IsDefined(typeof(ServiceRegistrationBaseAttribute), inherit: false))
            {
                try
                {
                    foreach (var descriptor in ServiceRegistrationBaseAttribute.CreateDescriptorsFromType(type, context.Exclusions))
                    {
                        AddService(context.Services, descriptor);
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
    private static void AddService(IServiceCollection services, ServiceDescriptor descriptor)
    {
        if (!_addedViaAttributes.Add(descriptor))
        {
            string msg = $"Duplicate service descriptor -> Service: {descriptor.ServiceType.GetName()}; Implementation: {descriptor.ImplementationType.GetName()}";
            Debug.Fail(msg);

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
        private static void AddService(IServiceCollection services, ServiceDescriptor descriptor)
        {
            services.Add(descriptor);
        }
#endif

    #endregion
}