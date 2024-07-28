using AttributeDI.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace AttributedDI.ConsoleExample.Services;

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

[DynamicServiceRegistration] // Invokes a the method decorated with DynamicServiceRegistrationMethodAttribute.
public sealed class QuestionService
{
    private readonly IQuestion _defaultQuestion;

    // Can be used register services with non-public constructors.
    internal QuestionService(IQuestion defaultQuestion)
    {
        _defaultQuestion = defaultQuestion;
    }

    public string Ask(IQuestion? question)
    {
        return question is null ? _defaultQuestion.Value : question.Value;
    }

    // The dynamic method is invoked during the registration process.
    // Recommended to keep its visibility private as it should, typically, only be called once.
    // If an IConfiguration instance was provided during registration, it can be used as an optional second parameter.
    // The EditorBrowsable attribute is there to only indicate intent that the method should not be called directly.
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

    // Even nested classes/structs can be registered.
    // Structs require a explicit public constructor (unless dynamic registration is used)
    // and their service type must be a interface.
    [ServiceStructRegistration(typeof(IQuestion), Lifetime = ServiceLifetime.Transient)]
    internal readonly struct Question : IQuestion
    {
        public string Value => "Hello, World!";

        public Question()
        {
        }
    }
}

// During the initial registration setup, services can be excluded from being registered for any reason.
// Exclusions can be added by the service or the implementation type.
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