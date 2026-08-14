namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class FormsShowcaseContractTests
{
    [Fact]
    public void FormsRouteDocumentsAndRendersEveryPlanFourComponent()
    {
        var root = FindRoot();
        var page = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "FormsAndDateSelection.razor"));

        Assert.Contains("@page \"/components/forms-and-date-selection\"", page, StringComparison.Ordinal);
        foreach (var component in new[] { "ShadcnInput", "ShadcnTextarea", "ShadcnInputGroup", "ShadcnInputOtp", "ShadcnNativeSelect", "ShadcnSelect", "ShadcnCombobox", "ShadcnCalendar", "ShadcnDatePicker" })
            Assert.Contains($"<{component}", page, StringComparison.Ordinal);
        foreach (var section in new[] { "Installation", "Binding and forms", "Accessibility", "Customization", "Keyboard support" })
            Assert.Contains(section, page, StringComparison.Ordinal);
        Assert.Contains("CultureInfo.GetCultureInfo(\"th-TH\")", page, StringComparison.Ordinal);
        Assert.Contains("dir=\"rtl\"", page, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
