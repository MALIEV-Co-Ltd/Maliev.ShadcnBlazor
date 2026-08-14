using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Components.Overlays;

/// <summary>Controls the visual and semantic emphasis of a menu item.</summary>
public enum ShadcnMenuItemVariant { Default, Destructive }

internal interface IShadcnMenuOwner { bool EffectiveOpen { get; } string TriggerId { get; } string ContentId { get; } ValueTask SetOpenAsync(bool value); }
internal sealed record ShadcnMenuContext(IShadcnMenuOwner Owner, string SlotPrefix);
internal sealed record ShadcnMenuRadioContext(string? Value, EventCallback<string?> ValueChanged) { internal async ValueTask SelectAsync(string value) { if (ValueChanged.HasDelegate) await ValueChanged.InvokeAsync(value); } }
internal static class ShadcnMenuValues { internal static string Variant(ShadcnMenuItemVariant value) => value switch { ShadcnMenuItemVariant.Default => "default", ShadcnMenuItemVariant.Destructive => "destructive", _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown menu item variant.") }; internal static string RequireValue(string value) => !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Menu item value cannot be empty.", nameof(value)); }
