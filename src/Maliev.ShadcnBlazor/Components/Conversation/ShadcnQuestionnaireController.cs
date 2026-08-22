namespace Maliev.ShadcnBlazor.Components.Conversation;

public sealed class ShadcnQuestionnaireController
{
    private IReadOnlyList<ShadcnQuestionnaireItemDefinition> _items;
    private readonly IReadOnlyList<ShadcnQuestionnaireItemDefinition> _initialItems;
    private readonly Dictionary<string, ShadcnQuestionnaireAnswer> _answers;
    private readonly IReadOnlyDictionary<string, ShadcnQuestionnaireAnswer> _initialAnswers;
    private readonly ShadcnQuestionnaireValidateItem? _validate;
    private readonly Dictionary<string, string> _errors = new(StringComparer.Ordinal);
    private string _active;

    public ShadcnQuestionnaireController(IReadOnlyList<ShadcnQuestionnaireItemDefinition> items, string? activeItemName = null, IReadOnlyDictionary<string, ShadcnQuestionnaireAnswer>? initialAnswers = null, ShadcnQuestionnaireValidateItem? validate = null)
    {
        ValidateDefinitions(items);
        _items = Active(items);
        _initialItems = items.ToArray();
        _initialAnswers = initialAnswers is null ? new Dictionary<string, ShadcnQuestionnaireAnswer>() : new Dictionary<string, ShadcnQuestionnaireAnswer>(initialAnswers, StringComparer.Ordinal);
        ValidateAnswers(items, _initialAnswers);
        _answers = _items.ToDictionary(item => item.Name, item => _initialAnswers.GetValueOrDefault(item.Name, ShadcnQuestionnaireAnswer.Empty), StringComparer.Ordinal);
        _validate = validate;
        _active = activeItemName ?? _items[0].Name;
        if (_items.All(item => !string.Equals(item.Name, _active, StringComparison.Ordinal))) throw new InvalidOperationException($"Unknown active questionnaire item '{_active}'.");
        Publish();
    }

    public ShadcnQuestionnaireState State { get; private set; } = null!;

    public void SetChoice(string itemName, string value, bool selected)
    {
        var item = GetItem(itemName);
        var choice = item.Choices?.SingleOrDefault(candidate => string.Equals(candidate.Value, value, StringComparison.Ordinal)) ?? throw new InvalidOperationException($"Unknown choice '{value}'.");
        if (choice.Disabled) throw new InvalidOperationException($"Choice '{value}' is disabled.");
        var answer = _answers[itemName];
        var values = item.Multiple ? answer.SelectedValues.ToList() : [];
        values.RemoveAll(current => string.Equals(current, value, StringComparison.Ordinal));
        if (selected) values.Add(value);
        var customSelected = item.Choices?.Any(candidate => candidate.Custom && values.Contains(candidate.Value, StringComparer.Ordinal)) == true;
        _answers[itemName] = Answer(values, customSelected ? answer.InputValue : null);
        _errors.Remove(itemName);
        Publish();
    }

    public void SetInput(string itemName, string? value)
    {
        var item = GetItem(itemName);
        if (!item.AllowsFreeform) throw new InvalidOperationException($"Item '{itemName}' does not accept freeform input.");
        var answer = _answers[itemName];
        var choices = item.Choices ?? Array.Empty<ShadcnQuestionnaireChoiceDefinition>();
        if (choices.Any(choice => choice.Custom) && !choices.Any(choice => choice.Custom && answer.SelectedValues.Contains(choice.Value, StringComparer.Ordinal)))
            throw new InvalidOperationException($"Item '{itemName}' accepts freeform input only when its custom choice is selected.");
        _answers[itemName] = Answer(answer.SelectedValues, string.IsNullOrWhiteSpace(value) ? null : value);
        _errors.Remove(itemName);
        Publish();
    }

    public void Skip(string itemName)
    {
        var item = GetItem(itemName);
        if (item.Required) throw new InvalidOperationException($"Required item '{itemName}' cannot be skipped.");
        _answers[itemName] = new(ShadcnQuestionnaireItemStatus.Skipped, Array.Empty<string>(), null);
        _errors.Remove(itemName);
        Publish();
    }

