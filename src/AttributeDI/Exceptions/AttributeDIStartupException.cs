using AttributeDI.Internal.Extensions;
using System.ComponentModel;

namespace AttributeDI.Exceptions;

/// <summary>
/// Represents an error thrown during a Dependency Injection (DI) operation during the startup of an application.
/// </summary>
public class AttributeDIStartupException : AttributeDIException
{
    private const string DEFAULT_MSG = "The dependency injection method from type '{0}' had an exception occur.";

    /// <summary>
    /// The class type of the Dependency Injection extension that caused
    /// the exception.
    /// </summary>
    public Type DIType { get; }

    /// <summary>
    /// <inheritdoc cref="AttributeDIStartupException(Type)" path="/protected"/>.
    /// </summary>
    /// <protected>
    ///     <summary>
    ///     Initializes a new instance of the <see cref="AttributeDIStartupException"/> class identifying the specified
    ///     Depedency Injection class as the source of the exception</summary>
    /// </protected>
    /// <param name="diType">The type of the static class that caused the exception.</param>
    public AttributeDIStartupException(Type diType)
        : this(diType, null, null)
    {
    }

    /// <inheritdoc cref="AttributeDIStartupException(Type)" path="/*[not(self::summary) and not(self::protected)]"/>
    /// <summary>
    /// <inheritdoc cref="AttributeDIStartupException(Type)" path="/protected"/>
    /// <inheritdoc cref="AttributeDIStartupException(Type, Exception)" path="/protected"/>
    /// </summary>
    /// <protected>
    ///     <summary>and a reference to the inner exception that is the cause.</summary>
    /// </protected>
    /// <param name="innerException">
    ///     <inheritdoc cref="Exception(string, Exception)" path="/param[last()]"/>
    /// </param>
    /// <param name="diType"><inheritdoc cref="AttributeDIStartupException(Type)"/></param>
    public AttributeDIStartupException(Type diType, Exception? innerException)
        : this(diType, null, innerException)
    {
    }

    /// <inheritdoc cref="AttributeDIStartupException(Type, Exception)"
    ///     path="/*[not(self::summary) and not(self::protected)]"/>
    /// <summary>
    /// <inheritdoc cref="AttributeDIStartupException(Type)" path="/protected"/> with a specified error message.
    /// </summary>
    /// <param name="diType"><inheritdoc cref="AttributeDIStartupException(Type)"/></param>
    /// <param name="message"><inheritdoc cref="Exception(string)" path="/param"/></param>
    public AttributeDIStartupException(Type diType, [Localizable(true)] string? message)
        : base(GetBaseMessageFromType(message, diType), null)
    {
        this.DIType = diType;
    }

    /// <inheritdoc cref="AttributeDIStartupException(Type, Exception)"
    ///     path="/*[not(self::summary) and not(self::protected)]"/>
    /// <summary>
    /// <inheritdoc cref="AttributeDIStartupException(Type)" path="/protected"/> with a specified error message
    /// <inheritdoc cref="AttributeDIStartupException(Type, Exception)" path="/protected"/>
    /// </summary>
    /// <param name="diType"><inheritdoc cref="AttributeDIStartupException(Type)"/></param>
    /// <param name="message"><inheritdoc cref="Exception(string)" path="/param"/></param>
    /// <param name="innerException"><inheritdoc cref="AttributeDIStartupException(Type, Exception)"/></param>
    public AttributeDIStartupException(Type diType, [Localizable(true)] string? message, Exception? innerException)
        : base(GetBaseMessageFromType(message, diType), innerException)
    {
        this.DIType = diType;
    }

    private static string GetBaseMessageFromType(string? message, Type type)
    {
        return GetMessageOrUseDefault(message, DEFAULT_MSG, type.GetName());
    }
}