using System.ComponentModel;

#nullable enable
namespace AttributeDI.Exceptions;

/// <summary>
/// The base <see cref="Exception"/> class for any errors thrown due to unintended consequences of 
/// operations related to MG.DependencyInjection library.
/// </summary>
public class AttributeDIException : Exception
{
    private const string THIS_DOT = "this.";
    private const string DEFAULT_MESSAGE = "An exception was thrown due to a invalid operation in the MG.DependencyInjection library.";

    /// <summary>
    /// Initializes a new instance of <see cref="AttributeDIException"/> without a message but with a 
    /// reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <remarks>
    ///     This constructor is intended for derived classes that override the 
    ///     <see cref="Exception.Message"/> property.
    /// </remarks>
    /// <param name="innerException">
    ///     <inheritdoc cref="Exception(string, Exception)" path="/param[last()]"/>
    /// </param>
    protected AttributeDIException(Exception? innerException)
        : base(null, innerException)
    {
    }

    /// <inheritdoc cref="Exception(string)" path="/*[not(self::summary)]"/>
    /// <summary>
    ///     <inheritdoc path="/summary/text()[1]"/>
    ///     <see cref="AttributeDIException"/><inheritdoc path="/summary/text()[2]"/>
    /// </summary>
    public AttributeDIException([Localizable(true)] string? message)
        : base(GetMessageOrUseDefault(message, DEFAULT_MESSAGE))
    {
    }
    /// <inheritdoc cref="Exception(string, Exception)" path="/*[not(self::summary)]"/>
    /// <summary>
    ///     <inheritdoc path="/summary/text()[1]"/>
    ///     <see cref="AttributeDIException"/><inheritdoc path="/summary/text()[2]"/>
    /// </summary>
    public AttributeDIException([Localizable(true)] string? message, Exception? innerException)
        : base(GetMessageOrUseDefault(message, DEFAULT_MESSAGE), innerException)
    {
    }

    /// <summary>
    /// Returns the non-null <see cref="string"/> of either the provided message or a default one if
    /// it is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="message"><inheritdoc cref="Exception(string)"/></param>
    /// <param name="defaultMessage">
    ///     The default message to use if <paramref name="message"/>
    ///     is <see langword="null"/> or empty. This should never be <see langword="null"/>.
    /// </param>
    /// <returns>
    ///     <paramref name="message"/> unchanged if it is not <see langword="null"/> or empty;
    ///     otherwise, <paramref name="defaultMessage"/>.
    /// </returns>
    protected static string GetMessageOrUseDefault([Localizable(true)] string? message, [Localizable(true)] string defaultMessage)
    {
        if (string.IsNullOrEmpty(message))
        {
            message = defaultMessage ?? DEFAULT_MESSAGE;
        }

        return message!;
    }

    /// <summary>
    /// Returns the non-null <see cref="string"/> of either the provided message or formats a default 
    /// one using the given object arguments if it is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="message"><inheritdoc cref="Exception(string)"/></param>
    /// <param name="defaultMsgFormat">
    ///     The default message format that will be formatted if <paramref name="message"/> is 
    ///     <see langword="null"/> or empty.
    /// </param>
    /// <param name="args">
    ///     <inheritdoc cref="string.Format(string, object?[])" path="/param[last()]"/>
    /// </param>
    /// <returns>
    ///     <paramref name="message"/> unchanged if it is not <see langword="null"/> or empty;
    ///     otherwise, <paramref name="defaultMsgFormat"/> when formatted with the arguments provided in
    ///     <paramref name="args"/>. A default message will be substituted if a 
    ///     <see cref="FormatException"/> or <see cref="ArgumentNullException"/> is thrown.
    /// </returns>
    protected static string GetMessageOrUseDefault(string? message, string defaultMsgFormat, params object?[] args)
    {
        string defMsg = DEFAULT_MESSAGE;
        if (string.IsNullOrEmpty(message))
        {
            try
            {
                message = string.Format(defaultMsgFormat, args);
            }
            catch (ArgumentNullException ae)
            {
                Debug.Fail(ae.Message);
                message = defMsg;
            }
            catch (FormatException fe)
            {
                Debug.Fail(fe.Message);
                message = defMsg;
            }
        }

        return message!;
    }

    private static bool ThisDotIsLonger([NotNullWhen(false)] string? argumentName)
    {
        return THIS_DOT.Length > argumentName?.Length;
    }
    /// <summary>
    /// Trims leading "this." from the specified <paramref name="argumentName"/> if it is present.
    /// </summary>
    /// <param name="argumentName">The argument name to trim.</param>
    /// <returns>
    /// The trimmed <paramref name="argumentName"/> if it starts with "this."; otherwise, the original string unchanged.
    /// </returns>
    [return: NotNullIfNotNull(nameof(argumentName))]
    protected static string? TrimThisDot(string? argumentName)
    {
        if (ThisDotIsLonger(argumentName) || !argumentName.StartsWith(THIS_DOT, StringComparison.Ordinal))
        {
            return argumentName;
        }

        ReadOnlySpan<char> name = argumentName.AsSpan(THIS_DOT.Length).Trim();
        return !name.IsEmpty
            ? name.ToString()
            : argumentName;
    }
}