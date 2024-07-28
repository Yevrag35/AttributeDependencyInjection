namespace AttributeDI.Exceptions;

/// <summary>
/// An exception thrown when a constructor is not found on a class type through reflection.
/// </summary>
public sealed class MissingConstructorException : AttributeDIException
{
    private const string MISSING_CTOR_FORMAT = "Unable to find a parameterless constructor for type '{0}' with flags: {1}.";

    /// <summary>
    /// The <see cref="BindingFlags"/> used in the search.
    /// </summary>
    public BindingFlags BindingFlagsUsed { get; }
    /// <summary>
    /// The class type whose <see cref="ConstructorInfo"/> was searched for.
    /// </summary>
    public Type ClassType { get; }
    /// <summary>
    /// The parameter types that were searched for - or - an empty array if searching for a parameterless constructor.
    /// </summary>
    public Type[] ParameterTypes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingConstructorException"/> exception with the specified class
    /// that was searched and the <see cref="BindingFlags"/> used.
    /// </summary>
    /// <param name="classType">
    ///     <inheritdoc cref="ClassType" path="/summary"/>
    /// </param>
    /// <param name="flagsUsed">
    ///     <inheritdoc cref="BindingFlagsUsed" path="/summary"/>
    /// </param>
    public MissingConstructorException(Type classType, BindingFlags flagsUsed)
        : this(classType, flagsUsed, null)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MissingConstructorException"/> exception with the specified class
    /// that was searched and the <see cref="BindingFlags"/> used and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="classType">
    ///     <inheritdoc cref="ClassType" path="/summary"/>
    /// </param>
    /// <param name="flagsUsed">
    ///     <inheritdoc cref="BindingFlagsUsed" path="/summary"/>
    /// </param>
    /// <param name="innerException">
    ///     <inheritdoc cref="Exception(string, Exception)" path="/param[last()]"/>
    /// </param>
    public MissingConstructorException(Type classType, BindingFlags flagsUsed, Exception? innerException)
        : base(FormatMessage(MISSING_CTOR_FORMAT, classType, flagsUsed), innerException)
    {
        this.ClassType = classType;
        this.BindingFlagsUsed = flagsUsed;
        this.ParameterTypes = Type.EmptyTypes;
    }

    private static string FormatMessage(string format, Type classType, BindingFlags flagsUsed)
    {
        return string.Format(format, classType, flagsUsed);
    }
}
