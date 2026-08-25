using Bunit;
using Maliev.ShadcnBlazor.Components.Conversation;
using Maliev.ShadcnBlazor.Components.DataDisplay;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Components.Conversation;

public sealed class QuestionnaireTests : BunitContext
{
    public QuestionnaireTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<IShadcnIdAllocator, ShadcnIdAllocator>();
    }

    [Fact]
    public void QuestionnaireRendersNativeAccessibleComposition()
    {
        var cut = Render<Fixtures.QuestionnaireFixture>();
        var form = cut.Find("form[data-slot='questionnaire']");
        Assert.Equal("workflow", form.GetAttribute("aria-label"));
        var progress = cut.Find("[data-slot='questionnaire-progress']");
        Assert.Equal("ltr", progress.QuerySelector("[data-slot='questionnaire-progress-value']")!.GetAttribute("dir"));
        Assert.Equal("progressbar", progress.GetAttribute("role"));
        Assert.Equal("1", progress.GetAttribute("aria-valuenow"));
        var active = cut.Find("fieldset[data-slot='questionnaire-item']:not([hidden])");
        Assert.Equal("scope", active.GetAttribute("name"));
        Assert.Equal("What may change?", active.QuerySelector("legend")!.TextContent);
        Assert.Equal("radio", active.QuerySelector("input")!.GetAttribute("type"));
        Assert.Equal("scope", active.QuerySelector("input")!.GetAttribute("name"));
        var descriptionId = active.GetAttribute("aria-describedby");
        Assert.NotNull(descriptionId);
        Assert.Equal("Choose scope", cut.Find($"#{descriptionId}").TextContent);
        Assert.All(cut.FindAll("fieldset[data-slot='questionnaire-item'][hidden]"), item => Assert.NotNull(item.GetAttribute("inert")));
        Assert.Equal(["previous", "skip", "next", "submit"], cut.FindAll("[data-testid]").Select(element => element.GetAttribute("data-testid")));
    }

    [Fact]
    public void ChoiceNavigationValidationAndSubmitMutateRealState()
    {
        var cut = Render<Fixtures.QuestionnaireFixture>();
        cut.Find("button[data-slot='questionnaire-next']").Click();
        Assert.Equal("true", cut.Find("fieldset[name='scope']").GetAttribute("aria-invalid"));
        Assert.Equal("An answer is required.", cut.Find("[data-slot='questionnaire-error']").TextContent);
        cut.Find("input[value='component']").Change(true);
        cut.Find("button[data-slot='questionnaire-next']").Click();
        Assert.Equal("checks", cut.Find("fieldset:not([hidden])").GetAttribute("name"));
        cut.Find("button[data-slot='questionnaire-skip']").Click();
        Assert.Equal("notes", cut.Find("fieldset:not([hidden])").GetAttribute("name"));
        cut.Find("input[data-slot='questionnaire-input']").Input("รายละเอียดภาษาไทย");
        cut.Find("button[data-slot='questionnaire-submit']").Click();
        Assert.Contains("submitted", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void NativeRadioValueSelectsSingleChoice()
    {
        var cut = Render<Fixtures.QuestionnaireFixture>();

        cut.Find("input[value='feature']").Change("feature");
        cut.Find("button[data-slot='questionnaire-next']").Click();

        Assert.Equal("checks", cut.Find("fieldset:not([hidden])").GetAttribute("name"));
    }

    [Fact]
    public void ChoiceCanHideItsNativeIndicatorWhileKeepingRadioSemantics()
    {
        var cut = Render<Fixtures.QuestionnaireFixture>();
        var feature = cut.Find("label:has(input[value='feature'])");

        Assert.Equal("hidden", feature.GetAttribute("data-indicator"));
        Assert.Equal("radio", feature.QuerySelector("input")!.GetAttribute("type"));
    }

    [Fact]
    public void CustomChoiceRevealsAssociatedInputAndClearsItWhenStandardChoiceWins()
    {
        var cut = Render<Fixtures.QuestionnaireCustomChoiceFixture>();
        var other = cut.Find("input[value='other']");

        Assert.Equal("false", other.GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("[data-slot='questionnaire-input']"));

        other.Change(true);
        var input = cut.Find("[data-slot='questionnaire-input']");
        Assert.Equal("true", cut.Find("input[value='other']").GetAttribute("aria-expanded"));
        Assert.Equal(input.Id, cut.Find("input[value='other']").GetAttribute("aria-controls"));
        input.Input("ชิ้นงานเฉพาะ · Custom part");
        Assert.Equal("ชิ้นงานเฉพาะ · Custom part", cut.Find("[data-testid='custom-answer']").TextContent);

        cut.Find("input[value='component']").Change(true);
        Assert.Empty(cut.FindAll("[data-slot='questionnaire-input']"));
        Assert.Equal(string.Empty, cut.Find("[data-testid='custom-answer']").TextContent);
    }

    [Fact]
    public void CustomChoiceValidationKeepsFocusContractOnTheOwningQuestion()
    {
        var cut = Render<Fixtures.QuestionnaireCustomChoiceFixture>();
        cut.Find("input[value='other']").Change(true);
        cut.Find("[data-testid='custom-submit']").Click();

        Assert.Equal("true", cut.Find("fieldset[name='scope']").GetAttribute("aria-invalid"));
        Assert.Equal("A custom answer is required.", cut.Find("[data-slot='questionnaire-error']").TextContent);
        Assert.NotNull(cut.Find("[data-slot='questionnaire-input']").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void ControlledItemRequiresCallbackAndPartsRejectOrphans()
    {
        var items = new[] { new ShadcnQuestionnaireItemDefinition("one") };
        Assert.ThrowsAny<Exception>(() => Render<ShadcnQuestionnaire>(p => p.Add(c => c.Items, items).Add(c => c.Item, "one").AddChildContent("x")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnQuestionnaireItem>(p => p.Add(c => c.Name, "one").AddChildContent("x")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnQuestionnaireInput>(p => p.Add(c => c.AccessibleName, "Answer")));
    }

    [Fact]
    public void ControlledAnswersRequireCallbackAndScopedIdRefsAreUnique()
    {
        var items = new[] { new ShadcnQuestionnaireItemDefinition("one") };
        var answers = new Dictionary<string, ShadcnQuestionnaireAnswer>();
        Assert.ThrowsAny<Exception>(() => Render<ShadcnQuestionnaire>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.AccessibleName, "controlled")
            .Add(c => c.Answers, answers)
            .AddChildContent("x")));

        var first = Render<Fixtures.QuestionnaireFixture>();
        var second = Render<Fixtures.QuestionnaireFixture>();
        var firstDescription = first.Find("fieldset[name='scope']").GetAttribute("aria-describedby");
        var secondDescription = second.Find("fieldset[name='scope']").GetAttribute("aria-describedby");
        Assert.NotEqual(firstDescription, secondDescription);
        Assert.Single(first.FindAll($"#{firstDescription}"));
        Assert.Single(second.FindAll($"#{secondDescription}"));

        using var freshScope = new BunitContext();
        freshScope.JSInterop.Mode = JSRuntimeMode.Loose;
        freshScope.Services.AddScoped<IShadcnIdAllocator, ShadcnIdAllocator>();
        Assert.ThrowsAny<Exception>(() => freshScope.Render<ShadcnQuestionnaire>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.AccessibleName, "controlled")
            .Add(c => c.Answers, answers)
            .AddChildContent("x")));
        var recreated = freshScope.Render<Fixtures.QuestionnaireFixture>();
        Assert.Equal(first.Find("form").Id, recreated.Find("form").Id);
    }

    [Fact]
    public void ControlledAnswerRestorationPreservesValidationErrors()
    {
        var cut = Render<Fixtures.QuestionnaireControlledFixture>();
        cut.Find("button[data-slot='questionnaire-next']").Click();
        Assert.Equal("true", cut.Find("fieldset[name='scope']").GetAttribute("aria-invalid"));
        var input = cut.Find("input[value='component']");
        var errorId = cut.Find("[data-slot='questionnaire-error']").Id;
        Assert.NotNull(errorId);
        Assert.Contains(errorId, input.GetAttribute("aria-describedby")!, StringComparison.Ordinal);
    }

    [Fact]
    public void DisabledDefinitionsRenderSafelyAndDoNotRequireAnswers()
    {
        var cut = Render<Fixtures.QuestionnaireDisabledFixture>();
        var disabled = cut.Find("fieldset[name='disabled']");
        Assert.True(disabled.HasAttribute("disabled"));
        Assert.True(disabled.HasAttribute("hidden"));
        Assert.Equal("unanswered", disabled.GetAttribute("data-status"));
    }

    [Fact]
    public void KeyboardBridgeIgnoresCompositionAndScopesFocusToRoot()
    {
        var source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-questionnaire.js"));
        Assert.Contains("event.isComposing", source, StringComparison.Ordinal);
        Assert.Contains("root.querySelectorAll", source, StringComparison.Ordinal);
        Assert.Contains("invokeMethodAsync(\"OnShortcutAsync\"", source, StringComparison.Ordinal);
        Assert.Contains("input:not([type='radio']):not([type='checkbox'])", source, StringComparison.Ordinal);
    }

    [Fact]
    public void QuestionnaireCssOwnsFocusInvalidMotionForcedAndLogicalContracts()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css"));
        Assert.Contains(".shadcn-questionnaire-choice:focus-within", css, StringComparison.Ordinal);
        Assert.Contains("[aria-invalid=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion", css, StringComparison.Ordinal);
        Assert.Contains("forced-colors", css, StringComparison.Ordinal);
        Assert.Contains("text-align:start", css, StringComparison.Ordinal);
        Assert.Matches(@"\.shadcn-questionnaire-choice\s*>\s*\[data-slot=""questionnaire-choice-label""\][^{]*\{[^}]*font-weight:\s*650", css);
        Assert.Matches(@"\.shadcn-questionnaire-choice-description\s*\{[^}]*font-weight:\s*400", css);
    }
}
