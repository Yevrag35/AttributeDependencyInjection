namespace AttributeDI.Internal.Extensions;

/// <summary>
/// An extension class for <see cref="Type"/> objects.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// Returns the fully qualified name or the member name of the current <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type whose name will be returned.</param>
    /// <returns>
    /// The <see cref="Type.FullName"/> of the current <see cref="Type"/> if it is not <see langword="null"/>;
    /// otherwise, the <see cref="MemberInfo.Name"/> instead.  If the type being extended is <see langword="null"/>,
    /// then an empty string is returned.
    /// </returns>
    internal static string GetName(this Type? type)
    {
        if (type is null)
        {
            return string.Empty;
        }

        return type.FullName ?? type.Name;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns>
    /// The underlying <see cref="Type"/> if <paramref name="type"/> is nullable; 
    /// otherwise, the original <paramref name="type"/> passed.
    /// </returns>
    /// <exception cref="ArgumentNullException"/>
    internal static Type GetUnderlyingType(this Type type)
    {
        Guard.ThrowIfNull(type, nameof(type));
        return !TryGetNullableFromNonNull(type, out Type? underlying)
            ? type
            : underlying;
    }

    [DebuggerStepThrough]
    internal static bool TryGetNullable(this Type type, [NotNullWhen(true)] out Type? underlying)
    {
        Guard.ThrowIfNull(type, nameof(type));
        return TryGetNullableFromNonNull(type, out underlying);
    }
    private static bool TryGetNullableFromNonNull(Type type, [NotNullWhen(true)] out Type? underlying)
    {
        underlying = Nullable.GetUnderlyingType(type);
        return underlying is not null;
    }
}
