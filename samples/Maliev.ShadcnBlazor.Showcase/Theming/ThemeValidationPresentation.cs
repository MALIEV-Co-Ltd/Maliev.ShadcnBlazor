namespace Maliev.ShadcnBlazor.Showcase.Theming;

public static class ThemeValidationPresentation
{
    public static string AdvisoryLabel(int advisories) =>
        $"{advisories} {(advisories == 1 ? "advisory" : "advisories")}";

    public static string StatusLabel(int errors, int advisories) => errors switch
    {
        > 0 => $"Export blocked · {errors} {(errors == 1 ? "error" : "errors")}",
        _ when advisories > 0 => $"Ready to export · {AdvisoryLabel(advisories)}",
        _ => "Ready to export"
    };
}
