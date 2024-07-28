# <img height="30px" src="./.assets/icon_64.png" alt="Attribute Dependency Injection"></img> Attribute Dependency Injection (DI)

 [![version](https://img.shields.io/nuget/v/AttributeDependencyInjection?style=flat-square)](https://www.nuget.org/packages/AttributeDependencyInjection) [![downloads](https://img.shields.io/nuget/dt/AttributeDependencyInjection?style=flat-square&color=darkgreen)](https://www.nuget.org/packages/AttributeDependencyInjection)

This project provides a set of extensions to the IServiceCollection for registering services using custom attributes.

* __Attribute-Based Injection__: Register services by decorating classes and structs with custom attributes.
* __Dynamic Service Registration__: Allows methods decorated with special attributes to be invoked during the registration process.
* __Exclusions__: Exclude specific services or implementations from being registered.

## How to Use

### Add <code>DependencyAssemblyAttribute</code> for Discoverability

By default, only assemblies with the <code>DependencyAssemblyAttribute</code> will be scanned for attributed services (this behavior can be changed through the startup options).

__.NET Core and later__
Add the attribute through the project's <code>.csproj</code> file:
```xml
<ItemGroup>
    <AssemblyAttribute Include="AttributeDI.Attributes.DependencyAssemblyAttribute" />
</ItemGroup>
```

__.NET Framework__
Add the attribute in the <code>AssemblyInfo.cs</code> file:
```csharp
[assembly: DependencyAssembly]
```

### Defining Services with Attributes

Services are defined using attributes that indicate how they should be registered with the DI container. The example below shows their basic usage:

```csharp
using AttributeDI.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace AttributedDI.ConsoleExample.Services
{
    [ServiceRegistration] // Registers the FileService as a singleton service.
    public sealed class FileService
    {
        public FileService()
        {
        }
    }

    public interface IResponderService
    {
        FileService Files { get; }
        void Respond();
    }

    [ServiceRegistration(typeof(IResponderService))] // Registers the ResponderService as the implementation with
                                                     // IResponderService as the service type.
    file sealed class ResponderService : IResponderService
    {
        public FileService Files { get; }

        public ResponderService(FileService fs)
        {
            this.Files = fs;
        }

        public void Respond()
        {
            Console.WriteLine("Hello, World!");
        }
    }

    public interface IQuestion
    {
        string Value { get; }
    }

    [DynamicServiceRegistration] // Invokes a method decorated with DynamicServiceRegistrationMethodAttribute.
    public sealed class QuestionService
    {
        private readonly IQuestion _defaultQuestion;

        internal QuestionService(IQuestion defaultQuestion)
        {
            _defaultQuestion = defaultQuestion;
        }

        public string Ask(IQuestion? question)
        {
            return question is null ? _defaultQuestion.Value : question.Value;
        }

        [DynamicServiceRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services)
        {
            services.AddSingleton(x =>
            {
                var question = x.GetRequiredService<IQuestion>();
                return new QuestionService(question);
            });
        }

        [ServiceStructRegistration(typeof(IQuestion), Lifetime = ServiceLifetime.Transient)]
        internal readonly struct Question : IQuestion
        {
            public string Value => "Hello, World!";

            public Question()
            {
            }
        }
    }

    [ServiceRegistration(typeof(IResponderService))]
    public sealed class ExcludeThisService : IResponderService
    {
        public ExcludeThisService(FileService fs)
        {
            this.Files = fs;
        }

        public FileService Files { get; }

        public void Respond()
        {
            throw new NotImplementedException();
        }
    }
}
```

### Call the AddAttributedServices Method

To add the attributed services into your application's startup routine, use one of the overloads for <code>AttributeDI.AttributeDIExtensions.AddAttributedServices()</code>

```csharp
using AttributeDI.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Configuration;

public class Startup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register services using assemblies and configuration
        services.AddAttributedServices(new[] { Assembly.GetExecutingAssembly() }, configuration);

        // Or configure options with an action
        services.AddAttributedServices(options =>
        {
            options.Configuration = configuration;
            options.IncludeNonAttributedAssembliesInScan = true;
        });
    }
}
```

### Excluding Services

During the initial registration setup, services can be excluded from being registered for any reason. Exclusions can be added by the service or the implementation type.

```csharp
using AttributeDI.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Configuration;

public class Startup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Exclude service(s) for testing
        services.AddAttributedServices(options =>
        {
            options.AddExclusions(x => 
            {
                x.Add<ExcludeThisService>()
                 .Add(typeof(TestCollection<>))
                 .Add<ITestInterface>();
            });
        });
    }
}
```

## Attributes

### <code>ServiceRegistrationAttribute</code>

Indicates that the decorated class should be registered as a service. The lifetime of the service can be specified through the attribute property <code>Lifetime</code>.

```csharp

// Registers a singleton service and implementation type of MyLoneService
[ServiceRegistration]
public sealed class MyLoneService
{
    // Implementation
}

// -- or --

// Registers a service type 'IServiceType' with the implementation of MyService.
[ServiceRegistration(typeof(IServiceType), Lifetime = ServiceLifetime.Scoped)]
internal sealed class MyService : IServiceType
{
    // Implementation
}
```

Generic types with this attribute will be registered with their generic type definition.

```csharp
[ServiceRegistration(typeof(IServiceType<>), Lifetime = ServiceLifetime.Scoped)]
public sealed class MyService<T> : IServiceType<T>
{
    // Implementation
}
```

### <code>ServiceStructRegistrationAttribute</code>

Registers a struct implementation using the specified interface service type.

```csharp
[ServiceStructRegistration(typeof(IServiceType), Lifetime = ServiceLifetime.Transient)]
public struct MyServiceStruct : IServiceType
{
    // Implementation
}
```

### <code>DynamicServiceRegistrationAttribute</code>

Indicates that a method decorated with the <code>DynamicServiceRegistrationMethodAttribute</code> should be invoked during the registration process to dynamically add services.

```csharp
[DynamicServiceRegistration]
public sealed class DynamicService
{
    private readonly IOtherService _other;

    private DynamicService(IOtherService other)
    {
        _other = other;
    }

    [DynamicServiceRegistrationMethod]
    private static void AddToServices(IServiceCollection services)
    {
        services.AddSingleton(x =>
        {
            var other = x.GetRequiredService<IOtherService>();
            return new DynamicService(other);
        });
    }
}
```

Optionally, if the application's <code>IConfiguration</code> is provided to the startup options, you can specify a different overload:

```csharp
[DynamicServiceRegistration]
public sealed class DynamicService
{
    [DynamicServiceRegistrationMethod]
    private static void AddToServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DynamicService>()
                .Bind(configuration);
    }
}
```