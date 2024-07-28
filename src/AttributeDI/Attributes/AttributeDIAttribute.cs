namespace AttributeDI.Attributes;

/// <summary>
/// The base class for all automatic dependency injection attributes whether explicit or dynamic.
/// </summary>
public abstract class AttributeDIAttribute : Attribute, IDependencyInjectionAttribute
{
}