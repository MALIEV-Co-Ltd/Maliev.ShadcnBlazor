using Maliev.ShadcnBlazor.Components.Conversation;

namespace Maliev.ShadcnBlazor.Tests.Components.Conversation;

public sealed class QuestionnaireControllerTests
{
    private static readonly ShadcnQuestionnaireItemDefinition[] Items =
    [
        new("scope", Required: true, Choices: [new("component"), new("feature")]),
        new("checks", Multiple: true, Choices: [new("tests"), new("review"), new("deploy", Disabled: true)]),
        new("notes", AllowsFreeform: true)
    ];

    [Fact]
    public void SingleAndMultipleAnswersHaveDeterministicStatus()
    {
        var controller = new ShadcnQuestionnaireController(Items);
        controller.SetChoice("scope", "component", true);
        controller.SetChoice("scope", "feature", true);
        controller.SetChoice("checks", "tests", true);
        controller.SetChoice("checks", "review", true);

        Assert.Equal(["feature"], controller.State.Answers["scope"].SelectedValues);
        Assert.Equal(["tests", "review"], controller.State.Answers["checks"].SelectedValues);
        Assert.Equal(ShadcnQuestionnaireItemStatus.Answered, controller.State.Answers["scope"].Status);
        Assert.Throws<InvalidOperationException>(() => controller.SetChoice("checks", "deploy", true));
    }

    [Fact]
    public void FreeformAndExplicitSkipRemainDistinct()
    {
        var controller = new ShadcnQuestionnaireController(Items);
        controller.SetInput("notes", "ตรวจสอบแบบเต็ม");
        Assert.Equal("ตรวจสอบแบบเต็ม", controller.State.Answers["notes"].InputValue);
        Assert.Equal(ShadcnQuestionnaireItemStatus.Answered, controller.State.Answers["notes"].Status);
        controller.Skip("notes");
        Assert.Equal(ShadcnQuestionnaireItemStatus.Skipped, controller.State.Answers["notes"].Status);
        Assert.Throws<InvalidOperationException>(() => controller.Skip("scope"));
    }

    [Fact]
    public void CustomChoiceOwnsItsInputAndSelectingAStandardChoiceClearsIt()
    {
        var items = new[]
        {
            new ShadcnQuestionnaireItemDefinition("scope", Required: true, AllowsFreeform: true,
                Choices: [new("component"), new("other", Custom: true)])
        };
        var controller = new ShadcnQuestionnaireController(items);

        Assert.Throws<InvalidOperationException>(() => controller.SetInput("scope", "ชิ้นงานเฉพาะ"));
        controller.SetChoice("scope", "other", true);
        controller.SetInput("scope", "ชิ้นงานเฉพาะ · Custom part");

        Assert.Equal(["other"], controller.State.Answers["scope"].SelectedValues);
        Assert.Equal("ชิ้นงานเฉพาะ · Custom part", controller.State.Answers["scope"].InputValue);

        controller.SetChoice("scope", "component", true);

        Assert.Equal(["component"], controller.State.Answers["scope"].SelectedValues);
        Assert.Null(controller.State.Answers["scope"].InputValue);
    }

