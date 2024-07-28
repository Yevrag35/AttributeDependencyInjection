namespace AttributeDI.Attributes;

/// <summary>
/// An attribute that indicates the decorated assembly contains services that can be registered dynamically
/// during an application's startup. Assemblies not decorated with this attribute will not be 
/// scanned for containing service types by default.
/// </summary>
/// <remarks>
///     To add this attribute to non-.NET Framework assemblies, add the following to the library's .csproj file:
///     <code>
///         &lt;ItemGroup&gt;
///             &lt;AssemblyAttribute Include="AttributeDI.Attributes.DependencyAssemblyAttribute" /&gt;
///         &lt;/ItemGroup&gt;
///     </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class DependencyAssemblyAttribute : Attribute, IDependencyInjectionAttribute
{
}
