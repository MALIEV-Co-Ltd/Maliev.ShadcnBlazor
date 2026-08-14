namespace Maliev.ShadcnBlazor.Showcase.Documentation.Api;

public sealed record ComponentApiDescriptor(
    string Name,
    string FullTypeName,
    IReadOnlyList<ComponentParameterDescriptor> Parameters);

public sealed record ComponentParameterDescriptor(
    string Name,
    string FriendlyType,
    string DefaultValue,
    bool Required,
    string? BindingPair,
    string Description,
    string Constraints,
    bool CapturesUnmatchedValues);