    [Fact]
    public void CustomChoiceDefinitionsAndControlledAnswersRejectAmbiguousInput()
    {
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController(
            [new("scope", Choices: [new("other", Custom: true)])]));
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController(
            [new("scope", AllowsFreeform: true, Choices: [new("other", Custom: true), new("custom", Custom: true)])]));

        var items = new[]
        {
            new ShadcnQuestionnaireItemDefinition("scope", AllowsFreeform: true,
                Choices: [new("component"), new("other", Custom: true)])
        };

        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController(items,
            initialAnswers: new Dictionary<string, ShadcnQuestionnaireAnswer>
            {
                ["scope"] = new(ShadcnQuestionnaireItemStatus.Answered, ["component"], "ambiguous")
            }));
    }

    [Fact]
    public async Task CustomChoiceRequiresUserProvidedTextBeforeNavigation()
    {
        var items = new[]
        {
            new ShadcnQuestionnaireItemDefinition("scope", Required: true, AllowsFreeform: true,
                Choices: [new("component"), new("other", Custom: true)])
        };
        var controller = new ShadcnQuestionnaireController(items);
        controller.SetChoice("scope", "other", true);

        var missing = await controller.NextAsync();

        Assert.False(missing.Succeeded);
        Assert.Equal("A custom answer is required.", missing.Error);
        Assert.Equal("scope", missing.FocusItemName);
    }

    [Fact]
    public void ExistingChoiceAndFreeformCompositionRemainsBackwardCompatibleWithoutCustomMetadata()
    {
        var items = new[]
        {
            new ShadcnQuestionnaireItemDefinition("scope", AllowsFreeform: true, Choices: [new("component")])
        };
        var controller = new ShadcnQuestionnaireController(items);

        controller.SetChoice("scope", "component", true);
        controller.SetInput("scope", "optional detail");

        Assert.Equal("optional detail", controller.State.Answers["scope"].InputValue);
    }

    [Fact]
    public async Task NavigationValidatesRequiredAndCustomRulesAndFocusesInvalidItem()
    {
        var controller = new ShadcnQuestionnaireController(Items, validate: (item, answer, _) =>
            ValueTask.FromResult<string?>(item.Name == "scope" && answer.SelectedValues.Contains("feature") ? "Choose a smaller scope." : null));

        var required = await controller.NextAsync();
        Assert.False(required.Succeeded);
        Assert.Equal("scope", required.FocusItemName);
        Assert.NotNull(required.Error);
        controller.SetChoice("scope", "feature", true);
        var custom = await controller.NextAsync();
        Assert.False(custom.Succeeded);
        Assert.Equal("Choose a smaller scope.", custom.Error);
        controller.SetChoice("scope", "component", true);
        var moved = await controller.NextAsync();
        Assert.True(moved.Succeeded);
        Assert.Equal("checks", controller.State.ActiveItemName);
    }

    [Fact]
    public async Task CustomValidatorCanLocalizeTheRequiredError()
    {
        var controller = new ShadcnQuestionnaireController(Items, validate: (item, answer, _) =>
            ValueTask.FromResult<string?>(item.Required && answer.Status is ShadcnQuestionnaireItemStatus.Unanswered ? "กรุณาเลือก" : null));

        var result = await controller.NextAsync();

        Assert.Equal("กรุณาเลือก", result.Error);
    }

    [Fact]
    public void ResumeResetAndConditionalItemsPreserveStableAnswers()
    {
        var initial = new Dictionary<string, ShadcnQuestionnaireAnswer>
        {
            ["scope"] = new(ShadcnQuestionnaireItemStatus.Answered, ["component"], null),
            ["checks"] = new(ShadcnQuestionnaireItemStatus.Answered, ["tests"], null)
        };
        var controller = new ShadcnQuestionnaireController(Items, activeItemName: "checks", initialAnswers: initial);
        controller.SetChoice("checks", "review", true);
        controller.SetItems([Items[0], Items[2]]);

        Assert.Equal("notes", controller.State.ActiveItemName);
        Assert.Equal(["component"], controller.State.Answers["scope"].SelectedValues);
        Assert.DoesNotContain("checks", controller.State.Answers.Keys);
        controller.Reset();
        Assert.Equal("scope", controller.State.ActiveItemName);
        Assert.Equal(["component"], controller.State.Answers["scope"].SelectedValues);
    }

    [Fact]
    public void ProgressExcludesDisabledItemsAndTracksSkippedAnswers()
    {
        var items = Items.Append(new ShadcnQuestionnaireItemDefinition("hidden", Disabled: true)).ToArray();
        var controller = new ShadcnQuestionnaireController(items);
        controller.SetChoice("scope", "component", true);
        controller.Skip("checks");

        Assert.Equal(3, controller.State.Total);
        Assert.Equal(2, controller.State.Completed);
        Assert.Equal(2d / 3d, controller.State.Progress);
    }

    [Fact]
    public void DefinitionsAndControlledStateRejectAmbiguity()
    {
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController([]));
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController([new("same"), new("same")]));
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController([new("x", Choices: [new("same"), new("same")])]));
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController(Items, activeItemName: "missing"));
    }

    [Fact]
    public void InitialAndControlledAnswersRejectUnknownNamesChoicesStatusesAndInconsistentValues()
    {
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController(Items, initialAnswers: new Dictionary<string, ShadcnQuestionnaireAnswer> { ["missing"] = ShadcnQuestionnaireAnswer.Empty }));
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController(Items, initialAnswers: new Dictionary<string, ShadcnQuestionnaireAnswer> { ["scope"] = new(ShadcnQuestionnaireItemStatus.Answered, ["unknown"], null) }));
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController(Items, initialAnswers: new Dictionary<string, ShadcnQuestionnaireAnswer> { ["scope"] = new((ShadcnQuestionnaireItemStatus)99, [], null) }));
        var controller = new ShadcnQuestionnaireController(Items);
        Assert.Throws<InvalidOperationException>(() => controller.SetAnswers(new Dictionary<string, ShadcnQuestionnaireAnswer> { ["scope"] = new(ShadcnQuestionnaireItemStatus.Unanswered, ["component"], null) }));
    }

    [Fact]
    public void InitialAndControlledAnswersRejectRequiredSkipAndDisabledChoices()
    {
        var items = new[] { new ShadcnQuestionnaireItemDefinition("scope", Required: true, Choices: [new("allowed"), new("disabled", Disabled: true)]) };
        Assert.Throws<InvalidOperationException>(() => new ShadcnQuestionnaireController(items, initialAnswers: new Dictionary<string, ShadcnQuestionnaireAnswer>
        {
            ["scope"] = new(ShadcnQuestionnaireItemStatus.Skipped, [], null)
        }));
        var controller = new ShadcnQuestionnaireController(items);
        Assert.Throws<InvalidOperationException>(() => controller.SetAnswers(new Dictionary<string, ShadcnQuestionnaireAnswer>
        {
            ["scope"] = new(ShadcnQuestionnaireItemStatus.Answered, ["disabled"], null)
        }));
    }
}
