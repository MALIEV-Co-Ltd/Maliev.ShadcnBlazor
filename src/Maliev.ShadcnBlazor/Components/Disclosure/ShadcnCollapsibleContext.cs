namespace Maliev.ShadcnBlazor.Components.Disclosure;

internal sealed record ShadcnCollapsibleContext(ShadcnCollapsible Owner, string TriggerId, string ContentId)
{
    internal bool Open => Owner.EffectiveOpen;
}
