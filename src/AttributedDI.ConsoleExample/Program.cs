using AttributedDI.ConsoleExample.Services;
using AttributeDI;
using AttributeDI.Startup;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        IServiceProvider provider = CreateServices();
        var responder = provider.GetRequiredService<IResponderService>();
        var fs = provider.GetRequiredService<FileService>();
        Debug.Assert(ReferenceEquals(fs, responder.Files));

        var questions = provider.GetRequiredService<QuestionService>();
        string question = questions.Ask(null);
        string question2 = questions.Ask(provider.GetRequiredService<IQuestion>());
        Debug.Assert(question == question2);

        var excluded = provider.GetServices<IResponderService>();
        Debug.Assert(1 == excluded.Count());

        Console.WriteLine("\r\nDone with tests!");
        Console.ReadKey();
    }

    private static IServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddAttributedServices(options =>
        {
            options.AllowDuplicateServiceRegistrations = true;
            options.IgnoreMultipleDynamicRegistrations = true;
            options.IncludePublicDynamicMethods = true;
            options.ThrowOnMissingDynamicRegistrationMethod = true;

            options.LoadReferences((in Referencer r) => r.Reference<IQuestion>());
            options.AddExclusions(e => e.Add<ExcludeThisService>());
        });

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }
}