    public async ValueTask<ShadcnQuestionnaireNavigationResult> NextAsync(CancellationToken cancellationToken = default)
    {
        var item = GetItem(_active);
        var answer = _answers[_active];
        string? error = _validate is null ? null : await _validate(item, answer, cancellationToken);
        error ??= item.Choices?.Any(choice => choice.Custom && answer.SelectedValues.Contains(choice.Value, StringComparer.Ordinal)) == true && string.IsNullOrWhiteSpace(answer.InputValue)
            ? "A custom answer is required."
            : null;
        error ??= item.Required && answer.Status is not ShadcnQuestionnaireItemStatus.Answered ? "An answer is required." : null;
        if (error is not null)
        {
            _errors[item.Name] = error;
            Publish();
            return new(false, item.Name, error);
        }
        var index = IndexOf(_active);
        if (index == _items.Count - 1) return new(true, Submitted: true);
        _active = _items[index + 1].Name;
        Publish();
        return new(true, _active);
    }

    public bool Previous()
    {
        var index = IndexOf(_active);
        if (index == 0) return false;
        _active = _items[index - 1].Name;
        Publish();
        return true;
    }

    public void SetActiveItem(string itemName)
    {
        _ = GetItem(itemName);
        _active = itemName;
        Publish();
    }

    public void SetItems(IReadOnlyList<ShadcnQuestionnaireItemDefinition> items)
    {
        ValidateDefinitions(items);
        var oldIndex = IndexOf(_active);
        _items = Active(items);
        foreach (var key in _answers.Keys.Except(_items.Select(item => item.Name), StringComparer.Ordinal).ToArray()) _answers.Remove(key);
        foreach (var item in _items) _answers.TryAdd(item.Name, _initialAnswers.GetValueOrDefault(item.Name, ShadcnQuestionnaireAnswer.Empty));
        if (_items.All(item => !string.Equals(item.Name, _active, StringComparison.Ordinal))) _active = _items[Math.Min(oldIndex, _items.Count - 1)].Name;
        Publish();
    }

    public void Reset()
    {
        _items = Active(_initialItems);
        _answers.Clear();
        foreach (var item in _items) _answers[item.Name] = _initialAnswers.GetValueOrDefault(item.Name, ShadcnQuestionnaireAnswer.Empty);
        _errors.Clear();
        _active = _items[0].Name;
        Publish();
    }

    public void SetAnswers(IReadOnlyDictionary<string, ShadcnQuestionnaireAnswer> answers, bool preserveErrors = false)
    {
        ValidateAnswers(_items, answers);
        _answers.Clear();
        foreach (var item in _items) _answers[item.Name] = answers.GetValueOrDefault(item.Name, ShadcnQuestionnaireAnswer.Empty);
        if (!preserveErrors) _errors.Clear();
        Publish();
    }

    public void SetErrors(IReadOnlyDictionary<string, string> errors)
    {
        if (errors.Keys.Any(name => _items.All(item => !string.Equals(item.Name, name, StringComparison.Ordinal)))) throw new InvalidOperationException("Questionnaire errors must reference active items.");
        _errors.Clear(); foreach (var error in errors) if (!string.IsNullOrWhiteSpace(error.Value)) _errors[error.Key] = error.Value; Publish();
    }

    private void Publish()
    {
        var current = IndexOf(_active) + 1;
        var completed = _answers.Values.Count(answer => answer.Status is not ShadcnQuestionnaireItemStatus.Unanswered);
        State = new(_active, current, _items.Count, completed, _items.Count == 0 ? 0 : (double)completed / _items.Count,
            new Dictionary<string, ShadcnQuestionnaireAnswer>(_answers, StringComparer.Ordinal), new Dictionary<string, string>(_errors, StringComparer.Ordinal));
    }

