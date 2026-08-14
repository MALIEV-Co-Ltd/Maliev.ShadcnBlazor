namespace Maliev.ShadcnBlazor.Components.Feedback.Toast;

using Microsoft.AspNetCore.Components;

/// <summary>Defines content, timing, and callbacks for a toast.</summary>
public sealed record ShadcnToastOptions(
    string Title,
    string? Description = null,
    ShadcnToastType Type = ShadcnToastType.Default,
    TimeSpan? Duration = null,
    string? ActionLabel = null,
    Func<Task>? Action = null,
    Action? OnDismiss = null,
    bool Urgent = false,
    ShadcnToastPriority Priority = ShadcnToastPriority.Normal,
    RenderFragment? Icon = null);
