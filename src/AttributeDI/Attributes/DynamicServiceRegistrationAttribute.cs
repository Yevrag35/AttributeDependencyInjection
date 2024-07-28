using Microsoft.Extensions.Configuration;

namespace AttributeDI.Attributes;

/// <summary>
/// An attribute on decorated classes/structs that will automatically call a registration method with an
/// application's dependency injection container at startup.
/// </summary>
/// <remarks>
///     Any class that uses this attribute to register itself must also have defined a <see langword="static"/>
///     method decorated with <see cref="DynamicServiceRegistrationMethodAttribute"/> that will be called to execute 
///     the registration.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class DynamicServiceRegistrationAttribute : Attribute, IDependencyInjectionAttribute
{
}

/// <summary>
/// An attribute indicating that the decorated method is called exclusively to register dependencies with an
/// application's dependency injection container.
/// </summary>
/// <remarks>
///     <para>
///     If the class this method is decorated on is not decorated with <see cref="DynamicServiceRegistrationAttribute"/>,
///     this attribute will be ignored. Classes with multiple decorated methods will throw an exception during the 
///     registration process.
///     </para>
///     For this attribute to function, the method must be <see langword="static"/> (with any visibility) and
///     with 1 of 2 overloads. The <see cref="IServiceCollection"/> parameter must always be the first, and 
///     optionally can contain <see cref="IConfiguration"/> as the second.
///     <para>
///         Examples of this are:
///     <code>
///         private static void AddToServices(IServiceCollection services)
///         // - or -
///         private static void AddToServices(IServiceCollection services, IConfiguration configuration)
///     </code>
///     </para>
///     <para>
///     It is possible that an <see cref="IConfiguration"/> implementation was not provided during the AttributeDI
///     extension registration process. The injection process does not check for <see langword="null"/> for
///     performance reasons. If it is *not* 100% confirmed that <see cref="IConfiguration"/> was provided during startup
///     registration, it is recommended to <see langword="null"/>-check the parameter.
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class DynamicServiceRegistrationMethodAttribute : Attribute, IDependencyInjectionAttribute
{
}