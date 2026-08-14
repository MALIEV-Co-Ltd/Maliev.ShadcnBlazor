namespace Maliev.ShadcnBlazor.Components.Feedback.Toast;

using Microsoft.AspNetCore.Components;

/// <summary>Represents one immutable toast snapshot.</summary>
public sealed record ShadcnToastItem(
    string Id,
    string Title,
    string? Description,
    ShadcnToastType Type,
    string? ActionLabel,
    Func<Task>? Action,
    Action? OnDismiss,
    string Live,
    TimeSpan? Duration,
    ShadcnToastPriority Priority,
    RenderFragment? Icon,
    string State = "open");