    private ShadcnQuestionnaireItemDefinition GetItem(string name) => _items.SingleOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal)) ?? throw new InvalidOperationException($"Unknown questionnaire item '{name}'.");
    private int IndexOf(string name) => _items.Select((item, index) => (item, index)).Single(pair => string.Equals(pair.item.Name, name, StringComparison.Ordinal)).index;
    private static ShadcnQuestionnaireAnswer Answer(IReadOnlyList<string> selected, string? input) => new(selected.Count > 0 || !string.IsNullOrWhiteSpace(input) ? ShadcnQuestionnaireItemStatus.Answered : ShadcnQuestionnaireItemStatus.Unanswered, selected.ToArray(), input);
    private static IReadOnlyList<ShadcnQuestionnaireItemDefinition> Active(IReadOnlyList<ShadcnQuestionnaireItemDefinition> items) => items.Where(item => !item.Disabled).ToArray();

    private static void ValidateDefinitions(IReadOnlyList<ShadcnQuestionnaireItemDefinition> items)
    {
        if (items.Count == 0) throw new InvalidOperationException("Questionnaire requires at least one item.");
        if (items.Any(item => string.IsNullOrWhiteSpace(item.Name))) throw new InvalidOperationException("Questionnaire item names must be non-empty.");
        if (items.Select(item => item.Name).Distinct(StringComparer.Ordinal).Count() != items.Count) throw new InvalidOperationException("Questionnaire item names must be unique.");
        foreach (var item in items)
        {
            var choices = item.Choices ?? Array.Empty<ShadcnQuestionnaireChoiceDefinition>();
            if (choices.Any(choice => string.IsNullOrWhiteSpace(choice.Value)) || choices.Select(choice => choice.Value).Distinct(StringComparer.Ordinal).Count() != choices.Count) throw new InvalidOperationException($"Questionnaire choices for '{item.Name}' must have unique non-empty values.");
            if (choices.Count(choice => choice.Custom) > 1) throw new InvalidOperationException($"Questionnaire item '{item.Name}' can define at most one custom choice.");
            if (choices.Any(choice => choice.Custom) && !item.AllowsFreeform) throw new InvalidOperationException($"Questionnaire item '{item.Name}' must allow freeform input when it defines a custom choice.");
        }
    }

    private static void ValidateAnswers(IReadOnlyList<ShadcnQuestionnaireItemDefinition> items, IReadOnlyDictionary<string, ShadcnQuestionnaireAnswer> answers)
    {
        foreach (var pair in answers)
        {
            var item = items.SingleOrDefault(candidate => string.Equals(candidate.Name, pair.Key, StringComparison.Ordinal)) ?? throw new InvalidOperationException($"Unknown questionnaire answer '{pair.Key}'.");
            if (!Enum.IsDefined(pair.Value.Status)) throw new InvalidOperationException($"Answer '{pair.Key}' has an invalid status.");
            if (item.Required && pair.Value.Status is ShadcnQuestionnaireItemStatus.Skipped) throw new InvalidOperationException($"Required answer '{pair.Key}' cannot be skipped.");
            if (pair.Value.SelectedValues.Any(value => item.Choices?.All(choice => !string.Equals(choice.Value, value, StringComparison.Ordinal)) != false)) throw new InvalidOperationException($"Answer '{pair.Key}' contains an unknown choice.");
            if (pair.Value.SelectedValues.Any(value => item.Choices?.Any(choice => string.Equals(choice.Value, value, StringComparison.Ordinal) && choice.Disabled) == true)) throw new InvalidOperationException($"Answer '{pair.Key}' contains a disabled choice.");
            if (pair.Value.SelectedValues.Count > 1 && !item.Multiple) throw new InvalidOperationException($"Answer '{pair.Key}' cannot select multiple choices.");
            var answered = pair.Value.SelectedValues.Count > 0 || !string.IsNullOrWhiteSpace(pair.Value.InputValue);
            if ((pair.Value.Status is ShadcnQuestionnaireItemStatus.Answered) != answered) throw new InvalidOperationException($"Answer '{pair.Key}' status does not match its value.");
            if (pair.Value.Status is ShadcnQuestionnaireItemStatus.Skipped && answered) throw new InvalidOperationException($"Skipped answer '{pair.Key}' cannot contain a value.");
            if (!item.AllowsFreeform && !string.IsNullOrWhiteSpace(pair.Value.InputValue)) throw new InvalidOperationException($"Answer '{pair.Key}' does not allow freeform input.");
            var choices = item.Choices ?? Array.Empty<ShadcnQuestionnaireChoiceDefinition>();
            if (!string.IsNullOrWhiteSpace(pair.Value.InputValue) && choices.Any(choice => choice.Custom) && !choices.Any(choice => choice.Custom && pair.Value.SelectedValues.Contains(choice.Value, StringComparer.Ordinal)))
                throw new InvalidOperationException($"Answer '{pair.Key}' contains freeform input without selecting its custom choice.");
        }
    }
}
