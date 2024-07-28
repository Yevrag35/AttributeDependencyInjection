namespace AttributeDI.Internal;

/// <summary>
/// Provides static methods to enforce various preconditions, such as type safety, non-null values, and range 
/// validation.
/// </summary>
/// <remarks>
/// This <see langword="static"/> class acts as a safeguard, throwing exceptions when preconditions for method 
/// arguments are not met.
/// </remarks>
internal static class Guard
{
    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if an object has been disposed.
    /// </summary>
    /// <typeparam name="T">The type of the disposed object.</typeparam>
    /// <param name="disposed">Boolean indicating whether the object is disposed.</param>
    /// <param name="instance">The instance of the object being checked.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the object is determined to be disposed.</exception>
    internal static void ThrowIfDisposed<T>([DoesNotReturnIf(true)] bool disposed, T instance)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(typeof(T).Name);
        }
    }

    /// <summary>
    /// Ensures an object is of a specified type and throws an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <typeparam name="T">The expected type of the object.</typeparam>
    /// <param name="value">The object to check.</param>
    /// <param name="paramName">The name of the parameter that holds the value.</param>
    /// <returns>The object cast to the specified type.</returns>
    /// <exception cref="ArgumentException">Thrown if the value is not of the expected type.</exception>
    [return: NotNull]
    internal static T ThrowIfNotType<T>(object value, string paramName)
    {
        if (!(value is T tVal))
        {
            GetTypeNames<T>(value, out string typeName, out string objTypeName);

            var castEx = new InvalidCastException($"The value of type \"{objTypeName}\" is not of the expected type \"{typeName}\"");
            throw new ArgumentException($"Parameter is not of the expected type \"{typeName}\".", paramName, castEx);
        }

        return tVal;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if a value is negative.
    /// </summary>
    /// <param name="value">The integer value to check.</param>
    /// <param name="paramName">The name of the parameter that holds the value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is less than zero.</exception>
    internal static void ThrowIfNegative(int value, string? paramName = null)
    {
        if (value < 0)
        {
            paramName ??= nameof(value);
            throw new ArgumentOutOfRangeException(paramName, value, "The value must be greater than or equal to 0.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if an object is <see langword="null"/>.
    /// </summary>
    /// <param name="value">The object to check for nullity.</param>
    /// <param name="paramName">The optional name of the parameter that holds the value.</param>
    /// <exception cref="ArgumentNullException">Thrown if the object is null.</exception>
    internal static void ThrowIfNull([NotNull] object? value, string? paramName = null)
    {
        if (value is null)
        {
            paramName ??= nameof(value);
            throw new ArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if a string is <see langword="null"/>, empty, or consists 
    /// only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The optional name of the parameter that holds the value.</param>
    /// <exception cref="ArgumentException">Thrown if the string is null, empty, or only white space.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the string is null.</exception>
    internal static void ThrowIfNullOrWhitespace([NotNull] string? value, string? paramName = null)
    {
        if (value is null)
        {
            paramName ??= nameof(value);
            throw new ArgumentNullException(paramName);
        }
        else if (string.Empty == value)
        {
            paramName ??= nameof(value);
            throw new ArgumentException("The value cannot be empty.", paramName);
        }
        else
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return;
                }
            }

            paramName ??= nameof(value);
            throw new ArgumentException("The value cannot be whitespace.", paramName);
        }
    }

    private static void GetTypeNames<T>(object value, out string typeName, out string objTypeName)
    {
        Type t = typeof(T);
        Type? oType = value?.GetType();

        typeName = t.FullName ?? t.Name;
        objTypeName = oType is null
            ? "null"
            : oType.FullName ?? oType.Name;
    }
}
