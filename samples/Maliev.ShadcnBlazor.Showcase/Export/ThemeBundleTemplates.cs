using System.Text;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Export;

public static class ThemeBundleTemplates
{
    public static string WriteThemeClass(ShadcnTheme theme) => Normalize($$"""
        using Maliev.ShadcnBlazor.Theming;

        namespace YourApp.Theming;

        public static class MalievShadcnTheme
        {
            public static ShadcnTheme Create()
            {
                return {{ShadcnThemeCSharpWriter.Write(theme)}}            }
        }
        """);

    public static string WriteReadme(
        ShadcnTheme theme,
        ThemeBundleOptions options,
        ShadcnThemeValidationResult validation) => Normalize($$"""
        # {{theme.Name}} — Maliev.ShadcnBlazor integration

        Package version: {{options.PackageVersion}}
        Schema version: {{theme.SchemaVersion}}
        Selected preset ancestry: {{options.PresetAncestry}}

        ## Install and register

        ```shell
        dotnet add package Maliev.ShadcnBlazor --version {{options.PackageVersion}}
        ```

        In `Program.cs`, call `builder.Services.AddMalievShadcn();`. The registration includes the supported MudBlazor services and configures the Shadcn overlay scope.

        ## Stylesheet order

        Add these files to the document head in this order, followed by your application stylesheet:

        ```html
        <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
        <link href="_content/Maliev.ShadcnBlazor/css/shadcn-base.css" rel="stylesheet" />
        <link href="_content/Maliev.ShadcnBlazor/css/shadcn-semantic-foundations.css" rel="stylesheet" />
        <link href="_content/Maliev.ShadcnBlazor/css/shadcn-disclosure-navigation.css" rel="stylesheet" />
        <link href="_content/Maliev.ShadcnBlazor/css/shadcn-actions.css" rel="stylesheet" />
        <link href="_content/Maliev.ShadcnBlazor/css/shadcn-data-display.css" rel="stylesheet" />
        <link href="_content/Maliev.ShadcnBlazor/css/shadcn-mudblazor.css" rel="stylesheet" />
        <link href="theme.css" rel="stylesheet" />
        ```

        Load `_content/MudBlazor/MudBlazor.min.js` once before the Blazor runtime script.

        ## Provider and typed theme

        Add `@using Maliev.ShadcnBlazor.Components`, `@using Maliev.ShadcnBlazor.Theming`, and `@using YourApp.Theming` to `_Imports.razor`. Wrap application content once, as shown in `Examples/AppShell.razor.txt`. `MalievShadcnTheme.Create()` uses only public typed RCL APIs and reproduces `theme.json`.

        `ShadcnThemeProvider` already renders `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider`, and `MudSnackbarProvider`. Do not render duplicate Mud overlay providers at the same root.

        ## RTL and localization

        Set `Direction="ShadcnDirection.RightToLeft"` for an RTL subtree or bind `Direction` to locale state. Keep translated accessible names in the application; generated technical identifiers stay invariant.

        ## Coexistence and migration

        Existing MudBlazor content can remain inside `ShadcnThemeProvider` while screens migrate incrementally. Keep MudBlazor on the package-supported 9.7.x boundary and validate adapter selectors before any MudBlazor upgrade. Remove duplicate root Mud theme and overlay providers when adopting the Shadcn provider.

        ## Import and export

        `theme.json` is the canonical import format. Import JSON only; do not import this ZIP. The showcase validates size, MIME, UTF-8, schema, fields, and token safety transactionally before changing the active theme. Keep `theme.css` and `MalievShadcnTheme.cs` generated from the same JSON revision.

        ## Pinned reference policy

        The package ships its pinned reference metadata as `reference/shadcn-reference.json`. Update that reference and re-run the component, contract, and browser certification gates before adopting upstream Shadcn changes.

        ## Validation report

        Structural errors: {{validation.Errors.Count}}
        Contrast warnings: {{validation.Warnings.Count}}
        Contrast checks: {{validation.ContrastResults.Count(item => item.Passes)}}/{{validation.ContrastResults.Count}} passing
        {{WriteMessages(validation)}}
        ## Files

        `manifest.json` records the byte size and SHA-256 digest for every payload file. Verify those hashes after transport before copying files into an application.
        """);

    public static string ProgramExample(string packageVersion) => Normalize($$"""
        using Maliev.ShadcnBlazor;

        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.Services.AddMalievShadcn();
        await builder.Build().RunAsync();

        // Package: Maliev.ShadcnBlazor {{packageVersion}}
        """);

    public static string AppShellExample() => Normalize("""
        @using Maliev.ShadcnBlazor.Components
        @using Maliev.ShadcnBlazor.Theming
        @using YourApp.Theming

        <ShadcnThemeProvider Theme="@MalievShadcnTheme.Create()"
                             IsDarkMode="@isDark"
                             Direction="@direction">
            @Body
        </ShadcnThemeProvider>

        @code {
            private bool isDark;
            private ShadcnDirection direction = ShadcnDirection.LeftToRight;
        }
        """);

    public static string FormExample() => Normalize("""
        @using Maliev.ShadcnBlazor.Components.Actions
        @using Maliev.ShadcnBlazor.Components.Forms
        @using Maliev.ShadcnBlazor.Components.Selection

        <ShadcnField DescriptionId="request-help">
            <ShadcnFieldLabel For="request-name">Request name</ShadcnFieldLabel>
            <input id="request-name" @bind="requestName" aria-describedby="request-help" />
            <ShadcnFieldDescription Id="request-help">Use a customer-facing name.</ShadcnFieldDescription>
        </ShadcnField>
        <label>
            <ShadcnCheckbox @bind-Value="approved" /> Approved
        </label>
        <ShadcnButton ButtonType="ShadcnButtonType.Submit">Submit</ShadcnButton>

        @code {
            private string requestName = string.Empty;
            private bool approved;
        }
        """);

    public static string OverlayExample() => Normalize("""
        @using Maliev.ShadcnBlazor.Components
        @using Maliev.ShadcnBlazor.Components.Direction
        @using Maliev.ShadcnBlazor.Theming

        <ShadcnDirectionProvider Direction="ShadcnDirection.RightToLeft">
            <p lang="th">ตัวอย่างเนื้อหาภาษาไทยแบบขวาไปซ้าย</p>
        </ShadcnDirectionProvider>

        @* ShadcnThemeProvider owns MudPopoverProvider, MudDialogProvider, and MudSnackbarProvider. *@
        """);

    private static string WriteMessages(ShadcnThemeValidationResult validation)
    {
        if (validation.Errors.Count == 0 && validation.Warnings.Count == 0)
            return "No structural errors or contrast warnings were reported.\n";

        var builder = new StringBuilder();
        foreach (var error in validation.Errors)
            builder.Append("- ERROR ").Append(error.Code).Append(" at ").Append(error.Path).Append(": ").Append(error.Message).Append('\n');
        foreach (var warning in validation.Warnings)
            builder.Append("- WARNING ").Append(warning.Code).Append(" at ").Append(warning.Path).Append(": ").Append(warning.Message).Append('\n');
        return builder.ToString();
    }

    private static string Normalize(string value)
    {
        var normalized = value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd();
        return normalized + "\n";
    }
}
