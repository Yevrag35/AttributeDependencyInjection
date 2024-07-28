namespace AttributeDI.Startup;

/// <summary>
/// A delegate that acts on a <see cref="Referencer"/>.
/// </summary>
/// <param name="referencer">The referencer used to load assemblies at application startup.</param>
public delegate void ActOnReferencer(in Referencer referencer);

/// <summary>
/// A struct used at application startup to force loading of assemblies by referencing a type
/// from each assembly.
/// </summary>
/// <remarks>
/// This struct is mainly used in <see cref="AttributedServiceOptions.LoadReferences(ActOnReferencer)"/> to ensure 
/// that certain assemblies are loaded into the application domain, but made public for use elsewhere in the application
/// startup process. By referencing types from these assemblies, they will force the assembly to load without performing 
/// any additional actions.
/// <para>
/// An example:
/// <code>
/// Referencer.LoadAll((in Referencer r) =>
/// {
///     r.Reference&lt;SomeTypeFromAssembly1&gt;()
///      .Reference&lt;SomeTypeFromAssembly2&gt;();
/// });
/// </code>
/// </para>
/// </remarks>
public readonly ref struct Referencer
{
    /// <summary>
    /// References the specified type. No action is taken.
    /// </summary>
    /// <typeparam name="T">The type within an assembly to load.</typeparam>
    /// <returns>
    ///     The same <see cref="Referencer"/> for chaining.
    /// </returns>
    public readonly Referencer Reference<T>()
    {
        return this;
    }
    /// <summary>
    /// References the specified type. No action is taken.
    /// </summary>
    /// <param name="type">The type within an assembly to load.</param>
    /// <returns>
    ///     The same <see cref="Referencer"/> for chaining.
    /// </returns>
    public readonly Referencer Reference(Type type)
    {
        return this;
    }

    /// <summary>
    /// Force loads referenced assemblies by executing the specified action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public static void LoadAll(ActOnReferencer action)
    {
        Referencer referencer = default;
        action(in referencer);
    }
}
