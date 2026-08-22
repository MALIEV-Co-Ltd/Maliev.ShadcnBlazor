namespace Maliev.ShadcnBlazor.Components.Conversation;

public enum ShadcnQuestionnaireItemStatus { Unanswered, Answered, Skipped }
public enum ShadcnQuestionnaireShortcutMode { None, Letters, Numbers }

public sealed record ShadcnQuestionnaireChoiceDefinition(string Value, bool Disabled = false, bool Custom = false);

public sealed record ShadcnQuestionnaireItemDefinition(
    string Name,
    bool Required = false,
    bool Disabled = false,
    bool Multiple = false,
    bool AllowsFreeform = false,
    IReadOnlyList<ShadcnQuestionnaireChoiceDefinition>? Choices = null);

public sealed record ShadcnQuestionnaireAnswer(ShadcnQuestionnaireItemStatus Status, IReadOnlyList<string> SelectedValues, string? InputValue)
{
    public static ShadcnQuestionnaireAnswer Empty { get; } = new(ShadcnQuestionnaireItemStatus.Unanswered, Array.Empty<string>(), null);
}

public sealed record ShadcnQuestionnaireState(string ActiveItemName, int Current, int Total, int Completed, double Progress, IReadOnlyDictionary<string, ShadcnQuestionnaireAnswer> Answers, IReadOnlyDictionary<string, string> Errors);
public sealed record ShadcnQuestionnaireNavigationResult(bool Succeeded, string? FocusItemName = null, string? Error = null, bool Submitted = false);
public delegate ValueTask<string?> ShadcnQuestionnaireValidateItem(ShadcnQuestionnaireItemDefinition item, ShadcnQuestionnaireAnswer answer, CancellationToken cancellationToken);
public delegate ValueTask<IReadOnlyDictionary<string, string>> ShadcnQuestionnaireValidate(ShadcnQuestionnaireState state, CancellationToken cancellationToken);
